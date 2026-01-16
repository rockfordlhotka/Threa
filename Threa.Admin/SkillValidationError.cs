namespace Threa.Admin;

/// <summary>
/// Represents a validation error found during skill import.
/// </summary>
public record SkillValidationError(int RowNumber, string ColumnName, string Message);
