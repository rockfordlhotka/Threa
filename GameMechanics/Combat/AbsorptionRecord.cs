namespace GameMechanics.Combat
{
  /// <summary>
  /// Records absorption by a single defensive layer (shield or armor piece).
  /// </summary>
  public class AbsorptionRecord
  {
    /// <summary>
    /// The item ID of the absorbing equipment.
    /// </summary>
    public string ItemId { get; init; } = string.Empty;

    /// <summary>
    /// The name of the absorbing equipment.
    /// </summary>
    public string ItemName { get; init; } = string.Empty;

    /// <summary>
    /// Whether this is a shield or armor piece.
    /// </summary>
    public bool IsShield { get; init; }

    /// <summary>
    /// The damage type absorbed.
    /// </summary>
    public DamageType DamageType { get; init; }

    /// <summary>
    /// Base absorption value for this damage type.
    /// </summary>
    public int BaseAbsorption { get; init; }

    /// <summary>
    /// Bonus from armor skill check (or shield block RV).
    /// </summary>
    public int SkillBonus { get; init; }

    /// <summary>
    /// Total absorption applied (may be limited by incoming SV).
    /// </summary>
    public int TotalAbsorbed { get; init; }

    /// <summary>
    /// SV remaining after this absorption.
    /// </summary>
    public int RemainingAfter { get; init; }

    /// <summary>
    /// Durability lost by this item.
    /// </summary>
    public int DurabilityLost { get; init; }

    /// <summary>
    /// Whether the item was destroyed (durability hit 0).
    /// </summary>
    public bool ItemDestroyed { get; init; }

    /// <summary>
    /// AP offset that was applied to reduce base absorption.
    /// </summary>
    public int ApOffsetApplied { get; init; }

    /// <summary>
    /// SV max threshold that was evaluated, if any.
    /// </summary>
    public int? SvMaxApplied { get; init; }

    /// <summary>
    /// Whether SV max was triggered (total absorption exceeded threshold).
    /// </summary>
    public bool SvMaxTriggered { get; init; }

    /// <summary>
    /// Human-readable description.
    /// </summary>
    public string Description { get; init; } = string.Empty;
  }
}
