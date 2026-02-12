# Phase 32: Layout Restructuring - Research

**Researched:** 2026-02-11
**Domain:** Blazor Razor UI layout refactoring (Play page tab restructuring)
**Confidence:** HIGH

## Summary

Phase 32 restructures the Play page's combat UI from a two-tab layout (Combat + Defense) to a single Combat tab with three button groups (Actions, Defense, Other) plus a left detail panel. This is purely a UI/layout change within existing Blazor Razor components -- no business logic, DAL, or game mechanics changes are required.

The current codebase provides a complete understanding of the change surface. The Combat tab (`TabCombat.razor`, ~1290 lines) currently has a two-column layout: combat skills list (col-md-7) with large action buttons (col-md-5). The Defense tab (`TabDefense.razor`, ~245 lines) contains defensive stances, armor summary, and incoming attack response. Both need to merge into a single restructured tab while preserving the sub-mode navigation (Attack, Defend, RangedAttack, Reload, Unload, Medical, ActivateImplant, SelectTarget, TakeDamage).

**Primary recommendation:** Restructure `TabCombat.razor`'s default mode markup from a skills+buttons two-column layout to a left-panel + three-group-cards layout, remove skills list, add activity feed, then remove the Defense tab from `Play.razor`'s tab array and delete `TabDefense.razor`.

## Standard Stack

### Core

| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Blazor Web App | .NET 10 | Component framework | Already in use |
| Bootstrap 5 | min.css (CDN) | Grid, spacing, responsive | Already in use |
| Bootstrap Icons | 1.11.3 (CDN) | `bi-*` icon classes | Already in use for all icons |
| Radzen.Blazor | 8.4.2 | UI components (Dialog, CheckBox, etc.) | Already in use |

### Supporting

| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| CSS Custom Properties | themes.css | Theme-aware styling | All new colors/borders must use theme variables |

### Alternatives Considered

| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Bootstrap Icons | Material Icons (Google Fonts) | Context says "Radzen's built-in icon set (Material Icons)" but codebase exclusively uses Bootstrap Icons (`bi-*`). Radzen's material-base.css provides Material icon styling but the actual Material Symbols font is NOT loaded. Adding it would be an unnecessary dependency. **Use Bootstrap Icons to stay consistent.** |
| Custom CSS | Radzen layout components | Radzen has panel/card components but the codebase uses raw Bootstrap grid + HTML cards everywhere. Consistency favors Bootstrap. |

**Installation:** No new packages needed. All tools are already in place.

## Architecture Patterns

### Current File Structure (Relevant Files)

```
Threa.Client/Components/Pages/GamePlay/
├── Play.razor              # Parent page - tab navigation + character header + activity log
├── TabCombat.razor         # Combat tab - skills list + action buttons + sub-modes
├── TabDefense.razor        # Defense tab - stances + armor + incoming attacks (TO BE REMOVED)
├── TabStatus.razor         # Status tab (untouched)
├── TabPlaySkills.razor     # Skills tab (untouched)
├── TabPlayMagic.razor      # Magic tab (untouched)
├── TabPlayInventory.razor  # Inventory tab (untouched)
├── AttackMode.razor        # Sub-mode (untouched)
├── DefendMode.razor        # Sub-mode (untouched)
├── DamageResolution.razor  # Sub-mode (untouched)
├── RangedAttackMode.razor  # Sub-mode (untouched)
├── ReloadMode.razor        # Sub-mode (untouched)
├── UnloadMode.razor        # Sub-mode (untouched)
├── MedicalMode.razor       # Sub-mode (untouched)
├── ActivateImplantMode.razor # Sub-mode (untouched)
└── DefenseHitData.cs       # DTO (untouched)

Threa/wwwroot/css/
└── themes.css              # Theme variables and styled components
```

### Pattern 1: Sub-Mode Navigation (CombatMode enum)

**What:** TabCombat uses an enum `CombatMode` to switch between different UI states. The Default mode shows the main buttons; clicking a button switches to a specific mode (Attack, Defend, etc.).

**When to use:** This pattern is preserved. Phase 32 only changes what the `CombatMode.Default` mode renders -- not the sub-mode components themselves.

**Current enum values:**
```csharp
private enum CombatMode { Default, Attack, Defend, TakeDamage, RangedAttack, Reload, Unload, Medical, SelectTarget, ActivateImplant }
```

### Pattern 2: Tab Navigation Array in Play.razor

**What:** Play.razor defines tabs with `AllTabNames` string array and filters by theme.

**Current:**
```csharp
private static readonly string[] AllTabNames = new[] { "Status", "Combat", "Defense", "Skills", "Magic", "Inventory" };
```

**Change needed:** Remove "Defense" from this array.

### Pattern 3: Card Container Pattern

**What:** The app uses Bootstrap cards extensively for grouped content. Card header uses theme variables via `.card-header` CSS which is already styled for both fantasy and sci-fi themes.

**Example from TabStatus.razor:**
```html
<div class="card mb-3">
    <div class="card-header">
        <strong><i class="bi bi-bar-chart"></i> Attributes</strong>
    </div>
    <div class="card-body">
        <!-- content -->
    </div>
</div>
```

This is the exact pattern to use for the three button groups (Actions, Defense, Other).

### Pattern 4: Responsive Column Layout

**What:** Bootstrap grid with `col-md-*` classes for responsive left/right panels. Already used in TabStatus (col-md-4 | col-md-4 | col-md-4) and TabCombat (col-md-7 | col-md-5).

**New layout:** `col-lg-3` for left panel + `col-lg-9` for button groups, collapsing to stacked on smaller screens.

### Pattern 5: Activity Log (Already Exists in Play.razor)

**What:** Play.razor has an activity log section at the bottom (lines 313-329) that shows the last 10 entries. The context says to "surface existing activity feed on the Combat tab in freed space."

**Current location:** Below all tab content in Play.razor. Can be moved into the Combat tab's default mode markup.

### Anti-Patterns to Avoid

- **Do not duplicate the activity log:** Move the rendering into TabCombat rather than copying. The data source (`activityLog` list) stays in Play.razor. Pass it as a parameter.
- **Do not hide buttons with `display:none`:** Context specifies disabled/grayed buttons for conditionally-available actions, not hidden ones. This prevents layout shifts.
- **Do not break sub-mode navigation:** The CombatMode enum and all sub-mode components must continue to work identically. Only the Default mode's markup changes.
- **Do not change card-header styling patterns:** Use existing `.card-header` CSS which already handles both themes. Adding custom card-header backgrounds would break theme consistency.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Responsive panel collapse | Custom media queries | Bootstrap `col-lg-3` / `col-lg-9` grid | Already works, well-tested |
| Card containers with theme styling | Custom styled divs | Bootstrap `.card` + existing theme CSS | Cards already themed for both fantasy/sci-fi |
| Disabled button styling | Custom opacity/color logic | Bootstrap `disabled` attribute | Already themed per sci-fi/fantasy |
| Icon rendering | Custom icon components | Bootstrap Icons `<i class="bi bi-*">` | Consistent with entire codebase |

**Key insight:** This phase is 100% reshuffling existing markup patterns into a new layout. Every building block (cards, buttons, grids, icons, theme variables) already exists and is themed.

## Common Pitfalls

### Pitfall 1: Breaking Sub-Mode Component Parameters

**What goes wrong:** Changing TabCombat's layout accidentally removes or renames parameters that sub-mode components (AttackMode, DefendMode, etc.) depend on.
**Why it happens:** The `@code` block has extensive state and parameters that feed into sub-modes.
**How to avoid:** The @code block stays identical. Only the `CombatMode.Default` HTML block is restructured. Sub-mode rendering (`else if` branches) is untouched.
**Warning signs:** Compile errors in TabCombat referencing missing fields.

### Pitfall 2: Forgetting Theme Compatibility

**What goes wrong:** New UI elements look fine in fantasy theme but break in sci-fi (or vice versa).
**Why it happens:** Hard-coded colors (`bg-danger`, `text-white`) instead of theme variables.
**How to avoid:** Use theme CSS variables for all new colors. Use `.card` and `.card-header` classes which are already themed. The group accent colors should use custom CSS classes that reference theme variables.
**Warning signs:** Light text on light background in fantasy theme; dark text on dark background in sci-fi theme.

### Pitfall 3: Activity Log Data Flow

**What goes wrong:** Moving the activity log rendering into TabCombat but the data (`activityLog` list, `GetActivityClass` method) lives in Play.razor.
**Why it happens:** The activity log is currently rendered directly in Play.razor's code block.
**How to avoid:** Pass activity log entries as a parameter to TabCombat, or render the activity log conditionally in Play.razor only when the Combat tab is active.
**Warning signs:** Null reference on activity log data in TabCombat.

### Pitfall 4: Resource Summary Duplication

**What goes wrong:** Adding AP/FAT/VIT to the Combat tab when it already exists in the page header.
**Why it happens:** Context explicitly says "No AP/FAT/VIT summary duplication (already in page header across all tabs)."
**How to avoid:** The resource summary card currently in TabCombat (lines 225-248) should be REMOVED, not moved. The header already shows AP/FAT/VIT.
**Warning signs:** Same numbers showing twice on screen.

### Pitfall 5: Layout Shift When Buttons Become Available

**What goes wrong:** Buttons appearing/disappearing (e.g., Ranged Attack only when ranged weapon equipped) cause the layout to jump.
**Why it happens:** Using `@if (condition)` to show/hide buttons.
**How to avoid:** Show ALL buttons always, but disable unavailable ones. Context explicitly states: "Conditionally-available buttons shown as disabled/grayed rather than hidden (consistent layout)."
**Warning signs:** Buttons jumping around when equipment changes.

## Code Examples

### New Default Mode Layout Structure

```razor
@* TabCombat.razor - Default mode only (replaces current Default block) *@
@if (combatMode == CombatMode.Default)
{
    <div class="row">
        <!-- Left Panel: Combat readiness at-a-glance -->
        <div class="col-lg-3 mb-3">
            <!-- FAT/VIT Detail Card -->
            <div class="card mb-3">
                <div class="card-header">
                    <strong><i class="bi bi-heart-pulse"></i> Health Pools</strong>
                </div>
                <div class="card-body">
                    @* FAT/VIT breakdown with base values, modifiers, wounds *@
                </div>
            </div>

            <!-- Armor Info Card -->
            <div class="card">
                <div class="card-header">
                    <strong><i class="bi bi-shield"></i> Armor</strong>
                </div>
                <div class="card-body">
                    @* Armor class + durability per equipped piece *@
                </div>
            </div>
        </div>

        <!-- Right Panel: Button Groups -->
        <div class="col-lg-9">
            <!-- Actions Group -->
            <div class="card mb-3 combat-group-actions">
                <div class="card-header">
                    <strong><i class="bi bi-lightning-charge"></i> Actions</strong>
                </div>
                <div class="card-body">
                    <div class="d-flex flex-wrap gap-2">
                        @* Compact tile buttons: Attack, Ranged, Use Skill, Anonymous Action *@
                    </div>
                </div>
            </div>

            <!-- Defense Group -->
            <div class="card mb-3 combat-group-defense">
                <div class="card-header">
                    <strong><i class="bi bi-shield-shaded"></i> Defense</strong>
                </div>
                <div class="card-body">
                    <div class="d-flex flex-wrap gap-2">
                        @* Compact tile buttons: Defend, Take Damage, Stances *@
                    </div>
                </div>
            </div>

            <!-- Other Group -->
            <div class="card mb-3 combat-group-other">
                <div class="card-header">
                    <strong><i class="bi bi-gear"></i> Other</strong>
                </div>
                <div class="card-body">
                    <div class="d-flex flex-wrap gap-2">
                        @* Compact tile buttons: Medical, Rest, Implants, Reload, Unload *@
                    </div>
                </div>
            </div>

            <!-- Activity Feed -->
            <div class="activity-log activity-log-container">
                @* Activity log entries passed from Play.razor *@
            </div>
        </div>
    </div>
}
```

### Compact Tile Button Pattern

```razor
@* Single compact action button - icon above label *@
<button class="btn combat-tile combat-tile-actions"
        @onclick="StartAttack"
        disabled="@(!CanAct())"
        title="Melee attack (1 AP + 1 FAT)">
    <i class="bi bi-bullseye"></i>
    <span>Attack</span>
</button>
```

### Compact Tile CSS (Add to themes.css)

```css
/* Compact combat tile buttons */
.combat-tile {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    min-width: 80px;
    padding: 0.5rem 0.75rem;
    gap: 0.25rem;
    font-size: 0.8rem;
    text-transform: uppercase;
    letter-spacing: 0.05em;
    font-family: var(--font-display);
    border: 1px solid var(--color-card-border);
    background: var(--color-card-bg);
    color: var(--color-text-primary);
    border-radius: var(--border-radius);
    transition: all 0.2s ease;
}

.combat-tile i {
    font-size: 1.2rem;
}

.combat-tile:disabled {
    opacity: 0.4;
    cursor: not-allowed;
}

/* Group accent colors */
.combat-tile-actions { border-color: var(--color-accent-red); }
.combat-tile-actions:not(:disabled):hover { background: var(--color-accent-red); color: #fff; }

.combat-tile-defense { border-color: var(--color-accent-blue); }
.combat-tile-defense:not(:disabled):hover { background: var(--color-accent-blue); color: #fff; }

.combat-tile-other { border-color: var(--color-accent-secondary); }
.combat-tile-other:not(:disabled):hover { background: var(--color-accent-secondary); color: #fff; }

/* Sci-fi theme enhancements */
[data-theme="scifi"] .combat-tile-actions { box-shadow: 0 0 5px var(--color-accent-red); }
[data-theme="scifi"] .combat-tile-defense { box-shadow: 0 0 5px var(--color-accent-blue); }
[data-theme="scifi"] .combat-tile-other { box-shadow: 0 0 5px var(--color-accent-secondary); }
```

### Tab Array Change in Play.razor

```csharp
// Before:
private static readonly string[] AllTabNames = new[] { "Status", "Combat", "Defense", "Skills", "Magic", "Inventory" };

// After:
private static readonly string[] AllTabNames = new[] { "Status", "Combat", "Skills", "Magic", "Inventory" };
```

### Activity Feed Parameter Pattern

```razor
@* In Play.razor - pass activity log to TabCombat *@
<TabCombat Character="character"
           Table="table"
           OnCharacterChanged="SaveCharacterAsync"
           AvailableTargets="@GetAvailableTargets()"
           OnInitiateTargeting="HandleInitiateTargeting"
           ActivityEntries="@activityLog"
           GetActivityCssClass="GetActivityClass" />

@* In TabCombat.razor - new parameters *@
[Parameter]
public List<ActivityLogEntry>? ActivityEntries { get; set; }

[Parameter]
public Func<ActivityCategory, string>? GetActivityCssClass { get; set; }
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Two separate tabs (Combat + Defense) | Single Combat tab with three groups | Phase 32 (this phase) | Fewer tab switches during combat |
| Large buttons with verbose labels | Compact icon + label tiles | Phase 32 (this phase) | More actions visible without scrolling |
| Skills list on Combat tab | Skills on dedicated Skills tab only | Phase 32 (this phase) | Cleaner Combat tab layout |

**Deprecated/outdated:**
- `TabDefense.razor`: Entirely removed. Content migrated to the Defense group + left panel.
- Resource Summary card in TabCombat: Removed (already in page header).
- Combat skills list in TabCombat: Removed from Combat tab (remains on Skills tab).

## Open Questions

1. **Activity log rendering location**
   - What we know: Activity log currently renders in Play.razor after all tab content. Context says to surface it on the Combat tab.
   - What's unclear: Whether to move the render inside TabCombat (requires parameter passing) or conditionally show it in Play.razor only when Combat tab is active.
   - Recommendation: Pass as parameters to TabCombat for cleaner encapsulation. The data stays in Play.razor; the rendering moves.

2. **Armor info for left panel**
   - What we know: Context specifies "armor class + current durability per equipped armor piece" in the left panel. TabDefense.razor had a placeholder for this (empty list, TODOs).
   - What's unclear: TabDefense's armor data was all placeholder/TODO. Equipment data IS available from `equippedItems` in TabCombat's code.
   - Recommendation: Implement basic armor display using equipped items data already loaded in TabCombat. Show slot, name, and durability if available from item properties.

3. **FAT/VIT detail breakdown for left panel**
   - What we know: Context specifies "base values, active modifiers, wounds affecting pools."
   - What's unclear: Character object has `.Fatigue.Value` and `.Fatigue.BaseValue`, but modifier breakdown isn't directly exposed in the same way attributes are (via `GetAttributeBreakdown`).
   - Recommendation: Show base value and current value. For wounds, count wound effects and show their AS penalty. Full modifier breakdown can be enhanced in later phases if needed.

4. **Bootstrap Icons vs Material Icons**
   - What we know: Context says "Use Radzen's built-in icon set (Material Icons)" but the entire codebase uses Bootstrap Icons (`bi-*`). No Material Icons/Symbols font is loaded.
   - What's unclear: Whether the user specifically wants Material Icons added, or meant "use the icon set that's already there."
   - Recommendation: Use Bootstrap Icons for consistency. The full Material Symbols font would need to be added as a new dependency and would be the only place it's used. Bootstrap Icons has equivalent icons for all needed actions (sword, shield, crosshair, heart, gear, etc.).

## Sources

### Primary (HIGH confidence)

- **Codebase inspection** - Direct reading of all relevant files:
  - `Play.razor` (1650 lines) - tab navigation, activity log, character header
  - `TabCombat.razor` (1289 lines) - combat skills, action buttons, sub-mode routing
  - `TabDefense.razor` (245 lines) - defensive stances, armor, incoming attacks
  - `TabStatus.razor` (818 lines) - health pools, effects, wounds display
  - `themes.css` (2060 lines) - theme variables, component styles for both themes
  - `app.css` (157 lines) - base styles and utility classes
  - `App.razor` - CSS/font dependencies (Bootstrap Icons CDN, Radzen material-base.css)
- **Context document** - `.planning/phases/32-layout-restructuring/32-CONTEXT.md` (locked decisions)
- **Roadmap** - `.planning/ROADMAP.md` (phase dependencies and success criteria)
- **Requirements** - `.planning/REQUIREMENTS.md` (LAY-01 through LAY-05)

### Secondary (MEDIUM confidence)

- **Bootstrap 5 documentation** - Grid system, card component, responsive breakpoints (verified via existing codebase usage patterns)
- **Bootstrap Icons library** - Icon availability for combat actions (verified: bi-bullseye, bi-shield-shaded, bi-crosshair, bi-heart-pulse, bi-gear, bi-cup-hot, bi-cpu, bi-box-arrow-in-down, bi-box-arrow-up all exist in current usage)

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All libraries already in use, no new dependencies
- Architecture: HIGH - Direct codebase inspection, all patterns well-established
- Pitfalls: HIGH - Based on actual code structure and theme system complexity
- Code examples: HIGH - Derived from existing patterns in the codebase

**Research date:** 2026-02-11
**Valid until:** Indefinite (no external dependencies to go stale)
