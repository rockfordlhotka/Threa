using Csla;
using GameMechanics.Combat;
using GameMechanics.Effects;
using GameMechanics.Items;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class ImplantTests : TestBase
{
  protected override void ConfigureAdditionalServices(IServiceCollection services)
  {
    services.AddGameMechanics();
  }

  #region Always-On Implant Effects

  [TestMethod]
  public async Task EquippingAlwaysOnImplant_AppliesWhileEquippedEffect()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var item = new CharacterItem { Id = Guid.NewGuid() };
    var template = new ItemTemplate
    {
      Id = 200,
      Name = "Subdermal Armor",
      ItemType = ItemType.Implant,
      EquipmentSlot = EquipmentSlot.ImplantSubdermal,
      Effects =
      [
        new ItemEffectDefinition
        {
          EffectType = EffectType.Buff,
          Name = "Subdermal Armor",
          Trigger = ItemEffectTrigger.WhileEquipped,
          IsActive = true,
          IsToggleable = false
        }
      ]
    };

    var result = await service.OnItemEquippedAsync(c, item, template);

    Assert.IsTrue(result.Success);
    Assert.AreEqual(1, result.AppliedEffects.Count);
    Assert.AreEqual("Subdermal Armor", result.AppliedEffects[0].Name);
  }

  [TestMethod]
  public async Task UnequippingAlwaysOnImplant_RemovesEffect()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var item = new CharacterItem { Id = Guid.NewGuid() };
    var template = new ItemTemplate
    {
      Id = 200,
      Name = "Subdermal Armor",
      ItemType = ItemType.Implant,
      Effects =
      [
        new ItemEffectDefinition
        {
          EffectType = EffectType.Buff,
          Name = "Subdermal Armor",
          Trigger = ItemEffectTrigger.WhileEquipped,
          IsActive = true,
          IsToggleable = false
        }
      ]
    };

    await service.OnItemEquippedAsync(c, item, template);
    Assert.AreEqual(1, c.Effects.Count);

    var unequipResult = service.OnItemUnequipped(c, item.Id);
    Assert.IsTrue(unequipResult.Success);
    Assert.AreEqual(0, c.Effects.Count);
  }

  #endregion

  #region Toggleable Implant Effects

  [TestMethod]
  public async Task EquippingToggleableImplant_DoesNotAutoApplyEffect()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var item = new CharacterItem { Id = Guid.NewGuid() };
    var template = new ItemTemplate
    {
      Id = 201,
      Name = "Wired Reflexes",
      ItemType = ItemType.Implant,
      Effects =
      [
        new ItemEffectDefinition
        {
          EffectType = EffectType.Buff,
          Name = "Wired Reflexes",
          Trigger = ItemEffectTrigger.WhileEquipped,
          IsActive = true,
          IsToggleable = true,
          ToggleApCost = 1
        }
      ]
    };

    var result = await service.OnItemEquippedAsync(c, item, template);

    Assert.IsTrue(result.Success);
    Assert.AreEqual(0, result.AppliedEffects.Count, "Toggleable effects should not auto-apply on equip");
    Assert.AreEqual(0, c.Effects.Count);
  }

  [TestMethod]
  public async Task ToggleOn_AppliesEffect_ToggleOff_RemovesEffect()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();
    var effectDef = new ItemEffectDefinition
    {
      EffectType = EffectType.Buff,
      Name = "Wired Reflexes",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsActive = true,
      IsToggleable = true,
      ToggleApCost = 0
    };

    // Toggle ON
    var onResult = await service.ToggleImplantEffectOnAsync(c, itemId, effectDef);
    Assert.IsTrue(onResult.Success);
    Assert.AreEqual(1, onResult.AppliedEffects.Count);
    Assert.AreEqual(1, c.Effects.Count);
    Assert.AreEqual("Wired Reflexes", c.Effects[0].Name);

    // Toggle OFF
    var offResult = service.ToggleImplantEffectOff(c, itemId, effectDef);
    Assert.IsTrue(offResult.Success);
    Assert.AreEqual(1, offResult.RemovedEffectIds.Count);
    Assert.AreEqual(0, c.Effects.Count);
  }

  [TestMethod]
  public async Task ToggleOn_WithApCost_DeductsAP()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    int initialAP = c.ActionPoints.Available;
    var itemId = Guid.NewGuid();
    var effectDef = new ItemEffectDefinition
    {
      EffectType = EffectType.Buff,
      Name = "Wired Reflexes",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsActive = true,
      IsToggleable = true,
      ToggleApCost = 1
    };

    var result = await service.ToggleImplantEffectOnAsync(c, itemId, effectDef);

    Assert.IsTrue(result.Success);
    Assert.AreEqual(initialAP - 1, c.ActionPoints.Available);
  }

  [TestMethod]
  public async Task ToggleOn_InsufficientAP_Fails()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    // Drain all AP
    c.ActionPoints.Available = 0;

    var itemId = Guid.NewGuid();
    var effectDef = new ItemEffectDefinition
    {
      EffectType = EffectType.Buff,
      Name = "Wired Reflexes",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsActive = true,
      IsToggleable = true,
      ToggleApCost = 1
    };

    var result = await service.ToggleImplantEffectOnAsync(c, itemId, effectDef);

    Assert.IsFalse(result.Success);
    Assert.IsTrue(result.ErrorMessage!.Contains("Insufficient AP"));
    Assert.AreEqual(0, c.Effects.Count);
  }

  [TestMethod]
  public async Task ToggleOn_AlreadyActive_Fails()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();
    var effectDef = new ItemEffectDefinition
    {
      EffectType = EffectType.Buff,
      Name = "Wired Reflexes",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsActive = true,
      IsToggleable = true,
      ToggleApCost = 0
    };

    await service.ToggleImplantEffectOnAsync(c, itemId, effectDef);

    var secondResult = await service.ToggleImplantEffectOnAsync(c, itemId, effectDef);
    Assert.IsFalse(secondResult.Success);
    Assert.IsTrue(secondResult.ErrorMessage!.Contains("already active"));
  }

  [TestMethod]
  public void ToggleOff_NotActive_Fails()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();
    var effectDef = new ItemEffectDefinition
    {
      EffectType = EffectType.Buff,
      Name = "Wired Reflexes",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsActive = true,
      IsToggleable = true,
      ToggleApCost = 0
    };

    var result = service.ToggleImplantEffectOff(c, itemId, effectDef);
    Assert.IsFalse(result.Success);
    Assert.IsTrue(result.ErrorMessage!.Contains("not currently active"));
  }

  [TestMethod]
  public void ToggleOff_NonToggleable_Fails()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();
    var effectDef = new ItemEffectDefinition
    {
      EffectType = EffectType.Buff,
      Name = "Subdermal Armor",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsActive = true,
      IsToggleable = false
    };

    var result = service.ToggleImplantEffectOff(c, itemId, effectDef);
    Assert.IsFalse(result.Success);
    Assert.IsTrue(result.ErrorMessage!.Contains("not toggleable"));
  }

  #endregion

  #region Implant Weapons via WeaponSelector

  [TestMethod]
  public void WeaponSelector_GetMeleeWeapons_FindsImplantMeleeWeapon()
  {
    var items = new List<EquippedItemInfo>
    {
      new(
        new CharacterItem { Id = Guid.NewGuid(), EquippedSlot = EquipmentSlot.ImplantHandRight },
        new ItemTemplate
        {
          Id = 202,
          Name = "Retractable Blade",
          ItemType = ItemType.Implant,
          WeaponType = WeaponType.Dagger,
          DamageClass = 2,
          DamageType = "Cutting"
        })
    };

    var meleeWeapons = WeaponSelector.GetMeleeWeapons(items).ToList();

    Assert.AreEqual(1, meleeWeapons.Count);
    Assert.AreEqual("Retractable Blade", meleeWeapons[0].Template.Name);
  }

  [TestMethod]
  public void WeaponSelector_GetRangedWeapons_FindsImplantRangedWeapon()
  {
    var items = new List<EquippedItemInfo>
    {
      new(
        new CharacterItem { Id = Guid.NewGuid(), EquippedSlot = EquipmentSlot.ImplantArmRight },
        new ItemTemplate
        {
          Id = 210,
          Name = "Built-in Laser",
          ItemType = ItemType.Implant,
          WeaponType = WeaponType.Pistol,
          Range = 30,
          DamageClass = 2,
          DamageType = "Energy"
        })
    };

    var rangedWeapons = WeaponSelector.GetRangedWeapons(items).ToList();

    Assert.AreEqual(1, rangedWeapons.Count);
    Assert.AreEqual("Built-in Laser", rangedWeapons[0].Template.Name);
  }

  [TestMethod]
  public void WeaponSelector_GetMeleeWeapons_ExcludesImplantRangedWeapon()
  {
    var items = new List<EquippedItemInfo>
    {
      new(
        new CharacterItem { Id = Guid.NewGuid(), EquippedSlot = EquipmentSlot.ImplantArmRight },
        new ItemTemplate
        {
          Id = 210,
          Name = "Built-in Laser",
          ItemType = ItemType.Implant,
          WeaponType = WeaponType.Pistol,
          Range = 30
        })
    };

    var meleeWeapons = WeaponSelector.GetMeleeWeapons(items).ToList();

    Assert.AreEqual(0, meleeWeapons.Count, "Ranged implant weapon should not appear in melee list");
  }

  [TestMethod]
  public void WeaponSelector_GetMeleeWeapons_IgnoresImplantWithNoWeaponType()
  {
    var items = new List<EquippedItemInfo>
    {
      new(
        new CharacterItem { Id = Guid.NewGuid(), EquippedSlot = EquipmentSlot.ImplantSpine },
        new ItemTemplate
        {
          Id = 201,
          Name = "Wired Reflexes",
          ItemType = ItemType.Implant,
          WeaponType = WeaponType.None
        })
    };

    var meleeWeapons = WeaponSelector.GetMeleeWeapons(items).ToList();
    var rangedWeapons = WeaponSelector.GetRangedWeapons(items).ToList();

    Assert.AreEqual(0, meleeWeapons.Count, "Non-weapon implant should not appear in melee list");
    Assert.AreEqual(0, rangedWeapons.Count, "Non-weapon implant should not appear in ranged list");
  }

  [TestMethod]
  public void ImplantWeapon_Modifiers_ApplyCorrectly()
  {
    var template = new ItemTemplate
    {
      Id = 202,
      Name = "Retractable Blade",
      ItemType = ItemType.Implant,
      WeaponType = WeaponType.Dagger,
      DamageClass = 2,
      DamageType = "Cutting",
      SVModifier = 1,
      AVModifier = -1
    };

    Assert.AreEqual(2, template.DamageClass);
    Assert.AreEqual("Cutting", template.DamageType);
    Assert.AreEqual(1, template.SVModifier);
    Assert.AreEqual(-1, template.AVModifier);
  }

  #endregion
}
