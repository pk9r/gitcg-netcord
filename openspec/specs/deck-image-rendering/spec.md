# Deck Image Rendering Specification

## Purpose

Provides visual rendering capabilities for Genius Invokation TCG decks into image streams, supporting distinct layouts including Simplest, GameBackground, and Genshincards.

## Requirements

### Requirement: Simplest deck layout rendering
The system SHALL render a deck containing 3 role cards and 30 action cards into a PNG image stream using the Simplest layout format.

#### Scenario: Default Simplest deck rendering
- **WHEN** a valid deck is provided to the Simplest deck image creator with default options
- **THEN** the system generates a PNG image stream with 3 role cards centered at the top and 30 action cards arranged in a 10-column by 3-row grid below on a transparent background.

#### Scenario: Custom background color and column configuration
- **WHEN** Simplest deck rendering options specify a custom background color and action card column count
- **THEN** the system adjusts the canvas width and row count accordingly, fills the specified background color, and renders all 30 action cards in the calculated grid.

#### Scenario: Deck containing Arcane Legend cards
- **WHEN** a deck containing one or more Arcane Legend action cards is rendered
- **THEN** the system applies the arcane card border and renders the image stream without crashing or leaking native graphic resources.

### Requirement: Discord slash command Simplest image decoding
The Discord bot application SHALL support decoding deck codes into the Simplest image format via the deck slash command.

#### Scenario: User requests Simplest image via deck slash command
- **WHEN** a user executes the deck command with a valid sharing code and selects `img-simplest` as the decode type
- **THEN** the system decodes the deck, renders the Simplest deck image using the Skia renderer, and sends the image as a message attachment.

### Requirement: Genshincards dual skill value rendering
The system SHALL render action cards with two skill values in the Genshincards layout with both numeric values vertically aligned on the same text baseline above their respective element icons.

#### Scenario: Rendering action card with primary and secondary skill values
- **WHEN** an action card having both primary (`SkillValue`) and secondary (`SkillValue2`) skill values is rendered in the Genshincards layout
- **THEN** both skill values are positioned on the identical vertical baseline with horizontal offsets (-11 for primary, +9 for secondary) relative to the card's skill value origin, directly above their respective skill element icons.
