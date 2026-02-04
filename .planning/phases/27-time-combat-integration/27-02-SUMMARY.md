---
phase: 27-time-combat-integration
plan: 02
subsystem: Combat/Time
tags: [targeting, visibility, time-advancement, activity-log, npc]

requires:
  - phase: 27-01
    provides: NPC targeting integration
  - phase: 26-02
    provides: NPC visibility filtering

provides:
  - Target invalidation when GM hides targeted NPC
  - Activity log announcement on NPC reveal
  - Verified NPC time advancement (CMBT-01, CMBT-03)

affects: []

tech-stack:
  added: []
  patterns:
    - Real-time character update handling with visibility checks
    - Activity log announcements using Announcement category

key-files:
  created: []
  modified:
    - Threa/Threa.Client/Components/Pages/GamePlay/Play.razor
    - Threa/Threa.Client/Components/Pages/GamePlay/GmTable.razor

key-decisions:
  - "Target invalidation uses VisibleToPlayers check in OnCharacterUpdateReceived"
  - "Reveal announcement uses ActivityCategory.Announcement for visibility"

patterns-established:
  - "OnCharacterUpdateReceived refreshes tableCharacters on any character update (not just self)"
  - "Visibility changes trigger target validation and potential action cancellation"

duration: 8min
completed: 2026-02-03
---

# Phase 27 Plan 02: Combat Polish Summary

**Target invalidation on NPC hide, reveal announcements to activity log, and verified NPC time advancement processing**

## Performance

- **Duration:** 8 min
- **Started:** 2026-02-03T10:00:00Z
- **Completed:** 2026-02-03T10:08:00Z
- **Tasks:** 3
- **Files modified:** 2

## Accomplishments

- Player targeting is automatically invalidated when GM hides their selected target
- Activity log shows "[NPC Name] appears!" when GM reveals hidden NPC
- Verified NPCs participate in time advancement (AP recovery, effect expiration) identically to PCs
- CMBT-01 and CMBT-03 success criteria confirmed via manual testing

## Task Commits

Each task was committed atomically:

1. **Task 1: Add Target Invalidation on Visibility Change** - `2a31cb0` (feat)
2. **Task 2: Add Reveal Activity Log Message** - `281571e` (feat)
3. **Task 3: Verify NPC Time Advancement** - No commit (verification checkpoint, user confirmed)

## Files Created/Modified

- `Threa/Threa.Client/Components/Pages/GamePlay/Play.razor` - Enhanced OnCharacterUpdateReceived to refresh tableCharacters on any character update and invalidate targeting if current target becomes hidden NPC
- `Threa/Threa.Client/Components/Pages/GamePlay/GmTable.razor` - Updated RevealNpc to publish "[NPC Name] appears!" announcement to activity log

## Decisions Made

| Decision | Rationale |
|----------|-----------|
| Target invalidation checks VisibleToPlayers on current target | Per CONTEXT.md: "If player is mid-action when GM hides target, action is invalidated" |
| Reveal uses ActivityCategory.Announcement | Stands out in activity log, player-visible notification |
| OnCharacterUpdateReceived refreshes ALL characters | Enables detecting visibility changes to any table character |

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

Phase 27 (Time & Combat Integration) is now complete. All success criteria met:

- [x] NPCs targetable by player combat actions (27-01)
- [x] Disposition-grouped target selection (27-01)
- [x] Target invalidation on NPC hide (27-02)
- [x] Reveal announcement in activity log (27-02)
- [x] CMBT-01: NPCs participate in round/time advancement (27-02 verified)
- [x] CMBT-03: Time advancement applies to NPCs same as PCs (27-02 verified)

The v1.6 milestone is complete. NPCs are fully integrated into the combat and time system.

---
*Phase: 27-time-combat-integration*
*Completed: 2026-02-03*
