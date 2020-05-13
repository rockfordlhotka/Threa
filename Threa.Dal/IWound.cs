namespace Threa.Dal
{
  public interface IWound
  {
    string Location { get; set; }
    int MaxWounds { get; set; }
    int LightWounds { get; set; }
    int SeriousWounds { get; set; }
    bool IsCrippled { get; set; }
    bool IsDestroyed { get; set; }
    int RoundsToDamage { get; set; }
  }
}
