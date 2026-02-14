using Csla;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics;

/// <summary>
/// CSLA command that creates a new NPC template from an active NPC instance.
/// Copies the NPC's current state (attributes, skills, equipment) but resets
/// health pools and clears temporary effects.
/// </summary>
[Serializable]
public class NpcTemplateCreator : CommandBase<NpcTemplateCreator>
{
    #region Input Parameters

    public static readonly PropertyInfo<int> SourceCharacterIdProperty = RegisterProperty<int>(nameof(SourceCharacterId));
    /// <summary>
    /// The ID of the NPC character to create a template from.
    /// </summary>
    public int SourceCharacterId
    {
        get => ReadProperty(SourceCharacterIdProperty);
        set => LoadProperty(SourceCharacterIdProperty, value);
    }

    public static readonly PropertyInfo<string> TemplateNameProperty = RegisterProperty<string>(nameof(TemplateName));
    /// <summary>
    /// The name for the new template.
    /// </summary>
    public string TemplateName
    {
        get => ReadProperty(TemplateNameProperty);
        set => LoadProperty(TemplateNameProperty, value);
    }

    public static readonly PropertyInfo<string?> CategoryProperty = RegisterProperty<string?>(nameof(Category));
    /// <summary>
    /// Optional category for template organization.
    /// </summary>
    public string? Category
    {
        get => ReadProperty(CategoryProperty);
        set => LoadProperty(CategoryProperty, value);
    }

    public static readonly PropertyInfo<string?> TagsProperty = RegisterProperty<string?>(nameof(Tags));
    /// <summary>
    /// Optional comma-separated tags for template filtering.
    /// </summary>
    public string? Tags
    {
        get => ReadProperty(TagsProperty);
        set => LoadProperty(TagsProperty, value);
    }

    #endregion

    #region Output

    public static readonly PropertyInfo<int> CreatedTemplateIdProperty = RegisterProperty<int>(nameof(CreatedTemplateId));
    /// <summary>
    /// The ID of the newly created template.
    /// </summary>
    public int CreatedTemplateId
    {
        get => ReadProperty(CreatedTemplateIdProperty);
        private set => LoadProperty(CreatedTemplateIdProperty, value);
    }

    public static readonly PropertyInfo<bool> SuccessProperty = RegisterProperty<bool>(nameof(Success));
    /// <summary>
    /// Whether the operation completed successfully.
    /// </summary>
    public bool Success
    {
        get => ReadProperty(SuccessProperty);
        private set => LoadProperty(SuccessProperty, value);
    }

    public static readonly PropertyInfo<string?> ErrorMessageProperty = RegisterProperty<string?>(nameof(ErrorMessage));
    /// <summary>
    /// Error message if the operation failed.
    /// </summary>
    public string? ErrorMessage
    {
        get => ReadProperty(ErrorMessageProperty);
        private set => LoadProperty(ErrorMessageProperty, value);
    }

    #endregion

    [Execute]
    private async Task ExecuteAsync([Inject] ICharacterDal characterDal)
    {
        try
        {
            // 1. Fetch source NPC
            var source = await characterDal.GetCharacterAsync(SourceCharacterId);
            if (source == null)
            {
                Success = false;
                ErrorMessage = $"Character {SourceCharacterId} not found";
                return;
            }

            // 2. Create new character DTO as template
            var template = new Character
            {
                Id = 0, // Will be assigned on save
                PlayerId = source.PlayerId,

                // Identity - use new name
                Name = TemplateName,
                TrueName = source.TrueName,
                Aliases = source.Aliases,
                Species = source.Species,
                Description = source.Description,
                ImageUrl = source.ImageUrl,

                // Physical description
                Height = source.Height,
                Weight = source.Weight,
                SkinDescription = source.SkinDescription,
                HairDescription = source.HairDescription,
                Birthdate = source.Birthdate,

                // Combat stats
                DamageClass = source.DamageClass,

                // Health pools - RESET to full (templates represent fresh state)
                FatBaseValue = source.FatBaseValue,
                FatValue = source.FatBaseValue, // Full health
                FatPendingDamage = 0,
                FatPendingHealing = 0,
                VitBaseValue = source.VitBaseValue,
                VitValue = source.VitBaseValue, // Full health
                VitPendingDamage = 0,
                VitPendingHealing = 0,

                // Action points - full
                ActionPointMax = source.ActionPointMax,
                ActionPointRecovery = source.ActionPointRecovery,
                ActionPointAvailable = source.ActionPointMax,

                // XP
                XPTotal = source.XPTotal,
                XPBanked = source.XPBanked,

                // Currency
                CopperCoins = source.CopperCoins,
                SilverCoins = source.SilverCoins,
                GoldCoins = source.GoldCoins,
                PlatinumCoins = source.PlatinumCoins,

                // NPC/Template flags
                IsNpc = true,
                IsTemplate = true,
                IsPlayable = false, // Templates are not directly playable
                VisibleToPlayers = true, // Templates visible in library
                IsArchived = false,

                // Setting (fantasy/scifi)
                Setting = source.Setting,

                // Template organization
                Category = Category,
                Tags = Tags,
                TemplateNotes = source.Notes, // Preserve notes as template notes
                DefaultDisposition = source.DefaultDisposition,
                DifficultyRating = source.DifficultyRating,

                // Source tracking - templates don't have source
                SourceTemplateId = null,
                SourceTemplateName = null,

                // Notes
                Notes = string.Empty,

                // Game time
                CurrentGameTimeSeconds = 0
            };

            // 3. Copy attributes
            template.AttributeList = source.AttributeList
                .Select(a => new CharacterAttribute
                {
                    Name = a.Name,
                    Value = a.Value
                })
                .ToList();

            // 4. Copy skills
            template.Skills = source.Skills
                .Select(s => new CharacterSkill
                {
                    Id = s.Id,
                    Name = s.Name,
                    Category = s.Category,
                    Description = s.Description,
                    IsSpecialized = s.IsSpecialized,
                    IsMagic = s.IsMagic,
                    IsTheology = s.IsTheology,
                    IsPsionic = s.IsPsionic,
                    Untrained = s.Untrained,
                    Trained = s.Trained,
                    PrimaryAttribute = s.PrimaryAttribute,
                    SecondaryAttribute = s.SecondaryAttribute,
                    TertiaryAttribute = s.TertiaryAttribute,
                    ImageUrl = s.ImageUrl,
                    ActionType = s.ActionType,
                    TargetValueType = s.TargetValueType,
                    DefaultTV = s.DefaultTV,
                    OpposedSkillId = s.OpposedSkillId,
                    CooldownType = s.CooldownType,
                    CooldownSeconds = s.CooldownSeconds,
                    ResultTable = s.ResultTable,
                    AppliesPhysicalityBonus = s.AppliesPhysicalityBonus,
                    RequiresTarget = s.RequiresTarget,
                    RequiresLineOfSight = s.RequiresLineOfSight,
                    ManaRequirements = s.ManaRequirements,
                    CanPumpWithFatigue = s.CanPumpWithFatigue,
                    CanPumpWithMana = s.CanPumpWithMana,
                    PumpDescription = s.PumpDescription,
                    IsFreeAction = s.IsFreeAction,
                    IsPassive = s.IsPassive,
                    ActionDescription = s.ActionDescription,
                    Level = s.Level,
                    XPBanked = s.XPBanked,
                    XPSpent = s.XPSpent
                })
                .ToList();

            // 5. Copy items (equipment persists to template)
            // CharacterItem references ItemTemplate by ID; we copy the instance-specific data
            template.Items = source.Items
                .Select(i => new CharacterItem
                {
                    Id = Guid.NewGuid(), // New instance ID
                    ItemTemplateId = i.ItemTemplateId,
                    OwnerCharacterId = 0, // Will be set after template is saved
                    ContainerItemId = null, // Don't preserve container relationships
                    EquippedSlot = i.EquippedSlot,
                    IsEquipped = i.IsEquipped,
                    StackSize = i.StackSize,
                    CurrentDurability = i.CurrentDurability,
                    CustomName = i.CustomName,
                    CreatedAt = DateTime.UtcNow,
                    CustomProperties = i.CustomProperties
                })
                .ToList();

            // 6. DO NOT copy effects - templates start clean
            // (Wounds, buffs, debuffs are combat state, not template state)
            template.Effects = new List<CharacterEffect>();

            // 7. Save template
            var saved = await characterDal.SaveCharacterAsync(template);

            CreatedTemplateId = saved.Id;
            Success = true;
        }
        catch (Exception ex)
        {
            Success = false;
            ErrorMessage = ex.Message;
        }
    }
}
