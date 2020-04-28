using Csla;
using System;

namespace GameMechanics
{
  [Serializable]
  public class Character : BusinessBase<Character>
  {
    public static readonly PropertyInfo<string> IdProperty = RegisterProperty<string>(nameof(Id));
    public string Id
    {
      get => GetProperty(IdProperty);
      private set => LoadProperty(IdProperty, value);
    }

    public static readonly PropertyInfo<string> PlayerIdProperty = RegisterProperty<string>(nameof(PlayerId));
    public string PlayerId
    {
      get => GetProperty(PlayerIdProperty);
      private set => LoadProperty(PlayerIdProperty, value);
    }

    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    public string Name
    {
      get => GetProperty(NameProperty);
      set => SetProperty(NameProperty, value);
    }

    public static readonly PropertyInfo<string> TrueNameProperty = RegisterProperty<string>(nameof(TrueName));
    public string TrueName
    {
      get => GetProperty(TrueNameProperty);
      set => SetProperty(TrueNameProperty, value);
    }

    public static readonly PropertyInfo<string> SpeciesProperty = RegisterProperty<string>(nameof(Species));
    public string Species
    {
      get => GetProperty(SpeciesProperty);
      set => SetProperty(SpeciesProperty, value);
    }

    public static readonly PropertyInfo<int> StrengthProperty = RegisterProperty<int>(nameof(Strength));
    public int Strength
    {
      get => GetProperty(StrengthProperty);
      private set => LoadProperty(StrengthProperty, value);
    }

    public static readonly PropertyInfo<int> DexterityProperty = RegisterProperty<int>(nameof(Dexterity));
    public int Dexterity
    {
      get => GetProperty(DexterityProperty);
      private set => LoadProperty(DexterityProperty, value);
    }

    public static readonly PropertyInfo<int> EnduranceProperty = RegisterProperty<int>(nameof(Endurance));
    public int Endurance
    {
      get => GetProperty(EnduranceProperty);
      private set => LoadProperty(EnduranceProperty, value);
    }

    public static readonly PropertyInfo<int> IntelligenceProperty = RegisterProperty<int>(nameof(Intelligence));
    public int Intelligence
    {
      get => GetProperty(IntelligenceProperty);
      private set => LoadProperty(IntelligenceProperty, value);
    }

    public static readonly PropertyInfo<int> IntuitionProperty = RegisterProperty<int>(nameof(Intuition));
    public int Intuition
    {
      get => GetProperty(IntuitionProperty);
      private set => LoadProperty(IntuitionProperty, value);
    }

    public static readonly PropertyInfo<int> WillpowerProperty = RegisterProperty<int>(nameof(Willpower));
    public int Willpower
    {
      get => GetProperty(WillpowerProperty);
      private set => LoadProperty(WillpowerProperty, value);
    }

    public static readonly PropertyInfo<int> SocialStandingProperty = RegisterProperty<int>(nameof(SocialStanding));
    public int SocialStanding
    {
      get => GetProperty(SocialStandingProperty);
      private set => LoadProperty(SocialStandingProperty, value);
    }

    public static readonly PropertyInfo<int> PhysicalBeautyProperty = RegisterProperty<int>(nameof(PhysicalBeauty));
    public int PhysicalBeauty
    {
      get => GetProperty(PhysicalBeautyProperty);
      set => SetProperty(PhysicalBeautyProperty, value);
    }

    public static readonly PropertyInfo<int> FatigueProperty = RegisterProperty<int>(nameof(Fatigue));
    public int Fatigue
    {
      get => GetProperty(FatigueProperty);
      set => SetProperty(FatigueProperty, value);
    }

    public static readonly PropertyInfo<int> FatigueBaseProperty = RegisterProperty<int>(nameof(FatigueBase));
    public int FatigueBase
    {
      get => GetProperty(FatigueBaseProperty);
      private set => LoadProperty(FatigueBaseProperty, value);
    }

    public static readonly PropertyInfo<int> VitalityProperty = RegisterProperty<int>(nameof(Vitality));
    public int Vitality
    {
      get => GetProperty(VitalityProperty);
      set => SetProperty(VitalityProperty, value);
    }

    public static readonly PropertyInfo<int> VitalityBaseProperty = RegisterProperty<int>(nameof(VitalityBase));
    public int VitalityBase
    {
      get => GetProperty(VitalityBaseProperty);
      private set => LoadProperty(VitalityBaseProperty, value);
    }

    public static readonly PropertyInfo<Skills> SkillsProperty = RegisterProperty<Skills>(nameof(Skills));
    public Skills Skills
    {
      get => GetProperty(SkillsProperty);
      private set => LoadProperty(SkillsProperty, value);
    }
  }
}
