using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Csla;
using GameMechanics.Messaging;
using GameMechanics.Time;
using Threa.Dal;

namespace GameMechanics.Batch;

/// <summary>
/// Service for applying batch actions (damage/healing, visibility toggle, dismiss/archive,
/// effect add/remove) to multiple characters. Follows TimeAdvancementService pattern:
/// sequential processing, error aggregation, single notification after batch completes.
/// </summary>
public class BatchActionService
{
    private readonly IDataPortal<CharacterEdit> _characterPortal;
    private readonly ITimeEventPublisher _timeEventPublisher;
    private readonly ITableDal _tableDal;
    private readonly IChildDataPortal<EffectRecord> _effectPortal;

    public BatchActionService(
        IDataPortal<CharacterEdit> characterPortal,
        ITimeEventPublisher timeEventPublisher,
        ITableDal tableDal,
        IChildDataPortal<EffectRecord> effectPortal)
    {
        _characterPortal = characterPortal;
        _timeEventPublisher = timeEventPublisher;
        _tableDal = tableDal;
        _effectPortal = effectPortal;
    }

    /// <summary>
    /// Applies damage to multiple characters' pending damage pools.
    /// </summary>
    /// <param name="request">The batch damage request.</param>
    /// <returns>Result with success/failure details.</returns>
    public async Task<BatchActionResult> ApplyDamageAsync(BatchActionRequest request)
    {
        return await ProcessBatchAsync(request with { ActionType = BatchActionType.Damage });
    }

    /// <summary>
    /// Applies healing to multiple characters' pending healing pools.
    /// </summary>
    /// <param name="request">The batch healing request.</param>
    /// <returns>Result with success/failure details.</returns>
    public async Task<BatchActionResult> ApplyHealingAsync(BatchActionRequest request)
    {
        return await ProcessBatchAsync(request with { ActionType = BatchActionType.Healing });
    }

    /// <summary>
    /// Toggles visibility on multiple NPCs. Non-NPC characters are silently skipped.
    /// </summary>
    /// <param name="request">The batch visibility request (VisibilityTarget = true to reveal, false to hide).</param>
    /// <returns>Result with success/failure details.</returns>
    public async Task<BatchActionResult> ToggleVisibilityAsync(BatchActionRequest request)
    {
        var result = new BatchActionResult
        {
            ActionType = BatchActionType.Visibility,
            VisibilityAction = request.VisibilityTarget ?? true
        };

        foreach (var characterId in request.CharacterIds)
        {
            try
            {
                var character = await _characterPortal.FetchAsync(characterId);

                if (!character.IsNpc)
                    continue;

                character.VisibleToPlayers = request.VisibilityTarget ?? true;
                await _characterPortal.UpdateAsync(character);
                result.SuccessIds.Add(characterId);
                result.SuccessNames.Add(character.Name);
            }
            catch (Exception ex)
            {
                result.FailedIds.Add(characterId);
                result.Errors.Add($"Character {characterId}: {ex.Message}");
            }
        }

        if (result.SuccessIds.Count > 0)
        {
            await _timeEventPublisher.PublishCharactersUpdatedAsync(
                new CharactersUpdatedMessage
                {
                    TableId = request.TableId,
                    CharacterIds = result.SuccessIds,
                    EventType = TimeEventType.EndOfRound,
                    SourceId = "BatchActionService"
                });
        }

        return result;
    }

    /// <summary>
    /// Dismisses (archives) multiple NPCs and removes them from the table.
    /// Non-NPC characters are silently skipped.
    /// </summary>
    /// <param name="request">The batch dismiss request.</param>
    /// <returns>Result with success/failure details.</returns>
    public async Task<BatchActionResult> DismissAsync(BatchActionRequest request)
    {
        var result = new BatchActionResult
        {
            ActionType = BatchActionType.Dismiss
        };

        foreach (var characterId in request.CharacterIds)
        {
            try
            {
                var character = await _characterPortal.FetchAsync(characterId);

                if (!character.IsNpc)
                    continue;

                // Step 1: Archive the character
                character.IsArchived = true;
                await _characterPortal.UpdateAsync(character);

                // Step 2: Remove from table
                await _tableDal.RemoveCharacterFromTableAsync(request.TableId, characterId);

                result.SuccessIds.Add(characterId);
                result.SuccessNames.Add(character.Name);
            }
            catch (Exception ex)
            {
                result.FailedIds.Add(characterId);
                result.Errors.Add($"Character {characterId}: {ex.Message}");
            }
        }

        if (result.SuccessIds.Count > 0)
        {
            await _timeEventPublisher.PublishCharactersUpdatedAsync(
                new CharactersUpdatedMessage
                {
                    TableId = request.TableId,
                    CharacterIds = result.SuccessIds,
                    EventType = TimeEventType.EndOfRound,
                    SourceId = "BatchActionService"
                });
        }

        return result;
    }

    /// <summary>
    /// Adds an effect to multiple characters. All characters get the same effect configuration
    /// with a shared game timestamp captured from the first character.
    /// </summary>
    /// <param name="request">The batch effect add request.</param>
    /// <returns>Result with success/failure details.</returns>
    public async Task<BatchActionResult> AddEffectAsync(BatchActionRequest request)
    {
        var result = new BatchActionResult
        {
            ActionType = BatchActionType.EffectAdd,
            EffectName = request.EffectName
        };

        // Capture game time once before loop (CONTEXT decision: shared timestamp)
        long? sharedGameTime = null;

        foreach (var characterId in request.CharacterIds)
        {
            try
            {
                var character = await _characterPortal.FetchAsync(characterId);

                // Capture game time from first character
                sharedGameTime ??= character.CurrentGameTimeSeconds;

                var effect = await _effectPortal.CreateChildAsync(
                    request.EffectType!.Value,
                    request.EffectName!,
                    (string?)null,  // no location for batch effects
                    request.DurationRounds,
                    request.BehaviorStateJson
                );

                // Apply shared timestamp
                if (sharedGameTime > 0)
                {
                    effect.CreatedAtEpochSeconds = sharedGameTime.Value;
                    if (request.DurationSeconds.HasValue)
                    {
                        effect.ExpiresAtEpochSeconds = sharedGameTime.Value + request.DurationSeconds.Value;
                    }
                }

                effect.Description = request.EffectDescription;
                effect.Source = "GM (Batch)";

                // Defer to EffectList.AddEffect stacking logic
                character.Effects.AddEffect(effect);

                await _characterPortal.UpdateAsync(character);
                result.SuccessIds.Add(characterId);
                result.SuccessNames.Add(character.Name);
            }
            catch (Exception ex)
            {
                result.FailedIds.Add(characterId);
                result.Errors.Add($"Character {characterId}: {ex.Message}");
            }
        }

        // Single notification after batch completes
        if (result.SuccessIds.Count > 0)
        {
            await _timeEventPublisher.PublishCharactersUpdatedAsync(
                new CharactersUpdatedMessage
                {
                    TableId = request.TableId,
                    CharacterIds = result.SuccessIds,
                    EventType = TimeEventType.EndOfRound,
                    SourceId = "BatchActionService"
                });
        }

        return result;
    }

    private async Task<BatchActionResult> ProcessBatchAsync(BatchActionRequest request)
    {
        var result = new BatchActionResult
        {
            ActionType = request.ActionType,
            Pool = request.Pool,
            Amount = request.Amount
        };

        // Sequential processing - CSLA business objects are NOT thread-safe
        foreach (var characterId in request.CharacterIds)
        {
            try
            {
                var character = await _characterPortal.FetchAsync(characterId);

                if (request.ActionType == BatchActionType.Damage)
                {
                    if (request.Pool == "FAT")
                        character.Fatigue.PendingDamage += request.Amount;
                    else
                        character.Vitality.PendingDamage += request.Amount;
                }
                else // Healing
                {
                    if (request.Pool == "FAT")
                        character.Fatigue.PendingHealing += request.Amount;
                    else
                        character.Vitality.PendingHealing += request.Amount;
                }

                await _characterPortal.UpdateAsync(character);
                result.SuccessIds.Add(characterId);
                result.SuccessNames.Add(character.Name);
            }
            catch (Exception ex)
            {
                result.FailedIds.Add(characterId);
                result.Errors.Add($"Character {characterId}: {ex.Message}");
            }
        }

        // Single notification after all updates complete (prevents N refresh cycles)
        if (result.SuccessIds.Count > 0)
        {
            await _timeEventPublisher.PublishCharactersUpdatedAsync(
                new CharactersUpdatedMessage
                {
                    TableId = request.TableId,
                    CharacterIds = result.SuccessIds,
                    EventType = TimeEventType.EndOfRound,
                    SourceId = "BatchActionService"
                });
        }

        return result;
    }
}
