using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameMechanics.Effects;

namespace GameMechanics.Time;

/// <summary>
/// Represents the current time state for a campaign/session.
/// </summary>
public class TimeState
{
    /// <summary>
    /// Total elapsed rounds since tracking began.
    /// </summary>
    public long TotalRounds { get; set; }

    /// <summary>
    /// Current round within the current minute (0-19).
    /// </summary>
    public int RoundInMinute { get; set; }

    /// <summary>
    /// Current minute within the current turn (0-9).
    /// </summary>
    public int MinuteInTurn { get; set; }

    /// <summary>
    /// Current turn within the current hour (0-5).
    /// </summary>
    public int TurnInHour { get; set; }

    /// <summary>
    /// Current hour within the current day (0-23).
    /// </summary>
    public int HourInDay { get; set; }

    /// <summary>
    /// Current day within the current week (0-6).
    /// </summary>
    public int DayInWeek { get; set; }

    /// <summary>
    /// Total elapsed weeks.
    /// </summary>
    public long TotalWeeks { get; set; }

    /// <summary>
    /// Gets elapsed time in seconds.
    /// </summary>
    public long ElapsedSeconds => TotalRounds * 3;

    /// <summary>
    /// Gets elapsed time as a TimeSpan.
    /// </summary>
    public TimeSpan ElapsedTime => TimeSpan.FromSeconds(ElapsedSeconds);

    /// <summary>
    /// Gets a display string for the current time.
    /// </summary>
    public string DisplayTime => $"Week {TotalWeeks + 1}, Day {DayInWeek + 1}, {HourInDay:D2}:{MinuteInTurn * 10 + RoundInMinute / 2:D2}";
}

/// <summary>
/// Result from advancing time.
/// </summary>
public class TimeAdvanceResult
{
    /// <summary>
    /// The type of time event that was processed.
    /// </summary>
    public TimeEventType EventType { get; init; }

    /// <summary>
    /// Number of units advanced.
    /// </summary>
    public int UnitsAdvanced { get; init; }

    /// <summary>
    /// Per-character results if rounds were processed.
    /// </summary>
    public List<CharacterRoundResult> CharacterResults { get; } = new();

    /// <summary>
    /// Time boundaries that were crossed.
    /// </summary>
    public List<TimeEventType> BoundariesCrossed { get; } = new();

    /// <summary>
    /// Summary messages for display.
    /// </summary>
    public List<string> Messages { get; } = new();

    /// <summary>
    /// The current time state after advancement.
    /// </summary>
    public TimeState? CurrentTime { get; set; }
}

/// <summary>
/// Interface for receiving time events.
/// Implement this to handle GM-triggered time advancement.
/// </summary>
public interface ITimeEventHandler
{
    /// <summary>
    /// Called when time advances.
    /// </summary>
    Task OnTimeAdvancedAsync(TimeAdvanceResult result);
}

/// <summary>
/// Top-level time manager that handles both combat and non-combat time.
/// This is the main entry point for GM-triggered time events.
/// </summary>
public class TimeManager
{
    private readonly RoundManager _roundManager = new();
    private readonly TimeState _timeState = new();
    private readonly List<ITimeEventHandler> _handlers = new();
    private readonly Dictionary<int, CharacterEdit> _trackedCharacters = new();
    private EffectManager? _effectManager;

    /// <summary>
    /// Event raised when time advances.
    /// </summary>
    public event EventHandler<TimeAdvanceResult>? TimeAdvanced;

    /// <summary>
    /// Gets the round manager for combat-specific operations.
    /// </summary>
    public RoundManager RoundManager => _roundManager;

    /// <summary>
    /// Gets the current time state.
    /// </summary>
    public TimeState CurrentTime => _timeState;

    /// <summary>
    /// Gets whether we are in active combat mode.
    /// </summary>
    public bool InCombat => _roundManager.InCombat;

    /// <summary>
    /// Sets the effect manager for processing effect durations.
    /// </summary>
    public void SetEffectManager(EffectManager effectManager)
    {
        _effectManager = effectManager;
        _roundManager.SetEffectManager(effectManager);
    }

    /// <summary>
    /// Registers a handler for time events.
    /// </summary>
    public void RegisterHandler(ITimeEventHandler handler)
    {
        _handlers.Add(handler);
    }

    /// <summary>
    /// Unregisters a time event handler.
    /// </summary>
    public void UnregisterHandler(ITimeEventHandler handler)
    {
        _handlers.Remove(handler);
    }

    /// <summary>
    /// Registers a character for time tracking.
    /// </summary>
    public void TrackCharacter(CharacterEdit character)
    {
        _trackedCharacters[character.Id] = character;
    }

    /// <summary>
    /// Unregisters a character from time tracking.
    /// </summary>
    public void UntrackCharacter(int characterId)
    {
        _trackedCharacters.Remove(characterId);
    }

    /// <summary>
    /// Enters combat mode with the currently tracked characters.
    /// </summary>
    public void EnterCombat()
    {
        foreach (var character in _trackedCharacters.Values)
        {
            _roundManager.RegisterCharacter(character);
        }
        _roundManager.StartCombat();
    }

    /// <summary>
    /// Exits combat mode.
    /// </summary>
    public void ExitCombat()
    {
        _roundManager.EndCombat();
    }

    /// <summary>
    /// Advances time by the specified event type.
    /// This is the main GM entry point for time advancement.
    /// </summary>
    public async Task<TimeAdvanceResult> AdvanceTimeAsync(TimeEventType eventType, int count = 1)
    {
        var result = new TimeAdvanceResult
        {
            EventType = eventType,
            UnitsAdvanced = count
        };

        for (int i = 0; i < count; i++)
        {
            switch (eventType)
            {
                case TimeEventType.EndOfRound:
                    await ProcessEndOfRoundAsync(result);
                    break;
                case TimeEventType.EndOfMinute:
                    await ProcessEndOfMinuteAsync(result);
                    break;
                case TimeEventType.EndOfTurn:
                    await ProcessEndOfTurnAsync(result);
                    break;
                case TimeEventType.EndOfHour:
                    await ProcessEndOfHourAsync(result);
                    break;
                case TimeEventType.EndOfDay:
                    await ProcessEndOfDayAsync(result);
                    break;
                case TimeEventType.EndOfWeek:
                    await ProcessEndOfWeekAsync(result);
                    break;
            }
        }

        result.CurrentTime = _timeState;
        result.Messages.Add($"Time is now: {_timeState.DisplayTime}");

        // Notify handlers
        foreach (var handler in _handlers)
        {
            await handler.OnTimeAdvancedAsync(result);
        }

        TimeAdvanced?.Invoke(this, result);
        return result;
    }

    /// <summary>
    /// Processes a single round (3 seconds).
    /// </summary>
    private async Task ProcessEndOfRoundAsync(TimeAdvanceResult result)
    {
        _timeState.TotalRounds++;
        _timeState.RoundInMinute++;

        // Process characters
        foreach (var character in _trackedCharacters.Values)
        {
            var charResult = new CharacterRoundResult
            {
                CharacterId = character.Id,
                CharacterName = character.Name
            };

            // Process effects if available
            if (_effectManager != null)
            {
                var effectResult = await _effectManager.ProcessEndOfRoundAsync(character.Id);
                charResult.EffectDamage = effectResult.FatigueDamage + effectResult.VitalityDamage;
                charResult.ExpiredEffects = effectResult.ExpiredCharacterEffects
                    .Select(e => e.Definition?.Name ?? "Unknown")
                    .ToList();

                character.Fatigue.PendingDamage += effectResult.FatigueDamage;
                character.Vitality.PendingDamage += effectResult.VitalityDamage;
            }

            // Capture before state
            var apBefore = character.ActionPoints.Available;
            var fatBefore = character.Fatigue.Value;
            var wasPassedOut = character.IsPassedOut;

            // Process end of round
            character.EndOfRound();

            // Calculate changes
            charResult.APRecovered = Math.Max(0, character.ActionPoints.Available - apBefore);
            charResult.FATRecovered = Math.Max(0, character.Fatigue.Value - fatBefore);
            charResult.FATDamageApplied = Math.Max(0, fatBefore - character.Fatigue.Value);
            charResult.PassedOut = character.IsPassedOut && !wasPassedOut;
            charResult.Died = character.Vitality.Value <= 0;

            result.CharacterResults.Add(charResult);
        }

        // Check for minute boundary
        if (_timeState.RoundInMinute >= 20)
        {
            _timeState.RoundInMinute = 0;
            result.BoundariesCrossed.Add(TimeEventType.EndOfMinute);
            await ProcessMinuteBoundaryAsync(result);
        }
    }

    /// <summary>
    /// Processes end of minute (skipping 20 rounds with summarized effects).
    /// </summary>
    private async Task ProcessEndOfMinuteAsync(TimeAdvanceResult result)
    {
        // Advance 20 rounds worth of time
        for (int i = 0; i < 20; i++)
        {
            await ProcessEndOfRoundAsync(result);
        }
        
        result.Messages.Add("Minute complete (20 rounds processed).");
    }

    /// <summary>
    /// Called when a minute boundary is crossed during round processing.
    /// </summary>
    private async Task ProcessMinuteBoundaryAsync(TimeAdvanceResult result)
    {
        _timeState.MinuteInTurn++;
        result.Messages.Add("End of minute.");

        // Process minute-specific effects (poison ticks, environmental hazards)
        if (_effectManager != null)
        {
            foreach (var character in _trackedCharacters.Values)
            {
                await _effectManager.ProcessTimeExpirationAsync(character.Id);
            }
        }

        // Check for turn boundary
        if (_timeState.MinuteInTurn >= 10)
        {
            _timeState.MinuteInTurn = 0;
            result.BoundariesCrossed.Add(TimeEventType.EndOfTurn);
            await ProcessTurnBoundaryAsync(result);
        }
    }

    /// <summary>
    /// Processes end of turn (10 minutes / 200 rounds).
    /// </summary>
    private async Task ProcessEndOfTurnAsync(TimeAdvanceResult result)
    {
        // Advance 10 minutes
        for (int i = 0; i < 10; i++)
        {
            await ProcessEndOfMinuteAsync(result);
        }

        result.Messages.Add("Turn complete (10 minutes processed).");
    }

    /// <summary>
    /// Called when a turn boundary is crossed.
    /// </summary>
    private async Task ProcessTurnBoundaryAsync(TimeAdvanceResult result)
    {
        _timeState.TurnInHour++;
        result.Messages.Add("End of turn (10 minutes).");

        // Turn-specific processing: torch consumption, buff expiration, exploration events
        // TODO: Add turn-specific effect processing

        // Check for hour boundary
        if (_timeState.TurnInHour >= 6)
        {
            _timeState.TurnInHour = 0;
            result.BoundariesCrossed.Add(TimeEventType.EndOfHour);
            await ProcessHourBoundaryAsync(result);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Processes end of hour.
    /// </summary>
    private async Task ProcessEndOfHourAsync(TimeAdvanceResult result)
    {
        // Advance 6 turns
        for (int i = 0; i < 6; i++)
        {
            await ProcessEndOfTurnAsync(result);
        }

        result.Messages.Add("Hour complete.");
    }

    /// <summary>
    /// Called when an hour boundary is crossed.
    /// </summary>
    private async Task ProcessHourBoundaryAsync(TimeAdvanceResult result)
    {
        _timeState.HourInDay++;
        result.Messages.Add("End of hour.");

        // Hour-specific processing: VIT recovery
        foreach (var character in _trackedCharacters.Values)
        {
            if (character.Vitality.Value > 0 && character.Vitality.Value < character.Vitality.BaseValue)
            {
                character.Vitality.PendingHealing += 1;
                result.Messages.Add($"  {character.Name}: +1 VIT recovery pending");
            }
        }

        // Check for day boundary
        if (_timeState.HourInDay >= 24)
        {
            _timeState.HourInDay = 0;
            result.BoundariesCrossed.Add(TimeEventType.EndOfDay);
            await ProcessDayBoundaryAsync(result);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Processes end of day.
    /// </summary>
    private async Task ProcessEndOfDayAsync(TimeAdvanceResult result)
    {
        // Advance 24 hours
        for (int i = 0; i < 24; i++)
        {
            await ProcessEndOfHourAsync(result);
        }

        result.Messages.Add("Day complete.");
    }

    /// <summary>
    /// Called when a day boundary is crossed.
    /// </summary>
    private async Task ProcessDayBoundaryAsync(TimeAdvanceResult result)
    {
        _timeState.DayInWeek++;
        result.Messages.Add("End of day.");

        // Day-specific processing: full rest, daily resets, condition progression
        // TODO: Add day-specific processing

        // Check for week boundary
        if (_timeState.DayInWeek >= 7)
        {
            _timeState.DayInWeek = 0;
            _timeState.TotalWeeks++;
            result.BoundariesCrossed.Add(TimeEventType.EndOfWeek);
            await ProcessWeekBoundaryAsync(result);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Processes end of week.
    /// </summary>
    private async Task ProcessEndOfWeekAsync(TimeAdvanceResult result)
    {
        // Advance 7 days
        for (int i = 0; i < 7; i++)
        {
            await ProcessEndOfDayAsync(result);
        }

        result.Messages.Add("Week complete.");
    }

    /// <summary>
    /// Called when a week boundary is crossed.
    /// </summary>
    private async Task ProcessWeekBoundaryAsync(TimeAdvanceResult result)
    {
        result.Messages.Add($"End of week {_timeState.TotalWeeks}.");

        // Week-specific processing: long-term healing, training, crafting
        // TODO: Add week-specific processing

        await Task.CompletedTask;
    }

    /// <summary>
    /// Skips time without detailed processing (narrative time skip).
    /// Only processes the final time unit's effects.
    /// </summary>
    public async Task<TimeAdvanceResult> SkipTimeAsync(TimeEventType unit, int count)
    {
        var result = new TimeAdvanceResult
        {
            EventType = unit,
            UnitsAdvanced = count
        };

        // Calculate total rounds to skip
        long roundsToSkip = unit switch
        {
            TimeEventType.EndOfRound => count,
            TimeEventType.EndOfMinute => count * 20L,
            TimeEventType.EndOfTurn => count * 200L,
            TimeEventType.EndOfHour => count * 1200L,
            TimeEventType.EndOfDay => count * 28800L,
            TimeEventType.EndOfWeek => count * 201600L,
            _ => 0
        };

        // Update time state directly
        _timeState.TotalRounds += roundsToSkip;
        
        // Recalculate all time boundaries
        var totalMinutes = _timeState.TotalRounds / 20;
        var totalTurns = totalMinutes / 10;
        var totalHours = totalTurns / 6;
        var totalDays = totalHours / 24;
        
        _timeState.RoundInMinute = (int)(_timeState.TotalRounds % 20);
        _timeState.MinuteInTurn = (int)(totalMinutes % 10);
        _timeState.TurnInHour = (int)(totalTurns % 6);
        _timeState.HourInDay = (int)(totalHours % 24);
        _timeState.DayInWeek = (int)(totalDays % 7);
        _timeState.TotalWeeks = totalDays / 7;

        // Process one final boundary at the target level
        switch (unit)
        {
            case TimeEventType.EndOfHour:
                foreach (var character in _trackedCharacters.Values)
                {
                    if (character.Vitality.Value > 0)
                    {
                        // Recover VIT equal to hours skipped
                        character.Vitality.PendingHealing += count;
                    }
                }
                break;
            // Add other skip processing as needed
        }

        result.CurrentTime = _timeState;
        result.Messages.Add($"Skipped {count} {unit}(s). Time is now: {_timeState.DisplayTime}");

        TimeAdvanced?.Invoke(this, result);
        return result;
    }
}
