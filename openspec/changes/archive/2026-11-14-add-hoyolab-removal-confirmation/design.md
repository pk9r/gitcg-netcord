# Design

## Context

See `proposal.md` for motivation.

In the current implementation:
- `RemoveHoyolabAccountComponentInteraction.SelectAsync` immediately calls `HoyolabAccountService.RemoveHoyolabAccountAsync` as soon as the user selects a UID from the dropdown menu.
- `RemoveHoyolabAccountSlashCommand.ExecuteAsync` immediately deletes the account upon command invocation.

To prevent accidental deletions, a confirmation step must intercept both flows, displaying an ephemeral confirmation message with explicit "Confirm Remove" (Danger) and "Cancel" (Secondary) buttons.

## Goals / Non-Goals

**Goals:**
- Provide a consistent two-step confirmation pattern for both interactive menu and slash command removals.
- Provide clear visual context (account UID, Region, and consequences of unlinking) before destructive action.
- Allow seamless cancellation that cleanly dismisses the action and disables interactive components.
- Keep the underlying `HoyolabAccountService` intact and maintain compatibility with existing active account fallback logic.

**Non-Goals:**
- Modifying `HoyolabAccountService` database transactions, credential wiping, or snapshot preservation routines.
- Changing other account management interactions (such as adding accounts or updating active accounts).
- Introducing graphics rendering or SkiaSharp components (this feature uses standard Discord embed and button interactions; no native bitmap allocations are involved).
- Modifying Aspire AppHost or infrastructure configurations.

## Decisions

### Decision 1: Shared Confirmation UI Builder
Instead of duplicating confirmation message formatting between the select menu interaction and the slash command, create a reusable helper method in `RemoveHoyolabAccountComponentInteraction`:
```csharp
public static void PopulateConfirmationPrompt(string uid, string region, MessageOptions message);
```
- **Rationale**: Keeps the UI representation consistent between the interactive dropdown flow and the slash command flow.
- **Alternatives Considered**:
  - Separate confirmation embeds for slash command vs. menu: Rejected to avoid UX drift and redundant code.

### Decision 2: NetCord Component Interaction with Parameterized CustomId
Use NetCord's `:` delimiter convention for passing the target UID to the confirmation button handler:
- Base Custom ID: `confirm-remove-hoyolab-account`
- Button Custom ID: `confirm-remove-hoyolab-account:{uid}`
- Cancel Custom ID: `cancel-remove-hoyolab-account`
- Handler signature:
  ```csharp
  public static async Task ConfirmRemovalAsync(
      HoyolabAccountService hoyolabAccountService,
      ButtonInteractionContext context,
      string uid
  );
  public static async Task CancelRemovalAsync(
      ButtonInteractionContext context
  );
  ```
- **Rationale**: Follows the established codebase pattern (e.g., `CopySharingCodeComponentInteraction`). NetCord automatically extracts path parameters after the `:` delimiter into method arguments.
- **Alternatives Considered**:
  - Discord Modals for confirmation text input: High friction for users on mobile; buttons provide one-tap explicit confirmation.
  - Storing pending removal UID in Redis/HybridCache: Unnecessary state complexity; embedding the UID directly in the ephemeral button's CustomId is stateless and self-contained.

### Decision 3: Handling Cancellation
When the user clicks "Cancel":
- Respond using `InteractionCallback.ModifyMessage(m => ...)` or modify the response to set `Content = "Account removal cancelled."` and clear all component buttons and embeds.
- **Rationale**: Clearly acknowledges the user's intent to cancel and prevents further interaction with expired buttons on the ephemeral message.
- **Alternatives Considered**:
  - Deleting the message entirely: Can be jarring to the user; showing an explicit cancellation note provides positive feedback.
  - Returning to the account selection dropdown: Could cause circular states; user can easily re-click "Remove account" from `/hoyolab-accounts` if they wish to pick a different account.

### Decision 4: Architectural Layer Separation
- **Domain & Service Layer**: `HoyolabAccountService` remains pure business logic.
- **Interaction / Presentation Layer**: `RemoveHoyolabAccountComponentInteraction` and `RemoveHoyolabAccountSlashCommand` handle user input, button rendering, and response modification.
- **Module Registration**: `HoyolabAccountModule` registers the two new button component interaction handlers.

## Risks / Trade-offs

- **[Risk] User clicks confirmation on an account that was already removed or invalidated in another session**
  → **Mitigation**: `HoyolabAccountService.RemoveHoyolabAccountAsync` returns a result object with `Success = false` and an informative message if the account is not found. `PopulateRemovalResponse` already handles failure results gracefully.

- **[Risk] Ephemeral message button tampering**
  → **Mitigation**: Discord prevents users from sending synthetic component interactions for components they cannot view, and `RemoveHoyolabAccountAsync` strictly verifies that the account belongs to `context.User.Id`.
