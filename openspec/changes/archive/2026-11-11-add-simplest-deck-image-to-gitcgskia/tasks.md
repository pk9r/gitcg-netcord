# Tasks

## 1. Helper & Options Setup in GitcgSkia

- [x] 1.1 Implement `SimplestDeckImageOptions` in `src/GitcgSkia/ImageCreators/Deck/Simplest/SimplestDeckImageOptions.cs` with `ActionCardColumns` and `BackgroundColor` properties, and verify file compiles.
- [x] 1.2 Add generalized `LoadBorderedIconAsync(this ImageCacheService service, ICardBasic card, SKSizeI size)` to `src/GitcgSkia/Extensions/ImageCacheServiceExtension.cs` and verify compilation.

## 2. Simplest Deck Image Creator Implementation

- [x] 2.1 Implement `SimplestDeckImageCreator` in `src/GitcgSkia/ImageCreators/Deck/Simplest/SimplestDeckImageCreator.cs` implementing `IDeckImageCreator<SimplestDeckImageOptions>` and `IDeckImageCreationService` using SkiaSharp, and verify layout calculations for 3 role cards and 30 action cards.
- [x] 2.2 Register `SimplestDeckImageCreator` in `src/GitcgSkia/Extensions/ServiceCollectionExtensions.cs` and verify `GitcgSkia` builds cleanly via `dotnet build src/GitcgSkia/GitcgSkia.csproj`.

## 3. MainApp Integration & Verification

- [x] 3.1 Update `DeckImageCreatorCollection.cs` in `src/GitcgNetCord.MainApp/Models/DeckImageCreatorCollection.cs` to include `SimplestDeckImageCreator Simplest`.
- [x] 3.2 Update `DeckSlashCommand.cs` in `src/GitcgNetCord.MainApp/Commands/Slash/DeckSlashCommand.cs` to map `SharingDecodeType.ImageSimplest` to `deckImageCreatorCollection.Simplest`.
- [x] 3.3 Build `GitcgNetCord.MainApp` and verify all project references and dependency injection bindings resolve without warnings or errors.
