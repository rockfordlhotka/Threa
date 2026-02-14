using Microsoft.VisualStudio.TestTools.UnitTesting;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class CurrencyProviderTests
{
    [TestMethod]
    public void FantasyProvider_Returns4Denominations()
    {
        var provider = new FantasyCurrencyProvider();
        Assert.AreEqual(4, provider.Denominations.Count);
    }

    [TestMethod]
    public void FantasyProvider_DenominationsHaveCorrectBaseValues()
    {
        var provider = new FantasyCurrencyProvider();
        Assert.AreEqual(8000, provider.Denominations.First(d => d.Code == "PP").BaseUnitValue);
        Assert.AreEqual(400, provider.Denominations.First(d => d.Code == "GP").BaseUnitValue);
        Assert.AreEqual(20, provider.Denominations.First(d => d.Code == "SP").BaseUnitValue);
        Assert.AreEqual(1, provider.Denominations.First(d => d.Code == "CP").BaseUnitValue);
    }

    [TestMethod]
    public void FantasyProvider_HasFixedExchangeRates()
    {
        var provider = new FantasyCurrencyProvider();
        Assert.IsTrue(provider.HasFixedExchangeRates);
    }

    [TestMethod]
    public void FantasyProvider_CalculateBaseValue_SumsCorrectly()
    {
        var provider = new FantasyCurrencyProvider();
        var wallet = new List<WalletEntry>
        {
            new() { CurrencyCode = "PP", Amount = 1 },
            new() { CurrencyCode = "GP", Amount = 2 },
            new() { CurrencyCode = "SP", Amount = 5 },
            new() { CurrencyCode = "CP", Amount = 10 },
        };
        // 8000 + 800 + 100 + 10 = 8910
        Assert.AreEqual(8910L, provider.CalculateBaseValue(wallet));
    }

    [TestMethod]
    public void FantasyProvider_FormatTotal_UsesMoneyChanger()
    {
        var provider = new FantasyCurrencyProvider();
        var wallet = new List<WalletEntry>
        {
            new() { CurrencyCode = "PP", Amount = 0 },
            new() { CurrencyCode = "GP", Amount = 0 },
            new() { CurrencyCode = "SP", Amount = 0 },
            new() { CurrencyCode = "CP", Amount = 50 },
        };
        var result = provider.FormatTotal(wallet);
        Assert.AreEqual("2sp 10cp", result);
    }

    [TestMethod]
    public void FantasyProvider_CreateEmptyWallet_Returns4Entries()
    {
        var provider = new FantasyCurrencyProvider();
        var wallet = provider.CreateEmptyWallet();
        Assert.AreEqual(4, wallet.Count);
        Assert.IsTrue(wallet.All(e => e.Amount == 0));
        Assert.IsTrue(wallet.Any(e => e.CurrencyCode == "PP"));
        Assert.IsTrue(wallet.Any(e => e.CurrencyCode == "GP"));
        Assert.IsTrue(wallet.Any(e => e.CurrencyCode == "SP"));
        Assert.IsTrue(wallet.Any(e => e.CurrencyCode == "CP"));
    }

    [TestMethod]
    public void FantasyProvider_FromLegacyCoins_ConvertsCorrectly()
    {
        var wallet = FantasyCurrencyProvider.FromLegacyCoins(10, 5, 2, 1);
        Assert.AreEqual(4, wallet.Count);
        Assert.AreEqual(1, wallet.First(e => e.CurrencyCode == "PP").Amount);
        Assert.AreEqual(2, wallet.First(e => e.CurrencyCode == "GP").Amount);
        Assert.AreEqual(5, wallet.First(e => e.CurrencyCode == "SP").Amount);
        Assert.AreEqual(10, wallet.First(e => e.CurrencyCode == "CP").Amount);
    }

    [TestMethod]
    public void SciFiProvider_Returns3Denominations()
    {
        var provider = new SciFiCurrencyProvider();
        Assert.AreEqual(3, provider.Denominations.Count);
    }

    [TestMethod]
    public void SciFiProvider_DenominationsHaveNoBaseValue()
    {
        var provider = new SciFiCurrencyProvider();
        Assert.IsTrue(provider.Denominations.All(d => d.BaseUnitValue == null));
    }

    [TestMethod]
    public void SciFiProvider_HasNoFixedExchangeRates()
    {
        var provider = new SciFiCurrencyProvider();
        Assert.IsFalse(provider.HasFixedExchangeRates);
    }

    [TestMethod]
    public void SciFiProvider_CalculateBaseValue_ReturnsNull()
    {
        var provider = new SciFiCurrencyProvider();
        var wallet = new List<WalletEntry>
        {
            new() { CurrencyCode = "IC", Amount = 500 },
            new() { CurrencyCode = "CS", Amount = 200 },
            new() { CurrencyCode = "HS", Amount = 100 },
        };
        Assert.IsNull(provider.CalculateBaseValue(wallet));
    }

    [TestMethod]
    public void SciFiProvider_FormatTotal_ShowsPerCurrency()
    {
        var provider = new SciFiCurrencyProvider();
        var wallet = new List<WalletEntry>
        {
            new() { CurrencyCode = "IC", Amount = 500 },
            new() { CurrencyCode = "CS", Amount = 200 },
            new() { CurrencyCode = "HS", Amount = 100 },
        };
        var result = provider.FormatTotal(wallet);
        Assert.AreEqual("500 IC | 200 CS | 100 HS", result);
    }

    [TestMethod]
    public void SciFiProvider_FormatTotal_SkipsZeroValues()
    {
        var provider = new SciFiCurrencyProvider();
        var wallet = new List<WalletEntry>
        {
            new() { CurrencyCode = "IC", Amount = 500 },
            new() { CurrencyCode = "CS", Amount = 0 },
            new() { CurrencyCode = "HS", Amount = 100 },
        };
        var result = provider.FormatTotal(wallet);
        Assert.AreEqual("500 IC | 100 HS", result);
    }

    [TestMethod]
    public void SciFiProvider_CreateEmptyWallet_Returns3Entries()
    {
        var provider = new SciFiCurrencyProvider();
        var wallet = provider.CreateEmptyWallet();
        Assert.AreEqual(3, wallet.Count);
        Assert.IsTrue(wallet.All(e => e.Amount == 0));
        Assert.IsTrue(wallet.Any(e => e.CurrencyCode == "IC"));
        Assert.IsTrue(wallet.Any(e => e.CurrencyCode == "CS"));
        Assert.IsTrue(wallet.Any(e => e.CurrencyCode == "HS"));
    }

    [TestMethod]
    public void Factory_ReturnsFantasyProvider_ForFantasySetting()
    {
        var provider = CurrencyProviderFactory.GetProvider(GameSettings.Fantasy);
        Assert.IsInstanceOfType(provider, typeof(FantasyCurrencyProvider));
    }

    [TestMethod]
    public void Factory_ReturnsSciFiProvider_ForSciFiSetting()
    {
        var provider = CurrencyProviderFactory.GetProvider(GameSettings.SciFi);
        Assert.IsInstanceOfType(provider, typeof(SciFiCurrencyProvider));
    }

    [TestMethod]
    public void Factory_ReturnsFantasyProvider_ForNullSetting()
    {
        var provider = CurrencyProviderFactory.GetProvider(null);
        Assert.IsInstanceOfType(provider, typeof(FantasyCurrencyProvider));
    }

    [TestMethod]
    public void Factory_ReturnsFantasyProvider_ForUnknownSetting()
    {
        var provider = CurrencyProviderFactory.GetProvider("steampunk");
        Assert.IsInstanceOfType(provider, typeof(FantasyCurrencyProvider));
    }
}
