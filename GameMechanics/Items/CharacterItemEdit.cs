using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Items;

[Serializable]
public class CharacterItemEdit : BusinessBase<CharacterItemEdit>
{
    public static readonly PropertyInfo<Guid> IdProperty = RegisterProperty<Guid>(nameof(Id));
    public Guid Id
    {
        get => GetProperty(IdProperty);
        private set => LoadProperty(IdProperty, value);
    }

    public static readonly PropertyInfo<int> ItemTemplateIdProperty = RegisterProperty<int>(nameof(ItemTemplateId));
    [Required]
    public int ItemTemplateId
    {
        get => GetProperty(ItemTemplateIdProperty);
        set => SetProperty(ItemTemplateIdProperty, value);
    }

    public static readonly PropertyInfo<int> OwnerCharacterIdProperty = RegisterProperty<int>(nameof(OwnerCharacterId));
    [Required]
    public int OwnerCharacterId
    {
        get => GetProperty(OwnerCharacterIdProperty);
        private set => LoadProperty(OwnerCharacterIdProperty, value);
    }

    public static readonly PropertyInfo<Guid?> ContainerItemIdProperty = RegisterProperty<Guid?>(nameof(ContainerItemId));
    public Guid? ContainerItemId
    {
        get => GetProperty(ContainerItemIdProperty);
        set => SetProperty(ContainerItemIdProperty, value);
    }

    public static readonly PropertyInfo<EquipmentSlot> EquippedSlotProperty = RegisterProperty<EquipmentSlot>(nameof(EquippedSlot));
    public EquipmentSlot EquippedSlot
    {
        get => GetProperty(EquippedSlotProperty);
        set => SetProperty(EquippedSlotProperty, value);
    }

    public static readonly PropertyInfo<bool> IsEquippedProperty = RegisterProperty<bool>(nameof(IsEquipped));
    public bool IsEquipped
    {
        get => GetProperty(IsEquippedProperty);
        set => SetProperty(IsEquippedProperty, value);
    }

    public static readonly PropertyInfo<int> StackSizeProperty = RegisterProperty<int>(nameof(StackSize));
    public int StackSize
    {
        get => GetProperty(StackSizeProperty);
        set => SetProperty(StackSizeProperty, value);
    }

    public static readonly PropertyInfo<int?> CurrentDurabilityProperty = RegisterProperty<int?>(nameof(CurrentDurability));
    public int? CurrentDurability
    {
        get => GetProperty(CurrentDurabilityProperty);
        set => SetProperty(CurrentDurabilityProperty, value);
    }

    public static readonly PropertyInfo<string?> CustomNameProperty = RegisterProperty<string?>(nameof(CustomName));
    public string? CustomName
    {
        get => GetProperty(CustomNameProperty);
        set => SetProperty(CustomNameProperty, value);
    }

    public static readonly PropertyInfo<DateTime> CreatedAtProperty = RegisterProperty<DateTime>(nameof(CreatedAt));
    public DateTime CreatedAt
    {
        get => GetProperty(CreatedAtProperty);
        private set => LoadProperty(CreatedAtProperty, value);
    }

    public static readonly PropertyInfo<string?> CustomPropertiesProperty = RegisterProperty<string?>(nameof(CustomProperties));
    public string? CustomProperties
    {
        get => GetProperty(CustomPropertiesProperty);
        set => SetProperty(CustomPropertiesProperty, value);
    }

    protected override void AddBusinessRules()
    {
        base.AddBusinessRules();

        BusinessRules.AddRule(new Csla.Rules.CommonRules.MinValue<int>(ItemTemplateIdProperty, 1));
        BusinessRules.AddRule(new Csla.Rules.CommonRules.MinValue<int>(OwnerCharacterIdProperty, 1));
        BusinessRules.AddRule(new Csla.Rules.CommonRules.MinValue<int>(StackSizeProperty, 1));
    }

    [Create]
    [RunLocal]
    private void Create(int characterId, int templateId)
    {
        using (BypassPropertyChecks)
        {
            Id = Guid.NewGuid();
            OwnerCharacterId = characterId;
            ItemTemplateId = templateId;
            ContainerItemId = null;
            EquippedSlot = EquipmentSlot.None;
            IsEquipped = false;
            StackSize = 1;
            CurrentDurability = null;
            CustomName = null;
            CreatedAt = DateTime.UtcNow;
            CustomProperties = null;
        }
        BusinessRules.CheckRules();
    }

    [Fetch]
    private async Task Fetch(Guid id, [Inject] ICharacterItemDal dal)
    {
        var data = await dal.GetItemAsync(id)
            ?? throw new InvalidOperationException($"CharacterItem {id} not found");
        LoadFromDto(data);
    }

    private void LoadFromDto(CharacterItem data)
    {
        using (BypassPropertyChecks)
        {
            Id = data.Id;
            ItemTemplateId = data.ItemTemplateId;
            OwnerCharacterId = data.OwnerCharacterId;
            ContainerItemId = data.ContainerItemId;
            EquippedSlot = data.EquippedSlot;
            IsEquipped = data.IsEquipped;
            StackSize = data.StackSize;
            CurrentDurability = data.CurrentDurability;
            CustomName = data.CustomName;
            CreatedAt = data.CreatedAt;
            CustomProperties = data.CustomProperties;
        }
        BusinessRules.CheckRules();
    }

    [Insert]
    private async Task Insert([Inject] ICharacterItemDal dal)
    {
        var dto = new CharacterItem
        {
            Id = Id,
            ItemTemplateId = ItemTemplateId,
            OwnerCharacterId = OwnerCharacterId,
            ContainerItemId = ContainerItemId,
            EquippedSlot = EquippedSlot,
            IsEquipped = IsEquipped,
            StackSize = StackSize,
            CurrentDurability = CurrentDurability,
            CustomName = CustomName,
            CreatedAt = CreatedAt,
            CustomProperties = CustomProperties
        };

        await dal.AddItemAsync(dto);
    }

    [Update]
    private async Task Update([Inject] ICharacterItemDal dal)
    {
        var dto = new CharacterItem
        {
            Id = Id,
            ItemTemplateId = ItemTemplateId,
            OwnerCharacterId = OwnerCharacterId,
            ContainerItemId = ContainerItemId,
            EquippedSlot = EquippedSlot,
            IsEquipped = IsEquipped,
            StackSize = StackSize,
            CurrentDurability = CurrentDurability,
            CustomName = CustomName,
            CreatedAt = CreatedAt,
            CustomProperties = CustomProperties
        };

        await dal.UpdateItemAsync(dto);
    }

    [Delete]
    private async Task Delete(Guid id, [Inject] ICharacterItemDal dal)
    {
        await dal.DeleteItemAsync(id);
    }
}
