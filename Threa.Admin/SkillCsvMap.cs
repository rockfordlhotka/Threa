using CsvHelper.Configuration;
using Threa.Dal.Dto;

namespace Threa.Admin;

/// <summary>
/// CsvHelper mapping for Skill import/export.
/// </summary>
public sealed class SkillCsvMap : ClassMap<Skill>
{
    public SkillCsvMap()
    {
        Map(m => m.Id).Name("Id");
        Map(m => m.Name).Name("Name");
        Map(m => m.Category).Name("Category");
        Map(m => m.IsSpecialized).Name("IsSpecialized");
        Map(m => m.IsMagic).Name("IsMagic");
        Map(m => m.IsTheology).Name("IsTheology");
        Map(m => m.IsPsionic).Name("IsPsionic").Optional();
        Map(m => m.Trained).Name("Trained");
        Map(m => m.Untrained).Name("Untrained");
        Map(m => m.PrimaryAttribute).Name("PrimaryAttribute");
        Map(m => m.SecondaryAttribute).Name("SecondaryAttribute").Optional();
        Map(m => m.TertiaryAttribute).Name("TertiaryAttribute").Optional();
        Map(m => m.ImageUrl).Name("ImageUrl").Optional();
    }
}
