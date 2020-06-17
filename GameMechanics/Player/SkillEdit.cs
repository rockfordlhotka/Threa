using System;
using System.Collections.Generic;
using System.Linq;
using Csla;
using Threa.Dal;

namespace GameMechanics.Player
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

    [CreateChild]
    private void Create(Reference.SkillInfo std)
    {
      Id = std.Id;
      Name = std.Name;
      PrimaryAttribute = std.PrimaryAttribute;
    }

    [FetchChild]
    private void Fetch(ICharacterSkill skill)
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
    private void InsertUpdate(List<ICharacterSkill> skills)
    {
      using (BypassPropertyChecks)
      {
        ICharacterSkill skill;
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
    private void Delete(List<ICharacterSkill> skills)
    {
      if (IsNew) return;
      skills.Remove(skills.Where(r => r.Name == Name).First());
    }
  }
}
