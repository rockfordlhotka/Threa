# Unarmed Combat System

## Overview

The unarmed combat system allows characters to fight without weapons, using natural attacks like punches and kicks. When a character has no weapon equipped in their hands, they can still engage in melee combat using these innate abilities.

**Related Documents**:
- [Combat System](COMBAT_SYSTEM.md) - Core combat mechanics
- [Equipment System](EQUIPMENT_SYSTEM.md) - Equipment slots and bonuses
- [Actions](ACTIONS.md) - Universal action resolution framework
- [Effects System](EFFECTS_SYSTEM.md) - Status effects from combat

---

## Design Rationale

### Problem Statement

When a character is disarmed or chooses not to use a weapon, they need a way to participate in melee combat. Weapons provide modifiers (SVModifier, AVModifier) that affect combat resolution. Without a weapon, the system needs default values for these modifiers.

### Solution Approach

Unarmed attacks are implemented as **virtual weapon templates** - predefined attack types with specific modifiers that are automatically available when a character's hands are empty. This approach:

1. Leverages the existing weapon/item infrastructure
2. Allows different attack types (punch, kick) with distinct trade-offs
3. Enables future expansion (martial arts skills, special techniques)
4. Maintains consistency with the combat resolver flow

---

## Unarmed Attack Types

### Punch

A quick strike with the fist. Favors speed over power.

| Property | Value |
|----------|-------|
| **SV Modifier** | +2 |
| **AS Modifier** | 0 |
| **Damage Type** | Bludgeoning |
| **Damage Class** | DC1 |
| **AP Cost** | 1 AP + 1 FAT (standard) |
| **Related Skill** | Unarmed Combat |
| **Equipment Slot** | MainHand or OffHand |

**Use Case**: Default unarmed attack. Good when accuracy matters more than damage. Useful for multiple attacks per round due to no AS penalty.

### Kick

A powerful strike with the leg. Trades accuracy for damage.

| Property | Value |
|----------|-------|
| **SV Modifier** | +4 |
| **AS Modifier** | -1 |
| **Damage Type** | Bludgeoning |
| **Damage Class** | DC1 |
| **AP Cost** | 1 AP + 1 FAT (standard) |
| **Related Skill** | Unarmed Combat |
| **Equipment Slot** | N/A (uses legs) |

**Use Case**: When landing a solid hit, kicks deal more damage. The -1 AS penalty makes them slightly harder to land but more rewarding on success.

---

## Unarmed Combat Skill

Unarmed Combat is a **Physicality (STR)**-based skill that governs all unarmed attacks.

### Ability Score Calculation

```
Unarmed Combat AS = Physicality + Unarmed Combat Skill Level - 5 + Modifiers
```

**Example**:
- Character with Physicality 12, Unarmed Combat skill 4
- Base AS = 12 + 4 - 5 = **11**
- Punch attack: AS 11, SV +2 on hit
- Kick attack: AS 11 - 1 = **10**, SV +4 on hit

### Untrained Unarmed Combat

Characters without the Unarmed Combat skill can still punch and kick, but with significant penalties:

```
Untrained AS = Physicality + 0 - 5 = Physicality - 5
```

**Example**:
- Character with Physicality 10, no Unarmed Combat skill
- Base AS = 10 + 0 - 5 = **5**
- This represents flailing rather than trained fighting

---

## Combat Resolution

### Attack Flow

Unarmed attacks follow the standard melee attack flow:

1. **Declare Attack**: Character chooses Punch or Kick
2. **Calculate AS**: Apply attack's AS modifier (0 for punch, -1 for kick)
3. **Roll Attack**: AS + 4dF+ = AV
4. **Defender Response**: Active or passive defense
5. **Calculate SV**: AV - TV
6. **Apply SV Modifier**: Add attack's SV modifier (+2 punch, +4 kick)
7. **Physicality Bonus**: Roll for additional SV (standard melee rule)
8. **Damage Resolution**: Look up final SV on damage table

### Example: Punch Attack

```
Aldric (Unarmed Combat AS 11) punches a Goblin (Dodge AS 9)

1. Attack roll: 11 + 4dF+ (+1) = 12 AV
2. Goblin passive defense: TV = 9 - 1 = 8
3. Base SV = 12 - 8 = 4
4. Punch SV modifier: +2
5. Effective SV before Physicality = 6
6. Physicality check (AS 10): 10 + 4dF+ (+2) = 12 vs TV 8
   RV = 12 - 8 = +4 → +2 SV bonus
7. Final SV = 6 + 2 = 8
8. Damage lookup: SV 8 → damage to Goblin
```

### Example: Kick Attack

```
Same scenario, Aldric kicks instead

1. Attack AS: 11 - 1 = 10 (kick penalty)
2. Attack roll: 10 + 4dF+ (+1) = 11 AV
3. Goblin passive defense: TV = 8
4. Base SV = 11 - 8 = 3
5. Kick SV modifier: +4
6. Effective SV before Physicality = 7
7. Final SV (after Physicality) = 7 + 2 = 9
8. Damage lookup: SV 9 → more damage than punch!
```

---

## Defensive Options While Unarmed

### Dodge (Recommended)

Works normally - no penalty for being unarmed.

### Parry with Fists

Characters can parry melee attacks while unarmed, but at a penalty:

| Defense Type | AS Modifier | Notes |
|--------------|-------------|-------|
| Unarmed Parry | -2 | Catching blades is dangerous |

**Mechanics**:
- Enter parry mode as normal (1 AP + 1 FAT)
- Use Unarmed Combat AS - 2 for parry TV
- Only works against melee attacks
- Higher risk of injury on failed parry (see Parry Failure below)

### Parry Failure Consequences

When an unarmed parry fails by 3 or more SV:

- The defender's arm takes a wound in addition to normal damage
- Represents the danger of blocking a weapon with bare hands

---

## Dual Unarmed Attacks

Characters can perform a flurry of unarmed strikes:

**Cost**: 2 AP + 2 FAT (or 4 AP)

| Attack | AS Modifier |
|--------|-------------|
| Primary hand | Normal |
| Off-hand | -2 |

Both punches resolve simultaneously. Kicks cannot be dual-wielded (only two hands, not four legs).

**Example - Punch Flurry**:
- Primary punch: Unarmed AS 11
- Off-hand punch: Unarmed AS 11 - 2 = 9
- Both get +2 SV if they hit

---

## Equipment Interaction

### Weapon Equipped

If a character has a weapon equipped in MainHand:
- **Punch**: Not available with that hand (weapon in the way)
- **Kick**: Always available (uses legs)

If both hands are equipped:
- Only kicks are available for unarmed attacks
- Or the character can drop/sheath weapons to punch

### Unarmed-Enhancing Items

Items can provide bonuses to unarmed combat:

| Item Example | Bonus Type | Effect |
|--------------|------------|--------|
| Brass Knuckles | +1 SV | Adds to punch SV modifier |
| Combat Gloves | +1 AS | Adds to Unarmed Combat AS |
| Steel-Toed Boots | +1 SV | Adds to kick SV modifier |
| Martial Arts Manual | +2 Skill | Bonus to Unarmed Combat skill |

These items stack with base unarmed modifiers.

---

## Implementation Plan

### Phase 1: Core System

1. **Add Unarmed Combat Skill**
   - Add to skill list with Physicality as governing attribute
   - Default skill level 0 for all characters

2. **Create Virtual Weapon Templates**
   - `Punch` template with SV +2, AS +0
   - `Kick` template with SV +4, AS -1
   - Both use `WeaponType.Unarmed`
   - Both use `DamageType = "Bludgeoning"`

3. **Update Combat Resolution**
   - When no weapon equipped, present Punch/Kick options
   - Apply unarmed modifiers in attack resolution
   - Handle SV modifier application for melee (currently only ranged uses it)

4. **Update TargetingInteractionManager**
   - Detect unarmed state
   - Populate attacker data with unarmed weapon stats
   - Support attack type selection (punch vs kick)

### Phase 2: UI Integration

5. **Combat UI Updates**
   - Show "Punch" and "Kick" as attack options when unarmed
   - Display modifiers in attack preview
   - Narration support for unarmed attacks

6. **Character Sheet Updates**
   - Display Unarmed Combat skill
   - Show equipped state affecting available attacks

### Phase 3: Testing

7. **Unit Tests**
   - Punch attack resolution with SV modifier
   - Kick attack resolution with AS and SV modifiers
   - Unarmed vs armed damage comparisons
   - Untrained unarmed combat
   - Dual unarmed attacks

8. **Integration Tests**
   - Full combat flow while unarmed
   - Weapon equip/unequip state transitions
   - Unarmed parry with failure consequences

---

## Future Enhancements

These are out of scope for initial implementation but should be considered in the design:

### Martial Arts Specializations

Different fighting styles could provide unique techniques:

| Style | Unlock Skill | Special Ability |
|-------|--------------|-----------------|
| Boxing | Unarmed 4 | Combo attacks, faster recovery |
| Kickboxing | Unarmed 4 | Reduced kick AS penalty |
| Grappling | Unarmed 6 | Grab, throw, submission moves |
| Pressure Points | Unarmed 8 | Stun effects on critical hits |

### Additional Unarmed Attacks

| Attack | SV Mod | AS Mod | Notes |
|--------|--------|--------|-------|
| Headbutt | +3 | -2 | Risk of self-damage on miss |
| Elbow Strike | +2 | 0 | Close quarters only |
| Knee Strike | +3 | -1 | Requires grab or close range |
| Backhand | +1 | +1 | Fast but light damage |

### Grappling System

A complete grappling subsystem with:
- Grab attempts
- Hold/restrain actions
- Throws and takedowns
- Ground fighting
- Submission holds

---

## Summary

| Attack | AS Mod | SV Mod | Best For |
|--------|--------|--------|----------|
| **Punch** | +0 | +2 | Accuracy, multiple attacks |
| **Kick** | -1 | +4 | Damage when you can afford the AS penalty |

Unarmed combat provides a viable fallback when disarmed and a foundation for martial arts characters. The trade-off between punch (accurate) and kick (powerful) gives players meaningful tactical choices.
