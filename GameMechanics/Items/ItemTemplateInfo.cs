using System;
using Csla;
using Threa.Dal.Dto;

namespace GameMechanics.Items;

[Serializable]
public class ItemTemplateInfo : ReadOnlyBase<ItemTemplateInfo>
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
        private set => LoadProperty(NameProperty, value);
    }

    public static readonly PropertyInfo<string> DescriptionProperty = RegisterProperty<string>(nameof(Description));
    public string Description
    {
        get => GetProperty(DescriptionProperty);
        private set => LoadProperty(DescriptionProperty, value);
    }

    public static readonly PropertyInfo<string> ShortDescriptionProperty = RegisterProperty<string>(nameof(ShortDescription));
    public string ShortDescription
    {
        get => GetProperty(ShortDescriptionProperty);
        private set => LoadProperty(ShortDescriptionProperty, value);
    }

    public static readonly PropertyInfo<ItemType> ItemTypeProperty = RegisterProperty<ItemType>(nameof(ItemType));
    public ItemType ItemType
    {
        get => GetProperty(ItemTypeProperty);
        private set => LoadProperty(ItemTypeProperty, value);
    }

    public static readonly PropertyInfo<decimal> WeightProperty = RegisterProperty<decimal>(nameof(Weight));
    public decimal Weight
    {
        get => GetProperty(WeightProperty);
        private set => LoadProperty(WeightProperty, value);
    }

    public static readonly PropertyInfo<int> ValueProperty = RegisterProperty<int>(nameof(Value));
    public int Value
    {
        get => GetProperty(ValueProperty);
        private set => LoadProperty(ValueProperty, value);
    }

    public static readonly PropertyInfo<ItemRarity> RarityProperty = RegisterProperty<ItemRarity>(nameof(Rarity));
    public ItemRarity Rarity
    {
        get => GetProperty(RarityProperty);
        private set => LoadProperty(RarityProperty, value);
    }

    public static readonly PropertyInfo<bool> IsActiveProperty = RegisterProperty<bool>(nameof(IsActive));
    public bool IsActive
    {
        get => GetProperty(IsActiveProperty);
        private set => LoadProperty(IsActiveProperty, value);
    }

    [FetchChild]
    private void Fetch(ItemTemplate dto)
    {
        LoadProperty(IdProperty, dto.Id);
        LoadProperty(NameProperty, dto.Name);
        LoadProperty(DescriptionProperty, dto.Description);
        LoadProperty(ShortDescriptionProperty, dto.ShortDescription);
        LoadProperty(ItemTypeProperty, dto.ItemType);
        LoadProperty(WeightProperty, dto.Weight);
        LoadProperty(ValueProperty, dto.Value);
        LoadProperty(RarityProperty, dto.Rarity);
        LoadProperty(IsActiveProperty, dto.IsActive);
    }

    public void LoadFromDto(ItemTemplate dto)
    {
        LoadProperty(IdProperty, dto.Id);
        LoadProperty(NameProperty, dto.Name);
        LoadProperty(DescriptionProperty, dto.Description);
        LoadProperty(ShortDescriptionProperty, dto.ShortDescription);
        LoadProperty(ItemTypeProperty, dto.ItemType);
        LoadProperty(WeightProperty, dto.Weight);
        LoadProperty(ValueProperty, dto.Value);
        LoadProperty(RarityProperty, dto.Rarity);
        LoadProperty(IsActiveProperty, dto.IsActive);
    }
}
