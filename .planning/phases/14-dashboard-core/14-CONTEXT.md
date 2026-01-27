# Phase 14: Dashboard Core - Context

**Gathered:** 2026-01-27
**Status:** Ready for planning

<domain>
## Phase Boundary

GM dashboard displaying compact character status cards for real-time table monitoring. Shows Fatigue/Vitality health pools, pending damage/healing, current Action Points, wound count, and active effects count for all characters at the table. Detailed character views and NPC management are separate phases.

</domain>

<decisions>
## Implementation Decisions

### Card Layout and Information Density
- Grid layout with cards that flow and wrap responsively
- Balanced visual hierarchy: character name and health bars get equal visual weight
- Minimal information density: name, health bars (visual only), AP badge, wounds badge, effects badge
- Wounds displayed as count badge with icon, hoverable for details
- Effects displayed as single badge with total count
- Color-coded card borders or backgrounds to indicate character state (green=healthy, yellow=wounded, red=critical)

### Health Pool Visualization
- Stacked progress bars: Fatigue above Vitality
- Current and maximum values shown on hover/tooltip only (bars are visual indicators)
- Pending damage and healing integrated into bars with segments (current solid, pending damage striped red, pending healing striped green)
- Distinct base colors for each pool: blue for Fatigue, red for Vitality
- No dynamic color shifts based on depletion percentage (state indicated by card border instead)

### Status Indicators and Effects Display
- Action Points: Simple badge with number
- Wounds: Small badge with count number and wound icon
- Wounds tooltip on hover: Shows list of wound types (Minor x2, Serious x1, etc.)
- Active effects: Single badge with total count
- Effects tooltip on hover: Shows list of effect names with durations where relevant (e.g., "Poison (3 rounds), Blessed, Shield Spell")
- All badges support hover for detail lists without requiring clicks

### Empty and Loading States
- Empty state: Minimal status message ("Waiting for players to join")
- Character add/remove: Instant appearance/disappearance, no loading indicators
- Initial dashboard load: Cards appear progressively as individual character data loads (no skeleton UI)
- Error handling: Failed character cards show error message with retry button

### Claude's Discretion
- Exact card dimensions and spacing
- Specific badge styling and positioning
- Animation timing for real-time updates (covered in Phase 15)
- Tooltip styling and positioning
- Border/background color shades for state indication

</decisions>

<specifics>
## Specific Ideas

No specific requirements — open to standard approaches within the defined patterns.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 14-dashboard-core*
*Context gathered: 2026-01-27*
