using Csla;
using System;
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
    private void Fetch(ICharacter character)
    {
      using (LoadListMode)
      {
        Add(DataPortal.FetchChild<Damage>("FAT", character));
        Add(DataPortal.FetchChild<Damage>("VIT", character));
      }
    }
  }
}
