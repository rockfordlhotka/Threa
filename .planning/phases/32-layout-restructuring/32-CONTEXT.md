# Phase 32: Layout Restructuring - Context

**Gathered:** 2026-02-11
**Status:** Ready for planning

<domain>
## Phase Boundary

Replace the two-tab combat/defense layout with a single Combat tab containing three button groups (Actions, Defense, Other) plus a left detail panel. Remove the Defense tab entirely. Remove the combat skills list from the Combat tab (skills remain on their own tab). Existing activity feed gets surfaced on the Combat tab in freed space.

</domain>

<decisions>
## Implementation Decisions

### Group arrangement
- Vertical stack of three groups: Actions, Defense, Other (top to bottom)
- Each group in a bordered card container with its label as the card header
- Buttons within each card flow horizontally with wrapping
- Groups are always expanded (not collapsible)

### Button style
- Compact tile style: icon above short label text
- Use Radzen's built-in icon set (Material Icons)
- Color-coded by group (each group has a distinct accent color)
- Conditionally-available buttons shown as disabled/grayed rather than hidden (consistent layout)

### Resource/defense left panel
- Left column on wide screens, collapses to top section on narrow/mobile screens
- No AP/FAT/VIT summary duplication (already in page header across all tabs)
- FAT/VIT detail with full breakdown: base values, active modifiers, wounds affecting pools
- Armor info: armor class + current durability per equipped armor piece

### Skills list removal
- Combat skills list removed from Combat tab entirely
- Skills remain accessible on the existing Skills tab
- Tab keeps the name "Combat"

### Activity feed
- Existing activity feed surfaced on the Combat tab in freed space
- Placement: Claude's discretion based on layout flow and available space

### Defense tab removal
- Defense tab removed from play page tab bar entirely
- All relevant content moves to new Combat tab (Defense group + left panel)
- No other content needs to be preserved elsewhere

### Claude's Discretion
- Activity feed placement (below button groups vs in left panel vs other)
- Freed space handling after skills list removal
- Exact spacing, typography, and card styling
- Specific Material Icons for each button
- Group accent color choices (fitting existing app theme)
- Responsive breakpoints for left panel collapse

</decisions>

<specifics>
## Specific Ideas

- Player wants to see combat event flow via the activity feed while using combat actions — reduces tab switching
- Buttons should maintain consistent positions even when some are disabled (no layout shifts)
- Left panel gives at-a-glance combat readiness: health pools + armor status

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 32-layout-restructuring*
*Context gathered: 2026-02-11*
