using System;
using Csla;
using Csla.Core;
using Csla.Rules;
using Csla.Rules.CommonRules;
using Threa.Dal.Dto;

namespace GameMechanics.Items;

/// <summary>
/// Editable child business object for an item effect definition.
/// These effects are applied to characters when items are equipped, possessed, or used.
/// </summary>
[Serializable]
public class ItemEffectEdit : BusinessBase<ItemEffectEdit>
{
    public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id));
    public int Id
    {
        get => GetProperty(IdProperty);
        private set => LoadProperty(IdProperty, value);
    }

    public static readonly PropertyInfo<int> ItemTemplateIdProperty = RegisterProperty<int>(nameof(ItemTemplateId));
    public int ItemTemplateId
    {
        get => GetProperty(ItemTemplateIdProperty);
        private set => LoadProperty(ItemTemplateIdProperty, value);
    }

    public static readonly PropertyInfo<int?> EffectDefinitionIdProperty = RegisterProperty<int?>(nameof(EffectDefinitionId));
    public int? EffectDefinitionId
    {
        get => GetProperty(EffectDefinitionIdProperty);
        set => SetProperty(EffectDefinitionIdProperty, value);
    }

    public static readonly PropertyInfo<EffectType> EffectTypeProperty = RegisterProperty<EffectType>(nameof(EffectType));
    public EffectType EffectType
    {
        get => GetProperty(EffectTypeProperty);
        set => SetProperty(EffectTypeProperty, value);
    }

    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    public string Name
    {
        get => GetProperty(NameProperty);
        set => SetProperty(NameProperty, value);
    }

    public static readonly PropertyInfo<string?> DescriptionProperty = RegisterProperty<string?>(nameof(Description));
    public string? Description
    {
        get => GetProperty(DescriptionProperty);
        set => SetProperty(DescriptionProperty, value);
    }

    public static readonly PropertyInfo<ItemEffectTrigger> TriggerProperty = RegisterProperty<ItemEffectTrigger>(nameof(Trigger));
    public ItemEffectTrigger Trigger
    {
        get => GetProperty(TriggerProperty);
        set => SetProperty(TriggerProperty, value);
    }

    public static readonly PropertyInfo<bool> IsCursedProperty = RegisterProperty<bool>(nameof(IsCursed));
    public bool IsCursed
    {
        get => GetProperty(IsCursedProperty);
        set => SetProperty(IsCursedProperty, value);
    }

    public static readonly PropertyInfo<bool> RequiresAttunementProperty = RegisterProperty<bool>(nameof(RequiresAttunement));
    public bool RequiresAttunement
    {
        get => GetProperty(RequiresAttunementProperty);
        set => SetProperty(RequiresAttunementProperty, value);
    }

    public static readonly PropertyInfo<int?> DurationRoundsProperty = RegisterProperty<int?>(nameof(DurationRounds));
    public int? DurationRounds
    {
        get => GetProperty(DurationRoundsProperty);
        set => SetProperty(DurationRoundsProperty, value);
    }

    public static readonly PropertyInfo<string?> BehaviorStateProperty = RegisterProperty<string?>(nameof(BehaviorState));
    public string? BehaviorState
    {
        get => GetProperty(BehaviorStateProperty);
        set => SetProperty(BehaviorStateProperty, value);
    }

    public static readonly PropertyInfo<string?> IconNameProperty = RegisterProperty<string?>(nameof(IconName));
    public string? IconName
    {
        get => GetProperty(IconNameProperty);
        set => SetProperty(IconNameProperty, value);
    }

    public static readonly PropertyInfo<bool> IsActiveProperty = RegisterProperty<bool>(nameof(IsActive));
    public bool IsActive
    {
        get => GetProperty(IsActiveProperty);
        set => SetProperty(IsActiveProperty, value);
    }

    public static readonly PropertyInfo<int> PriorityProperty = RegisterProperty<int>(nameof(Priority));
    public int Priority
    {
        get => GetProperty(PriorityProperty);
        set => SetProperty(PriorityProperty, value);
    }

    public static readonly PropertyInfo<bool> IsToggleableProperty = RegisterProperty<bool>(nameof(IsToggleable));
    public bool IsToggleable
    {
        get => GetProperty(IsToggleableProperty);
        set => SetProperty(IsToggleableProperty, value);
    }

    public static readonly PropertyInfo<int> ToggleApCostProperty = RegisterProperty<int>(nameof(ToggleApCost));
    public int ToggleApCost
    {
        get => GetProperty(ToggleApCostProperty);
        set => SetProperty(ToggleApCostProperty, value);
    }

    /// <summary>
    /// Gets the display name for the trigger type.
    /// </summary>
    public string TriggerDisplayName => GetTriggerDisplayName(Trigger);

    /// <summary>
    /// Gets a summary of the effect for display in lists.
    /// </summary>
    public string EffectSummary => BuildEffectSummary();

    protected override void AddBusinessRules()
    {
        base.AddBusinessRules();

        BusinessRules.AddRule(new Required(NameProperty) { MessageText = "Effect name is required." });
        BusinessRules.AddRule(new TriggerRequiredRule(TriggerProperty));
    }

    private string BuildEffectSummary()
    {
        var parts = new System.Collections.Generic.List<string>();

        if (IsToggleable)
            parts.Add(ToggleApCost > 0 ? $"Toggleable ({ToggleApCost} AP)" : "Toggleable (Free)");

        if (IsCursed)
            parts.Add("Cursed");

        if (RequiresAttunement)
            parts.Add("Attunement");

        if (DurationRounds.HasValue)
            parts.Add($"{DurationRounds} rounds");

        if (!string.IsNullOrEmpty(Description))
            parts.Add(Description);

        return parts.Count > 0 ? string.Join(" | ", parts) : EffectType.ToString();
    }

    private static string GetTriggerDisplayName(ItemEffectTrigger trigger) => trigger switch
    {
        ItemEffectTrigger.None => "None",
        ItemEffectTrigger.WhileEquipped => "While Equipped",
        ItemEffectTrigger.WhilePossessed => "While Possessed",
        ItemEffectTrigger.OnUse => "On Use",
        ItemEffectTrigger.OnAttackWith => "On Attack With",
        ItemEffectTrigger.OnHitWhileWearing => "On Hit While Wearing",
        ItemEffectTrigger.OnCritical => "On Critical",
        ItemEffectTrigger.OnPickup => "On Pickup",
        _ => trigger.ToString()
    };

    /// <summary>
    /// Gets a description of what the trigger does.
    /// </summary>
    public static string GetTriggerDescription(ItemEffectTrigger trigger) => trigger switch
    {
        ItemEffectTrigger.None => "Not from an item",
        ItemEffectTrigger.WhileEquipped => "Active while the item is equipped in a slot",
        ItemEffectTrigger.WhilePossessed => "Active while the item is in inventory (equipped or not)",
        ItemEffectTrigger.OnUse => "Applied when the item is used (consumables)",
        ItemEffectTrigger.OnAttackWith => "Applied when attacking with this weapon",
        ItemEffectTrigger.OnHitWhileWearing => "Applied when hit while wearing this item",
        ItemEffectTrigger.OnCritical => "Applied on critical hit with this weapon",
        ItemEffectTrigger.OnPickup => "Applied once when the item is first acquired",
        _ => string.Empty
    };

    /// <summary>
    /// Gets a description of how a curse blocks item actions for this trigger type.
    /// </summary>
    public static string GetCurseBlockingDescription(ItemEffectTrigger trigger) => trigger switch
    {
        ItemEffectTrigger.WhileEquipped => "Character cannot unequip this item until the curse is removed",
        ItemEffectTrigger.WhilePossessed or ItemEffectTrigger.OnPickup =>
            "Character cannot drop or transfer this item until the curse is removed",
        _ => "This effect cannot be removed normally"
    };

    [CreateChild]
    private void Create(int itemTemplateId)
    {
        using (BypassPropertyChecks)
        {
            Id = 0;
            ItemTemplateId = itemTemplateId;
            EffectDefinitionId = null;
            EffectType = EffectType.ItemEffect;
            Name = string.Empty;
            Description = null;
            Trigger = ItemEffectTrigger.WhileEquipped;
            IsCursed = false;
            RequiresAttunement = false;
            DurationRounds = null;
            BehaviorState = null;
            IconName = null;
            IsActive = true;
            Priority = 0;
            IsToggleable = false;
            ToggleApCost = 0;
        }
        BusinessRules.CheckRules();
    }

    [FetchChild]
    private void Fetch(ItemEffectDefinition dto)
    {
        using (BypassPropertyChecks)
        {
            Id = dto.Id;
            ItemTemplateId = dto.ItemTemplateId;
            EffectDefinitionId = dto.EffectDefinitionId;
            EffectType = dto.EffectType;
            Name = dto.Name;
            Description = dto.Description;
            Trigger = dto.Trigger;
            IsCursed = dto.IsCursed;
            RequiresAttunement = dto.RequiresAttunement;
            DurationRounds = dto.DurationRounds;
            BehaviorState = dto.BehaviorState;
            IconName = dto.IconName;
            IsActive = dto.IsActive;
            Priority = dto.Priority;
            IsToggleable = dto.IsToggleable;
            ToggleApCost = dto.ToggleApCost;
        }
        BusinessRules.CheckRules();
    }

    /// <summary>
    /// Converts this business object to a DTO for persistence.
    /// </summary>
    internal ItemEffectDefinition ToDto()
    {
        return new ItemEffectDefinition
        {
            Id = Id,
            ItemTemplateId = ItemTemplateId,
            EffectDefinitionId = EffectDefinitionId,
            EffectType = EffectType,
            Name = Name,
            Description = Description,
            Trigger = Trigger,
            IsCursed = IsCursed,
            RequiresAttunement = RequiresAttunement,
            DurationRounds = DurationRounds,
            BehaviorState = BehaviorState,
            IconName = IconName,
            IsActive = IsActive,
            Priority = Priority,
            IsToggleable = IsToggleable,
            ToggleApCost = ToggleApCost
        };
    }

    // Child objects don't need Insert/Update/Delete - they're saved with the parent
    // The parent ItemTemplateEdit converts the entire Effects list to DTOs when saving
}

/// <summary>
/// Validates that a trigger type has been selected (not None).
/// </summary>
public class TriggerRequiredRule : BusinessRule
{
    public TriggerRequiredRule(IPropertyInfo primaryProperty)
        : base(primaryProperty)
    {
        InputProperties.Add(primaryProperty);
    }

    protected override void Execute(IRuleContext context)
    {
        var value = (ItemEffectTrigger)context.InputPropertyValues[PrimaryProperty]!;
        if (value == ItemEffectTrigger.None)
        {
            context.AddErrorResult("A trigger type must be selected.");
        }
    }
}
