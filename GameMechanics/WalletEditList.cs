using System;
using System.Collections.Generic;
using System.Linq;
using Csla;
using Threa.Dal.Dto;

namespace GameMechanics
{
  [Serializable]
  public class WalletEditList : BusinessListBase<WalletEditList, WalletEntryEdit>
  {
    /// <summary>
    /// Gets the amount for a specific currency code.
    /// </summary>
    public int GetAmount(string code)
    {
      var entry = this.FirstOrDefault(e => e.CurrencyCode == code);
      return entry?.Amount ?? 0;
    }

    /// <summary>
    /// Sets the amount for a specific currency code.
    /// </summary>
    public void SetAmount(string code, int amount)
    {
      var entry = this.FirstOrDefault(e => e.CurrencyCode == code);
      if (entry != null)
        entry.Amount = amount;
    }

    [CreateChild]
    private void Create(string setting, [Inject] IChildDataPortal<WalletEntryEdit> entryPortal)
    {
      var provider = CurrencyProviderFactory.GetProvider(setting);
      foreach (var denom in provider.Denominations)
        Add(entryPortal.CreateChild(denom.Code));
    }

    [FetchChild]
    private void Fetch(List<WalletEntry> entries, [Inject] IChildDataPortal<WalletEntryEdit> entryPortal)
    {
      using (LoadListMode)
      {
        foreach (var entry in entries)
          Add(entryPortal.FetchChild(entry));
      }
    }
  }
}
