# Phase 28: Selection Infrastructure - Context

**Gathered:** 2026-02-04
**Status:** Ready for planning

<domain>
## Phase Boundary

Multi-character selection UI and state management for the GM dashboard. GMs can select multiple characters (NPCs and/or PCs) with clear visual feedback and controls. Batch actions that operate on selections are delivered in Phases 29-31.

</domain>

<decisions>
## Implementation Decisions

### Selection Visuals
- Checkbox always visible on every character card (not hover-revealed)
- Checkbox positioned in top-left corner of each card
- Selected cards show both border highlight AND background tint for strong visual distinction
- Subtle transition animation (~150ms) when toggling selection state

### Selection Controls Placement
- Select All/Deselect All controls are per-section (PCs, Hostile NPCs, Friendly NPCs separately)
- Controls appear in section header row, next to section title
- "X selected" count displays in a sticky top bar
- Sticky bar only appears when 1+ characters are selected (slides in/out)

### Selection Persistence
- Selections clear when GM navigates away from the dashboard
- Selections persist after batch actions complete (so GM can apply another action)
- Dismissed/archived characters silently drop from selection
- Escape key clears all selections

### Interaction Patterns
- Checkbox only toggles selection; clicking elsewhere on card opens details (existing behavior)
- No Shift+click range select (keep it simple)
- No Ctrl+click modifier needed (checkbox always toggles)
- Larger touch targets for checkboxes on mobile/touch devices

### Claude's Discretion
- Exact colors for border highlight and background tint (follow existing theme/Radzen patterns)
- Sticky bar styling and slide animation details
- Touch target sizing specifics
- Checkbox component choice (Radzen or custom)

</decisions>

<specifics>
## Specific Ideas

No specific requirements — open to standard approaches using existing Radzen components and theme.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 28-selection-infrastructure*
*Context gathered: 2026-02-04*
