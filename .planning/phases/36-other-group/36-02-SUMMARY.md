---
phase: 36-other-group
plan: 02
subsystem: ui
tags: [blazor, combat-tab, css, dimming, tooltips, accessibility]

# Dependency graph
requires:
  - phase: 36-01
    provides: combat-tile-dimmed CSS class, @if-guarded conditional buttons, CombatMode.Rest
  - phase: 35-defense-group
    provides: combat-tile-passive-only CSS pattern (now removed)
provides:
  - "Consistent cost-aware dimming across all three combat tile groups (Actions, Defense, Other)"
  - "Cost-explaining tooltips on all dimmed combat buttons"
  - "Disabled attribute reserved for hard blocks only (IsPassedOut, no weapon, no targets)"
  - "Consolidated combat-tile-dimmed replaces combat-tile-passive-only"
affects: []

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Dimmed-but-clickable pattern: CSS dimming for resource-insufficient states, disabled only for hard precondition failures"
    - "Cost-explaining tooltips: conditional title attributes that switch between action description and cost requirement"

key-files:
  created: []
  modified:
    - "Threa/Threa.Client/Components/Pages/GamePlay/TabCombat.razor"
    - "Threa/Threa/wwwroot/css/themes.css"

key-decisions:
  - "Disabled attribute reserved for hard blocks only: IsPassedOut, no weapon equipped, no targets available"

patterns-established:
  - "Dimming = CSS class (clickable), Disabled = HTML attribute (blocked). Two-tier visual feedback system."
  - "Tooltip content switches based on resource state to explain why action is dimmed"

# Metrics
duration: 4min
completed: 2026-02-13
---

# Phase 36 Plan 02: Cost-Aware Dimming Across All Combat Groups Summary

**combat-tile-dimmed applied to 12 buttons across Actions/Defense/Other groups with cost-explaining tooltips, combat-tile-passive-only removed**

## Performance

- **Duration:** 4 min
- **Started:** 2026-02-13T16:45:02Z
- **Completed:** 2026-02-13T16:49:27Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments
- Applied combat-tile-dimmed class to all 6 Actions group buttons when !CanAct()
- Replaced combat-tile-passive-only with combat-tile-dimmed on Defend button
- Applied combat-tile-dimmed to Medical, Rest, Reload, Unload, and Implants buttons in Other group
- Changed disabled from resource checks to hard blocks only (IsPassedOut + equipment preconditions)
- Added cost-explaining tooltips that switch between action description and cost requirement
- Removed stale combat-tile-passive-only CSS class from themes.css

## Task Commits

Each task was committed atomically:

1. **Task 1: Apply dimming and cost tooltips to Actions group buttons** - `a153b61` (feat)
2. **Task 2: Apply dimming to Defense and Other groups, consolidate CSS** - `307cf15` (feat)

## Files Created/Modified
- `Threa/Threa.Client/Components/Pages/GamePlay/TabCombat.razor` - 12 buttons with conditional combat-tile-dimmed class, cost-explaining tooltips, disabled reserved for hard blocks
- `Threa/Threa/wwwroot/css/themes.css` - Removed combat-tile-passive-only, consolidated to single combat-tile-dimmed class

## Decisions Made
- Disabled attribute is only used for hard blocks: IsPassedOut, no weapon equipped, no targets available. Resource insufficiency (AP/FAT) uses CSS dimming only, keeping buttons clickable.
- Rest button dimming uses CanRest() (not CanAct()) since Rest has different resource requirements (1 AP only, no FAT cost)
- Implants button dimming uses AP < 1 check directly since implant activation has variable FAT cost per toggle

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Phase 36 is now complete (both plans executed)
- All combat tile groups have consistent visual feedback system
- No blockers remain

---
*Phase: 36-other-group*
*Completed: 2026-02-13*
