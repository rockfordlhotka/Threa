# Phase 33: Attack Actions - Context

**Gathered:** 2026-02-12
**Status:** Ready for planning

<domain>
## Phase Boundary

Wire melee and ranged attack buttons in the Actions group (from Phase 32 layout) to existing attack flows. Add anonymous target verification: AV display for solo melee, TV modifier input + SV display for solo ranged. No new attack logic — this connects existing combat resolution to the new button layout.

Requirements: ACT-01, ACT-02, VER-01, VER-02

</domain>

<decisions>
## Implementation Decisions

### Button Wiring Behavior
- Melee attack button enters attack mode directly on click (no intermediate step)
- Ranged attack button enters ranged attack mode directly on click
- Attack UI (weapon selection, target selection, roll, results) **replaces the three button groups** when in attack mode — groups revert after resolution
- Player must **explicitly dismiss** the result (click Done/Back) to return to default button groups — no auto-return
- Melee and ranged buttons sit **side by side in the same row** within the Actions group

### Anonymous Target Results
- Solo melee: AV displays **in the result area** after rolling (alongside dice result), not before
- Solo ranged: single number field for total TV modifier (player calculates range/cover/etc. themselves) — no breakdown fields
- Solo ranged result shows **SV only** (roll - TV) — player interprets the result, no auto damage lookup
- Anonymous target attack results **log to the activity feed** like normal attacks, for session history

### Ranged Weapon Visibility
- When no ranged weapon equipped: button is **visible but disabled** (grayed out, tooltip "No ranged weapon equipped")
- All ranged types enable the button: bows, thrown weapons, and firearms
- Ranged weapons with melee skills (e.g., using bow as club) appear in the melee weapon picker too — but melee weapons never appear in ranged
- Melee weapon picker **always shows** because punch/kick hand-to-hand options are always available — never skip the picker

### Solo Attack Flow UX
- Target list always shown — "Anonymous Target" is an explicit option alongside any real targets (if any)
- Player must **click to roll** (never auto-roll) because AP bonuses can always be applied before rolling
- Solo attacks have **full AP/FAT cost** — they are real actions in game time, not free practice
- Solo ranged: TV modifier input appears **before the roll** — player enters TV, then rolls, then sees SV

### Claude's Discretion
- Exact layout/styling of the attack mode panel that replaces button groups
- How the weapon picker renders (dropdown, tiles, list)
- Dismiss button styling and placement
- How AP bonus application UI integrates with the attack flow
- Disabled ranged button tooltip text

</decisions>

<specifics>
## Specific Ideas

- Hand-to-hand combat (punch/kick) was a dedicated feature effort — the melee picker must always include these options regardless of equipped weapons
- The existing attack flow already handles weapon selection, dice rolling, AP costs, etc. — this phase wires buttons to those flows, not rebuilding them
- Anonymous target is a first-class target option in the list, not a fallback for when no targets exist

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 33-attack-actions*
*Context gathered: 2026-02-12*
