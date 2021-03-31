using Csla;
using System;
using System.Collections.Generic;
using Threa.Dal;

namespace GameMechanics.Player
{
  [Serializable]
  public class AttributeEditList : BusinessListBase<AttributeEditList, AttributeEdit>
  {
    [CreateChild]
    private void Create()
    {
      Add(DataPortal.CreateChild<AttributeEdit>("STR"));
      Add(DataPortal.CreateChild<AttributeEdit>("DEX"));
      Add(DataPortal.CreateChild<AttributeEdit>("END"));
      Add(DataPortal.CreateChild<AttributeEdit>("INT"));
      Add(DataPortal.CreateChild<AttributeEdit>("ITT"));
      Add(DataPortal.CreateChild<AttributeEdit>("WIL"));
      Add(DataPortal.CreateChild<AttributeEdit>("PHY"));
      Add(DataPortal.CreateChild<AttributeEdit>("SOC"));
    }

    [FetchChild]
    private void Fetch(List<ICharacterAttribute> list)
    {
      if (list == null)
      {
        Create();
      }
      else
      {
        using (LoadListMode)
        {
          foreach (var item in list)
            Add(DataPortal.FetchChild<AttributeEdit>(item));
        }
      }
    }
  }
}
