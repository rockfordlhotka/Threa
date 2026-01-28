using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb;

/// <summary>
/// Mock implementation of IEffectTemplateDal for development and testing.
/// </summary>
public class EffectTemplateDal : IEffectTemplateDal
{
    public Task<List<EffectTemplateDto>> GetAllTemplatesAsync()
    {
        var templates = MockDb.EffectTemplates.Where(t => t.IsActive).ToList();
        return Task.FromResult(templates);
    }

    public Task<List<EffectTemplateDto>> GetTemplatesByTypeAsync(EffectType type)
    {
        var templates = MockDb.EffectTemplates
            .Where(t => t.IsActive && t.EffectType == type)
            .ToList();
        return Task.FromResult(templates);
    }

    public Task<List<EffectTemplateDto>> SearchTemplatesAsync(string searchTerm)
    {
        var term = searchTerm.ToLowerInvariant();
        var templates = MockDb.EffectTemplates
            .Where(t => t.IsActive &&
                (t.Name.ToLowerInvariant().Contains(term) ||
                 (t.Tags?.ToLowerInvariant().Contains(term) ?? false) ||
                 (t.Description?.ToLowerInvariant().Contains(term) ?? false)))
            .ToList();
        return Task.FromResult(templates);
    }

    public Task<EffectTemplateDto?> GetTemplateAsync(int id)
    {
        var template = MockDb.EffectTemplates.FirstOrDefault(t => t.Id == id);
        return Task.FromResult(template);
    }

    public Task<EffectTemplateDto> SaveTemplateAsync(EffectTemplateDto template)
    {
        if (template.Id == 0)
        {
            template.Id = MockDb.EffectTemplates.Count == 0 ?
                1 : MockDb.EffectTemplates.Max(t => t.Id) + 1;
            template.CreatedAt = DateTime.UtcNow;
            MockDb.EffectTemplates.Add(template);
        }
        else
        {
            var existing = MockDb.EffectTemplates.FirstOrDefault(t => t.Id == template.Id);
            if (existing == null)
                throw new NotFoundException($"EffectTemplate {template.Id}");
            template.UpdatedAt = DateTime.UtcNow;
            MockDb.EffectTemplates.Remove(existing);
            MockDb.EffectTemplates.Add(template);
        }
        return Task.FromResult(template);
    }

    public Task DeleteTemplateAsync(int id)
    {
        var template = MockDb.EffectTemplates.FirstOrDefault(t => t.Id == id);
        if (template == null)
            throw new NotFoundException($"EffectTemplate {id}");
        if (template.IsSystem)
            throw new InvalidOperationException("Cannot delete system templates");
        template.IsActive = false;
        return Task.CompletedTask;
    }
}
