using System;
using System.Linq;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Reference
{
  [Serializable]
  public class SkillList : ReadOnlyListBase<SkillList, SkillInfo>
  {
    [Fetch]
    private async Task Fetch([Inject] ISkillDal skillDal, [Inject] IChildDataPortal<SkillInfo> skillPortal)
    {
      var skills = await skillDal.GetAllSkillsAsync();

      using (LoadListMode)
      {
        foreach (var skill in skills.OrderBy(s => s.Category).ThenBy(s => s.Name))
        {
          Add(skillPortal.FetchChild(skill));
        }
      }
    }
  }

  [Serializable]
  public class SkillInfo : ReadOnlyBase<SkillInfo>
  {
    public static readonly PropertyInfo<string> IdProperty = RegisterProperty<string>(nameof(Id));
    public string Id
    {
      get => GetProperty(IdProperty);
      private set => LoadProperty(IdProperty, value);
    }

    public static readonly PropertyInfo<SkillCategory> CategoryProperty = RegisterProperty<SkillCategory>(nameof(Category));
    public SkillCategory Category
    {
      get => GetProperty(CategoryProperty);
      private set => LoadProperty(CategoryProperty, value);
    }

    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    public string Name
    {
      get => GetProperty(NameProperty);
      private set => LoadProperty(NameProperty, value);
    }

    public static readonly PropertyInfo<bool> IsStandardProperty = RegisterProperty<bool>(nameof(IsStandard));
    public bool IsStandard
    {
      get => GetProperty(IsStandardProperty);
      private set => LoadProperty(IsStandardProperty, value);
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

    public static readonly PropertyInfo<bool> IsTheologyProperty = RegisterProperty<bool>(nameof(IsTheology));
    public bool IsTheology
    {
      get => GetProperty(IsTheologyProperty);
      private set => LoadProperty(IsTheologyProperty, value);
    }

    public static readonly PropertyInfo<bool> IsPsionicProperty = RegisterProperty<bool>(nameof(IsPsionic));
    public bool IsPsionic
    {
      get => GetProperty(IsPsionicProperty);
      private set => LoadProperty(IsPsionicProperty, value);
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

    public static readonly PropertyInfo<string> PrimaryAttributeProperty = RegisterProperty<string>(nameof(PrimaryAttribute));
    public string PrimaryAttribute
    {
      get => GetProperty(PrimaryAttributeProperty);
      private set => LoadProperty(PrimaryAttributeProperty, value);
    }

    public static readonly PropertyInfo<string?> SecondaryAttributeProperty = RegisterProperty<string?>(nameof(SecondaryAttribute));
    public string? SecondaryAttribute
    {
      get => GetProperty(SecondaryAttributeProperty);
      private set => LoadProperty(SecondaryAttributeProperty, value);
    }

    public static readonly PropertyInfo<string?> TertiaryAttributeProperty = RegisterProperty<string?>(nameof(TertiaryAttribute));
    public string? TertiaryAttribute
    {
      get => GetProperty(TertiaryAttributeProperty);
      private set => LoadProperty(TertiaryAttributeProperty, value);
    }

    [FetchChild]
    private void Fetch(Skill skill)
    {
      Id = skill.Id;
      Name = skill.Name;
      Category = skill.Category;
      IsStandard = skill.Category == SkillCategory.Standard;
      IsSpecialized = skill.IsSpecialized;
      IsMagic = skill.IsMagic;
      IsTheology = skill.IsTheology;
      IsPsionic = skill.IsPsionic;
      Untrained = skill.Untrained;
      Trained = skill.Trained;
      PrimaryAttribute = skill.PrimaryAttribute;
      SecondaryAttribute = skill.SecondaryAttribute;
      TertiaryAttribute = skill.TertiaryAttribute;
    }
  }
}
