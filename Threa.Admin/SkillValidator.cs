using Threa.Dal.Dto;

namespace Threa.Admin;

/// <summary>
/// Validates skill data for import.
/// </summary>
public static class SkillValidator
{
    public static readonly string[] ValidAttributes = ["STR", "DEX", "END", "INT", "ITT", "WIL", "PHY", "SOC"];
    public static readonly string[] ValidCategories = Enum.GetNames<SkillCategory>();

    /// <summary>
    /// Validates a list of skills and returns all errors found.
    /// </summary>
    public static List<SkillValidationError> ValidateSkills(List<Skill> skills)
    {
        var errors = new List<SkillValidationError>();
        var seenIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        for (int i = 0; i < skills.Count; i++)
        {
            int rowNumber = i + 2; // +2 because row 1 is header, data starts at row 2
            var skill = skills[i];

            // Validate Id
            if (string.IsNullOrWhiteSpace(skill.Id))
            {
                errors.Add(new SkillValidationError(rowNumber, "Id", "Id is required and cannot be empty"));
            }
            else if (seenIds.Contains(skill.Id))
            {
                errors.Add(new SkillValidationError(rowNumber, "Id", $"Duplicate skill ID '{skill.Id}' (already exists earlier in file)"));
            }
            else
            {
                seenIds.Add(skill.Id);
            }

            // Validate Name
            if (string.IsNullOrWhiteSpace(skill.Name))
            {
                errors.Add(new SkillValidationError(rowNumber, "Name", "Name is required and cannot be empty"));
            }
            else if (seenNames.Contains(skill.Name))
            {
                errors.Add(new SkillValidationError(rowNumber, "Name", $"Duplicate skill name '{skill.Name}' (already exists earlier in file)"));
            }
            else
            {
                seenNames.Add(skill.Name);
            }

            // Validate Category
            if (!Enum.IsDefined(skill.Category))
            {
                errors.Add(new SkillValidationError(rowNumber, "Category",
                    $"Invalid category '{skill.Category}'. Valid values: {string.Join(", ", ValidCategories)}"));
            }

            // Validate PrimaryAttribute
            var primaryAttrError = ValidateAttributeSpec(skill.PrimaryAttribute, "PrimaryAttribute", required: true);
            if (primaryAttrError != null)
            {
                errors.Add(new SkillValidationError(rowNumber, "PrimaryAttribute", primaryAttrError));
            }

            // Validate SecondaryAttribute (optional)
            if (!string.IsNullOrWhiteSpace(skill.SecondaryAttribute))
            {
                var secondaryAttrError = ValidateAttributeSpec(skill.SecondaryAttribute, "SecondaryAttribute", required: false);
                if (secondaryAttrError != null)
                {
                    errors.Add(new SkillValidationError(rowNumber, "SecondaryAttribute", secondaryAttrError));
                }
            }

            // Validate TertiaryAttribute (optional)
            if (!string.IsNullOrWhiteSpace(skill.TertiaryAttribute))
            {
                var tertiaryAttrError = ValidateAttributeSpec(skill.TertiaryAttribute, "TertiaryAttribute", required: false);
                if (tertiaryAttrError != null)
                {
                    errors.Add(new SkillValidationError(rowNumber, "TertiaryAttribute", tertiaryAttrError));
                }
            }

            // Validate Trained (must be positive)
            if (skill.Trained < 1)
            {
                errors.Add(new SkillValidationError(rowNumber, "Trained",
                    $"Trained cost must be a positive integer, got '{skill.Trained}'"));
            }

            // Validate Untrained (must be >= Trained, typically)
            if (skill.Untrained < 1)
            {
                errors.Add(new SkillValidationError(rowNumber, "Untrained",
                    $"Untrained cost must be a positive integer, got '{skill.Untrained}'"));
            }
            else if (skill.Untrained < skill.Trained)
            {
                errors.Add(new SkillValidationError(rowNumber, "Untrained",
                    $"Untrained cost ({skill.Untrained}) should typically be >= Trained cost ({skill.Trained})"));
            }
        }

        return errors;
    }

    /// <summary>
    /// Validates an attribute specification (single attribute or compound like "DEX/ITT").
    /// </summary>
    private static string? ValidateAttributeSpec(string? spec, string columnName, bool required)
    {
        if (string.IsNullOrWhiteSpace(spec))
        {
            return required ? $"{columnName} is required" : null;
        }

        var parts = spec.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        foreach (var part in parts)
        {
            if (!ValidAttributes.Contains(part.ToUpperInvariant()))
            {
                return $"Invalid attribute '{part}' in '{spec}'. Valid attributes: {string.Join(", ", ValidAttributes)}";
            }
        }

        return null;
    }

    /// <summary>
    /// Gets valid category names for error messages.
    /// </summary>
    public static string GetValidCategoriesMessage() => string.Join(", ", ValidCategories);

    /// <summary>
    /// Gets valid attribute codes for error messages.
    /// </summary>
    public static string GetValidAttributesMessage() => string.Join(", ", ValidAttributes);
}
