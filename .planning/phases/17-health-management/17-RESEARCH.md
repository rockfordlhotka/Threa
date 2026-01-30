# Phase 17: Health Management - Research

**Researched:** 2026-01-28
**Domain:** Blazor UI for Health Pool Management (Damage/Healing Controls with Real-time Updates)
**Confidence:** HIGH

## Summary

This phase extends the existing GM dashboard character modal to add enhanced damage/healing controls for FAT and VIT pools. The research confirms that the core infrastructure already exists in `CharacterDetailGmActions.razor` - it currently has separate damage and healing cards with numeric inputs and FAT/VIT buttons. The phase requirements call for refinement: a damage/healing mode toggle, overflow warnings when FAT damage would cascade to VIT, overheal notifications, and enhanced progress bar visualization with color-coded thresholds.

The existing codebase provides all necessary building blocks:
- **Business objects:** `Fatigue.cs` and `Vitality.cs` with PendingDamage and PendingHealing properties
- **UI components:** `PendingPoolBar.razor` for health visualization with pending damage/healing segments
- **Messaging:** `CharacterUpdateMessage` with `CharacterUpdateType.Damage` and `CharacterUpdateType.Healing` for real-time updates
- **Modal system:** `CharacterDetailModal.razor` with tabbed interface and `CharacterDetailGmActions.razor` GM actions tab

The implementation is primarily a UI enhancement to the existing `CharacterDetailGmActions.razor` component, not new infrastructure.

**Primary recommendation:** Refactor the existing damage/healing cards in `CharacterDetailGmActions.razor` into a unified health management section with mode toggle, enhanced `PendingPoolBar` with color thresholds, and overflow/overheal warning logic.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Blazor Web App | .NET 10 | Component framework | Already in use, SSR + InteractiveServer |
| CSLA.NET | 9.1.0 | Business objects (CharacterEdit, Fatigue, Vitality) | Core project architecture |
| Bootstrap 5 | bundled | CSS framework, form controls | Already used for all UI styling |
| Bootstrap Icons | bundled | Icon library | Already used for all icons (bi-* classes) |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| InMemoryMessageBus | project | Real-time pub/sub | Character update notifications |
| PendingPoolBar.razor | project | Health visualization | Display FAT/VIT with pending changes |
| CSS Variables (themes.css) | project | Theme-aware styling | Color thresholds, progress bars |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Mode toggle button | Separate damage/healing sections | Toggle is cleaner per CONTEXT.md decision |
| Bootstrap alerts for warnings | Radzen notifications | Bootstrap already integrated, simpler |
| Custom progress bar | Radzen progress | PendingPoolBar already handles pending visualization |

**Installation:**
```bash
# No new packages needed - all dependencies already in project
```

## Architecture Patterns

### Recommended Project Structure
```
Threa.Client/Components/
  Shared/
    CharacterDetailGmActions.razor    # MODIFY: Refactor to mode toggle, add warnings
    PendingPoolBar.razor              # MODIFY: Add color threshold support
    PendingPoolBar.razor.cs           # MODIFY: Add threshold calculation logic
    HealthManagementCard.razor        # NEW (optional): Extract health UI if complex
```

### Pattern 1: Mode Toggle with Single Input
**What:** Use a two-button toggle group to switch between damage and healing modes
**When to use:** When the same numeric input applies to different operations
**Example:**
```html
<!-- Source: Bootstrap button group pattern -->
<div class="btn-group w-100 mb-3" role="group">
    <input type="radio" class="btn-check" name="healthMode" id="modeDamage"
           checked="@(healthMode == HealthMode.Damage)"
           @onchange="@(() => healthMode = HealthMode.Damage)" />
    <label class="btn btn-outline-danger" for="modeDamage">
        <i class="bi bi-dash-circle me-1"></i>Damage
    </label>

    <input type="radio" class="btn-check" name="healthMode" id="modeHealing"
           checked="@(healthMode == HealthMode.Healing)"
           @onchange="@(() => healthMode = HealthMode.Healing)" />
    <label class="btn btn-outline-success" for="modeHealing">
        <i class="bi bi-plus-circle me-1"></i>Healing
    </label>
</div>
```

### Pattern 2: Overflow Warning Before Apply
**What:** Check if damage would overflow FAT and cascade to VIT before applying
**When to use:** Preventing unintended VIT damage from FAT overflow
**Example:**
```csharp
// Source: Game mechanics from Fatigue.cs EndOfRound logic
private bool WouldOverflowToVit()
{
    var projectedFat = Character.Fatigue.Value - Character.Fatigue.PendingDamage - damageAmount;
    return projectedFat < 0;
}

private int CalculateVitOverflow()
{
    var projectedFat = Character.Fatigue.Value - Character.Fatigue.PendingDamage - damageAmount;
    return projectedFat < 0 ? Math.Abs(projectedFat) : 0;
}
```

### Pattern 3: Color-Coded Progress Bar Thresholds
**What:** Change progress bar color based on current pool percentage
**When to use:** Visual health state indication per CONTEXT.md decision
**Example:**
```csharp
// Source: Pattern from existing CharacterStatusCard health state logic
protected string GetProgressBarColor()
{
    var percentage = MaxValue > 0 ? (double)CurrentValue / MaxValue * 100 : 0;

    if (percentage > 50) return "var(--color-health-full)";   // Green
    if (percentage > 25) return "var(--color-health-mid)";    // Yellow
    return "var(--color-health-low)";                          // Red
}
```

### Pattern 4: Confirmation Dialog for Overflow
**What:** Show modal confirmation when FAT damage would cascade to VIT
**When to use:** Per CONTEXT.md decision - warn GM and let them confirm or adjust
**Example:**
```html
<!-- Source: Existing confirmation pattern in CharacterDetailGmActions.razor -->
@if (showOverflowConfirm)
{
    <div class="alert alert-warning">
        <strong><i class="bi bi-exclamation-triangle me-1"></i>FAT Overflow</strong>
        <p class="mb-2">This damage will exceed FAT capacity. @overflowAmount points will cascade to VIT.</p>
        <div class="d-flex gap-2">
            <button class="btn btn-warning btn-sm" @onclick="ConfirmApply">Apply Anyway</button>
            <button class="btn btn-secondary btn-sm" @onclick="CancelApply">Adjust Amount</button>
        </div>
    </div>
}
```

### Anti-Patterns to Avoid
- **Applying damage directly without pending:** Always add to PendingDamage, never modify Value directly
- **Forgetting to publish CharacterUpdateMessage:** Real-time updates require message publication
- **Hard-coded color values:** Use CSS variables from themes.css for theme consistency
- **Blocking on confirmation:** Show inline warning, don't use blocking modal dialogs

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Health bar visualization | Custom progress bar | `PendingPoolBar.razor` | Already handles pending damage/healing segments |
| Real-time character updates | Custom SignalR | `CharacterUpdateMessage` + `ITimeEventPublisher` | Existing infrastructure |
| Theme-aware colors | Inline hex colors | CSS variables (--color-health-*) | Automatic fantasy/sci-fi theming |
| Damage/healing logic | Custom calculations | `Fatigue.PendingDamage`, `Vitality.PendingDamage` | Business objects handle overflow logic in EndOfRound |
| Form validation | Manual checks | Bootstrap form classes + component state | Standard pattern, accessible |

**Key insight:** The Fatigue and Vitality business objects already handle all the complex logic (overflow, capping, wounds). The UI only needs to add to PendingDamage or PendingHealing and save - the round processing will apply the changes correctly.

## Common Pitfalls

### Pitfall 1: Modifying Value Instead of PendingDamage
**What goes wrong:** Damage applies immediately and bypasses the pending system
**Why it happens:** Developer assumes Value should be modified directly
**How to avoid:** Always use `character.Fatigue.PendingDamage += amount` or `character.Vitality.PendingDamage += amount`, never modify Value directly
**Warning signs:** Damage appears without pending indicator, no animation opportunity

### Pitfall 2: Forgetting to Save After Modification
**What goes wrong:** Changes appear in UI but are lost on refresh
**Why it happens:** Business object is modified but `characterPortal.UpdateAsync()` is not called
**How to avoid:** Always follow the pattern:
```csharp
var character = await characterPortal.FetchAsync(CharacterId);
character.Fatigue.PendingDamage += damageAmount;
await characterPortal.UpdateAsync(character);
```
**Warning signs:** Values reset when switching tabs or refreshing

### Pitfall 3: Not Publishing CharacterUpdateMessage
**What goes wrong:** Other views (player dashboard, other GM tabs) don't update
**Why it happens:** Save succeeded but message was not published
**How to avoid:** Always publish after successful save:
```csharp
await TimeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
{
    CharacterId = CharacterId,
    UpdateType = CharacterUpdateType.Damage,
    CampaignId = TableId.ToString(),
    SourceId = "GM"
});
```
**Warning signs:** Player dashboard shows stale values, modal doesn't refresh

### Pitfall 4: Overheal Logic Confusion
**What goes wrong:** Healing is capped at max when temporary overheal should be allowed
**Why it happens:** Existing PendingHealing capping logic in PendingPoolBar
**How to avoid:** Per CONTEXT.md, pools can exceed maximum values. Modify PendingPoolBar calculation:
```csharp
// Allow healing percentage to extend beyond 100%
HealingPercentage = (double)PendingHealing / MaxValue * 100;
// Do NOT cap at MaxValue - current
```
**Warning signs:** Overheal buffs don't show in progress bar

### Pitfall 5: Progress Bar Percentage Exceeding Container
**What goes wrong:** When allowing overheal, bar extends beyond 100% width
**Why it happens:** CSS width is set to percentage without max-width constraint
**How to avoid:** For overheal visualization, use overlay or separate indicator:
```css
/* Overheal indicator extends beyond bar */
.overheal-indicator {
    position: absolute;
    right: -10px;
    background: var(--color-health-full);
    /* Use badge or glow effect */
}
```
**Warning signs:** Progress bar breaks layout when healing exceeds max

### Pitfall 6: Mode Toggle State Not Persisting
**What goes wrong:** Mode resets to damage after applying damage/healing
**Why it happens:** StateHasChanged resets component if not using component state properly
**How to avoid:** Store mode in component field, not computed property:
```csharp
private HealthMode healthMode = HealthMode.Damage;  // Persists across renders
```
**Warning signs:** User has to re-select healing mode for each application

## Code Examples

Verified patterns from the existing codebase:

### Current Damage Application (from CharacterDetailGmActions.razor)
```csharp
// Source: CharacterDetailGmActions.razor lines 169-209
private async Task ApplyDamageToPool(string pool)
{
    if (Character == null) return;
    isProcessing = true;

    try
    {
        var character = await characterPortal.FetchAsync(CharacterId);

        if (pool == "FAT")
        {
            character.Fatigue.PendingDamage += damageAmount;
        }
        else
        {
            character.Vitality.PendingDamage += damageAmount;
        }

        await characterPortal.UpdateAsync(character);

        await TimeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
        {
            CharacterId = CharacterId,
            UpdateType = CharacterUpdateType.Damage,
            CampaignId = TableId.ToString(),
            SourceId = "GM",
            Description = $"{damageAmount} {pool} damage"
        });

        ShowFeedback($"Saved: {damageAmount} {pool} damage applied", "alert-success", "bi-check-circle");
        await OnCharacterUpdated.InvokeAsync();
    }
    catch (Exception ex)
    {
        ShowFeedback($"Error saving: {ex.Message}", "alert-danger", "bi-exclamation-triangle");
    }
    finally
    {
        isProcessing = false;
    }
}
```

### PendingPoolBar Current Implementation
```csharp
// Source: PendingPoolBar.razor.cs
protected override void OnParametersSet()
{
    if (MaxValue <= 0)
    {
        BasePercentage = 0;
        DamagePercentage = 0;
        HealingPercentage = 0;
        return;
    }

    var current = Math.Clamp(CurrentValue, 0, MaxValue);
    var damage = Math.Clamp(PendingDamage, 0, current);
    var healing = Math.Max(0, PendingHealing);

    var valueAfterDamage = current - damage;

    BasePercentage = (double)valueAfterDamage / MaxValue * 100;
    DamagePercentage = (double)damage / MaxValue * 100;
    HealingPercentage = (double)Math.Min(healing, MaxValue - current) / MaxValue * 100;
}
```

### Enhanced Progress Bar with Color Thresholds
```csharp
// New: Add to PendingPoolBar.razor.cs
protected string GetBarColorClass()
{
    // Calculate effective value after pending changes
    var effectiveValue = CurrentValue - PendingDamage + PendingHealing;
    var percentage = MaxValue > 0 ? (double)effectiveValue / MaxValue * 100 : 0;

    if (percentage > 50) return "health-full";
    if (percentage > 25) return "health-mid";
    return "health-low";
}

// CSS variables already exist in themes.css:
// --color-health-full: #355e3b (fantasy) / #00ff88 (scifi)
// --color-health-mid: #b8860b (fantasy) / #ffcc00 (scifi)
// --color-health-low: #8b0000 (fantasy) / #ff3355 (scifi)
```

### Mode Toggle Implementation
```html
<!-- New: Refactored health management section -->
<div class="card h-100">
    <div class="card-header" style="background-color: @(healthMode == HealthMode.Damage ? "var(--btn-danger-bg)" : "var(--btn-success-bg)"); color: white;">
        <strong>
            <i class="bi @(healthMode == HealthMode.Damage ? "bi-heart-pulse" : "bi-bandaid") me-1"></i>
            @(healthMode == HealthMode.Damage ? "Apply Damage" : "Apply Healing")
        </strong>
    </div>
    <div class="card-body">
        <!-- Mode Toggle -->
        <div class="btn-group w-100 mb-3" role="group">
            <button class="btn @(healthMode == HealthMode.Damage ? "btn-danger" : "btn-outline-danger")"
                    @onclick="() => healthMode = HealthMode.Damage">
                <i class="bi bi-dash-circle me-1"></i>Damage
            </button>
            <button class="btn @(healthMode == HealthMode.Healing ? "btn-success" : "btn-outline-success")"
                    @onclick="() => healthMode = HealthMode.Healing">
                <i class="bi bi-plus-circle me-1"></i>Healing
            </button>
        </div>

        <!-- Amount Input -->
        <div class="mb-3">
            <label class="form-label">Amount</label>
            <input type="number" class="form-control" @bind="healthAmount" min="1" />
        </div>

        <!-- Apply Buttons -->
        <div class="d-flex gap-2">
            <button class="btn @(healthMode == HealthMode.Damage ? "btn-danger" : "btn-success") flex-grow-1"
                    @onclick="ApplyToFat" disabled="@isProcessing">
                FAT
            </button>
            <button class="btn @(healthMode == HealthMode.Damage ? "btn-danger" : "btn-success") flex-grow-1"
                    @onclick="ApplyToVit" disabled="@isProcessing">
                VIT
            </button>
        </div>

        <!-- Overflow Warning -->
        @if (showOverflowWarning)
        {
            <div class="alert alert-warning mt-3 mb-0">
                <i class="bi bi-exclamation-triangle me-1"></i>
                @overflowWarningMessage
                <div class="mt-2">
                    <button class="btn btn-warning btn-sm" @onclick="ConfirmApply">Apply Anyway</button>
                    <button class="btn btn-secondary btn-sm" @onclick="CancelApply">Cancel</button>
                </div>
            </div>
        }
    </div>
</div>
```

### Overflow Warning Logic
```csharp
// New: Add overflow detection and warning
private enum HealthMode { Damage, Healing }
private HealthMode healthMode = HealthMode.Damage;
private int healthAmount = 1;
private bool showOverflowWarning;
private string overflowWarningMessage = "";
private string pendingPool = "";

private async Task ApplyToFat()
{
    pendingPool = "FAT";

    if (healthMode == HealthMode.Damage && Character != null)
    {
        // Check for overflow
        var projectedFat = Character.Fatigue.Value - Character.Fatigue.PendingDamage - healthAmount;
        if (projectedFat < 0)
        {
            var overflow = Math.Abs(projectedFat);
            overflowWarningMessage = $"This will exceed FAT capacity. {overflow} points will cascade to VIT.";
            showOverflowWarning = true;
            return;
        }
    }
    else if (healthMode == HealthMode.Healing && Character != null)
    {
        // Check for overheal
        var projectedFat = Character.Fatigue.Value + Character.Fatigue.PendingHealing + healthAmount;
        if (projectedFat > Character.Fatigue.BaseValue)
        {
            var overheal = projectedFat - Character.Fatigue.BaseValue;
            overflowWarningMessage = $"This will exceed max FAT. {overheal} points of temporary overheal.";
            showOverflowWarning = true;
            return;
        }
    }

    await ExecuteApply();
}

private async Task ConfirmApply()
{
    showOverflowWarning = false;
    await ExecuteApply();
}

private void CancelApply()
{
    showOverflowWarning = false;
    pendingPool = "";
}

private async Task ExecuteApply()
{
    if (healthMode == HealthMode.Damage)
    {
        await ApplyDamageToPool(pendingPool);
    }
    else
    {
        await ApplyHealingToPool(pendingPool);
    }
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Separate damage/healing cards | Mode toggle with single input | Phase 17 | Cleaner UI, faster operation |
| No overflow warnings | Inline warnings with confirm | Phase 17 | Prevents accidental VIT damage |
| Fixed progress bar colors | Color-coded thresholds | Phase 17 | Better visual health indication |

**Deprecated/outdated:**
- Direct Value modification: Always use PendingDamage/PendingHealing
- Blocking confirmation dialogs: Use inline warnings for faster workflow

## Open Questions

Things that couldn't be fully resolved:

1. **Animation Timing for Progress Bars**
   - What we know: CONTEXT.md says "Visual animation only for confirmation - bars animate from old to new values when changes apply"
   - What's unclear: Exact animation duration and easing function
   - Recommendation: Use CSS transition with 300ms ease-out, matches Bootstrap patterns

2. **Overheal Visual Indicator Design**
   - What we know: Pools can exceed maximum (temporary overheal allowed)
   - What's unclear: Exact visual treatment when bar would exceed 100%
   - Recommendation: Show "+X" badge next to bar rather than extending bar beyond container

3. **Warning Threshold vs Apply Threshold**
   - What we know: Warn on overflow/overheal per CONTEXT.md
   - What's unclear: Should warning appear on input change or only on apply button click?
   - Recommendation: Show warning only when apply is clicked to avoid UI flicker during typing

## Sources

### Primary (HIGH confidence)
- `S:\src\rdl\threa\Threa\Threa.Client\Components\Shared\CharacterDetailGmActions.razor` - Existing damage/healing UI
- `S:\src\rdl\threa\GameMechanics\Fatigue.cs` - FAT business object with PendingDamage/PendingHealing
- `S:\src\rdl\threa\GameMechanics\Vitality.cs` - VIT business object with overflow to wounds logic
- `S:\src\rdl\threa\Threa\Threa.Client\Components\Shared\PendingPoolBar.razor` - Health visualization
- `S:\src\rdl\threa\GameMechanics\Messaging\TimeMessages.cs` - CharacterUpdateMessage definition
- `S:\src\rdl\threa\Threa\Threa\wwwroot\css\themes.css` - Color variables for thresholds
- `S:\src\rdl\threa\.planning\phases\17-health-management\17-CONTEXT.md` - User decisions

### Secondary (MEDIUM confidence)
- Phase 14 and 15 research - Dashboard patterns and modal infrastructure
- Bootstrap 5 documentation - Button groups, form controls, alerts

### Tertiary (LOW confidence)
- None - all findings verified against codebase

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All components already in project, no new dependencies
- Architecture: HIGH - Follows established patterns from Phase 14/15, extends existing component
- Pitfalls: HIGH - Based on actual codebase analysis and game mechanics review
- Code examples: HIGH - Derived from existing working code in CharacterDetailGmActions.razor

**Research date:** 2026-01-28
**Valid until:** 60 days (stable patterns, internal codebase only)
