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

    public static readonly PropertyInfo<int> XPBankedProperty = RegisterProperty<int>(nameof(XPBanked));
    public int XPBanked
    {
      get => GetProperty(XPBankedProperty);
      set => SetProperty(XPBankedProperty, value);
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
      get => Bonus + GetAttributeBase((CharacterEdit)((IParent)Parent).Parent, PrimaryAttribute);
    }

    /// <summary>
    /// Gets the base attribute value for the skill, accounting for 
    /// low FAT/VIT penalties.
    /// </summary>
    public static int GetAttributeBase(CharacterEdit character, string primaryAttribute)
    {
      var attributes = primaryAttribute.Split('/');
      int sum = 0;
      foreach (var item in attributes)
      {
        sum += character.GetAttribute(item);
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
    /// Gets the raw attribute value without FAT/VIT penalties.
    /// Used by the action system which applies penalties separately.
    /// </summary>
    public static int GetRawAttributeValue(CharacterEdit character, string primaryAttribute)
    {
      var attributes = primaryAttribute.Split('/');
      int sum = 0;
      foreach (var item in attributes)
      {
        sum += character.GetAttribute(item);
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
        PrimaryAttribute = skill.PrimaryAttribute;

        // Load action system properties
        ActionType = skill.ActionType;
        TargetValueType = skill.TargetValueType;
        DefaultTV = skill.DefaultTV;
        OpposedSkillId = skill.OpposedSkillId ?? string.Empty;
        ResultTable = skill.ResultTable;
        AppliesPhysicalityBonus = skill.AppliesPhysicalityBonus;
        RequiresTarget = skill.RequiresTarget;
        IsFreeAction = skill.IsFreeAction;
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
          skill = skills.Where(r => r.Name == Name).FirstOrDefault();
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
        skill.PrimaryAttribute = PrimaryAttribute;

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
