using System;
using Csla;
using Threa.Dal.Dto;

namespace GameMechanics.Magic;

[Serializable]
public class MagicSchoolInfo : ReadOnlyBase<MagicSchoolInfo>
{
    public static readonly PropertyInfo<string> IdProperty = RegisterProperty<string>(nameof(Id));
    public string Id
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

    public static readonly PropertyInfo<string> ShortDescriptionProperty = RegisterProperty<string>(nameof(ShortDescription));
    public string ShortDescription
    {
        get => GetProperty(ShortDescriptionProperty);
        private set => LoadProperty(ShortDescriptionProperty, value);
    }

    public static readonly PropertyInfo<string> ColorCodeProperty = RegisterProperty<string>(nameof(ColorCode));
    public string ColorCode
    {
        get => GetProperty(ColorCodeProperty);
        private set => LoadProperty(ColorCodeProperty, value);
    }

    public static readonly PropertyInfo<bool> IsActiveProperty = RegisterProperty<bool>(nameof(IsActive));
    public bool IsActive
    {
        get => GetProperty(IsActiveProperty);
        private set => LoadProperty(IsActiveProperty, value);
    }

    public static readonly PropertyInfo<bool> IsCoreProperty = RegisterProperty<bool>(nameof(IsCore));
    public bool IsCore
    {
        get => GetProperty(IsCoreProperty);
        private set => LoadProperty(IsCoreProperty, value);
    }

    public static readonly PropertyInfo<int> DisplayOrderProperty = RegisterProperty<int>(nameof(DisplayOrder));
    public int DisplayOrder
    {
        get => GetProperty(DisplayOrderProperty);
        private set => LoadProperty(DisplayOrderProperty, value);
    }

    public bool CanBeDeleted => !IsCore;

    [FetchChild]
    private void Fetch(MagicSchoolDefinition dto)
    {
        LoadProperty(IdProperty, dto.Id);
        LoadProperty(NameProperty, dto.Name);
        LoadProperty(ShortDescriptionProperty, dto.ShortDescription);
        LoadProperty(ColorCodeProperty, dto.ColorCode);
        LoadProperty(IsActiveProperty, dto.IsActive);
        LoadProperty(IsCoreProperty, dto.IsCore);
        LoadProperty(DisplayOrderProperty, dto.DisplayOrder);
    }
}
