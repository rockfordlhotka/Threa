namespace Threa.Dal
{
  public class Skill
  {
    public string Id { get; set; }
    public string Category { get; set; }
    public string Name { get; set; }
    public bool IsSpecialized { get; set; }
    public bool IsMagic { get; set; }
    public bool IsTheology { get; set; }
    public bool IsPsionic { get; set; }
    public int Untrained { get; set; }
    public int Trained { get; set; }
    public string PrimarySkill { get; set; }
    public string SecondarySkill { get; set; }
    public string TertiarySkill { get; set; }
  }
}
