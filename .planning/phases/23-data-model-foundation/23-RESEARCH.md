# Phase 23: Data Model Foundation - Research

**Researched:** 2026-02-01
**Domain:** CSLA.NET business object extension + SQLite JSON storage
**Confidence:** HIGH

## Summary

This phase adds three boolean flags (`IsNpc`, `IsTemplate`, `VisibleToPlayers`) to the existing Character model to enable NPC management features. The research confirms this is a straightforward extension following well-established codebase patterns.

The codebase uses SQLite with JSON serialization for character storage. Adding new properties to the DTO automatically includes them in serialization - no migration scripts are needed for the SQLite implementation. The CSLA business object uses the standard `PropertyInfo<T>` pattern, and `Csla.Data.DataMapper` handles DTO-to-BO mapping automatically for matching property names.

The DAL interface needs two new query methods: `GetNpcTemplatesAsync()` and `GetTableNpcsAsync(Guid tableId)`. These filter characters by the new flags rather than creating new tables.

**Primary recommendation:** Add properties to DTO, then business object, then DAL interface/implementation. All follow existing patterns with no architectural changes required.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CSLA.NET | 9.1.0 | Business object framework | Already in use, manages property state/rules |
| System.Text.Json | (built-in) | JSON serialization | Used by existing DAL for Character storage |
| Microsoft.Data.Sqlite | (latest) | SQLite database access | Current DAL implementation |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| MSTest | 3.x | Unit testing | Test new properties persist correctly |
| Microsoft.Extensions.DI | (latest) | Dependency injection | Already configured in TestBase |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| JSON storage | Explicit columns | More complex, no benefit since JSON already works |
| Separate NPC table | IsNpc flag | Flag approach reuses CharacterEdit, avoids code duplication |

**Installation:**
No new packages needed - all dependencies already in place.

## Architecture Patterns

### Recommended Project Structure

No new files needed for this phase. Modifications to existing files:

```
Threa.Dal/
  Dto/
    Character.cs           # Add 3 bool properties
  ICharacterDal.cs         # Add 2 new query methods

Threa.Dal.SqlLite/
  CharacterDal.cs          # Implement 2 new query methods

GameMechanics/
  CharacterEdit.cs         # Add 3 CSLA properties

GameMechanics.Test/
  CharacterTests.cs        # Add persistence tests for new properties
```

### Pattern 1: CSLA Boolean Property

**What:** Standard pattern for adding boolean properties to CSLA business objects
**When to use:** Any new boolean flag on a business object
**Example:**
```csharp
// Source: Existing CharacterEdit.cs pattern (IsPassedOut, IsPlayable)
public static readonly PropertyInfo<bool> IsNpcProperty = RegisterProperty<bool>(nameof(IsNpc));
public bool IsNpc
{
    get => GetProperty(IsNpcProperty);
    set => SetProperty(IsNpcProperty, value);
}
```

### Pattern 2: DTO Property

**What:** Simple POCO properties that JSON.Serialize handles automatically
**When to use:** Any new data field for storage
**Example:**
```csharp
// Source: Existing Character.cs DTO pattern
public bool IsNpc { get; set; }
public bool IsTemplate { get; set; }
public bool VisibleToPlayers { get; set; } = true;  // Default: visible
```

### Pattern 3: DAL Query Method

**What:** Async method returning filtered character list
**When to use:** Retrieving subsets of characters
**Example:**
```csharp
// Source: Based on existing GetCharactersAsync pattern
public async Task<List<Character>> GetNpcTemplatesAsync()
{
    // Fetch all, deserialize, filter by IsTemplate && IsNpc
    var all = await GetAllCharactersAsync();
    return all.Where(c => c.IsNpc && c.IsTemplate).ToList();
}
```

### Anti-Patterns to Avoid

- **Separate NPC model:** Creating a parallel `NpcEdit` class duplicates all character logic. Use `IsNpc` flag instead.
- **Complex migration:** The JSON storage pattern means no SQL ALTER TABLE needed. Just add properties to DTO.
- **Manual mapping in DataMapper.Map:** Properties with matching names map automatically. Don't add to `mapIgnore` unless special handling needed.

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| DTO serialization | Custom JSON handling | System.Text.Json (automatic) | Already works, adding properties just works |
| Property state tracking | Manual dirty tracking | CSLA PropertyInfo pattern | Framework handles all state management |
| Test database setup | Custom connection logic | TestBase.InitServices() | Established pattern with isolated DBs |
| DTO-BO mapping | Manual property copying | Csla.Data.DataMapper.Map() | Automatic for matching names |

**Key insight:** The existing architecture handles this extension entirely through existing patterns. No new infrastructure needed.

## Common Pitfalls

### Pitfall 1: Forgetting Default Values

**What goes wrong:** VisibleToPlayers defaults to false (C# default), hiding all existing characters
**Why it happens:** Boolean defaults to false in C#
**How to avoid:** Set `VisibleToPlayers { get; set; } = true;` in DTO and initialize to true in business object Create method
**Warning signs:** Existing characters disappear from player views after deployment

### Pitfall 2: Adding to mapIgnore Unnecessarily

**What goes wrong:** New properties don't map between DTO and business object
**Why it happens:** Developer assumes manual handling is needed
**How to avoid:** Only add to mapIgnore for complex types needing special handling. Simple bool maps automatically.
**Warning signs:** Properties always have default values after fetch

### Pitfall 3: Not Testing Persistence Round-Trip

**What goes wrong:** Properties appear to work but don't survive save/fetch cycle
**Why it happens:** JSON serialization issues, property naming mismatch
**How to avoid:** Write explicit test: create, set flags, save, fetch, verify flags
**Warning signs:** Properties reset after page reload

### Pitfall 4: Missing Test Data Update

**What goes wrong:** Seeded test characters don't include new flags
**Why it happens:** TestDataSeeder not updated
**How to avoid:** Add NPC template test data to seeder for integration testing
**Warning signs:** No test data available for later phases

## Code Examples

Verified patterns from codebase analysis:

### Adding Boolean Property to CharacterEdit

```csharp
// Source: S:/src/rdl/threa/GameMechanics/CharacterEdit.cs lines 347-360
// Pattern: Editable boolean property with SetProperty
public static readonly PropertyInfo<bool> IsNpcProperty = RegisterProperty<bool>(nameof(IsNpc));
[Display(Name = "Is NPC")]
public bool IsNpc
{
    get => GetProperty(IsNpcProperty);
    set => SetProperty(IsNpcProperty, value);
}

public static readonly PropertyInfo<bool> IsTemplateProperty = RegisterProperty<bool>(nameof(IsTemplate));
[Display(Name = "Is Template")]
public bool IsTemplate
{
    get => GetProperty(IsTemplateProperty);
    set => SetProperty(IsTemplateProperty, value);
}

public static readonly PropertyInfo<bool> VisibleToPlayersProperty = RegisterProperty<bool>(nameof(VisibleToPlayers));
[Display(Name = "Visible to Players")]
public bool VisibleToPlayers
{
    get => GetProperty(VisibleToPlayersProperty);
    set => SetProperty(VisibleToPlayersProperty, value);
}
```

### Adding Properties to DTO

```csharp
// Source: S:/src/rdl/threa/Threa.Dal/Dto/Character.cs
// Pattern: Simple POCO properties
public bool IsNpc { get; set; }
public bool IsTemplate { get; set; }
public bool VisibleToPlayers { get; set; } = true;  // Default visible
```

### DAL Interface Methods

```csharp
// Source: S:/src/rdl/threa/Threa.Dal/ICharacterDal.cs
// Pattern: Task<List<T>> for queries
Task<List<Character>> GetNpcTemplatesAsync();
Task<List<Character>> GetTableNpcsAsync(Guid tableId);
```

### DAL Implementation

```csharp
// Source: Based on S:/src/rdl/threa/Threa.Dal.SqlLite/CharacterDal.cs
public async Task<List<Character>> GetNpcTemplatesAsync()
{
    try
    {
        // Reuse existing fetch logic, filter in memory
        // JSON storage means all data is already loaded
        var all = await GetAllCharactersAsync();
        return all.Where(c => c.IsNpc && c.IsTemplate).ToList();
    }
    catch (Exception ex)
    {
        throw new OperationFailedException("Error getting NPC templates", ex);
    }
}

public async Task<List<Character>> GetTableNpcsAsync(Guid tableId)
{
    // Need to join with TableCharacters or implement similar pattern
    // For now, this requires integration with ITableDal
    // Implementation depends on how table-character links work
    throw new NotImplementedException("Requires TableDal integration");
}
```

### Unit Test Pattern

```csharp
// Source: S:/src/rdl/threa/GameMechanics.Test/CharacterTests.cs
// Pattern: Create, save, fetch, verify
[TestMethod]
public async Task Character_NpcFlags_PersistToDatabase()
{
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var c = dp.Create(42);

    // Set NPC flags
    c.Name = "Goblin Scout";
    c.Species = "Goblin";
    c.IsNpc = true;
    c.IsTemplate = false;
    c.VisibleToPlayers = false;  // Hidden for surprise

    c = await c.SaveAsync();
    var characterId = c.Id;

    // Fetch and verify
    var fetched = await dp.FetchAsync(characterId);
    Assert.IsTrue(fetched.IsNpc, "IsNpc should persist");
    Assert.IsFalse(fetched.IsTemplate, "IsTemplate should persist");
    Assert.IsFalse(fetched.VisibleToPlayers, "VisibleToPlayers should persist");
}

[TestMethod]
public async Task Character_Template_PersistsCorrectly()
{
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var c = dp.Create(42);

    // Create NPC template
    c.Name = "Goblin Warrior Template";
    c.Species = "Goblin";
    c.IsNpc = true;
    c.IsTemplate = true;
    c.VisibleToPlayers = true;  // Templates visible in library

    c = await c.SaveAsync();
    var characterId = c.Id;

    // Fetch and verify
    var fetched = await dp.FetchAsync(characterId);
    Assert.IsTrue(fetched.IsNpc, "Template IsNpc should persist");
    Assert.IsTrue(fetched.IsTemplate, "Template IsTemplate should persist");
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Separate NPC table (TableNpc) | CharacterEdit with IsNpc flag | This milestone | Full feature parity with PCs |
| Lightweight stat block | Full character model | This milestone | All GM tools work on NPCs |

**Deprecated/outdated:**
- `TableNpc` DTO: Still exists but will be replaced by CharacterEdit+IsNpc pattern
- `ITableDal.AddNpcToTableAsync`: Will link to Character records instead

## Open Questions

### 1. GetTableNpcsAsync Implementation

- **What we know:** Need to return NPCs attached to a specific table
- **What's unclear:** Exact join mechanism with TableCharacters table
- **Recommendation:** Investigate TableDal and TableCharacters table structure in Phase 24. For Phase 23, define the interface method signature only and implement `GetNpcTemplatesAsync` which doesn't need table join.

### 2. Default VisibleToPlayers for Templates

- **What we know:** Templates are used to spawn NPCs, players browse them in library
- **What's unclear:** Should templates themselves be visible?
- **Recommendation:** Default `true` for templates (GM library browsing), spawn with configurable visibility

## Sources

### Primary (HIGH confidence)
- `S:/src/rdl/threa/GameMechanics/CharacterEdit.cs` - Existing property patterns, DataMapper usage
- `S:/src/rdl/threa/Threa.Dal/Dto/Character.cs` - DTO structure, JSON serialization
- `S:/src/rdl/threa/Threa.Dal.SqlLite/CharacterDal.cs` - DAL implementation, JSON storage pattern
- `S:/src/rdl/threa/GameMechanics.Test/CharacterTests.cs` - Test patterns, save/fetch verification

### Secondary (MEDIUM confidence)
- `S:/src/rdl/threa/.planning/research/ARCHITECTURE.md` - Architecture decisions for NPC model

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Using existing libraries, patterns verified in codebase
- Architecture: HIGH - Direct pattern reuse from existing code
- Pitfalls: HIGH - Based on actual codebase analysis, common .NET/CSLA issues

**Research date:** 2026-02-01
**Valid until:** 2026-03-01 (stable patterns, 30 days)

---

## Implementation Checklist

For the planner, these are the verified implementation steps:

1. **DTO Changes** (Threa.Dal/Dto/Character.cs)
   - Add `bool IsNpc { get; set; }`
   - Add `bool IsTemplate { get; set; }`
   - Add `bool VisibleToPlayers { get; set; } = true;`

2. **Business Object Changes** (GameMechanics/CharacterEdit.cs)
   - Add `IsNpcProperty` with `PropertyInfo<bool>` pattern
   - Add `IsTemplateProperty` with `PropertyInfo<bool>` pattern
   - Add `VisibleToPlayersProperty` with `PropertyInfo<bool>` pattern
   - Initialize `VisibleToPlayers = true` in Create method

3. **DAL Interface Changes** (Threa.Dal/ICharacterDal.cs)
   - Add `Task<List<Character>> GetNpcTemplatesAsync();`
   - Add `Task<List<Character>> GetTableNpcsAsync(Guid tableId);`

4. **DAL Implementation** (Threa.Dal.SqlLite/CharacterDal.cs)
   - Implement `GetNpcTemplatesAsync()` - filter by IsNpc && IsTemplate
   - Stub `GetTableNpcsAsync()` - may need Phase 24 for full implementation

5. **Unit Tests** (GameMechanics.Test/CharacterTests.cs)
   - Test IsNpc flag persists
   - Test IsTemplate flag persists
   - Test VisibleToPlayers flag persists (default and explicit values)
   - Test GetNpcTemplatesAsync returns correct results

6. **Test Data** (Optional for Phase 23)
   - Add NPC template to TestDataSeeder for later phase integration tests
