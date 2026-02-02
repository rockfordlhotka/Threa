# Architecture Patterns: NPC Management Integration

**Domain:** TTRPG NPC Management System
**Researched:** 2026-02-01
**Confidence:** HIGH (based on thorough codebase analysis)

## Executive Summary

The Threa codebase already has a **foundation for NPCs** in place:
- `TableNpc` DTO exists in `Threa.Dal.Dto.GameTable`
- `ITableDal` has full NPC CRUD operations implemented
- `TableNpcs` SQLite table exists with JSON storage
- `NpcPlaceholder.razor` shows intended placement in GM dashboard

The key architectural decision is whether to:
1. **Use the lightweight `TableNpc` model** (JSON blob stats, table-scoped) - Quick to implement, limited features
2. **Reuse `CharacterEdit` model** (full CSLA business object) - Maximum feature parity with PCs

**Recommendation:** Use `CharacterEdit` with an `IsNpc` flag. This provides:
- 100% feature parity with player characters (same weapons, effects, wounds work)
- All existing GM manipulation tools work unchanged
- Single code path for time advancement, combat, effects
- Trade-off: Slightly more storage, but templates solve creation speed

## Current Architecture Analysis

### Existing NPC Foundation

```
Threa.Dal.Dto.GameTable.cs:
├── TableNpc class (lines 102-121)
│   ├── Id: Guid
│   ├── TableId: Guid
│   ├── Name: string
│   ├── VitValue, VitBaseValue, FatValue, FatBaseValue
│   ├── ActionPointMax, ActionPointAvailable
│   ├── CharacterTemplateId: int? (optional link to full character)
│   └── StatsJson: string (JSON blob for additional data)

ITableDal interface:
├── AddNpcToTableAsync(TableNpc)
├── UpdateTableNpcAsync(TableNpc)
├── RemoveNpcFromTableAsync(Guid)
└── GetTableNpcsAsync(Guid tableId)

TableDal.cs implementation:
└── Full CRUD via JSON serialization in TableNpcs table
```

**Problem with current `TableNpc`:** The "simplified stat block" approach means:
- No effects/wounds (different model than CharacterEffect)
- No skills (can't resolve ability checks)
- No equipment (can't use weapons/armor)
- Different damage/healing logic needed
- GM manipulation tools won't work

### Recommended Architecture

Use `CharacterEdit` with `IsNpc` flag for full feature parity.

```
Characters Table (enhanced):
├── Id: int (PK)
├── PlayerId: int (FK) - NULL for NPCs
├── IsNpc: bool (NEW)
├── IsTemplate: bool (NEW) - For NPC templates
├── VisibleToPlayers: bool (NEW) - For surprise encounters
├── [All existing character fields...]
└── Effects, Skills, Items, etc.

TableNpcs Table (enhanced):
├── TableId: Guid
├── CharacterId: int (FK to Characters)
└── SortOrder: int (for grouping display)
```

## Component Boundaries

### Layer 1: Data Access (Threa.Dal)

| Component | Responsibility | Changes Needed |
|-----------|---------------|----------------|
| `Character` DTO | Character data storage | Add `IsNpc`, `IsTemplate`, `VisibleToPlayers` flags |
| `ICharacterDal` | Character CRUD | Add `GetNpcTemplatesAsync()`, `GetTableNpcsAsync(tableId)` |
| `TableNpc` DTO | Table-NPC association | Simplify to just TableId + CharacterId + SortOrder |
| `ITableDal` | Table-NPC linking | Already has NPC methods, just change semantics |

### Layer 2: Business Logic (GameMechanics)

| Component | Responsibility | Changes Needed |
|-----------|---------------|----------------|
| `CharacterEdit` | Character business object | Add `IsNpc`, `IsTemplate`, `VisibleToPlayers` properties |
| `NpcTemplate` | Template instantiation | **NEW** - Creates NPC instance from template |
| `NpcTemplateList` | Browse/search templates | **NEW** - CSLA ReadOnlyListBase |
| `TableNpcInfo` | NPC display in dashboard | **NEW** - Similar to TableCharacterInfo |
| `TableNpcList` | NPCs at a table | **NEW** - CSLA ReadOnlyListBase |
| `TimeAdvancementService` | Round/time processing | Extend to include NPCs at table |

### Layer 3: Presentation (Threa.Client)

| Component | Responsibility | Changes Needed |
|-----------|---------------|----------------|
| `GmTable.razor` | GM dashboard | Add NPC section, replace NpcPlaceholder |
| `NpcSection.razor` | NPC display area | **NEW** - Collapsible section with NPC cards |
| `NpcStatusCard.razor` | Individual NPC card | **NEW** - Similar to CharacterStatusCard |
| `NpcDetailModal.razor` | NPC manipulation | **NEW** or reuse CharacterDetailModal |
| `NpcTemplateLibrary.razor` | Template browser | **NEW** - GM page for template management |
| `NpcQuickCreate.razor` | Quick NPC creation | **NEW** - Modal for during-play creation |

### Layer 4: Messaging (GameMechanics.Messaging)

| Component | Responsibility | Changes Needed |
|-----------|---------------|----------------|
| `CharacterUpdateMessage` | Real-time updates | Already works for NPCs (same character model) |
| `NpcVisibilityMessage` | Visibility toggles | **NEW** - For reveal/hide mechanics |

## Data Flow Diagrams

### NPC Creation from Template

```
GM clicks "Add NPC from Template"
    │
    ▼
NpcTemplateLibrary modal opens
    │
    ▼
GM selects template (e.g., "Goblin Warrior")
    │
    ▼
NpcTemplate.InstantiateAsync(templateId, tableId)
    │
    ├─► Fetch template CharacterEdit (IsTemplate=true)
    │
    ├─► Clone to new CharacterEdit (IsNpc=true, IsTemplate=false)
    │
    ├─► Insert new character via ICharacterDal
    │
    ├─► Create TableNpc link via ITableDal
    │
    ▼
Publish CharacterUpdateMessage
    │
    ▼
GmTable refreshes NPC list
```

### Quick NPC Creation

```
GM clicks "Quick Add NPC"
    │
    ▼
NpcQuickCreate modal opens
    │   ├─► Name field (required)
    │   ├─► Quick stat block (all 7 attributes)
    │   ├─► Health auto-calculated
    │   └─► Optional: pick base template
    │
    ▼
Create CharacterEdit directly
    │   ├─► IsNpc = true
    │   ├─► IsTemplate = false
    │   ├─► PlayerId = null
    │
    ▼
Insert + Link to table
    │
    ▼
Appear in NPC section immediately
```

### Time Advancement (Enhanced)

```
GM clicks "+1 Round"
    │
    ▼
TimeAdvancementService.AdvanceRoundsAsync(tableId, count, time)
    │
    ├─► Get all TableCharacters (existing)
    │
    ├─► Get all TableNpcs (NEW)
    │
    ▼
For each combatant (PC or NPC):
    │
    ├─► Fetch CharacterEdit
    │
    ├─► Call EndOfRound()
    │
    ├─► Save CharacterEdit
    │
    ▼
Publish CharactersUpdatedMessage
    │
    ▼
All clients refresh
```

### NPC Visibility Toggle

```
GM clicks visibility icon on NPC
    │
    ▼
Toggle VisibleToPlayers flag
    │
    ▼
Save CharacterEdit
    │
    ▼
Publish NpcVisibilityMessage
    │
    ▼
Player clients:
    ├─► If VisibleToPlayers = true → Show NPC in target lists
    └─► If VisibleToPlayers = false → Hide from target lists
```

## Integration Points

### 1. GM Dashboard (GmTable.razor)

**Current structure:**
```razor
<div class="col-md-6">
    <!-- Characters at Table -->
    <div class="card">
        @foreach (var character in tableCharacters)
        {
            <CharacterStatusCard ... />
        }
    </div>

    <!-- NPC Placeholder (line 187) -->
    <NpcPlaceholder />
</div>
```

**Enhanced structure:**
```razor
<div class="col-md-6">
    <!-- Player Characters -->
    <div class="card">
        <div class="card-header">
            <strong>Player Characters</strong>
            <span class="badge">@tableCharacters.Count()</span>
        </div>
        @foreach (var character in tableCharacters)
        {
            <CharacterStatusCard ... />
        }
    </div>

    <!-- NPCs (replace NpcPlaceholder) -->
    <NpcSection TableId="@TableId"
                Npcs="@tableNpcs"
                OnNpcClicked="OpenNpcDetails"
                OnAddNpcClicked="ShowNpcCreation" />
</div>
```

### 2. Character Detail Modal

**Key insight:** `CharacterDetailModal.razor` already works with `CharacterEdit`. NPCs using the same model get all features free:
- GM Actions tab (damage, healing, effects, wounds)
- Character Sheet tab (attributes, skills)
- Inventory tab (weapons, armor, items)
- Admin tab (remove from table)

**Only changes needed:**
- Filter "Narrative" tab for NPCs (no player notes)
- Add "Save as Template" action in Admin tab
- Add "Dismiss NPC" action (delete vs persist choice)

### 3. Time Advancement Service

**Current (Threa.Services/TimeAdvancementService.cs):**
```csharp
public async Task<TimeAdvancementResult> AdvanceRoundsAsync(
    Guid tableId, int roundCount, long currentTableTimeSeconds)
{
    var tableChars = await _tableDal.GetTableCharactersAsync(tableId);
    foreach (var tc in tableChars)
    {
        var character = await _characterPortal.FetchAsync(tc.CharacterId);
        character.EndOfRound();
        await _characterPortal.UpdateAsync(character);
    }
}
```

**Enhanced:**
```csharp
public async Task<TimeAdvancementResult> AdvanceRoundsAsync(
    Guid tableId, int roundCount, long currentTableTimeSeconds)
{
    // Process PCs
    var tableChars = await _tableDal.GetTableCharactersAsync(tableId);
    foreach (var tc in tableChars)
    {
        await ProcessCombatantRound(tc.CharacterId, roundCount);
    }

    // Process NPCs (NEW)
    var tableNpcs = await _tableDal.GetTableNpcsAsync(tableId);
    foreach (var npc in tableNpcs)
    {
        await ProcessCombatantRound(npc.CharacterId, roundCount);
    }
}
```

### 4. Combat Targeting

**Current targeting considers characters at table. NPCs should appear in target lists when:**
- NPC is at the table (linked via TableNpcs)
- NPC has `VisibleToPlayers = true` (or attacker is GM)

**Integration point:** `TargetSelectionModal.razor` needs to query both characters and NPCs.

## Suggested Build Order

Based on dependencies and incremental value:

### Phase 1: Data Model Foundation
1. Add `IsNpc`, `IsTemplate`, `VisibleToPlayers` to Character DTO
2. Add migration for new columns
3. Update CharacterEdit business object with new properties
4. Update CharacterDal with NPC query methods

**Rationale:** Everything else depends on the data model.

### Phase 2: NPC Creation UI
1. Create NpcQuickCreate modal (simple creation)
2. Create NpcTemplateList read-only object
3. Create template library UI
4. Create template instantiation logic

**Rationale:** Can't test dashboard integration without NPCs to display.

### Phase 3: Dashboard Integration
1. Create NpcStatusCard component
2. Create NpcSection component
3. Replace NpcPlaceholder in GmTable
4. Wire up NPC click → existing CharacterDetailModal

**Rationale:** NPCs visible and manipulable using existing tools.

### Phase 4: Time & Combat Integration
1. Extend TimeAdvancementService for NPCs
2. Add NPCs to targeting system
3. Handle visibility toggle
4. Real-time updates for NPC state changes

**Rationale:** NPCs participate in combat alongside PCs.

### Phase 5: Lifecycle Management
1. Add "Save as Template" action
2. Add "Dismiss NPC" action with persist/delete choice
3. Group management (select multiple, batch actions)
4. Encounter cleanup suggestions

**Rationale:** Session workflow improvements after core functionality.

## Anti-Patterns to Avoid

### Anti-Pattern 1: Parallel NPC Model
**What:** Creating a separate `NpcEdit` business object with different properties than `CharacterEdit`.
**Why bad:** Duplicates all combat, effect, wound, inventory logic. Maintenance nightmare.
**Instead:** Use `CharacterEdit` with `IsNpc` flag. Single code path.

### Anti-Pattern 2: JSON Blob Stats
**What:** Storing NPC stats as untyped JSON in `TableNpc.StatsJson`.
**Why bad:** Can't use existing resolvers, validators, or UI components.
**Instead:** Use full `CharacterEdit` model. Templates solve creation speed.

### Anti-Pattern 3: Tight Coupling to GmTable
**What:** Putting all NPC logic directly in GmTable.razor.
**Why bad:** 1000+ line component becomes unmanageable.
**Instead:** Extract `NpcSection.razor` and `NpcStatusCard.razor` components.

### Anti-Pattern 4: Eager NPC Loading
**What:** Loading full `CharacterEdit` for every NPC when dashboard loads.
**Why bad:** 20 NPCs = 20 full object fetches. Slow.
**Instead:** Use `TableNpcInfo` (like `TableCharacterInfo`) with minimal display data. Load full character on click.

### Anti-Pattern 5: Visibility via Delete
**What:** "Hiding" NPCs by removing them from table, "revealing" by re-adding.
**Why bad:** Loses all state (wounds, effects, HP). Confuses GM.
**Instead:** Use `VisibleToPlayers` flag. NPC stays in table, just not shown to players.

## Technical Decisions

### Decision 1: Character Model Reuse

**Question:** Should NPCs use `CharacterEdit` or a new lightweight model?

**Decision:** Use `CharacterEdit` with `IsNpc` flag.

**Rationale:**
- `CharacterEdit` already handles 47+ properties including attributes, skills, effects, wounds, items
- All resolvers (attack, defense, damage, effects) work unchanged
- All GM manipulation tools work unchanged (5 tabs in CharacterDetailModal)
- Time advancement works unchanged
- Only additional code is templates and dashboard display

**Trade-offs:**
- Slightly more database storage per NPC
- NPC creation slightly more complex (mitigated by templates)
- Some CharacterEdit properties don't apply (PlayerId, IsPlayable) - just ignore

### Decision 2: Template vs Instance

**Question:** How do NPC templates relate to NPC instances?

**Decision:** Templates are characters with `IsTemplate=true`. Instances are clones with `IsNpc=true, IsTemplate=false`.

**Rationale:**
- Templates can be edited and improved over time
- Instances are independent (changing template doesn't affect active NPCs)
- GM can "Save as Template" from any NPC instance
- Simple cloning logic (deep copy of CharacterEdit)

### Decision 3: Table Scope

**Question:** Are NPCs table-specific or campaign-wide?

**Decision:** NPCs are table-specific (same as player characters).

**Rationale:**
- Matches existing pattern (characters join specific tables)
- NPCs have table-specific state (wounds, effects from this encounter)
- GM chooses to persist (promote to template) or dismiss

### Decision 4: Visibility Model

**Question:** How do hidden NPCs work for surprise encounters?

**Decision:** `VisibleToPlayers` flag on CharacterEdit.

- `true` = Players see NPC in target lists, status displays
- `false` = Only GM sees NPC; players don't know it exists
- GM can toggle mid-combat ("The assassin reveals herself!")
- State persists across refreshes

**Messaging:** `NpcVisibilityMessage` notifies player clients to refresh target lists.

## Scalability Considerations

| Concern | At 5 NPCs | At 20 NPCs | At 50 NPCs |
|---------|-----------|------------|------------|
| Dashboard load | Instant | Fast (<500ms) | Consider pagination |
| Time advancement | Negligible | ~2s for all | Batch processing |
| Memory on client | Minimal | Monitor | Lazy load details |
| Database queries | Simple | Add indexes | Consider caching |

**Recommendation:** Start simple, add optimizations if needed. Most encounters have <10 NPCs.

## Sources

- `S:/src/rdl/threa/Threa.Dal/Dto/GameTable.cs` - Existing TableNpc DTO (lines 99-121)
- `S:/src/rdl/threa/Threa.Dal/ITableDal.cs` - NPC DAL interface (lines 64-81)
- `S:/src/rdl/threa/Threa.Dal.SqlLite/TableDal.cs` - NPC SQLite implementation (lines 401-477)
- `S:/src/rdl/threa/GameMechanics/CharacterEdit.cs` - Full character business object
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Pages/GamePlay/GmTable.razor` - GM dashboard
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Shared/CharacterStatusCard.razor` - Status card pattern
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Shared/CharacterDetailModal.razor` - Detail modal pattern
- `S:/src/rdl/threa/GameMechanics/GamePlay/TableCharacterInfo.cs` - Display DTO pattern
- `S:/src/rdl/threa/.planning/PROJECT.md` - v1.5 requirements

---

**Confidence Level:** HIGH

This architecture recommendation is based on thorough analysis of the existing codebase patterns. The `CharacterEdit` reuse approach aligns with the established CSLA patterns and maximizes code reuse while minimizing implementation risk.
