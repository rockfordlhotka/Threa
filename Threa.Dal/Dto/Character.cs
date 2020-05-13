using System;
using System.Collections.Generic;

namespace Threa.Dal.Dto
{
  public class Character : ICharacter
  {
    public string Id { get; set; }
    public string PlayerEmail { get; set; }
    public string Name { get; set; }
    public string TrueName { get; set; }
    public string Aliases { get; set; }
    public string Species { get; set; }
    public int DamageClass { get; set; }
    public double Height { get; set; }
    public double Weight { get; set; }
    public string Notes { get; set; }
    public string SkinDescription { get; set; }
    public string HairDescription { get; set; }
    public string Description { get; set; }
    public double Birthdate { get; set; }
    public bool IsPassedOut { get; set; }
    public double XPTotal { get; set; }
    public double XPBanked { get; set; }
    public IAttribute Strength { get; set; } = new CharacterAttribute();
    public IAttribute Dexterity { get; set; } = new CharacterAttribute();
    public IAttribute Endurance { get; set; } = new CharacterAttribute();
    public IAttribute Intelligence { get; set; } = new CharacterAttribute();
    public IAttribute Intuition { get; set; } = new CharacterAttribute();
    public IAttribute Willpower { get; set; } = new CharacterAttribute();
    public IAttribute PhysicalBeauty { get; set; } = new CharacterAttribute();
    public IAttribute SocialStanding { get; set; } = new CharacterAttribute();
    public IAttribute Fatigue { get; set; } = new CharacterAttribute();
    public IAttribute Vitality { get; set; } = new CharacterAttribute();
    public List<IWound> Wounds { get; set; } = new WoundList();
    public List<ICharacterSkill> Skills { get; set; } = new CharacterSkillList();
    public IActionPoints ActionPoints { get; set; } = new ActionPoints();
  }
}
