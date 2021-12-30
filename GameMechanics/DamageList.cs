using Csla;
using System;
using Threa.Dal;

namespace GameMechanics
{
  [Serializable]
  public class DamageList : BusinessListBase<DamageList, Damage>
  {
    [CreateChild]
    private void Create(CharacterEdit character, [Inject] IChildDataPortal<Damage> damagePortal)
    {
      Add(damagePortal.CreateChild("FAT", character));
      Add(damagePortal.CreateChild("VIT", character));
    }

    [FetchChild]
    private void Fetch(ICharacter character, [Inject] IChildDataPortal<Damage> damagePortal)
    {
      using (LoadListMode)
      {
        Add(damagePortal.FetchChild("FAT", character));
        Add(damagePortal.FetchChild("VIT", character));
      }
    }
  }
}
