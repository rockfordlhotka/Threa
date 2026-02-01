using Csla;
using Csla.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Test
{
  [TestClass]
  public class SkillCanUseTests : TestBase
  {
    /// <summary>
    /// Test that a skill with a single secondary attribute requirement works correctly.
    /// </summary>
    [TestMethod]
    public void CanUse_SingleSecondaryAttribute_BelowMinimum_ReturnsFalse()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var skillp = provider.GetRequiredService<IChildDataPortal<SkillEdit>>();
      var c = dp.Create(42);
      
      // Set STR to 7, which is below the secondary attribute minimum of 8
      c.AttributeList.Where(r => r.Name == "STR").First().Value = 7;
      c.Vitality.Value = 10;
      c.Fatigue.Value = 10;
      
      var cs = new CharacterSkill
      {
        Id = "test-skill",
        Name = "Test Skill",
        PrimaryAttribute = "DEX",
        SecondaryAttribute = "STR",  // Requires STR >= 8
        Level = 3
      };
      var s = skillp.FetchChild(cs);
      c.Skills.Add(s);
      
      Assert.IsFalse(s.CanUse, "Skill should not be usable with STR=7 when SecondaryAttribute requires STR>=8");
      Assert.IsNotNull(s.CannotUseReason, "Should have a reason for not being usable");
      Assert.IsTrue(s.CannotUseReason.Contains("STR"), "Reason should mention STR");
    }

    /// <summary>
    /// Test that a skill with a single secondary attribute requirement works when above minimum.
    /// </summary>
    [TestMethod]
    public void CanUse_SingleSecondaryAttribute_AboveMinimum_ReturnsTrue()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var skillp = provider.GetRequiredService<IChildDataPortal<SkillEdit>>();
      var c = dp.Create(42);
      
      // Set STR to 9, which is above the secondary attribute minimum of 8
      c.AttributeList.Where(r => r.Name == "STR").First().Value = 9;
      c.Vitality.Value = 10;
      c.Fatigue.Value = 10;
      
      var cs = new CharacterSkill
      {
        Id = "test-skill",
        Name = "Test Skill",
        PrimaryAttribute = "DEX",
        SecondaryAttribute = "STR",  // Requires STR >= 8
        Level = 3
      };
      var s = skillp.FetchChild(cs);
      c.Skills.Add(s);
      
      Assert.IsTrue(s.CanUse, "Skill should be usable with STR=9 when SecondaryAttribute requires STR>=8");
      Assert.IsNull(s.CannotUseReason, "Should not have a reason for not being usable");
    }

    /// <summary>
    /// Test that a skill with a compound secondary attribute (e.g., "INT/STR") 
    /// correctly averages the two attributes for the minimum check.
    /// This tests the bug fix for issue: "Skill stat minimums aren't working right"
    /// </summary>
    [TestMethod]
    public void CanUse_CompoundSecondaryAttribute_AveragesBothAttributes()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var skillp = provider.GetRequiredService<IChildDataPortal<SkillEdit>>();
      var c = dp.Create(42);
      
      // Set INT to 10 and STR to 6
      // Average = (10 + 6) / 2 = 8, which meets the minimum requirement of 8
      c.AttributeList.Where(r => r.Name == "INT").First().Value = 10;
      c.AttributeList.Where(r => r.Name == "STR").First().Value = 6;
      c.Vitality.Value = 10;
      c.Fatigue.Value = 10;
      
      var cs = new CharacterSkill
      {
        Id = "heavy-gun",
        Name = "Heavy gun",
        PrimaryAttribute = "INT/STR",
        SecondaryAttribute = "INT/STR",  // Requires average of INT/STR >= 8
        TertiaryAttribute = "ITT",
        Level = 3
      };
      var s = skillp.FetchChild(cs);
      c.Skills.Add(s);
      
      Assert.IsTrue(s.CanUse, "Skill should be usable when average of INT/STR = 8");
      Assert.IsNull(s.CannotUseReason, "Should not have a reason for not being usable");
    }

    /// <summary>
    /// Test that a skill with a compound secondary attribute fails when average is below minimum.
    /// </summary>
    [TestMethod]
    public void CanUse_CompoundSecondaryAttribute_BelowMinimum_ReturnsFalse()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var skillp = provider.GetRequiredService<IChildDataPortal<SkillEdit>>();
      var c = dp.Create(42);
      
      // Set INT to 8 and STR to 6
      // Average = (8 + 6) / 2 = 7, which is below the minimum requirement of 8
      c.AttributeList.Where(r => r.Name == "INT").First().Value = 8;
      c.AttributeList.Where(r => r.Name == "STR").First().Value = 6;
      c.Vitality.Value = 10;
      c.Fatigue.Value = 10;
      
      var cs = new CharacterSkill
      {
        Id = "heavy-gun",
        Name = "Heavy gun",
        PrimaryAttribute = "INT/STR",
        SecondaryAttribute = "INT/STR",  // Requires average of INT/STR >= 8
        TertiaryAttribute = "ITT",
        Level = 3
      };
      var s = skillp.FetchChild(cs);
      c.Skills.Add(s);
      
      Assert.IsFalse(s.CanUse, "Skill should not be usable when average of INT/STR = 7");
      Assert.IsNotNull(s.CannotUseReason, "Should have a reason for not being usable");
      Assert.IsTrue(s.CannotUseReason.Contains("INT/STR"), "Reason should mention INT/STR");
      Assert.IsTrue(s.CannotUseReason.Contains("7"), "Reason should show current value of 7");
    }

    /// <summary>
    /// Test that a skill with a single tertiary attribute requirement works correctly.
    /// </summary>
    [TestMethod]
    public void CanUse_SingleTertiaryAttribute_BelowMinimum_ReturnsFalse()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var skillp = provider.GetRequiredService<IChildDataPortal<SkillEdit>>();
      var c = dp.Create(42);
      
      // Set ITT to 5, which is below the tertiary attribute minimum of 6
      c.AttributeList.Where(r => r.Name == "ITT").First().Value = 5;
      c.Vitality.Value = 10;
      c.Fatigue.Value = 10;
      
      var cs = new CharacterSkill
      {
        Id = "test-skill",
        Name = "Test Skill",
        PrimaryAttribute = "DEX",
        TertiaryAttribute = "ITT",  // Requires ITT >= 6
        Level = 3
      };
      var s = skillp.FetchChild(cs);
      c.Skills.Add(s);
      
      Assert.IsFalse(s.CanUse, "Skill should not be usable with ITT=5 when TertiaryAttribute requires ITT>=6");
      Assert.IsNotNull(s.CannotUseReason, "Should have a reason for not being usable");
      Assert.IsTrue(s.CannotUseReason.Contains("ITT"), "Reason should mention ITT");
    }

    /// <summary>
    /// Test that both secondary and tertiary requirements are checked.
    /// </summary>
    [TestMethod]
    public void CanUse_BothSecondaryAndTertiary_OneBelow_ReturnsFalse()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var skillp = provider.GetRequiredService<IChildDataPortal<SkillEdit>>();
      var c = dp.Create(42);
      
      // Set STR to 9 (above secondary min of 8), but ITT to 5 (below tertiary min of 6)
      c.AttributeList.Where(r => r.Name == "STR").First().Value = 9;
      c.AttributeList.Where(r => r.Name == "ITT").First().Value = 5;
      c.Vitality.Value = 10;
      c.Fatigue.Value = 10;
      
      var cs = new CharacterSkill
      {
        Id = "test-skill",
        Name = "Test Skill",
        PrimaryAttribute = "DEX",
        SecondaryAttribute = "STR",   // Requires STR >= 8
        TertiaryAttribute = "ITT",    // Requires ITT >= 6
        Level = 3
      };
      var s = skillp.FetchChild(cs);
      c.Skills.Add(s);
      
      Assert.IsFalse(s.CanUse, "Skill should not be usable when tertiary attribute is below minimum");
      Assert.IsNotNull(s.CannotUseReason, "Should have a reason for not being usable");
      Assert.IsTrue(s.CannotUseReason.Contains("ITT"), "Reason should mention ITT");
    }

    /// <summary>
    /// Test the "heavy-gun" skill from the actual game data, which has:
    /// PrimaryAttribute: INT/STR
    /// SecondaryAttribute: INT/STR
    /// TertiaryAttribute: ITT
    /// </summary>
    [TestMethod]
    public void CanUse_HeavyGunSkill_RealWorldScenario()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var skillp = provider.GetRequiredService<IChildDataPortal<SkillEdit>>();
      var c = dp.Create(42);
      
      // Set attributes to meet all requirements:
      // INT = 10, STR = 8 -> average = 9, which is >= 8 for SecondaryAttribute
      // ITT = 7, which is >= 6 for TertiaryAttribute
      c.AttributeList.Where(r => r.Name == "INT").First().Value = 10;
      c.AttributeList.Where(r => r.Name == "STR").First().Value = 8;
      c.AttributeList.Where(r => r.Name == "ITT").First().Value = 7;
      c.Vitality.Value = 10;
      c.Fatigue.Value = 10;
      
      var cs = new CharacterSkill
      {
        Id = "heavy-gun",
        Name = "Heavy gun",
        PrimaryAttribute = "INT/STR",
        SecondaryAttribute = "INT/STR",
        TertiaryAttribute = "ITT",
        Level = 3
      };
      var s = skillp.FetchChild(cs);
      c.Skills.Add(s);
      
      Assert.IsTrue(s.CanUse, "Heavy gun skill should be usable with INT=10, STR=8, ITT=7");
      Assert.IsNull(s.CannotUseReason, "Should not have a reason for not being usable");
    }

    /// <summary>
    /// Test that compound attributes with spaces (e.g., "INT / STR") are handled correctly.
    /// </summary>
    [TestMethod]
    public void CanUse_CompoundSecondaryAttribute_WithSpaces_HandlesCorrectly()
    {
      var provider = InitServices();
      var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
      var skillp = provider.GetRequiredService<IChildDataPortal<SkillEdit>>();
      var c = dp.Create(42);
      
      // Set INT to 10 and STR to 6
      // Average = (10 + 6) / 2 = 8, which meets the minimum requirement of 8
      c.AttributeList.Where(r => r.Name == "INT").First().Value = 10;
      c.AttributeList.Where(r => r.Name == "STR").First().Value = 6;
      c.Vitality.Value = 10;
      c.Fatigue.Value = 10;
      
      var cs = new CharacterSkill
      {
        Id = "test-skill",
        Name = "Test Skill with Spaces",
        PrimaryAttribute = "DEX",
        SecondaryAttribute = "INT / STR",  // With spaces - should still work
        Level = 3
      };
      var s = skillp.FetchChild(cs);
      c.Skills.Add(s);
      
      Assert.IsTrue(s.CanUse, "Skill should be usable when average of 'INT / STR' = 8");
      Assert.IsNull(s.CannotUseReason, "Should not have a reason for not being usable");
    }
  }
}
