---
phase: 21-stat-editing
plan: 02
subsystem: ui
tags: [blazor, skills, edit-mode, validation, ap-calculation, csla]

# Dependency graph
requires:
  - phase: 21-stat-editing-01
    provides: Edit mode infrastructure, OnEditStatsRequested callback
provides:
  - Skill level editing with all skills visible in edit mode
  - AP Max preview with total skill levels display
  - Inline skill validation (red for <0, yellow for >10)
  - Combined save for both attributes and skills
  - AP capping when skill levels reduce AP max
  - Tab switching with edit mode from GM Actions button
affects: [gm-dashboard, character-management]

# Tech tracking
tech-stack:
  added: []
  patterns: [unified-save, tab-coordination, parent-child-edit-mode]

key-files:
  created: []
  modified:
    - Threa/Threa.Client/Components/Shared/CharacterDetailSheet.razor
    - Threa/Threa.Client/Components/Shared/CharacterDetailModal.razor

key-decisions:
  - "Skills card gets pencil button matching Attributes card pattern"
  - "All skills shown in edit mode (not just leveled ones) for full editing"
  - "AP Max preview at top of skills list with total levels count"
  - "Combined SaveAllChanges handles both attributes and skills in single save"
  - "AP capped at new max when skills reduced (matching health pool capping)"

patterns-established:
  - "Tab coordination: parent tracks isEditMode, child receives via parameter"
  - "SwitchToSheetAndEdit pattern for cross-tab edit mode triggers"

# Metrics
duration: 2.4min
completed: 2026-01-29
---

# Phase 21 Plan 02: Skill Editing Summary

**GM skill level editing with AP Max preview, validation warnings, combined save with attribute changes, and tab coordination**

## Performance

- **Duration:** 2.4 min (146 seconds)
- **Started:** 2026-01-29T20:21:03Z
- **Completed:** 2026-01-29T20:23:29Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments
- Pencil button on Skills card header for entering edit mode
- All skills displayed in edit mode (not just those with levels > 0)
- AP Max preview at top of skills list shows current max and total skill levels
- Skills grouped by primary attribute with number inputs
- Skill validation: red styling for <0 (blocks save), yellow warning for >10
- Combined CanSaveAll validates both attributes and skills
- SaveAllChanges handles both attribute and skill changes with AP capping
- CharacterDetailModal now coordinates edit mode between tabs
- "Edit Stats" button in GM Actions switches to Sheet tab AND enters edit mode

## Task Commits

Each task was committed atomically:

1. **Task 1: Add skill editing to CharacterDetailSheet** - `5651c46` (feat)
2. **Task 2: Wire CharacterDetailModal to support edit mode coordination** - `7c017ed` (feat)

## Files Created/Modified
- `Threa/Threa.Client/Components/Shared/CharacterDetailSheet.razor` - Added skill editing UI with AP preview, validation, and unified save
- `Threa/Threa.Client/Components/Shared/CharacterDetailModal.razor` - Added isEditMode state, wired parameters to sheet, added SwitchToSheetAndEdit

## Decisions Made
- Pencil button on Skills card header (matching Attributes card pattern)
- All skills visible in edit mode for complete editing capability
- AP Max preview shows both current value and total skill levels for transparency
- Combined save approach - attributes and skills saved together in one operation
- AP capped at new max when skill levels reduced (same pattern as FAT/VIT capping)

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None

## User Setup Required

None - no external service configuration required.

## Phase Completion

This completes Phase 21 - Stat Editing:
- Plan 01: Attribute editing with base values, effective badges, validation
- Plan 02: Skill editing with AP preview, combined save, tab coordination

The GM can now:
1. Click pencil on Attributes or Skills card to enter edit mode
2. Click "Edit Stats" in GM Actions to switch to Sheet tab and enter edit mode
3. Edit all attributes and skills in a single edit session
4. See real-time FAT/VIT/AP max previews as values change
5. Validation prevents invalid saves (attributes <1, skills <0)
6. Health pools and AP auto-cap at new max on save

---
*Phase: 21-stat-editing*
*Completed: 2026-01-29*
