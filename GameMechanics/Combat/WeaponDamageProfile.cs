using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace GameMechanics.Combat;

/// <summary>
/// Strongly-typed wrapper around per-damage-type SV modifiers.
/// Supports multiple simultaneous damage types (e.g., a flaming sword with Cutting +4 AND Energy +2).
/// JSON format mirrors ArmorAbsorption: {"Cutting": 4, "Energy": 2}
/// </summary>
public class WeaponDamageProfile
{
  private readonly Dictionary<DamageType, int> _modifiers;

  public WeaponDamageProfile()
  {
    _modifiers = new Dictionary<DamageType, int>();
  }

  public WeaponDamageProfile(Dictionary<DamageType, int> modifiers)
  {
    _modifiers = new Dictionary<DamageType, int>(modifiers);
  }

  /// <summary>
  /// Gets the SV modifier for a specific damage type. Returns 0 if not present.
  /// </summary>
  public int this[DamageType type] => _modifiers.TryGetValue(type, out var val) ? val : 0;

  /// <summary>
  /// The primary damage type (highest modifier). Used for display and backwards compat.
  /// </summary>
  public DamageType PrimaryDamageType =>
    _modifiers.Count == 0
      ? DamageType.Bashing
      : _modifiers.OrderByDescending(kv => kv.Value).First().Key;

  /// <summary>
  /// The primary SV modifier (highest value). Used for backwards compat.
  /// </summary>
  public int PrimarySVModifier =>
    _modifiers.Count == 0 ? 0 : _modifiers.Values.Max();

  /// <summary>
  /// Whether this profile has any non-zero damage types.
  /// </summary>
  public bool HasDamage => _modifiers.Any(kv => kv.Value != 0);

  /// <summary>
  /// Whether this profile has multiple non-zero damage types.
  /// </summary>
  public bool IsMultiType => GetNonZeroTypes().Count() > 1;

  /// <summary>
  /// Returns all damage types with non-zero modifiers.
  /// </summary>
  public IEnumerable<KeyValuePair<DamageType, int>> GetNonZeroTypes()
  {
    return _modifiers.Where(kv => kv.Value != 0).OrderByDescending(kv => kv.Value);
  }

  /// <summary>
  /// Returns all entries including zero values.
  /// </summary>
  public IReadOnlyDictionary<DamageType, int> GetAll() => _modifiers;

  /// <summary>
  /// Merges this profile with another (e.g., weapon + ammo). SV modifiers are summed per type.
  /// </summary>
  public WeaponDamageProfile MergeWith(WeaponDamageProfile other)
  {
    var merged = new Dictionary<DamageType, int>(_modifiers);
    foreach (var kv in other._modifiers)
    {
      if (merged.ContainsKey(kv.Key))
        merged[kv.Key] += kv.Value;
      else
        merged[kv.Key] = kv.Value;
    }
    return new WeaponDamageProfile(merged);
  }

  /// <summary>
  /// Parses from JSON like {"Cutting": 4, "Energy": 2}.
  /// </summary>
  public static WeaponDamageProfile? FromJson(string? json)
  {
    if (string.IsNullOrWhiteSpace(json))
      return null;

    try
    {
      var dict = JsonSerializer.Deserialize<Dictionary<string, int>>(json);
      if (dict == null || dict.Count == 0)
        return null;

      var modifiers = new Dictionary<DamageType, int>();
      foreach (var kv in dict)
      {
        if (Enum.TryParse<DamageType>(kv.Key, ignoreCase: true, out var damageType))
        {
          modifiers[damageType] = kv.Value;
        }
      }
      return modifiers.Count > 0 ? new WeaponDamageProfile(modifiers) : null;
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
  /// Serializes to JSON like {"Cutting": 4, "Energy": 2}.
  /// </summary>
  public string ToJson()
  {
    var dict = new Dictionary<string, int>();
    foreach (var kv in _modifiers.Where(kv => kv.Value != 0))
    {
      dict[kv.Key.ToString()] = kv.Value;
    }
    return JsonSerializer.Serialize(dict);
  }

  /// <summary>
  /// Returns a display string like "Cutting +4, Energy +2".
  /// </summary>
  public string ToDisplayString()
  {
    var types = GetNonZeroTypes().ToList();
    if (types.Count == 0)
      return "None";

    return string.Join(", ", types.Select(kv =>
      kv.Value >= 0 ? $"{kv.Key} +{kv.Value}" : $"{kv.Key} {kv.Value}"));
  }

  public override string ToString() => ToDisplayString();
}
