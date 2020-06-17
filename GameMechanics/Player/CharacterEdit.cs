using Csla;
using Csla.Core;
using Csla.Rules;
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
    public CharacterEdit()
    {
      this.ChildChanged += CharacterEdit_ChildChanged;
    }

    public bool IsBeingSaved { get; set; }

    private void CharacterEdit_ChildChanged(object sender, Csla.Core.ChildChangedEventArgs e)
    {
      if (!IsBeingSaved && e.ChildObject is AttributeEdit)
      {
        BusinessRules.CheckRules(FatigueProperty);
        BusinessRules.CheckRules(VitalityProperty);
      }
    }

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

    public static readonly PropertyInfo<string> HeightProperty = RegisterProperty<string>(nameof(Height));
    public string Height
    {
      get => GetProperty(HeightProperty);
      set => SetProperty(HeightProperty, value);
    }

    public static readonly PropertyInfo<string> WeightProperty = RegisterProperty<string>(nameof(Weight));
    public string Weight
    {
      get => GetProperty(WeightProperty);
      set => SetProperty(WeightProperty, value);
    }

    public static readonly PropertyInfo<long> BirthdateProperty = RegisterProperty<long>(nameof(Birthdate));
    [Display(Name = "Birth date")]
    public long Birthdate
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

    public static readonly PropertyInfo<bool> PlayableProperty = RegisterProperty<bool>(nameof(Playable));
    public bool Playable
    {
      get => GetProperty(PlayableProperty);
      set => SetProperty(PlayableProperty, value);
    }

    public static readonly PropertyInfo<AttributeEditList> AttributeListProperty = RegisterProperty<AttributeEditList>(nameof(AttributeList));
    public AttributeEditList AttributeList
    {
      get => GetProperty(AttributeListProperty);
      private set => LoadProperty(AttributeListProperty, value);
    }

    public static readonly PropertyInfo<int> FatigueProperty = RegisterProperty<int>(nameof(Fatigue));
    public int Fatigue
    {
      get => GetProperty(FatigueProperty);
      private set => LoadProperty(FatigueProperty, value);
    }

    public static readonly PropertyInfo<int> VitalityProperty = RegisterProperty<int>(nameof(Vitality));
    public int Vitality
    {
      get => GetProperty(VitalityProperty);
      private set => LoadProperty(VitalityProperty, value);
    }

    public static readonly PropertyInfo<SkillEditList> SkillsProperty = RegisterProperty<SkillEditList>(nameof(Skills));
    public SkillEditList Skills
    {
      get => GetProperty(SkillsProperty);
      set => SetProperty(SkillsProperty, value);
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

    protected override void AddBusinessRules()
    {
      base.AddBusinessRules();
      BusinessRules.AddRule(new CalculateFatigue { PrimaryProperty = FatigueProperty });
      BusinessRules.AddRule(new CalculateVitality { PrimaryProperty = VitalityProperty });
      BusinessRules.AddRule(new LockCharacter(PlayableProperty));
      BusinessRules.AddRule(new LockCharacter(DamageClassProperty));
      BusinessRules.AddRule(new LockCharacter(BirthdateProperty));
    }

    public class CalculateFatigue : BusinessRule
    {
      protected override void Execute(IRuleContext context)
      {
        var character = (CharacterEdit)context.Target;
        var end = character.GetAttribute("END");
        var wil = character.GetAttribute("WIL");
        context.AddOutValue(end + wil - 5);
      }
    }

    public class CalculateVitality : BusinessRule
    {
      protected override void Execute(IRuleContext context)
      {
        var character = (CharacterEdit)context.Target;
        var str = character.GetAttribute("STR");
        context.AddOutValue(str * 2 - 5);
      }
    }

    public class LockCharacter : AuthorizationRule
    {
      public LockCharacter(IPropertyInfo property)
        : base(AuthorizationActions.WriteProperty, property)
      {
        CacheResult = false;
      }

      protected override void Execute(IAuthorizationContext context)
      {
        var character = (CharacterEdit)context.Target;
        var locked = character.Playable;
        context.HasPermission = !locked;
      }
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
        AttributeList = DataPortal.CreateChild<AttributeEditList>();
        Skills = DataPortal.CreateChild<SkillEditList>();
      }
      BusinessRules.CheckRules();
    }

    private static readonly string[] mapIgnore = new string[]
      {
        nameof(AttributeList),
        nameof(Skills),
        nameof(Fatigue),
        nameof(Vitality),
        nameof(IsBeingSaved),
        nameof(Threa.Dal.Dto.Character.ActionPoints),
        nameof(Threa.Dal.Dto.Character.DamageList),
        nameof(Threa.Dal.Dto.Character.IsPassedOut),
        nameof(Threa.Dal.Dto.Character.Wounds),
      };

    [Fetch]
    private async Task FetchAsync(string id, [Inject] ICharacterDal dal)
    {
      var existing = await dal.GetCharacterAsync(id);
      using (BypassPropertyChecks)
      {
        Csla.Data.DataMapper.Map(existing, this, mapIgnore);
        if (existing.DamageList != null)
        {
          Fatigue = existing.DamageList.Where(r => r.Name == "FAT").First().BaseValue;
          Vitality = existing.DamageList.Where(r => r.Name == "VIT").First().BaseValue;
        }
        AttributeList = DataPortal.FetchChild<AttributeEditList>(existing.AttributeList);
        Skills = DataPortal.FetchChild<SkillEditList>(existing.Skills);
      }
      BusinessRules.CheckRules();
    }

    [Insert]
    private async Task InsertAsync([Inject] ICharacterDal dal)
    {
      var toSave = dal.GetBlank();
      using (BypassPropertyChecks)
      {
        Csla.Data.DataMapper.Map(this, toSave, mapIgnore);
        toSave.DamageList.Add(new Threa.Dal.Dto.Damage { Name = "FAT", BaseValue = Fatigue, Value = Fatigue });
        toSave.DamageList.Add(new Threa.Dal.Dto.Damage { Name = "VIT", BaseValue = Vitality, Value = Vitality });
        DataPortal.UpdateChild(AttributeList, toSave.AttributeList);
        DataPortal.UpdateChild(Skills, toSave.Skills);
      }
      var result = await dal.SaveCharacter(toSave);
      Id = result.Id;
    }

    [Update]
    private async Task UpdateAsync([Inject] ICharacterDal dal)
    {
      using (BypassPropertyChecks)
      {
        var existing = await dal.GetCharacterAsync(Id);
        Csla.Data.DataMapper.Map(this, existing, mapIgnore);
        existing.DamageList.Add(new Threa.Dal.Dto.Damage { Name = "FAT", BaseValue = Fatigue, Value = Fatigue });
        existing.DamageList.Add(new Threa.Dal.Dto.Damage { Name = "VIT", BaseValue = Vitality, Value = Vitality });
        DataPortal.UpdateChild(AttributeList, existing.AttributeList);
        DataPortal.UpdateChild(Skills, existing.Skills);
        await dal.SaveCharacter(existing);
      }
    }

    [Delete]
    private async Task DeleteAsync([Inject] ICharacterDal dal)
    {
      await dal.DeleteCharacter(Id);
    }
  }
}
