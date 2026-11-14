# Design: Improve Hoyolab Add Account Flow

## Context

When users add an account via `/hoyolab-accounts` or `/add-hoyolab-account`, `AddHoyolabAccountSlashCommand.ExecuteCoreAsync` calls Hoyolab's `GetGameRecordCardAsync` and filters the returned games by `x.GameId == 2` (Genshin Impact). Currently, it unconditionally caches the results into `HybridCache` and responds with a Discord `StringMenuProperties` component.

For single-character accounts (~80-90% of cases), requiring a redundant select menu click degrades user experience. For zero-character accounts, sending an empty options list causes NetCord and Discord to reject the interaction with an API validation error. For multi-character accounts, the select menu gives no hint about whether any character is already linked.

## Goals / Non-Goals

**Goals:**
- Branch flow based on character count:
  - `count == 0`: Return friendly error message in English.
  - `count == 1`: Auto-link immediately and display the confirmation embed with Daily Snapshot onboarding buttons.
  - `count > 1`: Display select menu with `[Already Linked - Select to update token]` annotation for existing UIDs.
- Re-use response generation for account addition across both `ExecuteCoreAsync` (single-character) and `SelectGameRecordComponentInteraction` (multi-character).
- Avoid unnecessary cache writes and read cycles for single-character accounts.

**Non-Goals:**
- Modifying modal fields, labels, or placeholders.
- Modifying database schemas or EF Core models.
- Changing `HoyolabAccountService.AddHoyolabAccountAsync` business logic (it already supports updating tokens for existing accounts).

## Decisions

### Decision 1: Shared Confirmation Embed & Button Builder
Extract response building into a reusable helper method, e.g. `HoyolabAccountResponseHelper.CreateAccountAddedResponse`:
```csharp
public static (EmbedProperties Embed, ActionRowProperties Components) CreateAccountAddedResponse(
    GameRecord gameRecord,
    Guid addedAccountId
)
```
- **Rationale**: Keeps the confirmation embed formatting and Daily Snapshot prompt buttons (`[ Enable Daily Snapshot ]`, `[ No, Maybe Later ]`) strictly consistent between `AddHoyolabAccountSlashCommand.ExecuteCoreAsync` (auto-link) and `SelectGameRecordComponentInteraction.ExecuteAsync` (menu selection).
- **Alternatives considered**: Duplicating embed/button code in both places (rejected due to maintenance risk and inconsistency).

### Decision 2: Direct Execution in `ExecuteCoreAsync` for Single-Character
When `genshinRecords.Count == 1`:
- Immediately call `await hoyolabAccountService.AddHoyolabAccountAsync(context.User.Id, authorize.HoyolabUserId, authorize.Token, singleRecord.GameRoleId, singleRecord.Region)`.
- Generate the response using `CreateAccountAddedResponse`.
- Update interaction via `context.Interaction.ModifyResponseAsync`.
- Do not write to `HybridCache`.
- **Rationale**: Eliminates an unnecessary round-trip interaction and avoids cache TTL expiration risks.

### Decision 3: Option Annotation for Multi-Character Accounts
When `genshinRecords.Count > 1`:
- Fetch existing accounts via `await hoyolabAccountService.GetHoyolabAccountsAsync(context.User.Id)`.
- Check if `existingUids.Contains(x.GameRoleId)`.
- If true, append ` [Already Linked - Select to update token]` to option description.
- If false, render standard description `Lv. {Level} {RegionName} ({UID})`.
- Cache records and authorize data in `HybridCache` as before and show the select menu.
- **Rationale**: Gives users clarity on whether selecting an option links a new character or refreshes the token for an existing one (Option A).

### Decision 4: Safe Zero-Character Notification
When `genshinRecords.Count == 0`:
- Call `context.Interaction.ModifyResponseAsync(m => m.Content = "No Genshin Impact characters found on this Hoyolab account. Please ensure you have created a Genshin Impact character and linked it to this Hoyolab account.");`.
- **Rationale**: Avoids passing an empty options list to `StringMenuProperties`, which triggers Discord HTTP 400 Bad Request.

## Risks / Trade-offs

- **[Risk] Duplicate GameRoleId in Hoyolab API response**: Discord select menus throw an exception if two options have identical values.
  → **Mitigation**: Use `.DistinctBy(x => x.GameRoleId)` when populating the select menu options.
- **[Risk] Failure during Auto-link in `ExecuteCoreAsync`**: HoyolabAccountService might throw DbUpdateException or other errors.
  → **Mitigation**: Wrap the `AddHoyolabAccountAsync` call in try-catch in `ExecuteCoreAsync`, mirroring the error handling in `SelectGameRecordComponentInteraction`.
