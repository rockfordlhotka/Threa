using System.Collections.Generic;
using System.Linq;
using GameMechanics.Combat;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMechanics.Test;

[TestClass]
public class WeaponDamageProfileTests
{
  [TestMethod]
  public void FromJson_ValidJson_ParsesCorrectly()
  {
    var profile = WeaponDamageProfile.FromJson("{\"Cutting\": 4, \"Energy\": 2}");

    Assert.IsNotNull(profile);
    Assert.AreEqual(4, profile[DamageType.Cutting]);
    Assert.AreEqual(2, profile[DamageType.Energy]);
    Assert.AreEqual(0, profile[DamageType.Bashing]);
  }

  [TestMethod]
  public void FromJson_NullOrEmpty_ReturnsNull()
  {
    Assert.IsNull(WeaponDamageProfile.FromJson(null));
    Assert.IsNull(WeaponDamageProfile.FromJson(""));
    Assert.IsNull(WeaponDamageProfile.FromJson("  "));
  }

  [TestMethod]
  public void FromJson_InvalidJson_ReturnsNull()
  {
    Assert.IsNull(WeaponDamageProfile.FromJson("not json"));
  }

  [TestMethod]
  public void ToJson_RoundTrips()
  {
    var original = new WeaponDamageProfile(new Dictionary<DamageType, int>
    {
      { DamageType.Cutting, 4 },
      { DamageType.Energy, 2 }
    });

    var json = original.ToJson();
    var restored = WeaponDamageProfile.FromJson(json);

    Assert.IsNotNull(restored);
    Assert.AreEqual(4, restored[DamageType.Cutting]);
    Assert.AreEqual(2, restored[DamageType.Energy]);
  }

  [TestMethod]
  public void FromLegacy_ValidType_CreatesProfile()
  {
    var profile = WeaponDamageProfile.FromLegacy("Cutting", 4);

    Assert.IsNotNull(profile);
    Assert.AreEqual(4, profile[DamageType.Cutting]);
    Assert.AreEqual(DamageType.Cutting, profile.PrimaryDamageType);
  }

  [TestMethod]
  public void FromLegacy_NullTypeWithModifier_DefaultsToBashing()
  {
    var profile = WeaponDamageProfile.FromLegacy(null, 3);

    Assert.IsNotNull(profile);
    Assert.AreEqual(3, profile[DamageType.Bashing]);
  }

  [TestMethod]
  public void FromLegacy_NullTypeZeroModifier_ReturnsNull()
  {
    Assert.IsNull(WeaponDamageProfile.FromLegacy(null, 0));
    Assert.IsNull(WeaponDamageProfile.FromLegacy("", 0));
  }

  [TestMethod]
  public void PrimaryDamageType_ReturnsHighestModifier()
  {
    var profile = new WeaponDamageProfile(new Dictionary<DamageType, int>
    {
      { DamageType.Cutting, 4 },
      { DamageType.Energy, 6 },
      { DamageType.Bashing, 1 }
    });

    Assert.AreEqual(DamageType.Energy, profile.PrimaryDamageType);
  }

  [TestMethod]
  public void MergeWith_CombinesModifiers()
  {
    var weapon = new WeaponDamageProfile(new Dictionary<DamageType, int>
    {
      { DamageType.Cutting, 4 }
    });
    var ammo = new WeaponDamageProfile(new Dictionary<DamageType, int>
    {
      { DamageType.Cutting, 1 },
      { DamageType.Energy, 2 }
    });

    var merged = weapon.MergeWith(ammo);

    Assert.AreEqual(5, merged[DamageType.Cutting]);
    Assert.AreEqual(2, merged[DamageType.Energy]);
  }

  [TestMethod]
  public void MergeWith_DoesNotMutateOriginals()
  {
    var weapon = new WeaponDamageProfile(new Dictionary<DamageType, int>
    {
      { DamageType.Cutting, 4 }
    });
    var ammo = new WeaponDamageProfile(new Dictionary<DamageType, int>
    {
      { DamageType.Energy, 2 }
    });

    weapon.MergeWith(ammo);

    Assert.AreEqual(4, weapon[DamageType.Cutting]);
    Assert.AreEqual(0, weapon[DamageType.Energy]);
  }

  [TestMethod]
  public void GetNonZeroTypes_ExcludesZeroes()
  {
    var profile = new WeaponDamageProfile(new Dictionary<DamageType, int>
    {
      { DamageType.Cutting, 4 },
      { DamageType.Energy, 0 },
      { DamageType.Bashing, 2 }
    });

    var nonZero = profile.GetNonZeroTypes().ToList();

    Assert.AreEqual(2, nonZero.Count);
    Assert.IsTrue(nonZero.Any(kv => kv.Key == DamageType.Cutting));
    Assert.IsTrue(nonZero.Any(kv => kv.Key == DamageType.Bashing));
  }

  [TestMethod]
  public void IsMultiType_SingleType_ReturnsFalse()
  {
    var profile = WeaponDamageProfile.FromSingle(DamageType.Cutting, 4);
    Assert.IsFalse(profile.IsMultiType);
  }

  [TestMethod]
  public void IsMultiType_MultipleTypes_ReturnsTrue()
  {
    var profile = new WeaponDamageProfile(new Dictionary<DamageType, int>
    {
      { DamageType.Cutting, 4 },
      { DamageType.Energy, 2 }
    });
    Assert.IsTrue(profile.IsMultiType);
  }

  [TestMethod]
  public void ToDisplayString_FormatsCorrectly()
  {
    var profile = new WeaponDamageProfile(new Dictionary<DamageType, int>
    {
      { DamageType.Cutting, 4 },
      { DamageType.Energy, 2 }
    });

    var display = profile.ToDisplayString();

    Assert.IsTrue(display.Contains("Cutting +4"));
    Assert.IsTrue(display.Contains("Energy +2"));
  }

  [TestMethod]
  public void ToJson_OmitsZeroValues()
  {
    var profile = new WeaponDamageProfile(new Dictionary<DamageType, int>
    {
      { DamageType.Cutting, 4 },
      { DamageType.Energy, 0 }
    });

    var json = profile.ToJson();

    Assert.IsTrue(json.Contains("Cutting"));
    Assert.IsFalse(json.Contains("Energy"));
  }

  [TestMethod]
  public void FromSingle_CreatesCorrectProfile()
  {
    var profile = WeaponDamageProfile.FromSingle(DamageType.Piercing, 3);

    Assert.AreEqual(3, profile[DamageType.Piercing]);
    Assert.AreEqual(DamageType.Piercing, profile.PrimaryDamageType);
    Assert.IsFalse(profile.IsMultiType);
  }
}
