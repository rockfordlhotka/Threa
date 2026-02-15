using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using GameMechanics.Combat.Effects;

namespace GameMechanics.Combat;

/// <summary>
/// Properties for ammunition items stored in ItemTemplate.CustomProperties.
/// </summary>
public class AmmunitionProperties
{
    /// <summary>Whether this item is ammunition.</summary>
    [JsonPropertyName("isAmmunition")]
    public bool IsAmmunition { get; set; }

    /// <summary>Type of ammunition (e.g., "9mm", ".45", "Arrow", "Bolt").</summary>
    [JsonPropertyName("ammoType")]
    public string? AmmoType { get; set; }

    /// <summary>Whether this is a container (magazine, quiver) vs loose ammo (bullets, arrows).</summary>
    [JsonPropertyName("isContainer")]
    public bool IsContainer { get; set; }

    /// <summary>Capacity if this is a container (magazine capacity).</summary>
    [JsonPropertyName("containerCapacity")]
    public int ContainerCapacity { get; set; }

    /// <summary>Damage modifier applied to hits with this ammo (legacy single-type).</summary>
    [JsonPropertyName("damageModifier")]
    public int DamageModifier { get; set; }

    /// <summary>
    /// Per-damage-type SV modifiers for this ammo.
    /// Format: {"Energy": 2, "Piercing": 1}
    /// When set, takes precedence over DamageModifier.
    /// </summary>
    [JsonPropertyName("damageModifiers")]
    public Dictionary<string, int>? DamageModifiers { get; set; }

    /// <summary>Accuracy modifier (AV) applied to attack rolls with this ammo. Cumulative with weapon AV.</summary>
    [JsonPropertyName("accuracyModifier")]
    public int AccuracyModifier { get; set; }

    /// <summary>Penetration modifier (reduces armor effectiveness).</summary>
    [JsonPropertyName("penetrationModifier")]
    public int PenetrationModifier { get; set; }

    /// <summary>Special effect applied on hit (e.g., "Incendiary", "Hollow-Point", "Tracer").</summary>
    [JsonPropertyName("specialEffect")]
    public string? SpecialEffect { get; set; }

    /// <summary>Whether this ammo is compatible with energy weapons.</summary>
    [JsonPropertyName("isEnergyAmmo")]
    public bool IsEnergyAmmo { get; set; }

    // ========== AOE Properties ==========

    /// <summary>Whether this ammunition causes area of effect damage (e.g., HE rounds, explosive arrows).</summary>
    [JsonPropertyName("isAOE")]
    public bool IsAOE { get; set; }

    /// <summary>Blast radius in meters for AOE ammo.</summary>
    [JsonPropertyName("blastRadius")]
    public int BlastRadius { get; set; }

    /// <summary>How damage decreases with distance: "Linear", "Steep", or "Flat".</summary>
    [JsonPropertyName("blastFalloff")]
    public string? BlastFalloff { get; set; }

    /// <summary>Extra SV bonus for direct hit target (center of blast).</summary>
    [JsonPropertyName("directHitBonus")]
    public int DirectHitBonus { get; set; }

    // ========== Attack Effect Properties ==========

    /// <summary>
    /// Structured on-hit effect definition. Takes precedence over SpecialEffect if both are set.
    /// </summary>
    [JsonPropertyName("onHitEffect")]
    public AttackEffectGrant? OnHitEffect { get; set; }

    /// <summary>
    /// Gets the parsed on-hit effect, either from the structured OnHitEffect property
    /// or by parsing the legacy SpecialEffect string.
    /// </summary>
    public AttackEffectGrant? GetOnHitEffect()
    {
        // Prefer structured effect if defined
        if (OnHitEffect != null)
            return OnHitEffect;

        // Fall back to parsing SpecialEffect string for backwards compatibility
        return GetParsedSpecialEffect();
    }

    /// <summary>
    /// Parses the SpecialEffect string into a structured AttackEffectGrant.
    /// Used for backwards compatibility with ammo that only has SpecialEffect defined.
    /// </summary>
    public AttackEffectGrant? GetParsedSpecialEffect()
    {
        if (string.IsNullOrEmpty(SpecialEffect))
            return null;

        return SpecialEffect.ToUpperInvariant() switch
        {
            "INCENDIARY" => AttackEffectGrant.CreateDotEffect(
                "Burning",
                damagePerRound: 2,
                DamageType.Energy,
                durationRounds: 3,
                AmmoType ?? "Incendiary Ammo"),

            "CRYO" or "FREEZING" => AttackEffectGrant.CreateDebuff(
                "Chilled",
                "Movement and actions slowed",
                JsonSerializer.Serialize(new { MovementPenalty = -2, APPenalty = -1 }),
                durationRounds: 2,
                AmmoType ?? "Cryo Ammo"),

            "SHOCK" or "ELECTRIC" => new AttackEffectGrant
            {
                EffectName = "Shocked",
                Description = "Electrical damage and potential stun",
                EffectType = Threa.Dal.Dto.EffectType.Debuff,
                BonusDamage = 1,
                BonusDamageType = DamageType.Energy,
                DurationRounds = 1,
                Source = AmmoType ?? "Shock Ammo"
            },

            "HOLLOW-POINT" or "HOLLOWPOINT" => new AttackEffectGrant
            {
                EffectName = "Expanded Wound",
                Description = "Increased damage to unarmored targets",
                BonusDamage = 2,
                BonusDamageType = DamageType.Projectile,
                Source = AmmoType ?? "Hollow-Point Ammo"
            },

            "EXPLOSIVE" or "HE" => new AttackEffectGrant
            {
                EffectName = "Explosive",
                Description = "Area damage",
                BonusDamage = 3,
                BonusDamageType = DamageType.Bashing,
                Source = AmmoType ?? "Explosive Ammo"
            },

            // Armor-piercing and tracer don't create effects (handled by penetration modifier)
            _ => null
        };
    }

    /// <summary>
    /// Gets a WeaponDamageProfile from per-type modifiers or legacy single-type modifier.
    /// </summary>
    public WeaponDamageProfile? GetWeaponDamageProfile()
    {
        if (DamageModifiers != null && DamageModifiers.Count > 0)
        {
            var modifiers = new Dictionary<DamageType, int>();
            foreach (var kv in DamageModifiers)
            {
                if (Enum.TryParse<DamageType>(kv.Key, ignoreCase: true, out var dt))
                    modifiers[dt] = kv.Value;
            }
            if (modifiers.Count > 0)
                return new WeaponDamageProfile(modifiers);
        }

        // Fall back to legacy single modifier (applied to Projectile by default)
        if (DamageModifier != 0)
            return WeaponDamageProfile.FromSingle(DamageType.Projectile, DamageModifier);

        return null;
    }

    /// <summary>
    /// Deserializes from JSON string.
    /// Returns the properties if this represents ammunition (IsAmmunition is true, 
    /// or if AmmoType is specified which implies it's ammunition).
    /// </summary>
    public static AmmunitionProperties? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var props = JsonSerializer.Deserialize<AmmunitionProperties>(json);
            // Consider it ammunition if explicitly marked OR if it has an ammo type defined
            if (props?.IsAmmunition == true || !string.IsNullOrEmpty(props?.AmmoType))
                return props;
            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Serializes to JSON string.
    /// </summary>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }
}
