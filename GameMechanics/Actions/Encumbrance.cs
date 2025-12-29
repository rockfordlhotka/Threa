using System;

namespace GameMechanics.Actions;

/// <summary>
/// Calculates encumbrance effects on movement based on carried weight vs capacity.
/// </summary>
public static class Encumbrance
{
    /// <summary>
    /// Base weight capacity at Physicality 10 (in pounds).
    /// </summary>
    public const double BaseWeightCapacity = 50.0;

    /// <summary>
    /// Base volume capacity at Physicality 10 (in cubic feet).
    /// </summary>
    public const double BaseVolumeCapacity = 10.0;

    /// <summary>
    /// Scaling factor per Physicality point (15% per point).
    /// </summary>
    public const double ScalingFactor = 1.15;

    /// <summary>
    /// Average Physicality value (bell curve center).
    /// </summary>
    public const int AveragePhysicality = 10;

    /// <summary>
    /// Encumbrance levels based on carried weight percentage.
    /// </summary>
    public static readonly EncumbranceThreshold[] Thresholds = new[]
    {
        new EncumbranceThreshold(0.0, EncumbranceLevel.Unencumbered, 0, "No penalty"),
        new EncumbranceThreshold(0.5, EncumbranceLevel.Light, 0, "Light load - no movement penalty"),
        new EncumbranceThreshold(0.75, EncumbranceLevel.Medium, -1, "Medium load - slightly slowed"),
        new EncumbranceThreshold(1.0, EncumbranceLevel.Heavy, -2, "Heavy load - significantly slowed"),
        new EncumbranceThreshold(1.25, EncumbranceLevel.VeryHeavy, -3, "Very heavy - struggling"),
        new EncumbranceThreshold(1.5, EncumbranceLevel.Overloaded, -4, "Overloaded - barely mobile")
    };

    /// <summary>
    /// Calculates maximum weight capacity based on Physicality.
    /// Formula: 50 lbs × (1.15 ^ (Physicality - 10))
    /// </summary>
    /// <param name="physicality">Character's Physicality attribute.</param>
    /// <returns>Maximum weight capacity in pounds.</returns>
    public static double CalculateMaxWeight(int physicality)
    {
        return BaseWeightCapacity * Math.Pow(ScalingFactor, physicality - AveragePhysicality);
    }

    /// <summary>
    /// Calculates maximum volume capacity based on Physicality.
    /// Formula: 10 cu.ft. × (1.15 ^ (Physicality - 10))
    /// </summary>
    /// <param name="physicality">Character's Physicality attribute.</param>
    /// <returns>Maximum volume capacity in cubic feet.</returns>
    public static double CalculateMaxVolume(int physicality)
    {
        return BaseVolumeCapacity * Math.Pow(ScalingFactor, physicality - AveragePhysicality);
    }

    /// <summary>
    /// Calculates encumbrance status based on current weight and known capacity.
    /// </summary>
    /// <param name="currentWeight">Current carried weight in pounds.</param>
    /// <param name="maxWeight">Maximum weight capacity in pounds.</param>
    /// <returns>Encumbrance status with level and penalties.</returns>
    public static EncumbranceStatus CalculateEncumbrance(double currentWeight, double maxWeight)
    {
        if (maxWeight <= 0)
        {
            return new EncumbranceStatus
            {
                Level = EncumbranceLevel.Overloaded,
                WeightRatio = double.MaxValue,
                CurrentWeight = currentWeight,
                MaxWeight = 0,
                RangePenalty = -4,
                Description = "Cannot carry anything"
            };
        }

        double ratio = currentWeight / maxWeight;
        return CalculateFromRatio(ratio, currentWeight, maxWeight);
    }

    /// <summary>
    /// Calculates encumbrance status based on current weight and Physicality.
    /// </summary>
    /// <param name="currentWeight">Current carried weight in pounds.</param>
    /// <param name="physicality">Character's Physicality attribute.</param>
    /// <returns>Encumbrance status with level and penalties.</returns>
    public static EncumbranceStatus CalculateFromPhysicality(double currentWeight, int physicality)
    {
        double maxWeight = CalculateMaxWeight(physicality);
        double ratio = currentWeight / maxWeight;
        
        return CalculateFromRatio(ratio, currentWeight, maxWeight);
    }

    private static EncumbranceStatus CalculateFromRatio(double ratio, double currentWeight, double maxWeight)
    {
        EncumbranceThreshold matched = Thresholds[0];

        foreach (var threshold in Thresholds)
        {
            if (ratio >= threshold.MinRatio)
            {
                matched = threshold;
            }
        }

        // If over 150%, use overloaded and can't move
        if (ratio >= 1.5)
        {
            return new EncumbranceStatus
            {
                Level = EncumbranceLevel.Overloaded,
                WeightRatio = ratio,
                CurrentWeight = currentWeight,
                MaxWeight = maxWeight,
                RangePenalty = -4,
                CanMove = false,
                Description = "Overloaded - cannot move"
            };
        }

        return new EncumbranceStatus
        {
            Level = matched.Level,
            WeightRatio = ratio,
            CurrentWeight = currentWeight,
            MaxWeight = maxWeight,
            RangePenalty = matched.RangePenalty,
            CanMove = true,
            Description = matched.Description
        };
    }

    /// <summary>
    /// Applies encumbrance penalty to a movement range.
    /// </summary>
    /// <param name="baseRange">Base movement range value.</param>
    /// <param name="encumbrance">Current encumbrance status.</param>
    /// <returns>Adjusted range after encumbrance penalty (minimum 0).</returns>
    public static int ApplyToMovement(int baseRange, EncumbranceStatus encumbrance)
    {
        if (!encumbrance.CanMove) return 0;
        return Math.Max(0, baseRange + encumbrance.RangePenalty);
    }

    /// <summary>
    /// Gets maximum achievable movement for a given movement type under encumbrance.
    /// </summary>
    /// <param name="movementType">The type of movement.</param>
    /// <param name="encumbrance">Current encumbrance status.</param>
    /// <returns>Maximum achievable range value.</returns>
    public static int GetMaxMovementRange(MovementType movementType, EncumbranceStatus encumbrance)
    {
        int baseRange = movementType switch
        {
            MovementType.FreePositioning => 2,
            MovementType.Sprint => 3,
            MovementType.FullRoundSprint => 5,
            _ => 0
        };

        return ApplyToMovement(baseRange, encumbrance);
    }
}

/// <summary>
/// Encumbrance levels from light to overloaded.
/// </summary>
public enum EncumbranceLevel
{
    /// <summary>Carrying less than 50% of capacity - no penalty.</summary>
    Unencumbered,
    
    /// <summary>Carrying 50-75% of capacity - no penalty.</summary>
    Light,
    
    /// <summary>Carrying 75-100% of capacity - -1 range.</summary>
    Medium,
    
    /// <summary>Carrying 100-125% of capacity - -2 range.</summary>
    Heavy,
    
    /// <summary>Carrying 125-150% of capacity - -3 range.</summary>
    VeryHeavy,
    
    /// <summary>Carrying over 150% of capacity - cannot move.</summary>
    Overloaded
}

/// <summary>
/// Defines an encumbrance threshold level.
/// </summary>
public class EncumbranceThreshold
{
    public double MinRatio { get; }
    public EncumbranceLevel Level { get; }
    public int RangePenalty { get; }
    public string Description { get; }

    public EncumbranceThreshold(double minRatio, EncumbranceLevel level, int rangePenalty, string description)
    {
        MinRatio = minRatio;
        Level = level;
        RangePenalty = rangePenalty;
        Description = description;
    }
}

/// <summary>
/// Current encumbrance status of a character.
/// </summary>
public class EncumbranceStatus
{
    /// <summary>The encumbrance level.</summary>
    public EncumbranceLevel Level { get; init; }
    
    /// <summary>Current weight as a ratio of max capacity (0.0 to 1.5+).</summary>
    public double WeightRatio { get; init; }
    
    /// <summary>Current carried weight in pounds.</summary>
    public double CurrentWeight { get; init; }
    
    /// <summary>Maximum weight capacity in pounds.</summary>
    public double MaxWeight { get; init; }
    
    /// <summary>Range penalty to apply to movement (-4 to 0).</summary>
    public int RangePenalty { get; init; }
    
    /// <summary>Whether the character can move at all.</summary>
    public bool CanMove { get; init; } = true;
    
    /// <summary>Human-readable description.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>Percentage of capacity used.</summary>
    public double PercentCapacity => WeightRatio * 100;

    /// <summary>Weight that can still be added before next threshold.</summary>
    public double RemainingCapacity => Math.Max(0, MaxWeight - CurrentWeight);

    /// <summary>Gets summary text for display.</summary>
    public string GetSummary()
    {
        return $"{Level}: {CurrentWeight:F1}/{MaxWeight:F1} lbs ({PercentCapacity:F0}%), {(RangePenalty >= 0 ? "no penalty" : $"{RangePenalty} range")}";
    }
}
