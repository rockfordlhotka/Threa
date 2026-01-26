# Phase 7: Item Distribution - Research

**Researched:** 2026-01-25
**Domain:** GM item granting workflow, real-time player notifications, Blazor InteractiveServer
**Confidence:** HIGH

## Summary

This phase implements the Game Master's ability to grant items to player characters during gameplay sessions. The research focused on understanding the existing infrastructure: GmTable.razor already displays characters at a table, ItemManagementService handles item creation with effect application, and the ITimeEventPublisher/Subscriber system provides real-time character update notifications.

The codebase already has all the building blocks:
1. **GmTable.razor** displays characters at the current table via TableCharacterList
2. **ItemManagementService.AddItemToInventoryAsync()** creates CharacterItem instances from templates with proper effect handling
3. **CharacterUpdateMessage** with CharacterUpdateType enum provides the notification mechanism
4. **Play.razor** subscribes to CharacterUpdateReceived and already calls LoadEquippedItemsAsync() on updates

The primary work is adding a new UI section to GmTable.razor for item granting, creating a new CharacterUpdateType.InventoryChanged enum value, and ensuring TabPlayInventory.razor receives the update message to reload inventory.

**Primary recommendation:** Add an "Item Distribution" panel to GmTable.razor with a split-view pattern: item template browser on top, character selector below. Single-click item template + selected character triggers ItemManagementService.AddItemToInventoryAsync(), followed by CharacterUpdateMessage publication. Player's Play.razor already handles updates; extend TabPlayInventory to reload on character change.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Csla.NET | 9.1.0 | Business objects (TableCharacterList, CharacterItem) | Already used throughout project |
| Radzen.Blazor | 8.4.2 | UI components (DataGrid, DropDown) | Already used in GmTable.razor |
| GameMechanics.Messaging | (in-project) | ITimeEventPublisher for notifications | Established pattern from GmTable damage/heal |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Bootstrap 5 | (via Radzen) | Split-view layout | Item browser + character list layout |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| CharacterUpdateMessage | New InventoryUpdateMessage | Existing message type simpler; add enum value instead |
| DataGrid for templates | Simple list | DataGrid provides filtering/search already proven in Phase 2 |
| Modal dialog for granting | Inline panel | Panel fits GmTable column layout; avoids modal context switching |

**Installation:**
No new packages needed - all functionality uses existing dependencies.

## Architecture Patterns

### Recommended Component Structure
```
Threa.Client/Components/Pages/GamePlay/
  GmTable.razor                # MODIFY: Add Item Distribution panel
  GmItemDistribution.razor     # NEW: Reusable item grant component (optional, can be inline)
  TabPlayInventory.razor       # MODIFY: Add OnParametersSetAsync reload
```

### Pattern 1: Item Distribution Panel in GmTable
**What:** New panel in GmTable.razor for selecting items and granting to characters
**When to use:** GM item distribution during gameplay
**Example:**
```razor
// Source: Existing GmTable.razor column layout + Phase 3 browse pattern
<!-- Inside GmTable.razor, after NPCs Panel -->
<div class="card">
    <div class="card-header d-flex justify-content-between align-items-center">
        <strong>Item Distribution</strong>
    </div>
    <div class="card-body">
        <!-- Item Template Browser (compact) -->
        <div class="mb-2">
            <div class="input-group input-group-sm">
                <RadzenTextBox @bind-Value="@itemSearchText" Placeholder="Search items..."
                    Style="flex: 1" />
                <RadzenDropDown TValue="ItemType?" Data="@itemTypes"
                    @bind-Value="@selectedItemType" Placeholder="Type"
                    Style="width: 100px" />
            </div>
        </div>

        <div style="max-height: 200px; overflow-y: auto;">
            @foreach (var template in filteredTemplates.Take(10))
            {
                <div class="item-row @(selectedTemplate?.Id == template.Id ? "selected" : "")"
                     @onclick="() => SelectTemplate(template)">
                    @template.Name <small class="text-muted">(@template.ItemType)</small>
                </div>
            }
        </div>

        <!-- Quantity and Grant -->
        @if (selectedTemplate != null && selectedCharacter != null)
        {
            <div class="mt-2 p-2 bg-light rounded">
                <strong>@selectedTemplate.Name</strong> to <strong>@selectedCharacter.CharacterName</strong>
                <div class="input-group input-group-sm mt-1">
                    <span class="input-group-text">Qty</span>
                    <input type="number" class="form-control" @bind="grantQuantity" min="1" max="100" />
                    <button class="btn btn-success" @onclick="GrantItem">Grant</button>
                </div>
            </div>
        }
        else
        {
            <p class="text-muted small mt-2">Select an item and a character to grant.</p>
        }
    </div>
</div>
```

### Pattern 2: Reuse Character Selection from Table List
**What:** The selectedCharacter already exists in GmTable.razor for damage/heal operations
**When to use:** When granting items - same character selection pattern
**Example:**
```csharp
// Source: Existing GmTable.razor pattern
// selectedCharacter is already populated when GM clicks a character card
// Just check it's not null before granting

private async Task GrantItem()
{
    if (selectedCharacter == null || selectedTemplate == null || table == null)
        return;

    // Create item using existing service
    var newItem = new CharacterItem
    {
        Id = Guid.NewGuid(),
        ItemTemplateId = selectedTemplate.Id,
        OwnerCharacterId = selectedCharacter.CharacterId,
        StackSize = grantQuantity,
        CurrentDurability = selectedTemplate.HasDurability ? selectedTemplate.MaxDurability : null,
        CreatedAt = DateTime.UtcNow
    };

    // Use ItemManagementService for proper effect handling
    var result = await itemManagementService.AddItemToInventoryAsync(
        await characterPortal.FetchAsync(selectedCharacter.CharacterId),
        newItem);

    if (!result.Success)
    {
        errorMessage = result.ErrorMessage;
        return;
    }

    // Notify player
    await TimeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
    {
        CharacterId = selectedCharacter.CharacterId,
        UpdateType = CharacterUpdateType.InventoryChanged,
        CampaignId = table.Id.ToString(),
        SourceId = "GM",
        Description = $"Granted {grantQuantity}x {selectedTemplate.Name}"
    });

    AddLogEntry($"Granted {grantQuantity}x {selectedTemplate.Name} to {selectedCharacter.CharacterName}",
                ActivityCategory.General);

    // Reset for next grant
    grantQuantity = 1;
}
```

### Pattern 3: Character Update Subscription for Inventory
**What:** Extend CharacterUpdateType enum and handle InventoryChanged in Play.razor
**When to use:** When player receives item grant notification
**Example:**
```csharp
// In TimeMessages.cs, add to CharacterUpdateType enum:
/// <summary>Inventory was modified (item added, removed, or changed).</summary>
InventoryChanged

// In Play.razor, OnCharacterUpdateReceived already handles updates:
private void OnCharacterUpdateReceived(object? sender, CharacterUpdateMessage e)
{
    if (character == null || e.CharacterId != character.Id) return;

    InvokeAsync(async () =>
    {
        // Existing pattern already refreshes character and calls LoadEquippedItemsAsync
        character = await characterPortal.FetchAsync(character.Id);
        await LoadEquippedItemsAsync();
        StateHasChanged();
        // This will trigger TabPlayInventory.OnParametersSetAsync if character param changes
    });
}
```

### Pattern 4: TabPlayInventory Reload on Character Change
**What:** TabPlayInventory already has OnParametersSetAsync that reloads items
**When to use:** Already implemented - verify it triggers correctly
**Example:**
```csharp
// Source: Existing TabPlayInventory.razor
protected override async Task OnParametersSetAsync()
{
    // Reload items when character changes
    if (Character != null)
    {
        await LoadItemsAsync();
    }
}
```

### Anti-Patterns to Avoid
- **Creating new messaging infrastructure:** CharacterUpdateMessage already exists and is subscribed
- **Bypassing ItemManagementService:** Service handles possession effects, curses - don't use DAL directly
- **Modal confirmation for granting:** GmTable uses inline actions (damage, heal) - stay consistent
- **Complex character fetch for granting:** Use ItemManagementService which fetches internally

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Character list at table | Custom query | TableCharacterList | Already fetched in GmTable |
| Item creation with effects | Direct DAL insert | ItemManagementService.AddItemToInventoryAsync | Handles possession effects, WhilePossessed triggers |
| Real-time notification | WebSocket/SignalR | ITimeEventPublisher.PublishCharacterUpdateAsync | Established pattern for GM-to-player updates |
| Item template browser | Custom list | RadzenDataGrid with filter | Proven in Phase 2 Items.razor |

**Key insight:** The codebase already has all the pieces. This phase is primarily UI work in GmTable.razor and ensuring the existing notification path works for inventory updates.

## Common Pitfalls

### Pitfall 1: Forgetting Effect Application
**What goes wrong:** Items granted without applying possession effects (WhilePossessed, OnPickup)
**Why it happens:** Using DAL directly instead of ItemManagementService
**How to avoid:** Always use `ItemManagementService.AddItemToInventoryAsync()` which handles effects
**Warning signs:** Cursed items grantable without effects, stat bonuses not applied

### Pitfall 2: Character Reference Stale After Update
**What goes wrong:** ItemManagementService needs CharacterEdit but we have TableCharacterInfo
**Why it happens:** TableCharacterInfo is read-only; service needs full CharacterEdit
**How to avoid:** Fetch CharacterEdit using characterPortal.FetchAsync(characterId) before calling service
**Warning signs:** NullReferenceException or "cannot add effect to read-only" errors

### Pitfall 3: Player Not Receiving Update
**What goes wrong:** Item granted but player inventory doesn't refresh
**Why it happens:** Missing CharacterUpdateMessage publication or wrong CharacterId
**How to avoid:** Publish CharacterUpdateMessage with correct CharacterId after successful grant
**Warning signs:** Player must manually refresh to see new items

### Pitfall 4: Wrong Table Context
**What goes wrong:** GM grants item but CampaignId/TableId doesn't match player subscription
**Why it happens:** CharacterUpdateMessage.CampaignId not set correctly
**How to avoid:** Use table.Id.ToString() for CampaignId as done in existing GmTable damage/heal
**Warning signs:** Update works for some players but not others

### Pitfall 5: Stackable Item Handling
**What goes wrong:** Granting quantity > 1 of stackable item creates multiple separate items
**Why it happens:** Not setting StackSize on CharacterItem
**How to avoid:** Set `newItem.StackSize = grantQuantity` before adding
**Warning signs:** 10 separate "Arrow" items instead of 1 stack of 10

### Pitfall 6: Template Not Active
**What goes wrong:** GM can select deactivated templates
**Why it happens:** Template filter not checking IsActive
**How to avoid:** Filter templates with `where t.IsActive` when loading
**Warning signs:** Players receive items that shouldn't be available

## Code Examples

Verified patterns from existing codebase:

### Load Item Templates in GmTable
```csharp
// Source: Phase 2 Items.razor pattern, adapted for GmTable
@inject IItemTemplateDal itemTemplateDal

private List<ItemTemplate>? allTemplates;
private ItemType? selectedItemType;
private string itemSearchText = "";

private IEnumerable<ItemTemplate> filteredTemplates =>
    allTemplates?
        .Where(t => t.IsActive)
        .Where(t => selectedItemType == null || t.ItemType == selectedItemType)
        .Where(t => string.IsNullOrEmpty(itemSearchText) ||
                    t.Name.Contains(itemSearchText, StringComparison.OrdinalIgnoreCase))
        .OrderBy(t => t.Name)
    ?? Enumerable.Empty<ItemTemplate>();

private async Task LoadTemplatesAsync()
{
    allTemplates = await itemTemplateDal.GetAllTemplatesAsync();
}
```

### Grant Item with Full Service
```csharp
// Source: ItemManagementService.AddItemToInventoryAsync + GmTable publish pattern
@inject GameMechanics.Items.ItemManagementService itemManagementService

private async Task GrantItem()
{
    if (selectedCharacter == null || selectedTemplate == null || table == null)
        return;

    try
    {
        // Need CharacterEdit for ItemManagementService
        var targetCharacter = await characterPortal.FetchAsync(selectedCharacter.CharacterId);

        var newItem = new CharacterItem
        {
            Id = Guid.NewGuid(),
            ItemTemplateId = selectedTemplate.Id,
            OwnerCharacterId = selectedCharacter.CharacterId,
            StackSize = selectedTemplate.IsStackable ? grantQuantity : 1,
            CurrentDurability = selectedTemplate.HasDurability ? selectedTemplate.MaxDurability : null,
            CreatedAt = DateTime.UtcNow
        };

        var result = await itemManagementService.AddItemToInventoryAsync(targetCharacter, newItem);

        if (!result.Success)
        {
            errorMessage = result.ErrorMessage;
            return;
        }

        // Notify player via existing messaging system
        await TimeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
        {
            CharacterId = selectedCharacter.CharacterId,
            UpdateType = CharacterUpdateType.InventoryChanged,
            CampaignId = table.Id.ToString(),
            SourceId = "GM",
            Description = $"Granted {grantQuantity}x {selectedTemplate.Name}"
        });

        AddLogEntry(
            $"Granted {grantQuantity}x {selectedTemplate.Name} to {selectedCharacter.CharacterName}",
            ActivityCategory.General);

        grantQuantity = 1;
        selectedTemplate = null;
    }
    catch (Exception ex)
    {
        errorMessage = $"Failed to grant item: {ex.Message}";
    }
}
```

### Activity Log Entry (Existing Pattern)
```csharp
// Source: GmTable.razor existing pattern
private void AddLogEntry(string message, ActivityCategory category = ActivityCategory.General)
{
    ActivityLog.Publish(TableId, message, "GM", category);
}
```

### Player Inventory Reload (Existing Pattern)
```csharp
// Source: TabPlayInventory.razor - already implemented
protected override async Task OnParametersSetAsync()
{
    if (Character != null)
    {
        await LoadItemsAsync();
    }
}

// The parent Play.razor passes Character as parameter:
// <TabPlayInventory Character="@character" Table="@table" OnCharacterChanged="RefreshCharacter" />

// When character is re-fetched after CharacterUpdateReceived,
// OnParametersSetAsync triggers automatically
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Manual refresh needed | Real-time via CharacterUpdateMessage | Already implemented | Players see items immediately |
| Direct DAL insert | ItemManagementService | Phase 4+ | Proper effect handling |

**Deprecated/outdated:**
- None - this is a new feature building on existing patterns

## Open Questions

Things that couldn't be fully resolved:

1. **Multiple items at once (batch grant)**
   - What we know: Requirements specify single item + quantity
   - What's unclear: Whether GM wants to grant multiple different items quickly
   - Recommendation: Start with single item grant; could add "quick grant" list later

2. **Custom name for granted items**
   - What we know: CharacterItem has CustomName field
   - What's unclear: Whether GM should set custom name when granting
   - Recommendation: Default to null (uses template name); add CustomName input if requested

3. **Durability override**
   - What we know: CharacterItem has CurrentDurability; defaults to template MaxDurability
   - What's unclear: Whether GM might want to grant damaged items
   - Recommendation: Default to max durability; add advanced options if needed

## Sources

### Primary (HIGH confidence)
- **Existing codebase files** - Direct code analysis:
  - `GmTable.razor` - GM table UI patterns, character list, damage/heal workflow
  - `TabPlayInventory.razor` - Player inventory display, OnParametersSetAsync reload
  - `Play.razor` - CharacterUpdateReceived handler, subscription pattern
  - `ItemManagementService.cs` - AddItemToInventoryAsync with effect handling
  - `TimeMessages.cs` - CharacterUpdateMessage, CharacterUpdateType enum
  - `ITimeEventPublisher.cs` - PublishCharacterUpdateAsync interface
  - `TableCharacterList.cs` - Characters at table query
  - `CharacterItem.cs` - Item instance DTO structure

### Secondary (MEDIUM confidence)
- Previous phase research (03-RESEARCH.md, 06-RESEARCH.md) - UI patterns for item browsing

### Tertiary (LOW confidence)
- None - all findings verified against codebase

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Uses existing CSLA, Radzen, and messaging patterns
- Architecture: HIGH - Extends proven GmTable patterns; minimal new code
- Pitfalls: HIGH - Derived from actual code analysis and existing patterns

**Research date:** 2026-01-25
**Valid until:** 60 days (stable patterns extending existing functionality)
