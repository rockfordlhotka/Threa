using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb;

/// <summary>
/// Mock database implementation for location-based spell effects.
/// </summary>
public class LocationEffectDal : ILocationEffectDal
{
    private readonly List<SpellLocation> _locations;
    private readonly List<LocationEffect> _effects;

    /// <summary>
    /// Creates a LocationEffectDal using instance-level storage (for test isolation).
    /// </summary>
    public LocationEffectDal()
    {
        _locations = new List<SpellLocation>();
        _effects = new List<LocationEffect>();
    }

    /// <summary>
    /// Creates a LocationEffectDal that uses the static MockDb storage.
    /// </summary>
    public static LocationEffectDal WithStaticStorage()
    {
        return new LocationEffectDal(MockDb.SpellLocations, MockDb.LocationEffects);
    }

    private LocationEffectDal(List<SpellLocation> locations, List<LocationEffect> effects)
    {
        _locations = locations;
        _effects = effects;
    }

    public Task<SpellLocation> CreateLocationAsync(SpellLocation location)
    {
        if (location.Id == Guid.Empty)
        {
            location.Id = Guid.NewGuid();
        }
        _locations.Add(location);
        return Task.FromResult(location);
    }

    public Task<SpellLocation?> GetLocationAsync(Guid locationId)
    {
        var location = _locations.FirstOrDefault(l => l.Id == locationId);
        return Task.FromResult(location);
    }

    public Task<List<SpellLocation>> GetCampaignLocationsAsync(int campaignId)
    {
        var locations = _locations
            .Where(l => l.CampaignId == campaignId)
            .ToList();
        return Task.FromResult(locations);
    }

    public Task<LocationEffect> CreateLocationEffectAsync(LocationEffect effect)
    {
        if (effect.Id == Guid.Empty)
        {
            effect.Id = Guid.NewGuid();
        }
        _effects.Add(effect);
        return Task.FromResult(effect);
    }

    public Task<List<LocationEffect>> GetActiveEffectsAtLocationAsync(Guid locationId)
    {
        var effects = _effects
            .Where(e => e.LocationId == locationId && e.IsActive)
            .ToList();
        return Task.FromResult(effects);
    }

    public Task<List<LocationEffect>> GetActiveCampaignEffectsAsync(int campaignId)
    {
        // Get all locations for the campaign
        var campaignLocationIds = _locations
            .Where(l => l.CampaignId == campaignId)
            .Select(l => l.Id)
            .ToHashSet();

        // Get active effects at those locations
        var effects = _effects
            .Where(e => campaignLocationIds.Contains(e.LocationId) && e.IsActive)
            .ToList();

        return Task.FromResult(effects);
    }

    public Task<List<LocationEffect>> ProcessRoundAdvanceAsync(int campaignId)
    {
        var campaignLocationIds = _locations
            .Where(l => l.CampaignId == campaignId)
            .Select(l => l.Id)
            .ToHashSet();

        var expiredEffects = new List<LocationEffect>();

        foreach (var effect in _effects.Where(e => campaignLocationIds.Contains(e.LocationId) && e.IsActive))
        {
            if (effect.RoundsRemaining.HasValue)
            {
                effect.RoundsRemaining--;
                if (effect.RoundsRemaining <= 0)
                {
                    effect.IsActive = false;
                    expiredEffects.Add(effect);
                }
            }
        }

        return Task.FromResult(expiredEffects);
    }

    public Task DeactivateEffectAsync(Guid effectId)
    {
        var effect = _effects.FirstOrDefault(e => e.Id == effectId);
        if (effect != null)
        {
            effect.IsActive = false;
        }
        return Task.CompletedTask;
    }

    public Task CleanupExpiredEffectsAsync(int campaignId)
    {
        var campaignLocationIds = _locations
            .Where(l => l.CampaignId == campaignId)
            .Select(l => l.Id)
            .ToHashSet();

        _effects.RemoveAll(e => 
            campaignLocationIds.Contains(e.LocationId) && 
            !e.IsActive);

        return Task.CompletedTask;
    }
}
