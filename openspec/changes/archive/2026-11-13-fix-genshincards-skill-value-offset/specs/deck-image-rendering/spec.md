# Spec Delta

## ADDED Requirements

### Requirement: Genshincards dual skill value rendering
The system SHALL render action cards with two skill values in the Genshincards layout with both numeric values vertically aligned on the same text baseline above their respective element icons.

#### Scenario: Rendering action card with primary and secondary skill values
- **WHEN** an action card having both primary (`SkillValue`) and secondary (`SkillValue2`) skill values is rendered in the Genshincards layout
- **THEN** both skill values are positioned on the identical vertical baseline with horizontal offsets (-11 for primary, +9 for secondary) relative to the card's skill value origin, directly above their respective skill element icons.
