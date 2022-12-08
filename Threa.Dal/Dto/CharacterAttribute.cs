namespace Threa.Dal.Dto
{
  public class CharacterAttribute : ICharacterAttribute
  {
    public string Name { get; set; }
    public int Value { get; set; }
    public int BaseValue { get; set; }
    public string ImageUrl { get; set; }
  }
}
