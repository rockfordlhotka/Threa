---
phase: 18-wound-management
plan: 01
subsystem: effects
tags: [wounds, effects, gm-tools, modals, blazor]

dependency_graph:
  requires: [17-health-management]
  provides: [wound-management-ui, custom-wound-severity]
  affects: [19-effect-management]

tech_stack:
  added: []
  patterns: [modal-over-modal, severity-based-defaults]

key_files:
  created:
    - Threa/Threa.Client/Components/Shared/WoundManagementModal.razor
    - Threa/Threa.Client/Components/Shared/WoundFormModal.razor
  modified:
    - GameMechanics/Effects/Behaviors/WoundBehavior.cs
    - Threa/Threa.Client/Components/Shared/CharacterDetailGmActions.razor

decisions:
  - key: severity-levels
    choice: Four fixed levels (Minor, Moderate, Severe, Critical)
    reason: Matches CONTEXT.md requirements, provides clear progression
  - key: severity-defaults
    choice: Auto-fill AS penalty and FAT/VIT rates when severity selected
    reason: Speeds up GM workflow while allowing customization
  - key: wound-templates
    choice: Seven common wound presets (broken arm/leg, concussion, etc.)
    reason: Quick entry for common injuries, GM can still customize

metrics:
  duration: 17 min
  completed: 2026-01-28
---

# Phase 18 Plan 01: Wound Management UI Summary

Extended WoundState with custom severity support and created wound management UI for GMs.

## Objective Achievement

Created wound management UI accessible from CharacterDetailGmActions that enables GMs to:
- Add wounds with severity levels (Minor/Moderate/Severe/Critical)
- Specify body location and description
- Use common wound templates for quick entry
- Customize AS penalty and FAT/VIT damage rates
- Edit and remove existing wounds

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Extend WoundState for Custom Severity | eba2ecf | WoundBehavior.cs |
| 2 | Create Wound Form Modal | 1e08b3d | WoundFormModal.razor |
| 3 | Create Wound Management Modal and Wire to GM Actions | bcb382a | WoundManagementModal.razor, CharacterDetailGmActions.razor |

## Implementation Details

### WoundState Extensions (Task 1)
Added five new properties to WoundState:
- `Severity` - GM-set severity label
- `Description` - GM-provided wound description
- `CustomASPenalty` - Override for AS penalty
- `FatDamagePerTick` - Override for FAT damage per 20 rounds
- `VitDamagePerTick` - Override for VIT damage per 20 rounds

Added static methods:
- `GetSeverityDefaults(severity)` - Returns (asPenalty, fatPerTick, vitPerTick) tuple
- `CreateCustomWound(...)` - Factory method for creating GM wounds

Updated `OnTick()` and `GetAbilityScoreModifiers()` to use custom values when set, falling back to severity defaults or legacy calculation.

### WoundFormModal (Task 2)
277-line Blazor component with:
- Severity dropdown (required, auto-fills defaults on change)
- Location dropdown (Head, Torso, Arms, Legs)
- Description text input (required, min 3 chars)
- Common wound template dropdown (7 presets)
- Customizable AS penalty, FAT/VIT damage fields
- Edit mode support (populates from existing wound)
- Validation and save logic with CharacterUpdateMessage publishing

### WoundManagementModal (Task 3)
298-line Blazor component with:
- Add Wound button at top
- Wound table with columns: Severity (badge), Location, Description, Effects, Actions
- Severity-based sorting (Critical first)
- Edit/Remove buttons per wound
- Remove confirmation dialog
- Summary stats (total wounds, total AS penalty)

### CharacterDetailGmActions Integration
- Added DialogService injection
- Added "Manage Wounds" card with danger styling
- Wound count badge displayed when wounds exist
- Opens WoundManagementModal via DialogService.OpenAsync

## Key Patterns

### Severity Defaults
| Severity | AS Penalty | FAT/Tick | VIT/Tick |
|----------|------------|----------|----------|
| Minor | -2 | 1 | 0 |
| Moderate | -2 | 2 | 1 |
| Severe | -4 | 3 | 2 |
| Critical | -6 | 4 | 3 |

### Common Wound Templates
- Broken Arm (RightArm, Moderate)
- Broken Leg (RightLeg, Moderate)
- Concussion (Head, Moderate)
- Deep Cut (Torso, Minor)
- Puncture Wound (Torso, Moderate)
- Burns (Torso, Minor)
- Internal Bleeding (Torso, Severe)

## Deviations from Plan

None - plan executed exactly as written.

## Decisions Made

1. **Severity defaults formula**: Used simple static values per severity level rather than complex calculation. Allows easy GM understanding and customization.

2. **CharacterUpdateType for edits**: Used `EffectAdded` for both add and edit operations since `EffectModified` doesn't exist in the enum. Message description clarifies the action.

3. **Badge styling**: Used Bootstrap badge classes - danger for Critical, warning for Severe/Moderate, secondary for Minor.

## Next Phase Readiness

Phase 18 Plan 02 can proceed with wound healing integration. The wound infrastructure is complete:
- WoundState supports custom severity and damage rates
- UI enables full CRUD operations on wounds
- Real-time updates work via CharacterUpdateMessage

No blockers identified.
