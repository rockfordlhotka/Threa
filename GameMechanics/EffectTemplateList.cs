using Csla;
using System;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics;

/// <summary>
/// Read-only CSLA list of effect templates.
/// Supports fetching all templates, filtering by type, and searching.
/// </summary>
[Serializable]
public class EffectTemplateList : ReadOnlyListBase<EffectTemplateList, EffectTemplate>
{
    #region Factory Methods

    /// <summary>
    /// Gets all active effect templates.
    /// </summary>
    public static async Task<EffectTemplateList> GetAllAsync(IDataPortal<EffectTemplateList> portal)
    {
        return await portal.FetchAsync();
    }

    /// <summary>
    /// Gets effect templates filtered by type.
    /// </summary>
    public static async Task<EffectTemplateList> GetByTypeAsync(IDataPortal<EffectTemplateList> portal, EffectType type)
    {
        return await portal.FetchAsync(type);
    }

    /// <summary>
    /// Searches for effect templates by name, tags, or description.
    /// </summary>
    public static async Task<EffectTemplateList> SearchAsync(IDataPortal<EffectTemplateList> portal, string searchTerm)
    {
        return await portal.FetchAsync(searchTerm);
    }

    #endregion

    #region Data Access

    [Fetch]
    private async Task FetchAsync(
        [Inject] IEffectTemplateDal dal,
        [Inject] IChildDataPortal<EffectTemplate> childPortal)
    {
        var dtos = await dal.GetAllTemplatesAsync();
        using (LoadListMode)
        {
            foreach (var dto in dtos)
            {
                Add(childPortal.FetchChild(dto));
            }
        }
    }

    [Fetch]
    private async Task FetchAsync(
        EffectType effectType,
        [Inject] IEffectTemplateDal dal,
        [Inject] IChildDataPortal<EffectTemplate> childPortal)
    {
        var dtos = await dal.GetTemplatesByTypeAsync(effectType);
        using (LoadListMode)
        {
            foreach (var dto in dtos)
            {
                Add(childPortal.FetchChild(dto));
            }
        }
    }

    [Fetch]
    private async Task FetchAsync(
        string searchTerm,
        [Inject] IEffectTemplateDal dal,
        [Inject] IChildDataPortal<EffectTemplate> childPortal)
    {
        var dtos = await dal.SearchTemplatesAsync(searchTerm);
        using (LoadListMode)
        {
            foreach (var dto in dtos)
            {
                Add(childPortal.FetchChild(dto));
            }
        }
    }

    #endregion
}
