using Csla;
using System;
using System.Collections.Generic;
using Threa.Dal;

namespace GameMechanics
{
  [Serializable]
  public class DamageList : BusinessListBase<DamageList, Damage>
  {
    [CreateChild]
    private void Create(Character character)
    {
      Add(DataPortal.CreateChild<Damage>("FAT", character));
      Add(DataPortal.CreateChild<Damage>("VIT", character));
    }

    [FetchChild]
    private void Fetch(List<IDamage> list)
    {
      if (list == null) return;
      using (LoadListMode)
      {
        foreach (var item in list)
          Add(DataPortal.FetchChild<Damage>(item));
      }
    }
  }
}
