using Csla;
using System;
using System.Collections.Generic;
using Threa.Dal;

namespace GameMechanics
{
  [Serializable]
  public class AttributeList : BusinessListBase<AttributeList, Attribute>
  {
    [CreateChild]
    private void Create()
    {
      Add(DataPortal.CreateChild<Attribute>("STR"));
      Add(DataPortal.CreateChild<Attribute>("DEX"));
      Add(DataPortal.CreateChild<Attribute>("END"));
      Add(DataPortal.CreateChild<Attribute>("INT"));
      Add(DataPortal.CreateChild<Attribute>("ITT"));
      Add(DataPortal.CreateChild<Attribute>("WIL"));
      Add(DataPortal.CreateChild<Attribute>("PHY"));
      Add(DataPortal.CreateChild<Attribute>("SOC"));
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
            Add(DataPortal.FetchChild<Attribute>(item));
        }
      }
    }
  }
}
