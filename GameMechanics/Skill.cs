using System;
using System.Linq;
using Csla;
using Csla.Core;
using GameMechanics.Reference;

namespace GameMechanics
{
  [Serializable]
  public class SkillList : BusinessListBase<SkillList, Skill>
  {
    public Reference.ResultValue SkillCheck(string skillName, int targetValue)
    {
      var skill = this.Where(r => r.Name == skillName).FirstOrDefault();
      if (skill == null)
      {
        var skillTemplate = Reference.SkillList.GetList().Where(r=>r.Name == skillName).FirstOrDefault();
        if (skillTemplate == null)
        {
          return ResultValues.GetResult(-10);
        }
        else
        {
          var baseAS = Skill.GetAttributeBase((Character)Parent, skillTemplate.PrimaryAttribute);
          return ResultValues.GetResult(Dice.Roll4dFWithBonus() + baseAS);
        }
      }
      else
      {
        return skill.SkillCheck();
      }
    }

    [CreateChild]
    private void Create()
    {
      var std = Reference.SkillList.GetList().Where(r => r.IsStandard);
      foreach (var item in std)
      {
        Add(DataPortal.CreateChild<Skill>(item));
      }
    }
  }

  [Serializable]
  public class Skill : BusinessBase<Skill>
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
      private set => LoadProperty(LevelProperty, value);
    }

    public static readonly PropertyInfo<string> PrimaryAttributeProperty = RegisterProperty<string>(nameof(PrimaryAttribute));
    public string PrimaryAttribute
    {
      get => GetProperty(PrimaryAttributeProperty);
      private set => LoadProperty(PrimaryAttributeProperty, value);
    }

    public static readonly PropertyInfo<double> XPBankedProperty = RegisterProperty<double>(nameof(XPBanked));
    public double XPBanked
    {
      get => GetProperty(XPBankedProperty);
      internal set => SetProperty(XPBankedProperty, value);
    }

    public ResultValue SkillCheck()
    {
      return ResultValues.GetResult(Dice.Roll4dFWithBonus() + AbilityScore);
    }

    public int Bonus
    {
      get => SkillCost.GetBonus(Level);
    }

    public int AbilityScore
    {
      get => Bonus + GetAttributeBase((Character)((IParent)Parent).Parent, PrimaryAttribute);
    }

    public static int GetAttributeBase(Character character, string primaryAttribute)
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

    [CreateChild]
    private void Create(Reference.SkillInfo std)
    {
      Id = std.Id;
      Name = std.Name;
      PrimaryAttribute = std.PrimaryAttribute;
    }

    [FetchChild]
    private void Fetch(Threa.Dal.ICharacterSkill skill)
    {
      using (BypassPropertyChecks)
      {
        Id = skill.Id;
        Name = skill.Name;
        Level = skill.Level;
        XPBanked = skill.XPBanked;
        PrimaryAttribute = skill.PrimarySkill;
      }
    }
  }
}
