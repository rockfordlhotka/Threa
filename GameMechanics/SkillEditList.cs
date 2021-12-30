using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Csla;
using GameMechanics.Reference;
using Threa.Dal;

namespace GameMechanics
{
  [Serializable]
  public class SkillEditList : BusinessListBase<SkillEditList, SkillEdit>
  {
    public Reference.ResultValue SkillCheck(string skillName, int targetValue)
    {
      var skill = this.Where(r => r.Name == skillName).FirstOrDefault();
      if (skill == null)
      {
        return ResultValues.GetResult(-10);
      }
      else
      {
        return skill.SkillCheck();
      }
    }

    [CreateChild]
    private async Task Create([Inject] IDataPortal<Reference.SkillList> skillListPortal, [Inject] IChildDataPortal<SkillEdit> skillPortal)
    {
      var std = (await skillListPortal.FetchAsync()).Where(r => r.IsStandard);
      foreach (var item in std)
        Add(skillPortal.CreateChild(item));
    }

    [FetchChild]
    private void Fetch(List<ICharacterSkill> skills, [Inject] IChildDataPortal<SkillEdit> skillPortal)
    {
      if (skills == null) return;
      using (LoadListMode)
      {
        foreach (var item in skills)
          Add(skillPortal.FetchChild(item));
      }
    }
  }
}