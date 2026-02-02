# Technology Stack for NPC Management

**Project:** Threa TTRPG Assistant - NPC Management System
**Researched:** 2026-02-01
**Dimension:** Stack additions for NPC-specific capabilities

## Executive Summary

The NPC Management System requires **no new external packages**. The existing stack is fully capable of implementing all required features. The key insight is that the project already has:

1. **Character model** - Full stats, equipment, effects, wounds (NPCs share this model)
2. **Template pattern** - Already proven with `ItemTemplate` and `EffectTemplate`
3. **Table-NPC infrastructure** - `TableNpc` DTO and DAL already exist
4. **Real-time messaging** - `InMemoryMessageBus` with Rx.NET for updates
5. **Radzen.Blazor** - DataGrid, dialogs, and batch selection capabilities

The work is primarily **data modeling and UI components**, not package integration.

## Existing Stack (Validated - DO NOT CHANGE)

### Core Framework
| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 10.0 | Runtime and framework |
| CSLA.NET | 9.1.0 | Business objects, data portal |
| Blazor Web App | SSR + InteractiveServer | UI rendering |

### Data Layer
| Technology | Version | Purpose |
|------------|---------|---------|
| Microsoft.Data.Sqlite | 10.0.1 | Database access |
| System.Text.Json | (built-in) | DTO serialization in SQLite |

### UI Components
| Technology | Version | Purpose |
|------------|---------|---------|
| Radzen.Blazor | 8.4.2 | DataGrid, dialogs, form controls |
| Bootstrap 5 | (CSS) | Layout and styling |

### Messaging
| Technology | Version | Purpose |
|------------|---------|---------|
| System.Reactive | 6.0.1 | Rx.NET for pub/sub messaging |

## Recommended Stack Additions: NONE

**Rationale:** All NPC management features can be built with existing stack:

| Feature | How Existing Stack Handles It |
|---------|-------------------------------|
| NPC Templates | Same pattern as `ItemTemplateEdit` / `EffectTemplate` |
| Quick NPC Creation | CSLA `[Create]` with template parameter (like `CharacterEdit.Create(playerId, species)`) |
| Visibility Toggle | Simple boolean property on `TableNpc` + conditional rendering |
| Group Management | `List<Guid>` grouping + LINQ batch operations |
| Batch Actions | Radzen DataGrid `SelectionMode="DataGridSelectionMode.Multiple"` |
| Real-time Updates | Existing `CharacterUpdateMessage` or new `NpcUpdateMessage` |
| Persistence Choice | Nullable `CharacterTemplateId` on `TableNpc` (already exists) |

## Existing Infrastructure to Leverage

### 1. Template Pattern (Already Proven)

The codebase has a well-established template pattern:

```csharp
// Read-only template (library item)
public class EffectTemplate : ReadOnlyBase<EffectTemplate>

// Editable template (for GM creation)
public class ItemTemplateEdit : BusinessBase<ItemTemplateEdit>

// Instance created from template
var character = await portal.CreateAsync(playerId, speciesInfo);
```

**For NPCs:**
- `NpcTemplate` - ReadOnly for library browsing
- `NpcTemplateEdit` - Editable for GM template creation
- `TableNpc` extended - Instance with optional template reference

### 2. TableNpc DTO (Already Exists)

```csharp
// From Threa.Dal\Dto\GameTable.cs
public class TableNpc
{
    public Guid Id { get; set; }
    public Guid TableId { get; set; }
    public string Name { get; set; }
    public int VitValue { get; set; }
    public int VitBaseValue { get; set; }
    public int FatValue { get; set; }
    public int FatBaseValue { get; set; }
    public int ActionPointMax { get; set; }
    public int ActionPointAvailable { get; set; }
    public int? CharacterTemplateId { get; set; }  // Template reference!
    public string StatsJson { get; set; }  // Extensible JSON blob
}
```

**Extensions needed:**
- `bool IsVisible` - Player visibility
- `string? GroupName` - Group membership
- `int? NpcTemplateId` - Reference to NPC template (distinct from character)

### 3. ITableDal NPC Operations (Already Exists)

```csharp
// From Threa.Dal\ITableDal.cs
Task<TableNpc> AddNpcToTableAsync(TableNpc npc);
Task UpdateTableNpcAsync(TableNpc npc);
Task RemoveNpcFromTableAsync(Guid npcId);
Task<List<TableNpc>> GetTableNpcsAsync(Guid tableId);
```

**No new DAL interfaces needed** - just extend `TableNpc` DTO.

### 4. Messaging Infrastructure (Already Exists)

```csharp
// From GameMechanics.Messaging.InMemory\InMemoryMessageBus.cs
public class InMemoryMessageBus : IDisposable
{
    // Existing message types
    private readonly Subject<CharacterUpdateMessage> _characterUpdates = new();
    private readonly Subject<TableUpdateMessage> _tableUpdates = new();

    // Pattern for adding new message type:
    // private readonly Subject<NpcUpdateMessage> _npcUpdates = new();
}
```

### 5. Radzen DataGrid (Already In Use)

```razor
<!-- Pattern from existing pages -->
<RadzenDataGrid Data="@items"
                TItem="ItemType"
                SelectionMode="DataGridSelectionMode.Multiple"
                @bind-Value="@selectedItems">
    <Columns>
        <RadzenDataGridColumn TItem="ItemType" Title="Select" Width="50px">
            <Template>
                <RadzenCheckBox @bind-Value="@context.IsSelected" />
            </Template>
        </RadzenDataGridColumn>
    </Columns>
</RadzenDataGrid>
```

## Specific Implementation Guidance

### NPC Template CSLA Objects

Follow existing patterns exactly:

```csharp
// New file: GameMechanics/Npc/NpcTemplate.cs
[Serializable]
public class NpcTemplate : ReadOnlyBase<NpcTemplate>
{
    // Same PropertyInfo pattern as EffectTemplate
    public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id));
    public int Id
    {
        get => GetProperty(IdProperty);
        private set => LoadProperty(IdProperty, value);
    }

    // Stats snapshot
    public static readonly PropertyInfo<string> StatsJsonProperty = ...

    [Fetch]
    private void Fetch(NpcTemplateDto dto) { ... }
}
```

### TableNpc Extensions

Extend existing DTO (no breaking changes):

```csharp
// Threa.Dal\Dto\GameTable.cs - extend TableNpc
public class TableNpc
{
    // Existing properties...

    // NEW: Visibility control
    public bool IsVisible { get; set; } = true;

    // NEW: Group management
    public string? GroupName { get; set; }

    // NEW: Template reference (separate from CharacterTemplateId)
    public int? NpcTemplateId { get; set; }
}
```

### Group Batch Operations

Use LINQ with existing infrastructure:

```csharp
// Service method pattern
public async Task ApplyToGroupAsync(Guid tableId, string groupName, Action<TableNpc> action)
{
    var npcs = await _tableDal.GetTableNpcsAsync(tableId);
    var group = npcs.Where(n => n.GroupName == groupName);
    foreach (var npc in group)
    {
        action(npc);
        await _tableDal.UpdateTableNpcAsync(npc);
    }
    _messageBus.PublishTableUpdate(new TableUpdateMessage { TableId = tableId });
}
```

## What NOT to Add

| Tempting Addition | Why NOT to Add |
|-------------------|----------------|
| SignalR | Rx.NET messaging already handles real-time updates via Blazor Server circuits |
| Entity Framework | SQLite with JSON serialization is simpler and already working |
| MediatR | CSLA data portal + Rx.NET messaging provides same patterns |
| AutoMapper | CSLA's `DataMapper` and manual mapping is sufficient |
| Separate NPC database | NPCs are table-scoped; `TableNpcs` table already exists |
| Redis/caching | In-memory state via Blazor Server is sufficient for session-scoped NPCs |

## Database Schema Extensions

Minimal changes to existing schema:

```sql
-- TableNpcs table already exists with JSON blob storage
-- Extensions are handled via JSON in StatsJson column or by adding columns:

-- Option 1: Add columns (cleaner queries)
ALTER TABLE TableNpcs ADD COLUMN IsVisible INTEGER DEFAULT 1;
ALTER TABLE TableNpcs ADD COLUMN GroupName TEXT;
ALTER TABLE TableNpcs ADD COLUMN NpcTemplateId INTEGER;

-- Option 2: Keep in JSON (no migration needed)
-- StatsJson already supports arbitrary properties

-- NEW TABLE: NPC Templates (same pattern as EffectTemplates)
CREATE TABLE IF NOT EXISTS NpcTemplates (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    Name TEXT NOT NULL,
    Description TEXT,
    Category TEXT,
    DifficultyTier TEXT,
    BaseStatsJson TEXT NOT NULL,
    IsSystem INTEGER DEFAULT 0,
    IsActive INTEGER DEFAULT 1,
    CreatedAt TEXT DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TEXT
);
```

## Confidence Assessment

| Aspect | Confidence | Rationale |
|--------|------------|-----------|
| No new packages needed | HIGH | Existing stack fully covers requirements |
| Template pattern | HIGH | Proven pattern in codebase (Items, Effects) |
| TableNpc extensions | HIGH | DTO is already JSON-serialized, extensible |
| Group batch operations | HIGH | LINQ + existing DAL is sufficient |
| Real-time updates | HIGH | Rx.NET messaging already in use |

## Sources

All findings are from direct codebase analysis:
- `S:\src\rdl\threa\Threa\Threa\Threa.csproj` - Package references
- `S:\src\rdl\threa\GameMechanics\EffectTemplate.cs` - Template pattern
- `S:\src\rdl\threa\GameMechanics\Items\ItemTemplateEdit.cs` - Editable template pattern
- `S:\src\rdl\threa\Threa.Dal\Dto\GameTable.cs` - Existing TableNpc DTO
- `S:\src\rdl\threa\Threa.Dal\ITableDal.cs` - Existing NPC DAL operations
- `S:\src\rdl\threa\GameMechanics.Messaging.InMemory\InMemoryMessageBus.cs` - Messaging pattern
- `S:\src\rdl\threa\Threa.Dal.SqlLite\TableDal.cs` - Existing NPC persistence
