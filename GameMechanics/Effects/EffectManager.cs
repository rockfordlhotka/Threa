using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Effects;

/// <summary>
/// Main service for managing effects on characters and items.
/// Handles effect application, removal, stacking, duration, and time-based processing.
/// </summary>
public class EffectManager
{
    private readonly ICharacterEffectDal _characterEffectDal;
    private readonly IItemEffectDal _itemEffectDal;
    private readonly IEffectDefinitionDal _definitionDal;
    private readonly EffectCalculator _calculator;

    public EffectManager(
        ICharacterEffectDal characterEffectDal,
        IItemEffectDal itemEffectDal,
        IEffectDefinitionDal definitionDal)
    {
        _characterEffectDal = characterEffectDal;
        _itemEffectDal = itemEffectDal;
        _definitionDal = definitionDal;
        _calculator = new EffectCalculator();
    }

    #region Character Effects

    /// <summary>
    /// Applies an effect to a character by effect name.
    /// </summary>
    /// <param name="characterId">The target character.</param>
    /// <param name="effectName">The name of the effect definition.</param>
    /// <param name="durationOverride">Optional override for duration value.</param>
    /// <param name="sourceEntityId">Optional source of the effect.</param>
    /// <returns>The applied effect instance.</returns>
    public async Task<CharacterEffect> ApplyEffectAsync(
        int characterId,
        string effectName,
        int? durationOverride = null,
        Guid? sourceEntityId = null)
    {
        var definition = await _definitionDal.GetDefinitionByNameAsync(effectName)
            ?? throw new ArgumentException($"Effect definition '{effectName}' not found.", nameof(effectName));

        return await ApplyEffectAsync(characterId, definition, durationOverride, sourceEntityId);
    }

    /// <summary>
    /// Applies an effect to a character using an effect definition.
    /// </summary>
    public async Task<CharacterEffect> ApplyEffectAsync(
        int characterId,
        EffectDefinition definition,
        int? durationOverride = null,
        Guid? sourceEntityId = null,
        string? woundLocation = null)
    {
        var duration = durationOverride ?? definition.DefaultDuration;
        var now = DateTime.UtcNow;

        var effect = new CharacterEffect
        {
            CharacterId = characterId,
            EffectDefinitionId = definition.Id,
            StartTime = now,
            SourceEntityId = sourceEntityId,
            WoundLocation = woundLocation ?? definition.WoundLocation,
            CurrentStacks = 1
        };

        // Set duration based on type
        switch (definition.DurationType)
        {
            case DurationType.Rounds:
                effect.RoundsRemaining = duration;
                break;
            case DurationType.Minutes:
                effect.EndTime = now.AddMinutes(duration);
                break;
            case DurationType.Hours:
                effect.EndTime = now.AddHours(duration);
                break;
            case DurationType.Days:
                effect.EndTime = now.AddDays(duration);
                break;
            case DurationType.Weeks:
                effect.EndTime = now.AddDays(duration * 7);
                break;
            case DurationType.Permanent:
            case DurationType.UntilRemoved:
                // No end time
                break;
        }

        // Set initial tick counter for DoT effects
        var dotImpact = definition.Impacts.FirstOrDefault(i => i.ImpactType == EffectImpactType.DamageOverTime);
        if (dotImpact?.DamageInterval != null)
        {
            effect.RoundsUntilTick = dotImpact.DamageInterval;
        }

        return await _characterEffectDal.ApplyEffectAsync(effect);
    }

    /// <summary>
    /// Gets all active effects on a character.
    /// </summary>
    public Task<List<CharacterEffect>> GetCharacterEffectsAsync(int characterId)
        => _characterEffectDal.GetCharacterEffectsAsync(characterId);

    /// <summary>
    /// Gets effects of a specific type on a character.
    /// </summary>
    public Task<List<CharacterEffect>> GetCharacterEffectsByTypeAsync(int characterId, EffectType effectType)
        => _characterEffectDal.GetCharacterEffectsByTypeAsync(characterId, effectType);

    /// <summary>
    /// Checks if a character has a specific effect.
    /// </summary>
    public Task<bool> HasEffectAsync(int characterId, string effectName)
        => _characterEffectDal.HasEffectAsync(characterId, effectName);

    /// <summary>
    /// Removes a specific effect from a character.
    /// </summary>
    public Task RemoveEffectAsync(Guid effectId)
        => _characterEffectDal.RemoveEffectAsync(effectId);

    /// <summary>
    /// Removes all effects of a type from a character.
    /// </summary>
    public Task RemoveEffectsByTypeAsync(int characterId, EffectType effectType)
        => _characterEffectDal.RemoveEffectsByTypeAsync(characterId, effectType);

    #endregion

    #region Item Effects

    /// <summary>
    /// Applies an effect to an item by effect name.
    /// </summary>
    public async Task<ItemEffect> ApplyItemEffectAsync(
        Guid itemId,
        string effectName,
        int? durationOverride = null,
        Guid? sourceEntityId = null)
    {
        var definition = await _definitionDal.GetDefinitionByNameAsync(effectName)
            ?? throw new ArgumentException($"Effect definition '{effectName}' not found.", nameof(effectName));

        return await ApplyItemEffectAsync(itemId, definition, durationOverride, sourceEntityId);
    }

    /// <summary>
    /// Applies an effect to an item using an effect definition.
    /// </summary>
    public async Task<ItemEffect> ApplyItemEffectAsync(
        Guid itemId,
        EffectDefinition definition,
        int? durationOverride = null,
        Guid? sourceEntityId = null)
    {
        var duration = durationOverride ?? definition.DefaultDuration;
        var now = DateTime.UtcNow;

        var effect = new ItemEffect
        {
            ItemId = itemId,
            EffectDefinitionId = definition.Id,
            StartTime = now,
            SourceEntityId = sourceEntityId,
            CurrentStacks = 1
        };

        // Set duration based on type
        switch (definition.DurationType)
        {
            case DurationType.Rounds:
                effect.RoundsRemaining = duration;
                break;
            case DurationType.Minutes:
                effect.EndTime = now.AddMinutes(duration);
                break;
            case DurationType.Hours:
                effect.EndTime = now.AddHours(duration);
                break;
            case DurationType.Days:
                effect.EndTime = now.AddDays(duration);
                break;
            case DurationType.Weeks:
                effect.EndTime = now.AddDays(duration * 7);
                break;
        }

        return await _itemEffectDal.ApplyEffectAsync(effect);
    }

    /// <summary>
    /// Gets all effects on an item.
    /// </summary>
    public Task<List<ItemEffect>> GetItemEffectsAsync(Guid itemId)
        => _itemEffectDal.GetItemEffectsAsync(itemId);

    /// <summary>
    /// Gets all effects on items held by a character (including inventory).
    /// </summary>
    public Task<List<ItemEffect>> GetAllItemEffectsForCharacterAsync(int characterId)
        => _itemEffectDal.GetAllItemEffectsForCharacterAsync(characterId);

    /// <summary>
    /// Gets effects on equipped items only.
    /// </summary>
    public Task<List<ItemEffect>> GetEquippedItemEffectsAsync(int characterId)
        => _itemEffectDal.GetEquippedItemEffectsAsync(characterId);

    /// <summary>
    /// Removes an effect from an item.
    /// </summary>
    public Task RemoveItemEffectAsync(Guid effectId)
        => _itemEffectDal.RemoveEffectAsync(effectId);

    #endregion

    #region Effect Calculations

    /// <summary>
    /// Gets the total attribute modifier from all active effects on a character.
    /// Includes effects from character and equipped items.
    /// </summary>
    public async Task<int> GetTotalAttributeModifierAsync(int characterId, string attributeName)
    {
        var characterEffects = await GetCharacterEffectsAsync(characterId);
        var itemEffects = await GetEquippedItemEffectsAsync(characterId);

        return _calculator.GetTotalAttributeModifier(characterEffects, itemEffects, attributeName);
    }

    /// <summary>
    /// Gets the total skill modifier from all active effects.
    /// </summary>
    public async Task<int> GetTotalSkillModifierAsync(int characterId, string skillName)
    {
        var characterEffects = await GetCharacterEffectsAsync(characterId);
        var itemEffects = await GetEquippedItemEffectsAsync(characterId);

        return _calculator.GetTotalSkillModifier(characterEffects, itemEffects, skillName);
    }

    /// <summary>
    /// Gets the total AS modifier from all active effects.
    /// </summary>
    public async Task<int> GetTotalASModifierAsync(int characterId, string? targetType = null)
    {
        var characterEffects = await GetCharacterEffectsAsync(characterId);
        var itemEffects = await GetEquippedItemEffectsAsync(characterId);

        return _calculator.GetTotalASModifier(characterEffects, itemEffects, targetType);
    }

    /// <summary>
    /// Gets the total AV modifier from all active effects.
    /// </summary>
    public async Task<int> GetTotalAVModifierAsync(int characterId)
    {
        var characterEffects = await GetCharacterEffectsAsync(characterId);
        var itemEffects = await GetEquippedItemEffectsAsync(characterId);

        return _calculator.GetTotalAVModifier(characterEffects, itemEffects);
    }

    /// <summary>
    /// Gets the total TV modifier from all active effects.
    /// </summary>
    public async Task<int> GetTotalTVModifierAsync(int characterId)
    {
        var characterEffects = await GetCharacterEffectsAsync(characterId);
        var itemEffects = await GetEquippedItemEffectsAsync(characterId);

        return _calculator.GetTotalTVModifier(characterEffects, itemEffects);
    }

    /// <summary>
    /// Gets the total SV modifier from all active effects.
    /// </summary>
    public async Task<int> GetTotalSVModifierAsync(int characterId)
    {
        var characterEffects = await GetCharacterEffectsAsync(characterId);
        var itemEffects = await GetEquippedItemEffectsAsync(characterId);

        return _calculator.GetTotalSVModifier(characterEffects, itemEffects);
    }

    /// <summary>
    /// Gets the damage over time that should be applied this tick.
    /// Returns FAT damage and VIT damage separately.
    /// </summary>
    public async Task<(int fatDamage, int vitDamage)> GetDamageOverTimeAsync(int characterId)
    {
        var characterEffects = await GetCharacterEffectsAsync(characterId);
        return _calculator.GetDamageOverTime(characterEffects);
    }

    /// <summary>
    /// Gets all effects for display/UI purposes.
    /// </summary>
    public async Task<EffectSummary> GetEffectSummaryAsync(int characterId)
    {
        var characterEffects = await GetCharacterEffectsAsync(characterId);
        var itemEffects = await GetEquippedItemEffectsAsync(characterId);

        return new EffectSummary
        {
            CharacterEffects = characterEffects,
            EquippedItemEffects = itemEffects,
            TotalASModifier = _calculator.GetTotalASModifier(characterEffects, itemEffects),
            TotalAVModifier = _calculator.GetTotalAVModifier(characterEffects, itemEffects),
            TotalTVModifier = _calculator.GetTotalTVModifier(characterEffects, itemEffects),
            AttributeModifiers = GetAttributeModifierSummary(characterEffects, itemEffects)
        };
    }

    private Dictionary<string, int> GetAttributeModifierSummary(
        List<CharacterEffect> characterEffects, 
        List<ItemEffect> itemEffects)
    {
        var attributes = new[] { "STR", "DEX", "END", "INT", "ITT", "WIL", "PHY" };
        var result = new Dictionary<string, int>();
        foreach (var attr in attributes)
        {
            var mod = _calculator.GetTotalAttributeModifier(characterEffects, itemEffects, attr);
            if (mod != 0)
                result[attr] = mod;
        }
        return result;
    }

    #endregion

    #region Time Processing

    /// <summary>
    /// Processes end-of-round effects for a character.
    /// Decrements round counters, applies DoT damage, removes expired effects.
    /// </summary>
    /// <returns>Results of end-of-round processing.</returns>
    public async Task<EndOfRoundResult> ProcessEndOfRoundAsync(int characterId)
    {
        var result = new EndOfRoundResult();

        // Get damage before processing (for effects that tick this round)
        var characterEffects = await GetCharacterEffectsAsync(characterId);
        var (fatDamage, vitDamage) = _calculator.GetDamageOverTime(characterEffects);
        result.FatigueDamage = fatDamage;
        result.VitalityDamage = vitDamage;

        // Process character effects
        result.ExpiredCharacterEffects = await _characterEffectDal.ProcessEndOfRoundAsync(characterId);

        // Process item effects
        result.ExpiredItemEffects = await _itemEffectDal.ProcessEndOfRoundAsync(characterId);

        return result;
    }

    /// <summary>
    /// Removes all time-expired effects (for non-combat time progression).
    /// </summary>
    public async Task ProcessTimeExpirationAsync(int characterId)
    {
        await _characterEffectDal.RemoveExpiredEffectsAsync(characterId);
        await _itemEffectDal.RemoveExpiredEffectsAsync(characterId);
    }

    #endregion

    #region Wound Effects

    /// <summary>
    /// Applies a wound effect to a character at a specific location.
    /// </summary>
    public async Task<CharacterEffect> ApplyWoundAsync(int characterId, string location)
    {
        var woundDef = await _definitionDal.GetDefinitionByNameAsync("Wound")
            ?? throw new InvalidOperationException("Wound effect definition not found in database.");

        return await ApplyEffectAsync(characterId, woundDef, woundLocation: location);
    }

    /// <summary>
    /// Gets all wound effects on a character.
    /// </summary>
    public async Task<List<CharacterEffect>> GetWoundsAsync(int characterId)
    {
        return await GetCharacterEffectsByTypeAsync(characterId, EffectType.Wound);
    }

    /// <summary>
    /// Gets the total wound count for a character.
    /// </summary>
    public async Task<int> GetTotalWoundCountAsync(int characterId)
    {
        var wounds = await GetWoundsAsync(characterId);
        return wounds.Sum(w => w.CurrentStacks);
    }

    #endregion
}

/// <summary>
/// Summary of all effects on a character for UI display.
/// </summary>
public class EffectSummary
{
    public List<CharacterEffect> CharacterEffects { get; set; } = [];
    public List<ItemEffect> EquippedItemEffects { get; set; } = [];
    public int TotalASModifier { get; set; }
    public int TotalAVModifier { get; set; }
    public int TotalTVModifier { get; set; }
    public Dictionary<string, int> AttributeModifiers { get; set; } = [];
}

/// <summary>
/// Results from end-of-round processing.
/// </summary>
public class EndOfRoundResult
{
    public List<CharacterEffect> ExpiredCharacterEffects { get; set; } = [];
    public List<ItemEffect> ExpiredItemEffects { get; set; } = [];
    public int FatigueDamage { get; set; }
    public int VitalityDamage { get; set; }
}
