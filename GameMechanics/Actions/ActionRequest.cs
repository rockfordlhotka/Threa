using System.Collections.Generic;
using Threa.Dal.Dto;

namespace GameMechanics.Actions;

/// <summary>
/// Request to execute an action using a skill.
/// Contains all information needed for resolution.
/// </summary>
public class ActionRequest
{
    /// <summary>
    /// The skill definition being used.
    /// </summary>
    public required Skill Skill { get; set; }

    /// <summary>
    /// The character's current level in this skill.
    /// </summary>
    public int SkillLevel { get; set; }

    /// <summary>
    /// The character's relevant attribute value(s).
    /// If multiple attributes, this should be the average.
    /// </summary>
    public int AttributeValue { get; set; }

    /// <summary>
    /// Name of the attribute(s) for display.
    /// </summary>
    public string AttributeName { get; set; } = string.Empty;

    /// <summary>
    /// Whether to use standard cost (1 AP + 1 FAT) or fatigue-free (2 AP).
    /// </summary>
    public bool UseFatigueFree { get; set; } = false;

    /// <summary>
    /// Number of AP to spend on boosts.
    /// </summary>
    public int BoostAP { get; set; } = 0;

    /// <summary>
    /// Number of FAT to spend on boosts.
    /// </summary>
    public int BoostFAT { get; set; } = 0;

    /// <summary>
    /// Whether this is a subsequent action in the round (applies -1 penalty).
    /// </summary>
    public bool IsMultipleAction { get; set; } = false;

    /// <summary>
    /// Number of active wounds affecting the character.
    /// </summary>
    public int WoundCount { get; set; } = 0;

    /// <summary>
    /// Whether an aim action was taken (for ranged attacks).
    /// </summary>
    public bool HasAimed { get; set; } = false;

    /// <summary>
    /// Additional modifiers to apply (from effects, equipment, etc.).
    /// </summary>
    public List<AsModifier> AdditionalModifiers { get; set; } = new();

    /// <summary>
    /// The target value, if known ahead of time.
    /// If null, use the skill's default TV.
    /// </summary>
    public TargetValue? TargetValue { get; set; }

    /// <summary>
    /// Optional description of the target.
    /// </summary>
    public string? TargetDescription { get; set; }
}
