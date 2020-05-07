using System.Collections.Generic;

namespace GameMechanics.Reference
{
  public static class DamageSheet
  {
    private static readonly List<DamageResult> damageResults = new List<DamageResult>();
  }

  public class DamageResult
  {
    public int Fatigue { get; set; }
    public int Vitality { get; set; }
    public int Wounds { get; set; }

    public DamageResult()
    { }

    public DamageResult(int fat, int vit, int wnd)
    {
      Fatigue = fat;
      Vitality = vit;
      Wounds = wnd;
    }
  }
}
