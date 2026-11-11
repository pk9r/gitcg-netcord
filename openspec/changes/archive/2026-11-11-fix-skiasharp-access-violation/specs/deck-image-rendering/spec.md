# Spec Delta

## MODIFIED Requirements

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
