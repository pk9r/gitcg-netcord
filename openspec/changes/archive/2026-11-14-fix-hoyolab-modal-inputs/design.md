# Design

## Context

See `proposal.md` for motivation. In NetCord (1.0.0-beta.29), Discord modals constructed using `LabelProperties` wrap input fields within `NetCord.Label` objects. The submitted interaction payload provides `context.Components` as `IReadOnlyList<IModalComponent>`. Because `NetCord.TextInput` implements `ILabelComponent` and not `IModalComponent`, querying `context.Components.OfType<TextInput>()` yields an empty sequence. Consequently, `AddHoyolabAccountComponentInteraction` and `UpdateActiveHoyolabAccountComponentInteraction` fail to populate their input variables, resulting in empty parameters being sent to downstream services.

## Goals / Non-Goals

**Goals:**
- Reliably extract `TextInput` components from `ModalInteractionContext` regardless of whether they are wrapped inside `NetCord.Label` or present directly.
- Provide a clean, reusable helper method for modal text input extraction across command handlers.
- Sanitize extracted inputs (e.g., trimming whitespace) to prevent validation and authentication issues.
- Add unit test coverage verifying modal component parsing.

**Non-Goals:**
- Changing Discord modal UI layouts or labels.
- Modifying `HoyolabHttpClientService` authentication or endpoint logic.
- Altering database schema or entity models.

## Decisions

### 1. Centralized Helper for Modal Input Extraction
Create an extension method `GetTextInputs(this IEnumerable<IModalComponent> components)` or `GetTextInputValue(this ModalInteractionContext context, string customId)` in `GitcgNetCord.MainApp.Extensions`:
- **Approach**: Iterate through `IModalComponent` items in `context.Components`. If the item is a `NetCord.Label label && label.Component is TextInput textInput`, yield/evaluate the `TextInput`. If the item is directly a `TextInput textInput`, yield/evaluate it as well.
- **Alternatives Considered**:
  - *Inline LINQ in each interaction handler*: e.g., `context.Components.OfType<Label>().Select(l => l.Component).OfType<TextInput>()`. While functional, duplicating this query in multiple handlers risks future regression if new modals are introduced.
  - *Custom NetCord service filter*: Overly complex for simple interaction handler parameter extraction.
- **Rationale**: A centralized extension method is clean, testable, and protects against layout differences.

### 2. Input Trimming and Validation
- **Approach**: Call `.Trim()` on extracted input values (`hoyolabUserId`, `token`, `uid`). If required fields are null or empty after trimming, the handler should abort early with a descriptive ephemeral error message before making network calls to Hoyolab.
- **Rationale**: Copy-pasting cookies or user IDs on mobile often introduces stray spaces or newlines, which can cause spurious Hoyolab API rejection.

## Risks / Trade-offs

- **[Risk] NetCord Modal Layout Changes** → **Mitigation**: The extraction helper checks both `Label.Component` and direct `TextInput`, ensuring resilience across potential NetCord component hierarchy updates.
- **[Risk] Untrimmed Multi-line Tokens** → **Mitigation**: `string.Trim()` strips surrounding whitespace without affecting interior base64 characters of `v2_` tokens.

## Migration Plan

No database or deployment migration required. The change is purely internal to the command interaction handlers and test suite.
