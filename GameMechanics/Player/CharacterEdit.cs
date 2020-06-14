using Csla;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal;

namespace GameMechanics.Player
{
  [Serializable]
  public class CharacterEdit : BusinessBase<CharacterEdit>
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

    public static readonly PropertyInfo<AttributeList> AttributeListProperty = RegisterProperty<AttributeList>(nameof(AttributeList));
    public AttributeList AttributeList
    {
      get => GetProperty(AttributeListProperty);
      private set => LoadProperty(AttributeListProperty, value);
    }

    public static readonly PropertyInfo<DamageList> DamageListProperty = RegisterProperty<DamageList>(nameof(DamageList));
    public DamageList DamageList
    {
      get => GetProperty(DamageListProperty);
      private set => LoadProperty(DamageListProperty, value);
    }

    public Damage Fatigue
    {
      get => DamageList.Where(r => r.Name == "FAT").First();
    }

    public Damage Vitality
    {
      get => DamageList.Where(r => r.Name == "VIT").First();
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
      return AttributeList.Where(r => r.Name == attributeName).First().Value;
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
        AttributeList = DataPortal.CreateChild<AttributeList>();
        DamageList = DataPortal.CreateChild<DamageList>(this);
        Wounds = DataPortal.CreateChild<WoundList>();
        Skills = DataPortal.CreateChild<SkillList>();
        ActionPoints = DataPortal.CreateChild<ActionPoints>(this);
      }
      BusinessRules.CheckRules();
    }

    private static readonly string[] mapIgnore = new string[]
      {
        nameof(AttributeList),
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
        AttributeList = DataPortal.FetchChild<AttributeList>(existing.AttributeList);
        DamageList = DataPortal.FetchChild<DamageList>(existing.DamageList);
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
