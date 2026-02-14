using System.Collections.Generic;
using System.Linq;
using Threa.Dal.Dto;

namespace GameMechanics;

/// <summary>
/// Sci-fi setting currency provider with independent currency types.
/// IC (Imperial Credits), CS (Confederate Standards), HS (Helicon Scrip).
/// No fixed exchange rates between denominations.
/// </summary>
public class SciFiCurrencyProvider : ICurrencyProvider
{
    public string Setting => GameSettings.SciFi;
    public bool HasFixedExchangeRates => false;

    private static readonly IReadOnlyList<CurrencyDenomination> _denominations =
    [
        new("IC", "Imperial Credits", "IC", 0, null),
        new("CS", "Confederate Standards", "CS", 1, null),
        new("HS", "Helicon Scrip", "HS", 2, null),
    ];

    public IReadOnlyList<CurrencyDenomination> Denominations => _denominations;

    public long? CalculateBaseValue(IEnumerable<WalletEntry> wallet) => null;

    public string FormatTotal(IEnumerable<WalletEntry> wallet)
    {
        var parts = new List<string>();
        foreach (var entry in wallet)
        {
            if (entry.Amount != 0)
                parts.Add(FormatDenomination(entry.CurrencyCode, entry.Amount));
        }
        return parts.Count > 0 ? string.Join(" | ", parts) : "0 IC";
    }

    public string FormatDenomination(string code, int amount)
    {
        var denom = _denominations.FirstOrDefault(d => d.Code == code);
        return denom != null ? $"{amount} {denom.Abbreviation}" : $"{amount} {code}";
    }

    public List<WalletEntry> CreateEmptyWallet()
    {
        return _denominations
            .Select(d => new WalletEntry { CurrencyCode = d.Code, Amount = 0 })
            .ToList();
    }
}
