using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using Threa.Dal.Dto;

namespace GameMechanics.Effects.Behaviors;

/// <summary>
/// Behavior for Concentration effects (magazine reloading, rituals, etc.).
/// Progress is tracked per round, and the effect can be interrupted.
/// </summary>
public class ConcentrationBehavior : IEffectBehavior
{
    public const string MagazineReloadName = "Reloading Magazine";

    public EffectType EffectType => EffectType.Concentration;

    public EffectAddResult OnAdding(EffectRecord effect, CharacterEdit character)
    {
        // Only one concentration effect at a time
        foreach (var existing in character.Effects)
        {
            if (existing.EffectType == EffectType.Concentration)
            {
                return EffectAddResult.Reject("Already concentrating on another action");
            }
        }
        return EffectAddResult.AddNormally();
    }

    public void OnApply(EffectRecord effect, CharacterEdit character)
    {
        // Nothing special on initial application
    }

    public EffectTickResult OnTick(EffectRecord effect, CharacterEdit character)
    {
        var state = ConcentrationState.FromJson(effect.BehaviorState);
        if (state == null)
            return EffectTickResult.ExpireEarly("Invalid concentration state");

        switch (effect.Name)
        {
            case MagazineReloadName:
                return TickMagazineReload(effect, character, state);
        }

        return EffectTickResult.Continue();
    }

    private EffectTickResult TickMagazineReload(EffectRecord effect, CharacterEdit character, ConcentrationState state)
    {
        // Each round, load some rounds into the magazine
        int roundsToLoad = Math.Min(state.RoundsPerTick, state.TotalRequired - state.CurrentProgress);
        state.CurrentProgress += roundsToLoad;

        // Update the state
        effect.BehaviorState = state.Serialize();

        // Check if complete
        if (state.CurrentProgress >= state.TotalRequired)
        {
            return EffectTickResult.ExpireEarly($"Magazine reload complete ({state.TotalRequired} rounds loaded)");
        }

        // Update description with progress
        effect.Description = $"Loading magazine: {state.CurrentProgress}/{state.TotalRequired} rounds";

        return EffectTickResult.Continue();
    }

    public void OnExpire(EffectRecord effect, CharacterEdit character)
    {
        // Normal completion - the item updates will be handled by the UI/controller
    }

    public void OnRemove(EffectRecord effect, CharacterEdit character)
    {
        // Interrupted - partial progress may be saved depending on implementation
    }

    public IEnumerable<EffectModifier> GetAttributeModifiers(EffectRecord effect, string attributeName, int baseValue)
    {
        return [];
    }

    public IEnumerable<EffectModifier> GetAbilityScoreModifiers(EffectRecord effect, string skillName, string attributeName, int currentAS)
    {
        // Concentrating on reloading reduces combat effectiveness
        if (effect.Name == MagazineReloadName)
        {
            if (skillName.Contains("Attack", StringComparison.OrdinalIgnoreCase) ||
                skillName.Contains("Ranged", StringComparison.OrdinalIgnoreCase))
            {
                return [new EffectModifier { Description = "Concentrating on reload", Value = -2 }];
            }
        }
        return [];
    }

    public IEnumerable<EffectModifier> GetSuccessValueModifiers(EffectRecord effect, string actionType, int currentSV)
    {
        return [];
    }

    /// <summary>
    /// Checks if the character is concentrating.
    /// </summary>
    public static bool IsConcentrating(CharacterEdit character)
    {
        foreach (var effect in character.Effects)
        {
            if (effect.EffectType == EffectType.Concentration)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if the character is reloading a magazine.
    /// </summary>
    public static bool IsReloadingMagazine(CharacterEdit character)
    {
        foreach (var effect in character.Effects)
        {
            if (effect.EffectType == EffectType.Concentration && effect.Name == MagazineReloadName)
                return true;
        }
        return false;
    }

    /// <summary>
    /// Gets the current reload progress.
    /// </summary>
    public static (int current, int total)? GetReloadProgress(CharacterEdit character)
    {
        foreach (var effect in character.Effects)
        {
            if (effect.EffectType == EffectType.Concentration && effect.Name == MagazineReloadName)
            {
                var state = ConcentrationState.FromJson(effect.BehaviorState);
                if (state != null)
                    return (state.CurrentProgress, state.TotalRequired);
            }
        }
        return null;
    }

    /// <summary>
    /// Breaks concentration, removing the effect.
    /// </summary>
    public static void BreakConcentration(CharacterEdit character, string reason = "Action interrupted")
    {
        var toRemove = new List<EffectRecord>();
        foreach (var effect in character.Effects)
        {
            if (effect.EffectType == EffectType.Concentration)
                toRemove.Add(effect);
        }

        foreach (var effect in toRemove)
        {
            character.Effects.RemoveEffect(effect.Id);
        }
    }

    /// <summary>
    /// Creates state for magazine reload concentration.
    /// </summary>
    public static string CreateMagazineReloadState(Guid magazineItemId, int totalRounds, int roundsPerTick = 3)
    {
        return new ConcentrationState
        {
            ConcentrationType = "MagazineReload",
            TargetItemId = magazineItemId,
            TotalRequired = totalRounds,
            CurrentProgress = 0,
            RoundsPerTick = roundsPerTick
        }.Serialize();
    }
}

/// <summary>
/// State for concentration effects.
/// </summary>
public class ConcentrationState
{
    [JsonPropertyName("type")]
    public string ConcentrationType { get; set; } = "";

    [JsonPropertyName("targetItemId")]
    public Guid? TargetItemId { get; set; }

    [JsonPropertyName("targetItemId2")]
    public Guid? SourceItemId { get; set; }

    [JsonPropertyName("totalRequired")]
    public int TotalRequired { get; set; }

    [JsonPropertyName("currentProgress")]
    public int CurrentProgress { get; set; }

    [JsonPropertyName("roundsPerTick")]
    public int RoundsPerTick { get; set; } = 3;

    public string Serialize()
    {
        return JsonSerializer.Serialize(this, new JsonSerializerOptions
        {
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
    }

    public static ConcentrationState? FromJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<ConcentrationState>(json);
        }
        catch
        {
            return null;
        }
    }
}
