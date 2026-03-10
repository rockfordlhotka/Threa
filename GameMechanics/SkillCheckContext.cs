namespace GameMechanics;

/// <summary>
/// Represents a skill available for a skill check — either a native character skill
/// or a skill granted by a loaded skill chip.
/// </summary>
public record SkillCheckContext(
    string Name,
    string PrimaryAttribute,
    int AbilityScore,
    bool IsChipSkill = false,
    string SkillId = "",
    int PreUseConcentrationRounds = 0,
    int PostUseConcentrationRounds = 0,
    int PostUseInterruptionPenaltyRounds = 0);
