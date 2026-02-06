using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Csla;
using GameMechanics.Messaging;
using GameMechanics.Time;

namespace GameMechanics.Batch;

/// <summary>
/// Service for applying batch actions (damage/healing) to multiple characters.
/// Follows TimeAdvancementService pattern: sequential processing, error aggregation,
/// single notification after batch completes.
/// </summary>
public class BatchActionService
{
    private readonly IDataPortal<CharacterEdit> _characterPortal;
    private readonly ITimeEventPublisher _timeEventPublisher;

    public BatchActionService(
        IDataPortal<CharacterEdit> characterPortal,
        ITimeEventPublisher timeEventPublisher)
    {
        _characterPortal = characterPortal;
        _timeEventPublisher = timeEventPublisher;
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
