using System.Collections.Generic;
using System.Linq;

namespace GameMechanics.Reference
{
  public static class ResultValues
  {
    private static readonly List<ResultValue> resultValues = new List<ResultValue>
    {
      new ResultValue(-10, "Terrible", -3, -3, -3, -3),
      new ResultValue(-9, "Terrible", -3, -3, -3, -3),
      new ResultValue(-8, "Horrible", -2, -2, -2, -2),
      new ResultValue(-7, "Horrible", -2, -2, -2, -2),
      new ResultValue(-6, "Worse", -2, -2, -2, -2),
      new ResultValue(-5, "Worse", -2, -2, -1, -1),
      new ResultValue(-4, "Bad", -1, -1, -1, -1),
      new ResultValue(-3, "Bad", -1, -1, 0, 0),
      new ResultValue(-2, "Fail", 0, 0, 0, 0),
      new ResultValue(-1, "Fail", 0, 0, 0, 0),
      new ResultValue(0, "Fair", 0, 1, 1, 1),
      new ResultValue(1, "Fair", 0, 1, 1, 1),
      new ResultValue(2, "Fair", 1, 1, 2, 2),
      new ResultValue(3, "Fair", 1, 1, 2, 2),
      new ResultValue(4, "Good", 2, 2, 3, 4),
      new ResultValue(5, "Good", 2, 2, 3, 4),
      new ResultValue(6, "Good", 2, 2, 4, 8),
      new ResultValue(7, "Good", 2, 2, 5, 8),
      new ResultValue(8, "Great", 3, 3, 6, 16),
      new ResultValue(9, "Great", 3, 3, 7, 32),
      new ResultValue(10, "Great", 3, 3, 8, 64),
      new ResultValue(11, "Great", 3, 3, 9, 128),
      new ResultValue(12, "Superb", 4, 4, 10, 256),
      new ResultValue(13, "Superb", 4, 4, 12, 512),
      new ResultValue(14, "Superb", 4, 4, 14, 1024),
      new ResultValue(15, "Superb", 4, 4, 14, 1024),
      new ResultValue(16, "Superb", 4, 4, 14, 1024),
      new ResultValue(17, "Superb", 4, 4, 14, 1024),
      new ResultValue(18, "Superb", 4, 4, 14, 1024)
    };

    /// <summary>
    /// Gets a result value descriptor for an RV value
    /// </summary>
    /// <param name="resultValue">RV value</param>
    /// <returns></returns>
    public static ResultValue GetResult(int resultValue)
    {
      if (resultValue < -10)
        resultValue = -10;
      else if (resultValue > 18)
        resultValue = 18;

      return resultValues.Where(r => r.RV == resultValue).First();
    }
  }

  public class ResultValue
  {
    /// <summary>
    /// Gets the RV value
    /// </summary>
    public int RV { get; private set; }
    /// <summary>
    /// Gets the RVr (text description) value
    /// </summary>
    public string RVr { get; private set; }
    /// <summary>
    /// Gets the RVs (damage SV) value
    /// </summary>
    public int RVs { get; private set; }
    /// <summary>
    /// Gets the RVa (armor adjustment) value
    /// </summary>
    public int RVa { get; private set; }
    /// <summary>
    /// Gets the RVe (energy) value
    /// </summary>
    public int RVe { get; private set; }
    /// <summary>
    /// gets the RVx (exponential) value
    /// </summary>
    public int RVx { get; private set; }
    /// <summary>
    /// Gets a value indicating whether the result is successful
    /// </summary>
    public bool Success { get => RV >= 0; }

    /// <summary>
    /// Gets a damage value for this result
    /// </summary>
    /// <param name="weaponSVBase">Base SV for weapon</param>
    /// <param name="damageClass">Damage class for weapon</param>
    /// <returns></returns>
    public DamageValue CalculateDamageValue(int weaponSVBase, int damageClass)
    {
      return new DamageValue(this, weaponSVBase, damageClass);
    }

    internal ResultValue(int rv, string rvr, int rvs, int rva, int rve, int rvx)
    {
      RV = rv;
      RVr = rvr;
      RVs = rvs;
      RVa = rva;
      RVe = rve;
      RVx = rvx;
    }
  }

  /// <summary>
  /// Represents a calculated damage value for
  /// an attack
  /// </summary>
  public class DamageValue
  {
    /// <summary>
    /// Calculated success value including
    /// weapon base
    /// </summary>
    public int SV { get; private set; }
    /// <summary>
    /// Randomized damage value ready to
    /// be applied to damage table
    /// </summary>
    public int Damage { get; private set; }
    /// <summary>
    /// Damage class
    /// </summary>
    public int Class { get; private set; }

    /// <summary>
    /// Gets a new damage value modified for
    /// the target's damage class
    /// </summary>
    /// <param name="targetClass">Target's damage class</param>
    /// <returns></returns>
    public int GetModifiedDamage(int targetClass)
    {
      if (Class == targetClass)
        return Damage;
      else if (targetClass < Class)
        return Damage * (Class - targetClass) * 10;
      else
        return Damage / ((targetClass - Class) * 10);
    }

    /// <summary>
    /// Calculates SV and randomized damage value
    /// </summary>
    /// <param name="resultValue">AV - TV</param>
    /// <param name="weaponSVBase">Weapon base SV</param>
    /// <param name="weaponClass">Weapon damage class</param>
    public DamageValue(ResultValue resultValue, int weaponSVBase, int weaponClass)
    {
      Class = weaponClass;
      SV = resultValue.RVs + weaponSVBase;
      if (SV > 20)
        SV = 20;
      switch (SV)
      {
        case 0:
          Damage = Dice.Roll(1, 2);
          break;
        case 1:
          Damage = Dice.Roll(1, 3);
          break;
        case 2:
          Damage = Dice.Roll(1, 6);
          break;
        case 3:
          Damage = Dice.Roll(1, 8);
          break;
        case 4:
          Damage = Dice.Roll(1, 10);
          break;
        case 5:
          Damage = Dice.Roll(1, 12);
          break;
        case 6:
          Damage = Dice.Roll(1, 6) + Dice.Roll(1, 8);
          break;
        case 7:
          Damage = Dice.Roll(2, 8);
          break;
        case 8:
          Damage = Dice.Roll(2, 10);
          break;
        case 9:
          Damage = Dice.Roll(2, 12);
          break;
        case 10:
          Damage = Dice.Roll(3, 10);
          break;
        case 11:
          Damage = Dice.Roll(3, 12);
          break;
        case 12:
        case 13:
        case 14:
          Damage = Dice.Roll(4, 10);
          break;
        case 15:
        case 16:
          Damage = Dice.Roll(1, 6);
          Class += 1;
          break;
        case 17:
        case 18:
          Damage = Dice.Roll(1, 8);
          Class += 1;
          break;
        default:
          Damage = Dice.Roll(1, 10);
          Class += 1;
          break;
      }
    }
  }
}
