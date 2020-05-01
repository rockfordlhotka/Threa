using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Csla;
using Csla.Core;

namespace GameMechanics
{
  [Serializable]
  public class Skills : BusinessListBase<Skills, Skill>
  {
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

    public int Bonus
    {
      get => SkillCost.GetBonus(Level);
    }

    public int AbilityScore
    {
      get => GetAbilityScore();
    }

    private int GetAbilityScore()
    {
      var bonus = Bonus;
      var attributeBase = GetAttributeBase();
      return bonus + attributeBase;
    }

    private int GetAttributeBase()
    {
      var attributes = PrimaryAttribute.Split('/');
      int sum = 0;
      foreach (var item in attributes)
      {
        sum += ((Character)((IParent)Parent).Parent).GetAttribute(item);
      }
      return sum / attributes.Length;
    }

    [CreateChild]
    private void Create(double xp)
    {
      XPBanked = xp;
    }

    [FetchChild]
    private void Fetch(Threa.Dal.CharacterSkill skill)
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
