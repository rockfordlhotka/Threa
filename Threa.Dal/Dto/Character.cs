using System.Collections.Generic;

namespace Threa.Dal.Dto
{
  public class Character : ICharacter
  {
    public string PlayerEmail { get; set; }
    public string Id { get; set; }
    public string Name { get; set; }
    public int DamageClass { get; set; }
    public IAttribute Strength { get; set; }
    public IAttribute Dexterity { get; set; }
    public IAttribute Endurance { get; set; }
    public IAttribute Intelligence { get; set; }
    public IAttribute Intuition { get; set; }
    public IAttribute Willpower { get; set; }
    public IAttribute PhysicalBeauty { get; set; }
    public IAttribute SocialStanding { get; set; }
    public IAttribute Fatigue { get; set; }
    public IAttribute Vitality { get; set; }
    public List<IWound> Wounds { get; set; }
    public List<ISkill> Skills { get; set; }
    public IActionPoints ActionPoints { get; set; }
  }
}
