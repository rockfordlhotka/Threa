using System;
using System.Collections.Generic;
using System.Linq;
using Csla;
using Threa.Dal.Dto;

namespace GameMechanics.Items;

/// <summary>
/// Editable child collection of item effects for an ItemTemplateEdit.
/// Effects are stored as part of the ItemTemplate and saved/loaded together with it.
/// </summary>
[Serializable]
public class ItemEffectEditList : BusinessListBase<ItemEffectEditList, ItemEffectEdit>
{
    [CreateChild]
    private void Create()
    {
        // Empty list for new item templates
    }

    [FetchChild]
    private void Fetch(List<ItemEffectDefinition> effects, [Inject] IChildDataPortal<ItemEffectEdit> childPortal)
    {
        foreach (var dto in effects)
        {
            Add(childPortal.FetchChild(dto));
        }
    }

    /// <summary>
    /// Adds a new effect to the list.
    /// </summary>
    /// <param name="itemTemplateId">The parent item template ID.</param>
    /// <param name="childPortal">The child data portal for creating new effects.</param>
    /// <returns>The newly created effect.</returns>
    public ItemEffectEdit AddNewEffect(int itemTemplateId, IChildDataPortal<ItemEffectEdit> childPortal)
    {
        var effect = childPortal.CreateChild(itemTemplateId);
        Add(effect);
        return effect;
    }

    /// <summary>
    /// Converts all effects in this list to DTOs for persistence.
    /// </summary>
    internal List<ItemEffectDefinition> ToDtoList()
    {
        var list = new List<ItemEffectDefinition>();
        int localId = 1;

        foreach (var effect in this)
        {
            var dto = effect.ToDto();
            // Assign sequential IDs for effects that don't have one yet
            if (dto.Id <= 0)
            {
                dto.Id = localId++;
            }
            else
            {
                // Make sure localId stays above existing IDs
                if (dto.Id >= localId)
                    localId = dto.Id + 1;
            }
            list.Add(dto);
        }

        return list;
    }

    /// <summary>
    /// Gets effects filtered by trigger type.
    /// </summary>
    public IEnumerable<ItemEffectEdit> GetByTrigger(ItemEffectTrigger trigger)
    {
        return this.Where(e => e.Trigger == trigger);
    }

    /// <summary>
    /// Gets all active effects.
    /// </summary>
    public IEnumerable<ItemEffectEdit> GetActiveEffects()
    {
        return this.Where(e => e.IsActive);
    }

    /// <summary>
    /// Gets all cursed effects.
    /// </summary>
    public IEnumerable<ItemEffectEdit> GetCursedEffects()
    {
        return this.Where(e => e.IsCursed);
    }
}
