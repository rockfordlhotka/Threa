# Phase 36: Other Group - Research

**Researched:** 2026-02-13
**Domain:** Blazor Combat Tab UI - Other group utility actions (Medical, Rest, Implants, Reload, Unload)
**Confidence:** HIGH

## Summary

Phase 36 completes the v1.7 Combat Tab Rework by wiring utility actions into the Other group. All five button handlers and mode components (MedicalMode, ReloadMode, UnloadMode, ActivateImplantMode) **already exist** and are fully functional in the codebase from Phase 32. The current Other group in `TabCombat.razor` (lines 245-289) already has all five buttons rendered with `disabled` attributes. This phase makes three targeted changes:

1. **Visibility change**: Convert three conditional buttons (Implants, Reload, Unload) from `disabled` to **hidden** (`@if` guarded) when their preconditions are not met. Medical and Rest remain always visible.
2. **Rest confirmation flow**: Replace the inline `Rest()` method (which executes immediately at line 1268) with a new `CombatMode.Rest` that shows a confirmation panel ("Spend 1 AP to recover 1 FAT" with Confirm/Cancel).
3. **Cost-aware dimming**: Apply the existing `combat-tile-passive-only` CSS class (opacity: 0.65) to ALL combat tiles across ALL three groups when the character lacks sufficient AP/FAT to use them, with cost-explaining tooltips.

**Primary recommendation:** This is a UI refinement phase with minimal new logic. Modify `TabCombat.razor` button rendering, add a `CombatMode.Rest` enum value and a simple inline confirmation panel, extend the `combat-tile-passive-only` pattern to a general-purpose dimming class, and apply it conditionally across all three groups.

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Blazor (InteractiveServer) | .NET 10 | UI rendering and reactivity | Already in use for all combat tab modes |
| Bootstrap 5 | Current | Layout, flex, gap utilities | Already in use for combat tile layout |
| Bootstrap Icons | Current | Icons for all buttons | Already in use (bi-heart-pulse, bi-cup-hot, bi-cpu, bi-box-arrow-in-down, bi-box-arrow-up) |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| CSLA .NET | 9.1.0 | Character save after Rest action | Already used for `savable.SaveAsync()` in existing Rest method |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Inline Rest confirmation panel | Radzen DialogService modal | Modal is heavier; CONTEXT.md specifies inline panel following combat mode pattern |
| CSS opacity dimming | Custom overlay or badge | Opacity reuses existing `combat-tile-passive-only` pattern; badges were explicitly rejected in CONTEXT.md |

## Architecture Patterns

### Existing Component Structure (Other Group Modes)
```
TabCombat.razor
  combatMode == Default      ->  Other group card (Medical + Rest + Implants + Reload + Unload tiles)
  combatMode == Medical      ->  <MedicalMode ... />
  combatMode == ActivateImplant -> <ActivateImplantMode ... />
  combatMode == Reload       ->  <ReloadMode ... />
  combatMode == Unload       ->  <UnloadMode ... />
  combatMode == Rest (NEW)   ->  Inline confirmation panel
```

### Pattern 1: Hidden vs Disabled Buttons
**What:** Use `@if` to conditionally render buttons (hidden when precondition is false) instead of the current `disabled` attribute approach.
**When to use:** For Implants, Reload, Unload buttons whose preconditions are equipment-based.
**Current code (TabCombat.razor lines 267-286):**
```html
<!-- CURRENT: disabled when no toggleable implants -->
<button class="btn combat-tile combat-tile-other"
        @onclick="StartActivateImplant"
        disabled="@(!hasToggleableImplantsEquipped)"
        title="Activate / Deactivate implants">
    <i class="bi bi-cpu"></i>
    <span>Implants</span>
</button>
```
**Target pattern:**
```html
<!-- NEW: hidden entirely when no toggleable implants -->
@if (hasToggleableImplantsEquipped)
{
    <button class="btn combat-tile combat-tile-other"
            @onclick="StartActivateImplant"
            title="Activate / Deactivate implants">
        <i class="bi bi-cpu"></i>
        <span>Implants</span>
    </button>
}
```

### Pattern 2: Inline Rest Confirmation Panel
**What:** A new `CombatMode.Rest` that replaces the default view with a simple confirmation panel, following the same combat mode pattern as all other modes.
**When to use:** When player clicks the Rest button.
**Design decision from CONTEXT.md:** "Rest gets an inline confirmation panel (like other combat modes) instead of executing instantly. Panel shows simple text: 'Spend 1 AP to recover 1 FAT' with Confirm/Cancel."
**Example approach:**
```html
else if (combatMode == CombatMode.Rest)
{
    <div class="card">
        <div class="card-header">
            <i class="bi bi-cup-hot"></i> <strong>Rest</strong>
        </div>
        <div class="card-body text-center">
            <p>Spend 1 AP to recover 1 FAT</p>
            <div class="d-flex justify-content-center gap-2">
                <button class="btn btn-success" @onclick="ConfirmRest">
                    <i class="bi bi-check"></i> Confirm
                </button>
                <button class="btn btn-outline-secondary" @onclick="ReturnToDefault">
                    <i class="bi bi-x-lg"></i> Cancel
                </button>
            </div>
        </div>
    </div>
}
```
The existing `Rest()` method logic (lines 1268-1295) moves into a new `ConfirmRest()` method. The `Rest` button's `@onclick` changes to `StartRest` which sets `combatMode = CombatMode.Rest`.

### Pattern 3: Cost-Aware Dimming Across All Groups
**What:** Apply a dimming CSS class to combat tiles when the character lacks sufficient AP/FAT to use the action, with a cost-explaining tooltip.
**When to use:** All combat tiles across Actions, Defense, and Other groups.
**Design decision from CONTEXT.md:** "Buttons dim (like Defend's `combat-tile-passive-only` pattern) when AP is too low to use them. Dimmed tooltip explains the cost. Apply dimming pattern to ALL combat tile groups for consistency."
**Existing CSS (themes.css line 2189):**
```css
.combat-tile-passive-only {
    opacity: 0.65;
}
```
**Implementation:** Rename or create a general-purpose `combat-tile-dimmed` class (same opacity: 0.65), and conditionally apply it to buttons that the player cannot currently afford. The `disabled` attribute should NOT be used for dimming (per CONTEXT.md: "disabled when passed out or no AP" is different from "dimmed when cost is too high"). Dimmed buttons remain clickable but the mode handles insufficient resources.

**Key distinction:**
- `disabled` = Cannot use at all (passed out, precondition not met) -- uses existing `:disabled { opacity: 0.4 }` CSS
- `combat-tile-dimmed` = Can conceptually use but lacks resources right now -- uses `opacity: 0.65` and cost tooltip

### Anti-Patterns to Avoid
- **Duplicating mode component logic**: Medical, Reload, Unload, Implant modes are fully functional. Wire them through as-is.
- **Building a standalone Rest component**: CONTEXT.md says "inline confirmation panel" -- a few lines of HTML directly in TabCombat, not a separate .razor file.
- **Applying dimming only to Other group**: CONTEXT.md explicitly says ALL three groups for consistency.
- **Using disabled for dimming**: Dimmed buttons should remain clickable; the mode itself validates. `disabled` makes buttons un-clickable.
- **Hiding Medical or Rest**: These are always visible per CONTEXT.md. Only Implants, Reload, Unload hide.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Medical treatment flow | New medical UI | Existing `MedicalMode.razor` (fully functional) | Has skill selection, concentration, cost, rolling |
| Reload flow | New reload UI | Existing `ReloadMode.razor` (fully functional) | Has weapon/ammo selection, cost, concentration |
| Unload flow | New unload UI | Existing `UnloadMode.razor` (fully functional) | Has weapon selection, immediate/concentration unload |
| Implant activation | New implant UI | Existing `ActivateImplantMode.razor` (fully functional) | Has toggle on/off, AP cost per toggle |
| Rest AP deduction | New rest logic | Existing `Rest()` method in TabCombat (lines 1268-1295) | Already handles parry mode end, AP deduction, FAT healing, save, logging |
| Equipment detection | Re-query items | Existing `hasRangedWeaponEquipped`, `hasToggleableImplantsEquipped`, `HasWeaponWithAmmo()` | Already computed in `LoadEquipmentAndSkills()` |

**Key insight:** This phase is ~95% UI wiring and ~5% new logic (Rest confirmation panel + dimming CSS class). Every mode component and handler already exists.

## Common Pitfalls

### Pitfall 1: Rest Confirmation Panel Not Following Mode Pattern
**What goes wrong:** Building the Rest confirmation as a separate component or as a modal dialog instead of an inline panel.
**Why it happens:** Other actions (Medical, Reload) have their own `.razor` component files. Rest seems like it should too.
**How to avoid:** CONTEXT.md explicitly says "inline confirmation panel (like other combat modes)." This means adding `CombatMode.Rest` to the enum and rendering a simple HTML block directly in `TabCombat.razor`'s mode switch, NOT creating a `RestMode.razor` file. The confirmation is ~10 lines of HTML.
**Warning signs:** A new `RestMode.razor` file being created.

### Pitfall 2: Dimming vs Disabled Confusion
**What goes wrong:** Using the HTML `disabled` attribute for AP-insufficient dimming, making buttons un-clickable even though the player might want to see why they can't act.
**Why it happens:** The current code already uses `disabled="@(!CanAct())"` everywhere.
**How to avoid:** Dimmed buttons use a CSS class (`combat-tile-dimmed`) for visual feedback but remain clickable. The `disabled` attribute is reserved for hard precondition failures (passed out, no weapon equipped). The mode component handles the "insufficient resources" case internally.
**Implementation note:** For the Actions group specifically, buttons like Attack that use `disabled="@(!CanAct())"` would change to use the dimming class instead when AP is insufficient, while keeping `disabled` only for `IsPassedOut`. However, per CONTEXT.md, the precise behavior is: "Buttons dim when AP is too low to use them" -- so the dimming is additive. A button can be both dimmed AND disabled (passed out + no AP). The simplest approach: add the dimming class conditionally, leave disabled as-is for truly impossible actions.

**Recommended approach for Actions group:**
- Keep `disabled="@(Character?.IsPassedOut ?? true)"` for the hard block
- Add `combat-tile-dimmed` class when `!CanAct()` but not passed out (i.e., AP or FAT insufficient)
- Update tooltip to show cost when dimmed

### Pitfall 3: Unload Visibility Condition
**What goes wrong:** Showing Unload button based on `hasRangedWeaponEquipped` instead of `HasWeaponWithAmmo()`.
**Why it happens:** Confusion between "has ranged weapon" (Reload's condition) and "has weapon with ammo loaded" (Unload's condition).
**How to avoid:** CONTEXT.md is explicit: Unload visible "when ANY equipped weapon has ammo loaded." The existing `HasWeaponWithAmmo()` method (TabCombat line 859-862) already implements this: `rangedWeapons.Any(w => w.LoadedAmmo > 0)`.
**Warning signs:** Unload button visible when weapons are equipped but have no ammo loaded.

### Pitfall 4: Rest Logic Not Ending Parry Mode
**What goes wrong:** Moving Rest logic to a confirmation flow but forgetting to keep the parry mode end behavior.
**Why it happens:** When refactoring `Rest()` into `StartRest()` + `ConfirmRest()`, the parry mode end call might be left in `StartRest()` (before confirmation) instead of `ConfirmRest()` (after confirmation).
**How to avoid:** Parry mode should only end in `ConfirmRest()` (when the rest actually happens), not in `StartRest()` (when the panel opens). If the player cancels, parry mode should not have been ended.

### Pitfall 5: Forgetting to Add Rest to CombatMode Enum
**What goes wrong:** The Rest confirmation panel doesn't render because `CombatMode.Rest` was never added to the `CombatMode` enum.
**Why it happens:** The enum is a private type inside TabCombat's `@code` block (line 457).
**How to avoid:** Add `Rest` to the `CombatMode` enum: `Default, Attack, Defend, TakeDamage, RangedAttack, Reload, Unload, Medical, SelectTarget, ActivateImplant, AnonymousAction, SkillCheck, Rest`

### Pitfall 6: CSS Transition for Button Appear/Disappear Not Working in Blazor
**What goes wrong:** CSS transitions don't animate `@if`-rendered elements because Blazor adds/removes DOM elements directly, bypassing CSS transition triggers.
**Why it happens:** CSS transitions work on property changes of existing elements, not on elements being added/removed from the DOM.
**How to avoid:** CONTEXT.md mentions "subtle CSS transition (fade/slide) when appearing/disappearing" but also says "buttons appear/disappear immediately via Blazor reactivity." These conflict. The pragmatic approach: use CSS transitions on opacity/max-height by toggling a visibility class instead of using `@if`, OR accept that `@if` gives instant appear/disappear (which CONTEXT.md also says is acceptable). Recommendation: use the simple `@if` approach for instant visibility changes -- "immediately via Blazor reactivity" takes priority.

## Code Examples

### Example 1: Current Other Group HTML (TabCombat.razor lines 245-289)
```html
<!-- Other Group -->
<div class="card combat-group-other mb-3">
    <div class="card-header">
        <i class="bi bi-gear"></i> <strong>Other</strong>
    </div>
    <div class="card-body">
        <div class="d-flex flex-wrap gap-2">
            <button class="btn combat-tile combat-tile-other"
                    @onclick="StartMedical"
                    disabled="@(!CanAct())"
                    title="First-Aid, Nursing, Doctor">
                <i class="bi bi-heart-pulse"></i>
                <span>Medical</span>
            </button>
            <button class="btn combat-tile combat-tile-other"
                    @onclick="Rest"
                    disabled="@(!CanRest())"
                    title="Spend 1 AP to recover 1 FAT">
                <i class="bi bi-cup-hot"></i>
                <span>Rest</span>
            </button>
            <button class="btn combat-tile combat-tile-other"
                    @onclick="StartActivateImplant"
                    disabled="@(!hasToggleableImplantsEquipped)"
                    title="Activate / Deactivate implants">
                <i class="bi bi-cpu"></i>
                <span>Implants</span>
            </button>
            <button class="btn combat-tile combat-tile-other"
                    @onclick="StartReload"
                    disabled="@(!CanAct() || !hasRangedWeaponEquipped)"
                    title="Reload a ranged weapon (1 AP + 1 FAT)">
                <i class="bi bi-box-arrow-in-down"></i>
                <span>Reload</span>
            </button>
            <button class="btn combat-tile combat-tile-other"
                    @onclick="StartUnload"
                    disabled="@(!CanAct() || !HasWeaponWithAmmo())"
                    title="Remove ammo from weapon">
                <i class="bi bi-box-arrow-up"></i>
                <span>Unload</span>
            </button>
        </div>
    </div>
</div>
```

### Example 2: Target Other Group HTML (After Phase 36)
```html
<!-- Other Group -->
<div class="card combat-group-other mb-3">
    <div class="card-header">
        <i class="bi bi-gear"></i> <strong>Other</strong>
    </div>
    <div class="card-body">
        <div class="d-flex flex-wrap gap-2">
            <!-- Medical: always visible -->
            <button class="btn combat-tile combat-tile-other @(!CanAct() ? "combat-tile-dimmed" : "")"
                    @onclick="StartMedical"
                    disabled="@(Character?.IsPassedOut ?? true)"
                    title="@(CanAct() ? "First-Aid, Nursing, Doctor (1 AP + 1 FAT)" : "Medical requires 1 AP + 1 FAT")">
                <i class="bi bi-heart-pulse"></i>
                <span>Medical</span>
            </button>
            <!-- Rest: always visible -->
            <button class="btn combat-tile combat-tile-other @(!CanRest() ? "combat-tile-dimmed" : "")"
                    @onclick="StartRest"
                    disabled="@(Character?.IsPassedOut ?? true)"
                    title="@(CanRest() ? "Spend 1 AP to recover 1 FAT" : "Rest requires 1 AP")">
                <i class="bi bi-cup-hot"></i>
                <span>Rest</span>
            </button>
            <!-- Implants: hidden when no toggleable implants -->
            @if (hasToggleableImplantsEquipped)
            {
                <button class="btn combat-tile combat-tile-other"
                        @onclick="StartActivateImplant"
                        title="Activate / Deactivate implants">
                    <i class="bi bi-cpu"></i>
                    <span>Implants</span>
                </button>
            }
            <!-- Reload: hidden when no ranged weapon -->
            @if (hasRangedWeaponEquipped)
            {
                <button class="btn combat-tile combat-tile-other @(!CanAct() ? "combat-tile-dimmed" : "")"
                        @onclick="StartReload"
                        disabled="@(Character?.IsPassedOut ?? true)"
                        title="@(CanAct() ? "Reload a ranged weapon (1 AP + 1 FAT)" : "Reload requires 1 AP + 1 FAT")">
                    <i class="bi bi-box-arrow-in-down"></i>
                    <span>Reload</span>
                </button>
            }
            <!-- Unload: hidden when no weapon with ammo -->
            @if (HasWeaponWithAmmo())
            {
                <button class="btn combat-tile combat-tile-other @(!CanAct() ? "combat-tile-dimmed" : "")"
                        @onclick="StartUnload"
                        disabled="@(Character?.IsPassedOut ?? true)"
                        title="@(CanAct() ? "Remove ammo from weapon (1 AP + 1 FAT)" : "Unload requires 1 AP + 1 FAT")">
                    <i class="bi bi-box-arrow-up"></i>
                    <span>Unload</span>
                </button>
            }
        </div>
    </div>
</div>
```

### Example 3: Current Rest Method (TabCombat.razor lines 1268-1295)
```csharp
private async Task Rest()
{
    if (!CanRest() || Character == null) return;

    // Resting ends parry mode
    if (CombatStanceBehavior.IsInParryMode(Character))
    {
        CombatStanceBehavior.EndParryMode(Character);
    }

    Character.ActionPoints.Available -= 1;
    Character.Fatigue.PendingHealing += 1;

    LogActivity($"{Character.Name} rests, recovering some fatigue.", ActivityCategory.Health);

    if (Character is Csla.Core.ISavable savable)
    {
        await savable.SaveAsync();
    }

    if (OnCharacterChanged.HasDelegate)
    {
        await OnCharacterChanged.InvokeAsync();
    }

    StateHasChanged();
}
```

### Example 4: Rest Confirmation Flow (New)
```csharp
private void StartRest()
{
    combatMode = CombatMode.Rest;
}

private async Task ConfirmRest()
{
    if (!CanRest() || Character == null) return;

    // Resting ends parry mode (moved from old Rest())
    if (CombatStanceBehavior.IsInParryMode(Character))
    {
        CombatStanceBehavior.EndParryMode(Character);
    }

    Character.ActionPoints.Available -= 1;
    Character.Fatigue.PendingHealing += 1;

    LogActivity($"{Character.Name} rests, recovering some fatigue.", ActivityCategory.Health);

    if (Character is Csla.Core.ISavable savable)
    {
        await savable.SaveAsync();
    }

    if (OnCharacterChanged.HasDelegate)
    {
        await OnCharacterChanged.InvokeAsync();
    }

    combatMode = CombatMode.Default;
    StateHasChanged();
}
```

### Example 5: Dimming CSS Class
```css
/* Cost-aware dimming for combat tiles (AP/FAT insufficient) */
.combat-tile-dimmed {
    opacity: 0.65;
}
```
Note: This is essentially the same as `combat-tile-passive-only` but named generically. The planner should decide whether to rename the existing class, create a new one, or alias them.

### Example 6: Dimming Applied to Actions Group (for consistency)
```html
<button class="btn combat-tile combat-tile-actions @(!CanAct() ? "combat-tile-dimmed" : "")"
        @onclick="StartAttack"
        disabled="@(Character?.IsPassedOut ?? true)"
        title="@(CanAct() ? "Melee attack (1 AP + 1 FAT)" : "Attack requires 1 AP + 1 FAT")">
    <i class="bi bi-bullseye"></i>
    <span>Attack</span>
</button>
```

## Existing Files Inventory

### Files to MODIFY

| File | What Changes |
|------|-------------|
| `Threa/Threa.Client/Components/Pages/GamePlay/TabCombat.razor` | (1) Change Implants/Reload/Unload from disabled to `@if` hidden. (2) Add `CombatMode.Rest` enum value. (3) Add Rest confirmation panel HTML. (4) Refactor `Rest()` into `StartRest()` + `ConfirmRest()`. (5) Apply dimming class to all three groups' buttons. (6) Update tooltips on dimmed buttons to show costs. |
| `Threa/Threa/wwwroot/css/themes.css` | Add `.combat-tile-dimmed` CSS class (or rename `combat-tile-passive-only` to be more generic). |

### Files to READ (reference only)

| File | What It Tells Us |
|------|-----------------|
| `Threa/Threa.Client/Components/Pages/GamePlay/MedicalMode.razor` | Medical treatment flow (DO NOT MODIFY -- already wired) |
| `Threa/Threa.Client/Components/Pages/GamePlay/ReloadMode.razor` | Reload flow (DO NOT MODIFY -- already wired) |
| `Threa/Threa.Client/Components/Pages/GamePlay/UnloadMode.razor` | Unload flow (DO NOT MODIFY -- already wired) |
| `Threa/Threa.Client/Components/Pages/GamePlay/ActivateImplantMode.razor` | Implant activation flow (DO NOT MODIFY -- already wired) |

### Files to IGNORE

| File | Why |
|------|-----|
| `Threa/Threa.Client/Components/Pages/GamePlay/TabDefense.razor` | Legacy prototype, superseded by Combat tab |

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Disabled buttons for conditional actions | `@if` hidden buttons (Phase 36) | Phase 36 | Cleaner UI -- no greyed-out buttons for equipment not equipped |
| Instant Rest execution | Rest confirmation panel | Phase 36 | Prevents accidental AP spend; matches combat mode pattern |
| No cost feedback on dimmed tiles | Cost-aware dimming with tooltips | Phase 36 | Players understand WHY a button is dimmed |
| Dimming only on Defend button | Dimming across all groups | Phase 36 | Consistent visual language for "insufficient resources" |

**Deprecated/outdated:**
- `TabDefense.razor`: Legacy prototype. All defense/other actions live in TabCombat.razor.
- The design doc's Rest rules (variable AP, locks remaining AP for round) differ from the current implementation (1 AP for 1 FAT). The current implementation is what the code does and what CONTEXT.md specifies. The design doc may be aspirational for a future version.

## Open Questions

1. **Dimming class naming: `combat-tile-dimmed` vs reusing `combat-tile-passive-only`**
   - What we know: `combat-tile-passive-only` already exists with `opacity: 0.65` and is currently only used on the Defend button.
   - What's unclear: Should we create a new `combat-tile-dimmed` class (semantically clearer for general use) or reuse the existing class?
   - Recommendation: Create `combat-tile-dimmed` for the general case. Keep `combat-tile-passive-only` as an alias or refactor the Defend button to use `combat-tile-dimmed` too. This way the class name describes the intent ("dimmed due to cost") rather than the specific case ("passive only for defense").

2. **CSS transitions for button appear/disappear vs instant Blazor reactivity**
   - What we know: CONTEXT.md says both "subtle CSS transition (fade/slide)" AND "buttons appear/disappear immediately via Blazor reactivity."
   - What's unclear: Which takes priority? CSS transitions don't work on `@if`-rendered elements because Blazor adds/removes DOM nodes.
   - Recommendation: Use `@if` for instant visibility. CSS transitions on `@if` elements are not possible without JavaScript interop or a Blazor transition library, which is out of scope. If transitions are truly needed later, a CSS class toggle approach (always render, hide with class) could be used, but this contradicts the "hidden entirely" decision.

3. **Rest button dimming condition differs from Medical/Reload/Unload**
   - What we know: Medical, Reload, Unload use `CanAct()` (1 AP + 1 FAT). Rest uses `CanRest()` (1 AP, FAT not full). Rest doesn't require FAT expenditure -- it recovers FAT.
   - What's unclear: Should Rest dim when `!CanRest()` or when `AP < 1`?
   - Recommendation: Rest dims when AP < 1, not when FAT is full. When FAT is at max, Rest should be disabled (not just dimmed) because you cannot rest when fully recovered. The dimming condition for Rest is: `AP >= 1 but FAT is at max` = disabled, `AP < 1 and not passed out` = dimmed, `passed out` = disabled.
   - Simplification: Rest uses `!CanRest()` for dimming and `IsPassedOut` for disabled. If `!CanRest()` is true because FAT is full, the button appears dimmed with a tooltip "Fatigue is already at maximum." If because AP < 1, tooltip says "Rest requires 1 AP."

## Sources

### Primary (HIGH confidence)
- Codebase analysis: `TabCombat.razor` (1545 lines) -- full combat tab with all modes, Other group at lines 245-289, Rest method at lines 1268-1295, existing condition checks at lines 1305-1313
- Codebase analysis: `MedicalMode.razor` -- complete medical treatment workflow
- Codebase analysis: `ReloadMode.razor` -- complete reload workflow with ActionCostSelector
- Codebase analysis: `UnloadMode.razor` -- complete unload workflow with ActionCostSelector
- Codebase analysis: `ActivateImplantMode.razor` (194 lines) -- complete implant toggle workflow
- Codebase analysis: `themes.css` -- combat tile CSS (lines 2066-2191), `.combat-tile-passive-only` (line 2189)
- Phase context: `36-CONTEXT.md` -- all user decisions for this phase
- Phase 35 research: `35-RESEARCH.md` -- established patterns for combat tile dimming and stance chips

### Secondary (MEDIUM confidence)
- Design docs: `ACTION_POINTS.md` -- Rest action rules (note: design doc describes more complex Rest than what's implemented)
- Phase 32 context: `32-CONTEXT.md` -- original layout decisions including "conditionally-available buttons shown as disabled/grayed"

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH -- all libraries already in use, no new dependencies needed
- Architecture: HIGH -- all patterns established in codebase (mode pattern, dimming, `@if` visibility, combat tiles)
- Pitfalls: HIGH -- identified from direct codebase analysis, specific line numbers cited

**Research date:** 2026-02-13
**Valid until:** 2026-03-15 (stable internal codebase, no external dependency changes)
