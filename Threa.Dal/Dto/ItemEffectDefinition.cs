namespace Threa.Dal.Dto;

/// <summary>
/// Defines an effect that an item can apply to a character.
/// Links an ItemTemplate to an effect configuration.
/// </summary>
public class ItemEffectDefinition
{
    /// <summary>
    /// Unique identifier for this item effect definition.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The item template that has this effect.
    /// </summary>
    public int ItemTemplateId { get; set; }

    /// <summary>
    /// Optional reference to a predefined EffectDefinition.
    /// If null, the effect is defined inline via EffectType and BehaviorState.
    /// </summary>
    public int? EffectDefinitionId { get; set; }

    /// <summary>
    /// The type of effect (used when EffectDefinitionId is null).
    /// </summary>
    public EffectType EffectType { get; set; }

    /// <summary>
    /// Display name for this effect (e.g., "Ring of Healing", "Poison Aura").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Description of what this effect does.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When this effect activates/deactivates.
    /// </summary>
    public ItemEffectTrigger Trigger { get; set; }

    /// <summary>
    /// Whether this effect is cursed (prevents unequipping until effect is removed).
    /// </summary>
    public bool IsCursed { get; set; }

    /// <summary>
    /// Whether this effect requires attunement (for future attunement slot limiting).
    /// </summary>
    public bool RequiresAttunement { get; set; }

    /// <summary>
    /// Override duration in rounds. Null means permanent while trigger condition is met.
    /// For OnUse triggers, this is the effect duration after use.
    /// </summary>
    public int? DurationRounds { get; set; }

    /// <summary>
    /// JSON configuration for the effect behavior.
    /// Structure depends on EffectType (e.g., BuffModifier list for Buff effects).
    /// </summary>
    public string? BehaviorState { get; set; }

    /// <summary>
    /// Icon name for UI display of this effect.
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// Whether this effect definition is active and should be applied.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Priority for effect application order (higher = applied first).
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// Whether this effect can be toggled on/off by the player (e.g., activatable implants).
    /// </summary>
    public bool IsToggleable { get; set; }

    /// <summary>
    /// AP cost to toggle this effect on or off. 0 = free action.
    /// </summary>
    public int ToggleApCost { get; set; }

    /// <summary>
    /// How this target effect interacts with the defender's armor at the hit location.
    /// Only relevant when Trigger is OnAttackWith or OnCritical and the effect targets the defender.
    /// </summary>
    public ArmorInteractionRule ArmorRule { get; set; } = ArmorInteractionRule.PenetrationOnly;

    /// <summary>
    /// The damage class of this effect (1–4). The effect will not apply if the target's armor,
    /// shield, or inherent damage class equals or exceeds this value.
    /// Set to 0 to bypass the damage class check entirely.
    /// </summary>
    public int EffectDamageClass { get; set; } = 0;
}
