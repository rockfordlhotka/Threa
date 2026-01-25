using System;
using Csla;
using Threa.Dal.Dto;

namespace GameMechanics.Items;

[Serializable]
public class CharacterItemInfo : ReadOnlyBase<CharacterItemInfo>
{
    public static readonly PropertyInfo<Guid> IdProperty = RegisterProperty<Guid>(nameof(Id));
    public Guid Id
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

    public static readonly PropertyInfo<int> OwnerCharacterIdProperty = RegisterProperty<int>(nameof(OwnerCharacterId));
    public int OwnerCharacterId
    {
        get => GetProperty(OwnerCharacterIdProperty);
        private set => LoadProperty(OwnerCharacterIdProperty, value);
    }

    public static readonly PropertyInfo<Guid?> ContainerItemIdProperty = RegisterProperty<Guid?>(nameof(ContainerItemId));
    public Guid? ContainerItemId
    {
        get => GetProperty(ContainerItemIdProperty);
        private set => LoadProperty(ContainerItemIdProperty, value);
    }

    public static readonly PropertyInfo<EquipmentSlot> EquippedSlotProperty = RegisterProperty<EquipmentSlot>(nameof(EquippedSlot));
    public EquipmentSlot EquippedSlot
    {
        get => GetProperty(EquippedSlotProperty);
        private set => LoadProperty(EquippedSlotProperty, value);
    }

    public static readonly PropertyInfo<bool> IsEquippedProperty = RegisterProperty<bool>(nameof(IsEquipped));
    public bool IsEquipped
    {
        get => GetProperty(IsEquippedProperty);
        private set => LoadProperty(IsEquippedProperty, value);
    }

    public static readonly PropertyInfo<int> StackSizeProperty = RegisterProperty<int>(nameof(StackSize));
    public int StackSize
    {
        get => GetProperty(StackSizeProperty);
        private set => LoadProperty(StackSizeProperty, value);
    }

    public static readonly PropertyInfo<int?> CurrentDurabilityProperty = RegisterProperty<int?>(nameof(CurrentDurability));
    public int? CurrentDurability
    {
        get => GetProperty(CurrentDurabilityProperty);
        private set => LoadProperty(CurrentDurabilityProperty, value);
    }

    public static readonly PropertyInfo<string?> CustomNameProperty = RegisterProperty<string?>(nameof(CustomName));
    public string? CustomName
    {
        get => GetProperty(CustomNameProperty);
        private set => LoadProperty(CustomNameProperty, value);
    }

    public static readonly PropertyInfo<DateTime> CreatedAtProperty = RegisterProperty<DateTime>(nameof(CreatedAt));
    public DateTime CreatedAt
    {
        get => GetProperty(CreatedAtProperty);
        private set => LoadProperty(CreatedAtProperty, value);
    }

    public static readonly PropertyInfo<string?> CustomPropertiesProperty = RegisterProperty<string?>(nameof(CustomProperties));
    public string? CustomProperties
    {
        get => GetProperty(CustomPropertiesProperty);
        private set => LoadProperty(CustomPropertiesProperty, value);
    }

    [FetchChild]
    private void Fetch(CharacterItem dto)
    {
        LoadProperty(IdProperty, dto.Id);
        LoadProperty(ItemTemplateIdProperty, dto.ItemTemplateId);
        LoadProperty(OwnerCharacterIdProperty, dto.OwnerCharacterId);
        LoadProperty(ContainerItemIdProperty, dto.ContainerItemId);
        LoadProperty(EquippedSlotProperty, dto.EquippedSlot);
        LoadProperty(IsEquippedProperty, dto.IsEquipped);
        LoadProperty(StackSizeProperty, dto.StackSize);
        LoadProperty(CurrentDurabilityProperty, dto.CurrentDurability);
        LoadProperty(CustomNameProperty, dto.CustomName);
        LoadProperty(CreatedAtProperty, dto.CreatedAt);
        LoadProperty(CustomPropertiesProperty, dto.CustomProperties);
    }
}
