using GameMechanics.Actions;

namespace GameMechanics.Effects;

/// <summary>
/// Represents a modifier applied by an effect to an attribute, ability score, or success value.
/// </summary>
public class EffectModifier
{
  /// <summary>
  /// The source of this modifier (for display in breakdowns).
  /// </summary>
  public ModifierSource Source { get; set; } = ModifierSource.Effect;

  /// <summary>
  /// Description of the modifier (e.g., "Wound: Left Arm", "Poison: Weakened").
  /// </summary>
  public required string Description { get; set; }

  /// <summary>
  /// The modifier value (positive for buffs, negative for penalties).
  /// </summary>
  public int Value { get; set; }

  /// <summary>
  /// Optional: specific attribute this modifier applies to (null = all attributes).
  /// </summary>
  public string? TargetAttribute { get; set; }

  /// <summary>
  /// Optional: specific skill this modifier applies to (null = all skills).
  /// </summary>
  public string? TargetSkill { get; set; }
}
