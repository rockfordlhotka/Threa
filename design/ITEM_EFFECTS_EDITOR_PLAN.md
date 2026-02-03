# Item Effects Editor Implementation Plan

## Overview

This plan outlines how to add item effect management to the GM item editor. When editing an item, the GM needs to add, edit, and remove effects that activate based on item state (equipped, possessed, on use, etc.).

## Current State

### What Exists

| Component | Status | Location |
|-----------|--------|----------|
| `ItemEffectDefinition` DTO | ✅ Complete | `Threa.Dal/Dto/ItemEffectDefinition.cs` |
| `ItemEffectTrigger` enum | ✅ Complete | `Threa.Dal/Dto/ItemEffectTrigger.cs` |
| `ItemTemplateEdit` business object | ✅ Partial | `GameMechanics/Items/ItemTemplateEdit.cs` |
| `ItemEffectBehavior` | ✅ Complete | `GameMechanics/Effects/Behaviors/ItemEffectBehavior.cs` |
| `ItemEffectService` | ✅ Complete | `GameMechanics/Effects/ItemEffectService.cs` |
| GM Item Editor UI | ✅ Partial | `Threa.Client/Components/Pages/GameMaster/ItemEdit.razor` |

### What's Missing

1. **DAL Layer**: `IItemEffectDal` interface and implementations for CRUD operations
2. **Business Objects**: `ItemEffectEdit` child object and `ItemEffectEditList` collection
3. **UI**: Effects tab in ItemEdit.razor with effect management UI

## Implementation Phases

---

## Phase 1: Data Access Layer

### 1.1 Create IItemEffectDal Interface

**File**: `Threa.Dal/IItemEffectDal.cs`

```csharp
public interface IItemEffectDal
{
    Task<List<ItemEffectDefinition>> GetEffectsForTemplateAsync(int itemTemplateId);
    Task<ItemEffectDefinition?> GetEffectAsync(int effectId);
    Task<ItemEffectDefinition> SaveEffectAsync(ItemEffectDefinition effect);
    Task DeleteEffectAsync(int effectId);
}
```

### 1.2 Implement MockDb DAL

**File**: `Threa.Dal.MockDb/ItemEffectDal.cs`

In-memory implementation using a `List<ItemEffectDefinition>` with auto-incrementing IDs.

### 1.3 Implement SQLite DAL

**File**: `Threa.Dal.Sqlite/ItemEffectDal.cs`

SQLite implementation with proper parameterized queries.

### 1.4 Create/Update Database Schema

**Table**: `ItemEffectDefinitions`

```sql
CREATE TABLE ItemEffectDefinitions (
    Id INTEGER PRIMARY KEY AUTOINCREMENT,
    ItemTemplateId INTEGER NOT NULL,
    EffectDefinitionId INTEGER NULL,
    EffectType INTEGER NOT NULL,
    Name TEXT NOT NULL,
    Description TEXT,
    Trigger INTEGER NOT NULL,
    IsCursed INTEGER NOT NULL DEFAULT 0,
    RequiresAttunement INTEGER NOT NULL DEFAULT 0,
    DurationRounds INTEGER NULL,
    BehaviorState TEXT,
    IconName TEXT,
    IsActive INTEGER NOT NULL DEFAULT 1,
    Priority INTEGER NOT NULL DEFAULT 0,
    FOREIGN KEY (ItemTemplateId) REFERENCES ItemTemplates(Id)
);
```

---

## Phase 2: Business Objects

### 2.1 Create ItemEffectEdit Child Object

**File**: `GameMechanics/Items/ItemEffectEdit.cs`

```csharp
[Serializable]
public class ItemEffectEdit : BusinessBase<ItemEffectEdit>
{
    // Properties matching ItemEffectDefinition DTO
    public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id));
    public static readonly PropertyInfo<int> ItemTemplateIdProperty = RegisterProperty<int>(nameof(ItemTemplateId));
    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    public static readonly PropertyInfo<string?> DescriptionProperty = RegisterProperty<string?>(nameof(Description));
    public static readonly PropertyInfo<EffectType> EffectTypeProperty = RegisterProperty<EffectType>(nameof(EffectType));
    public static readonly PropertyInfo<ItemEffectTrigger> TriggerProperty = RegisterProperty<ItemEffectTrigger>(nameof(Trigger));
    public static readonly PropertyInfo<bool> IsCursedProperty = RegisterProperty<bool>(nameof(IsCursed));
    public static readonly PropertyInfo<bool> RequiresAttunementProperty = RegisterProperty<bool>(nameof(RequiresAttunement));
    public static readonly PropertyInfo<int?> DurationRoundsProperty = RegisterProperty<int?>(nameof(DurationRounds));
    public static readonly PropertyInfo<string?> BehaviorStateProperty = RegisterProperty<string?>(nameof(BehaviorState));
    public static readonly PropertyInfo<string?> IconNameProperty = RegisterProperty<string?>(nameof(IconName));
    public static readonly PropertyInfo<bool> IsActiveProperty = RegisterProperty<bool>(nameof(IsActive));
    public static readonly PropertyInfo<int> PriorityProperty = RegisterProperty<int>(nameof(Priority));

    // Computed properties for UI display
    public string TriggerDisplayName => Trigger.GetDisplayName();
    public string EffectSummary => BuildEffectSummary();

    // Validation rules
    protected override void AddBusinessRules()
    {
        BusinessRules.AddRule(new Required(NameProperty));
        BusinessRules.AddRule(new TriggerRequiredRule(TriggerProperty));
    }

    // CSLA child operations
    [CreateChild]
    private void Create(int itemTemplateId) { ... }

    [FetchChild]
    private void Fetch(ItemEffectDefinition dto) { ... }

    [InsertChild]
    private async Task Insert([Inject] IItemEffectDal dal) { ... }

    [UpdateChild]
    private async Task Update([Inject] IItemEffectDal dal) { ... }

    [DeleteSelfChild]
    private async Task DeleteSelf([Inject] IItemEffectDal dal) { ... }
}
```

### 2.2 Create ItemEffectEditList Collection

**File**: `GameMechanics/Items/ItemEffectEditList.cs`

```csharp
[Serializable]
public class ItemEffectEditList : BusinessListBase<ItemEffectEditList, ItemEffectEdit>
{
    [FetchChild]
    private async Task Fetch(int itemTemplateId, [Inject] IItemEffectDal dal, [Inject] IChildDataPortal<ItemEffectEdit> childPortal)
    {
        var effects = await dal.GetEffectsForTemplateAsync(itemTemplateId);
        foreach (var dto in effects)
        {
            Add(await childPortal.FetchChildAsync(dto));
        }
    }

    // Helper method to add new effect
    public async Task<ItemEffectEdit> AddNewEffect(int itemTemplateId, [Inject] IChildDataPortal<ItemEffectEdit> childPortal)
    {
        var effect = await childPortal.CreateChildAsync(itemTemplateId);
        Add(effect);
        return effect;
    }
}
```

### 2.3 Update ItemTemplateEdit

**File**: `GameMechanics/Items/ItemTemplateEdit.cs`

Add Effects child collection:

```csharp
public static readonly PropertyInfo<ItemEffectEditList> EffectsProperty =
    RegisterProperty<ItemEffectEditList>(nameof(Effects));
public ItemEffectEditList Effects
{
    get => GetProperty(EffectsProperty);
    private set => LoadProperty(EffectsProperty, value);
}

// Update Fetch to load effects
[Fetch]
private async Task Fetch(int id, [Inject] IItemTemplateDal dal,
    [Inject] IChildDataPortal<ItemEffectEditList> effectsPortal)
{
    var data = await dal.GetTemplateAsync(id)
        ?? throw new InvalidOperationException($"ItemTemplate {id} not found");
    LoadFromDto(data);
    Effects = await effectsPortal.FetchChildAsync(id);
}

// Update Create to initialize empty list
[Create]
private async Task Create([Inject] IChildDataPortal<ItemEffectEditList> effectsPortal)
{
    // ... existing create logic ...
    Effects = await effectsPortal.CreateChildAsync();
}

// Insert/Update automatically save children via CSLA
```

---

## Phase 3: UI Components

### 3.1 Add Effects Tab to ItemEdit.razor

Add new tab to the existing `RadzenTabs`:

```razor
<RadzenTabsItem Text="Effects">
    <ItemEffectsEditor Item="@Item" />
</RadzenTabsItem>
```

### 3.2 Create ItemEffectsEditor Component

**File**: `Threa.Client/Components/GameMaster/ItemEffectsEditor.razor`

Main component showing list of effects with add/edit/delete functionality:

```razor
@using GameMechanics.Items
@using Threa.Dal.Dto

<div class="effects-editor">
    <div class="effects-toolbar">
        <RadzenButton Text="Add Effect" Icon="add" Click="@AddEffect" />
    </div>

    @if (Item.Effects.Count == 0)
    {
        <div class="no-effects">
            <p>No effects defined for this item.</p>
            <p class="hint">Effects allow items to apply buffs, debuffs, or special abilities when equipped, possessed, or used.</p>
        </div>
    }
    else
    {
        <RadzenDataGrid Data="@Item.Effects" TItem="ItemEffectEdit">
            <Columns>
                <RadzenDataGridColumn Property="Name" Title="Effect Name" />
                <RadzenDataGridColumn Property="TriggerDisplayName" Title="Trigger" Width="150px" />
                <RadzenDataGridColumn Property="EffectType" Title="Type" Width="100px" />
                <RadzenDataGridColumn Title="Cursed" Width="80px">
                    <Template Context="effect">
                        @if (effect.IsCursed)
                        {
                            <RadzenBadge Text="Cursed" BadgeStyle="BadgeStyle.Danger" />
                        }
                    </Template>
                </RadzenDataGridColumn>
                <RadzenDataGridColumn Title="Actions" Width="120px">
                    <Template Context="effect">
                        <RadzenButton Icon="edit" Size="ButtonSize.Small" Click="@(() => EditEffect(effect))" />
                        <RadzenButton Icon="delete" Size="ButtonSize.Small" ButtonStyle="ButtonStyle.Danger"
                                      Click="@(() => DeleteEffect(effect))" />
                    </Template>
                </RadzenDataGridColumn>
            </Columns>
        </RadzenDataGrid>
    }
</div>

@code {
    [Parameter] public ItemTemplateEdit Item { get; set; } = default!;

    [Inject] public DialogService DialogService { get; set; } = default!;

    private async Task AddEffect()
    {
        var newEffect = await Item.Effects.AddNewEffect(Item.Id);
        await EditEffect(newEffect);
    }

    private async Task EditEffect(ItemEffectEdit effect)
    {
        await DialogService.OpenAsync<ItemEffectEditDialog>(
            effect.IsNew ? "Add Effect" : "Edit Effect",
            new Dictionary<string, object> { { "Effect", effect } },
            new DialogOptions { Width = "600px", Height = "700px" });
    }

    private async Task DeleteEffect(ItemEffectEdit effect)
    {
        var confirmed = await DialogService.Confirm(
            $"Delete effect '{effect.Name}'?",
            "Confirm Delete",
            new ConfirmOptions { OkButtonText = "Delete", CancelButtonText = "Cancel" });

        if (confirmed == true)
        {
            Item.Effects.Remove(effect);
        }
    }
}
```

### 3.3 Create ItemEffectEditDialog Component

**File**: `Threa.Client/Components/GameMaster/ItemEffectEditDialog.razor`

Modal dialog for editing a single effect:

```razor
<div class="effect-edit-dialog">
    <!-- Basic Info Section -->
    <RadzenFieldset Text="Basic Information">
        <div class="row">
            <div class="col-12">
                <RadzenLabel Text="Effect Name" />
                <RadzenTextBox @bind-Value="Effect.Name" Style="width: 100%" />
            </div>
        </div>
        <div class="row mt-2">
            <div class="col-12">
                <RadzenLabel Text="Description" />
                <RadzenTextArea @bind-Value="Effect.Description" Rows="3" Style="width: 100%" />
            </div>
        </div>
    </RadzenFieldset>

    <!-- Trigger & Type Section -->
    <RadzenFieldset Text="Effect Configuration" class="mt-3">
        <div class="row">
            <div class="col-6">
                <RadzenLabel Text="Trigger" />
                <RadzenDropDown @bind-Value="Effect.Trigger"
                                Data="@_triggers"
                                TextProperty="Text"
                                ValueProperty="Value"
                                Style="width: 100%" />
                <small class="text-muted">@GetTriggerDescription(Effect.Trigger)</small>
            </div>
            <div class="col-6">
                <RadzenLabel Text="Effect Type" />
                <RadzenDropDown @bind-Value="Effect.EffectType"
                                Data="@_effectTypes"
                                TextProperty="Text"
                                ValueProperty="Value"
                                Style="width: 100%" />
            </div>
        </div>

        <!-- Duration (for OnUse effects) -->
        @if (Effect.Trigger == ItemEffectTrigger.OnUse)
        {
            <div class="row mt-2">
                <div class="col-6">
                    <RadzenLabel Text="Duration (rounds)" />
                    <RadzenNumeric @bind-Value="Effect.DurationRounds" Style="width: 100%" />
                    <small class="text-muted">Leave blank for instant effects</small>
                </div>
            </div>
        }
    </RadzenFieldset>

    <!-- Curse Section -->
    <RadzenFieldset Text="Curse Settings" class="mt-3">
        <div class="row">
            <div class="col-6">
                <RadzenCheckBox @bind-Value="Effect.IsCursed" />
                <RadzenLabel Text="Is Cursed" Style="margin-left: 8px" />
            </div>
            <div class="col-6">
                <RadzenCheckBox @bind-Value="Effect.RequiresAttunement" />
                <RadzenLabel Text="Requires Attunement" Style="margin-left: 8px" />
            </div>
        </div>
        @if (Effect.IsCursed)
        {
            <div class="curse-warning mt-2">
                <RadzenAlert AlertStyle="AlertStyle.Warning" ShowIcon="true" Variant="Variant.Flat">
                    @GetCurseBlockingDescription(Effect.Trigger)
                </RadzenAlert>
            </div>
        }
    </RadzenFieldset>

    <!-- Modifiers Section -->
    <RadzenFieldset Text="Effect Modifiers" class="mt-3">
        <ItemEffectModifiersEditor @bind-BehaviorState="Effect.BehaviorState" />
    </RadzenFieldset>

    <!-- Dialog Buttons -->
    <div class="dialog-buttons mt-3">
        <RadzenButton Text="Save" ButtonStyle="ButtonStyle.Primary" Click="@Save" />
        <RadzenButton Text="Cancel" ButtonStyle="ButtonStyle.Light" Click="@Cancel" />
    </div>
</div>

@code {
    [Parameter] public ItemEffectEdit Effect { get; set; } = default!;

    [Inject] public DialogService DialogService { get; set; } = default!;

    private List<DropdownItem<ItemEffectTrigger>> _triggers = new()
    {
        new("While Equipped", ItemEffectTrigger.WhileEquipped),
        new("While Possessed", ItemEffectTrigger.WhilePossessed),
        new("On Pickup", ItemEffectTrigger.OnPickup),
        new("On Use", ItemEffectTrigger.OnUse),
        new("On Attack With", ItemEffectTrigger.OnAttackWith),
        new("On Hit While Wearing", ItemEffectTrigger.OnHitWhileWearing),
        new("On Critical", ItemEffectTrigger.OnCritical)
    };

    private string GetTriggerDescription(ItemEffectTrigger trigger) => trigger switch
    {
        ItemEffectTrigger.WhileEquipped => "Active while the item is equipped in a slot",
        ItemEffectTrigger.WhilePossessed => "Active while the item is in inventory (equipped or not)",
        ItemEffectTrigger.OnPickup => "Applied once when the item is first acquired",
        ItemEffectTrigger.OnUse => "Applied when the item is used (consumables)",
        ItemEffectTrigger.OnAttackWith => "Applied when attacking with this weapon",
        ItemEffectTrigger.OnHitWhileWearing => "Applied when hit while wearing this item",
        ItemEffectTrigger.OnCritical => "Applied on critical hit with this weapon",
        _ => ""
    };

    private string GetCurseBlockingDescription(ItemEffectTrigger trigger) => trigger switch
    {
        ItemEffectTrigger.WhileEquipped => "Cursed: Character cannot unequip this item until the curse is removed",
        ItemEffectTrigger.WhilePossessed or ItemEffectTrigger.OnPickup =>
            "Cursed: Character cannot drop or transfer this item until the curse is removed",
        _ => "Cursed: This effect cannot be removed normally"
    };

    private void Save() => DialogService.Close(true);
    private void Cancel() => DialogService.Close(false);
}
```

### 3.4 Create ItemEffectModifiersEditor Component

**File**: `Threa.Client/Components/GameMaster/ItemEffectModifiersEditor.razor`

Editor for the `BehaviorState` JSON that defines modifiers:

```razor
<div class="modifiers-editor">
    <RadzenButton Text="Add Modifier" Icon="add" Size="ButtonSize.Small" Click="@AddModifier" />

    @foreach (var modifier in _modifiers)
    {
        <div class="modifier-row">
            <RadzenDropDown @bind-Value="modifier.Type" Data="@_modifierTypes"
                            TextProperty="Text" ValueProperty="Value" />

            @switch (modifier.Type)
            {
                case "Attribute":
                    <RadzenDropDown @bind-Value="modifier.Target" Data="@_attributes" />
                    <RadzenNumeric @bind-Value="modifier.Value" />
                    break;

                case "AbilityScore":
                    <RadzenTextBox @bind-Value="modifier.Target" Placeholder="Skill or 'Global'" />
                    <RadzenNumeric @bind-Value="modifier.Value" />
                    break;

                case "SuccessValue":
                    <RadzenNumeric @bind-Value="modifier.Value" />
                    break;

                case "HealingOverTime":
                case "DamageOverTime":
                    <RadzenDropDown @bind-Value="modifier.Target" Data="@_healthPools" />
                    <RadzenNumeric @bind-Value="modifier.Value" />
                    <RadzenLabel Text="every" />
                    <RadzenNumeric @bind-Value="modifier.IntervalRounds" />
                    <RadzenLabel Text="rounds" />
                    break;
            }

            <RadzenButton Icon="delete" Size="ButtonSize.Small"
                          ButtonStyle="ButtonStyle.Danger" Click="@(() => RemoveModifier(modifier))" />
        </div>
    }
</div>

@code {
    [Parameter] public string? BehaviorState { get; set; }
    [Parameter] public EventCallback<string?> BehaviorStateChanged { get; set; }

    private List<ModifierViewModel> _modifiers = new();

    // Deserialize BehaviorState JSON on init, serialize back on changes
}
```

---

## Phase 4: Integration & Testing

### 4.1 Register DAL Services

**File**: `Threa/Program.cs`

```csharp
// Add to service registration
builder.Services.AddScoped<IItemEffectDal, ItemEffectDal>();
```

### 4.2 Unit Tests

**File**: `GameMechanics.Test/ItemEffectEditTests.cs`

```csharp
[TestClass]
public class ItemEffectEditTests
{
    [TestMethod]
    public async Task Create_NewEffect_HasDefaultValues()
    {
        // Test that new effect has proper defaults
    }

    [TestMethod]
    public async Task Fetch_ExistingEffect_LoadsAllProperties()
    {
        // Test round-trip from DTO
    }

    [TestMethod]
    public void Validation_NameRequired_ReturnsError()
    {
        // Test validation rules
    }

    [TestMethod]
    public void Validation_TriggerRequired_ReturnsError()
    {
        // Test trigger validation
    }
}
```

### 4.3 Integration Tests

**File**: `GameMechanics.Test/ItemTemplateWithEffectsTests.cs`

```csharp
[TestClass]
public class ItemTemplateWithEffectsTests
{
    [TestMethod]
    public async Task SaveTemplate_WithEffects_PersistsEffects()
    {
        // Test that saving item template saves child effects
    }

    [TestMethod]
    public async Task FetchTemplate_WithEffects_LoadsEffects()
    {
        // Test that fetching item template loads child effects
    }

    [TestMethod]
    public async Task DeleteEffect_FromTemplate_RemovesEffect()
    {
        // Test effect deletion
    }
}
```

---

## UI Mockup

### Effects Tab Layout

```
┌─────────────────────────────────────────────────────────────────┐
│ [Basic] [Weapon] [Armor] [Container] [Effects]                  │
├─────────────────────────────────────────────────────────────────┤
│                                                                 │
│  [+ Add Effect]                                                 │
│                                                                 │
│  ┌─────────────────────────────────────────────────────────┐   │
│  │ Name            │ Trigger       │ Type   │      │ Actions│  │
│  ├─────────────────┼───────────────┼────────┼──────┼────────┤  │
│  │ Strength Boost  │ While Equipped│ Buff   │Cursed│ ✏️ 🗑️  │  │
│  │ Life Drain      │ While Equipped│ Debuff │Cursed│ ✏️ 🗑️  │  │
│  │ Regeneration    │ While Equipped│ Buff   │      │ ✏️ 🗑️  │  │
│  └─────────────────────────────────────────────────────────┘   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Effect Edit Dialog Layout

```
┌───────────────────────────────────────────────────┐
│ Edit Effect                               [X]     │
├───────────────────────────────────────────────────┤
│ ┌─ Basic Information ─────────────────────────┐   │
│ │ Effect Name: [Strength Boost            ]   │   │
│ │ Description: [Grants +3 STR while worn  ]   │   │
│ └─────────────────────────────────────────────┘   │
│                                                   │
│ ┌─ Effect Configuration ──────────────────────┐   │
│ │ Trigger: [While Equipped ▼]                 │   │
│ │   ℹ️ Active while the item is equipped      │   │
│ │                                             │   │
│ │ Effect Type: [Buff ▼]                       │   │
│ └─────────────────────────────────────────────┘   │
│                                                   │
│ ┌─ Curse Settings ────────────────────────────┐   │
│ │ [✓] Is Cursed    [ ] Requires Attunement    │   │
│ │ ⚠️ Character cannot unequip until curse     │   │
│ │    is removed                               │   │
│ └─────────────────────────────────────────────┘   │
│                                                   │
│ ┌─ Effect Modifiers ──────────────────────────┐   │
│ │ [+ Add Modifier]                            │   │
│ │                                             │   │
│ │ [Attribute ▼] [STR ▼] [+3]           [🗑️]  │   │
│ └─────────────────────────────────────────────┘   │
│                                                   │
│            [Save]  [Cancel]                       │
└───────────────────────────────────────────────────┘
```

---

## Implementation Order

### Sprint 1: Foundation (DAL + Business Objects)

1. Create `IItemEffectDal` interface
2. Implement `MockDbItemEffectDal`
3. Create database migration for `ItemEffectDefinitions` table
4. Implement `SqliteItemEffectDal`
5. Create `ItemEffectEdit` child business object
6. Create `ItemEffectEditList` collection
7. Update `ItemTemplateEdit` to include Effects child collection
8. Write unit tests for business objects

### Sprint 2: Basic UI

1. Create `ItemEffectsEditor` component (list view)
2. Add Effects tab to `ItemEdit.razor`
3. Create `ItemEffectEditDialog` component (basic fields only)
4. Wire up add/edit/delete functionality
5. Test end-to-end save/load cycle

### Sprint 3: Modifier Editor

1. Create `ItemEffectModifiersEditor` component
2. Implement `BehaviorState` JSON serialization/deserialization
3. Add modifier types: Attribute, AbilityScore, SuccessValue
4. Add modifier types: HealingOverTime, DamageOverTime
5. Add validation for modifier configurations

### Sprint 4: Polish & Advanced Features

1. Add effect templates/presets (common configurations)
2. Add copy effect functionality
3. Add effect preview (what it will do)
4. Add help tooltips throughout
5. Integration tests with full save/load cycle

---

## Key Design Decisions

### 1. Child Object Pattern

Effects are managed as CSLA child objects within `ItemTemplateEdit`. This provides:
- Automatic dirty tracking
- Coordinated save/rollback
- Standard CSLA validation

### 2. BehaviorState as JSON

The `BehaviorState` property stores modifier configuration as JSON. This:
- Matches existing `ItemEffectBehavior` implementation
- Provides flexibility for new modifier types
- Avoids complex relational modeling for modifiers

### 3. Trigger-Based UI

The UI adapts based on selected trigger:
- `OnUse` shows duration field
- Cursed effects show appropriate blocking warnings
- Combat triggers (`OnAttackWith`, `OnCritical`) may hide certain options

### 4. Modal Dialog for Editing

Using a modal dialog for effect editing:
- Keeps the main item editor clean
- Provides focused editing experience
- Allows cancel without affecting item state

---

## Dependencies

- Radzen.Blazor (already installed)
- CSLA.NET 9.1 (already installed)
- System.Text.Json for BehaviorState serialization

## Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| Complex JSON editing prone to errors | Provide structured modifier editor, validate on save |
| Users may not understand trigger types | Add inline help text and tooltips |
| Performance with many effects | Lazy-load effects list, paginate if needed |
| Database migration issues | Create reversible migrations, test on copy of data |

---

## Success Criteria

1. GM can add multiple effects to an item template
2. Effects persist correctly through save/load cycle
3. All trigger types are available and properly explained
4. Curse status correctly displays warnings about blocking behavior
5. Modifiers can be added, edited, and removed
6. Validation prevents invalid effect configurations
7. UI is consistent with existing item editor styling
