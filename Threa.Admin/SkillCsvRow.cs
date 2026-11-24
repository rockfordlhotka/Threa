using CsvHelper;
using Threa.Dal.Dto;

namespace Threa.Admin;

/// <summary>
/// CSV row for Skill import/export. Only these columns are read and written;
/// the remaining Skill properties keep their defaults on import.
/// </summary>
/// <remarks>
/// Every column is optional: a missing column leaves its default value, and
/// <see cref="SkillValidator"/> reports missing required values.
/// </remarks>
[CsvRecord]
public sealed partial class SkillCsvRow
{
    [CsvColumn(Optional = true)] public string Id { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public string Name { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public SkillCategory Category { get; set; } = SkillCategory.Standard;
    [CsvColumn(Optional = true)] public bool IsSpecialized { get; set; }
    [CsvColumn(Optional = true)] public bool IsMagic { get; set; }
    [CsvColumn(Optional = true)] public bool IsTheology { get; set; }
    [CsvColumn(Optional = true)] public bool IsPsionic { get; set; }
    [CsvColumn(Optional = true)] public int Trained { get; set; }
    [CsvColumn(Optional = true)] public int Untrained { get; set; }
    [CsvColumn(Optional = true)] public string PrimaryAttribute { get; set; } = string.Empty;
    [CsvColumn(Optional = true)] public string? SecondaryAttribute { get; set; }
    [CsvColumn(Optional = true)] public string? TertiaryAttribute { get; set; }
    [CsvColumn(Optional = true)] public string? ImageUrl { get; set; }

    public static SkillCsvRow FromSkill(Skill skill) => new()
    {
        Id = skill.Id,
        Name = skill.Name,
        Category = skill.Category,
        IsSpecialized = skill.IsSpecialized,
        IsMagic = skill.IsMagic,
        IsTheology = skill.IsTheology,
        IsPsionic = skill.IsPsionic,
        Trained = skill.Trained,
        Untrained = skill.Untrained,
        PrimaryAttribute = skill.PrimaryAttribute,
        SecondaryAttribute = skill.SecondaryAttribute,
        TertiaryAttribute = skill.TertiaryAttribute,
        ImageUrl = skill.ImageUrl
    };

    public Skill ToSkill() => new()
    {
        Id = Id,
        Name = Name,
        Category = Category,
        IsSpecialized = IsSpecialized,
        IsMagic = IsMagic,
        IsTheology = IsTheology,
        IsPsionic = IsPsionic,
        Trained = Trained,
        Untrained = Untrained,
        PrimaryAttribute = PrimaryAttribute,
        SecondaryAttribute = SecondaryAttribute,
        TertiaryAttribute = TertiaryAttribute,
        ImageUrl = ImageUrl
    };
}
