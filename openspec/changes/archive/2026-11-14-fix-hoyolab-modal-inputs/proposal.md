# Proposal

## Why

When users submit the "Add account" modal from `/hoyolab-accounts`, the application returns an error `Hoyolab API return non-zero Retcode: 10001.` (Please login), even though running the `/add-hoyolab-account` slash command with the exact same credentials succeeds. In NetCord (1.0.0-beta.29), modal components wrapped in `LabelProperties` are deserialized as `NetCord.Label` instances containing a nested `ILabelComponent` (`TextInput`), causing `context.Components.OfType<TextInput>()` to return an empty collection and submit empty credentials to the Hoyolab API. A similar issue affects the "Update active account" modal.

Fixing this extraction ensures users can seamlessly link new Hoyolab accounts and switch their active accounts directly through interactive Discord UI modals without errors.

## What Changes

- Fix modal component extraction in `AddHoyolabAccountComponentInteraction` to retrieve `TextInput` instances nested inside `NetCord.Label` components, correctly reading `hoyolab-user-id` and `token`.
- Fix modal component extraction in `UpdateActiveHoyolabAccountComponentInteraction` to retrieve `TextInput` instances nested inside `NetCord.Label` components, correctly reading `uid`.
- Add test coverage for modal interaction parsing and validation logic.

**SemVer Impact**: PATCH (bug fix; backwards compatible with no breaking API changes).

### In-Scope
- Component parsing logic in `AddHoyolabAccountComponentInteraction.PostAsync`.
- Component parsing logic in `UpdateActiveHoyolabAccountComponentInteraction.PostAsync`.
- Test cases for modal text input extraction and edge case validation (e.g. whitespace trimming or missing components).

### Out-of-Scope
- Modifications to `HoyolabHttpClient` or API request protocols.
- Modifications to Discord slash commands (`/add-hoyolab-account`, `/hoyolab-accounts`, `/remove-hoyolab-account`).
- Database schema changes or migrations.
- New infrastructure dependencies or Aspire AppHost configuration changes.

## Capabilities

### New Capabilities
None.

### Modified Capabilities
- `hoyolab-account-management`: Extend the capability specification to include requirements and scenarios for linking Hoyolab accounts via interactive modals and switching the active account via interactive modals.

## Impact

- Affected files:
  - `src/GitcgNetCord.MainApp/Commands/Interactions/AddHoyolabAccountComponentInteraction.cs`
  - `src/GitcgNetCord.MainApp/Commands/Interactions/UpdateActiveHoyolabAccountComponentInteraction.cs`
  - `tests/GitcgNetCord.MainApp.Tests/` (new/updated tests)
- APIs and Dependencies: No dependency changes; uses existing NetCord types (`NetCord.Label`, `NetCord.TextInput`).
- Infrastructure: None.
