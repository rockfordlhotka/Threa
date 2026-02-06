# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-02-04)

**Core value:** Players and Game Masters can easily access the system, manage their content securely, and focus on gameplay rather than administration.
**Current focus:** Phase 30 - Batch Visibility & Lifecycle

## Current Position

Milestone: v1.6 Batch Character Actions
Phase: 30 of 31 (Batch Visibility & Lifecycle)
Plan: 1 of 2 in current phase
Status: In progress
Last activity: 2026-02-05 - Completed 30-01-PLAN.md

Progress: [███████░░░░░░░░░░░░░] 67% (6/9 plans)

## Performance Metrics

**Velocity:**
- Total plans completed: 84
- Average duration: 7.7 min
- Total execution time: ~10.6 hours

**By Milestone:**

| Milestone | Phases | Plans | Total Time | Avg/Plan |
|-----------|--------|-------|------------|----------|
| v1.0 | 7 | 16 | 238 min | 15 min |
| v1.1 | 4 | 8 | 92 min | 11.5 min |
| v1.2 | 5 | 14 | 121 min | 8.6 min |
| v1.4 | 6 | 21 | 48 min | 6 min |
| v1.5 | 5 | 20 | 105 min | 5.3 min |
| v1.6 | 4 | 9 | 39 min | 6.5 min |

## Accumulated Context

### Decisions

All decisions are logged in PROJECT.md Key Decisions table.

**v1.6 Decisions (from research):**
- Selection state as HashSet<int> in GmTable.razor (component-level)
- Sequential processing pattern (mirrors TimeAdvancementService)
- Single CharactersUpdatedMessage after batch completes (prevents message flooding)
- Structured BatchActionResult for partial failure handling

**28-01 Decisions:**
- Checkbox container handles click with stopPropagation (not RadzenCheckBox directly)
- Compact checkbox variant for HiddenNpcCard (smaller touch target)
- IsMultiSelected/IsSelectable/OnSelectionChanged as standard selection parameters

**28-02 Decisions:**
- SelectionBar uses transform/opacity transition for slide-in animation
- Hidden section Select All buttons only visible when section expanded
- Escape key handling uses tabindex="0" on container (component scoped)
- Selection cleanup uses HashSet.RemoveWhere for efficient stale removal

**29-01 Decisions:**
- BatchActionRequest as record type (not class) to support 'with' expression in service

**29-02 Decisions:**
- BatchInputResult in GameMechanics.Batch namespace (standalone .cs file, not in razor component)

**29-03 Decisions:**
- Result feedback uses HandleBatchResult public method pattern
- Deselect All also dismisses any active result display

**30-01 Decisions:**
- ITableDal injected into BatchActionService for dismiss table-detach (existing DI registration)
- Non-NPC characters silently skipped (not counted) for visibility and dismiss operations
- VisibilityTarget defaults to true (reveal) when null

### Pending Todos

None.

### Blockers/Concerns

None.

**Known Technical Debt (non-blocking, from v1.0):**
- ArmorInfoFactory.cs orphaned
- Weapon filtering logic in UI layer
- Case sensitivity inconsistencies

## Session Continuity

Last session: 2026-02-05
Stopped at: Completed 30-01-PLAN.md
Resume file: None
Next action: Execute 30-02-PLAN.md

---
*Milestone v1.6 Batch Character Actions roadmap created 2026-02-04*
*28-01 completed 2026-02-04: Selection state and checkbox infrastructure*
*28-02 completed 2026-02-04: Selection controls, Escape key, cleanup*
*29-01 completed 2026-02-05: BatchActionService backend with sequential processing*
*29-02 completed 2026-02-05: SelectionBar batch action buttons with DialogService modal*
*29-03 completed 2026-02-05: Batch result feedback, selection cleanup, CSS styling*
*30-01 completed 2026-02-05: BatchActionService visibility toggle and dismiss/archive methods*
*Previous milestone archives: .planning/milestones/*
