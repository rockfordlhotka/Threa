# Phase 34: New Action Types - Context

**Gathered:** 2026-02-12
**Status:** Ready for planning

<domain>
## Phase Boundary

Two new standalone actions in the Actions group: Anonymous Action (attribute-only roll vs TV) and Use Skill (combat skill check via modal). Neither triggers the attack workflow. Both use the same cost model and inline panel pattern for rolling.

</domain>

<decisions>
## Implementation Decisions

### Anonymous Action flow
- Opens as an inline panel below the button (like existing attack modes), not a modal
- Step order: Attribute dropdown → Cost choice → TV input → AP bonus → Roll
- Attribute dropdown shows all 7 attributes (STR, DEX, END, INT, ITT, WIL, PHY)
- AP bonus works exactly like other skills — extra AP spent for +1 bonus each

### Skill check modal
- Player clicks Use Skill, modal opens with skills grouped by governing attribute
- Each skill shows name + AS value (e.g., "Stealth (AS 12)")
- All character skills shown, not just combat-tagged ones
- AS must be calculated using active (modified) attribute and skill levels, including any global AS modifiers
- After selecting a skill, modal closes and transitions to an inline panel (same pattern as Anonymous Action) with the skill pre-selected

### Result display
- Results show both inline (in the panel) AND logged to the activity feed
- Standard detail level with full roll breakdown:
  - Skill checks: "AS 12 + [+1,0,+1,-1] = 13 vs TV 10 → SV +3"
  - Anonymous actions: "STR 7 + [+1,+1,0,-1] = 8 vs TV 10 → SV -2"
- Result stays until player manually dismisses (close/done button), then returns to Combat tab default

### Cost handling
- Both Anonymous Action and Use Skill share the same cost model: 1AP+1FAT or 2AP
- Cost toggle appears during setup (before rolling), as part of the inline panel
- If a cost option is unaffordable, it is disabled/grayed out; the other auto-selects if available
- AP bonus cost displayed separately from base cost (not combined into a live total)

### Claude's Discretion
- Exact inline panel layout and spacing
- How the cost toggle is rendered (radio buttons, segmented control, etc.)
- Activity log entry format and styling
- How attribute groups are ordered in the skill modal

</decisions>

<specifics>
## Specific Ideas

- Both action types should feel consistent — same inline panel pattern, same cost toggle, same result display
- Skill modal → inline panel transition should feel seamless (modal closes, panel opens with skill context preserved)
- The AP bonus mechanism is identical to existing skill usage patterns in the codebase

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 34-new-action-types*
*Context gathered: 2026-02-12*
