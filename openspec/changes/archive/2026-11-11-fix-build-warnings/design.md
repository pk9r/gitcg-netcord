# Design

## Context

See `proposal.md` for motivation.
Currently, `DeckEditorGatewayHandler` and `DuelAssistantGatewayHandler` declare `GatewayClient client` as a primary constructor parameter, but neither class uses it in its implementation.
In `GitcgGatewayHandlerContext`, `public Message Message { get; set; }` is a non-nullable reference type registered as a singleton service in DI without a constructor or default initializer, causing warning CS8618.

## Goals / Non-Goals

**Goals:**
- Eliminate `CS9113` in `DeckEditorGatewayHandler` and `DuelAssistantGatewayHandler` by removing unused constructor parameters.
- Eliminate `CS8618` in `GitcgGatewayHandlerContext` by adding `= null!;` default initialization.
- Achieve a clean build (`dotnet build --no-incremental` reports 0 warnings, 0 errors).

**Non-Goals:**
- Refactoring `GitcgGatewayHandlerContext` concurrency lifecycle or converting it to a scoped service (can be explored in a future enhancement if concurrent Discord message interleaving becomes an issue).
- Modifying Discord gateway message flow or Semantic Kernel plugin interactions.

## Decisions

### Decision 1: Remove `GatewayClient client` from Gateway Handlers
- **Choice**: Remove `GatewayClient client` from the primary constructor signatures of `DeckEditorGatewayHandler` and `DuelAssistantGatewayHandler`.
- **Rationale**: Neither handler uses `client`. `DeckEditorPlugin` injects `client` separately where application emojis are queried. DI activator for NetCord handlers (`AddGatewayHandler<T>`) dynamically resolves available constructor parameters from `IServiceProvider`.
- **Alternatives considered**:
  - *Keep parameter and use `_ = client;` or `#pragma warning disable CS9113`*: Rejected as unnecessary clutter and code smell.

### Decision 2: Initialize `Message` with `= null!;` in `GitcgGatewayHandlerContext`
- **Choice**: Change property declaration to `public Message Message { get; set; } = null!;`.
- **Rationale**:
  - `GitcgGatewayHandlerContext` is registered as a singleton (`services.AddSingleton<GitcgGatewayHandlerContext>()`) and its `Message` property is assigned by `DeckEditorGatewayHandler` upon receiving a message before any plugin consumes it.
  - Using `= null!;` satisfies C# nullable reference analysis for uninitialized properties in models populated post-instantiation, without breaking callers (e.g. `DeckEditorPlugin` calling `gitcgGatewayHandlerContext.Message.Channel`).
- **Alternatives considered**:
  - *`public Message? Message { get; set; }`*: Requires null checks or null-forgiving operators across all consumers.
  - *`public required Message Message { get; set; }`*: Incompatible with parameterless DI activation for singleton services; throws runtime exception.

## Risks / Trade-offs

- **[Risk] NetCord handler registration failure**: If NetCord or DI expected a specific constructor shape containing `GatewayClient`.
  - *Mitigation*: NetCord's `AddGatewayHandler<T>` uses `ActivatorUtilities.CreateInstance`, which matches constructor parameters against registered services. Since `IServiceScopeFactory` and `GitcgGatewayHandlerContext` are registered services, resolution succeeds.
- **[Risk] Null reference if `GitcgGatewayHandlerContext.Message` is accessed before assignment**:
  - *Mitigation*: Existing execution order guarantees `DeckEditorGatewayHandler` sets `gitcgGatewayHandlerContext.Message = message` before executing Semantic Kernel plugins.
