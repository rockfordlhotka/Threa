using System;
using System.Collections.Generic;
using GameMechanics.Combat;

namespace GameMechanics.Messaging;

/// <summary>
/// State of a targeting interaction between attacker and defender.
/// </summary>
public enum TargetingState
{
    /// <summary>Attacker has initiated targeting, waiting for defender.</summary>
    Initiated,
    /// <summary>Defender is viewing the targeting modal.</summary>
    DefenderViewing,
    /// <summary>Attacker has confirmed their attack settings.</summary>
    AttackerConfirmed,
    /// <summary>Defender has confirmed their defense settings.</summary>
    DefenderConfirmed,
    /// <summary>Attack has been resolved with results.</summary>
    Resolved,
    /// <summary>Attacker cancelled the targeting.</summary>
    Cancelled
}

/// <summary>
/// Type of targeting action being performed.
/// </summary>
public enum TargetingActionType
{
    /// <summary>Melee attack.</summary>
    MeleeAttack,
    /// <summary>Ranged/firearm attack.</summary>
    RangedAttack,
    /// <summary>Medical action (healing, first aid, etc.).</summary>
    Medical
}

/// <summary>
/// Attacker-provided data for a targeting interaction.
/// </summary>
public class TargetingAttackerData
{
    /// <summary>
    /// Type of targeting action.
    /// </summary>
    public TargetingActionType ActionType { get; init; }

    /// <summary>
    /// ID of the skill being used for the attack.
    /// </summary>
    public string SkillId { get; init; } = string.Empty;

    /// <summary>
    /// Name of the skill being used (for display).
    /// </summary>
    public string SkillName { get; init; } = string.Empty;

    /// <summary>
    /// Attacker's base Ability Score for the skill.
    /// </summary>
    public int SkillAS { get; init; }

    /// <summary>
    /// Weapon AV modifier (quality, enchantment, etc.).
    /// </summary>
    public int WeaponAVModifier { get; init; }

    /// <summary>
    /// Weapon SV modifier added to successful hits.
    /// </summary>
    public int WeaponSVModifier { get; init; }

    /// <summary>
    /// Weapon damage class.
    /// </summary>
    public int WeaponDamageClass { get; init; }

    /// <summary>
    /// Weapon name (for display).
    /// </summary>
    public string? WeaponName { get; init; }

    /// <summary>
    /// Whether the attacker is moving (-2 AV).
    /// </summary>
    public bool IsMoving { get; init; }

    /// <summary>
    /// Action cost type for the attack (1 AP + 1 FAT or 2 AP).
    /// </summary>
    public ActionCostType ActionCostType { get; init; } = ActionCostType.OneAPOneFat;

    /// <summary>
    /// AP boost being applied.
    /// </summary>
    public int APBoost { get; init; }

    /// <summary>
    /// FAT boost being applied.
    /// </summary>
    public int FATBoost { get; init; }

    /// <summary>
    /// Whether this is a called shot (melee or ranged).
    /// </summary>
    public bool IsCalledShot { get; init; }

    /// <summary>
    /// Target location for called shot.
    /// </summary>
    public HitLocation? CalledShotLocation { get; init; }

    /// <summary>
    /// Whether physicality bonus is being used.
    /// </summary>
    public bool UsePhysicality { get; init; }

    // === Ranged-specific properties ===

    /// <summary>
    /// Range category for ranged attacks.
    /// </summary>
    public RangeCategory? Range { get; init; }

    /// <summary>
    /// Fire mode for ranged attacks.
    /// </summary>
    public FireMode? FireMode { get; init; }

    /// <summary>
    /// Number of rounds in burst (for burst mode).
    /// </summary>
    public int BurstSize { get; init; } = 3;

    /// <summary>
    /// Number of rounds for suppressive fire.
    /// </summary>
    public int SuppressiveRounds { get; init; } = 10;

    /// <summary>
    /// Aim bonus from aiming actions.
    /// </summary>
    public int AimBonus { get; init; }

    /// <summary>
    /// Whether this attack is AOE.
    /// </summary>
    public bool IsAOE { get; init; }

    /// <summary>
    /// Effective blast radius in meters (for AOE).
    /// </summary>
    public int BlastRadius { get; init; }

    /// <summary>
    /// Whether the weapon allows dodge (thrown, arrows vs bullets).
    /// </summary>
    public bool IsDodgeable { get; init; }

    /// <summary>
    /// Current loaded ammo for ammo tracking.
    /// </summary>
    public int CurrentLoadedAmmo { get; init; }

    /// <summary>
    /// Ammo damage modifier from special ammo types.
    /// </summary>
    public int AmmoDamageModifier { get; init; }

    /// <summary>
    /// Damage type from weapon/ammo.
    /// </summary>
    public DamageType DamageType { get; init; }

    /// <summary>
    /// Per-damage-type SV modifiers from weapon and ammo combined.
    /// When set, takes precedence over WeaponSVModifier and DamageType for damage resolution.
    /// Format: {"Cutting": 4, "Energy": 2}
    /// </summary>
    public Dictionary<string, int>? WeaponDamageModifiers { get; init; }

    /// <summary>
    /// The ItemTemplate ID of the weapon being used (0 = unknown/unarmed).
    /// Used by the defender's damage resolution to apply weapon on-hit effects.
    /// </summary>
    public int WeaponTemplateId { get; init; }

    /// <summary>
    /// The ID of the attacking character.
    /// Used by the defender to attribute on-hit effects correctly.
    /// </summary>
    public int AttackerCharacterId { get; init; }

    /// <summary>
    /// Special effect string from the loaded ammo (e.g., "Incendiary", "Cryo").
    /// Null if no ammo or no special effect. Used to apply ammo on-hit effects to the target.
    /// </summary>
    public string? AmmoSpecialEffect { get; init; }

    /// <summary>
    /// Available melee weapons for the attacker to choose from (melee attacks only).
    /// Populated by CombatContent so the targeting panel can present a weapon selector
    /// identical to the anonymous-attack AttackMode panel.
    /// </summary>
    public List<MeleeWeaponInfo>? AvailableWeapons { get; init; }

    // === Ranged weapon identity/capability (populated by CombatContent for ranged attacks) ===

    /// <summary>
    /// CharacterItem ID of the ranged weapon being used.
    /// </summary>
    public Guid WeaponItemId { get; init; }

    /// <summary>Whether the weapon supports single-shot fire.</summary>
    public bool SupportsSingle { get; init; } = true;

    /// <summary>Whether the weapon supports burst fire.</summary>
    public bool SupportsBurst { get; init; }

    /// <summary>Whether the weapon supports suppressive fire.</summary>
    public bool SupportsSuppression { get; init; }

    /// <summary>
    /// Gets a WeaponDamageProfile from per-type modifiers or legacy single-type data.
    /// </summary>
    public WeaponDamageProfile? GetWeaponDamageProfile()
    {
        if (WeaponDamageModifiers != null && WeaponDamageModifiers.Count > 0)
        {
            var modifiers = new Dictionary<DamageType, int>();
            foreach (var kv in WeaponDamageModifiers)
            {
                if (System.Enum.TryParse<DamageType>(kv.Key, ignoreCase: true, out var dt))
                    modifiers[dt] = kv.Value;
            }
            if (modifiers.Count > 0)
                return new WeaponDamageProfile(modifiers);
        }

        // Fall back to legacy single type
        return WeaponDamageProfile.FromSingle(DamageType, WeaponSVModifier);
    }
}

/// <summary>
/// Defender-provided data for a targeting interaction.
/// </summary>
public class TargetingDefenderData
{
    /// <summary>
    /// Type of defense being used.
    /// </summary>
    public DefenseType DefenseType { get; init; }

    /// <summary>
    /// Whether the defender is currently moving (+2 TV for ranged).
    /// </summary>
    public bool IsMoving { get; init; }

    /// <summary>
    /// Cover type (+1 half, +2 three-quarters TV).
    /// </summary>
    public CoverType Cover { get; init; } = CoverType.None;

    /// <summary>
    /// Whether the defender is prone (+2 TV for ranged).
    /// </summary>
    public bool IsProne { get; init; }

    /// <summary>
    /// Whether the defender is crouching (+2 TV for ranged).
    /// </summary>
    public bool IsCrouching { get; init; }

    /// <summary>
    /// Target size modifier for ranged attacks.
    /// </summary>
    public TargetSize Size { get; init; } = TargetSize.Normal;

    /// <summary>
    /// Action cost type for active defense (1 AP + 1 FAT or 2 AP). Only applies to active defense.
    /// </summary>
    public ActionCostType DefenseCostType { get; init; } = ActionCostType.OneAPOneFat;

    /// <summary>
    /// AP boost for active defense.
    /// </summary>
    public int APBoost { get; init; }

    /// <summary>
    /// FAT boost for active defense.
    /// </summary>
    public int FATBoost { get; init; }

    /// <summary>
    /// Defender's Dodge AS (for passive/active dodge).
    /// </summary>
    public int DodgeAS { get; init; }

    /// <summary>
    /// Defender's Parry AS (for parry defense).
    /// </summary>
    public int ParryAS { get; init; }

    /// <summary>
    /// Defender's Shield AS (for shield block).
    /// </summary>
    public int ShieldAS { get; init; }

    /// <summary>
    /// Whether the defender is currently in parry mode.
    /// </summary>
    public bool IsInParryMode { get; init; }
}

/// <summary>
/// Result data from attack resolution.
/// </summary>
public class TargetingResolutionData
{
    /// <summary>
    /// The attacker's final AV (roll + modifiers).
    /// </summary>
    public int AttackerAV { get; init; }

    /// <summary>
    /// The defender's TV.
    /// </summary>
    public int DefenderTV { get; init; }

    /// <summary>
    /// The raw dice roll result.
    /// </summary>
    public int DiceRoll { get; init; }

    /// <summary>
    /// The success value (AV - TV).
    /// </summary>
    public int SuccessValue { get; init; }

    /// <summary>
    /// Whether the attack hit.
    /// </summary>
    public bool IsHit { get; init; }

    /// <summary>
    /// Hit location determined.
    /// </summary>
    public HitLocation HitLocation { get; init; }

    /// <summary>
    /// Final damage class applied.
    /// </summary>
    public int DamageClass { get; init; }

    /// <summary>
    /// FAT damage dealt.
    /// </summary>
    public int FATDamage { get; init; }

    /// <summary>
    /// VIT damage dealt.
    /// </summary>
    public int VITDamage { get; init; }

    /// <summary>
    /// Narrative description for attacker.
    /// </summary>
    public string AttackerNarrative { get; init; } = string.Empty;

    /// <summary>
    /// Detailed breakdown for defender.
    /// </summary>
    public string DefenderDetails { get; init; } = string.Empty;

    /// <summary>
    /// Breakdown of attack calculation.
    /// </summary>
    public string AttackBreakdown { get; init; } = string.Empty;

    /// <summary>
    /// Breakdown of defense calculation.
    /// </summary>
    public string DefenseBreakdown { get; init; } = string.Empty;

    // === Ranged ammo tracking ===

    /// <summary>
    /// CharacterItem ID of the weapon used (ranged attacks only).
    /// </summary>
    public Guid WeaponItemId { get; init; }

    /// <summary>
    /// Number of rounds consumed by this attack.
    /// </summary>
    public int AmmoConsumed { get; init; }

    /// <summary>
    /// Rounds remaining in the weapon after this attack.
    /// </summary>
    public int AmmoRemaining { get; init; }
}

/// <summary>
/// Base class for targeting messages.
/// </summary>
public abstract class TargetingMessageBase
{
    /// <summary>
    /// Unique identifier for this message.
    /// </summary>
    public Guid MessageId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// Unique identifier for this targeting interaction.
    /// </summary>
    public Guid InteractionId { get; init; }

    /// <summary>
    /// The table where this interaction is happening.
    /// </summary>
    public Guid TableId { get; init; }

    /// <summary>
    /// When the message was created.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Message sent when attacker initiates targeting.
/// </summary>
public class TargetingRequestMessage : TargetingMessageBase
{
    /// <summary>
    /// ID of the attacking character.
    /// </summary>
    public int AttackerId { get; init; }

    /// <summary>
    /// Name of the attacking character (for display).
    /// </summary>
    public string AttackerName { get; init; } = string.Empty;

    /// <summary>
    /// ID of the defending character.
    /// </summary>
    public int DefenderId { get; init; }

    /// <summary>
    /// Name of the defending character (for display).
    /// </summary>
    public string DefenderName { get; init; } = string.Empty;

    /// <summary>
    /// Initial attacker data.
    /// </summary>
    public TargetingAttackerData AttackerData { get; init; } = new();
}

/// <summary>
/// Message sent when defender responds to targeting request.
/// </summary>
public class TargetingResponseMessage : TargetingMessageBase
{
    /// <summary>
    /// Whether the defender acknowledged the request.
    /// </summary>
    public bool Acknowledged { get; init; }

    /// <summary>
    /// Whether the defender is busy (already in another interaction).
    /// </summary>
    public bool IsBusy { get; init; }

    /// <summary>
    /// Position in queue if busy (0 = next up).
    /// </summary>
    public int QueuePosition { get; init; }
}

/// <summary>
/// Message sent when either party updates their data.
/// </summary>
public class TargetingUpdateMessage : TargetingMessageBase
{
    /// <summary>
    /// Whether this update is from the attacker (true) or defender (false).
    /// </summary>
    public bool IsFromAttacker { get; init; }

    /// <summary>
    /// Updated attacker data (if from attacker).
    /// </summary>
    public TargetingAttackerData? AttackerData { get; init; }

    /// <summary>
    /// Updated defender data (if from defender).
    /// </summary>
    public TargetingDefenderData? DefenderData { get; init; }

    /// <summary>
    /// Whether the party has confirmed their settings.
    /// </summary>
    public bool IsConfirmed { get; init; }
}

/// <summary>
/// Message sent when targeting is resolved with results.
/// </summary>
public class TargetingResultMessage : TargetingMessageBase
{
    /// <summary>
    /// ID of the attacking character.
    /// </summary>
    public int AttackerId { get; init; }

    /// <summary>
    /// ID of the defending character.
    /// </summary>
    public int DefenderId { get; init; }

    /// <summary>
    /// The resolution data.
    /// </summary>
    public TargetingResolutionData Resolution { get; init; } = new();

    /// <summary>
    /// Whether defender has accepted the damage.
    /// </summary>
    public bool DamageAccepted { get; init; }
}

/// <summary>
/// Message sent when attacker cancels targeting.
/// </summary>
public class TargetingCancelledMessage : TargetingMessageBase
{
    /// <summary>
    /// ID of the attacking character who cancelled.
    /// </summary>
    public int AttackerId { get; init; }

    /// <summary>
    /// ID of the defending character.
    /// </summary>
    public int DefenderId { get; init; }

    /// <summary>
    /// Reason for cancellation (optional).
    /// </summary>
    public string? Reason { get; init; }
}
