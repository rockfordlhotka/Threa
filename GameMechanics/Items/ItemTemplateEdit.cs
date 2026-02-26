using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Csla;
using Csla.Core;
using Csla.Rules;
using Csla.Rules.CommonRules;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Items;

[Serializable]
public class ItemTemplateEdit : BusinessBase<ItemTemplateEdit>
{
    public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id));
    public int Id
    {
        get => GetProperty(IdProperty);
        private set => LoadProperty(IdProperty, value);
    }

    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    public string Name
    {
        get => GetProperty(NameProperty);
        set => SetProperty(NameProperty, value);
    }

    public static readonly PropertyInfo<string> DescriptionProperty = RegisterProperty<string>(nameof(Description));
    public string Description
    {
        get => GetProperty(DescriptionProperty);
        set => SetProperty(DescriptionProperty, value);
    }

    public static readonly PropertyInfo<string> ShortDescriptionProperty = RegisterProperty<string>(nameof(ShortDescription));
    public string ShortDescription
    {
        get => GetProperty(ShortDescriptionProperty);
        set => SetProperty(ShortDescriptionProperty, value);
    }

    public static readonly PropertyInfo<ItemType> ItemTypeProperty = RegisterProperty<ItemType>(nameof(ItemType));
    public ItemType ItemType
    {
        get => GetProperty(ItemTypeProperty);
        set => SetProperty(ItemTypeProperty, value);
    }

    public static readonly PropertyInfo<WeaponType> WeaponTypeProperty = RegisterProperty<WeaponType>(nameof(WeaponType));
    public WeaponType WeaponType
    {
        get => GetProperty(WeaponTypeProperty);
        set => SetProperty(WeaponTypeProperty, value);
    }

    public static readonly PropertyInfo<EquipmentSlot> EquipmentSlotProperty = RegisterProperty<EquipmentSlot>(nameof(EquipmentSlot));
    public EquipmentSlot EquipmentSlot
    {
        get => GetProperty(EquipmentSlotProperty);
        set => SetProperty(EquipmentSlotProperty, value);
    }

    public static readonly PropertyInfo<List<EquipmentSlot>> EquipmentSlotsProperty = RegisterProperty<List<EquipmentSlot>>(nameof(EquipmentSlots));
    /// <summary>
    /// Valid slots for this item. Replaces EquipmentSlot as canonical field.
    /// </summary>
    public List<EquipmentSlot> EquipmentSlots
    {
        get => GetProperty(EquipmentSlotsProperty) ?? [];
        set => SetProperty(EquipmentSlotsProperty, value);
    }

    public static readonly PropertyInfo<bool> OccupiesAllSlotsProperty = RegisterProperty<bool>(nameof(OccupiesAllSlots));
    /// <summary>
    /// When true, equipping occupies ALL slots in EquipmentSlots simultaneously.
    /// When false, the player picks one slot.
    /// </summary>
    public bool OccupiesAllSlots
    {
        get => GetProperty(OccupiesAllSlotsProperty);
        set => SetProperty(OccupiesAllSlotsProperty, value);
    }

    public static readonly PropertyInfo<decimal> WeightProperty = RegisterProperty<decimal>(nameof(Weight));
    public decimal Weight
    {
        get => GetProperty(WeightProperty);
        set => SetProperty(WeightProperty, value);
    }

    public static readonly PropertyInfo<decimal> VolumeProperty = RegisterProperty<decimal>(nameof(Volume));
    public decimal Volume
    {
        get => GetProperty(VolumeProperty);
        set => SetProperty(VolumeProperty, value);
    }

    public static readonly PropertyInfo<int> ValueProperty = RegisterProperty<int>(nameof(Value));
    public int Value
    {
        get => GetProperty(ValueProperty);
        set => SetProperty(ValueProperty, value);
    }

    public static readonly PropertyInfo<bool> IsStackableProperty = RegisterProperty<bool>(nameof(IsStackable));
    public bool IsStackable
    {
        get => GetProperty(IsStackableProperty);
        set => SetProperty(IsStackableProperty, value);
    }

    public static readonly PropertyInfo<int> MaxStackSizeProperty = RegisterProperty<int>(nameof(MaxStackSize));
    public int MaxStackSize
    {
        get => GetProperty(MaxStackSizeProperty);
        set => SetProperty(MaxStackSizeProperty, value);
    }

    public static readonly PropertyInfo<bool> HasDurabilityProperty = RegisterProperty<bool>(nameof(HasDurability));
    public bool HasDurability
    {
        get => GetProperty(HasDurabilityProperty);
        set => SetProperty(HasDurabilityProperty, value);
    }

    public static readonly PropertyInfo<int?> MaxDurabilityProperty = RegisterProperty<int?>(nameof(MaxDurability));
    public int? MaxDurability
    {
        get => GetProperty(MaxDurabilityProperty);
        set => SetProperty(MaxDurabilityProperty, value);
    }

    public static readonly PropertyInfo<ItemRarity> RarityProperty = RegisterProperty<ItemRarity>(nameof(Rarity));
    public ItemRarity Rarity
    {
        get => GetProperty(RarityProperty);
        set => SetProperty(RarityProperty, value);
    }

    public static readonly PropertyInfo<bool> IsActiveProperty = RegisterProperty<bool>(nameof(IsActive));
    public bool IsActive
    {
        get => GetProperty(IsActiveProperty);
        set => SetProperty(IsActiveProperty, value);
    }

    public static readonly PropertyInfo<bool> IsVirtualProperty = RegisterProperty<bool>(nameof(IsVirtual));
    /// <summary>
    /// Whether this is a virtual weapon (innate ability like punch/kick).
    /// Virtual weapons are not shown in inventory or equipment UI,
    /// but appear as combat options when conditions are met.
    /// </summary>
    public bool IsVirtual
    {
        get => GetProperty(IsVirtualProperty);
        set => SetProperty(IsVirtualProperty, value);
    }

    public static readonly PropertyInfo<string?> TagsProperty = RegisterProperty<string?>(nameof(Tags));
    public string? Tags
    {
        get => GetProperty(TagsProperty);
        set => SetProperty(TagsProperty, value);
    }

    public static readonly PropertyInfo<string?> RelatedSkillProperty = RegisterProperty<string?>(nameof(RelatedSkill));
    public string? RelatedSkill
    {
        get => GetProperty(RelatedSkillProperty);
        set => SetProperty(RelatedSkillProperty, value);
    }

    public static readonly PropertyInfo<int> MinSkillLevelProperty = RegisterProperty<int>(nameof(MinSkillLevel));
    public int MinSkillLevel
    {
        get => GetProperty(MinSkillLevelProperty);
        set => SetProperty(MinSkillLevelProperty, value);
    }

    public static readonly PropertyInfo<int> DamageClassProperty = RegisterProperty<int>(nameof(DamageClass));
    public int DamageClass
    {
        get => GetProperty(DamageClassProperty);
        set => SetProperty(DamageClassProperty, value);
    }

    public static readonly PropertyInfo<string> DamageTypeProperty = RegisterProperty<string>(nameof(DamageType));
    public string DamageType
    {
        get => GetProperty(DamageTypeProperty);
        set => SetProperty(DamageTypeProperty, value);
    }

    public static readonly PropertyInfo<int> SVModifierProperty = RegisterProperty<int>(nameof(SVModifier));
    public int SVModifier
    {
        get => GetProperty(SVModifierProperty);
        set => SetProperty(SVModifierProperty, value);
    }

    public static readonly PropertyInfo<int> AVModifierProperty = RegisterProperty<int>(nameof(AVModifier));
    public int AVModifier
    {
        get => GetProperty(AVModifierProperty);
        set => SetProperty(AVModifierProperty, value);
    }

    public static readonly PropertyInfo<int> DodgeModifierProperty = RegisterProperty<int>(nameof(DodgeModifier));
    public int DodgeModifier
    {
        get => GetProperty(DodgeModifierProperty);
        set => SetProperty(DodgeModifierProperty, value);
    }

    public static readonly PropertyInfo<int?> RangeProperty = RegisterProperty<int?>(nameof(Range));
    public int? Range
    {
        get => GetProperty(RangeProperty);
        set => SetProperty(RangeProperty, value);
    }

    public static readonly PropertyInfo<string?> ArmorAbsorptionProperty = RegisterProperty<string?>(nameof(ArmorAbsorption));
    public string? ArmorAbsorption
    {
        get => GetProperty(ArmorAbsorptionProperty);
        set => SetProperty(ArmorAbsorptionProperty, value);
    }

    public static readonly PropertyInfo<string?> WeaponDamageProperty = RegisterProperty<string?>(nameof(WeaponDamage));
    /// <summary>
    /// Per-damage-type SV modifiers for weapons (JSON).
    /// Format: {"Cutting": 4, "Energy": 2}
    /// </summary>
    public string? WeaponDamage
    {
        get => GetProperty(WeaponDamageProperty);
        set => SetProperty(WeaponDamageProperty, value);
    }

    public static readonly PropertyInfo<string?> CustomPropertiesProperty = RegisterProperty<string?>(nameof(CustomProperties));
    public string? CustomProperties
    {
        get => GetProperty(CustomPropertiesProperty);
        set => SetProperty(CustomPropertiesProperty, value);
    }

    // Container properties
    public static readonly PropertyInfo<bool> IsContainerProperty = RegisterProperty<bool>(nameof(IsContainer));
    public bool IsContainer
    {
        get => GetProperty(IsContainerProperty);
        set => SetProperty(IsContainerProperty, value);
    }

    public static readonly PropertyInfo<decimal?> ContainerMaxWeightProperty = RegisterProperty<decimal?>(nameof(ContainerMaxWeight));
    public decimal? ContainerMaxWeight
    {
        get => GetProperty(ContainerMaxWeightProperty);
        set => SetProperty(ContainerMaxWeightProperty, value);
    }

    public static readonly PropertyInfo<decimal?> ContainerMaxVolumeProperty = RegisterProperty<decimal?>(nameof(ContainerMaxVolume));
    public decimal? ContainerMaxVolume
    {
        get => GetProperty(ContainerMaxVolumeProperty);
        set => SetProperty(ContainerMaxVolumeProperty, value);
    }

    public static readonly PropertyInfo<string?> ContainerAllowedTypesProperty = RegisterProperty<string?>(nameof(ContainerAllowedTypes));
    public string? ContainerAllowedTypes
    {
        get => GetProperty(ContainerAllowedTypesProperty);
        set => SetProperty(ContainerAllowedTypesProperty, value);
    }

    public static readonly PropertyInfo<decimal> ContainerWeightReductionProperty = RegisterProperty<decimal>(nameof(ContainerWeightReduction));
    public decimal ContainerWeightReduction
    {
        get => GetProperty(ContainerWeightReductionProperty);
        set => SetProperty(ContainerWeightReductionProperty, value);
    }

    public static readonly PropertyInfo<ItemEffectEditList> EffectsProperty = RegisterProperty<ItemEffectEditList>(nameof(Effects));
    /// <summary>
    /// Magic/tech effects that this item can apply to a character.
    /// These are applied based on their trigger conditions (equipped, possessed, used, etc.).
    /// </summary>
    public ItemEffectEditList Effects
    {
        get => GetProperty(EffectsProperty);
        private set => LoadProperty(EffectsProperty, value);
    }

    protected override void AddBusinessRules()
    {
        base.AddBusinessRules();

        // Required field validation
        BusinessRules.AddRule(new Required(NameProperty) { MessageText = "Name is required." });
        BusinessRules.AddRule(new ItemTypeRequiredRule(ItemTypeProperty));

        // Numeric range validation - weight and volume must be non-negative
        BusinessRules.AddRule(new MinValue<decimal>(WeightProperty, 0) { MessageText = "Weight must be non-negative." });
        BusinessRules.AddRule(new MinValue<decimal>(VolumeProperty, 0) { MessageText = "Volume must be non-negative." });

        // Container capacity validation - warning when container has no capacity set
        BusinessRules.AddRule(new ContainerCapacityWarningRule(IsContainerProperty, ContainerMaxWeightProperty, ContainerMaxVolumeProperty));

        // Ammo container validation - ammo containers cannot be stackable
        BusinessRules.AddRule(new AmmoContainerNotStackableRule(ItemTypeProperty, IsStackableProperty));
    }

    [Create]
    private async Task Create([Inject] IChildDataPortal<ItemEffectEditList> effectsPortal)
    {
        using (BypassPropertyChecks)
        {
            Id = 0;
            Name = string.Empty;
            Description = string.Empty;
            ShortDescription = string.Empty;
            ItemType = ItemType.Miscellaneous;
            WeaponType = WeaponType.None;
            EquipmentSlot = EquipmentSlot.None;
            EquipmentSlots = [];
            OccupiesAllSlots = false;
            Weight = 0;
            Volume = 0;
            Value = 0;
            IsStackable = false;
            MaxStackSize = 1;
            HasDurability = false;
            MaxDurability = 0;
            Rarity = ItemRarity.Common;
            IsActive = true;
            IsVirtual = false;
            Tags = null;
            RelatedSkill = null;
            MinSkillLevel = 0;
            DamageClass = 1;
            DamageType = string.Empty;
            SVModifier = 0;
            AVModifier = 0;
            DodgeModifier = 0;
            Range = null;
            ArmorAbsorption = null;
            WeaponDamage = null;
            CustomProperties = null;
            IsContainer = false;
            ContainerMaxWeight = null;
            ContainerMaxVolume = null;
            ContainerAllowedTypes = null;
            ContainerWeightReduction = 1.0m;
            Effects = effectsPortal.CreateChild();
        }
        BusinessRules.CheckRules();
        await Task.CompletedTask;
    }

    [Fetch]
    private async Task Fetch(int id, [Inject] IItemTemplateDal dal, [Inject] IChildDataPortal<ItemEffectEditList> effectsPortal)
    {
        var data = await dal.GetTemplateAsync(id)
            ?? throw new InvalidOperationException($"ItemTemplate {id} not found");
        LoadFromDto(data, effectsPortal);
    }

    private void LoadFromDto(ItemTemplate data, IChildDataPortal<ItemEffectEditList>? effectsPortal = null)
    {
        using (BypassPropertyChecks)
        {
            Id = data.Id;
            Name = data.Name;
            Description = data.Description;
            ShortDescription = data.ShortDescription;
            ItemType = data.ItemType;
            WeaponType = data.WeaponType;
            EquipmentSlot = data.EquipmentSlot;
            EquipmentSlots = new List<EquipmentSlot>(data.EquipmentSlots);
            OccupiesAllSlots = data.OccupiesAllSlots;
            Weight = data.Weight;
            Volume = data.Volume;
            Value = data.Value;
            IsStackable = data.IsStackable;
            MaxStackSize = data.MaxStackSize;
            HasDurability = data.HasDurability;
            MaxDurability = data.MaxDurability;
            Rarity = data.Rarity;
            IsActive = data.IsActive;
            IsVirtual = data.IsVirtual;
            Tags = data.Tags;
            RelatedSkill = data.RelatedSkill;
            MinSkillLevel = data.MinSkillLevel;
            DamageClass = data.DamageClass;
            DamageType = data.DamageType ?? string.Empty;
            SVModifier = data.SVModifier;
            AVModifier = data.AVModifier;
            DodgeModifier = data.DodgeModifier;
            Range = data.Range;
            ArmorAbsorption = data.ArmorAbsorption;
            // Migrate legacy single-type weapon damage to multi-type JSON
            if (!string.IsNullOrWhiteSpace(data.WeaponDamage))
            {
                WeaponDamage = data.WeaponDamage;
            }
            else if (!string.IsNullOrWhiteSpace(data.DamageType) || data.SVModifier != 0)
            {
                var profile = Combat.WeaponDamageProfile.FromLegacy(data.DamageType, data.SVModifier);
                WeaponDamage = profile?.ToJson();
            }
            else
            {
                WeaponDamage = null;
            }
            CustomProperties = data.CustomProperties;
            IsContainer = data.IsContainer;
            ContainerMaxWeight = data.ContainerMaxWeight;
            ContainerMaxVolume = data.ContainerMaxVolume;
            ContainerAllowedTypes = data.ContainerAllowedTypes;
            ContainerWeightReduction = data.ContainerWeightReduction;

            // Load effects child collection
            if (effectsPortal != null)
            {
                Effects = effectsPortal.FetchChild(data.Effects ?? []);
            }
        }
        BusinessRules.CheckRules();
    }

    [Insert]
    private async Task Insert([Inject] IItemTemplateDal dal)
    {
        // If EquipmentSlots is empty but the legacy EquipmentSlot is set, derive the list from it
        var effectiveSlots = EquipmentSlots.Count > 0
            ? EquipmentSlots
            : (EquipmentSlot != EquipmentSlot.None ? new List<EquipmentSlot> { EquipmentSlot } : new List<EquipmentSlot>());

        var dto = new ItemTemplate
        {
            Name = Name,
            Description = Description,
            ShortDescription = ShortDescription,
            ItemType = ItemType,
            WeaponType = WeaponType,
            EquipmentSlots = new List<EquipmentSlot>(effectiveSlots),
            OccupiesAllSlots = OccupiesAllSlots,
            EquipmentSlot = effectiveSlots.Count > 0 ? effectiveSlots[0] : EquipmentSlot.None,
            Weight = Weight,
            Volume = Volume,
            Value = Value,
            IsStackable = IsStackable,
            MaxStackSize = MaxStackSize,
            HasDurability = HasDurability,
            MaxDurability = MaxDurability,
            Rarity = Rarity,
            IsActive = IsActive,
            IsVirtual = IsVirtual,
            Tags = Tags,
            RelatedSkill = RelatedSkill,
            MinSkillLevel = MinSkillLevel,
            DamageClass = DamageClass,
            DamageType = DamageType,
            SVModifier = SVModifier,
            AVModifier = AVModifier,
            DodgeModifier = DodgeModifier,
            Range = Range,
            ArmorAbsorption = ArmorAbsorption,
            WeaponDamage = WeaponDamage,
            CustomProperties = CustomProperties,
            IsContainer = IsContainer,
            ContainerMaxWeight = ContainerMaxWeight,
            ContainerMaxVolume = ContainerMaxVolume,
            ContainerAllowedTypes = ContainerAllowedTypes,
            ContainerWeightReduction = ContainerWeightReduction,
            Effects = Effects?.ToDtoList() ?? []
        };

        var result = await dal.SaveTemplateAsync(dto);
        using (BypassPropertyChecks)
        {
            Id = result.Id;
        }
    }

    [Update]
    private async Task Update([Inject] IItemTemplateDal dal)
    {
        // If EquipmentSlots is empty but the legacy EquipmentSlot is set, derive the list from it
        var effectiveSlots = EquipmentSlots.Count > 0
            ? EquipmentSlots
            : (EquipmentSlot != EquipmentSlot.None ? new List<EquipmentSlot> { EquipmentSlot } : new List<EquipmentSlot>());

        var dto = new ItemTemplate
        {
            Id = Id,
            Name = Name,
            Description = Description,
            ShortDescription = ShortDescription,
            ItemType = ItemType,
            WeaponType = WeaponType,
            EquipmentSlots = new List<EquipmentSlot>(effectiveSlots),
            OccupiesAllSlots = OccupiesAllSlots,
            EquipmentSlot = effectiveSlots.Count > 0 ? effectiveSlots[0] : EquipmentSlot.None,
            Weight = Weight,
            Volume = Volume,
            Value = Value,
            IsStackable = IsStackable,
            MaxStackSize = MaxStackSize,
            HasDurability = HasDurability,
            MaxDurability = MaxDurability,
            Rarity = Rarity,
            IsActive = IsActive,
            IsVirtual = IsVirtual,
            Tags = Tags,
            RelatedSkill = RelatedSkill,
            MinSkillLevel = MinSkillLevel,
            DamageClass = DamageClass,
            DamageType = DamageType,
            SVModifier = SVModifier,
            AVModifier = AVModifier,
            DodgeModifier = DodgeModifier,
            Range = Range,
            ArmorAbsorption = ArmorAbsorption,
            WeaponDamage = WeaponDamage,
            CustomProperties = CustomProperties,
            IsContainer = IsContainer,
            ContainerMaxWeight = ContainerMaxWeight,
            ContainerMaxVolume = ContainerMaxVolume,
            ContainerAllowedTypes = ContainerAllowedTypes,
            ContainerWeightReduction = ContainerWeightReduction,
            Effects = Effects?.ToDtoList() ?? []
        };

        await dal.SaveTemplateAsync(dto);
    }

    [Delete]
    private async Task Delete(int id, [Inject] IItemTemplateDal dal)
    {
        await dal.DeactivateTemplateAsync(id);
    }
}

/// <summary>
/// Validates that ItemType is not the default value (must be explicitly set).
/// </summary>
public class ItemTypeRequiredRule : BusinessRule
{
    public ItemTypeRequiredRule(IPropertyInfo primaryProperty)
        : base(primaryProperty)
    {
        InputProperties.Add(primaryProperty);
    }

    protected override void Execute(IRuleContext context)
    {
        var value = (ItemType)context.InputPropertyValues[PrimaryProperty]!;
        if (value == default)
        {
            context.AddErrorResult("ItemType is required.");
        }
    }
}

/// <summary>
/// Warning rule that flags containers without capacity defined.
/// Per CONTEXT.md: Warning, not error - allows GM flexibility.
/// </summary>
public class ContainerCapacityWarningRule : BusinessRule
{
    private readonly IPropertyInfo _containerMaxWeightProperty;
    private readonly IPropertyInfo _containerMaxVolumeProperty;

    public ContainerCapacityWarningRule(
        IPropertyInfo isContainerProperty,
        IPropertyInfo containerMaxWeightProperty,
        IPropertyInfo containerMaxVolumeProperty)
        : base(isContainerProperty)
    {
        _containerMaxWeightProperty = containerMaxWeightProperty;
        _containerMaxVolumeProperty = containerMaxVolumeProperty;
        InputProperties.Add(isContainerProperty);
        InputProperties.Add(containerMaxWeightProperty);
        InputProperties.Add(containerMaxVolumeProperty);
    }

    protected override void Execute(IRuleContext context)
    {
        var isContainer = (bool)context.InputPropertyValues[PrimaryProperty]!;
        if (!isContainer)
            return;

        var maxWeight = context.InputPropertyValues[_containerMaxWeightProperty] as decimal?;
        var maxVolume = context.InputPropertyValues[_containerMaxVolumeProperty] as decimal?;

        if ((maxWeight == null || maxWeight <= 0) && (maxVolume == null || maxVolume <= 0))
        {
            context.AddWarningResult("Container has no capacity defined. Consider setting ContainerMaxWeight or ContainerMaxVolume.");
        }
    }
}

/// <summary>
/// Validation rule that prevents ammo containers from being stackable.
/// Ammo containers track their current ammunition count and cannot be stacked.
/// </summary>
public class AmmoContainerNotStackableRule : BusinessRule
{
    private readonly IPropertyInfo _itemTypeProperty;

    public AmmoContainerNotStackableRule(
        IPropertyInfo itemTypeProperty,
        IPropertyInfo isStackableProperty)
        : base(isStackableProperty)  // Primary property is IsStackable so error appears there
    {
        _itemTypeProperty = itemTypeProperty;
        InputProperties.Add(itemTypeProperty);
        InputProperties.Add(isStackableProperty);
        // Re-check when either property changes
        AffectedProperties.Add(isStackableProperty);
    }

    protected override void Execute(IRuleContext context)
    {
        var itemType = (ItemType)context.InputPropertyValues[_itemTypeProperty]!;
        var isStackable = (bool)context.InputPropertyValues[PrimaryProperty]!;

        if (itemType == ItemType.AmmoContainer && isStackable)
        {
            context.AddErrorResult("Ammo containers cannot be stackable because they track their current ammunition count.");
        }
    }
}
