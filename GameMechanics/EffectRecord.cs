using Csla;
using GameMechanics.Effects;
using System;
using System.Collections.Generic;
using System.Linq;
using Threa.Dal.Dto;

namespace GameMechanics;

/// <summary>
/// Represents an active effect instance on a character.
/// Effects include wounds, buffs, debuffs, poisons, spell effects, etc.
/// </summary>
[Serializable]
public class EffectRecord : BusinessBase<EffectRecord>
{
  #region Properties

  public static readonly PropertyInfo<Guid> IdProperty = RegisterProperty<Guid>(nameof(Id));
  /// <summary>
  /// Unique identifier for this effect instance.
  /// </summary>
  public Guid Id
  {
    get => GetProperty(IdProperty);
    private set => LoadProperty(IdProperty, value);
  }

  public static readonly PropertyInfo<EffectType> EffectTypeProperty = RegisterProperty<EffectType>(nameof(EffectType));
  /// <summary>
  /// The type/category of this effect.
  /// </summary>
  public EffectType EffectType
  {
    get => GetProperty(EffectTypeProperty);
    private set => LoadProperty(EffectTypeProperty, value);
  }

  public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
  /// <summary>
  /// Display name for this effect.
  /// </summary>
  public string Name
  {
    get => GetProperty(NameProperty);
    set => SetProperty(NameProperty, value);
  }

  public static readonly PropertyInfo<string?> DescriptionProperty = RegisterProperty<string?>(nameof(Description));
  /// <summary>
  /// Detailed description of this effect.
  /// </summary>
  public string? Description
  {
    get => GetProperty(DescriptionProperty);
    set => SetProperty(DescriptionProperty, value);
  }

  public static readonly PropertyInfo<string?> LocationProperty = RegisterProperty<string?>(nameof(Location));
  /// <summary>
  /// Body location for location-specific effects (wounds, limb effects).
  /// Null for global effects.
  /// </summary>
  public string? Location
  {
    get => GetProperty(LocationProperty);
    set => SetProperty(LocationProperty, value);
  }

  public static readonly PropertyInfo<string?> IconNameProperty = RegisterProperty<string?>(nameof(IconName));
  /// <summary>
  /// Icon name for UI display.
  /// </summary>
  public string? IconName
  {
    get => GetProperty(IconNameProperty);
    set => SetProperty(IconNameProperty, value);
  }

  public static readonly PropertyInfo<int?> DurationRoundsProperty = RegisterProperty<int?>(nameof(DurationRounds));
  /// <summary>
  /// Total duration in rounds. Null for permanent/until-removed effects.
  /// </summary>
  public int? DurationRounds
  {
    get => GetProperty(DurationRoundsProperty);
    set => SetProperty(DurationRoundsProperty, value);
  }

  public static readonly PropertyInfo<int> ElapsedRoundsProperty = RegisterProperty<int>(nameof(ElapsedRounds));
  /// <summary>
  /// Number of rounds this effect has been active.
  /// </summary>
  public int ElapsedRounds
  {
    get => GetProperty(ElapsedRoundsProperty);
    set => SetProperty(ElapsedRoundsProperty, value);
  }

  public static readonly PropertyInfo<int> CurrentStacksProperty = RegisterProperty<int>(nameof(CurrentStacks));
  /// <summary>
  /// Current stack count for stackable effects.
  /// </summary>
  public int CurrentStacks
  {
    get => GetProperty(CurrentStacksProperty);
    set => SetProperty(CurrentStacksProperty, value);
  }

  public static readonly PropertyInfo<bool> IsActiveProperty = RegisterProperty<bool>(nameof(IsActive));
  /// <summary>
  /// Whether this effect is currently active (can be paused).
  /// </summary>
  public bool IsActive
  {
    get => GetProperty(IsActiveProperty);
    set => SetProperty(IsActiveProperty, value);
  }

  public static readonly PropertyInfo<string?> BehaviorStateProperty = RegisterProperty<string?>(nameof(BehaviorState));
  /// <summary>
  /// Arbitrary state data used by the effect behavior implementation.
  /// Typically JSON, but format is up to the behavior.
  /// </summary>
  public string? BehaviorState
  {
    get => GetProperty(BehaviorStateProperty);
    set => SetProperty(BehaviorStateProperty, value);
  }

  public static readonly PropertyInfo<string?> SourceProperty = RegisterProperty<string?>(nameof(Source));
  /// <summary>
  /// Description of what caused this effect (spell name, item, etc.).
  /// </summary>
  public string? Source
  {
    get => GetProperty(SourceProperty);
    set => SetProperty(SourceProperty, value);
  }

  public static readonly PropertyInfo<Guid?> SourceItemIdProperty = RegisterProperty<Guid?>(nameof(SourceItemId));
  /// <summary>
  /// If this effect was created by an item, the ID of that CharacterItem instance.
  /// Used to remove effects when items are unequipped or dropped.
  /// </summary>
  public Guid? SourceItemId
  {
    get => GetProperty(SourceItemIdProperty);
    set => SetProperty(SourceItemIdProperty, value);
  }

  public static readonly PropertyInfo<ItemEffectTrigger> ItemEffectTriggerProperty = RegisterProperty<ItemEffectTrigger>(nameof(ItemEffectTrigger));
  /// <summary>
  /// If this effect is from an item, specifies when the effect activates/deactivates.
  /// </summary>
  public ItemEffectTrigger ItemEffectTrigger
  {
    get => GetProperty(ItemEffectTriggerProperty);
    set => SetProperty(ItemEffectTriggerProperty, value);
  }

  public static readonly PropertyInfo<bool> IsCursedProperty = RegisterProperty<bool>(nameof(IsCursed));
  /// <summary>
  /// Whether this effect is cursed and prevents unequipping the source item.
  /// </summary>
  public bool IsCursed
  {
    get => GetProperty(IsCursedProperty);
    set => SetProperty(IsCursedProperty, value);
  }

  #endregion

  #region Computed Properties

  /// <summary>
  /// Whether this effect originated from an item.
  /// </summary>
  public bool IsFromItem => SourceItemId.HasValue;

  /// <summary>
  /// Whether this effect is currently blocking item unequip (cursed, active, and triggered by equip).
  /// </summary>
  public bool IsBlockingUnequip => IsFromItem 
    && IsCursed 
    && IsActive 
    && ItemEffectTrigger == ItemEffectTrigger.WhileEquipped;

  /// <summary>
  /// Whether this effect is currently blocking item drop (cursed, active, and triggered by possession).
  /// </summary>
  public bool IsBlockingDrop => IsFromItem 
    && IsCursed 
    && IsActive 
    && (ItemEffectTrigger == ItemEffectTrigger.WhilePossessed || ItemEffectTrigger == ItemEffectTrigger.OnPickup);

  /// <summary>
  /// Whether this effect is blocking any form of item removal (unequip or drop).
  /// </summary>
  public bool IsBlockingItemRemoval => IsBlockingUnequip || IsBlockingDrop;

  /// <summary>
  /// Remaining duration in rounds, or null if permanent.
  /// </summary>
  public int? RemainingRounds => DurationRounds.HasValue ? DurationRounds.Value - ElapsedRounds : null;

  /// <summary>
  /// Whether this effect has expired based on duration.
  /// </summary>
  public bool IsExpired => DurationRounds.HasValue && ElapsedRounds >= DurationRounds.Value;

  /// <summary>
  /// Progress through the effect's duration (0.0 to 1.0), or null if permanent.
  /// </summary>
  public double? DurationProgress => DurationRounds.HasValue && DurationRounds.Value > 0
    ? (double)ElapsedRounds / DurationRounds.Value
    : null;

  /// <summary>
  /// Gets the behavior implementation for this effect type.
  /// </summary>
  public IEffectBehavior Behavior => EffectBehaviorFactory.GetBehavior(EffectType);

  #endregion

  #region Methods

  /// <summary>
  /// Gets attribute modifiers from this effect.
  /// </summary>
  public IEnumerable<EffectModifier> GetAttributeModifiers(string attributeName, int baseValue)
  {
    if (!IsActive)
      return [];
    return Behavior.GetAttributeModifiers(this, attributeName, baseValue);
  }

  /// <summary>
  /// Gets ability score modifiers from this effect.
  /// </summary>
  public IEnumerable<EffectModifier> GetAbilityScoreModifiers(string skillName, string attributeName, int currentAS)
  {
    if (!IsActive)
      return [];
    return Behavior.GetAbilityScoreModifiers(this, skillName, attributeName, currentAS);
  }

  /// <summary>
  /// Gets success value modifiers from this effect.
  /// </summary>
  public IEnumerable<EffectModifier> GetSuccessValueModifiers(string actionType, int currentSV)
  {
    if (!IsActive)
      return [];
    return Behavior.GetSuccessValueModifiers(this, actionType, currentSV);
  }

  #endregion

  #region Data Access

  [CreateChild]
  private void Create(EffectType effectType, string name, string? location, int? durationRounds, string? behaviorState)
  {
    using (BypassPropertyChecks)
    {
      Id = Guid.NewGuid();
      EffectType = effectType;
      Name = name;
      Location = location;
      DurationRounds = durationRounds;
      BehaviorState = behaviorState;
      ElapsedRounds = 0;
      CurrentStacks = 1;
      IsActive = true;
    }
  }

  /// <summary>
  /// Creates an effect from an item effect definition.
  /// </summary>
  [CreateChild]
  private void CreateFromItem(ItemEffectDefinition definition, Guid sourceItemId)
  {
    using (BypassPropertyChecks)
    {
      Id = Guid.NewGuid();
      EffectType = definition.EffectType;
      Name = definition.Name;
      Description = definition.Description;
      IconName = definition.IconName;
      DurationRounds = definition.DurationRounds; // null = permanent while trigger active
      BehaviorState = definition.BehaviorState;
      ElapsedRounds = 0;
      CurrentStacks = 1;
      IsActive = true;
      
      // Item-specific properties
      SourceItemId = sourceItemId;
      ItemEffectTrigger = definition.Trigger;
      IsCursed = definition.IsCursed;
      Source = definition.Name; // Use effect name as source description
    }
  }

  [FetchChild]
  private void Fetch(CharacterEffect dto)
  {
    using (BypassPropertyChecks)
    {
      Id = dto.Id;
      EffectType = dto.Definition?.EffectType ?? EffectType.Condition;
      Name = dto.Definition?.Name ?? "Unknown Effect";
      Description = dto.Definition?.Description;
      Location = dto.WoundLocation;
      IconName = dto.Definition?.IconName;
      DurationRounds = dto.RoundsRemaining.HasValue
        ? (dto.RoundsRemaining.Value + (int)(dto.StartTime - DateTime.UtcNow).TotalSeconds / 3)
        : null;
      ElapsedRounds = dto.RoundsRemaining.HasValue
        ? (DurationRounds ?? 0) - dto.RoundsRemaining.Value
        : 0;
      CurrentStacks = dto.CurrentStacks;
      IsActive = dto.IsActive;
      BehaviorState = dto.CustomProperties;
      Source = dto.Definition?.Source;
      
      // Item effect properties
      SourceItemId = dto.SourceItemId;
      ItemEffectTrigger = dto.ItemEffectTrigger;
      IsCursed = dto.IsCursed;
    }
  }

  [InsertChild]
  [UpdateChild]
  private void InsertUpdate(List<CharacterEffect> effects)
  {
    using (BypassPropertyChecks)
    {
      CharacterEffect dto;
      if (IsNew)
      {
        dto = new CharacterEffect
        {
          Id = Id,
          EffectDefinitionId = GetEffectDefinitionId(EffectType),
          StartTime = DateTime.UtcNow
        };
        effects.Add(dto);
      }
      else
      {
        dto = effects.FirstOrDefault(e => e.Id == Id) ?? new CharacterEffect { Id = Id };
        if (!effects.Contains(dto))
          effects.Add(dto);
      }

      dto.WoundLocation = Location;
      dto.RoundsRemaining = RemainingRounds;
      dto.CurrentStacks = CurrentStacks;
      dto.IsActive = IsActive;
      dto.CustomProperties = BehaviorState;
      dto.EffectDefinitionId = GetEffectDefinitionId(EffectType);
      
      // Item effect properties
      dto.SourceItemId = SourceItemId;
      dto.ItemEffectTrigger = ItemEffectTrigger;
      dto.IsCursed = IsCursed;
    }
  }

  [DeleteSelfChild]
  private void Delete(List<CharacterEffect> effects)
  {
    if (IsNew) return;
    var toRemove = effects.FirstOrDefault(e => e.Id == Id);
    if (toRemove != null)
      effects.Remove(toRemove);
  }

  /// <summary>
  /// Maps EffectType to the corresponding EffectDefinitionId.
  /// </summary>
  private static int GetEffectDefinitionId(EffectType effectType)
  {
    return effectType switch
    {
      EffectType.Wound => 1,
      EffectType.Condition => 10,
      EffectType.Poison => 20,
      EffectType.Disease => 25,
      EffectType.Buff => 30,
      EffectType.Debuff => 40,
      EffectType.SpellEffect => 50,
      EffectType.ObjectEffect => 60,
      EffectType.Environmental => 70,
      _ => 1
    };
  }

  #endregion
}
