# Phase 24: NPC Template System - Research

**Researched:** 2026-02-02
**Domain:** CSLA business objects + Blazor UI + template management patterns
**Confidence:** HIGH

## Summary

This phase builds the GM-facing NPC Template Library for creating, editing, organizing, and browsing NPC templates. Templates are Characters with `IsNpc=true` and `IsTemplate=true` flags (established in Phase 23). The architecture follows proven patterns from ItemTemplate management, with additions for category-based organization and tag filtering.

The implementation requires:
1. A Template Library page (`/gamemaster/templates`) with search/filter capabilities
2. A Template Editor page (`/gamemaster/templates/{id}`) reusing CharacterEdit tabs
3. New Character DTO properties for template organization (Category, Tags, Notes, DefaultDisposition, DifficultyRating)
4. DAL methods for category/tag management
5. Clone operation to copy stats from existing characters/templates

**Primary recommendation:** Reuse CharacterEdit patterns with template-specific wrapper. Add category/tag/disposition as new DTO properties. Follow ItemEdit.razor UI patterns for the library list and tag management.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CSLA.NET | 9.1.0 | Business objects | Existing character management |
| Radzen.Blazor | 8.4.2 | UI components | DataGrid, TextBox, Chips already in use |
| System.Text.Json | (built-in) | Serialization | Current DTO storage pattern |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Microsoft.AspNetCore.Components.QuickGrid | (built-in) | Alternative grid | Already used in Characters.razor |
| MSTest | 3.x | Unit testing | Test template persistence |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| RadzenDataGrid | QuickGrid | RadzenDataGrid has better filtering built-in |
| Separate NpcTemplate DTO | Character with IsTemplate flag | Flag reuses existing infrastructure |
| Complex tag database | Comma-separated string | Simpler, ItemTemplate already uses this pattern |

**Installation:**
No new packages needed - all dependencies in place.

## Architecture Patterns

### Recommended Project Structure

```
Threa.Dal/
  Dto/
    Character.cs           # Add: Category, Tags, Notes, DefaultDisposition, DifficultyRating
    NpcDisposition.cs      # NEW: Enum (Hostile, Neutral, Friendly)
  ICharacterDal.cs         # Add: GetNpcCategoriesAsync()

Threa.Dal.SqlLite/
  CharacterDal.cs          # Implement category extraction

GameMechanics/
  CharacterEdit.cs         # Add template-specific properties
  NpcTemplateList.cs       # NEW: ReadOnlyListBase for template browsing
  NpcTemplateInfo.cs       # NEW: ReadOnlyBase for list items

Threa.Client/Components/
  Pages/GameMaster/
    NpcTemplates.razor       # NEW: Template library page
    NpcTemplateEdit.razor    # NEW: Template editor page
```

### Pattern 1: Template Library List Page

**What:** RadzenDataGrid with search and multi-level filtering
**When to use:** Any browsable list with categories and tags
**Example:**
```razor
// Source: Based on Items.razor pattern
@page "/gamemaster/templates"
@rendermode InteractiveServer

<div class="info-box d-flex justify-content-between align-items-center">
    <div>
        <strong>NPC Template Library</strong>
        <span class="item-meta ms-3">@filteredCount of @totalCount templates</span>
    </div>
    <div class="d-flex gap-2">
        <RadzenButton Text="Create New" Icon="add" Click="@CreateNew" />
        <RadzenButton Text="Clone From..." Icon="content_copy" Click="@ShowCloneModal" />
    </div>
</div>

<div class="card">
    <div class="card-header d-flex gap-3 align-items-center flex-wrap">
        <RadzenTextBox @bind-Value="@searchText" Placeholder="Search templates..."
            @oninput="@OnSearchInput" Style="width: 300px" />
        <RadzenDropDown TValue="string" Data="@categories"
            @bind-Value="@selectedCategory" Change="@(_ => ApplyFilters())"
            Placeholder="All Categories" AllowClear="true" Style="width: 200px" />
        <RadzenDropDown TValue="string" Data="@availableTags"
            @bind-Value="@selectedTag" Change="@(_ => ApplyFilters())"
            Placeholder="All Tags" AllowClear="true" Style="width: 150px" />
    </div>
    <div class="card-body p-0">
        <RadzenDataGrid Data="@filteredTemplates" TItem="NpcTemplateInfo"
            AllowSorting="true" AllowPaging="true" PageSize="20"
            RowSelect="@(t => NavigateTo(t.Id))">
            <Columns>
                <RadzenDataGridColumn TItem="NpcTemplateInfo" Property="Name" Title="Name" />
                <RadzenDataGridColumn TItem="NpcTemplateInfo" Property="Category" Title="Category" />
                <RadzenDataGridColumn TItem="NpcTemplateInfo" Property="DifficultyRating" Title="Difficulty" />
                <RadzenDataGridColumn TItem="NpcTemplateInfo" Title="Tags">
                    <Template Context="item">
                        @foreach (var tag in item.TagList)
                        {
                            <span class="badge bg-secondary me-1">@tag</span>
                        }
                    </Template>
                </RadzenDataGridColumn>
                <RadzenDataGridColumn TItem="NpcTemplateInfo" Title="Status">
                    <Template Context="item">
                        @if (item.IsActive)
                        {
                            <span class="text-positive">Active</span>
                        }
                        else
                        {
                            <span class="text-negative">Inactive</span>
                        }
                    </Template>
                </RadzenDataGridColumn>
            </Columns>
        </RadzenDataGrid>
    </div>
</div>
```

### Pattern 2: Template Editor Page

**What:** CharacterEdit-style tabbed editor with template-specific metadata
**When to use:** Editing full character data with additional organization fields
**Example:**
```razor
// Source: Based on CharacterEdit.razor + ItemEdit.razor patterns
@page "/gamemaster/templates/{Id}"
@page "/gamemaster/templates/new"
@rendermode InteractiveServer

<div class="info-box d-flex justify-content-between align-items-center">
    <strong>@(IsNew ? "Create NPC Template" : $"Edit: {vm.Model?.Name}")</strong>
    <a href="/gamemaster/templates" class="btn btn-outline-secondary btn-sm">Back to Library</a>
</div>

@if (vm.Model != null)
{
    <RadzenTabs @bind-SelectedIndex="@selectedTabIndex">
        <Tabs>
            <RadzenTabsItem Text="Template Info">
                <!-- Name, Category, Tags, Notes, Disposition, Species -->
            </RadzenTabsItem>
            <RadzenTabsItem Text="Attributes">
                <TabAttributes vm="vm" />
            </RadzenTabsItem>
            <RadzenTabsItem Text="Skills">
                <TabSkills vm="vm" />
            </RadzenTabsItem>
            <RadzenTabsItem Text="Equipment">
                <TabItems vm="vm" />
            </RadzenTabsItem>
        </Tabs>
    </RadzenTabs>

    <div class="form-actions-bar">
        <button @onclick="SaveData" class="btn btn-primary" disabled="@(!vm.Model.IsSavable)">Save</button>
        <button @onclick="Cancel" class="btn btn-secondary ms-2">Cancel</button>
    </div>
}
```

### Pattern 3: Tag Management (Chips)

**What:** Comma-separated tags stored as string, edited via chip UI
**When to use:** Flexible categorization with user-defined tags
**Example:**
```razor
// Source: ItemEdit.razor tag management pattern
<tr>
    <td>Tags</td>
    <td>
        <div class="tags-input-container">
            @if (tagList.Any())
            {
                <div class="d-flex flex-wrap gap-1 mb-2">
                    @foreach (var tag in tagList)
                    {
                        <span class="badge bg-secondary d-flex align-items-center">
                            @tag
                            <button type="button" class="btn-close btn-close-white ms-1"
                                style="font-size: 0.6rem;"
                                @onclick="@(() => RemoveTag(tag))"></button>
                        </span>
                    }
                </div>
            }
            <div class="d-flex gap-2">
                <RadzenTextBox @bind-Value="@newTagInput" Placeholder="Add tag..."
                    @onkeydown="@OnTagKeyDown" Style="width: 200px" />
                <RadzenButton Text="Add" Icon="add" Click="@AddTag"
                    Disabled="@string.IsNullOrWhiteSpace(newTagInput)" Size="ButtonSize.Small" />
            </div>
        </div>
    </td>
</tr>
```

### Pattern 4: Clone Operation

**What:** Create new template by copying stats from existing character/template
**When to use:** Creating variants or converting PCs to NPC templates
**Example:**
```csharp
// Clone method in CharacterEdit or factory method
[Create]
private async Task CreateFromClone(int sourceCharacterId, bool copyEquipment,
    [Inject] ICharacterDal dal, ...)
{
    // Fetch source character
    var source = await dal.GetCharacterAsync(sourceCharacterId);

    // Copy stats
    using (BypassPropertyChecks)
    {
        // Copy attributes
        foreach (var attr in source.AttributeList)
        {
            var targetAttr = AttributeList.First(a => a.Name == attr.Name);
            targetAttr.BaseValue = attr.BaseValue;
        }

        // Copy skills
        foreach (var skill in source.Skills)
        {
            // Add skill with same level
        }

        // Set template flags
        IsNpc = true;
        IsTemplate = true;
        Name = $"{source.Name} (Template)";

        // Optionally copy equipment
        if (copyEquipment)
        {
            // Copy equipment list
        }
    }
}
```

### Pattern 5: Difficulty Rating Calculation

**What:** Auto-calculated rating based on combat effectiveness
**When to use:** Quick assessment of NPC threat level
**Example:**
```csharp
// Source: CONTEXT.md specifies AS values and equipment as inputs
public int CalculateDifficultyRating()
{
    // Base: Average of combat-relevant AS values
    int combatSkillSum = 0;
    int combatSkillCount = 0;

    foreach (var skill in Skills)
    {
        if (skill.IsCombatSkill)
        {
            combatSkillSum += skill.AbilityScore;
            combatSkillCount++;
        }
    }

    int baseRating = combatSkillCount > 0
        ? combatSkillSum / combatSkillCount
        : GetEffectiveAttribute("STR");

    // Modifiers
    int healthMod = (Fatigue.BaseValue + Vitality.BaseValue) / 10;
    int equipmentMod = CountEquippedItems() / 2;  // Rough estimate

    return Math.Max(1, baseRating + healthMod + equipmentMod - 10);
}
```

### Anti-Patterns to Avoid

- **Creating separate NpcTemplateEdit class:** Reuse CharacterEdit with wrapper. Templates ARE characters with flags set.
- **Complex tag database:** Use comma-separated string like ItemTemplate. Simpler, proven pattern.
- **Modal-based editing:** User decided dedicated page editor. Modals limit editing space for complex forms.
- **Auto-save:** User stays in editor to continue working - explicit save action.

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Tag storage | Junction table | Comma-separated string | ItemTemplate pattern, works well |
| Category dropdown | Manual fetch | Extract from existing data | Categories are user-defined |
| Character editing | New editor | CharacterEdit tabs | Full feature parity already built |
| Search debouncing | setTimeout | System.Timers.Timer | Items.razor pattern established |
| List filtering | Server-side | Client-side LINQ | Data fits in memory, faster UX |

**Key insight:** The existing codebase has patterns for every piece of this feature. The innovation is the composition, not the components.

## Common Pitfalls

### Pitfall 1: Forgetting to Set Template Flags

**What goes wrong:** Created templates appear as regular characters
**Why it happens:** Create flow doesn't set IsNpc and IsTemplate
**How to avoid:** Set flags immediately in Create operation, not in UI
**Warning signs:** Templates don't appear in GetNpcTemplatesAsync results

### Pitfall 2: Category vs Tag Confusion

**What goes wrong:** Users can't find templates because organization is inconsistent
**Why it happens:** Unclear distinction between category (singular, folder-like) and tags (multiple, labels)
**How to avoid:**
- Category is ONE value (dropdown selection)
- Tags are MANY values (chip input)
- UI should make distinction clear
**Warning signs:** Users putting tag-like values in category field

### Pitfall 3: Clone Copies References Not Values

**What goes wrong:** Editing cloned template affects original
**Why it happens:** Shallow copy of child objects
**How to avoid:** Clone creates new child objects, copies property values
**Warning signs:** Multiple templates showing same changes

### Pitfall 4: Missing Deactivation UI

**What goes wrong:** Users can't find how to "delete" templates
**Why it happens:** Soft delete pattern isn't obvious
**How to avoid:**
- Deactivate button in editor
- Deactivated templates show dimmed/strikethrough in list
- Include "Show Inactive" toggle in filter
**Warning signs:** Users asking how to delete templates

### Pitfall 5: Difficulty Rating Confusion

**What goes wrong:** GMs don't understand the number
**Why it happens:** Algorithm isn't explained, scale isn't defined
**How to avoid:**
- Show breakdown on hover/click
- Document scale (e.g., "1-5 easy, 6-10 moderate, 11+ hard")
**Warning signs:** GMs ignoring the difficulty field

## Code Examples

Verified patterns from codebase analysis:

### NpcDisposition Enum

```csharp
// Source: New enum, pattern from ItemType, EquipmentSlot
namespace Threa.Dal.Dto;

/// <summary>
/// Default NPC behavior/attitude toward players.
/// Can be overridden when spawning NPC from template.
/// </summary>
public enum NpcDisposition
{
    Hostile = 0,
    Neutral = 1,
    Friendly = 2
}
```

### Character DTO Extensions

```csharp
// Source: S:/src/rdl/threa/Threa.Dal/Dto/Character.cs
// Add these properties for template organization

/// <summary>
/// Category folder for organizing NPC templates (GM-defined).
/// Examples: "Humanoids", "Beasts", "Undead", "Constructs"
/// </summary>
public string? Category { get; set; }

/// <summary>
/// Comma-separated tags for flexible categorization.
/// Examples: "minion,melee", "boss,caster,fire"
/// </summary>
public string? Tags { get; set; }

/// <summary>
/// GM notes about this template (single free-text field).
/// </summary>
public string? TemplateNotes { get; set; }

/// <summary>
/// Default disposition when spawning NPCs from this template.
/// </summary>
public NpcDisposition DefaultDisposition { get; set; } = NpcDisposition.Hostile;

/// <summary>
/// Auto-calculated difficulty rating for quick assessment.
/// Higher values indicate more challenging opponents.
/// </summary>
public int DifficultyRating { get; set; }
```

### NpcTemplateInfo ReadOnly Class

```csharp
// Source: Based on ItemTemplateInfo pattern
[Serializable]
public class NpcTemplateInfo : ReadOnlyBase<NpcTemplateInfo>
{
    public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id));
    public int Id => GetProperty(IdProperty);

    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    public string Name => GetProperty(NameProperty);

    public static readonly PropertyInfo<string> SpeciesProperty = RegisterProperty<string>(nameof(Species));
    public string Species => GetProperty(SpeciesProperty);

    public static readonly PropertyInfo<string?> CategoryProperty = RegisterProperty<string?>(nameof(Category));
    public string? Category => GetProperty(CategoryProperty);

    public static readonly PropertyInfo<string?> TagsProperty = RegisterProperty<string?>(nameof(Tags));
    public string? Tags => GetProperty(TagsProperty);

    public IEnumerable<string> TagList => string.IsNullOrEmpty(Tags)
        ? Enumerable.Empty<string>()
        : Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim());

    public static readonly PropertyInfo<NpcDisposition> DefaultDispositionProperty =
        RegisterProperty<NpcDisposition>(nameof(DefaultDisposition));
    public NpcDisposition DefaultDisposition => GetProperty(DefaultDispositionProperty);

    public static readonly PropertyInfo<int> DifficultyRatingProperty = RegisterProperty<int>(nameof(DifficultyRating));
    public int DifficultyRating => GetProperty(DifficultyRatingProperty);

    public static readonly PropertyInfo<bool> IsActiveProperty = RegisterProperty<bool>(nameof(IsActive));
    public bool IsActive => GetProperty(IsActiveProperty);

    [FetchChild]
    private void Fetch(Character dto)
    {
        LoadProperty(IdProperty, dto.Id);
        LoadProperty(NameProperty, dto.Name);
        LoadProperty(SpeciesProperty, dto.Species);
        LoadProperty(CategoryProperty, dto.Category);
        LoadProperty(TagsProperty, dto.Tags);
        LoadProperty(DefaultDispositionProperty, dto.DefaultDisposition);
        LoadProperty(DifficultyRatingProperty, dto.DifficultyRating);
        LoadProperty(IsActiveProperty, !dto.IsNpc || dto.VisibleToPlayers); // Active if visible
    }
}
```

### NpcTemplateList ReadOnlyListBase

```csharp
// Source: Based on ItemTemplateList pattern
[Serializable]
public class NpcTemplateList : ReadOnlyListBase<NpcTemplateList, NpcTemplateInfo>
{
    [Fetch]
    private async Task Fetch([Inject] ICharacterDal dal,
        [Inject] IChildDataPortal<NpcTemplateInfo> childPortal)
    {
        var templates = await dal.GetNpcTemplatesAsync();
        using (LoadListMode)
        {
            foreach (var template in templates)
            {
                Add(childPortal.FetchChild(template));
            }
        }
    }
}
```

### DAL Category Extraction

```csharp
// Source: S:/src/rdl/threa/Threa.Dal.SqlLite/CharacterDal.cs
public async Task<List<string>> GetNpcCategoriesAsync()
{
    var templates = await GetNpcTemplatesAsync();
    return templates
        .Where(t => !string.IsNullOrWhiteSpace(t.Category))
        .Select(t => t.Category!)
        .Distinct()
        .OrderBy(c => c)
        .ToList();
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Separate NPC stat blocks | CharacterEdit with IsNpc flag | Phase 23 | Full mechanical parity |
| No template system | IsTemplate flag + library UI | This phase | Reusable NPC definitions |

**Deprecated/outdated:**
- None specific to this phase

## Open Questions

### 1. Difficulty Algorithm Details

- **What we know:** Based on combat AS values and equipment
- **What's unclear:** Exact formula, scale definition
- **Recommendation:**
  - Simple formula: `Average combat AS + (totalHealth/10) - 10`
  - Scale: 1-5 = easy, 6-10 = moderate, 11-15 = hard, 16+ = deadly
  - Show calculation breakdown in UI tooltip

### 2. Clone Modal Selection

- **What we know:** GM needs to pick source character/template
- **What's unclear:** Best UX for selection
- **Recommendation:** Modal with same search/filter as library, showing both templates and regular characters

### 3. Equipment Cloning Scope

- **What we know:** CONTEXT.md says "cloning copies stats only - not equipment or notes"
- **What's unclear:** Is this the right decision?
- **Recommendation:** Follow CONTEXT.md decision. GMs can manually add equipment after cloning.

## Sources

### Primary (HIGH confidence)
- `S:/src/rdl/threa/GameMechanics/CharacterEdit.cs` - Existing character business object
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Pages/GameMaster/Items.razor` - List page pattern
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Pages/GameMaster/ItemEdit.razor` - Editor page pattern with tags
- `S:/src/rdl/threa/GameMechanics/Items/ItemTemplateEdit.cs` - Template business object pattern
- `S:/src/rdl/threa/GameMechanics/Items/ItemTemplateList.cs` - List business object pattern
- `S:/src/rdl/threa/.planning/phases/24-npc-template-system/24-CONTEXT.md` - User decisions

### Secondary (MEDIUM confidence)
- `S:/src/rdl/threa/.planning/phases/23-data-model-foundation/23-RESEARCH.md` - Phase 23 patterns
- `S:/src/rdl/threa/design/GAME_RULES_SPECIFICATION.md` - Game mechanics reference

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Using existing libraries and patterns
- Architecture: HIGH - Direct pattern reuse from ItemTemplate and CharacterEdit
- Pitfalls: HIGH - Based on actual codebase patterns and common Blazor issues

**Research date:** 2026-02-02
**Valid until:** 2026-03-02 (stable patterns, 30 days)

---

## Implementation Checklist

For the planner, these are the verified implementation steps:

### Plan 1: Data Model Extensions

1. **New Enum** (Threa.Dal/Dto/NpcDisposition.cs)
   - Create `NpcDisposition` enum: Hostile, Neutral, Friendly

2. **DTO Changes** (Threa.Dal/Dto/Character.cs)
   - Add `string? Category { get; set; }`
   - Add `string? Tags { get; set; }`
   - Add `string? TemplateNotes { get; set; }`
   - Add `NpcDisposition DefaultDisposition { get; set; } = NpcDisposition.Hostile`
   - Add `int DifficultyRating { get; set; }`

3. **Business Object Changes** (GameMechanics/CharacterEdit.cs)
   - Add CSLA properties for: Category, Tags, TemplateNotes, DefaultDisposition, DifficultyRating
   - Add `CalculateDifficultyRating()` method

4. **DAL Changes** (Threa.Dal/ICharacterDal.cs)
   - Add `Task<List<string>> GetNpcCategoriesAsync()`

5. **DAL Implementation** (Threa.Dal.SqlLite/CharacterDal.cs)
   - Implement `GetNpcCategoriesAsync()` - extract distinct categories from templates

6. **Unit Tests**
   - Test new properties persist
   - Test difficulty calculation
   - Test GetNpcCategoriesAsync

### Plan 2: Business Objects for Library

1. **NpcTemplateInfo** (GameMechanics/NpcTemplateInfo.cs)
   - ReadOnlyBase with: Id, Name, Species, Category, Tags, DefaultDisposition, DifficultyRating, IsActive
   - TagList computed property for display

2. **NpcTemplateList** (GameMechanics/NpcTemplateList.cs)
   - ReadOnlyListBase fetching via GetNpcTemplatesAsync

3. **Unit Tests**
   - Test list fetches correctly
   - Test info loads all properties

### Plan 3: Template Library UI

1. **NpcTemplates.razor** (/gamemaster/templates)
   - Search box with debouncing
   - Category dropdown filter
   - Tag dropdown filter (populated from data)
   - RadzenDataGrid with: Name, Category, Difficulty, Tags, Status
   - Click row to edit
   - "Create New" and "Clone From..." buttons
   - Show inactive toggle

2. **Navigation**
   - Add to GM navigation menu

### Plan 4: Template Editor UI

1. **NpcTemplateEdit.razor** (/gamemaster/templates/{id} and /gamemaster/templates/new)
   - Template Info tab: Name, Species, Category, Tags (chips), Notes, DefaultDisposition
   - Attributes tab: Reuse TabAttributes
   - Skills tab: Reuse TabSkills
   - Equipment tab: Reuse TabItems
   - Sticky save/cancel bar
   - Deactivate button (for existing templates)

2. **Clone Modal**
   - Modal with character/template search
   - Checkbox for "Copy Equipment" (default: unchecked per CONTEXT.md)
   - Create action creates new template with copied stats

### Plan 5: Integration and Polish

1. **Category Management**
   - Categories are user-defined (no predefined list)
   - Autocomplete dropdown showing existing categories + free text entry

2. **Inactive Template Display**
   - Dimmed/strikethrough styling for inactive
   - Reactivate action in editor

3. **Difficulty Badge**
   - Color-coded display in list
   - Tooltip showing calculation breakdown

4. **End-to-End Tests**
   - Create template from scratch
   - Clone from existing character
   - Edit and save
   - Deactivate and reactivate
   - Search/filter operations
