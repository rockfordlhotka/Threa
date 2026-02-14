using Csla;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMechanics.Test;

[TestClass]
public class WalletEditTests : TestBase
{
    [TestMethod]
    public void CreateWallet_Fantasy_Has4Entries()
    {
        var provider = InitServices();
        var walletPortal = provider.GetRequiredService<IChildDataPortal<WalletEditList>>();
        var wallet = walletPortal.CreateChild(GameSettings.Fantasy);
        Assert.AreEqual(4, wallet.Count);
    }

    [TestMethod]
    public void CreateWallet_Fantasy_HasCorrectCodes()
    {
        var provider = InitServices();
        var walletPortal = provider.GetRequiredService<IChildDataPortal<WalletEditList>>();
        var wallet = walletPortal.CreateChild(GameSettings.Fantasy);
        Assert.IsTrue(wallet.Any(e => e.CurrencyCode == "PP"));
        Assert.IsTrue(wallet.Any(e => e.CurrencyCode == "GP"));
        Assert.IsTrue(wallet.Any(e => e.CurrencyCode == "SP"));
        Assert.IsTrue(wallet.Any(e => e.CurrencyCode == "CP"));
    }

    [TestMethod]
    public void CreateWallet_SciFi_Has3Entries()
    {
        var provider = InitServices();
        var walletPortal = provider.GetRequiredService<IChildDataPortal<WalletEditList>>();
        var wallet = walletPortal.CreateChild(GameSettings.SciFi);
        Assert.AreEqual(3, wallet.Count);
    }

    [TestMethod]
    public void CreateWallet_SciFi_HasCorrectCodes()
    {
        var provider = InitServices();
        var walletPortal = provider.GetRequiredService<IChildDataPortal<WalletEditList>>();
        var wallet = walletPortal.CreateChild(GameSettings.SciFi);
        Assert.IsTrue(wallet.Any(e => e.CurrencyCode == "IC"));
        Assert.IsTrue(wallet.Any(e => e.CurrencyCode == "CS"));
        Assert.IsTrue(wallet.Any(e => e.CurrencyCode == "HS"));
    }

    [TestMethod]
    public void GetAmount_ReturnsCorrectValue()
    {
        var provider = InitServices();
        var walletPortal = provider.GetRequiredService<IChildDataPortal<WalletEditList>>();
        var wallet = walletPortal.CreateChild(GameSettings.Fantasy);
        wallet.SetAmount("GP", 42);
        Assert.AreEqual(42, wallet.GetAmount("GP"));
    }

    [TestMethod]
    public void GetAmount_ReturnsZero_ForUnsetCode()
    {
        var provider = InitServices();
        var walletPortal = provider.GetRequiredService<IChildDataPortal<WalletEditList>>();
        var wallet = walletPortal.CreateChild(GameSettings.Fantasy);
        Assert.AreEqual(0, wallet.GetAmount("GP"));
    }

    [TestMethod]
    public void GetAmount_ReturnsZero_ForUnknownCode()
    {
        var provider = InitServices();
        var walletPortal = provider.GetRequiredService<IChildDataPortal<WalletEditList>>();
        var wallet = walletPortal.CreateChild(GameSettings.Fantasy);
        Assert.AreEqual(0, wallet.GetAmount("UNKNOWN"));
    }

    [TestMethod]
    public void SetAmount_UpdatesExistingEntry()
    {
        var provider = InitServices();
        var walletPortal = provider.GetRequiredService<IChildDataPortal<WalletEditList>>();
        var wallet = walletPortal.CreateChild(GameSettings.Fantasy);
        wallet.SetAmount("SP", 100);
        wallet.SetAmount("SP", 200);
        Assert.AreEqual(200, wallet.GetAmount("SP"));
    }

    [TestMethod]
    public void NegativeAmount_BreaksValidation()
    {
        var provider = InitServices();
        var walletPortal = provider.GetRequiredService<IChildDataPortal<WalletEditList>>();
        var wallet = walletPortal.CreateChild(GameSettings.Fantasy);
        var entry = wallet.First(e => e.CurrencyCode == "CP");
        entry.Amount = -5;
        Assert.IsFalse(entry.IsValid, "Entry should be invalid with negative amount");
    }

    [TestMethod]
    public void FetchWallet_LoadsFromEntries()
    {
        var provider = InitServices();
        var walletPortal = provider.GetRequiredService<IChildDataPortal<WalletEditList>>();
        var entries = new List<Threa.Dal.Dto.WalletEntry>
        {
            new() { CurrencyCode = "GP", Amount = 10 },
            new() { CurrencyCode = "CP", Amount = 50 },
        };
        var wallet = walletPortal.FetchChild(entries);
        Assert.AreEqual(2, wallet.Count);
        Assert.AreEqual(10, wallet.GetAmount("GP"));
        Assert.AreEqual(50, wallet.GetAmount("CP"));
    }
}
