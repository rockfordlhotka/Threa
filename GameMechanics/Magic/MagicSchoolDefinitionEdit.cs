using System;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Magic;

[Serializable]
public class MagicSchoolDefinitionEdit : BusinessBase<MagicSchoolDefinitionEdit>
{
    public static readonly PropertyInfo<string> IdProperty = RegisterProperty<string>(nameof(Id));
    public string Id
    {
        get => GetProperty(IdProperty);
        set => SetProperty(IdProperty, value);
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

    public static readonly PropertyInfo<string> ColorCodeProperty = RegisterProperty<string>(nameof(ColorCode));
    public string ColorCode
    {
        get => GetProperty(ColorCodeProperty);
        set => SetProperty(ColorCodeProperty, value);
    }

    public static readonly PropertyInfo<string?> IconUrlProperty = RegisterProperty<string?>(nameof(IconUrl));
    public string? IconUrl
    {
        get => GetProperty(IconUrlProperty);
        set => SetProperty(IconUrlProperty, value);
    }

    public static readonly PropertyInfo<bool> IsActiveProperty = RegisterProperty<bool>(nameof(IsActive));
    public bool IsActive
    {
        get => GetProperty(IsActiveProperty);
        set => SetProperty(IsActiveProperty, value);
    }

    public static readonly PropertyInfo<bool> IsCoreProperty = RegisterProperty<bool>(nameof(IsCore));
    public bool IsCore
    {
        get => GetProperty(IsCoreProperty);
        set => SetProperty(IsCoreProperty, value);
    }

    public static readonly PropertyInfo<string> ManaSkillIdProperty = RegisterProperty<string>(nameof(ManaSkillId));
    public string ManaSkillId
    {
        get => GetProperty(ManaSkillIdProperty);
        set => SetProperty(ManaSkillIdProperty, value);
    }

    public static readonly PropertyInfo<int> DisplayOrderProperty = RegisterProperty<int>(nameof(DisplayOrder));
    public int DisplayOrder
    {
        get => GetProperty(DisplayOrderProperty);
        set => SetProperty(DisplayOrderProperty, value);
    }

    public static readonly PropertyInfo<string?> TypicalSpellTypesProperty = RegisterProperty<string?>(nameof(TypicalSpellTypes));
    public string? TypicalSpellTypes
    {
        get => GetProperty(TypicalSpellTypesProperty);
        set => SetProperty(TypicalSpellTypesProperty, value);
    }

    public bool CanBeDeleted => !IsCore;

    [Create]
    private async Task Create()
    {
        using (BypassPropertyChecks)
        {
            Id = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            ShortDescription = string.Empty;
            ColorCode = "#FFFFFF";
            IconUrl = null;
            IsActive = true;
            IsCore = false;
            ManaSkillId = string.Empty;
            DisplayOrder = 100;
            TypicalSpellTypes = null;
        }
        BusinessRules.CheckRules();
        await Task.CompletedTask;
    }

    [Fetch]
    private async Task Fetch(string id, [Inject] IMagicSchoolDal dal)
    {
        var data = await dal.GetSchoolAsync(id)
            ?? throw new InvalidOperationException($"Magic school {id} not found");
        LoadFromDto(data);
    }

    private void LoadFromDto(MagicSchoolDefinition data)
    {
        using (BypassPropertyChecks)
        {
            Id = data.Id;
            Name = data.Name;
            Description = data.Description;
            ShortDescription = data.ShortDescription;
            ColorCode = data.ColorCode;
            IconUrl = data.IconUrl;
            IsActive = data.IsActive;
            IsCore = data.IsCore;
            ManaSkillId = data.ManaSkillId;
            DisplayOrder = data.DisplayOrder;
            TypicalSpellTypes = data.TypicalSpellTypes;
        }
        BusinessRules.CheckRules();
    }

    [Insert]
    [Update]
    private async Task Save([Inject] IMagicSchoolDal dal)
    {
        var dto = new MagicSchoolDefinition
        {
            Id = Id,
            Name = Name,
            Description = Description,
            ShortDescription = ShortDescription,
            ColorCode = ColorCode,
            IconUrl = IconUrl,
            IsActive = IsActive,
            IsCore = IsCore,
            ManaSkillId = ManaSkillId,
            DisplayOrder = DisplayOrder,
            TypicalSpellTypes = TypicalSpellTypes
        };

        await dal.SaveSchoolAsync(dto);
    }

    [Delete]
    private async Task Delete(string id, [Inject] IMagicSchoolDal dal)
    {
        var school = await dal.GetSchoolAsync(id);
        if (school != null && school.IsCore)
        {
            throw new InvalidOperationException("Core magic schools cannot be deleted.");
        }

        await dal.DeleteSchoolAsync(id);
    }
}
