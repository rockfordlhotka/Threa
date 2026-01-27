# Phase 15: Dashboard Details - Context

**Gathered:** 2026-01-27
**Status:** Ready for planning

<domain>
## Phase Boundary

GM can view detailed character information and dashboard updates in real-time. This phase extends the compact status cards from Phase 14 with drill-down capabilities, comprehensive character views, and live synchronization. The dashboard becomes a fully interactive, real-time monitoring hub for all player characters at the table.

</domain>

<decisions>
## Implementation Decisions

### Detail Panel Layout
- Modal overlay (large, centered, 80-90% of screen)
- Dark backdrop, Escape key and X button to close
- Minimal header: character name + close button only
- Character switcher dropdown in header for navigating between characters without closing modal
- GM can switch to any character's details from dropdown while modal remains open

### Information Organization
- Three tabs across the top: Character Sheet | Inventory | Narrative
- **Character Sheet tab:** Full character sheet replica (mirrors player's view exactly)
- **Inventory tab:** Equipped items shown in slots + separate list of carried items
- **Narrative tab:** Appearance + Backstory + Notes + **GM-only notes section** (private observations)
- GM notes persist per character, not visible to players

### Real-time Update Strategy
- Dashboard leverages Blazor Server's built-in data binding for automatic reactivity
- Subscribe to multiple message types: TimeAdvanceMessage, CharacterUpdateMessage, EffectExpiredMessage, etc.
- No visual indicator on updates (silent update) - values change seamlessly
- If detail modal is open when update occurs: update modal content in place (no disruption)
- Character data refreshes automatically via Rx.NET message subscriptions

### NPC Placeholder Design
- Collapsed/expandable section at bottom of dashboard
- Labeled section header: "NPCs" with expand/collapse icon
- When expanded: static image or illustration (mockup/wireframe) showing future NPC card concept
- Sets expectation for future functionality without implying it's available now

### Claude's Discretion
- Modal animation timing and easing
- Exact tab styling and transitions
- GM notes field size and formatting
- Mockup/wireframe visual design
- Message subscription initialization and cleanup patterns

</decisions>

<specifics>
## Specific Ideas

- **Blazor Server data binding:** "Data binding should make things refresh automatically in the browser, as long as the server updates the data values based on inbound Rx.NET messages or other factors that trigger changes" - leverage existing reactivity model
- Character switcher dropdown enables GMs to quickly compare characters without returning to dashboard
- GM notes section is private workspace for tracking character-specific observations, plot hooks, or session notes
- NPC placeholder mockup shows what's coming without being interactable (prevents confusion)

</specifics>

<deferred>
## Deferred Ideas

None - discussion stayed within phase scope

</deferred>

---

*Phase: 15-dashboard-details*
*Context gathered: 2026-01-27*
