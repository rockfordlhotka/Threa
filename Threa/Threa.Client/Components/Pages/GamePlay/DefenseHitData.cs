using GameMechanics.Combat;

namespace Threa.Client.Components.Pages.GamePlay;

/// <summary>
/// Data transferred when defense results in a hit, triggering damage resolution.
/// </summary>
public class DefenseHitData
{
    /// <summary>
    /// The Success Value from the attack.
    /// </summary>
    public int SV { get; set; }

    /// <summary>
    /// The type of damage being dealt (optional, can be set in damage resolution).
    /// </summary>
    public DamageType? DamageType { get; set; }

    /// <summary>
    /// The damage class of the attack (optional, can be set in damage resolution).
    /// </summary>
    public int? DamageClass { get; set; }

    /// <summary>
    /// Whether a shield block was attempted and succeeded.
    /// </summary>
    public bool? ShieldBlockSuccess { get; set; }

    /// <summary>
    /// The RV from the shield block roll (if attempted).
    /// </summary>
    public int? ShieldBlockRV { get; set; }

    /// <summary>
    /// The called shot location, if the attacker targeted a specific body part.
    /// If null, hit location will be determined randomly.
    /// </summary>
    public HitLocation? CalledShotLocation { get; set; }
}
