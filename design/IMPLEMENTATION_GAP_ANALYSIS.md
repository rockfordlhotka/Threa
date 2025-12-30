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
| Actions System | ✅ Complete | ✅ Implemented | None |
| Combat System | ✅ Complete | ❌ Not Implemented | High |
| Equipment/Items | ✅ Complete | ✅ DAL Implemented | Low |
| Inventory/Carrying Capacity | ✅ Complete | ✅ DAL Implemented | Low |
| Currency | ✅ Complete | ✅ Implemented | None |
| Magic/Mana | ✅ Complete | ❌ Not Implemented | High |
| Species Modifiers | ✅ Complete | ✅ Implemented | None |
| Action Points | ✅ Complete | ✅ Implemented | None |
| Time System | ✅ Complete | ✅ Implemented | None |
| Effects System | ✅ Complete | ✅ Implemented | None |
| Movement | ✅ Complete | ✅ Implemented | None |
| **Messaging (RabbitMQ)** | ✅ Complete | ✅ Implemented | None |

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

### 6. Actions System (IMPLEMENTED)

**Design Spec** ([ACTIONS.md](ACTIONS.md)):
- Universal action resolution framework for ALL skill uses
- 9-step resolution flow: Declare → Cost → Modifiers → AS → Roll → TV → SV → Result → Effects
- Standard cost: 1 AP + 1 FAT (or 2 AP fatigue-free)
- AS calculation: Attribute + Skill Level - 5 + modifiers
- Boost mechanic: 1 AP or 1 FAT = +1 AS (stackable, mixable)
- Multiple action penalty: -1 AS (not cumulative)
- TV: Fixed (difficulty table) or Opposed (defender roll)
- Passive TV: AS - 1 (no roll)
- SV determines success/failure and result magnitude
- Result lookup tables (general, combat damage, skill-specific)
- Skill definition requirements for app implementation

**Implementation** (GameMechanics.Actions namespace):
- ✅ `ActionResolver` - Main service implementing 9-step resolution flow ([ActionResolver.cs](../GameMechanics/Actions/ActionResolver.cs))
- ✅ `ActionRequest` - Input containing Skill, attribute, modifiers, boosts ([ActionRequest.cs](../GameMechanics/Actions/ActionRequest.cs))
- ✅ `ActionResult` - Complete result with AS, TV, SV, success/failure, quality ([ActionResult.cs](../GameMechanics/Actions/ActionResult.cs))
- ✅ `AbilityScore` - AS calculation with `BaseAS = Attribute + Skill - 5` ([AbilityScore.cs](../GameMechanics/Actions/AbilityScore.cs))
- ✅ `ActionCost` - Standard (1 AP + 1 FAT), FatigueFree (2 AP), Free, Spell ([ActionCost.cs](../GameMechanics/Actions/ActionCost.cs))
- ✅ `TargetValue` - Fixed, Opposed (with opponent roll), Passive (AS - 1) ([TargetValue.cs](../GameMechanics/Actions/TargetValue.cs))
- ✅ `ModifierStack` / `AsModifier` - Modifier aggregation with sources ([ModifierStack.cs](../GameMechanics/Actions/ModifierStack.cs))
- ✅ `ResultTables` - 7 result tables (General, CombatDamage, Defense, Social, Perception, Crafting, Healing) ([ResultTables.cs](../GameMechanics/Actions/ResultTables.cs))
- ✅ Boost mechanic: `ActionCost.WithAPBoost()`, `WithFATBoost()` add to AS
- ✅ Multiple action penalty via `ApplyMultipleActionPenalty()`
- ✅ Wound penalty via `ApplyWoundPenalties(count)` - 2 AS per wound
- ✅ Aim bonus via `ApplyAimBonus()` - +2 AS for ranged
- ✅ 58 unit tests validating action resolution ([ActionSystemTests.cs](../GameMechanics.Test/ActionSystemTests.cs), [ActionResolverTests.cs](../GameMechanics.Test/ActionResolverTests.cs), [ResultTablesTests.cs](../GameMechanics.Test/ResultTablesTests.cs))

**DAL Layer** (Threa.Dal.Dto):
- ✅ `ActionType` enum - 14 action categories (Attack, Defense, Spell, Social, etc.) ([ActionType.cs](../Threa.Dal/Dto/ActionType.cs))
- ✅ `TargetValueType` enum - Fixed, Opposed, Passive ([TargetValueType.cs](../Threa.Dal/Dto/TargetValueType.cs))
- ✅ `CooldownType` enum - None, SkillBased, Fixed, PrepRequired ([CooldownType.cs](../Threa.Dal/Dto/CooldownType.cs))
- ✅ `ResultTableType` enum - 8 result table types ([ResultTableType.cs](../Threa.Dal/Dto/ResultTableType.cs))
- ✅ `Skill` DTO extended with 14 action system properties (ActionType, TargetValueType, DefaultTV, ResultTable, etc.) ([Skill.cs](../Threa.Dal/Dto/Skill.cs))

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| ~~Critical~~ | ~~Create `Action` class with resolution flow~~ | ✅ `Actions/ActionResolver.cs` |
| ~~Critical~~ | ~~Implement AS calculation with modifiers~~ | ✅ `Actions/AbilityScore.cs` |
| ~~Critical~~ | ~~Implement boost mechanic~~ | ✅ `Actions/ActionCost.cs` |
| ~~High~~ | ~~Create modifier aggregation system~~ | ✅ `Actions/ModifierStack.cs` |
| ~~High~~ | ~~Implement TV calculation (fixed and opposed)~~ | ✅ `Actions/TargetValue.cs` |
| ~~High~~ | ~~Create result lookup tables~~ | ✅ `Actions/ResultTables.cs` |
| ~~Medium~~ | ~~Implement skill definition schema~~ | ✅ `Threa.Dal/Dto/Skill.cs` |
| Medium | Track multiple actions per round | Combat/round state tracking |
| Low | Create app action trigger interface | New `Actions/IActionTrigger.cs` |

---

### 7. Combat System (NOT IMPLEMENTED)

**Design Spec** ([COMBAT_SYSTEM.md](COMBAT_SYSTEM.md)):
- Initiative by Available AP (highest first)
- Action cost: 1 AP + 1 FAT (or 2 AP)
- Multiple action penalty: -1 AS (not cumulative)
- Boost mechanic: 1 AP or 1 FAT = +1 AS (stackable)
- Active defense (dodge, parry, shield): costs action
- Passive defense: Dodge AS - 1 (no action, no roll)
- Parry mode: enter as action, free defenses until broken
- Ranged cooldowns by skill level
- Cooldown interruption: resettable vs pausable

**Implementation**:
- ❌ No combat action classes
- ❌ No attack/defense resolution
- ❌ No initiative system
- ❌ No parry mode tracking
- ❌ No ranged combat/cooldowns
- ⚠️ `Calculator.cs` has partial damage/RV logic

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

### 8. Equipment System (DAL IMPLEMENTED)

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

### 9. Inventory & Carrying Capacity (DAL IMPLEMENTED)

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

### 10. Currency System (IMPLEMENTED)

**Design Spec**:
- 4 denominations: Copper, Silver (20cp), Gold (400cp), Platinum (8000cp)
- 100 coins = 1 pound

**Implementation**:

**DAL Layer** (Threa.Dal):
- ✅ Currency properties in `Character` DTO (CopperCoins, SilverCoins, GoldCoins, PlatinumCoins)
- ✅ `TotalCopperValue` calculated property for total value
- ✅ Coin item templates in MockDb (Gold/Silver/Copper coins as stackable items)
- ✅ SQL script for CharacterCurrency ([dbo.CharacterCurrency.sql](../Sql/dbo.CharacterCurrency.sql))

**GameMechanics Layer**:
- ✅ `Currency` class ([Currency.cs](../GameMechanics/Currency.cs)) - tracks coin quantities with INotifyPropertyChanged
- ✅ `CoinType` enum ([CoinType.cs](../GameMechanics/CoinType.cs)) - Copper, Silver, Gold, Platinum
- ✅ `MoneyChanger` static class ([MoneyChanger.cs](../GameMechanics/MoneyChanger.cs)) - currency exchange functionality:
  - ✅ `FromCopper()` - converts copper value to optimized coin distribution
  - ✅ `Optimize()` - minimizes coin count by converting to larger denominations
  - ✅ `TryMakeChange()` - calculates change for transactions
  - ✅ `TryPay()` / `TryPayExact()` - payment processing
  - ✅ `TryConvert()` - converts between denominations
  - ✅ `TryBreakCoin()` - breaks larger coins into smaller ones
  - ✅ `TryTransfer()` / `TryTransferValue()` - moves currency between purses
  - ✅ `TryParse()` / `FormatCopper()` - string parsing and formatting
- ✅ Unit tests ([CurrencyTests.cs](../GameMechanics.Test/CurrencyTests.cs)) - 36 tests covering all functionality

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| ~~Medium~~ | ~~Create `Currency` class~~ | ✅ `GameMechanics/Currency.cs` |
| ~~Low~~ | ~~Implement coin optimization/change-making~~ | ✅ `GameMechanics/MoneyChanger.cs` |
| ~~Low~~ | ~~Implement coin weight calculation~~ | ✅ `Currency.WeightInPounds` |
| Low | Add currency methods to CharacterEdit | `GameMechanics/CharacterEdit.cs` |

---

### 11. Magic & Mana System (NOT IMPLEMENTED)

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

### 12. Time System ✅ IMPLEMENTED

**Design Spec** ([TIME_SYSTEM.md](TIME_SYSTEM.md)):
- Round = 3 seconds
- Initiative based on Available AP (highest goes first)
- End-of-round processing: pending damage, FAT recovery, AP recovery, cooldowns
- Long-term time events: minute (20 rounds), turn (10 min), hour, day, week
- Cooldown system for actions with prep time (resettable vs pausable)
- GM-triggered time events affecting all tracked entities

**Implementation** (GameMechanics.Time namespace):

**Core Classes**:
- ✅ `TimeEventType` enum - EndOfRound, EndOfMinute, EndOfTurn, EndOfHour, EndOfDay, EndOfWeek ([TimeEventType.cs](../GameMechanics/Time/TimeEventType.cs))
- ✅ `TimeState` class - Tracks current time state (rounds, minutes, turns, hours, days, weeks) ([TimeManager.cs](../GameMechanics/Time/TimeManager.cs))
- ✅ `TimeManager` class - Top-level orchestrator for GM-triggered time advancement ([TimeManager.cs](../GameMechanics/Time/TimeManager.cs))
  - ✅ `AdvanceTimeAsync()` - Advances time by specified event type
  - ✅ `SkipTimeAsync()` - Narrative time skip without detailed processing
  - ✅ `TrackCharacter()`/`UntrackCharacter()` - Register characters for time events
  - ✅ `EnterCombat()`/`ExitCombat()` - Combat mode management
  - ✅ Event handlers for `TimeAdvanced` notifications
  - ✅ `ITimeEventHandler` interface for external time event subscribers

**Combat/Round Management**:
- ✅ `RoundManager` class - Manages combat rounds and per-character processing ([RoundManager.cs](../GameMechanics/Time/RoundManager.cs))
  - ✅ `StartCombat()` / `EndCombat()` - Combat lifecycle
  - ✅ `StartRound()` / `EndRoundAsync()` - Round lifecycle with end-of-round processing
  - ✅ Integration with existing `CharacterEdit.EndOfRound()` methods
  - ✅ Events: `RoundEnded`, `MinuteEnded`, `TurnEnded`, `HourEnded`
- ✅ `RoundResult` / `CharacterRoundResult` - Detailed results from round processing ([RoundResult.cs](../GameMechanics/Time/RoundResult.cs))
  - ✅ Tracks AP/FAT/VIT changes, cooldowns completed, effects expired, deaths

**Initiative System**:
- ✅ `InitiativeCalculator` class - AP-based initiative ordering ([InitiativeCalculator.cs](../GameMechanics/Time/InitiativeCalculator.cs))
  - ✅ Orders by Available AP (descending), Awareness as tiebreaker
  - ✅ `AddParticipant()` / `RemoveParticipant()` - Manage combatants
  - ✅ `CalculateInitiative()` - Recalculate order for new round
  - ✅ `ActAndAdvance()` / `PassAndAdvance()` / `Delay()` - Turn management
  - ✅ Tracks `HasActed`, `HasPassed`, `IsDelaying`, `CanAct` per participant
- ✅ `InitiativeEntry` class - Individual participant state

**Cooldown System**:
- ✅ `Cooldown` class - Individual action cooldown tracking ([Cooldown.cs](../GameMechanics/Time/Cooldown.cs))
  - ✅ `AdvanceRound()` / `AdvanceSeconds()` - Progress cooldown
  - ✅ `Interrupt()` / `Resume()` / `Restart()` - Interruption handling
  - ✅ `ForSkillBasedAction()` - Skill-level-based cooldown duration (per TIME_SYSTEM.md table)
  - ✅ `Progress` property - 0.0 to 1.0 completion percentage
- ✅ `CooldownBehavior` enum - Resettable vs Pausable ([CooldownType.cs](../GameMechanics/Time/CooldownType.cs))
- ✅ `CooldownState` enum - Ready, Active, Paused, Reset
- ✅ `CooldownTracker` class - Per-character cooldown management ([CooldownTracker.cs](../GameMechanics/Time/CooldownTracker.cs))
  - ✅ `StartCooldown()` / `StartSkillBasedCooldown()` - Begin tracking
  - ✅ `IsOnCooldown()` / `IsActionReady()` - Query status
  - ✅ `InterruptAll()` / `InterruptAction()` - Handle interruptions

**Unit Tests** ([TimeSystemTests.cs](../GameMechanics.Test/TimeSystemTests.cs)):
- ✅ 31 tests covering cooldowns, initiative, time state, and event types

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| ~~Critical~~ | ~~Create `TimeManager` class for round/time tracking~~ | ✅ `GameMechanics/Time/TimeManager.cs` |
| ~~Critical~~ | ~~Create time event system (EndOfRound, EndOfMinute, etc.)~~ | ✅ `GameMechanics/Time/TimeEventType.cs` |
| ~~High~~ | ~~Implement initiative calculation (by Available AP)~~ | ✅ `GameMechanics/Time/InitiativeCalculator.cs` |
| ~~High~~ | ~~Create cooldown tracking per character~~ | ✅ `GameMechanics/Time/CooldownTracker.cs` |
| ~~Medium~~ | ~~Integrate EndOfRound processing across systems~~ | ✅ `RoundManager.cs` |
| ~~Medium~~ | ~~Add RabbitMQ/messaging integration for GM triggers~~ | ✅ `GameMechanics.Messaging.RabbitMQ` project |
| Low | Add NPC tracking to initiative/time system | Extend `RoundManager` |

**Messaging System** (GameMechanics.Messaging / GameMechanics.Messaging.RabbitMQ):
- ✅ `ITimeEventPublisher` interface - Publish time events to message broker ([ITimeEventPublisher.cs](../GameMechanics/Messaging/ITimeEventPublisher.cs))
- ✅ `ITimeEventSubscriber` interface - Subscribe to time events ([ITimeEventSubscriber.cs](../GameMechanics/Messaging/ITimeEventSubscriber.cs))
- ✅ `TimeMessages.cs` - DTOs for messaging (TimeEventMessage, TimeSkipMessage, CombatStateMessage, TimeResultMessage)
- ✅ `MessagingOptions.cs` - Configuration for broker connections
- ✅ `RabbitMqTimeEventPublisher` - RabbitMQ implementation of publisher
- ✅ `RabbitMqTimeEventSubscriber` - RabbitMQ implementation of subscriber
- ✅ `TimeEventHandlerService` - Hosted service bridging messaging with TimeManager
- ✅ DI extensions for `AddRabbitMqMessaging()` and `AddTimeEventHandlerService()`

---

### 13. Combat System

---

### 13. Action Points System

**Design Spec** ([ACTION_POINTS.md](ACTION_POINTS.md)):
- Max AP = Total Skill Levels / 10 (minimum 1)
- Actions cost: 1 AP + 1 FAT (standard) OR 2 AP (fatigue-free)
- Recovery per round = Current FAT / 4 (minimum 1)
- Three states: Available, Spent, Locked

**Implementation** ([ActionPoints.cs](../GameMechanics/ActionPoints.cs)):
- ✅ Available, Locked, Spent, Max, Recovery properties
- ✅ `TakeActionWithFatigue()` - 1 AP + 1 FAT cost
- ✅ `TakeActionNoFatigue()` - 2 AP cost
- ✅ `CalculateRecovery()` - FAT / 4 (matches design)
- ✅ `EndOfRound()` - recovers AP, returns locked
- ✅ `Rest()` - trades AP for FAT healing
- ✅ `CalculateMax()` uses total skill levels / 10 (minimum 1)

**Action Items**: None - implementation matches design.

---

### 14. Effects System ✅ IMPLEMENTED

**Design Spec** ([EFFECTS_SYSTEM.md](EFFECTS_SYSTEM.md)):
- Effects apply to characters, NPCs, or objects
- Effect types: Wounds, Conditions, Poisons, Buffs, Debuffs, Spell Effects, Object Effects
- Duration types: Rounds, Minutes, Hours, Days, Weeks, Permanent, UntilRemoved
- Stacking behaviors: Replace, Extend, Intensify, Independent
- Effect impacts modify: Attributes, Skills, AS, AV, TV, SV, Recovery rates
- Cascading attribute modifiers to skills
- Time-based processing at round/minute/hour/day/week boundaries

**Implementation**:

**DAL Layer DTOs** (Threa.Dal.Dto):
- ✅ `EffectType` enum - 9 types: Wound, Condition, Poison, Disease, Buff, Debuff, SpellEffect, ObjectEffect, Environmental ([EffectType.cs](../Threa.Dal/Dto/EffectType.cs))
- ✅ `DurationType` enum - 7 types: Rounds, Minutes, Hours, Days, Weeks, Permanent, UntilRemoved ([DurationType.cs](../Threa.Dal/Dto/DurationType.cs))
- ✅ `StackBehavior` enum - 4 behaviors: Replace, Extend, Intensify, Independent ([StackBehavior.cs](../Threa.Dal/Dto/StackBehavior.cs))
- ✅ `EffectTargetType` enum - Character, Npc, Item, Location ([EffectTargetType.cs](../Threa.Dal/Dto/EffectTargetType.cs))
- ✅ `EffectImpactType` enum - 10 impact types: AttributeModifier, SkillModifier, ASModifier, AVModifier, TVModifier, SVModifier, RecoveryModifier, DamageOverTime, MovementModifier, SpecialAbility ([EffectImpactType.cs](../Threa.Dal/Dto/EffectImpactType.cs))
- ✅ `EffectImpact` class - Individual modifier within an effect ([EffectImpact.cs](../Threa.Dal/Dto/EffectImpact.cs))
- ✅ `EffectDefinition` class - Template for effects with name, description, duration, impacts, stacking rules ([EffectDefinition.cs](../Threa.Dal/Dto/EffectDefinition.cs))
- ✅ `CharacterEffect` class - Active effect instance on a character with stacks, duration tracking ([CharacterEffect.cs](../Threa.Dal/Dto/CharacterEffect.cs))
- ✅ `ItemEffect` class - Active effect instance on an item ([ItemEffect.cs](../Threa.Dal/Dto/ItemEffect.cs))

**DAL Interfaces** (Threa.Dal):
- ✅ `IEffectDefinitionDal` - CRUD for effect templates ([IEffectDal.cs](../Threa.Dal/IEffectDal.cs))
- ✅ `ICharacterEffectDal` - Character effect management with stacking logic, ProcessEndOfRoundAsync
- ✅ `IItemEffectDal` - Item effect management including GetEquippedItemEffectsAsync

**MockDb Implementation** (Threa.Dal.MockDb):
- ✅ `EffectDefinitionDal` - Full CRUD implementation ([EffectDefinitionDal.cs](../Threa.Dal.MockDb/EffectDefinitionDal.cs))
- ✅ `CharacterEffectDal` - Effect application with stacking behavior handling ([CharacterEffectDal.cs](../Threa.Dal.MockDb/CharacterEffectDal.cs))
- ✅ `ItemEffectDal` - Item effect management ([ItemEffectDal.cs](../Threa.Dal.MockDb/ItemEffectDal.cs))
- ✅ 20+ preset effect definitions in MockDb: Wound, Stunned, Unconscious, Prone, Blinded, Weak/Strong Poison, Strength/Agility Boost, Battle Focus, Weakened, Intoxicated, Invisibility, Magic Shield, Burning, Magical Light, Temporary Enchantment, Cursed ([MockDb.cs](../Threa.Dal.MockDb/MockDb.cs))

**SQLite Implementation** (Threa.Dal.SqlLite):
- ✅ `EffectDefinitionDal` - SQLite implementation with JSON storage ([EffectDefinitionDal.cs](../Threa.Dal.SqlLite/EffectDefinitionDal.cs))
- ✅ `CharacterEffectDal` - SQLite implementation ([CharacterEffectDal.cs](../Threa.Dal.SqlLite/CharacterEffectDal.cs))
- ✅ `ItemEffectDal` - SQLite implementation ([ItemEffectDal.cs](../Threa.Dal.SqlLite/ItemEffectDal.cs))

**SQL Scripts**:
- ✅ `dbo.EffectDefinition.sql` - EffectDefinition and EffectImpact tables ([dbo.EffectDefinition.sql](../Sql/dbo.EffectDefinition.sql))
- ✅ `dbo.CharacterEffect.sql` - CharacterEffect table with FK constraints ([dbo.CharacterEffect.sql](../Sql/dbo.CharacterEffect.sql))
- ✅ `dbo.ItemEffect.sql` - ItemEffect table with FK constraints ([dbo.ItemEffect.sql](../Sql/dbo.ItemEffect.sql))

**GameMechanics Layer** (GameMechanics.Effects):
- ✅ `EffectManager` - Main service for applying/removing effects, calculating modifiers, processing end-of-round ([EffectManager.cs](../GameMechanics/Effects/EffectManager.cs))
- ✅ `EffectCalculator` - Stateless calculator for attribute/skill modifiers, AS/AV/TV modifiers, DoT damage, special abilities ([EffectCalculator.cs](../GameMechanics/Effects/EffectCalculator.cs))
- ✅ `EffectModifierExtensions` - Integration with Actions system ModifierStack ([EffectModifierExtensions.cs](../GameMechanics/Effects/EffectModifierExtensions.cs))

**Unit Tests** (GameMechanics.Test):
- ✅ `EffectsSystemTests` - Comprehensive tests covering effect definitions, character effects, stacking behaviors, end-of-round processing, calculator functions, manager integration ([EffectsSystemTests.cs](../GameMechanics.Test/EffectsSystemTests.cs))

**Key Features Implemented**:
- ✅ Effect definition templates with reusable patterns
- ✅ Multiple effect impacts per definition
- ✅ Stack behavior handling (Replace removes old, Extend adds duration, Intensify increases stacks, Independent allows duplicates)
- ✅ Duration tracking with rounds remaining
- ✅ End-of-round processing (decrement duration, expire effects, DoT ticks)
- ✅ Character effects with source tracking
- ✅ Item effects with equipped item awareness
- ✅ Modifier calculations for attributes, skills, AS, AV, TV, SV
- ✅ Damage over time (FAT and VIT damage)
- ✅ Special ability detection
- ✅ Break conditions for concentration effects

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| ~~Critical~~ | ~~Create `Effect` base class and types~~ | ✅ `Threa.Dal/Dto/EffectDefinition.cs` |
| ~~Critical~~ | ~~Create `EffectManager` for tracking active effects~~ | ✅ `GameMechanics/Effects/EffectManager.cs` |
| ~~High~~ | ~~Implement effect impact calculations~~ | ✅ `GameMechanics/Effects/EffectCalculator.cs` |
| Medium | Integrate effects into CharacterEdit calculations | `CharacterEdit.cs` |
| Medium | Convert WoundList to use effects system | `WoundList.cs` |
| Low | Add effect display to UI | Threa.Client components |

---

### 15. Movement System ✅ IMPLEMENTED

**Design Spec** ([MOVEMENT.md](MOVEMENT.md)):
- Range system: Distance = Range Value² (in meters)
- Free positioning: Range 0-2 (0-4m) - no action cost
- Sprint action: Range 3 (9m) - costs 1 AP + 1 FAT
- Full-round sprint: Range 5 (25m) - uses entire round
- Travel rates: Walking 4m/round, Endurance 10m, Burst 12m, Sprint 16m
- Travel fatigue costs per distance traveled
- Encumbrance effects on movement

**Implementation** ([Movement.cs](../GameMechanics/Actions/Movement.cs), [SprintResolver.cs](../GameMechanics/Actions/SprintResolver.cs), [TravelCalculator.cs](../GameMechanics/Actions/TravelCalculator.cs), [Encumbrance.cs](../GameMechanics/Actions/Encumbrance.cs)):

**Core Movement** (`Movement.cs`):
- ✅ `RangeToMeters(range)` - Power-of-2 conversion (Range² = meters)
- ✅ `MetersToRange(meters)` - Inverse conversion (√meters = range)
- ✅ `GetRangeDescription(range)` - Human-readable descriptions
- ✅ `MovementType` enum - FreePositioning, Sprint, FullRoundSprint
- ✅ `MovementCost` class - AP/FAT costs for each movement type
- ✅ `MovementResult` class - Result container with base/achieved range

**Sprint Resolution** (`SprintResolver.cs`):
- ✅ `FreePositioning()` - No-cost movement up to Range 2
- ✅ `Sprint()` - Skill check for Range 3 movement
- ✅ `FullRoundSprint()` - Full-round Range 5 movement
- ✅ Terrain modifiers via `TerrainModifiers` constants
- ✅ Uses `IDiceRoller` DI for testability

**Travel System** (`TravelCalculator.cs`):
- ✅ `TravelType` enum - Walking, EnduranceRunning, BurstRunning, FastSprinting
- ✅ `TravelRate` class - Speed and fatigue cost per travel type
- ✅ `CalculateTravel(distance, type)` - Rounds and fatigue for travel
- ✅ `CalculateMaxDistance(fatigue, type)` - How far with available FAT
- ✅ Time constants: 3 sec/round, 20 rounds/min, 1200 rounds/hour

**Encumbrance** (`Encumbrance.cs`):
- ✅ `CalculateMaxWeight(physicality)` - 50 lbs × 1.15^(PHY-10)
- ✅ `CalculateMaxVolume(physicality)` - 10 cu.ft. × 1.15^(PHY-10)
- ✅ `CalculateEncumbrance(weight, capacity)` - Encumbrance status
- ✅ `EncumbranceLevel` enum - Unencumbered to Overloaded
- ✅ Movement penalties: -1 at 75%, -2 at 100%, -3 at 125%, can't move at 150%
- ✅ `ApplyToMovement(range, encumbrance)` - Apply penalties to movement

**Unit Tests** (`MovementTests.cs`):
- ✅ 91 tests covering all movement functionality
- ✅ Range conversion tests
- ✅ Sprint resolution tests with deterministic dice
- ✅ Travel calculation tests
- ✅ Encumbrance level and penalty tests

**Remaining Gaps** (marked "To be documented" in design):
- ⚠️ Species movement rate modifiers not implemented
- ⚠️ Special movement (climbing, swimming, jumping, falling) not implemented

**Action Items**:
| Priority | Task | File(s) |
|----------|------|---------|
| Low | Add species movement modifiers | `Movement.cs`, `SprintResolver.cs` |
| Low | Implement special movement types | New files or `Movement.cs` |

---

### 16. Reference Data Inconsistencies

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
| EffectDefinitions | ✅ Implemented (DTO, DAL, SQL) |
| CharacterEffects | ✅ Implemented (DTO, DAL, SQL) |

---

## Conclusion

The GameMechanics library has a solid foundation with dice mechanics, basic character attributes, health pools, and wounds. The DAL layer now includes comprehensive support for:

- ✅ **Equipment and Items** - Full ItemTemplate/CharacterItem system with bonuses
- ✅ **Inventory Management** - Container support with nesting and weight reduction
- ✅ **Currency** - 4-denomination system integrated into Character DTO
- ✅ **Effects System** - Full effect definitions, character/item effects, stacking, and modifier calculations

Remaining major systems to implement:

- ✅ **Actions System** - Universal action resolution framework implemented in `GameMechanics.Actions`
- ✅ **Effects System** - Complete implementation in `GameMechanics.Effects` with DAL support
- ❌ **Combat resolution** - Attack/defense mechanics, damage calculation
- ❌ **Magic system** - Mana pools, spells, magic schools
- ⚠️ **GameMechanics integration** - Wire item bonuses and effects into skill/attribute calculations

Next steps should focus on:
1. ~~Implementing the Actions System (universal action resolution)~~ ✅ Done
2. ~~Implementing the Effects System~~ ✅ Done
3. Integrating effects into CharacterEdit calculations
4. Adding carrying capacity calculation to GameMechanics
5. Wiring item bonuses into skill calculations
6. Implementing the combat system (builds on Actions System - now ready)
