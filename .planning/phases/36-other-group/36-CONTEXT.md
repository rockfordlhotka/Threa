# Phase 36: Other Group - Context

**Gathered:** 2026-02-13
**Status:** Ready for planning

<domain>
## Phase Boundary

Wire medical, rest, implants, reload, and unload utility actions into the Other group on the Combat tab. All five buttons and mode components already exist from Phase 32 — this phase changes conditional buttons from disabled to hidden, adds a Rest confirmation flow, and applies consistent cost-aware dimming across all combat tile groups.

</domain>

<decisions>
## Implementation Decisions

### Button visibility vs disabled
- Conditional buttons (Implants, Reload, Unload) are **hidden entirely** when their conditions aren't met — not shown as disabled
- Medical and Rest are **always visible** regardless of state (disabled when passed out or no AP, but never hidden)
- Other group always shows (Medical + Rest guarantee it's never empty)
- Hidden buttons use a **subtle CSS transition** (fade/slide) when appearing/disappearing mid-combat

### AP/FAT cost-aware dimming
- Tooltips stay as the cost display method — no visible badges or subtitles on tiles
- Buttons dim (like Defend's `combat-tile-passive-only` pattern) when AP is too low to use them
- Dimmed tooltip explains the **cost** (e.g., "Reload requires 1 AP + 1 FAT") rather than the blocker
- **Apply dimming pattern to ALL combat tile groups** (Actions, Defense, Other) for consistency — not just Other group

### Rest confirmation flow
- Rest gets an **inline confirmation panel** (like other combat modes) instead of executing instantly
- Panel shows simple text: "Spend 1 AP to recover 1 FAT" with Confirm/Cancel
- No AP/FAT value preview — just the action description
- After completion, **activity log only** — no extra visual feedback on bars

### Conditional visibility triggers
- **Reload**: visible when any ranged weapon is in equipment slots (matches existing `hasRangedWeaponEquipped`)
- **Unload**: visible when ANY equipped weapon has ammo loaded (mode lets you pick which weapon)
- **Implants**: visible when character has toggleable implants equipped only (passive implants don't count)
- Buttons appear/disappear **immediately** via Blazor reactivity when conditions change mid-combat

### Claude's Discretion
- Exact CSS transition timing and easing for button appear/disappear
- How to structure the Rest confirmation as a combat mode vs inline component
- Implementation details of cost-aware dimming across groups

</decisions>

<specifics>
## Specific Ideas

- Rest confirmation should follow the same inline panel pattern as other combat modes (replaces button area with confirm/cancel)
- Dimming pattern should reuse/extend the existing `combat-tile-passive-only` CSS class from Phase 35's Defend button

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 36-other-group*
*Context gathered: 2026-02-13*
