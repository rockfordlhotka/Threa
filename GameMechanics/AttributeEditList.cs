using Csla;
using System;
using System.Collections.Generic;
using Threa.Dal.Dto;

namespace GameMechanics
{
  [Serializable]
  public class AttributeEditList : BusinessListBase<AttributeEditList, AttributeEdit>
  {
    [CreateChild]
    private void Create([Inject] IChildDataPortal<AttributeEdit> attributePortal)
    {
      Add(attributePortal.CreateChild("STR"));
      Add(attributePortal.CreateChild("DEX"));
      Add(attributePortal.CreateChild("END"));
      Add(attributePortal.CreateChild("INT"));
      Add(attributePortal.CreateChild("ITT"));
      Add(attributePortal.CreateChild("WIL"));
      Add(attributePortal.CreateChild("PHY"));
    }

    [FetchChild]
    private void Fetch(List<CharacterAttribute> list, [Inject] IChildDataPortal<AttributeEdit> attributePortal)
    {
      if (list == null)
      {
        Create(attributePortal);
      }
      else
      {
        using (LoadListMode)
        {
          foreach (var item in list)
            Add(attributePortal.FetchChild(item));
        }
      }
    }
  }
}
