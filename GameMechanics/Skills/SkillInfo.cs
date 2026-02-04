using System;
using Csla;
using Threa.Dal.Dto;

namespace GameMechanics.Skills;

[Serializable]
public class SkillInfo : ReadOnlyBase<SkillInfo>
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

    public static readonly PropertyInfo<string?> DescriptionProperty = RegisterProperty<string?>(nameof(Description));
    public string? Description
    {
        get => GetProperty(DescriptionProperty);
        private set => LoadProperty(DescriptionProperty, value);
    }

    public static readonly PropertyInfo<SkillCategory> CategoryProperty = RegisterProperty<SkillCategory>(nameof(Category));
    public SkillCategory Category
    {
        get => GetProperty(CategoryProperty);
        private set => LoadProperty(CategoryProperty, value);
    }

    public static readonly PropertyInfo<bool> IsSpecializedProperty = RegisterProperty<bool>(nameof(IsSpecialized));
    public bool IsSpecialized
    {
        get => GetProperty(IsSpecializedProperty);
        private set => LoadProperty(IsSpecializedProperty, value);
    }

    public static readonly PropertyInfo<bool> IsMagicProperty = RegisterProperty<bool>(nameof(IsMagic));
    public bool IsMagic
    {
        get => GetProperty(IsMagicProperty);
        private set => LoadProperty(IsMagicProperty, value);
    }

    public static readonly PropertyInfo<string> PrimaryAttributeProperty = RegisterProperty<string>(nameof(PrimaryAttribute));
    public string PrimaryAttribute
    {
        get => GetProperty(PrimaryAttributeProperty);
        private set => LoadProperty(PrimaryAttributeProperty, value);
    }

    public static readonly PropertyInfo<int> UntrainedProperty = RegisterProperty<int>(nameof(Untrained));
    public int Untrained
    {
        get => GetProperty(UntrainedProperty);
        private set => LoadProperty(UntrainedProperty, value);
    }

    public static readonly PropertyInfo<int> TrainedProperty = RegisterProperty<int>(nameof(Trained));
    public int Trained
    {
        get => GetProperty(TrainedProperty);
        private set => LoadProperty(TrainedProperty, value);
    }

    public static readonly PropertyInfo<bool> RequiresPreUseConcentrationProperty = RegisterProperty<bool>(nameof(RequiresPreUseConcentration));
    public bool RequiresPreUseConcentration
    {
        get => GetProperty(RequiresPreUseConcentrationProperty);
        private set => LoadProperty(RequiresPreUseConcentrationProperty, value);
    }

    public static readonly PropertyInfo<int> PreUseConcentrationRoundsProperty = RegisterProperty<int>(nameof(PreUseConcentrationRounds));
    public int PreUseConcentrationRounds
    {
        get => GetProperty(PreUseConcentrationRoundsProperty);
        private set => LoadProperty(PreUseConcentrationRoundsProperty, value);
    }

    public static readonly PropertyInfo<bool> RequiresPostUseConcentrationProperty = RegisterProperty<bool>(nameof(RequiresPostUseConcentration));
    public bool RequiresPostUseConcentration
    {
        get => GetProperty(RequiresPostUseConcentrationProperty);
        private set => LoadProperty(RequiresPostUseConcentrationProperty, value);
    }

    public static readonly PropertyInfo<int> PostUseConcentrationRoundsProperty = RegisterProperty<int>(nameof(PostUseConcentrationRounds));
    public int PostUseConcentrationRounds
    {
        get => GetProperty(PostUseConcentrationRoundsProperty);
        private set => LoadProperty(PostUseConcentrationRoundsProperty, value);
    }

    public static readonly PropertyInfo<int> PostUseInterruptionPenaltyRoundsProperty = RegisterProperty<int>(nameof(PostUseInterruptionPenaltyRounds));
    public int PostUseInterruptionPenaltyRounds
    {
        get => GetProperty(PostUseInterruptionPenaltyRoundsProperty);
        private set => LoadProperty(PostUseInterruptionPenaltyRoundsProperty, value);
    }

    /// <summary>
    /// Whether this skill can be deleted.
    /// Standard attribute skills cannot be deleted.
    /// </summary>
    public bool CanBeDeleted => Category != SkillCategory.Standard;

    [FetchChild]
    private void Fetch(Skill dto)
    {
        LoadProperty(IdProperty, dto.Id);
        LoadProperty(NameProperty, dto.Name);
        LoadProperty(DescriptionProperty, dto.Description);
        LoadProperty(CategoryProperty, dto.Category);
        LoadProperty(IsSpecializedProperty, dto.IsSpecialized);
        LoadProperty(IsMagicProperty, dto.IsMagic);
        LoadProperty(PrimaryAttributeProperty, dto.PrimaryAttribute);
        LoadProperty(UntrainedProperty, dto.Untrained);
        LoadProperty(TrainedProperty, dto.Trained);
        LoadProperty(RequiresPreUseConcentrationProperty, dto.RequiresPreUseConcentration);
        LoadProperty(PreUseConcentrationRoundsProperty, dto.PreUseConcentrationRounds);
        LoadProperty(RequiresPostUseConcentrationProperty, dto.RequiresPostUseConcentration);
        LoadProperty(PostUseConcentrationRoundsProperty, dto.PostUseConcentrationRounds);
        LoadProperty(PostUseInterruptionPenaltyRoundsProperty, dto.PostUseInterruptionPenaltyRounds);
    }
}
