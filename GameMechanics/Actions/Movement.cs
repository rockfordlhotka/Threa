using System;

namespace GameMechanics.Actions;

/// <summary>
/// Represents movement distance using the power-of-2 range system.
/// Distance (meters) = RangeValue²
/// </summary>
public static class Movement
{
    /// <summary>
    /// Free positioning range (no action cost).
    /// </summary>
    public const int FreePositioningRange = 2;

    /// <summary>
    /// Standard sprint action range (1 AP + 1 FAT).
    /// </summary>
    public const int SprintActionRange = 3;

    /// <summary>
    /// Full-round sprint range (entire round).
    /// </summary>
    public const int FullRoundSprintRange = 5;

    /// <summary>
    /// Converts a range value to distance in meters.
    /// Distance = Range²
    /// </summary>
    /// <param name="rangeValue">The range value (0-5 typically).</param>
    /// <returns>Distance in meters.</returns>
    public static int RangeToMeters(int rangeValue)
    {
        if (rangeValue <= 0) return 0;
        return rangeValue * rangeValue;
    }

    /// <summary>
    /// Converts distance in meters to the closest range value.
    /// </summary>
    /// <param name="meters">Distance in meters.</param>
    /// <returns>The range value (rounded down).</returns>
    public static int MetersToRange(int meters)
    {
        if (meters <= 0) return 0;
        return (int)Math.Sqrt(meters);
    }

    /// <summary>
    /// Gets the description for a range value.
    /// </summary>
    public static string GetRangeDescription(int rangeValue)
    {
        return rangeValue switch
        {
            0 => "Touch/contact",
            1 => "Adjacent/reach (1m)",
            2 => "Short distance (4m)",
            3 => "Across a room (9m)",
            4 => "Across a large room (16m)",
            5 => "Maximum sprint (25m)",
            _ when rangeValue > 5 => $"Extended range ({RangeToMeters(rangeValue)}m)",
            _ => "Invalid range"
        };
    }
}

/// <summary>
/// The type of movement action being performed.
/// </summary>
public enum MovementType
{
    /// <summary>
    /// Free positioning up to 4m - no action cost.
    /// </summary>
    FreePositioning,

    /// <summary>
    /// Standard sprint action - 1 AP + 1 FAT.
    /// </summary>
    Sprint,

    /// <summary>
    /// Full-round sprint - uses entire round.
    /// </summary>
    FullRoundSprint
}

/// <summary>
/// Result of a movement action, including distance achieved.
/// </summary>
public class MovementResult
{
    /// <summary>
    /// The type of movement attempted.
    /// </summary>
    public MovementType MovementType { get; set; }

    /// <summary>
    /// The base range the movement would achieve (before skill check).
    /// </summary>
    public int BaseRange { get; set; }

    /// <summary>
    /// The actual range achieved after skill check.
    /// </summary>
    public int AchievedRange { get; set; }

    /// <summary>
    /// The distance in meters achieved.
    /// </summary>
    public int DistanceMeters => Movement.RangeToMeters(AchievedRange);

    /// <summary>
    /// Whether the movement fully succeeded.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Whether the character stumbled, fell, or had a mishap.
    /// </summary>
    public bool HasMishap { get; set; }

    /// <summary>
    /// Description of the result.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// The underlying action result (if a skill check was made).
    /// </summary>
    public ActionResult? ActionResult { get; set; }

    /// <summary>
    /// Gets a summary of the movement result.
    /// </summary>
    public string GetSummary()
    {
        if (MovementType == MovementType.FreePositioning)
        {
            return $"Free positioning: moved {DistanceMeters}m";
        }

        var result = IsSuccess ? "SUCCESS" : "FAILURE";
        var mishap = HasMishap ? " (MISHAP!)" : "";
        return $"{MovementType}: {result}{mishap} - moved {DistanceMeters}m ({Description})";
    }
}
