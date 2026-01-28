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
      var skill = this.Where(r => r.Name.Equals(skillName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
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
    /// Checks if a skill can be learned based on attribute requirements.
    /// Secondary attribute must be >= 8, tertiary must be >= 6.
    /// </summary>
    /// <param name="skill">The skill to check</param>
    /// <returns>True if the character meets all attribute requirements</returns>
    public bool CanLearnSkill(Skill skill)
    {
      var character = Parent as CharacterEdit;
      if (character == null) return true; // No character context

      // Check secondary attribute requirement (>= 8)
      if (!string.IsNullOrWhiteSpace(skill.SecondaryAttribute))
      {
        var secondaryValue = character.GetEffectiveAttribute(skill.SecondaryAttribute);
        if (secondaryValue < SkillEdit.SecondaryAttributeMinimum)
          return false;
      }

      // Check tertiary attribute requirement (>= 6)
      if (!string.IsNullOrWhiteSpace(skill.TertiaryAttribute))
      {
        var tertiaryValue = character.GetEffectiveAttribute(skill.TertiaryAttribute);
        if (tertiaryValue < SkillEdit.TertiaryAttributeMinimum)
          return false;
      }

      return true;
    }

    /// <summary>
    /// Gets the reason why a skill cannot be learned, or null if it can be learned.
    /// </summary>
    public string? WhyCannotLearnSkill(Skill skill)
    {
      var character = Parent as CharacterEdit;
      if (character == null) return null;

      var reasons = new List<string>();

      // Check secondary attribute requirement (>= 8)
      if (!string.IsNullOrWhiteSpace(skill.SecondaryAttribute))
      {
        var secondaryValue = character.GetEffectiveAttribute(skill.SecondaryAttribute);
        if (secondaryValue < SkillEdit.SecondaryAttributeMinimum)
          reasons.Add($"Requires {skill.SecondaryAttribute} >= {SkillEdit.SecondaryAttributeMinimum}, current: {secondaryValue}");
      }

      // Check tertiary attribute requirement (>= 6)
      if (!string.IsNullOrWhiteSpace(skill.TertiaryAttribute))
      {
        var tertiaryValue = character.GetEffectiveAttribute(skill.TertiaryAttribute);
        if (tertiaryValue < SkillEdit.TertiaryAttributeMinimum)
          reasons.Add($"Requires {skill.TertiaryAttribute} >= {SkillEdit.TertiaryAttributeMinimum}, current: {tertiaryValue}");
      }

      return reasons.Count > 0 ? string.Join("; ", reasons) : null;
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