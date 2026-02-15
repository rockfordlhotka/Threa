using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameMechanics.Combat;

/// <summary>
/// Result of multi-damage-type resolution. Contains per-type results
/// sharing a single armor skill roll.
/// </summary>
public class MultiDamageResolutionResult
{
  /// <summary>
  /// Individual damage resolution results, one per damage type.
  /// </summary>
  public List<DamageResolutionResult> PerTypeResults { get; init; } = new();

  /// <summary>
  /// The shared armor skill check roll (rolled once for all types).
  /// </summary>
  public int ArmorSkillRoll { get; init; }

  /// <summary>
  /// The shared armor skill RV.
  /// </summary>
  public int ArmorSkillRV { get; init; }

  /// <summary>
  /// The shared armor skill bonus.
  /// </summary>
  public int ArmorSkillBonus { get; init; }

  /// <summary>
  /// Total fatigue damage across all damage types.
  /// </summary>
  public int TotalFatigueDamage => PerTypeResults.Sum(r => r.FatigueDamage);

  /// <summary>
  /// Total vitality damage across all damage types.
  /// </summary>
  public int TotalVitalityDamage => PerTypeResults.Sum(r => r.VitalityDamage);

  /// <summary>
  /// Total wounds across all damage types.
  /// </summary>
  public int TotalWounds => PerTypeResults.Sum(r => r.WoundCount);

  /// <summary>
  /// Whether any damage type caused a wound.
  /// </summary>
  public bool CausedWound => PerTypeResults.Any(r => r.CausedWound);

  /// <summary>
  /// Whether all damage types were fully absorbed.
  /// </summary>
  public bool FullyAbsorbed => PerTypeResults.All(r => r.FullyAbsorbed);

  /// <summary>
  /// Combined human-readable summary.
  /// </summary>
  public string Summary
  {
    get
    {
      if (PerTypeResults.Count <= 1)
        return PerTypeResults.FirstOrDefault()?.Summary ?? "No damage";

      var sb = new StringBuilder();
      sb.AppendLine("Multi-type damage resolution:");
      foreach (var result in PerTypeResults)
      {
        sb.AppendLine($"  [{result.DamageType}] {result.Summary}");
      }

      sb.Append($"Total: {TotalFatigueDamage} FAT, {TotalVitalityDamage} VIT");
      if (TotalWounds > 0)
        sb.Append($", {TotalWounds} wound(s)");

      return sb.ToString();
    }
  }
}
