using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameMechanics.Combat;

/// <summary>
/// Properties for ammo container items (magazines, quivers, speedloaders) stored in ItemTemplate.CustomProperties.
/// </summary>
public class AmmoContainerProperties
{
    /// <summary>Whether this item is an ammo container.</summary>
    [JsonPropertyName("isAmmoContainer")]
    public bool IsAmmoContainer { get; set; } = true;

    /// <summary>Type of ammunition this container holds (e.g., "9mm", "Arrow").</summary>
    [JsonPropertyName("ammoType")]
    public string AmmoType { get; set; } = string.Empty;

    /// <summary>Maximum number of rounds this container can hold.</summary>
    [JsonPropertyName("capacity")]
    public int Capacity { get; set; }

    /// <summary>Type of container (Magazine, Quiver, Speedloader, Belt, Clip).</summary>
    [JsonPropertyName("containerType")]
    public string ContainerType { get; set; } = "Magazine";

    /// <summary>
    /// Comma-separated list of allowed ammo types.
    /// Null defaults to same as AmmoType (only accepts exact match).
    /// </summary>
    [JsonPropertyName("allowedAmmoTypes")]
    public string? AllowedAmmoTypes { get; set; }

    /// <summary>
    /// Gets the list of allowed ammo types for this container.
    /// If AllowedAmmoTypes is null or empty, returns just the primary AmmoType.
    /// </summary>
    public IEnumerable<string> GetAllowedAmmoTypes()
    {
        var allowedTypes = AllowedAmmoTypes ?? AmmoType;
        return allowedTypes.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    }

    /// <summary>
    /// Checks if this container can hold the specified ammo type.
    /// </summary>
    public bool CanHoldAmmoType(string ammoType)
    {
        return GetAllowedAmmoTypes().Contains(ammoType, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Deserializes from JSON string.
    /// </summary>
    public static AmmoContainerProperties? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            var props = JsonSerializer.Deserialize<AmmoContainerProperties>(json);
            return props?.IsAmmoContainer == true ? props : null;
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

/// <summary>
/// Types of ammo containers.
/// </summary>
public static class AmmoContainerType
{
    public const string Magazine = "Magazine";
    public const string Quiver = "Quiver";
    public const string Speedloader = "Speedloader";
    public const string Belt = "Belt";
    public const string Clip = "Clip";
    public const string PowerCell = "PowerCell";

    public static readonly string[] ValidTypes = [Magazine, Quiver, Speedloader, Belt, Clip, PowerCell];

    public static bool IsValid(string type)
    {
        return ValidTypes.Contains(type, StringComparer.OrdinalIgnoreCase);
    }
}
