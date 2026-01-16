using System;
using System.Text.Json;
using System.Text.Json.Serialization;

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

    /// <summary>Damage modifier applied to hits with this ammo.</summary>
    [JsonPropertyName("damageModifier")]
    public int DamageModifier { get; set; }

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

    /// <summary>
    /// Deserializes from JSON string.
    /// </summary>
    public static AmmunitionProperties? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var props = JsonSerializer.Deserialize<AmmunitionProperties>(json);
            return props?.IsAmmunition == true ? props : null;
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
