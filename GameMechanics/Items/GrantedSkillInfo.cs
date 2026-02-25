using System;

namespace GameMechanics.Items;

/// <summary>
/// A skill granted to the character by a loaded skill chip.
/// </summary>
public record GrantedSkillInfo(
    string SkillName,
    int SkillLevel,
    string PrimaryAttribute,
    string ChipName,
    Guid ChipItemId);
