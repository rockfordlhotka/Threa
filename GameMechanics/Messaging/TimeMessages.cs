using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using GameMechanics.Time;

namespace GameMechanics.Messaging;

/// <summary>
/// Base class for all time-related messages.
/// </summary>
public abstract class TimeMessageBase
{
    /// <summary>
    /// Unique identifier for this message.
    /// </summary>
    public Guid MessageId { get; init; } = Guid.NewGuid();

    /// <summary>
    /// The campaign this message applies to.
    /// </summary>
    public string CampaignId { get; init; } = string.Empty;

    /// <summary>
    /// When the message was created.
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// The GM or system that initiated this message.
    /// </summary>
    public string SourceId { get; init; } = string.Empty;
}

/// <summary>
/// Message to advance time by a specific time event type.
/// </summary>
public class TimeEventMessage : TimeMessageBase
{
    /// <summary>
    /// The type of time event to trigger.
    /// </summary>
    public TimeEventType EventType { get; init; }

    /// <summary>
    /// Number of time units to advance (e.g., 3 rounds).
    /// </summary>
    public int Count { get; init; } = 1;

    /// <summary>
    /// Optional reason/note for the time advancement.
    /// </summary>
    public string? Reason { get; init; }
}

/// <summary>
/// Message to skip time without detailed processing (narrative time skip).
/// </summary>
public class TimeSkipMessage : TimeMessageBase
{
    /// <summary>
    /// The time unit to skip.
    /// </summary>
    public TimeEventType SkipUnit { get; init; }

    /// <summary>
    /// Number of units to skip.
    /// </summary>
    public int Count { get; init; } = 1;

    /// <summary>
    /// Description of what happens during the skip.
    /// </summary>
    public string? NarrativeDescription { get; init; }
}

/// <summary>
/// Message to change combat state.
/// </summary>
public class CombatStateMessage : TimeMessageBase
{
    /// <summary>
    /// Whether combat is starting (true) or ending (false).
    /// </summary>
    public bool EnteringCombat { get; init; }

    /// <summary>
    /// Optional encounter identifier.
    /// </summary>
    public string? EncounterId { get; init; }

    /// <summary>
    /// Optional encounter name for display.
    /// </summary>
    public string? EncounterName { get; init; }
}

/// <summary>
/// Message published after time has been processed, containing results.
/// </summary>
public class TimeResultMessage : TimeMessageBase
{
    /// <summary>
    /// The original message that triggered this result.
    /// </summary>
    public Guid OriginalMessageId { get; init; }

    /// <summary>
    /// The event type that was processed.
    /// </summary>
    public TimeEventType EventType { get; init; }

    /// <summary>
    /// Number of units that were advanced.
    /// </summary>
    public int UnitsAdvanced { get; init; }

    /// <summary>
    /// Current round number after advancement.
    /// </summary>
    public long TotalRounds { get; init; }

    /// <summary>
    /// Display-friendly current time string.
    /// </summary>
    public string CurrentTimeDisplay { get; init; } = string.Empty;

    /// <summary>
    /// Summary messages from the time advancement.
    /// </summary>
    public List<string> SummaryMessages { get; init; } = [];

    /// <summary>
    /// Per-character result summaries.
    /// </summary>
    public List<CharacterTimeResult> CharacterResults { get; init; } = [];

    /// <summary>
    /// Time boundaries that were crossed during this advancement.
    /// </summary>
    public List<TimeEventType> BoundariesCrossed { get; init; } = [];
}

/// <summary>
/// Summary of time effects on a single character.
/// </summary>
public class CharacterTimeResult
{
    /// <summary>
    /// Character identifier.
    /// </summary>
    public int CharacterId { get; init; }

    /// <summary>
    /// Character name for display.
    /// </summary>
    public string CharacterName { get; init; } = string.Empty;

    /// <summary>
    /// AP recovered during this time period.
    /// </summary>
    public int APRecovered { get; init; }

    /// <summary>
    /// FAT recovered during this time period.
    /// </summary>
    public int FATRecovered { get; init; }

    /// <summary>
    /// FAT damage applied during this time period.
    /// </summary>
    public int FATDamageApplied { get; init; }

    /// <summary>
    /// VIT damage applied during this time period.
    /// </summary>
    public int VITDamageApplied { get; init; }

    /// <summary>
    /// Cooldowns that completed.
    /// </summary>
    public List<string> CompletedCooldowns { get; init; } = [];

    /// <summary>
    /// Effects that expired.
    /// </summary>
    public List<string> ExpiredEffects { get; init; } = [];

    /// <summary>
    /// Whether the character passed out.
    /// </summary>
    public bool PassedOut { get; init; }

    /// <summary>
    /// Whether the character died.
    /// </summary>
    public bool Died { get; init; }
}
