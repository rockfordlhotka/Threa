using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameMechanics.Combat;

/// <summary>
/// Tracks the ammunition state of a magazine or ammo container.
/// Stored in CharacterItem.CustomProperties for magazine instances.
/// </summary>
public class AmmoContainerState
{
    /// <summary>Number of rounds currently in the container.</summary>
    [JsonPropertyName("loadedAmmo")]
    public int LoadedAmmo { get; set; }

    /// <summary>Maximum capacity of the container.</summary>
    [JsonPropertyName("maxCapacity")]
    public int MaxCapacity { get; set; }

    /// <summary>Type of ammo in this container (e.g., "9mm", "Arrow").</summary>
    [JsonPropertyName("ammoType")]
    public string? AmmoType { get; set; }

    /// <summary>
    /// Whether the container is full.
    /// </summary>
    public bool IsFull => LoadedAmmo >= MaxCapacity;

    /// <summary>
    /// Whether the container is empty.
    /// </summary>
    public bool IsEmpty => LoadedAmmo <= 0;

    /// <summary>
    /// Space available in the container.
    /// </summary>
    public int SpaceAvailable => MaxCapacity - LoadedAmmo;

    /// <summary>
    /// Removes ammo from the container.
    /// </summary>
    /// <param name="count">Number of rounds to remove.</param>
    /// <returns>Actual number of rounds removed.</returns>
    public int RemoveAmmo(int count)
    {
        int toRemove = Math.Min(count, LoadedAmmo);
        LoadedAmmo -= toRemove;
        return toRemove;
    }

    /// <summary>
    /// Adds ammo to the container.
    /// </summary>
    /// <param name="count">Number of rounds to add.</param>
    /// <returns>Actual number of rounds added.</returns>
    public int AddAmmo(int count)
    {
        int toAdd = Math.Min(count, SpaceAvailable);
        LoadedAmmo += toAdd;
        return toAdd;
    }

    /// <summary>
    /// Deserializes from JSON string.
    /// </summary>
    public static AmmoContainerState FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new AmmoContainerState();

        try
        {
            return JsonSerializer.Deserialize<AmmoContainerState>(json) ?? new AmmoContainerState();
        }
        catch
        {
            return new AmmoContainerState();
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

    /// <summary>
    /// Merges this state into existing CustomProperties JSON.
    /// </summary>
    public static string MergeIntoCustomProperties(string? existingJson, AmmoContainerState state)
    {
        Dictionary<string, object>? existing = null;
        if (!string.IsNullOrWhiteSpace(existingJson))
        {
            try
            {
                existing = JsonSerializer.Deserialize<Dictionary<string, object>>(existingJson);
            }
            catch { }
        }

        existing ??= new Dictionary<string, object>();

        existing["loadedAmmo"] = state.LoadedAmmo;
        existing["maxCapacity"] = state.MaxCapacity;
        if (state.AmmoType != null)
            existing["ammoType"] = state.AmmoType;

        return JsonSerializer.Serialize(existing);
    }
}
