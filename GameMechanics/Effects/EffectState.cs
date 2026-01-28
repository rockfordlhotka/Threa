using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace GameMechanics.Effects;

/// <summary>
/// State data stored in BehaviorState for generic effects with modifiers.
/// Supports attribute modifiers, skill modifiers, global AS modifiers,
/// and periodic damage/healing per tick.
/// </summary>
public class EffectState
{
    /// <summary>
    /// Effect name override (null = use effect's Name property).
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Description override (null = use effect's Description property).
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Icon identifier for display.
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// Color for display (hex code or CSS class name).
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Attribute name to modifier value (e.g., {"STR": 2, "DEX": -1}).
    /// </summary>
    public Dictionary<string, int>? AttributeModifiers { get; set; }

    /// <summary>
    /// Skill name to modifier value.
    /// </summary>
    public Dictionary<string, int>? SkillModifiers { get; set; }

    /// <summary>
    /// Global ability score modifier (applies to all checks).
    /// </summary>
    public int? ASModifier { get; set; }

    /// <summary>
    /// FAT damage per tick (for poisons, burns, etc.).
    /// </summary>
    public int? FatDamagePerTick { get; set; }

    /// <summary>
    /// VIT damage per tick (for poisons, burns, etc.).
    /// </summary>
    public int? VitDamagePerTick { get; set; }

    /// <summary>
    /// FAT healing per tick (for regeneration effects).
    /// </summary>
    public int? FatHealingPerTick { get; set; }

    /// <summary>
    /// VIT healing per tick (for regeneration effects).
    /// </summary>
    public int? VitHealingPerTick { get; set; }

    /// <summary>
    /// Behavior tags that activate different handlers.
    /// Examples: ["modifier", "end-of-round-trigger", "narrative", "equipment-driven"]
    /// </summary>
    public List<string>? BehaviorTags { get; set; }

    /// <summary>
    /// Freeform JSON for effect-specific custom data.
    /// </summary>
    public string? CustomData { get; set; }

    /// <summary>
    /// Returns true if any modifier is set.
    /// </summary>
    public bool HasModifiers =>
        ASModifier.HasValue ||
        (AttributeModifiers?.Count > 0) ||
        (SkillModifiers?.Count > 0);

    /// <summary>
    /// Returns true if any damage or healing per tick is set.
    /// </summary>
    public bool HasTickEffects =>
        FatDamagePerTick.HasValue ||
        VitDamagePerTick.HasValue ||
        FatHealingPerTick.HasValue ||
        VitHealingPerTick.HasValue;

    /// <summary>
    /// Gets the modifier for a specific attribute.
    /// Returns 0 if not set.
    /// </summary>
    /// <param name="attrName">Attribute name (e.g., "STR", "DEX").</param>
    public int GetAttributeModifier(string attrName)
    {
        if (AttributeModifiers == null)
            return 0;
        return AttributeModifiers.TryGetValue(attrName, out int value) ? value : 0;
    }

    /// <summary>
    /// Gets the modifier for a specific skill.
    /// Returns 0 if not set.
    /// </summary>
    /// <param name="skillName">Skill name.</param>
    public int GetSkillModifier(string skillName)
    {
        if (SkillModifiers == null)
            return 0;
        return SkillModifiers.TryGetValue(skillName, out int value) ? value : 0;
    }

    /// <summary>
    /// Checks if a specific behavior tag is present.
    /// </summary>
    /// <param name="tag">The tag to check for.</param>
    public bool HasBehaviorTag(string tag)
    {
        return BehaviorTags?.Contains(tag) ?? false;
    }

    /// <summary>
    /// Serializes this state to JSON for storage in BehaviorState.
    /// </summary>
    public string Serialize() => JsonSerializer.Serialize(this);

    /// <summary>
    /// Deserializes effect state from BehaviorState JSON.
    /// Returns empty state if json is null or empty.
    /// </summary>
    public static EffectState Deserialize(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return new EffectState();
        return JsonSerializer.Deserialize<EffectState>(json) ?? new EffectState();
    }

    /// <summary>
    /// Creates a new EffectState with attribute modifiers.
    /// </summary>
    public static EffectState WithAttributeModifiers(params (string attr, int mod)[] modifiers)
    {
        var state = new EffectState
        {
            AttributeModifiers = new Dictionary<string, int>()
        };
        foreach (var (attr, mod) in modifiers)
        {
            state.AttributeModifiers[attr] = mod;
        }
        return state;
    }

    /// <summary>
    /// Creates a new EffectState with a global AS modifier.
    /// </summary>
    public static EffectState WithASModifier(int modifier)
    {
        return new EffectState { ASModifier = modifier };
    }

    /// <summary>
    /// Creates a new EffectState for damage-over-time effects.
    /// </summary>
    public static EffectState WithDamagePerTick(int fatDamage, int vitDamage = 0)
    {
        return new EffectState
        {
            FatDamagePerTick = fatDamage > 0 ? fatDamage : null,
            VitDamagePerTick = vitDamage > 0 ? vitDamage : null
        };
    }

    /// <summary>
    /// Creates a new EffectState for healing-over-time effects.
    /// </summary>
    public static EffectState WithHealingPerTick(int fatHealing, int vitHealing = 0)
    {
        return new EffectState
        {
            FatHealingPerTick = fatHealing > 0 ? fatHealing : null,
            VitHealingPerTick = vitHealing > 0 ? vitHealing : null
        };
    }
}
