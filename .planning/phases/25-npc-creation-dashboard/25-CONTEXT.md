# Phase 25: NPC Creation & Dashboard - Context

**Gathered:** 2026-02-02
**Status:** Ready for planning

<domain>
## Phase Boundary

GMs can spawn NPCs from templates during play and see them in the GM dashboard with full manipulation powers. NPCs appear in a separate section, grouped by disposition, and use the same CharacterDetailModal as PCs. Creating templates is Phase 24; visibility/lifecycle is Phase 26.

</domain>

<decisions>
## Implementation Decisions

### Spawn Workflow
- Spawn available from BOTH template library page AND GM dashboard
- Quick modal opens on spawn (not immediate) — allows name/disposition customization
- One NPC spawned per modal submission (no batch quantity)
- Modal includes disposition selector to override template default at spawn time

### Dashboard Layout
- NPCs in separate section BELOW PCs with clear header
- NPCs grouped by disposition (Hostile, Neutral, Friendly sub-groups)
- Empty disposition groups are HIDDEN (only show groups with NPCs)
- "+ Spawn NPC" button in the NPC section header

### Auto-naming
- Custom prefix + sequential number format (e.g., "Bandit 1", "Bandit 2")
- Spawn modal has editable name field pre-filled with auto-generated name
- GM can override to any custom name
- GLOBAL counter across all NPCs (not per-template)
- Custom prefix is REMEMBERED per template for future spawns in the session

### NPC Status Cards
- IDENTICAL layout to PC cards (same CharacterStatusCard component)
- Disposition indicator: ICON ONLY (skull/circle/heart) — no text badge
- Small muted label showing source template (e.g., "From: Goblin Warrior")
- Session notes visible ONLY in detail modal (no indicator on card)

### Claude's Discretion
- Exact icons for disposition (skull/circle/heart or alternatives)
- Modal styling and button placement
- How prefix memory persists (session state vs database)
- Sort order within disposition groups

</decisions>

<specifics>
## Specific Ideas

- Disposition icons should be immediately recognizable — red skull for Hostile makes sense
- Template label on card helps GM remember which template an NPC came from when many are in play
- Global counter prevents confusion ("which Goblin 2 is this?")

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 25-npc-creation-dashboard*
*Context gathered: 2026-02-02*
