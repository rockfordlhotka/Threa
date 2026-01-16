using System;
using System.Linq;
using GameMechanics.Time;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GameMechanics.Test;

[TestClass]
public class TimeSystemTests
{
    #region Cooldown Tests

    [TestMethod]
    public void Cooldown_AdvanceRound_DecreasesRemaining()
    {
        var cooldown = new Cooldown(1, "Test Action", 6.0);

        Assert.AreEqual(6.0, cooldown.RemainingSeconds);
        Assert.IsTrue(cooldown.IsActive);

        cooldown.AdvanceRound(); // -3 seconds

        Assert.AreEqual(3.0, cooldown.RemainingSeconds);
        Assert.IsTrue(cooldown.IsActive);
    }

    [TestMethod]
    public void Cooldown_AdvanceRound_CompletesWhenZero()
    {
        var cooldown = new Cooldown(1, "Test Action", 3.0);

        var completed = cooldown.AdvanceRound();

        Assert.IsTrue(completed);
        Assert.IsTrue(cooldown.IsReady);
        Assert.AreEqual(0.0, cooldown.RemainingSeconds);
    }

    [TestMethod]
    public void Cooldown_Interrupt_Resettable_ResetsProgress()
    {
        var cooldown = new Cooldown(1, "Aim", 6.0, CooldownBehavior.Resettable);
        cooldown.AdvanceRound(); // Progress to 50%

        cooldown.Interrupt();

        Assert.AreEqual(CooldownState.Reset, cooldown.State);
        Assert.AreEqual(6.0, cooldown.RemainingSeconds);
    }

    [TestMethod]
    public void Cooldown_Interrupt_Pausable_PausesProgress()
    {
        var cooldown = new Cooldown(1, "Reload", 6.0, CooldownBehavior.Pausable);
        cooldown.AdvanceRound(); // Progress to 50%

        cooldown.Interrupt();

        Assert.AreEqual(CooldownState.Paused, cooldown.State);
        Assert.AreEqual(3.0, cooldown.RemainingSeconds); // Progress preserved
    }

    [TestMethod]
    public void Cooldown_Resume_ContinuesPausedCooldown()
    {
        var cooldown = new Cooldown(1, "Reload", 6.0, CooldownBehavior.Pausable);
        cooldown.AdvanceRound();
        cooldown.Interrupt();

        cooldown.Resume();

        Assert.AreEqual(CooldownState.Active, cooldown.State);
        Assert.AreEqual(3.0, cooldown.RemainingSeconds);
    }

    [TestMethod]
    [DataRow(0, 6.0)]
    [DataRow(1, 5.0)]
    [DataRow(2, 4.0)]
    [DataRow(3, 3.0)]
    [DataRow(5, 2.0)]
    [DataRow(7, 1.0)]
    [DataRow(9, 0.5)]
    [DataRow(10, 0.0)]
    [DataRow(15, 0.0)]
    public void Cooldown_SkillBased_CalculatesCorrectDuration(int skillLevel, double expectedSeconds)
    {
        var duration = Cooldown.CalculateSkillBasedCooldown(skillLevel);
        Assert.AreEqual(expectedSeconds, duration);
    }

    [TestMethod]
    public void Cooldown_Progress_CalculatesCorrectly()
    {
        var cooldown = new Cooldown(1, "Test", 6.0);
        Assert.AreEqual(0.0, cooldown.Progress);

        cooldown.AdvanceRound(); // 50%
        Assert.AreEqual(0.5, cooldown.Progress);

        cooldown.AdvanceRound(); // 100%
        Assert.AreEqual(1.0, cooldown.Progress);
    }

    #endregion

    #region CooldownTracker Tests

    [TestMethod]
    public void CooldownTracker_StartCooldown_AddsToActive()
    {
        var tracker = new CooldownTracker(1);

        var cooldown = tracker.StartCooldown("Bow Shot", 3.0);

        Assert.AreEqual(1, tracker.ActiveCooldowns.Count);
        Assert.IsTrue(tracker.IsOnCooldown("Bow Shot"));
    }

    [TestMethod]
    public void CooldownTracker_AdvanceRound_AdvancesAllCooldowns()
    {
        var tracker = new CooldownTracker(1);
        tracker.StartCooldown("Action1", 6.0);
        tracker.StartCooldown("Action2", 3.0);

        var completed = tracker.AdvanceRound();

        Assert.AreEqual(1, completed.Count);
        Assert.AreEqual("Action2", completed[0].ActionName);
        Assert.AreEqual(1, tracker.ActiveCooldowns.Count); // Only Action1 still active
    }

    [TestMethod]
    public void CooldownTracker_InterruptAll_InterruptsActiveCooldowns()
    {
        var tracker = new CooldownTracker(1);
        tracker.StartCooldown("Action1", 6.0, CooldownBehavior.Resettable);
        tracker.StartCooldown("Action2", 6.0, CooldownBehavior.Pausable);

        tracker.InterruptAll();

        Assert.AreEqual(0, tracker.ActiveCooldowns.Count);
        var all = tracker.AllCooldowns.ToList();
        Assert.AreEqual(CooldownState.Reset, all.First(c => c.ActionName == "Action1").State);
        Assert.AreEqual(CooldownState.Paused, all.First(c => c.ActionName == "Action2").State);
    }

    [TestMethod]
    public void CooldownTracker_IsActionReady_ReturnsTrueWhenNoCooldown()
    {
        var tracker = new CooldownTracker(1);

        Assert.IsTrue(tracker.IsActionReady("Any Action"));
    }

    [TestMethod]
    public void CooldownTracker_IsActionReady_ReturnsFalseWhenOnCooldown()
    {
        var tracker = new CooldownTracker(1);
        tracker.StartCooldown("Bow Shot", 6.0);

        Assert.IsFalse(tracker.IsActionReady("Bow Shot"));
    }

    #endregion

    #region InitiativeCalculator Tests

    [TestMethod]
    public void Initiative_CalculateInitiative_OrdersByAPDescending()
    {
        var calc = new InitiativeCalculator();
        calc.AddParticipant(1, "Low AP", 3, 10);
        calc.AddParticipant(2, "High AP", 8, 10);
        calc.AddParticipant(3, "Med AP", 5, 10);

        calc.CalculateInitiative();

        var order = calc.InitiativeOrder;
        Assert.AreEqual("High AP", order[0].Name);
        Assert.AreEqual("Med AP", order[1].Name);
        Assert.AreEqual("Low AP", order[2].Name);
    }

    [TestMethod]
    public void Initiative_CalculateInitiative_UsesAwarenessAsTiebreaker()
    {
        var calc = new InitiativeCalculator();
        calc.AddParticipant(1, "Low Awareness", 5, 8);
        calc.AddParticipant(2, "High Awareness", 5, 12);

        calc.CalculateInitiative();

        var order = calc.InitiativeOrder;
        Assert.AreEqual("High Awareness", order[0].Name);
        Assert.AreEqual("Low Awareness", order[1].Name);
    }

    [TestMethod]
    public void Initiative_ActAndAdvance_MovesToNextParticipant()
    {
        var calc = new InitiativeCalculator();
        calc.AddParticipant(1, "First", 10, 10);
        calc.AddParticipant(2, "Second", 5, 10);
        calc.CalculateInitiative();

        Assert.AreEqual("First", calc.CurrentParticipant?.Name);

        var next = calc.ActAndAdvance();

        Assert.AreEqual("Second", next?.Name);
        Assert.IsTrue(calc.InitiativeOrder[0].HasActed);
    }

    [TestMethod]
    public void Initiative_PassAndAdvance_PreservesAPIntent()
    {
        var calc = new InitiativeCalculator();
        calc.AddParticipant(1, "Passer", 10, 10);
        calc.AddParticipant(2, "Actor", 5, 10);
        calc.CalculateInitiative();

        calc.PassAndAdvance();

        Assert.IsTrue(calc.InitiativeOrder[0].HasPassed);
        Assert.AreEqual("Actor", calc.CurrentParticipant?.Name);
    }

    [TestMethod]
    public void Initiative_IsRoundComplete_TrueWhenAllActedOrPassed()
    {
        var calc = new InitiativeCalculator();
        calc.AddParticipant(1, "Actor", 10, 10);
        calc.AddParticipant(2, "Passer", 5, 10);
        calc.CalculateInitiative();

        Assert.IsFalse(calc.IsRoundComplete);

        calc.ActAndAdvance();
        Assert.IsFalse(calc.IsRoundComplete);

        calc.PassAndAdvance();
        Assert.IsTrue(calc.IsRoundComplete);
    }

    [TestMethod]
    public void Initiative_Delay_AllowsLaterAction()
    {
        var calc = new InitiativeCalculator();
        calc.AddParticipant(1, "Delayer", 10, 10);
        calc.AddParticipant(2, "Actor", 5, 10);
        calc.CalculateInitiative();

        calc.Delay();

        Assert.IsTrue(calc.InitiativeOrder[0].IsDelaying);
        Assert.AreEqual(1, calc.GetDelayingParticipants().Count);
    }

    [TestMethod]
    public void Initiative_UpdateAvailableAP_ChangesValue()
    {
        var calc = new InitiativeCalculator();
        calc.AddParticipant(1, "Character", 10, 10);

        calc.UpdateAvailableAP(1, 5);

        Assert.AreEqual(5, calc.InitiativeOrder[0].AvailableAP);
    }

    [TestMethod]
    public void Initiative_SetCanAct_DisablesParticipant()
    {
        var calc = new InitiativeCalculator();
        calc.AddParticipant(1, "Character", 10, 10);
        calc.CalculateInitiative();

        calc.SetCanAct(1, false);
        calc.CalculateInitiative();

        // Incapacitated characters are moved to end and skipped
        Assert.IsFalse(calc.InitiativeOrder[0].CanAct);
    }

    #endregion

    #region TimeState Tests

    [TestMethod]
    public void TimeState_ElapsedSeconds_CalculatesCorrectly()
    {
        var state = new TimeState { TotalRounds = 100 };

        Assert.AreEqual(300, state.ElapsedSeconds); // 100 rounds × 3 seconds
    }

    [TestMethod]
    public void TimeState_DisplayTime_FormatsCorrectly()
    {
        // Set TotalRounds to produce the desired computed values:
        // TotalWeeks = 2, DayInWeek = 3, HourInDay = 14, MinuteInTurn = 3, RoundInMinute = 10
        // Calculation: 2*201600 + 3*28800 + 14*1200 + 3*20 + 10 = 506470
        var state = new TimeState
        {
            TotalRounds = 506470
        };

        // Verify computed values
        Assert.AreEqual(2, state.TotalWeeks);
        Assert.AreEqual(3, state.DayInWeek);
        Assert.AreEqual(14, state.HourInDay);
        Assert.AreEqual(3, state.MinuteInTurn);
        Assert.AreEqual(10, state.RoundInMinute);

        Assert.AreEqual("Week 3, Day 4, 14:35", state.DisplayTime);
    }

    #endregion

    #region TimeEventType Tests

    [TestMethod]
    public void TimeEventType_HasAllExpectedValues()
    {
        var values = Enum.GetValues<TimeEventType>();

        CollectionAssert.Contains(values, TimeEventType.EndOfRound);
        CollectionAssert.Contains(values, TimeEventType.EndOfMinute);
        CollectionAssert.Contains(values, TimeEventType.EndOfTurn);
        CollectionAssert.Contains(values, TimeEventType.EndOfHour);
        CollectionAssert.Contains(values, TimeEventType.EndOfDay);
        CollectionAssert.Contains(values, TimeEventType.EndOfWeek);
    }

    #endregion
}
