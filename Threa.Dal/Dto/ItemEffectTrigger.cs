namespace Threa.Dal.Dto;

/// <summary>
/// Defines when an item's effect is activated or deactivated.
/// </summary>
public enum ItemEffectTrigger
{
    /// <summary>
    /// Not from an item (normal effect, or not applicable).
    /// </summary>
    None = 0,

    /// <summary>
    /// Effect is active while the item is equipped.
    /// Removed when item is unequipped.
    /// </summary>
    WhileEquipped = 1,

    /// <summary>
    /// Effect is active while the item is in the character's inventory.
    /// Removed when item is dropped or transferred.
    /// </summary>
    WhilePossessed = 2,

    /// <summary>
    /// One-time effect applied when the item is used (e.g., potion).
    /// Item is typically consumed after use.
    /// </summary>
    OnUse = 3,

    /// <summary>
    /// Effect applied when attacking with this weapon.
    /// May be single application or per-attack.
    /// </summary>
    OnAttackWith = 4,

    /// <summary>
    /// Effect applied when hit while wearing this item (reactive armor, thorns).
    /// </summary>
    OnHitWhileWearing = 5,

    /// <summary>
    /// Effect applied on critical hit with this weapon.
    /// </summary>
    OnCritical = 6,

    /// <summary>
    /// Effect applied when the item is first picked up.
    /// Useful for cursed items that activate on touch.
    /// </summary>
    OnPickup = 7
}
