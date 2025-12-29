using System;
using Threa.Dal.Dto;

namespace GameMechanics.Actions;

/// <summary>
/// Resolves sprint/movement actions using the action system.
/// Sprint is a skill-based action where success determines distance achieved.
/// </summary>
public class SprintResolver
{
    private readonly ActionResolver _actionResolver;

    /// <summary>
    /// Creates a SprintResolver with the default random dice roller.
    /// </summary>
    public SprintResolver() : this(new RandomDiceRoller())
    {
    }

    /// <summary>
    /// Creates a SprintResolver with an injected dice roller.
    /// </summary>
    /// <param name="diceRoller">The dice roller to use for action resolution.</param>
    public SprintResolver(IDiceRoller diceRoller)
    {
        _actionResolver = new ActionResolver(diceRoller);
    }

    /// <summary>
    /// Creates a SprintResolver with an existing ActionResolver.
    /// </summary>
    /// <param name="actionResolver">The action resolver to use.</param>
    public SprintResolver(ActionResolver actionResolver)
    {
        _actionResolver = actionResolver ?? throw new ArgumentNullException(nameof(actionResolver));
    }

    /// <summary>
    /// Creates the Sprint skill definition.
    /// Sprint uses DEX (Dodge) as its primary attribute.
    /// </summary>
    public static Skill CreateSprintSkill()
    {
        return new Skill
        {
            Id = "sprint",
            Name = "Sprint",
            Category = "Movement",
            PrimaryAttribute = "DEX",
            ActionType = ActionType.Movement,
            TargetValueType = TargetValueType.Fixed,
            DefaultTV = 6, // Routine difficulty for normal terrain
            ResultTable = ResultTableType.Movement,
            AppliesPhysicalityBonus = false,
            RequiresTarget = false,
            RequiresLineOfSight = false,
            IsFreeAction = false,
            IsPassive = false,
            ActionDescription = "Move quickly across the battlefield. Success determines distance achieved."
        };
    }

    /// <summary>
    /// Performs free positioning (no action cost, no roll needed).
    /// </summary>
    /// <param name="requestedRange">The range to move (0-2 allowed).</param>
    /// <returns>The movement result.</returns>
    public MovementResult FreePositioning(int requestedRange = 2)
    {
        int clampedRange = Math.Clamp(requestedRange, 0, Movement.FreePositioningRange);
        
        return new MovementResult
        {
            MovementType = MovementType.FreePositioning,
            BaseRange = clampedRange,
            AchievedRange = clampedRange,
            IsSuccess = true,
            HasMishap = false,
            Description = "Free positioning (no action cost)"
        };
    }

    /// <summary>
    /// Performs a standard sprint action (1 AP + 1 FAT).
    /// </summary>
    /// <param name="sprintSkill">The sprint skill definition.</param>
    /// <param name="skillLevel">Character's sprint skill level.</param>
    /// <param name="dexterityValue">Character's DEX attribute value.</param>
    /// <param name="terrainModifier">Terrain difficulty modifier (0 for normal).</param>
    /// <param name="woundCount">Number of wounds affecting movement.</param>
    /// <param name="isMultipleAction">Whether this is not the first action this round.</param>
    /// <param name="boostAP">Additional AP spent for boost.</param>
    /// <param name="boostFAT">Additional FAT spent for boost.</param>
    /// <returns>The movement result.</returns>
    public MovementResult Sprint(
        Skill sprintSkill,
        int skillLevel,
        int dexterityValue,
        int terrainModifier = 0,
        int woundCount = 0,
        bool isMultipleAction = false,
        int boostAP = 0,
        int boostFAT = 0)
    {
        return PerformMovementAction(
            sprintSkill,
            skillLevel,
            dexterityValue,
            MovementType.Sprint,
            Movement.SprintActionRange,
            terrainModifier,
            woundCount,
            isMultipleAction,
            boostAP,
            boostFAT);
    }

    /// <summary>
    /// Performs a full-round sprint (uses entire round for maximum distance).
    /// </summary>
    /// <param name="sprintSkill">The sprint skill definition.</param>
    /// <param name="skillLevel">Character's sprint skill level.</param>
    /// <param name="dexterityValue">Character's DEX attribute value.</param>
    /// <param name="terrainModifier">Terrain difficulty modifier.</param>
    /// <param name="woundCount">Number of wounds affecting movement.</param>
    /// <param name="boostAP">Additional AP spent for boost.</param>
    /// <param name="boostFAT">Additional FAT spent for boost.</param>
    /// <returns>The movement result.</returns>
    public MovementResult FullRoundSprint(
        Skill sprintSkill,
        int skillLevel,
        int dexterityValue,
        int terrainModifier = 0,
        int woundCount = 0,
        int boostAP = 0,
        int boostFAT = 0)
    {
        // Full-round sprint cannot be a multiple action
        return PerformMovementAction(
            sprintSkill,
            skillLevel,
            dexterityValue,
            MovementType.FullRoundSprint,
            Movement.FullRoundSprintRange,
            terrainModifier,
            woundCount,
            isMultipleAction: false,
            boostAP,
            boostFAT);
    }

    /// <summary>
    /// Creates a sprint action request for preview (cost calculation, AS preview).
    /// </summary>
    public ActionRequest CreateSprintRequest(
        Skill sprintSkill,
        int skillLevel,
        int dexterityValue,
        int terrainModifier = 0,
        int woundCount = 0,
        bool isMultipleAction = false,
        int boostAP = 0,
        int boostFAT = 0)
    {
        var request = new ActionRequest
        {
            Skill = sprintSkill,
            SkillLevel = skillLevel,
            AttributeName = "DEX",
            AttributeValue = dexterityValue,
            WoundCount = woundCount,
            IsMultipleAction = isMultipleAction,
            BoostAP = boostAP,
            BoostFAT = boostFAT
        };

        // Apply terrain modifier if any
        if (terrainModifier != 0)
        {
            request.AdditionalModifiers.Add(
                new AsModifier(ModifierSource.Environment, "Terrain", terrainModifier));
        }

        return request;
    }

    private MovementResult PerformMovementAction(
        Skill sprintSkill,
        int skillLevel,
        int dexterityValue,
        MovementType movementType,
        int baseRange,
        int terrainModifier,
        int woundCount,
        bool isMultipleAction,
        int boostAP,
        int boostFAT)
    {
        var request = CreateSprintRequest(
            sprintSkill,
            skillLevel,
            dexterityValue,
            terrainModifier,
            woundCount,
            isMultipleAction,
            boostAP,
            boostFAT);

        // Resolve the action
        var actionResult = _actionResolver.Resolve(request);

        // Get the movement interpretation
        var interpretation = ResultTables.GetResult(actionResult.SuccessValue, ResultTableType.Movement);

        // Calculate achieved range
        int rangeModifier = interpretation.EffectValue;
        int achievedRange;
        bool hasMishap = false;

        if (rangeModifier <= -99)
        {
            // Critical failure - no movement, possibly fell
            achievedRange = 0;
            hasMishap = true;
        }
        else
        {
            achievedRange = Math.Max(0, baseRange + rangeModifier);
        }

        return new MovementResult
        {
            MovementType = movementType,
            BaseRange = baseRange,
            AchievedRange = achievedRange,
            IsSuccess = actionResult.IsSuccess,
            HasMishap = hasMishap,
            Description = interpretation.Label,
            ActionResult = actionResult
        };
    }
}

/// <summary>
/// Standard terrain modifiers affecting movement TV.
/// </summary>
public static class TerrainModifiers
{
    /// <summary>
    /// Normal, clear terrain - no modifier.
    /// </summary>
    public const int Clear = 0;

    /// <summary>
    /// Rough ground (rocks, debris) - harder to move quickly.
    /// </summary>
    public const int RoughGround = -1;

    /// <summary>
    /// Dense vegetation (forest, tall grass) - impedes movement.
    /// </summary>
    public const int DenseVegetation = -1;

    /// <summary>
    /// Steep uphill slope - harder to sprint.
    /// </summary>
    public const int SteepUphillSlope = -2;

    /// <summary>
    /// Shallow water (ankle to knee deep) - slows movement.
    /// </summary>
    public const int ShallowWater = -2;

    /// <summary>
    /// Slippery surface (ice, wet stone) - dangerous to run.
    /// </summary>
    public const int SlipperySurface = -2;

    /// <summary>
    /// Deep sand or mud - very difficult to move quickly.
    /// </summary>
    public const int DeepSandOrMud = -3;

    /// <summary>
    /// Steep downhill slope - easier to sprint (but may be dangerous).
    /// </summary>
    public const int SteepDownhillSlope = 1;
}
