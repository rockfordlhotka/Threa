using Csla;
using GameMechanics.Reference;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Threa.Dal;

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

    public static readonly PropertyInfo<string> PlayerEmailProperty = RegisterProperty<string>(nameof(PlayerEmail));
    [Required]
    [Display(Name = "Player email")]
    public string PlayerEmail
    {
      get => GetProperty(PlayerEmailProperty);
      private set => LoadProperty(PlayerEmailProperty, value);
    }

    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    [Required]
    [Display(Name = "Character name")]
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
    [Display(Name = "True name")]
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
    [Display(Name = "Skin")]
    public string SkinDescription
    {
      get => GetProperty(SkinDescriptionProperty);
      set => SetProperty(SkinDescriptionProperty, value);
    }

    public static readonly PropertyInfo<string> HairDescriptionProperty = RegisterProperty<string>(nameof(HairDescription));
    [Display(Name = "Hair")]
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
    [Display(Name = "Birth date")]
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
    [Display(Name = "Is passed out")]
    public bool IsPassedOut
    {
      get => GetProperty(IsPassedOutProperty);
      set => SetProperty(IsPassedOutProperty, value);
    }

    public static readonly PropertyInfo<ActionPoints> ActionPointsProperty = RegisterProperty<ActionPoints>(nameof(ActionPoints));
    [Display(Name = "Action points")]
    public ActionPoints ActionPoints
    {
      get => GetProperty(ActionPointsProperty);
      set => SetProperty(ActionPointsProperty, value);
    }

    public static readonly PropertyInfo<double> XPTotalProperty = RegisterProperty<double>(nameof(XPTotal));
    [Display(Name = "Total XP")]
    public double XPTotal
    {
      get => GetProperty(XPTotalProperty);
      private set => LoadProperty(XPTotalProperty, value);
    }

    public static readonly PropertyInfo<double> XPBankedProperty = RegisterProperty<double>(nameof(XPBanked));
    [Display(Name = "Banked XP")]
    public double XPBanked
    {
      get => GetProperty(XPBankedProperty);
      set => SetProperty(XPBankedProperty, value);
    }

    public static readonly PropertyInfo<int> DamageClassProperty = RegisterProperty<int>(nameof(DamageClass));
    [Display(Name = "Damage class")]
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
    [RunLocal]
    private void Create()
    {
      Create(Csla.ApplicationContext.User.Identity.Name);
    }

    [Create]
    [RunLocal]
    private void Create(string playerEmail)
    {
      using (BypassPropertyChecks)
      {
        PlayerEmail = playerEmail;
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
      BusinessRules.CheckRules();
    }

    private static readonly string[] mapIgnore = new string[]
      {
        nameof(Strength),
        nameof(Dexterity),
        nameof(Endurance),
        nameof(Intelligence),
        nameof(Intuition),
        nameof(Willpower),
        nameof(PhysicalBeauty),
        nameof(SocialStanding),
        nameof(Fatigue),
        nameof(Vitality),
        nameof(Wounds),
        nameof(Skills),
        nameof(ActionPoints),
      };

    [Fetch]
    private async Task Fetch(string id, [Inject] ICharacterDal dal)
    {
      var existing = await dal.GetCharacterAsync(id);
      using (BypassPropertyChecks)
      {
        Csla.Data.DataMapper.Map(existing, this, mapIgnore);
        Strength = DataPortal.FetchChild<Attribute>(existing.Strength);
        Dexterity = DataPortal.FetchChild<Attribute>(existing.Dexterity);
        Endurance = DataPortal.FetchChild<Attribute>(existing.Endurance);
        Intelligence = DataPortal.FetchChild<Attribute>(existing.Intelligence);
        Intuition = DataPortal.FetchChild<Attribute>(existing.Intuition);
        Willpower = DataPortal.FetchChild<Attribute>(existing.Willpower);
        PhysicalBeauty = DataPortal.FetchChild<Attribute>(existing.PhysicalBeauty);
        SocialStanding = DataPortal.FetchChild<Attribute>(existing.SocialStanding);
        Fatigue = DataPortal.FetchChild<Fatigue>(existing.Fatigue);
        Vitality = DataPortal.FetchChild<Vitality>(existing.Vitality);
        Wounds = DataPortal.FetchChild<WoundList>(existing.Wounds);
        Skills = DataPortal.FetchChild<SkillList>(existing.Skills);
        ActionPoints = DataPortal.FetchChild<ActionPoints>(existing.ActionPoints);
      }
      BusinessRules.CheckRules();
    }

    [Insert]
    [Update]
    private async Task InsertUpdate([Inject] ICharacterDal dal)
    {
      var toSave = dal.GetBlank();
      using (BypassPropertyChecks)
      {
        Csla.Data.DataMapper.Map(this, toSave, mapIgnore);
        FieldManager.UpdateChildren();
      }
      var result = await dal.SaveCharacter(toSave);
      Id = result.Id;
    }
  }
}
