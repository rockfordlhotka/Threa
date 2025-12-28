# Implementation Gap Analysis

## Overview

This document compares the design specifications in the `/design` folder against the actual implementation in `/GameMechanics`. It identifies discrepancies and provides an action list to bring the code into alignment with the design.

---

## Summary of Current Implementation Status

| System | Design Status | Implementation Status | Gap Level |
|--------|--------------|----------------------|-----------|
| Dice Mechanics | ✅ Complete | ✅ Implemented | Minor |
| Attributes | ✅ Complete | ⚠️ Partial | Medium |
| Health (Fatigue/Vitality) | ✅ Complete | ⚠️ Partial | Medium |
| Skills | ✅ Complete | ⚠️ Partial | High |
| Wounds | ✅ Complete | ✅ Implemented | Minor |
| Combat System | ✅ Complete | ❌ Not Implemented | High |
| Equipment/Items | ✅ Complete | ❌ Not Implemented | High |
| Inventory/Carrying Capacity | ✅ Complete | ❌ Not Implemented | High |
| Currency | ✅ Complete | ❌ Not Implemented | High |
| Magic/Mana | ✅ Complete | ❌ Not Implemented | High |
| Species Modifiers | ✅ Complete | ❌ Not Implemented | Medium |

---

## Detailed Gap Analysis

### 1. Dice Mechanics

**Design Spec** ([GAME_RULES_SPECIFICATION.md](GAME_RULES_SPECIFICATION.md)):
- 4dF (Fudge dice) system: range -4 to +4
- 4dF+ (Exploding dice): on +4 or -4, roll again counting only +/- results

**Implementation** ([Dice.cs](../GameMechanics/Dice.cs)):
- ✅ `Roll(count, "F")` - Basic Fudge dice implemented
- ✅ `Roll4dFWithBonus()` - Exploding dice implemented correctly

**Action Items**: None - implementation matches design.

---

### 2. Attribute System

**Design Spec**:
- 7 core attributes: Physicality (STR), Dodge (DEX), Drive (END), Reasoning (INT), Awareness (ITT), Focus (WIL), Bearing (PHY)
- Human baseline: 4dF + 10
- Species modifiers (Elf, Dwarf, Halfling, Orc) with attribute adjustments

**Implementation** ([AttributeEditList.cs](../GameMechanics/AttributeEditList.cs)):
- ⚠️ Creates 8 attributes (adds "SOC" not in design)
- ✅ Uses 4dF + 10 for attribute generation
- ❌ No species modifiers implemented
- ⚠️ Attribute names inconsistent with design (uses abbreviations STR, DEX, etc.)

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| High | Remove "SOC" attribute or clarify design if intentional | `AttributeEditList.cs` |
| High | Implement species modifiers (Human, Elf, Dwarf, Halfling, Orc) | `CharacterEdit.cs`, `AttributeEdit.cs` |
| Medium | Add Species property validation and modifier application | `CharacterEdit.cs` |
| Low | Create species enum or reference data | New file needed |

---

### 3. Health System (Fatigue & Vitality)

**Design Spec**:
- **Fatigue**: `(END × 2) - 5` with 3-second recovery cycle
- **Vitality**: `(STR + END) - 5` with hourly recovery
- Low FAT effects at 3, 2, 1, 0 with Focus checks at TV 5, 7, 12
- Low VIT effects at 4, 3, 2, 1, 0 with recovery slowdowns

**Implementation**:

**Fatigue** ([Fatigue.cs](../GameMechanics/Fatigue.cs)):
- ❌ Uses `END + WIL - 5` instead of `(END × 2) - 5`
- ⚠️ Focus check thresholds differ: checks at FAT < 6 (TV 5), < 4 (TV 7), < 2 (TV 12)
- Design says: FAT = 3 (TV 5), FAT = 2 (TV 7), FAT = 1 (TV 12)

**Vitality** ([Vitality.cs](../GameMechanics/Vitality.cs)):
- ❌ Uses `STR × 2 - 5` instead of `(STR + END) - 5`
- ❌ No low vitality effects implemented (recovery slowdowns, Focus checks)

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| **Critical** | Fix Fatigue formula: `(END × 2) - 5` | `Fatigue.cs` - `CalculateBase()` |
| **Critical** | Fix Vitality formula: `(STR + END) - 5` | `Vitality.cs` - `CalculateBase()` |
| High | Fix Focus check thresholds for low FAT | `Fatigue.cs` - `CheckFocusRolls()` |
| High | Implement low VIT effects (recovery slowdowns) | `Vitality.cs` |
| High | Add VIT = 0 death handling | `Vitality.cs` |

---

### 4. Skill System

**Design Spec**:
- 7 core attribute skills matching attributes
- Ability Score formula: `Related Attribute + Current Skill Level - 5`
- Usage-based progression with Base Cost and Multiplier per skill category
- Skill categories: Core Attribute, Weapon, Spell, Mana Recovery, Crafting, Social

**Implementation** ([SkillEdit.cs](../GameMechanics/SkillEdit.cs), [SkillCost.cs](../GameMechanics/SkillCost.cs)):
- ✅ Ability Score calculation matches design
- ⚠️ 8 standard skills created (Physicality, Dodge, Drive, Reasoning, Awareness, Focus, Bearing, **Influence**)
- ❌ "Influence" skill not in design specification
- ❌ SkillCost uses a fixed table instead of dynamic Base Cost × Multiplier^N formula
- ❌ No usage tracking (UsagePoints exists but no advancement logic)
- ❌ No skill categories defined in code

**SkillCost Discrepancy**:
Design formula: `Cost(N→N+1) = BaseCost × (Multiplier^N)`
Implementation: Uses hardcoded 10×14 lookup table with different values

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| High | Clarify "Influence" skill vs "Bearing" in design | Design doc or `SkillList.cs` |
| High | Implement dynamic skill progression formula | `SkillCost.cs` |
| High | Add skill category definitions | New `SkillCategory.cs` or database |
| High | Implement usage-based advancement logic | `SkillEdit.cs` |
| Medium | Add progression parameters (BaseCost, Multiplier) per skill | `SkillInfo.cs` |
| Medium | Implement usage event types (Routine, Challenging, Critical) | New service class |

---

### 5. Wound System

**Design Spec**:
- Wounds per location: 2 max for limbs/head, 4 max for torso
- 1 FAT damage per wound every 2 rounds (6 seconds)
- -2 AV penalty per wound
- 2 wounds to limb = useless

**Implementation** ([WoundList.cs](../GameMechanics/WoundList.cs), [WoundRecord.cs](../GameMechanics/WoundRecord.cs)):
- ✅ Location-based wounds implemented
- ✅ Max wounds: Head=2, Limbs=2, Torso=4
- ⚠️ Damage timing: 20 rounds vs design's 2 rounds
- ✅ Crippled/Destroyed tracking

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| Medium | Change `RoundsToDamage` from 20 to 2 | `WoundRecord.cs` |
| Medium | Implement -2 AV penalty per wound in skill checks | `SkillEdit.cs` |
| Low | Verify wound FAT damage formula matches design | `WoundRecord.cs` |

---

### 6. Combat System (NOT IMPLEMENTED)

**Design Spec** covers:
- Attack/Defense resolution (AV = AS + 4dF+, SV = AV - TV)
- Melee combat (1 FAT cost per attack/dodge)
- Parry mode (weapon skill for defense, no FAT cost)
- Ranged combat with range modifiers
- Physicality damage bonus (STR check vs TV 8)
- Result Value System (RVS) chart
- Defense sequence (Shield → Armor → Damage)
- Hit locations (1d12 roll)
- Damage classes 1-4

**Implementation**:
- ❌ No combat action classes
- ❌ No attack/defense resolution
- ❌ No parry mode
- ❌ No ranged combat
- ⚠️ `Calculator.cs` has partial damage/RV logic (test/reference only?)

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| **Critical** | Create `CombatAction` class for attack resolution | New `Combat/CombatAction.cs` |
| **Critical** | Implement attack/defense roll mechanics | New `Combat/AttackResolver.cs` |
| High | Implement parry mode system | New combat classes |
| High | Implement ranged combat with modifiers | New combat classes |
| High | Implement Physicality damage bonus | Combat classes |
| High | Implement defense sequence (Shield → Armor) | New `Combat/DefenseResolver.cs` |
| Medium | Implement hit location determination | `WoundList.cs` (partial exists) |

---

### 7. Equipment System (NOT IMPLEMENTED)

**Design Spec** covers:
- Weapon properties (skill, min level, damage type/class, SV/AV/DEX modifiers, range, durability)
- Armor properties (hit locations covered, absorption per damage type, penalties)
- Shield properties (costs 1 FAT, any location)
- Durability system with degradation
- Item skill bonuses and attribute modifiers
- Equipment slots (27+ slots defined)

**Implementation**:
- ❌ No Item/ItemTemplate classes
- ❌ No equipment slot management
- ❌ No weapon/armor properties
- ❌ No durability system
- ❌ No item bonuses

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| **Critical** | Create `ItemTemplate` class | New `Items/ItemTemplate.cs` |
| **Critical** | Create `Item` instance class | New `Items/Item.cs` |
| High | Implement equipment slot enum | New `Items/EquipmentSlot.cs` |
| High | Create `Weapon` subclass/properties | New `Items/Weapon.cs` |
| High | Create `Armor` subclass/properties | New `Items/Armor.cs` |
| High | Implement durability system | Item classes |
| High | Implement ItemSkillBonus | New `Items/ItemSkillBonus.cs` |
| High | Implement ItemAttributeModifier | New `Items/ItemAttributeModifier.cs` |
| Medium | Wire item bonuses into skill calculations | `SkillEdit.cs` |

---

### 8. Inventory & Carrying Capacity (NOT IMPLEMENTED)

**Design Spec**:
- Base weight: `50 lbs × (1.15 ^ (Physicality - 10))`
- Base volume: `10 cu.ft. × (1.15 ^ (Physicality - 10))`
- Container system with nesting
- Magical containers with weight reduction

**Implementation**:
- ❌ No inventory management
- ❌ No carrying capacity calculation
- ❌ No container system

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| High | Add `CalculateCarryingCapacity()` method | `CharacterEdit.cs` |
| High | Create `Inventory` class | New `Inventory/Inventory.cs` |
| High | Create `Container` functionality | New `Inventory/Container.cs` |
| Medium | Implement weight/volume tracking | Inventory classes |
| Medium | Implement magical container reductions | Container class |

---

### 9. Currency System (NOT IMPLEMENTED)

**Design Spec**:
- 4 denominations: Copper, Silver (20cp), Gold (400cp), Platinum (8000cp)
- 100 coins = 1 pound

**Implementation**:
- ❌ No currency tracking
- ❌ No coin weight calculation

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| Medium | Create `Currency` class | New `Currency.cs` |
| Medium | Add currency properties to CharacterEdit | `CharacterEdit.cs` |
| Low | Implement coin optimization | Currency class |

---

### 10. Magic & Mana System (NOT IMPLEMENTED)

**Design Spec**:
- Separate mana pools per magic school
- Individual spell skills
- Mana recovery skills per school
- Spell types: Targeted, Self-Buff, Area Effect, Environmental

**Implementation**:
- ❌ No mana pools
- ❌ No spell definitions
- ❌ No magic school system

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| High | Create `MagicSchool` class | New `Magic/MagicSchool.cs` |
| High | Create `ManaPool` class | New `Magic/ManaPool.cs` |
| High | Create `Spell` skill subtype | `SkillEdit.cs` or new class |
| Medium | Implement mana recovery mechanics | Magic classes |
| Medium | Implement spell casting | New `Magic/SpellCaster.cs` |

---

### 11. Action Points System

**Design Spec**:
- Not explicitly detailed in reviewed documents
- Referenced in combat (actions cost FAT)

**Implementation** ([ActionPoints.cs](../GameMechanics/ActionPoints.cs)):
- ✅ Implemented with Available, Locked, Spent, Max, Recovery
- ⚠️ May need alignment with combat action costs

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| Low | Verify AP mechanics align with combat design | `ActionPoints.cs` |
| Low | Document AP system in design docs if custom | New design doc |

---

### 12. Reference Data Inconsistencies

**SkillList.cs Issues**:
- "Influence" skill not in design (PHY-based) - may be meant to be "Bearing"?
- "Bearing" skill uses "SOC/INT" but design says Bearing is PHY attribute
- Untrained/Trained properties not in design spec

**ResultValues.cs**:
- RVs column differs slightly from design's RVS chart
- Contains RVe, RVx columns not in design (may be legacy)

**DamageSheet.cs**:
- Damage row 8 shows VIT=5, but design shows VIT=6

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| High | Reconcile Influence/Bearing skill confusion | `SkillList.cs`, design docs |
| Medium | Update ResultValues to match design RVS chart | `ResultValues.cs` |
| Low | Fix DamageSheet row 8 VIT value | `DamageSheet.cs` |

---

## Recommended Implementation Order

### Phase 1: Core Fixes (Critical)
1. Fix Fatigue formula: `(END × 2) - 5`
2. Fix Vitality formula: `(STR + END) - 5`
3. Resolve attribute count (7 vs 8) and skill list (7 vs 8)
4. Implement species modifiers

### Phase 2: Skill System Completion
1. Implement dynamic skill progression (BaseCost × Multiplier^N)
2. Add skill categories and parameters
3. Implement usage-based advancement
4. Wire wound penalties into skill checks

### Phase 3: Items & Equipment
1. Create ItemTemplate and Item classes
2. Implement equipment slots
3. Create weapon/armor property systems
4. Implement item bonuses affecting skills/attributes
5. Implement durability

### Phase 4: Combat System
1. Create combat action framework
2. Implement attack/defense resolution
3. Implement defense sequence (shield, armor)
4. Implement ranged combat
5. Implement parry mode

### Phase 5: Inventory & Economy
1. Implement carrying capacity
2. Create inventory management
3. Implement container system
4. Add currency tracking

### Phase 6: Magic System
1. Create magic schools
2. Implement mana pools
3. Create spell skill type
4. Implement spell casting mechanics

---

## Database Schema Alignment

The [DATABASE_DESIGN.md](DATABASE_DESIGN.md) defines tables that are not yet implemented:

| Table | Implementation Status |
|-------|----------------------|
| SkillDefinitions | ❌ Hardcoded in SkillList.cs |
| CharacterSkills | ⚠️ Partial (missing progression tracking) |
| MagicSchools | ❌ Not implemented |
| CharacterMana | ❌ Not implemented |
| ItemTemplates | ❌ Not implemented |
| Items | ❌ Not implemented |
| ItemSkillBonuses | ❌ Not implemented |
| ItemAttributeModifiers | ❌ Not implemented |
| CharacterCurrency | ❌ Not implemented |
| CharacterInventory | ❌ Not implemented |
| EffectDefinitions | ❌ Not implemented |
| CharacterEffects | ❌ Not implemented |

---

## Conclusion

The GameMechanics library has a solid foundation with dice mechanics, basic character attributes, health pools (with formula errors), and wounds. However, the majority of the game systems described in the design documents are not yet implemented, particularly:

- Combat resolution
- Equipment and items
- Inventory management
- Currency
- Magic system

The most critical immediate actions are fixing the health pool formulas and resolving the attribute/skill count discrepancies, as these affect all downstream calculations.
