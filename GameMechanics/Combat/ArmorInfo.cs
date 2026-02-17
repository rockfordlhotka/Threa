using System.Collections.Generic;

namespace GameMechanics.Combat
{
  /// <summary>
  /// Represents armor properties for damage resolution.
  /// This is a combat-focused view of armor data from ItemTemplate.
  /// </summary>
  public class ArmorInfo
  {
    /// <summary>
    /// Unique identifier for the armor item instance.
    /// Used to track durability changes.
    /// </summary>
    public string ItemId { get; init; } = string.Empty;

    /// <summary>
    /// Display name of the armor.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The hit locations this armor covers.
    /// </summary>
    public HitLocation[] CoveredLocations { get; init; } = [];

    /// <summary>
    /// The damage class of the armor (1-4).
    /// Higher class armor absorbs lower class damage as 1 SV.
    /// </summary>
    public int DamageClass { get; init; } = 1;

    /// <summary>
    /// Absorption values per damage type.
    /// </summary>
    public Dictionary<DamageType, int> Absorption { get; init; } = new();

    /// <summary>
    /// Current durability of the armor.
    /// </summary>
    public int CurrentDurability { get; set; }

    /// <summary>
    /// Maximum durability of the armor.
    /// </summary>
    public int MaxDurability { get; init; }

    /// <summary>
    /// Whether the armor is still functional.
    /// </summary>
    public bool IsIntact => CurrentDurability > 0;

    /// <summary>
    /// Layer order for stacking (lower = outer, absorbs first).
    /// </summary>
    public int LayerOrder { get; init; }

    /// <summary>
    /// Gets the absorption value for a specific damage type.
    /// Returns 0 if the damage type is not defined.
    /// </summary>
    public int GetAbsorption(DamageType damageType)
    {
      return Absorption.TryGetValue(damageType, out int value) ? value : 0;
    }

    /// <summary>
    /// Checks if this armor covers the specified hit location.
    /// </summary>
    public bool CoversLocation(HitLocation location)
    {
      foreach (var covered in CoveredLocations)
      {
        if (covered == location)
          return true;
      }
      return false;
    }

    /// <summary>
    /// Reduces durability by the specified amount.
    /// Returns the actual amount reduced (may be less if durability is low).
    /// </summary>
    public int ReduceDurability(int amount)
    {
      if (amount <= 0 || !IsIntact)
        return 0;

      int actualReduction = System.Math.Min(amount, CurrentDurability);
      CurrentDurability -= actualReduction;
      return actualReduction;
    }

    /// <summary>
    /// Creates a shallow clone with mutable durability for multi-damage-type resolution.
    /// </summary>
    public ArmorInfo Clone()
    {
      return new ArmorInfo
      {
        ItemId = ItemId,
        Name = Name,
        CoveredLocations = CoveredLocations,
        DamageClass = DamageClass,
        Absorption = Absorption,
        CurrentDurability = CurrentDurability,
        MaxDurability = MaxDurability,
        LayerOrder = LayerOrder
      };
    }
  }

  /// <summary>
  /// Represents shield properties for damage resolution.
  /// Shields can block any hit location but require a skill check.
  /// </summary>
  public class ShieldInfo
  {
    /// <summary>
    /// Unique identifier for the shield item instance.
    /// </summary>
    public string ItemId { get; init; } = string.Empty;

    /// <summary>
    /// Display name of the shield.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The damage class of the shield (1-4).
    /// </summary>
    public int DamageClass { get; init; } = 1;

    /// <summary>
    /// Absorption values per damage type.
    /// </summary>
    public Dictionary<DamageType, int> Absorption { get; init; } = new();

    /// <summary>
    /// Current durability of the shield.
    /// </summary>
    public int CurrentDurability { get; set; }

    /// <summary>
    /// Maximum durability of the shield.
    /// </summary>
    public int MaxDurability { get; init; }

    /// <summary>
    /// Whether the shield is still functional.
    /// </summary>
    public bool IsIntact => CurrentDurability > 0;

    /// <summary>
    /// Gets the absorption value for a specific damage type.
    /// </summary>
    public int GetAbsorption(DamageType damageType)
    {
      return Absorption.TryGetValue(damageType, out int value) ? value : 0;
    }

    /// <summary>
    /// Reduces durability by the specified amount.
    /// Returns the actual amount reduced.
    /// </summary>
    public int ReduceDurability(int amount)
    {
      if (amount <= 0 || !IsIntact)
        return 0;

      int actualReduction = System.Math.Min(amount, CurrentDurability);
      CurrentDurability -= actualReduction;
      return actualReduction;
    }

    /// <summary>
    /// Creates a shallow clone with mutable durability for multi-damage-type resolution.
    /// </summary>
    public ShieldInfo Clone()
    {
      return new ShieldInfo
      {
        ItemId = ItemId,
        Name = Name,
        DamageClass = DamageClass,
        Absorption = Absorption,
        CurrentDurability = CurrentDurability,
        MaxDurability = MaxDurability
      };
    }
  }
}
