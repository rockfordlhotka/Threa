namespace GameMechanics.Time;

/// <summary>
/// Types of time events that can be triggered by the GM.
/// Each event type triggers specific game mechanics processing.
/// </summary>
public enum TimeEventType
{
    /// <summary>
    /// End of a 3-second combat round.
    /// Triggers: AP recovery, pending damage/healing, FAT recovery, 
    /// wound damage (every 2 rounds), effect durations, cooldowns.
    /// </summary>
    EndOfRound,

    /// <summary>
    /// End of a minute (20 rounds).
    /// Triggers: Short-duration spell expirations, poison/toxin effects,
    /// environmental hazard updates.
    /// </summary>
    EndOfMinute,

    /// <summary>
    /// End of a turn (10 minutes / 200 rounds).
    /// Triggers: Torch/light consumption, wandering monster checks,
    /// short-term buff expirations, exploration timekeeping.
    /// </summary>
    EndOfTurn,

    /// <summary>
    /// End of an hour (60 minutes / 1200 rounds).
    /// Triggers: VIT recovery (+1 if VIT > 0), extended rest benefits,
    /// travel distance calculations, long-duration spell checks.
    /// </summary>
    EndOfHour,

    /// <summary>
    /// End of a day (24 hours).
    /// Triggers: Full rest recovery, daily spell/ability resets,
    /// condition/disease progression, narrative time advancement.
    /// </summary>
    EndOfDay,

    /// <summary>
    /// End of a week (7 days).
    /// Triggers: Long-term healing, training progression,
    /// crafting project advancement, major world event processing.
    /// </summary>
    EndOfWeek
}
