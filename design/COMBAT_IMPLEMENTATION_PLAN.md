# Combat System Implementation Plan

## Overview

This document outlines the phased approach to implementing the combat system defined in [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md). The implementation leverages existing systems already in place.

---

## Foundation Already Implemented

The following systems provide the foundation for combat:

| System | Location | Relevance to Combat |
|--------|----------|---------------------|
| **Actions System** | `GameMechanics/Actions/` | Universal 9-step resolution, AS calculation, boosts, modifiers, result tables |
| **Time System** | `GameMechanics/Time/` | Rounds, initiative (by AP), cooldowns, end-of-round processing |
| **Action Points** | `GameMechanics/ActionPoints.cs` | Action costs (1 AP + 1 FAT or 2 AP), recovery, rest |
| **Wounds** | `GameMechanics/WoundList.cs` | Location-based wounds, FAT damage over time, limb tracking |
| **Effects System** | `GameMechanics/Effects/` | Status effects, stacking, duration, modifiers |
| **Dice Mechanics** | `GameMechanics/Dice.cs` | 4dF+ exploding dice |
| **Movement/Range** | `GameMechanics/Actions/Movement.cs` | Range value system (Range² = meters) |

---

## Implementation Phases

### Phase 1: Attack Resolution (Foundation) ✅ COMPLETE

**Goal**: Enable basic melee attack resolution using the existing Actions framework.

**Implemented Files**:
- `GameMechanics/Combat/CombatResultTables.cs` - Physicality bonus and damage lookup tables
- `GameMechanics/Combat/HitLocation.cs` - Hit location enum and calculator (optimized single roll)
- `GameMechanics/Combat/AttackRequest.cs` - Input for attack resolution
- `GameMechanics/Combat/AttackResult.cs` - Output from attack resolution
- `GameMechanics/Combat/AttackResolver.cs` - Main service for attack resolution

**Implemented Features**:
- ✅ Melee attack roll: Weapon Skill AS + 4dF+ = AV
- ✅ Passive defense: Dodge AS - 1 = TV
- ✅ Hit location determination via optimized d24-equivalent roll
- ✅ Physicality bonus roll for melee attacks (automatic, free)
- ✅ Multiple action penalty (-1 AS after first action, not cumulative)
- ✅ Boost mechanic (AP and FAT boosts stack)
- ✅ Damage lookup table (SV → FAT/VIT damage, wound threshold)
- ✅ DI-based design with IDiceRoller for testability

**Tests**: 55 tests in `CombatTests.cs`

---

### Phase 2: Defense Resolution ✅ COMPLETE

**Goal**: Implement active and passive defense options.

**Implemented Files**:
- `GameMechanics/Combat/DefenseType.cs` - Enum: Passive, Dodge, Parry, ShieldBlock
- `GameMechanics/Combat/DefenseRequest.cs` - Input for defense resolution
- `GameMechanics/Combat/DefenseResult.cs` - Output from defense resolution
- `GameMechanics/Combat/DefenseResolver.cs` - Defense resolution service
- `GameMechanics/Combat/CombatState.cs` - Per-character combat state + CombatStateManager

**Implemented Features**:
- ✅ **Passive Defense**: Dodge AS - 1 (no roll, no cost)
- ✅ **Active Dodge**: Dodge AS + 4dF+ (costs action)
- ✅ **Active Parry**: Weapon/Shield AS + 4dF+ (costs action to enter mode)
- ✅ **Parry Mode**: Free defenses until broken by non-parry action
- ✅ **Shield Block**: Shield AS + 4dF+ vs TV 8 (free action, like Physicality bonus)
- ✅ Parry cannot be used against ranged attacks
- ✅ Combat state tracking per character (actions, parry mode)
- ✅ CombatStateManager for multi-combatant encounters
- ✅ Round tracking with parry mode persistence

**Integration with AttackResolver**:
- `DefenseResolver.Resolve()` returns `DefenseResult` with calculated TV
- `AttackResolver.ResolveWithTV()` accepts explicit TV from defense
- `ResolveWithShield()` combines primary defense with optional shield block

**Tests**: 29 additional tests (84 total combat tests)

---

### Phase 3: Damage Resolution ✅ COMPLETE

**Goal**: Implement the damage absorption sequence and apply wounds.

**Implemented Files**:
- `GameMechanics/Combat/DamageType.cs` - Enum: Bashing, Cutting, Piercing, Projectile, Energy
- `GameMechanics/Combat/ArmorInfo.cs` - Armor properties for damage resolution
- `GameMechanics/Combat/ShieldInfo.cs` - Shield properties (in ArmorInfo.cs)
- `GameMechanics/Combat/AbsorptionRecord.cs` - Record of each absorption step
- `GameMechanics/Combat/DamageRequest.cs` - Input for damage resolution
- `GameMechanics/Combat/DamageResolutionResult.cs` - Output with full damage breakdown
- `GameMechanics/Combat/DamageResolver.cs` - Main damage resolution service

**Implemented Features**:
- ✅ **Defense Sequence**: Shield → Armor (outer to inner) → Character
- ✅ **Armor Skill Check**: Free action, RV modifies absorption (like Physicality)
- ✅ Absorption by damage type (5 types: Bashing, Cutting, Piercing, Projectile, Energy)
- ✅ Armor layers ordered by LayerOrder (outer absorbs first)
- ✅ Armor skill bonus only applies to first (outermost) layer
- ✅ Durability reduction 1:1 with absorbed damage
- ✅ Equipment destruction when durability hits 0
- ✅ Damage class interactions (higher class absorbs lower class at 10× effectiveness)
- ✅ Absorption limited by remaining durability
- ✅ Shield block RV provides bonus absorption
- ✅ Location-based armor (only armor covering hit location applies)
- ✅ SV-to-damage conversion via existing damage tables
- ✅ Factory method to create DamageRequest from AttackResult

**Tests**: 24 additional tests (108 total combat tests)

---

### Phase 4: Ranged Combat ✅ COMPLETE

**Goal**: Extend combat system to support ranged attacks.

**Implemented Files**:
- `GameMechanics/Combat/RangeModifiers.cs` - Range category enum, TV modifiers, cover, target size
- `GameMechanics/Combat/WeaponRanges.cs` - Weapon range capabilities with presets
- `GameMechanics/Combat/RangedAttackRequest.cs` - Input for ranged attack resolution
- `GameMechanics/Combat/RangedAttackResult.cs` - Output from ranged attack resolution
- `GameMechanics/Combat/RangedAttackResolver.cs` - Ranged-specific resolution service
- `GameMechanics/Combat/AimState.cs` - Aim bonus tracking per character/target
- `GameMechanics/Combat/PrepState.cs` - Prep action handling and ranged cooldowns

**Implemented Features**:
- ✅ Range categories (Short=TV6, Medium=TV8, Long=TV10, Extreme=TV12)
- ✅ Out-of-range detection with clean failure result
- ✅ TV modifiers: target movement (+2), prone (+2), crouching (+2), cover (+1/+2), attacker motion (+2), target size (+1/+2)
- ✅ Aim action: +2 AS bonus if first action next round on same target
- ✅ Prep actions for ammunition (stack multiple, no cooldown)
- ✅ Ranged weapon cooldowns by skill level (0→6s to 10+→0s)
- ✅ Thrown weapons use Physicality bonus (same as melee)
- ✅ Cooldown interruption behavior: Pausable (bow/crossbow) vs Resettable (thrown)
- ✅ Multiple action penalty applied correctly
- ✅ Boost mechanics integrated

**Integration Points**:
- Uses existing `CooldownTracker` for weapon cooldowns
- Uses existing `Movement.RangeToMeters()` for distance calculation
- Compatible with `DamageResolver` for damage application
- Uses existing hit location and damage table systems

**Tests**: 79 tests in `RangedCombatTests.cs`

---

### Phase 5: Special Combat Actions ✅ COMPLETE

**Goal**: Implement tactical options beyond basic attacks.

**Implemented Files**:
- `GameMechanics/Combat/SpecialActions/SpecialActionType.cs` - Enum for action types + SpecialActionModifiers
- `GameMechanics/Combat/SpecialActions/KnockbackTable.cs` - RVE lookup table (SV → duration seconds)
- `GameMechanics/Combat/SpecialActions/SpecialActionRequest.cs` - Input for special action resolution
- `GameMechanics/Combat/SpecialActions/SpecialActionResult.cs` - Output with effect instructions
- `GameMechanics/Combat/SpecialActions/SpecialActionResolver.cs` - Main resolver with EffectManager integration

**Implemented Features**:
- ✅ **Knockback**: No AS penalty, prevents target from acting for RVE seconds (lookup table), critical knocks prone
- ✅ **Disarm**: -2 AS penalty, target drops weapon/item, critical breaks item
- ✅ **Called Shot**: -2 AS penalty, choose hit location, marginal success (SV 0-1) hits random location
- ✅ **Stunning Blow**: -2 AS penalty, applies Stunned effect for SV seconds (FAT=0), critical causes unconsciousness

**All special actions**:
- Use standard action cost (1 AP + 1 FAT or 2 AP)
- Apply appropriate AS penalties (defined per action type)
- Use same resolution framework as normal attacks (passive/active defense)
- Create effects via `EffectManager` for Stunned, Prone, Unconscious

**Integration Points**:
- `EffectManager.ApplyEffectAsync()` for status effects
- Uses existing "Stunned", "Prone", "Unconscious" effect definitions
- Fully testable with constructor that omits EffectManager

**Tests**: 46 tests in `SpecialActionTests.cs`
- Knockback duration table mapping
- AS penalty verification per action type
- Critical hit thresholds (Knockback: 8, Disarm: 8, Stun: 10)
- Called shot location mechanics (intended vs random)
- Effect application integration
- Active defense compatibility

**Deliverable**: Complete tactical combat system.

---

## Phase Dependencies

```
Phase 1: Attack Resolution
    ↓
Phase 2: Defense Resolution
    ↓
Phase 3: Damage Resolution
    ↓
Phase 4: Ranged Combat  (can be done in parallel with Phase 5)
    ↓
Phase 5: Special Actions (can be done in parallel with Phase 4)
```

---

## Estimated Scope

| Phase | New Files | Estimated Tests | Complexity | Status |
|-------|-----------|-----------------|------------|--------|
| Phase 1 | 5 | 55 | Medium | ✅ Complete |
| Phase 2 | 5 | 29 | Medium | ✅ Complete |
| Phase 3 | 6 | 24 | High | ✅ Complete |
| Phase 4 | 4 | 79 | Medium | ✅ Complete |
| Phase 5 | 5 | 46 | Medium | ✅ Complete |
| **Total** | **25** | **233** | | **5/5 Complete** |

---

## Success Criteria

Each phase is complete when:

1. All specified features are implemented
2. Unit tests pass with >90% coverage of new code
3. Integration with existing systems verified
4. No regressions in existing tests
5. Code reviewed and documented

---

## Related Documents

- [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md) - Full combat design specification
- [ACTIONS.md](ACTIONS.md) - Universal action resolution framework
- [ACTION_POINTS.md](ACTION_POINTS.md) - AP costs and recovery
- [TIME_SYSTEM.md](TIME_SYSTEM.md) - Rounds, initiative, cooldowns
- [GAME_RULES_SPECIFICATION.md](GAME_RULES_SPECIFICATION.md) - Dice mechanics, damage tables
