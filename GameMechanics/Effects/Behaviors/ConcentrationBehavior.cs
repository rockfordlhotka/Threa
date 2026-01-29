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

        // Handle casting-time concentration types (MagazineReload, SpellCasting, RitualPreparation)
        if (IsCastingTimeConcentration(state.ConcentrationType))
        {
            return TickCastingTime(effect, character, state);
        }

        // Sustained concentration handled in 22-03
        return EffectTickResult.Continue();
    }

    /// <summary>
    /// Checks if the concentration type is a casting-time type (has finite duration and deferred action).
    /// </summary>
    private static bool IsCastingTimeConcentration(string? concentrationType)
    {
        return concentrationType == "MagazineReload"
            || concentrationType == "SpellCasting"
            || concentrationType == "RitualPreparation";
    }

    /// <summary>
    /// Handles OnTick for casting-time concentration types.
    /// Increments progress, updates description, and expires early when complete.
    /// </summary>
    private EffectTickResult TickCastingTime(EffectRecord effect, CharacterEdit character, ConcentrationState state)
    {
        // Increment progress by RoundsPerTick (minimum 1)
        state.CurrentProgress += Math.Max(1, state.RoundsPerTick);

        // Update description with progress
        effect.Description = GetProgressDescription(state);

        // Check if complete
        if (state.TotalRequired > 0 && state.CurrentProgress >= state.TotalRequired)
        {
            // Serialize updated state before expiration
            effect.BehaviorState = state.Serialize();
            // Use CompleteEarly to trigger OnExpire (which executes deferred action)
            return EffectTickResult.CompleteEarly(state.CompletionMessage ?? $"{state.ConcentrationType} complete!");
        }

        // Not complete yet, save state and continue
        effect.BehaviorState = state.Serialize();
        return EffectTickResult.Continue();
    }

    /// <summary>
    /// Gets a progress description string based on concentration type.
    /// </summary>
    private static string GetProgressDescription(ConcentrationState state)
    {
        return state.ConcentrationType switch
        {
            "MagazineReload" => $"Loading magazine: {state.CurrentProgress}/{state.TotalRequired} rounds",
            "SpellCasting" => $"Casting spell: {state.CurrentProgress}/{state.TotalRequired} rounds",
            "RitualPreparation" => $"Preparing ritual: {state.CurrentProgress}/{state.TotalRequired} rounds",
            _ => $"Concentrating: {state.CurrentProgress}/{state.TotalRequired} rounds"
        };
    }

    public void OnExpire(EffectRecord effect, CharacterEdit character)
    {
        var state = ConcentrationState.FromJson(effect.BehaviorState);
        if (state == null)
            return;

        // For casting-time concentration, execute the deferred action
        if (IsCastingTimeConcentration(state.ConcentrationType) && !string.IsNullOrEmpty(state.DeferredActionType))
        {
            ExecuteDeferredAction(character, state);
        }
    }

    /// <summary>
    /// Executes the deferred action stored in the concentration state.
    /// Called when concentration completes normally (OnExpire).
    /// </summary>
    private void ExecuteDeferredAction(CharacterEdit character, ConcentrationState state)
    {
        switch (state.DeferredActionType)
        {
            case "MagazineReload":
                ExecuteMagazineReload(character, state);
                break;
            case "SpellCast":
                ExecuteSpellCast(character, state);
                break;
            // Future action types can be added here
        }
    }

    /// <summary>
    /// Handles magazine reload completion.
    /// Stores result in character.LastConcentrationResult for UI to process.
    /// </summary>
    private void ExecuteMagazineReload(CharacterEdit character, ConcentrationState state)
    {
        var payload = MagazineReloadPayload.FromJson(state.DeferredActionPayload);
        if (payload == null)
            return;

        // Store result for UI/controller to process
        character.LastConcentrationResult = new ConcentrationCompletionResult
        {
            ActionType = "MagazineReload",
            Payload = state.DeferredActionPayload,
            Message = state.CompletionMessage ?? "Magazine reloaded!",
            Success = true
        };
    }

    /// <summary>
    /// Handles spell cast completion.
    /// STUB: Full implementation pending spell system in future milestone.
    /// </summary>
    private void ExecuteSpellCast(CharacterEdit character, ConcentrationState state)
    {
        var payload = SpellCastPayload.FromJson(state.DeferredActionPayload);
        if (payload == null)
            return;

        // TODO: Spell effect creation pending spell system implementation
        // For now, just store result for UI/controller
        character.LastConcentrationResult = new ConcentrationCompletionResult
        {
            ActionType = "SpellCast",
            Payload = state.DeferredActionPayload,
            Message = state.CompletionMessage ?? "Spell cast successfully!",
            Success = true
        };
    }

    public void OnRemove(EffectRecord effect, CharacterEdit character)
    {
        var state = ConcentrationState.FromJson(effect.BehaviorState);
        if (state == null)
            return;

        // Handle casting-time concentration interruption
        if (IsCastingTimeConcentration(state.ConcentrationType))
        {
            HandleCastingTimeInterruption(character, state);
        }

        // Sustained concentration cleanup handled in 22-03
    }

    /// <summary>
    /// Handles interruption of casting-time concentration.
    /// Stores result with Success=false in character.LastConcentrationResult.
    /// NOTE: Does NOT execute the deferred action - the action is lost when interrupted.
    /// </summary>
    private void HandleCastingTimeInterruption(CharacterEdit character, ConcentrationState state)
    {
        var message = state.InterruptionMessage ?? $"{state.ConcentrationType ?? "Concentration"} interrupted!";

        character.LastConcentrationResult = new ConcentrationCompletionResult
        {
            ActionType = state.DeferredActionType ?? state.ConcentrationType ?? "Unknown",
            Payload = state.DeferredActionPayload,
            Message = message,
            Success = false
        };
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
    /// <param name="weaponItemId">The weapon CharacterItem ID being reloaded</param>
    /// <param name="magazineItemId">The magazine/ammo source CharacterItem ID</param>
    /// <param name="roundsToLoad">Number of rounds to load when complete</param>
    /// <param name="totalRounds">Number of concentration rounds required (default 3)</param>
    /// <returns>Serialized ConcentrationState JSON</returns>
    public static string CreateMagazineReloadState(
        Guid weaponItemId,
        Guid magazineItemId,
        int roundsToLoad,
        int totalRounds = 3)
    {
        var payload = new MagazineReloadPayload
        {
            WeaponItemId = weaponItemId,
            MagazineItemId = magazineItemId,
            RoundsToLoad = roundsToLoad
        };

        return new ConcentrationState
        {
            ConcentrationType = "MagazineReload",
            TotalRequired = totalRounds,
            CurrentProgress = 0,
            RoundsPerTick = 1,
            TargetItemId = magazineItemId,
            SourceItemId = weaponItemId,
            DeferredActionType = "MagazineReload",
            DeferredActionPayload = payload.Serialize(),
            CompletionMessage = $"Magazine loaded with {roundsToLoad} rounds!",
            InterruptionMessage = "Reload interrupted!"
        }.Serialize();
    }

    /// <summary>
    /// Creates state for spell casting concentration.
    /// STUB: Full implementation pending spell system in future milestone.
    /// </summary>
    /// <param name="spellId">The spell ID to cast</param>
    /// <param name="targetId">Target character ID (if single-target spell)</param>
    /// <param name="castingRounds">Number of rounds required to cast</param>
    /// <param name="spellName">Display name of the spell</param>
    /// <returns>Serialized ConcentrationState JSON</returns>
    public static string CreateSpellCastingState(
        int spellId,
        Guid? targetId,
        int castingRounds,
        string spellName)
    {
        var payload = new SpellCastPayload
        {
            SpellId = spellId,
            TargetId = targetId
        };

        return new ConcentrationState
        {
            ConcentrationType = "SpellCasting",
            TotalRequired = castingRounds,
            CurrentProgress = 0,
            RoundsPerTick = 1,
            DeferredActionType = "SpellCast",
            DeferredActionPayload = payload.Serialize(),
            CompletionMessage = $"{spellName} cast successfully!",
            InterruptionMessage = $"{spellName} interrupted!"
        }.Serialize();
    }
}

/// <summary>
/// State for concentration effects.
/// </summary>
public class ConcentrationState
{
    // ==== EXISTING FIELDS ====
    [JsonPropertyName("type")]
    public string ConcentrationType { get; set; } = "";  // "MagazineReload", "SpellCasting", "SustainedSpell"

    [JsonPropertyName("targetItemId")]
    public Guid? TargetItemId { get; set; }             // Magazine being reloaded

    [JsonPropertyName("targetItemId2")]
    public Guid? SourceItemId { get; set; }             // Weapon using the magazine

    [JsonPropertyName("totalRequired")]
    public int TotalRequired { get; set; }              // Total rounds needed

    [JsonPropertyName("currentProgress")]
    public int CurrentProgress { get; set; }            // Rounds completed

    [JsonPropertyName("roundsPerTick")]
    public int RoundsPerTick { get; set; } = 3;         // Progress per round (usually 1)

    // ==== NEW FIELDS: Casting-Time Concentration ====

    [JsonPropertyName("deferredActionType")]
    public string? DeferredActionType { get; set; }     // "MagazineReload", "SpellCast"

    [JsonPropertyName("deferredActionPayload")]
    public string? DeferredActionPayload { get; set; }  // JSON-serialized action parameters

    [JsonPropertyName("completionMessage")]
    public string? CompletionMessage { get; set; }      // "Magazine reloaded!"

    [JsonPropertyName("interruptionMessage")]
    public string? InterruptionMessage { get; set; }    // "Reload interrupted!"

    // ==== NEW FIELDS: Sustained Concentration ====

    [JsonPropertyName("spellName")]
    public string? SpellName { get; set; }              // Name of sustained spell

    [JsonPropertyName("linkedEffectIds")]
    public List<Guid>? LinkedEffectIds { get; set; }    // Active effects on target(s)

    [JsonPropertyName("fatDrainPerRound")]
    public int FatDrainPerRound { get; set; }           // FAT cost per round (0 for casting-time)

    [JsonPropertyName("vitDrainPerRound")]
    public int VitDrainPerRound { get; set; }           // VIT cost per round (0 for casting-time)

    [JsonPropertyName("sourceCasterId")]
    public Guid? SourceCasterId { get; set; }           // Character ID of caster

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

/// <summary>
/// Result of a concentration effect completing or being interrupted.
/// Used to communicate the outcome to the UI/controller layer.
/// </summary>
public class ConcentrationCompletionResult
{
    /// <summary>
    /// The type of action that was being concentrated on (e.g., "MagazineReload", "SpellCast").
    /// </summary>
    public string ActionType { get; set; } = "";

    /// <summary>
    /// The serialized payload for the action (for further processing by UI/controller).
    /// </summary>
    public string? Payload { get; set; }

    /// <summary>
    /// Message to display to the user.
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// True if concentration completed successfully, false if interrupted.
    /// </summary>
    public bool Success { get; set; }
}
