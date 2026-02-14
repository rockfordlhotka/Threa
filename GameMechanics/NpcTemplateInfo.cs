using System;
using System.Collections.Generic;
using System.Linq;
using Csla;
using Threa.Dal.Dto;

namespace GameMechanics;

/// <summary>
/// Read-only info object for displaying NPC templates in the library.
/// Used for list display without loading full CharacterEdit objects.
/// </summary>
[Serializable]
public class NpcTemplateInfo : ReadOnlyBase<NpcTemplateInfo>
{
    public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id));
    public int Id
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

    public static readonly PropertyInfo<string> SpeciesProperty = RegisterProperty<string>(nameof(Species));
    public string Species
    {
        get => GetProperty(SpeciesProperty);
        private set => LoadProperty(SpeciesProperty, value);
    }

    public static readonly PropertyInfo<string?> CategoryProperty = RegisterProperty<string?>(nameof(Category));
    public string? Category
    {
        get => GetProperty(CategoryProperty);
        private set => LoadProperty(CategoryProperty, value);
    }

    public static readonly PropertyInfo<string?> TagsProperty = RegisterProperty<string?>(nameof(Tags));
    public string? Tags
    {
        get => GetProperty(TagsProperty);
        private set => LoadProperty(TagsProperty, value);
    }

    public static readonly PropertyInfo<NpcDisposition> DefaultDispositionProperty = RegisterProperty<NpcDisposition>(nameof(DefaultDisposition));
    public NpcDisposition DefaultDisposition
    {
        get => GetProperty(DefaultDispositionProperty);
        private set => LoadProperty(DefaultDispositionProperty, value);
    }

    public static readonly PropertyInfo<int> DifficultyRatingProperty = RegisterProperty<int>(nameof(DifficultyRating));
    public int DifficultyRating
    {
        get => GetProperty(DifficultyRatingProperty);
        private set => LoadProperty(DifficultyRatingProperty, value);
    }

    public static readonly PropertyInfo<string> SettingProperty = RegisterProperty<string>(nameof(Setting));
    public string Setting
    {
        get => GetProperty(SettingProperty);
        private set => LoadProperty(SettingProperty, value);
    }

    public static readonly PropertyInfo<bool> IsActiveProperty = RegisterProperty<bool>(nameof(IsActive));
    public bool IsActive
    {
        get => GetProperty(IsActiveProperty);
        private set => LoadProperty(IsActiveProperty, value);
    }

    /// <summary>
    /// Parses the Tags property into a list of individual tags.
    /// Splits by comma, trims whitespace, and filters empty entries.
    /// </summary>
    public IEnumerable<string> TagList
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Tags))
                return Enumerable.Empty<string>();

            return Tags.Split(',')
                .Select(t => t.Trim())
                .Where(t => !string.IsNullOrEmpty(t));
        }
    }

    [FetchChild]
    private void Fetch(Character dto)
    {
        LoadProperty(IdProperty, dto.Id);
        LoadProperty(NameProperty, dto.Name);
        LoadProperty(SpeciesProperty, dto.Species);
        LoadProperty(CategoryProperty, dto.Category);
        LoadProperty(TagsProperty, dto.Tags);
        LoadProperty(DefaultDispositionProperty, dto.DefaultDisposition);
        LoadProperty(DifficultyRatingProperty, dto.DifficultyRating);
        LoadProperty(SettingProperty, dto.Setting);
        // IsActive based on VisibleToPlayers - true = active template
        LoadProperty(IsActiveProperty, dto.VisibleToPlayers);
    }
}
