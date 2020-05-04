namespace Threa.Dal.MockDb
{
  public class CharacterSkill : Skill, ICharacterSkill
  {
    public int Level { get; set; }
    public double XPBanked { get; set; }
  }
}
