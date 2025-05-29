using Csla;
using Csla.Core;
using Csla.Rules;
using GameMechanics.Reference;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Threa.Dal;

namespace GameMechanics
{
  [Serializable]
  public class CharacterEdit : BusinessBase<CharacterEdit>
  {
    public bool IsBeingSaved { get; set; }

    protected override void OnChildChanged(ChildChangedEventArgs e)
    {
      if (!IsBeingSaved && e.ChildObject is AttributeEdit)
      {
        BusinessRules.CheckRules(FatigueProperty);
        BusinessRules.CheckRules(VitalityProperty);
      }
    }

    public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id));
    public int Id
    {
      get => GetProperty(IdProperty);
      private set => LoadProperty(IdProperty, value);
    }

    public static readonly PropertyInfo<int> PlayerIdProperty = RegisterProperty<int>(nameof(PlayerId));
    public int PlayerId
    {
      get => GetProperty(PlayerIdProperty);
      private set => LoadProperty(PlayerIdProperty, value);
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

    public static readonly PropertyInfo<string> ImageUrlProperty = RegisterProperty<string>(nameof(ImageUrl));
    public string ImageUrl
    {
      get => GetProperty(ImageUrlProperty);
      set => SetProperty(ImageUrlProperty, value);
    }

    public static readonly PropertyInfo<string> NotesProperty = RegisterProperty<string>(nameof(Notes));
    public string Notes
    {
      get => GetProperty(NotesProperty);
      set => SetProperty(NotesProperty, value);
    }

    public static readonly PropertyInfo<AttributeEditList> AttributeListProperty = RegisterProperty<AttributeEditList>(nameof(AttributeList));
    public AttributeEditList AttributeList
    {
      get => GetProperty(AttributeListProperty);
      private set => LoadProperty(AttributeListProperty, value);
    }

    public static readonly PropertyInfo<Fatigue> FatigueProperty = RegisterProperty<Fatigue>(nameof(Fatigue));
    public Fatigue Fatigue
    {
      get => GetProperty(FatigueProperty);
      private set => LoadProperty(FatigueProperty, value);
    }

    public static readonly PropertyInfo<Vitality> VitalityProperty = RegisterProperty<Vitality>(nameof(Vitality));
    public Vitality Vitality
    {
      get => GetProperty(VitalityProperty);
      private set => LoadProperty(VitalityProperty, value);
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
      var result = AttributeList.Where(r => r.Name == attributeName).FirstOrDefault();
      if (result == null)
        return 0;
      else
        return result.Value;
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

    protected override void AddBusinessRules()
    {
      base.AddBusinessRules();
    }

    [Create]
    [RunLocal]
    private void Create([Inject] ApplicationContext applicationContext,
      [Inject] IChildDataPortal<AttributeEditList> attributePortal,
      [Inject] IChildDataPortal<SkillEditList> skillPortal,
      [Inject] IChildDataPortal<WoundList> woundPortal,
      [Inject] IChildDataPortal<ActionPoints> actionPointsPortal,
      [Inject] IChildDataPortal<Fatigue> fatPortal,
      [Inject] IChildDataPortal<Vitality> vitPortal)
    {
      var ci = (System.Security.Claims.ClaimsIdentity?)applicationContext.User.Identity ?? 
        throw new InvalidOperationException("User not authenticated");
      var playerId = int.Parse(ci.Claims.Where(r => r.Type == ClaimTypes.NameIdentifier).First().Value);
      Create(playerId, attributePortal, skillPortal, woundPortal, actionPointsPortal, fatPortal, vitPortal);
    }

    [Create]
    [RunLocal]
    private void Create(int playerId, 
      [Inject] IChildDataPortal<AttributeEditList> attributePortal,
      [Inject] IChildDataPortal<SkillEditList> skillPortal,
      [Inject] IChildDataPortal<WoundList> woundPortal,
      [Inject] IChildDataPortal<ActionPoints> actionPointsPortal,
      [Inject] IChildDataPortal<Fatigue> fatPortal,
      [Inject] IChildDataPortal<Vitality> vitPortal)
    {
      using (BypassPropertyChecks)
      {
        DamageClass = 1;
        PlayerId = playerId;
        AttributeList = attributePortal.CreateChild();
        Skills = skillPortal.CreateChild();
        Wounds = woundPortal.CreateChild();
        Fatigue = fatPortal.CreateChild(this);
        Vitality = vitPortal.CreateChild(this);
        ActionPoints = actionPointsPortal.CreateChild(this);
      }
      BusinessRules.CheckRules();
    }

    private static readonly string[] mapIgnore =
      [
        nameof(AttributeList),
        nameof(ActionPoints),
        nameof(Skills),
        nameof(Fatigue),
        nameof(Vitality),
        nameof(IsPassedOut),
        nameof(IsBeingSaved),
        nameof(Threa.Dal.Dto.Character.Wounds),
        nameof(Threa.Dal.Dto.Character.ActionPointAvailable),
        nameof(Threa.Dal.Dto.Character.ActionPointMax),
        nameof(Threa.Dal.Dto.Character.ActionPointRecovery),
        nameof(Threa.Dal.Dto.Character.FatValue),
        nameof(Threa.Dal.Dto.Character.FatBaseValue),
        nameof(Threa.Dal.Dto.Character.FatPendingDamage),
        nameof(Threa.Dal.Dto.Character.FatPendingHealing),
        nameof(Threa.Dal.Dto.Character.VitValue),
        nameof(Threa.Dal.Dto.Character.VitBaseValue),
        nameof(Threa.Dal.Dto.Character.VitPendingDamage),
        nameof(Threa.Dal.Dto.Character.VitPendingHealing),
      ];

    [Fetch]
    private async Task FetchAsync(int id, [Inject] ICharacterDal dal,
      [Inject] IChildDataPortal<AttributeEditList> attributePortal,
      [Inject] IChildDataPortal<SkillEditList> skillPortal,
      [Inject] IChildDataPortal<Fatigue> fatPortal,
      [Inject] IChildDataPortal<Vitality> vitPortal)
    {
      var existing = await dal.GetCharacterAsync(id);
      using (BypassPropertyChecks)
      {
        Csla.Data.DataMapper.Map(existing, this, mapIgnore);
        Fatigue = fatPortal.FetchChild(existing);
        Vitality = vitPortal.FetchChild(existing);
        AttributeList = attributePortal.FetchChild(existing.AttributeList);
        Skills = skillPortal.FetchChild(existing.Skills);
      }
      BusinessRules.CheckRules();
    }

    [Insert]
    private async Task InsertAsync([Inject] ICharacterDal dal,
      [Inject] IChildDataPortal<AttributeEditList> attributePortal,
      [Inject] IChildDataPortal<SkillEditList> skillPortal,
      [Inject] IChildDataPortal<Fatigue> fatPortal,
      [Inject] IChildDataPortal<Vitality> vitPortal)
    {
      var toSave = dal.GetBlank();
      using (BypassPropertyChecks)
      {
        Csla.Data.DataMapper.Map(this, toSave, mapIgnore);
        fatPortal.UpdateChild(Fatigue, toSave);
        vitPortal.UpdateChild(Vitality, toSave);
        attributePortal.UpdateChild(AttributeList, toSave.AttributeList);
        skillPortal.UpdateChild(Skills, toSave.Skills);
      }
      var result = await dal.SaveCharacterAsync(toSave);
      Id = result.Id;
    }

    [Update]
    private async Task UpdateAsync([Inject] ICharacterDal dal,
      [Inject] IChildDataPortal<AttributeEditList> attributePortal,
      [Inject] IChildDataPortal<SkillEditList> skillPortal,
      [Inject] IChildDataPortal<Fatigue> fatPortal,
      [Inject] IChildDataPortal<Vitality> vitPortal)
    {
      using (BypassPropertyChecks)
      {
        var existing = await dal.GetCharacterAsync(Id);
        Csla.Data.DataMapper.Map(this, existing, mapIgnore);
        fatPortal.UpdateChild(Fatigue, existing);
        vitPortal.UpdateChild(Vitality, existing);
        attributePortal.UpdateChild(AttributeList, existing.AttributeList);
        skillPortal.UpdateChild(Skills, existing.Skills);
        await dal.SaveCharacterAsync(existing);
      }
    }

    [Delete]
    private async Task DeleteAsync([Inject] ICharacterDal dal)
    {
      await dal.DeleteCharacterAsync(Id);
    }
  }
}
