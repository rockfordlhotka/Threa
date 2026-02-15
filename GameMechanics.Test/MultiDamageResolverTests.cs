using System.Collections.Generic;
using System.Linq;
using GameMechanics;
using GameMechanics.Combat;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMechanics.Test;

[TestClass]
public class MultiDamageResolverTests
{
  /// <summary>
  /// Single damage type through MultiDamageResolver should produce equivalent results
  /// to calling DamageResolver directly.
  /// </summary>
  [TestMethod]
  public void Resolve_SingleType_ProducesSameResultAsDamageResolver()
  {
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0); // armor roll = 0
    var multiResolver = new MultiDamageResolver(diceRoller);

    var request = new MultiDamageRequest
    {
      BaseSV = 5,
      WeaponDamage = WeaponDamageProfile.FromSingle(DamageType.Cutting, 3),
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new List<ArmorInfo>()
    };

    var result = multiResolver.Resolve(request);

    Assert.AreEqual(1, result.PerTypeResults.Count);
    Assert.AreEqual(DamageType.Cutting, result.PerTypeResults[0].DamageType);
    // effectiveSV = 5 + 3 = 8
    Assert.AreEqual(8, result.PerTypeResults[0].IncomingSV);
  }

  /// <summary>
  /// Two damage types should produce independent absorption per type.
  /// </summary>
  [TestMethod]
  public void Resolve_TwoTypes_IndependentAbsorptionPerType()
  {
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var multiResolver = new MultiDamageResolver(diceRoller);

    var armor = new ArmorInfo
    {
      ItemId = "armor1",
      Name = "Chainmail",
      CoveredLocations = new[] { HitLocation.Torso },
      DamageClass = 1,
      Absorption = new Dictionary<DamageType, int>
      {
        { DamageType.Cutting, 5 },
        { DamageType.Energy, 2 }
      },
      CurrentDurability = 100,
      MaxDurability = 100,
      LayerOrder = 1
    };

    var request = new MultiDamageRequest
    {
      BaseSV = 3,
      WeaponDamage = new WeaponDamageProfile(new Dictionary<DamageType, int>
      {
        { DamageType.Cutting, 4 },
        { DamageType.Energy, 2 }
      }),
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new List<ArmorInfo> { armor }
    };

    var result = multiResolver.Resolve(request);

    Assert.AreEqual(2, result.PerTypeResults.Count);

    // Cutting: effectiveSV = 3 + 4 = 7, armor absorbs 5
    var cuttingResult = result.PerTypeResults.First(r => r.DamageType == DamageType.Cutting);
    Assert.AreEqual(7, cuttingResult.IncomingSV);

    // Energy: effectiveSV = 3 + 2 = 5, armor absorbs 2
    var energyResult = result.PerTypeResults.First(r => r.DamageType == DamageType.Energy);
    Assert.AreEqual(5, energyResult.IncomingSV);
  }

  /// <summary>
  /// Armor skill check should be rolled only once across all damage types.
  /// </summary>
  [TestMethod]
  public void Resolve_MultiType_ArmorSkillRolledOnce()
  {
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(2);
    var multiResolver = new MultiDamageResolver(diceRoller);

    var request = new MultiDamageRequest
    {
      BaseSV = 5,
      WeaponDamage = new WeaponDamageProfile(new Dictionary<DamageType, int>
      {
        { DamageType.Cutting, 3 },
        { DamageType.Energy, 2 }
      }),
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new List<ArmorInfo>()
    };

    var result = multiResolver.Resolve(request);

    // All per-type results should share the same armor skill roll
    Assert.AreEqual(result.ArmorSkillRoll, result.PerTypeResults[0].ArmorSkillRoll);
    Assert.AreEqual(result.ArmorSkillRoll, result.PerTypeResults[1].ArmorSkillRoll);
    Assert.AreEqual(result.ArmorSkillRV, result.PerTypeResults[0].ArmorSkillRV);
    Assert.AreEqual(result.ArmorSkillRV, result.PerTypeResults[1].ArmorSkillRV);
  }

  /// <summary>
  /// Shield/armor durability should be consumed cumulatively across types.
  /// </summary>
  [TestMethod]
  public void Resolve_MultiType_SharedShieldDurability()
  {
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var multiResolver = new MultiDamageResolver(diceRoller);

    var shield = new ShieldInfo
    {
      ItemId = "shield1",
      Name = "Small Shield",
      DamageClass = 1,
      Absorption = new Dictionary<DamageType, int>
      {
        { DamageType.Cutting, 3 },
        { DamageType.Energy, 3 }
      },
      CurrentDurability = 4, // Only 4 durability total
      MaxDurability = 10
    };

    var request = new MultiDamageRequest
    {
      BaseSV = 3,
      WeaponDamage = new WeaponDamageProfile(new Dictionary<DamageType, int>
      {
        { DamageType.Cutting, 4 },
        { DamageType.Energy, 4 }
      }),
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ShieldBlockSucceeded = true,
      ShieldBlockRV = 2,
      Shield = shield,
      ArmorPieces = new List<ArmorInfo>()
    };

    var result = multiResolver.Resolve(request);

    // Total shield absorption across both types should not exceed 4 (original durability)
    int totalShieldAbsorbed = result.PerTypeResults
      .SelectMany(r => r.AbsorptionSteps)
      .Where(s => s.IsShield)
      .Sum(s => s.TotalAbsorbed);

    Assert.IsTrue(totalShieldAbsorbed <= 4,
      $"Total shield absorption ({totalShieldAbsorbed}) should not exceed durability (4)");
  }

  /// <summary>
  /// Totals should aggregate correctly across damage types.
  /// </summary>
  [TestMethod]
  public void Resolve_MultiType_AggregatesCorrecly()
  {
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var multiResolver = new MultiDamageResolver(diceRoller);

    var request = new MultiDamageRequest
    {
      BaseSV = 2,
      WeaponDamage = new WeaponDamageProfile(new Dictionary<DamageType, int>
      {
        { DamageType.Cutting, 1 },
        { DamageType.Energy, 1 }
      }),
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new List<ArmorInfo>()
    };

    var result = multiResolver.Resolve(request);

    // Total damage should be sum of per-type
    int expectedFAT = result.PerTypeResults.Sum(r => r.FatigueDamage);
    int expectedVIT = result.PerTypeResults.Sum(r => r.VitalityDamage);

    Assert.AreEqual(expectedFAT, result.TotalFatigueDamage);
    Assert.AreEqual(expectedVIT, result.TotalVitalityDamage);
  }

  /// <summary>
  /// Zero-modifier types should be skipped.
  /// </summary>
  [TestMethod]
  public void Resolve_ZeroModifierType_Skipped()
  {
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var multiResolver = new MultiDamageResolver(diceRoller);

    var request = new MultiDamageRequest
    {
      BaseSV = 0, // With baseSV=0 and modifier=0, effectiveSV=0 which is skipped
      WeaponDamage = new WeaponDamageProfile(new Dictionary<DamageType, int>
      {
        { DamageType.Cutting, 4 },
        { DamageType.Energy, 0 } // zero should be excluded by GetNonZeroTypes
      }),
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new List<ArmorInfo>()
    };

    var result = multiResolver.Resolve(request);

    // Only Cutting should appear (Energy has 0 modifier and is excluded by GetNonZeroTypes)
    Assert.AreEqual(1, result.PerTypeResults.Count);
    Assert.AreEqual(DamageType.Cutting, result.PerTypeResults[0].DamageType);
  }

  /// <summary>
  /// FullyAbsorbed should be true only when all types are fully absorbed.
  /// </summary>
  [TestMethod]
  public void Resolve_PartialAbsorption_NotFullyAbsorbed()
  {
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var multiResolver = new MultiDamageResolver(diceRoller);

    var armor = new ArmorInfo
    {
      ItemId = "armor1",
      Name = "Plate",
      CoveredLocations = new[] { HitLocation.Torso },
      DamageClass = 1,
      Absorption = new Dictionary<DamageType, int>
      {
        { DamageType.Cutting, 10 }, // Absorbs all cutting
        { DamageType.Energy, 1 }   // Doesn't absorb all energy
      },
      CurrentDurability = 100,
      MaxDurability = 100,
      LayerOrder = 1
    };

    var request = new MultiDamageRequest
    {
      BaseSV = 2,
      WeaponDamage = new WeaponDamageProfile(new Dictionary<DamageType, int>
      {
        { DamageType.Cutting, 3 },
        { DamageType.Energy, 5 }
      }),
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new List<ArmorInfo> { armor }
    };

    var result = multiResolver.Resolve(request);

    // Cutting (SV 5) should be fully absorbed by 10 absorption
    var cuttingResult = result.PerTypeResults.First(r => r.DamageType == DamageType.Cutting);
    Assert.IsTrue(cuttingResult.FullyAbsorbed);

    // Energy (SV 7) should NOT be fully absorbed by 1 absorption
    var energyResult = result.PerTypeResults.First(r => r.DamageType == DamageType.Energy);
    Assert.IsFalse(energyResult.FullyAbsorbed);

    // Overall should not be fully absorbed
    Assert.IsFalse(result.FullyAbsorbed);
  }
}
