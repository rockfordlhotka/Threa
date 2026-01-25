# Phase 3: Character Creation Inventory - Research

**Researched:** 2026-01-24
**Domain:** Blazor UI for inventory management during character creation with CSLA business objects
**Confidence:** HIGH

## Summary

This phase implements a player-facing inventory browser and management interface for character creation. The research focuses on enhancing the existing TabItems.razor component to provide a split-view interface where players can browse available item templates on the left and see their current inventory on the right.

The project already has a functional TabItems.razor implementation that handles basic inventory management during character creation. It uses direct DAL injection (IItemTemplateDal, ICharacterItemDal) rather than CSLA business objects for item operations. The key enhancements needed are: (1) adding a split-view layout with item browser, (2) implementing type filtering and search, (3) supporting stackable item quantity editing, and (4) displaying weight warnings based on STR-derived carrying capacity.

The existing Items.razor (GM item list page) from Phase 2 provides proven patterns for RadzenDataGrid with type filtering and debounced search that can be adapted for the player-facing item browser.

**Primary recommendation:** Enhance TabItems.razor with a split-view layout using Bootstrap grid columns. Implement item browser using RadzenDataGrid patterns from Phase 2 (type filter, debounced search). Add inline quantity editing for stackable items. Calculate and display weight warnings using the exponential carrying capacity formula from design docs.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Radzen.Blazor | 8.4.2 | UI components (DataGrid, buttons, inputs) | Already in project, comprehensive component set |
| Csla.Blazor | 9.1.0 | ViewModel binding for CharacterEdit | Already in project, handles character state |
| Bootstrap 5 | (via Radzen) | Grid layout for split view | Already available, standard responsive layout |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| System.Timers.Timer | (built-in) | Debounce implementation | Search box debouncing |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Bootstrap grid | Radzen Splitter | Splitter requires drag interaction; fixed proportions simpler |
| RadzenDataGrid for browser | Simple list with scrolling | DataGrid provides built-in filtering/sorting UX |
| Direct DAL access | CSLA business objects for items | Existing pattern uses DAL; changing would require significant refactor |

**Installation:**
No additional packages needed - all components already installed.

## Architecture Patterns

### Recommended Component Structure
The existing structure is appropriate with enhancements:
```
Threa.Client/Components/Pages/Character/
  TabItems.razor           # Enhanced with split-view layout
  CharacterEdit.razor      # Parent page (no changes needed)
```

### Pattern 1: Split-View Layout with Bootstrap Grid
**What:** Two-column layout with item browser on left, inventory on right
**When to use:** When user needs to compare/select from options while viewing current state
**Example:**
```razor
// Source: Bootstrap grid + project convention
<div class="row">
    <div class="col-md-6">
        <!-- Item Browser (left side) -->
        <h4>Available Items</h4>
        <div class="d-flex gap-3 mb-3">
            <RadzenTextBox @bind-Value="@searchText" Placeholder="Search items..."
                @oninput="@((e) => OnSearchInput(e.Value?.ToString() ?? ""))" Style="width: 200px" />
            <RadzenDropDown TValue="ItemType?" Data="@itemTypes"
                @bind-Value="@selectedType" Change="@(_ => ApplyFilters())"
                Placeholder="Filter by Type" AllowClear="true" Style="width: 150px" />
        </div>
        <RadzenDataGrid Data="@filteredTemplates" TItem="ItemTemplate"
            AllowPaging="true" PageSize="10" Style="height: 400px"
            RowSelect="@AddItemToInventory">
            <Columns>
                <RadzenDataGridColumn Property="Name" Title="Name" Width="200px" />
                <RadzenDataGridColumn Property="ItemType" Title="Type" Width="100px" />
                <RadzenDataGridColumn Property="Weight" Title="Weight" Width="60px" />
            </Columns>
        </RadzenDataGrid>
    </div>
    <div class="col-md-6">
        <!-- Current Inventory (right side) -->
        <h4>Starting Inventory</h4>
        <!-- Weight warning and inventory list -->
    </div>
</div>
```

### Pattern 2: Single-Click Add with Immediate Feedback
**What:** Click item in browser to add one copy to inventory immediately
**When to use:** Per CONTEXT.md decision - "Single click adds one copy of the item to inventory"
**Example:**
```csharp
// Source: CONTEXT.md decision, project convention
private async Task AddItemToInventory(ItemTemplate template)
{
    if (vm?.Model == null || vm.Model.Id == 0 || vm.Model.IsPlayable)
        return;

    var newItem = new CharacterItem
    {
        Id = Guid.NewGuid(),
        ItemTemplateId = template.Id,
        OwnerCharacterId = vm.Model.Id,
        StackSize = 1,
        CurrentDurability = template.HasDurability ? template.MaxDurability : null,
        CreatedAt = DateTime.UtcNow
    };

    await itemDal.AddItemAsync(newItem);
    await LoadItemsAsync();
    StateHasChanged();
}
```

### Pattern 3: Inline Quantity Editing for Stackable Items
**What:** Allow direct editing of stack size in inventory list
**When to use:** Per CONTEXT.md - "Players can edit quantity directly in their inventory list"
**Example:**
```razor
// Source: CONTEXT.md decision
@if (item.Template?.IsStackable == true)
{
    <input type="number" class="form-control form-control-sm" style="width: 70px"
           min="1" max="@item.Template.MaxStackSize"
           value="@item.StackSize"
           @onchange="@(e => UpdateQuantity(item.Id, int.Parse(e.Value?.ToString() ?? "1")))" />
}
else
{
    <span>1</span>
}
```

### Pattern 4: Weight Warning Display
**What:** Show warning when total inventory weight exceeds carrying capacity
**When to use:** Per CONTEXT.md - "Warning only, not enforced"
**Example:**
```csharp
// Source: CARRYING_CAPACITY_ANALYSIS.md, CONTEXT.md
private decimal CalculateCarryingCapacity()
{
    // Formula: 50 lbs * (1.15 ^ (STR - 10))
    var str = vm?.Model?.GetAttribute("STR") ?? 10;
    return 50m * (decimal)Math.Pow(1.15, str - 10);
}

private decimal CalculateTotalWeight()
{
    if (allItems == null) return 0;
    return allItems.Sum(i => (i.Template?.Weight ?? 0) * i.StackSize);
}
```

```razor
@{
    var totalWeight = CalculateTotalWeight();
    var maxWeight = CalculateCarryingCapacity();
    var isOverweight = totalWeight > maxWeight;
}
@if (isOverweight)
{
    <div class="alert alert-warning">
        <strong>Over Capacity!</strong> You are carrying @totalWeight.ToString("F1") lbs
        but your carrying capacity is @maxWeight.ToString("F1") lbs.
        <br/><small>This is allowed during character creation, but may affect gameplay.</small>
    </div>
}
else
{
    <div class="text-muted mb-2">
        Weight: @totalWeight.ToString("F1") / @maxWeight.ToString("F1") lbs
    </div>
}
```

### Pattern 5: Debounced Search (Reuse from Phase 2)
**What:** Delay search execution until user stops typing
**When to use:** Search boxes in item browser
**Example:**
```csharp
// Source: Phase 2 Items.razor pattern
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
```

### Anti-Patterns to Avoid
- **Modal dialogs for adding items:** CONTEXT.md specifies "Single click adds" - no confirmation dialogs
- **Toast/animation feedback:** CONTEXT.md specifies "Immediate visual update only" - no toast notifications
- **Budget enforcement:** CONTEXT.md specifies "No budget system" - don't block adding items
- **Weight enforcement:** CONTEXT.md specifies "Warning only, not enforced" - don't prevent character save

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Data grid with filtering | Custom table with filters | RadzenDataGrid | Built-in filtering, sorting, paging from Phase 2 |
| Debounced input | Complex Rx.NET | Timer pattern | Simple Timer works well, already proven in Phase 2 |
| Split view layout | Custom flexbox | Bootstrap grid | Responsive columns built-in, well-tested |
| Weight calculation | Custom formula | Design doc formula | Formula is `50 * 1.15^(STR-10)`, documented in CARRYING_CAPACITY_ANALYSIS.md |

**Key insight:** The existing TabItems.razor already handles the core inventory operations (add, remove, display). The enhancements are primarily UI layout (split view) and UX improvements (filter/search, quantity editing, weight warning). Reuse patterns from Phase 2 Items.razor.

## Common Pitfalls

### Pitfall 1: Unsaved Character Item Operations
**What goes wrong:** Items added/removed immediately via DAL but character has unsaved changes
**Why it happens:** TabItems.razor uses direct DAL access, not CSLA child objects
**How to avoid:** This is the existing design. Items are saved immediately, independent of CharacterEdit save. Accept this pattern; don't try to refactor to CSLA child objects in this phase.
**Warning signs:** User confusion if they cancel character edit but items already saved

### Pitfall 2: Timer Disposal on Tab Switch
**What goes wrong:** Timer callbacks fire after leaving Items tab, causing null reference
**Why it happens:** Timer continues running when user switches to another tab
**How to avoid:** Implement IDisposable and dispose timer in Dispose()
**Warning signs:** "Cannot access a disposed object" errors after tab switch

### Pitfall 3: Character Not Yet Saved
**What goes wrong:** Trying to add items to a new character that hasn't been saved yet (Id = 0)
**Why it happens:** CharacterEdit.Id is 0 until first save; items require valid OwnerCharacterId
**How to avoid:** Check `vm.Model.Id > 0` before enabling item operations. Display message: "Save your character first to add items"
**Warning signs:** Foreign key violations or orphaned items

### Pitfall 4: Stale Template List
**What goes wrong:** Item template list doesn't include newly created templates
**Why it happens:** Templates loaded once at component init
**How to avoid:** Load templates in OnParametersSetAsync or provide refresh mechanism. For MVP, loading once is acceptable since character creation typically happens shortly after GM sets up items.
**Warning signs:** "Item not found" when clicking recently added templates

### Pitfall 5: IsPlayable Lock Not Respected
**What goes wrong:** Allowing inventory edits after character is activated
**Why it happens:** Missing `IsPlayable` check on edit operations
**How to avoid:** Check `vm.Model.IsPlayable` before any add/remove/quantity operations. The existing TabItems.razor already does this correctly with `disabled="@vm.Model.IsPlayable"`.
**Warning signs:** Inventory changes appearing after character activation

### Pitfall 6: Quantity Below 1 or Above MaxStackSize
**What goes wrong:** User enters invalid quantity (0, negative, or exceeding max)
**Why it happens:** Direct number input without validation
**How to avoid:** Clamp input to valid range (1 to MaxStackSize) and delete item if quantity set to 0
**Warning signs:** Items with 0 quantity in database, stack overflow errors

## Code Examples

Verified patterns from existing project and official sources:

### Enhanced TabItems.razor Structure
```razor
// Source: Existing TabItems.razor + CONTEXT.md decisions
@inject ICharacterItemDal itemDal
@inject IItemTemplateDal itemTemplateDal

@implements IDisposable

@if (vm == null || vm.Model == null)
{
    <p>Loading...</p>
    return;
}

@if (vm.Model.Id == 0)
{
    <div class="alert alert-info">
        <p>Save your character first to manage inventory.</p>
    </div>
    return;
}

@if (!vm.Model.IsPlayable)
{
    <div class="alert alert-info">
        <p>During character creation, browse items on the left and click to add them to your inventory on the right.</p>
    </div>
}

<div class="row">
    @* Left column: Item Browser *@
    <div class="col-md-6">
        <h4>Available Items</h4>
        @if (!vm.Model.IsPlayable)
        {
            <div class="d-flex gap-2 mb-3 flex-wrap">
                <RadzenTextBox @bind-Value="@searchText" Placeholder="Search..."
                    @oninput="@((e) => OnSearchInput(e.Value?.ToString() ?? ""))"
                    Style="width: 180px" />
                <RadzenDropDown TValue="ItemType?" Data="@itemTypes"
                    @bind-Value="@selectedType" Change="@(_ => ApplyFilters())"
                    Placeholder="Type" AllowClear="true" Style="width: 140px" />
            </div>

            <RadzenDataGrid Data="@filteredTemplates" TItem="ItemTemplate"
                AllowSorting="true" AllowPaging="true" PageSize="10"
                RowSelect="@AddItemToInventory"
                Style="cursor: pointer; max-height: 400px; overflow-y: auto">
                <Columns>
                    <RadzenDataGridColumn TItem="ItemTemplate" Property="Name" Title="Name" Width="180px" />
                    <RadzenDataGridColumn TItem="ItemTemplate" Property="ItemType" Title="Type" Width="100px" />
                    <RadzenDataGridColumn TItem="ItemTemplate" Property="Weight" Title="Wt" Width="60px">
                        <Template Context="t">@t.Weight.ToString("F1")</Template>
                    </RadzenDataGridColumn>
                </Columns>
            </RadzenDataGrid>
        }
        else
        {
            <p class="text-muted">Character is active. Inventory managed by GM.</p>
        }
    </div>

    @* Right column: Current Inventory *@
    <div class="col-md-6">
        <h4>Starting Inventory</h4>
        @* Weight display and warning *@
        @* Inventory list with quantity edit and remove buttons *@
    </div>
</div>
```

### Quantity Update with Validation
```csharp
// Source: CONTEXT.md decision + best practices
private async Task UpdateQuantity(Guid itemId, int newQuantity)
{
    var item = allItems?.FirstOrDefault(i => i.Id == itemId);
    if (item == null || vm?.Model?.IsPlayable == true)
        return;

    var maxStack = item.Template?.MaxStackSize ?? 1;

    if (newQuantity <= 0)
    {
        // Quantity 0 or less = remove item
        await itemDal.DeleteItemAsync(itemId);
    }
    else
    {
        // Clamp to valid range
        item.StackSize = Math.Clamp(newQuantity, 1, maxStack);
        await itemDal.UpdateItemAsync(item);
    }

    await LoadItemsAsync();
    StateHasChanged();
}
```

### Container Contents Display (Flat List for Creation)
```csharp
// Source: CONTEXT.md "Claude's Discretion" - flat list during creation
// During character creation, show containers and their contents in a flat list
// with visual indentation to indicate nesting

@foreach (var item in inventoryItems.OrderBy(i => i.Template?.ItemType).ThenBy(i => i.Template?.Name))
{
    <div class="inventory-item">
        <!-- Item display with delete button and optional quantity edit -->
    </div>

    @if (item.Template?.IsContainer == true)
    {
        var contents = GetContainerContents(item.Id);
        @foreach (var contentItem in contents)
        {
            <div class="inventory-item ms-4" style="border-left: 2px solid #dee2e6;">
                <!-- Nested item display -->
            </div>
        }
    }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Dropdown to select item | DataGrid browser with click-to-add | Phase 3 | Better UX for browsing large item libraries |
| No filtering | Type filter + search | Phase 3 | Faster item discovery |
| No weight warning | STR-based carrying capacity display | Phase 3 | Player awareness of encumbrance |

**Deprecated/outdated:**
- The existing simple dropdown selector in TabItems.razor will be replaced with the split-view DataGrid browser

## Open Questions

Things that couldn't be fully resolved:

1. **Container Organization During Creation**
   - What we know: CONTEXT.md says "Claude's Discretion" for "flat list vs nested view"
   - What's unclear: How deep container nesting should be displayed
   - Recommendation: Use flat list with visual indentation (existing pattern in TabItems.razor). Don't allow drag-drop into containers during creation - that's Phase 4 gameplay complexity.

2. **Character ID 0 Handling**
   - What we know: New characters have Id = 0 until first save
   - What's unclear: Best UX for this edge case
   - Recommendation: Show message "Save your character first to add items" and disable item browser. This is simpler than complex state management.

3. **Item Template Refresh**
   - What we know: Templates are loaded once at component init
   - What's unclear: Whether to refresh when tab becomes active
   - Recommendation: Load once is sufficient for MVP. Character creation is typically shortly after setup. Add TODO comment for future enhancement if needed.

## Sources

### Primary (HIGH confidence)
- Existing project code: TabItems.razor - current inventory management patterns
- Existing project code: Items.razor (Phase 2) - RadzenDataGrid filtering, debounced search
- Existing project code: CharacterEdit.cs - GetAttribute method for STR access
- Design docs: CARRYING_CAPACITY_ANALYSIS.md - weight formula `50 * 1.15^(STR-10)`
- CONTEXT.md decisions - UI behavior specifications

### Secondary (MEDIUM confidence)
- [Radzen DataGrid documentation](https://blazor.radzen.com/datagrid) - RowSelect pattern
- [Bootstrap 5 Grid](https://getbootstrap.com/docs/5.0/layout/grid/) - responsive column layout

### Tertiary (LOW confidence)
- None - all patterns are from existing codebase or official docs

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All components already in use in project
- Architecture: HIGH - Extending existing TabItems.razor with proven patterns from Phase 2
- Pitfalls: MEDIUM - Some from existing code analysis, others from general Blazor experience

**Research date:** 2026-01-24
**Valid until:** 2026-02-24 (30 days - stable technologies, extending existing patterns)
