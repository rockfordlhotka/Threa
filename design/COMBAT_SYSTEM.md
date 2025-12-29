# Combat System

## Overview

Combat in Threa uses a skill-based resolution system built on the 4dF+ dice mechanic. Combat integrates with the Action Points (AP) and Time systems to create tactical depth where characters must manage their action economy and fatigue.

**Related Documents**:
- [Action Points](ACTION_POINTS.md) - AP calculation, costs, and recovery
- [Time System](TIME_SYSTEM.md) - Rounds, initiative, and cooldowns
- [Game Rules Specification](GAME_RULES_SPECIFICATION.md) - Dice mechanics, damage tables

---

## Initiative

Initiative determines the order in which characters act within a round.

### Initiative Order

Characters act in order of **Available AP** (highest first):

1. Character/NPC with highest Available AP acts first
2. Ties resolved by GM discretion (or Awareness skill as tiebreaker)
3. Initiative is recalculated each round based on current Available AP

**Strategic Implication**: Characters who pass or take fewer actions will have higher Available AP in subsequent rounds, potentially acting earlier.

### Passing and Delaying

- **Pass**: Take no action, preserving Available AP
- **Delay**: Wait for a specific trigger, then react (uses your action when triggered)

---

## Action Costs

### Standard Action Cost

Every action costs **1 AP** plus an additional cost paid with either FAT or AP:

| Payment Option | Cost | Total Resources |
|----------------|------|-----------------|
| Standard | 1 AP + 1 FAT | Uses both resources |
| Fatigue-Free | 2 AP | Preserves stamina |

### Multiple Actions Per Round

Characters can take multiple actions per round, limited only by Available AP.

**Multiple Action Penalty**: All actions after the first in a round suffer **-1 AS** penalty.
- The penalty is **not cumulative** (second, third, fourth actions all have -1 AS, not -1, -2, -3)
- This penalty can be offset with boosts (see below)

### Boost Mechanic

Characters can spend additional resources to improve their chances:

**Boost Cost**: 1 AP or 1 FAT = **+1 AS** to the action

- Boosts can be stacked (spend 3 FAT for +3 AS)
- AP and FAT can be mixed (1 AP + 2 FAT = +3 AS)
- Boosts are declared before rolling
- Common uses:
  - Offset the -1 AS multiple action penalty
  - Increase odds on critical attacks
  - Ensure a defensive action succeeds

**Example**: A character with Swords AS 12 takes a second attack this round:
- Base: 12 AS - 1 (multiple action) = 11 AS
- With 2 FAT boost: 12 AS - 1 + 2 = 13 AS

---

## Attack Resolution

### Attack Value (AV)

For all attacks, the attacker rolls:

```
AV = Ability Score (AS) + 4dF+
```

- Ability Score = Related Attribute + Skill Level - 5
- The exploding dice mechanic (4dF+) applies to all attack rolls

### Success Value (SV)

The margin of success or failure:

```
SV = AV - TV (Target Value)
```

| SV Result | Outcome |
|-----------|---------|
| SV ≥ 0 | Attack succeeds |
| SV < 0 | Attack fails |
| SV ≤ -3 | Negative consequences (see RVS chart) |

Higher SV values result in more damage.

---

## Defense Options

When attacked, a character has several defense options. All active defenses are actions with standard costs (1 AP + 1 FAT or 2 AP).

### Active Defense: Dodge

**Cost**: 1 AP + 1 FAT (or 2 AP)

The defender uses their Dodge (DEX) skill:

```
TV = Dodge AS + 4dF+
```

- Most versatile defense (works against melee and ranged)
- Requires an action to use

### Active Defense: Parry

**Cost**: 1 AP + 1 FAT (or 2 AP) to enter parry mode

While in parry mode:
- Use primary weapon skill OR shield skill for defense instead of Dodge
- **Parry defenses do not cost additional AP/FAT** while in mode
- Only works against melee attacks (not ranged)
- Character must meet minimum skill level for the equipped weapon/shield
- Parry mode ends when the character takes any non-parry action

```
TV = Weapon Skill AS + 4dF+
   or
TV = Shield Skill AS + 4dF+
```

**Parry vs Dodge Trade-off**:
| Aspect | Dodge | Parry |
|--------|-------|-------|
| Cost per defense | 1 AP + 1 FAT | Free (after entering mode) |
| Works against ranged | Yes | No |
| Requires action to start | No | Yes (entering mode) |
| Limits other actions | No | Yes (breaks mode) |
| Can use weapon or shield | No | Yes |

### Active Defense: Shield Block

**Cost**: 1 AP + 1 FAT (or 2 AP)

If the character has a shield equipped:

```
Shield Skill AS + 4dF+ vs TV 8
```

- On success, shield absorbs damage based on its absorption rating
- Can defend any hit location
- Shield durability reduced by absorbed SV
- Can be used in addition to dodge/parry for layered defense

### Passive Defense

If a character cannot or chooses not to spend an action on defense:

```
TV = Dodge Skill AS - 1 (no roll)
```

- No AP or FAT cost
- No dice roll - flat value
- Applies when:
  - Character has no Available AP
  - Character is surprised
  - Character chooses to save AP for offense
  - Character is incapacitated (low FAT/VIT)

---

## Melee Combat

### Melee Attack

**Cost**: 1 AP + 1 FAT (or 2 AP)

1. Attacker declares target and weapon
2. Attacker rolls: **Weapon Skill AS + 4dF+** = AV
3. Defender chooses defense (active or passive)
4. Calculate SV = AV - TV
5. If SV ≥ 0, proceed to damage resolution

### Physicality Damage Bonus

After a successful melee attack, a **Physicality (STR) skill check** occurs automatically:

**Cost**: None (automatic, free action)

```
Roll: Physicality AS + 4dF+ vs TV 8
RV = Roll Result - 8
```

Apply Result Value System (RVS) modifiers:

| RV | Effect |
|----|--------|
| -10 to -9 | -3 AV for 3 rounds |
| -8 to -7 | -2 AV for 2 rounds |
| -6 to -5 | -2 AV for 1 round |
| -4 to -3 | -1 AV for 1 round |
| -2 to +1 | +0 SV (no effect) |
| +2 to +3 | +1 SV |
| +4 to +7 | +2 SV |
| +8 to +11 | +3 SV |
| +12 to +18 | +4 SV |

### Dual Wielding

**Cost**: 2 AP + 2 FAT (or 4 AP) for both attacks

- Primary hand: **Weapon Skill AS + 4dF+**
- Off-hand: **Weapon Skill AS - 2 + 4dF+**
- Both attacks occur simultaneously (same action)
- Defender must defend against each attack separately

---

## Ranged Combat

### Range System

All ranges in Threa use a **power-of-2** model, measured in **meters**:

```
Actual Distance (meters) = Range Value²
```

| Range Value | Distance (meters) | Description |
|-------------|-------------------|-------------|
| 0 | 0 | Touch (contact) |
| 1 | 1 | Close quarters melee range |
| 2 | 4 | Extended melee / short throw |
| 3 | 9 | Across a room |
| 4 | 16 | Across a large room |
| 5 | 25 | Across a courtyard |
| 6 | 36 | Down a street |
| 7 | 49 | Across a field |
| 8 | 64 | Long bowshot |
| 9 | 81 | Extreme range |
| 10 | 100 | Maximum visibility |

### Weapon Range Categories

Each ranged weapon has four range values: **Short / Medium / Long / Extreme**

Example weapon ranges:
- Shortbow: 3/5/7/9 (actual: 9/25/49/81 units)
- Longbow: 4/6/8/10 (actual: 16/36/64/100 units)
- Crossbow: 5/7/9/11 (actual: 25/49/81/121 units)
- Thrown Dagger: 1/2/3/4 (actual: 1/4/9/16 units)

### Base Target Value by Range

| Range Category | Base TV |
|----------------|---------|
| Short | 6 |
| Medium | 8 |
| Long | 10 |
| Extreme | 12 |

### Ranged Attack

**Cost**: 1 AP + 1 FAT (or 2 AP)

1. Attacker declares target and weapon
2. Determine range category based on distance to target
3. Attacker rolls: **Ranged Weapon Skill AS + 4dF+** = AV
4. Calculate TV = Base TV + Modifiers
5. SV = AV - TV
6. If SV ≥ 0, proceed to damage resolution

### Target Value Modifiers

| Condition | TV Modifier |
|-----------|-------------|
| **Target Movement** | |
| Target in motion | +2 |
| Target prone | +2 |
| Target crouching | +2 |
| **Cover** | |
| Target behind ½ cover | +1 |
| Target behind ¾ cover | +2 |
| **Attacker Conditions** | |
| Attacker in motion | +2 |
| **Target Size** | |
| Tiny target | +2 |
| Small target | +1 |

Modifiers stack. Example: Small target behind ½ cover at long range = TV 10 + 1 + 1 = 12

### Prep Actions

Prep actions prepare ammunition or equipment for faster use. Some prep actions have cooldowns, others do not.

#### No-Cooldown Prep Actions

These can be performed multiple times per round (AP permitting):

| Prep Action | Effect |
|-------------|--------|
| Prep an arrow | Ready arrow in hand for instant nocking |
| Prep a magazine | Ready magazine for instant reload |
| Draw thrown weapon | Ready weapon in hand for instant throw |

#### Loading from Prepped Items

Loading a prepped item into a weapon has **zero cooldown**:

- Nock a prepped arrow → Instant (can fire immediately)
- Insert a prepped magazine → Instant (can fire immediately)
- Multiple shots per round possible if arrows/magazines are prepped

**Example Flow**:
1. Round 1: Prep 3 arrows (3 actions, no cooldown between)
2. Round 2: Nock + Fire, Nock + Fire, Nock + Fire (3 shots in one round)

Without prep: Each shot requires both prep AND fire, limited by weapon cooldown.

### Aim Action

**Cost**: 1 AP + 1 FAT (or 2 AP)
**Cooldown**: 1 round

Aiming provides a bonus to the next ranged attack:

- Spend action to aim at a specific target
- **+2 AS** bonus applies if:
  - First action of the next round is a ranged attack
  - Same target as the aim action
- Aim bonus is lost if:
  - Target moves significantly (GM discretion)
  - Attacker takes any other action before firing
  - Attacker is interrupted (takes damage, etc.)

### Ranged Weapon Cooldowns

After firing, ranged weapons require time before the next shot:

| Skill Level | Cooldown | Shots per Round |
|-------------|----------|-----------------|
| 0 | 6 seconds (2 rounds) | 0.5 |
| 1 | 5 seconds | 0.6 |
| 2 | 4 seconds | 0.75 |
| 3 | 3 seconds (1 round) | 1 |
| 4-5 | 2 seconds | 1.5 |
| 6-7 | 1 second | 3 |
| 8-9 | 0.5 seconds | 6 |
| 10+ | No cooldown | AP limited |

**Note**: These cooldowns apply when NOT using prepped ammunition. With prepped ammo, the limiting factor becomes AP and the weapon's mechanical rate of fire.

**Cooldown Mechanics**:
- Cooldown begins after firing
- Cannot fire again until cooldown completes
- Cooldown continues even if taking other actions
- Sub-round cooldowns (< 3 sec) allow multiple shots per round

### Cooldown Interruption

If a character is interrupted while preparing a ranged weapon:

| Weapon Type | Interruption Effect |
|-------------|---------------------|
| Bow (readying arrow) | **Pausable** - Progress freezes, resumes later |
| Crossbow (winding) | **Pausable** - Progress freezes, resumes later |
| Firearm (reloading) | **Pausable** - Progress freezes, resumes later |
| Thrown (drawing) | **Resettable** - Must start over |

**Interruption Triggers**:
- Taking damage
- Performing a different action
- Being knocked down or stunned

### Ammunition

- Ammunition is consumed on each shot
- Different ammunition types may provide bonuses or special effects
- Retrieving ammunition from a container has its own cooldown (typically 1 round)
- Pre-staged ammunition (arrows in hand, loaded magazine) bypasses retrieval cooldown

### Thrown Weapons

**Cost**: 1 AP + 1 FAT (or 2 AP)

- Use the same melee weapon skill (Daggers, etc.)
- Apply ranged attack TV modifiers
- Thrown items are removed from equipped/inventory
- Can be retrieved after combat
- Physicality damage bonus applies (like melee)

---

## Damage Resolution

### Defense Sequence

When an attack succeeds (SV ≥ 0), damage is absorbed in order:

1. **Shield Defense** (if equipped and action available)
   - Shield Skill AS + 4dF+ vs TV 8
   - Success: Shield absorbs SV based on damage type rating
   - Shield durability reduced by absorbed SV

2. **Armor Defense** (passive, no action required)
   - Armor Skill AS + 4dF+ vs TV 8
   - Success: Armor absorbs SV based on damage type rating
   - Armor durability reduced by absorbed SV
   - Multiple layers: outer absorbs before inner
   - Only armor covering hit location applies

3. **Damage Application**
   - Remaining SV determines damage to character

### Hit Locations

Determined by **1d12** roll:

| Roll | Location | Armor Slots |
|------|----------|-------------|
| 1 | Head* | Head, Face, Ears, Neck |
| 2-6 | Torso | Neck, Shoulders, Back, Chest, Waist |
| 7 | Left Arm | Shoulders, Arms(L), Wrists(L), Hands(L) |
| 8 | Right Arm | Shoulders, Arms(R), Wrists(R), Hands(R) |
| 9-10 | Left Leg | Waist, Legs, Ankles(L), Feet(L) |
| 11-12 | Right Leg | Waist, Legs, Ankles(R), Feet(R) |

*On roll of 1, roll 1d12 again: 1-6 = Head, 7-12 = Torso

### Damage Classes

| Class | Scale | Examples |
|-------|-------|----------|
| Class 1 | Normal | Human weapons, standard combat |
| Class 2 | Heavy (10×) | Vehicles, giant creatures |
| Class 3 | Massive (100×) | Armored vehicles, dragons |
| Class 4 | Siege (1000×) | Structures, legendary magic |

**Class Interactions**:
- Higher-class armor absorbs lower-class damage as 1 SV
- Damage penetrating lower-class armor translates UP (×10 multiplier)

### SV to Damage Conversion

See [GAME_RULES_SPECIFICATION.md](GAME_RULES_SPECIFICATION.md) for full damage tables.

---

## Combat Example

**Situation**: Aldric (Swords AS 14, Dodge AS 11, 5 Available AP, FAT 12) faces a Goblin (Daggers AS 8, Dodge AS 9, 3 Available AP).

**Round 1**:

1. **Initiative**: Aldric (5 AP) acts before Goblin (3 AP)

2. **Aldric's First Attack**:
   - Cost: 1 AP + 1 FAT → Now 4 AP, FAT 11
   - Roll: 14 + 4dF+ (+2) = 16 AV
   - Goblin defends (passive, no AP): TV = 9 - 1 = 8
   - SV = 16 - 8 = 8 → Hit! Proceed to damage.

3. **Aldric's Second Attack** (chooses to continue):
   - Cost: 1 AP + 1 FAT → Now 3 AP, FAT 10
   - Penalty: -1 AS for multiple action
   - Roll: 13 + 4dF+ (-1) = 12 AV
   - Goblin still passive: TV = 8
   - SV = 12 - 8 = 4 → Hit!

4. **Goblin's Turn** (3 AP):
   - Goblin attacks with dagger
   - Cost: 1 AP + 1 FAT → Now 2 AP
   - Roll: 8 + 4dF+ (+1) = 9 AV
   - Aldric actively dodges (1 AP + 1 FAT → Now 2 AP, FAT 9)
   - Aldric rolls: 11 + 4dF+ (+2) = 13 TV
   - SV = 9 - 13 = -4 → Miss!

**End of Round**:
- Aldric: Recovers FAT/4 = 2 AP (now 4 AP), +1 FAT baseline
- Goblin: Recovers FAT/4 AP, +1 FAT baseline

---

## Special Combat Actions

### Knockback

**Cost**: 1 AP + 1 FAT (or 2 AP)

Weapons with knockback capability can stun instead of damage:

- On success, target cannot act for **SV seconds**
- No damage dealt
- Useful for controlling enemies or creating escape opportunities

### Disarm

Disarm allows a character to attempt to knock a weapon or item out of an opponent's hands.

**Cost**: 1 AP + 1 FAT (or 2 AP)

**Mechanics**:
- Declare the target item or weapon before rolling AV
- Roll AV as normal
- Apply a **-2 AS penalty** for attempting to disarm
- If successful, the target drops the item or weapon
- Critical hits on disarm attempts may trigger additional effects (e.g., breaking the weapon)

### Called Shots

Called shots allow a character to target a specific body part of an opponent, potentially bypassing armor or inflicting additional effects.

**Cost**: 1 AP + 1 FAT (or 2 AP)

**Mechanics**:
- Declare the target location before rolling AV
- Roll AV as normal
- Apply a **-2 AS penalty** for targeting a specific location
- If successful, damage is applied to the chosen location, ignoring any armor not covering that location
- Critical hits on called shots may trigger additional effects (e.g., disarming, stunning)

### Stun or Knockout

Stun or Knockout allows a character to attempt to temporarily incapacitate an opponent, preventing them from taking actions for a short duration.

**Cost**: 1 AP + 1 FAT (or 2 AP)

**Mechanics**:
- Declare the target opponent before rolling AV
- Roll AV as normal
- Apply a **-2 AS penalty** for attempting to stun or knockout
- If successful, the target is stunned or knocked out for **SV seconds**
- Critical hits on stun or knockout attempts may trigger additional effects (e.g., longer duration, additional penalties)
- A "stun" is an effect on the target, like a wound - it goes in the target's status effects list and temporarily sets the target's FAT to 0.

---

## Combat Status Effects

### From Wounds

Each wound causes:
- **1 FAT damage** every 2 rounds
- **-2 AV penalty** to all actions (cumulative per wound)

Location-specific effects:
- **2 wounds to arm**: Arm is useless
- **2 wounds to leg**: Leg is useless
- **2 wounds to head**: Severe perception/mental penalties
- **2 wounds to torso**: Breathing difficulties, extra FAT drain

### From Low Health

See [ACTION_POINTS.md](ACTION_POINTS.md) for low FAT/VIT effects on action capability.

---

## Implementation Notes

### Combat Action Resolution Flow

1. Check initiative order (by Available AP)
2. Active character declares action and target
3. Check AP/FAT availability
4. Apply multiple action penalty if applicable
5. Apply any boosts
6. Attacker rolls AV
7. Defender chooses active or passive defense
8. Calculate TV
9. Calculate SV
10. If hit, resolve defense sequence
11. Apply damage
12. Deduct AP/FAT costs
13. Next character in initiative

### Required Tracking

Per character during combat:
- Available AP (changes each action)
- Current FAT (changes each action)
- Actions taken this round (for -1 penalty)
- Active cooldowns (seconds remaining)
- Parry mode status
- Active status effects (wounds, buffs, debuffs)
