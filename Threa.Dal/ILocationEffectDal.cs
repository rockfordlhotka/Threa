using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal;

/// <summary>
/// Data access layer for location-based spell effects.
/// </summary>
public interface ILocationEffectDal
{
    /// <summary>
    /// Creates a new spell location.
    /// </summary>
    /// <param name="location">The location to create.</param>
    /// <returns>The created location.</returns>
    Task<SpellLocation> CreateLocationAsync(SpellLocation location);

    /// <summary>
    /// Gets a location by its ID.
    /// </summary>
    /// <param name="locationId">The location ID.</param>
    /// <returns>The location, or null if not found.</returns>
    Task<SpellLocation?> GetLocationAsync(Guid locationId);

    /// <summary>
    /// Gets all locations for a campaign.
    /// </summary>
    /// <param name="campaignId">The campaign ID.</param>
    /// <returns>List of locations.</returns>
    Task<List<SpellLocation>> GetCampaignLocationsAsync(int campaignId);

    /// <summary>
    /// Creates a location effect.
    /// </summary>
    /// <param name="effect">The effect to create.</param>
    /// <returns>The created effect.</returns>
    Task<LocationEffect> CreateLocationEffectAsync(LocationEffect effect);

    /// <summary>
    /// Gets all active effects at a location.
    /// </summary>
    /// <param name="locationId">The location ID.</param>
    /// <returns>List of active effects.</returns>
    Task<List<LocationEffect>> GetActiveEffectsAtLocationAsync(Guid locationId);

    /// <summary>
    /// Gets all active location effects for a campaign.
    /// </summary>
    /// <param name="campaignId">The campaign ID.</param>
    /// <returns>List of active location effects.</returns>
    Task<List<LocationEffect>> GetActiveCampaignEffectsAsync(int campaignId);

    /// <summary>
    /// Decrements rounds remaining on all location effects.
    /// Used during round advancement.
    /// </summary>
    /// <param name="campaignId">The campaign ID.</param>
    /// <returns>List of effects that expired.</returns>
    Task<List<LocationEffect>> ProcessRoundAdvanceAsync(int campaignId);

    /// <summary>
    /// Deactivates a location effect.
    /// </summary>
    /// <param name="effectId">The effect ID.</param>
    Task DeactivateEffectAsync(Guid effectId);

    /// <summary>
    /// Removes expired and inactive location effects.
    /// </summary>
    /// <param name="campaignId">The campaign ID.</param>
    Task CleanupExpiredEffectsAsync(int campaignId);
}
