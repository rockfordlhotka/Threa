using Csla;
using GameMechanics.Items;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class SkillwireChipTests : TestBase
{
  protected override void ConfigureAdditionalServices(IServiceCollection services)
  {
    services.AddGameMechanics();
  }

  // Seeded template IDs (from TestDataSeeder)
  private const int SKILLWIRE_TEMPLATE_ID = 204;
  private const int HACKING_CHIP_TEMPLATE_ID = 205;  // Hacking 3, INT
  private const int COMBAT_CHIP_TEMPLATE_ID = 206;   // Pistols 2, DEX
  private const int STEALTH_CHIP_TEMPLATE_ID = 207;  // Stealth 4, DEX

  // -----------------------------------------------------------------------
  // Helper: build a minimal CharacterItem list with an equipped Skillwire
  // and a chip inside it.
  // -----------------------------------------------------------------------
  private static List<CharacterItem> BuildItemsWithChip(
      bool skillwireEquipped,
      string chipPrimaryAttr = "INT",
      string skillName = "Hacking",
      int chipLevel = 3)
  {
    var skillwireId = Guid.NewGuid();
    var chipId = Guid.NewGuid();

    var skillwireTemplate = new ItemTemplate
    {
      Id = 1001,
      Name = "Skillwire",
      ItemType = ItemType.Implant,
      IsContainer = true,
      MaxChipSlots = 4,
      EquipmentSlot = EquipmentSlot.ImplantNeural
    };

    var chipTemplate = new ItemTemplate
    {
      Id = 1002,
      Name = "Test Chip",
      ItemType = ItemType.SkillChip,
      CustomProperties = $$$"""{"chipPrimaryAttribute":"{{{chipPrimaryAttr}}}"}""",
      SkillBonuses =
      [
        new ItemSkillBonus
        {
          SkillName = skillName,
          BonusType = BonusType.GrantSkill,
          BonusValue = chipLevel
        }
      ]
    };

    return
    [
      new CharacterItem
      {
        Id = skillwireId,
        ItemTemplateId = skillwireTemplate.Id,
        Template = skillwireTemplate,
        IsEquipped = skillwireEquipped,
        EquippedSlot = skillwireEquipped ? EquipmentSlot.ImplantNeural : EquipmentSlot.None
      },
      new CharacterItem
      {
        Id = chipId,
        ItemTemplateId = chipTemplate.Id,
        Template = chipTemplate,
        ContainerItemId = skillwireId   // chip is inside the Skillwire
      }
    ];
  }

  // -----------------------------------------------------------------------
  // SetChipItems / GetChipGrantedSkills
  // -----------------------------------------------------------------------

  [TestMethod]
  public void SetChipItems_EquippedSkillwireWithChip_ReturnsGrantedSkill()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var items = BuildItemsWithChip(skillwireEquipped: true, chipPrimaryAttr: "INT",
                                    skillName: "Hacking", chipLevel: 3);
    character.SetChipItems(items);

    var granted = character.GetChipGrantedSkills().ToList();
    Assert.AreEqual(1, granted.Count);
    Assert.AreEqual("Hacking", granted[0].SkillName);
    Assert.AreEqual(3, granted[0].SkillLevel);
    Assert.AreEqual("INT", granted[0].PrimaryAttribute);
  }

  [TestMethod]
  public void SetChipItems_UnequippedSkillwire_ReturnsNoGrantedSkills()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var items = BuildItemsWithChip(skillwireEquipped: false);
    character.SetChipItems(items);

    Assert.AreEqual(0, character.GetChipGrantedSkills().Count());
  }

  [TestMethod]
  public void SetChipItems_NoItems_ReturnsNoGrantedSkills()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    character.SetChipItems([]);

    Assert.AreEqual(0, character.GetChipGrantedSkills().Count());
  }

  [TestMethod]
  public void SetChipItems_MultipleChips_ReturnsAllGrantedSkills()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var skillwireId = Guid.NewGuid();
    var skillwireTemplate = new ItemTemplate
    {
      Id = 1001,
      Name = "Skillwire",
      ItemType = ItemType.Implant,
      IsContainer = true,
      MaxChipSlots = 4
    };
    var hackTemplate = new ItemTemplate
    {
      Id = 1002,
      Name = "Hacking Chip",
      ItemType = ItemType.SkillChip,
      CustomProperties = """{"chipPrimaryAttribute":"INT"}""",
      SkillBonuses = [new ItemSkillBonus { SkillName = "Hacking", BonusType = BonusType.GrantSkill, BonusValue = 3 }]
    };
    var stealthTemplate = new ItemTemplate
    {
      Id = 1003,
      Name = "Stealth Chip",
      ItemType = ItemType.SkillChip,
      CustomProperties = """{"chipPrimaryAttribute":"DEX"}""",
      SkillBonuses = [new ItemSkillBonus { SkillName = "Stealth", BonusType = BonusType.GrantSkill, BonusValue = 4 }]
    };

    var items = new List<CharacterItem>
    {
      new() { Id = skillwireId, ItemTemplateId = 1001, Template = skillwireTemplate, IsEquipped = true, EquippedSlot = EquipmentSlot.ImplantNeural },
      new() { Id = Guid.NewGuid(), ItemTemplateId = 1002, Template = hackTemplate, ContainerItemId = skillwireId },
      new() { Id = Guid.NewGuid(), ItemTemplateId = 1003, Template = stealthTemplate, ContainerItemId = skillwireId }
    };

    character.SetChipItems(items);

    var granted = character.GetChipGrantedSkills().OrderBy(g => g.SkillName).ToList();
    Assert.AreEqual(2, granted.Count);
    Assert.AreEqual("Hacking", granted[0].SkillName);
    Assert.AreEqual("Stealth", granted[1].SkillName);
  }

  [TestMethod]
  public void ClearChipItems_AfterSet_ReturnsNoGrantedSkills()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    character.SetChipItems(BuildItemsWithChip(skillwireEquipped: true));
    Assert.AreEqual(1, character.GetChipGrantedSkills().Count());

    character.ClearChipItems();

    Assert.AreEqual(0, character.GetChipGrantedSkills().Count());
  }

  // -----------------------------------------------------------------------
  // GetChipSkillLevelBonus
  // -----------------------------------------------------------------------

  [TestMethod]
  public void GetChipSkillLevelBonus_ChipHigherThanNative_ReturnsDifference()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    // Chip grants Hacking 3, native Bonus (level 1) = -1 (SkillCost.GetBonus(1))
    character.SetChipItems(BuildItemsWithChip(skillwireEquipped: true, skillName: "Hacking", chipLevel: 3));

    // nativeSkillLevel = 1, which maps to Bonus = -1
    // chipLevel = 3, which maps to Bonus = -1 too... let me think
    // Actually GetChipSkillLevelBonus compares chipLevel (integer) to nativeSkillLevel (Bonus int passed in)
    // The Bonus for level 1 = -1. chipLevel = 3 (raw integer passed in GrantSkill).
    // best = 3, nativeSkillLevel = -1, so 3 > -1, return 3 - (-1) = 4
    var bonus = character.GetChipSkillLevelBonus("Hacking", nativeSkillLevel: -1);
    Assert.AreEqual(4, bonus, "Chip level 3 vs native Bonus -1 should give +4");
  }

  [TestMethod]
  public void GetChipSkillLevelBonus_NativeHigherThanChip_ReturnsZero()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    // Chip level 3, native Bonus = 5 (skill level 8)
    character.SetChipItems(BuildItemsWithChip(skillwireEquipped: true, skillName: "Hacking", chipLevel: 3));

    var bonus = character.GetChipSkillLevelBonus("Hacking", nativeSkillLevel: 5);
    Assert.AreEqual(0, bonus, "Chip should not reduce native skill");
  }

  [TestMethod]
  public void GetChipSkillLevelBonus_ChipEqualToNative_ReturnsZero()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    character.SetChipItems(BuildItemsWithChip(skillwireEquipped: true, skillName: "Hacking", chipLevel: 3));

    var bonus = character.GetChipSkillLevelBonus("Hacking", nativeSkillLevel: 3);
    Assert.AreEqual(0, bonus, "Equal chip level should give no bonus");
  }

  [TestMethod]
  public void GetChipSkillLevelBonus_UnknownSkill_ReturnsZero()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    character.SetChipItems(BuildItemsWithChip(skillwireEquipped: true, skillName: "Hacking", chipLevel: 3));

    var bonus = character.GetChipSkillLevelBonus("Stealth", nativeSkillLevel: 0);
    Assert.AreEqual(0, bonus, "Chip for different skill should give no bonus");
  }

  [TestMethod]
  public void GetChipSkillLevelBonus_NullChips_ReturnsZero()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);
    // SetChipItems never called — _chipGrantedSkills is null

    var bonus = character.GetChipSkillLevelBonus("Hacking", nativeSkillLevel: 0);
    Assert.AreEqual(0, bonus, "Before SetChipItems is called, bonus should be 0");
  }

  [TestMethod]
  public void GetChipSkillLevelBonus_EmptyChipList_ReturnsZeroForLowLevelSkill()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);
    // SetChipItems called with no items — _chipGrantedSkills = [] (empty, not null)
    character.SetChipItems([]);

    // Level 2 skill has Bonus = -3; without this fix DefaultIfEmpty(0) returns 0,
    // and 0 > -3 would incorrectly produce a chip bonus of 3.
    var bonus = character.GetChipSkillLevelBonus("Awareness", nativeSkillLevel: -3);
    Assert.AreEqual(0, bonus, "Empty chip list should not grant bonus for low-level skills");
  }

  [TestMethod]
  public void GetChipSkillLevelBonus_SkillNameCaseInsensitive_ReturnsBonus()
  {
    var provider = InitServices();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    character.SetChipItems(BuildItemsWithChip(skillwireEquipped: true, skillName: "Hacking", chipLevel: 3));

    var bonus = character.GetChipSkillLevelBonus("hacking", nativeSkillLevel: 0);
    Assert.AreEqual(3, bonus, "Skill name comparison should be case-insensitive");
  }

  // -----------------------------------------------------------------------
  // ItemManagementService — Skillwire container validation
  // -----------------------------------------------------------------------

  [TestMethod]
  public async Task MoveToContainer_NonChipIntoSkillwire_Fails()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var itemDal = provider.GetRequiredService<ICharacterItemDal>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    // Add an equipped Skillwire
    var skillwireItem = new CharacterItem
    {
      Id = Guid.NewGuid(),
      ItemTemplateId = SKILLWIRE_TEMPLATE_ID,
      OwnerCharacterId = character.Id,
      IsEquipped = true,
      EquippedSlot = EquipmentSlot.ImplantNeural
    };
    await itemDal.AddItemAsync(skillwireItem);

    // Add a non-chip item (sword template id=1)
    var sword = new CharacterItem
    {
      Id = Guid.NewGuid(),
      ItemTemplateId = 1,
      OwnerCharacterId = character.Id,
      StackSize = 1
    };
    await itemDal.AddItemAsync(sword);

    var result = await service.MoveToContainerAsync(character, sword.Id, skillwireItem.Id);

    Assert.IsFalse(result.Success);
    Assert.IsTrue(result.ErrorMessage!.Contains("skill chips"), $"Expected skill chips message, got: {result.ErrorMessage}");
  }

  [TestMethod]
  public async Task MoveToContainer_ChipIntoSkillwireWithSpace_Succeeds()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var itemDal = provider.GetRequiredService<ICharacterItemDal>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var skillwireItem = new CharacterItem
    {
      Id = Guid.NewGuid(),
      ItemTemplateId = SKILLWIRE_TEMPLATE_ID,
      OwnerCharacterId = character.Id,
      IsEquipped = true,
      EquippedSlot = EquipmentSlot.ImplantNeural
    };
    await itemDal.AddItemAsync(skillwireItem);

    var chip = new CharacterItem
    {
      Id = Guid.NewGuid(),
      ItemTemplateId = HACKING_CHIP_TEMPLATE_ID,
      OwnerCharacterId = character.Id,
      StackSize = 1
    };
    await itemDal.AddItemAsync(chip);

    var result = await service.MoveToContainerAsync(character, chip.Id, skillwireItem.Id);

    Assert.IsTrue(result.Success, $"Expected success, got: {result.ErrorMessage}");
  }

  [TestMethod]
  public async Task MoveToContainer_ChipIntoFullSkillwire_Fails()
  {
    var provider = InitServices();
    var service = provider.GetRequiredService<ItemManagementService>();
    var itemDal = provider.GetRequiredService<ICharacterItemDal>();
    var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
    var character = dp.Create(42);

    var skillwireItem = new CharacterItem
    {
      Id = Guid.NewGuid(),
      ItemTemplateId = SKILLWIRE_TEMPLATE_ID,
      OwnerCharacterId = character.Id,
      IsEquipped = true,
      EquippedSlot = EquipmentSlot.ImplantNeural
    };
    await itemDal.AddItemAsync(skillwireItem);

    // Fill all 4 slots with chips
    for (int i = 0; i < 4; i++)
    {
      var existingChip = new CharacterItem
      {
        Id = Guid.NewGuid(),
        ItemTemplateId = HACKING_CHIP_TEMPLATE_ID,
        OwnerCharacterId = character.Id,
        ContainerItemId = skillwireItem.Id
      };
      await itemDal.AddItemAsync(existingChip);
    }

    // Try to add a 5th chip
    var extraChip = new CharacterItem
    {
      Id = Guid.NewGuid(),
      ItemTemplateId = COMBAT_CHIP_TEMPLATE_ID,
      OwnerCharacterId = character.Id,
      StackSize = 1
    };
    await itemDal.AddItemAsync(extraChip);

    var result = await service.MoveToContainerAsync(character, extraChip.Id, skillwireItem.Id);

    Assert.IsFalse(result.Success);
    Assert.IsTrue(result.ErrorMessage!.Contains("full"), $"Expected full message, got: {result.ErrorMessage}");
  }
}
