# Actions System

## Overview

An **Action** is any use of a skill that requires time and effort. All skill-based activities in Threa follow the same fundamental resolution pattern, whether in combat, exploration, social encounters, or crafting.

This document defines the universal action framework. Specialized documents (Combat, Magic, Movement) provide additional rules for their domains but always build upon this foundation.

---

## Action Resolution Flow

Every action follows this sequence:

```
1. Declare Action     → What skill, what target/goal?
2. Pay Cost          → 1 AP + 1 FAT (or 2 AP)
3. Apply Modifiers   → Boosts, penalties, effects
4. Calculate AS      → Attribute + Skill Level - 5 + modifiers
5. Roll              → AS + 4dF+
6. Determine TV      → Fixed difficulty or opposing roll
7. Calculate SV      → Roll Result - TV
8. Lookup Result     → Success/failure effects based on SV
9. Apply Effects     → Damage, conditions, outcomes
```

---

## 1. Declare Action

The player declares:
- **Which skill** to use
- **What target** (if applicable): a creature, object, or location
- **What goal**: attack, defend, create, persuade, etc.

In the app, this typically happens when the user clicks on a skill or weapon.

---

## 2. Pay Cost

### Standard Action Cost

Every action costs **1 AP** plus an additional resource:

| Payment Option | Cost | When to Use |
|----------------|------|-------------|
| Standard | 1 AP + 1 FAT | Normal choice, conserves AP |
| Fatigue-Free | 2 AP | When FAT is low or critical |

### Free Actions

Some activities don't cost AP/FAT:
- Minor repositioning (Range 0-2, up to 4m)
- Speaking a few words
- Dropping an item
- Automatic reactions (armor absorption)

### Multiple Action Penalty

All actions after the first in a round suffer **-1 AS**.
- The penalty is NOT cumulative (2nd, 3rd, 4th actions all have -1)
- Can be offset with boosts

---

## 3. Apply Modifiers

Before rolling, calculate all modifiers to the action:

### Boost Mechanic

Spend additional resources for bonuses:

**1 AP or 1 FAT = +1 AS**

- Boosts stack (3 FAT = +3 AS)
- AP and FAT can be mixed
- Declare before rolling

### Penalty Sources

| Source | Penalty |
|--------|---------|
| Multiple action (not first) | -1 AS |
| Wound (per wound) | -2 AS |
| Effect (varies) | Per effect |
| Equipment (heavy/awkward) | Per item |
| Difficult conditions | GM discretion |

### Bonus Sources

| Source | Bonus |
|--------|-------|
| Boost spending | +1 AS per AP/FAT |
| Aim action (ranged) | +2 AS |
| Equipment bonuses | Per item |
| Effect bonuses | Per effect |
| Favorable conditions | GM discretion |

---

## 4. Calculate Ability Score (AS)

The Ability Score determines the base effectiveness of the action:

```
AS = Related Attribute + Skill Level - 5 + Modifiers
```

### Components

| Component | Source |
|-----------|--------|
| Related Attribute | Base attribute value (STR, DEX, etc.) |
| Skill Level | Current level in the skill |
| Base Offset | -5 (standard offset) |
| Modifiers | Boosts, penalties, effects, equipment |

### Example Calculation

Character with:
- Physicality (STR) attribute: 12
- Swords skill level: 4
- Wound penalty: -2
- Boost spent: +1

```
AS = 12 + 4 - 5 + (-2) + 1 = 10
```

---

## 5. Roll

Roll the dice and add to AS:

```
Roll Result = AS + 4dF+
```

### 4dF+ (Exploding Fudge Dice)

- Roll 4 Fudge dice (each shows +1, 0, or -1)
- Sum the results (range: -4 to +4)
- **On +4**: Roll again, add only the + results, repeat if another +4
- **On -4**: Roll again, add only the - results (making it worse), repeat if another -4

---

## 6. Determine Target Value (TV)

The TV represents the difficulty of the action.

### Fixed TV (vs Environment/Difficulty)

For actions against static difficulties:

| Difficulty | TV | Examples |
|------------|-----|----------|
| Trivial | 2 | Simple tasks with ample time |
| Easy | 4 | Basic skill application |
| Routine | 6 | Standard difficulty |
| Moderate | 8 | Requires competence |
| Challenging | 10 | Tests skilled practitioners |
| Hard | 12 | Pushes expert abilities |
| Very Hard | 14 | Near the limits of skill |
| Extreme | 16 | Legendary difficulty |
| Impossible | 18+ | Beyond normal capability |

### Opposed TV (vs Another Entity)

For actions against creatures or active opposition:

```
TV = Opponent's AS + 4dF+
```

The opponent rolls their relevant skill. Common opposed actions:

| Attacker Skill | Defender Skill |
|----------------|----------------|
| Attack (any weapon) | Dodge or Parry |
| Stealth | Awareness |
| Persuasion | Focus (WIL) |
| Deception | Reasoning (INT) |
| Intimidation | Focus (WIL) |

### Passive TV (No Active Defense)

When the target cannot or does not actively defend:

```
TV = Defender's Skill AS - 1 (no roll)
```

Used when:
- Target is surprised
- Target has no AP for defense
- Target chooses passive defense
- Target is incapacitated

---

## 7. Calculate Success Value (SV)

The difference between roll and TV:

```
SV = Roll Result - TV
```

| SV | Outcome |
|----|---------|
| SV ≥ 0 | Success |
| SV < 0 | Failure |

Higher SV = better success. Lower SV = worse failure.

---

## 8. Lookup Result

The SV determines the outcome. Different action types have different result tables.

### General Success Table

| SV | Result Quality |
|----|----------------|
| 0-1 | Marginal success - barely achieved |
| 2-3 | Standard success - competent result |
| 4-5 | Good success - above average result |
| 6-7 | Excellent success - impressive result |
| 8+ | Outstanding success - exceptional result |

### General Failure Table

| SV | Result Quality |
|----|----------------|
| -1 to -2 | Minor failure - task not completed |
| -3 to -4 | Failure - clear lack of success |
| -5 to -6 | Bad failure - complications arise |
| -7 to -8 | Severe failure - negative consequences |
| -9 or worse | Critical failure - serious problems |

### Combat Damage Table

For attack actions, SV determines damage dice. See [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md) and [GAME_RULES_SPECIFICATION.md](GAME_RULES_SPECIFICATION.md) for damage conversion tables.

### Skill-Specific Tables

Some skills have unique result tables:
- **Crafting**: SV determines item quality
- **Healing**: SV determines HP restored
- **Social**: SV determines influence level
- **Perception**: SV determines information gained

---

## 9. Apply Effects

Based on the result:

### On Success
- Apply intended effect (damage, healing, creation, etc.)
- May trigger secondary effects (Physicality bonus in melee)
- Target may gain conditions/effects
- Skill usage counts toward progression

### On Failure
- Primary goal not achieved
- SV ≤ -3 may trigger negative consequences
- May still cost resources (FAT already spent)
- Skill usage still counts toward progression

---

## Action Categories

### Combat Actions

Combat actions use this framework with additional rules:

| Action | Skill | TV Source | Special Rules |
|--------|-------|-----------|---------------|
| Melee Attack | Weapon skill | Defender's Dodge/Parry | Physicality damage bonus |
| Ranged Attack | Weapon skill | Fixed (6/8/10/12 by range) + modifiers | Cooldowns, ammo |
| Dodge | Dodge | Attacker's roll | Active defense |
| Parry | Weapon/Shield | Attacker's roll | Must be in parry mode |
| Shield Block | Shield | Fixed TV 8 | Absorbs damage |

See [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md) for complete combat rules.

### Movement Actions

Movement within combat:

| Action | Cost | Range | Notes |
|--------|------|-------|-------|
| Free positioning | None | 0-2 (0-4m) | Not an action |
| Sprint | 1 AP + 1 FAT | 3 (9m) | Standard action |
| Full-round sprint | Full round | 5 (25m) | No other actions |

See [MOVEMENT.md](MOVEMENT.md) for complete movement rules.

### Spell Actions

Spellcasting follows the standard action framework:

| Action | Skill | TV Source | Special Rules |
|--------|-------|-----------|---------------|
| Cast spell | Spell skill | Fixed or opposed | Mana cost, effects |
| Maintain spell | Spell skill | May require checks | Concentration |

*See Magic System documentation for complete spell rules.*

### Skill Actions (Non-Combat)

General skill use outside combat:

| Action Type | Examples | TV Typically |
|-------------|----------|--------------|
| Physical | Climbing, jumping, lifting | Fixed difficulty |
| Mental | Recall, analysis, planning | Fixed difficulty |
| Social | Persuasion, intimidation | Opposed (WIL/INT) |
| Perception | Search, listen, detect | Fixed or opposed (Stealth) |
| Crafting | Create, repair, modify | Fixed by item complexity |
| Knowledge | Lore, identification | Fixed by obscurity |

---

## Action Timing

### Within a Round (Combat)

- Multiple actions allowed, limited by AP
- Initiative by Available AP (highest first)
- -1 AS penalty after first action
- Actions resolve immediately

### Outside Combat

- Actions still cost AP/FAT (if tracking)
- Time taken varies by action complexity
- May skip AP tracking for routine activities

### Cooldowns

Some actions have cooldowns before they can be repeated:

| Cooldown Type | Examples |
|---------------|----------|
| Instant | Most melee attacks, movement |
| Skill-based | Ranged weapons (by skill level) |
| Fixed | Aim (1 round), some spells |
| Prep-required | Loading, readying |

See [TIME_SYSTEM.md](TIME_SYSTEM.md) for cooldown mechanics.

---

## App Implementation

### Action Trigger Flow

When a user clicks on a skill:

1. **Identify action type** from skill definition
2. **Check resources** (AP, FAT, mana if applicable)
3. **Gather modifiers** (effects, equipment, wounds)
4. **Prompt for target** if needed
5. **Prompt for boosts** (optional AP/FAT spending)
6. **Calculate AS** with all modifiers
7. **Roll 4dF+** (automated)
8. **Determine TV** (automated or prompt)
9. **Calculate SV** 
10. **Lookup result** from appropriate table
11. **Apply effects** (update character state)
12. **Deduct costs** (AP, FAT, ammo, etc.)
13. **Log for progression** (skill usage event)

### Skill Definition Requirements

Each skill needs:

```
SkillId: unique identifier
Name: display name
RelatedAttribute: STR, DEX, END, INT, ITT, WIL, PHY
ActionType: Attack, Defense, Spell, Social, Craft, Perception, etc.
DefaultTV: fixed TV or "Opposed"
OpposedSkill: if opposed, which skill defends
CooldownType: None, SkillBased, Fixed, PrepRequired
CooldownValue: seconds or rounds
ResultTable: which table to use for SV lookup
AppliesPhysicalityBonus: bool (for melee/thrown)
RequiresTarget: bool
RequiresLineOfSight: bool
ManaCost: for spells
```

### UI Considerations

- Display calculated AS before rolling
- Show breakdown of modifiers
- Allow boost spending before confirming
- Display roll animation
- Show TV comparison
- Display SV and result interpretation
- Summarize effects applied

---

## Examples

### Example 1: Sword Attack

1. **Declare**: Attack goblin with sword
2. **Cost**: 1 AP + 1 FAT
3. **Modifiers**: Second attack (-1 AS), no boosts
4. **AS**: STR 12 + Swords 4 - 5 - 1 = 10
5. **Roll**: 10 + 4dF+ (+2) = 12
6. **TV**: Goblin dodges: 8 + 4dF+ (-1) = 7
7. **SV**: 12 - 7 = 5
8. **Result**: Consult damage table for SV 5
9. **Effects**: Roll damage dice, apply to goblin

### Example 2: Persuasion

1. **Declare**: Persuade guard to let us pass
2. **Cost**: 1 AP + 1 FAT
3. **Modifiers**: Boost +2 (spend 2 FAT)
4. **AS**: PHY 11 + Persuasion 3 - 5 + 2 = 11
5. **Roll**: 11 + 4dF+ (+1) = 12
6. **TV**: Guard's Focus: 9 + 4dF+ (0) = 9
7. **SV**: 12 - 9 = 3
8. **Result**: Standard success - guard is convinced
9. **Effects**: Guard allows passage

### Example 3: Crafting

1. **Declare**: Craft iron dagger (Moderate difficulty)
2. **Cost**: 1 AP + 1 FAT (per attempt/hour)
3. **Modifiers**: Good tools (+1), workshop (+1)
4. **AS**: INT 10 + Blacksmithing 5 - 5 + 2 = 12
5. **Roll**: 12 + 4dF+ (+3) = 15
6. **TV**: Fixed 8 (Moderate)
7. **SV**: 15 - 8 = 7
8. **Result**: Excellent success - high quality dagger
9. **Effects**: Create dagger with quality bonus

---

## Related Documents

- [ACTION_POINTS.md](ACTION_POINTS.md) - AP calculation, costs, recovery
- [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md) - Combat-specific action rules
- [MOVEMENT.md](MOVEMENT.md) - Movement action rules
- [TIME_SYSTEM.md](TIME_SYSTEM.md) - Rounds, cooldowns, timing
- [EFFECTS_SYSTEM.md](EFFECTS_SYSTEM.md) - Effects applied by actions
- [GAME_RULES_SPECIFICATION.md](GAME_RULES_SPECIFICATION.md) - Dice mechanics, damage tables
