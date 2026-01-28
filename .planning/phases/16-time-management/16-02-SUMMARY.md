---
phase: 16-time-management
plan: 02
subsystem: ui
tags: [blazor, time-events, rx-net, player-ui]

# Dependency graph
requires:
  - phase: 16-time-management (01)
    provides: TimeSkipReceived event in ITimeEventSubscriber
provides:
  - Play.razor TimeSkipReceived subscription for non-combat time processing
  - "In Rounds" badge for player combat mode indicator
affects: [player-experience, time-management]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - TimeSkipReceived event subscription pattern for player time processing
    - Conditional badge rendering based on table combat state

key-files:
  created: []
  modified:
    - Threa/Threa.Client/Components/Pages/GamePlay/Play.razor

key-decisions:
  - "Cap time skip processing at 100 iterations to avoid UI freeze"
  - "Show 'In Rounds' badge only when in combat, hide when not"
  - "Remove redundant COMBAT/EXPLORATION badge, use simpler 'In Rounds' indicator"

patterns-established:
  - "Time skip round equivalents: minute=10, turn=100, hour=600, day=14400, week=100800"

# Metrics
duration: 5min
completed: 2026-01-27
---

# Phase 16 Plan 02: Player Time Events Summary

**Play.razor now subscribes to TimeSkipReceived events and shows "In Rounds" badge when table is in combat mode**

## Performance

- **Duration:** 5 min
- **Started:** 2026-01-27T10:00:00Z
- **Completed:** 2026-01-27T10:05:00Z
- **Tasks:** 2
- **Files modified:** 1

## Accomplishments
- Play.razor subscribes to TimeSkipReceived events for non-combat time advancement
- Characters process pending damage/healing and effect expiration when GM advances calendar time
- Player header shows red "In Rounds" badge when table is in combat mode
- Badge hidden when table is in normal (exploration) mode

## Task Commits

Each task was committed atomically:

1. **Task 1: Add TimeSkipReceived event subscription** - `77875aa` (feat)
2. **Task 2: Add "In Rounds" badge to player header** - `290cbef` (feat)

## Files Created/Modified
- `Threa/Threa.Client/Components/Pages/GamePlay/Play.razor` - Added TimeSkipReceived subscription and In Rounds badge

## Decisions Made
- Cap time skip processing at 100 iterations to prevent UI freeze on large time skips (e.g., skip 1 week = 100800 rounds)
- Show "In Rounds" badge only when in combat, not a COMBAT/EXPLORATION toggle - cleaner per CONTEXT.md
- Keep Round X badge always visible for continuity

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- TIME-10 through TIME-14 requirements now satisfied
- Player characters process time changes from both combat rounds and calendar time advancement
- Pending todo from STATE.md resolved: Play.razor now subscribes to TimeSkipReceived
- Ready for any remaining Phase 16 plans

---
*Phase: 16-time-management*
*Completed: 2026-01-27*
