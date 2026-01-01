namespace Threa.Dal.Dto;

public class Wound
{
    public string Location { get; set; } = string.Empty;
    public int MaxWounds { get; set; }
    public int LightWounds { get; set; }
    public int SeriousWounds { get; set; }
    public bool IsCrippled { get; set; }
    public bool IsDestroyed { get; set; }
    public int RoundsToDamage { get; set; }
}
