# Phase 2: GM Item Management - Context

**Gathered:** 2026-01-24
**Status:** Ready for planning

<domain>
## Phase Boundary

Web UI for Game Masters to create, edit, browse, search, filter, and manage item templates. GMs populate their game world with weapons, armor, consumables, containers, and other items that players will use. This builds on the Phase 1 business objects and provides the management interface before players interact with items in Phases 3-7.

</domain>

<decisions>
## Implementation Decisions

### List page layout
- **Data table format**: Spreadsheet-like table with sortable columns — dense, good for scanning many items
- **Minimal columns**: Show name, type, and actions only — 3-4 columns total, keeps table uncluttered
- **Click through only**: Click row to navigate to details/edit, no inline action buttons — cleaner table interface
- **Straight to edit**: Clicking a row goes directly to edit mode (not read-only detail view first) — faster workflow for GMs actively building library

### Edit form organization
- **Tabbed sections**: Organize form into tabs rather than single long scrollable form
- **Type-specific tabs**: Basic | Weapon | Armor | Container | Special tabs that show/hide based on ItemType selection — dynamic form that adapts to what's being edited
- **Inline validation only**: Error messages display directly under each invalid field (no summary at top) — clear which field has issue
- **Fixed bottom bar**: Save/Cancel buttons in fixed bottom bar always visible — no scrolling to save changes

### Complex property input
- **Inline grid editing**: For skill bonuses and attribute modifiers, use editable data grid where GMs type skill names and bonus amounts — faster for bulk entry than Add button forms
- **Checkboxes + text inputs**: Fire modes as checkbox list (Single/Burst/Auto), range bands as 4 number inputs (short/medium/long/extreme) — straightforward input for weapon properties
- **CustomProperties JSON storage**: Store bonuses, effects, fire modes, range bands in CustomProperties JSON field — flexible, already in DTO from Phase 1, matches existing pattern
- **Save on form submit**: All changes (including grid edits) save together when Save button clicked — safer, allows Cancel to revert everything

### Search and filtering
- **Debounced real-time search**: Search box waits 300ms after typing stops, then filters — balance of responsive and performant
- **Comprehensive search**: Search across Name, Description, and Tags fields — finds items by keywords in any of these
- **Single-select type filter**: "Filter by Type" dropdown to choose one ItemType at a time — simple, clear selection
- **Hybrid tag system**: Free text tags with autocomplete from existing tags — balance of flexibility (create new tags) and consistency (reuse common tags)

### Claude's Discretion
- Radzen component selection (DataGrid vs RadzenDataGrid, specific Radzen form controls)
- Tab icon/styling choices
- Exact debounce timing (300ms suggested but can adjust)
- Grid column widths and table styling
- Validation message phrasing
- Empty state messaging when no items match filters

</decisions>

<specifics>
## Specific Ideas

- Type-specific tabs should show/hide intelligently: Select "Weapon" ItemType → Weapon tab appears with fire modes/damage fields, Armor/Container tabs hidden
- Grid editing for bonuses should feel like Excel: Tab between cells, Enter to add new row, inline validation
- Search box should be prominent at top of page with clear "X" button to clear search
- Tags displayed as chips/badges with ability to click tag to filter by that tag
- Bottom action bar should be sticky even when scrolling through long forms

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 02-gm-item-management*
*Context gathered: 2026-01-24*
