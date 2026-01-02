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

    /// <summary>
    /// Adds a new skill from the skill reference list.
    /// </summary>
    public async Task<SkillEdit> AddSkillAsync(string skillIdentifier, IDataPortal<Reference.SkillList> skillListPortal, IChildDataPortal<SkillEdit> skillPortal)
    {
      var allSkills = await skillListPortal.FetchAsync();
      // Try to find by Id first, then by Name
      var skillInfo = allSkills.FirstOrDefault(s => s.Id == skillIdentifier) 
                   ?? allSkills.FirstOrDefault(s => s.Name == skillIdentifier);
      if (skillInfo == null)
        throw new ArgumentException($"Skill with identifier '{skillIdentifier}' not found", nameof(skillIdentifier));
      
      var newSkill = skillPortal.CreateChild(skillInfo);
      Add(newSkill);
      return newSkill;
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