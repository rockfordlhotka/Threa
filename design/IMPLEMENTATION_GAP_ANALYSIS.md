# Implementation Gap Analysis

## Overview

This document compares the design specifications in the `/design` folder against the actual implementation in `/GameMechanics`. It identifies discrepancies and provides an action list to bring the code into alignment with the design.

---

## Summary of Current Implementation Status

| System | Design Status | Implementation Status | Gap Level |
|--------|--------------|----------------------|-----------|
| Dice Mechanics | ✅ Complete | ✅ Implemented | Minor |
| Attributes | ✅ Complete | ✅ Implemented | None |
| Health (Fatigue/Vitality) | ✅ Complete | ✅ Core formulas correct | Low |
| Skills | ✅ Complete | ✅ Implemented | None |
| Wounds | ✅ Complete | ✅ Implemented | Minor |
| Combat System | ✅ Complete | ❌ Not Implemented | High |
| Equipment/Items | ✅ Complete | ✅ DAL Implemented | Low |
| Inventory/Carrying Capacity | ✅ Complete | ✅ DAL Implemented | Low |
| Currency | ✅ Complete | ✅ DAL Implemented | Low |
| Magic/Mana | ✅ Complete | ❌ Not Implemented | High |
| Species Modifiers | ✅ Complete | ✅ Implemented | None |

---

## Detailed Gap Analysis

### 1. Dice Mechanics

**Design Spec** ([GAME_RULES_SPECIFICATION.md](GAME_RULES_SPECIFICATION.md)):
- 4dF (Fudge dice) system: range -4 to +4
- 4dF+ (Exploding dice): on +4 or -4, roll again counting only +/- results

**Implementation** ([Dice.cs](../GameMechanics/Dice.cs)):
- ✅ `Roll(count, "F")` - Basic Fudge dice implemented
- ✅ `Roll4dFPlus()` - Exploding dice (4dF+) implemented correctly per spec

**Action Items**: None - implementation matches design.

---

### 2. Attribute System

**Design Spec**:
- 7 core attributes: Physicality (STR), Dodge (DEX), Drive (END), Reasoning (INT), Awareness (ITT), Focus (WIL), Bearing (PHY)
- Human baseline: 4dF + 10
- Species modifiers (Elf, Dwarf, Halfling, Orc) with attribute adjustments

**Implementation** ([AttributeEditList.cs](../GameMechanics/AttributeEditList.cs)):
- ✅ Creates 7 attributes (SOC removed)
- ✅ Uses 4dF + 10 for attribute generation
- ✅ Species modifiers implemented via database
- ✅ Species reference data in MockDb with all 5 species

**Action Items**: None - implementation complete.

---

### 3. Health System (Fatigue & Vitality)

**Design Spec** (updated to match code):
- **Fatigue**: `(END + WIL) - 5` with 3-second recovery cycle
- **Vitality**: `(STR × 2) - 5` with hourly recovery
- Low FAT effects at 3, 2, 1, 0 with Focus checks at TV 5, 7, 12
- Low VIT effects at 4, 3, 2, 1, 0 with recovery slowdowns

**Implementation**:

**Fatigue** ([Fatigue.cs](../GameMechanics/Fatigue.cs)):
- ✅ Uses correct formula `(END + WIL) - 5`
- ⚠️ Focus check thresholds differ: checks at FAT < 6 (TV 5), < 4 (TV 7), < 2 (TV 12)
- Design says: FAT = 3 (TV 5), FAT = 2 (TV 7), FAT = 1 (TV 12)

**Vitality** ([Vitality.cs](../GameMechanics/Vitality.cs)):
- ✅ Uses correct formula `(STR × 2) - 5`
- ❌ No low vitality effects implemented (recovery slowdowns, Focus checks)

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| High | Fix Focus check thresholds for low FAT | `Fatigue.cs` - `CheckFocusRolls()` |
| High | Implement low VIT effects (recovery slowdowns) | `Vitality.cs` |
| High | Add VIT = 0 death handling | `Vitality.cs` |

---

### 4. Skill System

**Design Spec**:
- 7 core attribute skills matching attributes
- Ability Score formula: `Related Attribute + Current Skill Level - 5`
- XP-based progression with 10×14 lookup table (level × difficulty)
- Skill difficulty ratings from 1 (easiest) to 14 (hardest)

**Implementation** ([SkillEdit.cs](../GameMechanics/SkillEdit.cs), [SkillCost.cs](../GameMechanics/SkillCost.cs)):
- ✅ Ability Score calculation matches design
- ✅ 7 standard skills (Influence removed)
- ✅ SkillCost uses 10×14 XP cost lookup table matching design
- ✅ GetBonus() returns `level - 5` per design
- ✅ GetLevelUpCost() returns XP cost based on current level and difficulty

**Action Items**: None - implementation matches design.

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

### 7. Equipment System (DAL IMPLEMENTED)

**Design Spec** covers:
- Weapon properties (skill, min level, damage type/class, SV/AV/DEX modifiers, range, durability)
- Armor properties (hit locations covered, absorption per damage type, penalties)
- Shield properties (costs 1 FAT, any location)
- Durability system with degradation
- Item skill bonuses and attribute modifiers
- Equipment slots (27+ slots defined)

**Implementation** (Threa.Dal layer):
- ✅ `ItemTemplate` DTO with full weapon/armor properties ([ItemTemplate.cs](../Threa.Dal/Dto/ItemTemplate.cs))
- ✅ `CharacterItem` DTO for item instances ([CharacterItem.cs](../Threa.Dal/Dto/CharacterItem.cs))
- ✅ `EquipmentSlot` enum with 27+ slots ([EquipmentSlot.cs](../Threa.Dal/Dto/EquipmentSlot.cs))
- ✅ `ItemSkillBonus` for skill bonuses ([ItemSkillBonus.cs](../Threa.Dal/Dto/ItemSkillBonus.cs))
- ✅ `ItemAttributeModifier` for attribute modifiers ([ItemAttributeModifier.cs](../Threa.Dal/Dto/ItemAttributeModifier.cs))
- ✅ `IItemTemplateDal` and `ICharacterItemDal` interfaces ([IItemDal.cs](../Threa.Dal/IItemDal.cs))
- ✅ MockDb implementation with sample items ([MockDb.cs](../Threa.Dal.MockDb/MockDb.cs))
- ✅ SQLite implementation ([ItemTemplateDal.cs](../Threa.Dal.SqlLite/ItemTemplateDal.cs), [CharacterItemDal.cs](../Threa.Dal.SqlLite/CharacterItemDal.cs))
- ✅ SQL scripts for database tables ([Sql/dbo.ItemTemplate.sql](../Sql/dbo.ItemTemplate.sql), etc.)

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| ~~Critical~~ | ~~Create `ItemTemplate` class~~ | ✅ `Threa.Dal/Dto/ItemTemplate.cs` |
| ~~Critical~~ | ~~Create `Item` instance class~~ | ✅ `Threa.Dal/Dto/CharacterItem.cs` |
| ~~High~~ | ~~Implement equipment slot enum~~ | ✅ `Threa.Dal/Dto/EquipmentSlot.cs` |
| ~~High~~ | ~~Implement ItemSkillBonus~~ | ✅ `Threa.Dal/Dto/ItemSkillBonus.cs` |
| ~~High~~ | ~~Implement ItemAttributeModifier~~ | ✅ `Threa.Dal/Dto/ItemAttributeModifier.cs` |
| Medium | Wire item bonuses into skill calculations | `GameMechanics/SkillEdit.cs` |
| Medium | Create GameMechanics Item business logic | New `GameMechanics/Items/` |
| Low | Implement durability degradation logic | GameMechanics layer |

---

### 8. Inventory & Carrying Capacity (DAL IMPLEMENTED)

**Design Spec**:
- Base weight: `50 lbs × (1.15 ^ (Physicality - 10))`
- Base volume: `10 cu.ft. × (1.15 ^ (Physicality - 10))`
- Container system with nesting
- Magical containers with weight reduction

**Implementation** (Threa.Dal layer):
- ✅ Container support in `ItemTemplate` (IsContainer, ContainerMaxWeight, ContainerMaxVolume)
- ✅ Container nesting via `CharacterItem.ContainerItemId` self-reference
- ✅ `ContainerWeightReduction` for magical containers
- ✅ `ContainerAllowedTypes` for restricted containers (e.g., quivers)
- ✅ `MoveToContainerAsync()` operation in DAL
- ⚠️ Carrying capacity calculation not yet in GameMechanics layer

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| High | Add `CalculateCarryingCapacity()` method | `GameMechanics/CharacterEdit.cs` |
| ~~High~~ | ~~Create `Inventory` class~~ | ✅ Via `ICharacterItemDal` |
| ~~High~~ | ~~Create `Container` functionality~~ | ✅ Via `ItemTemplate.IsContainer` |
| Medium | Implement weight/volume tracking in GameMechanics | New `GameMechanics/Inventory/` |
| ~~Medium~~ | ~~Implement magical container reductions~~ | ✅ `ItemTemplate.ContainerWeightReduction` |

---

### 9. Currency System (DAL IMPLEMENTED)

**Design Spec**:
- 4 denominations: Copper, Silver (20cp), Gold (400cp), Platinum (8000cp)
- 100 coins = 1 pound

**Implementation** (Threa.Dal layer):
- ✅ Currency properties in `Character` DTO (CopperCoins, SilverCoins, GoldCoins, PlatinumCoins)
- ✅ `TotalCopperValue` calculated property for total value
- ✅ Coin item templates in MockDb (Gold/Silver/Copper coins as stackable items)
- ✅ SQL script for CharacterCurrency ([dbo.CharacterCurrency.sql](../Sql/dbo.CharacterCurrency.sql))

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| ~~Medium~~ | ~~Create `Currency` class~~ | ✅ Via `Character` DTO properties |
| Medium | Add currency methods to CharacterEdit | `GameMechanics/CharacterEdit.cs` |
| Low | Implement coin optimization/change-making | GameMechanics layer |
| Low | Implement coin weight calculation | GameMechanics layer |

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

### Phase 1: Core Fixes (Critical) ✅ COMPLETE
1. ~~Fix Fatigue formula~~ ✅ Design docs updated to match code
2. ~~Fix Vitality formula~~ ✅ Design docs updated to match code
3. ~~Remove SOC attribute and Influence skill from code~~ ✅ Done
4. ~~Implement species modifiers~~ ✅ Done (database-driven)

### Phase 2: Skill System ✅ COMPLETE
1. ~~XP-based skill progression~~ ✅ 10×14 lookup table implemented
2. ~~Skill difficulty ratings~~ ✅ Design updated to match implementation
3. Wire wound penalties into skill checks (deferred to combat)

### Phase 3: Items & Equipment ✅ DAL COMPLETE
1. ~~Create ItemTemplate and Item classes~~ ✅ Done (Threa.Dal.Dto)
2. ~~Implement equipment slots~~ ✅ Done (EquipmentSlot enum)
3. ~~Create weapon/armor property systems~~ ✅ Done (ItemTemplate properties)
4. ~~Implement item bonuses affecting skills/attributes~~ ✅ Done (ItemSkillBonus, ItemAttributeModifier)
5. Wire item bonuses into GameMechanics skill calculations (TODO)

### Phase 4: Combat System
1. Create combat action framework
2. Implement attack/defense resolution
3. Implement defense sequence (shield, armor)
4. Implement ranged combat
5. Implement parry mode

### Phase 5: Inventory & Economy ✅ DAL COMPLETE
1. Implement carrying capacity calculation in GameMechanics (TODO)
2. ~~Create inventory management~~ ✅ Done (ICharacterItemDal)
3. ~~Implement container system~~ ✅ Done (Container properties in ItemTemplate)
4. ~~Add currency tracking~~ ✅ Done (Currency properties in Character)

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
| ItemTemplates | ✅ Implemented (DTO, DAL, SQL) |
| Items (CharacterItem) | ✅ Implemented (DTO, DAL, SQL) |
| ItemSkillBonuses | ✅ Implemented (DTO, SQL) |
| ItemAttributeModifiers | ✅ Implemented (DTO, SQL) |
| CharacterCurrency | ✅ Implemented (Character DTO, SQL) |
| CharacterInventory | ✅ Via CharacterItem table |
| EffectDefinitions | ❌ Not implemented |
| CharacterEffects | ❌ Not implemented |

---

## Conclusion

The GameMechanics library has a solid foundation with dice mechanics, basic character attributes, health pools, and wounds. The DAL layer now includes comprehensive support for:

- ✅ **Equipment and Items** - Full ItemTemplate/CharacterItem system with bonuses
- ✅ **Inventory Management** - Container support with nesting and weight reduction
- ✅ **Currency** - 4-denomination system integrated into Character DTO

Remaining major systems to implement:

- ❌ **Combat resolution** - Attack/defense mechanics, damage calculation
- ❌ **Magic system** - Mana pools, spells, magic schools
- ⚠️ **GameMechanics integration** - Wire item bonuses into skill/attribute calculations

Next steps should focus on:
1. Adding carrying capacity calculation to GameMechanics
2. Wiring item bonuses into skill calculations
3. Implementing the combat system
