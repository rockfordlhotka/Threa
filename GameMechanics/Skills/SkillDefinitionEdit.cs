using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Skills;

[Serializable]
public class SkillDefinitionEdit : BusinessBase<SkillDefinitionEdit>
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

    public static readonly PropertyInfo<string?> DescriptionProperty = RegisterProperty<string?>(nameof(Description));
    public string? Description
    {
        get => GetProperty(DescriptionProperty);
        set => SetProperty(DescriptionProperty, value);
    }

    public static readonly PropertyInfo<SkillCategory> CategoryProperty = RegisterProperty<SkillCategory>(nameof(Category));
    public SkillCategory Category
    {
        get => GetProperty(CategoryProperty);
        set => SetProperty(CategoryProperty, value);
    }

    public static readonly PropertyInfo<bool> IsSpecializedProperty = RegisterProperty<bool>(nameof(IsSpecialized));
    public bool IsSpecialized
    {
        get => GetProperty(IsSpecializedProperty);
        set => SetProperty(IsSpecializedProperty, value);
    }

    public static readonly PropertyInfo<bool> IsMagicProperty = RegisterProperty<bool>(nameof(IsMagic));
    public bool IsMagic
    {
        get => GetProperty(IsMagicProperty);
        set => SetProperty(IsMagicProperty, value);
    }

    public static readonly PropertyInfo<bool> IsTheologyProperty = RegisterProperty<bool>(nameof(IsTheology));
    public bool IsTheology
    {
        get => GetProperty(IsTheologyProperty);
        set => SetProperty(IsTheologyProperty, value);
    }

    public static readonly PropertyInfo<bool> IsPsionicProperty = RegisterProperty<bool>(nameof(IsPsionic));
    public bool IsPsionic
    {
        get => GetProperty(IsPsionicProperty);
        set => SetProperty(IsPsionicProperty, value);
    }

    public static readonly PropertyInfo<int> UntrainedProperty = RegisterProperty<int>(nameof(Untrained));
    public int Untrained
    {
        get => GetProperty(UntrainedProperty);
        set => SetProperty(UntrainedProperty, value);
    }

    public static readonly PropertyInfo<int> TrainedProperty = RegisterProperty<int>(nameof(Trained));
    public int Trained
    {
        get => GetProperty(TrainedProperty);
        set => SetProperty(TrainedProperty, value);
    }

    public static readonly PropertyInfo<string> PrimaryAttributeProperty = RegisterProperty<string>(nameof(PrimaryAttribute));
    public string PrimaryAttribute
    {
        get => GetProperty(PrimaryAttributeProperty);
        set => SetProperty(PrimaryAttributeProperty, value);
    }

    public static readonly PropertyInfo<string?> SecondaryAttributeProperty = RegisterProperty<string?>(nameof(SecondaryAttribute));
    public string? SecondaryAttribute
    {
        get => GetProperty(SecondaryAttributeProperty);
        set => SetProperty(SecondaryAttributeProperty, value);
    }

    public static readonly PropertyInfo<string?> TertiaryAttributeProperty = RegisterProperty<string?>(nameof(TertiaryAttribute));
    public string? TertiaryAttribute
    {
        get => GetProperty(TertiaryAttributeProperty);
        set => SetProperty(TertiaryAttributeProperty, value);
    }

    public static readonly PropertyInfo<ActionType> ActionTypeProperty = RegisterProperty<ActionType>(nameof(ActionType));
    public ActionType ActionType
    {
        get => GetProperty(ActionTypeProperty);
        set => SetProperty(ActionTypeProperty, value);
    }

    public static readonly PropertyInfo<TargetValueType> TargetValueTypeProperty = RegisterProperty<TargetValueType>(nameof(TargetValueType));
    public TargetValueType TargetValueType
    {
        get => GetProperty(TargetValueTypeProperty);
        set => SetProperty(TargetValueTypeProperty, value);
    }

    public static readonly PropertyInfo<int> DefaultTVProperty = RegisterProperty<int>(nameof(DefaultTV));
    public int DefaultTV
    {
        get => GetProperty(DefaultTVProperty);
        set => SetProperty(DefaultTVProperty, value);
    }

    public static readonly PropertyInfo<string?> OpposedSkillIdProperty = RegisterProperty<string?>(nameof(OpposedSkillId));
    public string? OpposedSkillId
    {
        get => GetProperty(OpposedSkillIdProperty);
        set => SetProperty(OpposedSkillIdProperty, value);
    }

    public static readonly PropertyInfo<ResultTableType> ResultTableProperty = RegisterProperty<ResultTableType>(nameof(ResultTable));
    public ResultTableType ResultTable
    {
        get => GetProperty(ResultTableProperty);
        set => SetProperty(ResultTableProperty, value);
    }

    public static readonly PropertyInfo<bool> AppliesPhysicalityBonusProperty = RegisterProperty<bool>(nameof(AppliesPhysicalityBonus));
    public bool AppliesPhysicalityBonus
    {
        get => GetProperty(AppliesPhysicalityBonusProperty);
        set => SetProperty(AppliesPhysicalityBonusProperty, value);
    }

    public static readonly PropertyInfo<bool> RequiresTargetProperty = RegisterProperty<bool>(nameof(RequiresTarget));
    public bool RequiresTarget
    {
        get => GetProperty(RequiresTargetProperty);
        set => SetProperty(RequiresTargetProperty, value);
    }

    public static readonly PropertyInfo<bool> RequiresLineOfSightProperty = RegisterProperty<bool>(nameof(RequiresLineOfSight));
    public bool RequiresLineOfSight
    {
        get => GetProperty(RequiresLineOfSightProperty);
        set => SetProperty(RequiresLineOfSightProperty, value);
    }

    public static readonly PropertyInfo<bool> CanPumpWithFatigueProperty = RegisterProperty<bool>(nameof(CanPumpWithFatigue));
    public bool CanPumpWithFatigue
    {
        get => GetProperty(CanPumpWithFatigueProperty);
        set => SetProperty(CanPumpWithFatigueProperty, value);
    }

    public static readonly PropertyInfo<bool> CanPumpWithManaProperty = RegisterProperty<bool>(nameof(CanPumpWithMana));
    public bool CanPumpWithMana
    {
        get => GetProperty(CanPumpWithManaProperty);
        set => SetProperty(CanPumpWithManaProperty, value);
    }

    public static readonly PropertyInfo<string?> PumpDescriptionProperty = RegisterProperty<string?>(nameof(PumpDescription));
    public string? PumpDescription
    {
        get => GetProperty(PumpDescriptionProperty);
        set => SetProperty(PumpDescriptionProperty, value);
    }

    public static readonly PropertyInfo<bool> IsFreeActionProperty = RegisterProperty<bool>(nameof(IsFreeAction));
    public bool IsFreeAction
    {
        get => GetProperty(IsFreeActionProperty);
        set => SetProperty(IsFreeActionProperty, value);
    }

    public static readonly PropertyInfo<bool> IsPassiveProperty = RegisterProperty<bool>(nameof(IsPassive));
    public bool IsPassive
    {
        get => GetProperty(IsPassiveProperty);
        set => SetProperty(IsPassiveProperty, value);
    }

    public static readonly PropertyInfo<string?> ActionDescriptionProperty = RegisterProperty<string?>(nameof(ActionDescription));
    public string? ActionDescription
    {
        get => GetProperty(ActionDescriptionProperty);
        set => SetProperty(ActionDescriptionProperty, value);
    }

    // Helper properties for UI
    public bool IsSpell => IsMagic || IsTheology || IsPsionic;
    public bool IsManaSkill => Category == SkillCategory.Mana;
    public bool IsActiveSpell => Category == SkillCategory.Spell || Category == SkillCategory.Theology || Category == SkillCategory.Psionic;
    public bool IsCombatSkill => Category == SkillCategory.Combat;
    public bool CanBeDeleted => Category != SkillCategory.Standard;

    protected override void AddBusinessRules()
    {
        base.AddBusinessRules();

        // Primary attribute is required and must be valid attribute code(s)
        BusinessRules.AddRule(new AttributeValidationRule(PrimaryAttributeProperty, required: true));

        // Secondary and tertiary attributes are optional but must be valid if provided
        BusinessRules.AddRule(new AttributeValidationRule(SecondaryAttributeProperty, required: false));
        BusinessRules.AddRule(new AttributeValidationRule(TertiaryAttributeProperty, required: false));
    }

    [Create]
    private async Task Create()
    {
        using (BypassPropertyChecks)
        {
            Id = string.Empty;
            Name = string.Empty;
            Description = null;
            Category = SkillCategory.Other;
            IsSpecialized = false;
            IsMagic = false;
            IsTheology = false;
            IsPsionic = false;
            Untrained = 5;
            Trained = 3;
            PrimaryAttribute = string.Empty;
            SecondaryAttribute = null;
            TertiaryAttribute = null;
            ActionType = ActionType.None;
            TargetValueType = TargetValueType.Fixed;
            DefaultTV = 6;
            OpposedSkillId = null;
            ResultTable = ResultTableType.General;
            AppliesPhysicalityBonus = false;
            RequiresTarget = false;
            RequiresLineOfSight = false;
            CanPumpWithFatigue = false;
            CanPumpWithMana = false;
            PumpDescription = null;
            IsFreeAction = false;
            IsPassive = false;
            ActionDescription = null;
        }
        BusinessRules.CheckRules();
        await Task.CompletedTask;
    }

    [Fetch]
    private async Task Fetch(string id, [Inject] ISkillDal dal)
    {
        var data = await dal.GetSkillAsync(id)
            ?? throw new InvalidOperationException($"Skill {id} not found");
        LoadFromDto(data);
    }

    private void LoadFromDto(Skill data)
    {
        using (BypassPropertyChecks)
        {
            Id = data.Id;
            Name = data.Name;
            Description = data.Description;
            Category = data.Category;
            IsSpecialized = data.IsSpecialized;
            IsMagic = data.IsMagic;
            IsTheology = data.IsTheology;
            IsPsionic = data.IsPsionic;
            Untrained = data.Untrained;
            Trained = data.Trained;
            PrimaryAttribute = data.PrimaryAttribute;
            SecondaryAttribute = data.SecondaryAttribute;
            TertiaryAttribute = data.TertiaryAttribute;
            ActionType = data.ActionType;
            TargetValueType = data.TargetValueType;
            DefaultTV = data.DefaultTV;
            OpposedSkillId = data.OpposedSkillId;
            ResultTable = data.ResultTable;
            AppliesPhysicalityBonus = data.AppliesPhysicalityBonus;
            RequiresTarget = data.RequiresTarget;
            RequiresLineOfSight = data.RequiresLineOfSight;
            CanPumpWithFatigue = data.CanPumpWithFatigue;
            CanPumpWithMana = data.CanPumpWithMana;
            PumpDescription = data.PumpDescription;
            IsFreeAction = data.IsFreeAction;
            IsPassive = data.IsPassive;
            ActionDescription = data.ActionDescription;
        }
        BusinessRules.CheckRules();
    }

    [Insert]
    [Update]
    private async Task Save([Inject] ISkillDal dal)
    {
        var dto = new Skill
        {
            Id = Id,
            Name = Name,
            Description = Description,
            Category = Category,
            IsSpecialized = IsSpecialized,
            IsMagic = IsMagic,
            IsTheology = IsTheology,
            IsPsionic = IsPsionic,
            Untrained = Untrained,
            Trained = Trained,
            PrimaryAttribute = PrimaryAttribute,
            SecondaryAttribute = SecondaryAttribute,
            TertiaryAttribute = TertiaryAttribute,
            ActionType = ActionType,
            TargetValueType = TargetValueType,
            DefaultTV = DefaultTV,
            OpposedSkillId = OpposedSkillId,
            ResultTable = ResultTable,
            AppliesPhysicalityBonus = AppliesPhysicalityBonus,
            RequiresTarget = RequiresTarget,
            RequiresLineOfSight = RequiresLineOfSight,
            CanPumpWithFatigue = CanPumpWithFatigue,
            CanPumpWithMana = CanPumpWithMana,
            PumpDescription = PumpDescription,
            IsFreeAction = IsFreeAction,
            IsPassive = IsPassive,
            ActionDescription = ActionDescription
        };

        await dal.SaveSkillAsync(dto);
    }

    [Delete]
    private async Task Delete(string id, [Inject] ISkillDal dal)
    {
        // Prevent deletion of core attribute skills
        var skill = await dal.GetSkillAsync(id);
        if (skill != null && skill.Category == SkillCategory.Standard)
        {
            throw new InvalidOperationException("Core attribute skills cannot be deleted.");
        }
        
        await dal.DeleteSkillAsync(id);
    }
}
