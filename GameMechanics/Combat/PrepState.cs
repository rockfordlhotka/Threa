using System.Collections.Generic;

namespace GameMechanics.Combat
{
  /// <summary>
  /// Tracks prepped ammunition/items for fast loading.
  /// Prepped items can be loaded instantly (no cooldown).
  /// </summary>
  public class PrepState
  {
    private readonly Dictionary<string, int> _preppedItems = new();

    /// <summary>
    /// Gets the count of a specific prepped item type.
    /// </summary>
    public int GetPreppedCount(string itemType)
    {
      return _preppedItems.TryGetValue(itemType, out int count) ? count : 0;
    }

    /// <summary>
    /// Gets all prepped item types and counts.
    /// </summary>
    public IReadOnlyDictionary<string, int> AllPrepped => _preppedItems;

    /// <summary>
    /// Preps an item for fast use.
    /// Each prep action costs 1 AP + 1 FAT (or 2 AP).
    /// There is no cooldown between prep actions.
    /// </summary>
    /// <param name="itemType">The type of item being prepped (e.g., "Arrow", "Magazine").</param>
    /// <returns>The new count of prepped items of this type.</returns>
    public int PrepItem(string itemType)
    {
      if (!_preppedItems.ContainsKey(itemType))
        _preppedItems[itemType] = 0;

      _preppedItems[itemType]++;
      return _preppedItems[itemType];
    }

    /// <summary>
    /// Uses a prepped item (e.g., loading an arrow).
    /// Returns true if a prepped item was available and consumed.
    /// </summary>
    public bool UsePreppedItem(string itemType)
    {
      if (!_preppedItems.TryGetValue(itemType, out int count) || count <= 0)
        return false;

      _preppedItems[itemType]--;
      if (_preppedItems[itemType] <= 0)
        _preppedItems.Remove(itemType);

      return true;
    }

    /// <summary>
    /// Checks if a prepped item is available.
    /// </summary>
    public bool HasPreppedItem(string itemType)
    {
      return GetPreppedCount(itemType) > 0;
    }

    /// <summary>
    /// Clears all prepped items of a specific type.
    /// </summary>
    public void ClearItemType(string itemType)
    {
      _preppedItems.Remove(itemType);
    }

    /// <summary>
    /// Clears all prepped items.
    /// </summary>
    public void ClearAll()
    {
      _preppedItems.Clear();
    }
  }

  /// <summary>
  /// Manages ranged weapon cooldowns based on skill level.
  /// </summary>
  public static class RangedCooldowns
  {
    /// <summary>
    /// Standard ammunition types for prep tracking.
    /// </summary>
    public static class AmmoTypes
    {
      public const string Arrow = "Arrow";
      public const string Bolt = "Bolt";
      public const string Bullet = "Bullet";
      public const string Magazine = "Magazine";
      public const string ThrownWeapon = "ThrownWeapon";
    }

    /// <summary>
    /// Gets the cooldown duration in seconds based on skill level.
    /// </summary>
    public static double GetCooldownSeconds(int skillLevel)
    {
      return skillLevel switch
      {
        <= 0 => 6.0,   // 2 rounds
        1 => 5.0,
        2 => 4.0,
        3 => 3.0,      // 1 round
        4 or 5 => 2.0,
        6 or 7 => 1.0,
        8 or 9 => 0.5,
        _ => 0.0       // 10+ = no cooldown
      };
    }

    /// <summary>
    /// Gets the approximate shots per round based on skill level.
    /// </summary>
    public static double GetShotsPerRound(int skillLevel)
    {
      return skillLevel switch
      {
        <= 0 => 0.5,   // 1 shot per 2 rounds
        1 => 0.6,
        2 => 0.75,
        3 => 1.0,      // 1 shot per round
        4 or 5 => 1.5,
        6 or 7 => 3.0,
        8 or 9 => 6.0,
        _ => double.MaxValue // Limited only by AP
      };
    }

    /// <summary>
    /// Checks if using prepped ammo (bypasses normal cooldown).
    /// </summary>
    public static bool CanFireWithPreppedAmmo(PrepState prepState, string ammoType)
    {
      return prepState.HasPreppedItem(ammoType);
    }

    /// <summary>
    /// Gets whether an interruption pauses or resets the cooldown.
    /// Bows, crossbows, firearms: Pausable
    /// Thrown weapons: Resettable
    /// </summary>
    public static Time.CooldownBehavior GetInterruptionBehavior(bool isThrownWeapon)
    {
      return isThrownWeapon
        ? Time.CooldownBehavior.Resettable
        : Time.CooldownBehavior.Pausable;
    }
  }
}
