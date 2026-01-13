# Threa - Game Rules Specification

## Overview

This document defines the core game mechanics and rules for the Threa tabletop role-playing game system. These rules are used by the Threa TTRPG Character Sheet Assistant to help players manage their characters, calculate derived values, and resolve game mechanics during tabletop play.

## Core Dice Mechanics

Threa uses a **4dF (Fudge/Fate Dice)** system as the foundation for all skill checks, combat resolution, and random events. This provides consistent, predictable probability curves that favor average results while still allowing for dramatic successes and failures.

### Fudge Dice (dF) Properties

- **6-sided dice** with special faces:
  - **2 "+" sides**: Positive result (+1 each)
  - **2 "-" sides**: Negative result (-1 each)
  - **2 blank sides**: Neutral result (0 each)
- **4dF roll range**: -4 to +4 (bell curve distribution)
- **Average result**: 0 (most common outcome)
- **Probability distribution**: Heavily weighted toward 0, with decreasing probability for extreme results

### Skill Check Resolution

- **Skill Check Formula**: `Character Skill + 4dF+ vs. Target Number or Opposing Skill + 4dF+`
- **Combat Resolution**: `Attack Skill + 4dF+ vs. Defense Skill + 4dF+`
- **Difficulty Modifiers**: Applied as bonuses/penalties to the target number
- **Margin of Success**: Difference between final results determines outcome quality

### Dice Roll Notation

- **4dF**: Standard fudge dice roll (no exploding mechanics)
- **4dF+**: Exploding fudge dice roll (uses exploding mechanics described below)

### Exploding Dice Mechanics

For dramatic moments and critical situations, some rolls use **exploding dice** rules (denoted as **4dF+**):

- **+4 Result (Maximum Success)**:
  - Roll 4dF again, but **only count the "+" results**
  - Add these additional "+" results to the total
  - Continue rolling if another +4 is achieved (rare but possible)

- **-4 Result (Critical Failure)**:
  - Roll 4dF again, but **only count the "-" results**
  - Add these additional "-" results to the total (making failure worse)
  - Continue rolling if another -4 is achieved

### When Exploding Dice Apply

- **Combat**: Critical hits and fumbles use **4dF+** exploding mechanics
- **High-Stakes Skill Checks**: Important story moments, dangerous actions use **4dF+**
- **Spell Casting**: Particularly powerful or risky magic uses **4dF+**
- **Crafting**: When attempting masterwork items or using rare materials uses **4dF+**
- **Social Encounters**: Dramatic persuasion, intimidation, or deception attempts use **4dF+**
- **Routine Non-Skill Rolls**: Only pure random events may use non-exploding **4dF**; all skill-driven actions use **4dF+**

### Examples

- **Standard Combat**: Sword skill 12 + **4dF+** (+1) = 13 vs. Dodge skill 10 + **4dF+** (-1) = 9. Attack succeeds by 4.
- **Exploding Success**: Fire Bolt skill 8 + **4dF+** (+4) = 12, then exploding roll 4dF yields 2 more "+" = 14 total. Devastating magical attack!
- **Exploding Failure**: Lockpicking skill 6 + **4dF+** (-4) = 2, then exploding roll 4dF yields 2 more "-" = 0 total. Lock mechanism jams permanently.

---

## Attribute System and Species Modifiers

### Base Attribute Calculation

All characters start with eight core attributes that serve as the foundation for all derived skills and secondary statistics. These attributes are calculated using the 4dF system combined with species-specific modifiers.

#### Human Baseline (Default Species)

For Humans, all seven attributes are calculated as:
**Attribute Value = 4dF + 10**

This gives each Human attribute an average value of **10**, with the following probability distribution:

- **Range**: 6-14 (possible values)
- **Average**: 10 (most common result)
- **Distribution**: Bell curve weighted toward 10, with decreasing probability for extreme values

#### Core Attributes

All characters possess these eight core attributes:

1. **Physicality (STR)** - Physical strength and power
2. **Dodge (DEX)** - Agility and evasion ability
3. **Drive (END)** - Endurance and stamina
4. **Reasoning (INT)** - Intelligence and logical thinking
5. **Awareness (ITT)** - Intuition and perception
6. **Focus (WIL)** - Willpower and mental concentration
7. **Influence (PHY)** - Physical beauty and attractiveness
8. **Bearing (SOC)** - Social standing and presence

### Species Attribute Modifiers

Each non-Human species applies specific modifiers to the base Human attribute calculation during character creation, creating distinct racial advantages and disadvantages:

#### Available Species and Modifiers

- **Human**: No modifiers (baseline)
  - All attributes: 4dF + 10

- **Elf**: Intellectual and agile, but physically delicate
  - **Reasoning (INT)**: 4dF + 11 (+1 modifier to attribute)
  - **Physicality (STR)**: 4dF + 9 (-1 modifier to attribute)
  - All other attributes: 4dF + 10

- **Dwarf**: Strong and resilient, but less agile
  - **Physicality (STR)**: 4dF + 11 (+1 modifier to attribute)
  - **Dodge (DEX)**: 4dF + 9 (-1 modifier to attribute)
  - All other attributes: 4dF + 10

- **Halfling**: Quick and perceptive, but physically weak
  - **Dodge (DEX)**: 4dF + 11 (+1 modifier to attribute)
  - **Awareness (ITT)**: 4dF + 11 (+1 modifier to attribute)
  - **Physicality (STR)**: 4dF + 8 (-2 modifier to attribute)
  - All other attributes: 4dF + 10

- **Orc**: Physically powerful and enduring, but less intelligent and social
  - **Physicality (STR)**: 4dF + 12 (+2 modifier to attribute)
  - **Drive (END)**: 4dF + 11 (+1 modifier to attribute)
  - **Reasoning (INT)**: 4dF + 9 (-1 modifier to attribute)
  - **Bearing (SOC)**: 4dF + 9 (-1 modifier to attribute)
  - All other attributes: 4dF + 10

### Attribute Usage and Relationships

#### Direct Attribute Effects

While all action resolution uses skills rather than raw attributes, attributes serve several critical functions:

1. **Skill Starting Values**: Many skills begin with a base value derived from related attributes
2. **Health Calculations**: Primary and secondary health pools are calculated from attributes
3. **Skill Advancement Modifiers**: Higher attributes may provide learning bonuses for related skills
4. **Equipment Requirements**: Some items may require minimum attribute thresholds
5. **Skill Caps**: Attributes may influence the maximum potential of related skills

#### Health Pool Calculations

- **Fatigue (FAT)**: (END + WIL) - 5
  - Represents stamina, exhaustion, and non-lethal damage capacity
  - **Low fatigue effects** (applies based on current FAT after pending damage):
    - **FAT = 3**: Must pass a Focus skill check (AS + 4dF+) against target value (TV) 5 or the action fails (no fatigue cost)
    - **FAT = 2**: Must pass a Focus skill check (AS + 4dF+) against TV 7 or the action fails (no fatigue cost)
    - **FAT = 1**: Must pass a Focus skill check (AS + 4dF+) against TV 12 or the action fails (no fatigue cost)
    - **FAT = 0**: Character immediately suffers 2 Vitality damage and cannot perform actions until FAT recovers above 0
- **Vitality (VIT)**: (STR × 2) - 5
  - Represents life force and lethal damage capacity
  - Baseline recovery restores 1 VIT every hour when the character is alive (VIT > 0)
  - **Low vitality effects** (applies based on current VIT after pending damage):
    - **VIT = 4**: Fatigue recovery slows to 1 point per minute
    - **VIT = 3**: Fatigue recovery slows to 1 point every 30 minutes and requires a Focus skill check (AS + 4dF+) against TV 7 to attempt any action
    - **VIT = 2**: Fatigue recovery slows to 1 point per hour and requires a Focus skill check (AS + 4dF+) against TV 12 to attempt any action
    - **VIT = 1**: Fatigue recovery halts entirely and the character cannot perform actions
    - **VIT = 0**: The character dies immediately

#### Skill Relationship Examples

- **Weapon Skills**: May use Physicality for damage bonuses and weapon requirements
- **Dodge Skill**: Starts with base value influenced by Dodge attribute
- **Spell Skills**: Different spells may have attribute relationships (Fire Bolt with Focus, etc.)
- **Social Skills**: Bearing influences persuasion, intimidation, and leadership abilities
- **Crafting Skills**: Reasoning affects complex crafting and blueprint understanding

### Character Creation

During character creation:

1. **Species Selection**: Player chooses species, which determines attribute modifiers
2. **Attribute Rolling**: Roll 4dF for each attribute and apply species modifiers to the base
3. **Health Calculation**: Fatigue and Vitality are automatically calculated from final attribute values
4. **Starting Skills**: Core attribute skills are set to their calculated attribute values
5. **Skill Point Allocation**: Player receives bonus skill points to distribute among learned skills

---

## Skill System

### Skill System Architecture

- **Core Attribute Skills** (all characters start with these):
  - Physicality (STR) - physical strength and power
  - Dodge (DEX) - agility and evasion
  - Drive (END) - endurance and stamina
  - Reasoning (INT) - intelligence and logic
  - Awareness (ITT) - intuition and perception
  - Focus (WIL) - willpower and concentration
  - Influence (PHY) - physical beauty and attractiveness
  - Bearing (SOC) - social standing and presence

- **Health System**:
  - **Fatigue (FAT)**: Calculated as (END + WIL) - 5, represents stamina and exhaustion
    - Baseline recovery restores 1 FAT every 3 seconds outside of pending damage application
  - **Vitality (VIT)**: Calculated as (STR × 2) - 5, represents physical health and life force
  - Both FAT and VIT track current values that can be reduced by damage and restored by rest/healing
  - Death occurs when VIT reaches 0; unconsciousness when FAT reaches 0
  - **Pending Damage/Healing System**:
    - Each character has pending FAT and VIT pools that accumulate damage/healing
    - Positive pending values represent damage to be applied over time
    - Negative pending values represent healing to be applied over time
- **Drive Action**: Characters with Drive ability scores (AS) of 8 or higher can perform a Drive action to trade vitality for fatigue recovery. The action rolls a Drive skill check against TV 8 and, on success, queues 1 point of pending VIT damage and FAT healing equal to `AS - TV + 2`.
  - Every 3 seconds, half the pending pool value is applied to current health (rounded to ensure pools reach zero)
    - This creates a gradual damage/healing effect rather than instant application

- **Weapon Skills** (learned through practice):
  - Swords, Axes, Maces, Polearms, Bows, Crossbows, Throwing, etc.

- **Individual Spell Skills** (each spell is its own skill):
  - Fire Bolt, Heal, Lightning Strike, Invisibility, etc.
  - Organized by magic schools for mana management

- **Mana Recovery Skills** (per magic school):
  - Fire Mana Recovery, Healing Mana Recovery, etc.
  - Determines how quickly mana regenerates for that school

- **Crafting Skills**:
  - Blacksmithing, Alchemy, Carpentry, Cooking, etc.

### Practice-Based Progression

- Skills improve through actual use (practice-based advancement)
- No traditional "experience points" or "levels"
- Skill advancement rates vary by skill type and difficulty
- Actions are resolved through skill checks, never direct attribute use
- Higher skills unlock advanced techniques and abilities

---

## Skill Progression Mechanics

### Usage-Based Advancement System

All skills advance through actual usage rather than traditional experience points. Each skill tracks usage events and converts them into skill level increases based on that skill's individual progression parameters.

### Base Cost and Multiplier System

Each skill is defined with two key progression parameters:

1. **Base Cost**: The number of usage events required to advance from level 0 to level 1
2. **Multiplier**: A real number that compounds the cost for each subsequent level

#### Cost Calculation Formula

The number of usage events required to advance from level N to level N+1 is calculated as:

Cost (N → N+1) = Base Cost × (Multiplier^N)

Where:

- **Base Cost**: Skill-specific starting difficulty (typically 10-100 usage events)
- **Multiplier**: Progression difficulty scaling (typically 2.0-3.5)
- **N**: Current skill level

### Skill Category Progression Parameters

Different categories of skills have distinct progression parameters reflecting their learning difficulty. **Game Balance Note**: Skills are not intended to progress significantly beyond level 10, as each level represents a substantial improvement. Progression becomes prohibitively expensive after level 5 to maintain balanced gameplay.

#### Core Attribute Skills

- **Base Cost**: 15 (easy to improve early attributes)
- **Multiplier**: 2.5 (becomes very expensive after level 5)
- **Rationale**: Fundamental abilities that see frequent use but become exponentially harder to master

#### Weapon Skills

- **Base Cost**: 25 (moderate starting difficulty)
- **Multiplier**: 2.2 (steep progression curve after early levels)
- **Rationale**: Combat skills require practice but become extremely difficult to master beyond competent levels

#### Individual Spell Skills

- **Base Cost**: Variable by spell complexity
  - **Cantrips** (Basic spells): 20, Multiplier 2.0
  - **Standard spells**: 40, Multiplier 2.3
  - **Advanced spells**: 80, Multiplier 2.8
  - **Master spells**: 150, Multiplier 3.5
- **Rationale**: Magic mastery scales dramatically with spell power and complexity; high-level spells should be extremely rare

#### Mana Recovery Skills

- **Base Cost**: 30 (moderate starting investment)
- **Multiplier**: 2.1 (steady but steep improvement)
- **Rationale**: Magical stamina training follows consistent progression patterns but plateaus quickly

#### Crafting Skills

- **Base Cost**: 35 (requires sustained practice)
- **Multiplier**: 2.4 (becomes quite difficult to master)
- **Rationale**: Craftsmanship demands both repetition and increasingly refined technique

#### Social Skills

- **Base Cost**: 20 (natural social interaction provides frequent practice)
- **Multiplier**: 2.0 (personality development has diminishing returns)
- **Rationale**: Basic social skills develop naturally but mastery requires dedicated effort

### Example Progression Costs

**Weapon Skill** (Base Cost: 25, Multiplier: 2.2):

- Level 0→1: 25 uses
- Level 1→2: 55 uses (25 × 2.2¹)
- Level 2→3: 121 uses (25 × 2.2²)
- Level 3→4: 266 uses (25 × 2.2³)
- Level 4→5: 585 uses (25 × 2.2⁴)
- Level 5→6: 1,287 uses (25 × 2.2⁵) - **Extremely expensive**

**Core Attribute** (Base Cost: 15, Multiplier: 2.5):

- Level 0→1: 15 uses
- Level 1→2: 38 uses (15 × 2.5¹)
- Level 2→3: 94 uses (15 × 2.5²)
- Level 3→4: 234 uses (15 × 2.5³)
- Level 4→5: 586 uses (15 × 2.5⁴)

### Usage Event Types and Values

Not all skill usage generates the same advancement potential:

1. **Routine Use** (1.0x multiplier): Standard skill application under normal conditions
2. **Challenging Use** (1.5x multiplier): Skill use under difficult circumstances or against higher-level opposition
3. **Critical Success** (2.0x multiplier): Exceptional skill performance (exploding dice results)
4. **Teaching Others** (0.8x multiplier): Instructing other players provides modest skill advancement
5. **Training Practice** (0.5x multiplier): Deliberate practice in safe conditions (slower but guaranteed advancement)

### Skill Level Ranges and Meaning

- **Levels 1-3**: **Novice** - Learning fundamentals, frequent advancement possible
- **Levels 4-5**: **Competent** - Reliable skill use, moderate advancement rate
- **Levels 6-7**: **Expert** - Advanced techniques available, very slow advancement
- **Levels 8-10**: **Master** - Exceptional ability, extremely rare to achieve
- **Levels 11+**: **Legendary** - Mythical skill levels, virtually impossible without extraordinary circumstances

---

## Combat System

### Core Combat Mechanics

Combat uses a skill-based resolution system built on the 4dF+ dice mechanic. All combat actions consume Fatigue (FAT), creating a natural pacing mechanism where characters tire from extended fighting.

#### Attack and Defense Resolution

**Attack Value (AV)**: For all attacks, the attacker rolls:

- **AV = Ability Score (AS) + 4dF+**
- The exploding dice mechanic (4dF+) automatically applies to all attack rolls

**Success Value (SV)**: The margin of success or failure:

- **SV = AV - TV (Target Value)**
- SV ≥ 0 indicates a successful attack
- Higher SV values result in more damage
- SV < 0 indicates a failed attack
- SV ≤ -3 triggers negative consequences (see Result Value System)

#### Melee Combat

**Attacking**: Melee attacks cost **1 FAT** and use the weapon's associated skill.

- Attacker rolls: **Weapon Skill AS + 4dF+** = AV
- If the defender is not surprised and has FAT > 0, they automatically defend

**Defending**: The defender uses their Dodge (DEX) skill.

- Defender rolls: **Dodge AS + 4dF+** = TV
- Defending costs **1 FAT**
- If the attack succeeds (AV ≥ TV), armor and shields may still absorb damage

**Parry Mode**: Characters can enter parry mode as an action.

- While in parry mode, the character uses their primary weapon skill for defense instead of Dodge
- Parry does NOT cost FAT per defense (unlike Dodge)
- Parry only works against melee attacks (not ranged)
- Character must meet minimum skill level to use the equipped weapon
- Parry mode ends when the character takes another action

**Dual Wielding**: Attacking with both weapons costs **2 FAT** total.

- Primary hand attacks normally: **Weapon Skill AS + 4dF+**
- Off-hand attacks at penalty: **Weapon Skill AS - 2 + 4dF+**

#### Ranged Combat

**Range Categories**:

- **Range 0**: Melee only
- **Range 1**: Short range (same room/area)
- **Range 2**: Long range (adjacent area)

**Ranged Attack Resolution**:

- Attacker rolls: **Ranged Weapon Skill AS + 4dF+** = AV
- Target Value starts at **TV = 8** and is modified by circumstances:
  - Target has taken any action within 3 seconds: **+2 TV**
  - Target is hiding/under cover: **+2 TV**
  - Target is at long range: **+2 TV**
  - Area is crowded (>3 characters/creatures): **+1 TV**

**Ranged Weapon Cooldowns** (based on skill level):

- Level 0: 6 seconds
- Level 1: 5 seconds
- Level 2: 4 seconds
- Level 3: 3 seconds
- Level 4-5: 2 seconds
- Level 6-7: 1 second
- Level 8-9: 0.5 seconds
- Level 10+: No cooldown

**Ammunition**: Ranged weapons (bows, crossbows) require ammunition.

- Ammunition is consumed on each shot
- Different ammunition types may provide bonuses or special effects

**Thrown Weapons**: Daggers, darts, and other thrown items use the same melee weapon skill.

- Use ranged attack TV modifiers
- Thrown items are removed from equipped/inventory
- Can be retrieved after combat

#### Physicality and Damage Bonus

After a successful melee or thrown weapon attack, a **Physicality (STR) skill check** occurs automatically at **0 FAT cost**.

- Roll: **Physicality AS + 4dF+** vs **TV 8**
- Calculate Result Value: **RV = Physicality Roll - 8**
- Apply Result Value System (RVS) modifiers to SV or AV

**Result Value System (RVS) Chart**:

| RV | Effect on SV/AV |
|----|-----------------|
| -10 to -9 | -3 AV for 3 rounds |
| -8 to -7 | -2 AV for 2 rounds |
| -6 to -5 | -2 AV for 1 round |
| -4 to -3 | -1 AV for 1 round |
| -2 to +1 | +0 SV (no effect) |
| +2 to +3 | +1 SV |
| +4 to +7 | +2 SV |
| +8 to +11 | +3 SV |
| +12 to +18 | +4 SV |

#### Defense Sequence

When an attack succeeds (SV ≥ 0), damage is absorbed in the following order:

1. **Shield Defense** (if equipped and location allows):
   - Costs **1 FAT** per use
   - Shield skill check: **Shield Skill AS + 4dF+** vs **TV 8**
   - On success, shield absorbs SV based on its absorption rating for the damage type
   - Shield durability reduced by absorbed SV
   - Shields can absorb damage to any hit location

2. **Armor Defense** (if location is armored):
   - Does NOT cost FAT
   - Armor skill check: **Armor Skill AS + 4dF+** vs **TV 8**
   - On success, armor absorbs SV based on its absorption rating for the damage type
   - Armor durability reduced by absorbed SV
   - Multiple armor pieces layer: outer garments absorb before inner
   - Only armor covering the hit location applies

3. **Damage Application**: Any remaining SV after absorption determines damage to the character

#### Hit Locations

Hit location is determined by **1d12** roll:

| Roll | Location | Armor Slots That Protect |
|------|----------|--------------------------|
| 1 | Head (roll 1d12 again: 1-6=Head, 7-12=Torso) | Head, Face, Ears, Neck |
| 2-6 | Torso | Neck, Shoulders, Back, Chest, Waist |
| 7 | Left Arm | Shoulders, Arms(L), Wrists(L), Hands(L) |
| 8 | Right Arm | Shoulders, Arms(R), Wrists(R), Hands(R) |
| 9-10 | Left Leg | Waist, Legs, Ankles(L), Feet(L) |
| 11-12 | Right Leg | Waist, Legs, Ankles(R), Feet(R) |

### Damage System

#### Damage Classes

Damage and durability are organized into **classes** representing scale:

- **Class 1**: Normal weapons, human-scale combat
- **Class 2**: Vehicles, giant creatures, falling trees (10× Class 1)
- **Class 3**: Armored vehicles, structures, dragons (10× Class 2)
- **Class 4**: Armored structures, high-end magic (10× Class 3)

**Class Interactions**:

- Higher-class armor **completely absorbs** lower-class damage as 1 SV
- Same-class armor absorbs normally
- Damage that **penetrates** lower-class armor is translated UP in class (×10 multiplier)
- Layered armor prevents class multiplication until damage reaches a lower class

#### SV to Damage Roll Conversion

After final SV is calculated (including absorption), consult the damage roll table:

| SV | Damage Roll | | SV | Damage Roll |
|----|-------------|---|----|-------------|
| 0 | 1d6÷3 | | 11 | 3d12 |
| 1 | 1d6÷2 | | 12 | 4d10 |
| 2 | 1d6 | | 13 | 4d10 |
| 3 | 1d8 | | 14 | 4d10 |
| 4 | 1d10 | | 15 | 1d6 Class+1 |
| 5 | 1d12 | | 16 | 1d6 Class+1 |
| 6 | 1d6+1d8 | | 17 | 1d8 Class+1 |
| 7 | 2d8 | | 18 | 1d8 Class+1 |
| 8 | 2d10 | | 19 | 1d10 Class+1 |
| 9 | 2d12 | | 20+ | 1d10 Class+1 |
| 10 | 3d10 | | | |

#### Damage to Health Pools

After rolling damage dice, convert the total to FAT/VIT damage:

| Damage | FAT | VIT | Wounds |
|--------|-----|-----|--------|
| 1-4 | = Damage | 0 | 0 |
| 5 | 5 | 1 | 0 |
| 6 | 6 | 2 | 0 |
| 7 | 7 | 4 | 1 |
| 8 | 8 | 6 | 1 |
| 9 | 9 | 8 | 1 |
| 10 | 10 | 10 | 2 |
| 11 | 11 | 11 | 2 |
| 12 | 12 | 12 | 2 |
| 13 | 13 | 13 | 2 |
| 14 | 14 | 14 | 2 |
| 15 | 15 | 15 | 3 |
| 16 | 16 | 16 | 3 |
| 17 | 17 | 17 | 3 |
| 18 | 18 | 18 | 3 |
| 19 | 19 | 19 | 3 |
| 20 | 20 | 20 | 4 (Apply as Class+1: ×10) |

**Pending Damage Pools**: Damage is not applied instantly but accumulates in pending pools.

- Damage accumulates in pending FAT and VIT pools (positive values)
- Every 3 seconds, half the pending damage is applied to current health values
- Multiple attacks can accumulate in pending pools before being applied
- This creates realistic "bleeding out" or gradual injury effects

**Healing System**: Works through the same pending pool mechanism.

- Healing adds negative values to pending pools (representing recovery)
- Negative pending values gradually restore current health over time
- Allows for "healing over time" effects and prevents instant full healing

#### Wounds

**Wound Effects**:

- Wounds are long-term injuries that take **hours** to heal naturally
- Each wound causes **1 FAT damage every 2 rounds** (6 seconds)
- Each wound applies **-2 AV penalty** to all actions
- Wounds stack cumulatively

**Location-Specific Wound Effects**:

- **2 wounds to one arm**: That arm is useless (cannot hold items, use two-handed weapons, etc.)
- **2 wounds to one leg**: That leg is useless (movement severely impaired)
- **2 wounds to head**: Severe penalties to perception and mental skills
- **2 wounds to torso**: Breathing difficulties, additional FAT drain

**Wound Recovery**:

- Natural healing: 1 wound per 4 hours of rest
- Medical treatment can speed recovery
- Magical healing can remove wounds instantly

---

## Equipment Mechanics

### Weapon Properties

All weapons have the following properties:

- **Associated Skill**: Specific weapon skill (Swords, Axes, Bows, etc.)
- **Minimum Skill Level**: Required skill level to use effectively (default: 0)
- **Damage Type**: Bashing, Cutting, Piercing, Projectile, Energy, Heat, Cold, Acid
- **Damage Class**: 1-4 (default: 1)
- **Base SV Modifier**: Added to attack SV after success (can be positive or negative)
- **AV Modifier**: Bonus or penalty to attack rolls (quality, magic, or heavy weapons)
- **DEX Modifier**: Affects wielder's Dodge skill (typically negative for heavy weapons)
- **Range**: 0 (melee), 1 (short), 2 (long)
- **Knockback Capable**: Whether weapon can perform knockback attacks
- **Durability**: Current/Maximum durability
- **Two-Handed**: Whether weapon requires both hands

**Two-Handed Weapons**:

- Cannot be used with a shield
- Typically have higher base SV modifiers
- May have negative AV modifiers (harder to hit, but devastating damage)
- Often have negative DEX modifiers

**Knockback Attacks**: Weapons with knockback capability can perform special attacks.

- Knockback is a separate action (costs 1 FAT)
- Instead of causing damage, successful knockback prevents target from acting
- Duration: **SV seconds** (the success value determines how long target is stunned)
- Example: Polearms, staves, shields can knockback

### Armor Properties

All armor pieces have the following properties:

- **Associated Skill**: Armor type skill (Light Armor, Heavy Armor, Shields)
- **Minimum Skill Level**: Required skill level to use effectively (default: 0)
- **Hit Locations Covered**: Which body parts this armor protects
- **Damage Class**: 1-4 (default: 1)
- **Damage Type Absorption**: SV reduction for each damage type (0-n):
  - Bashing damage absorbed
  - Cutting damage absorbed
  - Piercing damage absorbed
  - Projectile damage absorbed
  - Energy damage absorbed
  - Heat damage absorbed
  - Cold damage absorbed
  - Acid damage absorbed
- **DEX Penalty**: Reduction to Dodge skill AS (0-n)
- **STR Penalty**: Reduction to Physicality skill AS (0-n)
- **Durability**: Current/Maximum durability
- **Layer Priority**: Order in absorption sequence (outer garments before inner)

### Shield Properties

Shields follow the same property structure as armor with these distinctions:

- **Can defend any hit location** (not location-specific)
- **Costs 1 FAT per use** (unlike armor)
- Requires successful skill check to absorb damage
- May have special properties (buckler vs tower shield, etc.)

### Durability System

**Durability Loss**:

- **Weapons**: Lose durability equal to SV of damage dealt on successful attacks
- **Armor/Shields**: Lose durability equal to SV absorbed when defending

**Degraded Performance**:

- **At 50% max durability**: Item begins to degrade
- **At 25% max durability**:
  - Weapons: Deal only 50% of base SV bonus
  - Armor: Absorbs only 50% of rated absorption
- **At 0 durability**:
  - Item provides no inherent SV bonuses
  - Character's skill may still generate some effect
- **At -50% max durability**: Item is **destroyed** and cannot be repaired

**Durability Restoration**:

- Crafting skills can restore durability
- Magical repair spells/items
- Maximum durability may increase or decrease based on crafting success/failure
- Perfect repairs may increase max durability
- Failed repairs may decrease max durability

---

## Inventory and Carrying Capacity

### Weight and Volume System

- Each item has weight (pounds) and volume (cubic feet)
- Base carrying capacity uses exponential scaling: 50 lbs × (1.15 ^ (Physicality - 10))
- Base volume capacity uses exponential scaling: 10 cu.ft. × (1.15 ^ (Physicality - 10))
- Exponential formula reflects the bell-curve rarity of extreme attribute values from 4dF rolls
- Examples: PHY 6 = ~29 lbs, PHY 10 = 50 lbs, PHY 14 = ~87 lbs
- Exceeding weight limits reduces movement speed or prevents movement
- Exceeding volume limits prevents picking up new items

### Container System

- Containers are special items that can hold other items (bags, backpacks, quivers, chests, boxes)
- Each container has maximum weight and volume limits
- Some containers are restricted to specific item types
  - Quivers: only arrows and bolts
  - Spell component pouches: only spell components
  - General bags/backpacks: any item type
- Containers can be nested (bags within bags, with cumulative restrictions)
- Container weight = container's own weight + weight of all contained items
- **Magical containers** can reduce effective weight/volume via reduction multipliers
  - Example: Bag of Holding with 0.1 weight reduction makes items weigh only 10% of normal

### Equipment Slots

Characters can equip items in specific slots:

- **Body**: Head, Face, Ears, Neck, Shoulders, Back, Chest, Waist, Legs
- **Arms**: ArmLeft/Right, WristLeft/Right, HandLeft/Right
- **Weapons**: MainHand, OffHand, TwoHand
- **Jewelry**: Ears, Neck, FingerLeft1-5, FingerRight1-5 (10 ring slots total)
- **Feet**: AnkleLeft/Right, FootLeft/Right

---

## Magic and Spells

### Individual Spell Skills System

- Each spell is a separate skill that must be learned and practiced
- Spells organized into schools (Fire, Healing, Illusion, etc.)
- Spell effectiveness increases with individual spell skill level
- Area of effect, targeted, self-buff, and environmental spell types

### Spell Effect Types

- **Targeted Spells**: Affect specific targets (Fire Bolt, Heal, Lightning Strike)
- **Self-Buff Spells**: Enhance the caster's abilities (Strength, Invisibility, Shield)
- **Area Effect Spells**: Affect multiple targets in the area (Fireball, Mass Heal, Earthquake)
- **Environmental Spells**: Create persistent effects in an area
  - Wall of Fire (causes periodic damage)
  - Fog Cloud (reduces visibility)
  - Entangle (prevents movement)
  - Consecrate (provides healing bonuses)
  - Darkness (limits vision)
  - Silence (prevents spellcasting)

### Mana System

- Separate mana pools for each magic school
- Mana recovery rate determined by school-specific recovery skills
- Casting spells consumes mana from the appropriate school
- Mana regeneration continues during rest
- Environmental spells typically consume more mana due to their persistent nature

### Magic Items

- Enchanted weapons and armor
- Consumable magical items
- Artifacts with unique properties
- Items may provide bonuses to specific spell skills or mana recovery
- Some magical items can create environmental effects when activated

---

## Currency System

### Coin Denominations and Exchange Rates

| Currency | Symbol | Exchange Rate |
|----------|--------|---------------|
| Copper   | cp     | Base unit (1 cp) |
| Silver   | sp     | 20 cp = 1 sp |
| Gold     | gp     | 20 sp = 400 cp = 1 gp |
| Platinum | pp     | 20 gp = 8,000 cp = 1 pp |

### Currency Usage and Rarity

- **Copper**: Most common currency, used for everyday transactions (food, drink, basic supplies)
- **Silver**: Moderate-value currency, used for quality goods (weapons, armor, professional services)
- **Gold**: High-value currency, used for rare and exceptional items (enchanted equipment, rare materials)
- **Platinum**: Exceedingly rare currency (legendary items, unique treasures)

### Coin Weight

- 100 coins of any type = 1 pound
- Coin weight affects character carrying capacity

### Pricing Guidelines

| Category | Price Range |
|----------|-------------|
| Food and Drink | 1-50 cp |
| Basic Supplies | 5-100 cp |
| Common Weapons/Armor | 50 cp - 10 sp |
| Quality Weapons/Armor | 10 sp - 5 gp |
| Masterwork Equipment | 5-20 gp |
| Enchanted Items (minor) | 20-100 gp |
| Enchanted Items (major) | 100+ gp |

---

## Item Bonuses and Effects

Items can provide bonuses to characters through two mechanisms:

### Skill Bonuses

- Direct bonuses to specific skills
- **Flat Bonus**: Added directly to the skill's effective level
- **Percentage Bonus**: Multiplies the skill's effectiveness
- **Cooldown Reduction**: Reduces skill cooldown time

### Attribute Modifiers

- Bonuses to base attributes
- **Cascading Effects**: Attribute modifiers affect ALL skills that use that attribute
- Example: +3 Physicality item improves all STR-based skills by +3

### Stacking

- Multiple items with same skill bonus stack
- Multiple items with same attribute modifier stack
- Creates rich strategic choices for item selection and character builds
