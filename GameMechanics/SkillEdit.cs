using System;
using System.Collections.Generic;
using System.Linq;
using Csla;
using Csla.Core;
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

    public static readonly PropertyInfo<double> XPBankedProperty = RegisterProperty<double>(nameof(XPBanked));
    public double XPBanked
    {
      get => GetProperty(XPBankedProperty);
      set => SetProperty(XPBankedProperty, value);
    }

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
          skill = skills.Where(r => r.Name == Name).First();
        }

        skill.Id = Id;
        skill.Name = Name;
        skill.Level = Level;
        skill.XPBanked = XPBanked;
        skill.PrimaryAttribute = PrimaryAttribute;
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
