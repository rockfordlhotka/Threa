using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Csla;
using GameMechanics.Combat;
using GameMechanics.Effects.Behaviors;
using GameMechanics.Messaging;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Time;

/// <summary>
/// Result from processing time advancement for all characters at a table.
/// </summary>
public class TimeAdvancementResult
{
    /// <summary>
    /// The table ID that was processed.
    /// </summary>
    public Guid TableId { get; init; }

    /// <summary>
    /// The type of time event processed.
    /// </summary>
    public TimeEventType EventType { get; init; }

    /// <summary>
    /// Number of units advanced.
    /// </summary>
    public int Count { get; init; }

    /// <summary>
    /// IDs of characters that were successfully updated.
    /// </summary>
    public List<int> UpdatedCharacterIds { get; init; } = new();

    /// <summary>
    /// IDs of characters that failed to update.
    /// </summary>
    public List<int> FailedCharacterIds { get; init; } = new();

    /// <summary>
    /// Error messages for failed updates.
    /// </summary>
    public List<string> Errors { get; init; } = new();

    /// <summary>
    /// Summary messages for display.
    /// </summary>
    public List<string> Messages { get; init; } = new();

    /// <summary>
    /// Whether all characters were successfully updated.
    /// </summary>
    public bool Success => FailedCharacterIds.Count == 0;
}

/// <summary>
/// Service for processing time advancement on all characters at a table.
/// This is the authoritative source for time-based game mechanics processing.
/// 
/// When the GM advances time, this service:
/// 1. Loads all characters attached to the table
/// 2. Processes the appropriate time effects on each character
/// 3. Saves the updated characters
/// 4. Publishes a notification so player clients can refresh their displays
/// 
/// This ensures consistent game state even for disconnected players.
/// </summary>
public class TimeAdvancementService
{
    private readonly ITableDal _tableDal;
    private readonly IDataPortal<CharacterEdit> _characterPortal;
    private readonly IChildDataPortal<EffectRecord> _effectPortal;
    private readonly ITimeEventPublisher _timeEventPublisher;
    private readonly ICharacterItemDal _itemDal;
    private readonly IItemTemplateDal _templateDal;

    public TimeAdvancementService(
        ITableDal tableDal,
        IDataPortal<CharacterEdit> characterPortal,
        IChildDataPortal<EffectRecord> effectPortal,
        ITimeEventPublisher timeEventPublisher,
        ICharacterItemDal itemDal,
        IItemTemplateDal templateDal)
    {
        _tableDal = tableDal;
        _characterPortal = characterPortal;
        _effectPortal = effectPortal;
        _timeEventPublisher = timeEventPublisher;
        _itemDal = itemDal;
        _templateDal = templateDal;
    }

    /// <summary>
    /// Advances time by the specified number of rounds for all characters at a table.
    /// Processes end-of-round effects (AP recovery, pending pool flow, effect duration).
    /// </summary>
    /// <param name="tableId">The table ID.</param>
    /// <param name="roundCount">Number of rounds to advance (default 1).</param>
    /// <param name="currentTableTimeSeconds">Current game time in seconds from the table.</param>
    /// <returns>Result containing updated character IDs and any errors.</returns>
    public async Task<TimeAdvancementResult> AdvanceRoundsAsync(
        Guid tableId, 
        int roundCount = 1,
        long currentTableTimeSeconds = 0)
    {
        var result = new TimeAdvancementResult
        {
            TableId = tableId,
            EventType = TimeEventType.EndOfRound,
            Count = roundCount
        };

        try
        {
            // Load all characters at the table
            var tableCharacters = await _tableDal.GetTableCharactersAsync(tableId);

            foreach (var tc in tableCharacters)
            {
                try
                {
                    // Load the character
                    var character = await _characterPortal.FetchAsync(tc.CharacterId);

                    // Sync character's game time with table
                    if (currentTableTimeSeconds > 0 && 
                        (character.CurrentGameTimeSeconds == 0 || character.CurrentGameTimeSeconds < currentTableTimeSeconds))
                    {
                        character.CurrentGameTimeSeconds = currentTableTimeSeconds;
                    }

                    // Process end-of-round for each round
                    for (int i = 0; i < roundCount; i++)
                    {
                        character.EndOfRound(_effectPortal);

                        // Process any concentration result (deferred actions like ammo reload)
                        if (character.LastConcentrationResult != null)
                        {
                            var concentrationMessage = await ProcessConcentrationResultAsync(character);
                            if (!string.IsNullOrEmpty(concentrationMessage))
                            {
                                result.Messages.Add(concentrationMessage);
                            }
                        }
                    }

                    // Save the updated character
                    await _characterPortal.UpdateAsync(character);
                    result.UpdatedCharacterIds.Add(tc.CharacterId);
                }
                catch (Exception ex)
                {
                    result.FailedCharacterIds.Add(tc.CharacterId);
                    result.Errors.Add($"Character {tc.CharacterId}: {ex.Message}");
                }
            }

            result.Messages.Add($"Processed {roundCount} round(s) for {result.UpdatedCharacterIds.Count} character(s)");

            // Notify clients that characters have been updated
            await PublishCharactersUpdatedAsync(tableId, result.UpdatedCharacterIds, TimeEventType.EndOfRound);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to load table characters: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Advances time by the specified time skip (minutes, turns, hours, days, weeks).
    /// Processes recovery, effect expiration, and other time-based mechanics.
    /// </summary>
    /// <param name="tableId">The table ID.</param>
    /// <param name="skipUnit">The time unit to skip.</param>
    /// <param name="count">Number of units to skip.</param>
    /// <param name="currentTableTimeSeconds">Current game time in seconds from the table.</param>
    /// <returns>Result containing updated character IDs and any errors.</returns>
    public async Task<TimeAdvancementResult> ProcessTimeSkipAsync(
        Guid tableId,
        TimeEventType skipUnit,
        int count = 1,
        long currentTableTimeSeconds = 0)
    {
        var result = new TimeAdvancementResult
        {
            TableId = tableId,
            EventType = skipUnit,
            Count = count
        };

        try
        {
            // Load all characters at the table
            var tableCharacters = await _tableDal.GetTableCharactersAsync(tableId);

            foreach (var tc in tableCharacters)
            {
                try
                {
                    // Load the character
                    var character = await _characterPortal.FetchAsync(tc.CharacterId);

                    // Sync character's game time with table
                    if (currentTableTimeSeconds > 0 && 
                        (character.CurrentGameTimeSeconds == 0 || character.CurrentGameTimeSeconds < currentTableTimeSeconds))
                    {
                        character.CurrentGameTimeSeconds = currentTableTimeSeconds;
                    }

                    // Process the time skip
                    character.ProcessTimeSkip(skipUnit, count, _effectPortal);

                    // Process any concentration result (deferred actions like ammo reload)
                    if (character.LastConcentrationResult != null)
                    {
                        var concentrationMessage = await ProcessConcentrationResultAsync(character);
                        if (!string.IsNullOrEmpty(concentrationMessage))
                        {
                            result.Messages.Add(concentrationMessage);
                        }
                    }

                    // Save the updated character
                    await _characterPortal.UpdateAsync(character);
                    result.UpdatedCharacterIds.Add(tc.CharacterId);
                }
                catch (Exception ex)
                {
                    result.FailedCharacterIds.Add(tc.CharacterId);
                    result.Errors.Add($"Character {tc.CharacterId}: {ex.Message}");
                }
            }

            var timeDescription = GetTimeDescription(skipUnit, count);
            result.Messages.Add($"Processed {timeDescription} for {result.UpdatedCharacterIds.Count} character(s)");

            // Notify clients that characters have been updated
            await PublishCharactersUpdatedAsync(tableId, result.UpdatedCharacterIds, skipUnit);
        }
        catch (Exception ex)
        {
            result.Errors.Add($"Failed to load table characters: {ex.Message}");
        }

        return result;
    }

    /// <summary>
    /// Publishes a notification that characters have been updated after time processing.
    /// Player clients listen for this to refresh their character state from the database.
    /// </summary>
    private async Task PublishCharactersUpdatedAsync(Guid tableId, List<int> characterIds, TimeEventType eventType)
    {
        if (characterIds.Count == 0) return;

        await _timeEventPublisher.PublishCharactersUpdatedAsync(new CharactersUpdatedMessage
        {
            TableId = tableId,
            CharacterIds = characterIds,
            EventType = eventType,
            SourceId = "TimeAdvancementService"
        });
    }

    private static string GetTimeDescription(TimeEventType eventType, int count)
    {
        return eventType switch
        {
            TimeEventType.EndOfRound => $"{count} round(s)",
            TimeEventType.EndOfMinute => $"{count} minute(s)",
            TimeEventType.EndOfTurn => $"{count} turn(s) ({count * 10} minutes)",
            TimeEventType.EndOfHour => $"{count} hour(s)",
            TimeEventType.EndOfDay => $"{count} day(s)",
            TimeEventType.EndOfWeek => $"{count} week(s)",
            _ => $"{count} time unit(s)"
        };
    }

    /// <summary>
    /// Processes the LastConcentrationResult from a character after concentration ends.
    /// Handles magazine reload completion and ammo container reload.
    /// </summary>
    /// <returns>A message describing what happened, or null if nothing to process.</returns>
    private async Task<string?> ProcessConcentrationResultAsync(CharacterEdit character)
    {
        var result = character.LastConcentrationResult;
        if (result == null) return null;

        try
        {
            switch (result.ActionType)
            {
                case "MagazineReload":
                    if (result.Success)
                    {
                        await ExecuteMagazineReloadAsync(result.Payload);
                        return result.Message;
                    }
                    break;

                case "AmmoContainerReload":
                    if (result.Success)
                    {
                        await ExecuteAmmoContainerReloadAsync(result.Payload);
                        return result.Message;
                    }
                    break;

                case "AmmoContainerUnload":
                    if (result.Success)
                    {
                        await ExecuteAmmoContainerUnloadAsync(result.Payload);
                        return result.Message;
                    }
                    break;

                case "MedicalHealing":
                    if (result.Success)
                    {
                        await ExecuteMedicalHealingAsync(result.Payload);
                        return result.Message;
                    }
                    break;
            }

            // Clear the result after processing
            character.LastConcentrationResult = null;
            return result.Success ? result.Message : null;
        }
        catch (Exception ex)
        {
            return $"Error processing concentration result: {ex.Message}";
        }
    }

    /// <summary>
    /// Executes the magazine reload by updating the weapon's ammo state.
    /// </summary>
    private async Task ExecuteMagazineReloadAsync(string? payloadJson)
    {
        if (string.IsNullOrEmpty(payloadJson)) return;

        var payload = MagazineReloadPayload.FromJson(payloadJson);
        if (payload == null) return;

        // Update weapon ammo state
        var weapon = await _itemDal.GetItemAsync(payload.WeaponItemId);
        if (weapon != null)
        {
            var weaponState = WeaponAmmoState.FromJson(weapon.CustomProperties);
            weaponState.LoadedAmmo += payload.RoundsToLoad;
            weapon.CustomProperties = WeaponAmmoState.MergeIntoCustomProperties(
                weapon.CustomProperties, weaponState);
            await _itemDal.UpdateItemAsync(weapon);
        }

        // Reduce magazine ammo
        var magazine = await _itemDal.GetItemAsync(payload.MagazineItemId);
        if (magazine != null)
        {
            var magazineState = AmmoContainerState.FromJson(magazine.CustomProperties);
            magazineState.LoadedAmmo = Math.Max(0, magazineState.LoadedAmmo - payload.RoundsToLoad);
            magazine.CustomProperties = AmmoContainerState.MergeIntoCustomProperties(
                magazine.CustomProperties, magazineState);
            await _itemDal.UpdateItemAsync(magazine);
        }
    }

    /// <summary>
    /// Executes the ammo container reload by updating the container's ammo state and reducing the loose ammo stack.
    /// </summary>
    private async Task ExecuteAmmoContainerReloadAsync(string? payloadJson)
    {
        if (string.IsNullOrEmpty(payloadJson)) return;

        var payload = AmmoContainerReloadPayload.FromJson(payloadJson);
        if (payload == null) return;

        // Update container ammo state
        var container = await _itemDal.GetItemAsync(payload.ContainerId);
        if (container != null)
        {
            var containerState = AmmoContainerState.FromJson(container.CustomProperties);

            // If max capacity not set, try to get from template properties
            if (containerState.MaxCapacity == 0 && container.Template != null)
            {
                var containerProps = AmmoContainerProperties.FromJson(container.Template.CustomProperties);
                if (containerProps != null)
                {
                    containerState.MaxCapacity = containerProps.Capacity;
                }
            }

            containerState.LoadedAmmo += payload.RoundsToLoad;
            if (payload.AmmoType != null)
                containerState.AmmoType = payload.AmmoType;
            container.CustomProperties = AmmoContainerState.MergeIntoCustomProperties(
                container.CustomProperties, containerState);
            await _itemDal.UpdateItemAsync(container);
        }

        // Reduce source ammo stack
        var source = await _itemDal.GetItemAsync(payload.SourceItemId);
        if (source != null)
        {
            source.StackSize -= payload.RoundsToLoad;
            if (source.StackSize <= 0)
                await _itemDal.DeleteItemAsync(payload.SourceItemId);
            else
                await _itemDal.UpdateItemAsync(source);
        }
    }

    /// <summary>
    /// Executes the ammo container unload by reducing the container's ammo
    /// and returning rounds to the character's inventory as loose ammo.
    /// </summary>
    private async Task ExecuteAmmoContainerUnloadAsync(string? payloadJson)
    {
        if (string.IsNullOrEmpty(payloadJson)) return;

        var payload = AmmoContainerUnloadPayload.FromJson(payloadJson);
        if (payload == null) return;

        // Update container ammo state
        var container = await _itemDal.GetItemAsync(payload.ContainerId);
        if (container == null) return;

        var containerState = AmmoContainerState.FromJson(container.CustomProperties);
        var roundsToUnload = Math.Min(payload.RoundsToUnload, containerState.LoadedAmmo);
        if (roundsToUnload <= 0) return;

        // Reduce container ammo
        containerState.LoadedAmmo -= roundsToUnload;
        container.CustomProperties = AmmoContainerState.MergeIntoCustomProperties(
            container.CustomProperties, containerState);
        await _itemDal.UpdateItemAsync(container);

        // Get all character's items to find a matching loose ammo stack
        var characterItems = await _itemDal.GetCharacterItemsAsync(payload.CharacterId);

        // Find existing loose ammo of the same type
        CharacterItem? existingLooseAmmo = null;
        foreach (var item in characterItems)
        {
            // Must not be in a container
            if (item.ContainerItemId != null) continue;

            // Get template to check if it's ammunition of the right type
            var template = await _templateDal.GetTemplateAsync(item.ItemTemplateId);
            if (template?.ItemType != ItemType.Ammunition) continue;

            var ammoProps = AmmunitionProperties.FromJson(template.CustomProperties);
            if (ammoProps == null) continue;
            if (ammoProps.IsContainer) continue; // Not loose ammo

            // Check if same ammo type
            if (string.Equals(ammoProps.AmmoType, payload.AmmoType, StringComparison.OrdinalIgnoreCase))
            {
                existingLooseAmmo = item;
                break;
            }
        }

        if (existingLooseAmmo != null)
        {
            // Add to existing stack
            existingLooseAmmo.StackSize += roundsToUnload;
            await _itemDal.UpdateItemAsync(existingLooseAmmo);
        }
        else
        {
            // Need to create a new loose ammo item - find the template
            var ammoTemplates = await _templateDal.GetTemplatesByTypeAsync(ItemType.Ammunition);
            ItemTemplate? matchingTemplate = null;

            foreach (var template in ammoTemplates)
            {
                var ammoProps = AmmunitionProperties.FromJson(template.CustomProperties);
                if (ammoProps == null) continue;
                if (ammoProps.IsContainer) continue; // Not loose ammo

                if (string.Equals(ammoProps.AmmoType, payload.AmmoType, StringComparison.OrdinalIgnoreCase))
                {
                    matchingTemplate = template;
                    break;
                }
            }

            if (matchingTemplate != null)
            {
                var newAmmo = new CharacterItem
                {
                    Id = Guid.NewGuid(),
                    ItemTemplateId = matchingTemplate.Id,
                    OwnerCharacterId = payload.CharacterId,
                    StackSize = roundsToUnload,
                    CreatedAt = DateTime.UtcNow
                };
                await _itemDal.AddItemAsync(newAmmo);
            }
            // If no matching template found, the rounds are lost - this shouldn't happen
            // if the ammo type is properly set up
        }
    }

    /// <summary>
    /// Executes the medical healing by applying FAT/VIT healing to the target character.
    /// Healing amount is determined by the SV stored in the payload.
    /// </summary>
    private async Task ExecuteMedicalHealingAsync(string? payloadJson)
    {
        if (string.IsNullOrEmpty(payloadJson)) return;

        var payload = MedicalHealingPayload.FromJson(payloadJson);
        if (payload == null) return;

        // Get healing amount from result tables based on stored SV
        var healingResult = GameMechanics.Actions.ResultTables.GetResult(
            payload.SuccessValue,
            ResultTableType.Healing);

        if (!healingResult.IsSuccess || healingResult.EffectValue <= 0)
        {
            // Failed check - no healing to apply
            return;
        }

        int healingAmount = healingResult.EffectValue;

        // Load the target character
        var targetCharacter = await _characterPortal.FetchAsync(payload.TargetCharacterId);
        if (targetCharacter == null) return;

        // Apply healing to both FAT and VIT pools (player chooses how to distribute later,
        // but for now we apply evenly - prioritize FAT first since it's typically depleted first)
        int fatHealing = Math.Min(healingAmount,
            targetCharacter.Fatigue.BaseValue - targetCharacter.Fatigue.Value);
        int vitHealing = Math.Min(healingAmount,
            targetCharacter.Vitality.BaseValue - targetCharacter.Vitality.Value);

        // Add to pending healing - will be applied at end of round
        if (fatHealing > 0)
        {
            targetCharacter.Fatigue.PendingHealing += fatHealing;
        }
        if (vitHealing > 0)
        {
            targetCharacter.Vitality.PendingHealing += vitHealing;
        }

        // Save the target character
        await _characterPortal.UpdateAsync(targetCharacter);
    }
}
