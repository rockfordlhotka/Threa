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
            if (dal == null)
                throw new ArgumentNullException(nameof(dal), "ISpeciesDal was not injected");
            if (childPortal == null)
                throw new ArgumentNullException(nameof(childPortal), "IChildDataPortal<SpeciesInfo> was not injected");

             var species = await dal.GetAllSpeciesAsync();
            if (species != null)
            {
                using (LoadListMode)
                {
                    foreach (var item in species)
                    {
                        if (item != null)
                        {
                            try
                            {
                                Add(childPortal.FetchChild(item));
                            }
                            catch (Exception ex)
                            {
                                // Log the specific item that failed
                                throw new InvalidOperationException($"Failed to fetch child for species '{item.Id}' ('{item.Name}')", ex);
                            }
                        }
                    }
                }
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
        /// Whether this species can be deleted.
        /// Human cannot be deleted.
        /// </summary>
        public bool CanBeDeleted => !Id.Equals("Human", StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Gets the modifier for a specific attribute, or 0 if none.
        /// </summary>
        public int GetModifier(string attributeName)
        {
            if (AttributeModifiers == null || string.IsNullOrEmpty(attributeName))
                return 0;
                
            var modifier = AttributeModifiers.FirstOrDefault(m => m.AttributeName == attributeName);
            return modifier?.Modifier ?? 0;
        }

        [FetchChild]
        private void Fetch(Threa.Dal.Dto.Species species, [Inject] IChildDataPortal<SpeciesAttributeModifierList> modifiersPortal)
        {
            if (species == null)
                throw new ArgumentNullException(nameof(species));
            if (modifiersPortal == null)
                throw new ArgumentNullException(nameof(modifiersPortal), "IChildDataPortal<SpeciesAttributeModifierList> was not injected");

            Id = species.Id ?? string.Empty;
            Name = species.Name ?? string.Empty;
            Description = species.Description ?? string.Empty;
            
            try
            {
                AttributeModifiers = modifiersPortal.FetchChild(species.AttributeModifiers ?? new List<Threa.Dal.Dto.SpeciesAttributeModifier>());
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to fetch attribute modifiers for species '{species.Id}' ('{species.Name}')", ex);
            }
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
            if (modifiers != null)
            {
                using (LoadListMode)
                {
                    foreach (var item in modifiers)
                    {
                        if (item != null)
                            Add(childPortal.FetchChild(item));
                    }
                }
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
            AttributeName = modifier?.AttributeName ?? string.Empty;
            Modifier = modifier?.Modifier ?? 0;
        }
    }
}
