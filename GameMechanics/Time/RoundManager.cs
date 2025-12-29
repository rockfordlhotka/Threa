using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameMechanics.Effects;

namespace GameMechanics.Time;

/// <summary>
/// Interface for entities that can participate in round-based time tracking.
/// Implemented by characters, NPCs, and potentially other tracked entities.
/// </summary>
public interface IRoundParticipant
{
    /// <summary>
    /// Unique identifier for this participant.
    /// </summary>
    int Id { get; }

    /// <summary>
    /// Display name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Current available action points (determines initiative).
    /// </summary>
    int AvailableAP { get; }

    /// <summary>
    /// Awareness attribute (tiebreaker for initiative).
    /// </summary>
    int Awareness { get; }

    /// <summary>
    /// Whether this participant can take actions.
    /// </summary>
    bool CanAct { get; }

    /// <summary>
    /// Process end-of-round effects for this participant.
    /// </summary>
    void EndOfRound();
}

/// <summary>
/// Manages combat rounds including initiative, turn order, and end-of-round processing.
/// </summary>
public class RoundManager
{
    private readonly InitiativeCalculator _initiative = new();
    private readonly Dictionary<int, CooldownTracker> _cooldowns = new();
    private readonly Dictionary<int, CharacterEdit> _characters = new();
    private EffectManager? _effectManager;
    
    private int _currentRound = 0;
    private int _roundsInMinute = 0;  // 20 rounds per minute
    private int _minutesInTurn = 0;   // 10 minutes per turn
    private int _turnsInHour = 0;     // 6 turns per hour

    /// <summary>
    /// Event raised at end of each round.
    /// </summary>
    public event EventHandler<RoundResult>? RoundEnded;

    /// <summary>
    /// Event raised at end of each minute (every 20 rounds).
    /// </summary>
    public event EventHandler<int>? MinuteEnded;

    /// <summary>
    /// Event raised at end of each turn (every 10 minutes).
    /// </summary>
    public event EventHandler<int>? TurnEnded;

    /// <summary>
    /// Event raised at end of each hour.
    /// </summary>
    public event EventHandler<int>? HourEnded;

    /// <summary>
    /// Gets the current round number.
    /// </summary>
    public int CurrentRound => _currentRound;

    /// <summary>
    /// Gets whether we are in active combat (participants registered).
    /// </summary>
    public bool InCombat => _characters.Count > 0;

    /// <summary>
    /// Gets the initiative calculator for turn order management.
    /// </summary>
    public InitiativeCalculator Initiative => _initiative;

    /// <summary>
    /// Sets the effect manager for processing effect durations.
    /// </summary>
    public void SetEffectManager(EffectManager effectManager)
    {
        _effectManager = effectManager;
    }

    /// <summary>
    /// Registers a character to participate in round tracking.
    /// </summary>
    public void RegisterCharacter(CharacterEdit character)
    {
        _characters[character.Id] = character;
        _cooldowns[character.Id] = new CooldownTracker(character.Id);
        
        _initiative.AddParticipant(
            character.Id,
            character.Name,
            character.ActionPoints.Available,
            character.GetAttribute("ITT"),
            isPC: true
        );
    }

    /// <summary>
    /// Unregisters a character from round tracking.
    /// </summary>
    public void UnregisterCharacter(int characterId)
    {
        _characters.Remove(characterId);
        _cooldowns.Remove(characterId);
        _initiative.RemoveParticipant(characterId);
    }

    /// <summary>
    /// Gets the cooldown tracker for a character.
    /// </summary>
    public CooldownTracker? GetCooldownTracker(int characterId)
    {
        return _cooldowns.TryGetValue(characterId, out var tracker) ? tracker : null;
    }

    /// <summary>
    /// Starts a new combat encounter, calculating initial initiative.
    /// </summary>
    public void StartCombat()
    {
        _currentRound = 0;
        _initiative.CalculateInitiative();
    }

    /// <summary>
    /// Starts a new round, recalculating initiative based on current AP.
    /// </summary>
    public void StartRound()
    {
        _currentRound++;
        
        // Update AP values in initiative
        foreach (var character in _characters.Values)
        {
            _initiative.UpdateAvailableAP(character.Id, character.ActionPoints.Available);
            _initiative.SetCanAct(character.Id, !character.IsPassedOut);
        }
        
        _initiative.CalculateInitiative();
    }

    /// <summary>
    /// Ends the current round and processes all end-of-round effects.
    /// </summary>
    public async Task<RoundResult> EndRoundAsync()
    {
        var result = new RoundResult 
        { 
            RoundNumber = _currentRound,
            TriggeredEndOfMinute = (_roundsInMinute + 1) >= 20,
            TriggeredEndOfTurn = (_roundsInMinute + 1) >= 20 && (_minutesInTurn + 1) >= 10,
            TriggeredEndOfHour = (_roundsInMinute + 1) >= 20 && (_minutesInTurn + 1) >= 10 && (_turnsInHour + 1) >= 6
        };

        // Process each character
        foreach (var character in _characters.Values)
        {
            var charResult = await ProcessCharacterEndOfRoundAsync(character);
            result.CharacterResults.Add(charResult);

            // Update initiative with new AP values
            _initiative.UpdateAvailableAP(character.Id, character.ActionPoints.Available);
            _initiative.SetCanAct(character.Id, !character.IsPassedOut);
        }

        // Track time boundaries
        _roundsInMinute++;
        if (_roundsInMinute >= 20)
        {
            _roundsInMinute = 0;
            _minutesInTurn++;
            MinuteEnded?.Invoke(this, _currentRound / 20);

            if (_minutesInTurn >= 10)
            {
                _minutesInTurn = 0;
                _turnsInHour++;
                TurnEnded?.Invoke(this, _currentRound / 200);

                if (_turnsInHour >= 6)
                {
                    _turnsInHour = 0;
                    await ProcessEndOfHourAsync();
                    HourEnded?.Invoke(this, _currentRound / 1200);
                }
            }
        }

        // Generate summary messages
        GenerateSummaryMessages(result);

        RoundEnded?.Invoke(this, result);
        return result;
    }

    /// <summary>
    /// Processes end-of-round for a single character.
    /// </summary>
    private async Task<CharacterRoundResult> ProcessCharacterEndOfRoundAsync(CharacterEdit character)
    {
        var result = new CharacterRoundResult
        {
            CharacterId = character.Id,
            CharacterName = character.Name
        };

        // Capture state before processing
        var apBefore = character.ActionPoints.Available;
        var fatBefore = character.Fatigue.Value;
        var vitBefore = character.Vitality.Value;
        var wasPassedOut = character.IsPassedOut;

        // Process cooldowns
        var cooldownTracker = _cooldowns[character.Id];
        var completedCooldowns = cooldownTracker.AdvanceRound();
        result.CompletedCooldowns = completedCooldowns.Select(c => c.ActionName).ToList();

        // Process effects (if effect manager is available)
        if (_effectManager != null)
        {
            var effectResult = await _effectManager.ProcessEndOfRoundAsync(character.Id);
            result.EffectDamage = effectResult.FatigueDamage + effectResult.VitalityDamage;
            result.ExpiredEffects = effectResult.ExpiredCharacterEffects
                .Select(e => e.Definition?.Name ?? "Unknown")
                .ToList();

            // Apply effect damage to pending pools
            character.Fatigue.PendingDamage += effectResult.FatigueDamage;
            character.Vitality.PendingDamage += effectResult.VitalityDamage;
        }

        // Call the existing EndOfRound methods
        character.EndOfRound();

        // Calculate what changed
        result.APRecovered = character.ActionPoints.Available - apBefore + character.ActionPoints.Spent;
        result.FATRecovered = Math.Max(0, character.Fatigue.Value - fatBefore);
        result.FATDamageApplied = Math.Max(0, fatBefore - character.Fatigue.Value);
        result.VITDamageApplied = Math.Max(0, vitBefore - character.Vitality.Value);
        result.PassedOut = character.IsPassedOut && !wasPassedOut;
        result.Died = character.Vitality.Value <= 0;

        return result;
    }

    /// <summary>
    /// Processes end-of-hour effects (VIT recovery).
    /// </summary>
    private async Task ProcessEndOfHourAsync()
    {
        foreach (var character in _characters.Values)
        {
            // VIT recovery: +1 if VIT > 0
            if (character.Vitality.Value > 0 && character.Vitality.Value < character.Vitality.BaseValue)
            {
                character.Vitality.PendingHealing += 1;
            }
        }
        await Task.CompletedTask;
    }

    /// <summary>
    /// Generates human-readable summary messages for the round result.
    /// </summary>
    private void GenerateSummaryMessages(RoundResult result)
    {
        result.SummaryMessages.Add($"Round {result.RoundNumber} complete.");

        foreach (var cr in result.CharacterResults)
        {
            var messages = new List<string>();

            if (cr.APRecovered > 0)
                messages.Add($"+{cr.APRecovered} AP");
            if (cr.FATRecovered > 0)
                messages.Add($"+{cr.FATRecovered} FAT");
            if (cr.FATDamageApplied > 0)
                messages.Add($"-{cr.FATDamageApplied} FAT");
            if (cr.VITDamageApplied > 0)
                messages.Add($"-{cr.VITDamageApplied} VIT");
            if (cr.CompletedCooldowns.Count > 0)
                messages.Add($"{string.Join(", ", cr.CompletedCooldowns)} ready");
            if (cr.ExpiredEffects.Count > 0)
                messages.Add($"{string.Join(", ", cr.ExpiredEffects)} expired");

            if (messages.Count > 0)
            {
                result.SummaryMessages.Add($"  {cr.CharacterName}: {string.Join(", ", messages)}");
            }

            if (cr.PassedOut)
                result.SummaryMessages.Add($"  ⚠️ {cr.CharacterName} passed out!");
            if (cr.Died)
                result.SummaryMessages.Add($"  💀 {cr.CharacterName} died!");
        }

        if (result.TriggeredEndOfHour)
            result.SummaryMessages.Add("⏰ End of hour - VIT recovery processed");
        else if (result.TriggeredEndOfTurn)
            result.SummaryMessages.Add("⏰ End of turn (10 minutes)");
        else if (result.TriggeredEndOfMinute)
            result.SummaryMessages.Add("⏰ End of minute");
    }

    /// <summary>
    /// Ends combat and clears all participants.
    /// </summary>
    public void EndCombat()
    {
        _characters.Clear();
        _cooldowns.Clear();
        _initiative.Clear();
        _currentRound = 0;
        _roundsInMinute = 0;
        _minutesInTurn = 0;
        _turnsInHour = 0;
    }

    /// <summary>
    /// Interrupts a character's cooldowns (e.g., when they take damage).
    /// </summary>
    public void InterruptCooldowns(int characterId)
    {
        if (_cooldowns.TryGetValue(characterId, out var tracker))
        {
            tracker.InterruptAll();
        }
    }
}
