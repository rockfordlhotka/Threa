namespace Threa.Dal
{
  public interface IDamage
  {
    string Name { get; set; }
    int Value { get; set; }
    int BaseValue { get; set; }
    int PendingHealing { get; set; }
    int PendingDamage { get; set; }
  }
}
