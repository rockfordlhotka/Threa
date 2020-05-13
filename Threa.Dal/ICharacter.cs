using System;
using System.Collections.Generic;
using System.Reflection;

namespace Threa.Dal
{
  public interface ICharacter
  {
    string PlayerEmail { get; set; }
    string Id { get; set; }
    string Name { get; set; }
    string TrueName { get; set; }
    string Aliases { get; set; }
    string Species { get; set; }
    int DamageClass { get; set; }
    double Height { get; set; }
    double Weight { get; set; }
    string Notes { get; set; }
    string SkinDescription { get; set; }
    string HairDescription { get; set; }
    string Description { get; set; }
    double Birthdate { get; set; }
    bool IsPassedOut { get; set; }
    double XPTotal { get; set; }
    double XPBanked { get; set; }
    IAttribute Strength { get; set; }
    IAttribute Dexterity { get; set; }
    IAttribute Endurance { get; set; }
    IAttribute Intelligence { get; set; }
    IAttribute Intuition { get; set; }
    IAttribute Willpower { get; set; }
    IAttribute PhysicalBeauty { get; set; }
    IAttribute SocialStanding { get; set; }
    IAttribute Fatigue { get; set; }
    IAttribute Vitality { get; set; }
    List<IWound> Wounds { get; set; }
    List<ICharacterSkill> Skills { get; set; }
    IActionPoints ActionPoints { get; set; }
  }
}
