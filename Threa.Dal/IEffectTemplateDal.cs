using System.Collections.Generic;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal;

/// <summary>
/// Data access layer for effect templates (system-wide effect library).
/// </summary>
public interface IEffectTemplateDal
{
    /// <summary>
    /// Gets all active templates.
    /// </summary>
    Task<List<EffectTemplateDto>> GetAllTemplatesAsync();

    /// <summary>
    /// Gets templates filtered by effect type.
    /// </summary>
    /// <param name="type">The type of effects to retrieve.</param>
    Task<List<EffectTemplateDto>> GetTemplatesByTypeAsync(EffectType type);

    /// <summary>
    /// Searches for templates by name or tags.
    /// </summary>
    /// <param name="searchTerm">The search term.</param>
    Task<List<EffectTemplateDto>> SearchTemplatesAsync(string searchTerm);

    /// <summary>
    /// Gets a specific template by ID.
    /// </summary>
    /// <param name="id">The template ID.</param>
    Task<EffectTemplateDto?> GetTemplateAsync(int id);

    /// <summary>
    /// Creates or updates an effect template.
    /// </summary>
    /// <param name="template">The template to save.</param>
    Task<EffectTemplateDto> SaveTemplateAsync(EffectTemplateDto template);

    /// <summary>
    /// Soft deletes an effect template (sets IsActive = false).
    /// System templates cannot be deleted.
    /// </summary>
    /// <param name="id">The template ID to delete.</param>
    Task DeleteTemplateAsync(int id);
}
