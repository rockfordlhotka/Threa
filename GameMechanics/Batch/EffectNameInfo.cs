using Threa.Dal.Dto;

namespace GameMechanics.Batch;

/// <summary>
/// Represents a unique effect name found across selected characters for the batch remove modal.
/// Shows the effect type and how many selected characters have this effect.
/// </summary>
public class EffectNameInfo
{
    /// <summary>
    /// The effect name (matching EffectRecord.Name).
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// The effect type (for icon display in the modal).
    /// </summary>
    public EffectType EffectType { get; set; }

    /// <summary>
    /// How many of the selected characters have this effect active.
    /// </summary>
    public int CharacterCount { get; set; }
}
