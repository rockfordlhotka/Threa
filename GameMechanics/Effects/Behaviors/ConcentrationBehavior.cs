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

        // Handle sustained concentration types (SustainedSpell, SustainedAbility, MentalControl)
        if (IsSustainedConcentration(state.ConcentrationType))
        {
            return TickSustainedConcentration(effect, character, state);
        }

        // Unknown concentration type, just continue
        return EffectTickResult.Continue();
    }

    /// <summary>
    /// Checks if the concentration type is a casting-time type (has finite duration and deferred action).
    /// </summary>
    private static bool IsCastingTimeConcentration(string? concentrationType)
    {
        return concentrationType == "MagazineReload"
            || concentrationType == "SpellCasting"
            || concentrationType == "RitualPreparation"
            || concentrationType == "AmmoContainerReload"
            || concentrationType == "AmmoContainerUnload";
    }

    /// <summary>
    /// Checks if the concentration type is a sustained type (maintains active effect indefinitely).
    /// </summary>
    private static bool IsSustainedConcentration(string? concentrationType)
    {
        return concentrationType == "SustainedSpell"
            || concentrationType == "SustainedAbility"
            || concentrationType == "MentalControl";
    }

    /// <summary>
    /// Applies FAT/VIT drain to the character for sustained concentration.
    /// </summary>
    /// <param name="character">The character concentrating</param>
    /// <param name="state">The concentration state with drain values</param>
    /// <returns>True if character can continue, false if exhausted</returns>
    private static bool ApplyDrain(CharacterEdit character, ConcentrationState state)
    {
        // Apply FAT drain
        if (state.FatDrainPerRound > 0)
        {
            character.Fatigue.PendingDamage += state.FatDrainPerRound;
        }

        // Apply VIT drain
        if (state.VitDrainPerRound > 0)
        {
            character.Vitality.PendingDamage += state.VitDrainPerRound;
        }

        // Check if character is exhausted
        // Current Value minus pending damage equals effective pool
        // Exhausted when effective pool <= 0
        int effectiveFat = character.Fatigue.Value - character.Fatigue.PendingDamage;
        int effectiveVit = character.Vitality.Value - character.Vitality.PendingDamage;

        bool fatigueExhausted = effectiveFat <= 0;
        bool vitalityExhausted = effectiveVit <= 0;

        return !(fatigueExhausted || vitalityExhausted);
    }

    /// <summary>
    /// Handles OnTick for sustained concentration types.
    /// Applies FAT/VIT drain and expires early if character is exhausted.
    /// </summary>
    private EffectTickResult TickSustainedConcentration(EffectRecord effect, CharacterEdit character, ConcentrationState state)
    {
        // Apply drain
        bool canContinue = ApplyDrain(character, state);

        if (!canContinue)
        {
            // Character is exhausted, concentration breaks
            string message = $"Too exhausted to maintain {state.SpellName ?? "concentration"}";
            return EffectTickResult.ExpireEarly(message);
        }

        // Update description with drain info
        var drainParts = new List<string>();
        if (state.FatDrainPerRound > 0)
            drainParts.Add($"{state.FatDrainPerRound} FAT");
        if (state.VitDrainPerRound > 0)
            drainParts.Add($"{state.VitDrainPerRound} VIT");

        string drainDesc = drainParts.Count > 0 ? $" ({string.Join(" + ", drainParts)}/round)" : "";
        effect.Description = $"Sustaining {state.SpellName ?? "effect"}{drainDesc}";

        return EffectTickResult.Continue();
    }

    /// <summary>
    /// Stores linked effect removal info in LastConcentrationResult for UI processing.
    /// The actual removal must be done by the UI/controller layer which has access to all characters.
    /// </summary>
    private void PrepareLinkedEffectRemoval(CharacterEdit character, ConcentrationState state)
    {
        if (state.LinkedEffectIds == null || state.LinkedEffectIds.Count == 0)
            return;

        // Store result for UI/controller to process
        // The UI layer will iterate through table characters and remove effects with matching IDs
        character.LastConcentrationResult = new ConcentrationCompletionResult
        {
            ActionType = "SustainedBreak",
            Payload = JsonSerializer.Serialize(new
            {
                LinkedEffectIds = state.LinkedEffectIds,
                SpellName = state.SpellName
            }),
            Message = $"{state.SpellName ?? "Sustained effect"} ended",
            Success = false  // Indicates concentration was broken, not completed
        };
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
            "AmmoContainerReload" => $"Loading ammo: {state.CurrentProgress}/{state.TotalRequired} rounds",
            "AmmoContainerUnload" => $"Unloading ammo: {state.CurrentProgress}/{state.TotalRequired} rounds",
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
            case "AmmoContainerReload":
                ExecuteAmmoContainerReload(character, state);
                break;
            case "AmmoContainerUnload":
                ExecuteAmmoContainerUnload(character, state);
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

    /// <summary>
    /// Handles ammo container reload completion.
    /// Stores result in character.LastConcentrationResult for UI to process.
    /// </summary>
    private void ExecuteAmmoContainerReload(CharacterEdit character, ConcentrationState state)
    {
        var payload = AmmoContainerReloadPayload.FromJson(state.DeferredActionPayload);
        if (payload == null)
            return;

        // Store result for UI/controller to process
        character.LastConcentrationResult = new ConcentrationCompletionResult
        {
            ActionType = "AmmoContainerReload",
            Payload = state.DeferredActionPayload,
            Message = state.CompletionMessage ?? "Ammo container reloaded!",
            Success = true
        };
    }

    /// <summary>
    /// Handles ammo container unload completion.
    /// Stores result in character.LastConcentrationResult for server-side processing.
    /// </summary>
    private void ExecuteAmmoContainerUnload(CharacterEdit character, ConcentrationState state)
    {
        var payload = AmmoContainerUnloadPayload.FromJson(state.DeferredActionPayload);
        if (payload == null)
            return;

        // Store result for TimeAdvancementService to process
        character.LastConcentrationResult = new ConcentrationCompletionResult
        {
            ActionType = "AmmoContainerUnload",
            Payload = state.DeferredActionPayload,
            Message = state.CompletionMessage ?? "Ammo container unloaded!",
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

        // Handle sustained concentration cleanup
        if (IsSustainedConcentration(state.ConcentrationType))
        {
            PrepareLinkedEffectRemoval(character, state);
        }
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

    /// <summary>
    /// Creates state for sustained spell concentration.
    /// </summary>
    /// <param name="spellName">Display name of the sustained spell</param>
    /// <param name="linkedEffectIds">Effect IDs on target characters (can be empty, added later)</param>
    /// <param name="fatDrainPerRound">FAT cost per round (default 1)</param>
    /// <param name="vitDrainPerRound">VIT cost per round (default 0)</param>
    /// <param name="casterId">The character ID of the caster</param>
    /// <returns>Serialized ConcentrationState JSON</returns>
    public static string CreateSustainedConcentrationState(
        string spellName,
        List<Guid>? linkedEffectIds = null,
        int fatDrainPerRound = 1,
        int vitDrainPerRound = 0,
        Guid? casterId = null)
    {
        return new ConcentrationState
        {
            ConcentrationType = "SustainedSpell",
            SpellName = spellName,
            LinkedEffectIds = linkedEffectIds ?? new List<Guid>(),
            FatDrainPerRound = fatDrainPerRound,
            VitDrainPerRound = vitDrainPerRound,
            SourceCasterId = casterId,
            // No fixed duration - sustained until dropped or broken
            TotalRequired = 0,
            CurrentProgress = 0
        }.Serialize();
    }

    /// <summary>
    /// Creates state for ammo container reload concentration.
    /// Rate: 3 rounds per game round (1 round per second).
    /// </summary>
    /// <param name="containerId">The ammo container CharacterItem ID being loaded</param>
    /// <param name="sourceItemId">The loose ammo source CharacterItem ID</param>
    /// <param name="roundsToLoad">Number of rounds to load when complete</param>
    /// <param name="containerName">Display name of the container</param>
    /// <param name="ammoType">Type of ammo being loaded (optional)</param>
    /// <returns>Serialized ConcentrationState JSON</returns>
    public static string CreateAmmoContainerReloadState(
        Guid containerId,
        Guid sourceItemId,
        int roundsToLoad,
        string containerName,
        string? ammoType = null)
    {
        // Rate: 3 rounds per game round (1 round per second)
        int totalRounds = (int)Math.Ceiling(roundsToLoad / 3.0);

        var payload = new AmmoContainerReloadPayload
        {
            ContainerId = containerId,
            SourceItemId = sourceItemId,
            RoundsToLoad = roundsToLoad,
            AmmoType = ammoType,
            ContainerName = containerName
        };

        return new ConcentrationState
        {
            ConcentrationType = "AmmoContainerReload",
            TotalRequired = totalRounds,
            CurrentProgress = 0,
            RoundsPerTick = 1,
            TargetItemId = containerId,
            SourceItemId = sourceItemId,
            DeferredActionType = "AmmoContainerReload",
            DeferredActionPayload = payload.Serialize(),
            CompletionMessage = $"{containerName} loaded with {roundsToLoad} rounds!",
            InterruptionMessage = "Reload interrupted!"
        }.Serialize();
    }

    /// <summary>
    /// Creates state for ammo container unload concentration.
    /// Used when unloading individual rounds from a magazine/container back to loose ammo.
    /// Rate: 3 rounds per game round (1 round per second).
    /// </summary>
    /// <param name="containerId">The ammo container CharacterItem ID being unloaded</param>
    /// <param name="characterId">The character ID who owns the container</param>
    /// <param name="roundsToUnload">Number of rounds to unload when complete</param>
    /// <param name="containerName">Display name of the container</param>
    /// <param name="ammoType">Type of ammo being unloaded (optional)</param>
    /// <returns>Serialized ConcentrationState JSON</returns>
    public static string CreateAmmoContainerUnloadState(
        Guid containerId,
        int characterId,
        int roundsToUnload,
        string containerName,
        string? ammoType = null)
    {
        // Rate: 3 rounds per game round (1 round per second)
        int totalRounds = (int)Math.Ceiling(roundsToUnload / 3.0);

        var payload = new AmmoContainerUnloadPayload
        {
            ContainerId = containerId,
            CharacterId = characterId,
            RoundsToUnload = roundsToUnload,
            AmmoType = ammoType,
            ContainerName = containerName
        };

        return new ConcentrationState
        {
            ConcentrationType = "AmmoContainerUnload",
            TotalRequired = totalRounds,
            CurrentProgress = 0,
            RoundsPerTick = 1,
            TargetItemId = containerId,
            DeferredActionType = "AmmoContainerUnload",
            DeferredActionPayload = payload.Serialize(),
            CompletionMessage = $"{containerName} unloaded - {roundsToUnload} rounds returned to inventory!",
            InterruptionMessage = "Unload interrupted!"
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
