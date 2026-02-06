---
phase: 31-batch-effects
plan: 02
subsystem: batch-actions, ui
tags: [blazor, csla, batch-operations, effects, dialog-service, effect-remove]

# Dependency graph
requires:
  - phase: 31-batch-effects/01
    provides: BatchActionService.AddEffectAsync, BatchEffectConfig, EffectAdd/EffectRemove enums, BatchActionRequest.EffectNamesToRemove, BatchActionResult.TotalEffectsRemoved, GmTable EffectRemove case
  - phase: 29-batch-damage-healing
    provides: BatchActionService, BatchActionRequest, BatchActionResult, SelectionBar, HandleBatchResult pattern
provides:
  - BatchActionService.RemoveEffectsAsync for batch effect removal
  - BatchEffectRemoveModal with union effect name checkbox list
  - EffectNameInfo class for effect name aggregation
  - SelectionBar "Remove Effects" button wiring
affects:
  - future effect management phases (batch operations now fully bidirectional)

# Tech tracking
tech-stack:
  added: []
  patterns:
    - Union effect name aggregation across selected characters
    - Checkbox multi-select modal with character count badges
    - Effect type icon mapping for visual identification

# File tracking
key-files:
  created:
    - GameMechanics/Batch/EffectNameInfo.cs
    - Threa/Threa.Client/Components/Shared/BatchEffectRemoveModal.razor
  modified:
    - GameMechanics/Batch/BatchActionService.cs
    - Threa/Threa.Client/Components/Shared/SelectionBar.razor

# Decisions
decisions:
  - id: 31-02-01
    decision: Build union effect name list by fetching all selected characters via IDataPortal
    reason: Consistent with existing CSLA data access patterns; ensures fresh data
  - id: 31-02-02
    decision: Exclude Wound type AND inactive effects from removal candidates
    reason: Wounds have special lifecycle management; inactive effects should not be removable
  - id: 31-02-03
    decision: Sort effect list by EffectType then Name for consistent display ordering
    reason: Groups related effects together for easier scanning by GM
  - id: 31-02-04
    decision: Modal returns List<string> of effect names (not IDs) for removal
    reason: Name-based removal allows removing same-named effects from different characters in one operation

# Metrics
metrics:
  duration: 5 min
  completed: 2026-02-05
  tasks: 2/2
  test-results: "1075 passed, 0 failed, 0 skipped"
---

# Phase 31 Plan 02: Batch Effect Remove Summary

**Batch effect removal via union name list modal with RemoveEffectsAsync using EffectList.RemoveEffect behavior callbacks**

## What Was Done

### Task 1: Add RemoveEffectsAsync to BatchActionService
Added `RemoveEffectsAsync` method following the same sequential processing pattern as all other batch methods:
- Iterates characters sequentially (CSLA objects not thread-safe)
- For each character, finds effects matching requested names (excluding Wounds and inactive effects)
- Uses `EffectList.RemoveEffect(effectId)` which calls `behavior.OnRemove` for proper cleanup
- Only calls `UpdateAsync` if at least one effect was removed from that character
- Characters with zero matching effects are silently skipped (not counted in results, not errors)
- Tracks `TotalEffectsRemoved` across all characters for summary display
- Publishes single `CharactersUpdatedMessage` after batch completes

### Task 2: Create BatchEffectRemoveModal and Wire SelectionBar
Created three new pieces and wired them together:

**EffectNameInfo** (`GameMechanics/Batch/EffectNameInfo.cs`): Simple class holding effect name, type, and character count for the modal display.

**BatchEffectRemoveModal** (`Threa/Threa.Client/Components/Shared/BatchEffectRemoveModal.razor`): Modal with:
- Checkbox list of unique effect names found across selected characters
- Effect type icons for visual identification
- Character count badges showing how many characters have each effect
- Select All / Clear links for quick selection management
- Confirm button text shows "Remove N Effect(s)" with dynamic count
- Empty state message when no removable effects found
- Sorted by effect type then name for consistent ordering

**SelectionBar Updates**: Added "Remove Effects" button (btn-outline-warning) next to "Add Effect", with:
- `GetUnionEffectNamesAsync()`: Fetches all selected characters, builds deduplicated effect name list
- `OpenBatchEffectRemove()`: Opens modal with union list, handles result
- `ExecuteBatchEffectRemove()`: Creates request and calls `RemoveEffectsAsync`

## Decisions Made

| # | Decision | Rationale |
|---|----------|-----------|
| 1 | Fetch all selected characters to build union list | Consistent CSLA data access; ensures fresh data |
| 2 | Exclude Wounds AND inactive effects | Wounds have special lifecycle; inactive effects not removable |
| 3 | Sort by EffectType then Name | Groups related effects for easier GM scanning |
| 4 | Name-based removal (not ID-based) | Same effect name removed from all characters in one pass |

## Deviations from Plan

None - plan executed exactly as written.

## Commits

| # | Hash | Message |
|---|------|---------|
| 1 | ea4fde7 | feat(31-02): add RemoveEffectsAsync to BatchActionService |
| 2 | c45aed2 | feat(31-02): batch effect remove modal and SelectionBar wiring |

## Next Phase Readiness

Phase 31 (Batch Effects) is now complete. Both batch effect add and remove are implemented:
- **Add**: BatchEffectAddModal with template picker, custom form, modifiers, and duration options
- **Remove**: BatchEffectRemoveModal with union name list, multi-select checkboxes, and character counts

The batch character actions milestone (v1.6) is complete with all planned functionality:
- Selection infrastructure (phase 28)
- Batch damage/healing (phase 29)
- Batch visibility/dismiss (phase 30)
- Batch effect add/remove (phase 31)
