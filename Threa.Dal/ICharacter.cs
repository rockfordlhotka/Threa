using System.Collections.Generic;
using System.Reflection;

namespace Threa.Dal
{
  public interface ICharacter
  {
    string PlayerEmail { get; set; }
    string Id { get; set; }
    string Name { get; set; }
    int DamageClass { get; set; }
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
    List<ISkill> Skills { get; set; }
    IActionPoints ActionPoints { get; set; }
  }

  public interface IAttribute
  {
    int Value { get; set; }
    int BaseValue { get; set; }
  }

  public interface IWound
  {
    string Location { get; set; }
    int MaxWounds { get; set; }
    int LightWounds { get; set; }
    int SeriousWounds { get; set; }
    bool IsCrippled { get; set; }
    bool IsDestroyed { get; set; }
    int RoundsToDamage { get; set; }
  }

  public interface IActionPoints
  {
    int Max { get; set; }
    int Value { get; set; }
  }
}
