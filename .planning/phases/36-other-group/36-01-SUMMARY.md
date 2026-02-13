---
phase: 36-other-group
plan: 01
subsystem: ui
tags: [blazor, combat-tab, css, confirmation-flow]

# Dependency graph
requires:
  - phase: 32-combat-tab-layout
    provides: Other group button layout, Medical/Reload/Unload/Implant mode components
  - phase: 35-defense-group
    provides: combat-tile-passive-only CSS pattern, CombatStanceBehavior
provides:
  - "@if-guarded conditional buttons (Implants, Reload, Unload) hidden when preconditions not met"
  - "CombatMode.Rest enum value and inline Rest confirmation panel"
  - ".combat-tile-dimmed CSS class for cost-aware dimming"
affects: [36-02-dimming-and-tooltips]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Hidden-when-not-applicable button pattern using @if guards instead of disabled attribute"
    - "Inline confirmation panel for simple actions (Rest) following CombatMode pattern"

key-files:
  created: []
  modified:
    - "Threa/Threa.Client/Components/Pages/GamePlay/TabCombat.razor"
    - "Threa/Threa/wwwroot/css/themes.css"

key-decisions:
  - "StartRest checks only IsPassedOut, not CanRest(), allowing panel to open even with insufficient AP"

patterns-established:
  - "Hidden conditional buttons: use @if guard for equipment-dependent buttons, always-visible for universal actions"
  - "Confirmation panel pattern: StartX opens panel, ConfirmX executes logic, Cancel uses ReturnToDefault"

# Metrics
duration: 3min
completed: 2026-02-13
---

# Phase 36 Plan 01: Other Group Button Visibility and Rest Confirmation Summary

**Conditional Other group buttons hidden via @if guards, Rest confirmation panel with CombatMode.Rest, combat-tile-dimmed CSS class**

## Performance

- **Duration:** 3 min
- **Started:** 2026-02-13T16:39:30Z
- **Completed:** 2026-02-13T16:42:21Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments
- Converted Implants, Reload, Unload buttons from disabled to hidden (@if-guarded) when preconditions not met
- Replaced instant Rest execution with inline confirmation panel following CombatMode pattern
- Added .combat-tile-dimmed CSS class (opacity: 0.65) ready for Plan 02 to apply across all groups

## Task Commits

Each task was committed atomically:

1. **Task 1: Add combat-tile-dimmed CSS and convert conditional Other group buttons to hidden** - `36d95c3` (feat)
2. **Task 2: Add Rest confirmation flow with CombatMode.Rest** - `51244b5` (feat)

## Files Created/Modified
- `Threa/Threa/wwwroot/css/themes.css` - Added .combat-tile-dimmed CSS class with opacity: 0.65
- `Threa/Threa.Client/Components/Pages/GamePlay/TabCombat.razor` - @if-guarded conditional buttons, CombatMode.Rest enum, StartRest/ConfirmRest methods, Rest confirmation panel

## Decisions Made
- StartRest checks only IsPassedOut (not CanRest) so the confirmation panel can open even when AP is insufficient, matching how other combat modes work. The Confirm button is disabled via CanRest() within the panel.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- combat-tile-dimmed CSS class is ready for Plan 02 to apply across all three combat groups
- Rest confirmation panel follows the established CombatMode pattern, consistent with Medical/Reload/Unload modes
- No blockers for Plan 02

---
*Phase: 36-other-group*
*Completed: 2026-02-13*
