using System.Collections.Generic;
using System.Linq;
using Threa.Dal.Dto;

namespace GameMechanics;

/// <summary>
/// Fantasy setting currency provider using the existing coin system (CP/SP/GP/PP).
/// Wraps existing Currency.cs constants and MoneyChanger formatting.
/// </summary>
public class FantasyCurrencyProvider : ICurrencyProvider
{
    public string Setting => GameSettings.Fantasy;
    public bool HasFixedExchangeRates => true;

    private static readonly IReadOnlyList<CurrencyDenomination> _denominations =
    [
        new("PP", "Platinum Pieces", "pp", 0, Currency.CopperPerPlatinum),
        new("GP", "Gold Pieces", "gp", 1, Currency.CopperPerGold),
        new("SP", "Silver Pieces", "sp", 2, Currency.CopperPerSilver),
        new("CP", "Copper Pieces", "cp", 3, 1),
    ];

    public IReadOnlyList<CurrencyDenomination> Denominations => _denominations;

    public long? CalculateBaseValue(IEnumerable<WalletEntry> wallet)
    {
        long total = 0;
        foreach (var entry in wallet)
        {
            var denom = _denominations.FirstOrDefault(d => d.Code == entry.CurrencyCode);
            if (denom?.BaseUnitValue != null)
                total += (long)entry.Amount * denom.BaseUnitValue.Value;
        }
        return total;
    }

    public string FormatTotal(IEnumerable<WalletEntry> wallet)
    {
        var baseValue = CalculateBaseValue(wallet);
        return baseValue.HasValue ? MoneyChanger.FormatCopper(baseValue.Value) : "0cp";
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

    /// <summary>
    /// Converts legacy per-coin-type fields into WalletEntry list.
    /// Used during migration from the old Character DTO fields.
    /// </summary>
    public static List<WalletEntry> FromLegacyCoins(int copper, int silver, int gold, int platinum)
    {
        return
        [
            new() { CurrencyCode = "PP", Amount = platinum },
            new() { CurrencyCode = "GP", Amount = gold },
            new() { CurrencyCode = "SP", Amount = silver },
            new() { CurrencyCode = "CP", Amount = copper },
        ];
    }
}
