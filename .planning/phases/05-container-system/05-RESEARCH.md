# Phase 5: Container System - Research

**Researched:** 2026-01-25
**Domain:** Blazor container management UI with weight/volume tracking and hierarchical item organization
**Confidence:** HIGH

## Summary

This phase implements the container system where players organize items inside container items with physical constraints (weight, volume) tracked and enforced through warnings. The research focuses on adding a container contents panel to the existing TabPlayInventory.razor, implementing container capacity calculations, and creating visual fill indicators.

The project already has robust infrastructure for containers:
1. **ItemTemplate fields** - `IsContainer`, `ContainerMaxWeight`, `ContainerMaxVolume`, `ContainerAllowedTypes`, `ContainerWeightReduction`
2. **CharacterItem field** - `ContainerItemId` for parent-child relationship
3. **ICharacterItemDal** - `GetContainerContentsAsync()`, `MoveToContainerAsync()` methods already implemented
4. **ItemManagementService** - `MoveToContainerAsync()` method with ammo compatibility validation

The UI implementation follows the established split-view pattern from Phase 4 and character creation. The container panel appears on the right when a container is selected, similar to the equipment slots panel. Container capacity enforcement follows the same warning-only pattern as character carrying capacity.

**Primary recommendation:** Add container contents panel to TabPlayInventory.razor using the existing side panel layout. Implement visual fill indicators using CSS classes (green/yellow/red based on fill percentage). Use ItemManagementService.MoveToContainerAsync() for all container operations. Warn (don't block) when capacity limits are exceeded.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| GameMechanics.Items.ItemManagementService | (project) | MoveToContainerAsync with validation | Already handles container operations |
| ICharacterItemDal | (project) | GetContainerContentsAsync, MoveToContainerAsync | DAL methods already implemented |
| Bootstrap 5 | (via Radzen) | Side panel layout, grid columns | Already in use, consistent with Phase 4 |
| Radzen.Blazor | 8.4.2 | DialogService for drop confirmation | Already registered, used in Phase 4 |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| EquipmentSlotExtensions | (project) | Container-related equipment slots (Back, Waist) | Identifying equippable containers |
| ItemType enum | (project) | ItemType.Container identification | Determining container validity |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Side panel for container | Modal dialog | Side panel better for compare/drag operations |
| CSS classes for fill indicator | SVG/Canvas | CSS classes simpler, matches existing tile styling |
| Warning-only enforcement | Blocking enforcement | Per CONTEXT.md - warnings only, GM flexibility |

**Installation:**
No additional packages needed - all components already installed.

## Architecture Patterns

### Recommended Component Structure
```
Threa.Client/Components/Pages/GamePlay/
  TabPlayInventory.razor       # Enhanced with container panel
```

No new files needed - extend existing TabPlayInventory.razor.

### Pattern 1: Side Panel Container View
**What:** Container contents displayed in collapsible/toggleable right panel when container selected
**When to use:** Per CONTEXT.md - "Side panel layout: main inventory on left, selected container contents on right"
**Example:**
```razor
// Source: CONTEXT.md decision + Phase 4 pattern
<div class="row">
    <div class="col-md-7">
        <!-- Main Inventory Grid (existing) -->
        <div class="inventory-grid">
            @foreach (var item in inventoryItems)
            {
                <div class="inventory-tile @GetContainerFillClass(item)"
                     @onclick="() => HandleItemClick(item)">
                    <!-- Item display with container fill indicator -->
                </div>
            }
        </div>
    </div>
    <div class="col-md-5">
        @if (selectedContainer != null)
        {
            <!-- Container Contents Panel -->
            <div class="card">
                <div class="card-header">
                    <strong>@GetItemName(selectedContainer)</strong>
                    <span class="float-end">@GetCapacityDisplay(selectedContainer)</span>
                </div>
                <div class="card-body">
                    @foreach (var item in containerContents)
                    {
                        <div class="container-item" @onclick="() => SelectContainerItem(item)">
                            @item.Template?.Name
                        </div>
                    }
                </div>
                <div class="card-footer">
                    <button class="btn btn-outline-primary" @onclick="RemoveSelectedFromContainer"
                            disabled="@(selectedContainerItem == null)">
                        Remove to Inventory
                    </button>
                </div>
            </div>
        }
        else
        {
            <!-- Equipment Slots (existing) -->
            @RenderEquipmentSlots()
        }
    </div>
</div>
```

### Pattern 2: Move Items Into Container
**What:** Select item(s) in main inventory, then click container tile to move
**When to use:** Per CONTEXT.md - "Move items into containers: select item(s) in main inventory, then click container tile"
**Example:**
```csharp
// Source: CONTEXT.md decision
private async Task HandleItemClick(CharacterItem item)
{
    var template = GetTemplate(item.ItemTemplateId);

    // If a non-container item is selected and user clicks a container, move item into container
    if (selectedItem != null && !selectedItem.Template?.IsContainer == true && IsContainerItem(item))
    {
        await MoveToContainer(selectedItem.Id, item.Id);
        return;
    }

    // If clicking a container, open its contents panel
    if (IsContainerItem(item))
    {
        await OpenContainerPanel(item);
        return;
    }

    // Normal selection toggle
    SelectItem(item);
}

private async Task MoveToContainer(Guid itemId, Guid containerId)
{
    var result = await itemService.MoveToContainerAsync(Character!, itemId, containerId);
    if (!result.Success)
    {
        // Show capacity warning (but item was moved anyway per CONTEXT.md)
        warningMessage = result.ErrorMessage;
    }
    await LoadItemsAsync();
    selectedItem = null;
    selectedItemId = null;
}
```

### Pattern 3: Visual Fill Indicator with CSS Classes
**What:** Container tile appearance changes based on fill level (empty/partial/full)
**When to use:** Per CONTEXT.md - "Visual fill indicator: container icon changes appearance based on fill level"
**Example:**
```csharp
// Source: CONTEXT.md decision + Claude's Discretion
private string GetContainerFillClass(CharacterItem item)
{
    if (!IsContainerItem(item)) return "";

    var (weightPercent, volumePercent) = CalculateContainerFillPercent(item);
    var fillPercent = Math.Max(weightPercent, volumePercent);

    if (fillPercent >= 100) return "container-full";      // Red
    if (fillPercent >= 75) return "container-warning";    // Yellow
    if (fillPercent > 0) return "container-partial";      // Green
    return "container-empty";                              // Gray/neutral
}

private (decimal weightPercent, decimal volumePercent) CalculateContainerFillPercent(CharacterItem container)
{
    var template = GetTemplate(container.ItemTemplateId);
    if (template == null || !template.IsContainer) return (0, 0);

    var contents = containerContentsCache.GetValueOrDefault(container.Id, new List<CharacterItem>());

    decimal totalWeight = contents.Sum(i => (GetTemplate(i.ItemTemplateId)?.Weight ?? 0) * i.StackSize);
    decimal totalVolume = contents.Sum(i => (GetTemplate(i.ItemTemplateId)?.Volume ?? 0) * i.StackSize);

    // Apply weight reduction for magical containers
    totalWeight *= template.ContainerWeightReduction;

    decimal weightPercent = template.ContainerMaxWeight > 0
        ? (totalWeight / template.ContainerMaxWeight.Value) * 100
        : 0;
    decimal volumePercent = template.ContainerMaxVolume > 0
        ? (totalVolume / template.ContainerMaxVolume.Value) * 100
        : 0;

    return (weightPercent, volumePercent);
}
```

### Pattern 4: Container Capacity CSS Styling
**What:** Color-coded container tiles matching fill status
**When to use:** Per CONTEXT.md - "Fill status display: color-coded status only (green/yellow/red)"
**Example:**
```css
/* Source: CONTEXT.md Claude's Discretion - fill indicator colors */
.inventory-tile.container-empty {
    /* Neutral/gray tint for empty containers */
    background: #f8f9fa;
}

.inventory-tile.container-partial {
    /* Green tint for containers with room */
    border-color: var(--bs-success);
    background: rgba(var(--bs-success-rgb), 0.05);
}

.inventory-tile.container-warning {
    /* Yellow/orange tint for containers nearing capacity */
    border-color: var(--bs-warning);
    background: rgba(var(--bs-warning-rgb), 0.1);
}

.inventory-tile.container-full {
    /* Red tint for full/over-capacity containers */
    border-color: var(--bs-danger);
    background: rgba(var(--bs-danger-rgb), 0.1);
}

/* Visual fill indicator bar at bottom of container tile */
.container-fill-bar {
    position: absolute;
    bottom: 0;
    left: 0;
    height: 3px;
    background: linear-gradient(to right, var(--bs-success), var(--bs-warning), var(--bs-danger));
    transition: width 0.3s ease;
}
```

### Pattern 5: Nesting Enforcement (One Level Only)
**What:** Containers can hold containers, but nested containers cannot hold more containers
**When to use:** Per CONTEXT.md - "Nesting: one level only"
**Example:**
```csharp
// Source: CONTEXT.md decision
private async Task<ItemOperationResult> ValidateAndMoveToContainer(Guid itemId, Guid containerId)
{
    var item = await _itemDal.GetItemAsync(itemId);
    var container = await _itemDal.GetItemAsync(containerId);
    var containerTemplate = await _templateDal.GetTemplateAsync(container.ItemTemplateId);

    // Check if container is already inside another container
    if (container.ContainerItemId.HasValue)
    {
        // Container is nested - cannot add items to it
        return ItemOperationResult.Failed(
            "Cannot place items in a container that is itself inside another container.");
    }

    // If item being moved is a container, it can go into another container
    // but warn that it won't be able to hold items while nested
    var itemTemplate = await _templateDal.GetTemplateAsync(item.ItemTemplateId);
    if (itemTemplate.IsContainer)
    {
        // Check if the container being moved has contents
        var nestedContents = await _itemDal.GetContainerContentsAsync(itemId);
        if (nestedContents.Any())
        {
            return ItemOperationResult.Failed(
                "Cannot place a container with items inside another container. Empty it first.");
        }
    }

    return await MoveToContainerAsync(itemId, containerId);
}
```

### Pattern 6: Type Restriction Warning
**What:** Warn when placing non-preferred item type in restricted container
**When to use:** Per CONTEXT.md - "Item type restrictions: warn only"
**Example:**
```csharp
// Source: CONTEXT.md decision
private string? ValidateContainerTypeRestriction(CharacterItem item, CharacterItem container)
{
    var containerTemplate = GetTemplate(container.ItemTemplateId);
    if (string.IsNullOrEmpty(containerTemplate?.ContainerAllowedTypes))
        return null; // No restrictions

    var itemTemplate = GetTemplate(item.ItemTemplateId);
    if (itemTemplate == null) return null;

    var allowedTypes = containerTemplate.ContainerAllowedTypes
        .Split(',')
        .Select(t => t.Trim())
        .ToList();

    if (!allowedTypes.Contains(itemTemplate.ItemType.ToString()))
    {
        return $"This container is designed for {containerTemplate.ContainerAllowedTypes}. " +
               $"Placing {itemTemplate.ItemType} items is unusual but allowed.";
    }

    return null;
}
```

### Pattern 7: Drop Container with Contents
**What:** Confirm dialog when dropping container with items inside
**When to use:** Per CONTEXT.md - "Dropping containers: ask player each time via confirmation dialog"
**Example:**
```csharp
// Source: CONTEXT.md decision
private async Task ConfirmDropContainer(CharacterItem container)
{
    var contents = await itemDal.GetContainerContentsAsync(container.Id);
    if (!contents.Any())
    {
        // Empty container - normal drop
        await ConfirmDropItem();
        return;
    }

    var itemCount = contents.Count;
    var containerName = GetItemName(container);

    var message = $"'{containerName}' contains {itemCount} item(s). " +
                  "Do you want to drop the container with all its contents?";

    var options = new string[] { "Drop All", "Empty First", "Cancel" };
    var result = await DialogService.OpenAsync<DropContainerDialog>(
        "Drop Container?",
        new Dictionary<string, object>
        {
            { "Message", message },
            { "ContainerName", containerName },
            { "ItemCount", itemCount }
        });

    if (result == "DropAll")
    {
        await DropContainerWithContents(container.Id);
    }
    else if (result == "EmptyFirst")
    {
        await EmptyContainerToInventory(container.Id);
        successMessage = $"Items moved from {containerName} to inventory. Select drop again to remove the container.";
    }
}
```

### Anti-Patterns to Avoid
- **Direct DAL calls for container operations:** Use ItemManagementService.MoveToContainerAsync - handles validation
- **Blocking capacity enforcement:** CONTEXT.md specifies "warn but allow" - same as carrying capacity
- **Deep nesting:** CONTEXT.md limits to one level - nested containers cannot hold items
- **Silent type restriction:** CONTEXT.md says "warn only" - show message but allow placement
- **Forgetting magical container weight reduction:** Apply ContainerWeightReduction when calculating fill

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Container contents fetch | Custom query | ICharacterItemDal.GetContainerContentsAsync() | Already implemented, handles template population |
| Move to container | Custom update | ItemManagementService.MoveToContainerAsync() | Handles unequip, ammo validation |
| Container validity check | Custom IsContainer | ItemTemplate.IsContainer OR IsContainerItem() helper | Template field already exists |
| Weight calculation | Manual loop | Existing pattern from TabItems.razor | CalculateTotalWeight pattern |
| Drop confirmation | Custom modal | RadzenDialog.Confirm() | Already configured, used in Phase 4 |
| Equipment slot check | Custom list | EquipmentSlot enum + IsEquipmentSlotContainer() | Back, Waist slots for bags |

**Key insight:** The DAL and ItemManagementService already have container support. This phase is primarily UI work on top of existing infrastructure.

## Common Pitfalls

### Pitfall 1: Forgetting ContainerWeightReduction
**What goes wrong:** Magical containers (Bag of Holding) show wrong capacity
**Why it happens:** Not applying weight reduction factor when calculating fill
**How to avoid:** Always multiply contents weight by `template.ContainerWeightReduction` (default 1.0)
**Warning signs:** Bag of Holding shows 100% full with 10lbs when it should hold much more

### Pitfall 2: Allowing Items in Nested Containers
**What goes wrong:** Infinite nesting or confusing UI
**Why it happens:** Not checking if target container is itself inside another container
**How to avoid:** Check `container.ContainerItemId.HasValue` before allowing placement
**Warning signs:** Items "disappearing" into deeply nested containers

### Pitfall 3: Losing Container Contents on Drop
**What goes wrong:** Dropping container deletes all contents without warning
**Why it happens:** Not checking for contents before deletion
**How to avoid:** Check GetContainerContentsAsync before drop, show dialog per CONTEXT.md
**Warning signs:** Player loses items unexpectedly

### Pitfall 4: Not Refreshing Container Contents
**What goes wrong:** Container panel shows stale data after operations
**Why it happens:** Cache not invalidated after move/remove
**How to avoid:** Reload container contents after any container operation
**Warning signs:** Items appear in both inventory and container, or missing from both

### Pitfall 5: Equipped Items Moved to Container
**What goes wrong:** Equipped item moved to container but still shows equipped
**Why it happens:** ItemManagementService.MoveToContainerAsync handles this, but UI may not update
**How to avoid:** Call LoadItemsAsync after any container operation to refresh equipped status
**Warning signs:** Item shows in container AND in equipment slot

### Pitfall 6: Volume Not Tracked
**What goes wrong:** Container fills up but shows plenty of room
**Why it happens:** Only checking weight, not volume
**How to avoid:** Track BOTH weight and volume - container full when EITHER limit reached
**Warning signs:** Small container can hold unlimited small items

### Pitfall 7: Container Access During Combat Not Enforced
**What goes wrong:** Player accesses unequipped container during combat
**Why it happens:** Not checking equipped status in combat context
**How to avoid:** Per CONTEXT.md - "only equipped containers accessible during combat"
**Warning signs:** Player can freely access backpack sitting on ground during fight

## Code Examples

Verified patterns from existing project:

### Container Contents Display (from TabItems.razor)
```csharp
// Source: Threa/Threa.Client/Components/Pages/Character/TabItems.razor
private List<CharacterItem> GetContainerContents(Guid containerId)
{
    if (allItems == null)
        return new List<CharacterItem>();

    return allItems.Where(i => i.ContainerItemId == containerId).ToList();
}
```

### IsContainerItem Helper
```csharp
// Source: Inferred from ItemTemplate fields
private bool IsContainerItem(CharacterItem item)
{
    var template = GetTemplate(item.ItemTemplateId);
    if (template == null) return false;

    // Container by type
    if (template.IsContainer) return true;

    // Container by equipment slot (backpack, belt pouch)
    if (template.ItemType == ItemType.Container) return true;

    // AmmoContainer is a special container type
    if (template.ItemType == ItemType.AmmoContainer) return true;

    return false;
}
```

### MoveToContainerAsync from ItemManagementService
```csharp
// Source: GameMechanics/Items/ItemManagementService.cs (existing)
public async Task<ItemOperationResult> MoveToContainerAsync(
    CharacterEdit character,
    Guid itemId,
    Guid? containerItemId)
{
    try
    {
        var item = await _itemDal.GetItemAsync(itemId);
        if (item == null)
            return ItemOperationResult.Failed("Item not found.");

        // If item is equipped, need to unequip first
        if (item.IsEquipped && containerItemId.HasValue)
        {
            var unequipResult = await UnequipItemAsync(character, itemId);
            if (!unequipResult.Success)
                return unequipResult;
        }

        // Validate ammo compatibility if moving into an AmmoContainer
        if (containerItemId.HasValue)
        {
            var containerItem = await _itemDal.GetItemAsync(containerItemId.Value);
            if (containerItem != null)
            {
                var containerTemplate = await _templateDal.GetTemplateAsync(containerItem.ItemTemplateId);

                // If target is an AmmoContainer, validate ammo compatibility
                if (containerTemplate.ItemType == ItemType.AmmoContainer)
                {
                    // ... ammo validation logic
                }
            }
        }

        await _itemDal.MoveToContainerAsync(itemId, containerItemId);
        item.ContainerItemId = containerItemId;

        return ItemOperationResult.Succeeded(item);
    }
    catch (Exception ex)
    {
        return ItemOperationResult.Failed($"Failed to move item: {ex.Message}");
    }
}
```

### Container Capacity Calculation
```csharp
// Source: CONTEXT.md + ItemTemplate fields
private ContainerCapacity GetContainerCapacity(CharacterItem container)
{
    var template = GetTemplate(container.ItemTemplateId);
    if (template == null || !template.IsContainer)
        return new ContainerCapacity();

    var contents = GetContainerContents(container.Id);

    decimal currentWeight = 0;
    decimal currentVolume = 0;

    foreach (var item in contents)
    {
        var itemTemplate = GetTemplate(item.ItemTemplateId);
        if (itemTemplate == null) continue;

        currentWeight += itemTemplate.Weight * item.StackSize;
        currentVolume += itemTemplate.Volume * item.StackSize;
    }

    // Apply magical weight reduction
    currentWeight *= template.ContainerWeightReduction;

    return new ContainerCapacity
    {
        CurrentWeight = currentWeight,
        MaxWeight = template.ContainerMaxWeight ?? decimal.MaxValue,
        CurrentVolume = currentVolume,
        MaxVolume = template.ContainerMaxVolume ?? decimal.MaxValue,
        WeightPercent = template.ContainerMaxWeight > 0
            ? (currentWeight / template.ContainerMaxWeight.Value) * 100 : 0,
        VolumePercent = template.ContainerMaxVolume > 0
            ? (currentVolume / template.ContainerMaxVolume.Value) * 100 : 0,
        IsOverCapacity = (template.ContainerMaxWeight > 0 && currentWeight > template.ContainerMaxWeight) ||
                        (template.ContainerMaxVolume > 0 && currentVolume > template.ContainerMaxVolume)
    };
}

private record ContainerCapacity
{
    public decimal CurrentWeight { get; init; }
    public decimal MaxWeight { get; init; }
    public decimal CurrentVolume { get; init; }
    public decimal MaxVolume { get; init; }
    public decimal WeightPercent { get; init; }
    public decimal VolumePercent { get; init; }
    public bool IsOverCapacity { get; init; }
}
```

### Container Fill CSS Classes
```css
/* Source: CONTEXT.md Claude's Discretion */
.inventory-tile.is-container {
    /* Base container styling */
    position: relative;
}

.inventory-tile.container-empty::after {
    content: "";
    position: absolute;
    bottom: 0;
    left: 0;
    width: 100%;
    height: 3px;
    background: #6c757d;
}

.inventory-tile.container-partial::after {
    content: "";
    position: absolute;
    bottom: 0;
    left: 0;
    height: 3px;
    background: var(--bs-success);
    /* Width set dynamically via style attribute */
}

.inventory-tile.container-warning::after {
    content: "";
    position: absolute;
    bottom: 0;
    left: 0;
    height: 3px;
    background: var(--bs-warning);
}

.inventory-tile.container-full::after {
    content: "";
    position: absolute;
    bottom: 0;
    left: 0;
    width: 100%;
    height: 3px;
    background: var(--bs-danger);
}

/* Container icon modification based on fill */
.inventory-tile.container-full .item-icon {
    color: var(--bs-danger);
}

.inventory-tile.container-warning .item-icon {
    color: var(--bs-warning);
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Direct DAL for container ops | ItemManagementService.MoveToContainerAsync | Phase 2 | Unified validation |
| No container UI | Side panel container view | Phase 5 | Full container management |
| No capacity tracking | Weight + Volume tracking | Phase 5 | Physical constraints enforced |

**Deprecated/outdated:**
- Using direct DAL calls for container operations - use ItemManagementService instead
- Character creation container view (flat list) is adequate for creation but gameplay needs full panel

## Open Questions

Things that couldn't be fully resolved:

1. **Equipped Container Behavior Toggle**
   - What we know: CONTEXT.md says only equipped containers accessible during combat
   - What's unclear: Should there be a visual distinction for equipped vs unequipped containers?
   - Recommendation: Show equipped badge on container tiles like regular equipped items. Add "(Combat Ready)" label to equipped containers in panel header.

2. **Container Panel Layout When Equipment Slots Also Visible**
   - What we know: Equipment slots currently in right column, container panel also goes there
   - What's unclear: Should both be visible simultaneously or mutually exclusive?
   - Recommendation: Toggle between them - selecting container shows container panel, clicking elsewhere shows equipment slots. Per CONTEXT.md the side panel replaces equipment when container selected.

3. **Performance with Many Nested Items**
   - What we know: GetContainerContentsAsync called per container
   - What's unclear: Performance impact with many containers
   - Recommendation: Cache container contents in component, invalidate on operations. Pre-load all items once, filter client-side.

4. **Multi-Select for Bulk Move**
   - What we know: CONTEXT.md says "select item(s)" suggesting multi-select possible
   - What's unclear: Implementation complexity of multi-select
   - Recommendation: Start with single-select for MVP. Multi-select could be added as enhancement (Ctrl+click pattern).

## Sources

### Primary (HIGH confidence)
- Existing project code: `GameMechanics/Items/ItemManagementService.cs` - MoveToContainerAsync method
- Existing project code: `Threa.Dal/IItemDal.cs` - GetContainerContentsAsync, MoveToContainerAsync interfaces
- Existing project code: `Threa.Dal.MockDb/CharacterItemDal.cs` - Implementation patterns
- Existing project code: `Threa.Dal/Dto/ItemTemplate.cs` - Container fields (IsContainer, ContainerMax*, etc.)
- Existing project code: `Threa/Threa.Client/Components/Pages/Character/TabItems.razor` - GetContainerContents pattern
- Existing project code: `Threa/Threa.Client/Components/Pages/GamePlay/TabPlayInventory.razor` - Base component
- Design docs: `design/ITEM_SYSTEM_OVERVIEW.md` - Container hierarchy, weight calculation
- Design docs: `design/DATABASE_DESIGN.md` - Container schema
- Phase 5 CONTEXT.md decisions - All UI/UX requirements

### Secondary (MEDIUM confidence)
- Phase 4 RESEARCH.md - Patterns for inventory grid, equipment panel
- Phase 3 RESEARCH.md - Split-view layout patterns

### Tertiary (LOW confidence)
- None - all patterns from existing codebase

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All infrastructure already exists, just UI work
- Architecture: HIGH - Patterns proven in Phase 3 and 4, extending them
- Pitfalls: HIGH - Clear from existing code and CONTEXT.md constraints

**Research date:** 2026-01-25
**Valid until:** 2026-02-25 (30 days - stable technologies, extending existing patterns)
