using System;
using System.Collections.Generic;
using System.Linq;
using Csla;
using Threa.Dal;

namespace GameMechanics.Player
{
  [Serializable]
  public class SkillEditList : BusinessListBase<SkillEditList, SkillEdit>
  {
    [CreateChild]
    private void Create()
    {
      var std = Reference.SkillList.GetList().Where(r => r.IsStandard);
      foreach (var item in std)
        Add(DataPortal.CreateChild<SkillEdit>(item));
    }

    [FetchChild]
    private void Fetch(List<ICharacterSkill> skills)
    {
      if (skills == null) return;
      using (LoadListMode)
      {
        foreach (var item in skills)
          Add(DataPortal.FetchChild<SkillEdit>(item));
      }
    }
  }
}