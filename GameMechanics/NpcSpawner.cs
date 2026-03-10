using Csla;
using System;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics;

/// <summary>
/// CSLA command that spawns a new NPC instance from a template.
/// Creates a full Character with IsNpc=true, IsTemplate=false, IsPlayable=true.
/// The spawned NPC is attached to the specified table immediately.
/// </summary>
[Serializable]
public class NpcSpawner : CommandBase<NpcSpawner>
{
    #region Input Parameters

    public static readonly PropertyInfo<int> TemplateIdProperty = RegisterProperty<int>(nameof(TemplateId));
    /// <summary>
    /// The ID of the template character to spawn from.
    /// </summary>
    public int TemplateId
    {
        get => ReadProperty(TemplateIdProperty);
        set => LoadProperty(TemplateIdProperty, value);
    }

    public static readonly PropertyInfo<Guid> TableIdProperty = RegisterProperty<Guid>(nameof(TableId));
    /// <summary>
    /// The table to attach the spawned NPC to.
    /// </summary>
    public Guid TableId
    {
        get => ReadProperty(TableIdProperty);
        set => LoadProperty(TableIdProperty, value);
    }

    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    /// <summary>
    /// The name for the spawned NPC (can be auto-generated or custom).
    /// </summary>
    public string Name
    {
        get => ReadProperty(NameProperty);
        set => LoadProperty(NameProperty, value);
    }

    public static readonly PropertyInfo<NpcDisposition> DispositionProperty = RegisterProperty<NpcDisposition>(nameof(Disposition));
    /// <summary>
    /// The disposition for the spawned NPC. Can override the template default.
    /// </summary>
    public NpcDisposition Disposition
    {
        get => ReadProperty(DispositionProperty);
        set => LoadProperty(DispositionProperty, value);
    }

    public static readonly PropertyInfo<string?> SessionNotesProperty = RegisterProperty<string?>(nameof(SessionNotes));
    /// <summary>
    /// Optional session-specific notes for this NPC instance.
    /// </summary>
    public string? SessionNotes
    {
        get => ReadProperty(SessionNotesProperty);
        set => LoadProperty(SessionNotesProperty, value);
    }

    #endregion

    #region Output

    public static readonly PropertyInfo<int> SpawnedCharacterIdProperty = RegisterProperty<int>(nameof(SpawnedCharacterId));
    /// <summary>
    /// The ID of the newly created NPC character.
    /// </summary>
    public int SpawnedCharacterId
    {
        get => ReadProperty(SpawnedCharacterIdProperty);
        private set => LoadProperty(SpawnedCharacterIdProperty, value);
    }

    public static readonly PropertyInfo<bool> SuccessProperty = RegisterProperty<bool>(nameof(Success));
    /// <summary>
    /// Whether the spawn operation completed successfully.
    /// </summary>
    public bool Success
    {
        get => ReadProperty(SuccessProperty);
        private set => LoadProperty(SuccessProperty, value);
    }

    public static readonly PropertyInfo<string?> ErrorMessageProperty = RegisterProperty<string?>(nameof(ErrorMessage));
    /// <summary>
    /// Error message if the spawn operation failed.
    /// </summary>
    public string? ErrorMessage
    {
        get => ReadProperty(ErrorMessageProperty);
        private set => LoadProperty(ErrorMessageProperty, value);
    }

    #endregion

    [Create]
    private void Create() { }

    [Execute]
    private async Task ExecuteAsync(
        [Inject] ICharacterDal characterDal,
        [Inject] ITableDal tableDal)
    {
        try
        {
            // 1. Fetch template character
            var template = await characterDal.GetCharacterAsync(TemplateId);
            if (template == null)
            {
                Success = false;
                ErrorMessage = $"Template {TemplateId} not found";
                return;
            }

            // Verify it's actually a template
            if (!template.IsTemplate)
            {
                Success = false;
                ErrorMessage = $"Character {TemplateId} is not a template";
                return;
            }

            // 2. Determine setting from table theme (preferred) or template
            var gameTable = await tableDal.GetTableAsync(TableId);
            var setting = gameTable?.Theme ?? template.Setting;

            // 3. Create new character DTO by copying template
            var npc = new Character
            {
                // Id = 0 means insert (SaveCharacterAsync handles this)
                Id = 0,
                PlayerId = template.PlayerId, // GM's player ID

                // Identity
                Name = Name,
                TrueName = template.TrueName,
                Aliases = template.Aliases,
                Species = template.Species,
                Description = template.Description,
                ImageUrl = template.ImageUrl,

                // Physical description
                Height = template.Height,
                Weight = template.Weight,
                SkinDescription = template.SkinDescription,
                HairDescription = template.HairDescription,
                Birthdate = template.Birthdate,

                // Combat stats
                DamageClass = template.DamageClass,

                // Health pools - copy base values and set current to max (full health)
                FatBaseValue = template.FatBaseValue,
                FatValue = template.FatBaseValue, // Start at full
                FatPendingDamage = 0,
                FatPendingHealing = 0,
                VitBaseValue = template.VitBaseValue,
                VitValue = template.VitBaseValue, // Start at full
                VitPendingDamage = 0,
                VitPendingHealing = 0,

                // Action points - start at max
                ActionPointMax = template.ActionPointMax,
                ActionPointRecovery = template.ActionPointRecovery,
                ActionPointAvailable = template.ActionPointMax,

                // XP
                XPTotal = template.XPTotal,
                XPBanked = template.XPBanked,

                // Currency - use table's setting to determine wallet type
                Wallet = setting == template.Setting
                    ? template.Wallet.Select(w => new WalletEntry { CurrencyCode = w.CurrencyCode, Amount = w.Amount }).ToList()
                    : CurrencyProviderFactory.GetProvider(setting).CreateEmptyWallet(),

                // NPC flags - this is the key part
                IsNpc = true,
                IsTemplate = false,
                IsPlayable = true, // Ready for play immediately
                VisibleToPlayers = false, // Spawned NPCs start hidden for surprise encounters

                // Disposition - use parameter override or template default
                DefaultDisposition = Disposition,

                // Template organization - NPCs don't need these
                Category = null,
                Tags = null,
                TemplateNotes = null,
                DifficultyRating = template.DifficultyRating,

                // Source template tracking
                SourceTemplateId = TemplateId,
                SourceTemplateName = template.Name,

                // Setting (fantasy/scifi) - use table's theme
                Setting = setting,

                // Session notes
                Notes = SessionNotes ?? string.Empty,

                // Game time - start at 0 (will sync with table time)
                CurrentGameTimeSeconds = 0
            };

            // 4. Copy attributes
            npc.AttributeList = template.AttributeList
                .Select(a => new CharacterAttribute
                {
                    Name = a.Name,
                    Value = a.Value
                })
                .ToList();

            // 5. Copy skills (CharacterSkill extends Skill, so we copy all Skill properties plus Level)
            npc.Skills = template.Skills
                .Select(s => new CharacterSkill
                {
                    // Skill base properties
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
                    PostUseConcentrationRounds = s.PostUseConcentrationRounds,
                    SvBonus = s.SvBonus,

                    // CharacterSkill-specific properties
                    Level = s.Level,
                    XPBanked = s.XPBanked,
                    XPSpent = s.XPSpent
                })
                .ToList();

            // 6. Copy items (equipment and inventory) from the template
            npc.Items = template.Items
                .Select(i => new CharacterItem
                {
                    Id = Guid.NewGuid(),
                    ItemTemplateId = i.ItemTemplateId,
                    OwnerCharacterId = 0, // Set by DAL after save
                    ContainerItemId = null, // Don't preserve container nesting
                    EquippedSlot = i.EquippedSlot,
                    EquippedSlots = i.EquippedSlots?.ToList() ?? [],
                    IsEquipped = i.IsEquipped,
                    StackSize = i.StackSize,
                    CurrentDurability = i.CurrentDurability,
                    CustomName = i.CustomName,
                    CreatedAt = DateTime.UtcNow,
                    CustomProperties = i.CustomProperties
                })
                .ToList();

            // 7. Save the new NPC character (returns character with new Id)
            var saved = await characterDal.SaveCharacterAsync(npc);

            // 8. Attach to table
            var tableChar = new TableCharacter
            {
                TableId = TableId,
                CharacterId = saved.Id,
                PlayerId = template.PlayerId,
                JoinedAt = DateTime.UtcNow,
                ConnectionStatus = ConnectionStatus.Disconnected,
                GmNotes = SessionNotes
            };
            await tableDal.AddCharacterToTableAsync(tableChar);

            SpawnedCharacterId = saved.Id;
            Success = true;
        }
        catch (Exception ex)
        {
            Success = false;
            ErrorMessage = ex.Message;
        }
    }
}
