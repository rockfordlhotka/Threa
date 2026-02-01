using Csla;
using Csla.Core;
using Csla.Rules;
using GameMechanics.Effects.Behaviors;
using GameMechanics.Items;
using GameMechanics.Reference;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics
{
  [Serializable]
  public class CharacterEdit : BusinessBase<CharacterEdit>
  {
    public bool IsBeingSaved { get; set; }

    // Static calculator for item bonuses (stateless, thread-safe)
    private static readonly ItemBonusCalculator _itemBonusCalculator = new();

    // Non-CSLA property for equipped items (loaded on demand for bonus calculations)
    [NonSerialized]
    private List<EquippedItemInfo>? _equippedItems;

    /// <summary>
    /// Result from the last concentration effect that completed or was interrupted.
    /// Used by UI/controller to handle deferred actions like magazine reload.
    /// Cleared after processing.
    /// </summary>
    [NonSerialized]
    private ConcentrationCompletionResult? _lastConcentrationResult;

    /// <summary>
    /// Sets equipped items for bonus calculations. Call this after fetching character.
    /// </summary>
    /// <param name="items">Character items with templates populated.</param>
    public void SetEquippedItems(IEnumerable<CharacterItem> items)
    {
      _equippedItems = items
        .Where(i => i.Template != null)
        .Select(i => new EquippedItemInfo(i, i.Template!))
        .ToList();
    }

    /// <summary>
    /// Clears equipped items cache. Call when items are modified.
    /// </summary>
    public void ClearEquippedItems()
    {
      _equippedItems = null;
    }

    /// <summary>
    /// Result from the last concentration effect that completed or was interrupted.
    /// The UI/controller layer should read this after processing effect expiration/removal.
    /// </summary>
    public ConcentrationCompletionResult? LastConcentrationResult
    {
      get => _lastConcentrationResult;
      set => _lastConcentrationResult = value;
    }

    /// <summary>
    /// Clears the last concentration result after processing.
    /// Call this after the UI/controller has handled the result.
    /// </summary>
    public void ClearConcentrationResult()
    {
      _lastConcentrationResult = null;
    }

    protected override void OnChildChanged(ChildChangedEventArgs e)
    {
      if (!IsBeingSaved)
      {
        if (e.ChildObject is AttributeEdit)
        {
          BusinessRules.CheckRules(FatigueProperty);
          BusinessRules.CheckRules(VitalityProperty);
          OnPropertyChanged(FatigueProperty);
          OnPropertyChanged(VitalityProperty);
        }
        else if (e.ChildObject is SkillEdit)
        {
          BusinessRules.CheckRules(ActionPointsProperty);
          OnPropertyChanged(ActionPointsProperty);
        }
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

    /// <summary>
    /// Gets the active concentration effect on this character, if any.
    /// </summary>
    /// <returns>The concentration effect, or null if not concentrating</returns>
    public EffectRecord? GetConcentrationEffect()
    {
      foreach (var effect in Effects)
      {
        if (effect.EffectType == EffectType.Concentration)
          return effect;
      }
      return null;
    }

    /// <summary>
    /// Gets the type of concentration the character is performing.
    /// </summary>
    /// <returns>The concentration type (e.g., "MagazineReload", "SustainedSpell"), or null if not concentrating</returns>
    public string? GetConcentrationType()
    {
      var effect = GetConcentrationEffect();
      if (effect == null)
        return null;

      var state = ConcentrationState.FromJson(effect.BehaviorState);
      return state?.ConcentrationType;
    }

    /// <summary>
    /// Performs a concentration check against an attacker's AV.
    /// Called when the character uses passive defense while concentrating.
    /// </summary>
    /// <param name="attackerAV">The attacker's Attack Value (target value for the check)</param>
    /// <param name="damageDealt">Damage dealt by the attack (applies -1 per 2 damage penalty)</param>
    /// <param name="diceRoller">Dice roller for testability</param>
    /// <returns>Result of the concentration check</returns>
    public ConcentrationCheckResult CheckConcentration(int attackerAV, int damageDealt, IDiceRoller diceRoller)
    {
      // Check if actually concentrating
      if (GetConcentrationEffect() == null)
      {
        return new ConcentrationCheckResult
        {
          Success = true,  // Not concentrating, no check needed
          Reason = "Not concentrating"
        };
      }

      // Find Focus skill
      var focusSkill = Skills.FirstOrDefault(s =>
          s.Name.Equals("Focus", StringComparison.OrdinalIgnoreCase));

      if (focusSkill == null)
      {
        // No Focus skill - automatic failure
        ConcentrationBehavior.BreakConcentration(this, "No Focus skill - concentration broken");
        return new ConcentrationCheckResult
        {
          Success = false,
          Reason = "No Focus skill",
          TV = attackerAV
        };
      }

      // Calculate damage penalty: -1 per 2 damage dealt (round down)
      int damagePenalty = -(damageDealt / 2);

      // Calculate Focus AS (uses skill's AbilityScore property which includes attribute + bonus)
      int focusAS = focusSkill.AbilityScore + damagePenalty;

      // Roll 4dF+
      int roll = diceRoller.Roll4dFPlus();

      // Calculate result
      int result = focusAS + roll;
      bool success = result >= attackerAV;

      // Break concentration if failed
      if (!success)
      {
        string reason = $"Failed concentration check ({result} vs {attackerAV})";
        ConcentrationBehavior.BreakConcentration(this, reason);
      }

      return new ConcentrationCheckResult
      {
        Success = success,
        AS = focusAS,
        Roll = roll,
        Result = result,
        TV = attackerAV,
        DamagePenalty = damagePenalty,
        Reason = success ? null : "Check failed"
      };
    }

    /// <summary>
    /// Performs a concentration check against an attacker's AV using default dice roller.
    /// </summary>
    /// <param name="attackerAV">The attacker's Attack Value (target value for the check)</param>
    /// <param name="damageDealt">Damage dealt by the attack (applies -1 per 2 damage penalty)</param>
    /// <returns>Result of the concentration check</returns>
    public ConcentrationCheckResult CheckConcentration(int attackerAV, int damageDealt)
    {
      return CheckConcentration(attackerAV, damageDealt, new RandomDiceRoller());
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

    public static readonly PropertyInfo<long> CurrentGameTimeSecondsProperty = RegisterProperty<long>(nameof(CurrentGameTimeSeconds));
    /// <summary>
    /// Current game time in seconds from epoch 0.
    /// Updated when character processes time events (rounds or time skips).
    /// Used for epoch-based effect expiration.
    /// </summary>
    public long CurrentGameTimeSeconds
    {
      get => GetProperty(CurrentGameTimeSecondsProperty);
      set => SetProperty(CurrentGameTimeSecondsProperty, value);
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

    /// <summary>
    /// Dictionary tracking the original skill levels when the character was loaded or created.
    /// Used to determine which skill levels can be decreased during character creation.
    /// Key is skill name, value is the original level.
    /// Note: CSLA business objects are not thread-safe by design. This field should only be
    /// accessed from a single thread.
    /// </summary>
    private Dictionary<string, int> _originalSkillLevels = new Dictionary<string, int>();

    /// <summary>
    /// Gets the original skill levels when the character was loaded or created.
    /// Skills can be decreased to their original level during character creation (before activation).
    /// Note: CSLA business objects are not thread-safe. This property returns the internal dictionary
    /// as IReadOnlyDictionary for single-threaded use only.
    /// </summary>
    public IReadOnlyDictionary<string, int> OriginalSkillLevels => _originalSkillLevels;

    /// <summary>
    /// Captures the current skill levels as the baseline for future modifications.
    /// This method is private and called only during character initialization (Create/Fetch operations).
    /// Re-capturing after initialization would break the contract that original levels represent
    /// the state when the character was first loaded/created.
    /// </summary>
    private void CaptureOriginalSkillLevels()
    {
      _originalSkillLevels.Clear();
      foreach (var skill in Skills)
      {
        _originalSkillLevels[skill.Name] = skill.Level;
      }
    }

    public static readonly PropertyInfo<int> XPTotalProperty = RegisterProperty<int>(nameof(XPTotal));
    [Display(Name = "Total XP")]
    public int XPTotal
    {
      get => GetProperty(XPTotalProperty);
      private set => SetProperty(XPTotalProperty, value);
    }

    public static readonly PropertyInfo<int> XPBankedProperty = RegisterProperty<int>(nameof(XPBanked));
    [Display(Name = "Banked XP")]
    public int XPBanked
    {
      get => GetProperty(XPBankedProperty);
      set => SetProperty(XPBankedProperty, value);
    }

    /// <summary>
    /// Spends XP from the banked pool to advance a skill.
    /// Updates XPTotal to track cumulative spending.
    /// </summary>
    public void SpendXP(int amount)
    {
        if (amount > XPBanked)
            throw new InvalidOperationException("Insufficient XP");
        XPBanked -= amount;
        XPTotal += amount;
    }

    /// <summary>
    /// Refunds XP to the banked pool when a skill is de-leveled.
    /// Updates XPTotal to reverse cumulative spending.
    /// </summary>
    public void RefundXP(int amount)
    {
        XPBanked += amount;
        XPTotal -= amount;
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
      var result = AttributeList.Where(r => r.Name.Equals(attributeName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
      if (result == null)
        return 0;
      else
        return result.Value;
    }

    /// <summary>
    /// Gets the effective attribute value including item bonuses and effect modifiers.
    /// Use this for game mechanics calculations (combat, skills, etc.).
    /// Per CONTEXT.md: layered calculation - base + items + effects.
    /// </summary>
    /// <param name="attributeName">The attribute name (STR, DEX, END, INT, ITT, WIL, PHY).</param>
    /// <returns>The effective attribute value after all modifiers.</returns>
    public int GetEffectiveAttribute(string attributeName)
    {
      var baseValue = GetAttribute(attributeName);

      // Layer 1: Item bonuses (per CONTEXT.md layered calculation)
      var itemBonus = _equippedItems != null
        ? _itemBonusCalculator.GetAttributeBonus(_equippedItems, attributeName)
        : 0;

      // Layer 2: Effect modifiers (applied after item bonuses)
      var effectModifier = Effects.GetAttributeModifier(attributeName, baseValue + itemBonus);

      return baseValue + itemBonus + effectModifier;
    }

    /// <summary>
    /// Gets detailed breakdown of attribute value including item bonuses.
    /// </summary>
    /// <param name="attributeName">The attribute name (STR, DEX, etc.).</param>
    /// <returns>Breakdown showing base value, item bonus, and effect bonus.</returns>
    public AttributeBonusBreakdown GetAttributeBreakdown(string attributeName)
    {
      var baseValue = GetAttribute(attributeName);
      var itemBonus = _equippedItems != null
        ? _itemBonusCalculator.GetAttributeBonus(_equippedItems, attributeName)
        : 0;
      var effectBonus = Effects.GetAttributeModifier(attributeName, baseValue + itemBonus);

      return new AttributeBonusBreakdown
      {
        AttributeName = attributeName,
        BaseValue = baseValue,
        ItemBonus = itemBonus,
        EffectBonus = effectBonus
      };
    }

    /// <summary>
    /// Gets detailed breakdown of item bonuses for an attribute.
    /// </summary>
    /// <param name="attributeName">The attribute name (STR, DEX, etc.).</param>
    /// <returns>List of (ItemName, Bonus) tuples for each contributing item.</returns>
    public List<(string ItemName, int Bonus)> GetAttributeItemBreakdown(string attributeName)
    {
      return _equippedItems != null
        ? _itemBonusCalculator.GetAttributeBonusBreakdown(_equippedItems, attributeName)
        : new List<(string, int)>();
    }

    /// <summary>
    /// Gets the item skill bonus for a specific skill.
    /// Used when calculating Ability Scores: AS = Attribute + SkillLevel + ItemSkillBonus - 5 + Modifiers
    /// </summary>
    /// <param name="skillName">The skill name to get bonus for.</param>
    /// <returns>Total skill bonus from equipped items.</returns>
    public int GetSkillItemBonus(string skillName)
    {
      return _equippedItems != null
        ? _itemBonusCalculator.GetSkillBonus(_equippedItems, skillName)
        : 0;
    }

    /// <summary>
    /// Gets detailed breakdown of skill bonuses from each equipped item.
    /// </summary>
    /// <param name="skillName">The skill name to get breakdown for.</param>
    /// <returns>List of (ItemName, Bonus) tuples for each contributing item.</returns>
    public List<(string ItemName, int Bonus)> GetSkillItemBreakdown(string skillName)
    {
      return _equippedItems != null
        ? _itemBonusCalculator.GetSkillBonusBreakdown(_equippedItems, skillName)
        : new List<(string, int)>();
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

    /// <summary>
    /// Processes end-of-round effects. Advances game time by 3 seconds (1 round).
    /// </summary>
    public void EndOfRound(IChildDataPortal<EffectRecord>? effectPortal = null)
    {
      // Advance game time by 1 round (3 seconds)
      CurrentGameTimeSeconds += 3;

      Fatigue.EndOfRound();
      Vitality.EndOfRound(effectPortal);
      Effects.EndOfRound(CurrentGameTimeSeconds);
      ActionPoints.EndOfRound();
    }

    /// <summary>
    /// Processes a time skip (calendar time advancement like minutes, hours, days, weeks).
    /// Applies hourly VIT recovery, VIT-level-dependent FAT recovery, effect expiration, and pending pool flow.
    /// Uses epoch-based time tracking for O(1) effect expiration performance.
    /// </summary>
    public void ProcessTimeSkip(GameMechanics.Time.TimeEventType skipUnit, int count, IChildDataPortal<EffectRecord>? effectPortal = null)
    {
      // Calculate total seconds passed
      long totalSecondsPassed = skipUnit switch
      {
        GameMechanics.Time.TimeEventType.EndOfMinute => count * 60L,           // 1 min = 60 seconds
        GameMechanics.Time.TimeEventType.EndOfTurn => count * 600L,            // 10 min = 600 seconds
        GameMechanics.Time.TimeEventType.EndOfHour => count * 3600L,           // 1 hour = 3600 seconds
        GameMechanics.Time.TimeEventType.EndOfDay => count * 86400L,           // 1 day = 86400 seconds
        GameMechanics.Time.TimeEventType.EndOfWeek => count * 604800L,         // 1 week = 604800 seconds
        _ => 0
      };

      // Advance game time by the full time skip
      CurrentGameTimeSeconds += totalSecondsPassed;

      // Calculate total hours passed
      double hoursPassedDecimal = totalSecondsPassed / 3600.0;
      int hoursPassed = (int)Math.Floor(hoursPassedDecimal);

      // VIT Recovery: 1 VIT per hour when alive (VIT > 0)
      // Per GAME_RULES_SPECIFICATION.md line 162
      if (Vitality.Value > 0 && hoursPassed > 0)
      {
        Vitality.PendingHealing += hoursPassed;
      }

      // FAT Recovery: Rate depends on current VIT level
      // Per design/GAME_RULES_SPECIFICATION.md and Fatigue.cs comments:
      // VIT >= 5: 1 per round (handled in combat via EndOfRound)
      // VIT = 4: 1 per minute
      // VIT = 3: 1 per 30 minutes
      // VIT = 2: 1 per hour
      // VIT <= 1: No recovery
      if (Fatigue.Value < Fatigue.BaseValue)
      {
        int fatRecovery = Vitality.Value switch
        {
          4 => (int)Math.Floor(count * GetMinutesForTimeSkip(skipUnit)),        // 1 per minute
          3 => (int)Math.Floor(count * GetMinutesForTimeSkip(skipUnit) / 30.0),  // 1 per 30 minutes
          2 => hoursPassed,                                                       // 1 per hour
          _ => 0  // VIT <= 1 or VIT >= 5 (VIT >= 5 handles in combat only)
        };

        if (fatRecovery > 0)
        {
          Fatigue.PendingHealing += fatRecovery;
        }
      }

      // Process effect expiration using epoch time (O(1) per effect - no loops!)
      // With 236 wounds and 1 week skip, this is now instant instead of 23M+ operations
      Effects.ProcessTimeSkip(CurrentGameTimeSeconds);

      // Process pending pools and AP recovery multiple times to simulate gradual healing
      // Cap at 100 iterations to avoid performance issues
      int totalRoundsPassed = (int)(totalSecondsPassed / 3);  // Each round = 3 seconds
      int iterations = Math.Min(totalRoundsPassed, 100);
      for (int i = 0; i < iterations; i++)
      {
        // Tick effects first to apply wound damage and other periodic effects
        Effects.EndOfRound(CurrentGameTimeSeconds);
        Fatigue.EndOfRound();
        Vitality.EndOfRound(effectPortal);
        ActionPoints.EndOfRound();
      }

      // Final pass to ensure all pending pools are fully resolved
      if (totalRoundsPassed > 0)
      {
        Fatigue.EndOfRound();
        Vitality.EndOfRound(effectPortal);
        ActionPoints.EndOfRound();
      }
    }

    private double GetMinutesForTimeSkip(GameMechanics.Time.TimeEventType skipUnit)
    {
      return skipUnit switch
      {
        GameMechanics.Time.TimeEventType.EndOfMinute => 1,
        GameMechanics.Time.TimeEventType.EndOfTurn => 10,
        GameMechanics.Time.TimeEventType.EndOfHour => 60,
        GameMechanics.Time.TimeEventType.EndOfDay => 60 * 24,
        GameMechanics.Time.TimeEventType.EndOfWeek => 60 * 24 * 7,
        _ => 0
      };
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
      BusinessRules.AddRule(new ActionPointsMax());
      BusinessRules.AddRule(new ActionPointsRecovery());
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
      
      // Capture the baseline skill levels for this new character
      CaptureOriginalSkillLevels();
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
        nameof(LastConcentrationResult),
        nameof(OriginalSkillLevels),
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
      
      // Capture the baseline skill levels when loading an existing character
      CaptureOriginalSkillLevels();
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
    private async Task DeleteAsync(int id, [Inject] ICharacterDal dal)
    {
      await dal.DeleteCharacterAsync(id);
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

    private class ActionPointsMax : PropertyRule
    {
      public ActionPointsMax() : base(SkillsProperty)
      {
        InputProperties.Add(SkillsProperty);
        AffectedProperties.Add(ActionPointsProperty);
      }

#pragma warning disable CSLA0017 // Find Business Rules That Do Not Use Add() Methods on the Context
      protected override void Execute(IRuleContext context)
#pragma warning restore CSLA0017 // Find Business Rules That Do Not Use Add() Methods on the Context
      {
        var target = (CharacterEdit)context.Target;
        target.ActionPoints.RecalculateMax(target);
      }
    }

    private class ActionPointsRecovery : PropertyRule
    {
      public ActionPointsRecovery() : base(FatigueProperty)
      {
        InputProperties.Add(FatigueProperty);
        AffectedProperties.Add(ActionPointsProperty);
      }

#pragma warning disable CSLA0017 // Find Business Rules That Do Not Use Add() Methods on the Context
      protected override void Execute(IRuleContext context)
#pragma warning restore CSLA0017 // Find Business Rules That Do Not Use Add() Methods on the Context
      {
        var target = (CharacterEdit)context.Target;
        target.ActionPoints.RecalculateRecovery(target);
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

  /// <summary>
  /// Result of a concentration check when defending passively while concentrating.
  /// </summary>
  public class ConcentrationCheckResult
  {
    /// <summary>
    /// True if concentration was maintained, false if broken.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The Ability Score used (Focus AS with damage penalty applied).
    /// </summary>
    public int AS { get; set; }

    /// <summary>
    /// The 4dF+ roll result.
    /// </summary>
    public int Roll { get; set; }

    /// <summary>
    /// The total result (AS + Roll).
    /// </summary>
    public int Result { get; set; }

    /// <summary>
    /// The Target Value (attacker's AV).
    /// </summary>
    public int TV { get; set; }

    /// <summary>
    /// The damage penalty applied (-1 per 2 damage).
    /// </summary>
    public int DamagePenalty { get; set; }

    /// <summary>
    /// Reason for failure (if Success is false).
    /// </summary>
    public string? Reason { get; set; }
  }
}
