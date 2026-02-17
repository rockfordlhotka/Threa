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

  // --- AP/SvMax tests ---

  [TestMethod]
  public void FromJson_ExtendedFormat_ParsesApAndSvMax()
  {
    var json = "{\"Piercing\": {\"sv\": 3, \"ap\": 5, \"svMax\": 8}}";
    var profile = WeaponDamageProfile.FromJson(json);

    Assert.IsNotNull(profile);
    Assert.AreEqual(3, profile[DamageType.Piercing]);
    Assert.AreEqual(5, profile.GetApOffset(DamageType.Piercing));
    Assert.AreEqual(8, profile.GetSvMax(DamageType.Piercing));
  }

  [TestMethod]
  public void FromJson_MixedFormat_ParsesBothFormats()
  {
    var json = "{\"Piercing\": {\"sv\": 3, \"ap\": 5}, \"Energy\": 2}";
    var profile = WeaponDamageProfile.FromJson(json);

    Assert.IsNotNull(profile);
    Assert.AreEqual(3, profile[DamageType.Piercing]);
    Assert.AreEqual(5, profile.GetApOffset(DamageType.Piercing));
    Assert.IsNull(profile.GetSvMax(DamageType.Piercing));
    Assert.AreEqual(2, profile[DamageType.Energy]);
    Assert.AreEqual(0, profile.GetApOffset(DamageType.Energy));
    Assert.IsNull(profile.GetSvMax(DamageType.Energy));
  }

  [TestMethod]
  public void ToJson_ExtendedFormat_SerializesCorrectly()
  {
    var entries = new Dictionary<DamageType, DamageTypeEntry>
    {
      { DamageType.Piercing, new DamageTypeEntry(3, 5, 8) }
    };
    var profile = new WeaponDamageProfile(entries);

    var json = profile.ToJson();
    var restored = WeaponDamageProfile.FromJson(json);

    Assert.IsNotNull(restored);
    Assert.AreEqual(3, restored[DamageType.Piercing]);
    Assert.AreEqual(5, restored.GetApOffset(DamageType.Piercing));
    Assert.AreEqual(8, restored.GetSvMax(DamageType.Piercing));
  }

  [TestMethod]
  public void ToJson_SimpleEntry_UsesLegacyFormat()
  {
    var profile = WeaponDamageProfile.FromSingle(DamageType.Cutting, 4);
    var json = profile.ToJson();

    // Should use compact int format, not object
    Assert.IsTrue(json.Contains("\"Cutting\":4") || json.Contains("\"Cutting\": 4"));
    Assert.IsFalse(json.Contains("\"sv\""));
  }

  [TestMethod]
  public void GetEntry_MissingType_ReturnsDefault()
  {
    var profile = new WeaponDamageProfile();
    var entry = profile.GetEntry(DamageType.Cutting);

    Assert.AreEqual(0, entry.SvModifier);
    Assert.AreEqual(0, entry.ApOffset);
    Assert.IsNull(entry.SvMax);
  }

  [TestMethod]
  public void MergeWith_ApOffsetsSummed()
  {
    var weapon = new WeaponDamageProfile(new Dictionary<DamageType, DamageTypeEntry>
    {
      { DamageType.Piercing, new DamageTypeEntry(3, 2) }
    });
    var ammo = new WeaponDamageProfile(new Dictionary<DamageType, DamageTypeEntry>
    {
      { DamageType.Piercing, new DamageTypeEntry(1, 3) }
    });

    var merged = weapon.MergeWith(ammo);

    Assert.AreEqual(4, merged[DamageType.Piercing]);
    Assert.AreEqual(5, merged.GetApOffset(DamageType.Piercing));
  }

  [TestMethod]
  public void MergeWith_SvMaxTakesMinimum()
  {
    var weapon = new WeaponDamageProfile(new Dictionary<DamageType, DamageTypeEntry>
    {
      { DamageType.Bashing, new DamageTypeEntry(2, 0, 8) }
    });
    var ammo = new WeaponDamageProfile(new Dictionary<DamageType, DamageTypeEntry>
    {
      { DamageType.Bashing, new DamageTypeEntry(1, 0, 5) }
    });

    var merged = weapon.MergeWith(ammo);

    Assert.AreEqual(5, merged.GetSvMax(DamageType.Bashing));
  }

  [TestMethod]
  public void MergeWith_SvMaxOneNull_TakesNonNull()
  {
    var weapon = new WeaponDamageProfile(new Dictionary<DamageType, DamageTypeEntry>
    {
      { DamageType.Bashing, new DamageTypeEntry(2, 0, null) }
    });
    var ammo = new WeaponDamageProfile(new Dictionary<DamageType, DamageTypeEntry>
    {
      { DamageType.Bashing, new DamageTypeEntry(1, 0, 5) }
    });

    var merged = weapon.MergeWith(ammo);

    Assert.AreEqual(5, merged.GetSvMax(DamageType.Bashing));
  }

  [TestMethod]
  public void ToDisplayString_WithApOffset_ShowsAp()
  {
    var entries = new Dictionary<DamageType, DamageTypeEntry>
    {
      { DamageType.Piercing, new DamageTypeEntry(3, 5) }
    };
    var profile = new WeaponDamageProfile(entries);

    var display = profile.ToDisplayString();

    Assert.IsTrue(display.Contains("Piercing +3"));
    Assert.IsTrue(display.Contains("AP 5"));
  }

  [TestMethod]
  public void ToDisplayString_WithSvMax_ShowsSvMax()
  {
    var entries = new Dictionary<DamageType, DamageTypeEntry>
    {
      { DamageType.Bashing, new DamageTypeEntry(2, 0, 5) }
    };
    var profile = new WeaponDamageProfile(entries);

    var display = profile.ToDisplayString();

    Assert.IsTrue(display.Contains("Bashing +2"));
    Assert.IsTrue(display.Contains("SvMax 5"));
  }
}
