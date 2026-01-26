# Phase 2: GM Item Management - Research

**Researched:** 2026-01-24
**Domain:** Blazor Web UI for CRUD operations with CSLA business objects
**Confidence:** HIGH

## Summary

This phase implements a Game Master interface for managing item templates. The research focuses on enhancing the existing Items.razor and ItemEdit.razor pages to add filtering, searching, tagging, and improved form organization according to the decisions in CONTEXT.md.

The project already has a functional foundation: QuickGrid for list views, Bootstrap table-based forms, CSLA ViewModel pattern for data binding, and Radzen.Blazor 8.4.2 available. The key enhancements needed are: (1) replacing QuickGrid with RadzenDataGrid for filtering/sorting, (2) reorganizing the edit form into tabs, (3) adding debounced search, (4) adding a Tags property with autocomplete, and (5) inline editing for skill bonuses and attribute modifiers.

The existing ItemEdit.razor already handles type-specific fieldsets (Weapon, Armor, Container, Ammunition). The phase's work is primarily UI reorganization and enhancement, not new business logic.

**Primary recommendation:** Leverage existing Radzen.Blazor components (RadzenDataGrid, RadzenTabs) with established CSLA ViewModel patterns. Add Tags property to DTO/business objects. Use client-side filtering/sorting since the item template list is small enough (under 1000 items expected).

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Radzen.Blazor | 8.4.2 | UI components (DataGrid, Tabs, Form controls) | Already in project, comprehensive component set |
| Csla.Blazor | 9.1.0 | ViewModel binding, validation display | Already in project, CSLA is the business object framework |
| Microsoft.AspNetCore.Components.QuickGrid | (built-in) | Lightweight data grid | Currently used but will be replaced by RadzenDataGrid |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| System.Text.Json | (built-in) | CustomProperties JSON serialization | Weapon/armor property editing |
| Timer (System.Timers) | (built-in) | Debounce implementation | Search box debouncing |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| RadzenDataGrid | QuickGrid | QuickGrid lacks built-in filtering UI; RadzenDataGrid provides filter row |
| Custom debounce | Majorsoft.Blazor.Components.Debounce | External dependency; simple Timer-based approach is sufficient |
| RadzenTabs | Custom tab implementation | RadzenTabs provides keyboard navigation, styling, events built-in |

**Installation:**
No additional packages needed - Radzen.Blazor 8.4.2 is already installed.

## Architecture Patterns

### Recommended Project Structure
The existing structure is appropriate:
```
Threa.Client/Components/Pages/GameMaster/
  Items.razor           # List page with RadzenDataGrid
  ItemEdit.razor        # Edit page with RadzenTabs
  ItemDelete.razor      # Delete confirmation (existing)
```

### Pattern 1: RadzenDataGrid with Client-Side Filtering
**What:** Use RadzenDataGrid with AllowFiltering, AllowSorting, built-in search
**When to use:** List pages with moderate data volume (<1000 rows)
**Example:**
```csharp
// Source: https://blazor.radzen.com/datagrid
<RadzenDataGrid Data="@items" TItem="ItemTemplateInfo"
    AllowFiltering="true" FilterMode="FilterMode.Simple"
    AllowSorting="true" AllowPaging="true" PageSize="20">
    <Columns>
        <RadzenDataGridColumn TItem="ItemTemplateInfo" Property="Name" Title="Name"
            Filterable="true" Sortable="true" />
        <RadzenDataGridColumn TItem="ItemTemplateInfo" Property="ItemType" Title="Type"
            Filterable="true" Sortable="true" />
    </Columns>
</RadzenDataGrid>
```

### Pattern 2: RadzenTabs for Form Sections
**What:** Organize complex forms into tabbed sections that show/hide based on ItemType
**When to use:** Edit forms with conditional sections
**Example:**
```csharp
// Source: https://blazor.radzen.com/tabs
<RadzenTabs @bind-SelectedIndex="@selectedTabIndex">
    <Tabs>
        <RadzenTabsItem Text="Basic">
            <!-- Basic properties always visible -->
        </RadzenTabsItem>
        @if (vm.Model.ItemType == ItemType.Weapon)
        {
            <RadzenTabsItem Text="Weapon">
                <!-- Weapon-specific properties -->
            </RadzenTabsItem>
        }
        @if (vm.Model.ItemType == ItemType.Armor)
        {
            <RadzenTabsItem Text="Armor">
                <!-- Armor-specific properties -->
            </RadzenTabsItem>
        }
    </Tabs>
</RadzenTabs>
```

### Pattern 3: Debounced Search with Timer
**What:** Delay search execution until user stops typing
**When to use:** Search boxes that trigger server/filtering operations
**Example:**
```csharp
// Source: Project convention
private string searchText = "";
private System.Timers.Timer? debounceTimer;
private const int DebounceMs = 300;

private void OnSearchInput(string value)
{
    searchText = value;
    debounceTimer?.Stop();
    debounceTimer?.Dispose();
    debounceTimer = new System.Timers.Timer(DebounceMs);
    debounceTimer.Elapsed += (s, e) =>
    {
        debounceTimer?.Stop();
        InvokeAsync(() => { ApplyFilters(); StateHasChanged(); });
    };
    debounceTimer.Start();
}

private void ApplyFilters()
{
    filteredItems = allItems
        .Where(i => string.IsNullOrEmpty(searchText) ||
            i.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
            i.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
            (i.Tags?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false))
        .ToList();
}
```

### Pattern 4: CSLA ViewModel with Form Binding
**What:** Use Csla.Blazor.ViewModel<T> for edit forms with automatic dirty tracking
**When to use:** All edit pages with CSLA business objects
**Example:**
```csharp
// Source: Existing project pattern (ItemEdit.razor, SkillEdit.razor)
@inject IDataPortal<ItemTemplateEdit> itemPortal
@inject ViewModel<ItemTemplateEdit> vm

protected override async Task OnInitializedAsync()
{
    vm.Saved += () => NavigationManager.NavigateTo("/gamemaster/items");
    vm.ModelChanged += async () => await InvokeAsync(() => StateHasChanged());
    vm.ModelPropertyChanged += async (s, a) => await InvokeAsync(() => StateHasChanged());

    if (IsNewItem)
        await vm.RefreshAsync(() => itemPortal.CreateAsync());
    else
        await vm.RefreshAsync(() => itemPortal.FetchAsync(itemId));
}

// Save button disabled until valid
<button disabled="@(!vm.Model.IsSavable)" @onclick="vm.SaveAsync">Save</button>
```

### Pattern 5: Fixed Bottom Action Bar
**What:** Sticky footer with Save/Cancel buttons always visible
**When to use:** Long scrollable forms
**Example:**
```css
/* CSS */
.form-actions-bar {
    position: sticky;
    bottom: 0;
    background: var(--rz-base-background-color);
    padding: 1rem;
    border-top: 1px solid var(--rz-border-color);
    z-index: 100;
}
```
```html
<div class="form-actions-bar">
    <button class="btn btn-primary" disabled="@(!vm.Model.IsSavable)" @onclick="SaveData">Save</button>
    <button class="btn btn-secondary ms-2" @onclick="Cancel">Cancel</button>
</div>
```

### Anti-Patterns to Avoid
- **Inline editing in list view:** CONTEXT.md specifies "click through only" - clicking row navigates to edit, no inline action buttons in table
- **Summary validation messages:** CONTEXT.md specifies "inline validation only" - no summary at top of form
- **Multiple type filters:** CONTEXT.md specifies "single-select type filter" - use dropdown, not multi-select
- **Add button dialogs for bonuses:** CONTEXT.md specifies "inline grid editing" for skill bonuses and attribute modifiers

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Data grid filtering | Custom filter UI | RadzenDataGrid AllowFiltering | Built-in filter row, operators, apply/clear buttons |
| Tab navigation | Custom div-based tabs | RadzenTabs | Keyboard navigation, ARIA support, events |
| Debounced input | Complex Rx.NET Subject | Simple Timer | Timer is sufficient for single input scenario |
| Form validation display | Custom validation messages | CSLA ViewModel error text + vm.GetPropertyInfo | CSLA integration already handles broken rules |
| Autocomplete/typeahead | Custom dropdown logic | RadzenDropDown with AllowFiltering | Built-in search, async data loading |

**Key insight:** Radzen.Blazor is already in the project and provides comprehensive components. The existing patterns in SkillEdit.razor and ItemEdit.razor demonstrate working CSLA + Radzen integration. Do not introduce additional UI libraries.

## Common Pitfalls

### Pitfall 1: Tab Index Reset on ItemType Change
**What goes wrong:** When ItemType changes and tabs show/hide, SelectedIndex may point to a now-hidden tab, causing blank content
**Why it happens:** RadzenTabs SelectedIndex is an integer index, not tab identity
**How to avoid:** Reset SelectedIndex to 0 (Basic tab) whenever ItemType changes
**Warning signs:** Empty tab content panel after changing ItemType

### Pitfall 2: CSLA ViewModel StateHasChanged Loop
**What goes wrong:** UI becomes slow after save, with hundreds of PropertyChanged events
**Why it happens:** StateHasChanged triggers UI refresh which triggers PropertyChanged on model
**How to avoid:** Use InvokeAsync to batch UI updates; avoid calling StateHasChanged directly in PropertyChanged handlers
**Warning signs:** Significant delay (>1 second) between clicking Save and navigation

### Pitfall 3: RadzenDataGrid IQueryable vs IEnumerable
**What goes wrong:** Filtering/sorting doesn't work or causes exceptions
**Why it happens:** RadzenDataGrid expects IQueryable for server-side operations
**How to avoid:** For client-side filtering with in-memory data, use `items.AsQueryable()` and set LoadData mode appropriately
**Warning signs:** "The provider for the source IQueryable doesn't implement IAsyncQueryProvider" errors

### Pitfall 4: Tags Property Not in DTO
**What goes wrong:** Tags cannot be saved because DTO lacks Tags property
**Why it happens:** CONTEXT.md mentions tags but Phase 1 DTO doesn't include them
**How to avoid:** Add Tags property to ItemTemplate DTO, ItemTemplateEdit, ItemTemplateInfo, and DAL before UI work
**Warning signs:** Tags display in UI but disappear after save

### Pitfall 5: Timer Disposal on Page Navigation
**What goes wrong:** Timer callbacks fire after component disposed, causing null reference exceptions
**Why it happens:** Timer continues running after user navigates away
**How to avoid:** Implement IDisposable and dispose timer in Dispose()
**Warning signs:** "Cannot access a disposed object" errors in console after navigation

### Pitfall 6: RadzenDataGrid DeleteRow Bug
**What goes wrong:** Deleting one new row removes all new rows from grid
**Why it happens:** Known Radzen bug with inline editing and multiple uncommitted rows
**How to avoid:** Commit each row before adding another, or refresh grid after delete
**Warning signs:** Multiple rows disappear when only one should be deleted

## Code Examples

Verified patterns from official sources and existing project:

### RadzenDataGrid with Type Filter and Search
```csharp
// Adapted from: https://blazor.radzen.com/datagrid, project conventions
@code {
    private IEnumerable<ItemTemplateInfo>? allItems;
    private IEnumerable<ItemTemplateInfo>? filteredItems;
    private string searchText = "";
    private ItemType? selectedType;

    protected override async Task OnInitializedAsync()
    {
        await vm.RefreshAsync(() => itemListPortal.FetchAsync());
        allItems = vm.Model;
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        var query = allItems?.AsEnumerable() ?? Enumerable.Empty<ItemTemplateInfo>();

        if (selectedType.HasValue)
            query = query.Where(i => i.ItemType == selectedType.Value);

        if (!string.IsNullOrWhiteSpace(searchText))
            query = query.Where(i =>
                i.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                i.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                (i.Tags?.Contains(searchText, StringComparison.OrdinalIgnoreCase) ?? false));

        filteredItems = query.ToList();
    }
}

<div class="d-flex gap-3 mb-3">
    <RadzenTextBox @bind-Value="@searchText" Placeholder="Search..."
        @oninput="@((e) => OnSearchInput(e.Value?.ToString() ?? ""))"
        Style="width: 300px" />
    <RadzenDropDown TValue="ItemType?" Data="@Enum.GetValues<ItemType>()"
        @bind-Value="@selectedType" Change="@(_ => ApplyFilters())"
        Placeholder="Filter by Type" AllowClear="true" />
</div>

<RadzenDataGrid Data="@filteredItems" TItem="ItemTemplateInfo"
    AllowSorting="true" AllowPaging="true" PageSize="20"
    RowSelect="@(item => NavigationManager.NavigateTo($"/gamemaster/items/{item.Id}"))">
    <Columns>
        <RadzenDataGridColumn TItem="ItemTemplateInfo" Property="Name" Title="Name" />
        <RadzenDataGridColumn TItem="ItemTemplateInfo" Property="ItemType" Title="Type" />
    </Columns>
</RadzenDataGrid>
```

### Tabbed Edit Form with Type-Specific Tabs
```csharp
// Adapted from: https://blazor.radzen.com/tabs, project conventions
@code {
    private int selectedTabIndex = 0;

    private void OnItemTypeChanged()
    {
        selectedTabIndex = 0; // Reset to Basic tab when type changes
        StateHasChanged();
    }
}

<RadzenTabs @bind-SelectedIndex="@selectedTabIndex">
    <Tabs>
        <RadzenTabsItem Text="Basic">
            <!-- Name, Description, Type, Weight, Value, etc. -->
            <TextInputRow Property="vm.GetPropertyInfo<string>(() => vm.Model.Name)" />
            <tr>
                <td>Item Type</td>
                <td>
                    <select @bind="vm.Model.ItemType" @bind:after="OnItemTypeChanged" class="form-control">
                        @foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
                        {
                            <option value="@type">@type</option>
                        }
                    </select>
                </td>
            </tr>
        </RadzenTabsItem>

        @if (vm.Model.ItemType == ItemType.Weapon)
        {
            <RadzenTabsItem Text="Weapon">
                <!-- Weapon properties -->
            </RadzenTabsItem>
        }

        @if (vm.Model.ItemType == ItemType.Armor || vm.Model.ItemType == ItemType.Shield)
        {
            <RadzenTabsItem Text="Armor">
                <!-- Armor properties -->
            </RadzenTabsItem>
        }

        @if (vm.Model.ItemType == ItemType.Container)
        {
            <RadzenTabsItem Text="Container">
                <!-- Container properties -->
            </RadzenTabsItem>
        }

        <RadzenTabsItem Text="Bonuses">
            <!-- Skill bonuses and attribute modifiers grids -->
        </RadzenTabsItem>
    </Tabs>
</RadzenTabs>
```

### Inline Grid for Skill Bonuses
```csharp
// Adapted from: https://blazor.radzen.com/datagrid-inline-edit
<RadzenDataGrid @ref="bonusGrid" Data="@skillBonuses" TItem="SkillBonusEdit"
    EditMode="DataGridEditMode.Single" RowUpdate="@OnBonusRowUpdate">
    <Columns>
        <RadzenDataGridColumn TItem="SkillBonusEdit" Property="SkillName" Title="Skill">
            <EditTemplate Context="bonus">
                <RadzenDropDown @bind-Value="bonus.SkillName" Data="@availableSkills"
                    TextProperty="Name" ValueProperty="Id" AllowFiltering="true" />
            </EditTemplate>
        </RadzenDataGridColumn>
        <RadzenDataGridColumn TItem="SkillBonusEdit" Property="BonusValue" Title="Bonus">
            <EditTemplate Context="bonus">
                <RadzenNumeric @bind-Value="bonus.BonusValue" />
            </EditTemplate>
        </RadzenDataGridColumn>
        <RadzenDataGridColumn TItem="SkillBonusEdit" Width="120px">
            <Template Context="bonus">
                <RadzenButton Icon="edit" Click="@(() => bonusGrid.EditRow(bonus))" />
                <RadzenButton Icon="delete" Click="@(() => RemoveBonus(bonus))" />
            </Template>
            <EditTemplate Context="bonus">
                <RadzenButton Icon="check" Click="@(() => bonusGrid.UpdateRow(bonus))" />
                <RadzenButton Icon="close" Click="@(() => bonusGrid.CancelEditRow(bonus))" />
            </EditTemplate>
        </RadzenDataGridColumn>
    </Columns>
</RadzenDataGrid>
<RadzenButton Text="Add Bonus" Icon="add" Click="@AddNewBonus" />
```

### Tags Input with Autocomplete
```csharp
// Adapted from: Radzen AutoComplete pattern
<RadzenTextBox @bind-Value="@newTag" Placeholder="Add tag..."
    @onkeydown="@OnTagKeyDown" />
<RadzenDropDown TValue="string" Data="@suggestedTags"
    @bind-Value="@newTag" AllowFiltering="true" FilterCaseSensitivity="FilterCaseSensitivity.CaseInsensitive"
    Visible="@(suggestedTags?.Any() ?? false)" />

@if (vm.Model.TagList.Any())
{
    <div class="d-flex flex-wrap gap-1 mt-2">
        @foreach (var tag in vm.Model.TagList)
        {
            <span class="badge bg-secondary">
                @tag
                <button type="button" class="btn-close btn-close-white ms-1"
                    @onclick="@(() => RemoveTag(tag))"></button>
            </span>
        }
    </div>
}

@code {
    private string newTag = "";
    private IEnumerable<string>? suggestedTags;

    private void OnTagKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter" && !string.IsNullOrWhiteSpace(newTag))
        {
            vm.Model.TagList.Add(newTag.Trim());
            newTag = "";
        }
    }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| QuickGrid for lists | RadzenDataGrid with filtering | This phase | Better filtering/sorting UX |
| Long scrollable form | Tabbed form sections | This phase | Faster navigation to relevant fields |
| Add button + modal for bonuses | Inline grid editing | This phase | Excel-like editing experience |

**Deprecated/outdated:**
- None applicable - this is new UI work

## Open Questions

Things that couldn't be fully resolved:

1. **Tags Property Location**
   - What we know: CONTEXT.md mentions tags but ItemTemplate DTO has no Tags property
   - What's unclear: Whether Tags should be a comma-separated string or separate table
   - Recommendation: Add `public string? Tags { get; set; }` to DTO (comma-separated), parse to list in business object. Simple approach sufficient for v1.

2. **Hard Delete vs Soft Delete for Templates**
   - What we know: GM-12 says "delete templates that have never been instantiated"
   - What's unclear: How to check if template has been instantiated (CharacterItem references)
   - Recommendation: Add `HasInstances()` check to DAL, show error if trying to hard-delete used template. Soft delete (deactivate) always allowed.

3. **Tag Autocomplete Source**
   - What we know: CONTEXT.md wants "autocomplete from existing tags"
   - What's unclear: Whether to query distinct tags from all templates or maintain separate tag list
   - Recommendation: Query distinct tags from existing templates on page load. Simple and self-updating.

## Sources

### Primary (HIGH confidence)
- Existing project code: Items.razor, ItemEdit.razor, SkillEdit.razor - established patterns
- Existing project code: ItemTemplateEdit.cs, IItemDal.cs - CSLA and DAL patterns
- [Radzen DataGrid documentation](https://blazor.radzen.com/datagrid) - filtering, sorting API
- [Radzen Tabs documentation](https://blazor.radzen.com/tabs) - tab component API

### Secondary (MEDIUM confidence)
- [Radzen DataGrid Inline Edit](https://blazor.radzen.com/datagrid-inline-edit) - inline editing pattern
- [CSLA Blazor ViewModel](https://cslanet.com/5.4.2/html/class_csla_1_1_blazor_1_1_view_model.html) - ViewModel class reference

### Tertiary (LOW confidence)
- [Radzen forum: DeleteRow bug](https://forum.radzen.com/t/datagrid-inline-editing-deleterow-removes-all-created-rows-in-grid/14240) - known issue with inline editing

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Radzen.Blazor 8.4.2 already in project, patterns established
- Architecture: HIGH - Existing code demonstrates working CSLA + Radzen + Blazor patterns
- Pitfalls: MEDIUM - Some pitfalls from forum research, others from general Blazor experience

**Research date:** 2026-01-24
**Valid until:** 2026-02-24 (30 days - stable technologies, established patterns)
