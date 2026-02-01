using System.Collections.Generic;
using System.Text;

namespace GameMechanics.Combat;

/// <summary>
/// Result of a firearm attack with fire mode support.
/// Provides SV for targets to use in damage resolution.
/// </summary>
public class FirearmAttackResult
{
    /// <summary>Fire mode used.</summary>
    public FireMode FireMode { get; set; }

    /// <summary>The AV base (AS + weapon mod + movement penalty).</summary>
    public int AVBase { get; set; }

    /// <summary>The dice roll (4dF+).</summary>
    public int DiceRoll { get; set; }

    /// <summary>The total AV (base + roll).</summary>
    public int AV { get; set; }

    /// <summary>The base TV from range and conditions.</summary>
    public int BaseTV { get; set; }

    /// <summary>The TV adjustment from dodge.</summary>
    public int TVAdjustment { get; set; }

    /// <summary>The final TV used.</summary>
    public int TV { get; set; }

    /// <summary>The Result Value (AV - TV).</summary>
    public int RV { get; set; }

    /// <summary>Whether the first shot hit.</summary>
    public bool Hit { get; set; }

    /// <summary>Ammo consumed by this attack.</summary>
    public int AmmoConsumed { get; set; }

    /// <summary>Ammo remaining after attack.</summary>
    public int AmmoRemaining { get; set; }

    /// <summary>
    /// For Single/Burst: list of hit results with SV for each projectile.
    /// </summary>
    public List<ProjectileHitResult> Hits { get; set; } = new();

    /// <summary>
    /// For Suppression/AOE: the SV that GM distributes to affected targets.
    /// </summary>
    public int? OutputSV { get; set; }

    // ========== AOE-specific Properties ==========

    /// <summary>Whether this was an AOE attack (from weapon or ammo).</summary>
    public bool IsAOE { get; set; }

    /// <summary>Blast radius in meters for AOE attacks.</summary>
    public int BlastRadius { get; set; }

    /// <summary>Blast falloff type: "Linear", "Steep", or "Flat".</summary>
    public string? BlastFalloff { get; set; }

    /// <summary>SV for the direct hit target (includes DirectHitBonus).</summary>
    public int? DirectHitSV { get; set; }

    /// <summary>Human-readable description of the result.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Whether there was not enough ammo for the attack.
    /// </summary>
    public bool InsufficientAmmo { get; set; }

    /// <summary>
    /// Generates a formatted summary of the attack.
    /// </summary>
    public string GetFormattedSummary()
    {
        var sb = new StringBuilder();

        if (InsufficientAmmo)
        {
            sb.AppendLine("INSUFFICIENT AMMO");
            return sb.ToString();
        }

        // Display attack type
        if (IsAOE)
        {
            sb.AppendLine($"Attack Type: AOE ({BlastRadius}m radius, {BlastFalloff ?? "Linear"} falloff)");
        }
        else
        {
            sb.AppendLine($"Fire Mode: {FireMode}");
        }

        sb.AppendLine($"AV: {AVBase} (base) + {DiceRoll} (roll) = {AV}");
        sb.AppendLine($"TV: {BaseTV} (base) + {TVAdjustment} (dodge) = {TV}");
        sb.AppendLine($"RV: {RV} ({(Hit ? "HIT" : "MISS")})");
        sb.AppendLine();

        if (IsAOE)
        {
            if (Hit && OutputSV.HasValue)
            {
                sb.AppendLine($"Area SV: {OutputSV.Value}");
                if (DirectHitSV.HasValue && DirectHitSV.Value != OutputSV.Value)
                {
                    sb.AppendLine($"Direct Hit SV: {DirectHitSV.Value}");
                }
                sb.AppendLine($"Blast radius: {BlastRadius}m");
                sb.AppendLine("GM determines which targets are in the blast area.");
                sb.AppendLine($"Each hit target should apply appropriate SV via Damage Resolution");
            }
        }
        else if (FireMode == FireMode.Single)
        {
            if (Hit && Hits.Count > 0)
            {
                sb.AppendLine($"SV: {Hits[0].SV}");
                sb.AppendLine($"Target should apply SV {Hits[0].SV} via Damage Resolution");
            }
        }
        else if (FireMode == FireMode.Burst)
        {
            sb.AppendLine("Burst Results:");
            int hitCount = 0;
            foreach (var hit in Hits)
            {
                string status = hit.Hit ? $"HIT (SV {hit.SV})" : "MISS";
                sb.AppendLine($"  Shot {hit.ShotNumber}: TV {hit.TVForShot}, RV {hit.RVForShot} - {status}");
                if (hit.Hit) hitCount++;
            }
            if (hitCount > 0)
            {
                sb.AppendLine($"{hitCount} hit(s)! Each hit applies its individual SV via Damage Resolution");
            }
        }
        else if (FireMode == FireMode.Suppression)
        {
            if (Hit && OutputSV.HasValue)
            {
                sb.AppendLine($"Output SV: {OutputSV.Value}");
                sb.AppendLine("GM determines which targets are hit.");
                sb.AppendLine($"Each hit target should apply SV {OutputSV.Value} via Damage Resolution");
            }
        }

        sb.AppendLine();
        sb.AppendLine($"Ammo: {AmmoConsumed} consumed, {AmmoRemaining} remaining");

        return sb.ToString();
    }

    /// <summary>
    /// Creates an insufficient ammo result.
    /// </summary>
    public static FirearmAttackResult NotEnoughAmmo(int currentAmmo, int required)
    {
        return new FirearmAttackResult
        {
            InsufficientAmmo = true,
            AmmoRemaining = currentAmmo,
            Description = $"Insufficient ammo: have {currentAmmo}, need {required}"
        };
    }
}

/// <summary>
/// Result for a single projectile in a burst or single shot.
/// </summary>
public class ProjectileHitResult
{
    /// <summary>Shot number in the burst (1, 2, 3, etc.).</summary>
    public int ShotNumber { get; set; }

    /// <summary>TV for this specific shot (increases for burst).</summary>
    public int TVForShot { get; set; }

    /// <summary>RV for this shot.</summary>
    public int RVForShot { get; set; }

    /// <summary>Whether this shot hit.</summary>
    public bool Hit { get; set; }

    /// <summary>The SV if hit (base + RV/2 + ammo modifier).</summary>
    public int SV { get; set; }
}
