using Threa.Dal.Dto;

namespace GameMechanics.Actions;

/// <summary>
/// Extension methods and helpers for building ActionRequests from 
/// character and skill data.
/// </summary>
public static class ActionRequestBuilder
{
    /// <summary>
    /// Creates an ActionRequest from a SkillEdit and CharacterEdit.
    /// </summary>
    /// <param name="skillEdit">The character's skill.</param>
    /// <param name="character">The character performing the action.</param>
    /// <param name="skillDefinition">The skill definition with action properties.</param>
    /// <returns>A populated ActionRequest ready for resolution.</returns>
    public static ActionRequest FromSkillEdit(
        SkillEdit skillEdit, 
        CharacterEdit character, 
        Skill skillDefinition)
    {
        // Calculate attribute value from primary attribute(s)
        int attributeValue = SkillEdit.GetAttributeBase(character, skillEdit.PrimaryAttribute);
        
        // Count wounds for penalty calculation
        int woundCount = character.Effects?.TotalWoundCount ?? 0;

        return new ActionRequest
        {
            Skill = skillDefinition,
            SkillLevel = skillEdit.Level,
            AttributeValue = attributeValue,
            AttributeName = skillEdit.PrimaryAttribute,
            WoundCount = woundCount
        };
    }

    /// <summary>
    /// Creates an ActionRequest for a weapon attack.
    /// </summary>
    public static ActionRequest ForWeaponAttack(
        Skill weaponSkill,
        int skillLevel,
        int attributeValue,
        string attributeName,
        int woundCount = 0,
        bool isMultipleAction = false,
        TargetValue? targetValue = null)
    {
        return new ActionRequest
        {
            Skill = weaponSkill,
            SkillLevel = skillLevel,
            AttributeValue = attributeValue,
            AttributeName = attributeName,
            WoundCount = woundCount,
            IsMultipleAction = isMultipleAction,
            TargetValue = targetValue
        };
    }

    /// <summary>
    /// Creates an ActionRequest for a ranged attack with range category.
    /// </summary>
    public static ActionRequest ForRangedAttack(
        Skill weaponSkill,
        int skillLevel,
        int attributeValue,
        string attributeName,
        RangeCategory range,
        int woundCount = 0,
        bool isMultipleAction = false,
        bool hasAimed = false)
    {
        // Base TV for ranged is by range category: 6/8/10/12
        int baseTV = range switch
        {
            RangeCategory.Short => 6,
            RangeCategory.Medium => 8,
            RangeCategory.Long => 10,
            RangeCategory.Extreme => 12,
            _ => 6
        };

        var targetValue = TargetValue.Fixed(baseTV, $"{range} Range");

        return new ActionRequest
        {
            Skill = weaponSkill,
            SkillLevel = skillLevel,
            AttributeValue = attributeValue,
            AttributeName = attributeName,
            WoundCount = woundCount,
            IsMultipleAction = isMultipleAction,
            HasAimed = hasAimed,
            TargetValue = targetValue
        };
    }

    /// <summary>
    /// Creates an ActionRequest for a defense action (dodge/parry).
    /// </summary>
    public static ActionRequest ForDefense(
        Skill defenseSkill,
        int skillLevel,
        int attributeValue,
        string attributeName,
        int attackerRollResult,
        int woundCount = 0,
        bool isMultipleAction = false)
    {
        // For defense, TV is the attacker's roll result
        var targetValue = TargetValue.Fixed(attackerRollResult, "Attacker's Attack");

        return new ActionRequest
        {
            Skill = defenseSkill,
            SkillLevel = skillLevel,
            AttributeValue = attributeValue,
            AttributeName = attributeName,
            WoundCount = woundCount,
            IsMultipleAction = isMultipleAction,
            TargetValue = targetValue
        };
    }

    /// <summary>
    /// Creates an ActionRequest for an opposed skill check.
    /// </summary>
    public static ActionRequest ForOpposedCheck(
        Skill skill,
        int skillLevel,
        int attributeValue,
        string attributeName,
        int opponentAS,
        int opponentRoll,
        string opponentDescription = "Opponent",
        int woundCount = 0,
        bool isMultipleAction = false)
    {
        var targetValue = TargetValue.Opposed(opponentAS, opponentRoll, opponentDescription);

        return new ActionRequest
        {
            Skill = skill,
            SkillLevel = skillLevel,
            AttributeValue = attributeValue,
            AttributeName = attributeName,
            WoundCount = woundCount,
            IsMultipleAction = isMultipleAction,
            TargetValue = targetValue
        };
    }

    /// <summary>
    /// Creates an ActionRequest against a passive (non-rolling) defender.
    /// </summary>
    public static ActionRequest AgainstPassiveDefense(
        Skill skill,
        int skillLevel,
        int attributeValue,
        string attributeName,
        int defenderAS,
        string defenderDescription = "Target",
        int woundCount = 0,
        bool isMultipleAction = false)
    {
        var targetValue = TargetValue.Passive(defenderAS, defenderDescription);

        return new ActionRequest
        {
            Skill = skill,
            SkillLevel = skillLevel,
            AttributeValue = attributeValue,
            AttributeName = attributeName,
            WoundCount = woundCount,
            IsMultipleAction = isMultipleAction,
            TargetValue = targetValue
        };
    }

    /// <summary>
    /// Adds range modifiers to a target value for ranged attacks.
    /// </summary>
    public static void AddRangedModifiers(
        TargetValue targetValue,
        bool targetMoving = false,
        bool targetProne = false,
        bool targetCrouching = false,
        bool attackerMoving = false,
        CoverLevel cover = CoverLevel.None,
        TargetSize targetSize = TargetSize.Medium)
    {
        if (targetMoving)
            targetValue.Modifiers.Add(ModifierSource.Environment, "Target Moving", 2);
        if (targetProne)
            targetValue.Modifiers.Add(ModifierSource.Environment, "Target Prone", 2);
        if (targetCrouching)
            targetValue.Modifiers.Add(ModifierSource.Environment, "Target Crouching", 2);
        if (attackerMoving)
            targetValue.Modifiers.Add(ModifierSource.Environment, "Attacker Moving", 2);
        
        int coverMod = cover switch
        {
            CoverLevel.Half => 1,
            CoverLevel.ThreeQuarters => 2,
            _ => 0
        };
        if (coverMod > 0)
            targetValue.Modifiers.Add(ModifierSource.Environment, $"{cover} Cover", coverMod);

        int sizeMod = targetSize switch
        {
            TargetSize.Tiny => 2,
            TargetSize.Small => 1,
            TargetSize.Large => -1,
            TargetSize.Huge => -2,
            _ => 0
        };
        if (sizeMod != 0)
            targetValue.Modifiers.Add(ModifierSource.Environment, $"{targetSize} Target", sizeMod);
    }
}

/// <summary>
/// Cover levels for ranged attack modifiers.
/// </summary>
public enum CoverLevel
{
    None = 0,
    Half = 1,
    ThreeQuarters = 2
}

/// <summary>
/// Target size categories for ranged attack modifiers.
/// </summary>
public enum TargetSize
{
    Tiny = -2,
    Small = -1,
    Medium = 0,
    Large = 1,
    Huge = 2
}
