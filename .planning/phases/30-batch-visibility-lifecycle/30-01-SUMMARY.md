---
phase: 30-batch-visibility-lifecycle
plan: 01
subsystem: api
tags: [csla, batch-actions, visibility, archive, npc-management]

# Dependency graph
requires:
  - phase: 29-batch-action-framework
    provides: BatchActionService, BatchActionRequest/Result DTOs, sequential processing pattern
provides:
  - BatchActionType.Visibility and BatchActionType.Dismiss enum values
  - BatchActionRequest.VisibilityTarget property for reveal/hide toggle
  - BatchActionResult action-type-aware Summary (contextual text per action type)
  - BatchActionService.ToggleVisibilityAsync method
  - BatchActionService.DismissAsync method with archive + table detach
affects: [30-02 UI wiring for visibility/dismiss buttons]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "NPC-only batch operations: silently skip non-NPC characters"
    - "Two-step dismiss: archive character then remove from table"

key-files:
  created: []
  modified:
    - GameMechanics/Batch/BatchActionRequest.cs
    - GameMechanics/Batch/BatchActionResult.cs
    - GameMechanics/Batch/BatchActionService.cs
    - GameMechanics/ServiceCollectionExtensions.cs

key-decisions:
  - "ITableDal injected into BatchActionService for dismiss table-detach (existing DI registration)"
  - "Non-NPC characters silently skipped (no error, no count) for visibility and dismiss"
  - "VisibilityTarget defaults to true (reveal) when null"

patterns-established:
  - "NPC-gated batch operations: check IsNpc before processing, continue on false"
  - "Action-type-aware Summary: switch expression on BatchActionType for contextual result text"

# Metrics
duration: 6min
completed: 2026-02-05
---

# Phase 30 Plan 01: Batch Visibility & Lifecycle DTOs and Service Summary

**BatchActionService extended with ToggleVisibilityAsync and DismissAsync for NPC visibility toggle and archive/detach operations**

## Performance

- **Duration:** 6 min
- **Started:** 2026-02-06T02:43:49Z
- **Completed:** 2026-02-06T02:49:58Z
- **Tasks:** 2
- **Files modified:** 4

## Accomplishments
- Extended BatchActionType enum with Visibility and Dismiss values
- BatchActionResult.Summary now produces contextual text per action type (NPC revealed/hidden, Dismissed N NPCs)
- ToggleVisibilityAsync sets VisibleToPlayers on NPCs only, silently skipping PCs
- DismissAsync archives NPCs (IsArchived=true) and removes from table via ITableDal
- Both new methods publish single CharactersUpdatedMessage after batch completes
- All 1075 existing tests pass with new constructor parameter

## Task Commits

Each task was committed atomically:

1. **Task 1: Extend batch action DTOs for visibility and dismiss** - `a15e28a` (feat)
2. **Task 2: Add ToggleVisibilityAsync and DismissAsync to BatchActionService** - `efe5020` (feat)

## Files Created/Modified
- `GameMechanics/Batch/BatchActionRequest.cs` - Added Visibility/Dismiss enum values, VisibilityTarget property
- `GameMechanics/Batch/BatchActionResult.cs` - Added VisibilityAction property, action-type-aware Summary switch expression
- `GameMechanics/Batch/BatchActionService.cs` - Added ITableDal injection, ToggleVisibilityAsync, DismissAsync methods
- `GameMechanics/ServiceCollectionExtensions.cs` - Updated DI registration comment

## Decisions Made
- ITableDal already registered in DI; no ServiceCollectionExtensions change needed for the interface, only comment update
- Non-NPC characters silently skipped (not counted in success or failure) -- consistent with NPC-only operation semantics
- VisibilityTarget defaults to true (reveal) when null, matching the common "show hidden NPCs" use case

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Service layer complete for visibility toggle and dismiss/archive
- Ready for 30-02: UI wiring of SelectionBar buttons to these service methods
- No blockers or concerns

---
*Phase: 30-batch-visibility-lifecycle*
*Completed: 2026-02-05*
