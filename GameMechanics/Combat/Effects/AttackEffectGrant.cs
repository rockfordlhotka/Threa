using System.Text.Json.Serialization;
using Threa.Dal.Dto;

namespace GameMechanics.Combat.Effects;

/// <summary>
/// Represents an effect to apply when an attack hits.
/// Used by weapons, ammunition, buffs, and equipped items to grant effects on attack.
/// </summary>
public class AttackEffectGrant
{
    /// <summary>
    /// Display name of the effect (e.g., "Burning", "Frost Damage", "Life Drain").
    /// </summary>
    [JsonPropertyName("effectName")]
    public string EffectName { get; set; } = string.Empty;

    /// <summary>
    /// Description of what the effect does.
    /// </summary>
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    /// <summary>
    /// The type of effect to apply.
    /// </summary>
    [JsonPropertyName("effectType")]
    public EffectType EffectType { get; set; } = EffectType.Debuff;

    /// <summary>
    /// JSON-serialized behavior state for the effect.
    /// Structure depends on EffectType (e.g., DoT configuration, debuff modifiers).
    /// </summary>
    [JsonPropertyName("behaviorState")]
    public string? BehaviorState { get; set; }

    /// <summary>
    /// Duration of the effect in rounds. Null for permanent until removed.
    /// </summary>
    [JsonPropertyName("durationRounds")]
    public int? DurationRounds { get; set; }

    /// <summary>
    /// Bonus damage to add to the attack's SV.
    /// </summary>
    [JsonPropertyName("bonusDamage")]
    public int BonusDamage { get; set; }

    /// <summary>
    /// Type of bonus damage (for elemental/energy damage).
    /// </summary>
    [JsonPropertyName("bonusDamageType")]
    public DamageType? BonusDamageType { get; set; }

    /// <summary>
    /// The source of this effect grant (weapon name, spell name, ammo type, etc.).
    /// Used for narrative descriptions.
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>
    /// If true, this effect applies to the attacker instead of the target.
    /// Used for life-steal, self-buff on hit, etc.
    /// </summary>
    [JsonPropertyName("appliesToAttacker")]
    public bool AppliesToAttacker { get; set; }

    /// <summary>
    /// Chance for this effect to trigger (0.0 to 1.0). Default is 1.0 (always).
    /// </summary>
    [JsonPropertyName("triggerChance")]
    public double TriggerChance { get; set; } = 1.0;

    /// <summary>
    /// Whether this effect only triggers on critical hits.
    /// </summary>
    [JsonPropertyName("criticalOnly")]
    public bool CriticalOnly { get; set; }

    /// <summary>
    /// Icon name for UI display.
    /// </summary>
    [JsonPropertyName("iconName")]
    public string? IconName { get; set; }

    /// <summary>
    /// Creates an AttackEffectGrant for a simple bonus damage effect.
    /// </summary>
    public static AttackEffectGrant CreateBonusDamage(int damage, DamageType damageType, string source)
    {
        return new AttackEffectGrant
        {
            EffectName = $"{damageType} Damage",
            Description = $"+{damage} {damageType} damage",
            BonusDamage = damage,
            BonusDamageType = damageType,
            Source = source
        };
    }

    /// <summary>
    /// Creates an AttackEffectGrant for a damage-over-time effect.
    /// </summary>
    public static AttackEffectGrant CreateDotEffect(
        string effectName,
        int damagePerRound,
        DamageType damageType,
        int durationRounds,
        string source)
    {
        var dotState = new DotEffectState
        {
            DamagePerRound = damagePerRound,
            DamageType = damageType,
            DamageTarget = "FAT"
        };

        return new AttackEffectGrant
        {
            EffectName = effectName,
            Description = $"{damagePerRound} {damageType} damage per round for {durationRounds} rounds",
            EffectType = EffectType.Debuff,
            BehaviorState = System.Text.Json.JsonSerializer.Serialize(dotState),
            DurationRounds = durationRounds,
            Source = source
        };
    }

    /// <summary>
    /// Creates an AttackEffectGrant for a debuff effect (e.g., Slowed, Weakened).
    /// </summary>
    public static AttackEffectGrant CreateDebuff(
        string effectName,
        string? description,
        string? behaviorState,
        int durationRounds,
        string source)
    {
        return new AttackEffectGrant
        {
            EffectName = effectName,
            Description = description,
            EffectType = EffectType.Debuff,
            BehaviorState = behaviorState,
            DurationRounds = durationRounds,
            Source = source
        };
    }
}

/// <summary>
/// State data for damage-over-time effects.
/// </summary>
public class DotEffectState
{
    [JsonPropertyName("damagePerRound")]
    public int DamagePerRound { get; set; }

    [JsonPropertyName("damageType")]
    public DamageType DamageType { get; set; }

    [JsonPropertyName("damageTarget")]
    public string DamageTarget { get; set; } = "FAT"; // "FAT" or "VIT"
}
