# Phase 18: Wound Management - Research

**Researched:** 2026-01-28
**Domain:** Wound CRUD operations, CSLA integration, Blazor modal patterns
**Confidence:** HIGH

## Summary

This research investigated how to implement wound management for the GM dashboard. The codebase already has a sophisticated wound system implemented through the effects infrastructure (EffectRecord + WoundBehavior). Phase 18's scope is adding GM UI controls to create, view, edit, and remove wounds through the existing CharacterDetailModal.

Key findings:
1. **Wounds are EffectRecords** - No new business object needed. Wounds use `EffectType.Wound` with `WoundState` stored in `BehaviorState` JSON.
2. **WoundBehavior provides helpers** - Static methods `TakeWound()`, `HealWound()`, `GetAllWoundStates()` already exist.
3. **RadzenDialog supports stacking** - Multiple dialogs work by default, enabling modal-over-modal pattern.
4. **AS penalties are automatic** - `WoundBehavior.GetAbilityScoreModifiers()` already returns -2 per wound for all skills.

**Primary recommendation:** Create a WoundModal component that opens from CharacterDetailGmActions. Use the existing WoundBehavior static methods for CRUD operations. Display wounds in a compact table with edit/delete per row.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CSLA.NET | 9.1.0 | Business object framework | Project standard, EffectRecord is CSLA |
| Radzen.Blazor | 8.4.2 | DialogService for modals | Already configured, supports stacking |
| Bootstrap 5 | 5.x | UI styling, badges, tables | Project standard |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| System.Text.Json | builtin | WoundState serialization | Deserialize BehaviorState |
| Bootstrap Icons | builtin | Wound/bandaid icons | Visual indicators |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| New Wound business object | EffectRecord + WoundState | Use existing - wounds are effects |
| Custom modal system | RadzenDialog | Use existing - already configured |
| Inline editing | Modal form | Modal aligns with CONTEXT.md decision |

**Installation:**
No new packages required. All dependencies already in project.

## Architecture Patterns

### Existing Wound Data Model
```
CharacterEdit
  └── Effects (EffectList : BusinessListBase<EffectList, EffectRecord>)
       └── EffectRecord (where EffectType == Wound)
            ├── Id (Guid)
            ├── EffectType = EffectType.Wound
            ├── Name = "Wound: {Location}"
            ├── Location = "Head" | "Torso" | "LeftArm" | etc.
            ├── BehaviorState = WoundState JSON
            └── IsActive = true

WoundState (JSON in BehaviorState):
{
  "LightWounds": int,
  "SeriousWounds": int,
  "MaxWounds": int,
  "IsCrippled": bool,
  "IsDestroyed": bool,
  "RoundsToDamage": int
}
```

### Recommended Component Structure
```
Components/Shared/
├── CharacterDetailGmActions.razor   # Existing - add "Manage Wounds" button
├── WoundManagementModal.razor       # NEW - wound list + add/edit
├── WoundFormModal.razor             # NEW - add/edit single wound
└── WoundListTable.razor             # NEW - compact wound display table
```

### Pattern 1: Modal-over-Modal with RadzenDialog
**What:** Open WoundFormModal from within CharacterDetailModal
**When to use:** Add/Edit wound actions
**Example:**
```csharp
// In CharacterDetailGmActions.razor
@inject DialogService DialogService

private async Task OpenAddWoundModal()
{
    var result = await DialogService.OpenAsync<WoundFormModal>(
        "Add Wound",
        new Dictionary<string, object>
        {
            { "CharacterId", CharacterId },
            { "Character", Character }
        },
        new DialogOptions
        {
            Width = "500px",
            CloseDialogOnOverlayClick = true
        });

    if (result == true)
    {
        await OnCharacterUpdated.InvokeAsync();
    }
}
```

### Pattern 2: WoundBehavior CRUD Operations
**What:** Use existing static methods for wound operations
**When to use:** All wound creation/modification
**Example:**
```csharp
// Source: GameMechanics/Effects/Behaviors/WoundBehavior.cs

// Adding a wound
WoundBehavior.TakeWound(character, location, effectPortal);

// Healing a wound (serious -> light -> removed)
WoundBehavior.HealWound(character, location);

// Getting all wounds
var wounds = WoundBehavior.GetAllWoundStates(character);
// Returns IEnumerable<(string Location, WoundState State)>

// Getting specific location
var state = WoundBehavior.GetLocationState(character, "Head");
```

### Pattern 3: Custom Wound Creation (Phase 18 Extension)
**What:** Create wound with GM-specified values (not just combat wound)
**When to use:** GM manually adds wound with custom severity/effects
**Example:**
```csharp
// Create EffectRecord directly for custom wound
var effect = await effectPortal.CreateChildAsync(
    EffectType.Wound,
    $"Wound: {location}",  // Name
    location,               // Location
    (int?)null,            // DurationRounds - wounds don't expire
    customWoundState.Serialize()  // BehaviorState
);
effect.Source = "GM";
character.Effects.AddEffect(effect);
```

### Anti-Patterns to Avoid
- **Creating new business object for wounds:** Wounds ARE effects. Use EffectRecord.
- **Bypassing WoundBehavior:** Direct manipulation of BehaviorState loses AS penalty calculation.
- **Forgetting to save:** Always save character after wound modifications.

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Wound AS penalty | Custom penalty calc | WoundBehavior.PenaltyPerWound (-2) | Already integrated with effect system |
| Wound periodic damage | Custom damage timer | WoundBehavior.OnTick() | Already integrated with time system |
| Wound severity tracking | Custom severity field | WoundState.SeriousWounds/LightWounds | State serialization handled |
| Modal stacking | Custom overlay system | RadzenDialog | Works by default |
| Wound count display | Custom counter | EffectList.TotalWoundCount | Already computed |

**Key insight:** The wound system is fully implemented in `WoundBehavior.cs`. Phase 18 is purely UI - adding forms and displays for existing functionality.

## Common Pitfalls

### Pitfall 1: State Loss on Nested Dialog
**What goes wrong:** Opening modal from modal can reset parent state
**Why it happens:** RadzenDialog re-renders parent when child opens/closes
**How to avoid:** Don't await nested dialog if parent state is critical; use callbacks
**Warning signs:** Form data disappears when closing child modal

### Pitfall 2: WoundState Deserialization Failure
**What goes wrong:** Null or empty WoundState when reading existing wound
**Why it happens:** BehaviorState is null or malformed JSON
**How to avoid:** Always use `WoundState.Deserialize()` which handles null/empty
**Warning signs:** Missing wound counts, null reference exceptions

### Pitfall 3: Forgetting Character Save
**What goes wrong:** Wound changes don't persist after modal closes
**Why it happens:** CSLA tracks dirty state but requires explicit save
**How to avoid:** Always call `await characterPortal.UpdateAsync(character)` after modifications
**Warning signs:** Wounds disappear on page refresh

### Pitfall 4: Direct EffectRecord Modification
**What goes wrong:** Wound doesn't apply AS penalty or periodic damage
**Why it happens:** Bypassed WoundBehavior.OnAdding/OnApply
**How to avoid:** Use `character.Effects.AddEffect(effect)` which calls behavior hooks
**Warning signs:** Wound shows in list but doesn't affect calculations

### Pitfall 5: RadzenDialog Not in Interactive Context
**What goes wrong:** DialogService.OpenAsync fails silently
**Why it happens:** RadzenDialog component missing from interactive render tree
**How to avoid:** Ensure `<RadzenDialog />` exists in GmTable.razor (already done)
**Warning signs:** Modal never appears, no errors logged

## Code Examples

Verified patterns from the codebase:

### Creating Wound via WoundBehavior
```csharp
// Source: GameMechanics/Effects/Behaviors/WoundBehavior.cs:231-268
public static void TakeWound(CharacterEdit character, string location,
    Csla.IChildDataPortal<EffectRecord> effectPortal)
{
    var existingWound = character.Effects
        .Where(e => e.EffectType == EffectType.Wound && e.Location == location)
        .FirstOrDefault();

    if (existingWound != null)
    {
        // Add to existing wound at location
        var state = WoundState.Deserialize(existingWound.BehaviorState);
        state.SeriousWounds++;
        // ... state updates
        existingWound.BehaviorState = state.Serialize();
    }
    else
    {
        // Create new wound effect
        var state = new WoundState
        {
            SeriousWounds = 1,
            MaxWounds = WoundState.GetMaxWoundsForLocation(location),
            RoundsToDamage = DamageIntervalRounds
        };

        var newWound = effectPortal.CreateChild(
            EffectType.Wound,
            $"Wound: {location}",
            location,
            null, // Wounds don't expire by duration
            state.Serialize());

        character.Effects.Add(newWound);
    }
}
```

### Getting Wound AS Penalty
```csharp
// Source: GameMechanics/Effects/Behaviors/WoundBehavior.cs:202-219
public IEnumerable<EffectModifier> GetAbilityScoreModifiers(
    EffectRecord effect, string skillName, string attributeName, int currentAS)
{
    var state = WoundState.Deserialize(effect.BehaviorState);
    if (state.TotalWounds == 0)
        return [];

    // Each wound applies a -2 penalty to ALL ability scores
    var totalPenalty = state.TotalWounds * PenaltyPerWound;

    return [
        new EffectModifier
        {
            Description = $"Wound: {effect.Location} ({state.TotalWounds})",
            Value = totalPenalty
        }
    ];
}
```

### Dialog Pattern from Existing Code
```csharp
// Source: GmTable.razor:689-703
var result = await DialogService.OpenAsync<CharacterDetailModal>(
    character.CharacterName,
    new Dictionary<string, object>
    {
        { "CharacterId", character.CharacterId },
        { "TableId", TableId },
        { "AllCharacters", tableCharacters ?? Enumerable.Empty<TableCharacterInfo>() }
    },
    new DialogOptions
    {
        Width = "90%",
        Height = "90%",
        CloseDialogOnOverlayClick = true,
        ShowClose = false
    });
```

### Effect Creation Pattern
```csharp
// Source: CharacterDetailGmActions.razor:312-324
var effect = await effectPortal.CreateChildAsync(
    EffectType.Condition,
    selectedEffect,
    (string?)null,
    (int?)null,
    (string?)null
);
effect.Source = "GM";

character.Effects.AddEffect(effect);
await characterPortal.UpdateAsync(character);
```

### Publishing Character Update
```csharp
// Source: CharacterDetailGmActions.razor:326-334
await TimeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
{
    CharacterId = CharacterId,
    UpdateType = CharacterUpdateType.EffectAdded,
    CampaignId = TableId.ToString(),
    SourceId = "GM",
    Description = selectedEffect
});
```

## Integration Points

### CharacterDetailGmActions Integration
- Add "Manage Wounds" or "Add Wound" button to GM Actions card
- Pattern: Same as existing "Apply Effect" card
- Location: New card or within existing Effects card

### CharacterDetailModal Header Integration
- CONTEXT.md: "Title bar icons: One icon/badge per wound, hover shows tooltip"
- Location: Lines 62-66 already show wound badge
- Enhancement: Individual wound icons with tooltips

### Health Management Integration (Phase 17)
- CONTEXT.md: "Damage-to-wound prompt when VIT damage exceeds threshold"
- Pattern: Check `projectedVit < 0` in damage flow, show prompt
- Integration point: `CharacterDetailGmActions.ApplyToPool()` line 206-215

### Time System Integration
- Already implemented: `WoundBehavior.OnTick()` applies periodic damage
- Already integrated: `EffectList.EndOfRound()` calls OnTick
- No new integration needed for basic CRUD

### AS Penalty Display
- Already computed: `Effects.GetAbilityScoreModifiers()`
- Display location: CharacterDetailSheet or TabStatus
- Existing pattern: TabStatus.razor lines 540-542 show TotalWoundPenalty

## Severity Mapping

CONTEXT.md defines severity levels. Map to existing WoundState:

| Phase 18 Severity | WoundState Mapping | AS Penalty | FAT/VIT Damage |
|-------------------|-------------------|------------|----------------|
| Minor | LightWounds = 1 | -2 | FAT only per tick |
| Moderate | SeriousWounds = 1 | -2 | FAT + VIT per tick |
| Severe | SeriousWounds = 2 | -4 | FAT + VIT per tick |
| Critical | SeriousWounds = 3+ | -6+ | FAT + VIT per tick |

**Decision needed:** CONTEXT.md specifies custom FAT/VIT damage rates per severity. This extends beyond current WoundState model.

### Extended WoundState Option
```csharp
public class ExtendedWoundState : WoundState
{
    public string Severity { get; set; }        // Minor, Moderate, Severe, Critical
    public string Description { get; set; }     // Custom description
    public int CustomASPenalty { get; set; }    // GM-specified penalty
    public int FatDamagePerMinute { get; set; } // Custom FAT rate
    public int VitDamagePerMinute { get; set; } // Custom VIT rate
}
```

**Recommendation:** Start with existing WoundState for Phase 18. Severity can be inferred from wound counts. Custom FAT/VIT rates would require WoundBehavior modification - defer to Phase 18 implementation discretion.

## Common Wound Templates

CONTEXT.md: "Common wound dropdown: Quick-select from preset wounds"

Suggested presets (Claude's discretion per CONTEXT.md):

| Template | Location | Severity | Description |
|----------|----------|----------|-------------|
| Broken Arm | LeftArm/RightArm | Moderate | Fractured bone |
| Broken Leg | LeftLeg/RightLeg | Moderate | Fractured bone |
| Concussion | Head | Moderate | Head trauma |
| Deep Cut | Any | Minor | Bleeding wound |
| Puncture Wound | Torso | Moderate | Piercing injury |
| Burns | Any | Varies | Thermal damage |
| Internal Bleeding | Torso | Severe | Internal injury |

**Implementation:** Static list in WoundFormModal, populates form fields on selection.

## Open Questions

Things that couldn't be fully resolved:

1. **Custom Severity Persistence**
   - What we know: WoundState has SeriousWounds/LightWounds, not custom severity
   - What's unclear: Should we extend WoundState or use Description field?
   - Recommendation: Use Description for custom text, infer severity from wound counts for mechanics

2. **FAT/VIT Damage Rate Customization**
   - What we know: WoundBehavior has fixed damage rates (1 VIT + 2 FAT per serious wound)
   - What's unclear: CONTEXT.md says GM sets final values - requires behavior modification
   - Recommendation: Document as future enhancement, use standard rates for Phase 18

3. **Unified Healing UI**
   - What we know: CONTEXT.md says "Healing mode allows GM to apply healing to pools OR reduce wound severity"
   - What's unclear: Is this separate from Health Management (Phase 17)?
   - Recommendation: Add wound healing option to existing damage/healing card

## Sources

### Primary (HIGH confidence)
- `GameMechanics/Effects/Behaviors/WoundBehavior.cs` - Complete wound implementation
- `GameMechanics/EffectRecord.cs` - Effect business object
- `GameMechanics/EffectList.cs` - Effect collection with wound helpers
- `Threa/Threa.Client/Components/Shared/CharacterDetailGmActions.razor` - Existing GM actions
- `Threa/Threa.Client/Components/Shared/CharacterDetailModal.razor` - Modal structure

### Secondary (MEDIUM confidence)
- [Radzen Dialog Service Multiple Modals](https://forum.radzen.com/t/dialog-service-multiple-modals-overlayed/20485) - Confirms stacking works
- `design/EFFECTS_SYSTEM.md` - Effect system design document

### Tertiary (LOW confidence)
- [Radzen GitHub Issue #174](https://github.com/radzenhq/radzen-blazor/issues/174) - State loss with nested dialogs (edge case)

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All components exist in codebase
- Architecture: HIGH - Clear patterns from existing code
- Pitfalls: MEDIUM - Modal stacking has known edge cases
- Custom severity: MEDIUM - May require WoundState extension

**Research date:** 2026-01-28
**Valid until:** 30+ days (stable domain, existing infrastructure)
