using Threa.Dal.Dto;

namespace GameMechanics.Actions;

/// <summary>
/// Service that resolves skill-based actions following the 
/// universal action resolution framework defined in ACTIONS.md.
/// </summary>
public class ActionResolver
{
    /// <summary>
    /// Resolves an action and returns the complete result.
    /// </summary>
    /// <param name="request">The action request with all parameters.</param>
    /// <returns>The action result with all calculated values.</returns>
    public ActionResult Resolve(ActionRequest request)
    {
        // 1. Build the ability score
        var abilityScore = BuildAbilityScore(request);

        // 2. Determine the target value
        var targetValue = request.TargetValue ?? BuildDefaultTargetValue(request.Skill);

        // 3. Calculate the cost
        var cost = BuildCost(request);

        // 4. Apply boosts to ability score
        if (cost.BoostBonus > 0)
        {
            abilityScore.ApplyBoostBonus(cost.BoostBonus);
        }

        // 5. Roll the dice
        int diceRoll = request.OverrideDiceRoll ?? Dice.Roll4dFPlus();

        // 6. Build and return the result
        return new ActionResult
        {
            SkillName = request.Skill.Name,
            ActionType = request.Skill.ActionType,
            AbilityScore = abilityScore,
            TargetValue = targetValue,
            DiceRoll = diceRoll,
            Cost = cost,
            TargetDescription = request.TargetDescription
        };
    }

    /// <summary>
    /// Calculates the Ability Score without rolling (for preview).
    /// </summary>
    public AbilityScore CalculateAbilityScore(ActionRequest request)
    {
        var abilityScore = BuildAbilityScore(request);
        
        // Include boost preview
        int totalBoost = request.BoostAP + request.BoostFAT;
        if (totalBoost > 0)
        {
            abilityScore.ApplyBoostBonus(totalBoost);
        }

        return abilityScore;
    }

    /// <summary>
    /// Checks if the character has sufficient resources for the action.
    /// </summary>
    /// <param name="request">The action request.</param>
    /// <param name="currentAP">Character's current available AP.</param>
    /// <param name="currentFAT">Character's current FAT.</param>
    /// <param name="currentMana">Character's current Mana (if applicable).</param>
    /// <returns>True if the character can afford the action.</returns>
    public bool CanAfford(ActionRequest request, int currentAP, int currentFAT, int currentMana = int.MaxValue)
    {
        var cost = BuildCost(request);
        return currentAP >= cost.TotalAP && 
               currentFAT >= cost.TotalFAT && 
               currentMana >= cost.Mana;
    }

    /// <summary>
    /// Gets a preview of the action cost.
    /// </summary>
    public ActionCost GetCost(ActionRequest request)
    {
        return BuildCost(request);
    }

    private AbilityScore BuildAbilityScore(ActionRequest request)
    {
        var abilityScore = new AbilityScore
        {
            SkillName = request.Skill.Name,
            AttributeName = request.AttributeName,
            AttributeValue = request.AttributeValue,
            SkillLevel = request.SkillLevel
        };

        // Apply penalties
        if (request.IsMultipleAction)
        {
            abilityScore.ApplyMultipleActionPenalty();
        }

        if (request.WoundCount > 0)
        {
            abilityScore.ApplyWoundPenalties(request.WoundCount);
        }

        if (request.HasAimed && request.Skill.ActionType == ActionType.Attack)
        {
            abilityScore.ApplyAimBonus();
        }

        // Apply additional modifiers
        abilityScore.Modifiers.AddRange(request.AdditionalModifiers);

        return abilityScore;
    }

    private TargetValue BuildDefaultTargetValue(Skill skill)
    {
        return skill.TargetValueType switch
        {
            TargetValueType.Fixed => TargetValue.Fixed(skill.DefaultTV, skill.Name + " (Default)"),
            TargetValueType.Passive => TargetValue.Fixed(skill.DefaultTV, skill.Name + " (Default)"),
            TargetValueType.Opposed => TargetValue.Fixed(skill.DefaultTV, skill.Name + " (No opponent specified)"),
            _ => TargetValue.Fixed(6, "Routine")
        };
    }

    private ActionCost BuildCost(ActionRequest request)
    {
        ActionCost cost;

        if (request.Skill.IsFreeAction)
        {
            cost = ActionCost.Free();
        }
        else if (request.Skill.ManaCost > 0)
        {
            cost = ActionCost.Spell(request.Skill.ManaCost);
        }
        else if (request.UseFatigueFree)
        {
            cost = ActionCost.FatigueFree();
        }
        else
        {
            cost = ActionCost.Standard();
        }

        cost.BoostAP = request.BoostAP;
        cost.BoostFAT = request.BoostFAT;

        return cost;
    }
}
