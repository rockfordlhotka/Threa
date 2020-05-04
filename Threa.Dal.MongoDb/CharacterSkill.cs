namespace Threa.Dal.MongoDb
{
  public class CharacterSkill : Skill, ICharacterSkill
  {
    public int Level { get; set; }
    public double XPBanked { get; set; }
  }
}
