using Csla;
using GameMechanics.Reference;
using System;
using System.Collections.Generic;
using System.Linq;
using Threa.Dal.Dto;

namespace GameMechanics
{
  [Serializable]
  public class AttributeEditList : BusinessListBase<AttributeEditList, AttributeEdit>
  {
    private static readonly string[] AttributeNames = ["STR", "DEX", "END", "INT", "ITT", "WIL", "PHY"];

    public int InitialSum => this.Sum(a => a.BaseValue);

    public int CurrentSum => this.Sum(a => a.Value);

    protected override void OnChildChanged(Csla.Core.ChildChangedEventArgs e)
    {
      base.OnChildChanged(e);
      // Notify parent (CharacterEdit) to recalculate FAT/VIT when attributes change
      if (Parent is CharacterEdit character)
      {
        character.Fatigue.CalculateBase(character);
        character.Vitality.CalculateBase(character);
      }
    }

    /// <summary>
    /// Creates attributes with no species modifiers (Human baseline).
    /// </summary>
    [CreateChild]
    private void Create([Inject] IChildDataPortal<AttributeEdit> attributePortal)
    {
      foreach (var name in AttributeNames)
        Add(attributePortal.CreateChild(name, 0));
    }

    /// <summary>
    /// Creates attributes with species-specific modifiers.
    /// </summary>
    [CreateChild]
    private void Create(SpeciesInfo species, [Inject] IChildDataPortal<AttributeEdit> attributePortal)
    {
      foreach (var name in AttributeNames)
      {
        int modifier = species?.GetModifier(name) ?? 0;
        Add(attributePortal.CreateChild(name, modifier));
      }
    }

    [FetchChild]
    private void Fetch(List<CharacterAttribute> list, [Inject] IChildDataPortal<AttributeEdit> attributePortal)
    {
      Fetch(list, null, attributePortal);
    }

    [FetchChild]
    private void Fetch(List<CharacterAttribute> list, Reference.SpeciesInfo? species, [Inject] IChildDataPortal<AttributeEdit> attributePortal)
    {
      if (list == null)
      {
        Create(attributePortal);
      }
      else
      {
        using (LoadListMode)
        {
          foreach (var item in list)
            Add(attributePortal.FetchChild(item, species));
        }
      }
    }
  }
}
