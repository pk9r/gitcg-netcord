# Tasks

## 1. Fix Native Lifecycle in ImageCacheService & Extensions

- [x] 1.1 Remove premature disposal of `arcaneBorderResized` in `LoadArcaneBorderAsync` (`ImageCacheServiceExtension.cs`) and verify ownership transfers to caller without Access Violation.
- [x] 1.2 Add deterministic disposal of intermediate unscaled `icon` in `LoadBorderedIconAsync(card)` and `cachedBorderedIcon` in `LoadBorderedIconAsync(card, size)` (`ImageCacheServiceExtension.cs`).
- [x] 1.3 Add null-safety guards in `BorderIcon` and verify exception handling when input bitmaps are invalid or resize fails.
- [x] 1.4 Wrap encoded PNG `SKData` buffer in `using var data` in `ImageCacheService.CacheImageAsync` to promptly release unmanaged memory.

## 2. Resource Disposal in Deck Image Creators

- [x] 2.1 Wrap card bitmap drawing in `try ... finally` blocks to dispose all elements in `roleBitmaps` and `actionBitmaps` in `SimplestDeckImageCreator.cs`.
- [x] 2.2 Wrap card bitmap drawing in `try ... finally` blocks to dispose all elements in `roleBitmaps` and `actionBitmaps` in `GameBackgroundDeckImageCreator.cs`.

## 3. Verification & Regression Testing

- [x] 3.1 Add or update unit tests to render a deck containing Arcane Legend cards and verify the image renders successfully to a PNG stream.
- [x] 3.2 Build the solution and run `dotnet test` to verify all tests pass without errors or memory faults.
