# Phase 4: Gameplay Inventory Core - Research

**Researched:** 2026-01-25
**Domain:** Blazor gameplay inventory management with grid-based display, equipment slots, and equip/unequip operations
**Confidence:** HIGH

## Summary

This phase implements the player-facing inventory management interface on the Play page during active gameplay. The research focuses on transforming the placeholder TabPlayInventory.razor component into a fully functional inventory grid view and equipment slot interface that integrates with the existing ItemManagementService for proper cursed item handling.

The project already has robust infrastructure for this phase:
1. **ItemManagementService** - High-level service coordinating DAL operations with item effect lifecycle (equip/unequip/drop with curse blocking)
2. **ItemEffectService** - Handles curse detection via `CanUnequipItem()` and `CanDropItem()` methods
3. **ICharacterItemDal** - Complete interface for `GetCharacterItemsAsync`, `EquipItemAsync`, `UnequipItemAsync`, `DeleteItemAsync`
4. **EquipmentSlot enum + extensions** - 50+ equipment slots with display names and helper methods (`IsImplant()`, `IsWeaponSlot()`, etc.)

The key implementation decision is to use ItemManagementService (not direct DAL calls) for all equip/unequip/drop operations to ensure cursed item constraints are respected and item effects are properly applied/removed.

**Primary recommendation:** Replace TabPlayInventory.razor with a two-column layout: left column for grid-based inventory (CSS grid with item tiles), right column for equipment slots displayed as a categorized list. Use ItemManagementService for all operations. Use RadzenDialog.Confirm() for drop confirmation dialogs.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Radzen.Blazor | 8.4.2 | UI components (Dialog, DataGrid for fallback) | Already in project, DialogService for confirmations |
| Bootstrap 5 | (via Radzen) | CSS Grid layout, responsive columns | Already available, natural grid support |
| GameMechanics.Items.ItemManagementService | (project) | Equip/Unequip/Drop with curse handling | Coordinates DAL + effect system |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| GameMechanics.Effects.ItemEffectService | (project) | Curse checking, effect lifecycle | Internal to ItemManagementService |
| EquipmentSlotExtensions | (project) | Slot display names, slot categorization | UI display, slot compatibility |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| CSS Grid for inventory | RadzenDataGrid | Grid provides better visual item tiles; DataGrid better for lists |
| Bootstrap modal | RadzenDialog | RadzenDialog already set up in project, consistent styling |
| Direct DAL calls | ItemManagementService | Service handles curse blocking automatically - use service |

**Installation:**
No additional packages needed - all components already installed.

## Architecture Patterns

### Recommended Component Structure
```
Threa.Client/Components/Pages/GamePlay/
  TabPlayInventory.razor       # Full rewrite with grid + equipment slots
  Play.razor                   # Parent page (minimal changes - pass OnCharacterChanged)
```

### Pattern 1: Two-Column Layout (Inventory Grid + Equipment Slots)
**What:** Left column for scrollable inventory grid, right column for equipment slot list
**When to use:** Per CONTEXT.md decisions - grid-based inventory, list-based equipment slots
**Example:**
```razor
// Source: CONTEXT.md decisions + Bootstrap grid
<div class="row">
    <div class="col-md-7">
        <!-- Inventory Grid -->
        <h4>Inventory</h4>
        @RenderInventoryGrid()
    </div>
    <div class="col-md-5">
        <!-- Equipment Slots -->
        <h4>Equipment</h4>
        @RenderEquipmentSlots()
    </div>
</div>
```

### Pattern 2: CSS Grid for Item Tiles
**What:** Display items in a responsive grid of tiles with icons
**When to use:** Per CONTEXT.md - "Grid-based display with icons"
**Example:**
```razor
// Source: CONTEXT.md, CSS Grid best practices
<div class="inventory-grid" style="display: grid; grid-template-columns: repeat(auto-fill, minmax(80px, 1fr)); gap: 8px;">
    @foreach (var item in inventoryItems)
    {
        <div class="inventory-tile @(item.IsEquipped ? "equipped" : "") @(selectedItemId == item.Id ? "selected" : "")"
             @onclick="() => SelectItem(item)">
            <div class="item-icon">@GetItemIcon(item)</div>
            <div class="item-name">@item.Template?.Name</div>
            @if (item.Template?.IsStackable == true && item.StackSize > 1)
            {
                <div class="item-quantity">x@item.StackSize</div>
            }
            @if (item.IsEquipped)
            {
                <div class="equipped-badge" title="Equipped">E</div>
            }
        </div>
    }
</div>
```

### Pattern 3: Two-Step Equip Flow
**What:** Select item first, then click target equipment slot
**When to use:** Per CONTEXT.md - "Two-step interaction: click item in inventory, then click target equipment slot"
**Example:**
```csharp
// Source: CONTEXT.md decision
private CharacterItem? selectedItem;

private void SelectItem(CharacterItem item)
{
    // Toggle selection
    if (selectedItemId == item.Id)
        selectedItemId = null;
    else
        selectedItemId = item.Id;
    selectedItem = item;
}

private async Task OnEquipmentSlotClick(EquipmentSlot slot)
{
    if (selectedItem == null) return;

    // Check slot compatibility
    if (!IsSlotCompatible(selectedItem.Template, slot))
    {
        errorMessage = $"This item cannot be equipped in {slot.GetDisplayName()}.";
        return;
    }

    // Use ItemManagementService for equip (handles auto-swap and curse checking)
    var result = await itemService.EquipItemAsync(Character, selectedItem.Id, slot);
    if (!result.Success)
    {
        errorMessage = result.ErrorMessage;
        return;
    }

    selectedItem = null;
    selectedItemId = null;
    await ReloadItems();
    await NotifyCharacterChanged();
}
```

### Pattern 4: Equipment Slot List with Categories
**What:** Group equipment slots by category (Body, Weapons, Jewelry, Implants)
**When to use:** Per CONTEXT.md - "Show all equipment slots always, including empty ones"
**Example:**
```razor
// Source: CONTEXT.md + EquipmentSlotExtensions
@foreach (var category in slotCategories)
{
    <div class="slot-category mb-3">
        <h6 class="text-muted">@category.Name</h6>
        @foreach (var slot in category.Slots)
        {
            var equipped = GetEquippedItem(slot);
            <div class="equipment-slot @(IsSlotClickable(slot) ? "clickable" : "")"
                 @onclick="() => OnEquipmentSlotClick(slot)">
                <span class="slot-name">@slot.GetDisplayName()</span>
                @if (equipped != null)
                {
                    <span class="item-name">@equipped.Template?.Name</span>
                    @if (!CanUnequipItem(equipped.Id))
                    {
                        <span class="curse-indicator" title="Cursed - cannot unequip">!</span>
                    }
                }
                else
                {
                    <span class="empty-slot">Empty</span>
                }
            </div>
        }
    </div>
}
```

### Pattern 5: ItemManagementService for All Operations
**What:** Use service instead of direct DAL for equip/unequip/drop
**When to use:** ALWAYS for gameplay operations (ensures curse handling, effect lifecycle)
**Example:**
```csharp
// Source: GameMechanics/Items/ItemManagementService.cs
@inject ItemManagementService itemService

private async Task EquipItem(Guid itemId, EquipmentSlot slot)
{
    var result = await itemService.EquipItemAsync(Character, itemId, slot);
    if (!result.Success)
    {
        errorMessage = result.ErrorMessage;  // e.g., "Cannot unequip: cursed by..."
        return;
    }
    // Result.Success means item equipped, effects applied, old item auto-swapped
    await NotifyCharacterChanged();
}

private async Task UnequipItem(Guid itemId)
{
    var result = await itemService.UnequipItemAsync(Character, itemId);
    if (!result.Success)
    {
        errorMessage = result.ErrorMessage;  // Curse blocking message
        return;
    }
    await NotifyCharacterChanged();
}

private async Task DropItem(Guid itemId)
{
    var result = await itemService.RemoveItemFromInventoryAsync(Character, itemId);
    if (!result.Success)
    {
        errorMessage = result.ErrorMessage;  // Curse blocking message
        return;
    }
    await NotifyCharacterChanged();
}
```

### Pattern 6: RadzenDialog for Confirmations
**What:** Use DialogService.Confirm() for drop confirmation
**When to use:** Per CONTEXT.md - "Always confirm before dropping"
**Example:**
```csharp
// Source: Radzen documentation, CONTEXT.md
@inject DialogService DialogService

private async Task ConfirmAndDropItem()
{
    if (selectedItem == null) return;

    var itemName = selectedItem.CustomName ?? selectedItem.Template?.Name ?? "this item";
    var message = $"Are you sure you want to drop {itemName}? This cannot be undone.";

    // For stackable items with quantity > 1
    if (selectedItem.Template?.IsStackable == true && selectedItem.StackSize > 1)
    {
        // Show quantity dialog instead
        await ShowDropQuantityDialog(selectedItem);
        return;
    }

    var confirmed = await DialogService.Confirm(message, "Drop Item",
        new ConfirmOptions { OkButtonText = "Drop", CancelButtonText = "Cancel" });

    if (confirmed == true)
    {
        await DropItem(selectedItem.Id);
    }
}
```

### Anti-Patterns to Avoid
- **Direct DAL calls for equip/unequip/drop:** Use ItemManagementService - handles curse blocking and effect lifecycle
- **Skipping curse check on auto-swap:** ItemManagementService handles this; if target slot has cursed item, swap fails
- **Allowing implant unequip without check:** Implant slots should show warning/block (future phase)
- **Drag-and-drop for equipping:** CONTEXT.md specifies "two-step interaction" - click item then click slot

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Curse detection | Custom effect scanning | ItemManagementService.CanUnequipItem() / CanDropItem() | Already handles all cursor trigger types |
| Effect lifecycle on equip | Manual effect add/remove | ItemManagementService.EquipItemAsync() | Applies WhileEquipped effects automatically |
| Auto-swap on equip | Custom unequip-then-equip logic | DAL.EquipItemAsync() (via service) | DAL handles unequipping existing item |
| Slot display names | Switch statement | EquipmentSlotExtensions.GetDisplayName() | Already defined for all 50+ slots |
| Slot compatibility | Custom mapping | Template.EquipmentSlot + IsCompatibleSlot() | Service already validates |
| Confirmation dialogs | Custom modal | RadzenDialog.Confirm() | Project already has DialogService configured |

**Key insight:** The ItemManagementService was specifically built to coordinate DAL operations with the effect system. Bypassing it for "simplicity" will break curse handling and effect lifecycle.

## Common Pitfalls

### Pitfall 1: Bypassing ItemManagementService
**What goes wrong:** Cursed items can be unequipped, effects not applied/removed properly
**Why it happens:** Direct DAL calls seem simpler
**How to avoid:** ALWAYS use ItemManagementService for EquipItemAsync, UnequipItemAsync, RemoveItemFromInventoryAsync
**Warning signs:** Cursed items being unequipped, stat modifiers not changing on equip/unequip

### Pitfall 2: Forgetting Cursed Item Check on Auto-Swap
**What goes wrong:** Equipping to a slot with a cursed item succeeds when it should fail
**Why it happens:** Only checking if the NEW item can be equipped, not if OLD item can be unequipped
**How to avoid:** ItemManagementService handles this - don't circumvent. Check existing item's curse status in UI for better UX
**Warning signs:** "Cannot unequip: cursed" appearing AFTER equip animation starts

### Pitfall 3: Not Saving Character After Item Operations
**What goes wrong:** Character stat changes from item effects not persisted
**Why it happens:** Item operations update DAL but character effects need save
**How to avoid:** Call character save (via OnCharacterChanged callback) after successful item operations
**Warning signs:** Effects disappear on page refresh

### Pitfall 4: Equipped Items Missing from Inventory Grid
**What goes wrong:** Users can't see which items they have equipped
**Why it happens:** Filtering out equipped items from inventory view
**How to avoid:** Per CONTEXT.md: "Equipped items appear in both equipment slots AND inventory grid with visual indicator"
**Warning signs:** Item count mismatch, confusion about what's equipped

### Pitfall 5: Implant Slots Treated Like Normal Slots
**What goes wrong:** Players equipping/unequipping implants freely
**Why it happens:** Implant slots use same EquipmentSlot enum
**How to avoid:** Check `slot.IsImplant()` and show appropriate messaging (surgery required). For MVP, may just display warning.
**Warning signs:** Neural implants being swapped like rings

### Pitfall 6: DialogService Not Registered
**What goes wrong:** Confirm dialog throws null reference
**Why it happens:** RadzenDialog not in App.razor, DialogService not in DI
**How to avoid:** Verify `<RadzenDialog />` in App.razor, `AddRadzenComponents()` in Program.cs
**Warning signs:** Null reference on DialogService.Confirm()

## Code Examples

Verified patterns from existing project:

### ItemManagementService Usage (from existing code)
```csharp
// Source: GameMechanics/Items/ItemManagementService.cs

// Equipping with curse/effect handling
public async Task<ItemOperationResult> EquipItemAsync(
    CharacterEdit character,
    Guid itemId,
    EquipmentSlot slot)
{
    var item = await _itemDal.GetItemAsync(itemId);
    // ... validation ...

    // Equip in DAL (handles unequipping existing items)
    await _itemDal.EquipItemAsync(itemId, slot);

    // Apply equip effects (WhileEquipped)
    if (template.Effects.Count > 0)
    {
        await _effectService.OnItemEquippedAsync(character, item, template);
    }
    // ...
}

// Unequipping with curse check
public async Task<ItemOperationResult> UnequipItemAsync(
    CharacterEdit character,
    Guid itemId)
{
    // Check for curse blocking
    var curseCheck = _effectService.CanUnequipItem(character, itemId);
    if (!curseCheck.IsAllowed)
        return ItemOperationResult.Failed(curseCheck.BlockReason!);

    // Remove equip effects (but keep possession effects)
    _effectService.OnItemUnequipped(character, itemId);

    // Unequip in DAL
    await _itemDal.UnequipItemAsync(itemId);
    // ...
}
```

### Equipment Slot Categories
```csharp
// Source: EquipmentSlot.cs + EquipmentSlotExtensions.cs
private static readonly SlotCategory[] slotCategories = new[]
{
    new SlotCategory("Weapons", new[] {
        EquipmentSlot.MainHand,
        EquipmentSlot.OffHand,
        EquipmentSlot.TwoHand
    }),
    new SlotCategory("Head & Neck", new[] {
        EquipmentSlot.Head,
        EquipmentSlot.Face,
        EquipmentSlot.Ears,
        EquipmentSlot.Neck
    }),
    new SlotCategory("Body", new[] {
        EquipmentSlot.Shoulders,
        EquipmentSlot.Back,
        EquipmentSlot.Chest,
        EquipmentSlot.Waist
    }),
    new SlotCategory("Arms", new[] {
        EquipmentSlot.ArmLeft, EquipmentSlot.ArmRight,
        EquipmentSlot.WristLeft, EquipmentSlot.WristRight,
        EquipmentSlot.HandLeft, EquipmentSlot.HandRight
    }),
    new SlotCategory("Legs & Feet", new[] {
        EquipmentSlot.Legs,
        EquipmentSlot.AnkleLeft, EquipmentSlot.AnkleRight,
        EquipmentSlot.FootLeft, EquipmentSlot.FootRight
    }),
    new SlotCategory("Rings - Left", new[] {
        EquipmentSlot.FingerLeft1, EquipmentSlot.FingerLeft2,
        EquipmentSlot.FingerLeft3, EquipmentSlot.FingerLeft4,
        EquipmentSlot.FingerLeft5
    }),
    new SlotCategory("Rings - Right", new[] {
        EquipmentSlot.FingerRight1, EquipmentSlot.FingerRight2,
        EquipmentSlot.FingerRight3, EquipmentSlot.FingerRight4,
        EquipmentSlot.FingerRight5
    })
    // Note: Implants shown separately or hidden for MVP
};

private record SlotCategory(string Name, EquipmentSlot[] Slots);
```

### Inventory Tile CSS
```css
/* Source: CONTEXT.md + CSS Grid best practices */
.inventory-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(80px, 1fr));
    gap: 8px;
    max-height: 500px;
    overflow-y: auto;
    padding: 8px;
}

.inventory-tile {
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;
    padding: 8px;
    border: 1px solid #dee2e6;
    border-radius: 4px;
    cursor: pointer;
    background: #f8f9fa;
    min-height: 80px;
    transition: all 0.15s ease-in-out;
}

.inventory-tile:hover {
    border-color: #adb5bd;
    background: #e9ecef;
}

.inventory-tile.selected {
    border-color: var(--bs-primary);
    border-width: 2px;
    background: rgba(var(--bs-primary-rgb), 0.1);
}

.inventory-tile.equipped {
    border-color: var(--bs-success);
}

.equipped-badge {
    position: absolute;
    top: 2px;
    right: 2px;
    background: var(--bs-success);
    color: white;
    border-radius: 50%;
    width: 16px;
    height: 16px;
    font-size: 10px;
    display: flex;
    align-items: center;
    justify-content: center;
}

.item-quantity {
    position: absolute;
    bottom: 2px;
    right: 2px;
    background: rgba(0,0,0,0.6);
    color: white;
    padding: 0 4px;
    border-radius: 2px;
    font-size: 11px;
}

.item-icon {
    font-size: 24px;
    margin-bottom: 4px;
}

.item-name {
    font-size: 11px;
    text-align: center;
    overflow: hidden;
    text-overflow: ellipsis;
    max-width: 100%;
}
```

### Drop Confirmation with Quantity
```csharp
// Source: CONTEXT.md "For stackable items: prompt for quantity"
private async Task ShowDropQuantityDialog(CharacterItem item)
{
    var itemName = item.CustomName ?? item.Template?.Name ?? "item";

    // Simple prompt - RadzenDialog with numeric input
    var options = new Dictionary<string, object>
    {
        { "Item", item },
        { "MaxQuantity", item.StackSize }
    };

    var result = await DialogService.OpenAsync<DropQuantityDialog>(
        $"Drop {itemName}",
        options,
        new DialogOptions { Width = "300px" });

    if (result is int quantity && quantity > 0)
    {
        await DropQuantity(item, quantity);
    }
}

// Simple alternative: drop all with confirmation
private async Task DropAllWithConfirm(CharacterItem item)
{
    var message = item.StackSize > 1
        ? $"Drop all {item.StackSize} {item.Template?.Name}?"
        : $"Drop {item.Template?.Name}?";

    var confirmed = await DialogService.Confirm(message, "Confirm Drop");
    if (confirmed == true)
    {
        await itemService.RemoveItemFromInventoryAsync(Character, item.Id);
    }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Direct DAL for item ops | ItemManagementService | Phase 2 | Unified curse/effect handling |
| Placeholder TabPlayInventory | Full inventory grid + equipment | Phase 4 | Complete gameplay inventory UI |
| Static record types | CharacterItem with Template | Phase 2 | Full item metadata available |

**Deprecated/outdated:**
- The placeholder `QuickUseItem`, `EquippedItem`, `CombatItem` records in TabPlayInventory.razor will be removed
- Direct `itemDal` calls in gameplay context - use `ItemManagementService` instead

## Open Questions

Things that couldn't be fully resolved:

1. **Item Icons**
   - What we know: CONTEXT.md says "grid-based display with icons"
   - What's unclear: Where icons come from - ItemTemplate has no IconUrl field currently
   - Recommendation: Use text-based icons (first letter, emoji by ItemType) for MVP. Add IconUrl field in future phase.

2. **Animation for Equip/Unequip**
   - What we know: CONTEXT.md says "Animation/transition visual feedback"
   - What's unclear: Complexity of animation desired
   - Recommendation: Simple CSS transition (fade, scale) on tile state change. Complex animations deferred.

3. **Implant Slots Display**
   - What we know: Implants exist in EquipmentSlot, require surgery to change
   - What's unclear: Should they show on Play page at all?
   - Recommendation: Show implants in separate "Implants" category with "Surgical removal required" note. Don't allow equip/unequip in this phase.

4. **Quantity Dialog Implementation**
   - What we know: CONTEXT.md requires "prompt for quantity" on stackable item drop
   - What's unclear: RadzenDialog custom component vs simple input
   - Recommendation: Create simple DropQuantityDialog.razor component or use prompt with validation. Low complexity.

## Sources

### Primary (HIGH confidence)
- Existing project code: `GameMechanics/Items/ItemManagementService.cs` - service for all item operations
- Existing project code: `GameMechanics/Effects/ItemEffectService.cs` - curse checking methods
- Existing project code: `Threa.Dal/Dto/EquipmentSlot.cs` - all equipment slots
- Existing project code: `Threa.Dal/Dto/EquipmentSlotExtensions.cs` - slot helpers
- Existing project code: `Threa.Client/Components/Pages/Character/TabItems.razor` - character creation patterns
- Existing project code: `Threa.Client/Components/Pages/GamePlay/Play.razor` - parent page structure
- Phase 4 CONTEXT.md decisions - all UI/UX requirements

### Secondary (MEDIUM confidence)
- [Radzen DialogService](https://blazor.radzen.com/docs/api/Radzen.DialogService.html) - Confirm() method signature

### Tertiary (LOW confidence)
- None - all patterns from existing codebase or official docs

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All components already in use in project
- Architecture: HIGH - ItemManagementService pattern well-established, slot system complete
- Pitfalls: HIGH - Clear from existing service code what must not be bypassed

**Research date:** 2026-01-25
**Valid until:** 2026-02-25 (30 days - stable technologies, extending existing patterns)
