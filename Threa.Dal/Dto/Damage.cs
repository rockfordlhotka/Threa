namespace Threa.Dal.Dto
{
  public class Damage : IDamage
  {
    public string Name { get; set; }
    public int Value { get; set; }
    public int BaseValue { get; set; }
    public int PendingHealing { get; set; }
    public int PendingDamage { get; set; }
  }
}
