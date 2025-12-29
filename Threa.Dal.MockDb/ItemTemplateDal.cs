using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb;

/// <summary>
/// Mock implementation of IItemTemplateDal for development and testing.
/// </summary>
public class ItemTemplateDal : IItemTemplateDal
{
    public Task<List<ItemTemplate>> GetAllTemplatesAsync()
    {
        var templates = MockDb.ItemTemplates.Where(t => t.IsActive).ToList();
        return Task.FromResult(templates);
    }

    public Task<List<ItemTemplate>> GetTemplatesByTypeAsync(ItemType itemType)
    {
        var templates = MockDb.ItemTemplates
            .Where(t => t.IsActive && t.ItemType == itemType)
            .ToList();
        return Task.FromResult(templates);
    }

    public Task<ItemTemplate> GetTemplateAsync(int id)
    {
        var template = MockDb.ItemTemplates.FirstOrDefault(t => t.Id == id);
        if (template == null)
            throw new NotFoundException($"ItemTemplate {id}");
        return Task.FromResult(template);
    }

    public Task<List<ItemTemplate>> SearchTemplatesAsync(string searchTerm)
    {
        var term = searchTerm.ToLowerInvariant();
        var templates = MockDb.ItemTemplates
            .Where(t => t.IsActive && 
                (t.Name.ToLowerInvariant().Contains(term) || 
                 t.Description.ToLowerInvariant().Contains(term)))
            .ToList();
        return Task.FromResult(templates);
    }

    public Task<ItemTemplate> SaveTemplateAsync(ItemTemplate template)
    {
        if (template.Id == 0)
        {
            template.Id = MockDb.ItemTemplates.Count == 0 ? 
                1 : MockDb.ItemTemplates.Max(t => t.Id) + 1;
            MockDb.ItemTemplates.Add(template);
        }
        else
        {
            var existing = MockDb.ItemTemplates.FirstOrDefault(t => t.Id == template.Id);
            if (existing == null)
                throw new NotFoundException($"ItemTemplate {template.Id}");
            MockDb.ItemTemplates.Remove(existing);
            MockDb.ItemTemplates.Add(template);
        }
        return Task.FromResult(template);
    }

    public Task DeactivateTemplateAsync(int id)
    {
        var template = MockDb.ItemTemplates.FirstOrDefault(t => t.Id == id);
        if (template == null)
            throw new NotFoundException($"ItemTemplate {id}");
        template.IsActive = false;
        return Task.CompletedTask;
    }
}
