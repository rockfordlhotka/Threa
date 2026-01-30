namespace GameMechanics.Actions;

/// <summary>
/// Calculates travel time and fatigue costs for sustained movement.
/// </summary>
public interface ITravelCalculator
{
    /// <summary>
    /// Gets the travel rate definition for a travel type.
    /// </summary>
    /// <param name="travelType">The type of travel.</param>
    /// <returns>The travel rate definition.</returns>
    TravelRate GetTravelRate(TravelType travelType);

    /// <summary>
    /// Calculates travel result for a given distance and travel type.
    /// </summary>
    /// <param name="distanceMeters">Distance to travel in meters.</param>
    /// <param name="travelType">The travel type/pace.</param>
    /// <returns>Travel result with time and fatigue calculations.</returns>
    TravelResult CalculateTravel(int distanceMeters, TravelType travelType);

    /// <summary>
    /// Calculates how far a character can travel with available fatigue.
    /// </summary>
    /// <param name="availableFatigue">Available FAT to spend.</param>
    /// <param name="travelType">The travel type/pace.</param>
    /// <returns>Maximum distance achievable in meters.</returns>
    int CalculateMaxDistance(int availableFatigue, TravelType travelType);

    /// <summary>
    /// Converts rounds to a readable time string.
    /// </summary>
    /// <param name="rounds">Number of rounds.</param>
    /// <returns>Human-readable time string.</returns>
    string RoundsToTimeString(int rounds);
}
