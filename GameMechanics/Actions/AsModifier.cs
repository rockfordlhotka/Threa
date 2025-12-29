namespace GameMechanics.Actions;

/// <summary>
/// Categorizes the source of a modifier to AS.
/// </summary>
public enum ModifierSource
{
    /// <summary>
    /// Base value from attribute + skill calculation.
    /// </summary>
    Base = 0,

    /// <summary>
    /// Bonus from equipment (weapon, armor, accessories).
    /// </summary>
    Equipment = 1,

    /// <summary>
    /// Penalty from wounds (-2 per wound).
    /// </summary>
    Wound = 2,

    /// <summary>
    /// Penalty for multiple actions in a round (-1 for non-first actions).
    /// </summary>
    MultipleAction = 3,

    /// <summary>
    /// Bonus from boost spending (+1 per AP/FAT spent).
    /// </summary>
    Boost = 4,

    /// <summary>
    /// Bonus from aim action (+2 AS).
    /// </summary>
    Aim = 5,

    /// <summary>
    /// Modifier from active effect (buff, debuff, spell, poison, etc.).
    /// </summary>
    Effect = 6,

    /// <summary>
    /// Modifier from environmental conditions.
    /// </summary>
    Environment = 7,

    /// <summary>
    /// Modifier from low fatigue state.
    /// </summary>
    LowFatigue = 8,

    /// <summary>
    /// Modifier from low vitality state.
    /// </summary>
    LowVitality = 9,

    /// <summary>
    /// GM-applied situational modifier.
    /// </summary>
    Situational = 10
}

/// <summary>
/// Represents a single modifier to an Ability Score.
/// </summary>
public class AsModifier
{
    /// <summary>
    /// The source category of this modifier.
    /// </summary>
    public ModifierSource Source { get; set; }

    /// <summary>
    /// Descriptive name for display (e.g., "Sword of Sharpness +1", "Arm Wound").
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The modifier value (positive = bonus, negative = penalty).
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// Creates a new AS modifier.
    /// </summary>
    public AsModifier(ModifierSource source, string description, int value)
    {
        Source = source;
        Description = description;
        Value = value;
    }

    public override string ToString() => $"{Description}: {(Value >= 0 ? "+" : "")}{Value}";
}
