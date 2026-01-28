using Csla;
using GameMechanics.Effects;
using System;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics;

/// <summary>
/// Read-only CSLA business object representing an effect template.
/// Effect templates define reusable effect configurations that GMs can apply to characters.
/// </summary>
[Serializable]
public class EffectTemplate : ReadOnlyBase<EffectTemplate>
{
    #region Properties

    public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id));
    /// <summary>
    /// Template ID (primary key).
    /// </summary>
    public int Id
    {
        get => GetProperty(IdProperty);
        private set => LoadProperty(IdProperty, value);
    }

    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    /// <summary>
    /// Template name (e.g., "Stunned", "Blessed", "Poisoned").
    /// </summary>
    public string Name
    {
        get => GetProperty(NameProperty);
        private set => LoadProperty(NameProperty, value);
    }

    public static readonly PropertyInfo<EffectType> EffectTypeProperty = RegisterProperty<EffectType>(nameof(EffectType));
    /// <summary>
    /// Effect category (Buff, Debuff, Condition, Poison, etc.).
    /// </summary>
    public EffectType EffectType
    {
        get => GetProperty(EffectTypeProperty);
        private set => LoadProperty(EffectTypeProperty, value);
    }

    public static readonly PropertyInfo<string?> DescriptionProperty = RegisterProperty<string?>(nameof(Description));
    /// <summary>
    /// What this effect does.
    /// </summary>
    public string? Description
    {
        get => GetProperty(DescriptionProperty);
        private set => LoadProperty(DescriptionProperty, value);
    }

    public static readonly PropertyInfo<string?> IconNameProperty = RegisterProperty<string?>(nameof(IconName));
    /// <summary>
    /// Icon identifier for display.
    /// </summary>
    public string? IconName
    {
        get => GetProperty(IconNameProperty);
        private set => LoadProperty(IconNameProperty, value);
    }

    public static readonly PropertyInfo<string?> ColorProperty = RegisterProperty<string?>(nameof(Color));
    /// <summary>
    /// Display color (hex code or CSS class name).
    /// </summary>
    public string? Color
    {
        get => GetProperty(ColorProperty);
        private set => LoadProperty(ColorProperty, value);
    }

    public static readonly PropertyInfo<int?> DefaultDurationValueProperty = RegisterProperty<int?>(nameof(DefaultDurationValue));
    /// <summary>
    /// Default duration amount.
    /// </summary>
    public int? DefaultDurationValue
    {
        get => GetProperty(DefaultDurationValueProperty);
        private set => LoadProperty(DefaultDurationValueProperty, value);
    }

    public static readonly PropertyInfo<DurationType> DurationTypeProperty = RegisterProperty<DurationType>(nameof(DurationType));
    /// <summary>
    /// Duration type (Rounds, Minutes, Hours, Days, Permanent).
    /// </summary>
    public DurationType DurationType
    {
        get => GetProperty(DurationTypeProperty);
        private set => LoadProperty(DurationTypeProperty, value);
    }

    public static readonly PropertyInfo<string?> StateJsonProperty = RegisterProperty<string?>(nameof(StateJson));
    /// <summary>
    /// Serialized EffectState with modifiers (JSON).
    /// </summary>
    public string? StateJson
    {
        get => GetProperty(StateJsonProperty);
        private set => LoadProperty(StateJsonProperty, value);
    }

    public static readonly PropertyInfo<string?> TagsProperty = RegisterProperty<string?>(nameof(Tags));
    /// <summary>
    /// Comma-separated tags for organization/filtering.
    /// </summary>
    public string? Tags
    {
        get => GetProperty(TagsProperty);
        private set => LoadProperty(TagsProperty, value);
    }

    public static readonly PropertyInfo<bool> IsSystemProperty = RegisterProperty<bool>(nameof(IsSystem));
    /// <summary>
    /// True for built-in templates that cannot be deleted.
    /// </summary>
    public bool IsSystem
    {
        get => GetProperty(IsSystemProperty);
        private set => LoadProperty(IsSystemProperty, value);
    }

    #endregion

    #region Computed Properties

    // Cached deserialized state
    [NonSerialized]
    private EffectState? _cachedState;

    /// <summary>
    /// Deserialized EffectState from StateJson.
    /// Caches the result for performance.
    /// </summary>
    public EffectState State
    {
        get
        {
            if (_cachedState == null)
            {
                _cachedState = EffectState.Deserialize(StateJson);
            }
            return _cachedState;
        }
    }

    /// <summary>
    /// Tags split into an array.
    /// </summary>
    public string[] TagList
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Tags))
                return [];
            return Tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
    }

    #endregion

    #region Data Access

    [Fetch]
    private void Fetch(EffectTemplateDto dto)
    {
        Id = dto.Id;
        Name = dto.Name;
        EffectType = dto.EffectType;
        Description = dto.Description;
        IconName = dto.IconName;
        Color = dto.Color;
        DefaultDurationValue = dto.DefaultDurationValue;
        DurationType = dto.DurationType;
        StateJson = dto.StateJson;
        Tags = dto.Tags;
        IsSystem = dto.IsSystem;
    }

    #endregion

    #region Factory Methods

    /// <summary>
    /// Gets a specific template by ID.
    /// </summary>
    public static async Task<EffectTemplate?> GetByIdAsync(IDataPortal<EffectTemplate> portal, IEffectTemplateDal dal, int id)
    {
        var dto = await dal.GetTemplateAsync(id);
        if (dto == null)
            return null;
        return await portal.FetchAsync(dto);
    }

    #endregion
}
