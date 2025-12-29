using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb;

/// <summary>
/// Mock implementation of IItemEffectDal for development and testing.
/// </summary>
public class ItemEffectDal : IItemEffectDal
{
    private readonly IEffectDefinitionDal _definitionDal;
    private readonly ICharacterItemDal _itemDal;

    public ItemEffectDal(IEffectDefinitionDal definitionDal, ICharacterItemDal itemDal)
    {
        _definitionDal = definitionDal;
        _itemDal = itemDal;
    }

    public async Task<List<ItemEffect>> GetItemEffectsAsync(Guid itemId)
    {
        var effects = MockDb.ItemEffects
            .Where(e => e.ItemId == itemId && e.IsActive)
            .ToList();

        foreach (var effect in effects)
        {
            effect.Definition = await _definitionDal.GetDefinitionAsync(effect.EffectDefinitionId);
        }

        return effects;
    }

    public async Task<List<ItemEffect>> GetAllItemEffectsForCharacterAsync(int characterId)
    {
        // Get all items owned by character
        var items = await _itemDal.GetCharacterItemsAsync(characterId);
        var itemIds = items.Select(i => i.Id).ToHashSet();

        var effects = MockDb.ItemEffects
            .Where(e => itemIds.Contains(e.ItemId) && e.IsActive)
            .ToList();

        foreach (var effect in effects)
        {
            effect.Definition = await _definitionDal.GetDefinitionAsync(effect.EffectDefinitionId);
            effect.Item = items.First(i => i.Id == effect.ItemId);
        }

        return effects;
    }

    public async Task<List<ItemEffect>> GetEquippedItemEffectsAsync(int characterId)
    {
        // Get equipped items
        var items = await _itemDal.GetEquippedItemsAsync(characterId);
        var itemIds = items.Select(i => i.Id).ToHashSet();

        var effects = MockDb.ItemEffects
            .Where(e => itemIds.Contains(e.ItemId) && e.IsActive)
            .ToList();

        foreach (var effect in effects)
        {
            effect.Definition = await _definitionDal.GetDefinitionAsync(effect.EffectDefinitionId);
            effect.Item = items.First(i => i.Id == effect.ItemId);
        }

        return effects;
    }

    public async Task<ItemEffect?> GetEffectAsync(Guid id)
    {
        var effect = MockDb.ItemEffects.FirstOrDefault(e => e.Id == id);
        if (effect != null)
        {
            effect.Definition = await _definitionDal.GetDefinitionAsync(effect.EffectDefinitionId);
        }
        return effect;
    }

    public async Task<ItemEffect> ApplyEffectAsync(ItemEffect effect)
    {
        var definition = await _definitionDal.GetDefinitionAsync(effect.EffectDefinitionId);
        effect.Definition = definition;

        // Handle stacking for item effects
        var existingEffects = await GetItemEffectsAsync(effect.ItemId);
        var matchingEffects = existingEffects
            .Where(e => e.EffectDefinitionId == effect.EffectDefinitionId)
            .ToList();

        if (matchingEffects.Count > 0 && !definition.IsStackable)
        {
            switch (definition.StackBehavior)
            {
                case StackBehavior.Replace:
                    foreach (var existing in matchingEffects)
                    {
                        MockDb.ItemEffects.Remove(existing);
                    }
                    break;

                case StackBehavior.Extend:
                    var toExtend = matchingEffects.First();
                    if (toExtend.RoundsRemaining.HasValue && effect.RoundsRemaining.HasValue)
                        toExtend.RoundsRemaining += effect.RoundsRemaining;
                    return toExtend;

                case StackBehavior.Intensify:
                    var toIntensify = matchingEffects.First();
                    if (toIntensify.CurrentStacks < definition.MaxStacks)
                        toIntensify.CurrentStacks++;
                    return toIntensify;

                case StackBehavior.Independent:
                    break;
            }
        }

        if (effect.Id == Guid.Empty)
            effect.Id = Guid.NewGuid();

        MockDb.ItemEffects.Add(effect);
        return effect;
    }

    public Task<ItemEffect> UpdateEffectAsync(ItemEffect effect)
    {
        var existing = MockDb.ItemEffects.FirstOrDefault(e => e.Id == effect.Id);
        if (existing == null)
            throw new NotFoundException($"ItemEffect {effect.Id}");

        MockDb.ItemEffects.Remove(existing);
        MockDb.ItemEffects.Add(effect);
        return Task.FromResult(effect);
    }

    public Task RemoveEffectAsync(Guid id)
    {
        var effect = MockDb.ItemEffects.FirstOrDefault(e => e.Id == id);
        if (effect != null)
        {
            MockDb.ItemEffects.Remove(effect);
        }
        return Task.CompletedTask;
    }

    public Task RemoveAllEffectsAsync(Guid itemId)
    {
        var effects = MockDb.ItemEffects.Where(e => e.ItemId == itemId).ToList();
        foreach (var effect in effects)
        {
            MockDb.ItemEffects.Remove(effect);
        }
        return Task.CompletedTask;
    }

    public async Task RemoveExpiredEffectsAsync(int characterId)
    {
        var now = DateTime.UtcNow;
        var effects = await GetAllItemEffectsForCharacterAsync(characterId);
        var expired = effects.Where(e => e.EndTime.HasValue && e.EndTime < now).ToList();

        foreach (var effect in expired)
        {
            MockDb.ItemEffects.Remove(effect);
        }
    }

    public async Task<List<ItemEffect>> ProcessEndOfRoundAsync(int characterId)
    {
        var expiredEffects = new List<ItemEffect>();
        var effects = await GetAllItemEffectsForCharacterAsync(characterId);

        foreach (var effect in effects.ToList())
        {
            if (effect.RoundsRemaining.HasValue)
            {
                effect.RoundsRemaining--;
                if (effect.RoundsRemaining <= 0)
                {
                    expiredEffects.Add(effect);
                    MockDb.ItemEffects.Remove(effect);
                }
            }
        }

        return expiredEffects;
    }
}
