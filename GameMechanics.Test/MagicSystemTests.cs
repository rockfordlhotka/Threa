using System.Threading.Tasks;
using GameMechanics;
using GameMechanics.Magic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Threa.Dal;
using Threa.Dal.Dto;
using Threa.Dal.MockDb;

namespace GameMechanics.Test;

/// <summary>
/// Tests for the magic and mana system.
/// </summary>
[TestClass]
public class MagicSystemTests
{
    #region Mana Pool Tests

    [TestMethod]
    public async Task GetManaPool_ReturnsNull_WhenPoolDoesNotExist()
    {
        // Arrange
        var dal = new ManaDal();
        var diceRoller = new DeterministicDiceRoller();
        var manager = new ManaManager(dal, diceRoller);

        // Act
        var pool = await manager.GetManaPoolAsync(999, MagicSchool.Fire);

        // Assert
        Assert.IsNull(pool);
    }

    [TestMethod]
    public async Task EnsureManaPool_CreatesNewPool()
    {
        // Arrange
        var dal = new ManaDal();
        var diceRoller = new DeterministicDiceRoller();
        var manager = new ManaManager(dal, diceRoller);
        int characterId = 1;
        int skillLevel = 3;

        // Act
        var pool = await manager.EnsureManaPoolAsync(
            characterId, MagicSchool.Fire, "fire-mana", skillLevel);

        // Assert
        Assert.IsNotNull(pool);
        Assert.AreEqual(characterId, pool.CharacterId);
        Assert.AreEqual(MagicSchool.Fire, pool.MagicSchool);
        Assert.AreEqual(skillLevel, pool.CurrentMana); // Starts full
        Assert.AreEqual("fire-mana", pool.ManaSkillId);
    }

    [TestMethod]
    public async Task HasSufficientMana_ReturnsFalse_WhenNoPool()
    {
        // Arrange
        var dal = new ManaDal();
        var diceRoller = new DeterministicDiceRoller();
        var manager = new ManaManager(dal, diceRoller);

        // Act
        var hasMana = await manager.HasSufficientManaAsync(999, MagicSchool.Fire, 1);

        // Assert
        Assert.IsFalse(hasMana);
    }

    [TestMethod]
    public async Task HasSufficientMana_ReturnsTrue_WhenEnoughMana()
    {
        // Arrange
        var dal = new ManaDal();
        var diceRoller = new DeterministicDiceRoller();
        var manager = new ManaManager(dal, diceRoller);
        int characterId = 1;
        int skillLevel = 5;

        await manager.EnsureManaPoolAsync(characterId, MagicSchool.Fire, "fire-mana", skillLevel);

        // Act
        var hasMana = await manager.HasSufficientManaAsync(characterId, MagicSchool.Fire, 3);

        // Assert
        Assert.IsTrue(hasMana);
    }

    [TestMethod]
    public async Task HasSufficientMana_ReturnsFalse_WhenNotEnoughMana()
    {
        // Arrange
        var dal = new ManaDal();
        var diceRoller = new DeterministicDiceRoller();
        var manager = new ManaManager(dal, diceRoller);
        int characterId = 1;
        int skillLevel = 2;

        await manager.EnsureManaPoolAsync(characterId, MagicSchool.Fire, "fire-mana", skillLevel);

        // Act
        var hasMana = await manager.HasSufficientManaAsync(characterId, MagicSchool.Fire, 5);

        // Assert
        Assert.IsFalse(hasMana);
    }

    #endregion

    #region Mana Spending Tests

    [TestMethod]
    public async Task SpendMana_ReducesManaPool()
    {
        // Arrange
        var dal = new ManaDal();
        var diceRoller = new DeterministicDiceRoller();
        var manager = new ManaManager(dal, diceRoller);
        int characterId = 1;
        int skillLevel = 5;

        await manager.EnsureManaPoolAsync(characterId, MagicSchool.Fire, "fire-mana", skillLevel);

        // Act
        var result = await manager.SpendManaAsync(characterId, MagicSchool.Fire, 2);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.ManaSpent);
        Assert.AreEqual(3, result.CurrentMana);
    }

    [TestMethod]
    public async Task SpendMana_Fails_WhenInsufficientMana()
    {
        // Arrange
        var dal = new ManaDal();
        var diceRoller = new DeterministicDiceRoller();
        var manager = new ManaManager(dal, diceRoller);
        int characterId = 1;
        int skillLevel = 2;

        await manager.EnsureManaPoolAsync(characterId, MagicSchool.Fire, "fire-mana", skillLevel);

        // Act
        var result = await manager.SpendManaAsync(characterId, MagicSchool.Fire, 5);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.AreEqual(0, result.ManaSpent);
        Assert.IsTrue(result.ErrorMessage!.Contains("Insufficient"));
    }

    [TestMethod]
    public async Task SpendMana_Fails_WhenNoPool()
    {
        // Arrange
        var dal = new ManaDal();
        var diceRoller = new DeterministicDiceRoller();
        var manager = new ManaManager(dal, diceRoller);

        // Act
        var result = await manager.SpendManaAsync(999, MagicSchool.Fire, 1);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.ErrorMessage!.Contains("no mana pool"));
    }

    #endregion

    #region Mana Recovery Action Tests

    [TestMethod]
    public async Task AttemptManaRecovery_RecoversMana_OnSuccess()
    {
        // Arrange
        var dal = new ManaDal();
        // Roll +4 on 4dF+ for a good success (AS 3 + Roll 4 = 7, TV 6, SV = 1)
        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(4);
        var manager = new ManaManager(dal, diceRoller);
        int characterId = 1;
        int skillLevel = 5; // Max 5 mana

        await manager.EnsureManaPoolAsync(characterId, MagicSchool.Fire, "fire-mana", skillLevel);
        // Spend some mana first
        await manager.SpendManaAsync(characterId, MagicSchool.Fire, 3);

        // Act
        var result = await manager.AttemptManaRecoveryAsync(
            characterId, MagicSchool.Fire, skillLevel, attributeBonus: 2);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.ManaRecovered > 0);
        Assert.AreEqual(result.ManaRecovered, result.MinutesSpent); // 1 minute per mana
    }

    [TestMethod]
    public async Task AttemptManaRecovery_Fails_WhenPoolFull()
    {
        // Arrange
        var dal = new ManaDal();
        var diceRoller = new DeterministicDiceRoller();
        var manager = new ManaManager(dal, diceRoller);
        int characterId = 1;
        int skillLevel = 3;

        await manager.EnsureManaPoolAsync(characterId, MagicSchool.Fire, "fire-mana", skillLevel);

        // Act - pool starts full
        var result = await manager.AttemptManaRecoveryAsync(
            characterId, MagicSchool.Fire, skillLevel, attributeBonus: 2);

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsTrue(result.ErrorMessage!.Contains("already full"));
    }

    [TestMethod]
    public async Task AttemptManaRecovery_UsesTV6_ByDefault()
    {
        // Arrange
        var dal = new ManaDal();
        var diceRoller = new DeterministicDiceRoller();
        var manager = new ManaManager(dal, diceRoller);
        int characterId = 1;
        int skillLevel = 3;

        await manager.EnsureManaPoolAsync(characterId, MagicSchool.Fire, "fire-mana", skillLevel);
        await manager.SpendManaAsync(characterId, MagicSchool.Fire, 2);

        // Act
        var result = await manager.AttemptManaRecoveryAsync(
            characterId, MagicSchool.Fire, skillLevel, attributeBonus: 2);

        // Assert
        Assert.AreEqual(ManaManager.DefaultRecoveryTV, result.TV);
        Assert.AreEqual(6, result.TV);
    }

    [TestMethod]
    public async Task AttemptManaRecovery_RespectsMaxMana()
    {
        // Arrange
        var dal = new ManaDal();
        // High roll for lots of recovery potential
        var diceRoller = DeterministicDiceRoller.WithFixed4dFPlus(8);
        var manager = new ManaManager(dal, diceRoller);
        int characterId = 1;
        int skillLevel = 3; // Max 3 mana

        await manager.EnsureManaPoolAsync(characterId, MagicSchool.Fire, "fire-mana", skillLevel);
        await manager.SpendManaAsync(characterId, MagicSchool.Fire, 2); // Now at 1

        // Act
        var result = await manager.AttemptManaRecoveryAsync(
            characterId, MagicSchool.Fire, skillLevel, attributeBonus: 2);

        // Assert
        Assert.IsTrue(result.Success);
        Assert.AreEqual(2, result.ManaRecovered); // Capped at max 3 - current 1 = 2
        Assert.AreEqual(3, result.CurrentMana);
        Assert.AreEqual(3, result.MaxMana);
    }

    #endregion

    #region Multi-School Tests

    [TestMethod]
    public async Task MultipleSchools_HaveSeparatePools()
    {
        // Arrange
        var dal = new ManaDal();
        var diceRoller = new DeterministicDiceRoller();
        var manager = new ManaManager(dal, diceRoller);
        int characterId = 1;

        await manager.EnsureManaPoolAsync(characterId, MagicSchool.Fire, "fire-mana", 3);
        await manager.EnsureManaPoolAsync(characterId, MagicSchool.Life, "life-mana", 5);

        // Act
        await manager.SpendManaAsync(characterId, MagicSchool.Fire, 2);

        // Assert
        var firePool = await manager.GetManaPoolAsync(characterId, MagicSchool.Fire);
        var lifePool = await manager.GetManaPoolAsync(characterId, MagicSchool.Life);

        Assert.AreEqual(1, firePool!.CurrentMana); // 3 - 2 = 1
        Assert.AreEqual(5, lifePool!.CurrentMana); // Unchanged
    }

    [TestMethod]
    public async Task GetAllManaPools_ReturnsAllSchools()
    {
        // Arrange
        var dal = new ManaDal();
        var diceRoller = new DeterministicDiceRoller();
        var manager = new ManaManager(dal, diceRoller);
        int characterId = 1;

        await manager.EnsureManaPoolAsync(characterId, MagicSchool.Fire, "fire-mana", 3);
        await manager.EnsureManaPoolAsync(characterId, MagicSchool.Water, "water-mana", 4);
        await manager.EnsureManaPoolAsync(characterId, MagicSchool.Life, "life-mana", 5);

        // Act
        var pools = await manager.GetAllManaPoolsAsync(characterId);

        // Assert
        Assert.AreEqual(3, pools.Count);
    }

    #endregion

    #region Utility Method Tests

    [TestMethod]
    [DataRow(MagicSchool.Fire, "fire-mana")]
    [DataRow(MagicSchool.Water, "water-mana")]
    [DataRow(MagicSchool.Light, "light-mana")]
    [DataRow(MagicSchool.Life, "life-mana")]
    public void GetManaSkillId_ReturnsCorrectId(MagicSchool school, string expectedId)
    {
        // Act
        var skillId = ManaManager.GetManaSkillId(school);

        // Assert
        Assert.AreEqual(expectedId, skillId);
    }

    [TestMethod]
    [DataRow("fire-mana", MagicSchool.Fire)]
    [DataRow("water-mana", MagicSchool.Water)]
    [DataRow("light-mana", MagicSchool.Light)]
    [DataRow("life-mana", MagicSchool.Life)]
    public void GetSchoolFromManaSkillId_ReturnsCorrectSchool(string skillId, MagicSchool expectedSchool)
    {
        // Act
        var school = ManaManager.GetSchoolFromManaSkillId(skillId);

        // Assert
        Assert.AreEqual(expectedSchool, school);
    }

    [TestMethod]
    public void GetSchoolFromManaSkillId_ReturnsNull_ForUnknownSkill()
    {
        // Act
        var school = ManaManager.GetSchoolFromManaSkillId("unknown-skill");

        // Assert
        Assert.IsNull(school);
    }

    #endregion
}
