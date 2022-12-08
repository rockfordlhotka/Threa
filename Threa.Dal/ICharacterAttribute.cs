namespace Threa.Dal
{
  public interface ICharacterAttribute
  {
    string Name { get; set; }
    int Value { get; set; }
    int BaseValue { get; set; }
    string ImageUrl { get; set; }
  }
}
