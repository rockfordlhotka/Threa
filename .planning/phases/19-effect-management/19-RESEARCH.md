# Phase 19: Effect Management - Research

**Researched:** 2026-01-28
**Domain:** TTRPG Effect System CRUD Operations with CSLA.NET and Blazor
**Confidence:** HIGH

## Summary

Phase 19 implements a GM-facing effect management system for creating, applying, editing, and templating character effects (buffs, debuffs, conditions). The existing codebase already has a comprehensive effect infrastructure:

- **EffectRecord** (CSLA BusinessBase) - Active effect instances on characters
- **EffectList** (CSLA BusinessListBase) - Collection with lifecycle management
- **IEffectBehavior** - Strategy pattern for effect-type-specific behavior
- **WoundBehavior** - Reference implementation showing custom modifiers, tick handling

The primary work is **UI construction** and **template persistence**, as the effect engine itself is already production-ready. The wound management UI (Phase 18) provides the exact component patterns to follow.

**Primary recommendation:** Extend existing infrastructure with EffectTemplate CSLA object for template storage, create EffectManagementModal and EffectFormModal mirroring WoundManagementModal/WoundFormModal patterns.

## Standard Stack

### Core (Already Exists)

| Library/Component | Version | Purpose | Status |
|-------------------|---------|---------|--------|
| CSLA.NET | 9.1.0 | Business object framework | In use |
| Blazor Server | .NET 10 | UI framework with SSR + InteractiveServer | In use |
| Radzen.Blazor | 8.4.2 | UI component library (DialogService) | In use |
| System.Text.Json | Built-in | JSON serialization for BehaviorState | In use |

### Needs Creation

| Component | Purpose | Pattern Source |
|-----------|---------|----------------|
| EffectTemplate | CSLA ReadOnlyBase for template library | ItemTemplate pattern |
| EffectTemplateList | CSLA ReadOnlyListBase for template collection | Similar lists in codebase |
| EffectManagementModal | Blazor modal for effect CRUD | WoundManagementModal.razor |
| EffectFormModal | Blazor modal for add/edit form | WoundFormModal.razor |
| CharacterDetailEffects | Effects tab in character detail | CharacterDetailGmActions pattern |

### Supporting (Already Exists)

| Component | Purpose | Notes |
|-----------|---------|-------|
| ITimeEventPublisher | Real-time update notifications | CharacterUpdateType.EffectAdded/EffectRemoved |
| EffectBehaviorFactory | Effect-type behavior dispatch | Extensible via RegisterBehavior |
| EffectType enum | Effect categories (Buff, Debuff, Condition, etc.) | 12 types defined |
| CharacterEffect DTO | Persistence model | JSON storage in SQLite |

## Architecture Patterns

### Recommended Component Structure

```
Threa.Client/Components/
├── Shared/
│   ├── EffectManagementModal.razor      # Main effect list/management
│   ├── EffectFormModal.razor            # Add/edit effect form
│   ├── EffectTemplatePickerModal.razor  # Browse/apply templates
│   ├── EffectCard.razor                 # Compact effect display
│   └── CharacterDetailEffects.razor     # Tab content for detail modal
└── Pages/GamePlay/
    └── TabStatus.razor                  # Already shows effects (readonly)
```

### Pattern 1: Modal-Based CRUD (From WoundManagement)

**What:** Nested Radzen dialogs for list management and detail forms
**When to use:** Complex entity management requiring multiple views
**Example (from WoundManagementModal.razor):**
```csharp
private async Task OpenAddWound()
{
    var result = await DialogService.OpenAsync<WoundFormModal>(
        "Add Wound",
        new Dictionary<string, object>
        {
            { "Character", Character! },
            { "CharacterId", CharacterId },
            { "TableId", TableId }
        },
        new DialogOptions { Width = "500px", CloseDialogOnOverlayClick = true });

    if (result is true)
    {
        await RefreshCharacter();
        ShowFeedback("Wound added successfully", "alert-success", "bi-check-circle");
    }
}
```

### Pattern 2: BehaviorState JSON Storage

**What:** Custom effect data stored as JSON in BehaviorState property
**When to use:** Effect-type-specific data that varies per type
**Example (from WoundState):**
```csharp
public class EffectState
{
    public int? CustomASPenalty { get; set; }
    public int? FatDamagePerTick { get; set; }
    public int? VitDamagePerTick { get; set; }
    public Dictionary<string, int>? AttributeModifiers { get; set; }
    public Dictionary<string, int>? SkillModifiers { get; set; }
    public List<string>? Tags { get; set; }

    public string Serialize() => JsonSerializer.Serialize(this);
    public static EffectState Deserialize(string? json) =>
        string.IsNullOrEmpty(json) ? new() : JsonSerializer.Deserialize<EffectState>(json) ?? new();
}
```

### Pattern 3: Effect Duration Types

**What:** Four duration types per CONTEXT.md decisions
**When to use:** All effect creation/editing
**Implementation:**
```csharp
public enum EffectDurationType
{
    Rounds,        // Combat rounds (6 seconds each)
    Turns,         // Character's turns only
    RealTime,      // Epoch-based timestamp
    Permanent      // Until manually removed
}
```

### Pattern 4: Effect Template Application

**What:** Template provides defaults, GM can modify before applying
**When to use:** Applying saved templates to characters
**Flow:**
1. GM selects template from picker
2. Template data pre-fills EffectFormModal
3. GM modifies any values (or accepts defaults)
4. Effect created with final values

### Anti-Patterns to Avoid

- **Direct EffectRecord construction:** Always use ChildDataPortal.CreateChild for proper CSLA initialization
- **Saving effects without publishing updates:** Always call ITimeEventPublisher.PublishCharacterUpdateAsync
- **Modifying effects without re-fetch:** Always fetch fresh CharacterEdit before modifying Effects collection
- **Using Definition navigation property:** Store Name/Type directly on CharacterEffect (already done)

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Effect stacking logic | Custom stacking code | EffectList.AddEffect() | Already implements Replace/Extend/Intensify/Independent |
| Modifier calculation | Manual summing | EffectList.GetAbilityScoreModifier() | Handles all effect types correctly |
| Duration tracking | Custom timers | ExpiresAtEpochSeconds + IsExpired() | O(1) expiration check, handles time skips |
| Real-time updates | Custom SignalR | ITimeEventPublisher | Already integrated with dashboard |
| Effect behavior | Big switch statement | IEffectBehavior implementations | Strategy pattern for extensibility |

**Key insight:** The effect engine is already built and tested. Phase 19 is primarily UI work with a small template storage addition.

## Common Pitfalls

### Pitfall 1: Not Publishing Character Updates

**What goes wrong:** GM applies effect, other clients don't see it until page refresh
**Why it happens:** Forgetting to call ITimeEventPublisher after saving
**How to avoid:** Every characterPortal.UpdateAsync() should be followed by PublishCharacterUpdateAsync()
**Warning signs:** Effects work locally but not on player's view

### Pitfall 2: Stale Character State

**What goes wrong:** Effects appear to be applied but aren't persisted, or wrong effects modified
**Why it happens:** Using cached Character parameter instead of fresh fetch
**How to avoid:** Always `await characterPortal.FetchAsync(CharacterId)` before modifications
**Warning signs:** "Object reference not set" errors, duplicate effects

### Pitfall 3: Duration Type Confusion

**What goes wrong:** Effects expire at wrong times or never expire
**Why it happens:** Mixing round-based and epoch-based duration systems
**How to avoid:** Use ExpiresAtEpochSeconds for all new effects (legacy RoundsRemaining is deprecated)
**Warning signs:** Effects lasting forever, effects expiring immediately

### Pitfall 4: Missing Template Fields

**What goes wrong:** Templates lose data when saved/loaded
**Why it happens:** Not including all EffectState properties in template schema
**How to avoid:** Template mirrors EffectRecord properties exactly, including BehaviorState
**Warning signs:** Modifiers missing when applying template

### Pitfall 5: Dialog Result Handling

**What goes wrong:** Parent modal doesn't refresh after child dialog closes
**Why it happens:** Not awaiting or checking DialogService.OpenAsync result
**How to avoid:** Follow WoundManagementModal pattern with result check and RefreshCharacter()
**Warning signs:** Need to close/reopen modal to see changes

## Code Examples

### Creating a Custom Effect (from existing WoundBehavior)

```csharp
// Source: GameMechanics/Effects/Behaviors/WoundBehavior.cs
public static EffectRecord CreateCustomEffect(
    CharacterEdit character,
    IChildDataPortal<EffectRecord> effectPortal,
    EffectType type,
    string name,
    string? description,
    int? durationRounds,
    EffectState? state)
{
    var effect = effectPortal.CreateChild(
        type,
        name,
        (string?)null,     // location (null for non-wound effects)
        durationRounds,
        state?.Serialize());

    effect.Description = description;
    effect.Source = "GM";

    character.Effects.AddEffect(effect);
    return effect;
}
```

### Effect Form Save Pattern (from WoundFormModal)

```csharp
// Source: Threa.Client/Components/Shared/WoundFormModal.razor
private async Task SaveEffect()
{
    if (!IsFormValid) return;
    isProcessing = true;
    errorMessage = null;

    try
    {
        // Always re-fetch for fresh state
        var character = await characterPortal.FetchAsync(CharacterId);

        if (ExistingEffect != null)
        {
            // Edit: Find and update
            var effect = character.Effects.FirstOrDefault(e => e.Id == ExistingEffect.Id);
            if (effect != null)
            {
                // Update properties
                effect.Name = effectName;
                effect.Description = description;
                effect.BehaviorState = BuildState().Serialize();
            }
        }
        else
        {
            // Add: Create new
            var newEffect = await effectPortal.CreateChildAsync(
                selectedType, effectName, null, durationRounds, BuildState().Serialize());
            newEffect.Description = description;
            newEffect.Source = "GM";
            character.Effects.AddEffect(newEffect);
        }

        await characterPortal.UpdateAsync(character);

        // Publish real-time update
        await TimeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
        {
            CharacterId = CharacterId,
            UpdateType = CharacterUpdateType.EffectAdded,
            CampaignId = TableId.ToString(),
            SourceId = "GM",
            Description = $"Effect {(ExistingEffect != null ? "updated" : "added")}: {effectName}"
        });

        DialogService.Close(true);
    }
    catch (Exception ex)
    {
        errorMessage = $"Error saving effect: {ex.Message}";
    }
    finally
    {
        isProcessing = false;
    }
}
```

### Template Storage Pattern

```csharp
// Recommended: EffectTemplate.cs (new file)
[Serializable]
public class EffectTemplate : ReadOnlyBase<EffectTemplate>
{
    public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id));
    public int Id => GetProperty(IdProperty);

    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    public string Name => GetProperty(NameProperty);

    public static readonly PropertyInfo<EffectType> TypeProperty = RegisterProperty<EffectType>(nameof(Type));
    public EffectType Type => GetProperty(TypeProperty);

    public static readonly PropertyInfo<string?> DescriptionProperty = RegisterProperty<string?>(nameof(Description));
    public string? Description => GetProperty(DescriptionProperty);

    public static readonly PropertyInfo<int?> DefaultDurationProperty = RegisterProperty<int?>(nameof(DefaultDuration));
    public int? DefaultDuration => GetProperty(DefaultDurationProperty);

    public static readonly PropertyInfo<string?> StateJsonProperty = RegisterProperty<string?>(nameof(StateJson));
    public string? StateJson => GetProperty(StateJsonProperty);

    public static readonly PropertyInfo<string?> TagsProperty = RegisterProperty<string?>(nameof(Tags));
    public string? Tags => GetProperty(TagsProperty); // Comma-separated

    public static readonly PropertyInfo<bool> IsSystemProperty = RegisterProperty<bool>(nameof(IsSystem));
    public bool IsSystem => GetProperty(IsSystemProperty); // System templates can't be deleted

    [Fetch]
    private void Fetch(EffectTemplateDto dto)
    {
        using (BypassPropertyChecks)
        {
            Id = dto.Id;
            Name = dto.Name;
            Type = dto.Type;
            Description = dto.Description;
            DefaultDuration = dto.DefaultDuration;
            StateJson = dto.StateJson;
            Tags = dto.Tags;
            IsSystem = dto.IsSystem;
        }
    }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| RoundsRemaining tracking | ExpiresAtEpochSeconds | v1.2 | O(1) expiration, handles time skips |
| EffectDefinition lookup | Direct properties on CharacterEffect | v1.2 | No Definition dependency |
| Manual stacking | EffectList.AddEffect() with StackBehavior | Initial | Correct stacking automatically |

**Deprecated/outdated:**
- `DurationRounds` and `ElapsedRounds` on EffectRecord: Use epoch-based expiration
- `Definition` navigation property: Store type/name directly on effect

## Open Questions

### Question 1: Template Storage Location

**What we know:** Templates need persistence, SQLite DAL exists for effects
**What's unclear:** Whether templates go in existing EffectDefinition table or new EffectTemplate table
**Recommendation:** Create new EffectTemplates table - EffectDefinition is for system-level effect types, templates are user-created presets

### Question 2: Equipment-Triggered Effects in Phase 19

**What we know:** CONTEXT.md mentions equipment integration with effectTemplateId
**What's unclear:** Whether equipment effect auto-apply is Phase 19 or later
**Recommendation:** Phase 19 implements template system; equipment integration is Phase 19 scope per CONTEXT.md (add effectTemplateId support to items)

### Question 3: Tag System Implementation

**What we know:** Effects have behavior tags like `["modifier", "end-of-round-trigger"]`
**What's unclear:** Whether tags are predefined list or freeform
**Recommendation:** Use predefined tag set for behavior tags (system-defined), allow freeform for organization tags (user-defined)

## Sources

### Primary (HIGH confidence)

- **Codebase analysis:** GameMechanics/Effects/, Threa.Client/Components/Shared/Wound*.razor
- **Design documents:** design/EFFECTS_SYSTEM.md, design/DATABASE_DESIGN.md
- **Phase context:** .planning/phases/19-effect-management/19-CONTEXT.md

### Secondary (MEDIUM confidence)

- **CSLA patterns:** Observed from existing BusinessBase/BusinessListBase usage in codebase
- **Blazor patterns:** Observed from existing Radzen DialogService usage

### Tertiary (LOW confidence)

None - all findings derived from codebase analysis.

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Verified from codebase, no external libraries needed
- Architecture: HIGH - Existing patterns well-established (wound management)
- Pitfalls: HIGH - Based on existing code patterns and observed anti-patterns

**Research date:** 2026-01-28
**Valid until:** N/A (internal codebase research, stable patterns)

---

## Implementation Checklist Summary

### New Files Required

1. **GameMechanics/Effects/EffectState.cs** - JSON state for generic effects (similar to WoundState)
2. **GameMechanics/EffectTemplate.cs** - CSLA ReadOnlyBase for templates
3. **GameMechanics/EffectTemplateList.cs** - CSLA ReadOnlyListBase for template collection
4. **Threa.Dal/IEffectTemplateDal.cs** - Template persistence interface
5. **Threa.Dal.SqlLite/EffectTemplateDal.cs** - SQLite template persistence
6. **Threa.Dal.MockDb/EffectTemplateDal.cs** - Mock template persistence
7. **Threa.Dal/Dto/EffectTemplateDto.cs** - Template DTO
8. **Threa.Client/Components/Shared/EffectManagementModal.razor** - Effect list modal
9. **Threa.Client/Components/Shared/EffectFormModal.razor** - Effect add/edit form
10. **Threa.Client/Components/Shared/EffectTemplatePickerModal.razor** - Template browser
11. **Threa.Client/Components/Shared/CharacterDetailEffects.razor** - Effects tab content

### Existing Files to Modify

1. **CharacterDetailModal.razor** - Add Effects tab
2. **CharacterDetailGmActions.razor** - Add "Manage Effects" button (similar to wounds)
3. **DI registration** - Register new DAL implementations

### Database Migration

- Create EffectTemplates table with: Id, Name, Type, Description, DefaultDuration, StateJson, Tags, IsSystem, CreatedAt
