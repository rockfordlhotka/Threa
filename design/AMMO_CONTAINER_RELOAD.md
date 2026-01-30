# Ammo Container Reload System

## Overview

This feature enables players to reload ammo containers (magazines, speedloaders, quivers, etc.) by putting individual rounds into them from loose ammo in inventory. This is a timed action using the concentration system.

**Rate**: 3 rounds per game round (1 per second)
**Duration**: `Math.Ceiling(roundsToLoad / 3)` rounds of concentration

## Design Decision

**Location**: Inventory Tab (`TabPlayInventory.razor`)
**UI Pattern**: Modal dialog triggered by action button when ammo container is selected

This differs from the combat `ReloadMode.razor` pattern because:
1. It's a preparation/downtime activity, not a combat action
2. Inventory tab already has action buttons for selected items
3. Modal dialog fits better than a full "mode" switch

---

## Architecture: GM-Centric Time Model

**IMPORTANT**: All time advancement occurs in the GM page/instance, not in player pages.

### Flow Diagram

```
PLAYER                          GM                              SERVER
───────                         ──                              ──────
1. Select magazine
   in inventory
       │
2. Click "Reload"
   → Modal opens
       │
3. Confirm reload
   → Create concentration
     effect on character
   → Save character
       │
       └──────────────────────────────────────────────────────────────┐
                                                                      │
                                4. GM clicks "+1 Round"               │
                                       │                              │
                                       └─────────────────────────────>│
                                                                      │
                                              5. TimeAdvancementService
                                                 loads character
                                                       │
                                              6. character.EndOfRound()
                                                 → Effects.EndOfRound()
                                                   → ConcentrationBehavior.OnTick()
                                                     → progress++
                                                       │
                                              7. If complete:
                                                 → OnExpire()
                                                 → ExecuteAmmoContainerReload()
                                                 → UPDATE ITEMS DIRECTLY
                                                       │
                                              8. Save character
                                                       │
                                              9. Publish CharactersUpdatedMessage
                                                       │
       ┌──────────────────────────────────────────────┘
       │
10. Receive message
    → Reload character from DB
    → Items already updated!
    → UI refreshes
```

### Key Architectural Constraint

`LastConcentrationResult` is a private field (not a CSLA registered property) in `CharacterEdit.cs` (line 35-36). CSLA's MobileFormatter only serializes registered properties and IMobileObject implementations, so this field does NOT survive:
1. Database persistence (it's also in `mapIgnore`)
2. Tier-to-tier serialization via MobileFormatter

When the GM's `TimeAdvancementService` processes effects and saves the character, then the player reloads from DB, the result is lost.

**Solution**: Item updates must happen **server-side** during `TimeAdvancementService` processing, not in `Play.razor`. The `TimeAdvancementService` needs DAL access to update items directly when concentration completes.

### Player→GM Sync

Note: Players CAN still modify character state directly (e.g., spending AP, taking actions) and those changes sync to the GM page immediately via `CharacterUpdateMessage`. This is separate from time advancement - it's about real-time collaboration.

For ammo container reload, the flow is:
1. **Player initiates**: Creates concentration effect, saves character → GM sees effect appear
2. **GM advances time**: Server processes effect, updates items directly
3. **Player refreshes**: Sees items already updated

---

## Implementation Steps

### Step 1: Create AmmoContainerReloadPayload

**File**: `GameMechanics/Effects/Behaviors/AmmoContainerReloadPayload.cs` (NEW)

```csharp
using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// Payload for ammo container reload deferred action.
/// Stores all data needed to complete the reload when concentration finishes.
/// </summary>
public class AmmoContainerReloadPayload
{
    /// <summary>
    /// The magazine/container CharacterItem ID being loaded.
    /// </summary>
    [JsonPropertyName("containerId")]
    public Guid ContainerId { get; set; }

    /// <summary>
    /// The loose ammo CharacterItem ID providing the rounds.
    /// </summary>
    [JsonPropertyName("sourceItemId")]
    public Guid SourceItemId { get; set; }

    /// <summary>
    /// Number of rounds to load into the container.
    /// </summary>
    [JsonPropertyName("roundsToLoad")]
    public int RoundsToLoad { get; set; }

    /// <summary>
    /// Type of ammo being loaded (e.g., "9mm", "Arrow").
    /// </summary>
    [JsonPropertyName("ammoType")]
    public string? AmmoType { get; set; }

    /// <summary>
    /// Display name of the container for messages.
    /// </summary>
    [JsonPropertyName("containerName")]
    public string? ContainerName { get; set; }

    public string Serialize()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = false });
    }

    public static AmmoContainerReloadPayload? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            return JsonSerializer.Deserialize<AmmoContainerReloadPayload>(json);
        }
        catch
        {
            return null;
        }
    }
}
```

### Step 2: Update ConcentrationBehavior

**File**: `GameMechanics/Effects/Behaviors/ConcentrationBehavior.cs`

**A. Add to `IsCastingTimeConcentration()` (~line 62):**
```csharp
case "AmmoContainerReload":
    return true;
```

**B. Add to `ExecuteDeferredAction()` (~line 223):**
```csharp
case "AmmoContainerReload":
    ExecuteAmmoContainerReload(character, state);
    break;
```

**C. Add new factory method (~line 433):**
```csharp
/// <summary>
/// Creates state for ammo container reload concentration.
/// Used when loading individual rounds into a magazine/container from loose ammo.
/// </summary>
/// <param name="containerId">The magazine CharacterItem ID being loaded</param>
/// <param name="sourceItemId">The loose ammo CharacterItem ID providing rounds</param>
/// <param name="roundsToLoad">Number of rounds to load when complete</param>
/// <param name="containerName">Display name of the container</param>
/// <param name="ammoType">Type of ammo being loaded</param>
/// <returns>Serialized ConcentrationState JSON</returns>
public static string CreateAmmoContainerReloadState(
    Guid containerId,
    Guid sourceItemId,
    int roundsToLoad,
    string containerName,
    string? ammoType = null)
{
    // 3 rounds per game round
    int totalRounds = (int)Math.Ceiling(roundsToLoad / 3.0);

    var payload = new AmmoContainerReloadPayload
    {
        ContainerId = containerId,
        SourceItemId = sourceItemId,
        RoundsToLoad = roundsToLoad,
        AmmoType = ammoType,
        ContainerName = containerName
    };

    return new ConcentrationState
    {
        ConcentrationType = "AmmoContainerReload",
        TotalRequired = totalRounds,
        CurrentProgress = 0,
        RoundsPerTick = 1,
        TargetItemId = containerId,
        SourceItemId = sourceItemId,
        DeferredActionType = "AmmoContainerReload",
        DeferredActionPayload = payload.Serialize(),
        CompletionMessage = $"{containerName} loaded with {roundsToLoad} rounds!",
        InterruptionMessage = "Reload interrupted!"
    }.Serialize();
}
```

**D. Add executor method (~line 254):**
```csharp
/// <summary>
/// Handles ammo container reload completion.
/// Stores result in character.LastConcentrationResult for UI to process.
/// </summary>
private void ExecuteAmmoContainerReload(CharacterEdit character, ConcentrationState state)
{
    var payload = AmmoContainerReloadPayload.FromJson(state.DeferredActionPayload);
    if (payload == null) return;

    character.LastConcentrationResult = new ConcentrationCompletionResult
    {
        ActionType = "AmmoContainerReload",
        Payload = state.DeferredActionPayload,
        Message = state.CompletionMessage ?? "Magazine reloaded!",
        Success = true
    };
}
```

### Step 3: Update TimeAdvancementService for Server-Side Item Updates

**File**: `GameMechanics/Time/TimeAdvancementService.cs`

Since `LastConcentrationResult` is `[NonSerialized]`, item updates must happen server-side before the character is saved. Add a method to process concentration results with DAL access.

**A. Add ICharacterItemDal dependency:**
```csharp
private readonly ICharacterItemDal _characterItemDal;

public TimeAdvancementService(
    IChildDataPortal<CharacterEdit> characterPortal,
    IChildDataPortal<EffectRecord> effectPortal,
    ITableDal tableDal,
    ICharacterItemDal characterItemDal,  // NEW
    ITimeEventPublisher timeEventPublisher,
    ILogger<TimeAdvancementService> logger)
{
    // ...
    _characterItemDal = characterItemDal;
}
```

**B. Add concentration result processing after EndOfRound (~line 130):**
```csharp
// Process end-of-round for each round
for (int i = 0; i < roundCount; i++)
{
    character.EndOfRound(_effectPortal);
}

// NEW: Process any concentration completion results
await ProcessConcentrationResultAsync(character);

// Save the updated character
await _characterPortal.UpdateAsync(character);
```

**C. Add processing method:**
```csharp
/// <summary>
/// Processes concentration completion results that require item updates.
/// Called after EndOfRound before saving character.
/// </summary>
private async Task ProcessConcentrationResultAsync(CharacterEdit character)
{
    var result = character.LastConcentrationResult;
    if (result == null) return;

    try
    {
        switch (result.ActionType)
        {
            case "MagazineReload":
                await ExecuteMagazineReloadAsync(result.Payload);
                break;
            case "AmmoContainerReload":
                await ExecuteAmmoContainerReloadAsync(result.Payload);
                break;
        }
    }
    finally
    {
        character.ClearConcentrationResult();
    }
}

private async Task ExecuteAmmoContainerReloadAsync(string? payloadJson)
{
    if (string.IsNullOrEmpty(payloadJson)) return;

    var payload = AmmoContainerReloadPayload.FromJson(payloadJson);
    if (payload == null) return;

    // Update container ammo state
    var container = await _characterItemDal.GetItemAsync(payload.ContainerId);
    if (container != null)
    {
        var containerState = AmmoContainerState.FromJson(container.CustomProperties);
        containerState.LoadedAmmo += payload.RoundsToLoad;
        if (payload.AmmoType != null)
            containerState.AmmoType = payload.AmmoType;
        container.CustomProperties = AmmoContainerState.MergeIntoCustomProperties(
            container.CustomProperties, containerState);
        await _characterItemDal.UpdateItemAsync(container);
    }

    // Reduce source ammo stack
    var source = await _characterItemDal.GetItemAsync(payload.SourceItemId);
    if (source != null)
    {
        source.StackSize -= payload.RoundsToLoad;
        if (source.StackSize <= 0)
            await _characterItemDal.DeleteItemAsync(payload.SourceItemId);
        else
            await _characterItemDal.UpdateItemAsync(source);
    }
}

private async Task ExecuteMagazineReloadAsync(string? payloadJson)
{
    // Similar pattern for weapon magazine reload
    if (string.IsNullOrEmpty(payloadJson)) return;

    var payload = MagazineReloadPayload.FromJson(payloadJson);
    if (payload == null) return;

    var weapon = await _characterItemDal.GetItemAsync(payload.WeaponItemId);
    if (weapon != null)
    {
        var ammoState = WeaponAmmoState.FromJson(weapon.CustomProperties);
        ammoState.LoadedAmmo = payload.RoundsToLoad;
        weapon.CustomProperties = WeaponAmmoState.MergeIntoCustomProperties(
            weapon.CustomProperties, ammoState);
        await _characterItemDal.UpdateItemAsync(weapon);
    }
}
```

**NOTE**: This also fixes the existing `MagazineReload` concentration which currently has the same serialization issue.

### Step 4: Create ReloadAmmoContainerModal Component

**File**: `Threa/Threa.Client/Components/Shared/ReloadAmmoContainerModal.razor` (NEW)

```razor
@using GameMechanics
@using GameMechanics.Combat
@using Threa.Dal.Dto

<div class="modal fade show d-block" tabindex="-1" style="background-color: rgba(0,0,0,0.5);">
    <div class="modal-dialog">
        <div class="modal-content">
            <div class="modal-header">
                <h5 class="modal-title">Reload @ContainerName</h5>
                <button type="button" class="btn-close" @onclick="OnCancel"></button>
            </div>
            <div class="modal-body">
                @* Container Info *@
                <div class="mb-3">
                    <strong>Container:</strong> @ContainerName
                    <span class="badge bg-secondary ms-2">
                        @CurrentAmmo / @Capacity
                    </span>
                    <span class="text-muted ms-2">(@AmmoType)</span>
                </div>

                @* Ammo Source Selection *@
                <div class="mb-3">
                    <label class="form-label">Ammo Source</label>
                    <select class="form-select" @bind="selectedSourceId">
                        <option value="">-- Select ammo --</option>
                        @foreach (var ammo in AvailableAmmo)
                        {
                            <option value="@ammo.Id">
                                @ammo.Name (x@ammo.StackSize)
                            </option>
                        }
                    </select>
                </div>

                @* Quantity Slider *@
                @if (selectedSourceId != Guid.Empty)
                {
                    <div class="mb-3">
                        <label class="form-label">Rounds to Load: @roundsToLoad</label>
                        <input type="range" class="form-range"
                               min="1" max="@MaxLoadable"
                               @bind="roundsToLoad" @bind:event="oninput" />
                        <div class="d-flex justify-content-between text-muted small">
                            <span>1</span>
                            <span>@MaxLoadable</span>
                        </div>
                    </div>

                    <div class="alert alert-info">
                        <i class="bi bi-clock me-2"></i>
                        <strong>Duration:</strong> @DurationRounds round(s) of concentration
                        <br/>
                        <small class="text-muted">(3 rounds loaded per game round)</small>
                    </div>
                }
            </div>
            <div class="modal-footer">
                <button class="btn btn-secondary" @onclick="OnCancel">Cancel</button>
                <button class="btn btn-primary"
                        disabled="@(selectedSourceId == Guid.Empty || roundsToLoad <= 0)"
                        @onclick="OnConfirm">
                    Start Reload
                </button>
            </div>
        </div>
    </div>
</div>

@code {
    [Parameter] public CharacterEdit? Character { get; set; }
    [Parameter] public CharacterItemDto? Container { get; set; }
    [Parameter] public ItemTemplateDto? ContainerTemplate { get; set; }
    [Parameter] public List<CharacterItemDto> AvailableAmmo { get; set; } = new();
    [Parameter] public Func<Guid, ItemTemplateDto?> GetTemplate { get; set; } = _ => null;
    [Parameter] public EventCallback<ReloadAmmoContainerResult> OnConfirmReload { get; set; }
    [Parameter] public EventCallback OnCancelReload { get; set; }

    private Guid selectedSourceId = Guid.Empty;
    private int roundsToLoad = 1;

    private string ContainerName => ContainerTemplate?.Name ?? "Magazine";
    private string AmmoType => AmmoContainerProperties.FromJson(ContainerTemplate?.CustomProperties).AmmoType ?? "Unknown";
    private int Capacity => AmmoContainerProperties.FromJson(ContainerTemplate?.CustomProperties).Capacity;
    private int CurrentAmmo => AmmoContainerState.FromJson(Container?.CustomProperties).LoadedAmmo;
    private int SpaceAvailable => Capacity - CurrentAmmo;

    private int MaxLoadable
    {
        get
        {
            if (selectedSourceId == Guid.Empty) return 0;
            var source = AvailableAmmo.FirstOrDefault(a => a.Id == selectedSourceId);
            if (source == null) return 0;
            return Math.Min(SpaceAvailable, source.StackSize);
        }
    }

    private int DurationRounds => (int)Math.Ceiling(roundsToLoad / 3.0);

    private async Task OnConfirm()
    {
        if (Container == null || selectedSourceId == Guid.Empty) return;

        var source = AvailableAmmo.FirstOrDefault(a => a.Id == selectedSourceId);
        if (source == null) return;

        var result = new ReloadAmmoContainerResult
        {
            ContainerId = Container.Id,
            SourceItemId = selectedSourceId,
            RoundsToLoad = roundsToLoad,
            ContainerName = ContainerName,
            AmmoType = AmmoType,
            DurationRounds = DurationRounds
        };

        await OnConfirmReload.InvokeAsync(result);
    }

    private async Task OnCancel()
    {
        await OnCancelReload.InvokeAsync();
    }
}

@code {
    public class ReloadAmmoContainerResult
    {
        public Guid ContainerId { get; set; }
        public Guid SourceItemId { get; set; }
        public int RoundsToLoad { get; set; }
        public string ContainerName { get; set; } = "";
        public string? AmmoType { get; set; }
        public int DurationRounds { get; set; }
    }
}
```

### Step 5: Add "Reload" Button to Inventory Tab

**File**: `Threa/Threa.Client/Components/Pages/GamePlay/TabPlayInventory.razor`

**A. Add helper methods:**
```csharp
private bool IsReloadableAmmoContainer(CharacterItem? item)
{
    if (item == null) return false;
    var template = GetTemplate(item.ItemTemplateId);
    if (template == null) return false;

    var containerProps = AmmoContainerProperties.FromJson(template.CustomProperties);
    if (!containerProps.IsAmmoContainer) return false;

    var containerState = AmmoContainerState.FromJson(item.CustomProperties);
    return containerState.SpaceAvailable > 0;
}

private List<CharacterItemDto> GetCompatibleLooseAmmo(CharacterItem container)
{
    var template = GetTemplate(container.ItemTemplateId);
    var containerProps = AmmoContainerProperties.FromJson(template?.CustomProperties);
    var allowedTypes = containerProps.GetAllowedAmmoTypes();

    return inventoryItems.Where(item => {
        var itemTemplate = GetTemplate(item.ItemTemplateId);
        var ammoProps = AmmunitionProperties.FromJson(itemTemplate?.CustomProperties);
        return ammoProps.IsAmmunition &&
               !ammoProps.IsContainer &&
               allowedTypes.Contains(ammoProps.AmmoType, StringComparer.OrdinalIgnoreCase);
    }).ToList();
}
```

**B. Add button in action area (~line 323, near Drop button):**
```razor
@if (IsReloadableAmmoContainer(selectedItem) && GetCompatibleLooseAmmo(selectedItem).Any())
{
    <button class="btn btn-outline-primary"
            @onclick="OpenReloadAmmoContainerModal">
        <i class="bi bi-box-arrow-in-down"></i> Reload
    </button>
}
```

**C. Add modal state and methods:**
```csharp
private bool showReloadAmmoContainerModal = false;
private CharacterItemDto? reloadTargetContainer = null;
private List<CharacterItemDto> availableLooseAmmo = new();

private void OpenReloadAmmoContainerModal()
{
    reloadTargetContainer = selectedItem;
    availableLooseAmmo = GetCompatibleLooseAmmo(selectedItem!);
    showReloadAmmoContainerModal = true;
}

private void CloseReloadAmmoContainerModal()
{
    showReloadAmmoContainerModal = false;
    reloadTargetContainer = null;
    availableLooseAmmo.Clear();
}

private async Task OnReloadAmmoContainerConfirm(ReloadAmmoContainerResult result)
{
    showReloadAmmoContainerModal = false;

    // Check/break existing concentration
    var concentrationEffect = Character!.GetConcentrationEffect();
    if (concentrationEffect != null)
    {
        var state = ConcentrationState.FromJson(concentrationEffect.BehaviorState);
        var confirmed = await ConcentrationBreakDialog.ShowAsync(
            DialogService, state!, concentrationEffect.Name);
        if (!confirmed) return;
        ConcentrationBehavior.BreakConcentration(Character, "Started ammo container reload");
    }

    // Create concentration effect
    var behaviorState = ConcentrationBehavior.CreateAmmoContainerReloadState(
        result.ContainerId,
        result.SourceItemId,
        result.RoundsToLoad,
        result.ContainerName,
        result.AmmoType);

    // Add effect via EffectPortal
    var effect = await EffectPortal.CreateChild(
        EffectType.Concentration,
        $"Reloading {result.ContainerName}",
        "Concentration",
        null, // Duration handled by concentration system
        behaviorState);

    Character.Effects.AddEffect(effect);
    await Character.SaveAsync();

    successMessage = $"Started reloading {result.ContainerName} ({result.RoundsToLoad} rounds, {result.DurationRounds} round(s))";
    await NotifyCharacterChanged();
}
```

**D. Add modal to markup (at end of component):**
```razor
@if (showReloadAmmoContainerModal && reloadTargetContainer != null)
{
    <ReloadAmmoContainerModal
        Character="Character"
        Container="reloadTargetContainer"
        ContainerTemplate="GetTemplate(reloadTargetContainer.ItemTemplateId)"
        AvailableAmmo="availableLooseAmmo"
        GetTemplate="GetTemplate"
        OnConfirmReload="OnReloadAmmoContainerConfirm"
        OnCancelReload="CloseReloadAmmoContainerModal" />
}
```

---

## Files Summary

| File | Action | Changes |
|------|--------|---------|
| `GameMechanics/Effects/Behaviors/AmmoContainerReloadPayload.cs` | **NEW** | Payload class |
| `GameMechanics/Effects/Behaviors/ConcentrationBehavior.cs` | Modify | Add type check, factory, executor |
| `GameMechanics/Time/TimeAdvancementService.cs` | Modify | Add DAL dependency, process concentration results server-side |
| `Threa.Client/Components/Shared/ReloadAmmoContainerModal.razor` | **NEW** | Modal UI component |
| `Threa.Client/Components/Pages/GamePlay/TabPlayInventory.razor` | Modify | Add button, modal, handlers |

**Note**: This design also fixes the existing `MagazineReload` concentration which has the same `[NonSerialized]` issue with `LastConcentrationResult`.

---

## Key Classes Reference

| Class | File | Purpose |
|-------|------|---------|
| `AmmoContainerState` | `GameMechanics/Combat/AmmoContainerState.cs` | Track loaded ammo in container |
| `AmmoContainerProperties` | `GameMechanics/Combat/AmmoContainerProperties.cs` | Container template metadata |
| `AmmunitionProperties` | `GameMechanics/Combat/AmmunitionProperties.cs` | Ammo template metadata |
| `ConcentrationState` | `ConcentrationBehavior.cs` | Concentration effect state |
| `ConcentrationBreakDialog` | `Threa.Client/Components/Shared/` | Dialog for breaking concentration |

---

## User Flow

### Player Initiates Reload
1. Player opens Inventory tab
2. Clicks on a magazine (ammo container) with space available
3. Sees "Reload" button (only if compatible loose ammo exists)
4. Clicks "Reload" → Modal opens
5. Modal shows:
   - Container: "9mm Magazine (12/30)"
   - Available ammo dropdown: "9mm Rounds (x50)"
   - Quantity slider: 1-18 (limited by space and available)
   - Duration: "6 rounds of concentration"
6. Clicks "Start Reload"
7. If already concentrating → ConcentrationBreakDialog confirms
8. Concentration effect created and saved on character
9. Player sees concentration effect in effects panel

### GM Advances Time (Round-by-Round)
10. GM clicks "+1 Round" in GM controls
11. Server-side `TimeAdvancementService.AdvanceRoundsAsync()`:
    - Loads character
    - Calls `character.EndOfRound()` → `Effects.EndOfRound()`
    - Concentration progresses: `OnTick()` increments `CurrentProgress`
    - If complete: `OnExpire()` → `ExecuteAmmoContainerReload()` sets `LastConcentrationResult`
    - **Server processes result**: Updates container and source items via DAL
    - Clears `LastConcentrationResult`
    - Saves character
    - Publishes `CharactersUpdatedMessage`
12. Player receives update notification
13. Player UI reloads character from DB
14. Items are already updated! Magazine now has ammo, loose ammo reduced
15. Concentration effect gone (expired)

---

## Verification

1. **Build**: `dotnet build Threa.sln`

2. **Unit Tests** (new):
   - `TimeAdvancementService.ProcessConcentrationResultAsync()` updates items correctly
   - `AmmoContainerReloadPayload` serialization/deserialization
   - Duration calculation: `Math.Ceiling(roundsToLoad / 3.0)`

3. **Manual Test** (requires GM + Player):

   **Setup**:
   - Create character with empty magazine (e.g., "9mm Magazine" capacity 30, loaded 0)
   - Add loose ammo to inventory (e.g., "9mm Rounds" x50)
   - Join character to a table

   **Player Side**:
   - Open Inventory tab
   - Click the magazine
   - Verify "Reload" button appears
   - Click Reload → Modal opens
   - Select "9mm Rounds" as source
   - Set quantity to 18 (should show "6 rounds of concentration")
   - Click "Start Reload"
   - Verify concentration effect appears: "Reloading 9mm Magazine"

   **GM Side**:
   - Click "+1 Round" 6 times (or "+6 Rounds" if available)
   - Observe log entries showing progress

   **Player Side (after time advances)**:
   - Verify concentration effect is gone
   - Verify magazine now shows "18/30"
   - Verify loose ammo reduced from 50 to 32

4. **Edge Cases**:
   - Interrupt concentration mid-reload (player takes action)
   - Reload when source ammo is exactly the amount needed
   - Reload when source ammo is less than space available

---

## Related Documents

- [RANGED_WEAPONS_SCIFI.md](RANGED_WEAPONS_SCIFI.md) - Ammunition system and reload actions
- [CONCENTRATION_SYSTEM.md](CONCENTRATION_SYSTEM.md) - Concentration mechanics for timed actions
- [EFFECTS_SYSTEM.md](EFFECTS_SYSTEM.md) - Effect types and management
