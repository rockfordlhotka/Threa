using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace GameMechanics.Combat;

/// <summary>
/// Strongly-typed wrapper around per-damage-type weapon entries.
/// Supports multiple simultaneous damage types (e.g., a flaming sword with Cutting +4 AND Energy +2).
/// Extended JSON format supports AP offset and SV max per type:
///   Legacy: {"Cutting": 4, "Energy": 2}
///   Extended: {"Cutting": {"sv": 4, "ap": 5, "svMax": 8}}
///   Mixed: {"Cutting": {"sv": 4, "ap": 5}, "Energy": 2}
/// </summary>
public class WeaponDamageProfile
{
  private readonly Dictionary<DamageType, DamageTypeEntry> _entries;

  public WeaponDamageProfile()
  {
    _entries = new Dictionary<DamageType, DamageTypeEntry>();
  }

  public WeaponDamageProfile(Dictionary<DamageType, int> modifiers)
  {
    _entries = modifiers.ToDictionary(kv => kv.Key, kv => new DamageTypeEntry(kv.Value));
  }

  public WeaponDamageProfile(Dictionary<DamageType, DamageTypeEntry> entries)
  {
    _entries = new Dictionary<DamageType, DamageTypeEntry>(entries);
  }

  /// <summary>
  /// Gets the SV modifier for a specific damage type. Returns 0 if not present.
  /// </summary>
  public int this[DamageType type] => _entries.TryGetValue(type, out var entry) ? entry.SvModifier : 0;

  /// <summary>
  /// Gets the full DamageTypeEntry for a specific damage type.
  /// Returns a default entry (SvModifier=0) if not present.
  /// </summary>
  public DamageTypeEntry GetEntry(DamageType type) =>
    _entries.TryGetValue(type, out var entry) ? entry : new DamageTypeEntry(0);

  /// <summary>
  /// Gets the AP offset for a specific damage type. Returns 0 if not present.
  /// </summary>
  public int GetApOffset(DamageType type) => GetEntry(type).ApOffset;

  /// <summary>
  /// Gets the SV max for a specific damage type. Returns null if not present.
  /// </summary>
  public int? GetSvMax(DamageType type) => GetEntry(type).SvMax;

  /// <summary>
  /// The primary damage type (highest modifier). Used for display and backwards compat.
  /// </summary>
  public DamageType PrimaryDamageType =>
    _entries.Count == 0
      ? DamageType.Bashing
      : _entries.OrderByDescending(kv => kv.Value.SvModifier).First().Key;

  /// <summary>
  /// The primary SV modifier (highest value). Used for backwards compat.
  /// </summary>
  public int PrimarySVModifier =>
    _entries.Count == 0 ? 0 : _entries.Values.Max(e => e.SvModifier);

  /// <summary>
  /// Whether this profile has any non-zero damage types.
  /// </summary>
  public bool HasDamage => _entries.Any(kv => kv.Value.SvModifier != 0);

  /// <summary>
  /// Whether this profile has multiple non-zero damage types.
  /// </summary>
  public bool IsMultiType => GetNonZeroTypes().Count() > 1;

  /// <summary>
  /// Returns all damage types with non-zero modifiers.
  /// </summary>
  public IEnumerable<KeyValuePair<DamageType, int>> GetNonZeroTypes()
  {
    return _entries
      .Where(kv => kv.Value.SvModifier != 0)
      .Select(kv => new KeyValuePair<DamageType, int>(kv.Key, kv.Value.SvModifier))
      .OrderByDescending(kv => kv.Value);
  }

  /// <summary>
  /// Returns all entries including zero values as int modifiers (backwards compat).
  /// </summary>
  public IReadOnlyDictionary<DamageType, int> GetAll() =>
    _entries.ToDictionary(kv => kv.Key, kv => kv.Value.SvModifier);

  /// <summary>
  /// Merges this profile with another (e.g., weapon + ammo).
  /// SV modifiers are summed, AP offsets are summed, SV max takes minimum non-null value.
  /// </summary>
  public WeaponDamageProfile MergeWith(WeaponDamageProfile other)
  {
    var merged = new Dictionary<DamageType, DamageTypeEntry>(_entries);
    foreach (var kv in other._entries)
    {
      if (merged.TryGetValue(kv.Key, out var existing))
      {
        int mergedSv = existing.SvModifier + kv.Value.SvModifier;
        int mergedAp = existing.ApOffset + kv.Value.ApOffset;
        int? mergedSvMax = MergeSvMax(existing.SvMax, kv.Value.SvMax);
        merged[kv.Key] = new DamageTypeEntry(mergedSv, mergedAp, mergedSvMax);
      }
      else
      {
        merged[kv.Key] = kv.Value;
      }
    }
    return new WeaponDamageProfile(merged);
  }

  private static int? MergeSvMax(int? a, int? b)
  {
    if (a == null) return b;
    if (b == null) return a;
    return Math.Min(a.Value, b.Value);
  }

  /// <summary>
  /// Parses from JSON. Supports legacy int format and extended object format:
  ///   Legacy: {"Cutting": 4}
  ///   Extended: {"Cutting": {"sv": 4, "ap": 5, "svMax": 8}}
  ///   Mixed: {"Cutting": {"sv": 4, "ap": 5}, "Energy": 2}
  /// </summary>
  public static WeaponDamageProfile? FromJson(string? json)
  {
    if (string.IsNullOrWhiteSpace(json))
      return null;

    try
    {
      using var doc = JsonDocument.Parse(json);
      var root = doc.RootElement;
      if (root.ValueKind != JsonValueKind.Object)
        return null;

      var entries = new Dictionary<DamageType, DamageTypeEntry>();
      foreach (var prop in root.EnumerateObject())
      {
        if (!Enum.TryParse<DamageType>(prop.Name, ignoreCase: true, out var damageType))
          continue;

        if (prop.Value.ValueKind == JsonValueKind.Number)
        {
          // Legacy int format
          entries[damageType] = new DamageTypeEntry(prop.Value.GetInt32());
        }
        else if (prop.Value.ValueKind == JsonValueKind.Object)
        {
          // Extended object format
          int sv = prop.Value.TryGetProperty("sv", out var svProp) ? svProp.GetInt32() : 0;
          int ap = prop.Value.TryGetProperty("ap", out var apProp) ? apProp.GetInt32() : 0;
          int? svMax = prop.Value.TryGetProperty("svMax", out var svMaxProp)
            ? svMaxProp.GetInt32()
            : null;
          entries[damageType] = new DamageTypeEntry(sv, ap, svMax);
        }
      }

      return entries.Count > 0 ? new WeaponDamageProfile(entries) : null;
    }
    catch
    {
      return null;
    }
  }

  /// <summary>
  /// Creates a profile from legacy single-type data.
  /// </summary>
  public static WeaponDamageProfile? FromLegacy(string? damageType, int svModifier)
  {
    if (string.IsNullOrWhiteSpace(damageType) && svModifier == 0)
      return null;

    if (Enum.TryParse<DamageType>(damageType, ignoreCase: true, out var dt))
    {
      return new WeaponDamageProfile(new Dictionary<DamageType, int> { { dt, svModifier } });
    }

    // If we have an SV modifier but no valid type, default to Bashing
    if (svModifier != 0)
    {
      return new WeaponDamageProfile(new Dictionary<DamageType, int> { { DamageType.Bashing, svModifier } });
    }

    return null;
  }

  /// <summary>
  /// Creates a profile from a single damage type and modifier.
  /// </summary>
  public static WeaponDamageProfile FromSingle(DamageType damageType, int svModifier)
  {
    return new WeaponDamageProfile(new Dictionary<DamageType, int> { { damageType, svModifier } });
  }

  /// <summary>
  /// Serializes to JSON. Entries with only SvModifier use legacy int format;
  /// entries with AP offset or SV max use extended object format.
  /// </summary>
  public string ToJson()
  {
    var dict = new Dictionary<string, object>();
    foreach (var kv in _entries.Where(kv => kv.Value.SvModifier != 0 || kv.Value.ApOffset != 0 || kv.Value.SvMax != null))
    {
      if (kv.Value.ApOffset == 0 && kv.Value.SvMax == null)
      {
        // Legacy compact format
        dict[kv.Key.ToString()] = kv.Value.SvModifier;
      }
      else
      {
        // Extended object format
        var entry = new Dictionary<string, object> { { "sv", kv.Value.SvModifier } };
        if (kv.Value.ApOffset != 0)
          entry["ap"] = kv.Value.ApOffset;
        if (kv.Value.SvMax != null)
          entry["svMax"] = kv.Value.SvMax.Value;
        dict[kv.Key.ToString()] = entry;
      }
    }
    return JsonSerializer.Serialize(dict);
  }

  /// <summary>
  /// Returns a display string like "Cutting +4, Energy +2" or "Piercing +3 (AP 5)".
  /// </summary>
  public string ToDisplayString()
  {
    var nonZero = _entries
      .Where(kv => kv.Value.SvModifier != 0 || kv.Value.ApOffset != 0 || kv.Value.SvMax != null)
      .OrderByDescending(kv => kv.Value.SvModifier)
      .ToList();

    if (nonZero.Count == 0)
      return "None";

    return string.Join(", ", nonZero.Select(kv =>
    {
      var entry = kv.Value;
      var svPart = entry.SvModifier >= 0 ? $"{kv.Key} +{entry.SvModifier}" : $"{kv.Key} {entry.SvModifier}";

      var extras = new List<string>();
      if (entry.ApOffset != 0)
        extras.Add($"AP {entry.ApOffset}");
      if (entry.SvMax != null)
        extras.Add($"SvMax {entry.SvMax}");

      return extras.Count > 0 ? $"{svPart} ({string.Join(", ", extras)})" : svPart;
    }));
  }

  public override string ToString() => ToDisplayString();
}
