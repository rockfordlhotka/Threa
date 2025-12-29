using System.Collections.Generic;
using Threa.Dal.Dto;

namespace GameMechanics.Actions;

/// <summary>
/// The complete result of an action, including all calculation details.
/// </summary>
public class ActionResult
{
    /// <summary>
    /// The skill used for this action.
    /// </summary>
    public string SkillName { get; set; } = string.Empty;

    /// <summary>
    /// The action type performed.
    /// </summary>
    public ActionType ActionType { get; set; }

    /// <summary>
    /// The calculated Ability Score with all modifiers.
    /// </summary>
    public AbilityScore AbilityScore { get; set; } = new();

    /// <summary>
    /// The Target Value with all modifiers.
    /// </summary>
    public TargetValue TargetValue { get; set; } = new();

    /// <summary>
    /// The 4dF+ dice roll result.
    /// </summary>
    public int DiceRoll { get; set; }

    /// <summary>
    /// The final roll result: AS + DiceRoll.
    /// </summary>
    public int RollResult => AbilityScore.FinalAS + DiceRoll;

    /// <summary>
    /// The Success Value: RollResult - TV.
    /// </summary>
    public int SuccessValue => RollResult - TargetValue.FinalTV;

    /// <summary>
    /// Whether the action succeeded (SV >= 0).
    /// </summary>
    public bool IsSuccess => SuccessValue >= 0;

    /// <summary>
    /// The result quality description based on SV.
    /// </summary>
    public string ResultQuality => GetResultQuality(SuccessValue, IsSuccess);

    /// <summary>
    /// The cost paid for this action.
    /// </summary>
    public ActionCost Cost { get; set; } = ActionCost.Standard();

    /// <summary>
    /// Target description (for UI display).
    /// </summary>
    public string? TargetDescription { get; set; }

    /// <summary>
    /// Additional notes or effect descriptions.
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Gets the result quality description.
    /// </summary>
    private static string GetResultQuality(int sv, bool success)
    {
        if (!success)
        {
            return sv switch
            {
                >= -2 => "Minor Failure",
                >= -4 => "Failure",
                >= -6 => "Bad Failure",
                >= -8 => "Severe Failure",
                _ => "Critical Failure"
            };
        }
        else
        {
            return sv switch
            {
                <= 1 => "Marginal Success",
                <= 3 => "Standard Success",
                <= 5 => "Good Success",
                <= 7 => "Excellent Success",
                _ => "Outstanding Success"
            };
        }
    }

    /// <summary>
    /// Gets a formatted summary for display.
    /// </summary>
    public string GetSummary()
    {
        var result = IsSuccess ? "SUCCESS" : "FAILURE";
        return $"{SkillName}: {result} ({ResultQuality})\n" +
               $"Roll: {AbilityScore.FinalAS} + ({DiceRoll}) = {RollResult} vs TV {TargetValue.FinalTV}\n" +
               $"SV: {SuccessValue}";
    }

    /// <summary>
    /// Gets a detailed breakdown for display.
    /// </summary>
    public string GetDetailedBreakdown()
    {
        var lines = new List<string>
        {
            $"=== {SkillName} ===",
            "",
            "-- Ability Score --",
            AbilityScore.GetBreakdown(),
            "",
            "-- Target Value --",
            TargetValue.GetBreakdown(),
            "",
            "-- Roll --",
            $"4dF+: {(DiceRoll >= 0 ? "+" : "")}{DiceRoll}",
            $"Roll Result: {AbilityScore.FinalAS} + {DiceRoll} = {RollResult}",
            "",
            "-- Result --",
            $"Success Value: {RollResult} - {TargetValue.FinalTV} = {SuccessValue}",
            $"Outcome: {(IsSuccess ? "SUCCESS" : "FAILURE")}",
            $"Quality: {ResultQuality}",
            "",
            "-- Cost --",
            Cost.ToString()
        };

        if (!string.IsNullOrEmpty(Notes))
        {
            lines.Add("");
            lines.Add("-- Notes --");
            lines.Add(Notes);
        }

        return string.Join("\n", lines);
    }
}
