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
      set => SetProperty(IdProperty, value);
    }

    public static readonly PropertyInfo<string> PlayerIdProperty = RegisterProperty<string>(nameof(PlayerId));
    public string PlayerId
    {
      get => GetProperty(PlayerIdProperty);
      set => SetProperty(PlayerIdProperty, value);
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
      set => SetProperty(StrengthProperty, value);
    }

    public static readonly PropertyInfo<int> DexterityProperty = RegisterProperty<int>(nameof(Dexterity));
    public int Dexterity
    {
      get => GetProperty(DexterityProperty);
      set => SetProperty(DexterityProperty, value);
    }

    public static readonly PropertyInfo<int> EnduranceProperty = RegisterProperty<int>(nameof(Endurance));
    public int Endurance
    {
      get => GetProperty(EnduranceProperty);
      set => SetProperty(EnduranceProperty, value);
    }

    public static readonly PropertyInfo<int> IntelligenceProperty = RegisterProperty<int>(nameof(Intelligence));
    public int Intelligence
    {
      get => GetProperty(IntelligenceProperty);
      set => SetProperty(IntelligenceProperty, value);
    }

    public static readonly PropertyInfo<int> IntuitionProperty = RegisterProperty<int>(nameof(Intuition));
    public int Intuition
    {
      get => GetProperty(IntuitionProperty);
      set => SetProperty(IntuitionProperty, value);
    }

    public static readonly PropertyInfo<int> WillpowerProperty = RegisterProperty<int>(nameof(Willpower));
    public int Willpower
    {
      get => GetProperty(WillpowerProperty);
      set => SetProperty(WillpowerProperty, value);
    }

    public static readonly PropertyInfo<int> SocialStandingProperty = RegisterProperty<int>(nameof(SocialStanding));
    public int SocialStanding
    {
      get => GetProperty(SocialStandingProperty);
      set => SetProperty(SocialStandingProperty, value);
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
      set => SetProperty(FatigueBaseProperty, value);
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
      set => SetProperty(VitalityBaseProperty, value);
    }

    public static readonly PropertyInfo<Skills> SkillsProperty = RegisterProperty<Skills>(nameof(Skills));
    public Skills Skills
    {
      get => GetProperty(SkillsProperty);
      set => SetProperty(SkillsProperty, value);
    }

    public int GetAttribute(string attributeName)
    {
      switch (attributeName)
      {
        case "STR":
        case "STT":
          return Strength;
        case "DEX":
          return Dexterity;
        case "END":
          return Endurance;
        case "WIL":
          return Willpower;
        case "INT":
          return Intelligence;
        case "ITT":
          return Intuition;
        case "PHY":
          return PhysicalBeauty;
        case "SOC":
          return SocialStanding;
        default:
          throw new ArgumentException(nameof(attributeName));
      }
    }

    private static int CalcFat(int end, int wil)
    {
      return (end + wil) / 2 + 14;
    }

    private static int CalcVit(int str)
    {
      return (str * 2) / 2 + 14;
    }

    [Create]
    private void Create()
    {
      using (BypassPropertyChecks)
      {
        Skills = DataPortal.CreateChild<Skills>();
        Strength = 5;
        Dexterity = 5;
        Endurance = 5;
        Intelligence = 5;
        Intuition = 5;
        Willpower = 5;
        PhysicalBeauty = 5;
        SocialStanding = 5;
        FatigueBase = CalcFat(Endurance, Willpower);
        Fatigue = FatigueBase;
        VitalityBase = CalcVit(Strength);
        Vitality = VitalityBase;
      }
    }
  }
}
