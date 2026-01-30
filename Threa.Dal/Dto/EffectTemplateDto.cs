using System;
using Csla.Core;
using Csla.Serialization.Mobile;

namespace Threa.Dal.Dto;

/// <summary>
/// Template for creating effects - provides defaults that GM can modify before applying.
/// Used for system-wide effect library (Stunned, Blessed, Poisoned, etc.).
/// </summary>
public class EffectTemplateDto : IMobileObject
{
    /// <summary>
    /// Primary key (0 for new templates).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Template name (required).
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Effect category (Buff, Debuff, Condition, Poison, etc.).
    /// </summary>
    public EffectType EffectType { get; set; }

    /// <summary>
    /// What this effect does.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Icon identifier for display.
    /// </summary>
    public string? IconName { get; set; }

    /// <summary>
    /// Display color (hex code or CSS class name).
    /// </summary>
    public string? Color { get; set; }

    /// <summary>
    /// Default duration amount.
    /// </summary>
    public int? DefaultDurationValue { get; set; }

    /// <summary>
    /// Duration type (Rounds, Minutes, Hours, Days, Permanent).
    /// </summary>
    public DurationType DurationType { get; set; }

    /// <summary>
    /// Serialized EffectState with modifiers (JSON).
    /// </summary>
    public string? StateJson { get; set; }

    /// <summary>
    /// Comma-separated tags for organization/filtering.
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// True for built-in templates that cannot be deleted.
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Soft delete flag (false = deleted).
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }

    #region IMobileObject implementation

    /// <summary>
    /// Gets the object's state for CSLA serialization.
    /// </summary>
    public void GetState(SerializationInfo info)
    {
        info.AddValue(nameof(Id), Id);
        info.AddValue(nameof(Name), Name);
        info.AddValue(nameof(EffectType), (int)EffectType);
        info.AddValue(nameof(Description), Description);
        info.AddValue(nameof(IconName), IconName);
        info.AddValue(nameof(Color), Color);
        info.AddValue(nameof(DefaultDurationValue), DefaultDurationValue);
        info.AddValue(nameof(DurationType), (int)DurationType);
        info.AddValue(nameof(StateJson), StateJson);
        info.AddValue(nameof(Tags), Tags);
        info.AddValue(nameof(IsSystem), IsSystem);
        info.AddValue(nameof(IsActive), IsActive);
        info.AddValue(nameof(CreatedAt), CreatedAt);
        info.AddValue(nameof(UpdatedAt), UpdatedAt);
    }

    /// <summary>
    /// Sets the object's state from CSLA deserialization.
    /// </summary>
    public void SetState(SerializationInfo info)
    {
        Id = info.GetValue<int>(nameof(Id));
        Name = info.GetValue<string>(nameof(Name));
        EffectType = (EffectType)info.GetValue<int>(nameof(EffectType));
        Description = info.GetValue<string?>(nameof(Description));
        IconName = info.GetValue<string?>(nameof(IconName));
        Color = info.GetValue<string?>(nameof(Color));
        DefaultDurationValue = info.GetValue<int?>(nameof(DefaultDurationValue));
        DurationType = (DurationType)info.GetValue<int>(nameof(DurationType));
        StateJson = info.GetValue<string?>(nameof(StateJson));
        Tags = info.GetValue<string?>(nameof(Tags));
        IsSystem = info.GetValue<bool>(nameof(IsSystem));
        IsActive = info.GetValue<bool>(nameof(IsActive));
        CreatedAt = info.GetValue<DateTime>(nameof(CreatedAt));
        UpdatedAt = info.GetValue<DateTime?>(nameof(UpdatedAt));
    }

    /// <summary>
    /// Gets the object's children for CSLA serialization.
    /// </summary>
    public void GetChildren(SerializationInfo info, MobileFormatter formatter)
    {
        // No child objects to serialize
    }

    /// <summary>
    /// Sets the object's children from CSLA deserialization.
    /// </summary>
    public void SetChildren(SerializationInfo info, MobileFormatter formatter)
    {
        // No child objects to deserialize
    }

    #endregion
}
