using GameMechanics.Items;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class ItemBonusCalculatorTests
{
    private ItemBonusCalculator _calculator = null!;

    [TestInitialize]
    public void Setup()
    {
        _calculator = new ItemBonusCalculator();
    }

    #region Helper Methods

    private EquippedItemInfo CreateEquippedItem(string name, List<ItemAttributeModifier>? attributeModifiers = null, List<ItemSkillBonus>? skillBonuses = null)
    {
        var template = new ItemTemplate
        {
            Id = 1,
            Name = name,
            ItemType = ItemType.Armor,
            AttributeModifiers = attributeModifiers ?? [],
            SkillBonuses = skillBonuses ?? []
        };

        var characterItem = new CharacterItem
        {
            Id = Guid.NewGuid(),
            ItemTemplateId = template.Id,
            IsEquipped = true,
            EquippedSlot = EquipmentSlot.Chest,
            Template = template
        };

        return new EquippedItemInfo(characterItem, template);
    }

    private ItemAttributeModifier CreateAttributeModifier(string attributeName, decimal value, BonusType bonusType = BonusType.FlatBonus)
    {
        return new ItemAttributeModifier
        {
            AttributeName = attributeName,
            ModifierValue = value,
            ModifierType = bonusType
        };
    }

    private ItemSkillBonus CreateSkillBonus(string skillName, decimal value, BonusType bonusType = BonusType.FlatBonus)
    {
        return new ItemSkillBonus
        {
            SkillName = skillName,
            BonusValue = value,
            BonusType = bonusType
        };
    }

    #endregion

    #region GetAttributeBonus Tests

    [TestMethod]
    public void GetAttributeBonus_NoItems_ReturnsZero()
    {
        // Arrange
        var emptyItems = new List<EquippedItemInfo>();

        // Act
        var result = _calculator.GetAttributeBonus(emptyItems, "STR");

        // Assert
        Assert.AreEqual(0, result, "Empty collection should return 0");
    }

    [TestMethod]
    public void GetAttributeBonus_SingleItemWithBonus_ReturnsBonus()
    {
        // Arrange
        var items = new List<EquippedItemInfo>
        {
            CreateEquippedItem("Gauntlets of Strength", [CreateAttributeModifier("STR", 2)])
        };

        // Act
        var result = _calculator.GetAttributeBonus(items, "STR");

        // Assert
        Assert.AreEqual(2, result, "Should return +2 STR from single item");
    }

    [TestMethod]
    public void GetAttributeBonus_MultipleItems_SumsAdditively()
    {
        // Arrange
        var items = new List<EquippedItemInfo>
        {
            CreateEquippedItem("Gauntlets of Strength", [CreateAttributeModifier("STR", 2)]),
            CreateEquippedItem("Belt of Might", [CreateAttributeModifier("STR", 2)])
        };

        // Act
        var result = _calculator.GetAttributeBonus(items, "STR");

        // Assert
        Assert.AreEqual(4, result, "Two +2 STR items should sum to +4");
    }

    [TestMethod]
    public void GetAttributeBonus_MixedPositiveNegative_SumsAlgebraically()
    {
        // Arrange
        var items = new List<EquippedItemInfo>
        {
            CreateEquippedItem("Gauntlets of Strength", [CreateAttributeModifier("STR", 3)]),
            CreateEquippedItem("Cursed Bracers", [CreateAttributeModifier("STR", -1)])
        };

        // Act
        var result = _calculator.GetAttributeBonus(items, "STR");

        // Assert
        Assert.AreEqual(2, result, "+3 and -1 should sum to +2");
    }

    [TestMethod]
    public void GetAttributeBonus_PercentageBonus_IsIgnored()
    {
        // Arrange
        var items = new List<EquippedItemInfo>
        {
            CreateEquippedItem("Percentage Item", [CreateAttributeModifier("STR", 10, BonusType.PercentageBonus)])
        };

        // Act
        var result = _calculator.GetAttributeBonus(items, "STR");

        // Assert
        Assert.AreEqual(0, result, "Percentage bonuses should be ignored");
    }

    [TestMethod]
    public void GetAttributeBonus_WrongAttribute_ReturnsZero()
    {
        // Arrange
        var items = new List<EquippedItemInfo>
        {
            CreateEquippedItem("Gauntlets of Strength", [CreateAttributeModifier("STR", 2)])
        };

        // Act
        var result = _calculator.GetAttributeBonus(items, "DEX");

        // Assert
        Assert.AreEqual(0, result, "STR bonus should not affect DEX query");
    }

    [TestMethod]
    public void GetAttributeBonus_CaseInsensitive_MatchesBonus()
    {
        // Arrange
        var items = new List<EquippedItemInfo>
        {
            CreateEquippedItem("Gauntlets of Strength", [CreateAttributeModifier("STR", 2)])
        };

        // Act
        var result = _calculator.GetAttributeBonus(items, "str");

        // Assert
        Assert.AreEqual(2, result, "Should match 'str' to 'STR' case-insensitively");
    }

    [TestMethod]
    public void GetAttributeBonus_NullCollection_ReturnsZero()
    {
        // Act
        var result = _calculator.GetAttributeBonus(null!, "STR");

        // Assert
        Assert.AreEqual(0, result, "Null collection should return 0");
    }

    #endregion

    #region GetSkillBonus Tests

    [TestMethod]
    public void GetSkillBonus_NoItems_ReturnsZero()
    {
        // Arrange
        var emptyItems = new List<EquippedItemInfo>();

        // Act
        var result = _calculator.GetSkillBonus(emptyItems, "Swords");

        // Assert
        Assert.AreEqual(0, result, "Empty collection should return 0");
    }

    [TestMethod]
    public void GetSkillBonus_SingleItemWithBonus_ReturnsBonus()
    {
        // Arrange
        var items = new List<EquippedItemInfo>
        {
            CreateEquippedItem("Fencing Gloves", skillBonuses: [CreateSkillBonus("Swords", 1)])
        };

        // Act
        var result = _calculator.GetSkillBonus(items, "Swords");

        // Assert
        Assert.AreEqual(1, result, "Should return +1 Swords from single item");
    }

    [TestMethod]
    public void GetSkillBonus_MultipleItems_SumsAdditively()
    {
        // Arrange
        var items = new List<EquippedItemInfo>
        {
            CreateEquippedItem("Fencing Gloves", skillBonuses: [CreateSkillBonus("Swords", 1)]),
            CreateEquippedItem("Swordmaster Belt", skillBonuses: [CreateSkillBonus("Swords", 2)])
        };

        // Act
        var result = _calculator.GetSkillBonus(items, "Swords");

        // Assert
        Assert.AreEqual(3, result, "+1 and +2 Swords should sum to +3");
    }

    [TestMethod]
    public void GetSkillBonus_PercentageBonus_IsIgnored()
    {
        // Arrange
        var items = new List<EquippedItemInfo>
        {
            CreateEquippedItem("Percentage Item", skillBonuses: [CreateSkillBonus("Swords", 10, BonusType.PercentageBonus)])
        };

        // Act
        var result = _calculator.GetSkillBonus(items, "Swords");

        // Assert
        Assert.AreEqual(0, result, "Percentage bonuses should be ignored");
    }

    #endregion

    #region GetAttributeBonusBreakdown Tests

    [TestMethod]
    public void GetAttributeBonusBreakdown_ReturnsPerItemBreakdown()
    {
        // Arrange
        var items = new List<EquippedItemInfo>
        {
            CreateEquippedItem("Gauntlets of Strength", [CreateAttributeModifier("STR", 2)]),
            CreateEquippedItem("Belt of Might", [CreateAttributeModifier("STR", 3)])
        };

        // Act
        var breakdown = _calculator.GetAttributeBonusBreakdown(items, "STR");

        // Assert
        Assert.AreEqual(2, breakdown.Count, "Should return 2 items in breakdown");
        Assert.IsTrue(breakdown.Any(b => b.ItemName == "Gauntlets of Strength" && b.Bonus == 2),
            "Should include Gauntlets with +2");
        Assert.IsTrue(breakdown.Any(b => b.ItemName == "Belt of Might" && b.Bonus == 3),
            "Should include Belt with +3");
    }

    [TestMethod]
    public void GetAttributeBonusBreakdown_ExcludesZeroBonus()
    {
        // Arrange
        var items = new List<EquippedItemInfo>
        {
            CreateEquippedItem("STR Item", [CreateAttributeModifier("STR", 2)]),
            CreateEquippedItem("DEX Item", [CreateAttributeModifier("DEX", 2)]) // No STR bonus
        };

        // Act
        var breakdown = _calculator.GetAttributeBonusBreakdown(items, "STR");

        // Assert
        Assert.AreEqual(1, breakdown.Count, "Should only include items with STR bonus");
        Assert.AreEqual("STR Item", breakdown[0].ItemName, "Should only include STR Item");
    }

    [TestMethod]
    public void GetAttributeBonusBreakdown_IncludesNegativeBonuses()
    {
        // Arrange
        var items = new List<EquippedItemInfo>
        {
            CreateEquippedItem("Cursed Bracers", [CreateAttributeModifier("STR", -1)])
        };

        // Act
        var breakdown = _calculator.GetAttributeBonusBreakdown(items, "STR");

        // Assert
        Assert.AreEqual(1, breakdown.Count, "Should include items with negative bonus");
        Assert.AreEqual(-1, breakdown[0].Bonus, "Should show -1 bonus");
    }

    #endregion

    #region GetSkillBonusBreakdown Tests

    [TestMethod]
    public void GetSkillBonusBreakdown_ReturnsPerItemBreakdown()
    {
        // Arrange
        var items = new List<EquippedItemInfo>
        {
            CreateEquippedItem("Fencing Gloves", skillBonuses: [CreateSkillBonus("Swords", 1)]),
            CreateEquippedItem("Swordmaster Belt", skillBonuses: [CreateSkillBonus("Swords", 2)])
        };

        // Act
        var breakdown = _calculator.GetSkillBonusBreakdown(items, "Swords");

        // Assert
        Assert.AreEqual(2, breakdown.Count, "Should return 2 items in breakdown");
        Assert.IsTrue(breakdown.Any(b => b.ItemName == "Fencing Gloves" && b.Bonus == 1),
            "Should include Fencing Gloves with +1");
        Assert.IsTrue(breakdown.Any(b => b.ItemName == "Swordmaster Belt" && b.Bonus == 2),
            "Should include Swordmaster Belt with +2");
    }

    #endregion

    #region Multi-stat Item Tests

    [TestMethod]
    public void GetAttributeBonus_MultiStatItem_ReturnsCorrectBonus()
    {
        // Arrange - single item grants both STR and DEX
        var items = new List<EquippedItemInfo>
        {
            CreateEquippedItem("Ring of Prowess", [
                CreateAttributeModifier("STR", 2),
                CreateAttributeModifier("DEX", 3)
            ])
        };

        // Act
        var strBonus = _calculator.GetAttributeBonus(items, "STR");
        var dexBonus = _calculator.GetAttributeBonus(items, "DEX");

        // Assert
        Assert.AreEqual(2, strBonus, "Should return +2 STR");
        Assert.AreEqual(3, dexBonus, "Should return +3 DEX");
    }

    #endregion
}
