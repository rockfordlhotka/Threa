using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Reference;

[Serializable]
public class SpeciesEdit : BusinessBase<SpeciesEdit>
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

    public static readonly PropertyInfo<string> AttributeModifiersJsonProperty = RegisterProperty<string>(nameof(AttributeModifiersJson));
    public string AttributeModifiersJson
    {
        get => GetProperty(AttributeModifiersJsonProperty);
        set => SetProperty(AttributeModifiersJsonProperty, value);
    }

    // Attribute modifier properties for easy binding
    public static readonly PropertyInfo<int> StrModifierProperty = RegisterProperty<int>(nameof(StrModifier));
    public int StrModifier
    {
        get => GetProperty(StrModifierProperty);
        set => SetProperty(StrModifierProperty, value);
    }

    public static readonly PropertyInfo<int> DexModifierProperty = RegisterProperty<int>(nameof(DexModifier));
    public int DexModifier
    {
        get => GetProperty(DexModifierProperty);
        set => SetProperty(DexModifierProperty, value);
    }

    public static readonly PropertyInfo<int> EndModifierProperty = RegisterProperty<int>(nameof(EndModifier));
    public int EndModifier
    {
        get => GetProperty(EndModifierProperty);
        set => SetProperty(EndModifierProperty, value);
    }

    public static readonly PropertyInfo<int> IntModifierProperty = RegisterProperty<int>(nameof(IntModifier));
    public int IntModifier
    {
        get => GetProperty(IntModifierProperty);
        set => SetProperty(IntModifierProperty, value);
    }

    public static readonly PropertyInfo<int> IttModifierProperty = RegisterProperty<int>(nameof(IttModifier));
    public int IttModifier
    {
        get => GetProperty(IttModifierProperty);
        set => SetProperty(IttModifierProperty, value);
    }

    public static readonly PropertyInfo<int> WilModifierProperty = RegisterProperty<int>(nameof(WilModifier));
    public int WilModifier
    {
        get => GetProperty(WilModifierProperty);
        set => SetProperty(WilModifierProperty, value);
    }

    public static readonly PropertyInfo<int> PhyModifierProperty = RegisterProperty<int>(nameof(PhyModifier));
    public int PhyModifier
    {
        get => GetProperty(PhyModifierProperty);
        set => SetProperty(PhyModifierProperty, value);
    }

    public bool CanBeDeleted => !Id.Equals("Human", StringComparison.OrdinalIgnoreCase);

    [Create]
    private async Task Create()
    {
        using (BypassPropertyChecks)
        {
            Id = string.Empty;
            Name = string.Empty;
            Description = string.Empty;
            AttributeModifiersJson = string.Empty;
            StrModifier = 0;
            DexModifier = 0;
            EndModifier = 0;
            IntModifier = 0;
            IttModifier = 0;
            WilModifier = 0;
            PhyModifier = 0;
        }
        BusinessRules.CheckRules();
        await Task.CompletedTask;
    }

    [Fetch]
    private async Task Fetch(string id, [Inject] ISpeciesDal dal)
    {
        var data = await dal.GetSpeciesAsync(id);
        LoadFromDto(data);
    }

    private void LoadFromDto(Species data)
    {
        using (BypassPropertyChecks)
        {
            Id = data.Id;
            Name = data.Name;
            Description = data.Description;

            // Load attribute modifiers
            StrModifier = data.AttributeModifiers.FirstOrDefault(m => m.AttributeName == "STR")?.Modifier ?? 0;
            DexModifier = data.AttributeModifiers.FirstOrDefault(m => m.AttributeName == "DEX")?.Modifier ?? 0;
            EndModifier = data.AttributeModifiers.FirstOrDefault(m => m.AttributeName == "END")?.Modifier ?? 0;
            IntModifier = data.AttributeModifiers.FirstOrDefault(m => m.AttributeName == "INT")?.Modifier ?? 0;
            IttModifier = data.AttributeModifiers.FirstOrDefault(m => m.AttributeName == "ITT")?.Modifier ?? 0;
            WilModifier = data.AttributeModifiers.FirstOrDefault(m => m.AttributeName == "WIL")?.Modifier ?? 0;
            PhyModifier = data.AttributeModifiers.FirstOrDefault(m => m.AttributeName == "PHY")?.Modifier ?? 0;
        }
        BusinessRules.CheckRules();
    }

    [Insert]
    [Update]
    private async Task Save([Inject] ISpeciesDal dal)
    {
        var modifiers = new List<SpeciesAttributeModifier>();

        if (StrModifier != 0) modifiers.Add(new SpeciesAttributeModifier { AttributeName = "STR", Modifier = StrModifier });
        if (DexModifier != 0) modifiers.Add(new SpeciesAttributeModifier { AttributeName = "DEX", Modifier = DexModifier });
        if (EndModifier != 0) modifiers.Add(new SpeciesAttributeModifier { AttributeName = "END", Modifier = EndModifier });
        if (IntModifier != 0) modifiers.Add(new SpeciesAttributeModifier { AttributeName = "INT", Modifier = IntModifier });
        if (IttModifier != 0) modifiers.Add(new SpeciesAttributeModifier { AttributeName = "ITT", Modifier = IttModifier });
        if (WilModifier != 0) modifiers.Add(new SpeciesAttributeModifier { AttributeName = "WIL", Modifier = WilModifier });
        if (PhyModifier != 0) modifiers.Add(new SpeciesAttributeModifier { AttributeName = "PHY", Modifier = PhyModifier });

        var dto = new Species
        {
            Id = Id,
            Name = Name,
            Description = Description,
            AttributeModifiers = modifiers
        };

        await dal.SaveSpeciesAsync(dto);
    }

    [Delete]
    private async Task Delete(string id, [Inject] ISpeciesDal dal)
    {
        if (id.Equals("Human", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Cannot delete the Human species.");
        }

        await dal.DeleteSpeciesAsync(id);
    }
}
