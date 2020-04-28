using System;
using Csla;

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

    public static readonly PropertyInfo<int> LevelProperty = RegisterProperty<int>(nameof(Level));
    public int Level
    {
      get => GetProperty(LevelProperty);
      private set => LoadProperty(LevelProperty, value);
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
  }
}
