using Csla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace GameMechanics.Effects;

/// <summary>
/// Result of attempting to apply item effects.
/// </summary>
public class ItemEffectApplicationResult
{
  /// <summary>
  /// Whether the operation succeeded.
  /// </summary>
  public bool Success { get; set; }

  /// <summary>
  /// Error message if the operation failed.
  /// </summary>
  public string? ErrorMessage { get; set; }

  /// <summary>
  /// Effects that were applied.
  /// </summary>
  public List<EffectRecord> AppliedEffects { get; set; } = [];

  /// <summary>
  /// Effects that were removed.
  /// </summary>
  public List<Guid> RemovedEffectIds { get; set; } = [];

  public static ItemEffectApplicationResult Succeeded(List<EffectRecord>? applied = null)
  {
    return new ItemEffectApplicationResult
    {
      Success = true,
      AppliedEffects = applied ?? []
    };
  }

  public static ItemEffectApplicationResult Failed(string errorMessage)
  {
    return new ItemEffectApplicationResult
    {
      Success = false,
      ErrorMessage = errorMessage
    };
  }
}

/// <summary>
/// Result of checking if an item action is allowed.
/// </summary>
public class ItemActionCheckResult
{
  /// <summary>
  /// Whether the action is allowed.
  /// </summary>
  public bool IsAllowed { get; set; }

  /// <summary>
  /// Reason the action is blocked (if not allowed).
  /// </summary>
  public string? BlockReason { get; set; }

  /// <summary>
  /// Effects that are blocking the action.
  /// </summary>
  public List<EffectRecord> BlockingEffects { get; set; } = [];

  public static ItemActionCheckResult Allowed() => new() { IsAllowed = true };

  public static ItemActionCheckResult Blocked(string reason, List<EffectRecord>? blockingEffects = null)
  {
    return new ItemActionCheckResult
    {
      IsAllowed = false,
      BlockReason = reason,
      BlockingEffects = blockingEffects ?? []
    };
  }
}

/// <summary>
/// Service for managing item-based effects on characters.
/// Handles applying effects when items are picked up or equipped,
/// and removing effects when items are unequipped or dropped.
/// </summary>
public class ItemEffectService
{
  private readonly IChildDataPortal<EffectRecord> _effectPortal;

  public ItemEffectService(IChildDataPortal<EffectRecord> effectPortal)
  {
    _effectPortal = effectPortal;
  }

  #region Action Checks

  /// <summary>
  /// Checks if an item can be unequipped.
  /// </summary>
  /// <param name="character">The character.</param>
  /// <param name="itemId">The item to check.</param>
  /// <returns>Result indicating if unequip is allowed.</returns>
  public ItemActionCheckResult CanUnequipItem(CharacterEdit character, Guid itemId)
  {
    var blockingEffects = character.Effects
      .Where(e => e.SourceItemId == itemId && e.IsBlockingUnequip)
      .ToList();

    if (blockingEffects.Count > 0)
    {
      var effectNames = string.Join(", ", blockingEffects.Select(e => e.Name));
      return ItemActionCheckResult.Blocked(
        $"Cannot unequip: cursed by {effectNames}",
        blockingEffects);
    }

    return ItemActionCheckResult.Allowed();
  }

  /// <summary>
  /// Checks if an item can be dropped or transferred.
  /// </summary>
  /// <param name="character">The character.</param>
  /// <param name="itemId">The item to check.</param>
  /// <returns>Result indicating if drop is allowed.</returns>
  public ItemActionCheckResult CanDropItem(CharacterEdit character, Guid itemId)
  {
    var blockingEffects = character.Effects
      .Where(e => e.SourceItemId == itemId && e.IsBlockingDrop)
      .ToList();

    if (blockingEffects.Count > 0)
    {
      var effectNames = string.Join(", ", blockingEffects.Select(e => e.Name));
      return ItemActionCheckResult.Blocked(
        $"Cannot drop: cursed by {effectNames}",
        blockingEffects);
    }

    return ItemActionCheckResult.Allowed();
  }

  #endregion

  #region Apply Effects

  /// <summary>
  /// Applies effects when an item is picked up (added to inventory).
  /// </summary>
  /// <param name="character">The character picking up the item.</param>
  /// <param name="item">The item being picked up.</param>
  /// <param name="itemTemplate">The item's template with effect definitions.</param>
  /// <returns>Result of applying effects.</returns>
  public async Task<ItemEffectApplicationResult> OnItemPickedUpAsync(
    CharacterEdit character,
    CharacterItem item,
    ItemTemplate itemTemplate)
  {
    var appliedEffects = new List<EffectRecord>();

    foreach (var effectDef in itemTemplate.Effects.Where(e => e.IsActive))
    {
      // Apply effects triggered by possession or pickup
      if (effectDef.Trigger == ItemEffectTrigger.WhilePossessed ||
          effectDef.Trigger == ItemEffectTrigger.OnPickup)
      {
        var effect = await CreateAndApplyEffectAsync(character, item.Id, effectDef);
        if (effect != null)
        {
          appliedEffects.Add(effect);
        }
      }
    }

    return ItemEffectApplicationResult.Succeeded(appliedEffects);
  }

  /// <summary>
  /// Applies effects when an item is equipped.
  /// </summary>
  /// <param name="character">The character equipping the item.</param>
  /// <param name="item">The item being equipped.</param>
  /// <param name="itemTemplate">The item's template with effect definitions.</param>
  /// <returns>Result of applying effects.</returns>
  public async Task<ItemEffectApplicationResult> OnItemEquippedAsync(
    CharacterEdit character,
    CharacterItem item,
    ItemTemplate itemTemplate)
  {
    var appliedEffects = new List<EffectRecord>();

    foreach (var effectDef in itemTemplate.Effects.Where(e => e.IsActive))
    {
      // Apply effects triggered by equipping
      // Skip toggleable effects — those require explicit player activation
      if (effectDef.Trigger == ItemEffectTrigger.WhileEquipped && !effectDef.IsToggleable)
      {
        var effect = await CreateAndApplyEffectAsync(character, item.Id, effectDef);
        if (effect != null)
        {
          appliedEffects.Add(effect);
        }
      }
    }

    return ItemEffectApplicationResult.Succeeded(appliedEffects);
  }

  /// <summary>
  /// Applies effects for a newly acquired item that is immediately equipped.
  /// This handles both possession and equip triggers.
  /// </summary>
  /// <param name="character">The character.</param>
  /// <param name="item">The item.</param>
  /// <param name="itemTemplate">The item's template.</param>
  /// <returns>Result of applying effects.</returns>
  public async Task<ItemEffectApplicationResult> OnItemAcquiredAndEquippedAsync(
    CharacterEdit character,
    CharacterItem item,
    ItemTemplate itemTemplate)
  {
    var appliedEffects = new List<EffectRecord>();

    foreach (var effectDef in itemTemplate.Effects.Where(e => e.IsActive))
    {
      // Apply all relevant triggers
      // Skip toggleable WhileEquipped effects — those require explicit player activation
      if (effectDef.Trigger == ItemEffectTrigger.WhilePossessed ||
          effectDef.Trigger == ItemEffectTrigger.OnPickup ||
          (effectDef.Trigger == ItemEffectTrigger.WhileEquipped && !effectDef.IsToggleable))
      {
        var effect = await CreateAndApplyEffectAsync(character, item.Id, effectDef);
        if (effect != null)
        {
          appliedEffects.Add(effect);
        }
      }
    }

    return ItemEffectApplicationResult.Succeeded(appliedEffects);
  }

  #endregion

  #region Remove Effects

  /// <summary>
  /// Removes effects when an item is unequipped (moved to inventory).
  /// Only removes WhileEquipped effects; possession effects remain.
  /// </summary>
  /// <param name="character">The character unequipping the item.</param>
  /// <param name="itemId">The item being unequipped.</param>
  /// <returns>Result of the operation.</returns>
  public ItemEffectApplicationResult OnItemUnequipped(CharacterEdit character, Guid itemId)
  {
    // First check if allowed
    var check = CanUnequipItem(character, itemId);
    if (!check.IsAllowed)
    {
      return ItemEffectApplicationResult.Failed(check.BlockReason!);
    }

    // Remove only equip-triggered effects
    var removed = new List<Guid>();
    var toRemove = character.Effects
      .Where(e => e.SourceItemId == itemId && e.ItemEffectTrigger == ItemEffectTrigger.WhileEquipped)
      .ToList();

    foreach (var effect in toRemove)
    {
      removed.Add(effect.Id);
      character.Effects.RemoveEffect(effect.Id);
    }

    return new ItemEffectApplicationResult
    {
      Success = true,
      RemovedEffectIds = removed
    };
  }

  /// <summary>
  /// Removes effects when an item is dropped or transferred away.
  /// Removes all effects from this item (both possession and equip).
  /// </summary>
  /// <param name="character">The character dropping the item.</param>
  /// <param name="itemId">The item being dropped.</param>
  /// <returns>Result of the operation.</returns>
  public ItemEffectApplicationResult OnItemDropped(CharacterEdit character, Guid itemId)
  {
    // First check if allowed
    var check = CanDropItem(character, itemId);
    if (!check.IsAllowed)
    {
      return ItemEffectApplicationResult.Failed(check.BlockReason!);
    }

    // Remove all effects from this item
    var removed = new List<Guid>();
    var toRemove = character.Effects
      .Where(e => e.SourceItemId == itemId)
      .ToList();

    foreach (var effect in toRemove)
    {
      removed.Add(effect.Id);
      character.Effects.RemoveEffect(effect.Id);
    }

    return new ItemEffectApplicationResult
    {
      Success = true,
      RemovedEffectIds = removed
    };
  }

  #endregion

  #region Curse Management

  /// <summary>
  /// Attempts to remove a curse from an item, allowing it to be unequipped/dropped.
  /// Does not remove the item, just the curse effect.
  /// </summary>
  /// <param name="character">The character.</param>
  /// <param name="itemId">The cursed item.</param>
  /// <param name="removalPower">The power of the removal attempt (compared to curse difficulty).</param>
  /// <returns>Result of the curse removal attempt.</returns>
  public ItemEffectApplicationResult RemoveCurseFromItem(
    CharacterEdit character,
    Guid itemId,
    int removalPower)
  {
    var cursedEffects = character.Effects
      .Where(e => e.SourceItemId == itemId && e.IsCursed && e.IsActive)
      .ToList();

    if (cursedEffects.Count == 0)
    {
      return ItemEffectApplicationResult.Failed("No curse found on this item.");
    }

    var removed = new List<Guid>();

    foreach (var effect in cursedEffects)
    {
      var state = Behaviors.ItemEffectState.Deserialize(effect.BehaviorState);
      
      // Check if removal power is sufficient
      if (removalPower >= state.RemovalDifficulty)
      {
        removed.Add(effect.Id);
        character.Effects.RemoveEffect(effect.Id);
      }
    }

    if (removed.Count == 0)
    {
      return ItemEffectApplicationResult.Failed("Curse is too powerful to remove.");
    }

    return new ItemEffectApplicationResult
    {
      Success = true,
      RemovedEffectIds = removed
    };
  }

  /// <summary>
  /// Gets all cursed effects on a character.
  /// </summary>
  /// <param name="character">The character.</param>
  /// <returns>All cursed effects.</returns>
  public IEnumerable<EffectRecord> GetAllCursedEffects(CharacterEdit character)
  {
    return character.Effects.Where(e => e.IsCursed && e.IsActive);
  }

  /// <summary>
  /// Gets cursed effects grouped by source item.
  /// </summary>
  /// <param name="character">The character.</param>
  /// <returns>Dictionary of item IDs to their cursed effects.</returns>
  public Dictionary<Guid, List<EffectRecord>> GetCursedEffectsByItem(CharacterEdit character)
  {
    return character.Effects
      .Where(e => e.IsCursed && e.IsActive && e.SourceItemId.HasValue)
      .GroupBy(e => e.SourceItemId!.Value)
      .ToDictionary(g => g.Key, g => g.ToList());
  }

  #endregion

  #region Implant Toggle

  /// <summary>
  /// Activates a toggleable implant effect on a character.
  /// </summary>
  /// <param name="character">The character.</param>
  /// <param name="itemId">The equipped item's ID.</param>
  /// <param name="effectDef">The toggleable effect definition to activate.</param>
  /// <returns>Result of the toggle operation.</returns>
  public async Task<ItemEffectApplicationResult> ToggleImplantEffectOnAsync(
    CharacterEdit character,
    Guid itemId,
    ItemEffectDefinition effectDef)
  {
    if (!effectDef.IsToggleable)
      return ItemEffectApplicationResult.Failed("This effect is not toggleable.");

    // Check if already active
    var existing = character.Effects
      .FirstOrDefault(e => e.SourceItemId == itemId && e.Name == effectDef.Name);
    if (existing != null)
      return ItemEffectApplicationResult.Failed("This effect is already active.");

    // Check and deduct AP cost
    if (effectDef.ToggleApCost > 0)
    {
      if (character.ActionPoints.Available < effectDef.ToggleApCost)
        return ItemEffectApplicationResult.Failed(
          $"Insufficient AP. Need {effectDef.ToggleApCost}, have {character.ActionPoints.Available}.");

      character.ActionPoints.Available -= effectDef.ToggleApCost;
      character.ActionPoints.Spent += effectDef.ToggleApCost;
    }

    var effect = await CreateAndApplyEffectAsync(character, itemId, effectDef);
    if (effect == null)
      return ItemEffectApplicationResult.Failed("Failed to apply effect.");

    return ItemEffectApplicationResult.Succeeded([effect]);
  }

  /// <summary>
  /// Deactivates a toggleable implant effect on a character.
  /// </summary>
  /// <param name="character">The character.</param>
  /// <param name="itemId">The equipped item's ID.</param>
  /// <param name="effectDef">The toggleable effect definition to deactivate.</param>
  /// <returns>Result of the toggle operation.</returns>
  public ItemEffectApplicationResult ToggleImplantEffectOff(
    CharacterEdit character,
    Guid itemId,
    ItemEffectDefinition effectDef)
  {
    if (!effectDef.IsToggleable)
      return ItemEffectApplicationResult.Failed("This effect is not toggleable.");

    // Find the active effect
    var existing = character.Effects
      .FirstOrDefault(e => e.SourceItemId == itemId && e.Name == effectDef.Name);
    if (existing == null)
      return ItemEffectApplicationResult.Failed("This effect is not currently active.");

    // Check and deduct AP cost
    if (effectDef.ToggleApCost > 0)
    {
      if (character.ActionPoints.Available < effectDef.ToggleApCost)
        return ItemEffectApplicationResult.Failed(
          $"Insufficient AP. Need {effectDef.ToggleApCost}, have {character.ActionPoints.Available}.");

      character.ActionPoints.Available -= effectDef.ToggleApCost;
      character.ActionPoints.Spent += effectDef.ToggleApCost;
    }

    character.Effects.RemoveEffect(existing.Id);

    return new ItemEffectApplicationResult
    {
      Success = true,
      RemovedEffectIds = [existing.Id]
    };
  }

  #endregion

  #region Private Methods

  private async Task<EffectRecord?> CreateAndApplyEffectAsync(
    CharacterEdit character,
    Guid itemId,
    ItemEffectDefinition effectDef)
  {
    try
    {
      // Create the effect from the item definition
      var effect = await _effectPortal.CreateChildAsync(effectDef, itemId);

      // Add to character's effects
      if (character.Effects.AddEffect(effect))
      {
        return effect;
      }

      return null;
    }
    catch
    {
      // Log error in production
      return null;
    }
  }

  #endregion
}
