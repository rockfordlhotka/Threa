using System.Collections.Generic;

namespace GameMechanics.Time;

/// <summary>
/// Result of processing end-of-round for a single character.
/// </summary>
public class CharacterRoundResult
{
    /// <summary>
    /// The character ID processed.
    /// </summary>
    public int CharacterId { get; init; }

    /// <summary>
    /// Character name for display.
    /// </summary>
    public string CharacterName { get; init; } = string.Empty;

    /// <summary>
    /// AP recovered this round.
    /// </summary>
    public int APRecovered { get; set; }

    /// <summary>
    /// FAT recovered this round (if any).
    /// </summary>
    public int FATRecovered { get; set; }

    /// <summary>
    /// FAT damage applied this round (from pending).
    /// </summary>
    public int FATDamageApplied { get; set; }

    /// <summary>
    /// VIT damage applied this round (from pending or overflow).
    /// </summary>
    public int VITDamageApplied { get; set; }

    /// <summary>
    /// FAT damage from wounds this round.
    /// </summary>
    public int WoundDamage { get; set; }

    /// <summary>
    /// FAT damage from effects (DoT) this round.
    /// </summary>
    public int EffectDamage { get; set; }

    /// <summary>
    /// Cooldowns that completed this round.
    /// </summary>
    public List<string> CompletedCooldowns { get; set; } = new();

    /// <summary>
    /// Effects that expired this round.
    /// </summary>
    public List<string> ExpiredEffects { get; set; } = new();

    /// <summary>
    /// Whether the character passed out this round.
    /// </summary>
    public bool PassedOut { get; set; }

    /// <summary>
    /// Whether the character died this round.
    /// </summary>
    public bool Died { get; set; }

    /// <summary>
    /// Any Focus checks required and their results.
    /// </summary>
    public List<string> FocusCheckResults { get; set; } = new();
}

/// <summary>
/// Result of processing a complete round for all participants.
/// </summary>
public class RoundResult
{
    /// <summary>
    /// The round number that was just completed.
    /// </summary>
    public int RoundNumber { get; init; }

    /// <summary>
    /// Per-character results.
    /// </summary>
    public List<CharacterRoundResult> CharacterResults { get; } = new();

    /// <summary>
    /// Whether this round triggered an end-of-minute event.
    /// </summary>
    public bool TriggeredEndOfMinute { get; init; }

    /// <summary>
    /// Whether this round triggered an end-of-turn event.
    /// </summary>
    public bool TriggeredEndOfTurn { get; init; }

    /// <summary>
    /// Whether this round triggered an end-of-hour event.
    /// </summary>
    public bool TriggeredEndOfHour { get; init; }

    /// <summary>
    /// Summary messages for GM display.
    /// </summary>
    public List<string> SummaryMessages { get; } = new();
}
