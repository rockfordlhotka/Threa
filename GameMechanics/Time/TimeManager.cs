using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameMechanics.Effects;

namespace GameMechanics.Time;

/// <summary>
/// Represents the current time state for a campaign/session.
/// All sub-unit values are calculated from TotalRounds.
/// </summary>
public class TimeState
{
    // Time unit constants (in rounds, where 1 round = 3 seconds)
    public const int RoundsPerMinute = 20;      // 60 seconds
    public const int RoundsPerTurn = 200;       // 10 minutes
    public const int RoundsPerHour = 1200;      // 60 minutes
    public const int RoundsPerDay = 28800;      // 24 hours
    public const int RoundsPerWeek = 201600;    // 7 days

    /// <summary>
    /// Total elapsed rounds since tracking began.
    /// This is the single source of truth for all time calculations.
    /// </summary>
    public long TotalRounds { get; set; }

    /// <summary>
    /// Current round within the current minute (0-19).
    /// </summary>
    public int RoundInMinute => (int)(TotalRounds % RoundsPerMinute);

    /// <summary>
    /// Current minute within the current turn (0-9).
    /// </summary>
    public int MinuteInTurn => (int)((TotalRounds % RoundsPerTurn) / RoundsPerMinute);

    /// <summary>
    /// Current turn within the current hour (0-5).
    /// </summary>
    public int TurnInHour => (int)((TotalRounds % RoundsPerHour) / RoundsPerTurn);

    /// <summary>
    /// Current hour within the current day (0-23).
    /// </summary>
    public int HourInDay => (int)((TotalRounds % RoundsPerDay) / RoundsPerHour);

    /// <summary>
    /// Current day within the current week (0-6).
    /// </summary>
    public int DayInWeek => (int)((TotalRounds % RoundsPerWeek) / RoundsPerDay);

    /// <summary>
    /// Total elapsed weeks.
    /// </summary>
    public long TotalWeeks => TotalRounds / RoundsPerWeek;

    /// <summary>
    /// Total elapsed minutes.
    /// </summary>
    public long TotalMinutes => TotalRounds / RoundsPerMinute;

    /// <summary>
    /// Total elapsed turns.
    /// </summary>
    public long TotalTurns => TotalRounds / RoundsPerTurn;

    /// <summary>
    /// Total elapsed hours.
    /// </summary>
    public long TotalHours => TotalRounds / RoundsPerHour;

    /// <summary>
    /// Total elapsed days.
    /// </summary>
    public long TotalDays => TotalRounds / RoundsPerDay;

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

        // Convert the event type to rounds
        int roundsToAdvance = eventType switch
        {
            TimeEventType.EndOfRound => count,
            TimeEventType.EndOfMinute => count * TimeState.RoundsPerMinute,
            TimeEventType.EndOfTurn => count * TimeState.RoundsPerTurn,
            TimeEventType.EndOfHour => count * TimeState.RoundsPerHour,
            TimeEventType.EndOfDay => count * TimeState.RoundsPerDay,
            TimeEventType.EndOfWeek => count * TimeState.RoundsPerWeek,
            _ => 0
        };

        // Process each round individually to handle all boundaries
        for (int i = 0; i < roundsToAdvance; i++)
        {
            await ProcessSingleRoundAsync(result);
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
    /// Processes a single round (3 seconds) and checks for all boundary crossings.
    /// </summary>
    private async Task ProcessSingleRoundAsync(TimeAdvanceResult result)
    {
        // Capture "before" state for boundary detection
        long previousRounds = _timeState.TotalRounds;
        long previousMinutes = _timeState.TotalMinutes;
        long previousTurns = _timeState.TotalTurns;
        long previousHours = _timeState.TotalHours;
        long previousDays = _timeState.TotalDays;
        long previousWeeks = _timeState.TotalWeeks;

        // Advance one round
        _timeState.TotalRounds++;

        // Process character end-of-round effects
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

        // Check for boundary crossings (from smallest to largest time unit)
        if (_timeState.TotalMinutes > previousMinutes)
        {
            await ProcessMinuteBoundaryAsync(result);
            result.BoundariesCrossed.Add(TimeEventType.EndOfMinute);
        }

        if (_timeState.TotalTurns > previousTurns)
        {
            await ProcessTurnBoundaryAsync(result);
            result.BoundariesCrossed.Add(TimeEventType.EndOfTurn);
        }

        if (_timeState.TotalHours > previousHours)
        {
            await ProcessHourBoundaryAsync(result);
            result.BoundariesCrossed.Add(TimeEventType.EndOfHour);
        }

        if (_timeState.TotalDays > previousDays)
        {
            await ProcessDayBoundaryAsync(result);
            result.BoundariesCrossed.Add(TimeEventType.EndOfDay);
        }

        if (_timeState.TotalWeeks > previousWeeks)
        {
            await ProcessWeekBoundaryAsync(result);
            result.BoundariesCrossed.Add(TimeEventType.EndOfWeek);
        }
    }

    /// <summary>
    /// Called when a minute boundary is crossed.
    /// </summary>
    private async Task ProcessMinuteBoundaryAsync(TimeAdvanceResult result)
    {
        result.Messages.Add($"End of minute {_timeState.TotalMinutes}.");

        // Process minute-specific effects (poison ticks, environmental hazards)
        if (_effectManager != null)
        {
            foreach (var character in _trackedCharacters.Values)
            {
                await _effectManager.ProcessTimeExpirationAsync(character.Id);
            }
        }

        // FAT recovery for VIT=4 characters (1 FAT per minute)
        foreach (var character in _trackedCharacters.Values)
        {
            if (character.Vitality.Value == 4 && character.Fatigue.Value < character.Fatigue.BaseValue)
            {
                character.Fatigue.PendingHealing += 1;
                result.Messages.Add($"  {character.Name}: +1 FAT recovery pending (low VIT)");
            }
        }
    }

    /// <summary>
    /// Called when a turn boundary is crossed (every 10 minutes).
    /// </summary>
    private async Task ProcessTurnBoundaryAsync(TimeAdvanceResult result)
    {
        result.Messages.Add($"End of turn {_timeState.TotalTurns} (10 minutes).");

        // Turn-specific processing: torch consumption, buff expiration, exploration events
        // TODO: Add turn-specific effect processing

        // FAT recovery for VIT=3 characters (1 FAT per 30 minutes)
        // Check if this is the 30-minute mark within the hour (turn 3 or 6 within the hour)
        // TurnInHour is 0-5, so positions 2 and 5 are 30 and 60 minutes
        if (_timeState.TurnInHour == 2) // 30 minutes into the hour (0, 1, 2 = 30 min mark)
        {
            foreach (var character in _trackedCharacters.Values)
            {
                if (character.Vitality.Value == 3 && character.Fatigue.Value < character.Fatigue.BaseValue)
                {
                    character.Fatigue.PendingHealing += 1;
                    result.Messages.Add($"  {character.Name}: +1 FAT recovery pending (very low VIT)");
                }
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Called when an hour boundary is crossed.
    /// </summary>
    private async Task ProcessHourBoundaryAsync(TimeAdvanceResult result)
    {
        result.Messages.Add($"End of hour {_timeState.TotalHours}.");

        // FAT recovery for low VIT characters
        foreach (var character in _trackedCharacters.Values)
        {
            // VIT=2: 1 FAT per hour
            if (character.Vitality.Value == 2 && character.Fatigue.Value < character.Fatigue.BaseValue)
            {
                character.Fatigue.PendingHealing += 1;
                result.Messages.Add($"  {character.Name}: +1 FAT recovery pending (critical VIT)");
            }
            // VIT=3: 1 FAT per 30 minutes (also applies at hour boundary = 60 min mark)
            else if (character.Vitality.Value == 3 && character.Fatigue.Value < character.Fatigue.BaseValue)
            {
                character.Fatigue.PendingHealing += 1;
                result.Messages.Add($"  {character.Name}: +1 FAT recovery pending (very low VIT)");
            }
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Called when a day boundary is crossed.
    /// </summary>
    private async Task ProcessDayBoundaryAsync(TimeAdvanceResult result)
    {
        result.Messages.Add($"End of day {_timeState.TotalDays}.");

        // VIT recovery: 1 VIT per day (if alive and not at max)
        foreach (var character in _trackedCharacters.Values)
        {
            if (character.Vitality.Value > 0 && character.Vitality.Value < character.Vitality.BaseValue)
            {
                character.Vitality.PendingHealing += 1;
                result.Messages.Add($"  {character.Name}: +1 VIT recovery pending (daily healing)");
            }
        }

        await Task.CompletedTask;
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
    /// Calculates recovery based on time skipped without processing each round.
    /// </summary>
    public async Task<TimeAdvanceResult> SkipTimeAsync(TimeEventType unit, int count)
    {
        var result = new TimeAdvanceResult
        {
            EventType = unit,
            UnitsAdvanced = count
        };

        // Capture before state
        long previousDays = _timeState.TotalDays;

        // Calculate total rounds to skip
        long roundsToSkip = unit switch
        {
            TimeEventType.EndOfRound => count,
            TimeEventType.EndOfMinute => count * TimeState.RoundsPerMinute,
            TimeEventType.EndOfTurn => count * TimeState.RoundsPerTurn,
            TimeEventType.EndOfHour => count * TimeState.RoundsPerHour,
            TimeEventType.EndOfDay => count * TimeState.RoundsPerDay,
            TimeEventType.EndOfWeek => count * TimeState.RoundsPerWeek,
            _ => 0
        };

        // Update time state - all other values are computed automatically
        _timeState.TotalRounds += roundsToSkip;

        // Calculate how many days were skipped for VIT recovery
        long daysSkipped = _timeState.TotalDays - previousDays;

        // Apply recovery based on time skipped
        foreach (var character in _trackedCharacters.Values)
        {
            // VIT recovery: 1 per day skipped
            if (daysSkipped > 0 && character.Vitality.Value > 0 && character.Vitality.Value < character.Vitality.BaseValue)
            {
                int vitRecovery = (int)Math.Min(daysSkipped, character.Vitality.BaseValue - character.Vitality.Value);
                character.Vitality.PendingHealing += vitRecovery;
                result.Messages.Add($"  {character.Name}: +{vitRecovery} VIT recovery pending ({daysSkipped} days)");
            }

            // FAT recovery depends on VIT level - assume full recovery during narrative skip
            if (character.Fatigue.Value < character.Fatigue.BaseValue && character.Vitality.Value >= 2)
            {
                int fatRecovery = character.Fatigue.BaseValue - character.Fatigue.Value;
                character.Fatigue.PendingHealing += fatRecovery;
                result.Messages.Add($"  {character.Name}: +{fatRecovery} FAT recovery pending (rest)");
            }
        }

        result.CurrentTime = _timeState;
        result.Messages.Add($"Skipped {count} {unit}(s). Time is now: {_timeState.DisplayTime}");

        TimeAdvanced?.Invoke(this, result);
        return await Task.FromResult(result);
    }
}
