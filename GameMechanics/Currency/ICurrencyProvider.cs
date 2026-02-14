using System.Collections.Generic;
using Threa.Dal.Dto;

namespace GameMechanics;

/// <summary>
/// Abstraction for setting-specific currency systems.
/// Fantasy uses fixed exchange rates (CP/SP/GP/PP), sci-fi uses independent currencies (IC/CS/HS).
/// </summary>
public interface ICurrencyProvider
{
    string Setting { get; }
    IReadOnlyList<CurrencyDenomination> Denominations { get; }
    bool HasFixedExchangeRates { get; }
    long? CalculateBaseValue(IEnumerable<WalletEntry> wallet);
    string FormatTotal(IEnumerable<WalletEntry> wallet);
    string FormatDenomination(string code, int amount);
    List<WalletEntry> CreateEmptyWallet();
}
