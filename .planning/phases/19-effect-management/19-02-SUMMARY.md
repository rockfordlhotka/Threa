---
phase: 19
plan: 02
subsystem: effect-management
tags: [effects, templates, csla, behaviors]

dependency-graph:
  requires: [19-01-effect-data-foundation]
  provides: [effect-template-csla, effect-template-list, generic-effect-behavior]
  affects: [effect-ui-integration, gm-effect-application]

tech-stack:
  added: []
  patterns: [csla-readonly-base, csla-readonly-list-base, behavior-pattern]

key-files:
  created:
    - GameMechanics/EffectTemplate.cs
    - GameMechanics/EffectTemplateList.cs
    - GameMechanics/Effects/Behaviors/GenericEffectBehavior.cs
  modified:
    - GameMechanics/Effects/EffectBehaviorFactory.cs

decisions:
  - key: generic-behavior-registration
    choice: Register GenericEffectBehavior for Disease, ObjectEffect, Environmental types only
    reason: Buff, Debuff, Condition already have specialized behaviors
  - key: template-state-caching
    choice: Cache deserialized EffectState in computed property
    reason: Avoid repeated JSON deserialization on each access

metrics:
  duration: 3.8 min
  completed: 2026-01-28
---

# Phase 19 Plan 02: Effect Templates and Generic Behavior Summary

**One-liner:** CSLA EffectTemplate/EffectTemplateList for data portal access plus GenericEffectBehavior applying EffectState modifiers.

## What Was Built

### EffectTemplate CSLA ReadOnlyBase
Created `GameMechanics/EffectTemplate.cs` - read-only business object for effect templates:
- PropertyInfo pattern for all properties (Id, Name, EffectType, Description, IconName, Color)
- Duration properties: DefaultDurationValue, DurationType
- State properties: StateJson, Tags, IsSystem
- Computed property `State` with cached EffectState deserialization
- Computed property `TagList` splits comma-separated tags to array
- `[Fetch]` operation loads from EffectTemplateDto
- Factory method `GetByIdAsync` for single template lookup

### EffectTemplateList CSLA ReadOnlyListBase
Created `GameMechanics/EffectTemplateList.cs` - read-only list of templates:
- `GetAllAsync` - fetches all active templates
- `GetByTypeAsync(EffectType)` - filters by effect type
- `SearchAsync(string)` - searches by name/tags/description
- Uses IChildDataPortal<EffectTemplate> for child creation
- Delegates to IEffectTemplateDal for data access

### GenericEffectBehavior
Created `GameMechanics/Effects/Behaviors/GenericEffectBehavior.cs` - behavior for EffectState-based effects:
- Implements IEffectBehavior interface
- `GetAttributeModifiers` - returns modifiers from EffectState.AttributeModifiers dictionary
- `GetAbilityScoreModifiers` - applies global ASModifier and skill-specific modifiers
- `OnTick` - applies FatDamagePerTick, VitDamagePerTick, FatHealingPerTick, VitHealingPerTick
- `OnAdding` - allows stacking by default for user-created effects
- Registered in EffectBehaviorFactory for Disease, ObjectEffect, Environmental types

## Decisions Made

1. **GenericEffectBehavior registration** - Only registered for types that don't have specialized behaviors (Disease, ObjectEffect, Environmental). Buff, Debuff, and Condition retain their specialized behaviors (SpellBuffBehavior, DebuffBehavior, ConditionBehavior) which have more complex state handling.

2. **State caching** - EffectTemplate.State property caches the deserialized EffectState to avoid repeated JSON parsing when accessed multiple times during modifier calculation.

3. **Stacking behavior** - GenericEffectBehavior allows stacking by default since template-based effects are user-created and stacking is a common use case.

## Deviations from Plan

None - plan executed exactly as written.

## Commit Log

| Commit | Description |
|--------|-------------|
| 610f995 | feat(19-02): create EffectTemplate CSLA ReadOnlyBase |
| 08ac038 | feat(19-02): create EffectTemplateList CSLA ReadOnlyListBase |
| 037fe8a | feat(19-02): create GenericEffectBehavior for template-based effects |

## Next Phase Readiness

**Ready for UI integration:**
- EffectTemplateList can be fetched via CSLA data portal
- Templates provide all needed display properties
- GenericEffectBehavior handles EffectState modifiers
- Seed templates from 19-01 available for UI testing

**No blockers identified.**

---

*Phase: 19-effect-management | Plan: 02 | Duration: 3.8 min*
