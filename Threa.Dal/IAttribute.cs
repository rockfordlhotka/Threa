namespace Threa.Dal
{
  public interface IAttribute
  {
    string Name { get; set; }
    int Value { get; set; }
    int BaseValue { get; set; }
  }
}
