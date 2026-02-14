namespace GameMechanics;

/// <summary>
/// Factory for creating setting-appropriate currency providers.
/// </summary>
public static class CurrencyProviderFactory
{
    public static ICurrencyProvider GetProvider(string? setting) => setting switch
    {
        GameSettings.SciFi => new SciFiCurrencyProvider(),
        _ => new FantasyCurrencyProvider()
    };
}
