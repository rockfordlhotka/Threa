---
phase: 01-foundation
plan: 01
subsystem: items
tags: [csla, validation, testing, item-templates]
dependency-graph:
  requires: []
  provides: [ItemTemplateEdit-validation, ItemTemplate-unit-tests]
  affects: [02-01, 02-02, 02-03]
tech-stack:
  added: []
  patterns: [CSLA-BusinessRules, MSTest-async-patterns]
key-files:
  created:
    - GameMechanics.Test/ItemTemplateTests.cs
  modified:
    - GameMechanics/Items/ItemTemplateEdit.cs
decisions:
  - id: validation-approach
    summary: Used CSLA CommonRules (Required, MinValue) plus custom rules for ItemType and container warnings
  - id: container-warning
    summary: Container capacity validation is a warning, not error, per CONTEXT.md (GM flexibility)
metrics:
  duration: 5m 35s
  completed: 2026-01-25
---

# Phase 01 Plan 01: ItemTemplate Validation & Tests Summary

**One-liner:** Added CSLA validation rules to ItemTemplateEdit (Name/ItemType required, Weight/Volume non-negative, container capacity warning) plus 11 comprehensive unit tests covering CRUD and validation.

## What Was Built

### ItemTemplateEdit Validation Rules

Added `AddBusinessRules()` override with:

1. **Required field validation**
   - `Name`: Required using `Csla.Rules.CommonRules.Required`
   - `ItemType`: Required (non-default) using custom `ItemTypeRequiredRule`

2. **Numeric range validation**
   - `Weight`: >= 0 using `Csla.Rules.CommonRules.MinValue<decimal>`
   - `Volume`: >= 0 using `Csla.Rules.CommonRules.MinValue<decimal>`

3. **Container capacity warning**
   - Custom `ContainerCapacityWarningRule` - when `IsContainer=true`, warns if no capacity defined
   - Per CONTEXT.md: Warning not error, allows GM flexibility for "magic bag of holding" scenarios

### Unit Tests (ItemTemplateTests.cs)

**CRUD Tests (4 tests):**
- `ItemTemplateEdit_Create_InitializesDefaults` - Verifies default values on create
- `ItemTemplateEdit_Fetch_LoadsExistingTemplate` - Fetches Longsword (Id=10), verifies properties
- `ItemTemplateEdit_SaveAndFetch_PropertiesPersist` - Round-trip save/fetch verification
- `ItemTemplateEdit_Update_ChangesPersist` - Modify and verify changes persist

**Validation Tests (7 tests):**
- `ItemTemplateEdit_Validation_NameRequired` - Empty name = not savable
- `ItemTemplateEdit_Validation_ItemTypeRequired` - Default ItemType validation
- `ItemTemplateEdit_Validation_WeightNonNegative` - Negative weight fails
- `ItemTemplateEdit_Validation_VolumeNonNegative` - Negative volume fails
- `ItemTemplateEdit_Validation_ValidTemplateIsSavable` - Valid template passes
- `ItemTemplateEdit_Validation_ContainerWarning` - Container without capacity gets warning
- `ItemTemplateEdit_Validation_ContainerWithCapacityNoWarning` - Container with capacity, no warning

## Implementation Details

### Custom Validation Rules Created

**ItemTypeRequiredRule:**
```csharp
// Validates ItemType is not default enum value (must be explicitly set)
public class ItemTypeRequiredRule : BusinessRule
```

**ContainerCapacityWarningRule:**
```csharp
// Warning when container has no capacity - per CONTEXT.md GM flexibility
public class ContainerCapacityWarningRule : BusinessRule
```

### Test Pattern Used

Following existing `CharacterSaveLoadTest.cs` pattern:
- `InitServices()` method with `AddCsla()` and `AddMockDb()`
- `IDataPortal<ItemTemplateEdit>` injection
- Async tests with proper assertions
- MockDb provides seed data (Longsword Id=10, etc.)

## Commits

| Hash | Type | Description |
|------|------|-------------|
| dc7238a | feat | Add validation rules to ItemTemplateEdit |
| e950b52 | test | Add unit tests for ItemTemplateEdit |

## Deviations from Plan

None - plan executed exactly as written.

## Verification Results

1. `dotnet build Threa.sln` - Passed (1 pre-existing warning unrelated to changes)
2. `dotnet test --filter "FullyQualifiedName~ItemTemplateTests"` - All 11 tests pass

## Next Phase Readiness

**Ready for:** Plan 02 (CharacterItemEdit validation and tests)

**Dependencies satisfied:**
- ItemTemplateEdit now has proper validation rules
- Test infrastructure pattern established
- MockDb contains test data (Longsword, Backpack, Health Potion, etc.)

**No blockers identified.**
