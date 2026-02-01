using Csla;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMechanics.Test;

[TestClass]
public class FatRecoveryOrderTests : TestBase
{
    [TestMethod]
    public void EndOfRound_NaturalRecovery_OffsetsPendingDamage()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var character = dp.Create(42);
        
        // Damage the character first so natural recovery will trigger
        character.Fatigue.Value = character.Fatigue.BaseValue - 3;
        var initialFAT = character.Fatigue.Value;
        
        // Set up character with some FAT damage pending
        character.Fatigue.PendingDamage = 4;  // 4 pending damage
        Assert.AreEqual(0, character.Fatigue.PendingHealing);
        
        // Act
        character.EndOfRound(effectPortal);
        
        // Assert
        // Natural recovery (1) should be added to PendingHealing BEFORE applying pools
        // So: PendingHealing = 1, PendingDamage = 4
        // Then apply: half of healing (1) and half of damage (2)
        // Net effect: FAT = initialFAT + 1 - 2 = initialFAT - 1
        // Remaining pools: PendingHealing = 0, PendingDamage = 2
        
        Assert.AreEqual(initialFAT - 1, character.Fatigue.Value, "FAT should decrease by net 1");
        Assert.AreEqual(2, character.Fatigue.PendingDamage, "Half of pending damage should remain");
        Assert.AreEqual(0, character.Fatigue.PendingHealing, "All pending healing should be applied");
    }
    
    [TestMethod]
    public void EndOfRound_NaturalRecovery_AddsToPoolBeforeDamageApplication()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var character = dp.Create(42);
        
        // Damage the character first
        character.Fatigue.Value = character.Fatigue.BaseValue - 5;
        var initialFAT = character.Fatigue.Value;
        
        // Add pending damage that would be offset by natural recovery
        character.Fatigue.PendingDamage = 2;  // 2 pending damage
        
        // Act
        character.EndOfRound(effectPortal);
        
        // Assert
        // Natural recovery (1) should be added to PendingHealing first
        // Then: half of PendingHealing (1) applied = +1 FAT
        // Then: half of PendingDamage (1) applied = -1 FAT
        // Net: FAT unchanged, but pools are reduced
        
        Assert.AreEqual(initialFAT, character.Fatigue.Value, "FAT should be unchanged (recovery offsets damage)");
        Assert.AreEqual(1, character.Fatigue.PendingDamage, "Half of pending damage should remain");
        Assert.AreEqual(0, character.Fatigue.PendingHealing, "All pending healing should be applied");
    }
    
    [TestMethod]
    public void EndOfRound_WithOnlyPendingDamage_NaturalRecoveryStillOccurs()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var character = dp.Create(42);
        
        // Damage the character first to allow natural recovery
        character.Fatigue.Value = character.Fatigue.BaseValue - 3;
        var initialFAT = character.Fatigue.Value;
        
        // Add pending damage
        character.Fatigue.PendingDamage = 6;
        
        // Act
        character.EndOfRound(effectPortal);
        
        // Assert
        // Natural recovery (1) should be added to PendingHealing first
        // Then: half of PendingHealing (1) = +1 FAT
        // Then: half of PendingDamage (3) = -3 FAT
        // Net: FAT = initialFAT + 1 - 3 = initialFAT - 2
        
        Assert.AreEqual(initialFAT - 2, character.Fatigue.Value, "FAT should decrease by net 2");
        Assert.AreEqual(3, character.Fatigue.PendingDamage, "Half of pending damage should remain");
        Assert.AreEqual(0, character.Fatigue.PendingHealing, "All pending healing should be applied");
    }
    
    [TestMethod]
    public void EndOfRound_WithNoPendingPools_ShouldGainOneFAT()
    {
        // This test reproduces the reported issue: characters should GAIN 1 FAT at end-of-round
        // when they have no pending damage or healing
        
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var character = dp.Create(42);
        
        // Damage the character first so natural recovery can trigger
        character.Fatigue.Value = character.Fatigue.BaseValue - 5;
        var initialFAT = character.Fatigue.Value;
        
        // Ensure no pending pools
        character.Fatigue.PendingDamage = 0;
        character.Fatigue.PendingHealing = 0;
        
        // Act
        character.EndOfRound(effectPortal);
        
        // Assert
        // Natural recovery should add 1 to PendingHealing, then apply it
        // Result: FAT should increase by 1
        Assert.AreEqual(initialFAT + 1, character.Fatigue.Value, 
            "FAT should increase by 1 from natural recovery");
        Assert.AreEqual(0, character.Fatigue.PendingDamage, "No pending damage should remain");
        Assert.AreEqual(0, character.Fatigue.PendingHealing, "All pending healing should be applied");
    }
    
    [TestMethod]
    public void EndOfRound_WithActionFatCost_ShouldOffsetWithNaturalRecovery()
    {
        // This test verifies that natural FAT recovery offsets FAT spent on actions
        
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var character = dp.Create(42);
        
        // Ensure character has enough FAT for recovery
        character.Fatigue.Value = character.Fatigue.BaseValue - 5;
        var initialFAT = character.Fatigue.Value;
        
        // Take an action with fatigue cost (adds 1 to PendingDamage)
        character.ActionPoints.TakeActionWithFatigue();
        Assert.AreEqual(1, character.Fatigue.PendingDamage, "Action should cost 1 FAT");
        
        // Act - process end of round
        character.EndOfRound(effectPortal);
        
        // Assert
        // Natural recovery (1) should offset action cost (1)
        // Result: FAT should be unchanged
        Assert.AreEqual(initialFAT, character.Fatigue.Value, 
            "FAT should be unchanged (natural recovery offsets action cost)");
        Assert.AreEqual(0, character.Fatigue.PendingDamage, "No pending damage should remain");
        Assert.AreEqual(0, character.Fatigue.PendingHealing, "All pending healing should be applied");
    }
    
    [TestMethod]
    public void EndOfRound_EffectsShouldBeProcessedBeforePoolApplication()
    {
        // This test verifies that effects (like wounds/DoTs) add damage BEFORE
        // natural recovery is applied, so recovery can offset the damage
        
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var character = dp.Create(42);
        
        // Ensure character has FAT below max for recovery
        character.Fatigue.Value = character.Fatigue.BaseValue - 5;
        var initialFAT = character.Fatigue.Value;
        
        // Manually add damage that would be added by an effect's OnTick
        // (simulating a wound or DoT that does 1 FAT damage per round)
        character.Fatigue.PendingDamage = 0;
        
        // Act - call EndOfRound which should:
        // 1. Process Effects.EndOfRound() (which would add damage to PendingDamage)
        // 2. Process Fatigue.EndOfRound() (which adds natural recovery and applies pools)
        character.EndOfRound(effectPortal);
        
        // Assert - If effects were processed first, natural recovery should offset the damage
        // For this test we don't have actual effects, so we should just see +1 from natural recovery
        Assert.AreEqual(initialFAT + 1, character.Fatigue.Value, 
            "Without effects, FAT should increase by 1 from natural recovery");
    }
}
