using Csla;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace GameMechanics.Test
{
  [TestClass]
  public class SkillLevelDecreaseTest : TestBase
  {
    [TestMethod]
    public void NewCharacter_CapturesOriginalSkillLevels()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var character = dp.Create(42);
      
      // New character should have standard skills at level 0
      Assert.AreEqual(8, character.Skills.Count, "Should have 8 standard skills");
      
      // OriginalSkillLevels should be captured
      Assert.IsNotNull(character.OriginalSkillLevels);
      Assert.AreEqual(8, character.OriginalSkillLevels.Count, "Should have 8 skill entries");
      
      // All standard skills should have original level 0
      foreach (var skill in character.Skills)
      {
        Assert.IsTrue(character.OriginalSkillLevels.ContainsKey(skill.Name), $"Original levels should contain {skill.Name}");
        Assert.AreEqual(0, character.OriginalSkillLevels[skill.Name], $"{skill.Name} original level should be 0");
      }
    }

    [TestMethod]
    public void SkillLevelIncrease_AllowsDecreaseToOriginal()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var character = dp.Create(42);
      
      // Set up character with XP
      character.XPBanked = 1000;
      
      // Get a skill and increase its level
      var skill = character.Skills.First();
      var originalLevel = skill.Level;
      
      // Increase level twice
      skill.Level++;
      character.SpendXP(SkillCost.GetLevelUpCost(originalLevel, 5));
      
      skill.Level++;
      character.SpendXP(SkillCost.GetLevelUpCost(originalLevel + 1, 5));
      
      // Verify increases worked
      Assert.AreEqual(originalLevel + 2, skill.Level);
      
      // Verify original level is still tracked correctly
      Assert.AreEqual(originalLevel, character.OriginalSkillLevels[skill.Name]);
      
      // Simulate decrease back to original level
      // (The UI would check: skill.Level > character.OriginalSkillLevels[skill.Name])
      bool canDecrease = skill.Level > character.OriginalSkillLevels[skill.Name];
      Assert.IsTrue(canDecrease, "Should be able to decrease skill that was increased");
      
      // Decrease once
      skill.Level--;
      character.RefundXP(SkillCost.GetLevelUpCost(skill.Level, 5));
      
      // Can still decrease
      canDecrease = skill.Level > character.OriginalSkillLevels[skill.Name];
      Assert.IsTrue(canDecrease, "Should still be able to decrease");
      
      // Decrease to original
      skill.Level--;
      character.RefundXP(SkillCost.GetLevelUpCost(skill.Level, 5));
      
      // Now at original level - should not be able to decrease further
      canDecrease = skill.Level > character.OriginalSkillLevels[skill.Name];
      Assert.IsFalse(canDecrease, "Should NOT be able to decrease below original level");
      Assert.AreEqual(originalLevel, skill.Level);
    }

    [TestMethod]
    public void OriginalSkillLevels_PersistsAcrossOperations()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var character = dp.Create(42);
      
      character.XPBanked = 1000;
      
      // Capture the initial original levels
      var originalLevels = character.OriginalSkillLevels.ToDictionary(kv => kv.Key, kv => kv.Value);
      
      // Increase a skill
      var skill = character.Skills.First();
      skill.Level++;
      character.SpendXP(SkillCost.GetLevelUpCost(0, 5));
      
      // Original levels should not change
      foreach (var kvp in originalLevels)
      {
        Assert.AreEqual(kvp.Value, character.OriginalSkillLevels[kvp.Key], 
          $"Original level for {kvp.Key} should not change after skill increases");
      }
    }

    [TestMethod]
    public void NotActivatedCharacter_AllowsSkillDecreaseCheck()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var character = dp.Create(42);
      
      character.XPBanked = 1000;
      var skill = character.Skills.First();
      skill.Level++;
      character.SpendXP(SkillCost.GetLevelUpCost(0, 5));
      
      // Character is not activated (IsPlayable is false)
      Assert.IsFalse(character.IsPlayable);
      
      // Original skill levels should still be accessible
      Assert.IsNotNull(character.OriginalSkillLevels);
      Assert.AreEqual(8, character.OriginalSkillLevels.Count);
      
      // The check in UI would be: !character.IsPlayable && skill.Level > original
      // Before activation, this should be true for skills that were increased
      bool canDecrease = !character.IsPlayable && skill.Level > character.OriginalSkillLevels[skill.Name];
      Assert.IsTrue(canDecrease, "Should allow decrease before activation for increased skills");
    }
  }
}
