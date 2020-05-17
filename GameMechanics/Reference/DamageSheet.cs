using System.Collections.Generic;

namespace GameMechanics.Reference
{
  public static class DamageSheet
  {
    private static readonly List<DamageResult> damageResults = new List<DamageResult>
    {
      new DamageResult(1, 0, 0),
      new DamageResult(2, 0, 0),
      new DamageResult(3, 0, 0),
      new DamageResult(4, 0, 0),
      new DamageResult(5, 1, 0),
      new DamageResult(6, 2, 0),
      new DamageResult(7, 4, 1),
      new DamageResult(8, 5, 1),
      new DamageResult(9, 8, 1),
      new DamageResult(10, 10, 2),
      new DamageResult(11, 11, 2),
      new DamageResult(12, 12, 2),
      new DamageResult(13, 13, 2),
      new DamageResult(14, 14, 2),
      new DamageResult(15, 15, 3),
      new DamageResult(16, 16, 3),
      new DamageResult(17, 17, 3),
      new DamageResult(18, 18, 3),
      new DamageResult(19, 19, 3)
    };

    public static DamageResult GetDamageResult(int sv)
    {
      return damageResults[sv - 1];
    }
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
