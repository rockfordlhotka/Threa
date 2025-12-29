using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb;

/// <summary>
/// Mock implementation of ICharacterEffectDal for development and testing.
/// </summary>
public class CharacterEffectDal : ICharacterEffectDal
{
    private readonly IEffectDefinitionDal _definitionDal;

    public CharacterEffectDal(IEffectDefinitionDal definitionDal)
    {
        _definitionDal = definitionDal;
    }

    public async Task<List<CharacterEffect>> GetCharacterEffectsAsync(int characterId)
    {
        var effects = MockDb.CharacterEffects
            .Where(e => e.CharacterId == characterId && e.IsActive)
            .ToList();

        // Populate definitions
        foreach (var effect in effects)
        {
            effect.Definition = await _definitionDal.GetDefinitionAsync(effect.EffectDefinitionId);
        }

        return effects;
    }

    public async Task<List<CharacterEffect>> GetCharacterEffectsByTypeAsync(int characterId, EffectType effectType)
    {
        var effects = MockDb.CharacterEffects
            .Where(e => e.CharacterId == characterId && e.IsActive)
            .ToList();

        var result = new List<CharacterEffect>();
        foreach (var effect in effects)
        {
            effect.Definition = await _definitionDal.GetDefinitionAsync(effect.EffectDefinitionId);
            if (effect.Definition.EffectType == effectType)
                result.Add(effect);
        }

        return result;
    }

    public async Task<CharacterEffect?> GetEffectAsync(Guid id)
    {
        var effect = MockDb.CharacterEffects.FirstOrDefault(e => e.Id == id);
        if (effect != null)
        {
            effect.Definition = await _definitionDal.GetDefinitionAsync(effect.EffectDefinitionId);
        }
        return effect;
    }

    public async Task<bool> HasEffectAsync(int characterId, string effectName)
    {
        var effects = await GetCharacterEffectsAsync(characterId);
        return effects.Any(e => e.Definition?.Name.Equals(effectName, StringComparison.OrdinalIgnoreCase) == true);
    }

    public async Task<List<CharacterEffect>> GetEffectsByNameAsync(int characterId, string effectName)
    {
        var effects = await GetCharacterEffectsAsync(characterId);
        return effects
            .Where(e => e.Definition?.Name.Equals(effectName, StringComparison.OrdinalIgnoreCase) == true)
            .ToList();
    }

    public async Task<CharacterEffect> ApplyEffectAsync(CharacterEffect effect)
    {
        var definition = await _definitionDal.GetDefinitionAsync(effect.EffectDefinitionId);
        effect.Definition = definition;

        // Handle stacking behavior
        var existingEffects = await GetEffectsByNameAsync(effect.CharacterId, definition.Name);

        if (existingEffects.Count > 0 && !definition.IsStackable)
        {
            // Not stackable - check stack behavior
            switch (definition.StackBehavior)
            {
                case StackBehavior.Replace:
                    // Remove old, add new
                    foreach (var existing in existingEffects)
                    {
                        MockDb.CharacterEffects.Remove(existing);
                    }
                    break;

                case StackBehavior.Extend:
                    // Extend duration of existing
                    var toExtend = existingEffects.First();
                    if (toExtend.RoundsRemaining.HasValue && effect.RoundsRemaining.HasValue)
                        toExtend.RoundsRemaining += effect.RoundsRemaining;
                    if (toExtend.EndTime.HasValue && effect.EndTime.HasValue)
                        toExtend.EndTime = toExtend.EndTime.Value.Add(effect.EndTime.Value - effect.StartTime);
                    return toExtend;

                case StackBehavior.Intensify:
                    // Increase stack count
                    var toIntensify = existingEffects.First();
                    if (toIntensify.CurrentStacks < definition.MaxStacks)
                        toIntensify.CurrentStacks++;
                    return toIntensify;

                case StackBehavior.Independent:
                    // Allow new instance
                    break;
            }
        }
        else if (existingEffects.Count > 0 && definition.IsStackable)
        {
            // Stackable - check if under max stacks
            if (existingEffects.Count >= definition.MaxStacks)
            {
                // At max stacks - apply stack behavior
                switch (definition.StackBehavior)
                {
                    case StackBehavior.Replace:
                        // Remove oldest
                        var oldest = existingEffects.OrderBy(e => e.StartTime).First();
                        MockDb.CharacterEffects.Remove(oldest);
                        break;
                    case StackBehavior.Extend:
                    case StackBehavior.Intensify:
                        // Update most recent
                        var mostRecent = existingEffects.OrderByDescending(e => e.StartTime).First();
                        if (definition.StackBehavior == StackBehavior.Extend)
                        {
                            if (mostRecent.RoundsRemaining.HasValue && effect.RoundsRemaining.HasValue)
                                mostRecent.RoundsRemaining += effect.RoundsRemaining;
                        }
                        else
                        {
                            mostRecent.CurrentStacks = Math.Min(mostRecent.CurrentStacks + 1, definition.MaxStacks);
                        }
                        return mostRecent;
                    case StackBehavior.Independent:
                        // Don't add - at max
                        return existingEffects.First();
                }
            }
        }

        // Add new effect
        if (effect.Id == Guid.Empty)
            effect.Id = Guid.NewGuid();

        MockDb.CharacterEffects.Add(effect);
        return effect;
    }

    public Task<CharacterEffect> UpdateEffectAsync(CharacterEffect effect)
    {
        var existing = MockDb.CharacterEffects.FirstOrDefault(e => e.Id == effect.Id);
        if (existing == null)
            throw new NotFoundException($"CharacterEffect {effect.Id}");

        MockDb.CharacterEffects.Remove(existing);
        MockDb.CharacterEffects.Add(effect);
        return Task.FromResult(effect);
    }

    public Task RemoveEffectAsync(Guid id)
    {
        var effect = MockDb.CharacterEffects.FirstOrDefault(e => e.Id == id);
        if (effect != null)
        {
            MockDb.CharacterEffects.Remove(effect);
        }
        return Task.CompletedTask;
    }

    public async Task RemoveEffectsByTypeAsync(int characterId, EffectType effectType)
    {
        var effects = await GetCharacterEffectsByTypeAsync(characterId, effectType);
        foreach (var effect in effects)
        {
            MockDb.CharacterEffects.Remove(effect);
        }
    }

    public Task RemoveExpiredEffectsAsync(int characterId)
    {
        var now = DateTime.UtcNow;
        var expired = MockDb.CharacterEffects
            .Where(e => e.CharacterId == characterId && e.EndTime.HasValue && e.EndTime < now)
            .ToList();

        foreach (var effect in expired)
        {
            MockDb.CharacterEffects.Remove(effect);
        }

        return Task.CompletedTask;
    }

    public async Task<List<CharacterEffect>> ProcessEndOfRoundAsync(int characterId)
    {
        var expiredEffects = new List<CharacterEffect>();
        var effects = await GetCharacterEffectsAsync(characterId);

        foreach (var effect in effects.ToList())
        {
            // Decrement rounds remaining
            if (effect.RoundsRemaining.HasValue)
            {
                effect.RoundsRemaining--;
                if (effect.RoundsRemaining <= 0)
                {
                    expiredEffects.Add(effect);
                    MockDb.CharacterEffects.Remove(effect);
                }
            }

            // Decrement rounds until tick (for DoT effects)
            if (effect.RoundsUntilTick.HasValue)
            {
                effect.RoundsUntilTick--;
                if (effect.RoundsUntilTick <= 0)
                {
                    // Reset tick counter from definition
                    var dotImpact = effect.Definition?.Impacts
                        .FirstOrDefault(i => i.ImpactType == EffectImpactType.DamageOverTime);
                    if (dotImpact?.DamageInterval != null)
                    {
                        effect.RoundsUntilTick = dotImpact.DamageInterval;
                    }
                }
            }
        }

        return expiredEffects;
    }
}
