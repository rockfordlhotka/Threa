using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GameMechanics.Combat;

/// <summary>
/// Tracks the current ammunition state of a weapon instance.
/// Stored in CharacterItem.CustomProperties.
/// </summary>
public class WeaponAmmoState
{
    /// <summary>Number of rounds currently loaded in the weapon.</summary>
    [JsonPropertyName("loadedAmmo")]
    public int LoadedAmmo { get; set; }

    /// <summary>Type of ammo currently loaded (e.g., "Standard", "AP", "Incendiary").</summary>
    [JsonPropertyName("loadedAmmoType")]
    public string? LoadedAmmoType { get; set; }

    /// <summary>Whether there is a round in the chamber (for weapons with +1 chamber capacity).</summary>
    [JsonPropertyName("chamberLoaded")]
    public bool ChamberLoaded { get; set; }

    /// <summary>ID of the currently loaded magazine (if applicable).</summary>
    [JsonPropertyName("loadedMagazineId")]
    public Guid? LoadedMagazineId { get; set; }

    /// <summary>
    /// Gets the total available rounds (loaded + chamber if applicable).
    /// </summary>
    public int TotalAvailable => LoadedAmmo + (ChamberLoaded ? 1 : 0);

    /// <summary>
    /// Consumes ammo from the weapon.
    /// </summary>
    /// <param name="count">Number of rounds to consume.</param>
    /// <returns>True if enough ammo was available, false otherwise.</returns>
    public bool ConsumeAmmo(int count)
    {
        if (TotalAvailable < count)
            return false;

        // Consume from chamber first, then magazine
        int remaining = count;
        if (ChamberLoaded && remaining > 0)
        {
            ChamberLoaded = false;
            remaining--;
        }

        LoadedAmmo -= remaining;
        return true;
    }

    /// <summary>
    /// Loads ammo into the weapon.
    /// </summary>
    /// <param name="count">Number of rounds to load.</param>
    /// <param name="capacity">Maximum capacity of the weapon.</param>
    /// <param name="chamberCapacity">Chamber capacity (0 or 1 typically).</param>
    /// <param name="ammoType">Type of ammo being loaded.</param>
    /// <returns>Number of rounds actually loaded.</returns>
    public int LoadAmmo(int count, int capacity, int chamberCapacity, string? ammoType)
    {
        int loaded = 0;

        // Load chamber first if empty and has capacity
        if (!ChamberLoaded && chamberCapacity > 0 && count > 0)
        {
            ChamberLoaded = true;
            loaded++;
            count--;
        }

        // Load remaining into magazine
        int spaceInMagazine = capacity - LoadedAmmo;
        int toLoad = Math.Min(count, spaceInMagazine);
        LoadedAmmo += toLoad;
        loaded += toLoad;

        if (loaded > 0 && ammoType != null)
            LoadedAmmoType = ammoType;

        return loaded;
    }

    /// <summary>
    /// Checks if the weapon is empty.
    /// </summary>
    public bool IsEmpty => LoadedAmmo == 0 && !ChamberLoaded;

    /// <summary>
    /// Deserializes from JSON string.
    /// </summary>
    public static WeaponAmmoState FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new WeaponAmmoState();

        try
        {
            return JsonSerializer.Deserialize<WeaponAmmoState>(json) ?? new WeaponAmmoState();
        }
        catch
        {
            return new WeaponAmmoState();
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
    /// Merges this ammo state into existing CustomProperties JSON.
    /// </summary>
    public static string MergeIntoCustomProperties(string? existingJson, WeaponAmmoState state)
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
        existing["chamberLoaded"] = state.ChamberLoaded;
        if (state.LoadedAmmoType != null)
            existing["loadedAmmoType"] = state.LoadedAmmoType;
        if (state.LoadedMagazineId.HasValue)
            existing["loadedMagazineId"] = state.LoadedMagazineId.Value.ToString();
        else
            existing.Remove("loadedMagazineId");

        return JsonSerializer.Serialize(existing);
    }
}
