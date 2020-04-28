using System;
using Csla;

namespace GameMechanics
{
  [Serializable]
  public class SkillList : ReadOnlyListBase<SkillList, SkillInfo>
  {
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

    public static readonly PropertyInfo<string> PrimarySkillProperty = RegisterProperty<string>(nameof(PrimarySkill));
    public string PrimarySkill
    {
      get => GetProperty(PrimarySkillProperty);
      private set => LoadProperty(PrimarySkillProperty, value);
    }

    public static readonly PropertyInfo<string> SecondarySkillProperty = RegisterProperty<string>(nameof(SecondarySkill));
    public string SecondarySkill
    {
      get => GetProperty(SecondarySkillProperty);
      private set => LoadProperty(SecondarySkillProperty, value);
    }

    public static readonly PropertyInfo<string> TertiarySkillProperty = RegisterProperty<string>(nameof(TertiarySkill));
    public string TertiarySkill
    {
      get => GetProperty(TertiarySkillProperty);
      private set => LoadProperty(TertiarySkillProperty, value);
    }
  }
}
