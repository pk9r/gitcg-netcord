# Design

## Context

See `proposal.md` for the motivation and background. `GitcgSkia` contains SkiaSharp implementations for `GameBackgroundDeckImageCreator` and `GenshincardsDeckImageCreator`. The `Simplest` deck layout, previously built on SixLabors.ImageSharp in `GitcgPainter`, needs to be implemented in `GitcgSkia`. In `GitcgNetCord.MainApp`, references to `SimplestDeckImageCreator` exist in `DeckImageCreatorCollection.cs` and `DeckSlashCommand.cs` but are commented out pending this implementation. Per project requirements, `RolesImageCreator` and the `GitcgPainter` directory remain preserved on disk.

## Goals / Non-Goals

**Goals:**
- Implement `SimplestDeckImageOptions` and `SimplestDeckImageCreator` in `GitcgSkia` using SkiaSharp.
- Render all 30 action cards individually without grouping/stacking (10 columns x 3 rows by default) beneath 3 horizontally centered role cards.
- Implement thread-safe rendering: pre-fetch and cache card bordered bitmaps asynchronously in parallel, followed by single-threaded drawing onto `SKCanvas`.
- Centralize resized bordered card caching in `ImageCacheServiceExtension` so both `SimplestDeckImageCreator` and `GameBackgroundDeckImageCreator` share it.
- Re-enable `SimplestDeckImageCreator` in `GitcgNetCord.MainApp` to restore the `/deck` slash command option `img-simplest`.

**Non-Goals:**
- Modifying, deleting, or porting `GitcgPainter` or `RolesImageCreator`.
- Changing existing behavior of `GameBackgroundDeckImageCreator` or `GenshincardsDeckImageCreator`.
- Changing action card stacking logic in `Simplest` (cards remain individual 30 distinct slots).

## Decisions

### Decision 1: Parallel asset pre-fetching followed by synchronous canvas drawing
- **Choice**: Preload all role card and action card bordered bitmaps using `Task.WhenAll`, then perform all canvas drawing operations sequentially on the main thread.
- **Rationale**: `SKCanvas` and its underlying Skia drawing context are not thread-safe. Initiating drawing calls concurrently from background tasks can lead to memory corruption or crashes in native Skia code. Separating parallel I/O and caching from canvas drawing guarantees thread safety while maintaining sub-millisecond drawing performance.
- **Alternatives considered**: Concurrent canvas drawing with locks (adds lock contention overhead and risk of deadlock) or fully sequential fetching (slow network turnaround).

### Decision 2: Generalize resized bordered icon caching in `ImageCacheServiceExtension`
- **Choice**: Provide `LoadBorderedIconAsync(this ImageCacheService service, ICardBasic card, SKSizeI size)` in `ImageCacheServiceExtension`.
- **Rationale**: Currently, `GameBackgroundDeckImageCreator` implements a private helper that constructs the cache key (`card.GetCacheKeyIconBordered(size)`), fetches the bordered card, resizes it with linear/mipmap sampling, and stores it in cache. Making this an extension method allows `SimplestDeckImageCreator` to reuse the exact same caching mechanism and eliminates code duplication.
- **Alternatives considered**: Duplicating the private helper in `SimplestDeckImageCreator`.

### Decision 3: Option fidelity with `SimplestDeckImageOptions`
- **Choice**: Maintain `ActionCardColumns` (int, default 10) and `BackgroundColor` (string, default `#00000000`) on `SimplestDeckImageOptions`, matching the contract from `GitcgPainter`.
- **Rationale**: Preserves API symmetry and allows callers to customize background fill and grid layout if desired. Color parsing uses `SKColor.TryParse` falling back to `SKColors.Transparent`.
- **Alternatives considered**: Hardcoding dimensions and background color without options.

## Risks / Trade-offs

- [Risk: Network or image asset failures for specific cards] → Handled by `ImageCacheService` using HTTP resilience and fallback to default border bitmaps.
- [Risk: Process file locking during compilation if `GitcgNetCord.MainApp` is running] → Mitigation: Development build can target `GitcgSkia` individually (`dotnet build src/GitcgSkia/GitcgSkia.csproj`) without needing to rebuild locked app executables until ready.

## Migration Plan

1. Implement `SimplestDeckImageOptions.cs` and `SimplestDeckImageCreator.cs` under `src/GitcgSkia/ImageCreators/Deck/Simplest/`.
2. Add `LoadBorderedIconAsync(card, size)` to `src/GitcgSkia/Extensions/ImageCacheServiceExtension.cs`.
3. Register `SimplestDeckImageCreator` in `src/GitcgSkia/Extensions/ServiceCollectionExtensions.cs`.
4. Uncomment `Simplest` in `src/GitcgNetCord.MainApp/Models/DeckImageCreatorCollection.cs` and `src/GitcgNetCord.MainApp/Commands/Slash/DeckSlashCommand.cs`.
5. Verify build and output image generation.
