namespace Threa.Dal
{
  public interface ISkill
  {
    string Id { get; set; }
    string Category { get; set; }
    string Name { get; set; }
    bool IsSpecialized { get; set; }
    bool IsMagic { get; set; }
    bool IsTheology { get; set; }
    bool IsPsionic { get; set; }
    int Untrained { get; set; }
    int Trained { get; set; }
    string PrimaryAttribute { get; set; }
    string SecondarySkill { get; set; }
    string TertiarySkill { get; set; }
  }
}
