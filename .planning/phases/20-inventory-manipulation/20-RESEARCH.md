# Phase 20: Inventory Manipulation - Research

**Researched:** 2026-01-29
**Domain:** GM Inventory Management with CSLA.NET, Blazor, and Real-time Updates
**Confidence:** HIGH

## Summary

Phase 20 implements a comprehensive GM-facing inventory management system for directly manipulating character inventories during gameplay. The existing codebase provides an extensive foundation:

- **ItemManagementService** - High-level service coordinating DAL and effect system for all item operations
- **CharacterItemEdit** (CSLA BusinessBase) - Item instance business object with full CRUD support
- **ItemTemplateInfo/ItemTemplateList** (CSLA ReadOnlyBase/ListBase) - Template browsing infrastructure
- **CharacterDetailInventory.razor** - Read-only inventory display (to be enhanced)
- **EffectTemplatePickerModal.razor** - Reference pattern for item template picker UI

The primary work is enhancing the existing **CharacterDetailInventory** component to support GM actions (add/remove/equip/unequip) and creating an **ItemTemplatePickerModal** following the EffectTemplatePickerModal pattern. The ItemManagementService already handles all complex operations including curse checking, container management, and effect lifecycle.

**Primary recommendation:** Enhance CharacterDetailInventory with inline action menus and quantity editing, create ItemTemplatePickerModal mirroring EffectTemplatePickerModal, integrate with existing ItemManagementService and CharacterUpdateMessage infrastructure.

## Standard Stack

### Core (Already Exists)

| Library/Component | Version | Purpose | Status |
|-------------------|---------|---------|--------|
| CSLA.NET | 9.1.0 | Business object framework | In use |
| Blazor Server | .NET 10 | UI framework with SSR + InteractiveServer | In use |
| Radzen.Blazor | 8.4.2 | UI component library (DialogService) | In use |
| ItemManagementService | Current | Coordinates item operations with effect system | In use |
| ITimeEventPublisher | Current | Real-time update notifications | In use |

### Needs Creation/Enhancement

| Component | Purpose | Pattern Source |
|-----------|---------|----------------|
| ItemTemplatePickerModal.razor | Browse and select item templates | EffectTemplatePickerModal.razor |
| CharacterDetailInventory.razor | Enhanced with GM actions | CharacterDetailEffects.razor pattern |
| Add quantity prompt | Post-selection quantity input | Custom inline form |

### Supporting (Already Exists)

| Component | Purpose | Notes |
|-----------|---------|-------|
| ICharacterItemDal | Item CRUD operations | GetCharacterItemsAsync, AddItemAsync, etc. |
| IItemTemplateDal | Template lookup | GetAllTemplatesAsync, SearchTemplatesAsync |
| CharacterUpdateMessage | Real-time notifications | CharacterUpdateType.InventoryChanged |
| ItemTemplateList | CSLA ReadOnlyListBase | Template collection for picker |
| CharacterEdit.Currency | Currency properties | CopperCoins, SilverCoins, GoldCoins, PlatinumCoins |

## Architecture Patterns

### Recommended Component Structure

```
Threa.Client/Components/Shared/
    CharacterDetailInventory.razor    # Enhanced with GM actions (modify existing)
    ItemTemplatePickerModal.razor     # Browse/select item templates (new)
    QuantityInputPopover.razor        # Inline quantity input (optional - may inline)
```

### Pattern 1: Item Template Picker Modal (from EffectTemplatePickerModal)

**What:** Modal with searchable grid for browsing and selecting item templates
**When to use:** GM adding new items to character inventory
**Example (adapted from EffectTemplatePickerModal.razor):**
```razor
<div class="row g-3 mb-4">
    <div class="col-md-6">
        <div class="input-group">
            <span class="input-group-text"><i class="bi bi-search"></i></span>
            <input type="text" class="form-control" placeholder="Search items..."
                   @bind="searchTerm" @bind:event="oninput" @bind:after="OnSearchInput" />
        </div>
    </div>
    <div class="col-md-3">
        <select class="form-select" @bind="filterType">
            <option value="">All Types</option>
            @foreach (var type in Enum.GetValues<ItemType>())
            {
                <option value="@type">@type</option>
            }
        </select>
    </div>
    <div class="col-md-3">
        <select class="form-select" @bind="filterRarity">
            <option value="">All Rarities</option>
            @foreach (var rarity in Enum.GetValues<ItemRarity>())
            {
                <option value="@rarity">@rarity</option>
            }
        </select>
    </div>
</div>
```

### Pattern 2: Context Menu for Item Actions (from CharacterDetailEffects)

**What:** Dropdown menu per item for actions (remove, equip, unequip, move)
**When to use:** Inline item management in inventory grid
**Example:**
```razor
<div class="dropdown">
    <button class="btn btn-sm btn-outline-secondary" data-bs-toggle="dropdown">
        <i class="bi bi-three-dots-vertical"></i>
    </button>
    <ul class="dropdown-menu">
        @if (item.IsEquipped)
        {
            <li><button class="dropdown-item" @onclick="() => UnequipItem(item)">
                <i class="bi bi-box-arrow-down me-2"></i>Unequip
            </button></li>
        }
        else if (CanEquip(item))
        {
            <li><button class="dropdown-item" @onclick="() => EquipItem(item)">
                <i class="bi bi-box-arrow-in-up me-2"></i>Equip
            </button></li>
        }
        <li><button class="dropdown-item text-danger" @onclick="() => RemoveItem(item)">
            <i class="bi bi-trash me-2"></i>Remove
        </button></li>
    </ul>
</div>
```

### Pattern 3: Inline Feedback (from CONTEXT.md decisions)

**What:** Success/error messages displayed inline within inventory view
**When to use:** All inventory operations (add, remove, equip, etc.)
**Example:**
```razor
@if (!string.IsNullOrEmpty(feedbackMessage))
{
    <div class="alert @feedbackClass alert-dismissible fade show" role="alert">
        <i class="bi @feedbackIcon me-1"></i>@feedbackMessage
        <button type="button" class="btn-close" @onclick="ClearFeedback"></button>
    </div>
}
```

### Pattern 4: Quantity Input Flow (per CONTEXT.md)

**What:** Prompt for quantity after template selection, before adding to inventory
**When to use:** Adding stackable items
**Flow:**
1. GM opens ItemTemplatePickerModal
2. GM selects item template
3. If item.IsStackable, show quantity input (default: 1)
4. If item is container, optionally show container target selection
5. GM confirms, item added with specified quantity

### Pattern 5: Currency Editing Integration

**What:** Inline editable currency fields in Inventory tab
**When to use:** GM needs to adjust character currency
**Example:**
```razor
<div class="card">
    <div class="card-header d-flex justify-content-between align-items-center">
        <strong>Currency</strong>
        @if (!isEditingCurrency)
        {
            <button class="btn btn-sm btn-outline-secondary" @onclick="StartEditCurrency">
                <i class="bi bi-pencil"></i>
            </button>
        }
    </div>
    <div class="card-body">
        @if (isEditingCurrency)
        {
            <div class="row g-2">
                <div class="col-3">
                    <input type="number" class="form-control form-control-sm"
                           @bind="editPlatinum" min="0" />
                    <small class="text-muted">pp</small>
                </div>
                <!-- Similar for gp, sp, cp -->
            </div>
            <div class="mt-2">
                <button class="btn btn-sm btn-success" @onclick="SaveCurrency">Save</button>
                <button class="btn btn-sm btn-secondary" @onclick="CancelEditCurrency">Cancel</button>
            </div>
        }
        else
        {
            <div class="d-flex gap-3">
                <span><strong>@Character.PlatinumCoins</strong> pp</span>
                <!-- ... -->
            </div>
        }
    </div>
</div>
```

### Anti-Patterns to Avoid

- **Bypassing ItemManagementService:** Never call DAL directly for item operations - always use ItemManagementService to ensure effect lifecycle is handled
- **Not checking curse status:** Use `ItemManagementService.CanUnequipItem()` and `CanDropItem()` before operations
- **Forgetting CharacterUpdateMessage:** Every inventory change must publish CharacterUpdateType.InventoryChanged
- **Not refreshing character after operations:** Always re-fetch CharacterEdit after ItemManagementService operations
- **Manual equipped badge management:** The IsEquipped property on CharacterItem already tracks this

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Item add/remove | Direct DAL calls | ItemManagementService.AddItemToInventoryAsync/RemoveItemFromInventoryAsync | Handles effect lifecycle, curse checks |
| Equip/unequip | Direct DAL calls | ItemManagementService.EquipItemAsync/UnequipItemAsync | Curse checking, effect triggers |
| Curse detection | Manual effect scanning | ItemManagementService.CanUnequipItem/CanDropItem | Already implemented correctly |
| Real-time updates | Custom SignalR | ITimeEventPublisher.PublishCharacterUpdateAsync | Dashboard integration exists |
| Container validation | Manual checking | ItemManagementService.MoveToContainerAsync | Ammo compatibility, weight/volume checks |
| Template searching | Manual filtering | IItemTemplateDal.SearchTemplatesAsync | SQL-optimized search |

**Key insight:** ItemManagementService is the single entry point for all item operations. It already handles the complex interactions between DAL, effect system, and curse mechanics.

## Common Pitfalls

### Pitfall 1: Not Publishing Inventory Updates

**What goes wrong:** GM changes inventory, player dashboard doesn't update
**Why it happens:** Forgetting to call ITimeEventPublisher after ItemManagementService operations
**How to avoid:** Every ItemManagementService call should be followed by PublishCharacterUpdateAsync with UpdateType.InventoryChanged
**Warning signs:** Inventory changes work locally but not on player's view

### Pitfall 2: Equipped Item Removal Without Unequip

**What goes wrong:** Error when trying to remove equipped items
**Why it happens:** Not auto-unequipping before removal
**How to avoid:** Per CONTEXT.md, auto-unequip then remove (ItemManagementService.RemoveItemFromInventoryAsync handles this if item is equipped)
**Warning signs:** "Item is equipped" errors on removal

### Pitfall 3: Curse Blocking Without UI Feedback

**What goes wrong:** Items can't be removed/unequipped with no explanation
**Why it happens:** Not checking and displaying curse block reasons
**How to avoid:** Use ItemManagementService.GetUnequipBlockReason/GetDropBlockReason and display in UI
**Warning signs:** Silent operation failures

### Pitfall 4: Stale Character State

**What goes wrong:** Wrong items modified, duplicate items
**Why it happens:** Using cached Character parameter instead of fresh fetch
**How to avoid:** Always `await characterPortal.FetchAsync(CharacterId)` before modifications
**Warning signs:** Items appearing/disappearing unexpectedly

### Pitfall 5: Missing Confirmation for Rare Items

**What goes wrong:** Accidental deletion of valuable items
**Why it happens:** No confirmation prompt for rare/equipped items
**How to avoid:** Per CONTEXT.md, confirm only for equipped or rare items (Rarity >= Rare)
**Warning signs:** GM complaints about accidental item removal

### Pitfall 6: Container Target Selection Complexity

**What goes wrong:** Items can't be added to correct container
**Why it happens:** Not providing container selection during add flow
**How to avoid:** Per CONTEXT.md, GM can choose target container during add - include container dropdown
**Warning signs:** All items going to main inventory only

## Code Examples

### Adding Item from Template (ItemManagementService Integration)

```csharp
// Source: Adapted from ItemManagementService pattern
private async Task AddItemFromTemplate(ItemTemplateInfo template, int quantity, Guid? containerItemId)
{
    isProcessing = true;
    ClearFeedback();

    try
    {
        // Re-fetch character for fresh state
        var character = await characterPortal.FetchAsync(CharacterId);

        // Create CharacterItem from template
        var item = new CharacterItem
        {
            Id = Guid.NewGuid(),
            ItemTemplateId = template.Id,
            OwnerCharacterId = CharacterId,
            ContainerItemId = containerItemId,
            StackSize = quantity,
            IsEquipped = false,
            EquippedSlot = EquipmentSlot.None,
            CreatedAt = DateTime.UtcNow
        };

        // Use ItemManagementService for proper effect handling
        var result = await itemManagementService.AddItemToInventoryAsync(character, item);

        if (result.Success)
        {
            // Save character
            await characterPortal.UpdateAsync(character);

            // Publish real-time update
            await timeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
            {
                CharacterId = CharacterId,
                UpdateType = CharacterUpdateType.InventoryChanged,
                CampaignId = TableId.ToString(),
                SourceId = "GM",
                Description = $"Added {quantity}x {template.Name} to inventory"
            });

            ShowFeedback($"Added {quantity}x {template.Name}", "alert-success", "bi-check-circle");
            await RefreshItems();
        }
        else
        {
            ShowFeedback(result.ErrorMessage!, "alert-danger", "bi-exclamation-circle");
        }
    }
    catch (Exception ex)
    {
        ShowFeedback($"Error: {ex.Message}", "alert-danger", "bi-exclamation-circle");
    }
    finally
    {
        isProcessing = false;
    }
}
```

### Remove Item with Confirmation (per CONTEXT.md)

```csharp
// Source: Adapted from existing patterns
private async Task RemoveItem(CharacterItem item)
{
    // Get template for rarity check
    var template = templates?.GetValueOrDefault(item.ItemTemplateId);
    var needsConfirmation = item.IsEquipped || (template?.Rarity >= ItemRarity.Rare);

    if (needsConfirmation)
    {
        var confirmed = await DialogService.Confirm(
            $"Remove {GetItemName(item)}?",
            "Confirm Removal",
            new ConfirmOptions { OkButtonText = "Remove", CancelButtonText = "Cancel" });

        if (confirmed != true) return;
    }

    isProcessing = true;
    ClearFeedback();

    try
    {
        var character = await characterPortal.FetchAsync(CharacterId);

        // Check for curse blocking
        var blockReason = itemManagementService.GetDropBlockReason(character, item.Id);
        if (blockReason != null)
        {
            ShowFeedback($"Cannot remove: {blockReason}", "alert-warning", "bi-exclamation-triangle");
            return;
        }

        var result = await itemManagementService.RemoveItemFromInventoryAsync(character, item.Id);

        if (result.Success)
        {
            await characterPortal.UpdateAsync(character);

            await timeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
            {
                CharacterId = CharacterId,
                UpdateType = CharacterUpdateType.InventoryChanged,
                CampaignId = TableId.ToString(),
                SourceId = "GM",
                Description = $"Removed {GetItemName(item)} from inventory"
            });

            ShowFeedback($"Removed {GetItemName(item)}", "alert-success", "bi-check-circle");
            await RefreshItems();
        }
        else
        {
            ShowFeedback(result.ErrorMessage!, "alert-danger", "bi-exclamation-circle");
        }
    }
    catch (Exception ex)
    {
        ShowFeedback($"Error: {ex.Message}", "alert-danger", "bi-exclamation-circle");
    }
    finally
    {
        isProcessing = false;
    }
}
```

### Equip Item to Slot

```csharp
// Source: Adapted from ItemManagementService
private async Task EquipItem(CharacterItem item)
{
    isProcessing = true;
    ClearFeedback();

    try
    {
        var character = await characterPortal.FetchAsync(CharacterId);
        var template = templates?.GetValueOrDefault(item.ItemTemplateId);

        if (template == null)
        {
            ShowFeedback("Item template not found", "alert-danger", "bi-exclamation-circle");
            return;
        }

        // Determine target slot from template
        var targetSlot = template.EquipmentSlot;
        if (targetSlot == EquipmentSlot.None)
        {
            ShowFeedback("This item cannot be equipped", "alert-warning", "bi-exclamation-triangle");
            return;
        }

        var result = await itemManagementService.EquipItemAsync(character, item.Id, targetSlot);

        if (result.Success)
        {
            await characterPortal.UpdateAsync(character);

            await timeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
            {
                CharacterId = CharacterId,
                UpdateType = CharacterUpdateType.InventoryChanged,
                CampaignId = TableId.ToString(),
                SourceId = "GM",
                Description = $"Equipped {GetItemName(item)} to {targetSlot.GetDisplayName()}"
            });

            ShowFeedback($"Equipped {GetItemName(item)}", "alert-success", "bi-check-circle");
            await RefreshItems();
        }
        else
        {
            ShowFeedback(result.ErrorMessage!, "alert-danger", "bi-exclamation-circle");
        }
    }
    catch (Exception ex)
    {
        ShowFeedback($"Error: {ex.Message}", "alert-danger", "bi-exclamation-circle");
    }
    finally
    {
        isProcessing = false;
    }
}
```

### Currency Editing

```csharp
// Source: New pattern for currency editing
private int editPlatinum, editGold, editSilver, editCopper;
private bool isEditingCurrency = false;

private void StartEditCurrency()
{
    editPlatinum = Character?.PlatinumCoins ?? 0;
    editGold = Character?.GoldCoins ?? 0;
    editSilver = Character?.SilverCoins ?? 0;
    editCopper = Character?.CopperCoins ?? 0;
    isEditingCurrency = true;
}

private async Task SaveCurrency()
{
    isProcessing = true;
    ClearFeedback();

    try
    {
        var character = await characterPortal.FetchAsync(CharacterId);

        character.PlatinumCoins = Math.Max(0, editPlatinum);
        character.GoldCoins = Math.Max(0, editGold);
        character.SilverCoins = Math.Max(0, editSilver);
        character.CopperCoins = Math.Max(0, editCopper);

        await characterPortal.UpdateAsync(character);

        await timeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
        {
            CharacterId = CharacterId,
            UpdateType = CharacterUpdateType.InventoryChanged,
            CampaignId = TableId.ToString(),
            SourceId = "GM",
            Description = "Currency updated"
        });

        isEditingCurrency = false;
        ShowFeedback("Currency updated", "alert-success", "bi-check-circle");

        // Refresh Character reference
        Character = character;
    }
    catch (Exception ex)
    {
        ShowFeedback($"Error: {ex.Message}", "alert-danger", "bi-exclamation-circle");
    }
    finally
    {
        isProcessing = false;
    }
}

private void CancelEditCurrency()
{
    isEditingCurrency = false;
}
```

### Item Rarity Badge Styling

```csharp
// Source: Standard rarity color coding
private static string GetRarityBadgeClass(ItemRarity rarity) => rarity switch
{
    ItemRarity.Common => "bg-secondary",
    ItemRarity.Uncommon => "bg-success",
    ItemRarity.Rare => "bg-primary",
    ItemRarity.Epic => "bg-purple",
    ItemRarity.Legendary => "bg-warning text-dark",
    _ => "bg-secondary"
};

private static string GetItemTypeIcon(ItemType type) => type switch
{
    ItemType.Weapon => "bi-sword",
    ItemType.Armor => "bi-shield-fill",
    ItemType.Shield => "bi-shield-shaded",
    ItemType.Ammunition => "bi-bullseye",
    ItemType.AmmoContainer => "bi-box",
    ItemType.Consumable => "bi-capsule",
    ItemType.Container => "bi-box-seam",
    ItemType.Jewelry => "bi-gem",
    ItemType.Clothing => "bi-person-arms-up",
    ItemType.Food => "bi-egg-fried",
    ItemType.Drink => "bi-cup-straw",
    ItemType.Tool => "bi-tools",
    ItemType.Key => "bi-key",
    ItemType.Magic => "bi-magic",
    ItemType.Treasure => "bi-coin",
    ItemType.RawMaterial => "bi-box-seam-fill",
    ItemType.Miscellaneous => "bi-question-circle",
    _ => "bi-question-circle"
};
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Direct DAL item operations | ItemManagementService | Item effects system | Effect lifecycle auto-managed |
| Manual curse checking | Service-level curse checks | Item effects system | Curse blocking integrated |
| No real-time inventory updates | CharacterUpdateMessage | v1.2 | Dashboard reflects changes instantly |

**Deprecated/outdated:**
- Direct ICharacterItemDal calls for add/remove/equip: Use ItemManagementService wrapper
- Manual effect application on equip: ItemManagementService handles this

## Open Questions

### Question 1: Slot Selection for Multi-Slot Items

**What we know:** Weapons can go in MainHand, OffHand, or TwoHand depending on type
**What's unclear:** Whether GM should pick slot or auto-select based on template
**Recommendation:** Auto-select from template.EquipmentSlot, but allow GM override for weapons (show slot picker for Weapon type items)

### Question 2: Stack Quantity Modification

**What we know:** INVT-04 requires GM can modify quantity of stackable items
**What's unclear:** Whether this is inline edit or separate dialog
**Recommendation:** Inline quantity input field in item action dropdown for stackable items (no dialog needed)

### Question 3: Container Content Display

**What we know:** CONTEXT.md says show "Container organization (items grouped by container)"
**What's unclear:** Whether to show nested tree or flat with container labels
**Recommendation:** Flat list with container name badge on items inside containers (simpler, less UI complexity)

## Sources

### Primary (HIGH confidence)

- **Codebase analysis:** GameMechanics/Items/, Threa.Client/Components/Shared/*.razor
- **Design documents:** design/EQUIPMENT_SYSTEM.md, design/ITEM_EFFECTS_SYSTEM.md, design/DATABASE_DESIGN.md
- **Phase context:** .planning/phases/20-inventory-manipulation/20-CONTEXT.md
- **Reference patterns:** EffectTemplatePickerModal.razor, CharacterDetailEffects.razor

### Secondary (MEDIUM confidence)

- **CSLA patterns:** Observed from existing BusinessBase/BusinessListBase usage in codebase
- **Blazor patterns:** Observed from existing Radzen DialogService usage

### Tertiary (LOW confidence)

None - all findings derived from codebase analysis.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Verified from codebase, all components exist or have clear patterns
- Architecture: HIGH - Existing patterns well-established (effect management, item system)
- Pitfalls: HIGH - Based on existing code patterns and ItemManagementService requirements

**Research date:** 2026-01-29
**Valid until:** N/A (internal codebase research, stable patterns)

---

## Implementation Checklist Summary

### New Files Required

1. **Threa.Client/Components/Shared/ItemTemplatePickerModal.razor** - Browse/select item templates

### Existing Files to Modify

1. **CharacterDetailInventory.razor** - Add GM actions, context menus, currency editing
2. **CharacterDetailModal.razor** - Pass TableId to CharacterDetailInventory (already passes Character)

### No Database Changes Required

All required tables and DAL interfaces already exist:
- ItemTemplates table with IItemTemplateDal
- Items table with ICharacterItemDal
- Character currency properties on CharacterEdit

### Service Registration

ItemManagementService is already registered via `services.AddGameMechanics()` in ServiceCollectionExtensions.cs
