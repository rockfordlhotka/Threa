using System.Collections.Generic;

namespace GameMechanics.Combat;

/// <summary>
/// Information about an available melee weapon for the combat UI.
/// This can represent either an equipped physical weapon or a virtual weapon (Punch, Kick).
/// </summary>
public class MeleeWeaponInfo
{
    /// <summary>
    /// The ItemTemplate ID (for virtual weapons) or CharacterItem template ID.
    /// </summary>
    public int TemplateId { get; set; }

    /// <summary>
    /// Weapon display name (e.g., "Longsword", "Punch", "Kick").
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    /// The skill used for this weapon (e.g., "Sword", "Hand-to-Hand").
    /// </summary>
    public string SkillName { get; set; } = "";

    /// <summary>
    /// Weapon's AV modifier (applied to attack roll).
    /// </summary>
    public int WeaponAVModifier { get; set; }

    /// <summary>
    /// Weapon's SV modifier (applied to damage calculation).
    /// </summary>
    public int WeaponSVModifier { get; set; }

    /// <summary>
    /// Damage class (1-4).
    /// </summary>
    public int WeaponDamageClass { get; set; } = 1;

    /// <summary>
    /// Damage type (e.g., "Bludgeoning", "Cutting").
    /// </summary>
    public string DamageType { get; set; } = "Bashing";

    /// <summary>
    /// Per-damage-type SV modifiers from weapon.
    /// When set, takes precedence over WeaponSVModifier and DamageType.
    /// </summary>
    public Dictionary<string, int>? WeaponDamageModifiers { get; set; }

    /// <summary>
    /// Whether this is a virtual weapon (innate ability, not a physical item).
    /// </summary>
    public bool IsVirtual { get; set; }

    /// <summary>
    /// Short description for tooltip display.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    /// Ability Score for the weapon's associated skill (e.g., Sword AS, Hand-to-Hand AS).
    /// Used by the targeting panel to update the effective AS when the weapon changes.
    /// </summary>
    public int SkillAS { get; set; }
}
