# Phase 12: Table Foundation - Context

**Gathered:** 2026-01-26
**Status:** Ready for planning

<domain>
## Phase Boundary

GM creates campaign tables and navigates to manage them. This establishes the foundational campaign entity that players will join in Phase 13. The phase covers:
- Creating new campaign tables with user-chosen names
- Selecting Fantasy or Sci-Fi theme during creation
- Setting starting epoch time for the campaign world
- Viewing a list of all campaigns the GM created
- Opening a campaign to access its management dashboard

Player joining, character status tracking, and time control belong in later phases.

</domain>

<decisions>
## Implementation Decisions

### Creation workflow
- **Navigation:** Dedicated /campaigns/create page (not modal, not inline)
- **Form layout:** Single page with all fields (name, theme, time) and submit button
- **Validation:** Real-time inline validation (errors show as user types/selects)
- **Post-creation:** Navigate directly to the newly created campaign's dashboard

### Theme selection
- **Presentation:** Dropdown/select menu (simple and functional)
- **Visual feedback:** Show preview card/panel with theme-appropriate styling when theme selected
- **Default:** Fantasy pre-selected by default
- **Theme switching:** Fix existing broken theme infrastructure during this phase - make it work properly
- **Context note:** Theme switching already exists in codebase but is currently broken; this phase repairs it

### Time initialization
- **Input method:** Single numeric input field for raw `long` epoch time value
- **Default values:** Theme-specific - Fantasy starts at 0, Sci-Fi starts at far future timestamp (Claude determines appropriate future value like year 2400 equivalent)
- **Validation:** Accept full valid `long` range including negative values (Unix epoch allows pre-1970 times)
- **Future scope:** Human-readable time translation (per-theme calendar views) is deferred to future phase

### Campaign list view
- **Display format:** Data table with rows (dense, organized)
- **Columns displayed:** Campaign name, Theme indicator, Current time (epoch), Created date
- **Sorting:** Fixed sort order - newest first (most recently created at top)
- **No sorting controls:** Keep simple - no column header sorting UI

### Claude's Discretion
- Exact theme preview styling details (colors, borders, icons)
- Specific far-future timestamp value for Sci-Fi default
- Error message wording and formatting
- Campaign dashboard initial view (what shows when GM opens campaign after creation)
- Theme indicator visual treatment (icon, text, badge style)
- Table styling and responsive behavior

</decisions>

<specifics>
## Specific Ideas

- Theme switching infrastructure already exists but is broken - needs repair not rebuild
- Raw `long` epoch time is intentional simplification - human-readable translation comes later
- Unix epoch semantics apply - negative values represent times before Jan 1, 1970
- Newest-first sort matches typical "most recent work first" user expectation

</specifics>

<deferred>
## Deferred Ideas

- Human-readable time display with per-theme calendars (e.g., "Year 1, Day 52" for Fantasy, "Stardate 2405.3" for Sci-Fi) - future phase
- Campaign search/filtering - not needed until GMs have many campaigns
- Campaign editing (rename, change theme, etc.) - separate feature, not in this phase
- Campaign archiving/deletion - separate feature, not in this phase

</deferred>

---

*Phase: 12-table-foundation*
*Context gathered: 2026-01-26*
