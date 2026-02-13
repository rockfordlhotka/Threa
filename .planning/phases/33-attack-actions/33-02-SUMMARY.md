---
phase: 33-attack-actions
plan: 02
subsystem: ui
tags: [blazor, ranged-combat, anonymous-target, tv-modifier, sv-display]

# Dependency graph
requires:
  - phase: 32-layout-restructuring
    provides: Combat tab layout with Actions group buttons
  - phase: 33-attack-actions plan 01
    provides: Anonymous target pattern for melee AttackMode
provides:
  - Anonymous target selection step in ranged attack flow
  - Simplified TV modifier input for solo ranged attacks
  - SV-only result display for anonymous ranged targets
  - Activity log integration for anonymous ranged attacks
affects: [33-attack-actions remaining plans]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Anonymous target conditional rendering in ranged attack mode"
    - "TVAdjustment offset calculation to make resolver produce desired total TV"

key-files:
  created: []
  modified:
    - Threa/Threa.Client/Components/Pages/GamePlay/RangedAttackMode.razor

key-decisions:
  - "Used FirearmAttackResolver with TVAdjustment offset instead of bypassing resolver for anonymous targets"
  - "SV for anonymous targets calculated as AV - TV (player-entered) rather than resolver's internal SV calculation"

patterns-established:
  - "Anonymous ranged target: zero out conditions, offset TVAdjustment to cancel range base TV and fire mode modifier"
  - "GetAnonymousSV() helper method for Razor template (avoids @{} block issue)"

# Metrics
duration: 5min
completed: 2026-02-13
---

# Phase 33 Plan 02: Anonymous Ranged Target Summary

**Anonymous target support in ranged attack flow with simplified TV modifier input and SV-only result display using FirearmAttackResolver with TVAdjustment offset**

## Performance

- **Duration:** 5 min
- **Started:** 2026-02-13T04:39:37Z
- **Completed:** 2026-02-13T04:45:00Z
- **Tasks:** 1
- **Files modified:** 1

## Accomplishments
- Target selection step as first step with "Anonymous Target" option (matches melee AttackMode pattern)
- Simplified single TV modifier input replaces full range/conditions/cover/size/dodge UI for anonymous targets
- SV-only result display showing AV - TV = SV prominently (fs-3 size)
- Activity log shows AV, TV, SV for anonymous ranged attacks
- Weapon selection, fire mode, AP/FAT costs fully functional for anonymous targets
- "Change Target" button in result footer to switch between target types
- Dynamic step numbering (5 steps for anonymous, 6-8 for full)

## Task Commits

Each task was committed atomically:

1. **Task 1: Add target selection and anonymous ranged attack flow** - `5ee0a0f` (feat)

**Plan metadata:** pending

## Files Created/Modified
- `Threa/Threa.Client/Components/Pages/GamePlay/RangedAttackMode.razor` - Added target selection step, anonymous TV input, SV-only result display, conditional rendering for anonymous vs full flow, TVAdjustment offset calculation in ExecuteAttack(), simplified activity log message

## Decisions Made
- **Used resolver with TVAdjustment offset** instead of bypassing FirearmAttackResolver: The resolver adds range base TV (6 for Short) and fire mode TV (+1 burst, +3 suppression) internally. For anonymous targets, TVAdjustment is set to `anonymousTVModifier - baseRangeTV - fireModeTV` so the resolver produces total TV equal to what the player entered. This preserves all resolver logic (ammo consumption, hit detection, SV calculation for damage) while giving the player full control over TV.
- **SV displayed as AV - TV** (player's entered value) rather than resolver's internal RV, since the player entered the total TV manually and expects SV = AV - TV directly.
- **GetAnonymousSV() helper method** used instead of inline `@{}` variable block to avoid Razor compilation error (RZ1010).

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed Razor @{} block compilation error**
- **Found during:** Task 1 (Result phase implementation)
- **Issue:** `@{ var anonSV = ... }` inside an `@if` block caused Razor error RZ1010 "Unexpected { after @ character"
- **Fix:** Replaced inline variable with `GetAnonymousSV()` helper method in the @code block
- **Files modified:** RangedAttackMode.razor
- **Verification:** Build succeeds with zero errors
- **Committed in:** 5ee0a0f (part of task commit)

---

**Total deviations:** 1 auto-fixed (1 bug)
**Impact on plan:** Minor syntax fix, no scope change.

## Issues Encountered
None beyond the Razor syntax issue noted above.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Anonymous target support complete for both melee (plan 01) and ranged (plan 02)
- Ready for remaining Phase 33 plans (button wiring, disabled state, etc.)
- All existing ranged attack functionality preserved (no regression)

---
*Phase: 33-attack-actions*
*Completed: 2026-02-13*
