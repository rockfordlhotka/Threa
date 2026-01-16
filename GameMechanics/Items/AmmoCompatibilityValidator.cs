using System;
using GameMechanics.Combat;

namespace GameMechanics.Items;

/// <summary>
/// Result of an ammunition compatibility validation.
/// </summary>
public record AmmoValidationResult(bool IsValid, string? ErrorMessage)
{
    public static AmmoValidationResult Ok() => new(true, null);
    public static AmmoValidationResult Fail(string message) => new(false, message);
}

/// <summary>
/// Validates compatibility between ammunition, ammo containers, and weapons.
/// </summary>
public class AmmoCompatibilityValidator
{
    /// <summary>
    /// Checks if loose ammo can be loaded into an ammo container.
    /// </summary>
    /// <param name="ammo">The ammunition properties.</param>
    /// <param name="container">The container properties.</param>
    /// <returns>Validation result indicating if the ammo is compatible.</returns>
    public AmmoValidationResult CanLoadAmmoIntoContainer(
        AmmunitionProperties ammo,
        AmmoContainerProperties container)
    {
        if (ammo == null)
            return AmmoValidationResult.Fail("Ammunition properties are required.");

        if (container == null)
            return AmmoValidationResult.Fail("Container properties are required.");

        if (string.IsNullOrWhiteSpace(ammo.AmmoType))
            return AmmoValidationResult.Fail("Ammunition must have an ammo type.");

        if (!container.CanHoldAmmoType(ammo.AmmoType))
        {
            var allowedTypes = string.Join(", ", container.GetAllowedAmmoTypes());
            return AmmoValidationResult.Fail(
                $"Cannot load {ammo.AmmoType} into {container.ContainerType}. " +
                $"Allowed ammo types: {allowedTypes}");
        }

        return AmmoValidationResult.Ok();
    }

    /// <summary>
    /// Checks if an ammo container can be loaded into a weapon.
    /// </summary>
    /// <param name="container">The container properties.</param>
    /// <param name="weapon">The weapon properties.</param>
    /// <returns>Validation result indicating if the container is compatible.</returns>
    public AmmoValidationResult CanLoadContainerIntoWeapon(
        AmmoContainerProperties container,
        RangedWeaponProperties weapon)
    {
        if (container == null)
            return AmmoValidationResult.Fail("Container properties are required.");

        if (weapon == null)
            return AmmoValidationResult.Fail("Weapon properties are required.");

        if (string.IsNullOrWhiteSpace(weapon.AmmoType))
            return AmmoValidationResult.Fail("Weapon must have an ammo type specified.");

        if (!string.Equals(container.AmmoType, weapon.AmmoType, StringComparison.OrdinalIgnoreCase))
        {
            return AmmoValidationResult.Fail(
                $"Cannot load {container.AmmoType} {container.ContainerType.ToLowerInvariant()} " +
                $"into {weapon.AmmoType} weapon.");
        }

        // Check container type compatibility with reload type
        var reloadType = weapon.GetReloadType();
        var isCompatible = (container.ContainerType, reloadType) switch
        {
            (AmmoContainerType.Magazine, ReloadType.Magazine) => true,
            (AmmoContainerType.Speedloader, ReloadType.Cylinder) => true,
            (AmmoContainerType.Belt, ReloadType.Belt) => true,
            (AmmoContainerType.Clip, ReloadType.SingleRound) => true, // Clips can work with internal magazines
            // Energy weapons: PowerCell or Magazine with Battery reload
            (AmmoContainerType.PowerCell, ReloadType.Battery) => true,
            (AmmoContainerType.Magazine, ReloadType.Battery) => true, // Allow Magazine for compatibility
            // Quiver for arrow/bolt weapons
            (AmmoContainerType.Quiver, _) when weapon.AmmoType?.Contains("Arrow", StringComparison.OrdinalIgnoreCase) == true => true,
            (AmmoContainerType.Quiver, _) when weapon.AmmoType?.Contains("Bolt", StringComparison.OrdinalIgnoreCase) == true => true,
            _ => false
        };

        if (!isCompatible)
        {
            return AmmoValidationResult.Fail(
                $"This weapon's reload type ({reloadType}) is not compatible with " +
                $"{container.ContainerType.ToLowerInvariant()}s.");
        }

        return AmmoValidationResult.Ok();
    }

    /// <summary>
    /// Checks if loose ammo can be loaded directly into a weapon.
    /// </summary>
    /// <param name="ammo">The ammunition properties.</param>
    /// <param name="weapon">The weapon properties.</param>
    /// <returns>Validation result indicating if the ammo can be loaded directly.</returns>
    public AmmoValidationResult CanLoadLooseAmmoIntoWeapon(
        AmmunitionProperties ammo,
        RangedWeaponProperties weapon)
    {
        if (ammo == null)
            return AmmoValidationResult.Fail("Ammunition properties are required.");

        if (weapon == null)
            return AmmoValidationResult.Fail("Weapon properties are required.");

        // Check if weapon accepts loose ammo
        if (!weapon.AcceptsLooseAmmo)
        {
            var reloadType = weapon.GetReloadType();
            return AmmoValidationResult.Fail(
                $"This weapon requires a {reloadType.ToString().ToLowerInvariant()} and cannot be loaded with loose ammunition.");
        }

        // Check ammo type compatibility
        if (string.IsNullOrWhiteSpace(weapon.AmmoType))
            return AmmoValidationResult.Fail("Weapon must have an ammo type specified.");

        if (!string.Equals(ammo.AmmoType, weapon.AmmoType, StringComparison.OrdinalIgnoreCase))
        {
            return AmmoValidationResult.Fail(
                $"Cannot load {ammo.AmmoType} ammunition into {weapon.AmmoType} weapon.");
        }

        return AmmoValidationResult.Ok();
    }

    /// <summary>
    /// Checks if a container has enough remaining capacity for additional ammo.
    /// </summary>
    /// <param name="container">The container properties.</param>
    /// <param name="currentCount">Current number of rounds in the container.</param>
    /// <param name="addCount">Number of rounds to add.</param>
    /// <returns>Validation result.</returns>
    public AmmoValidationResult CanAddAmmoToContainer(
        AmmoContainerProperties container,
        int currentCount,
        int addCount)
    {
        if (container == null)
            return AmmoValidationResult.Fail("Container properties are required.");

        var remainingCapacity = container.Capacity - currentCount;
        if (addCount > remainingCapacity)
        {
            return AmmoValidationResult.Fail(
                $"Container can only hold {remainingCapacity} more rounds " +
                $"(capacity: {container.Capacity}, current: {currentCount}).");
        }

        return AmmoValidationResult.Ok();
    }
}
