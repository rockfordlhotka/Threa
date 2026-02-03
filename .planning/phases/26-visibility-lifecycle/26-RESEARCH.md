# Phase 26: Visibility & Lifecycle - Research

**Researched:** 2026-02-03
**Domain:** NPC visibility state + lifecycle management + UI patterns
**Confidence:** HIGH

## Summary

This phase adds visibility toggling for NPCs (hidden/revealed for surprise encounters) and lifecycle management (dismiss/archive/delete, save as template). The implementation builds on Phase 25's NPC dashboard, extending it with a dedicated "Hidden" section for concealed NPCs and adding lifecycle actions to the CharacterDetailModal.

Key findings:
1. **VisibleToPlayers property already exists** on CharacterEdit (added in Phase 23) - no data model changes needed
2. **Existing UI patterns** provide all components needed: collapsible sections, confirmation dialogs, modal actions
3. **Archive vs delete** requires new IsArchived flag on Character DTO (or soft-delete pattern)
4. **Save as template** can clone NPC back to template using existing NpcSpawner pattern in reverse

**Primary recommendation:** Implement visibility as UI filtering using existing VisibleToPlayers property. Hidden NPCs render in a collapsed "Hidden" section with minimized cards. Lifecycle actions (dismiss, archive, delete, save-as-template) go in CharacterDetailModal's Admin tab (for NPCs only) or new GM Actions section.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CSLA.NET | 9.1.0 | Business objects | CharacterEdit already has VisibleToPlayers |
| Bootstrap | 5.x | Collapse components | Built-in accordion/collapse patterns |
| Bootstrap Icons | (bundled) | Eye/eye-slash, archive, trash icons | Consistent with existing UI |
| Radzen.Blazor | 8.4.2 | DialogService for confirmations | Existing modal pattern |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| N/A | | | No new libraries needed |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| UI-only visibility filtering | Separate API endpoint for hidden NPCs | Simpler; all NPCs already loaded |
| Hard delete | Soft delete only | User wants both options with confirmation |
| Archive in separate table | IsArchived flag on Character | Single table simpler, flag approach |

**Installation:**
No new packages needed - all dependencies in place.

## Architecture Patterns

### Recommended Project Structure

```
GameMechanics/
  CharacterEdit.cs           # MODIFY: Add IsArchived property if using soft-delete
  NpcTemplateCreator.cs      # NEW: Command to create template from active NPC

Threa.Dal/
  Dto/Character.cs           # MODIFY: Add IsArchived property
  ICharacterDal.cs           # MODIFY: Add GetArchivedNpcsAsync method

Threa.Client/Components/
  Pages/GamePlay/
    GmTable.razor            # MODIFY: Add Hidden section, archive modal access
  Pages/GameMaster/
    NpcArchive.razor         # NEW: Page for viewing/restoring archived NPCs
  Shared/
    CharacterDetailModal.razor    # MODIFY: Add visibility toggle, lifecycle actions
    CharacterDetailAdmin.razor    # MODIFY: Add NPC lifecycle options
    HiddenNpcCard.razor           # NEW: Minimized card for hidden NPCs
    SaveAsTemplateModal.razor     # NEW: Modal for template name/category/tags
    ArchiveConfirmModal.razor     # OPTIONAL: Could use inline confirmation
```

### Pattern 1: Hidden Section UI

**What:** Collapsible section showing hidden NPCs with minimized cards
**When to use:** GmTable.razor NPC section
**Example:**
```razor
// Source: Based on Bootstrap collapse pattern + CONTEXT.md decisions
@* Hidden Section - Collapsed by default, shows count *@
@if (hiddenNpcs.Any())
{
    <div class="npc-group mb-3">
        <button class="btn btn-sm w-100 d-flex justify-content-between align-items-center py-1"
                style="background: var(--color-bg-tertiary);"
                @onclick="() => showHiddenSection = !showHiddenSection">
            <span>
                <i class="bi bi-eye-slash text-muted me-1"></i>
                Hidden (@hiddenNpcs.Count())
            </span>
            <i class="bi @(showHiddenSection ? "bi-chevron-up" : "bi-chevron-down")"></i>
        </button>

        @if (showHiddenSection)
        {
            <div class="mt-2">
                <div class="row row-cols-2 row-cols-lg-3 g-2">
                    @foreach (var npc in hiddenNpcs)
                    {
                        <div class="col">
                            <HiddenNpcCard Character="@npc"
                                          OnReveal="RevealNpc"
                                          OnClick="OpenCharacterDetails" />
                        </div>
                    }
                </div>
            </div>
        }
    </div>
}

@code {
    private bool showHiddenSection = false;

    private IEnumerable<TableCharacterInfo> hiddenNpcs =>
        tableNpcs.Where(c => !c.VisibleToPlayers);

    private IEnumerable<TableCharacterInfo> visibleHostileNpcs =>
        tableNpcs.Where(c => c.VisibleToPlayers && c.Disposition == NpcDisposition.Hostile);
    // ... similar for neutral, friendly
}
```

### Pattern 2: Minimized Hidden NPC Card

**What:** Compact card showing just name and quick-reveal button
**When to use:** Hidden section display
**Example:**
```razor
// HiddenNpcCard.razor
<div class="hidden-npc-card p-2 rounded border d-flex justify-content-between align-items-center"
     style="background: var(--color-bg-secondary); cursor: pointer;"
     @onclick="() => OnClick.InvokeAsync(Character)">
    <div class="d-flex align-items-center gap-2">
        <i class="bi bi-eye-slash text-muted"></i>
        <span class="small">@Character.CharacterName</span>
        @* Disposition indicator *@
        @switch (Character.Disposition)
        {
            case NpcDisposition.Hostile:
                <i class="bi bi-skull text-danger small"></i>
                break;
            case NpcDisposition.Neutral:
                <i class="bi bi-circle text-secondary small"></i>
                break;
            case NpcDisposition.Friendly:
                <i class="bi bi-heart text-success small"></i>
                break;
        }
    </div>
    <button class="btn btn-outline-primary btn-sm"
            @onclick:stopPropagation="true"
            @onclick="() => OnReveal.InvokeAsync(Character)">
        <i class="bi bi-eye"></i>
    </button>
</div>

@code {
    [Parameter, EditorRequired]
    public TableCharacterInfo Character { get; set; } = null!;

    [Parameter]
    public EventCallback<TableCharacterInfo> OnReveal { get; set; }

    [Parameter]
    public EventCallback<TableCharacterInfo> OnClick { get; set; }
}
```

### Pattern 3: Visibility Toggle in CharacterDetailModal

**What:** Toggle button in modal header or GM Actions tab
**When to use:** CharacterDetailModal for NPCs
**Example:**
```razor
// In CharacterDetailModal.razor header area
@if (character?.IsNpc == true)
{
    <button class="btn @(character.VisibleToPlayers ? "btn-outline-warning" : "btn-outline-success") btn-sm"
            @onclick="ToggleVisibility"
            title="@(character.VisibleToPlayers ? "Hide from players" : "Reveal to players")">
        <i class="bi @(character.VisibleToPlayers ? "bi-eye-slash" : "bi-eye")"></i>
        @(character.VisibleToPlayers ? "Hide" : "Reveal")
    </button>
}

@code {
    private async Task ToggleVisibility()
    {
        if (character == null) return;

        character.VisibleToPlayers = !character.VisibleToPlayers;
        await characterPortal.UpdateAsync(character);

        // Notify dashboard to refresh
        await TimeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
        {
            CharacterId = character.Id,
            UpdateType = CharacterUpdateType.StatusChange,
            CampaignId = TableId.ToString(),
            Description = character.VisibleToPlayers ? "Revealed to players" : "Hidden from players"
        });

        await OnCharacterUpdated.InvokeAsync();
    }
}
```

### Pattern 4: NPC Lifecycle Actions

**What:** Dismiss (archive or delete) and Save as Template actions
**When to use:** CharacterDetailModal Admin tab for NPCs
**Example:**
```razor
// In CharacterDetailAdmin.razor - add NPC-specific section
@if (Character?.IsNpc == true)
{
    <div class="card mt-3">
        <div class="card-header bg-secondary text-white">
            <strong><i class="bi bi-gear me-1"></i>NPC Lifecycle</strong>
        </div>
        <div class="card-body">
            <div class="d-flex flex-column gap-2">
                @* Save as Template *@
                <button class="btn btn-outline-primary" @onclick="OpenSaveAsTemplateModal">
                    <i class="bi bi-file-earmark-plus me-1"></i>Save as Template
                </button>

                @* Archive (instant, no confirmation) *@
                <button class="btn btn-outline-secondary" @onclick="ArchiveNpc">
                    <i class="bi bi-archive me-1"></i>Archive
                </button>

                @* Delete (requires confirmation) *@
                <button class="btn btn-outline-danger" @onclick="ConfirmDelete">
                    <i class="bi bi-trash me-1"></i>Delete Permanently
                </button>
            </div>
        </div>
    </div>
}
```

### Pattern 5: Save as Template Modal

**What:** Modal for capturing template name, category, tags before saving
**When to use:** When GM clicks "Save as Template"
**Example:**
```razor
// SaveAsTemplateModal.razor
<div class="modal fade show d-block" style="background-color: rgba(0,0,0,0.5);">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Save as Template</h5>
                <button type="button" class="btn-close" @onclick="Cancel"></button>
            </div>
            <div class="modal-body">
                <div class="mb-3">
                    <label class="form-label">Template Name</label>
                    <input type="text" class="form-control" @bind="templateName" />
                </div>
                <div class="mb-3">
                    <label class="form-label">Category</label>
                    <input type="text" class="form-control" @bind="category"
                           list="categoryList" placeholder="e.g., Humanoids, Beasts" />
                    <datalist id="categoryList">
                        @foreach (var cat in existingCategories)
                        {
                            <option value="@cat" />
                        }
                    </datalist>
                </div>
                <div class="mb-3">
                    <label class="form-label">Tags</label>
                    <input type="text" class="form-control" @bind="tags"
                           placeholder="e.g., minion, melee, boss" />
                    <small class="text-muted">Comma-separated</small>
                </div>
            </div>
            <div class="modal-footer">
                <button class="btn btn-secondary" @onclick="Cancel">Cancel</button>
                <button class="btn btn-primary" @onclick="Save"
                        disabled="@string.IsNullOrWhiteSpace(templateName)">
                    <i class="bi bi-file-earmark-plus me-1"></i>Save Template
                </button>
            </div>
        </div>
    </div>
</div>

@code {
    [Parameter] public CharacterEdit Character { get; set; } = null!;
    [Parameter] public EventCallback<bool> OnClose { get; set; }

    private string templateName = "";
    private string category = "";
    private string tags = "";
    private List<string> existingCategories = new();

    protected override async Task OnInitializedAsync()
    {
        templateName = Character.Name;
        category = Character.Category ?? "";
        tags = Character.Tags ?? "";
        existingCategories = await characterDal.GetNpcCategoriesAsync();
    }
}
```

### Pattern 6: NpcTemplateCreator Command

**What:** CSLA command that creates a template from an active NPC
**When to use:** "Save as Template" action
**Example:**
```csharp
// GameMechanics/NpcTemplateCreator.cs
[Serializable]
public class NpcTemplateCreator : CommandBase<NpcTemplateCreator>
{
    public static readonly PropertyInfo<int> SourceCharacterIdProperty = RegisterProperty<int>(nameof(SourceCharacterId));
    public int SourceCharacterId
    {
        get => ReadProperty(SourceCharacterIdProperty);
        set => LoadProperty(SourceCharacterIdProperty, value);
    }

    public static readonly PropertyInfo<string> TemplateNameProperty = RegisterProperty<string>(nameof(TemplateName));
    public string TemplateName
    {
        get => ReadProperty(TemplateNameProperty);
        set => LoadProperty(TemplateNameProperty, value);
    }

    public static readonly PropertyInfo<string?> CategoryProperty = RegisterProperty<string?>(nameof(Category));
    public string? Category
    {
        get => ReadProperty(CategoryProperty);
        set => LoadProperty(CategoryProperty, value);
    }

    public static readonly PropertyInfo<string?> TagsProperty = RegisterProperty<string?>(nameof(Tags));
    public string? Tags
    {
        get => ReadProperty(TagsProperty);
        set => LoadProperty(TagsProperty, value);
    }

    // Output
    public static readonly PropertyInfo<int> CreatedTemplateIdProperty = RegisterProperty<int>(nameof(CreatedTemplateId));
    public int CreatedTemplateId
    {
        get => ReadProperty(CreatedTemplateIdProperty);
        private set => LoadProperty(CreatedTemplateIdProperty, value);
    }

    public static readonly PropertyInfo<bool> SuccessProperty = RegisterProperty<bool>(nameof(Success));
    public bool Success
    {
        get => ReadProperty(SuccessProperty);
        private set => LoadProperty(SuccessProperty, value);
    }

    [Execute]
    private async Task ExecuteAsync([Inject] ICharacterDal dal)
    {
        // 1. Fetch source NPC
        var source = await dal.GetCharacterAsync(SourceCharacterId);

        // 2. Create new character as template (clone logic similar to NpcSpawner but reverse)
        var template = CloneAsTemplate(source);
        template.Name = TemplateName;
        template.Category = Category;
        template.Tags = Tags;
        template.IsNpc = true;
        template.IsTemplate = true;
        template.IsPlayable = false;
        template.VisibleToPlayers = true; // Templates visible in library
        template.SourceTemplateId = null; // Templates don't have source
        template.SourceTemplateName = null;

        // 3. Reset health to full
        template.FatValue = template.FatBaseValue;
        template.VitValue = template.VitBaseValue;
        template.FatPendingDamage = 0;
        template.FatPendingHealing = 0;
        template.VitPendingDamage = 0;
        template.VitPendingHealing = 0;

        // 4. Clear effects (wounds, buffs, etc.)
        template.Effects.Clear();

        // 5. Save template
        var saved = await dal.SaveCharacterAsync(template);
        CreatedTemplateId = saved.Id;
        Success = true;
    }
}
```

### Anti-Patterns to Avoid

- **Separate visibility API:** NPCs already loaded; filter in UI instead of extra round-trips
- **Complex archive table:** Use IsArchived flag on Character; simpler than separate table
- **Hard delete without confirmation:** Per CONTEXT.md, delete requires confirmation dialog
- **Visibility state in session:** Must persist; use VisibleToPlayers property (already persisted)
- **Template copy losing state:** Per CONTEXT.md, copy FULL current state including wounds/effects

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Collapsible sections | Custom JS toggle | Bootstrap collapse/accordion | Built-in, accessible |
| Confirmation dialogs | Custom modal | Radzen DialogService or inline confirm | Consistent pattern |
| Character cloning | Manual property copy | Existing NpcSpawner clone logic | Already handles all fields |
| Visibility persistence | Session state | CharacterEdit.VisibleToPlayers | Already exists, persisted |
| Icon system | Custom icons | Bootstrap Icons bi-eye, bi-eye-slash | Already used |

**Key insight:** The VisibleToPlayers property was added in Phase 23 specifically for this use case. No data model changes needed for visibility. Archive requires one new property (IsArchived), but the filtering and UI patterns already exist throughout the codebase.

## Common Pitfalls

### Pitfall 1: Forgetting to Update Dashboard After Visibility Toggle

**What goes wrong:** NPC hidden but still shows in disposition groups
**Why it happens:** Character updated but dashboard not refreshed
**How to avoid:** Publish CharacterUpdateMessage after visibility change; dashboard already subscribes
**Warning signs:** NPC appears in both Hidden section AND disposition group

### Pitfall 2: Newly Spawned NPCs Visible by Default

**What goes wrong:** GM spawns NPC and players immediately see it (no surprise)
**Why it happens:** NpcSpawner sets VisibleToPlayers = template.VisibleToPlayers (usually true)
**How to avoid:** Per CONTEXT.md, spawned NPCs start hidden: `VisibleToPlayers = false`
**Warning signs:** Surprise encounters ruined; players ask "what's that new NPC?"

### Pitfall 3: Archive Not Detaching from Table

**What goes wrong:** Archived NPC still appears in table character list
**Why it happens:** Only set IsArchived flag, didn't remove from TableCharacter
**How to avoid:** Archive = set IsArchived + detach from table (remove TableCharacter record)
**Warning signs:** Archived NPCs still in dashboard (greyed out or visible)

### Pitfall 4: Restored NPC Visible Instead of Hidden

**What goes wrong:** GM restores NPC and it immediately appears to players
**Why it happens:** Restore sets VisibleToPlayers = true
**How to avoid:** Per CONTEXT.md, restored NPCs return in hidden state
**Warning signs:** Players see "returned" NPC before GM is ready

### Pitfall 5: Save as Template Includes Active Effects

**What goes wrong:** Template has wounds, buffs from the combat encounter
**Why it happens:** Copying NPC state directly to template
**How to avoid:** Clear Effects list, reset health pools to full when creating template
**Warning signs:** Templates with "Bleeding (3 rounds)" or similar

### Pitfall 6: Delete Not Cleaning Up TableCharacter

**What goes wrong:** Orphaned TableCharacter records; potential FK violations
**Why it happens:** Delete character but not table attachment
**How to avoid:** Delete TableCharacter first, then Character (or use cascade)
**Warning signs:** Database errors on future fetches

## Code Examples

Verified patterns from codebase analysis:

### TableCharacterInfo VisibleToPlayers Property

```csharp
// Already exists in TableCharacterInfo.cs - need to add property and populate
// Source: Based on existing TableCharacterInfo pattern

public static readonly PropertyInfo<bool> VisibleToPlayersProperty = RegisterProperty<bool>(nameof(VisibleToPlayers));
/// <summary>
/// Whether this NPC is visible to players (false = hidden for surprise).
/// Always true for PCs.
/// </summary>
public bool VisibleToPlayers
{
    get => GetProperty(VisibleToPlayersProperty);
    private set => LoadProperty(VisibleToPlayersProperty, value);
}

// In Fetch method:
if (character != null)
{
    // ... existing code ...
    VisibleToPlayers = character.VisibleToPlayers;
}
```

### Character DTO IsArchived Property

```csharp
// Add to Threa.Dal.Dto.Character
// Source: New property for soft-delete/archive pattern

/// <summary>
/// Whether this character is archived (dismissed but not deleted).
/// Archived characters don't appear in active lists but can be restored.
/// </summary>
public bool IsArchived { get; set; }
```

### CharacterEdit IsArchived Property

```csharp
// Add to GameMechanics/CharacterEdit.cs
public static readonly PropertyInfo<bool> IsArchivedProperty = RegisterProperty<bool>(nameof(IsArchived));
[Display(Name = "Is Archived")]
public bool IsArchived
{
    get => GetProperty(IsArchivedProperty);
    set => SetProperty(IsArchivedProperty, value);
}
```

### ICharacterDal Archive Methods

```csharp
// Add to Threa.Dal/ICharacterDal.cs
/// <summary>
/// Gets archived NPCs for the archive browser.
/// </summary>
Task<List<Character>> GetArchivedNpcsAsync();

/// <summary>
/// Soft-deletes (archives) a character. Does not actually delete.
/// </summary>
Task ArchiveCharacterAsync(int id);

/// <summary>
/// Restores an archived character.
/// </summary>
Task RestoreCharacterAsync(int id);
```

### Visibility Icons

```razor
@* Visibility toggle button - Bootstrap Icons *@

@* Hidden (eye-slash with line through) *@
<i class="bi bi-eye-slash text-muted" title="Hidden from players"></i>

@* Visible (eye) *@
<i class="bi bi-eye text-primary" title="Visible to players"></i>

@* Archive icon *@
<i class="bi bi-archive" title="Archive"></i>

@* Restore icon *@
<i class="bi bi-arrow-counterclockwise" title="Restore from archive"></i>

@* Delete icon *@
<i class="bi bi-trash text-danger" title="Delete permanently"></i>

@* Save as template icon *@
<i class="bi bi-file-earmark-plus" title="Save as template"></i>
```

### NpcSpawner Modification for Hidden Default

```csharp
// In GameMechanics/NpcSpawner.cs [Execute] method
// MODIFY: Spawned NPCs start hidden per CONTEXT.md

// Current:
// VisibleToPlayers = template.VisibleToPlayers,

// Change to:
VisibleToPlayers = false, // Spawned NPCs start hidden for surprise
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| NPCs always visible | VisibleToPlayers toggle | Phase 26 | Surprise encounters |
| Hard delete only | Archive + Delete options | Phase 26 | NPC recovery |
| Manual template creation | Save as Template from NPC | Phase 26 | Captures battle-tested NPCs |
| Visible on spawn | Hidden on spawn by default | Phase 26 | GM controls reveal timing |

**Deprecated/outdated:**
- Direct NPC spawn to visible state: Now spawns hidden by default
- TableNpc DTO visibility: Using Character.VisibleToPlayers instead

## Open Questions

### 1. Archive Page Location

- **What we know:** Archived NPCs accessible via separate archive page/modal
- **What's unclear:** URL path and navigation location
- **Recommendation:** `/gamemaster/npcs/archive` page accessible from NPC Templates page header or GmTable dropdown menu

### 2. Archive Scope - Per-Table or Global

- **What we know:** NPCs are attached to tables
- **What's unclear:** Should archive be per-table or global across all tables?
- **Recommendation:** Global archive (archived NPCs not attached to any table). When restored, GM selects which table to attach to.

### 3. Visibility Toggle Location - Card vs Modal Only

- **What we know:** CONTEXT.md says "accessible from both minimized card AND CharacterDetailModal"
- **What's unclear:** How prominent on minimized card?
- **Recommendation:** Small eye icon button on minimized card (quick toggle), full toggle button in modal header

### 4. Save as Template - Include or Clear Wounds/Effects

- **What we know:** CONTEXT.md says "copies full current state: attributes, skills, equipment, wounds, effects, health, notes"
- **What's unclear:** Should template really have wounds/effects?
- **Recommendation:** Ask user. Common pattern would be to clear combat state (wounds, effects, pending pools) but keep permanent state (attributes, skills, equipment, notes). Template represents the "fresh" version of the NPC.

## Sources

### Primary (HIGH confidence)
- `S:/src/rdl/threa/GameMechanics/CharacterEdit.cs` - Existing VisibleToPlayers property (line 379-385)
- `S:/src/rdl/threa/Threa.Dal/Dto/Character.cs` - DTO with VisibleToPlayers (line 15)
- `S:/src/rdl/threa/GameMechanics/NpcSpawner.cs` - Spawn logic to modify for hidden default
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Pages/GamePlay/GmTable.razor` - Dashboard NPC section
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Shared/NpcStatusCard.razor` - Existing card component
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Shared/CharacterDetailModal.razor` - Modal for actions
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Shared/CharacterDetailAdmin.razor` - Admin actions tab
- `S:/src/rdl/threa/.planning/phases/26-visibility-lifecycle/26-CONTEXT.md` - User decisions
- `S:/src/rdl/threa/.planning/phases/25-npc-creation-dashboard/25-RESEARCH.md` - Prior phase patterns

### Secondary (MEDIUM confidence)
- `S:/src/rdl/threa/Threa.Dal/ICharacterDal.cs` - DAL interface for new methods
- `S:/src/rdl/threa/GameMechanics/NpcTemplateList.cs` - Template listing pattern

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Using existing libraries and established patterns
- Architecture: HIGH - Direct extension of Phase 25 patterns, clear user decisions
- Pitfalls: HIGH - Based on actual codebase analysis and user decisions in CONTEXT.md

**Research date:** 2026-02-03
**Valid until:** 2026-03-03 (stable patterns, 30 days)

---

## Implementation Checklist

For the planner, these are the verified implementation steps:

### Plan 1: Data Model Extensions

1. **TableCharacterInfo** (GameMechanics/GamePlay/TableCharacterInfo.cs)
   - Add `VisibleToPlayers` property (bool)
   - Populate from character.VisibleToPlayers in Fetch

2. **Character DTO** (Threa.Dal/Dto/Character.cs)
   - Add `IsArchived` property (bool, default false)

3. **CharacterEdit** (GameMechanics/CharacterEdit.cs)
   - Add `IsArchived` CSLA property
   - Add to DataMapper ignore list if needed

4. **ICharacterDal** (Threa.Dal/ICharacterDal.cs)
   - Add `GetArchivedNpcsAsync()` method
   - Optionally add `ArchiveCharacterAsync(int id)` helper

5. **DAL Implementations**
   - Implement GetArchivedNpcsAsync in MockDb and SqlLite

6. **Unit Tests**
   - Test IsArchived persists
   - Test VisibleToPlayers in TableCharacterInfo

### Plan 2: Visibility Toggle

1. **NpcSpawner Modification** (GameMechanics/NpcSpawner.cs)
   - Change spawned NPC to `VisibleToPlayers = false` (hidden by default)

2. **GmTable.razor Hidden Section**
   - Add hidden NPCs filtering: `!c.VisibleToPlayers`
   - Add collapsible "Hidden" section above disposition groups
   - Show count in collapsed header

3. **HiddenNpcCard.razor Component**
   - Minimized card: name, disposition icon, reveal button
   - OnClick opens detail modal
   - OnReveal toggles visibility

4. **CharacterDetailModal Visibility Toggle**
   - Add toggle button in header for NPCs
   - Toggle updates VisibleToPlayers and saves
   - Publish CharacterUpdateMessage

5. **Integration Tests**
   - Spawn NPC -> appears in Hidden section
   - Toggle reveal -> moves to disposition group
   - Toggle hide -> moves back to Hidden section

### Plan 3: NPC Lifecycle - Dismiss/Archive

1. **CharacterDetailAdmin.razor NPC Section**
   - Add NPC Lifecycle card for IsNpc characters
   - Archive button (instant, no confirm)
   - Delete button (with confirm)

2. **Archive Flow**
   - Set IsArchived = true
   - Detach from table (remove TableCharacter)
   - Close modal with "archived" result
   - Dashboard refreshes

3. **Delete Flow**
   - Show confirmation dialog
   - On confirm: detach from table, delete character
   - Close modal with "deleted" result
   - Dashboard refreshes

4. **Unit Tests**
   - Archive sets IsArchived, removes from table
   - Delete removes character

### Plan 4: NPC Archive Browser

1. **NpcArchive.razor Page**
   - Route: `/gamemaster/npcs/archive`
   - List archived NPCs (filter IsArchived = true)
   - Restore button per NPC
   - Delete button per NPC

2. **Restore Flow**
   - Set IsArchived = false
   - Set VisibleToPlayers = false (per CONTEXT.md)
   - Prompt: select table to attach to
   - Create TableCharacter record
   - Navigate to GmTable

3. **Navigation Integration**
   - Add link in NpcTemplates.razor header
   - Optionally add in GmTable menu

### Plan 5: Save as Template

1. **NpcTemplateCreator Command** (GameMechanics/NpcTemplateCreator.cs)
   - Input: SourceCharacterId, TemplateName, Category, Tags
   - Output: CreatedTemplateId, Success
   - Logic: Clone NPC, set IsTemplate=true, IsNpc=true, IsPlayable=false
   - Clear effects, reset health to full

2. **SaveAsTemplateModal.razor**
   - Pre-fill name from NPC name
   - Category input with datalist from existing categories
   - Tags input (comma-separated)
   - Save executes NpcTemplateCreator

3. **CharacterDetailGmActions or Admin Tab**
   - Add "Save as Template" button for NPCs
   - Opens SaveAsTemplateModal

4. **Unit Tests**
   - Test template created with correct flags
   - Test template has reset health
   - Test template has cleared effects

### Plan 6: Integration and Polish

1. **Dashboard Updates**
   - Verify hidden section displays correctly
   - Verify visibility toggle updates dashboard
   - Verify archived NPCs disappear from dashboard

2. **CharacterDetailModal Updates**
   - Verify visibility toggle works
   - Verify lifecycle actions work
   - Verify save-as-template works

3. **End-to-End Testing**
   - Spawn NPC (appears hidden)
   - Reveal NPC (moves to disposition group)
   - Hide NPC (moves to hidden section)
   - Archive NPC (disappears from dashboard)
   - Restore NPC (reappears in hidden section)
   - Delete NPC (permanently gone)
   - Save NPC as template (appears in template library)
