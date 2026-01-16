using System;
using System.Threading.Tasks;
using Csla;
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

    [Create]
    private async Task Create()
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
            Weight = 0;
            Volume = 0;
            Value = 0;
            IsStackable = false;
            MaxStackSize = 1;
            HasDurability = false;
            MaxDurability = 0;
            Rarity = ItemRarity.Common;
            IsActive = true;
            RelatedSkill = null;
            MinSkillLevel = 0;
            DamageClass = 1;
            DamageType = string.Empty;
            SVModifier = 0;
            AVModifier = 0;
            DodgeModifier = 0;
            Range = null;
            ArmorAbsorption = null;
            CustomProperties = null;
            IsContainer = false;
            ContainerMaxWeight = null;
            ContainerMaxVolume = null;
            ContainerAllowedTypes = null;
            ContainerWeightReduction = 1.0m;
        }
        BusinessRules.CheckRules();
        await Task.CompletedTask;
    }

    [Fetch]
    private async Task Fetch(int id, [Inject] IItemTemplateDal dal)
    {
        var data = await dal.GetTemplateAsync(id)
            ?? throw new InvalidOperationException($"ItemTemplate {id} not found");
        LoadFromDto(data);
    }

    private void LoadFromDto(ItemTemplate data)
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
            Weight = data.Weight;
            Volume = data.Volume;
            Value = data.Value;
            IsStackable = data.IsStackable;
            MaxStackSize = data.MaxStackSize;
            HasDurability = data.HasDurability;
            MaxDurability = data.MaxDurability;
            Rarity = data.Rarity;
            IsActive = data.IsActive;
            RelatedSkill = data.RelatedSkill;
            MinSkillLevel = data.MinSkillLevel;
            DamageClass = data.DamageClass;
            DamageType = data.DamageType ?? string.Empty;
            SVModifier = data.SVModifier;
            AVModifier = data.AVModifier;
            DodgeModifier = data.DodgeModifier;
            Range = data.Range;
            ArmorAbsorption = data.ArmorAbsorption;
            CustomProperties = data.CustomProperties;
            IsContainer = data.IsContainer;
            ContainerMaxWeight = data.ContainerMaxWeight;
            ContainerMaxVolume = data.ContainerMaxVolume;
            ContainerAllowedTypes = data.ContainerAllowedTypes;
            ContainerWeightReduction = data.ContainerWeightReduction;
        }
        BusinessRules.CheckRules();
    }

    [Insert]
    private async Task Insert([Inject] IItemTemplateDal dal)
    {
        var dto = new ItemTemplate
        {
            Name = Name,
            Description = Description,
            ShortDescription = ShortDescription,
            ItemType = ItemType,
            WeaponType = WeaponType,
            EquipmentSlot = EquipmentSlot,
            Weight = Weight,
            Volume = Volume,
            Value = Value,
            IsStackable = IsStackable,
            MaxStackSize = MaxStackSize,
            HasDurability = HasDurability,
            MaxDurability = MaxDurability,
            Rarity = Rarity,
            IsActive = IsActive,
            RelatedSkill = RelatedSkill,
            MinSkillLevel = MinSkillLevel,
            DamageClass = DamageClass,
            DamageType = DamageType,
            SVModifier = SVModifier,
            AVModifier = AVModifier,
            DodgeModifier = DodgeModifier,
            Range = Range,
            ArmorAbsorption = ArmorAbsorption,
            CustomProperties = CustomProperties,
            IsContainer = IsContainer,
            ContainerMaxWeight = ContainerMaxWeight,
            ContainerMaxVolume = ContainerMaxVolume,
            ContainerAllowedTypes = ContainerAllowedTypes,
            ContainerWeightReduction = ContainerWeightReduction
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
        var dto = new ItemTemplate
        {
            Id = Id,
            Name = Name,
            Description = Description,
            ShortDescription = ShortDescription,
            ItemType = ItemType,
            WeaponType = WeaponType,
            EquipmentSlot = EquipmentSlot,
            Weight = Weight,
            Volume = Volume,
            Value = Value,
            IsStackable = IsStackable,
            MaxStackSize = MaxStackSize,
            HasDurability = HasDurability,
            MaxDurability = MaxDurability,
            Rarity = Rarity,
            IsActive = IsActive,
            RelatedSkill = RelatedSkill,
            MinSkillLevel = MinSkillLevel,
            DamageClass = DamageClass,
            DamageType = DamageType,
            SVModifier = SVModifier,
            AVModifier = AVModifier,
            DodgeModifier = DodgeModifier,
            Range = Range,
            ArmorAbsorption = ArmorAbsorption,
            CustomProperties = CustomProperties,
            IsContainer = IsContainer,
            ContainerMaxWeight = ContainerMaxWeight,
            ContainerMaxVolume = ContainerMaxVolume,
            ContainerAllowedTypes = ContainerAllowedTypes,
            ContainerWeightReduction = ContainerWeightReduction
        };

        await dal.SaveTemplateAsync(dto);
    }

    [Delete]
    private async Task Delete(int id, [Inject] IItemTemplateDal dal)
    {
        await dal.DeactivateTemplateAsync(id);
    }
}
