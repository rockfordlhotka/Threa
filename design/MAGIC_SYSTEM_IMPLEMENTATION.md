# Magic & Mana System Implementation Plan

## Overview

This document outlines the phased implementation plan for the magic and mana systems in Threa. The approach prioritizes building common infrastructure first, enabling individual spells to be added incrementally over time.

**Design Reference**: See [GAME_RULES_SPECIFICATION.md](GAME_RULES_SPECIFICATION.md) for the full magic system design.

---

## Design Decisions

These decisions were made during implementation planning:

| Topic | Decision |
|-------|----------|
| **Magic Schools** | Start with 4 schools: Fire, Water, Light, Life |
| **Max Mana** | Equals the mana skill level for each school (e.g., Fire Mana skill at level 3 = max 3 fire mana) |
| **Mana Recovery** | Active skill action using RVS result table, TV 6 default, takes 1 minute per mana recovered |
| **Spell Resistance** | Per-spell, defined in SpellDefinition (None, Fixed, Opposed, or Willpower-based) |
| **Concentration** | Deferred to later implementation |

---

## Phase 1: Foundation (Common Infrastructure) ✅ COMPLETED

This phase builds the core data structures and services that all spells will need.

### 1.1 DTOs and Enums ✅

| Component | Location | Status |
|-----------|----------|--------|
| `MagicSchool` enum | `Threa.Dal/Dto/MagicSchool.cs` | ✅ Fire, Water, Light, Life |
| `SpellType` enum | `Threa.Dal/Dto/SpellType.cs` | ✅ Targeted, SelfBuff, AreaEffect, Environmental |
| `CharacterMana` DTO | `Threa.Dal/Dto/CharacterMana.cs` | ✅ Per-school mana tracking |
| `SpellDefinition` DTO | `Threa.Dal/Dto/SpellDefinition.cs` | ✅ With SpellResistanceType enum |

### 1.2 DAL Interfaces ✅

| Interface | Location | Status |
|-----------|----------|--------|
| `IManaDal` | `Threa.Dal/IManaDal.cs` | ✅ CRUD for character mana pools |
| `ISpellDefinitionDal` | `Threa.Dal/ISpellDefinitionDal.cs` | ✅ Access to spell definitions |

### 1.3 Core Services ✅

#### ManaManager (`GameMechanics/Magic/ManaManager.cs`) ✅

Manages mana pools per character and magic school:

```csharp
// Core operations
Task<CharacterMana> GetManaPoolAsync(int characterId, MagicSchool school);
Task<List<CharacterMana>> GetAllManaPoolsAsync(int characterId);
Task<bool> HasSufficientManaAsync(int characterId, MagicSchool school, int amount);
Task<SpendManaResult> SpendManaAsync(int characterId, MagicSchool school, int amount);
Task<int> RecoverManaAsync(int characterId, MagicSchool school, int amount, int maxMana);
Task RestoreAllManaAsync(int characterId, Dictionary<MagicSchool, int> manaSkillLevels);

// Active recovery action (uses RVS table, TV 6, 1 minute per mana)
Task<ManaRecoveryResult> AttemptManaRecoveryAsync(int characterId, MagicSchool school, int manaSkillLevel, int attributeBonus, int tv = 6);

// Pool initialization
Task<CharacterMana> EnsureManaPoolAsync(int characterId, MagicSchool school, string manaSkillId, int skillLevel);
```

#### SpellCastingService (`GameMechanics/Magic/SpellCastingService.cs`) ✅

Base service for validating and executing spell casting:

```csharp
// Validation
Task<SpellCastValidation> ValidateSpellCastAsync(SpellCastRequest request);

// Casting (validates mana, performs skill check, creates effects)
Task<SpellCastResult> CastSpellAsync(SpellCastRequest request);
```

### 1.4 Mana Recovery Result Table ✅

Added `ManaRecovery` to `ResultTableType` enum and implemented in `ResultTables.cs`:

| SV | Result | Effect |
|----|--------|--------|
| < -6 | Backlash | Lose 1 mana from pool |
| -6 to -5 | Exhausted | Lose 1 FAT |
| -4 to -3 | Distracted | No recovery |
| -2 to -1 | No Recovery | No recovery |
| 0-1 | Recover 1 | Recover 1 mana (1 minute) |
| 2-3 | Recover 2 | Recover 2 mana (2 minutes) |
| 4-5 | Recover 3 | Recover 3 mana (3 minutes) |
| 6-7 | Recover 4 | Recover 4 mana (4 minutes) |
| 8+ | Recover 5 | Recover 5 mana (5 minutes) |

### 1.5 MockDb Implementations ✅

| Implementation | Status |
|----------------|--------|
| `ManaDal` | ✅ Instance-level storage for test isolation |
| `SpellDefinitionDal` | ✅ With 9 sample spells across all 4 schools |
| DI Registration | ✅ In ConfigurationExtensions |

### 1.6 Unit Tests ✅

All 23 tests passing in `MagicSystemTests.cs`:
- ManaManager tests (spending, recovery, multi-school)
- SpellCastingService tests (validation, casting)
- Active mana recovery tests (RVS table integration)

#### SpellCastRequest and SpellCastResult ✅

```csharp
public class SpellCastRequest
{
    public int CasterId { get; set; }
    public string SpellSkillId { get; set; }
    public int? TargetCharacterId { get; set; }
    public Guid? TargetItemId { get; set; }
    public string? TargetLocation { get; set; }
    public int CasterSkillLevel { get; set; }
    public int CasterAttributeBonus { get; set; }
}

public class SpellCastResult
{
    public bool Success { get; set; }
    public int AV { get; set; } // Attack Value
    public int TV { get; set; } // Target Value
    public int SV { get; set; } // Success Value (AV - TV)
    public int Roll { get; set; }
    public int ManaSpent { get; set; }
    public string ResultDescription { get; set; }
    public List<CharacterEffect> AppliedEffects { get; set; }
    public string? ErrorMessage { get; set; }
}
```

### 1.7 Time System Integration (Pending)

Integrate mana recovery with existing time events:

| Event | Recovery |
|-------|----------|
| Active Mana Recovery | Skill action, RVS table, 1 minute per mana |
| Rest/Sleep | Full mana pool restoration |

**Note**: Mana recovery is primarily an active action, not passive. Characters must spend time meditating/focusing to recover mana. Full restoration happens during extended rest.

### 1.8 Database Tables (Pending SQL Migration)

```sql
-- Character mana pools
CREATE TABLE CharacterMana (
    Id INT PRIMARY KEY IDENTITY,
    CharacterId INT NOT NULL REFERENCES Character(Id),
    MagicSchool INT NOT NULL, -- 1=Fire, 2=Water, 3=Light, 4=Life
    CurrentMana INT NOT NULL DEFAULT 0,
    ManaSkillId NVARCHAR(50) NOT NULL, -- Reference to mana skill
    LastUpdated DATETIME2,
    UNIQUE(CharacterId, MagicSchool)
);

-- Spell definitions (extends Skills)
CREATE TABLE SpellDefinition (
    SkillId NVARCHAR(50) PRIMARY KEY,
    MagicSchool INT NOT NULL, -- 1=Fire, 2=Water, 3=Light, 4=Life
    SpellType INT NOT NULL, -- 1=Targeted, 2=SelfBuff, 3=AreaEffect, 4=Environmental
    ManaCost INT NOT NULL,
    Range INT, -- Units of distance
    AreaRadius INT, -- For area effects
    EffectDefinitionId INT REFERENCES EffectDefinition(Id),
    DefaultDuration INT, -- Duration in rounds/minutes
    ResistanceType INT NOT NULL, -- 0=None, 1=Fixed, 2=Opposed, 3=Willpower
    FixedResistanceTV INT, -- For Fixed resistance type
    OpposedResistanceSkillId NVARCHAR(50) -- For Opposed resistance type
);
```

---

## Phase 2: Spell Category Resolvers ✅ COMPLETED

Build category-specific resolvers that handle the mechanics for each spell type.

### Design Decisions (Phase 2)

| Topic | Decision |
|-------|----------|
| **Area Target Selection** | Caller provides list of target IDs (no spatial simulation) |
| **Multiple Target Rolls** | Caster rolls once (AV), each target defends individually (TV) |
| **Environmental Locations** | Narrative names with generic location construct (no coordinate system) |
| **Effect Duration** | Environmental spells persist at location; other spells affect target directly |
| **Spell Effects** | Per-spell interpretation: some use damage tables, some custom lookups, some narrative (SV passed to resolver) |

### 2.1 Core Types ✅

| Component | Location | Purpose |
|-----------|----------|---------|
| `ISpellResolver` | `GameMechanics/Magic/Resolvers/ISpellResolver.cs` | Interface for spell type resolvers |
| `SpellResolutionContext` | Same file | Input data for resolution |
| `SpellResolutionResult` | Same file | Output from resolution |
| `SpellTargetResult` | Same file | Per-target outcome for multi-target spells |
| `SpellTypeValidation` | Same file | Validation result with valid flag and reasons |
| `SpellResolverFactory` | `GameMechanics/Magic/Resolvers/SpellResolverFactory.cs` | Factory to get resolver by SpellType |

### 2.2 Targeted Spell Resolver ✅ (`GameMechanics/Magic/Resolvers/TargetedSpellResolver.cs`)

Handles single-target offensive and beneficial spells:

- Validates exactly one target is specified
- Performs spell skill check vs target's resistance (if offensive)
- Calculates TV based on SpellResistanceType (None, Fixed, Willpower, Opposed)
- Uses `ResultTableType.CombatDamage` for damage spells
- Applies effect to single target
- Examples: Fire Bolt, Heal, Curse

### 2.3 Self-Buff Resolver ✅ (`GameMechanics/Magic/Resolvers/SelfBuffResolver.cs`)

Handles caster-only enhancement spells:

- No target validation needed (targets caster automatically)
- Automatic success (no opposed roll, TV = 0)
- SV equals AV (full success value)
- Applies effect to caster
- Examples: Strength, Invisibility, Shield

### 2.4 Area Effect Resolver ✅ (`GameMechanics/Magic/Resolvers/AreaEffectResolver.cs`)

Handles multi-target spells:

- Validates at least one target is specified via `TargetCharacterIds`
- Caster AV applies to all targets
- Each target defends individually using `TargetDefenseValues` dictionary
- Returns `TargetResults` list with per-target outcomes
- Examples: Fireball, Mass Heal, Fear

### 2.5 Environmental Spell Resolver ✅ (`GameMechanics/Magic/Resolvers/EnvironmentalSpellResolver.cs`)

Handles persistent location-based effects:

- Creates `SpellLocation` with narrative name and description
- Creates `LocationEffect` with duration (in rounds)
- Effect persists at location independently of caster
- Can affect characters already at location (uses Willpower resistance)
- Returns `AffectedLocation` in result
- Uses `ILocationEffectDal` for persistence
- Examples: Wall of Fire, Fog Cloud, Silence, Darkness

### 2.6 Location System ✅

| Component | Location | Purpose |
|-----------|----------|---------|
| `SpellLocation` DTO | `Threa.Dal/Dto/SpellLocation.cs` | Location entity (Id, Name, Description, CampaignId) |
| `LocationEffect` DTO | Same file | Persistent effect at location (SpellSkillId, CasterId, Duration, SV) |
| `ILocationEffectDal` | `Threa.Dal/ILocationEffectDal.cs` | DAL interface for locations and effects |
| `LocationEffectDal` | `Threa.Dal.MockDb/LocationEffectDal.cs` | MockDb implementation |

### 2.7 SpellCastingService Updates ✅

Extended to use resolver factory:

```csharp
public class SpellCastRequest
{
    // Existing properties...
    public List<int>? TargetCharacterIds { get; set; }  // NEW: For area effects
    public Dictionary<int, int>? TargetDefenseValues { get; set; }  // NEW: Per-target TV
    public int? CampaignId { get; set; }  // NEW: For environmental spells
}

public class SpellCastResult
{
    // Existing properties...
    public List<SpellTargetResult> TargetResults { get; set; }  // NEW: Per-target outcomes
    public SpellLocation? AffectedLocation { get; set; }  // NEW: For environmental spells
}
```

### 2.8 Unit Tests ✅

All 24 resolver tests passing in `SpellResolverTests.cs`:

| Category | Tests |
|----------|-------|
| SpellResolverFactory | 2 tests (returns correct resolver, custom registration) |
| SelfBuffResolver | 4 tests (always succeeds, validation, targets caster, SV=AV) |
| TargetedSpellResolver | 6 tests (success, failure, validation, fixed/willpower/opposed resistance) |
| AreaEffectResolver | 6 tests (multi-target, validation, individual defenses, partial success) |
| EnvironmentalSpellResolver | 6 tests (creates location, persists effect, affects characters, validation) |

**Total magic/spell tests**: 48 (23 Phase 1 + 24 Phase 2 + 1 integration)

---

## Phase 3: Individual Spells

Each spell is primarily data-driven, with custom code only where needed.

### 3.1 Data-Driven Spells

Most spells can be fully defined through configuration:

1. **Skill Definition** - The spell as a learnable skill (name, attributes, costs)
2. **Spell Definition** - Spell-specific metadata (school, type, mana cost, range)
3. **Effect Definition** - What the spell does when successful (buffs, damage, conditions)

**Example: Fire Bolt (Data-Driven)**
```
Skill: Fire Bolt, INT/WIL, Base Cost 20, Multiplier 2.0
Spell: Fire School, Targeted, 5 Mana, Range 2
Effect: Fire Damage, SV-based damage to FAT/VIT
```

### 3.2 Custom Spell Resolvers

Complex spells with unique mechanics need custom code:

| Spell | Reason for Custom Code |
|-------|------------------------|
| Teleport | Location validation, distance calculations |
| Summon Creature | Creates temporary NPC |
| Polymorph | Complex attribute/skill transformation |
| Dispel Magic | Interacts with other spell effects |
| Time Stop | Affects initiative/turn order |

Custom resolvers implement `ISpellResolver` and are registered in the factory.

### 3.3 Spell Implementation Checklist

For each new spell:

- [ ] Create/update Skill definition in database
- [ ] Create SpellDefinition record
- [ ] Create EffectDefinition (if spell has persistent effects)
- [ ] If custom logic needed, create resolver class
- [ ] Add unit tests
- [ ] Document in spell reference

---

## Implementation Order

### Priority 1: Core Infrastructure
1. Enums: `MagicSchool`, `SpellType`
2. DTOs: `CharacterMana`, `SpellDefinition`
3. DAL interfaces and implementations
4. `ManaManager` service
5. Unit tests for mana operations

### Priority 2: Spell Casting Framework
1. `SpellCastRequest`/`SpellCastResult` models
2. `SpellCastingService` base implementation
3. Integration with `EffectManager`
4. Time system integration for mana recovery
5. Unit tests for spell casting flow

### Priority 3: Category Resolvers
1. `SelfBuffResolver` (simplest)
2. `TargetedSpellResolver`
3. `AreaEffectResolver`
4. `EnvironmentalSpellResolver`
5. Unit tests for each resolver

### Priority 4: Sample Spells
1. Implement 2-3 spells per category to validate the system
2. One data-driven spell per category
3. One custom resolver spell to validate extensibility

---

## Integration Points

| System | Integration |
|--------|-------------|
| **SkillEdit** | Spells are skills; spell-specific data in SpellDefinition |
| **ActionResolver** | Spell casting is an action type |
| **EffectManager** | Spell effects use existing effect infrastructure |
| **TimeManager** | Mana recovery hooks into time events |
| **CharacterEdit** | Character has mana pools per school |
| **Combat** | Spells can be used in combat with AP/FAT costs |

---

## Related Documents

- [GAME_RULES_SPECIFICATION.md](GAME_RULES_SPECIFICATION.md) - Magic system design
- [EFFECTS_SYSTEM.md](EFFECTS_SYSTEM.md) - Effect infrastructure used by spells
- [TIME_SYSTEM.md](TIME_SYSTEM.md) - Time events for mana recovery
- [ACTIONS.md](ACTIONS.md) - Action system integration
- [SKILL_PROGRESSION.md](SKILL_PROGRESSION.md) - Spell skill costs and progression
