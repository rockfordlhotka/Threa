namespace Threa.Dal
{
  public interface ICharacterSkill : ISkill
  {
    int Level { get; set; }
    double XPBanked { get; set; }
  }
}
