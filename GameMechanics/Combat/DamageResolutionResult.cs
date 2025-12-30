using System.Collections.Generic;

namespace GameMechanics.Combat
{
  /// <summary>
  /// Result of damage resolution after defense sequence.
  /// </summary>
  public class DamageResolutionResult
  {
    /// <summary>
    /// The original incoming SV from the attack.
    /// </summary>
    public int IncomingSV { get; init; }

    /// <summary>
    /// The hit location struck.
    /// </summary>
    public HitLocation HitLocation { get; init; }

    /// <summary>
    /// The damage type of the attack.
    /// </summary>
    public DamageType DamageType { get; init; }

    /// <summary>
    /// The damage class of the attack.
    /// </summary>
    public int DamageClass { get; init; }

    /// <summary>
    /// The armor skill check roll (4dF+).
    /// </summary>
    public int ArmorSkillRoll { get; init; }

    /// <summary>
    /// The armor skill RV (roll + AS - 8).
    /// </summary>
    public int ArmorSkillRV { get; init; }

    /// <summary>
    /// Bonus absorption from armor skill check.
    /// </summary>
    public int ArmorSkillBonus { get; init; }

    /// <summary>
    /// Records of each absorption step (shield, then armor layers).
    /// </summary>
    public List<AbsorptionRecord> AbsorptionSteps { get; init; } = new();

    /// <summary>
    /// Total SV absorbed by all defenses.
    /// </summary>
    public int TotalAbsorbed { get; init; }

    /// <summary>
    /// Final SV after all absorption (penetrating damage).
    /// </summary>
    public int PenetratingSV { get; init; }

    /// <summary>
    /// The final damage result based on penetrating SV.
    /// </summary>
    public DamageResult FinalDamage { get; init; } = DamageResult.None;

    /// <summary>
    /// Whether a wound was caused.
    /// </summary>
    public bool CausedWound => FinalDamage.CausesWound;

    /// <summary>
    /// FAT damage to apply.
    /// </summary>
    public int FatigueDamage => FinalDamage.FatigueDamage;

    /// <summary>
    /// VIT damage to apply.
    /// </summary>
    public int VitalityDamage => FinalDamage.VitalityDamage;

    /// <summary>
    /// Whether all damage was absorbed (no penetration).
    /// </summary>
    public bool FullyAbsorbed => PenetratingSV <= 0;

    /// <summary>
    /// Human-readable summary.
    /// </summary>
    public string Summary { get; init; } = string.Empty;
  }
}
