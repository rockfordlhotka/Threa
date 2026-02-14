using System;
using System.Collections.Generic;
using System.Linq;
using Csla;
using Csla.Rules.CommonRules;
using Threa.Dal.Dto;

namespace GameMechanics
{
  [Serializable]
  public class WalletEntryEdit : BusinessBase<WalletEntryEdit>
  {
    public static readonly PropertyInfo<string> CurrencyCodeProperty = RegisterProperty<string>(nameof(CurrencyCode));
    public string CurrencyCode
    {
      get => GetProperty(CurrencyCodeProperty);
      private set => LoadProperty(CurrencyCodeProperty, value);
    }

    public static readonly PropertyInfo<int> AmountProperty = RegisterProperty<int>(nameof(Amount));
    public int Amount
    {
      get => GetProperty(AmountProperty);
      set => SetProperty(AmountProperty, value);
    }

    /// <summary>
    /// Gets the display name for this denomination (e.g., "Gold Pieces", "Imperial Credits").
    /// </summary>
    public string DisplayName
    {
      get
      {
        var provider = CurrencyProviderFactory.GetProvider(null);
        var denom = provider.Denominations.FirstOrDefault(d => d.Code == CurrencyCode);
        return denom?.Name ?? CurrencyCode;
      }
    }

    /// <summary>
    /// Gets the abbreviation for this denomination (e.g., "gp", "IC").
    /// </summary>
    public string Abbreviation
    {
      get
      {
        var provider = CurrencyProviderFactory.GetProvider(null);
        var denom = provider.Denominations.FirstOrDefault(d => d.Code == CurrencyCode);
        return denom?.Abbreviation ?? CurrencyCode;
      }
    }

    protected override void AddBusinessRules()
    {
      base.AddBusinessRules();
      BusinessRules.AddRule(new MinValue<int>(AmountProperty, 0)
        { MessageText = "Currency amount cannot be negative." });
    }

    [CreateChild]
    private void Create(string currencyCode)
    {
      using (BypassPropertyChecks)
      {
        CurrencyCode = currencyCode;
        LoadProperty(AmountProperty, 0);
      }
    }

    [FetchChild]
    private void Fetch(WalletEntry entry)
    {
      using (BypassPropertyChecks)
      {
        CurrencyCode = entry.CurrencyCode;
        LoadProperty(AmountProperty, entry.Amount);
      }
    }

    [InsertChild]
    [UpdateChild]
    private void InsertUpdate(List<WalletEntry> walletEntries)
    {
      using (BypassPropertyChecks)
      {
        var item = walletEntries.FirstOrDefault(e => e.CurrencyCode == CurrencyCode);
        if (item == null)
        {
          item = new WalletEntry { CurrencyCode = CurrencyCode };
          walletEntries.Add(item);
        }
        item.Amount = Amount;
      }
    }
  }
}
