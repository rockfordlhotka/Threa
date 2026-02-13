# Phase 35: Defense Group - Research

**Researched:** 2026-02-13
**Domain:** Blazor Combat Tab UI - Defense actions, damage resolution, and defensive stances
**Confidence:** HIGH

## Summary

Phase 35 wires three capabilities into the Defense group on the Combat tab: Defend (entering DefendMode), Take Damage (entering TakeDamageMode/DamageResolution), and Defensive Stances (Normal, Parry Mode, Dodge Focus, Block with Shield). All three capabilities **already exist** in the codebase as functional components and game logic. This phase is primarily a UI integration task: adding stance toggles to the Defense group card in default mode, and ensuring the existing Defend and Take Damage buttons wire through to the existing mode components with stance-awareness.

The existing `DefendMode.razor` already handles all four defense types (passive, dodge, parry, shield block) with full AP/boost/roll flows. The existing `DamageResolution.razor` handles SV-based damage input with hit location, armor, and shield block integration. The existing `CombatStanceBehavior.cs` manages Parry Mode via the Effects system. The new stances (Dodge Focus, Block with Shield, Normal) need to either extend `CombatStanceBehavior` or use a simpler in-memory state since some are just preference hints rather than game-mechanical effects.

**Primary recommendation:** Add stance toggle chips to the Defense group card in `TabCombat.razor` default mode. Store active stance in component state. Pass the stance to `DefendMode` as a new parameter so it can pre-select the matching defense type. Extend `CombatStanceBehavior` to handle Dodge Focus and Block with Shield stances similarly to Parry Mode.

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Blazor (InteractiveServer) | .NET 10 | UI rendering | Already in use for all Combat tab modes |
| Bootstrap 5 | Current | Layout, badges/chips | Already in use for combat tile styling |
| Bootstrap Icons | Current | Icons for stances/tiles | Already in use throughout combat UI |
| CSLA .NET | 9.1.0 | Effect management for CombatStance type | Already used for Parry Mode effects |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Radzen.Blazor | 8.4.2 | DialogService for modals | Already injected, may be used for confirmations |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| CombatStance effects for all stances | Simple component state (string field) | Simpler but won't persist across page reloads; effects persist but add complexity for stances that don't mechanically change anything (Normal) |

## Architecture Patterns

### Existing Component Structure (Combat Tab Modes)
```
TabCombat.razor
  combatMode == Default  ->  Defense group card (Defend + Take Damage tiles + Stance toggles)
  combatMode == Defend   ->  <DefendMode ... />
  combatMode == TakeDamage -> <DamageResolution ... />
```

### Pattern 1: Inline Mode Panel (Combat Mode Pattern)
**What:** Clicking a button in the default view sets `combatMode` to a specific enum value, replacing the entire combat tab content with a dedicated mode component.
**When to use:** For Defend and Take Damage actions (already implemented).
**Example (from TabCombat.razor):**
```csharp
// StartDefend already exists:
private void StartDefend()
{
    combatMode = CombatMode.Defend;
}

// DefendMode is already wired:
else if (combatMode == CombatMode.Defend)
{
    <DefendMode Character="Character"
                Table="Table"
                HasWeaponEquipped="hasWeaponEquipped"
                HasShieldEquipped="hasShieldEquipped"
                WeaponSkillAS="weaponSkillAS"
                WeaponSkillName="weaponSkillName"
                ShieldSkillAS="shieldSkillAS"
                OnCancel="ReturnToDefault"
                OnDefenseComplete="OnDefenseComplete"
                OnHit="OnDefenseHit" />
}
```

### Pattern 2: Stance Toggle Chips (New for Phase 35)
**What:** Always-visible small toggle buttons in the Defense group card that set a defensive stance without entering a mode.
**When to use:** For stance selection (Normal, Parry Mode, Dodge Focus, Block with Shield).
**Design decision from CONTEXT.md:** "Stances use a different visual style from the standard compact tiles -- smaller toggles/chips to differentiate from action buttons."
**Example approach:**
```html
<!-- Inside Defense group card-body, after the action tiles -->
<hr class="my-2" />
<div class="d-flex flex-wrap gap-1">
    <button class="btn btn-sm stance-chip @(activeStance == "normal" ? "active" : "")"
            @onclick="() => SetStance("normal")"
            title="No special defensive focus">
        Normal
    </button>
    <button class="btn btn-sm stance-chip @(activeStance == "parry" ? "active" : "")"
            @onclick="() => ActivateParryMode()"
            disabled="@(!hasWeaponEquipped)"
            title="@(!hasWeaponEquipped ? "Requires weapon equipped" : "Free parry defenses (costs 1 AP + 1 FAT to enter)")">
        Parry Mode
    </button>
    <!-- etc. -->
</div>
```

### Pattern 3: CombatStance Effect for Persistent Stances
**What:** Game-mechanical stances like Parry Mode are stored as `EffectType.CombatStance` effects via `CombatStanceBehavior`, which persists them across save/load and integrates with the effect system.
**When to use:** For stances that have game-mechanical consequences (Parry Mode already does this).
**Already implemented in `CombatStanceBehavior.cs`:**
```csharp
public static bool IsInParryMode(CharacterEdit character)
{
    return character.Effects.GetEffectsByType(EffectType.CombatStance)
        .Any(e => e.Name == ParryModeName && e.IsActive);
}
```

### Pattern 4: Stance-to-DefenseType Pre-selection
**What:** When a stance is active, the DefendMode component should pre-select the matching defense type option.
**When to use:** When entering DefendMode while a stance is active.
**Mapping:**
- Normal stance -> Passive pre-selected (default behavior already)
- Parry Mode stance -> Parry pre-selected (already partially handled by `isInParryMode` check)
- Dodge Focus stance -> Dodge pre-selected
- Block with Shield stance -> ShieldBlock pre-selected

### Anti-Patterns to Avoid
- **Duplicating defense logic**: The DefendMode and DamageResolution components are fully functional. Do NOT rebuild defense/damage flows. Wire them through as-is.
- **Modifying DefenseResolver for stances**: Stances are a UI convenience / pre-selection hint. The core defense resolution math does not change.
- **Using full-size combat tiles for stances**: CONTEXT.md explicitly says stances should use smaller toggles/chips to differentiate from action buttons.
- **Making stance switching cost AP**: CONTEXT.md explicitly says "Switching stance is a free action -- no AP or fatigue cost." However, Parry Mode already costs AP to ENTER. The stance toggle should trigger the existing enter-parry-mode flow (which costs AP).

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Defense resolution | New defense calc | Existing `DefenseResolver.cs` + `DefendMode.razor` | Fully tested, handles all 4 types + concentration |
| Damage resolution | New damage flow | Existing `DamageResolution.razor` + `DamageResolver.cs` | Full armor, shield, hit location, wound system |
| Parry Mode management | New stance state | Existing `CombatStanceBehavior.cs` | Handles enter/exit/check/persist via Effects |
| Action cost selection | New cost picker | Existing `ActionCostSelector` + `BoostSelector` | Shared components already used by all modes |
| Shield detection | Re-query items | Existing `hasShieldEquipped` / `hasWeaponEquipped` state | Already computed in `LoadEquipmentAndSkills()` |

**Key insight:** This phase is 90% UI wiring and 10% new stance logic. Almost all game logic already exists.

## Common Pitfalls

### Pitfall 1: Parry Mode Stance Toggle vs Enter Parry Mode Flow
**What goes wrong:** Treating the "Parry Mode" stance chip as a simple state toggle when Parry Mode actually costs 1 AP + 1 FAT to enter and creates a CombatStance effect.
**Why it happens:** Other stances (Normal, Dodge Focus) are free preference changes, but Parry Mode has a real game cost.
**How to avoid:** The Parry Mode stance chip should either:
  - (a) Trigger the existing `EnterParryMode()` flow from DefendMode (which deducts AP/FAT and creates the effect), OR
  - (b) Enter DefendMode with "Enter Parry" pre-selected so the player can confirm and pay the cost
**Warning signs:** Parry Mode activating without AP/FAT deduction.

### Pitfall 2: Stance State Persistence
**What goes wrong:** Stance resets on page navigation or component re-render.
**Why it happens:** If stances are stored only in component state (`private string activeStance`) they won't survive navigation.
**How to avoid:** Use CombatStance effects (already done for Parry Mode). For Dodge Focus and Block with Shield, decide: either also use effects, or accept they reset (acceptable per CONTEXT.md since they're just pre-selection hints). Normal stance means "no stance effect active."
**Recommendation:** Store Dodge Focus and Block with Shield as CombatStance effects following the Parry Mode pattern, since they should persist within a combat session. Normal = absence of any stance effect.

### Pitfall 3: Disabled Stance Without Tooltip
**What goes wrong:** "Block with Shield" is grayed out but player doesn't know why.
**Why it happens:** Disabled buttons need explicit tooltip text per CONTEXT.md: "Disabled stances show tooltip explaining what's needed."
**How to avoid:** Use HTML `title` attribute on disabled buttons (already used on combat tiles -- follow the same pattern).

### Pitfall 4: Stance Breaking on Actions (Parry Mode Specific)
**What goes wrong:** Player sets stance to Parry Mode, then attacks, but the stance chip still shows Parry Mode as active.
**Why it happens:** All attack/action methods already call `CombatStanceBehavior.EndParryMode(Character)`, but the stance chip state in the UI may not reflect this.
**How to avoid:** Re-check `CombatStanceBehavior.IsInParryMode(Character)` after returning from any action mode. The stance chips should derive their "active" state from the actual character effects, not from a separate UI state variable.

### Pitfall 5: Take Damage Mode Entry Without Data
**What goes wrong:** Player clicks Take Damage but the DamageResolution component expects initial SV/damage data that doesn't exist yet.
**Why it happens:** When coming from DefendMode via `OnDefenseHit`, data is passed. When clicking Take Damage directly, no initial data exists.
**How to avoid:** This is **already handled**. `StartTakeDamage()` calls `ClearPendingDamage()` first, and `DamageResolution` handles null initial parameters gracefully (player enters SV manually).

### Pitfall 6: Dodge Focus and Block with Shield Game Mechanics
**What goes wrong:** Implementing these stances with mechanical effects that aren't defined in the game rules.
**Why it happens:** The design docs mention Parry Mode extensively but Dodge Focus and Block with Shield stances are not mechanically defined beyond being preference hints.
**How to avoid:** Per CONTEXT.md: "Active stance pre-selects the matching defense type in DefendMode (player can still override)." These stances are UI convenience, not mechanical bonuses. Do NOT add +2 to dodge or other bonuses (the old TabDefense.razor had "+2 to dodge checks" but that's from an outdated prototype). The only mechanical stance is Parry Mode (free parry defenses while active).

## Code Examples

### Example 1: Current Defense Group HTML (TabCombat.razor lines 194-216)
```html
<!-- Defense Group -->
<div class="card combat-group-defense mb-3">
    <div class="card-header">
        <i class="bi bi-shield-shaded"></i> <strong>Defense</strong>
    </div>
    <div class="card-body">
        <div class="d-flex flex-wrap gap-2">
            <button class="btn combat-tile combat-tile-defense"
                    @onclick="StartDefend"
                    disabled="@(Character?.IsPassedOut ?? true)"
                    title="Defend against an attack">
                <i class="bi bi-shield-shaded"></i>
                <span>Defend</span>
            </button>
            <button class="btn combat-tile combat-tile-defense"
                    @onclick="StartTakeDamage"
                    title="Resolve damage from falls, spells, or other sources">
                <i class="bi bi-bandaid"></i>
                <span>Take Damage</span>
            </button>
        </div>
    </div>
</div>
```

### Example 2: Existing Parry Mode Entry (DefendMode.razor lines 935-968)
```csharp
private async Task EnterParryMode()
{
    if (!CanAct() || Character == null) return;

    DeductCosts();

    var effect = EffectPortal.CreateChild(
        EffectType.CombatStance,
        CombatStanceBehavior.ParryModeName,
        null,
        null,  // indefinite duration
        CombatStanceBehavior.CreateParryModeState(WeaponSkillName, WeaponSkillAS));

    effect.Description = CombatStanceBehavior.GetParryModeDescription(WeaponSkillName);
    effect.Source = "Combat";
    Character.Effects.AddEffect(effect);

    if (Character is Csla.Core.ISavable savable)
    {
        await savable.SaveAsync();
    }

    isInParryMode = true;
    // ...
}
```

### Example 3: Stance-Aware DefendMode Pre-selection
```csharp
// New parameter for DefendMode:
[Parameter] public string? ActiveStance { get; set; }

// In OnParametersSetAsync, after checking parry mode:
defenseType = ActiveStance switch
{
    "dodge" => DefenseTypeOption.Dodge,
    "parry" when isInParryMode => DefenseTypeOption.Parry,
    "block" when HasShieldEquipped => DefenseTypeOption.ShieldBlock,
    _ => DefenseTypeOption.Passive
};
```

### Example 4: CSS for Stance Chips
```css
/* Stance toggle chips - smaller than combat tiles */
.stance-chip {
    font-size: 0.75rem;
    padding: 0.2rem 0.6rem;
    border-radius: 1rem; /* pill shape */
    border: 1px solid var(--color-border-primary);
    background: transparent;
    color: var(--color-text-secondary);
    transition: all 0.2s ease;
}

.stance-chip.active {
    background: var(--color-accent-blue);
    color: white;
    border-color: var(--color-accent-blue);
}

.stance-chip:disabled {
    opacity: 0.4;
    cursor: not-allowed;
}
```

### Example 5: Deriving Active Stance from Character State
```csharp
private string GetActiveStance()
{
    if (Character == null) return "normal";

    var stanceEffect = Character.Effects.GetEffectsByType(EffectType.CombatStance)
        .FirstOrDefault(e => e.IsActive);

    if (stanceEffect == null) return "normal";

    return stanceEffect.Name switch
    {
        CombatStanceBehavior.ParryModeName => "parry",
        "Dodge Focus" => "dodge",
        "Block with Shield" => "block",
        _ => "normal"
    };
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| TabDefense.razor (old standalone tab) | Defense group inside TabCombat.razor | Phase 32 layout restructuring | TabDefense.razor is legacy; defense now lives in the Combat tab |
| Stances as simple string in component state | CombatStance effects (Parry Mode) | Phase 32+ | Parry Mode persists via Effects; new stances should follow same pattern |

**Deprecated/outdated:**
- `TabDefense.razor`: This file exists but is an old prototype. The Combat tab (TabCombat.razor) now contains the Defense group. The old file has hard-coded stance buttons with TODO comments and fake data. Do NOT use it as the base; use the existing Defense group in TabCombat.razor.
- "+2 to dodge checks" in old TabDefense: This bonus is not in the game rules. Dodge Focus should be a preference hint only.

## Existing Files Inventory

### Files to MODIFY (not create from scratch)

| File | What Changes |
|------|-------------|
| `Threa/Threa.Client/Components/Pages/GamePlay/TabCombat.razor` | Add stance chips to Defense group card. Add `activeStance` state. Pass stance to DefendMode. |
| `Threa/Threa/wwwroot/css/themes.css` | Add `.stance-chip` CSS styles |
| `GameMechanics/Effects/Behaviors/CombatStanceBehavior.cs` | Add constants/helpers for Dodge Focus and Block with Shield stances |
| `Threa/Threa.Client/Components/Pages/GamePlay/DefendMode.razor` | Add `ActiveStance` parameter for pre-selection logic |

### Files to READ (reference only)

| File | What It Tells Us |
|------|-----------------|
| `GameMechanics/Combat/DefenseResolver.cs` | Defense resolution logic (DO NOT MODIFY) |
| `GameMechanics/Combat/DefenseRequest.cs` | Defense request shape (DO NOT MODIFY) |
| `GameMechanics/Combat/DefenseResult.cs` | Defense result shape (DO NOT MODIFY) |
| `GameMechanics/Combat/DefenseType.cs` | The 4 defense types enum (Passive, Dodge, Parry, ShieldBlock) |
| `Threa/Threa.Client/Components/Pages/GamePlay/DamageResolution.razor` | Take damage flow (DO NOT MODIFY -- already wired) |
| `Threa/Threa.Client/Components/Pages/GamePlay/DefenseHitData.cs` | Hit data transfer (already works) |
| `Threa/Threa.Client/Components/Shared/ActionCostSelector.razor*` | Shared cost selector (already used by DefendMode) |
| `Threa/Threa.Client/Components/Shared/BoostSelector.razor*` | Shared boost selector (already used by DefendMode) |

### Files to IGNORE

| File | Why |
|------|-----|
| `Threa/Threa.Client/Components/Pages/GamePlay/TabDefense.razor` | Legacy prototype with TODOs. Superseded by Combat tab Defense group. |

## Open Questions

1. **Dodge Focus and Block with Shield as CombatStance effects vs. component state**
   - What we know: Parry Mode uses CombatStance effects. Normal = no effect.
   - What's unclear: Should Dodge Focus and Block with Shield also be CombatStance effects (persistent, survives navigation) or just component state (simpler, resets on navigation)?
   - Recommendation: Use CombatStance effects for all stances. This matches Parry Mode pattern, keeps them consistent, and makes stance visible on the Effects tab. Normal = clearing any active stance effect.

2. **Parry Mode stance chip behavior -- inline activation vs. redirect to DefendMode**
   - What we know: Parry Mode costs 1 AP + 1 FAT to enter. Other stances are free.
   - What's unclear: Should clicking the "Parry Mode" chip activate it directly (deducting AP/FAT inline) or enter DefendMode with "Enter Parry" pre-selected?
   - Recommendation: Activate directly from the chip using the same logic as `DefendMode.EnterParryMode()`. This is faster and matches the CONTEXT.md flow ("stance selection is a free action" -- but Parry Mode specifically costs AP). The chip click should deduct cost and activate, consistent with how the existing EnterParryMode works. If the user can't afford it, the chip should be visually hindered (same as "Defend button always visible; visual hint when no AP available").

3. **"Defend" button AP hint**
   - What we know: CONTEXT.md says "Defend button always visible; visual hint when no AP available for active defense (passive still works)."
   - What's unclear: What visual hint? Reduced opacity? Warning icon? Text change?
   - Recommendation: Use reduced opacity (0.6) + small warning badge or changed icon tint, similar to how other disabled-but-not-truly-disabled buttons work. The button stays clickable (passive defense always works) but visual hint tells player active defense won't be available.

## Sources

### Primary (HIGH confidence)
- Codebase analysis: `TabCombat.razor` (1398 lines) - full combat tab with all modes
- Codebase analysis: `DefendMode.razor` (1221 lines) - complete defense workflow
- Codebase analysis: `DamageResolution.razor` (748 lines) - complete damage resolution
- Codebase analysis: `CombatStanceBehavior.cs` (208 lines) - Parry Mode effect management
- Codebase analysis: `DefenseResolver.cs` (184 lines) - defense math
- Codebase analysis: `themes.css` - combat tile and group styling
- Phase context: `35-CONTEXT.md` - all user decisions

### Secondary (MEDIUM confidence)
- Design docs: `COMBAT_SYSTEM.md` - defense options and parry mode rules
- Design docs: `GAME_RULES_SPECIFICATION.md` - parry mode mechanics
- Design docs: `PLAY_PAGE_DESIGN.md` - defend mode workflow design

### Tertiary (LOW confidence)
- `TabDefense.razor` - old prototype, useful only for understanding original stance concept

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - all libraries already in use, no new dependencies
- Architecture: HIGH - all patterns already established in codebase (mode pattern, CombatStance effects, combat tile styling)
- Pitfalls: HIGH - identified from direct codebase analysis, no external speculation

**Research date:** 2026-02-13
**Valid until:** 2026-03-15 (stable internal codebase, no external dependency changes)
