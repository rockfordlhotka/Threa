# Phase 1: Foundation - Research

**Researched:** 2026-01-24
**Domain:** CSLA .NET Business Objects, DAL Implementation, Seed Data
**Confidence:** HIGH

## Summary

Phase 1 establishes the foundation for the equipment/item system by implementing CSLA business objects for ItemTemplate and CharacterItem, plus populating the database with seed test data. The research found that substantial infrastructure already exists:

1. **DAL interfaces exist** (`IItemTemplateDal`, `ICharacterItemDal`) with comprehensive method signatures
2. **Mock and SQLite implementations exist** for both DAL interfaces
3. **ItemTemplateEdit already exists** with full CRUD operations and property definitions
4. **ItemTemplateList/Info read-only objects exist** following CSLA patterns
5. **Seed data already exists** in MockDb.cs with ~40 item templates covering weapons, armor, containers, consumables, ammunition

**Primary recommendation:** Create CharacterItemEdit business object following existing patterns, add validation rules to ItemTemplateEdit, augment seed data to meet DATA-01 through DATA-07 requirements, and add unit tests.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CSLA.NET | 9.1.0 | Business object framework | Project standard, all BOs use CSLA |
| Microsoft.Data.Sqlite | (project) | SQLite database access | Project standard for persistence |
| System.Text.Json | (built-in) | JSON serialization for CustomProperties | Already used in SQLite DAL |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| MSTest | (project) | Unit testing | All tests use MSTest framework |
| Csla.Configuration | 9.1.0 | DI/IoC for CSLA | Test initialization |

**Installation:**
No additional packages needed - all dependencies already in project.

## Architecture Patterns

### Recommended Project Structure
```
GameMechanics/
├── Items/
│   ├── ItemTemplateEdit.cs       # EXISTS - Edit business object
│   ├── ItemTemplateList.cs       # EXISTS - Read-only list
│   ├── ItemTemplateInfo.cs       # EXISTS - Read-only info
│   ├── CharacterItemEdit.cs      # TO CREATE - Edit business object
│   ├── CharacterItemList.cs      # TO CREATE - Read-only list
│   └── CharacterItemInfo.cs      # TO CREATE - Read-only info
│
Threa.Dal/
├── IItemDal.cs                   # EXISTS - Contains both interfaces
├── Dto/
│   ├── ItemTemplate.cs           # EXISTS - DTO class
│   ├── CharacterItem.cs          # EXISTS - DTO class
│   ├── ItemType.cs               # EXISTS - Enum
│   ├── WeaponType.cs             # EXISTS - Enum
│   └── EquipmentSlot.cs          # EXISTS - Enum
│
Threa.Dal.MockDb/
├── MockDb.cs                     # EXISTS - Contains seed data
├── ItemTemplateDal.cs            # EXISTS - Mock implementation
└── CharacterItemDal.cs           # EXISTS - Mock implementation
│
GameMechanics.Test/
└── ItemTests.cs                  # TO CREATE - Unit tests
```

### Pattern 1: CSLA BusinessBase Edit Pattern
**What:** Standard editable business object inheriting from `BusinessBase<T>`
**When to use:** For objects that need CRUD operations (ItemTemplateEdit, CharacterItemEdit)
**Example:**
```csharp
// Source: Existing CharacterEdit.cs and ItemTemplateEdit.cs patterns
[Serializable]
public class CharacterItemEdit : BusinessBase<CharacterItemEdit>
{
    // Static PropertyInfo pattern for all properties
    public static readonly PropertyInfo<Guid> IdProperty = RegisterProperty<Guid>(nameof(Id));
    public Guid Id
    {
        get => GetProperty(IdProperty);
        private set => LoadProperty(IdProperty, value);
    }

    // [Create], [Fetch], [Insert], [Update], [Delete] methods
    // with [Inject] for DAL dependencies
}
```

### Pattern 2: CSLA ReadOnlyBase Info Pattern
**What:** Read-only business object for display/lists
**When to use:** List items, lookup data, display-only scenarios
**Example:**
```csharp
// Source: Existing ItemTemplateInfo.cs
[Serializable]
public class CharacterItemInfo : ReadOnlyBase<CharacterItemInfo>
{
    // LoadProperty for all properties (private setters)

    [FetchChild]
    private void Fetch(CharacterItem dto)
    {
        LoadProperty(IdProperty, dto.Id);
        // ... load all properties
    }
}
```

### Pattern 3: CSLA ReadOnlyListBase List Pattern
**What:** Read-only list of Info objects
**When to use:** Fetching collections for display
**Example:**
```csharp
// Source: Existing ItemTemplateList.cs
[Serializable]
public class CharacterItemList : ReadOnlyListBase<CharacterItemList, CharacterItemInfo>
{
    [Fetch]
    private async Task Fetch(int characterId,
        [Inject] ICharacterItemDal dal,
        [Inject] IChildDataPortal<CharacterItemInfo> childPortal)
    {
        var items = await dal.GetCharacterItemsAsync(characterId);
        using (LoadListMode)
        {
            foreach (var item in items)
            {
                Add(childPortal.FetchChild(item));
            }
        }
    }
}
```

### Pattern 4: Validation Rules
**What:** Business rules for property validation
**When to use:** Required fields, value constraints, cross-field validation
**Example:**
```csharp
// Source: Existing SkillDefinitionEdit.cs
protected override void AddBusinessRules()
{
    base.AddBusinessRules();

    // Use data annotation attributes for simple rules
    // [Required] on NameProperty

    // Custom rules for complex validation
    BusinessRules.AddRule(new RequiredRule(NameProperty));
    BusinessRules.AddRule(new MinValueRule(WeightProperty, 0));
}
```

### Anti-Patterns to Avoid
- **Don't use SetProperty for identity fields:** Use `LoadProperty` with private setter for Id fields
- **Don't call DAL directly in properties:** All data access through data portal methods
- **Don't mix Edit and ReadOnly patterns:** Edit for mutation, ReadOnly for display

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Property change tracking | Manual dirty flags | CSLA PropertyInfo pattern | Built-in change tracking, undo support |
| Validation | Custom validation code | CSLA BusinessRules + DataAnnotations | Integrated with UI binding, rule engine |
| Object creation | new() constructor | IDataPortal.Create() | DI injection, initialization rules |
| List filtering | LINQ in UI | Criteria-based Fetch | Server-side filtering, better performance |
| Soft delete | Boolean flag + manual checks | `IsActive` property + `DeactivateAsync()` | Already implemented in DAL |

**Key insight:** The CSLA framework provides all infrastructure for business objects. The codebase already follows these patterns consistently - follow them exactly.

## Common Pitfalls

### Pitfall 1: Incorrect Property Pattern for Identity Fields
**What goes wrong:** Using `SetProperty` for Id allows modification after creation
**Why it happens:** Copy-paste from editable property pattern
**How to avoid:** Always use `LoadProperty` with private setter for identity fields
**Warning signs:** Id changes after save, duplicate records created

### Pitfall 2: Missing BypassPropertyChecks in Data Operations
**What goes wrong:** Property setters trigger validation during load/save
**Why it happens:** Forgetting that SetProperty triggers rules
**How to avoid:** Wrap all LoadProperty calls in `using (BypassPropertyChecks)`
**Warning signs:** Validation errors during Fetch, infinite loops

### Pitfall 3: Forgetting BusinessRules.CheckRules() After Load
**What goes wrong:** Object appears valid but has pending validation errors
**Why it happens:** LoadProperty doesn't trigger rules automatically
**How to avoid:** Call `BusinessRules.CheckRules()` at end of Create/Fetch methods
**Warning signs:** IsSavable returns true when it shouldn't

### Pitfall 4: Inconsistent Soft vs Hard Delete
**What goes wrong:** Data integrity issues, orphaned records
**Why it happens:** Mixing delete strategies without clear policy
**How to avoid:** Per CONTEXT.md: Use `Deactivate()` for soft delete (IsActive=false), `Delete()` for hard delete. Templates soft-delete, items can hard-delete.
**Warning signs:** "deleted" items still appearing, FK violations

### Pitfall 5: Not Handling Nullable Template Reference
**What goes wrong:** NullReferenceException when CharacterItem.Template is null
**Why it happens:** Template may not load if template was deleted/deactivated
**How to avoid:** Always null-check Template before accessing properties
**Warning signs:** Runtime exceptions when displaying items with deleted templates

## Code Examples

Verified patterns from existing codebase:

### Data Portal Create Pattern
```csharp
// Source: ItemTemplateEdit.cs lines 230-269
[Create]
private async Task Create()
{
    using (BypassPropertyChecks)
    {
        Id = 0;  // or Guid.NewGuid() for GUID-based
        Name = string.Empty;
        // ... initialize all properties to defaults
        IsActive = true;
    }
    BusinessRules.CheckRules();
    await Task.CompletedTask;
}
```

### Data Portal Fetch Pattern
```csharp
// Source: ItemTemplateEdit.cs lines 271-277
[Fetch]
private async Task Fetch(int id, [Inject] IItemTemplateDal dal)
{
    var data = await dal.GetTemplateAsync(id)
        ?? throw new InvalidOperationException($"ItemTemplate {id} not found");
    LoadFromDto(data);
}

private void LoadFromDto(ItemTemplate data)
{
    using (BypassPropertyChecks)
    {
        Id = data.Id;
        Name = data.Name;
        // ... load all properties
    }
    BusinessRules.CheckRules();
}
```

### Data Portal Insert/Update Pattern
```csharp
// Source: ItemTemplateEdit.cs lines 318-401
[Insert]
private async Task Insert([Inject] IItemTemplateDal dal)
{
    var dto = new ItemTemplate
    {
        Name = Name,
        // ... map all properties to DTO
    };
    var result = await dal.SaveTemplateAsync(dto);
    using (BypassPropertyChecks)
    {
        Id = result.Id;
    }
}

[Update]
private async Task Update([Inject] IItemTemplateDal dal)
{
    var dto = new ItemTemplate
    {
        Id = Id,
        Name = Name,
        // ... map all properties to DTO
    };
    await dal.SaveTemplateAsync(dto);
}
```

### Data Portal Delete Pattern (Soft Delete)
```csharp
// Source: ItemTemplateEdit.cs lines 403-407
[Delete]
private async Task Delete(int id, [Inject] IItemTemplateDal dal)
{
    await dal.DeactivateTemplateAsync(id);
}
```

### Unit Test Service Setup Pattern
```csharp
// Source: CharacterSaveLoadTest.cs lines 14-20
private ServiceProvider InitServices()
{
    IServiceCollection services = new ServiceCollection();
    services.AddCsla();
    services.AddMockDb();
    return services.BuildServiceProvider();
}

[TestMethod]
public async Task SomeTest()
{
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<ItemTemplateEdit>>();
    var item = dp.Create();
    // ... test logic
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Manual property tracking | CSLA PropertyInfo | CSLA 9+ | Simplified code, better DI support |
| Attribute-based DataPortal | Method-based with [Create]/[Fetch] etc. | CSLA 8+ | More flexible, better async support |
| Constructor injection | [Inject] attribute | CSLA 9+ | Cleaner, more explicit dependencies |

**Deprecated/outdated:**
- `ObjectFactory` pattern: Replaced by attribute-based operations
- `PropertyManager` manual calls: Use `RegisterProperty<T>` static pattern

## Existing Code Analysis

### What Already Exists (DO NOT RECREATE)

| Component | Location | Status | Notes |
|-----------|----------|--------|-------|
| IItemTemplateDal | Threa.Dal/IItemDal.cs | Complete | All methods defined |
| ICharacterItemDal | Threa.Dal/IItemDal.cs | Complete | All methods defined |
| ItemTemplate DTO | Threa.Dal/Dto/ItemTemplate.cs | Complete | All properties including bonuses/effects |
| CharacterItem DTO | Threa.Dal/Dto/CharacterItem.cs | Complete | All properties including Template reference |
| MockDb ItemTemplateDal | Threa.Dal.MockDb/ItemTemplateDal.cs | Complete | Full implementation |
| MockDb CharacterItemDal | Threa.Dal.MockDb/CharacterItemDal.cs | Complete | Full implementation with equip/container logic |
| SQLite ItemTemplateDal | Threa.Dal.SqlLite/ItemTemplateDal.cs | Complete | JSON serialization pattern |
| SQLite CharacterItemDal | Threa.Dal.SqlLite/CharacterItemDal.cs | Complete | JSON serialization pattern |
| ItemTemplateEdit | GameMechanics/Items/ItemTemplateEdit.cs | Complete | Full CRUD, needs validation rules |
| ItemTemplateList | GameMechanics/Items/ItemTemplateList.cs | Complete | Read-only list |
| ItemTemplateInfo | GameMechanics/Items/ItemTemplateInfo.cs | Complete | Read-only info |
| Seed data | Threa.Dal.MockDb/MockDb.cs | Partial | Has items but needs augmentation for requirements |

### What Needs to Be Created

| Component | Location | Priority | Dependencies |
|-----------|----------|----------|--------------|
| CharacterItemEdit | GameMechanics/Items/CharacterItemEdit.cs | High | Existing DTOs and DAL |
| CharacterItemList | GameMechanics/Items/CharacterItemList.cs | Medium | CharacterItemInfo |
| CharacterItemInfo | GameMechanics/Items/CharacterItemInfo.cs | Medium | CharacterItem DTO |
| ItemTemplateEdit validation rules | GameMechanics/Items/ItemTemplateEdit.cs | High | Existing class |
| Seed data augmentation | Threa.Dal.MockDb/MockDb.cs | High | DATA-01 to DATA-07 requirements |
| Unit tests | GameMechanics.Test/ItemTests.cs | High | All business objects |

### Existing Seed Data Gap Analysis

Current seed data in MockDb.CreateItemTemplates():

| Requirement | Current State | Gap |
|-------------|---------------|-----|
| DATA-01: 3-5 melee weapons | Longsword (10), Enchanted Longsword (11), Battle Axe (12), Dagger (13) | Need 1 more melee weapon variety |
| DATA-02: 3-5 ranged weapons | Shortbow (14) | Need 2-4 more ranged weapons (bows, crossbows) |
| DATA-03: 3-5 firearms | 9mm Pistol (100) | Need 2-4 more firearms (rifles, SMG, shotgun) |
| DATA-04: 2-3 armor pieces | Leather Armor (20), Chain Mail (21), Steel Helmet (22), Wooden Shield (23) | Met |
| DATA-05: 2-3 ammo types | 9mm FMJ (101), 9mm HP (102), Arrow (104) | Met |
| DATA-06: 1-2 containers | Large Backpack (30), Belt Pouch (31), Quiver (32), Bag of Holding (33) | Met |
| DATA-07: 1-2 consumables | Health Potion (50), Stamina Potion (51) | Met |

**Seed data additions needed:**
1. Add 1 more melee weapon (mace, spear, or staff)
2. Add 2-4 more traditional ranged weapons (longbow, crossbow, throwing knives)
3. Add 3-4 more sci-fi firearms matching RANGED_WEAPONS_SCIFI.md (rifle, shotgun, SMG, energy weapon)

## Open Questions

Things that couldn't be fully resolved:

1. **Child Collections for Bonuses/Effects**
   - What we know: ItemTemplate DTO has `SkillBonuses`, `AttributeModifiers`, `Effects` lists
   - What's unclear: Whether to expose as child business lists in ItemTemplateEdit or keep as simple DTOs
   - Recommendation: Keep as JSON CustomProperties for v1 (bonuses/effects deferred per CONTEXT.md), expose full child collections in future phases

2. **CharacterItemEdit Parent Navigation**
   - What we know: CharacterItem DTO has OwnerCharacterId
   - What's unclear: Whether to include CharacterId in business object for query convenience
   - Recommendation: Include CharacterId as read-only property set during Create, enables list filtering by character

3. **Fetch Criteria Patterns**
   - What we know: Current DAL methods use simple ID parameters
   - What's unclear: Whether to add criteria objects for complex queries (by type, by character, etc.)
   - Recommendation: Use simple parameters for Phase 1, add criteria in Phase 2 if UI query patterns require it

## Sources

### Primary (HIGH confidence)
- S:\src\rdl\threa\GameMechanics\Items\ItemTemplateEdit.cs - Existing CRUD pattern
- S:\src\rdl\threa\GameMechanics\CharacterEdit.cs - Complex business object patterns
- S:\src\rdl\threa\Threa.Dal\IItemDal.cs - DAL interface definitions
- S:\src\rdl\threa\Threa.Dal.MockDb\MockDb.cs - Seed data patterns
- S:\src\rdl\threa\GameMechanics.Test\CharacterSaveLoadTest.cs - Test patterns

### Secondary (MEDIUM confidence)
- S:\src\rdl\threa\design\DATABASE_DESIGN.md - Schema design
- S:\src\rdl\threa\design\EQUIPMENT_SYSTEM.md - Equipment rules
- S:\src\rdl\threa\design\RANGED_WEAPONS_SCIFI.md - Firearm categories

### Tertiary (LOW confidence)
- None required - all patterns verified from existing codebase

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All patterns verified from existing code
- Architecture: HIGH - Matches existing CSLA patterns exactly
- Pitfalls: HIGH - Based on CSLA best practices and existing code review
- Existing code analysis: HIGH - Direct file inspection

**Research date:** 2026-01-24
**Valid until:** 2026-02-24 (stable patterns, unlikely to change)
