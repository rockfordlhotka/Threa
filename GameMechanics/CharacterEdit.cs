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
        OnPropertyChanged(FatigueProperty);
        OnPropertyChanged(VitalityProperty);
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

    public static readonly PropertyInfo<string> TrueNameProperty = RegisterProperty<string>(nameof(TrueName));
    [Display(Name = "True name")]
    public string TrueName
    {
      get => GetProperty(TrueNameProperty);
      set => SetProperty(TrueNameProperty, value);
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

    public static readonly PropertyInfo<EffectList> EffectsProperty = RegisterProperty<EffectList>(nameof(Effects));
    /// <summary>
    /// All active effects on this character (wounds, buffs, debuffs, poisons, spells, etc.).
    /// </summary>
    public EffectList Effects
    {
      get => GetProperty(EffectsProperty);
      private set => LoadProperty(EffectsProperty, value);
    }

    public static readonly PropertyInfo<bool> IsPassedOutProperty = RegisterProperty<bool>(nameof(IsPassedOut));
    [Display(Name = "Is passed out")]
    public bool IsPassedOut
    {
      get => GetProperty(IsPassedOutProperty);
      set => SetProperty(IsPassedOutProperty, value);
    }

    public static readonly PropertyInfo<bool> IsPlayableProperty = RegisterProperty<bool>(nameof(IsPlayable));
    [Display(Name = "Is playable")]
    public bool IsPlayable
    {
      get => GetProperty(IsPlayableProperty);
      private set => LoadProperty(IsPlayableProperty, value);
    }

    /// <summary>
    /// Activates the character, making it playable. This is a one-way operation.
    /// Once activated, certain properties like attributes become read-only.
    /// </summary>
    public void Activate()
    {
      if (!IsPlayable)
      {
        IsPlayable = true;
      }
    }

    public static readonly PropertyInfo<ActionPoints> ActionPointsProperty = RegisterProperty<ActionPoints>(nameof(ActionPoints));
    [Display(Name = "Action points")]
    public ActionPoints ActionPoints
    {
      get => GetProperty(ActionPointsProperty);
      private set => LoadProperty(ActionPointsProperty, value);
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

    public static readonly PropertyInfo<int> CopperCoinsProperty = RegisterProperty<int>(nameof(CopperCoins));
    [Display(Name = "Copper coins")]
    public int CopperCoins
    {
      get => GetProperty(CopperCoinsProperty);
      set => SetProperty(CopperCoinsProperty, value);
    }

    public static readonly PropertyInfo<int> SilverCoinsProperty = RegisterProperty<int>(nameof(SilverCoins));
    [Display(Name = "Silver coins")]
    public int SilverCoins
    {
      get => GetProperty(SilverCoinsProperty);
      set => SetProperty(SilverCoinsProperty, value);
    }

    public static readonly PropertyInfo<int> GoldCoinsProperty = RegisterProperty<int>(nameof(GoldCoins));
    [Display(Name = "Gold coins")]
    public int GoldCoins
    {
      get => GetProperty(GoldCoinsProperty);
      set => SetProperty(GoldCoinsProperty, value);
    }

    public static readonly PropertyInfo<int> PlatinumCoinsProperty = RegisterProperty<int>(nameof(PlatinumCoins));
    [Display(Name = "Platinum coins")]
    public int PlatinumCoins
    {
      get => GetProperty(PlatinumCoinsProperty);
      set => SetProperty(PlatinumCoinsProperty, value);
    }

    /// <summary>
    /// Gets the total currency value in copper pieces.
    /// 1 sp = 20 cp, 1 gp = 400 cp, 1 pp = 8000 cp
    /// </summary>
    public int TotalCopperValue => CopperCoins + (SilverCoins * 20) + (GoldCoins * 400) + (PlatinumCoins * 8000);

    /// <summary>
    /// Gets the raw/base attribute value without effect modifiers.
    /// </summary>
    /// <param name="attributeName">The attribute name (STR, DEX, END, INT, ITT, WIL, PHY).</param>
    /// <returns>The base attribute value.</returns>
    public int GetAttribute(string attributeName)
    {
      var result = AttributeList.Where(r => r.Name == attributeName).FirstOrDefault();
      if (result == null)
        return 0;
      else
        return result.Value;
    }

    /// <summary>
    /// Gets the effective attribute value including all active effect modifiers.
    /// Use this for game mechanics calculations (combat, skills, etc.).
    /// </summary>
    /// <param name="attributeName">The attribute name (STR, DEX, END, INT, ITT, WIL, PHY).</param>
    /// <returns>The effective attribute value after effect modifiers.</returns>
    public int GetEffectiveAttribute(string attributeName)
    {
      var baseValue = GetAttribute(attributeName);
      var modifier = Effects.GetAttributeModifier(attributeName, baseValue);
      return baseValue + modifier;
    }

    /// <summary>
    /// Updates attribute modifiers when species changes.
    /// Maintains base values and recalculates final values with new species modifiers.
    /// </summary>
    /// <param name="newSpeciesInfo">The new species information with modifiers.</param>
    public void UpdateSpeciesModifiers(Reference.SpeciesInfo? newSpeciesInfo)
    {
      if (AttributeList == null) return;
      
      foreach (var attribute in AttributeList)
      {
        // Get new modifier for this attribute
        int newModifier = newSpeciesInfo?.GetModifier(attribute.Name) ?? 0;
        
        // Update the species modifier (this will trigger recalculation via business rule)
        attribute.UpdateSpeciesModifier(newModifier);
      }
      
      // Recalculate health pools with new attribute values
      BusinessRules.CheckRules(FatigueProperty);
      BusinessRules.CheckRules(VitalityProperty);
    }

    public void EndOfRound(IChildDataPortal<EffectRecord>? effectPortal = null)
    {
      Fatigue.EndOfRound();
      Vitality.EndOfRound(effectPortal);
      Effects.EndOfRound();
      ActionPoints.EndOfRound();
    }

    public void TakeDamage(DamageValue damageValue, IChildDataPortal<EffectRecord> effectPortal)
    {
      Fatigue.TakeDamage(damageValue);
      Vitality.TakeDamage(damageValue);
      Effects.TakeDamage(damageValue, effectPortal);
    }

    protected override void AddBusinessRules()
    {
      base.AddBusinessRules();
      BusinessRules.AddRule(new FatigueBase());
      BusinessRules.AddRule(new VitalityBase());
      BusinessRules.AddRule(new AttributeSumValidation());
    }

    [Create]
    [RunLocal]
    private void Create([Inject] ApplicationContext applicationContext,
      [Inject] IChildDataPortal<AttributeEditList> attributePortal,
      [Inject] IChildDataPortal<SkillEditList> skillPortal,
      [Inject] IChildDataPortal<EffectList> effectPortal,
      [Inject] IChildDataPortal<ActionPoints> actionPointsPortal,
      [Inject] IChildDataPortal<Fatigue> fatPortal,
      [Inject] IChildDataPortal<Vitality> vitPortal)
    {
      var ci = (System.Security.Claims.ClaimsIdentity?)applicationContext.User.Identity ?? 
        throw new InvalidOperationException("User not authenticated");
      var playerId = int.Parse(ci.Claims.Where(r => r.Type == ClaimTypes.NameIdentifier).First().Value);
      CreateInternal(playerId, null, attributePortal, skillPortal, effectPortal, actionPointsPortal, fatPortal, vitPortal);
    }

    [Create]
    [RunLocal]
    private void Create(int playerId, 
      [Inject] IChildDataPortal<AttributeEditList> attributePortal,
      [Inject] IChildDataPortal<SkillEditList> skillPortal,
      [Inject] IChildDataPortal<EffectList> effectPortal,
      [Inject] IChildDataPortal<ActionPoints> actionPointsPortal,
      [Inject] IChildDataPortal<Fatigue> fatPortal,
      [Inject] IChildDataPortal<Vitality> vitPortal)
    {
      CreateInternal(playerId, null, attributePortal, skillPortal, effectPortal, actionPointsPortal, fatPortal, vitPortal);
    }

    /// <summary>
    /// Creates a new character with species-specific attribute modifiers.
    /// </summary>
    /// <param name="playerId">The player ID.</param>
    /// <param name="species">The species information with attribute modifiers.</param>
    [Create]
    [RunLocal]
    private void Create(int playerId, Reference.SpeciesInfo species,
      [Inject] IChildDataPortal<AttributeEditList> attributePortal,
      [Inject] IChildDataPortal<SkillEditList> skillPortal,
      [Inject] IChildDataPortal<EffectList> effectPortal,
      [Inject] IChildDataPortal<ActionPoints> actionPointsPortal,
      [Inject] IChildDataPortal<Fatigue> fatPortal,
      [Inject] IChildDataPortal<Vitality> vitPortal)
    {
      CreateInternal(playerId, species, attributePortal, skillPortal, effectPortal, actionPointsPortal, fatPortal, vitPortal);
    }

    private void CreateInternal(int playerId, Reference.SpeciesInfo? species,
      IChildDataPortal<AttributeEditList> attributePortal,
      IChildDataPortal<SkillEditList> skillPortal,
      IChildDataPortal<EffectList> effectPortal,
      IChildDataPortal<ActionPoints> actionPointsPortal,
      IChildDataPortal<Fatigue> fatPortal,
      IChildDataPortal<Vitality> vitPortal)
    {
      using (BypassPropertyChecks)
      {
        DamageClass = 1;
        PlayerId = playerId;
        Species = species?.Id ?? "Human";
        
        // Apply species modifiers to attributes during creation
        if (species != null)
          AttributeList = attributePortal.CreateChild(species);
        else
          AttributeList = attributePortal.CreateChild();
        
        Skills = skillPortal.CreateChild();
        Effects = effectPortal.CreateChild();
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
        nameof(Effects),
        nameof(IsPassedOut),
        nameof(IsBeingSaved),
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
        nameof(Threa.Dal.Dto.Character.Items),
        nameof(Threa.Dal.Dto.Character.Effects),
        nameof(TotalCopperValue),
        nameof(Threa.Dal.Dto.Character.TotalCopperValue),
      ];

    [Fetch]
    private async Task FetchAsync(int id, [Inject] ICharacterDal dal,
      [Inject] IChildDataPortal<AttributeEditList> attributePortal,
      [Inject] IChildDataPortal<SkillEditList> skillPortal,
      [Inject] IChildDataPortal<Fatigue> fatPortal,
      [Inject] IChildDataPortal<Vitality> vitPortal,
      [Inject] IChildDataPortal<ActionPoints> actionPointsPortal,
      [Inject] IChildDataPortal<EffectList> effectPortal,
      [Inject] IDataPortal<Reference.SpeciesList> speciesPortal)
    {
      var existing = await dal.GetCharacterAsync(id);
      using (BypassPropertyChecks)
      {
        Csla.Data.DataMapper.Map(existing, this, mapIgnore);
        Fatigue = fatPortal.FetchChild(existing);
        Vitality = vitPortal.FetchChild(existing);
        ActionPoints = actionPointsPortal.FetchChild(existing);
        Effects = effectPortal.FetchChild(existing.Effects);
        
        // Load species info to pass modifiers to AttributeList
        var speciesList = await speciesPortal.FetchAsync();
        var speciesInfo = speciesList.FirstOrDefault(s => s.Id == existing.Species);
        AttributeList = attributePortal.FetchChild(existing.AttributeList, speciesInfo);
        
        Skills = skillPortal.FetchChild(existing.Skills);
      }
      BusinessRules.CheckRules();
    }

    [Insert]
    private async Task InsertAsync([Inject] ICharacterDal dal,
      [Inject] IChildDataPortal<AttributeEditList> attributePortal,
      [Inject] IChildDataPortal<SkillEditList> skillPortal,
      [Inject] IChildDataPortal<Fatigue> fatPortal,
      [Inject] IChildDataPortal<Vitality> vitPortal,
      [Inject] IChildDataPortal<ActionPoints> actionPointsPortal,
      [Inject] IChildDataPortal<EffectList> effectPortal)
    {
      var toSave = dal.GetBlank();
      using (BypassPropertyChecks)
      {
        Csla.Data.DataMapper.Map(this, toSave, mapIgnore);
        fatPortal.UpdateChild(Fatigue, toSave);
        vitPortal.UpdateChild(Vitality, toSave);
        actionPointsPortal.UpdateChild(ActionPoints, toSave);
        effectPortal.UpdateChild(Effects, toSave.Effects);
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
      [Inject] IChildDataPortal<Vitality> vitPortal,
      [Inject] IChildDataPortal<ActionPoints> actionPointsPortal,
      [Inject] IChildDataPortal<EffectList> effectPortal)
    {
      using (BypassPropertyChecks)
      {
        var existing = await dal.GetCharacterAsync(Id);
        Csla.Data.DataMapper.Map(this, existing, mapIgnore);
        fatPortal.UpdateChild(Fatigue, existing);
        vitPortal.UpdateChild(Vitality, existing);
        actionPointsPortal.UpdateChild(ActionPoints, existing);
        effectPortal.UpdateChild(Effects, existing.Effects);
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

    private class FatigueBase : PropertyRule
    {
        public FatigueBase() : base(AttributeListProperty)
        {
            InputProperties.Add(AttributeListProperty);
            AffectedProperties.Add(FatigueProperty);
        }

#pragma warning disable CSLA0017 // Find Business Rules That Do Not Use Add() Methods on the Context
      protected override void Execute(IRuleContext context)
#pragma warning restore CSLA0017 // Find Business Rules That Do Not Use Add() Methods on the Context
      {
            var target = (CharacterEdit)context.Target;
            target.Fatigue.CalculateBase(target);
        }
    }

    private class VitalityBase : PropertyRule
    {
      public VitalityBase() : base(AttributeListProperty)
      {
        InputProperties.Add(AttributeListProperty);
        AffectedProperties.Add(VitalityProperty);
      }

#pragma warning disable CSLA0017 // Find Business Rules That Do Not Use Add() Methods on the Context
      protected override void Execute(IRuleContext context)
#pragma warning restore CSLA0017 // Find Business Rules That Do Not Use Add() Methods on the Context
      {
        var target = (CharacterEdit)context.Target;
        target.Vitality.CalculateBase(target);
      }
    }

    private class AttributeSumValidation : BusinessRule
    {
      public AttributeSumValidation() : base(AttributeListProperty)
      {
        InputProperties.Add(AttributeListProperty);
      }

      protected override void Execute(IRuleContext context)
      {
        var target = (CharacterEdit)context.Target;
        
        // Only validate if character is not playable yet
        if (!target.IsPlayable)
        {
          var attributeList = target.AttributeList;
          if (attributeList.CurrentSum != attributeList.InitialSum)
          {
            context.AddErrorResult("The sum of attribute values must equal " + attributeList.InitialSum);
          }
        }
      }
    }
  }
}
