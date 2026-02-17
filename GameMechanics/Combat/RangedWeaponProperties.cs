using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameMechanics.Combat;

/// <summary>
/// Strongly-typed properties for ranged weapons stored in ItemTemplate.CustomProperties.
/// </summary>
public class RangedWeaponProperties
{
    /// <summary>Whether this item is a ranged weapon.</summary>
    [JsonPropertyName("isRangedWeapon")]
    public bool IsRangedWeapon { get; set; }

    /// <summary>
    /// Whether this is a thrown weapon where the weapon itself is consumed when thrown.
    /// Examples: throwing knife, javelin, shuriken, grenade.
    /// When true, the weapon should be stackable and throwing consumes one from the stack.
    /// Capacity/AmmoType are ignored for thrown weapons.
    /// </summary>
    [JsonPropertyName("isThrown")]
    public bool IsThrown { get; set; }

    /// <summary>
    /// The skill used when firing/throwing this weapon at range.
    /// Examples: "Archery" for bows, "Pistols" for handguns, "Throwing - Light" for throwing knives.
    /// The melee skill is stored in ItemTemplate.RelatedSkill.
    /// </summary>
    [JsonPropertyName("rangedSkill")]
    public string? RangedSkill { get; set; }

    /// <summary>Short range distance (base TV 8).</summary>
    [JsonPropertyName("shortRange")]
    public int ShortRange { get; set; }

    /// <summary>Medium range distance (base TV 10).</summary>
    [JsonPropertyName("mediumRange")]
    public int MediumRange { get; set; }

    /// <summary>Long range distance (base TV 12).</summary>
    [JsonPropertyName("longRange")]
    public int LongRange { get; set; }

    /// <summary>Extreme range distance (base TV 14).</summary>
    [JsonPropertyName("extremeRange")]
    public int ExtremeRange { get; set; }

    /// <summary>Magazine/internal capacity (not including chamber).</summary>
    [JsonPropertyName("capacity")]
    public int Capacity { get; set; }

    /// <summary>Chamber capacity (typically 0 or 1).</summary>
    [JsonPropertyName("chamberCapacity")]
    public int ChamberCapacity { get; set; }

    /// <summary>How the weapon is reloaded.</summary>
    [JsonPropertyName("reloadType")]
    public string ReloadType { get; set; } = "Magazine";

    /// <summary>Whether the weapon can accept loose ammo directly (arrows, individual rounds).</summary>
    [JsonPropertyName("acceptsLooseAmmo")]
    public bool AcceptsLooseAmmo { get; set; }

    /// <summary>Type of ammunition this weapon uses (9mm, .45, Arrow, etc.).</summary>
    [JsonPropertyName("ammoType")]
    public string? AmmoType { get; set; }

    /// <summary>Available fire modes for this weapon.</summary>
    [JsonPropertyName("fireModes")]
    public List<string> FireModes { get; set; } = new() { "Single" };

    /// <summary>Number of rounds in a burst (for burst fire mode).</summary>
    [JsonPropertyName("burstSize")]
    public int BurstSize { get; set; } = 3;

    /// <summary>Number of rounds for suppressive fire.</summary>
    [JsonPropertyName("suppressiveRounds")]
    public int SuppressiveRounds { get; set; } = 10;

    /// <summary>Whether targets can attempt to dodge this weapon's attacks.</summary>
    [JsonPropertyName("isDodgeable")]
    public bool IsDodgeable { get; set; }

    /// <summary>Base SV modifier added to successful hits (legacy single-type).</summary>
    [JsonPropertyName("baseSVModifier")]
    public int BaseSVModifier { get; set; }

    /// <summary>Alias for BaseSVModifier for compatibility.</summary>
    [JsonPropertyName("damageModifier")]
    public int DamageModifier { get => BaseSVModifier; set => BaseSVModifier = value; }

    /// <summary>
    /// Per-damage-type SV modifiers for this ranged weapon.
    /// Format: {"Projectile": 4, "Energy": 2}
    /// When set, takes precedence over BaseSVModifier.
    /// </summary>
    [JsonPropertyName("damageModifiers")]
    public Dictionary<string, int>? DamageModifiers { get; set; }

    /// <summary>AV modifier from weapon quality (positive = more accurate).</summary>
    [JsonPropertyName("accuracyModifier")]
    public int AccuracyModifier { get; set; }

    // ========== Inherent AOE Properties (for grenades, rockets, etc.) ==========

    /// <summary>Whether the weapon itself causes area of effect damage (e.g., grenade, rocket).</summary>
    [JsonPropertyName("isInherentAOE")]
    public bool IsInherentAOE { get; set; }

    /// <summary>Default blast radius in meters (used if ammo doesn't specify).</summary>
    [JsonPropertyName("defaultBlastRadius")]
    public int DefaultBlastRadius { get; set; }

    /// <summary>Default blast falloff: "Linear", "Steep", or "Flat" (used if ammo doesn't specify).</summary>
    [JsonPropertyName("defaultBlastFalloff")]
    public string? DefaultBlastFalloff { get; set; }

    /// <summary>Total capacity including chamber.</summary>
    public int TotalCapacity => Capacity + ChamberCapacity;

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
        if (BaseSVModifier != 0)
            return WeaponDamageProfile.FromSingle(DamageType.Projectile, BaseSVModifier);

        return null;
    }

    /// <summary>
    /// Gets the parsed reload type enum.
    /// </summary>
    public ReloadType GetReloadType()
    {
        return ReloadType?.ToLowerInvariant() switch
        {
            "none" => Combat.ReloadType.None,
            "magazine" => Combat.ReloadType.Magazine,
            "singleround" => Combat.ReloadType.SingleRound,
            "cylinder" => Combat.ReloadType.Cylinder,
            "belt" => Combat.ReloadType.Belt,
            "battery" => Combat.ReloadType.Battery,
            _ => Combat.ReloadType.Magazine
        };
    }

    /// <summary>
    /// Gets the available fire modes as enums.
    /// Note: AOE is no longer a fire mode - it's determined by weapon/ammo properties.
    /// </summary>
    public IEnumerable<FireMode> GetFireModes()
    {
        foreach (var mode in FireModes)
        {
            var fireMode = mode?.ToLowerInvariant() switch
            {
                "single" => FireMode.Single,
                "burst" => FireMode.Burst,
                "suppression" => FireMode.Suppression,
                "aoe" => (FireMode?)null, // Skip legacy AOE entries - it's now a property
                _ => FireMode.Single
            };
            if (fireMode.HasValue)
                yield return fireMode.Value;
        }
    }

    /// <summary>
    /// Checks if a specific fire mode is available.
    /// </summary>
    public bool HasFireMode(FireMode mode)
    {
        var modeName = mode.ToString();
        return FireModes.Any(m => m.Equals(modeName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Deserializes from JSON string.
    /// </summary>
    public static RangedWeaponProperties? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var props = JsonSerializer.Deserialize<RangedWeaponProperties>(json);
            return props?.IsRangedWeapon == true ? props : null;
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
