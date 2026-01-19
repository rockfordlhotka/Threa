using System.Collections.Generic;
using System.Linq;

namespace GameMechanics.Combat.Effects;

/// <summary>
/// Result of collecting attack-triggered effects from all sources.
/// </summary>
public class AttackEffectCollectionResult
{
    /// <summary>
    /// Effects to apply to the target (debuffs, DoT, etc.).
    /// </summary>
    public List<CollectedAttackEffect> TargetEffects { get; init; } = [];

    /// <summary>
    /// Effects to apply to the attacker (life-steal, self-buffs, etc.).
    /// </summary>
    public List<CollectedAttackEffect> AttackerEffects { get; init; } = [];

    /// <summary>
    /// Bonus damage to add to the attack's SV before damage calculation.
    /// </summary>
    public List<BonusDamageEntry> BonusDamage { get; init; } = [];

    /// <summary>
    /// Narrative descriptions of what effects triggered.
    /// </summary>
    public List<string> TriggerDescriptions { get; init; } = [];

    /// <summary>
    /// Whether any effects were collected.
    /// </summary>
    public bool HasEffects => TargetEffects.Count > 0 || AttackerEffects.Count > 0 || BonusDamage.Count > 0;

    /// <summary>
    /// Total bonus damage to add to the attack SV.
    /// </summary>
    public int TotalBonusDamage => BonusDamage.Sum(b => b.Damage);

    /// <summary>
    /// Creates an empty result (no effects triggered).
    /// </summary>
    public static AttackEffectCollectionResult Empty() => new();

    /// <summary>
    /// Adds an effect to apply to the target.
    /// </summary>
    public void AddTargetEffect(AttackEffectGrant grant, string sourceName)
    {
        TargetEffects.Add(new CollectedAttackEffect
        {
            Grant = grant,
            SourceName = sourceName
        });

        if (grant.BonusDamage > 0)
        {
            BonusDamage.Add(new BonusDamageEntry
            {
                Damage = grant.BonusDamage,
                DamageType = grant.BonusDamageType ?? DamageType.Energy,
                Source = sourceName
            });
        }

        if (!string.IsNullOrEmpty(grant.Description))
        {
            TriggerDescriptions.Add($"{sourceName}: {grant.Description}");
        }
    }

    /// <summary>
    /// Adds an effect to apply to the attacker.
    /// </summary>
    public void AddAttackerEffect(AttackEffectGrant grant, string sourceName)
    {
        AttackerEffects.Add(new CollectedAttackEffect
        {
            Grant = grant,
            SourceName = sourceName
        });

        if (!string.IsNullOrEmpty(grant.Description))
        {
            TriggerDescriptions.Add($"{sourceName} (self): {grant.Description}");
        }
    }

    /// <summary>
    /// Adds bonus damage without an associated effect.
    /// </summary>
    public void AddBonusDamage(int damage, DamageType damageType, string source)
    {
        BonusDamage.Add(new BonusDamageEntry
        {
            Damage = damage,
            DamageType = damageType,
            Source = source
        });

        TriggerDescriptions.Add($"{source}: +{damage} {damageType} damage");
    }

    /// <summary>
    /// Merges another result into this one.
    /// </summary>
    public void Merge(AttackEffectCollectionResult other)
    {
        TargetEffects.AddRange(other.TargetEffects);
        AttackerEffects.AddRange(other.AttackerEffects);
        BonusDamage.AddRange(other.BonusDamage);
        TriggerDescriptions.AddRange(other.TriggerDescriptions);
    }

    /// <summary>
    /// Gets a summary of all triggered effects for combat log.
    /// </summary>
    public string GetSummary()
    {
        if (!HasEffects)
            return "No special effects triggered.";

        var parts = new List<string>();

        if (BonusDamage.Count > 0)
        {
            var damageDesc = string.Join(", ", BonusDamage.Select(b => $"+{b.Damage} {b.DamageType}"));
            parts.Add($"Bonus damage: {damageDesc}");
        }

        if (TargetEffects.Count > 0)
        {
            var effectNames = string.Join(", ", TargetEffects.Select(e => e.Grant.EffectName));
            parts.Add($"Target effects: {effectNames}");
        }

        if (AttackerEffects.Count > 0)
        {
            var effectNames = string.Join(", ", AttackerEffects.Select(e => e.Grant.EffectName));
            parts.Add($"Attacker effects: {effectNames}");
        }

        return string.Join("; ", parts);
    }
}

/// <summary>
/// A collected effect with its source information.
/// </summary>
public class CollectedAttackEffect
{
    /// <summary>
    /// The effect grant data.
    /// </summary>
    public required AttackEffectGrant Grant { get; init; }

    /// <summary>
    /// The name of the source (weapon, spell, item).
    /// </summary>
    public required string SourceName { get; init; }
}

/// <summary>
/// Bonus damage entry with type and source.
/// </summary>
public class BonusDamageEntry
{
    /// <summary>
    /// The amount of bonus damage.
    /// </summary>
    public int Damage { get; init; }

    /// <summary>
    /// The type of damage.
    /// </summary>
    public DamageType DamageType { get; init; }

    /// <summary>
    /// The source of the bonus damage.
    /// </summary>
    public required string Source { get; init; }
}
