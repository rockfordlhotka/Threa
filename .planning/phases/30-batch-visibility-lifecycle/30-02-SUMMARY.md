---
phase: 30-batch-visibility-lifecycle
plan: 02
subsystem: ui
tags: [blazor, batch-actions, selection-bar, visibility, dismiss, modal, npc-management]

# Dependency graph
requires:
  - phase: 30-01
    provides: "BatchActionService.ToggleVisibilityAsync and DismissAsync, BatchActionType.Visibility/Dismiss enums, VisibilityTarget property"
  - phase: 29-03
    provides: "SelectionBar with HandleBatchResult, result feedback display, Damage/Heal buttons"
  - phase: 28-01
    provides: "Selection state HashSet<int>, checkbox infrastructure, TableCharacterInfo.IsNpc/VisibleToPlayers"
provides:
  - "Toggle Visibility button in SelectionBar with contextual Reveal/Hide label"
  - "Dismiss button with BatchDismissConfirmModal confirmation"
  - "NPC filtering in SelectionBar (AllCharacters parameter, GetSelectedNpcIds)"
  - "Action-type-aware selection cleanup in GmTable HandleBatchActionCompleted"
affects: []

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "NPC-only buttons conditionally rendered via HasSelectedNpcs computed property"
    - "Action-type-aware selection cleanup using switch on BatchActionResult.ActionType"
    - "Pre-filtered NPC IDs at UI layer before service calls (PCs never sent to visibility/dismiss)"

key-files:
  created:
    - "Threa/Threa.Client/Components/Shared/BatchDismissConfirmModal.razor"
  modified:
    - "Threa/Threa.Client/Components/Shared/SelectionBar.razor"
    - "Threa/Threa.Client/Components/Pages/GamePlay/GmTable.razor"

key-decisions:
  - "Clear button at far left, Dismiss isolated at far right with separator"
  - "Visibility toggle has no confirmation (non-destructive); Dismiss requires confirmation modal"
  - "VisibilityButtonLabel is contextual: Reveal when any hidden, Hide when all visible"
  - "After visibility toggle, selection stays intact for chaining; after dismiss, only dismissed IDs removed"

patterns-established:
  - "NPC-only batch buttons: conditionally render via HasSelectedNpcs computed from AllCharacters parameter"
  - "Action-type switch in HandleBatchActionCompleted for per-action selection cleanup behavior"

# Metrics
duration: 4min
completed: 2026-02-05
---

# Phase 30 Plan 02: Batch Visibility & Dismiss UI Summary

**SelectionBar with Toggle Visibility (contextual Reveal/Hide) and Dismiss buttons, NPC filtering via AllCharacters, confirmation modal, and action-type-aware selection cleanup in GmTable**

## Performance

- **Duration:** 4 min
- **Started:** 2026-02-06T02:54:10Z
- **Completed:** 2026-02-06T02:57:53Z
- **Tasks:** 2
- **Files modified:** 3

## Accomplishments
- SelectionBar now has Toggle Visibility and Dismiss buttons that only appear when NPCs are selected
- BatchDismissConfirmModal provides confirmation before destructive dismiss/archive
- Button layout follows CONTEXT: Clear | Damage | Heal | Toggle Vis | ... | Dismiss
- GmTable HandleBatchActionCompleted now handles all four action types with distinct selection cleanup behavior

## Task Commits

Each task was committed atomically:

1. **Task 1: Create BatchDismissConfirmModal and update SelectionBar** - `05613ea` (feat)
2. **Task 2: Wire GmTable with AllCharacters parameter and action-type-aware selection cleanup** - `148bb73` (feat)

## Files Created/Modified
- `Threa/Threa.Client/Components/Shared/BatchDismissConfirmModal.razor` - Simple confirmation modal for batch dismiss with NPC count display
- `Threa/Threa.Client/Components/Shared/SelectionBar.razor` - Added AllCharacters parameter, NPC filtering, Toggle Visibility button, Dismiss button with confirmation
- `Threa/Threa.Client/Components/Pages/GamePlay/GmTable.razor` - Added AllCharacters binding, action-type-aware HandleBatchActionCompleted

## Decisions Made
- Clear button placed at far left of selection bar (most accessible position)
- Dismiss button isolated at far right with pipe separator (visual separation for destructive action)
- Toggle Visibility has no confirmation dialog (non-destructive, reversible action per CONTEXT)
- Dismiss requires explicit confirmation via BatchDismissConfirmModal (destructive/archive action)
- VisibilityButtonLabel: "Reveal" when any selected NPC is hidden, "Hide" when all are visible
- After visibility toggle, selection stays intact (GM may chain additional actions)
- After dismiss, only dismissed NPC IDs removed from selection (remaining PCs/NPCs stay selected)
- NPC IDs pre-filtered at UI layer via GetSelectedNpcIds() -- PCs are never sent to visibility/dismiss service calls

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Phase 30 (Batch Visibility & Lifecycle) is now complete
- All batch action types (Damage, Healing, Visibility, Dismiss) have full UI integration
- SelectionBar supports all four action types with appropriate UX for each
- GmTable handles action-type-specific selection cleanup

---
*Phase: 30-batch-visibility-lifecycle*
*Completed: 2026-02-05*
