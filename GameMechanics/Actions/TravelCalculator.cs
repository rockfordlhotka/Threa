using System;
using GameMechanics.Time;

namespace GameMechanics.Actions;

/// <summary>
/// Calculates travel time and fatigue costs for sustained movement.
/// Injectable service implementation.
/// </summary>
public class TravelCalculator : ITravelCalculator
{
    private readonly IGameTimeFormatService _timeFormat;

    /// <summary>
    /// Seconds per round constant for backward compatibility.
    /// </summary>
    public const int SecondsPerRound = 3;

    /// <summary>
    /// Rounds per minute constant for backward compatibility.
    /// </summary>
    public const int RoundsPerMinute = 20;

    /// <summary>
    /// Rounds per hour constant for backward compatibility.
    /// </summary>
    public const int RoundsPerHour = 1200;

    /// <summary>
    /// Creates a new TravelCalculator with default time formatting.
    /// </summary>
    public TravelCalculator() : this(new DefaultGameTimeFormatService())
    {
    }

    /// <summary>
    /// Creates a new TravelCalculator with the specified time format service.
    /// </summary>
    /// <param name="timeFormat">The time format service to use.</param>
    public TravelCalculator(IGameTimeFormatService timeFormat)
    {
        _timeFormat = timeFormat ?? throw new ArgumentNullException(nameof(timeFormat));
    }

    /// <inheritdoc />
    public TravelRate GetTravelRate(TravelType travelType)
    {
        return travelType switch
        {
            TravelType.Walking => new TravelRate
            {
                Type = TravelType.Walking,
                MetersPerRound = 4,
                FatigueCostPerMeter = 1.0 / 2000.0, // 1 FAT per 2km
                Description = "Sustainable walking pace"
            },
            TravelType.EnduranceRunning => new TravelRate
            {
                Type = TravelType.EnduranceRunning,
                MetersPerRound = 10,
                FatigueCostPerMeter = 1.0 / 1000.0, // 1 FAT per km
                Description = "Sustainable jogging pace"
            },
            TravelType.BurstRunning => new TravelRate
            {
                Type = TravelType.BurstRunning,
                MetersPerRound = 12,
                FatigueCostPerMeter = 1.0 / 6.0, // 1 FAT per 6m
                Description = "Short-term speed boost"
            },
            TravelType.FastSprinting => new TravelRate
            {
                Type = TravelType.FastSprinting,
                MetersPerRound = 16,
                FatigueCostPerMeter = 1.0 / 5.0, // 1 FAT per 5m
                Description = "Maximum effort sprinting"
            },
            _ => throw new ArgumentOutOfRangeException(nameof(travelType))
        };
    }

    /// <inheritdoc />
    public TravelResult CalculateTravel(int distanceMeters, TravelType travelType)
    {
        if (distanceMeters <= 0)
        {
            return new TravelResult
            {
                DistanceMeters = 0,
                TravelType = travelType,
                RoundsRequired = 0,
                FatigueCost = 0
            };
        }

        var rate = GetTravelRate(travelType);

        // Calculate rounds needed (round up for partial rounds)
        int rounds = (int)Math.Ceiling((double)distanceMeters / rate.MetersPerRound);

        // Calculate fatigue cost
        int fatigue = (int)Math.Ceiling(distanceMeters * rate.FatigueCostPerMeter);

        return new TravelResult
        {
            DistanceMeters = distanceMeters,
            TravelType = travelType,
            RoundsRequired = rounds,
            FatigueCost = fatigue
        };
    }

    /// <inheritdoc />
    public int CalculateMaxDistance(int availableFatigue, TravelType travelType)
    {
        if (availableFatigue <= 0) return 0;

        var rate = GetTravelRate(travelType);

        // Invert the fatigue cost formula
        // fatigue = distance * costPerMeter
        // distance = fatigue / costPerMeter
        return (int)(availableFatigue / rate.FatigueCostPerMeter);
    }

    /// <inheritdoc />
    public string RoundsToTimeString(int rounds)
    {
        return _timeFormat.FormatRoundsDetailed(rounds);
    }
}

/// <summary>
/// Types of sustained travel movement.
/// </summary>
public enum TravelType
{
    /// <summary>
    /// Normal walking pace - 4m/round, very low fatigue.
    /// </summary>
    Walking,

    /// <summary>
    /// Sustained jogging - 10m/round, moderate fatigue.
    /// </summary>
    EnduranceRunning,

    /// <summary>
    /// Fast running - 12m/round, high fatigue.
    /// </summary>
    BurstRunning,

    /// <summary>
    /// Maximum speed sprint - 16m/round, extreme fatigue.
    /// </summary>
    FastSprinting
}

/// <summary>
/// Defines the properties of a travel rate.
/// </summary>
public class TravelRate
{
    /// <summary>
    /// The travel type.
    /// </summary>
    public TravelType Type { get; init; }

    /// <summary>
    /// Speed in meters per round.
    /// </summary>
    public int MetersPerRound { get; init; }

    /// <summary>
    /// Fatigue cost per meter traveled.
    /// </summary>
    public double FatigueCostPerMeter { get; init; }

    /// <summary>
    /// Human-readable description.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Speed in meters per hour.
    /// </summary>
    public int MetersPerHour => MetersPerRound * TravelCalculator.RoundsPerHour;

    /// <summary>
    /// Speed in kilometers per hour.
    /// </summary>
    public double KilometersPerHour => MetersPerHour / 1000.0;
}

/// <summary>
/// Result of a travel calculation.
/// </summary>
public class TravelResult
{
    /// <summary>
    /// Distance traveled in meters.
    /// </summary>
    public int DistanceMeters { get; init; }

    /// <summary>
    /// The travel type used.
    /// </summary>
    public TravelType TravelType { get; init; }

    /// <summary>
    /// Number of rounds required.
    /// </summary>
    public int RoundsRequired { get; init; }

    /// <summary>
    /// Total fatigue cost.
    /// </summary>
    public int FatigueCost { get; init; }

    /// <summary>
    /// Time required in seconds.
    /// </summary>
    public int TimeSeconds => RoundsRequired * TravelCalculator.SecondsPerRound;

    /// <summary>
    /// Human-readable time string.
    /// </summary>
    public string TimeString
    {
        get
        {
            // Use the default time format service for static access
            var formatter = new DefaultGameTimeFormatService();
            return formatter.FormatRoundsDetailed(RoundsRequired);
        }
    }

    /// <summary>
    /// Gets a summary of the travel result.
    /// </summary>
    public string GetSummary()
    {
        return $"{TravelType}: {DistanceMeters}m in {TimeString}, costs {FatigueCost} FAT";
    }
}
