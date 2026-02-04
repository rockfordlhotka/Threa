using Csla;
using Csla.Configuration;
using GameMechanics.Effects;
using GameMechanics.Effects.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

[TestClass]
public class SkillConcentrationTests : TestBase
{
    #region Pre-Use Concentration Tests

    [TestMethod]
    public void CreatePreUseSkillState_CreatesValidState()
    {
        // Arrange & Act
        var stateJson = ConcentrationBehavior.CreatePreUseSkillState(
            skillId: "skill-123",
            skillName: "Power Strike",
            concentrationRounds: 2);

        var state = ConcentrationState.FromJson(stateJson);

        // Assert
        Assert.IsNotNull(state);
        Assert.AreEqual("PreUseSkill", state.ConcentrationType);
        Assert.AreEqual(2, state.TotalRequired);
        Assert.AreEqual(0, state.CurrentProgress);
        Assert.AreEqual(1, state.RoundsPerTick);
        Assert.AreEqual("SkillUse", state.DeferredActionType);
        Assert.IsNotNull(state.DeferredActionPayload);
        Assert.IsTrue(state.CompletionMessage?.Contains("Power Strike"));
        Assert.IsTrue(state.InterruptionMessage?.Contains("Power Strike"));

        // Verify nested payload
        var payload = SkillUsePayload.FromJson(state.DeferredActionPayload);
        Assert.IsNotNull(payload);
        Assert.AreEqual("skill-123", payload.SkillId);
        Assert.AreEqual("Power Strike", payload.SkillName);
        Assert.AreEqual(0, payload.InterruptionPenaltyRounds, "Pre-use should have no penalty");
    }

    [TestMethod]
    public void OnTick_PreUseSkill_TracksProgress()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var stateJson = ConcentrationBehavior.CreatePreUseSkillState(
            skillId: "skill-123",
            skillName: "Power Strike",
            concentrationRounds: 3);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Concentrating on Power Strike",
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Act - first tick
        c.Effects.EndOfRound(0);

        // Assert - should still be concentrating with progress = 1
        var concentrationEffect = c.Effects.FirstOrDefault(e => e.EffectType == EffectType.Concentration);
        Assert.IsNotNull(concentrationEffect);

        var state = ConcentrationState.FromJson(concentrationEffect.BehaviorState);
        Assert.IsNotNull(state);
        Assert.AreEqual(1, state.CurrentProgress);

        // Description should show progress
        Assert.IsTrue(concentrationEffect.Description?.Contains("1/3"),
            $"Description should contain progress '1/3'. Actual: {concentrationEffect.Description}");
    }

    [TestMethod]
    public void OnExpire_PreUseSkill_SetsSkillUseResult()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var stateJson = ConcentrationBehavior.CreatePreUseSkillState(
            skillId: "skill-123",
            skillName: "Power Strike",
            concentrationRounds: 2);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Concentrating on Power Strike",
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Act - run to completion
        c.Effects.EndOfRound(0);
        c.Effects.EndOfRound(0);

        // Assert
        Assert.AreEqual(0, c.Effects.Count, "Effect should be removed after completion");
        Assert.IsNotNull(c.LastConcentrationResult);
        Assert.AreEqual("SkillUse", c.LastConcentrationResult.ActionType);
        Assert.IsTrue(c.LastConcentrationResult.Success);
        Assert.IsTrue(c.LastConcentrationResult.Message.Contains("Power Strike"));
        Assert.IsTrue(c.LastConcentrationResult.Message.Contains("ready"));

        // Verify payload can be deserialized
        var payload = SkillUsePayload.FromJson(c.LastConcentrationResult.Payload);
        Assert.IsNotNull(payload);
        Assert.AreEqual("skill-123", payload.SkillId);
        Assert.AreEqual("Power Strike", payload.SkillName);
    }

    [TestMethod]
    public void OnRemove_PreUseSkill_PreventsSkillExecution()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var stateJson = ConcentrationBehavior.CreatePreUseSkillState(
            skillId: "skill-123",
            skillName: "Power Strike",
            concentrationRounds: 4);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Concentrating on Power Strike",
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Run one tick then interrupt
        c.Effects.EndOfRound(0);

        // Act - manually remove (simulating interruption)
        ConcentrationBehavior.BreakConcentration(c);

        // Assert
        Assert.IsNotNull(c.LastConcentrationResult);
        Assert.IsFalse(c.LastConcentrationResult.Success, "Success should be false for interruption");
        Assert.IsTrue(c.LastConcentrationResult.Message.Contains("interrupted"));
        Assert.IsTrue(c.LastConcentrationResult.Message.Contains("not usable") ||
                      c.LastConcentrationResult.Message.Contains("interrupted"),
            $"Message should indicate skill not usable. Actual: {c.LastConcentrationResult.Message}");
    }

    #endregion

    #region Post-Use Concentration Tests

    [TestMethod]
    public void CreatePostUseSkillState_CreatesValidState()
    {
        // Arrange & Act
        var debuffConfig = new InterruptionDebuffConfig
        {
            Name = "Mighty Blow Broken",
            DurationRounds = 5,
            GlobalAsPenalty = -1
        };

        var stateJson = ConcentrationBehavior.CreatePostUseSkillState(
            skillId: "skill-456",
            skillName: "Mighty Blow",
            concentrationRounds: 3,
            interruptionDebuff: debuffConfig);

        var state = ConcentrationState.FromJson(stateJson);

        // Assert
        Assert.IsNotNull(state);
        Assert.AreEqual("PostUseSkill", state.ConcentrationType);
        Assert.AreEqual(3, state.TotalRequired);
        Assert.AreEqual(0, state.CurrentProgress);
        Assert.AreEqual(1, state.RoundsPerTick);
        Assert.IsNotNull(state.DeferredActionPayload);
        Assert.IsTrue(state.CompletionMessage?.Contains("Mighty Blow"));
        Assert.IsTrue(state.InterruptionMessage?.Contains("Mighty Blow"));

        // Verify nested payload
        var payload = SkillUsePayload.FromJson(state.DeferredActionPayload);
        Assert.IsNotNull(payload);
        Assert.AreEqual("skill-456", payload.SkillId);
        Assert.AreEqual("Mighty Blow", payload.SkillName);
        Assert.IsNotNull(payload.InterruptionDebuff);
        Assert.AreEqual(5, payload.InterruptionDebuff.DurationRounds);
        Assert.AreEqual(-1, payload.InterruptionDebuff.GlobalAsPenalty);
    }

    [TestMethod]
    public void OnTick_PostUseSkill_TracksProgress()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var stateJson = ConcentrationBehavior.CreatePostUseSkillState(
            skillId: "skill-456",
            skillName: "Mighty Blow",
            concentrationRounds: 4);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Maintaining Mighty Blow",
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Act - run 2 ticks
        c.Effects.EndOfRound(0);
        c.Effects.EndOfRound(0);

        // Assert - should still be concentrating with progress = 2
        var concentrationEffect = c.Effects.FirstOrDefault(e => e.EffectType == EffectType.Concentration);
        Assert.IsNotNull(concentrationEffect);

        var state = ConcentrationState.FromJson(concentrationEffect.BehaviorState);
        Assert.IsNotNull(state);
        Assert.AreEqual(2, state.CurrentProgress);
    }

    [TestMethod]
    public void OnExpire_PostUseSkill_CompletesWithoutPenalty()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var stateJson = ConcentrationBehavior.CreatePostUseSkillState(
            skillId: "skill-456",
            skillName: "Mighty Blow",
            concentrationRounds: 2);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Maintaining Mighty Blow",
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Act - run to completion
        c.Effects.EndOfRound(0);
        c.Effects.EndOfRound(0);

        // Assert
        Assert.AreEqual(0, c.Effects.Count, "Effect should be removed after completion");

        // Post-use completion signals linked effects should be removed (no debuff)
        Assert.IsNotNull(c.LastConcentrationResult);
        Assert.AreEqual("PostUseSkillEnded", c.LastConcentrationResult.ActionType);
        Assert.IsTrue(c.LastConcentrationResult.Success, "Normal completion should be successful");
    }

    [TestMethod]
    public void OnRemove_PostUseSkill_TriggersInterruptionPenaltyResult()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var debuffConfig = new InterruptionDebuffConfig
        {
            Name = "Mighty Blow Broken",
            DurationRounds = 5,
            GlobalAsPenalty = -1
        };

        var stateJson = ConcentrationBehavior.CreatePostUseSkillState(
            skillId: "skill-456",
            skillName: "Mighty Blow",
            concentrationRounds: 4,
            interruptionDebuff: debuffConfig);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Maintaining Mighty Blow",
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Run one tick then interrupt
        c.Effects.EndOfRound(0);

        // Act - manually remove (simulating interruption)
        ConcentrationBehavior.BreakConcentration(c);

        // Assert
        Assert.IsNotNull(c.LastConcentrationResult);
        Assert.AreEqual("PostUseSkillInterrupted", c.LastConcentrationResult.ActionType);
        Assert.IsFalse(c.LastConcentrationResult.Success);
        Assert.IsTrue(c.LastConcentrationResult.Message.Contains("interrupted"));

        // Verify payload contains debuff config
        var payload = SkillUsePayload.FromJson(c.LastConcentrationResult.Payload);
        Assert.IsNotNull(payload);
        Assert.IsNotNull(payload.InterruptionDebuff);
        Assert.AreEqual(5, payload.InterruptionDebuff.DurationRounds);
        Assert.AreEqual("Mighty Blow", payload.SkillName);
    }

    [TestMethod]
    public void OnRemove_PostUseSkill_StillSignalsInterruptionWithDefaultDebuff()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        // Create without explicit debuff config - will get default
        var stateJson = ConcentrationBehavior.CreatePostUseSkillState(
            skillId: "skill-456",
            skillName: "Mighty Blow",
            concentrationRounds: 4);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Maintaining Mighty Blow",
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);
        c.Effects.EndOfRound(0);
        c.ClearConcentrationResult();

        // Act - manually remove (simulating interruption)
        ConcentrationBehavior.BreakConcentration(c);

        // Assert - interruption is signaled with default debuff config
        Assert.IsNotNull(c.LastConcentrationResult);
        Assert.AreEqual("PostUseSkillInterrupted", c.LastConcentrationResult.ActionType);

        var payload = SkillUsePayload.FromJson(c.LastConcentrationResult.Payload);
        Assert.IsNotNull(payload?.InterruptionDebuff);
        Assert.AreEqual(3, payload.InterruptionDebuff.DurationRounds, "Default debuff should be 3 rounds");
        Assert.AreEqual(-1, payload.InterruptionDebuff.GlobalAsPenalty, "Default penalty should be -1 AS");
    }

    #endregion

    #region SkillUsePayload Tests

    [TestMethod]
    public void SkillUsePayload_Serialization_RoundTrips()
    {
        // Arrange
        var original = new SkillUsePayload
        {
            SkillId = "test-skill-id",
            SkillName = "Test Skill",
            InterruptionPenaltyRounds = 3,
            AdditionalData = "{\"customField\":\"customValue\"}"
        };

        // Act
        var json = original.Serialize();
        var deserialized = SkillUsePayload.FromJson(json);

        // Assert
        Assert.IsNotNull(deserialized);
        Assert.AreEqual(original.SkillId, deserialized.SkillId);
        Assert.AreEqual(original.SkillName, deserialized.SkillName);
        Assert.AreEqual(original.InterruptionPenaltyRounds, deserialized.InterruptionPenaltyRounds);
        Assert.AreEqual(original.AdditionalData, deserialized.AdditionalData);
    }

    [TestMethod]
    public void SkillUsePayload_FromJson_HandlesNullInput()
    {
        // Act
        var result = SkillUsePayload.FromJson(null);

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void SkillUsePayload_FromJson_HandlesEmptyInput()
    {
        // Act
        var result = SkillUsePayload.FromJson("");

        // Assert
        Assert.IsNull(result);
    }

    [TestMethod]
    public void SkillUsePayload_FromJson_HandlesInvalidJson()
    {
        // Act
        var result = SkillUsePayload.FromJson("not valid json");

        // Assert
        Assert.IsNull(result);
    }

    #endregion

    #region Combined Pre and Post-Use Tests

    [TestMethod]
    public void PreUseSkill_ThenPostUseSkill_CanChain()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        // Start with pre-use concentration
        var preUseState = ConcentrationBehavior.CreatePreUseSkillState(
            skillId: "chain-skill",
            skillName: "Chain Skill",
            concentrationRounds: 1);

        var preUseEffect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Concentrating on Chain Skill",
            null,
            null,
            preUseState);

        c.Effects.AddEffect(preUseEffect);

        // Complete pre-use concentration
        c.Effects.EndOfRound(0);

        // Assert pre-use completed
        Assert.AreEqual(0, c.Effects.Count);
        Assert.IsNotNull(c.LastConcentrationResult);
        Assert.AreEqual("SkillUse", c.LastConcentrationResult.ActionType);
        Assert.IsTrue(c.LastConcentrationResult.Success);

        // Clear and apply post-use concentration (simulating skill execution)
        c.ClearConcentrationResult();

        var postUseState = ConcentrationBehavior.CreatePostUseSkillState(
            skillId: "chain-skill",
            skillName: "Chain Skill",
            concentrationRounds: 2);

        var postUseEffect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Maintaining Chain Skill",
            null,
            null,
            postUseState);

        c.Effects.AddEffect(postUseEffect);

        // Act - complete post-use concentration
        c.Effects.EndOfRound(0);
        c.Effects.EndOfRound(0);

        // Assert - post-use completed without penalty
        Assert.AreEqual(0, c.Effects.Count, "Post-use effect should be removed");
        Assert.IsNotNull(c.LastConcentrationResult);
        Assert.AreEqual("PostUseSkillEnded", c.LastConcentrationResult.ActionType);
        Assert.IsTrue(c.LastConcentrationResult.Success);
    }

    #endregion

    #region Linked Effects Tests

    [TestMethod]
    public void CreatePostUseSkillState_WithLinkedEffects_PreservesEffectInfo()
    {
        // Arrange
        var linkedEffects = new List<LinkedEffectInfo>
        {
            new LinkedEffectInfo
            {
                EffectId = Guid.NewGuid(),
                TargetCharacterId = 42,
                Description = "Blinded on Goblin"
            },
            new LinkedEffectInfo
            {
                EffectId = Guid.NewGuid(),
                TargetCharacterId = 43,
                Description = "Blinded on Orc"
            }
        };

        // Act
        var stateJson = ConcentrationBehavior.CreatePostUseSkillState(
            skillId: "blindness-spell",
            skillName: "Blindness",
            concentrationRounds: 3,
            linkedEffects: linkedEffects);

        var state = ConcentrationState.FromJson(stateJson);

        // Assert
        Assert.IsNotNull(state);
        Assert.IsNotNull(state.LinkedEffects);
        Assert.AreEqual(2, state.LinkedEffects.Count);
        Assert.AreEqual(42, state.LinkedEffects[0].TargetCharacterId);
        Assert.AreEqual(43, state.LinkedEffects[1].TargetCharacterId);

        // Also verify in payload
        var payload = SkillUsePayload.FromJson(state.DeferredActionPayload);
        Assert.IsNotNull(payload?.LinkedEffects);
        Assert.AreEqual(2, payload.LinkedEffects.Count);
    }

    [TestMethod]
    public void OnExpire_PostUseSkill_WithLinkedEffects_SignalsRemovalNeeded()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var linkedEffectId = Guid.NewGuid();
        var linkedEffects = new List<LinkedEffectInfo>
        {
            new LinkedEffectInfo
            {
                EffectId = linkedEffectId,
                TargetCharacterId = 99,
                Description = "Blinded on Target"
            }
        };

        var stateJson = ConcentrationBehavior.CreatePostUseSkillState(
            skillId: "blindness-spell",
            skillName: "Blindness",
            concentrationRounds: 2,
            linkedEffects: linkedEffects);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Maintaining Blindness",
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);

        // Act - run to completion
        c.Effects.EndOfRound(0);
        c.Effects.EndOfRound(0);

        // Assert - should signal linked effect removal
        Assert.IsNotNull(c.LastConcentrationResult);
        Assert.AreEqual("PostUseSkillEnded", c.LastConcentrationResult.ActionType);
        Assert.IsTrue(c.LastConcentrationResult.Success);

        // Verify payload contains linked effect info for removal
        var payload = SkillUsePayload.FromJson(c.LastConcentrationResult.Payload);
        Assert.IsNotNull(payload?.LinkedEffects);
        Assert.AreEqual(1, payload.LinkedEffects.Count);
        Assert.AreEqual(linkedEffectId, payload.LinkedEffects[0].EffectId);
        Assert.AreEqual(99, payload.LinkedEffects[0].TargetCharacterId);
    }

    [TestMethod]
    public void OnRemove_PostUseSkill_WithLinkedEffects_SignalsRemovalAndDebuff()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var linkedEffectId = Guid.NewGuid();
        var linkedEffects = new List<LinkedEffectInfo>
        {
            new LinkedEffectInfo
            {
                EffectId = linkedEffectId,
                TargetCharacterId = 99,
                Description = "Grappled on Target"
            }
        };

        var debuffConfig = new InterruptionDebuffConfig
        {
            Name = "Grapple Broken",
            DurationRounds = 2,
            GlobalAsPenalty = -2
        };

        var stateJson = ConcentrationBehavior.CreatePostUseSkillState(
            skillId: "grapple",
            skillName: "Grapple",
            concentrationRounds: 5,
            linkedEffects: linkedEffects,
            interruptionDebuff: debuffConfig);

        var effect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Maintaining Grapple",
            null,
            null,
            stateJson);

        c.Effects.AddEffect(effect);
        c.Effects.EndOfRound(0);
        c.ClearConcentrationResult();

        // Act - interrupt
        ConcentrationBehavior.BreakConcentration(c);

        // Assert - should signal both linked effect removal AND debuff
        Assert.IsNotNull(c.LastConcentrationResult);
        Assert.AreEqual("PostUseSkillInterrupted", c.LastConcentrationResult.ActionType);
        Assert.IsFalse(c.LastConcentrationResult.Success);

        var payload = SkillUsePayload.FromJson(c.LastConcentrationResult.Payload);
        Assert.IsNotNull(payload?.LinkedEffects);
        Assert.AreEqual(1, payload.LinkedEffects.Count);
        Assert.AreEqual(linkedEffectId, payload.LinkedEffects[0].EffectId);

        Assert.IsNotNull(payload?.InterruptionDebuff);
        Assert.AreEqual("Grapple Broken", payload.InterruptionDebuff.Name);
        Assert.AreEqual(2, payload.InterruptionDebuff.DurationRounds);
        Assert.AreEqual(-2, payload.InterruptionDebuff.GlobalAsPenalty);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void CreatePreUseSkillState_WithAdditionalData_PreservesData()
    {
        // Arrange & Act
        var stateJson = ConcentrationBehavior.CreatePreUseSkillState(
            skillId: "skill-123",
            skillName: "Power Strike",
            concentrationRounds: 2,
            additionalData: "{\"targetId\":\"target-456\",\"boost\":3}");

        var state = ConcentrationState.FromJson(stateJson);
        var payload = SkillUsePayload.FromJson(state!.DeferredActionPayload);

        // Assert
        Assert.IsNotNull(payload);
        Assert.AreEqual("{\"targetId\":\"target-456\",\"boost\":3}", payload.AdditionalData);
    }

    [TestMethod]
    public void CreatePostUseSkillState_WithAdditionalData_PreservesData()
    {
        // Arrange & Act
        var stateJson = ConcentrationBehavior.CreatePostUseSkillState(
            skillId: "skill-456",
            skillName: "Mighty Blow",
            concentrationRounds: 3,
            additionalData: "{\"damage\":15}");

        var state = ConcentrationState.FromJson(stateJson);
        var payload = SkillUsePayload.FromJson(state!.DeferredActionPayload);

        // Assert
        Assert.IsNotNull(payload);
        Assert.AreEqual("{\"damage\":15}", payload.AdditionalData);
    }

    [TestMethod]
    public void OnlyOneConcentrationAtATime_RejectsSecondConcentration()
    {
        // Arrange
        var provider = InitServices();
        var dp = provider.GetRequiredService<IDataPortal<CharacterEdit>>();
        var effectPortal = provider.GetRequiredService<IChildDataPortal<EffectRecord>>();
        var c = dp.Create(42);

        var firstState = ConcentrationBehavior.CreatePreUseSkillState(
            skillId: "skill-1",
            skillName: "First Skill",
            concentrationRounds: 3);

        var firstEffect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Concentrating on First Skill",
            null,
            null,
            firstState);

        c.Effects.AddEffect(firstEffect);

        var secondState = ConcentrationBehavior.CreatePreUseSkillState(
            skillId: "skill-2",
            skillName: "Second Skill",
            concentrationRounds: 2);

        var secondEffect = effectPortal.CreateChild(
            EffectType.Concentration,
            "Concentrating on Second Skill",
            null,
            null,
            secondState);

        // Act
        var added = c.Effects.AddEffect(secondEffect);

        // Assert
        Assert.IsFalse(added, "Second concentration should be rejected");
        Assert.AreEqual(1, c.Effects.Count, "Should only have one concentration effect");
        Assert.AreEqual("Concentrating on First Skill", c.Effects.First().Name);
    }

    #endregion
}
