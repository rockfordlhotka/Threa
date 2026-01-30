using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Csla;
using GameMechanics.Messaging;
using Threa.Dal;

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

    public TimeAdvancementService(
        ITableDal tableDal,
        IDataPortal<CharacterEdit> characterPortal,
        IChildDataPortal<EffectRecord> effectPortal,
        ITimeEventPublisher timeEventPublisher)
    {
        _tableDal = tableDal;
        _characterPortal = characterPortal;
        _effectPortal = effectPortal;
        _timeEventPublisher = timeEventPublisher;
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
}
