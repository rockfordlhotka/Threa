using Csla;
using Csla.Configuration;
using GameMechanics.Effects;
using GameMechanics.Effects.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class ItemEffectTests
{
  private ServiceProvider InitServices()
  {
    IServiceCollection services = new ServiceCollection();
    services.AddCsla();
    services.AddMockDb();
    services.AddGameMechanics();
    return services.BuildServiceProvider();
  }

  #region ItemEffectState Tests

  [TestMethod]
  public void ItemEffectState_Serialize_RoundTrips()
  {
    var state = new ItemEffectState
    {
      EffectName = "Strength Boost",
      ItemName = "Belt of Giants",
      Description = "+3 to STR",
      TickIntervalRounds = 10,
      RoundsUntilTick = 5,
      IsRevealed = true,
      IdentifyDifficulty = 8,
      RemovalDifficulty = 12,
      Modifiers =
      [
        new BuffModifier
        {
          Type = BuffModifierType.Attribute,
          Target = "STR",
          Value = 3
        }
      ]
    };

    var json = state.Serialize();
    var restored = ItemEffectState.Deserialize(json);

    Assert.AreEqual(state.EffectName, restored.EffectName);
    Assert.AreEqual(state.ItemName, restored.ItemName);
    Assert.AreEqual(state.Description, restored.Description);
    Assert.AreEqual(state.TickIntervalRounds, restored.TickIntervalRounds);
    Assert.AreEqual(state.RoundsUntilTick, restored.RoundsUntilTick);
    Assert.AreEqual(state.IsRevealed, restored.IsRevealed);
    Assert.AreEqual(state.IdentifyDifficulty, restored.IdentifyDifficulty);
    Assert.AreEqual(state.RemovalDifficulty, restored.RemovalDifficulty);
    Assert.AreEqual(1, restored.Modifiers.Count);
    Assert.AreEqual(BuffModifierType.Attribute, restored.Modifiers[0].Type);
    Assert.AreEqual("STR", restored.Modifiers[0].Target);
    Assert.AreEqual(3, restored.Modifiers[0].Value);
  }

  [TestMethod]
  public void ItemEffectState_Deserialize_HandlesNull()
  {
    var restored = ItemEffectState.Deserialize(null);

    Assert.IsNotNull(restored);
    Assert.AreEqual("Item Effect", restored.EffectName);
  }

  [TestMethod]
  public void ItemEffectState_Deserialize_HandlesEmpty()
  {
    var restored = ItemEffectState.Deserialize("");

    Assert.IsNotNull(restored);
    Assert.AreEqual("Item Effect", restored.EffectName);
  }

  [TestMethod]
  public void ItemEffectState_CreateAttributeModifier_CreatesCorrectState()
  {
    var state = ItemEffectState.CreateAttributeModifier("Magic Ring", "Strength Boost", "STR", 2);

    Assert.AreEqual("Magic Ring", state.ItemName);
    Assert.AreEqual("Strength Boost", state.EffectName);
    Assert.AreEqual("+2 to STR", state.Description);
    Assert.AreEqual(1, state.Modifiers.Count);
    Assert.AreEqual(BuffModifierType.Attribute, state.Modifiers[0].Type);
    Assert.AreEqual("STR", state.Modifiers[0].Target);
    Assert.AreEqual(2, state.Modifiers[0].Value);
  }

  [TestMethod]
  public void ItemEffectState_CreateHealingOverTime_CreatesCorrectState()
  {
    var state = ItemEffectState.CreateHealingOverTime("Ring of Healing", "Regeneration", "FAT", 1, 10);

    Assert.AreEqual("Ring of Healing", state.ItemName);
    Assert.AreEqual("Regeneration", state.EffectName);
    Assert.AreEqual("Heals 1 FAT every 10 rounds", state.Description);
    Assert.AreEqual(10, state.TickIntervalRounds);
    Assert.AreEqual(10, state.RoundsUntilTick);
    Assert.AreEqual(1, state.Modifiers.Count);
    Assert.AreEqual(BuffModifierType.HealingOverTime, state.Modifiers[0].Type);
  }

  [TestMethod]
  public void ItemEffectState_CreateDamageOverTime_CreatesCorrectState()
  {
    var state = ItemEffectState.CreateDamageOverTime("Cursed Ring", "Life Drain", "VIT", 1, 20, true);

    Assert.AreEqual("Cursed Ring", state.ItemName);
    Assert.AreEqual("Life Drain", state.EffectName);
    Assert.IsFalse(state.IsRevealed); // Cursed effects start hidden
    Assert.AreEqual(1, state.Modifiers.Count);
    Assert.AreEqual(-1, state.Modifiers[0].Value); // Negative for damage
  }

  #endregion

  #region EffectRecord Item Properties Tests

  [TestMethod]
  public void EffectRecord_CreateFromItem_SetsItemProperties()
  {
    var provider = InitServices();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();

    var itemId = Guid.NewGuid();
    var definition = new ItemEffectDefinition
    {
      Id = 1,
      ItemTemplateId = 100,
      EffectType = EffectType.ItemEffect,
      Name = "Strength Boost",
      Description = "+2 to STR",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsCursed = false,
      BehaviorState = ItemEffectState.CreateAttributeModifier("Belt", "Strength", "STR", 2).Serialize()
    };

    var effect = effectPortal.CreateChild(definition, itemId);

    Assert.AreEqual(itemId, effect.SourceItemId);
    Assert.AreEqual(ItemEffectTrigger.WhileEquipped, effect.ItemEffectTrigger);
    Assert.IsFalse(effect.IsCursed);
    Assert.AreEqual("Strength Boost", effect.Name);
    Assert.IsTrue(effect.IsFromItem);
  }

  [TestMethod]
  public void EffectRecord_IsFromItem_TrueWhenHasSourceItemId()
  {
    var provider = InitServices();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();

    var itemId = Guid.NewGuid();
    var definition = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Test Effect",
      Trigger = ItemEffectTrigger.WhileEquipped
    };

    var effect = effectPortal.CreateChild(definition, itemId);

    Assert.IsTrue(effect.IsFromItem);
  }

  [TestMethod]
  public void EffectRecord_IsFromItem_FalseWhenNoSourceItemId()
  {
    var provider = InitServices();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();

    var effect = effectPortal.CreateChild(
      EffectType.Buff,
      "Spell Buff",
      null,
      10,
      null);

    Assert.IsFalse(effect.IsFromItem);
  }

  [TestMethod]
  public void EffectRecord_IsBlockingUnequip_TrueForCursedEquipEffect()
  {
    var provider = InitServices();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();

    var itemId = Guid.NewGuid();
    var definition = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Curse",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsCursed = true
    };

    var effect = effectPortal.CreateChild(definition, itemId);

    Assert.IsTrue(effect.IsBlockingUnequip);
    Assert.IsFalse(effect.IsBlockingDrop);
  }

  [TestMethod]
  public void EffectRecord_IsBlockingDrop_TrueForCursedPossessionEffect()
  {
    var provider = InitServices();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();

    var itemId = Guid.NewGuid();
    var definition = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Curse",
      Trigger = ItemEffectTrigger.WhilePossessed,
      IsCursed = true
    };

    var effect = effectPortal.CreateChild(definition, itemId);

    Assert.IsFalse(effect.IsBlockingUnequip);
    Assert.IsTrue(effect.IsBlockingDrop);
  }

  [TestMethod]
  public void EffectRecord_IsBlockingDrop_TrueForCursedOnPickupEffect()
  {
    var provider = InitServices();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();

    var itemId = Guid.NewGuid();
    var definition = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Curse",
      Trigger = ItemEffectTrigger.OnPickup,
      IsCursed = true
    };

    var effect = effectPortal.CreateChild(definition, itemId);

    Assert.IsFalse(effect.IsBlockingUnequip);
    Assert.IsTrue(effect.IsBlockingDrop);
  }

  [TestMethod]
  public void EffectRecord_IsBlockingItemRemoval_CombinesUnequipAndDrop()
  {
    var provider = InitServices();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();

    var itemId = Guid.NewGuid();

    // Equip curse
    var equipDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Equip Curse",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsCursed = true
    };
    var equipEffect = effectPortal.CreateChild(equipDef, itemId);
    Assert.IsTrue(equipEffect.IsBlockingItemRemoval);

    // Possession curse
    var possessDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Possess Curse",
      Trigger = ItemEffectTrigger.WhilePossessed,
      IsCursed = true
    };
    var possessEffect = effectPortal.CreateChild(possessDef, itemId);
    Assert.IsTrue(possessEffect.IsBlockingItemRemoval);

    // Non-cursed effect
    var normalDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Normal Buff",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsCursed = false
    };
    var normalEffect = effectPortal.CreateChild(normalDef, itemId);
    Assert.IsFalse(normalEffect.IsBlockingItemRemoval);
  }

  #endregion

  #region EffectList Item Effect Management Tests

  [TestMethod]
  public void EffectList_GetEffectsFromItem_ReturnsCorrectEffects()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var itemId1 = Guid.NewGuid();
    var itemId2 = Guid.NewGuid();

    var def1 = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Effect 1",
      Trigger = ItemEffectTrigger.WhileEquipped
    };
    var def2 = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Effect 2",
      Trigger = ItemEffectTrigger.WhileEquipped
    };

    var effect1 = effectPortal.CreateChild(def1, itemId1);
    var effect2 = effectPortal.CreateChild(def2, itemId2);

    c.Effects.AddEffect(effect1);
    c.Effects.AddEffect(effect2);

    var item1Effects = c.Effects.GetEffectsFromItem(itemId1).ToList();
    Assert.AreEqual(1, item1Effects.Count);
    Assert.AreEqual("Effect 1", item1Effects[0].Name);
  }

  [TestMethod]
  public void EffectList_RemoveEquipEffects_OnlyRemovesEquipTrigger()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();

    var equipDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Equip Effect",
      Trigger = ItemEffectTrigger.WhileEquipped
    };
    var possessDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Possess Effect",
      Trigger = ItemEffectTrigger.WhilePossessed
    };

    c.Effects.AddEffect(effectPortal.CreateChild(equipDef, itemId));
    c.Effects.AddEffect(effectPortal.CreateChild(possessDef, itemId));

    Assert.AreEqual(2, c.Effects.Count);

    c.Effects.RemoveEquipEffects(itemId);

    Assert.AreEqual(1, c.Effects.Count);
    Assert.AreEqual("Possess Effect", c.Effects[0].Name);
  }

  [TestMethod]
  public void EffectList_RemovePossessionEffects_RemovesAllItemEffects()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();

    var equipDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Equip Effect",
      Trigger = ItemEffectTrigger.WhileEquipped
    };
    var possessDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Possess Effect",
      Trigger = ItemEffectTrigger.WhilePossessed
    };

    c.Effects.AddEffect(effectPortal.CreateChild(equipDef, itemId));
    c.Effects.AddEffect(effectPortal.CreateChild(possessDef, itemId));

    Assert.AreEqual(2, c.Effects.Count);

    c.Effects.RemovePossessionEffects(itemId);

    Assert.AreEqual(0, c.Effects.Count);
  }

  [TestMethod]
  public void EffectList_IsItemBlockingUnequip_DetectsCurse()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();
    var cursedDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Curse",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsCursed = true
    };

    Assert.IsFalse(c.Effects.IsItemBlockingUnequip(itemId));

    c.Effects.AddEffect(effectPortal.CreateChild(cursedDef, itemId));

    Assert.IsTrue(c.Effects.IsItemBlockingUnequip(itemId));
  }

  [TestMethod]
  public void EffectList_IsItemBlockingDrop_DetectsCurse()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();
    var cursedDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Curse",
      Trigger = ItemEffectTrigger.WhilePossessed,
      IsCursed = true
    };

    Assert.IsFalse(c.Effects.IsItemBlockingDrop(itemId));

    c.Effects.AddEffect(effectPortal.CreateChild(cursedDef, itemId));

    Assert.IsTrue(c.Effects.IsItemBlockingDrop(itemId));
  }

  [TestMethod]
  public void EffectList_CanUnequipItem_FalseWhenCursed()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();
    var cursedDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Curse",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsCursed = true
    };

    Assert.IsTrue(c.Effects.CanUnequipItem(itemId));

    c.Effects.AddEffect(effectPortal.CreateChild(cursedDef, itemId));

    Assert.IsFalse(c.Effects.CanUnequipItem(itemId));
  }

  [TestMethod]
  public void EffectList_CanDropItem_FalseWhenCursed()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();
    var cursedDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Curse",
      Trigger = ItemEffectTrigger.WhilePossessed,
      IsCursed = true
    };

    Assert.IsTrue(c.Effects.CanDropItem(itemId));

    c.Effects.AddEffect(effectPortal.CreateChild(cursedDef, itemId));

    Assert.IsFalse(c.Effects.CanDropItem(itemId));
  }

  #endregion

  #region ItemEffectService Tests

  [TestMethod]
  public void ItemEffectService_CanUnequipItem_AllowedWhenNotCursed()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();
    var result = service.CanUnequipItem(c, itemId);

    Assert.IsTrue(result.IsAllowed);
    Assert.IsNull(result.BlockReason);
  }

  [TestMethod]
  public void ItemEffectService_CanUnequipItem_BlockedWhenCursed()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();
    var cursedDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Dark Binding",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsCursed = true
    };
    c.Effects.AddEffect(effectPortal.CreateChild(cursedDef, itemId));

    var result = service.CanUnequipItem(c, itemId);

    Assert.IsFalse(result.IsAllowed);
    Assert.IsNotNull(result.BlockReason);
    Assert.IsTrue(result.BlockReason!.Contains("Dark Binding"));
    Assert.AreEqual(1, result.BlockingEffects.Count);
  }

  [TestMethod]
  public void ItemEffectService_CanDropItem_BlockedWhenCursed()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();
    var cursedDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Bound Soul",
      Trigger = ItemEffectTrigger.WhilePossessed,
      IsCursed = true
    };
    c.Effects.AddEffect(effectPortal.CreateChild(cursedDef, itemId));

    var result = service.CanDropItem(c, itemId);

    Assert.IsFalse(result.IsAllowed);
    Assert.IsTrue(result.BlockReason!.Contains("Bound Soul"));
  }

  [TestMethod]
  public async Task ItemEffectService_OnItemPickedUpAsync_AppliesPossessionEffects()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var item = new CharacterItem { Id = Guid.NewGuid() };
    var template = new ItemTemplate
    {
      Id = 1,
      Name = "Cursed Idol",
      Effects =
      [
        new ItemEffectDefinition
        {
          EffectType = EffectType.ItemEffect,
          Name = "Haunting",
          Trigger = ItemEffectTrigger.WhilePossessed,
          IsActive = true
        },
        new ItemEffectDefinition
        {
          EffectType = EffectType.ItemEffect,
          Name = "Strength Boost",
          Trigger = ItemEffectTrigger.WhileEquipped, // Should NOT be applied
          IsActive = true
        }
      ]
    };

    var result = await service.OnItemPickedUpAsync(c, item, template);

    Assert.IsTrue(result.Success);
    Assert.AreEqual(1, result.AppliedEffects.Count);
    Assert.AreEqual("Haunting", result.AppliedEffects[0].Name);
  }

  [TestMethod]
  public async Task ItemEffectService_OnItemEquippedAsync_AppliesEquipEffects()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var item = new CharacterItem { Id = Guid.NewGuid() };
    var template = new ItemTemplate
    {
      Id = 1,
      Name = "Magic Ring",
      Effects =
      [
        new ItemEffectDefinition
        {
          EffectType = EffectType.ItemEffect,
          Name = "Protection",
          Trigger = ItemEffectTrigger.WhileEquipped,
          IsActive = true
        },
        new ItemEffectDefinition
        {
          EffectType = EffectType.ItemEffect,
          Name = "Aura",
          Trigger = ItemEffectTrigger.WhilePossessed, // Should NOT be applied here
          IsActive = true
        }
      ]
    };

    var result = await service.OnItemEquippedAsync(c, item, template);

    Assert.IsTrue(result.Success);
    Assert.AreEqual(1, result.AppliedEffects.Count);
    Assert.AreEqual("Protection", result.AppliedEffects[0].Name);
  }

  [TestMethod]
  public async Task ItemEffectService_OnItemAcquiredAndEquippedAsync_AppliesAllRelevantEffects()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var item = new CharacterItem { Id = Guid.NewGuid() };
    var template = new ItemTemplate
    {
      Id = 1,
      Name = "Magic Ring",
      Effects =
      [
        new ItemEffectDefinition
        {
          EffectType = EffectType.ItemEffect,
          Name = "Protection",
          Trigger = ItemEffectTrigger.WhileEquipped,
          IsActive = true
        },
        new ItemEffectDefinition
        {
          EffectType = EffectType.ItemEffect,
          Name = "Aura",
          Trigger = ItemEffectTrigger.WhilePossessed,
          IsActive = true
        },
        new ItemEffectDefinition
        {
          EffectType = EffectType.ItemEffect,
          Name = "On Attack",
          Trigger = ItemEffectTrigger.OnAttackWith, // Should NOT be applied
          IsActive = true
        }
      ]
    };

    var result = await service.OnItemAcquiredAndEquippedAsync(c, item, template);

    Assert.IsTrue(result.Success);
    Assert.AreEqual(2, result.AppliedEffects.Count);
  }

  [TestMethod]
  public void ItemEffectService_OnItemUnequipped_RemovesEquipEffectsOnly()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();

    // Add both equip and possession effects
    var equipDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Equip Buff",
      Trigger = ItemEffectTrigger.WhileEquipped
    };
    var possessDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Possess Buff",
      Trigger = ItemEffectTrigger.WhilePossessed
    };

    c.Effects.AddEffect(effectPortal.CreateChild(equipDef, itemId));
    c.Effects.AddEffect(effectPortal.CreateChild(possessDef, itemId));

    Assert.AreEqual(2, c.Effects.Count);

    var result = service.OnItemUnequipped(c, itemId);

    Assert.IsTrue(result.Success);
    Assert.AreEqual(1, result.RemovedEffectIds.Count);
    Assert.AreEqual(1, c.Effects.Count);
    Assert.AreEqual("Possess Buff", c.Effects[0].Name);
  }

  [TestMethod]
  public void ItemEffectService_OnItemUnequipped_FailsWhenCursed()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();
    var cursedDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Curse",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsCursed = true
    };

    c.Effects.AddEffect(effectPortal.CreateChild(cursedDef, itemId));

    var result = service.OnItemUnequipped(c, itemId);

    Assert.IsFalse(result.Success);
    Assert.IsNotNull(result.ErrorMessage);
    Assert.AreEqual(1, c.Effects.Count); // Effect should still be there
  }

  [TestMethod]
  public void ItemEffectService_OnItemDropped_RemovesAllEffects()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();

    var equipDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Equip Buff",
      Trigger = ItemEffectTrigger.WhileEquipped
    };
    var possessDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Possess Buff",
      Trigger = ItemEffectTrigger.WhilePossessed
    };

    c.Effects.AddEffect(effectPortal.CreateChild(equipDef, itemId));
    c.Effects.AddEffect(effectPortal.CreateChild(possessDef, itemId));

    var result = service.OnItemDropped(c, itemId);

    Assert.IsTrue(result.Success);
    Assert.AreEqual(2, result.RemovedEffectIds.Count);
    Assert.AreEqual(0, c.Effects.Count);
  }

  [TestMethod]
  public void ItemEffectService_OnItemDropped_FailsWhenPossessionCursed()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();
    var cursedDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Bound",
      Trigger = ItemEffectTrigger.WhilePossessed,
      IsCursed = true
    };

    c.Effects.AddEffect(effectPortal.CreateChild(cursedDef, itemId));

    var result = service.OnItemDropped(c, itemId);

    Assert.IsFalse(result.Success);
    Assert.AreEqual(1, c.Effects.Count);
  }

  [TestMethod]
  public void ItemEffectService_RemoveCurseFromItem_RemovesCurseEffects()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();
    var cursedDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Curse",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsCursed = true,
      BehaviorState = new ItemEffectState { RemovalDifficulty = 10 }.Serialize()
    };

    c.Effects.AddEffect(effectPortal.CreateChild(cursedDef, itemId));

    Assert.IsFalse(c.Effects.CanUnequipItem(itemId));

    // Remove with sufficient power
    var result = service.RemoveCurseFromItem(c, itemId, 15);

    Assert.IsTrue(result.Success);
    Assert.AreEqual(1, result.RemovedEffectIds.Count);
    Assert.IsTrue(c.Effects.CanUnequipItem(itemId));
  }

  [TestMethod]
  public void ItemEffectService_RemoveCurseFromItem_FailsWhenPowerInsufficient()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var itemId = Guid.NewGuid();
    var cursedDef = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Powerful Curse",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsCursed = true,
      BehaviorState = new ItemEffectState { RemovalDifficulty = 20 }.Serialize()
    };

    c.Effects.AddEffect(effectPortal.CreateChild(cursedDef, itemId));

    // Try to remove with insufficient power
    var result = service.RemoveCurseFromItem(c, itemId, 10);

    Assert.IsFalse(result.Success);
    Assert.AreEqual("Curse is too powerful to remove.", result.ErrorMessage);
    Assert.IsFalse(c.Effects.CanUnequipItem(itemId));
  }

  [TestMethod]
  public void ItemEffectService_GetAllCursedEffects_ReturnsAllCurses()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
    var service = provider.GetRequiredService<ItemEffectService>();
    var c = dp.Create(42);

    var item1 = Guid.NewGuid();
    var item2 = Guid.NewGuid();

    var curse1 = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Curse 1",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsCursed = true
    };
    var curse2 = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Curse 2",
      Trigger = ItemEffectTrigger.WhilePossessed,
      IsCursed = true
    };
    var normal = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Normal",
      Trigger = ItemEffectTrigger.WhileEquipped,
      IsCursed = false
    };

    c.Effects.AddEffect(effectPortal.CreateChild(curse1, item1));
    c.Effects.AddEffect(effectPortal.CreateChild(curse2, item2));
    c.Effects.AddEffect(effectPortal.CreateChild(normal, item1));

    var curses = service.GetAllCursedEffects(c).ToList();

    Assert.AreEqual(2, curses.Count);
  }

  #endregion

  #region ItemEffectBehavior Tests

  [TestMethod]
  public void ItemEffectBehavior_GetAttributeModifiers_ReturnsCorrectModifiers()
  {
    var provider = InitServices();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();

    var state = ItemEffectState.CreateAttributeModifier("Belt of Giants", "Strength Boost", "STR", 3);
    var definition = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Strength Boost",
      Trigger = ItemEffectTrigger.WhileEquipped,
      BehaviorState = state.Serialize()
    };

    var effect = effectPortal.CreateChild(definition, Guid.NewGuid());
    var behavior = new ItemEffectBehavior();

    var modifiers = behavior.GetAttributeModifiers(effect, "STR", 10).ToList();

    Assert.AreEqual(1, modifiers.Count);
    Assert.AreEqual(3, modifiers[0].Value);
    Assert.IsTrue(modifiers[0].Description.Contains("Belt of Giants"));
  }

  [TestMethod]
  public void ItemEffectBehavior_GetAttributeModifiers_IgnoresOtherAttributes()
  {
    var provider = InitServices();
    var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();

    var state = ItemEffectState.CreateAttributeModifier("Belt of Giants", "Strength Boost", "STR", 3);
    var definition = new ItemEffectDefinition
    {
      EffectType = EffectType.ItemEffect,
      Name = "Strength Boost",
      Trigger = ItemEffectTrigger.WhileEquipped,
      BehaviorState = state.Serialize()
    };

    var effect = effectPortal.CreateChild(definition, Guid.NewGuid());
    var behavior = new ItemEffectBehavior();

    var modifiers = behavior.GetAttributeModifiers(effect, "DEX", 10).ToList();

    Assert.AreEqual(0, modifiers.Count);
  }

  #endregion

  #region EquipmentSlot Extension Tests

  [TestMethod]
  [DataRow(EquipmentSlot.ImplantNeural, true)]
  [DataRow(EquipmentSlot.ImplantCardiac, true)]
  [DataRow(EquipmentSlot.ImplantArmLeft, true)]
  [DataRow(EquipmentSlot.Head, false)]
  [DataRow(EquipmentSlot.MainHand, false)]
  [DataRow(EquipmentSlot.FingerLeft1, false)]
  public void EquipmentSlot_IsImplant_CorrectlyIdentifiesImplants(EquipmentSlot slot, bool expected)
  {
    Assert.AreEqual(expected, slot.IsImplant());
  }

  [TestMethod]
  [DataRow(EquipmentSlot.FingerLeft1, true)]
  [DataRow(EquipmentSlot.FingerRight5, true)]
  [DataRow(EquipmentSlot.MainHand, false)]
  [DataRow(EquipmentSlot.Head, false)]
  public void EquipmentSlot_IsFingerSlot_CorrectlyIdentifiesFingers(EquipmentSlot slot, bool expected)
  {
    Assert.AreEqual(expected, slot.IsFingerSlot());
  }

  [TestMethod]
  [DataRow(EquipmentSlot.MainHand, true)]
  [DataRow(EquipmentSlot.OffHand, true)]
  [DataRow(EquipmentSlot.TwoHand, true)]
  [DataRow(EquipmentSlot.Head, false)]
  [DataRow(EquipmentSlot.FingerLeft1, false)]
  public void EquipmentSlot_IsWeaponSlot_CorrectlyIdentifiesWeapons(EquipmentSlot slot, bool expected)
  {
    Assert.AreEqual(expected, slot.IsWeaponSlot());
  }

  [TestMethod]
  public void EquipmentSlot_GetDisplayName_ReturnsReadableName()
  {
    Assert.AreEqual("Neural Implant", EquipmentSlot.ImplantNeural.GetDisplayName());
    Assert.AreEqual("Left Ring Finger", EquipmentSlot.FingerLeft4.GetDisplayName());
    Assert.AreEqual("Main Hand", EquipmentSlot.MainHand.GetDisplayName());
  }

  #endregion
}
