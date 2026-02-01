using System;
using System.Collections.Generic;
using System.Linq;
using Csla;
using Csla.Core;
using GameMechanics.Actions;
using GameMechanics.Reference;
using Threa.Dal.Dto;

namespace GameMechanics
{
  [Serializable]
  public class SkillEdit : BusinessBase<SkillEdit>
  {
    public static readonly PropertyInfo<string> IdProperty = RegisterProperty<string>(nameof(Id));
    public string Id
    {
      get => GetProperty(IdProperty);
      private set => LoadProperty(IdProperty, value);
    }

    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    public string Name
    {
      get => GetProperty(NameProperty);
      private set => LoadProperty(NameProperty, value);
    }

    public static readonly PropertyInfo<int> LevelProperty = RegisterProperty<int>(nameof(Level));
    public int Level
    {
      get => GetProperty(LevelProperty);
      set => SetProperty(LevelProperty, value);
    }

    public static readonly PropertyInfo<string> PrimaryAttributeProperty = RegisterProperty<string>(nameof(PrimaryAttribute));
    public string PrimaryAttribute
    {
      get => GetProperty(PrimaryAttributeProperty);
      private set => LoadProperty(PrimaryAttributeProperty, value);
    }

    public static readonly PropertyInfo<string?> SecondaryAttributeProperty = RegisterProperty<string?>(nameof(SecondaryAttribute));
    /// <summary>
    /// Secondary attribute - character must have current value >= 8 to use this skill.
    /// </summary>
    public string? SecondaryAttribute
    {
      get => GetProperty(SecondaryAttributeProperty);
      private set => LoadProperty(SecondaryAttributeProperty, value);
    }

    public static readonly PropertyInfo<string?> TertiaryAttributeProperty = RegisterProperty<string?>(nameof(TertiaryAttribute));
    /// <summary>
    /// Tertiary attribute - character must have current value >= 6 to use this skill.
    /// </summary>
    public string? TertiaryAttribute
    {
      get => GetProperty(TertiaryAttributeProperty);
      private set => LoadProperty(TertiaryAttributeProperty, value);
    }

    public static readonly PropertyInfo<int> XPBankedProperty = RegisterProperty<int>(nameof(XPBanked));
    public int XPBanked
    {
      get => GetProperty(XPBankedProperty);
      set => SetProperty(XPBankedProperty, value);
    }

    public static readonly PropertyInfo<int> XPSpentProperty = RegisterProperty<int>(nameof(XPSpent));
    /// <summary>
    /// Total XP spent to reach current skill level.
    /// Used to track XP allocation during character creation.
    /// </summary>
    public int XPSpent
    {
      get => GetProperty(XPSpentProperty);
      set => SetProperty(XPSpentProperty, value);
    }

    // === Action System Properties ===

    public static readonly PropertyInfo<ActionType> ActionTypeProperty = RegisterProperty<ActionType>(nameof(ActionType));
    /// <summary>
    /// The type of action this skill performs when used.
    /// </summary>
    public ActionType ActionType
    {
      get => GetProperty(ActionTypeProperty);
      private set => LoadProperty(ActionTypeProperty, value);
    }

    public static readonly PropertyInfo<TargetValueType> TargetValueTypeProperty = RegisterProperty<TargetValueType>(nameof(TargetValueType));
    /// <summary>
    /// How Target Value is determined for this skill.
    /// </summary>
    public TargetValueType TargetValueType
    {
      get => GetProperty(TargetValueTypeProperty);
      private set => LoadProperty(TargetValueTypeProperty, value);
    }

    public static readonly PropertyInfo<int> DefaultTVProperty = RegisterProperty<int>(nameof(DefaultTV));
    /// <summary>
    /// Default fixed TV for this skill (if TargetValueType is Fixed).
    /// </summary>
    public int DefaultTV
    {
      get => GetProperty(DefaultTVProperty);
      private set => LoadProperty(DefaultTVProperty, value);
    }

    public static readonly PropertyInfo<string> OpposedSkillIdProperty = RegisterProperty<string>(nameof(OpposedSkillId));
    /// <summary>
    /// The skill ID used by opponents to oppose this action.
    /// </summary>
    public string OpposedSkillId
    {
      get => GetProperty(OpposedSkillIdProperty);
      private set => LoadProperty(OpposedSkillIdProperty, value);
    }

    public static readonly PropertyInfo<ResultTableType> ResultTableProperty = RegisterProperty<ResultTableType>(nameof(ResultTable));
    /// <summary>
    /// Which result table to use for interpreting SV.
    /// </summary>
    public ResultTableType ResultTable
    {
      get => GetProperty(ResultTableProperty);
      private set => LoadProperty(ResultTableProperty, value);
    }

    public static readonly PropertyInfo<bool> AppliesPhysicalityBonusProperty = RegisterProperty<bool>(nameof(AppliesPhysicalityBonus));
    /// <summary>
    /// Whether this skill applies the Physicality damage bonus on success.
    /// </summary>
    public bool AppliesPhysicalityBonus
    {
      get => GetProperty(AppliesPhysicalityBonusProperty);
      private set => LoadProperty(AppliesPhysicalityBonusProperty, value);
    }

    public static readonly PropertyInfo<bool> RequiresTargetProperty = RegisterProperty<bool>(nameof(RequiresTarget));
    /// <summary>
    /// Whether this skill requires a target entity.
    /// </summary>
    public bool RequiresTarget
    {
      get => GetProperty(RequiresTargetProperty);
      private set => LoadProperty(RequiresTargetProperty, value);
    }

    public static readonly PropertyInfo<bool> IsFreeActionProperty = RegisterProperty<bool>(nameof(IsFreeAction));
    /// <summary>
    /// Whether this skill is a free action (no AP/FAT cost).
    /// </summary>
    public bool IsFreeAction
    {
      get => GetProperty(IsFreeActionProperty);
      private set => LoadProperty(IsFreeActionProperty, value);
    }

    public static readonly PropertyInfo<SkillCategory> CategoryProperty = RegisterProperty<SkillCategory>(nameof(Category));
    /// <summary>
    /// Category classification for UI filtering and display logic.
    /// </summary>
    public SkillCategory Category
    {
      get => GetProperty(CategoryProperty);
      private set => LoadProperty(CategoryProperty, value);
    }

    public static readonly PropertyInfo<bool> IsMagicProperty = RegisterProperty<bool>(nameof(IsMagic));
    /// <summary>
    /// Whether this is a magic skill.
    /// </summary>
    public bool IsMagic
    {
      get => GetProperty(IsMagicProperty);
      private set => LoadProperty(IsMagicProperty, value);
    }

    public static readonly PropertyInfo<bool> IsTheologyProperty = RegisterProperty<bool>(nameof(IsTheology));
    /// <summary>
    /// Whether this is a theology/divine skill.
    /// </summary>
    public bool IsTheology
    {
      get => GetProperty(IsTheologyProperty);
      private set => LoadProperty(IsTheologyProperty, value);
    }

    public static readonly PropertyInfo<bool> IsPsionicProperty = RegisterProperty<bool>(nameof(IsPsionic));
    /// <summary>
    /// Whether this is a psionic skill.
    /// </summary>
    public bool IsPsionic
    {
      get => GetProperty(IsPsionicProperty);
      private set => LoadProperty(IsPsionicProperty, value);
    }

    /// <summary>
    /// Whether this is a spell skill (magic, theology, or psionic).
    /// </summary>
    public bool IsSpell => IsMagic || IsTheology || IsPsionic;

    /// <summary>
    /// Whether this is a combat skill.
    /// </summary>
    public bool IsCombatSkill => Category == SkillCategory.Combat;

    /// <summary>
    /// Whether this is a mana channeling/gathering skill.
    /// </summary>
    public bool IsManaSkill => Category == SkillCategory.Mana;

    // === Attribute Requirement Constants ===

    /// <summary>
    /// Minimum attribute value required for secondary attribute.
    /// </summary>
    public const int SecondaryAttributeMinimum = 8;

    /// <summary>
    /// Minimum attribute value required for tertiary attribute.
    /// </summary>
    public const int TertiaryAttributeMinimum = 6;

    // === Skill Usage Validation ===

    /// <summary>
    /// Whether this skill can be used based on attribute requirements.
    /// Returns false if secondary attribute is below 8 or tertiary is below 6.
    /// Handles compound attributes (e.g., "STR/ITT") by averaging them.
    /// </summary>
    public bool CanUse
    {
      get
      {
        var character = GetCharacter();
        if (character == null) return true; // No character context, assume usable

        // Check secondary attribute requirement (>= 8)
        if (!string.IsNullOrWhiteSpace(SecondaryAttribute))
        {
          var secondaryValue = GetEffectiveAttributeValue(character, SecondaryAttribute);
          if (secondaryValue < SecondaryAttributeMinimum)
            return false;
        }

        // Check tertiary attribute requirement (>= 6)
        if (!string.IsNullOrWhiteSpace(TertiaryAttribute))
        {
          var tertiaryValue = GetEffectiveAttributeValue(character, TertiaryAttribute);
          if (tertiaryValue < TertiaryAttributeMinimum)
            return false;
        }

        return true;
      }
    }

    /// <summary>
    /// Explanation of why this skill cannot be used, or null if it can be used.
    /// Handles compound attributes (e.g., "STR/ITT") by averaging them.
    /// </summary>
    public string? CannotUseReason
    {
      get
      {
        var character = GetCharacter();
        if (character == null) return null;

        var reasons = new List<string>();

        // Check secondary attribute requirement (>= 8)
        if (!string.IsNullOrWhiteSpace(SecondaryAttribute))
        {
          var secondaryValue = GetEffectiveAttributeValue(character, SecondaryAttribute);
          if (secondaryValue < SecondaryAttributeMinimum)
            reasons.Add($"Requires {SecondaryAttribute} >= {SecondaryAttributeMinimum}, current: {secondaryValue}");
        }

        // Check tertiary attribute requirement (>= 6)
        if (!string.IsNullOrWhiteSpace(TertiaryAttribute))
        {
          var tertiaryValue = GetEffectiveAttributeValue(character, TertiaryAttribute);
          if (tertiaryValue < TertiaryAttributeMinimum)
            reasons.Add($"Requires {TertiaryAttribute} >= {TertiaryAttributeMinimum}, current: {tertiaryValue}");
        }

        return reasons.Count > 0 ? string.Join("; ", reasons) : null;
      }
    }

    /// <summary>
    /// Gets the parent character, or null if not available.
    /// </summary>
    private CharacterEdit? GetCharacter()
    {
      try
      {
        return (CharacterEdit)((IParent)Parent).Parent;
      }
      catch
      {
        return null;
      }
    }

    /// <summary>
    /// Gets the effective attribute value, handling compound attributes (e.g., "STR/ITT").
    /// For compound attributes, returns the average of the constituent attributes.
    /// </summary>
    /// <param name="character">The character to get the attribute from.</param>
    /// <param name="attributeName">The attribute name, which may be a compound attribute like "STR/ITT".</param>
    /// <returns>The effective attribute value, or the average if compound.</returns>
    private static int GetEffectiveAttributeValue(CharacterEdit character, string attributeName)
    {
      var attributes = attributeName.Split('/');
      int sum = 0;
      foreach (var attr in attributes)
      {
        sum += character.GetEffectiveAttribute(attr);
      }
      return sum / attributes.Length;
    }

    // === Existing Skill Check Logic ===

    public ResultValue SkillCheck()
    {
      return ResultValues.GetResult(Dice.Roll4dFPlus() + AbilityScore);
    }

    public int Bonus
    {
      get => SkillCost.GetBonus(Level);
    }

    public int AbilityScore
    {
      get
      {
        var character = (CharacterEdit)((IParent)Parent).Parent;
        var baseAS = Bonus + GetAttributeBase(character, PrimaryAttribute);

        // Apply ability score modifiers from active effects (wounds, debuffs, etc.)
        var effectModifier = character.Effects.GetAbilityScoreModifier(Name, PrimaryAttribute, baseAS);

        return baseAS + effectModifier;
      }
    }

    /// <summary>
    /// Gets the base attribute value for the skill, accounting for
    /// item bonuses, effect attribute modifiers, and low FAT/VIT penalties.
    /// </summary>
    public static int GetAttributeBase(CharacterEdit character, string primaryAttribute)
    {
      var attributes = primaryAttribute.Split('/');
      int sum = 0;
      foreach (var item in attributes)
      {
        // Use GetEffectiveAttribute to include item bonuses and effect attribute modifiers
        sum += character.GetEffectiveAttribute(item);
      }
      var result = sum / attributes.Length;
      if (character.Fatigue.Value < 1)
        result = 0;
      else if (character.Fatigue.Value < 2)
        result -= 4;
      else if (character.Fatigue.Value < 4)
        result -= 2;
      else if (character.Fatigue.Value < 6)
        result -= 1;
      if (character.Vitality.Value < 4)
        result -= 6;
      else if (character.Fatigue.Value < 6)
        result -= 4;
      return result;
    }

    /// <summary>
    /// Gets the effective attribute value without FAT/VIT penalties.
    /// Includes item bonuses and effect attribute modifiers.
    /// Used by the action system which applies penalties separately.
    /// </summary>
    public static int GetRawAttributeValue(CharacterEdit character, string primaryAttribute)
    {
      var attributes = primaryAttribute.Split('/');
      int sum = 0;
      foreach (var item in attributes)
      {
        // Use GetEffectiveAttribute to include item bonuses and effect attribute modifiers
        sum += character.GetEffectiveAttribute(item);
      }
      return sum / attributes.Length;
    }

    // === CSLA Data Access ===

    [CreateChild]
    private void Create(Reference.SkillInfo std)
    {
      Id = std.Id;
      Name = std.Name;
      PrimaryAttribute = std.PrimaryAttribute;
      SecondaryAttribute = std.SecondaryAttribute;
      TertiaryAttribute = std.TertiaryAttribute;
    }

    [FetchChild]
    private void Fetch(CharacterSkill skill)
    {
      using (BypassPropertyChecks)
      {
        Id = skill.Id;
        Name = skill.Name;
        Level = skill.Level;
        XPBanked = skill.XPBanked;
        XPSpent = skill.XPSpent;
        
        // Note: For backwards compatibility with existing characters that don't have XPSpent set,
        // we would need access to the skill's difficulty (Trained value) which isn't available here.
        // The UI will need to calculate and update XPSpent when the character is next saved.
        
        PrimaryAttribute = skill.PrimaryAttribute;
        SecondaryAttribute = skill.SecondaryAttribute;
        TertiaryAttribute = skill.TertiaryAttribute;

        // Load action system properties
        ActionType = skill.ActionType;
        TargetValueType = skill.TargetValueType;
        DefaultTV = skill.DefaultTV;
        OpposedSkillId = skill.OpposedSkillId ?? string.Empty;
        ResultTable = skill.ResultTable;
        AppliesPhysicalityBonus = skill.AppliesPhysicalityBonus;
        RequiresTarget = skill.RequiresTarget;
        IsFreeAction = skill.IsFreeAction;

        // Load magic/category properties
        Category = skill.Category;
        IsMagic = skill.IsMagic;
        IsTheology = skill.IsTheology;
        IsPsionic = skill.IsPsionic;
      }
    }

    [InsertChild]
    [UpdateChild]
    private void InsertUpdate(List<CharacterSkill> skills)
    {
      using (BypassPropertyChecks)
      {
        CharacterSkill skill;
        if (IsNew)
        {
          skill = new Threa.Dal.Dto.CharacterSkill();
          skills.Add(skill);
        }
        else
        {
          skill = skills.Where(r => r.Name == Name).FirstOrDefault()!;
          if (skill == null)
          {
            skill = new Threa.Dal.Dto.CharacterSkill();
            skills.Add(skill);
          }
        }

        skill.Id = Id;
        skill.Name = Name;
        skill.Level = Level;
        skill.XPBanked = XPBanked;
        skill.XPSpent = XPSpent;
        skill.PrimaryAttribute = PrimaryAttribute;
        skill.SecondaryAttribute = SecondaryAttribute;
        skill.TertiaryAttribute = TertiaryAttribute;

        // Save action system properties
        skill.ActionType = ActionType;
        skill.TargetValueType = TargetValueType;
        skill.DefaultTV = DefaultTV;
        skill.OpposedSkillId = OpposedSkillId;
        skill.ResultTable = ResultTable;
        skill.AppliesPhysicalityBonus = AppliesPhysicalityBonus;
        skill.RequiresTarget = RequiresTarget;
        skill.IsFreeAction = IsFreeAction;
      }
    }

    [DeleteSelfChild]
    private void Delete(List<CharacterSkill> skills)
    {
      if (IsNew) return;
      skills.Remove(skills.Where(r => r.Name == Name).First());
    }
  }
}
