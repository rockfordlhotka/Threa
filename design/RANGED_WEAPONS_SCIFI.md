# Sci-Fi Ranged Weapons System

## Overview

This document extends the ranged combat rules in [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md) with mechanics for modern and futuristic firearms, energy weapons, and heavy ordnance. These rules support cyberpunk, space opera, and military sci-fi settings.

**Related Documents**:
- [Combat System](COMBAT_SYSTEM.md) - Base ranged combat mechanics, range categories, reload actions
- [Effects System](EFFECTS_SYSTEM.md) - Status effects from special ammunition
- [Equipment System](EQUIPMENT_SYSTEM.md) - Weapon slots and bonuses

---

## Weapon Categories

Range uses the **Short Range** value from [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md). Categories are:
- **Short**: Range 1 to [Short Range] → +0 AV
- **Medium**: Short Range + 1 → -1 AV
- **Long**: Short Range + 2 → -2 AV
- **Extreme**: Short Range + 3 → -4 AV

### Personal Weapons (Damage Class 1)

| Category | Examples | Short Range | Capacity | Notes |
|----------|----------|-------------|----------|-------|
| **Pistol** | 9mm, .45, laser pistol | 4 | 8-18 | Concealable, one-handed |
| **SMG** | MP5, PDW, pulse SMG | 4 | 20-50 | Burst/auto capable |
| **Rifle** | AR-15, AK, pulse rifle | 6 | 20-40 | Standard infantry weapon |
| **Shotgun** | Pump, auto, plasma scatter | 3 | 4-12 | High close-range damage |
| **Sniper Rifle** | Bolt-action, gauss rifle | 8 | 5-10 | +2 AV when aiming |
| **Energy Pistol** | Laser, plasma | 5 | 20-30 shots | No bullet drop |
| **Energy Rifle** | Laser carbine, plasma rifle | 6 | 30-50 shots | No bullet drop |

### Heavy Weapons (Damage Class 2+)

| Category | DC | Examples | Short Range | Notes |
|----------|-----|----------|-------------|-------|
| **Anti-Materiel Rifle** | 2 | .50 cal, railgun | 9 | Penetrates light vehicles |
| **Grenade Launcher** | 2 | 40mm, plasma grenade | 5 | Area effect |
| **Rocket Launcher** | 2-3 | RPG, guided missile | 6 | Anti-vehicle |
| **Plasma Cannon** | 2 | Man-portable plasma | 6 | Energy-based heavy |
| **Minigun** | 2 | Gatling, rotary plasma | 5 | Suppression, high ammo use |
| **Vehicle Mounted** | 2-3 | Turret guns, cannons | 9 | Requires vehicle/mount |

---

## Fire Modes

Modern weapons can have multiple fire modes, selected before attacking.

### Single Fire

**Cost**: 1 AP + 1 FAT (or 2 AP)
**Ammo**: 1 round

Standard attack as per base ranged combat rules. Most accurate mode.

### Burst Fire

**Cost**: 1 AP + 1 FAT (or 2 AP)
**Ammo**: 3 rounds
**Modifier**: +1 SV on hit, -1 AS

Fires a short burst of 3 rounds. Higher damage potential but less accurate.

- If attack succeeds: +1 SV bonus (more rounds hit)
- If attack fails by 1-2: One round may graze (GM discretion, half damage)
- Cannot be used at Extreme range

### Automatic Fire

**Cost**: 2 AP + 2 FAT (or 4 AP)
**Ammo**: 10 rounds
**Modifier**: +2 SV on hit, -2 AS

Sustained fire for suppression or volume of fire.

- If attack succeeds: +2 SV bonus
- Area suppression: Can force targets in a small area to seek cover
- Cannot be used beyond Medium range
- May hit unintended targets on fumble (RV -5 or worse)

### Suppressive Fire

**Cost**: 2 AP + 2 FAT (or 4 AP)
**Ammo**: 15 rounds
**Effect**: Zone control, no direct damage

Creates a suppressed zone in a 3-meter radius area:

- Targets in zone must make WIL check (TV 8) to take actions
- Failed check: Target takes no action or moves to cover
- Lasts until start of suppressor's next turn
- Does not directly damage targets (unless they fail check and stay exposed)

---

## Ammunition System

### Loaded Ammo and Capacity

Each weapon has:
- **Capacity**: Maximum rounds the weapon holds when fully loaded
- **Loaded Ammo**: Current rounds in the weapon (0 = empty, must reload)

When Loaded Ammo reaches 0, the weapon cannot fire until reloaded.

### Reload is an Action

**Key Principle**: Reloading always requires an action. There are no "free" reloads.

| Weapon Type | Reload Action | Result |
|-------------|---------------|--------|
| **Magazine-fed** | Magazine Swap (1 AP + 1 FAT) | Loaded = Capacity |
| **Revolver/Cylinder** | Single Round (1 AP + 1 FAT each) | +1 per action |
| **Bow/Crossbow** | Nock Arrow/Load Bolt (1 AP + 1 FAT) | +1 per action |
| **Break-action** | Load Shells (1 AP + 1 FAT per 2) | +2 per action |
| **Belt-fed** | Belt Change (2 AP + 2 FAT) | Loaded = Capacity |
| **Energy Weapon** | Cell Swap (1 AP + 1 FAT) | Loaded = Capacity |

### Reload Action Details

| Reload Type | Cost | Description |
|-------------|------|-------------|
| **Magazine Swap** | 1 AP + 1 FAT | Replace empty/partial magazine with full one |
| **Speed Reload** | 2 AP | Fast swap, can fire same round (skill 6+ required) |
| **Tactical Reload** | 1 AP + 1 FAT | Swap partially-used magazine (keep partial mag) |
| **Single Round** | 1 AP + 1 FAT | Load one round (shotguns, revolvers, bows) |
| **Energy Cell Swap** | 1 AP + 1 FAT | Replace depleted power cell |

### Partial Reloads

For weapons without detachable magazines (shotguns, revolvers, some rifles):

- **Single Round Load**: 1 AP + 1 FAT per round
- Can fire between individual round loads
- Useful for topping off during lulls in combat

**Example**: Pump shotgun (8 capacity), currently has 3 rounds:
1. Fire (2 rounds remaining)
2. Load 1 round (1 AP + 1 FAT) -> 3 rounds
3. Fire (2 rounds remaining)
4. Load 2 rounds (2 AP + 2 FAT) -> 4 rounds

### Prepped Magazines

As per base ranged combat rules, magazines can be prepped in advance:

- **Prep Magazine**: 1 AP + 1 FAT (ready a magazine for instant swap)
- **Prepped Magazine Swap**: 0 additional cost (instant reload)

This allows very fast reloads in combat if magazines were prepped beforehand.

---

## Ammunition Types

### Ammunition AV Modifiers

Some ammunition types provide accuracy modifiers (AV) in addition to damage modifiers. **Ammunition AV is cumulative with weapon AV** - if a weapon has +1 AV and the ammo has +1 AV, the total modifier is +2 AV.

| Ammo Type | AV Modifier | Notes |
|-----------|-------------|-------|
| Standard | +0 | Default accuracy |
| Smart Rounds | +2 | Guided projectile |
| Tracer | +1* | *To subsequent shots only |
| Subsonic | -1 | Lower velocity affects accuracy |

### Standard Ammunition

Default ammunition type. No special effects or modifiers.

### Armor-Piercing (AP)

- **Effect**: -2 to armor's absorption rating
- **Trade-off**: -1 SV (smaller wound channel)
- **Cost**: 2× standard ammo cost
- **Use Case**: Armored targets, light vehicles

### Hollow-Point / Expanding

- **Effect**: +1 SV against unarmored targets
- **Trade-off**: -1 SV against armored targets (fragments on armor)
- **Cost**: 1.5× standard ammo cost
- **Use Case**: Soft targets, stopping power

### Explosive / High-Explosive (HE)

- **Effect**: +2 SV, small area effect (1m radius)
- **Trade-off**: Loud, may cause collateral damage
- **Cost**: 5× standard ammo cost
- **Use Case**: Groups of enemies, demolition
- **Available For**: Grenade launchers, some large-caliber weapons

### Incendiary

- **Effect**: On hit, target gains "Burning" effect (1 FAT/round for 3 rounds)
- **Trade-off**: -1 SV initial damage
- **Cost**: 3× standard ammo cost
- **Use Case**: Area denial, unarmored targets

### EMP / Disruption

- **Effect**: On hit, disables electronics for SV rounds
- **Trade-off**: No physical damage
- **Cost**: 10× standard ammo cost
- **Use Case**: Disabling implants, vehicles, drones
- **Note**: Electronics may require repair after extended disable

### Tracer

- **Effect**: +1 AS to subsequent shots at same target (this round)
- **Trade-off**: Reveals shooter position
- **Cost**: 1.5× standard ammo cost
- **Use Case**: Low-light conditions, target designation

### Subsonic / Suppressed

- **Effect**: Reduces weapon noise significantly
- **Trade-off**: -1 SV (lower velocity)
- **Cost**: 2× standard ammo cost
- **Use Case**: Stealth operations
- **Requires**: Suppressor attachment

### Smart Rounds

- **Effect**: +2 AS (guided projectile), ignores cover penalties
- **Trade-off**: Costs 20× standard ammo
- **Use Case**: High-value targets, extreme range
- **Requires**: Smart weapon system, target lock

---

## Energy Weapons

Energy weapons use power cells instead of conventional ammunition.

### Power Cells

| Cell Type | Capacity | Recharge Time | Notes |
|-----------|----------|---------------|-------|
| **Standard Cell** | 20-30 shots | 4 hours (charger) | Common, inexpensive |
| **High-Capacity** | 50-75 shots | 8 hours (charger) | Heavier, more expensive |
| **Micro Cell** | 10-15 shots | 2 hours (charger) | Compact weapons, holdouts |
| **Military Grade** | 40-60 shots | Field swappable | Standardized, expensive |

### Energy Weapon Properties

- **No Bullet Drop**: No range penalty for gravity (flat trajectory)
- **Visible Beam/Bolt**: Reveals shooter position
- **Weather Sensitive**: Rain/fog may reduce effectiveness (-1 AS)
- **EMP Vulnerable**: Disabled by EMP effects until repaired

### Power Modes

Some energy weapons can adjust power output:

| Mode | Effect | Shots per Cell |
|------|--------|----------------|
| **Low Power** | -2 SV | 2× capacity |
| **Standard** | Normal | 1× capacity |
| **Overcharge** | +2 SV, +1 cooldown round | 0.5× capacity |

---

## Heavy Weapons

### Damage Class 2 Weapons

These weapons are designed to damage vehicles, power armor, and hardened targets.

| Weapon | Base SV Bonus | Ammo | Special |
|--------|---------------|------|---------|
| Anti-Materiel Rifle | +4 | 5-round mag | Penetrates DC1 armor completely |
| Railgun | +5 | 3-round mag, power cell | Pierces most cover |
| Plasma Cannon | +4 | Power cell (10 shots) | Burn effect on hit |
| Rocket Launcher | +6 | 1 rocket | Area effect 3m radius |
| Grenade Launcher | +3 | 6-round cylinder | Various grenade types |

### Using Heavy Weapons

- **Encumbrance**: Most require STR 12+ or mount/bipod
- **Setup Time**: Some require 1 round to deploy (bipod, brace)
- **Minimum Range**: Rockets/grenades have minimum arming distance (Range 2+)
- **Backblast**: Rockets create danger zone behind shooter

### Anti-Vehicle Ammunition

For heavy weapons targeting vehicles:

| Ammo Type | Effect |
|-----------|--------|
| **HEAT** (High-Explosive Anti-Tank) | +2 SV vs vehicles, -2 vs infantry |
| **APFSDS** (Armor-Piercing Fin-Stabilized) | Ignores first 5 points of armor |
| **Thermobaric** | +4 SV in enclosed spaces, area effect |
| **Guided** | +3 AS, requires target lock |

---

## Weapon Attachments

Attachments modify weapon performance.

| Attachment | Effect | Slot |
|------------|--------|------|
| **Scope (2×-4×)** | +1 AS at Medium+, -1 AS at Short | Optic |
| **Scope (6×-12×)** | +2 AS at Long+, -2 AS at Short/Medium | Optic |
| **Red Dot** | +1 AS at Short/Medium | Optic |
| **Suppressor** | Reduces noise, enables subsonic ammo | Muzzle |
| **Extended Magazine** | +50% capacity | Magazine |
| **Bipod** | +2 AS when prone/braced, setup required | Underbarrel |
| **Laser Sight** | +1 AS at Short range | Underbarrel |
| **Foregrip** | Reduces auto fire penalty by 1 | Underbarrel |
| **Flashlight** | Illumination, can blind at Short range | Underbarrel |

---

## Example Weapons

### Ares Predator (Heavy Pistol)

```
Type: Pistol
Damage Class: 1
Short Range: 4 (Extreme at 7 = 49m)
Capacity: 15 rounds
Fire Modes: Single, Burst
Base Damage: +1 SV
Attachments: 2 slots (Optic, Underbarrel)
Special: Iconic, reliable
```

### Tsunami Arms Pulse Rifle

```
Type: Energy Rifle
Damage Class: 1
Short Range: 6 (Extreme at 9 = 81m)
Capacity: 40 shots (standard cell)
Fire Modes: Single, Burst, Overcharge
Base Damage: +0 SV
Attachments: 3 slots
Special: No bullet drop, EMP vulnerable
Power Modes: Low/Standard/Overcharge
```

### Militech M-31 Anti-Materiel Rifle

```
Type: Anti-Materiel Rifle
Damage Class: 2
Short Range: 9 (Extreme at 12 = 144m)
Capacity: 5 rounds
Fire Modes: Single only
Base Damage: +4 SV
Attachments: 2 slots (Optic, Bipod recommended)
Special: Requires STR 12+ or bipod
         +2 AV when aiming
         Penetrates DC1 armor completely
```

### Arasaka Smart-Link SMG

```
Type: SMG
Damage Class: 1
Short Range: 4 (Extreme at 7 = 49m)
Capacity: 30 rounds
Fire Modes: Single, Burst, Auto
Base Damage: +0 SV
Attachments: 2 slots (has built-in smart-link)
Special: Smart rounds compatible
         +1 AS with smart ammo
         Neural interface integration
```

---

## Combat Flow Example

**Situation**: Street samurai with pulse rifle (40-shot cell, currently 28 shots) engaging two gang members behind a car at Range 5 (Medium).

**Round 1**:
1. Select fire mode: Burst (3 shots)
2. Roll: Firearms AS 14 - 1 (burst penalty) + 4dF+ = 15 AV
3. TV: Base 8 (Medium) + 1 (half cover) = TV 9
4. SV: 15 - 9 = 6, +1 (burst) = **SV 7**
5. Ammo: 28 - 3 = 25 shots remaining
6. Damage resolution per COMBAT_SYSTEM.md

**Round 2** (second target, suppressed behind car):
1. Suppressive Fire: 2 AP + 2 FAT, 15 shots
2. Creates suppression zone around car
3. Target must make WIL check TV 8 to act
4. Ammo: 25 - 15 = 10 shots remaining

**Round 3** (low ammo, tactical decision):
1. Tactical Reload: 1 AP + 1 FAT, swap to fresh cell (keep 10-shot cell)
2. Now at 40 shots
3. Fire single shot at remaining target

---

## Integration with Base System

This document supplements [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md). Use the base document for:
- Range categories and base TV
- Target Value modifiers (movement, cover, size)
- Defense options (dodge, passive defense)
- Damage resolution (hit location, armor, damage classes)
- Cooldowns and prep actions (still apply)

Weapon skill progression (cooldown reduction) from COMBAT_SYSTEM.md applies to all weapon types here.

---

## Related Documents

- [Combat System](COMBAT_SYSTEM.md) - Base mechanics
- [Effects System](EFFECTS_SYSTEM.md) - Burning, EMP disable effects
- [Implants System](IMPLANTS_SYSTEM.md) - Smart-link neural interfaces
- [Equipment System](EQUIPMENT_SYSTEM.md) - Weapon slots, attachments
