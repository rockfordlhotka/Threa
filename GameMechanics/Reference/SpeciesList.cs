using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;

namespace GameMechanics.Reference
{
    /// <summary>
    /// Read-only list of all available species.
    /// </summary>
    [Serializable]
    public class SpeciesList : ReadOnlyListBase<SpeciesList, SpeciesInfo>
    {
        [Fetch]
        private async Task Fetch([Inject] ISpeciesDal dal, [Inject] IChildDataPortal<SpeciesInfo> childPortal)
        {
            var species = await dal.GetAllSpeciesAsync();
            using (LoadListMode)
            {
                foreach (var item in species)
                    Add(childPortal.FetchChild(item));
            }
        }
    }

    /// <summary>
    /// Read-only information about a species.
    /// </summary>
    [Serializable]
    public class SpeciesInfo : ReadOnlyBase<SpeciesInfo>
    {
        public static readonly PropertyInfo<string> IdProperty = RegisterProperty<string>(nameof(Id));
        public string Id
        {
            get => GetProperty(IdProperty);
            private set => LoadProperty(IdProperty, value);
        }

        public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
        public string Name
        {
            get => GetProperty(NameProperty);
            private set => LoadProperty(NameProperty, value);
        }

        public static readonly PropertyInfo<string> DescriptionProperty = RegisterProperty<string>(nameof(Description));
        public string Description
        {
            get => GetProperty(DescriptionProperty);
            private set => LoadProperty(DescriptionProperty, value);
        }

        public static readonly PropertyInfo<SpeciesAttributeModifierList> AttributeModifiersProperty = 
            RegisterProperty<SpeciesAttributeModifierList>(nameof(AttributeModifiers));
        public SpeciesAttributeModifierList AttributeModifiers
        {
            get => GetProperty(AttributeModifiersProperty);
            private set => LoadProperty(AttributeModifiersProperty, value);
        }

        /// <summary>
        /// Gets the modifier for a specific attribute, or 0 if none.
        /// </summary>
        public int GetModifier(string attributeName)
        {
            var modifier = AttributeModifiers.FirstOrDefault(m => m.AttributeName == attributeName);
            return modifier?.Modifier ?? 0;
        }

        [FetchChild]
        private void Fetch(Threa.Dal.Dto.Species species, [Inject] IChildDataPortal<SpeciesAttributeModifierList> modifiersPortal)
        {
            Id = species.Id;
            Name = species.Name;
            Description = species.Description;
            AttributeModifiers = modifiersPortal.FetchChild(species.AttributeModifiers);
        }
    }

    /// <summary>
    /// Read-only list of attribute modifiers for a species.
    /// </summary>
    [Serializable]
    public class SpeciesAttributeModifierList : ReadOnlyListBase<SpeciesAttributeModifierList, SpeciesAttributeModifierInfo>
    {
        [FetchChild]
        private void Fetch(List<Threa.Dal.Dto.SpeciesAttributeModifier> modifiers, 
            [Inject] IChildDataPortal<SpeciesAttributeModifierInfo> childPortal)
        {
            using (LoadListMode)
            {
                foreach (var item in modifiers)
                    Add(childPortal.FetchChild(item));
            }
        }
    }

    /// <summary>
    /// Read-only information about a species attribute modifier.
    /// </summary>
    [Serializable]
    public class SpeciesAttributeModifierInfo : ReadOnlyBase<SpeciesAttributeModifierInfo>
    {
        public static readonly PropertyInfo<string> AttributeNameProperty = RegisterProperty<string>(nameof(AttributeName));
        public string AttributeName
        {
            get => GetProperty(AttributeNameProperty);
            private set => LoadProperty(AttributeNameProperty, value);
        }

        public static readonly PropertyInfo<int> ModifierProperty = RegisterProperty<int>(nameof(Modifier));
        public int Modifier
        {
            get => GetProperty(ModifierProperty);
            private set => LoadProperty(ModifierProperty, value);
        }

        [FetchChild]
        private void Fetch(Threa.Dal.Dto.SpeciesAttributeModifier modifier)
        {
            AttributeName = modifier.AttributeName;
            Modifier = modifier.Modifier;
        }
    }
}
