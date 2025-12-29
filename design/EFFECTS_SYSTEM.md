# Effects System

## Overview

An **Effect** is anything that affects an entity (character, NPC, or object) and persists over time. Effects are the primary mechanism for representing temporary or ongoing changes to game state, from wounds and poisons to spell buffs and environmental hazards.

**Effects do NOT include**:
- FAT/VIT pending damage/healing pools (handled by health system)
- AP recovery (handled by action point system)

---

## Effect Targets

Effects can apply to different types of entities:

| Target Type | Description | Examples |
|-------------|-------------|----------|
| **Character** | Player characters | Poison, wounds, strength buff |
| **NPC** | GM-controlled characters | Same as characters |
| **Object** | Items held by a character or on the ground | Enchantment, light source, curse |
| **Location** | A place in the game world | *Future: environmental effects* |

For the assistant app, effects matter primarily on:
- Characters
- NPCs (functionally the same as characters for the GM)
- Objects held by or associated with a character

---

## Effect Properties

### Core Properties

| Property | Type | Description |
|----------|------|-------------|
| **Id** | GUID | Unique identifier |
| **Name** | string | Display name (e.g., "Poisoned", "Strength Boost") |
| **Description** | string | Detailed description of the effect |
| **EffectType** | enum | Category (see Effect Types below) |
| **Source** | string | What caused the effect (spell, poison, wound, item) |
| **SourceEntityId** | GUID? | The entity that caused the effect (if applicable) |

### Duration

| Property | Type | Description |
|----------|------|-------------|
| **DurationType** | enum | Rounds, Minutes, Hours, Days, Weeks, Permanent, UntilRemoved |
| **DurationValue** | int | Number of duration units |
| **StartTime** | DateTime | When the effect began |
| **EndTime** | DateTime? | When the effect will expire (null for permanent) |
| **RoundsRemaining** | int? | For round-based tracking during combat |

### Stacking

| Property | Type | Description |
|----------|------|-------------|
| **IsStackable** | bool | Can multiple instances exist? |
| **MaxStacks** | int | Maximum stack count (if stackable) |
| **CurrentStacks** | int | Current stack count |
| **StackBehavior** | enum | Replace, Extend, Intensify, Independent |

### Removal

| Property | Type | Description |
|----------|------|-------------|
| **CanBeRemoved** | bool | Can the effect be dispelled/cured? |
| **RemovalMethods** | list | How the effect can be removed (spell, medicine, skill, time) |
| **RemovalDifficulty** | int | TV for skill-based removal attempts |

---

## Effect Types

### Wounds

Wounds are a special category of effect representing physical injuries.

| Property | Description |
|----------|-------------|
| **Location** | Body part (Head, Torso, LeftArm, RightArm, LeftLeg, RightLeg) |
| **Severity** | Wound count at this location |
| **ASPenalty** | Penalty to AS (typically -2 per wound) |
| **AffectedSkillTypes** | All, Physical, Mental, or specific skills |
| **RecoveryModifier** | Effect on FAT/VIT/AP recovery |

**Standard Wound Effects**:
- -2 AS to all skills per wound (cumulative)
- 1 FAT damage every 2 rounds per wound
- Location-specific effects at 2+ wounds

### Conditions

General status conditions affecting the character.

| Condition | Effects |
|-----------|---------|
| **Stunned** | Cannot take actions; FAT set to 0 temporarily |
| **Unconscious** | Cannot take actions; unaware of surroundings |
| **Paralyzed** | Cannot move or take physical actions |
| **Blinded** | Cannot see; severe penalties to visual actions |
| **Deafened** | Cannot hear; penalties to awareness |
| **Prone** | On the ground; penalties to defense, bonuses to hide |
| **Grappled** | Movement restricted; limited actions |

### Poisons & Diseases

| Property | Description |
|----------|-------------|
| **DamagePerInterval** | FAT/VIT damage at each trigger |
| **Interval** | How often damage occurs (rounds, minutes, hours) |
| **AttributePenalties** | Temporary attribute reductions |
| **SkillPenalties** | Temporary skill penalties |
| **ProgressionStages** | Some poisons/diseases worsen over time |

### Buffs

Positive effects that enhance the character.

| Buff Type | Examples |
|-----------|----------|
| **Attribute Boost** | +2 STR, +1 DEX |
| **Skill Boost** | +3 to Swords, +2 to all magic skills |
| **AS Boost** | +2 AS to specific actions |
| **AV Boost** | +2 AV to attacks |
| **TV Reduction** | -2 to target's TV (easier to hit) |
| **Damage Boost** | +2 SV on successful attacks |
| **Defense Boost** | +2 to all defense rolls |
| **Recovery Boost** | Faster FAT/VIT/AP recovery |

### Debuffs

Negative effects that hinder the character.

| Debuff Type | Examples |
|-------------|----------|
| **Attribute Penalty** | -2 STR, -1 WIL |
| **Skill Penalty** | -3 to Dodge, -2 to all physical skills |
| **AS Penalty** | -2 AS to all actions |
| **AV Penalty** | -2 AV to attacks |
| **TV Increase** | +2 to defender's TV (harder to hit) |
| **Recovery Penalty** | Slower FAT/VIT/AP recovery |

### Spell Effects

Non-instant spell results that persist over time.

| Spell Effect Type | Examples |
|-------------------|----------|
| **Appearance** | Invisibility, disguise, illusory form |
| **Sensory** | Darkvision, detect magic, true sight |
| **Movement** | Silent movement, flight, water walking |
| **Protection** | Magic shield, fire resistance, armor |
| **Aura** | Light source, fear aura, inspiring presence |
| **Sustained Attack** | Burning, bleeding, curse damage |

### Object Effects

Effects on items rather than characters.

| Object Effect Type | Examples |
|--------------------|----------|
| **Light Source** | Glowing item, magical light |
| **Enchantment** | Temporary magic weapon, armor enhancement |
| **Curse** | Penalty while holding/wearing |
| **Transformation** | Item changed in form or function |

---

## Effect Impacts

Effects modify the character through one or more impacts. An effect can have multiple impacts.

### Impact Types

| Impact Type | Target | Description |
|-------------|--------|-------------|
| **AttributeModifier** | Specific attribute | +/- to STR, DEX, END, INT, ITT, WIL, PHY |
| **SkillModifier** | Specific skill or category | +/- to skill level or AS |
| **ASModifier** | All or specific actions | +/- to Ability Score calculations |
| **AVModifier** | Attack actions | +/- to Attack Value rolls |
| **TVModifier** | Defense or targeting | +/- to Target Value |
| **SVModifier** | Damage | +/- to Success Value |
| **RecoveryModifier** | FAT/VIT/AP | Multiplier or +/- to recovery rates |
| **DamageOverTime** | FAT/VIT | Periodic damage to health pools |
| **MovementModifier** | Movement | +/- to movement range or speed |
| **SpecialAbility** | Various | Grants or removes special capabilities |

### Impact Properties

```
ImpactType: enum (from above)
Target: string (attribute name, skill name, or "All")
Value: decimal (the modifier amount)
IsPercentage: bool (true = percentage, false = flat value)
Condition: string? (when does this impact apply?)
```

### Cascading Effects

When an effect modifies an **attribute**, it cascades to all skills using that attribute:

- Effect grants +2 STR
- All STR-based skills (Physicality, melee damage, etc.) gain +2 AS
- This follows the same cascading rules as item attribute modifiers

---

## Duration and Timing

### Duration Types

| Duration Type | Description | Trigger |
|---------------|-------------|---------|
| **Rounds** | Combat time (3 seconds each) | EndOfRound event |
| **Minutes** | Short-term (20 rounds) | EndOfMinute event |
| **Hours** | Medium-term (60 minutes) | EndOfHour event |
| **Days** | Long-term (24 hours) | EndOfDay event |
| **Weeks** | Extended (7 days) | EndOfWeek event |
| **Permanent** | Until explicitly removed | Never expires |
| **UntilRemoved** | Until a condition is met | Removal trigger |

### Effect Processing

Effects are processed at time boundaries:

**End of Round**:
- Decrement round-based effect durations
- Apply per-round damage (poisons, bleeds, wounds)
- Check for effect expiration

**End of Minute**:
- Decrement minute-based effect durations
- Apply per-minute effects
- Check for effect expiration

**End of Hour**:
- Decrement hour-based effect durations
- Apply per-hour effects (disease progression)
- Check for effect expiration

**End of Day/Week**:
- Decrement longer-term effect durations
- Apply periodic effects
- Check for effect expiration

---

## Effect Removal

### Automatic Removal

Effects are automatically removed when:
- Duration expires
- Character dies (most effects)
- Character is healed to full (some effects)

### Manual Removal

Effects can be removed by:

| Method | Description |
|--------|-------------|
| **Spell** | Dispel, cure, healing magic |
| **Medicine** | Antidotes, medical treatment |
| **Skill Action** | Using a skill to counteract the effect |
| **Rest** | Extended rest removes some effects |
| **Item** | Consumables that cure specific conditions |

### Removal Difficulty

Some effects require a skill check to remove:

```
Removal Check: Skill AS + 4dF+ vs Effect RemovalDifficulty
```

---

## Stacking Behavior

When the same effect is applied multiple times:

| Behavior | Description |
|----------|-------------|
| **Replace** | New effect replaces old (resets duration) |
| **Extend** | Duration is extended, intensity unchanged |
| **Intensify** | Effect grows stronger, duration unchanged |
| **Independent** | Multiple separate instances tracked |

### Stack Examples

- **Poison (Intensify)**: Each poison dose increases damage per interval
- **Strength Buff (Replace)**: Recasting replaces the previous buff
- **Wound (Independent)**: Each wound is tracked separately by location
- **Invisibility (Extend)**: Recasting extends the duration

---

## Common Effects Reference

### Standard Wound

```
Name: "Wound"
EffectType: Wound
DurationType: UntilRemoved
Impacts:
  - Type: ASModifier, Target: "All", Value: -2
  - Type: DamageOverTime, Target: "FAT", Value: 1, Interval: 2 rounds
RemovalMethods: [Medicine, Spell, NaturalHealing]
RemovalDifficulty: 8 (for medicine skill check)
NaturalHealingTime: 4 hours per wound
```

### Poisoned (Weak)

```
Name: "Weak Poison"
EffectType: Poison
DurationType: Minutes
DurationValue: 10
Impacts:
  - Type: DamageOverTime, Target: "FAT", Value: 1, Interval: 1 minute
  - Type: ASModifier, Target: "All", Value: -1
RemovalMethods: [Medicine, Spell, Antidote]
RemovalDifficulty: 6
```

### Strength Boost

```
Name: "Strength Boost"
EffectType: Buff
DurationType: Minutes
DurationValue: 10
Impacts:
  - Type: AttributeModifier, Target: "STR", Value: +2
StackBehavior: Replace
RemovalMethods: [Dispel, Time]
```

### Invisibility

```
Name: "Invisibility"
EffectType: SpellEffect
DurationType: Minutes
DurationValue: 5
Impacts:
  - Type: SpecialAbility, Target: "Visibility", Value: "Invisible"
  - Type: TVModifier, Target: "Attacker", Value: +4 (harder to hit invisible target)
BreakConditions: ["Attack", "CastSpell", "TakeDamage"]
StackBehavior: Extend
RemovalMethods: [Dispel, Detection, BreakAction]
```

### Stunned

```
Name: "Stunned"
EffectType: Condition
DurationType: Rounds
DurationValue: (SV from attack)
Impacts:
  - Type: SpecialAbility, Target: "Actions", Value: "None"
  - Type: RecoveryModifier, Target: "FAT", Value: 0 (temporarily)
RemovalMethods: [Time, Healing]
```

### Drunk

```
Name: "Intoxicated"
EffectType: Condition
DurationType: Hours
DurationValue: 2
Impacts:
  - Type: ASModifier, Target: "PhysicalSkills", Value: -2
  - Type: ASModifier, Target: "MentalSkills", Value: -1
  - Type: AttributeModifier, Target: "WIL", Value: -2
StackBehavior: Intensify (penalties increase)
RemovalMethods: [Time, Medicine]
```

### Light Source (Object)

```
Name: "Magical Light"
EffectType: ObjectEffect
TargetType: Object
DurationType: Hours
DurationValue: 4
Impacts:
  - Type: SpecialAbility, Target: "LightRadius", Value: 10 (meters)
RemovalMethods: [Dispel, Time]
```

---

## Implementation Notes

### Effect Service Interface

The effect system should provide:

```csharp
// Core operations
AddEffect(targetId, effect) -> effectId
RemoveEffect(effectId)
RemoveEffectsByType(targetId, effectType)

// Query operations
GetActiveEffects(targetId) -> List<Effect>
GetEffectsByType(targetId, effectType) -> List<Effect>
HasEffect(targetId, effectName) -> bool

// Calculation operations
GetTotalAttributeModifier(targetId, attribute) -> int
GetTotalSkillModifier(targetId, skillId) -> int
GetTotalASModifier(targetId, actionType) -> int
GetTotalAVModifier(targetId) -> int
GetTotalTVModifier(targetId) -> int
GetTotalSVModifier(targetId) -> int

// Time event handlers
ProcessEndOfRound(targetId)
ProcessEndOfMinute(targetId)
ProcessEndOfHour(targetId)
ProcessEndOfDay(targetId)
ProcessEndOfWeek(targetId)
```

### Integration Points

The effect system integrates with:

| System | Integration |
|--------|-------------|
| **Character** | Effects modify character attributes and skills |
| **Combat** | Effects modify AV, TV, SV during resolution |
| **Time** | Time events trigger effect processing |
| **Health** | Effects can deal damage or modify recovery |
| **Items** | Items can grant effects while equipped |
| **Spells** | Most non-instant spells create effects |

### Calculation Priority

When calculating effective values, apply modifiers in order:

1. Base attribute value
2. Species modifiers (permanent)
3. Equipment modifiers (while equipped)
4. Effect modifiers (temporary)
5. Situational modifiers (combat conditions)

### Performance Considerations

- Cache computed modifiers when possible
- Invalidate cache when effects change
- Batch effect processing at time boundaries
- Index effects by target for efficient lookup

---

## Related Documents

- [TIME_SYSTEM.md](TIME_SYSTEM.md) - Effect duration and time events
- [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md) - Combat status effects
- [GAME_RULES_SPECIFICATION.md](GAME_RULES_SPECIFICATION.md) - Wound mechanics
- [DATABASE_DESIGN.md](DATABASE_DESIGN.md) - EffectDefinitions and CharacterEffects tables
