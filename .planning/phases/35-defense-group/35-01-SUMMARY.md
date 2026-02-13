---
phase: 35-defense-group
plan: 01
subsystem: ui, combat
tags: [blazor, css, combat-stance, defense, effects]

# Dependency graph
requires:
  - phase: 32-layout-restructuring
    provides: Combat tab tile layout and group structure
  - phase: 33-attack-actions
    provides: CombatStanceBehavior with Parry Mode support
provides:
  - Stance chip CSS styles (.stance-chip) for pill-shaped toggle buttons
  - CombatStanceBehavior constants and helpers for Dodge Focus and Block with Shield stances
  - Stance chip UI in Defense group with activation/deactivation wiring
  - OnAdding behavior updated to enforce single active stance
affects: [35-02, defend-mode-pre-selection]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Stance chips: pill-shaped toggle buttons below action tiles for persistent UI preferences"
    - "Effect-derived UI state: GetActiveStance reads from Character.Effects, no cached component state"

key-files:
  created: []
  modified:
    - Threa/Threa/wwwroot/css/themes.css
    - GameMechanics/Effects/Behaviors/CombatStanceBehavior.cs
    - Threa/Threa.Client/Components/Pages/GamePlay/TabCombat.razor

key-decisions:
  - "Parry Mode cost handled in chip click (1AP+1FAT or 2AP fallback) matching existing DefendMode logic"
  - "OnAdding updated to replace ANY existing CombatStance (not just same-name) for mutual exclusivity"

patterns-established:
  - "Stance chip pattern: small pill buttons with active/disabled/hover states, derive from effects"

# Metrics
duration: 4min
completed: 2026-02-13
---

# Phase 35 Plan 01: Defense Group Stance Chips Summary

**Stance chip UI in Defense group with 4 toggleable stances (Normal, Parry Mode, Dodge Focus, Block with Shield) derived from CombatStance effects**

## Performance

- **Duration:** 4 min
- **Started:** 2026-02-13T06:36:50Z
- **Completed:** 2026-02-13T06:40:52Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- Added stance chip CSS (pill-shaped, active/disabled/hover states, scifi theme glow)
- Extended CombatStanceBehavior with DodgeFocus and BlockWithShield constants, helpers, and state creators
- Updated OnAdding to enforce single active stance (replaces any existing CombatStance)
- Added 4 stance chips to Defense group in TabCombat with full activation wiring
- Parry Mode chip deducts AP/FAT cost; Dodge Focus and Block with Shield are free toggles
- Disabled states with explanatory tooltips for Parry Mode (no weapon/insufficient AP) and Block (no shield)

## Task Commits

Each task was committed atomically:

1. **Task 1: Add stance chip CSS and extend CombatStanceBehavior** - `bc6c9c6` (feat)
2. **Task 2: Add stance chips to Defense group in TabCombat** - `0bf2e35` (feat)

## Files Created/Modified
- `Threa/Threa/wwwroot/css/themes.css` - Added .stance-chip CSS with active, disabled, hover, and scifi theme styles
- `GameMechanics/Effects/Behaviors/CombatStanceBehavior.cs` - Added DodgeFocus/BlockWithShield constants, IsIn*, GetActiveStanceName, ClearStance, Create*State, Get*Description helpers; updated OnAdding for mutual exclusivity
- `Threa/Threa.Client/Components/Pages/GamePlay/TabCombat.razor` - Added EffectPortal injection, stance chip HTML in Defense group, GetActiveStance, SetStanceNormal, SetStanceDodgeFocus, SetStanceBlockWithShield, ActivateParryModeFromChip, CanPerformActiveDefenseForStance, GetParryModeChipTooltip methods

## Decisions Made
- Parry Mode cost handled directly in chip click handler (1AP+1FAT with 2AP fallback) to match existing DefendMode.EnterParryMode logic
- OnAdding updated to replace ANY existing CombatStance effect (not just same-name match) to enforce single-stance mutual exclusivity

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Stance chips are visible and functional in the Defense group
- Plan 02 can wire stance pre-selection into DefendMode for automatic defense type selection
- All 1089 existing tests pass with no regressions

---
*Phase: 35-defense-group*
*Completed: 2026-02-13*
