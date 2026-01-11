# Item Effects System

## Overview

The Item Effects System allows items (equipment, magic items, tech gadgets, implants) to apply effects to characters. This extends the existing effect system to support item-triggered buffs, debuffs, curses, and other magical or technological enhancements.

## Key Concepts

### Item Effect Triggers

Effects can be triggered by different item states:

| Trigger | Description | Example | Removed When |
|---------|-------------|---------|--------------|
| `WhileEquipped` | Active while item is equipped | Ring of Protection (+2 defense) | Unequipped |
| `WhilePossessed` | Active while item is in inventory | Cursed idol (causes nightmares) | Dropped/transferred |
| `OnPickup` | Applied when first acquired | Cursed item activates on touch | Dropped/transferred |
| `OnUse` | One-time effect when used | Healing potion | Duration expires |
| `OnAttackWith` | Applied when attacking | Flaming sword (fire damage) | After attack resolves |
| `OnHitWhileWearing` | Applied when hit | Thorns armor (reflect damage) | After defense resolves |
| `OnCritical` | Applied on critical hit | Vorpal blade (instant kill chance) | After attack resolves |

### Item Possession vs Equipment

There are two distinct states for items:

| State | Description | Triggers Activated |
|-------|-------------|-------------------|
| **Possessed** | Item is in character's inventory (any slot, container, or carried) | `WhilePossessed`, `OnPickup` |
| **Equipped** | Item is worn/wielded in an equipment slot | `WhileEquipped` (plus possession triggers) |

**Important**: An equipped item is also possessed. Unequipping moves the item to inventory (still possessed), while dropping removes it entirely.

```
[Pick Up Item]
    ?
    ??? Activate WhilePossessed effects
    ??? Activate OnPickup effects (one-time trigger)

[Equip Item]
    ?
    ??? Activate WhileEquipped effects
        (WhilePossessed effects remain active)

[Unequip Item]
    ?
    ??? Deactivate WhileEquipped effects
        (WhilePossessed effects remain - item still in inventory)

[Drop Item]
    ?
    ??? Deactivate WhileEquipped effects (if equipped)
    ??? Deactivate WhilePossessed effects
```

### Effect Lifecycle

```
[Item Acquired/Equipped]
    ?
    ?
Check ItemTemplate.Effects
    ?
    ?
For each ItemEffectDefinition matching trigger:
    ?
    ?
Create EffectRecord with:
  - SourceItemId = item.Id
  - ItemEffectTrigger = definition.Trigger
  - IsCursed = definition.IsCursed
  - DurationRounds = null (permanent while triggered)
    ?
    ?
EffectList.AddEffect()
    ?
    ?
Effect applies modifiers via existing behavior system

[Item Unequipped]
    ?
    ?
Check for cursed WhileEquipped effects ? Block if any active
    ?
    ?
EffectList.RemoveEquipEffects(itemId)
    (Possession effects remain active)

[Item Dropped/Transferred]
    ?
    ?
Check for cursed WhilePossessed/OnPickup effects ? Block if any active
    ?
    ?
EffectList.RemovePossessionEffects(itemId)
    (Removes ALL effects from this item)
```

### Curse Handling

Cursed items have effects with `IsCursed = true`. The curse behavior depends on the trigger:

| Trigger | Curse Blocks | Example |
|---------|-------------|---------|
| `WhileEquipped` | Unequipping only | Cursed ring can't be removed, but could be cut off with the finger |
| `WhilePossessed` | Dropping/transferring | Cursed idol can't be discarded |
| `OnPickup` | Dropping/transferring | Cursed coin bonds to whoever picks it up |

#### Curse Detection Methods

```csharp
// Check if item blocks unequip (WhileEquipped curse)
bool canUnequip = character.Effects.CanUnequipItem(itemId);

// Check if item blocks drop (WhilePossessed/OnPickup curse)  
bool canDrop = character.Effects.CanDropItem(itemId);

// Check if item has any curse at all
bool isCursed = character.Effects.IsItemCursed(itemId);
```

#### Remove Curse Flow

```
[Cast Remove Curse]
    ?
    ?
Find cursed effects on character
    ?
    ?
Remove the curse effect (not the item)
    ?
    ?
Character can now unequip/drop the item
    ?
    ?
If re-equipped/re-picked-up, curse reactivates (unless item is unenchanted)
```

### Implants

Cybernetic/biotech implants use the same system with special equipment slots:

| Slot | Description |
|------|-------------|
| `ImplantNeural` | Brain/neural interface |
| `ImplantOpticLeft/Right` | Eye enhancements |
| `ImplantAuralLeft/Right` | Hearing enhancements |
| `ImplantCardiac` | Heart/circulatory |
| `ImplantSpine` | Spinal enhancement |
| `ImplantArmLeft/Right` | Cybernetic arms |
| `ImplantLegLeft/Right` | Cybernetic legs |
| `ImplantSubdermal` | Under-skin armor/sensors |
| `ImplantOrgan` | Organ replacement |
| `ImplantHandLeft/Right` | Hand enhancements |

**Special Rules for Implants:**
- Require surgery skill check to install/remove
- May require medical facility
- Removal may cause damage or debuffs
- Can have "rejection" effects if incompatible

## Data Model

### ItemEffectDefinition

Defines an effect that an item can apply:

```csharp
public class ItemEffectDefinition
{
    public int Id { get; set; }
    public int ItemTemplateId { get; set; }
    public int? EffectDefinitionId { get; set; }
    public EffectType EffectType { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public ItemEffectTrigger Trigger { get; set; }
    public bool IsCursed { get; set; }
    public bool RequiresAttunement { get; set; }
    public int? DurationRounds { get; set; }
    public string? BehaviorState { get; set; }
    public string? IconName { get; set; }
    public bool IsActive { get; set; }
    public int Priority { get; set; }
}
```

### EffectRecord Extensions

New properties on EffectRecord:

```csharp
public Guid? SourceItemId { get; set; }        // Links to CharacterItem
public ItemEffectTrigger ItemEffectTrigger { get; set; }
public bool IsCursed { get; set; }

// Computed properties
public bool IsFromItem => SourceItemId.HasValue;

// Blocking checks - considers both trigger type and curse status
public bool IsBlockingUnequip => IsFromItem && IsCursed && IsActive 
    && ItemEffectTrigger == ItemEffectTrigger.WhileEquipped;

public bool IsBlockingDrop => IsFromItem && IsCursed && IsActive 
    && (ItemEffectTrigger == ItemEffectTrigger.WhilePossessed 
        || ItemEffectTrigger == ItemEffectTrigger.OnPickup);

public bool IsBlockingItemRemoval => IsBlockingUnequip || IsBlockingDrop;
```

### EffectList Helper Methods

```csharp
// Get effects from a specific item
IEnumerable<EffectRecord> GetEffectsFromItem(Guid itemId);

// Remove effects based on item action
bool RemoveEffectsFromItem(Guid itemId, ItemEffectTrigger? trigger = null);
bool RemoveEquipEffects(Guid itemId);      // Only WhileEquipped effects
bool RemovePossessionEffects(Guid itemId); // All possession + equip effects

// Curse/blocking checks
bool IsItemBlockingUnequip(Guid itemId);   // WhileEquipped curses
bool IsItemBlockingDrop(Guid itemId);      // WhilePossessed/OnPickup curses
bool IsItemCursed(Guid itemId);            // Any curse

// Permission checks
bool CanUnequipItem(Guid itemId);          // !IsItemBlockingUnequip
bool CanDropItem(Guid itemId);             // !IsItemBlockingDrop

// Get all item-based effects
IEnumerable<EffectRecord> GetAllItemEffects();
IEnumerable<EffectRecord> GetCursedEffectsFromItem(Guid itemId);
```

## Examples



### Ring of Healing

```json
{
  "Name": "Ring of Healing",
  "EquipmentSlot": "FingerLeft1",
  "Effects": [
    {
      "Name": "Regeneration",
      "EffectType": "Buff",
      "Trigger": "WhileEquipped",
      "BehaviorState": {
        "Modifiers": [
          {
            "Type": "HealingOverTime",
            "Target": "FAT",
            "Value": 1,
            "HealIntervalRounds": 10
          }
        ]
      }
    }
  ]
}
```

### Cursed Ring of Power

```json
{
  "Name": "Cursed Ring of Power",
  "EquipmentSlot": "FingerRight1",
  "Effects": [
    {
      "Name": "Strength Boost",
      "EffectType": "Buff",
      "Trigger": "WhileEquipped",
      "IsCursed": true,
      "BehaviorState": {
        "Modifiers": [
          { "Type": "Attribute", "Target": "STR", "Value": 3 }
        ]
      }
    },
    {
      "Name": "Life Drain",
      "EffectType": "Debuff",
      "Trigger": "WhileEquipped",
      "IsCursed": true,
      "BehaviorState": {
        "Modifiers": [
          { "Type": "DamageOverTime", "Target": "VIT", "Value": 1, "IntervalRounds": 100 }
        ]
      }
    }
  ]
}
```

### Neural Implant

```json
{
  "Name": "Neural Processing Unit v2",
  "EquipmentSlot": "ImplantNeural",
  "Effects": [
    {
      "Name": "Enhanced Cognition",
      "EffectType": "Buff",
      "Trigger": "WhileEquipped",
      "BehaviorState": {
        "Modifiers": [
          { "Type": "Attribute", "Target": "INT", "Value": 2 },
          { "Type": "AbilityScoreAttribute", "Target": "INT", "Value": 1 }
        ]
      }
    },
    {
      "Name": "Power Dependency",
      "EffectType": "Condition",
      "Trigger": "WhileEquipped",
      "Description": "Requires power cell. Without power: -4 to all INT skills."
    }
  ]
}
```

### Cursed Idol (Possession-Based Curse)

```json
{
  "Name": "Idol of Nightmares",
  "ItemType": "Trinket",
  "EquipmentSlot": "None",
  "Effects": [
    {
      "Name": "Haunting Visions",
      "EffectType": "Debuff",
      "Trigger": "WhilePossessed",
      "IsCursed": true,
      "Description": "Cannot drop this idol. Suffer nightmares that prevent restful sleep.",
      "BehaviorState": {
        "Modifiers": [
          { "Type": "Attribute", "Target": "WIL", "Value": -2 },
          { "Type": "HealingReduction", "Target": "FAT", "Value": 50, "Description": "50% reduced fatigue recovery from rest" }
        ]
      }
    }
  ]
}
```

### Cursed Coin (OnPickup Trigger)

```json
{
  "Name": "Beggar's Curse Coin",
  "ItemType": "Currency",
  "EquipmentSlot": "None",
  "IsStackable": false,
  "Effects": [
    {
      "Name": "Miser's Burden",
      "EffectType": "Debuff",
      "Trigger": "OnPickup",
      "IsCursed": true,
      "Description": "This coin bonds to whoever picks it up. Causes misfortune until a Remove Curse spell is cast.",
      "BehaviorState": {
        "Modifiers": [
          { "Type": "SuccessValueGlobal", "Value": -1, "Description": "-1 to all success values" }
        ]
      }
    }
  ]
}
```

## Relationship to Existing Systems

### Item Skill/Attribute Bonuses (Unchanged)

The existing `ItemSkillBonus` and `ItemAttributeModifier` on `ItemTemplate` remain for **simple, direct bonuses** that don't need effect lifecycle:

- +2 to Swords skill
- +3 to STR attribute
- Weapon damage modifiers

### Item Effects (New)

Use the new effect system for **complex behaviors**:

- Healing/damage over time
- Conditional effects
- Cursed items
- Effects with custom behavior
- Effects that need to be dispelled

### When to Use Which

| Behavior | Use ItemTemplate Bonuses | Use Item Effects |
|----------|-------------------------|------------------|
| +2 STR while equipped | ? | |
| +1 SV for this weapon | ? | |
| Heal 1 FAT per 10 rounds | | ? |
| Cursed (can't unequip) | | ? |
| Poison on hit | | ? |
| Conditional bonus | ? (with Condition) | ? (if complex) |

## Implementation Status

### Phase 1: Core Data Model ?

- `ItemEffectTrigger` enum - when item effects activate
- `ItemEffectDefinition` DTO - links items to effects
- `SourceItemId`, `ItemEffectTrigger`, `IsCursed` on `EffectRecord`
- Implant slots on `EquipmentSlot`
- `EquipmentSlotExtensions` for slot helpers

### Phase 2: Effect Application Logic ?

#### New EffectType

Added `EffectType.ItemEffect = 9` for item-based effects.

#### ItemEffectBehavior

Handles item effect lifecycle:

```csharp
public class ItemEffectBehavior : IEffectBehavior
{
    public EffectType EffectType => EffectType.ItemEffect;
    
    // OnAdding: Replaces existing effect from same item with same name
    // OnTick: Processes healing/damage over time using PendingHealing/PendingDamage
    // GetAttributeModifiers/GetAbilityScoreModifiers/GetSuccessValueModifiers: 
    //   Returns modifiers from ItemEffectState.Modifiers
}
```

#### ItemEffectState

State stored in `BehaviorState` JSON:

```csharp
public class ItemEffectState
{
    public string EffectName { get; set; }
    public string ItemName { get; set; }
    public List<BuffModifier> Modifiers { get; set; }  // Reuses BuffModifier
    public int TickIntervalRounds { get; set; }
    public int RoundsUntilTick { get; set; }
    public bool IsRevealed { get; set; }
    public int IdentifyDifficulty { get; set; }
    public int RemovalDifficulty { get; set; }
}
```

#### ItemEffectService

Service for managing item effect lifecycle:

```csharp
public class ItemEffectService
{
    // Action checks
    ItemActionCheckResult CanUnequipItem(CharacterEdit character, Guid itemId);
    ItemActionCheckResult CanDropItem(CharacterEdit character, Guid itemId);
    
    // Apply effects
    Task<ItemEffectApplicationResult> OnItemPickedUpAsync(...);  // WhilePossessed, OnPickup
    Task<ItemEffectApplicationResult> OnItemEquippedAsync(...);  // WhileEquipped
    Task<ItemEffectApplicationResult> OnItemAcquiredAndEquippedAsync(...);  // All triggers
    
    // Remove effects
    ItemEffectApplicationResult OnItemUnequipped(...);  // Only WhileEquipped
    ItemEffectApplicationResult OnItemDropped(...);     // All effects
    
    // Curse management
    ItemEffectApplicationResult RemoveCurseFromItem(...);
    IEnumerable<EffectRecord> GetAllCursedEffects(...);
}
```

#### EffectRecord CreateChild Overload

New factory method for item effects:

```csharp
[CreateChild]
private void CreateFromItem(ItemEffectDefinition definition, Guid sourceItemId)
{
    // Sets SourceItemId, ItemEffectTrigger, IsCursed from definition
}
```

#### CharacterEffect DTO Updates

Added persistence for item effect properties:

```csharp
public Guid? SourceItemId { get; set; }
public ItemEffectTrigger ItemEffectTrigger { get; set; }
public bool IsCursed { get; set; }
```

#### Service Registration ?

GameMechanics services registered via extension method:

```csharp
// In GameMechanics/ServiceCollectionExtensions.cs
public static IServiceCollection AddGameMechanics(this IServiceCollection services)
{
    services.AddScoped<ItemEffectService>();
    return services;
}

// In Program.cs
builder.Services.AddGameMechanics();
```

#### Unit Tests ?

Comprehensive tests in `GameMechanics.Test/ItemEffectTests.cs`:

- `ItemEffectState` serialization and factory methods
- `EffectRecord` item properties and blocking checks
- `EffectList` item effect management methods
- `ItemEffectService` pickup/equip/unequip/drop workflows
- `ItemEffectService` curse detection and removal
- `ItemEffectBehavior` modifier calculations
- `EquipmentSlot` extension methods

#### ItemManagementService ?

High-level service that coordinates DAL and effect system:

```csharp
public class ItemManagementService
{
    // Item operations with integrated effect management
    Task<ItemOperationResult> AddItemToInventoryAsync(character, item);
    Task<ItemOperationResult> EquipItemAsync(character, itemId, slot);
    Task<ItemOperationResult> UnequipItemAsync(character, itemId);
    Task<ItemOperationResult> RemoveItemFromInventoryAsync(character, itemId);
    Task<ItemOperationResult> TransferItemAsync(fromCharacter, toCharacter, itemId);
    Task<ItemOperationResult> MoveToContainerAsync(character, itemId, containerItemId);
    Task<ItemOperationResult> UseItemAsync(character, itemId);
    
    // Curse checks for UI
    bool CanUnequipItem(character, itemId);
    bool CanDropItem(character, itemId);
    string? GetUnequipBlockReason(character, itemId);
    string? GetDropBlockReason(character, itemId);
}
```

### Phase 3: Curse Handling (Partial)

Basic curse handling is implemented:

- `CanUnequipItem()` / `CanDropItem()` check for blocking curses
- `RemoveCurseFromItem()` removes curse effects if removal power is sufficient
- Curse effects have `RemovalDifficulty` in their state

Still needed:
- Integration with spell system for Remove Curse
- UI feedback for curse blocking
- Unenchant Item spell for permanent curse removal

### Phase 4: Attunement System (Future)

Not yet implemented. Consider:
- Limit concurrent attuned magical items
- Attunement slot management
- Attunement requirements (class, level, etc.)
- Similar to drug interaction system

## Integration Points

### ItemManagementService (Recommended)

Use `ItemManagementService` for all item operations - it handles both DAL updates and effect management:

| Event | Method |
|-------|--------|
| Item picked up / looted | `AddItemToInventoryAsync()` |
| Item equipped | `EquipItemAsync()` |
| Item unequipped | `UnequipItemAsync()` |
| Item dropped / sold | `RemoveItemFromInventoryAsync()` |
| Item given to another character | `TransferItemAsync()` |
| Item moved to container | `MoveToContainerAsync()` |
| Consumable used | `UseItemAsync()` |

### Direct ItemEffectService (Lower Level)

If you need more control, call `ItemEffectService` methods directly:

| Event | Method to Call |
|-------|----------------|
| Item picked up / added to inventory | `OnItemPickedUpAsync()` |
| Item equipped from inventory | `OnItemEquippedAsync()` |
| Item looted and immediately equipped | `OnItemAcquiredAndEquippedAsync()` |
| Item unequipped to inventory | `OnItemUnequipped()` |
| Item dropped / transferred / sold | `OnItemDropped()` |
| Remove Curse spell cast | `RemoveCurseFromItem()` |




