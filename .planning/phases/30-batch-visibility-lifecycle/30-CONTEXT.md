# Phase 30: Batch Visibility & Lifecycle - Context

**Gathered:** 2026-02-05
**Status:** Ready for planning

<domain>
## Phase Boundary

Batch operations for toggling NPC visibility (reveal/hide) and dismissing/archiving multiple NPCs at once. Extends the SelectionBar action row from Phase 29 with new buttons. PCs are silently excluded from these NPC-only operations.

</domain>

<decisions>
## Implementation Decisions

### Action bar layout
- Flat row of buttons (no grouped sections): Clear Selection | Damage | Heal | Toggle Visibility | ... | Dismiss
- Clear Selection button at far left (before action buttons)
- Dismiss button isolated at far right end (destructive action separation)
- Toggle Visibility is a single contextual button (not separate Reveal/Hide buttons)

### Confirmation UX
- Dismiss always requires confirmation via DialogService modal (same pattern as damage/heal modals from Phase 29)
- Dismiss confirmation shows simple count: "Dismiss 5 NPCs?" with confirm/cancel
- Visibility toggle does NOT require confirmation (non-destructive, reversible)

### Mixed selection handling
- PCs are silently skipped for visibility/dismiss operations — apply to NPCs only
- If ALL selected characters are PCs (no NPCs), visibility/dismiss buttons are hidden (not rendered)
- Visibility toggle with mixed visible/hidden NPCs: reveal all (reveal takes priority over true toggle)
- Result feedback shows only successes: "3 NPCs revealed" (does NOT mention skipped PCs)

### Post-action state
- After dismiss: only dismissed NPCs are removed from selection; remaining selected characters stay selected
- After visibility toggle: selection remains (GM may want to chain additional actions)
- Batch result feedback shows after dismiss ("Dismissed 5 NPCs") even though NPCs vanish from table
- Batch result feedback shows after visibility toggle using same Phase 29 result pattern

### Claude's Discretion
- Button icons and visual styling
- Exact dismiss confirmation modal layout
- How visibility toggle button label updates based on selection state
- Error handling for edge cases (e.g., NPC already dismissed by another action)

</decisions>

<specifics>
## Specific Ideas

- Clear Selection button was explicitly requested as part of this phase — lives in the action button row at far left
- Dismiss is the most destructive batch action — isolation at far right communicates this
- "Reveal all" semantics for mixed visibility state keeps the toggle simple and predictable

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 30-batch-visibility-lifecycle*
*Context gathered: 2026-02-05*
