# Phase 21: Stat Editing - Research

**Researched:** 2026-01-29
**Domain:** Blazor form patterns, CSLA business rule recalculation, real-time updates
**Confidence:** HIGH

## Summary

Phase 21 implements GM editing of character attributes and skills through the dashboard. The research focuses on understanding the existing codebase patterns for:

1. **Attribute structure**: `AttributeEdit` has `BaseValue`, `SpeciesModifier`, and `Value` (effective = base + modifier). GM edits `BaseValue`, which triggers business rules to recalculate `Value`.
2. **Health pool calculations**: `Fatigue.CalculateBase()` uses `END + WIL - 5`, `Vitality` uses `STR * 2 - 5`. Both are triggered automatically via CSLA business rules on `AttributeListProperty` changes.
3. **Skill structure**: `SkillEdit.Level` is directly editable. Skills affect `ActionPoints.Max` via `TotalSkillLevels / 10`.
4. **Real-time updates**: `CharacterUpdateMessage` with `CharacterUpdateType.StatsChanged` already exists for this purpose.

The implementation follows established patterns from `CharacterDetailGmActions.razor` (damage/healing) and `EffectFormModal.razor` (form handling). The existing CSLA business rules handle all recalculation cascades automatically.

**Primary recommendation:** Use inline number input fields with edit mode toggle, leverage existing CSLA business rules for recalculation, and validate in UI before save to prevent invalid states.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CSLA.NET | 9.1.0 | Business object framework | Already used throughout project |
| Blazor | .NET 10 | UI framework | Project standard |
| Bootstrap 5 | 5.x | CSS/styling | Already used for all UI |
| Radzen.Blazor | 8.4.2 | Dialog service, components | Already used for modals |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| System.Text.Json | Built-in | Serialization | If any JSON needed |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Inline editing | Modal form | Modal would require extra click; inline matches context decisions |
| Per-field save | Batch save | Batch save allows cancel/revert which was explicitly decided |

**Installation:**
No new packages required - all dependencies already in project.

## Architecture Patterns

### Recommended Component Structure

The stat editing functionality integrates into the existing dashboard modal structure:

```
Threa.Client/Components/Shared/
├── CharacterDetailModal.razor        # Parent - already exists
├── CharacterDetailSheet.razor        # Modify to add edit mode
├── CharacterDetailGmActions.razor    # Add "Edit Stats" button trigger
└── StatEditForm.razor (optional)     # Could inline in Sheet or extract
```

### Pattern 1: Edit Mode Toggle with CSLA Objects

**What:** Single button toggles entire section between view/edit mode, working directly with CSLA business objects.

**When to use:** When editing multiple related fields that should be saved or reverted together.

**Example:**
```csharp
// Source: Existing pattern from CharacterDetailGmActions.razor
@code {
    private bool isEditMode = false;
    private CharacterEdit? editingCharacter; // Working copy for editing

    private async Task StartEdit()
    {
        // Fetch fresh copy for editing
        editingCharacter = await characterPortal.FetchAsync(CharacterId);
        isEditMode = true;
    }

    private async Task SaveChanges()
    {
        // Validate before save
        if (!ValidateChanges()) return;

        await characterPortal.UpdateAsync(editingCharacter!);
        await PublishUpdate();
        isEditMode = false;
        editingCharacter = null;
        await OnCharacterUpdated.InvokeAsync();
    }

    private void CancelEdit()
    {
        isEditMode = false;
        editingCharacter = null; // Discard changes
    }
}
```

### Pattern 2: Attribute Value Binding with Validation

**What:** Bind directly to `AttributeEdit.BaseValue` and show calculated `Value` alongside.

**When to use:** When editing attributes to show both base and effective values.

**Example:**
```razor
@foreach (var attr in editingCharacter.AttributeList)
{
    <div class="row mb-2">
        <label class="col-3">@attr.Name</label>
        <div class="col-4">
            <input type="number" class="form-control form-control-sm"
                   @bind="attr.BaseValue"
                   @bind:event="onblur"
                   min="1" />
            @if (attr.BaseValue < 1)
            {
                <div class="text-danger small">Minimum value is 1</div>
            }
            else if (attr.BaseValue > 14)
            {
                <div class="text-warning small">Unusually high (typical: 6-14)</div>
            }
        </div>
        <div class="col-3">
            <span class="badge bg-secondary">
                = @attr.Value (with @(attr.SpeciesModifier >= 0 ? "+" : "")@attr.SpeciesModifier species)
            </span>
        </div>
    </div>
}
```

### Pattern 3: CSLA Business Rule Recalculation

**What:** Trigger BusinessRules.CheckRules() for dependent properties after edits.

**When to use:** When attribute changes need to cascade to health pools, AP recovery, etc.

**Example:**
```csharp
// Source: CharacterEdit.cs existing pattern
// Business rules are already defined:
// - FatigueBase: AttributeListProperty -> FatigueProperty (END + WIL - 5)
// - VitalityBase: AttributeListProperty -> VitalityProperty (STR * 2 - 5)
// - ActionPointsMax: SkillsProperty -> ActionPointsProperty (TotalSkillLevels / 10)
// - ActionPointsRecovery: FatigueProperty -> ActionPointsProperty (BaseValue / 4)

// After editing attributes, trigger recalculation:
private void OnAttributeBlur()
{
    // CSLA automatically handles cascades via OnChildChanged in CharacterEdit
    // FAT/VIT recalculate automatically when attributes change
    StateHasChanged(); // Refresh UI to show new derived values
}
```

### Anti-Patterns to Avoid

- **Editing Value directly:** Never edit `AttributeEdit.Value` directly - always edit `BaseValue` and let rules calculate `Value = BaseValue + SpeciesModifier`
- **Manual recalculation:** Don't manually calculate FAT/VIT - use existing `CalculateBase()` methods triggered by business rules
- **Save without validation:** Always validate minimum values (attributes >= 1, skills >= 0) before allowing save
- **Immediate persistence:** Don't save on each field change - batch all changes and save on explicit Save button click

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Derived stat calculation | Manual FAT/VIT formulas | `Fatigue.CalculateBase(character)`, `Vitality.CalculateBase(character)` | Formulas already implemented and tested |
| AS recalculation | Manual skill AS formulas | Existing `SkillEdit.AbilityScore` property | Already handles attribute + skill + penalties |
| AP calculations | Manual AP recovery/max | `ActionPoints.RecalculateMax()`, `ActionPoints.RecalculateRecovery()` | Already correct formulas |
| Real-time notifications | Custom SignalR | `ITimeEventPublisher.PublishCharacterUpdateAsync()` | Infrastructure already exists |
| Species modifier lookup | Hardcoded values | `SpeciesInfo.GetModifier(attributeName)` | Already handles all species correctly |

**Key insight:** The entire recalculation cascade is already implemented via CSLA business rules. Editing `AttributeEdit.BaseValue` automatically triggers `OnChildChanged` in `AttributeEditList`, which notifies `CharacterEdit`, which re-runs rules for `FatigueProperty` and `VitalityProperty`. No manual calculation code needed.

## Common Pitfalls

### Pitfall 1: Editing Value Instead of BaseValue

**What goes wrong:** Editing `attr.Value` directly causes it to be overwritten when business rules run, losing the edit.
**Why it happens:** `Value` looks like the main property but it's actually computed.
**How to avoid:** Always bind to `attr.BaseValue`, display `attr.Value` as read-only effective value.
**Warning signs:** Edits disappear on blur or after save; species modifiers appear to be lost.

### Pitfall 2: Health Pool Overflow on Attribute Decrease

**What goes wrong:** Lowering END/STR reduces FAT/VIT max, but current value might exceed new max.
**Why it happens:** Character has FAT=15 out of 15 max; GM reduces END causing max to become 13.
**How to avoid:** After recalculating `BaseValue`, cap `Value` at `BaseValue`: `if (pool.Value > pool.BaseValue) pool.Value = pool.BaseValue;`
**Warning signs:** Current FAT/VIT exceeds max in UI; negative available pools shown.

### Pitfall 3: Forgetting Skill Count Affects AP Max

**What goes wrong:** GM edits skill levels but AP Max doesn't update.
**Why it happens:** AP Max = TotalSkillLevels / 10. If GM removes skill levels, this cascades.
**How to avoid:** Rely on existing business rules - `ActionPointsMax` rule listens to `SkillsProperty` changes. Just ensure `StateHasChanged()` is called to refresh display.
**Warning signs:** AP max shows stale value after skill edits.

### Pitfall 4: Concurrent Edit Conflicts

**What goes wrong:** GM edits stats while time event processes, causing data loss.
**Why it happens:** Two operations fetch and save the same character object.
**How to avoid:** Fetch fresh character on save, apply UI changes to that fresh copy, then save. Or block editing during active time processing.
**Warning signs:** Some changes lost after save; effects or damage from time events disappear.

### Pitfall 5: Validation Only in UI

**What goes wrong:** Invalid values get saved because validation was only client-side.
**Why it happens:** Relying solely on HTML `min` attribute without server validation.
**How to avoid:** Add CSLA validation rules or check in the component before calling `characterPortal.UpdateAsync()`.
**Warning signs:** Database contains attributes with value 0 or negative numbers.

## Code Examples

Verified patterns from the existing codebase:

### CharacterUpdateMessage Publishing
```csharp
// Source: CharacterDetailGmActions.razor line 318-325
await TimeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
{
    CharacterId = CharacterId,
    UpdateType = CharacterUpdateType.StatsChanged, // Use this for stat edits
    CampaignId = TableId.ToString(),
    SourceId = "GM",
    Description = "Attributes and skills modified"
});
```

### Attribute Base/Value Structure
```csharp
// Source: AttributeEdit.cs
// BaseValue: The raw value before species modifier (what GM edits)
// SpeciesModifier: Applied based on character's species (read-only)
// Value: Effective value = BaseValue + SpeciesModifier (auto-calculated)

// Business rule recalculates Value when BaseValue changes:
private class RecalculateValue : Csla.Rules.BusinessRule
{
    protected override void Execute(IRuleContext context)
    {
        var target = (AttributeEdit)context.Target;
        var newValue = target.BaseValue + target.SpeciesModifier;
        context.AddOutValue(ValueProperty, newValue);
    }
}
```

### Health Pool Calculation
```csharp
// Source: Fatigue.cs line 105-109
internal void CalculateBase(CharacterEdit character)
{
    var end = character.GetAttribute("END");
    var wil = character.GetAttribute("WIL");
    BaseValue = end + wil - 5;
}

// Source: Vitality follows same pattern with STR * 2 - 5
```

### Edit Mode Toggle Pattern
```csharp
// Source: Pattern derived from CharacterDetailGmActions.razor
@code {
    [Parameter] public CharacterEdit? Character { get; set; }
    [Parameter] public int CharacterId { get; set; }
    [Parameter] public Guid TableId { get; set; }
    [Parameter] public EventCallback OnCharacterUpdated { get; set; }

    @inject IDataPortal<CharacterEdit> characterPortal
    @inject ITimeEventPublisher TimeEventPublisher

    private bool isEditMode = false;
    private bool isProcessing = false;
    private CharacterEdit? editingCharacter;

    private async Task EnterEditMode()
    {
        editingCharacter = await characterPortal.FetchAsync(CharacterId);
        isEditMode = true;
    }

    private async Task SaveAndExit()
    {
        if (!ValidateAll()) return;

        isProcessing = true;
        try
        {
            // Health pool adjustment: cap current at new max
            AdjustHealthPoolsToNewMax();

            await characterPortal.UpdateAsync(editingCharacter!);
            await PublishCharacterUpdate();
            await OnCharacterUpdated.InvokeAsync();

            isEditMode = false;
            editingCharacter = null;
        }
        finally
        {
            isProcessing = false;
        }
    }

    private void CancelEdit()
    {
        isEditMode = false;
        editingCharacter = null;
    }

    private void AdjustHealthPoolsToNewMax()
    {
        if (editingCharacter!.Fatigue.Value > editingCharacter.Fatigue.BaseValue)
            editingCharacter.Fatigue.Value = editingCharacter.Fatigue.BaseValue;
        if (editingCharacter.Vitality.Value > editingCharacter.Vitality.BaseValue)
            editingCharacter.Vitality.Value = editingCharacter.Vitality.BaseValue;
    }
}
```

### Input with Validation Feedback
```razor
<!-- Source: Pattern from EffectFormModal.razor -->
<div class="col-md-4">
    <label class="form-label">@attr.Name Base</label>
    <input type="number"
           class="form-control form-control-sm @(attr.BaseValue < 1 ? "is-invalid" : attr.BaseValue > 14 ? "border-warning" : "")"
           @bind="attr.BaseValue"
           @bind:event="onblur"
           min="1" />
    @if (attr.BaseValue < 1)
    {
        <div class="invalid-feedback">Minimum value is 1</div>
    }
    else if (attr.BaseValue > 14)
    {
        <div class="text-warning small"><i class="bi bi-exclamation-triangle me-1"></i>Above typical range (6-14)</div>
    }
</div>
<div class="col-md-2 d-flex align-items-center">
    <span class="badge bg-info" title="Effective value (base @(attr.SpeciesModifier >= 0 ? "+" : "")@attr.SpeciesModifier species)">
        = @attr.Value
    </span>
</div>
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Manual recalculation | CSLA BusinessRules cascade | Project inception | All derived stats auto-update |
| Direct property edit | BaseValue + SpeciesModifier pattern | Character creation phase | Species modifiers preserved |
| Modal for every action | Inline edit mode toggle | Recent phases (20-inventory) | Fewer clicks, better UX |

**Deprecated/outdated:**
- None identified - current patterns are stable

## Open Questions

None - all technical aspects are clear from codebase analysis.

## Sources

### Primary (HIGH confidence)
- `GameMechanics/CharacterEdit.cs` - Business rules for FAT/VIT/AP recalculation
- `GameMechanics/AttributeEdit.cs` - BaseValue/SpeciesModifier/Value structure
- `GameMechanics/Fatigue.cs` - CalculateBase implementation (END + WIL - 5)
- `GameMechanics/ActionPoints.cs` - CalculateMax (TotalSkillLevels / 10), CalculateRecovery (FAT / 4)
- `GameMechanics/SkillEdit.cs` - Level property, AbilityScore calculation
- `design/GAME_RULES_SPECIFICATION.md` - Health pool formulas, attribute ranges
- `Threa.Client/Components/Shared/CharacterDetailGmActions.razor` - Edit patterns, messaging
- `Threa.Client/Components/Shared/EffectFormModal.razor` - Form validation patterns
- `GameMechanics/Messaging/TimeMessages.cs` - CharacterUpdateType.StatsChanged

### Secondary (MEDIUM confidence)
- None needed - all information from direct codebase analysis

### Tertiary (LOW confidence)
- None

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All libraries already in use
- Architecture: HIGH - Following existing patterns exactly
- Pitfalls: HIGH - Based on codebase structure analysis

**Research date:** 2026-01-29
**Valid until:** 60 days (stable patterns, no external dependencies)
