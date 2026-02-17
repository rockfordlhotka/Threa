namespace GameMechanics.Combat;

/// <summary>
/// Per-damage-type weapon entry containing SV modifier and optional armor-piercing properties.
/// </summary>
/// <param name="SvModifier">SV modifier applied to base attack SV for this damage type.</param>
/// <param name="ApOffset">Armor Piercing offset - reduces armor base absorption before DC scaling.</param>
/// <param name="SvMax">Max effective SV cap when armor defense exceeds this threshold.</param>
public record DamageTypeEntry(int SvModifier, int ApOffset = 0, int? SvMax = null);
