using System;
using Threa.Dal.Dto;

namespace GameMechanics.Actions;

/// <summary>
/// Medical skill type enumeration.
/// </summary>
public enum MedicalSkillType
{
    FirstAid,
    Nursing,
    Doctor
}

/// <summary>
/// Request for a medical skill check.
/// </summary>
public class MedicalRequest
{
    /// <summary>
    /// The type of medical skill being used.
    /// </summary>
    public required MedicalSkillType SkillType { get; set; }

    /// <summary>
    /// The healer's skill level.
    /// </summary>
    public int SkillLevel { get; set; }

    /// <summary>
    /// The healer's relevant attribute value (WIL for First-Aid, INT for Nursing/Doctor).
    /// </summary>
    public int AttributeValue { get; set; }

    /// <summary>
    /// Name of the attribute for display.
    /// </summary>
    public string AttributeName { get; set; } = "";

    /// <summary>
    /// Number of active wounds affecting the healer.
    /// </summary>
    public int WoundCount { get; set; }

    /// <summary>
    /// Whether this is a multiple action in the round.
    /// </summary>
    public bool IsMultipleAction { get; set; }
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
        // Calculate ability score: Attribute + Skill Level - 5
        int abilityScore = request.AttributeValue + request.SkillLevel - 5;

        // Apply wound penalty (-2 per wound)
        if (request.WoundCount > 0)
        {
            abilityScore -= request.WoundCount * 2;
        }

        // Apply multiple action penalty
        if (request.IsMultipleAction)
        {
            abilityScore -= 1;
        }

        // Roll dice
        int diceRoll = _diceRoller.Roll4dFPlus();
        int totalRoll = abilityScore + diceRoll;

        // Calculate SV
        int sv = totalRoll - MedicalTV;

        // Get healing amount from result table
        var healingResult = ResultTables.GetResult(sv, ResultTableType.Healing);
        int healingAmount = healingResult.IsSuccess ? healingResult.EffectValue : 0;

        // Get concentration rounds based on skill type
        int concentrationRounds = GetConcentrationRounds(request.SkillType);

        return new MedicalCheckResult
        {
            Success = sv >= 0,
            SuccessValue = sv,
            HealingAmount = healingAmount,
            ConcentrationRounds = concentrationRounds,
            TotalRoll = totalRoll,
            DiceRoll = diceRoll,
            AbilityScore = abilityScore,
            ResultDescription = healingResult.Description
        };
    }

    /// <summary>
    /// Gets the number of concentration rounds required for each medical skill type.
    /// </summary>
    public static int GetConcentrationRounds(MedicalSkillType skillType)
    {
        return skillType switch
        {
            MedicalSkillType.FirstAid => 2,
            MedicalSkillType.Nursing => 3,
            MedicalSkillType.Doctor => 4,
            _ => 2
        };
    }

    /// <summary>
    /// Gets the attribute name used for each medical skill type.
    /// </summary>
    public static string GetAttributeName(MedicalSkillType skillType)
    {
        return skillType switch
        {
            MedicalSkillType.FirstAid => "WIL",
            MedicalSkillType.Nursing => "INT",
            MedicalSkillType.Doctor => "INT",
            _ => "INT"
        };
    }

    /// <summary>
    /// Gets the display name for each medical skill type.
    /// </summary>
    public static string GetSkillDisplayName(MedicalSkillType skillType)
    {
        return skillType switch
        {
            MedicalSkillType.FirstAid => "First-Aid",
            MedicalSkillType.Nursing => "Nursing",
            MedicalSkillType.Doctor => "Doctor",
            _ => "Medical"
        };
    }
}
