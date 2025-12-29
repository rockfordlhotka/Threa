using System.Collections.Generic;

namespace Threa.Dal.Dto;

/// <summary>
/// Template/definition for an effect type that can be applied to entities.
/// EffectDefinitions are the blueprints from which CharacterEffects are instantiated.
/// </summary>
public class EffectDefinition
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Display name (e.g., "Poisoned", "Strength Boost", "Burning").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Detailed description of the effect.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Category of the effect.
    /// </summary>
    public EffectType EffectType { get; set; }

    /// <summary>
    /// What type of entity this effect targets.
    /// </summary>
    public EffectTargetType TargetType { get; set; } = EffectTargetType.Character;

    /// <summary>
    /// What caused this effect (for display/tracking).
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Icon name for UI display.
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// How duration is measured.
    /// </summary>
    public DurationType DurationType { get; set; }

    /// <summary>
    /// Default duration value in the units specified by DurationType.
    /// </summary>
    public int DefaultDuration { get; set; }

    /// <summary>
    /// Whether multiple instances of this effect can exist.
    /// </summary>
    public bool IsStackable { get; set; }

    /// <summary>
    /// Maximum number of stacks if stackable.
    /// </summary>
    public int MaxStacks { get; set; } = 1;

    /// <summary>
    /// How multiple applications interact.
    /// </summary>
    public StackBehavior StackBehavior { get; set; } = StackBehavior.Replace;

    /// <summary>
    /// Whether this effect can be removed by spells, skills, or items.
    /// </summary>
    public bool CanBeRemoved { get; set; } = true;

    /// <summary>
    /// Comma-separated list of removal methods (e.g., "Spell,Medicine,Antidote,Rest").
    /// </summary>
    public string? RemovalMethods { get; set; }

    /// <summary>
    /// TV for skill-based removal attempts.
    /// </summary>
    public int RemovalDifficulty { get; set; }

    /// <summary>
    /// For wounds: body location (Head, Torso, LeftArm, etc.).
    /// </summary>
    public string? WoundLocation { get; set; }

    /// <summary>
    /// Comma-separated conditions that break the effect (e.g., "Attack,CastSpell,TakeDamage").
    /// </summary>
    public string? BreakConditions { get; set; }

    /// <summary>
    /// Whether this definition is available for use.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// The impacts this effect applies.
    /// </summary>
    public List<EffectImpact> Impacts { get; set; } = [];
}
