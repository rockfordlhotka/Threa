# Combat System

## Overview

Combat in Threa uses a skill-based resolution system built on the 4dF+ dice mechanic. Combat integrates with the Action Points (AP) and Time systems to create tactical depth where characters must manage their action economy and fatigue.

Combat actions follow the universal action resolution framework defined in [ACTIONS.md](ACTIONS.md). This document provides combat-specific rules and extensions.

**Related Documents**:
- [Actions](ACTIONS.md) - Universal action resolution framework
- [Action Points](ACTION_POINTS.md) - AP calculation, costs, and recovery
- [Time System](TIME_SYSTEM.md) - Rounds, initiative, and cooldowns
- [Game Rules Specification](GAME_RULES_SPECIFICATION.md) - Dice mechanics, damage tables
- [Ranged Weapons (Sci-Fi)](RANGED_WEAPONS_SCIFI.md) - Modern/futuristic firearms, fire modes, ammo types
- [Implants System](IMPLANTS_SYSTEM.md) - Cybernetic implants, installation, activation
- [Effects System](EFFECTS_SYSTEM.md) - Status effects, buffs, debuffs

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

Each ranged weapon has a **Short Range** value. Range categories are derived from this:

| Category | Range Values | AV Modifier |
|----------|--------------|-------------|
| **Short** | 1 to [Short Range] | +0 |
| **Medium** | Short Range + 1 | -1 |
| **Long** | Short Range + 2 | -2 |
| **Extreme** | Short Range + 3 | -4 |

**Example - Straight Bow (Short Range 4)**:
| Category | Range Values | Distance (meters) | AV Mod |
|----------|--------------|-------------------|--------|
| Short | 1-4 | 1-16m | +0 |
| Medium | 5 | 25m | -1 |
| Long | 6 | 36m | -2 |
| Extreme | 7 | 49m | -4 |

**Example Weapon Short Ranges**:
| Weapon | Short Range | Extreme Range | Max Distance |
|--------|-------------|---------------|--------------|
| Thrown Dagger | 2 | 5 | 25m |
| Shortbow | 3 | 6 | 36m |
| Straight Bow | 4 | 7 | 49m |
| Longbow | 5 | 8 | 64m |
| Crossbow | 6 | 9 | 81m |
| Pistol | 4 | 7 | 49m |
| Rifle | 6 | 9 | 81m |
| Sniper Rifle | 8 | 11 | 121m |

### Ranged Attack

**Cost**: 1 AP + 1 FAT (or 2 AP)

1. Attacker declares target and weapon
2. Determine range category based on distance to target
3. Apply range AV modifier to weapon skill
4. Attacker rolls: **(Weapon Skill AS + Range Modifier) + 4dF+** = AV
5. Defender determines TV (passive defense or active dodge)
6. SV = AV - TV
7. If SV ≥ 0, proceed to damage resolution

**Base TV**: Ranged attacks use **passive defense TV** (Dodge AS - 1) unless the defender spends an action to actively dodge.

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
| Attacker in motion | -2 AV (penalty to attacker) |
| **Target Size** | |
| Tiny target | +2 |
| Small target | +1 |

Modifiers stack. Example: Small target (+1 TV) behind ½ cover (+1 TV) at long range (-2 AV) = effective -2 AV and +2 TV

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

### Reload Mechanics

**Reload is an Action**: When a ranged weapon's loaded ammo reaches 0, it must be reloaded before firing again. Reloading always costs an action.

| Weapon Type | Reload Action | Cost | Result |
|-------------|---------------|------|--------|
| **Bow** | Nock Arrow | 1 AP + 1 FAT | +1 arrow loaded |
| **Crossbow** | Load Bolt | 1 AP + 1 FAT | +1 bolt loaded |
| **Revolver** | Load Round | 1 AP + 1 FAT | +1 round loaded |
| **Magazine Weapon** | Magazine Swap | 1 AP + 1 FAT | Full capacity |
| **Thrown** | Draw Weapon | 1 AP + 1 FAT | Ready to throw |

**Weapon Capacity Examples**:
- Bow: 1 (reload after every shot)
- Crossbow: 1 (reload after every shot)
- Revolver: 6 (reload after 6 shots)
- Pistol: 8-18 (reload when magazine empty)

**Speed and Skill**:
- **Speed Reload** (skill 6+): 2 AP, can fire same round
- **Prepped Ammunition**: If ammo was prepped in advance, reload is instant (0 additional cost)

**Multiple Shots Per Round**:
Characters with sufficient AP can fire multiple times per round, limited by:
- Available AP (each shot costs 1 AP + 1 FAT)
- Loaded ammo (must reload when empty)
- Prepped ammunition (enables rapid fire without reload actions)

See [Ranged Weapons (Sci-Fi)](RANGED_WEAPONS_SCIFI.md) for modern/futuristic weapon mechanics including fire modes and ammunition types.

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

| Class | Scale | Fantasy Examples | Sci-Fi Examples |
|-------|-------|------------------|-----------------|
| Class 1 | Normal | Human weapons, standard combat | Pistols, rifles, personal melee |
| Class 2 | Heavy (10×) | Vehicles, giant creatures | Powered armor, APCs, anti-materiel rifles |
| Class 3 | Massive (100×) | Armored vehicles, dragons | Tanks, gunships, mech suits |
| Class 4 | Siege (1000×) | Structures, legendary magic | Starship weapons, orbital strikes |

**Class Interactions**:
- Higher-class armor **absorbs** lower-class damage as 1 SV maximum (trivial damage)
- When damage **penetrates** from a higher DC to a lower DC target inside, SV is multiplied by 10×

### Armor Piercing (AP Offset) and Armor Vulnerable (SV Max)

Some weapons and ammunition modify how armor absorption works against them. These properties are defined per damage type on the weapon/ammo profile.

#### AP Offset (Armor Piercing)

AP offset reduces the armor's **base absorption value** before damage class scaling is applied. This represents rounds designed to penetrate armor material.

**Formula**: `effectiveAbsorption = max(0, rawAbsorption - apOffset)`

The reduced value is then processed through normal DC scaling.

**Example - AP Rounds vs Plate Armor**:
```
Attack: Rifle with AP 5 hits with SV 10
Target: Plate armor (absorption 12, DC1)

Step 1: Apply AP offset
  - Raw absorption: 12
  - AP offset: 5
  - Reduced absorption: max(0, 12 - 5) = 7

Step 2: Normal absorption
  - Armor absorbs 7 SV
  - Penetrating SV: 10 - 7 = 3
```

AP offset applies to both armor and shields. When merging weapon + ammo profiles, AP offsets are **summed**.

#### SV Max (Armor Vulnerable)

SV Max caps the effective Success Value when armor defense exceeds the threshold. This represents ammunition that works well against soft targets but is stopped cold by strong armor (e.g., hollow-point or soft-target rounds).

**Rule**: If total armor absorption > SvMax, the remaining SV after absorption is set to **0** (the round is completely stopped). Armor durability takes SvMax damage.

If total armor absorption ≤ SvMax (or SvMax is null), normal damage processing applies.

**Example - Soft-Point Rounds vs Heavy Armor**:
```
Attack: Pistol with SvMax 5 hits with SV 10
Target: Heavy armor (absorption 8, DC1)

Step 1: Check SvMax
  - Total absorption (8) > SvMax (5)
  - SvMax triggered!

Step 2: Apply cap
  - Remaining SV: 0 (round stopped by armor)
  - Armor durability loss: 5 (takes SvMax damage)
```

**Example - Soft-Point Rounds vs Light Armor**:
```
Attack: Pistol with SvMax 5 hits with SV 10
Target: Light armor (absorption 3, DC1)

Step 1: Check SvMax
  - Total absorption (3) ≤ SvMax (5)
  - Normal processing

Step 2: Normal absorption
  - Armor absorbs 3 SV
  - Penetrating SV: 10 - 3 = 7
```

When merging weapon + ammo profiles, SvMax takes the **minimum** non-null value (most restrictive cap wins).

#### Combined AP Offset and SV Max

A weapon can have both AP offset and SV Max. AP offset is applied first (reducing armor absorption), then SV Max is checked against the resulting total absorption.

### Damage Class Escalation

When a weapon deals exceptionally high damage, the damage value itself determines the effective damage class. This allows powerful weapons or exceptional rolls to punch above their weight class.

| Damage Value | Effective DC | Notes |
|--------------|--------------|-------|
| 1-19 | Weapon's DC | Normal damage |
| 20-29 | DC 2 (minimum) | Crosses into "heavy" damage |
| 30-39 | DC 3 (minimum) | Crosses into "massive" damage |
| 40+ | DC 4 (minimum) | Crosses into "siege" damage |

**Key Rules**:
- The effective DC is the **higher** of the weapon's innate DC and the damage-based DC
- A DC1 pistol dealing 22 damage becomes DC2 damage against DC2 armor
- A DC2 railgun dealing 35 damage becomes DC3 damage against DC3 armor
- This scaling applies AFTER armor absorption at the weapon's base DC

**Example - Pistol vs Powered Armor**:
1. Pistol (DC1) scores SV 8, rolls 2d10 = 17 damage
2. DC2 powered armor absorbs DC1 damage as 1 SV → 16 damage gets through
3. 16 < 20, so damage remains DC1 against the wearer's DC1 body
4. Apply 16 damage to FAT/VIT per damage table

**Example - Pistol Critical vs Powered Armor**:
1. Pistol (DC1) scores SV 12 (critical!), rolls 4d10 = 28 damage
2. DC2 powered armor absorbs DC1 damage as 1 SV → 27 damage gets through
3. 27 is in 20-29 range = DC2 damage
4. Since target's body is DC1, this DC2 damage is devastating (×10 effect)

**Example - Railgun vs Tank**:
1. Railgun (DC2) scores SV 10, rolls 3d10 = 24 damage
2. Tank (DC3) armor absorbs DC2 damage as 1 SV → 23 damage
3. 23 is in 20-29 range = DC2 (no escalation, weapon already DC2)
4. 23 damage applied normally to tank systems

### Powered Armor and Vehicles (Layered Defense)

Powered armor and vehicles act as **layered defense**. The outer shell absorbs damage at its damage class, but any penetrating damage translates down to affect occupants inside.

**Powered Armor** (DC2):
- Provides DC2 protection to the wearer
- DC1 weapons deal 1 SV maximum (trivial scratches)
- Wearer's body remains DC1 inside the armor
- Armor has its own structure pool (typically 20-50)
- Characters can wear additional DC1 armor underneath

**Penetration and Translation**:
When damage exceeds the outer armor's absorption, the remaining SV penetrates and is **multiplied by 10×** as it translates down to the next damage class layer.

**Example - Arnie in Powered Armor**:
```
Arnie wears:
  - Powered Armor (DC2, absorbs 4 SV)
  - Blast-resistant shirt (DC1, absorbs 3 SV)

Attack: Railgun (DC2 weapon) hits with SV 5

Step 1: Powered Armor (DC2) absorbs damage
  - Armor absorbs 4 SV
  - 1 SV penetrates the outer shell

Step 2: Penetrating damage translates to DC1
  - 1 SV at DC2 → 10 SV at DC1 (×10 multiplier)
  - This represents the catastrophic effect of a heavy round
    punching through into the vulnerable interior

Step 3: Inner armor (DC1) absorbs remaining damage
  - Blast shirt absorbs 3 SV
  - 10 - 3 = 7 SV reaches Arnie

Step 4: Damage resolution
  - Roll damage at SV 7 against Arnie normally
  - SV 7 = 2d8 damage dice
```

**Vehicles** (DC2-3):
- Hull absorbs damage at vehicle's DC
- Penetrating damage affects crew at DC1 (×10 per class difference)
- Crew compartment may have separate armor rating
- Hull damage vs crew damage tracked separately
- Disabled vehicles may still provide cover
- See [Ranged Weapons (Sci-Fi)](RANGED_WEAPONS_SCIFI.md) for vehicle-scale weapons

**Example - Tank Crew**:
```
Tank (DC3, absorbs 6 SV) hit by anti-tank missile (DC3) at SV 8

Step 1: Tank hull absorbs 6 SV → 2 SV penetrates
Step 2: 2 SV at DC3 → 20 SV at DC2 → 200 SV at DC1
        (This is catastrophic - crew is killed instantly)
```

**Targeting Weak Points**:
- Called shot to weak point: -4 AS penalty
- Success: Damage bypasses outer armor entirely (no absorption)
- Examples: Vision slit, joint, exhaust port, sensor array

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
