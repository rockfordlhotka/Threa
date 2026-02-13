# Phase 35: Defense Group - Context

**Gathered:** 2026-02-13
**Status:** Ready for planning

<domain>
## Phase Boundary

Wire defend, take damage, and defensive stances into the Defense group on the Combat tab. Player can initiate defense rolls, take damage with full resolution, and toggle defensive stances. All three capabilities use existing game logic — this phase connects them to the new Combat tab layout.

</domain>

<decisions>
## Implementation Decisions

### Defend options flow
- Clicking Defend enters a DefendMode (inline mode panel, like AttackMode pattern)
- DefendMode shows 4 defense type buttons: passive, dodge, parry, shield block
- For active defense types, player can apply an AP bonus (same as other skill-based actions)
- Passive defense displays the defense value and auto-applies — no confirmation step needed
- Active defense types require a roll, then show result
- Result display shows DV + full breakdown (attribute + skill + roll + modifiers)

### Stance selection & display
- All 4 stances (Normal, Parry Mode, Dodge Focus, Block with Shield) shown as always-visible toggles in the Defense group
- Stances use a different visual style from the standard compact tiles — smaller toggles/chips to differentiate from action buttons
- Switching stance is a free action — no AP or fatigue cost
- All stances always visible; unavailable stances are disabled (e.g., Block with Shield without shield equipped)
- Disabled stances show tooltip explaining what's needed ("Requires shield equipped")
- Active stance pre-selects the matching defense type in DefendMode (player can still override)

### Take Damage flow
- Clicking Take Damage enters a TakeDamageMode (inline mode panel)
- Player inputs: Success Value (SV) and damage type
- Hit location: random roll by default, with optional "Called Shot" toggle to pick specific body part
- SV-based resolution only — no direct damage entry mode
- Result display: full breakdown showing hit location, armor applied, damage to FAT/VIT, wound effects if any

### Button visibility
- Defend button always visible; visual hint when no AP available for active defense (passive still works)
- Take Damage button always visible
- Defend and Take Damage use standard compact icon+label tiles (matching Actions group style)

### Claude's Discretion
- Exact icons for Defend and Take Damage tiles
- Stance toggle visual design (chips, small toggles, etc.)
- How the AP-unavailable hint appears on the Defend button
- Called Shot toggle UX and body part picker implementation
- Damage type input method (dropdown, buttons, etc.)

</decisions>

<specifics>
## Specific Ideas

- Stances should feel distinct from action buttons — smaller toggles or chips, not full tiles
- Active stance auto-selecting defense type creates a smooth flow: set stance once, then just click Defend each time
- Passive defense should be instant — no unnecessary clicks

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 35-defense-group*
*Context gathered: 2026-02-13*
