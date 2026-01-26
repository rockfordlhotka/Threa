# Phase 6: Item Bonuses & Combat - Context

**Gathered:** 2026-01-25
**Status:** Ready for planning

<domain>
## Phase Boundary

Equipped items provide stat bonuses (attribute and skill) that affect character effectiveness, and integrate with the combat system so weapons and armor influence attack resolution and damage calculation. This connects the inventory system (Phases 1-5) with existing combat mechanics.

</domain>

<decisions>
## Implementation Decisions

### Bonus Calculation Timing
- Bonuses computed on-demand using computed properties (no cached state)
- Calculate every time stats are accessed - always fresh, always accurate
- Both attribute bonuses (cascade to all related skills) and skill-specific bonuses supported per item
- Layered calculation: item bonuses applied to base stats first, then effect bonuses (spells, wounds) layer on top
- Equipment changes during combat immediately recalculate bonuses for next action
- Cursed items continue to provide their bonuses (positive or negative) even when unequippable
- Flat bonuses only - no percentage-based bonuses (+2 STR, not +10% STR)
- Item bonuses constant regardless of item condition (no durability system)
- No caching or performance optimization - recalculate from equipped items every time
- Calculation order doesn't matter - all bonuses additive, no priority rules

### Bonus Stacking Behavior
- All bonuses stack additively - simple sum of all equipped item bonuses
- No caps or limits on total bonuses from items
- Multi-stat items allowed - single item can grant bonuses to multiple different attributes/skills
- Positive and negative bonuses sum algebraically (net calculation: +3 STR + -1 STR = +2 STR)

### Combat Integration Flow
- Combat mode (melee/ranged) automatically filters to show only equipped weapons valid for that mode
- Weapon modifiers (SV/AV) merge with character's base attack skill before resolution
- Location-based armor system - each armor piece protects specific body location
- Hit location tracking added to combat system - attack determines location, armor checks coverage for that location
- Equipped weapons appear in mode-appropriate selection (melee mode uses MainHand/OffHand/TwoHand weapons)

### Visual Feedback
- Stat display shows inline breakdown: "STR 12 (10 base + 2 items)"
- Detailed calculation breakdown available - players can see: base attribute, item bonuses, effect modifiers
- Color-coded indicators: positive bonuses green, negative bonuses/penalties red
- Combat UI shows weapon list with full stats (damage, modifiers) - player picks which to use
- Combat results show detailed damage breakdown: "15 damage dealt → 5 absorbed by armor → 10 damage taken"

### Claude's Discretion
- Specific CSLA property implementation patterns
- Error handling for edge cases (e.g., no equipped weapon)
- UI layout details for bonus breakdowns
- Exact tooltip/hover interaction patterns
- Performance monitoring and optimization if needed later

</decisions>

<specifics>
## Specific Ideas

- "I want players to see exactly how their gear is helping them" - transparent bonus calculations
- "Combat should clearly show weapon choice and armor protection" - detailed feedback
- Hit location system should integrate naturally with existing combat flow
- Inline stat display preferred over separate panels - keep information where players look

</specifics>

<deferred>
## Deferred Ideas

- Percentage-based bonuses (e.g., +10% STR) - rejected for this phase, could be future enhancement
- Item durability/condition affecting bonuses - out of scope, would be separate phase
- Bonus caps or diminishing returns - not needed, could be added for balance later
- Advanced optimization/caching - start simple, optimize if performance issues emerge

</deferred>

---

*Phase: 06-item-bonuses-and-combat*
*Context gathered: 2026-01-25*
