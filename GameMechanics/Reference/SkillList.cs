using System;
using Csla;

namespace GameMechanics.Reference
{
  [Serializable]
  public class SkillList : ReadOnlyListBase<SkillList, SkillInfo>
  {
    [Fetch]
    private void Fetch([Inject] IChildDataPortal<SkillInfo> skillPortal)
    {
      using (LoadListMode)
      {
        Add(skillPortal.FetchChild("Physicality"));
        Add(skillPortal.FetchChild("Dodge"));
        Add(skillPortal.FetchChild("Drive"));
        Add(skillPortal.FetchChild("Reasoning"));
        Add(skillPortal.FetchChild("Awareness"));
        Add(skillPortal.FetchChild("Focus"));
        Add(skillPortal.FetchChild("Bearing"));
        Add(skillPortal.FetchChild("Influence"));
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

    public static readonly PropertyInfo<string> CategoryProperty = RegisterProperty<string>(nameof(Category));
    public string Category
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

    public static readonly PropertyInfo<string> SecondaryAttributeProperty = RegisterProperty<string>(nameof(SecondaryAttribute));
    public string SecondaryAttribute
    {
      get => GetProperty(SecondaryAttributeProperty);
      private set => LoadProperty(SecondaryAttributeProperty, value);
    }

    public static readonly PropertyInfo<string> TertiaryAttributeProperty = RegisterProperty<string>(nameof(TertiaryAttribute));
    public string TertiaryAttribute
    {
      get => GetProperty(TertiaryAttributeProperty);
      private set => LoadProperty(TertiaryAttributeProperty, value);
    }

    [FetchChild]
    private void Fetch()
    {

    }

    [FetchChild]
    private void Fetch(string skillName)
    {
      switch (skillName)
      {
        case "Physicality":
          Id = "";
          Name = skillName;
          IsStandard = true;
          Untrained = 3;
          Trained = 1;
          PrimaryAttribute = "STR";
          break;
        case "Dodge":
          Id = "";
          Name = skillName;
          IsStandard = true;
          Untrained = 6;
          Trained = 4;
          PrimaryAttribute = "DEX/ITT";
          break;
        case "Drive":
          Id = "";
          Name = skillName;
          IsStandard = true;
          Untrained = 5;
          Trained = 3;
          PrimaryAttribute = "WIL/END";
          break;
        case "Reasoning":
          Id = "";
          Name = skillName;
          IsStandard = true;
          Untrained = 5;
          Trained = 3;
          PrimaryAttribute = "INT";
          break;
        case "Awareness":
          Id = "";
          Name = skillName;
          IsStandard = true;
          Untrained = 5;
          Trained = 2;
          PrimaryAttribute = "ITT";
          break;
        case "Focus":
          Id = "";
          Name = skillName;
          IsStandard = true;
          Untrained = 5;
          Trained = 2;
          PrimaryAttribute = "WIL";
          break;
        case "Bearing":
          Id = "";
          Name = skillName;
          IsStandard = true;
          Untrained = 4;
          Trained = 2;
          PrimaryAttribute = "SOC/INT";
          break;
        case "Influence":
          Id = "";
          Name = skillName;
          IsStandard = true;
          Untrained = 4;
          Trained = 2;
          PrimaryAttribute = "PHY";
          break;
        default:
          break;
      }
    }
  }
}
