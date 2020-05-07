using Csla;
using GameMechanics.Reference;
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

    public static readonly PropertyInfo<string> AliasesProperty = RegisterProperty<string>(nameof(Aliases));
    public string Aliases
    {
      get => GetProperty(AliasesProperty);
      set => SetProperty(AliasesProperty, value);
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

    public static readonly PropertyInfo<string> SkinDescriptionProperty = RegisterProperty<string>(nameof(SkinDescription));
    public string SkinDescription
    {
      get => GetProperty(SkinDescriptionProperty);
      set => SetProperty(SkinDescriptionProperty, value);
    }

    public static readonly PropertyInfo<string> HairDescriptionProperty = RegisterProperty<string>(nameof(HairDescription));
    public string HairDescription
    {
      get => GetProperty(HairDescriptionProperty);
      set => SetProperty(HairDescriptionProperty, value);
    }

    public static readonly PropertyInfo<double> HeightProperty = RegisterProperty<double>(nameof(Height));
    public double Height
    {
      get => GetProperty(HeightProperty);
      set => SetProperty(HeightProperty, value);
    }

    public static readonly PropertyInfo<double> WeightProperty = RegisterProperty<double>(nameof(Weight));
    public double Weight
    {
      get => GetProperty(WeightProperty);
      set => SetProperty(WeightProperty, value);
    }

    public static readonly PropertyInfo<double> BirthdateProperty = RegisterProperty<double>(nameof(Birthdate));
    public double Birthdate
    {
      get => GetProperty(BirthdateProperty);
      set => SetProperty(BirthdateProperty, value);
    }

    public static readonly PropertyInfo<string> DescriptionProperty = RegisterProperty<string>(nameof(Description));
    public string Description
    {
      get => GetProperty(DescriptionProperty);
      set => SetProperty(DescriptionProperty, value);
    }

    public static readonly PropertyInfo<string> NotesProperty = RegisterProperty<string>(nameof(Notes));
    public string Notes
    {
      get => GetProperty(NotesProperty);
      set => SetProperty(NotesProperty, value);
    }

    public static readonly PropertyInfo<Attribute> StrengthProperty = RegisterProperty<Attribute>(nameof(Strength));
    public Attribute Strength
    {
      get => GetProperty(StrengthProperty);
      private set => LoadProperty(StrengthProperty, value);
    }

    public static readonly PropertyInfo<Attribute> DexterityProperty = RegisterProperty<Attribute>(nameof(Dexterity));
    public Attribute Dexterity
    {
      get => GetProperty(DexterityProperty);
      private set => LoadProperty(DexterityProperty, value);
    }

    public static readonly PropertyInfo<Attribute> EnduranceProperty = RegisterProperty<Attribute>(nameof(Endurance));
    public Attribute Endurance
    {
      get => GetProperty(EnduranceProperty);
      private set => LoadProperty(EnduranceProperty, value);
    }

    public static readonly PropertyInfo<Attribute> WillpowerProperty = RegisterProperty<Attribute>(nameof(Willpower));
    public Attribute Willpower
    {
      get => GetProperty(WillpowerProperty);
      private set => LoadProperty(WillpowerProperty, value);
    }

    public static readonly PropertyInfo<Attribute> IntelligenceProperty = RegisterProperty<Attribute>(nameof(Intelligence));
    public Attribute Intelligence
    {
      get => GetProperty(IntelligenceProperty);
      private set => LoadProperty(IntelligenceProperty, value);
    }

    public static readonly PropertyInfo<Attribute> IntuitionProperty = RegisterProperty<Attribute>(nameof(Intuition));
    public Attribute Intuition
    {
      get => GetProperty(IntuitionProperty);
      private set => LoadProperty(IntuitionProperty, value);
    }

    public static readonly PropertyInfo<Attribute> SocialStandingProperty = RegisterProperty<Attribute>(nameof(SocialStanding));
    public Attribute SocialStanding
    {
      get => GetProperty(SocialStandingProperty);
      private set => LoadProperty(SocialStandingProperty, value);
    }

    public static readonly PropertyInfo<Attribute> PhysicalBeautyProperty = RegisterProperty<Attribute>(nameof(PhysicalBeauty));
    public Attribute PhysicalBeauty
    {
      get => GetProperty(PhysicalBeautyProperty);
      private set => LoadProperty(PhysicalBeautyProperty, value);
    }

    public static readonly PropertyInfo<Fatigue> FatigueProperty = RegisterProperty<Fatigue>(nameof(Fatigue));
    public Fatigue Fatigue
    {
      get => GetProperty(FatigueProperty);
      set => SetProperty(FatigueProperty, value);
    }

    public static readonly PropertyInfo<Vitality> VitalityProperty = RegisterProperty<Vitality>(nameof(Vitality));
    public Vitality Vitality
    {
      get => GetProperty(VitalityProperty);
      set => SetProperty(VitalityProperty, value);
    }

    public static readonly PropertyInfo<SkillList> SkillsProperty = RegisterProperty<SkillList>(nameof(Skills));
    public SkillList Skills
    {
      get => GetProperty(SkillsProperty);
      set => SetProperty(SkillsProperty, value);
    }

    public static readonly PropertyInfo<WoundList> WoundsProperty = RegisterProperty<WoundList>(nameof(Wounds));
    public WoundList Wounds
    {
      get => GetProperty(WoundsProperty);
      private set => LoadProperty(WoundsProperty, value);
    }

    public static readonly PropertyInfo<bool> IsPassedOutProperty = RegisterProperty<bool>(nameof(IsPassedOut));
    public bool IsPassedOut
    {
      get => GetProperty(IsPassedOutProperty);
      set => SetProperty(IsPassedOutProperty, value);
    }

    public static readonly PropertyInfo<ActionPoints> ActionPointsProperty = RegisterProperty<ActionPoints>(nameof(ActionPoints));
    public ActionPoints ActionPoints
    {
      get => GetProperty(ActionPointsProperty);
      set => SetProperty(ActionPointsProperty, value);
    }

    public static readonly PropertyInfo<double> XPTotalProperty = RegisterProperty<double>(nameof(XPTotal));
    public double XPTotal
    {
      get => GetProperty(XPTotalProperty);
      private set => LoadProperty(XPTotalProperty, value);
    }

    public static readonly PropertyInfo<double> XPBankedProperty = RegisterProperty<double>(nameof(XPBanked));
    public double XPBanked
    {
      get => GetProperty(XPBankedProperty);
      set => SetProperty(XPBankedProperty, value);
    }

    public static readonly PropertyInfo<int> DamageClassProperty = RegisterProperty<int>(nameof(DamageClass));
    public int DamageClass
    {
      get => GetProperty(DamageClassProperty);
      set => SetProperty(DamageClassProperty, value);
    }

    public int GetAttribute(string attributeName)
    {
      switch (attributeName)
      {
        case "STR":
        case "STT":
          return Strength.Value;
        case "DEX":
          return Dexterity.Value;
        case "END":
          return Endurance.Value;
        case "WIL":
          return Willpower.Value;
        case "INT":
          return Intelligence.Value;
        case "ITT":
          return Intuition.Value;
        case "PHY":
          return PhysicalBeauty.Value;
        case "SOC":
          return SocialStanding.Value;
        default:
          throw new ArgumentException(nameof(attributeName));
      }
    }

    public void EndOfRound()
    {
      Fatigue.EndOfRound();
      Vitality.EndOfRound();
      Wounds.EndOfRound();
      ActionPoints.EndOfRound();
    }

    public void TakeDamage(DamageValue damageValue)
    {
      Fatigue.TakeDamage(damageValue);
      Vitality.TakeDamage(damageValue);
      Wounds.TakeDamage(damageValue);
    }

    [Create]
    private void Create(string playerId)
    {
      using (BypassPropertyChecks)
      {
        PlayerId = playerId;
        DamageClass = 1;
        Strength = DataPortal.CreateChild<Attribute>();
        Dexterity = DataPortal.CreateChild<Attribute>();
        Endurance = DataPortal.CreateChild<Attribute>();
        Intelligence = DataPortal.CreateChild<Attribute>();
        Intuition = DataPortal.CreateChild<Attribute>();
        Willpower = DataPortal.CreateChild<Attribute>();
        PhysicalBeauty = DataPortal.CreateChild<Attribute>();
        SocialStanding = DataPortal.CreateChild<Attribute>();
        Fatigue = DataPortal.CreateChild<Fatigue>(this);
        Vitality = DataPortal.CreateChild<Vitality>(this);
        Wounds = DataPortal.CreateChild<WoundList>();
        Skills = DataPortal.CreateChild<SkillList>();
        ActionPoints = DataPortal.CreateChild<ActionPoints>(this);
      }
    }
  }
}
