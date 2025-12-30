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

### Phase 3: Damage Resolution

**Goal**: Implement the damage absorption sequence and apply wounds.

**New Files**:
- `GameMechanics/Combat/DamageResolver.cs` - Damage resolution service
- `GameMechanics/Combat/DamageResult.cs` - Detailed damage breakdown
- `GameMechanics/Combat/ArmorAbsorption.cs` - Armor absorption calculation

**Key Features**:
- **Defense Sequence**: Shield → Armor → Character
- Armor absorption by hit location and damage type
- Durability reduction for shields/armor
- SV-to-damage conversion (using existing damage tables)
- Apply damage to Vitality/Fatigue
- Create wounds via existing `WoundList`

**Integration Points**:
- Uses `ItemTemplate` armor properties from DAL
- Integrates with `CharacterItem` for equipped armor
- Connects to existing `Vitality` and `Fatigue` systems
- Wire wound penalties into combat (already -2 AV per wound in design)

**Tests**:
- Shield absorption scenarios
- Armor absorption by location
- Penetrating damage after absorption
- Wound creation from successful hits
- Durability degradation

**Deliverable**: Complete melee combat with damage, armor, and wounds.

---

### Phase 4: Ranged Combat

**Goal**: Extend combat system to support ranged attacks.

**New/Modified Files**:
- `GameMechanics/Combat/RangedAttackResolver.cs` - Ranged-specific resolution
- `GameMechanics/Combat/RangeModifiers.cs` - TV modifiers by range/conditions
- `GameMechanics/Combat/AimAction.cs` - Aim bonus tracking
- `GameMechanics/Combat/PrepAction.cs` - Prep action handling

**Key Features**:
- Range categories (Short/Medium/Long/Extreme) with base TV
- TV modifiers: target movement, cover, attacker movement, target size
- Aim action: +2 AS bonus for next attack on same target
- Prep actions for ammunition (no cooldown, enables multi-shot)
- Ranged weapon cooldowns by skill level (integrate with `CooldownTracker`)
- Thrown weapons (use melee skill, ranged TV modifiers)
- Cooldown interruption (Pausable vs Resettable)

**Integration Points**:
- Uses `Movement.RangeToMeters()` for distance calculation
- Integrates with `CooldownTracker` for weapon cooldowns
- Uses existing `ActionCost` patterns

**Tests**:
- Range category determination
- TV modifier stacking
- Aim bonus application and loss conditions
- Prep action + multiple shots per round
- Cooldown progression and interruption

**Deliverable**: Full ranged combat including bows, crossbows, thrown weapons.

---

### Phase 5: Special Combat Actions

**Goal**: Implement tactical options beyond basic attacks.

**New Files**:
- `GameMechanics/Combat/SpecialActions/KnockbackAction.cs`
- `GameMechanics/Combat/SpecialActions/DisarmAction.cs`
- `GameMechanics/Combat/SpecialActions/CalledShotAction.cs`
- `GameMechanics/Combat/SpecialActions/StunAction.cs`

**Key Features**:
- **Knockback**: Stun instead of damage (SV seconds)
- **Disarm**: -2 AS penalty, target drops weapon
- **Called Shot**: -2 AS penalty, choose hit location
- **Stun/Knockout**: -2 AS penalty, target incapacitated for SV seconds

**All special actions**:
- Use standard action cost (1 AP + 1 FAT or 2 AP)
- Apply -2 AS penalty (except Knockback)
- Use same resolution framework as normal attacks
- Create appropriate effects via `EffectManager`

**Integration Points**:
- Uses `EffectManager` for status effects (Stunned, etc.)
- Uses existing effect definitions in MockDb

**Tests**:
- Each special action type
- AS penalty application
- Effect creation and duration
- Critical hit handling

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
| Phase 3 | 3 | 20-30 | High | Not Started |
| Phase 4 | 4 | 25-35 | Medium | Not Started |
| Phase 5 | 4 | 15-20 | Low | Not Started |
| **Total** | **21** | **144-169** | | **2/5 Complete** |

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
