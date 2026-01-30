---
phase: 19
plan: 01
subsystem: effect-management
tags: [effects, templates, dal, data-model]

dependency-graph:
  requires: [18-wound-management]
  provides: [effect-state-model, effect-template-dal, effect-template-dto]
  affects: [19-02-effect-ui, equipment-integration]

tech-stack:
  added: []
  patterns: [json-state-serialization, behavior-tags]

key-files:
  created:
    - GameMechanics/Effects/EffectState.cs
    - Threa.Dal/Dto/EffectTemplateDto.cs
    - Threa.Dal/IEffectTemplateDal.cs
    - Threa.Dal.MockDb/EffectTemplateDal.cs
    - Threa.Dal.SqlLite/EffectTemplateDal.cs
  modified:
    - Threa.Dal.MockDb/ConfigurationExtensions.cs
    - Threa.Dal.MockDb/MockDb.cs
    - Threa.Dal.SqlLite/ConfigurationExtensions.cs

decisions:
  - key: effect-state-structure
    choice: Dictionary-based modifiers for attributes and skills
    reason: Flexible extensibility without schema changes
  - key: template-seed-data
    choice: 7 system templates covering common effect types
    reason: Provides realistic test data and common-case coverage

metrics:
  duration: 4.5 min
  completed: 2026-01-28
---

# Phase 19 Plan 01: Effect Data Foundation Summary

**One-liner:** JSON-serializable EffectState with attribute/skill modifiers and template DAL with 7 seed templates.

## What Was Built

### EffectState Class
Created `GameMechanics/Effects/EffectState.cs` - a JSON-serializable state class for generic effects:
- Attribute modifiers dictionary (`{"STR": 2, "DEX": -1}`)
- Skill modifiers dictionary
- Global AS modifier (applies to all ability checks)
- FAT/VIT damage and healing per tick (for DoT/HoT effects)
- Behavior tags list for effect-type handling
- Custom data field for effect-specific extensions
- Helper methods: `GetAttributeModifier()`, `GetSkillModifier()`, `HasBehaviorTag()`
- Factory methods: `WithAttributeModifiers()`, `WithASModifier()`, `WithDamagePerTick()`, `WithHealingPerTick()`

### EffectTemplateDto
Created `Threa.Dal/Dto/EffectTemplateDto.cs` - DTO for template persistence:
- Core properties: Id, Name, EffectType, Description
- Display properties: IconName, Color
- Duration: DefaultDurationValue, DurationType (uses existing enum)
- Modifiers: StateJson (serialized EffectState)
- Organization: Tags (comma-separated), IsSystem, IsActive
- Audit: CreatedAt, UpdatedAt

### IEffectTemplateDal Interface
Created `Threa.Dal/IEffectTemplateDal.cs` with full CRUD:
- `GetAllTemplatesAsync()` - All active templates
- `GetTemplatesByTypeAsync(EffectType)` - Filter by category
- `SearchTemplatesAsync(string)` - Search name/tags/description
- `GetTemplateAsync(int)` - Single by ID
- `SaveTemplateAsync(EffectTemplateDto)` - Create or update
- `DeleteTemplateAsync(int)` - Soft delete (IsActive = false)

### DAL Implementations

**MockDb** (`Threa.Dal.MockDb/EffectTemplateDal.cs`):
- In-memory List storage
- Thread-safe not required (single-user mock)
- Auto-increment ID on save

**SQLite** (`Threa.Dal.SqlLite/EffectTemplateDal.cs`):
- Lazy table initialization on first access
- Automatic seeding on empty table
- Proper parameterized queries
- ISO 8601 date format for timestamps

### Seed Templates (7 total)

| Template | Type | Effect | Duration |
|----------|------|--------|----------|
| Stunned | Condition | AS -4 | 1 round |
| Blessed | Buff | AS +2 | 10 rounds |
| Poisoned | Poison | FAT 2/tick, VIT 1/tick | 5 rounds |
| Haste | Buff | DEX +2 | 3 rounds |
| Weakened | Debuff | STR -2 | 5 rounds |
| Regenerating | Buff | FAT heal 1/tick, VIT heal 1/tick | 10 rounds |
| Blinded | Condition | AS -6 | 3 rounds |

## Decisions Made

1. **Dictionary-based modifiers** - Using `Dictionary<string, int>` for attribute/skill modifiers allows any attribute/skill to be modified without schema changes. Follows existing patterns in the codebase.

2. **Behavior tags as list** - Storing tags as `List<string>` in JSON allows multiple behaviors per effect (e.g., `["modifier", "end-of-round-trigger"]`). Avoids flag enum explosion.

3. **Lazy table initialization** - SQLite DAL creates table and seeds on first access, avoiding migration complexity for new tables.

4. **Seed templates are IsSystem = true** - System templates cannot be deleted, providing stable baseline for testing and common use cases.

## Deviations from Plan

None - plan executed exactly as written.

## Commit Log

| Commit | Description |
|--------|-------------|
| f3f2e5b | feat(19-01): create EffectState class for generic effect modifiers |
| ef69517 | feat(19-01): add EffectTemplateDto and IEffectTemplateDal interface |
| a78999b | feat(19-01): implement MockDb and SQLite DAL for effect templates |

## Next Phase Readiness

**Required for 19-02:**
- EffectState serialization tested via build verification
- Template DAL registered and accessible via DI
- Seed templates available for UI development

**No blockers identified.**

---

*Phase: 19-effect-management | Plan: 01 | Duration: 4.5 min*
