---
phase: 26-visibility-lifecycle
plan: 04
subsystem: ui
tags: [blazor, npc, archive, gamemaster, restore]

# Dependency graph
requires:
  - phase: 26-01
    provides: IsArchived property on Character DTO and CharacterEdit
  - phase: 26-03
    provides: Archive button in CharacterDetailAdmin
provides:
  - NPC Archive browser page at /gamemaster/npcs/archive
  - Restore flow with table selection and hidden state
  - Delete flow with confirmation modal
  - Archive navigation link in NpcTemplates page
affects: [26-05]

# Tech tracking
tech-stack:
  added: []
  patterns: [archive-restore-flow, table-selection-modal]

key-files:
  created:
    - Threa/Threa.Client/Components/Pages/GameMaster/NpcArchive.razor
  modified:
    - Threa/Threa.Client/Components/Pages/GameMaster/NpcTemplates.razor

key-decisions:
  - "Restored NPCs return in hidden state per CONTEXT.md specification"
  - "Delete requires confirmation; archive is instant (per CONTEXT.md)"
  - "GetActiveTablesAsync already existed in ITableDal - no DAL changes needed"

patterns-established:
  - "Archive browser pattern: card grid with restore/delete actions"
  - "Restore flow: modal with table selection, sets VisibleToPlayers=false"

# Metrics
duration: 5min
completed: 2026-02-03
---

# Phase 26 Plan 04: NPC Archive Browser Summary

**Archive browser page with table-selection restore flow and permanent delete with confirmation**

## Performance

- **Duration:** 5 min
- **Started:** 2026-02-03T19:18:07Z
- **Completed:** 2026-02-03T19:23:03Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments
- Created NpcArchive.razor page at /gamemaster/npcs/archive with card grid display
- Implemented restore flow with table selection modal, restored NPCs return hidden
- Implemented delete flow with confirmation modal for permanent removal
- Added Archive link with count badge to NpcTemplates page header

## Task Commits

Each task was committed atomically:

1. **Task 1: Create NpcArchive Page** - `9fc854b` (feat)
2. **Task 2: Add Navigation Link** - `d797504` (feat)

## Files Created/Modified
- `Threa/Threa.Client/Components/Pages/GameMaster/NpcArchive.razor` - Archive browser page with restore and delete flows
- `Threa/Threa.Client/Components/Pages/GameMaster/NpcTemplates.razor` - Added Archive link with count badge

## Decisions Made
- Restored NPCs return in hidden state (VisibleToPlayers = false) per CONTEXT.md specification
- Delete requires confirmation dialog; archive is instant (no confirmation) per CONTEXT.md
- GetActiveTablesAsync already exists in ITableDal and SqliteTableDal - no DAL changes required
- Used GameTable DTO (not Table) with local GetTableStatusDisplay helper for status rendering

## Deviations from Plan

None - plan executed exactly as written. The GetActiveTablesAsync method already existed in the interface and implementation from earlier phases.

## Issues Encountered
None - execution proceeded smoothly. The plan referenced types (`Table`) that were actually named `GameTable` in the codebase, but this was a minor adjustment during implementation.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Archive browser complete and accessible from NPC Templates page
- GMs can restore archived NPCs to any active table in hidden state
- GMs can permanently delete archived NPCs with confirmation
- Ready for Phase 26-05 (Save as Template feature)

---
*Phase: 26-visibility-lifecycle*
*Completed: 2026-02-03*
