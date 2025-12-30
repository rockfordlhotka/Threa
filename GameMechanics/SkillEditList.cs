using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Csla;
using GameMechanics.Reference;
using Threa.Dal.Dto;

namespace GameMechanics
{
  [Serializable]
  public class SkillEditList : BusinessListBase<SkillEditList, SkillEdit>
  {
    /// <summary>
    /// Gets the sum of all individual skill levels.
    /// Used for calculating Max AP = TotalSkillLevels / 10.
    /// </summary>
    public int TotalSkillLevels => this.Sum(s => s.Level);

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
    private void Fetch(List<CharacterSkill> skills, [Inject] IChildDataPortal<SkillEdit> skillPortal)
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