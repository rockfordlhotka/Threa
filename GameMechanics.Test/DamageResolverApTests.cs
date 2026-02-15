using System.Collections.Generic;
using System.Linq;
using GameMechanics;
using GameMechanics.Combat;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMechanics.Test;

[TestClass]
public class DamageResolverApTests
{
  private static ArmorInfo CreateArmor(int absorption, DamageType type = DamageType.Piercing, int durability = 100)
  {
    return new ArmorInfo
    {
      ItemId = "armor1",
      Name = "Test Armor",
      CoveredLocations = new[] { HitLocation.Torso },
      DamageClass = 1,
      Absorption = new Dictionary<DamageType, int> { { type, absorption } },
      CurrentDurability = durability,
      MaxDurability = durability,
      LayerOrder = 1
    };
  }

  private static ShieldInfo CreateShield(int absorption, DamageType type = DamageType.Piercing, int durability = 100)
  {
    return new ShieldInfo
    {
      ItemId = "shield1",
      Name = "Test Shield",
      DamageClass = 1,
      Absorption = new Dictionary<DamageType, int> { { type, absorption } },
      CurrentDurability = durability,
      MaxDurability = durability
    };
  }

  // --- AP Offset Tests ---

  [TestMethod]
  public void ApOffset_ReducesArmorAbsorption()
  {
    // SV 10, armor absorbs 12, AP 5 → armor reduced to 7, penetrating = 3
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var resolver = new DamageResolver(diceRoller);

    var request = new DamageRequest
    {
      IncomingSV = 10,
      DamageType = DamageType.Piercing,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8, // AS 8 + roll 0 = 8, TV 8, RV 0 → bonus 0
      ArmorPieces = new List<ArmorInfo> { CreateArmor(12) },
      ApOffset = 5
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(7, result.TotalAbsorbed);
    Assert.AreEqual(3, result.PenetratingSV);
  }

  [TestMethod]
  public void ApOffset_CannotReduceBelowZero()
  {
    // Armor absorbs 3, AP 10 → reduced to 0, all SV penetrates
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var resolver = new DamageResolver(diceRoller);

    var request = new DamageRequest
    {
      IncomingSV = 5,
      DamageType = DamageType.Piercing,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new List<ArmorInfo> { CreateArmor(3) },
      ApOffset = 10
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(0, result.TotalAbsorbed);
    Assert.AreEqual(5, result.PenetratingSV);
  }

  [TestMethod]
  public void ApOffset_AppliesBeforeDCScaling()
  {
    // Armor DC 2 vs Attack DC 1: armor gets 10x bonus
    // Armor raw absorption = 6, AP = 4 → reduced to 2, then scaled 2*10 = 20
    // Without AP: 6*10 = 60
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var resolver = new DamageResolver(diceRoller);

    var armor = new ArmorInfo
    {
      ItemId = "armor1",
      Name = "Heavy Plate",
      CoveredLocations = new[] { HitLocation.Torso },
      DamageClass = 2,
      Absorption = new Dictionary<DamageType, int> { { DamageType.Piercing, 6 } },
      CurrentDurability = 100,
      MaxDurability = 100,
      LayerOrder = 1
    };

    var request = new DamageRequest
    {
      IncomingSV = 10,
      DamageType = DamageType.Piercing,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new List<ArmorInfo> { armor },
      ApOffset = 4
    };

    var result = resolver.Resolve(request);

    // (6 - 4) * 10 = 20, absorbs all 10 SV
    Assert.AreEqual(10, result.TotalAbsorbed);
    Assert.AreEqual(0, result.PenetratingSV);
  }

  [TestMethod]
  public void ApOffset_WorksOnShields()
  {
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var resolver = new DamageResolver(diceRoller);

    var request = new DamageRequest
    {
      IncomingSV = 10,
      DamageType = DamageType.Piercing,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ShieldBlockSucceeded = true,
      ShieldBlockRV = 0,
      Shield = CreateShield(8),
      ArmorPieces = new List<ArmorInfo>(),
      ApOffset = 5
    };

    var result = resolver.Resolve(request);

    // Shield base 8, AP -5 → 3 absorbed by shield, 7 penetrates
    var shieldStep = result.AbsorptionSteps.First(s => s.IsShield);
    Assert.AreEqual(3, shieldStep.TotalAbsorbed);
    Assert.AreEqual(5, shieldStep.ApOffsetApplied);
  }

  // --- SV Max Tests ---

  [TestMethod]
  public void SvMax_CapsWhenArmorExceedsThreshold()
  {
    // SV 10, armor absorbs 8, svMax 5 → armor (8) > svMax (5), so remaining = 0
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var resolver = new DamageResolver(diceRoller);

    var request = new DamageRequest
    {
      IncomingSV = 10,
      DamageType = DamageType.Piercing,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new List<ArmorInfo> { CreateArmor(8) },
      SvMax = 5
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(0, result.PenetratingSV);
    var armorStep = result.AbsorptionSteps.First();
    Assert.IsTrue(armorStep.SvMaxTriggered);
    Assert.AreEqual(5, armorStep.SvMaxApplied);
  }

  [TestMethod]
  public void SvMax_NormalWhenArmorBelowThreshold()
  {
    // SV 10, armor absorbs 3, svMax 5 → armor (3) < svMax (5), normal processing
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var resolver = new DamageResolver(diceRoller);

    var request = new DamageRequest
    {
      IncomingSV = 10,
      DamageType = DamageType.Piercing,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new List<ArmorInfo> { CreateArmor(3) },
      SvMax = 5
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(7, result.PenetratingSV);
    var armorStep = result.AbsorptionSteps.First();
    Assert.IsFalse(armorStep.SvMaxTriggered);
  }

  // --- Combined AP + SvMax Tests ---

  [TestMethod]
  public void ApAndSvMax_Combined()
  {
    // SV 10, armor raw 12, AP 4 → reduced to 8, svMax 5 → 8 > 5 → capped, remaining = 0
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var resolver = new DamageResolver(diceRoller);

    var request = new DamageRequest
    {
      IncomingSV = 10,
      DamageType = DamageType.Piercing,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new List<ArmorInfo> { CreateArmor(12) },
      ApOffset = 4,
      SvMax = 5
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(0, result.PenetratingSV);
    var armorStep = result.AbsorptionSteps.First();
    Assert.IsTrue(armorStep.SvMaxTriggered);
    Assert.AreEqual(4, armorStep.ApOffsetApplied);
  }

  [TestMethod]
  public void ApAndSvMax_ApReducesBelowSvMax_NormalProcessing()
  {
    // SV 10, armor raw 8, AP 5 → reduced to 3, svMax 5 → 3 < 5 → normal, penetrating = 7
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var resolver = new DamageResolver(diceRoller);

    var request = new DamageRequest
    {
      IncomingSV = 10,
      DamageType = DamageType.Piercing,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new List<ArmorInfo> { CreateArmor(8) },
      ApOffset = 5,
      SvMax = 5
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(7, result.PenetratingSV);
    var armorStep = result.AbsorptionSteps.First();
    Assert.IsFalse(armorStep.SvMaxTriggered);
  }

  // --- Multi-Damage-Type with AP/SvMax ---

  [TestMethod]
  public void MultiDamage_PerTypeApAndSvMax()
  {
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var multiResolver = new MultiDamageResolver(diceRoller);

    var armor = new ArmorInfo
    {
      ItemId = "armor1",
      Name = "Composite Armor",
      CoveredLocations = new[] { HitLocation.Torso },
      DamageClass = 1,
      Absorption = new Dictionary<DamageType, int>
      {
        { DamageType.Piercing, 10 },
        { DamageType.Energy, 8 }
      },
      CurrentDurability = 200,
      MaxDurability = 200,
      LayerOrder = 1
    };

    // Piercing: sv=3, ap=5 → armor 10-5=5, effective SV = 5+3=8, 8-5=3 penetrating
    // Energy: sv=2, svMax=3 → armor 8 > svMax 3, capped, 0 penetrating
    var entries = new Dictionary<DamageType, DamageTypeEntry>
    {
      { DamageType.Piercing, new DamageTypeEntry(3, ApOffset: 5) },
      { DamageType.Energy, new DamageTypeEntry(2, SvMax: 3) }
    };
    var profile = new WeaponDamageProfile(entries);

    var request = new MultiDamageRequest
    {
      BaseSV = 5,
      WeaponDamage = profile,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new List<ArmorInfo> { armor }
    };

    var result = multiResolver.Resolve(request);

    Assert.AreEqual(2, result.PerTypeResults.Count);

    var piercingResult = result.PerTypeResults.First(r => r.DamageType == DamageType.Piercing);
    Assert.AreEqual(3, piercingResult.PenetratingSV);

    var energyResult = result.PerTypeResults.First(r => r.DamageType == DamageType.Energy);
    Assert.AreEqual(0, energyResult.PenetratingSV);
  }

  // --- No AP/SvMax (backwards compat) ---

  [TestMethod]
  public void NoApNoSvMax_BehavesNormally()
  {
    var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(0);
    var resolver = new DamageResolver(diceRoller);

    var request = new DamageRequest
    {
      IncomingSV = 10,
      DamageType = DamageType.Piercing,
      DamageClass = 1,
      HitLocation = HitLocation.Torso,
      DefenderArmorAS = 8,
      ArmorPieces = new List<ArmorInfo> { CreateArmor(4) }
    };

    var result = resolver.Resolve(request);

    Assert.AreEqual(4, result.TotalAbsorbed);
    Assert.AreEqual(6, result.PenetratingSV);
    var armorStep = result.AbsorptionSteps.First();
    Assert.AreEqual(0, armorStep.ApOffsetApplied);
    Assert.IsNull(armorStep.SvMaxApplied);
    Assert.IsFalse(armorStep.SvMaxTriggered);
  }
}
