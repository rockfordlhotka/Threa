using System;
using Threa.Dal.Dto;

namespace GameMechanics.Actions;

/// <summary>
/// Request for a medical skill check.
/// </summary>
public class MedicalRequest
{
    /// <summary>
    /// Display name of the skill (e.g., "First-Aid", "Nursing", "Doctor").
    /// </summary>
    public string SkillDisplayName { get; set; } = "";

    /// <summary>
    /// The healer's pre-computed ability score, including attribute, skill bonus,
    /// wound penalties, and any active effect modifiers. Comes from SkillEdit.AbilityScore.
    /// </summary>
    public int AbilityScore { get; set; }

    /// <summary>
    /// Name of the attribute for display (e.g., "WIL", "INT").
    /// </summary>
    public string AttributeName { get; set; } = "";

    /// <summary>
    /// Number of concentration rounds required after a successful check.
    /// Comes from the skill definition's PostUseConcentrationRounds.
    /// </summary>
    public int ConcentrationRounds { get; set; }

    /// <summary>
    /// Whether this is a multiple action in the round (-1 AS penalty).
    /// </summary>
    public bool IsMultipleAction { get; set; }

    /// <summary>
    /// AP/FAT boost to add to the ability score. Each point costs 1 AP + 1 FAT (or 2 AP).
    /// </summary>
    public int Boost { get; set; }
}

/// <summary>
/// Result of a medical skill check.
/// </summary>
public class MedicalCheckResult
{
    /// <summary>
    /// Whether the check succeeded (SV >= 0).
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The Success Value from the roll.
    /// </summary>
    public int SuccessValue { get; set; }

    /// <summary>
    /// The calculated healing amount based on SV.
    /// </summary>
    public int HealingAmount { get; set; }

    /// <summary>
    /// Number of concentration rounds required.
    /// </summary>
    public int ConcentrationRounds { get; set; }

    /// <summary>
    /// The total roll result (AS + dice).
    /// </summary>
    public int TotalRoll { get; set; }

    /// <summary>
    /// The dice roll portion.
    /// </summary>
    public int DiceRoll { get; set; }

    /// <summary>
    /// The ability score used.
    /// </summary>
    public int AbilityScore { get; set; }

    /// <summary>
    /// Description of the result for display.
    /// </summary>
    public string ResultDescription { get; set; } = "";
}

/// <summary>
/// Service that resolves medical skill checks for First-Aid, Nursing, and Doctor skills.
/// Medical checks roll immediately, then apply a concentration effect.
/// On completion, healing is applied based on the SV from the initial roll.
/// </summary>
public class MedicalResolver
{
    /// <summary>
    /// Fixed Target Value for all medical checks.
    /// </summary>
    public const int MedicalTV = 8;

    private readonly IDiceRoller _diceRoller;

    public MedicalResolver() : this(new RandomDiceRoller())
    {
    }

    public MedicalResolver(IDiceRoller diceRoller)
    {
        _diceRoller = diceRoller ?? throw new ArgumentNullException(nameof(diceRoller));
    }

    /// <summary>
    /// Resolves a medical skill check, returning the result including SV and healing amount.
    /// The roll happens immediately; concentration determines when healing applies.
    /// </summary>
    public MedicalCheckResult ResolveCheck(MedicalRequest request)
    {
        // Start from the pre-computed ability score (includes attribute, skill, wounds, effects)
        int abilityScore = request.AbilityScore;

        // Apply multiple action penalty
        if (request.IsMultipleAction)
        {
            abilityScore -= 1;
        }

        // Apply boost
        if (request.Boost > 0)
        {
            abilityScore += request.Boost;
        }

        // Roll dice
        int diceRoll = _diceRoller.Roll4dFPlus();
        int totalRoll = abilityScore + diceRoll;

        // Calculate SV
        int sv = totalRoll - MedicalTV;

        // Get healing amount from result table
        var healingResult = ResultTables.GetResult(sv, ResultTableType.Healing);
        int healingAmount = healingResult.IsSuccess ? healingResult.EffectValue : 0;

        return new MedicalCheckResult
        {
            Success = sv >= 0,
            SuccessValue = sv,
            HealingAmount = healingAmount,
            ConcentrationRounds = request.ConcentrationRounds,
            TotalRoll = totalRoll,
            DiceRoll = diceRoll,
            AbilityScore = abilityScore,
            ResultDescription = healingResult.Description
        };
    }
}
