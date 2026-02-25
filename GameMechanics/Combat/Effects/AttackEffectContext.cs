using System;
using System.Collections.Generic;
using Threa.Dal.Dto;

namespace GameMechanics.Combat.Effects;

/// <summary>
/// Context for collecting attack-triggered effects.
/// Contains all information about an attack needed to determine which effects should trigger.
/// </summary>
public class AttackEffectContext
{
    /// <summary>
    /// The attacking character.
    /// </summary>
    public required CharacterEdit Attacker { get; init; }

    /// <summary>
    /// The target character being attacked.
    /// </summary>
    public required CharacterEdit Target { get; init; }

    /// <summary>
    /// The CharacterItem ID of the weapon used (if any).
    /// </summary>
    public Guid? WeaponItemId { get; init; }

    /// <summary>
    /// The weapon template (for effect definitions).
    /// </summary>
    public ItemTemplate? WeaponTemplate { get; init; }

    /// <summary>
    /// The success value of the attack roll.
    /// Used for SV-dependent effect triggers.
    /// </summary>
    public int AttackSV { get; init; }

    /// <summary>
    /// The body location hit.
    /// </summary>
    public HitLocation HitLocation { get; init; }

    /// <summary>
    /// Whether this was a critical hit (high SV or special condition).
    /// </summary>
    public bool IsCriticalHit { get; init; }

    /// <summary>
    /// The damage type of the base attack.
    /// </summary>
    public DamageType BaseDamageType { get; init; }

    /// <summary>
    /// Additional context data for specialized attack types.
    /// </summary>
    public Dictionary<string, object> AdditionalData { get; init; } = [];

    /// <summary>
    /// The damage class of the armor at the hit location (1–4), or null if the target
    /// has no armor there.
    /// </summary>
    public int? ArmorDamageClass { get; init; }

    /// <summary>
    /// The damage class of any shield that was involved (1–4), or null if no shield.
    /// </summary>
    public int? ShieldDamageClass { get; init; }

    /// <summary>
    /// The target's inherent damage class (1–4). Represents natural toughness (e.g.,
    /// a dragon's hide). 0 means no inherent DC restriction.
    /// </summary>
    public int TargetDamageClass { get; init; } = 0;

    /// <summary>
    /// Whether the attack penetrated the target's armor (i.e., the net SV after armor
    /// absorption was still positive). True if there is no armor at the hit location.
    /// </summary>
    public bool ArmorWasPenetrated { get; init; } = true;

    /// <summary>
    /// Determines if the attack is considered a critical based on SV threshold.
    /// By default, SV >= 8 is a critical.
    /// </summary>
    public static bool IsCritical(int sv, int threshold = 8) => sv >= threshold;
}

/// <summary>
/// Context for ranged attacks, extending the base context with ammunition information.
/// </summary>
public class RangedAttackEffectContext : AttackEffectContext
{
    /// <summary>
    /// The CharacterItem ID of the ammunition used (if any).
    /// </summary>
    public Guid? AmmunitionItemId { get; init; }

    /// <summary>
    /// The ammunition template (for effect definitions).
    /// </summary>
    public ItemTemplate? AmmunitionTemplate { get; init; }

    /// <summary>
    /// The parsed ammunition properties from the ammo's CustomProperties.
    /// </summary>
    public AmmunitionProperties? AmmoProperties { get; init; }

    /// <summary>
    /// Range band at which the attack was made (affects some effect triggers).
    /// </summary>
    public RangeBand? RangeBand { get; init; }
}

/// <summary>
/// Range bands for ranged attacks.
/// </summary>
public enum RangeBand
{
    /// <summary>Point blank range (within 3 meters).</summary>
    PointBlank,

    /// <summary>Close range (within effective range).</summary>
    Close,

    /// <summary>Medium range (up to 2x effective range).</summary>
    Medium,

    /// <summary>Long range (up to 4x effective range).</summary>
    Long,

    /// <summary>Extreme range (beyond 4x effective range).</summary>
    Extreme
}
