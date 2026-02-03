# Phase 25: NPC Creation & Dashboard - Research

**Researched:** 2026-02-02
**Domain:** CSLA business objects + Blazor UI + NPC spawning + Dashboard integration
**Confidence:** HIGH

## Summary

This phase enables GMs to spawn NPCs from templates during gameplay and manage them through the GM dashboard. The architecture decision (from v1.5) dictates that spawned NPCs are full `CharacterEdit` objects with `IsNpc=true` (not separate lightweight stat blocks), enabling complete feature parity with PCs.

The implementation requires:
1. NPC Spawner business object to create character instances from templates
2. Spawn modal UI with name customization and disposition override
3. Dashboard NPC section with disposition-based grouping
4. Integration of CharacterStatusCard and CharacterDetailModal for NPCs
5. Auto-naming service with global counter and template prefix memory

**Primary recommendation:** NPCs are Characters. Spawn creates a new Character with `IsNpc=true`, `IsTemplate=false`, copied from the template. Dashboard reuses existing CharacterStatusCard with minimal NPC-specific additions (disposition icon, template label). The existing `TableNpc` DTO is NOT used - NPCs are full characters attached via TableCharacter like PCs.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CSLA.NET | 9.1.0 | Business objects | CharacterEdit already supports IsNpc flag |
| Radzen.Blazor | 8.4.2 | UI components | DialogService for modals, existing integration |
| Bootstrap Icons | (bundled) | Disposition icons | Already used throughout UI |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| System.Text.Json | (built-in) | Session state serialization | Prefix memory persistence |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Full CharacterEdit for NPCs | TableNpc lightweight DTO | User decision: NPCs need full manipulation parity |
| Database counter | Session state counter | User decided global session counter, not persistent |
| Separate NPC card component | Extended CharacterStatusCard | Same component with conditional rendering |

**Installation:**
No new packages needed - all dependencies in place from Phase 24.

## Architecture Patterns

### Recommended Project Structure

```
GameMechanics/
  NpcSpawner.cs              # NEW: Command to spawn NPC from template
  NpcSpawnResult.cs          # NEW: Result DTO with spawned character ID

Threa.Client/Components/
  Pages/GameMaster/
    NpcTemplates.razor       # MODIFY: Add spawn button per template row
  Pages/GamePlay/
    GmTable.razor            # MODIFY: Replace NpcPlaceholder with NPC section
  Shared/
    NpcPlaceholder.razor     # DELETE: Replace with real NPC section
    NpcSpawnModal.razor      # NEW: Modal for name/disposition customization
    NpcStatusCard.razor      # NEW: Wrapper around CharacterStatusCard with disposition
    CharacterStatusCard.razor # MODIFY: Add optional disposition icon slot

Threa.Client/Services/
  NpcAutoNamingService.cs    # NEW: Session-scoped service for auto-naming
```

### Pattern 1: NPC Spawner Command Object

**What:** CSLA CommandBase that creates a new Character from template
**When to use:** Spawning NPCs from templates
**Example:**
```csharp
// Source: Based on existing CharacterEdit patterns
[Serializable]
public class NpcSpawner : CommandBase<NpcSpawner>
{
    public static readonly PropertyInfo<int> TemplateIdProperty = RegisterProperty<int>(nameof(TemplateId));
    public int TemplateId
    {
        get => ReadProperty(TemplateIdProperty);
        set => LoadProperty(TemplateIdProperty, value);
    }

    public static readonly PropertyInfo<Guid> TableIdProperty = RegisterProperty<Guid>(nameof(TableId));
    public Guid TableId
    {
        get => ReadProperty(TableIdProperty);
        set => LoadProperty(TableIdProperty, value);
    }

    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    public string Name
    {
        get => ReadProperty(NameProperty);
        set => LoadProperty(NameProperty, value);
    }

    public static readonly PropertyInfo<NpcDisposition> DispositionProperty = RegisterProperty<NpcDisposition>(nameof(Disposition));
    public NpcDisposition Disposition
    {
        get => ReadProperty(DispositionProperty);
        set => LoadProperty(DispositionProperty, value);
    }

    public static readonly PropertyInfo<string?> SessionNotesProperty = RegisterProperty<string?>(nameof(SessionNotes));
    public string? SessionNotes
    {
        get => ReadProperty(SessionNotesProperty);
        set => LoadProperty(SessionNotesProperty, value);
    }

    // Result: spawned character ID
    public static readonly PropertyInfo<int> SpawnedCharacterIdProperty = RegisterProperty<int>(nameof(SpawnedCharacterId));
    public int SpawnedCharacterId
    {
        get => ReadProperty(SpawnedCharacterIdProperty);
        private set => LoadProperty(SpawnedCharacterIdProperty, value);
    }

    [Execute]
    private async Task ExecuteAsync(
        [Inject] ICharacterDal characterDal,
        [Inject] ITableDal tableDal,
        [Inject] IDataPortal<CharacterEdit> characterPortal,
        ...)
    {
        // 1. Fetch template
        var template = await characterDal.GetCharacterAsync(TemplateId);

        // 2. Create new character (clone from template)
        var npc = await characterPortal.CreateAsync(template.PlayerId);

        // 3. Copy all stats from template
        CopyFromTemplate(npc, template);

        // 4. Set NPC-specific properties
        npc.IsNpc = true;
        npc.IsTemplate = false;
        npc.IsPlayable = true; // Ready for play
        npc.Name = Name;
        npc.DefaultDisposition = Disposition;
        npc.Notes = SessionNotes ?? string.Empty;

        // 5. Save NPC
        npc = await characterPortal.UpdateAsync(npc);

        // 6. Attach to table
        var tableChar = new TableCharacter
        {
            TableId = TableId,
            CharacterId = npc.Id,
            PlayerId = template.PlayerId, // GM's player ID
            JoinedAt = DateTime.UtcNow,
            ConnectionStatus = ConnectionStatus.Connected
        };
        await tableDal.AddCharacterToTableAsync(tableChar);

        SpawnedCharacterId = npc.Id;
    }
}
```

### Pattern 2: Auto-Naming Service (Session-Scoped)

**What:** Service that generates unique names with template prefix memory
**When to use:** Pre-filling spawn modal name field
**Example:**
```csharp
// Source: Session-scoped service per CONTEXT.md decisions
public class NpcAutoNamingService
{
    private int _globalCounter = 0;
    private readonly Dictionary<int, string> _templatePrefixes = new();

    public string GenerateName(int templateId, string templateName)
    {
        _globalCounter++;

        // Get or create prefix for this template
        if (!_templatePrefixes.TryGetValue(templateId, out var prefix))
        {
            prefix = templateName;
            _templatePrefixes[templateId] = prefix;
        }

        return $"{prefix} {_globalCounter}";
    }

    public void SetPrefixForTemplate(int templateId, string prefix)
    {
        _templatePrefixes[templateId] = prefix;
    }

    public string? GetPrefixForTemplate(int templateId)
    {
        return _templatePrefixes.TryGetValue(templateId, out var prefix) ? prefix : null;
    }
}

// Registration in Program.cs
builder.Services.AddScoped<NpcAutoNamingService>();
```

### Pattern 3: Dashboard NPC Section

**What:** NPCs displayed below PCs, grouped by disposition
**When to use:** GmTable.razor NPC section
**Example:**
```razor
// Source: Based on CONTEXT.md decisions
@* NPC Section - Below Characters *@
<div class="card mt-3">
    <div class="card-header d-flex justify-content-between align-items-center">
        <strong><i class="bi bi-people me-1"></i>NPCs</strong>
        <button class="btn btn-primary btn-sm" @onclick="ShowSpawnModal">
            <i class="bi bi-plus-lg"></i> Spawn NPC
        </button>
    </div>
    <div class="card-body" style="max-height: 500px; overflow-y: auto;">
        @if (tableNpcs == null || !tableNpcs.Any())
        {
            <div class="text-center text-muted py-3">
                No NPCs at this table. Spawn from templates to begin.
            </div>
        }
        else
        {
            @* Hostile Group *@
            @if (hostileNpcs.Any())
            {
                <div class="mb-3">
                    <h6 class="text-danger mb-2">
                        <i class="bi bi-skull-crossbones"></i> Hostile
                    </h6>
                    <div class="row row-cols-1 row-cols-lg-2 g-2">
                        @foreach (var npc in hostileNpcs)
                        {
                            <div class="col">
                                <NpcStatusCard Character="@npc" OnClick="OpenNpcDetails" />
                            </div>
                        }
                    </div>
                </div>
            }

            @* Neutral Group *@
            @if (neutralNpcs.Any())
            {
                <div class="mb-3">
                    <h6 class="text-secondary mb-2">
                        <i class="bi bi-circle"></i> Neutral
                    </h6>
                    @* ... same pattern ... *@
                </div>
            }

            @* Friendly Group *@
            @if (friendlyNpcs.Any())
            {
                <div class="mb-3">
                    <h6 class="text-success mb-2">
                        <i class="bi bi-heart"></i> Friendly
                    </h6>
                    @* ... same pattern ... *@
                </div>
            }
        }
    </div>
</div>
```

### Pattern 4: NPC Status Card with Disposition Icon

**What:** CharacterStatusCard wrapped with NPC-specific additions
**When to use:** Displaying NPCs in dashboard
**Example:**
```razor
// NpcStatusCard.razor
@inherits CharacterStatusCardBase

<div class="npc-status-card position-relative">
    @* Disposition Icon - Top Right *@
    <div class="position-absolute" style="top: 4px; right: 4px; z-index: 1;">
        @switch (Disposition)
        {
            case NpcDisposition.Hostile:
                <i class="bi bi-skull text-danger" title="Hostile"></i>
                break;
            case NpcDisposition.Neutral:
                <i class="bi bi-circle text-secondary" title="Neutral"></i>
                break;
            case NpcDisposition.Friendly:
                <i class="bi bi-heart text-success" title="Friendly"></i>
                break;
        }
    </div>

    @* Reuse existing CharacterStatusCard *@
    <CharacterStatusCard Character="@Character"
                         IsSelected="@IsSelected"
                         OnClick="@OnClick" />

    @* Template Label - Below Card *@
    @if (!string.IsNullOrEmpty(SourceTemplateName))
    {
        <small class="text-muted d-block text-center" style="font-size: 0.7rem;">
            From: @SourceTemplateName
        </small>
    }
</div>

@code {
    [Parameter] public NpcDisposition Disposition { get; set; }
    [Parameter] public string? SourceTemplateName { get; set; }
}
```

### Pattern 5: Spawn Modal

**What:** Modal for customizing NPC name and disposition before spawn
**When to use:** Spawning from template library or dashboard
**Example:**
```razor
// NpcSpawnModal.razor
<div class="modal fade show d-block" style="background-color: rgba(0,0,0,0.5);">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Spawn NPC: @TemplateName</h5>
                <button type="button" class="btn-close" @onclick="Cancel"></button>
            </div>
            <div class="modal-body">
                <div class="mb-3">
                    <label class="form-label">Name</label>
                    <input type="text" class="form-control" @bind="npcName" />
                    <small class="text-muted">Auto-generated from template. Customize as needed.</small>
                </div>
                <div class="mb-3">
                    <label class="form-label">Disposition</label>
                    <select class="form-select" @bind="disposition">
                        <option value="@NpcDisposition.Hostile">Hostile</option>
                        <option value="@NpcDisposition.Neutral">Neutral</option>
                        <option value="@NpcDisposition.Friendly">Friendly</option>
                    </select>
                </div>
                <div class="mb-3">
                    <label class="form-label">Session Notes (optional)</label>
                    <textarea class="form-control" rows="3" @bind="sessionNotes"
                        placeholder="Notes about this specific NPC instance..."></textarea>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-secondary" @onclick="Cancel">Cancel</button>
                <button type="button" class="btn btn-primary" @onclick="Spawn"
                    disabled="@string.IsNullOrWhiteSpace(npcName)">
                    <i class="bi bi-plus-lg"></i> Spawn
                </button>
            </div>
        </div>
    </div>
</div>

@code {
    [Parameter] public int TemplateId { get; set; }
    [Parameter] public string TemplateName { get; set; } = "";
    [Parameter] public NpcDisposition DefaultDisposition { get; set; }
    [Parameter] public string AutoGeneratedName { get; set; } = "";
    [Parameter] public EventCallback<NpcSpawnRequest> OnSpawn { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    private string npcName = "";
    private NpcDisposition disposition;
    private string sessionNotes = "";

    protected override void OnParametersSet()
    {
        npcName = AutoGeneratedName;
        disposition = DefaultDisposition;
    }

    private async Task Spawn()
    {
        await OnSpawn.InvokeAsync(new NpcSpawnRequest
        {
            TemplateId = TemplateId,
            Name = npcName,
            Disposition = disposition,
            SessionNotes = string.IsNullOrWhiteSpace(sessionNotes) ? null : sessionNotes
        });
    }

    private async Task Cancel() => await OnCancel.InvokeAsync();
}
```

### Anti-Patterns to Avoid

- **Using TableNpc DTO:** User decision mandates full CharacterEdit. TableNpc was placeholder.
- **Creating separate NPC detail modal:** Reuse CharacterDetailModal - NPCs are characters.
- **Batch spawn UI:** User decided one-at-a-time spawn with customization modal.
- **Per-template counter:** User decided global counter for clarity ("which Goblin 2?").
- **Persistent counter:** Session-scoped counter resets each session, which is fine.

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| NPC status display | New card component | CharacterStatusCard + wrapper | Same stats, same display |
| NPC detail editing | New modal | CharacterDetailModal | Full manipulation parity |
| NPC attachment to table | New linking mechanism | TableCharacter | NPCs attach like PCs |
| Modal dialogs | Custom modal | Radzen DialogService | Consistent with existing modals |
| Session state | Custom storage | Scoped service | Built-in DI lifecycle |

**Key insight:** NPCs are Characters. The existing CharacterEdit, CharacterStatusCard, and CharacterDetailModal already handle everything NPCs need. The innovation is the spawning workflow and dashboard organization, not new character management.

## Common Pitfalls

### Pitfall 1: Not Setting IsPlayable on Spawn

**What goes wrong:** Spawned NPCs can't be manipulated because IsPlayable=false
**Why it happens:** Template has IsPlayable=false (unactivated)
**How to avoid:** NpcSpawner explicitly sets IsPlayable=true
**Warning signs:** "Activate character" prompts appearing for NPCs

### Pitfall 2: Confusion Between Template and Instance Properties

**What goes wrong:** Editing spawned NPC affects template
**Why it happens:** Shallow copy or reference sharing
**How to avoid:** Deep copy in NpcSpawner - new Character with copied values
**Warning signs:** Template stats changing after NPC manipulation

### Pitfall 3: NPC Counter Collision

**What goes wrong:** Multiple "Goblin 1" at same table
**Why it happens:** Counter not shared or reset improperly
**How to avoid:** Single NpcAutoNamingService instance per session (scoped DI)
**Warning signs:** Duplicate names appearing

### Pitfall 4: Missing Table Attachment

**What goes wrong:** Spawned NPC doesn't appear in dashboard
**Why it happens:** Character created but not linked to table
**How to avoid:** NpcSpawner atomically creates character AND attaches to table
**Warning signs:** NPCs visible in admin but not GM dashboard

### Pitfall 5: Session Notes Lost on Reload

**What goes wrong:** Session notes disappear after page refresh
**Why it happens:** Notes stored only in session state
**How to avoid:** Store session notes in Character.Notes field (persisted to DB)
**Warning signs:** Notes present in modal, gone after refresh

### Pitfall 6: Disposition Not Persisted

**What goes wrong:** NPC disposition resets to template default
**Why it happens:** Disposition override not saved
**How to avoid:** Store disposition in Character.DefaultDisposition (already exists)
**Warning signs:** All NPCs showing as Hostile after refresh

## Code Examples

Verified patterns from codebase analysis:

### TableCharacterInfo Extensions for NPCs

```csharp
// Add NPC-specific properties to TableCharacterInfo
// Source: Based on existing TableCharacterInfo pattern

public static readonly PropertyInfo<bool> IsNpcProperty = RegisterProperty<bool>(nameof(IsNpc));
public bool IsNpc
{
    get => GetProperty(IsNpcProperty);
    private set => LoadProperty(IsNpcProperty, value);
}

public static readonly PropertyInfo<NpcDisposition> DispositionProperty = RegisterProperty<NpcDisposition>(nameof(Disposition));
public NpcDisposition Disposition
{
    get => GetProperty(DispositionProperty);
    private set => LoadProperty(DispositionProperty, value);
}

public static readonly PropertyInfo<int?> SourceTemplateIdProperty = RegisterProperty<int?>(nameof(SourceTemplateId));
public int? SourceTemplateId
{
    get => GetProperty(SourceTemplateIdProperty);
    private set => LoadProperty(SourceTemplateIdProperty, value);
}

public static readonly PropertyInfo<string?> SourceTemplateNameProperty = RegisterProperty<string?>(nameof(SourceTemplateName));
public string? SourceTemplateName
{
    get => GetProperty(SourceTemplateNameProperty);
    private set => LoadProperty(SourceTemplateNameProperty, value);
}

// In Fetch method:
if (character != null)
{
    // ... existing code ...
    IsNpc = character.IsNpc;
    Disposition = character.DefaultDisposition;
    // Source template tracking requires new field in Character DTO
}
```

### Character DTO Extension for Template Tracking

```csharp
// Add to Threa.Dal.Dto.Character
// Source: New property for spawn tracking

/// <summary>
/// ID of the template this NPC was spawned from. Null for PCs and templates.
/// </summary>
public int? SourceTemplateId { get; set; }
```

### CharacterEdit Property for Template Tracking

```csharp
// Add to GameMechanics.CharacterEdit
// Source: Based on existing property patterns

public static readonly PropertyInfo<int?> SourceTemplateIdProperty = RegisterProperty<int?>(nameof(SourceTemplateId));
[Display(Name = "Source Template")]
public int? SourceTemplateId
{
    get => GetProperty(SourceTemplateIdProperty);
    set => SetProperty(SourceTemplateIdProperty, value);
}
```

### Disposition Icons (Bootstrap Icons)

```razor
@* Recommended icons per CONTEXT.md discretion *@

@* Hostile - Red skull *@
<i class="bi bi-skull text-danger" title="Hostile"></i>

@* Neutral - Gray circle *@
<i class="bi bi-circle text-secondary" title="Neutral"></i>

@* Friendly - Green heart *@
<i class="bi bi-heart text-success" title="Friendly"></i>
```

### NpcAutoNamingService Full Implementation

```csharp
// Source: Based on CONTEXT.md auto-naming decisions
namespace Threa.Services;

/// <summary>
/// Session-scoped service for generating unique NPC names.
/// Global counter across all NPCs, with template-specific prefix memory.
/// </summary>
public class NpcAutoNamingService
{
    private int _globalCounter = 0;
    private readonly Dictionary<int, string> _templatePrefixes = new();

    /// <summary>
    /// Generates the next unique name for an NPC from the given template.
    /// </summary>
    /// <param name="templateId">Template ID for prefix lookup.</param>
    /// <param name="defaultPrefix">Default prefix (template name) if not previously set.</param>
    /// <returns>Generated name like "Goblin 1", "Bandit 2".</returns>
    public string GenerateName(int templateId, string defaultPrefix)
    {
        _globalCounter++;
        var prefix = GetOrSetPrefix(templateId, defaultPrefix);
        return $"{prefix} {_globalCounter}";
    }

    /// <summary>
    /// Gets the current prefix for a template, or sets it if not yet defined.
    /// </summary>
    public string GetOrSetPrefix(int templateId, string defaultPrefix)
    {
        if (!_templatePrefixes.TryGetValue(templateId, out var prefix))
        {
            prefix = defaultPrefix;
            _templatePrefixes[templateId] = prefix;
        }
        return prefix;
    }

    /// <summary>
    /// Updates the prefix for a template. Called when GM customizes the name.
    /// Extracts prefix from name like "Bandit Leader 3" -> "Bandit Leader".
    /// </summary>
    public void UpdatePrefixFromName(int templateId, string fullName)
    {
        // Extract prefix by removing trailing number
        var match = System.Text.RegularExpressions.Regex.Match(fullName, @"^(.+?)\s*\d*$");
        if (match.Success && !string.IsNullOrWhiteSpace(match.Groups[1].Value))
        {
            _templatePrefixes[templateId] = match.Groups[1].Value.Trim();
        }
    }

    /// <summary>
    /// Gets the current global counter value.
    /// </summary>
    public int CurrentCounter => _globalCounter;
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| TableNpc lightweight DTO | Full CharacterEdit | Phase 25 decision | Full manipulation parity |
| NPCs as separate concept | NPCs as Characters with flag | Phase 23 foundation | Unified character system |
| Manual NPC stat entry | Spawn from template | This phase | Rapid NPC deployment |

**Deprecated/outdated:**
- `TableNpc` DTO: Not used for spawned NPCs. May still be used for truly lightweight stat blocks in future, but Phase 25 NPCs are full characters.
- `NpcPlaceholder.razor`: Will be replaced with real NPC section.

## Open Questions

### 1. Source Template Name Resolution

- **What we know:** We need to show "From: Goblin Warrior" on NPC cards
- **What's unclear:** Best way to resolve template name from ID
- **Recommendation:**
  - Option A: Store SourceTemplateName in Character DTO (denormalized, simple)
  - Option B: Lookup via DAL when building TableCharacterInfo (normalized, extra query)
  - **Recommend Option A** - simpler, template names rarely change

### 2. NPC Removal Flow

- **What we know:** GMs should be able to remove NPCs from table
- **What's unclear:** Should removal delete the character or just detach?
- **Recommendation:** Detach from table (delete TableCharacter record), keep Character for potential reuse or history. CharacterDetailAdmin already has "Remove from Table" which does detachment.

### 3. NPC Time Processing

- **What we know:** TimeAdvancementService processes all characters at table
- **What's unclear:** Should NPCs participate in time advancement?
- **Recommendation:** Yes - NPCs have wounds, effects, etc. that need processing. The existing TableCharacterList already fetches all attached characters, so no change needed.

## Sources

### Primary (HIGH confidence)
- `S:/src/rdl/threa/GameMechanics/CharacterEdit.cs` - Existing IsNpc, IsTemplate, DefaultDisposition properties
- `S:/src/rdl/threa/Threa.Dal/Dto/Character.cs` - DTO with template properties
- `S:/src/rdl/threa/Threa.Dal/Dto/GameTable.cs` - TableCharacter, TableNpc DTOs
- `S:/src/rdl/threa/Threa.Dal/Dto/NpcDisposition.cs` - Enum already exists
- `S:/src/rdl/threa/GameMechanics/GamePlay/TableCharacterInfo.cs` - Read-only info pattern
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Shared/CharacterStatusCard.razor` - Card component
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Shared/CharacterDetailModal.razor` - Detail modal
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Pages/GamePlay/GmTable.razor` - GM dashboard
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Pages/GameMaster/NpcTemplates.razor` - Template library
- `S:/src/rdl/threa/.planning/phases/25-npc-creation-dashboard/25-CONTEXT.md` - User decisions

### Secondary (MEDIUM confidence)
- `S:/src/rdl/threa/.planning/phases/24-npc-template-system/24-RESEARCH.md` - Prior phase patterns
- `S:/src/rdl/threa/Threa.Dal.SqlLite/TableDal.cs` - TableNpc DAL implementation

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Using existing libraries and established patterns
- Architecture: HIGH - Direct extension of Phase 24 patterns, user decisions clear
- Pitfalls: HIGH - Based on actual codebase analysis and common CSLA/Blazor issues

**Research date:** 2026-02-02
**Valid until:** 2026-03-02 (stable patterns, 30 days)

---

## Implementation Checklist

For the planner, these are the verified implementation steps:

### Plan 1: Data Model Extensions

1. **Character DTO** (Threa.Dal/Dto/Character.cs)
   - Add `int? SourceTemplateId` property
   - Add `string? SourceTemplateName` property (denormalized for display)

2. **CharacterEdit Business Object** (GameMechanics/CharacterEdit.cs)
   - Add `SourceTemplateId` CSLA property
   - Add `SourceTemplateName` CSLA property
   - Add to DataMapper ignore list as needed

3. **TableCharacterInfo** (GameMechanics/GamePlay/TableCharacterInfo.cs)
   - Add `IsNpc` property
   - Add `Disposition` property (NpcDisposition)
   - Add `SourceTemplateId` property
   - Add `SourceTemplateName` property
   - Populate in Fetch from character data

4. **Unit Tests**
   - Test new properties persist
   - Test TableCharacterInfo populates NPC fields

### Plan 2: NPC Spawner Business Object

1. **NpcSpawner Command** (GameMechanics/NpcSpawner.cs)
   - CommandBase with TemplateId, TableId, Name, Disposition, SessionNotes parameters
   - Result: SpawnedCharacterId
   - Execute method: clone template, set properties, save, attach to table

2. **NpcSpawnResult DTO** (optional - can use properties on command)

3. **Unit Tests**
   - Test spawn creates new character
   - Test spawn copies template stats
   - Test spawn attaches to table
   - Test spawned NPC has correct flags (IsNpc=true, IsTemplate=false, IsPlayable=true)

### Plan 3: Auto-Naming Service

1. **NpcAutoNamingService** (Threa/Services/NpcAutoNamingService.cs)
   - Global counter across session
   - Template prefix memory
   - GenerateName(templateId, defaultPrefix) method
   - UpdatePrefixFromName(templateId, fullName) method

2. **Service Registration** (Program.cs)
   - Register as Scoped service

3. **Unit Tests**
   - Test counter increments globally
   - Test prefix memory per template
   - Test prefix extraction from customized name

### Plan 4: Spawn Modal UI

1. **NpcSpawnModal.razor** (Components/Shared/)
   - Parameters: TemplateId, TemplateName, DefaultDisposition, AutoGeneratedName
   - Events: OnSpawn, OnCancel
   - Fields: Name (editable), Disposition (dropdown), SessionNotes (optional)

2. **NpcSpawnRequest DTO** (or inline record)
   - TemplateId, Name, Disposition, SessionNotes

3. **Integration Points**
   - NpcTemplates.razor: Add "Spawn" button per row
   - GmTable.razor: Add spawn button in NPC section header

### Plan 5: Dashboard NPC Section

1. **Replace NpcPlaceholder** in GmTable.razor
   - Fetch NPCs via TableCharacterList (filter IsNpc=true)
   - Group by disposition (Hostile, Neutral, Friendly)
   - Hide empty groups
   - Add "+ Spawn NPC" button in header

2. **NpcStatusCard.razor** (Components/Shared/)
   - Wrapper around CharacterStatusCard
   - Add disposition icon (skull/circle/heart)
   - Add source template label

3. **CSS Styling**
   - Disposition group headers
   - Icon positioning

### Plan 6: Integration and Polish

1. **CharacterDetailModal Integration**
   - Verify NPCs open correctly in modal
   - Verify all tabs work (GM Actions, Sheet, Inventory, Narrative, Admin)
   - Session notes editable in Narrative tab

2. **NpcTemplates.razor Spawn Button**
   - Add spawn button per template row
   - Open spawn modal
   - Pass template info to modal

3. **Real-time Updates**
   - NPC spawns broadcast CharacterUpdateMessage
   - Dashboard refreshes when NPC spawned
   - Other GMs see new NPCs (multi-GM scenario)

4. **End-to-End Testing**
   - Spawn from template library
   - Spawn from dashboard
   - Customize name and disposition
   - Verify NPC appears in correct disposition group
   - Open NPC detail modal
   - Manipulate NPC (damage, effects, etc.)
   - Time advancement affects NPCs
