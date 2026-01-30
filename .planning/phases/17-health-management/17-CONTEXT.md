# Phase 17: Health Management - Context

**Gathered:** 2026-01-28
**Status:** Ready for planning

<domain>
## Phase Boundary

GM applies damage and healing to character FAT/VIT pools through the existing character modal in the dashboard. This phase extends the modal's current FAT/VIT display with controls for applying numeric damage/healing values. Changes integrate with the existing pending pool system from v1.2 and trigger real-time dashboard updates.

</domain>

<decisions>
## Implementation Decisions

### Input Interface Design
- Separate numeric inputs for FAT and VIT (side-by-side or stacked in modal)
- Damage/healing mode toggle - GM switches between "Apply Damage" and "Apply Healing" modes, then enters positive values
- All controls live in the existing character modal window (no inline table controls or quick shortcuts)
- Modal-only interaction - no quick-access buttons in character rows

### Value Application Flow
- Immediate application when values are entered/submitted
- Modal uses existing close behavior (ESC key or click outside bounds)
- Always add to pending - new damage/healing values accumulate with existing pending FAT/VIT amounts
- Warn on overkill/overheal - show warning if healing exceeds max pools or damage would drop below zero

### Pool Interaction Behavior
- Damage overflow with confirmation - warn GM when FAT damage would overflow to VIT, let them confirm or adjust
- Separate healing inputs per pool - GM controls FAT healing and VIT healing independently
- Allow simultaneous operations - GM can damage FAT while healing VIT in the same action
- Allow temporary overheal - pools can exceed maximum values (for buffs, special abilities, etc.)

### Visual Feedback and States
- Progress bars with overlays showing current fill, max capacity, and pending changes
- Color-coded thresholds: Green > 50%, Yellow 25-50%, Red < 25%
- Visual animation only for confirmation - bars animate from old to new values when changes apply
- No distinction between GM-applied vs combat-generated changes

### Claude's Discretion
- Exact layout and spacing of FAT/VIT inputs within modal
- Specific wording for overflow warnings and overheal notifications
- Progress bar animation timing and easing
- Input field validation patterns (prevent non-numeric, handle empty, etc.)

</decisions>

<specifics>
## Specific Ideas

- Integrate seamlessly with the existing modal's FAT/VIT functionality from v1.2
- Damage/healing mode toggle should be prominent and clear
- Progress bars should make pending changes immediately obvious before they're applied
- Color transitions on health bars should be smooth and not jarring

</specifics>

<deferred>
## Deferred Ideas

None - discussion stayed within phase scope

</deferred>

---

*Phase: 17-health-management*
*Context gathered: 2026-01-28*
