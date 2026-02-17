namespace GameMechanics;

/// <summary>
/// Defines a single currency denomination within a game setting.
/// </summary>
public record CurrencyDenomination(
    string Code,
    string Name,
    string Abbreviation,
    int SortOrder,
    int? BaseUnitValue
);
