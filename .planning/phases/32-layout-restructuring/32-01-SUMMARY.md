---
phase: 32-layout-restructuring
plan: 01
subsystem: ui
tags: [blazor, css, combat-tab, tile-buttons, layout, themes]

# Dependency graph
requires: []
provides:
  - "Combat tile CSS (.combat-tile, .combat-tile-actions/defense/other)"
  - "Restructured TabCombat Default mode with three card groups"
  - "Left panel with health pools and armor info"
  - "Activity feed rendering area with parameters"
  - "ActivityLogEntry record type on TabCombat"
affects:
  - "32-02 (activity log wiring to TabCombat parameters)"
  - "32-03 (any further combat UI polish)"

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Combat tile buttons: compact icon-above-label tiles using .combat-tile CSS class"
    - "Group accent colors via CSS custom properties (red=actions, blue=defense, secondary=other)"
    - "All combat buttons always rendered, disabled when unavailable (no @if hiding)"

key-files:
  created: []
  modified:
    - "Threa/Threa/wwwroot/css/themes.css"
    - "Threa/Threa.Client/Components/Pages/GamePlay/TabCombat.razor"

key-decisions:
  - "Defined ActivityLogEntry as public record in TabCombat @code block (mirrors Play.razor's private record for parameter passing)"
  - "All buttons always rendered and disabled when unavailable instead of hidden with @if"
  - "Target tiles shown for both melee and ranged, disabled when no targets available"
  - "Wound display uses EffectType.Wound count matching Play.razor header pattern"

patterns-established:
  - "Combat tile pattern: <button class='btn combat-tile combat-tile-{group}'> with icon + span label"
  - "Group card pattern: <div class='card combat-group-{name}'> with accent-line card-header"

# Metrics
duration: 6min
completed: 2026-02-12
---

# Phase 32 Plan 01: Layout Restructuring Summary

**Combat tab restructured from two-column skills+buttons to left-panel + three-group compact tile layout with health/armor detail and activity feed**

## Performance

- **Duration:** 6 min
- **Started:** 2026-02-12T05:26:26Z
- **Completed:** 2026-02-12T05:32:41Z
- **Tasks:** 2
- **Files modified:** 2

## Accomplishments
- Added 93 lines of combat tile CSS with group accent colors, sci-fi glow effects, and card header accent lines
- Replaced 194 lines of old two-column layout with 237 lines of new three-group card layout
- Left panel shows FAT/VIT progress bars with color-coded values, wound count with AS penalty, and equipped armor with durability bars
- All 11 combat action buttons always visible, disabled when unavailable (Ranged, Implants, Reload, Unload no longer hidden)
- Activity feed area ready for Plan 02 wiring with ActivityLogEntry record and GetActivityCssClass delegate

## Task Commits

Each task was committed atomically:

1. **Task 1: Add combat tile CSS to themes.css** - `fb86c13` (feat)
2. **Task 2: Restructure TabCombat.razor Default mode** - `6fe2983` (feat)

## Files Created/Modified
- `Threa/Threa/wwwroot/css/themes.css` - Combat tile button styles, group accent colors, sci-fi glow, card header accent lines
- `Threa/Threa.Client/Components/Pages/GamePlay/TabCombat.razor` - Three-group Default mode layout, left health/armor panel, activity feed area, ActivityLogEntry type

## Decisions Made
- Defined `ActivityLogEntry` as a public record directly in TabCombat's @code block rather than creating a shared file. This mirrors Play.razor's private record and keeps the type close to its consumer. Plan 02 can refactor to a shared type if needed when wiring the parameters.
- All combat buttons rendered unconditionally with `disabled` attribute instead of `@if` conditionals. This provides better discoverability -- players see all available actions and understand what is currently unavailable rather than wondering where a button went.
- Target tiles (melee + ranged) shown alongside their attack counterparts in the Actions group, disabled when `HasTargetsAvailable()` returns false.

## Deviations from Plan
None - plan executed exactly as written.

## Issues Encountered
None.

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- TabCombat.razor has `ActivityEntries` and `GetActivityCssClass` parameters ready for Plan 02 wiring
- CSS combat tile classes available for any future combat UI components
- Build passes with 0 errors

---
*Phase: 32-layout-restructuring*
*Completed: 2026-02-12*
