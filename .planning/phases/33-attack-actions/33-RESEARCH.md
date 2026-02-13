# Phase 33: Attack Actions - Research

**Researched:** 2026-02-12
**Domain:** Blazor UI wiring, existing combat resolution integration, anonymous target flow
**Confidence:** HIGH

## Summary

Phase 33 wires the existing melee and ranged attack buttons in the Combat tab's Actions group to existing attack flows, and adds "anonymous target" support for solo play. The core finding is that the button infrastructure and combat mode components (`AttackMode.razor`, `RangedAttackMode.razor`) already exist and function correctly. The phase's work centers on three areas: (1) adding "Anonymous Target" as a first-class target option in the attack flows, (2) displaying AV in melee results and adding TV modifier input for ranged anonymous targets, and (3) ensuring the ranged button shows disabled-with-tooltip when no ranged weapon is equipped.

The existing code already handles skill selection, weapon selection, AP/FAT costs, boost mechanics, dice rolling, and activity log integration. No new combat resolution logic or business objects are needed. All work is UI-layer modifications in Blazor components.

**Primary recommendation:** Modify the existing `AttackMode.razor` and `RangedAttackMode.razor` components to support anonymous target selection and result display adjustments. Do NOT create new components or resolvers.

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Blazor (InteractiveServer) | .NET 10 | UI rendering | Already in use for all play pages |
| Bootstrap 5 + Bootstrap Icons | Current | Styling, icons | Already in use throughout |
| Radzen.Blazor | 8.4.2 | Dialog service | Already injected, used for concentration break dialogs |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| GameMechanics.Combat | Local | Attack/defense resolvers | Already used by AttackMode and RangedAttackMode |
| GameMechanics.Messaging | Local | Activity log service | Already wired via IActivityLogService |

### Alternatives Considered
None. The entire stack is already established. This phase adds no new libraries.

## Architecture Patterns

### Existing Component Hierarchy (NO CHANGES NEEDED)
```
Play.razor
  └── TabCombat.razor  (CombatMode state machine)
        ├── Default mode: 3 button groups (Actions, Defense, Other)
        ├── Attack mode: AttackMode.razor
        ├── RangedAttack mode: RangedAttackMode.razor
        ├── Defend mode: DefendMode.razor
        ├── TakeDamage mode: DamageResolution.razor
        ├── Reload mode: ReloadMode.razor
        ├── Unload mode: UnloadMode.razor
        ├── Medical mode: MedicalMode.razor
        ├── ActivateImplant mode: ActivateImplantMode.razor
        └── SelectTarget mode: TargetSelectionModal.razor
```

### Pattern 1: CombatMode State Machine
**What:** `TabCombat.razor` uses a `CombatMode` enum to switch between default (button groups) and specialized mode components.
**When to use:** All combat actions follow this pattern.
**Key mechanics:**
- `CombatMode.Default` renders the three button groups
- Clicking a button calls a method (e.g., `StartAttack()`) that sets `combatMode = CombatMode.Attack`
- The mode component renders in place of the button groups
- On completion, the mode component fires `OnCancel` or `OnAttackComplete` callbacks
- The callback sets `combatMode = CombatMode.Default` to return to button groups

**Source:** `TabCombat.razor` lines 20-369, 683-692

### Pattern 2: Attack Mode Internal State Machine
**What:** `AttackMode.razor` has its own two-phase state: `hasRolled = false` (setup) vs `hasRolled = true` (results). This is the existing pattern for all attack flows.
**Key mechanics:**
- Setup phase: skill selection, cost selection, boost, options, summary card
- Roll button calls `ExecuteAttack()` which rolls dice, builds result data, sets `hasRolled = true`
- Results phase: shows breakdown, AV, dice roll
- "Done" button fires `OnAttackComplete` callback with message string
- "New Attack" button resets `hasRolled = false`

**Source:** `AttackMode.razor` lines 17-258

### Pattern 3: Activity Log Integration
**What:** Attack completion messages are passed up to `TabCombat.razor` which logs them via `IActivityLogService.Publish()`.
**Key mechanics:**
- `OnAttackComplete` callback in `TabCombat.razor` calls `LogActivity(resultMessage, ActivityCategory.Combat)`
- `LogActivity` calls `ActivityLog.Publish(Table.Id, message, Character.Name, category)`
- This is already fully wired for both melee and ranged attacks

**Source:** `TabCombat.razor` lines 884-890, 921-970, 1195-1201

### Anti-Patterns to Avoid
- **Don't create new CombatMode enum values:** The existing `CombatMode.Attack` and `CombatMode.RangedAttack` are correct. Anonymous target support should be handled WITHIN the existing attack mode components, not as new modes.
- **Don't bypass the AttackMode/RangedAttackMode components:** The phase context says "this connects existing combat resolution to the new button layout" — modify existing components, don't build parallel ones.
- **Don't create new resolver classes:** The existing `AttackResolver` and `FirearmAttackResolver` are not directly used by the UI components for solo attacks (AttackMode.razor rolls dice inline with `GameMechanics.Dice.Roll4dFPlus()`). The anonymous target flow should follow the same inline pattern.

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Dice rolling | Custom dice method | `GameMechanics.Dice.Roll4dFPlus()` | Already used by AttackMode.razor (line 439) |
| AP/FAT cost handling | Custom cost logic | Existing `ActionCostSelector` + `BoostSelector` components | Already wired in both attack modes |
| Activity logging | Custom log method | `IActivityLogService.Publish()` via `TabCombat.LogActivity()` | Already wired for attack completion |
| Weapon detection | Custom equipment scan | `WeaponSelector.GetMeleeWeapons()` and `GetRangedWeapons()` | Used by `TabCombat.LoadEquipmentAndSkills()` |
| Concentration break | Custom dialog | `ConcentrationBreakDialog.ShowAsync()` | Already used in both attack mode components |

**Key insight:** Phase 33 requires zero new utility code. Every piece of infrastructure (dice, costs, logging, weapon detection) is already built and tested.

## Existing State of the Codebase (CRITICAL)

### What Already Works (DO NOT REBUILD)

1. **Melee Attack button** (`TabCombat.razor` line 148-154): Already wired to `StartAttack()` which sets `combatMode = CombatMode.Attack`. Renders `AttackMode.razor`.

2. **Ranged Attack button** (`TabCombat.razor` line 162-168): Already wired to `StartRangedAttack()`. Renders `RangedAttackMode.razor`. Already checks `hasRangedWeaponEquipped` for disabled state.

3. **AttackMode.razor**: Full melee attack flow — skill selection, cost selection, boost, physicality option, roll, results with AV display, Done/New Attack buttons. AV is already displayed in results (line 192-195).

4. **RangedAttackMode.razor**: Full ranged attack flow — weapon selection, fire mode, range, target conditions, attacker conditions, dodge adjustment, cost, boost, roll, results with AV/TV/RV/SV display, Done/New Attack buttons.

5. **Activity log integration**: Both `OnAttackComplete` (melee) and `OnRangedAttackComplete` (ranged) in TabCombat already log to the activity feed.

6. **Ranged button disabled state**: Already disabled when `!hasRangedWeaponEquipped` (line 164).

### What's Missing (PHASE 33 SCOPE)

1. **Side-by-side button layout**: Currently the Actions group has 4 buttons (Attack, Target, Ranged, Ranged Target) in a flex-wrap row. Per context: melee and ranged buttons should sit "side by side in the same row." The current layout already does this via `d-flex flex-wrap gap-2`, but the button arrangement may need adjustment based on the phase decisions about which buttons exist (solo melee, solo ranged, targeted melee, targeted ranged).

2. **Anonymous target as first-class option**: Neither `AttackMode.razor` nor `RangedAttackMode.razor` have a target selection step currently. The phase requires a target selection step where "Anonymous Target" is an option alongside real targets. When anonymous target is selected:
   - Melee: AV displays after rolling (this ALREADY happens at line 192-195)
   - Ranged: Single TV modifier field, SV display (this is a simplified version of the existing range/condition/cover UI)

3. **Ranged button visible-but-disabled with tooltip**: The button is already disabled when no ranged weapon equipped, but the context specifies it should be "visible but disabled (grayed out, tooltip 'No ranged weapon equipped')." Need to verify the tooltip text and ensure styling matches the context requirement.

4. **Solo ranged simplified TV input**: The existing `RangedAttackMode.razor` has full range/target-condition/cover/size/dodge-adjustment UI for TV calculation. For anonymous targets, the context requires "single number field for total TV modifier" — the player enters one number. This needs a conditional path: if target is anonymous, show simplified TV input; if target is real, show full breakdown.

5. **Solo ranged SV-only result**: The existing ranged results show full breakdown (AV, TV, RV, burst/suppression/AOE details). For anonymous targets, show "SV only (roll - TV)."

### Key Code Locations for Modification

| File | What to Modify | Why |
|------|---------------|-----|
| `TabCombat.razor` lines 147-178 | Actions group button layout | Ensure melee + ranged side by side, adjust button arrangement |
| `TabCombat.razor` line 164 | Ranged button disabled state | Add tooltip "No ranged weapon equipped" |
| `AttackMode.razor` | Add target selection as first step | "Anonymous Target" as option; if anonymous selected, current flow proceeds as-is (AV already shown) |
| `RangedAttackMode.razor` | Add target selection and anonymous path | If anonymous: simplified TV input + SV-only result display |
| `TabCombat.razor` lines 683-708 | `StartAttack()` and `StartAttackWithSkill()` | May need to pass target info to AttackMode |

## Common Pitfalls

### Pitfall 1: Over-engineering Target Selection
**What goes wrong:** Creating a whole new TargetSelectionModal or component for the anonymous target option when a simple in-component dropdown or button group suffices.
**Why it happens:** The existing `TargetSelectionModal.razor` is designed for real targets with IDs, names, NPC disposition. Anonymous target doesn't fit that model.
**How to avoid:** Add a simple target selector directly into `AttackMode.razor` and `RangedAttackMode.razor` as their first step. A short list showing "Anonymous Target" (always present) plus any real targets passed in.
**Warning signs:** Creating new files for target selection within solo attack flows.

### Pitfall 2: Duplicating Ranged Attack Logic
**What goes wrong:** Building a separate simplified ranged attack flow for anonymous targets instead of conditionally simplifying the existing one.
**Why it happens:** The existing `RangedAttackMode.razor` has extensive UI for range categories, target conditions, cover, etc. Tempting to build a separate component.
**How to avoid:** Use a boolean flag (e.g., `isAnonymousTarget`) to conditionally render the simplified TV input vs. full breakdown within the same component.
**Warning signs:** Creating a new `SoloRangedAttackMode.razor` file.

### Pitfall 3: Breaking the Existing Full Attack Flow
**What goes wrong:** Modifying `AttackMode.razor` or `RangedAttackMode.razor` in a way that breaks the targeted attack flow that already works.
**Why it happens:** Adding target selection as a first step changes the flow sequence.
**How to avoid:** The target selection step should be additive: after target is selected, the rest of the flow proceeds exactly as it does today. For anonymous target, the flow should be: select anonymous -> same setup phase -> same roll phase -> same result phase (with minor display differences for anonymous).
**Warning signs:** Changing the existing `ExecuteAttack()` or `CompleteAttack()` methods in incompatible ways.

### Pitfall 4: Forgetting AP/FAT Costs for Solo Attacks
**What goes wrong:** Making solo anonymous attacks "free" because there's no real game consequence.
**Why it happens:** Temptation to skip cost deduction for anonymous targets.
**How to avoid:** Context explicitly says "Solo attacks have full AP/FAT cost — they are real actions in game time, not free practice." Existing code already handles costs. Just ensure the anonymous path goes through the same cost logic.
**Warning signs:** Adding `if (isAnonymous) return;` before cost deduction.

### Pitfall 5: Auto-Rolling on Target Selection
**What goes wrong:** Automatically rolling when anonymous target is selected.
**Why it happens:** Since there's nothing to "resolve" against a target, it seems natural to auto-roll.
**How to avoid:** Context says "Player must click to roll (never auto-roll) because AP bonuses can always be applied before rolling." The existing click-to-roll pattern in both attack modes already works correctly. Just don't add auto-roll logic for anonymous targets.
**Warning signs:** Calling `ExecuteAttack()` immediately after target selection.

## Code Examples

### Existing Melee Attack Button Wiring (already works)
```csharp
// Source: TabCombat.razor line 683-692
private void StartAttack()
{
    // Attacking ends parry mode
    if (Character != null && CombatStanceBehavior.IsInParryMode(Character))
    {
        CombatStanceBehavior.EndParryMode(Character);
    }
    selectedSkill = attackSkills.FirstOrDefault();
    combatMode = CombatMode.Attack;
}
```

### Existing AV Display in Results (already works for melee)
```razor
@* Source: AttackMode.razor lines 191-195 *@
<tr class="table-primary">
    <td><strong>Attack Value (AV):</strong></td>
    <td><strong class="fs-4">@attackResult.AV</strong></td>
</tr>
```

### Existing Activity Log Pattern
```csharp
// Source: TabCombat.razor lines 884-890
private async Task OnAttackComplete(string resultMessage)
{
    LogActivity(resultMessage, ActivityCategory.Combat);
    combatMode = CombatMode.Default;
    selectedSkill = null;
    await InvokeAsync(StateHasChanged);
}
```

### Existing Ranged Disabled State
```razor
@* Source: TabCombat.razor line 162-168 *@
<button class="btn combat-tile combat-tile-actions"
        @onclick="StartRangedAttack"
        disabled="@(!CanAct() || !hasRangedWeaponEquipped)"
        title="Ranged attack">
    <i class="bi bi-crosshair"></i>
    <span>Ranged</span>
</button>
```

### Example: Anonymous Target Selector Pattern (recommended)
```razor
@* Add as first step in AttackMode.razor, before skill selection *@
@if (!targetSelected)
{
    <div class="card mb-3">
        <div class="card-header"><strong>Select Target</strong></div>
        <div class="card-body">
            <div class="list-group">
                <button class="list-group-item list-group-item-action"
                        @onclick="() => SelectAnonymousTarget()">
                    <i class="bi bi-question-circle text-muted me-2"></i>
                    <strong>Anonymous Target</strong>
                    <small class="text-muted d-block">Solo attack — AV displayed after rolling</small>
                </button>
                @* Future: real targets will be listed here *@
            </div>
        </div>
    </div>
}
else
{
    @* Existing skill selection, cost, boost, roll flow *@
}
```

### Example: Simplified Ranged TV Input for Anonymous Target
```razor
@* Replace the full range/condition/cover UI when anonymous target selected *@
@if (isAnonymousTarget)
{
    <div class="card mb-3">
        <div class="card-header"><strong>Target Value</strong></div>
        <div class="card-body">
            <div class="input-group">
                <span class="input-group-text">Total TV Modifier</span>
                <input type="number" class="form-control" @bind="anonymousTVModifier" />
            </div>
            <small class="text-muted">Enter total TV modifier (range, cover, conditions, etc.)</small>
        </div>
    </div>
}
else
{
    @* Existing full range/conditions UI *@
}
```

### Example: Solo Ranged SV-Only Result
```razor
@* Simplified result for anonymous ranged target *@
@if (isAnonymousTarget)
{
    <div class="alert alert-info">
        <strong>Success Value (SV): @(attackResult.AV - anonymousTVModifier)</strong>
        <br />
        <small>AV @attackResult.AV - TV @anonymousTVModifier</small>
    </div>
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Separate defense tab | Defense group in Combat tab | Phase 32 (2026-02-12) | Defense buttons now in same tab as attack buttons |
| Activity log in bottom panel only | Activity log in Combat tab layout | Phase 32 (2026-02-12) | Combat tab has integrated activity feed |

**Deprecated/outdated:**
- `TabDefense.razor`: File still exists but Defense tab was removed from Play.razor navigation in Phase 32. Still in codebase as reference.

## Open Questions

1. **Target selection step for existing (non-anonymous) attacks**
   - What we know: Context says "Target list always shown — 'Anonymous Target' is an explicit option alongside any real targets (if any)." The existing `AttackMode.razor` and `RangedAttackMode.razor` do NOT have a target list today.
   - What's unclear: Should this phase add the real target list too, or only anonymous? The context seems to imply both: "alongside any real targets (if any)."
   - Recommendation: Add the target list with Anonymous Target always present. If `AvailableTargets` parameter is provided (from TabCombat which gets it from Play.razor), show them too. This is minimal extra work and avoids having to retrofit target selection later.

2. **Melee weapon picker always shows**
   - What we know: Context says "Melee weapon picker always shows because punch/kick hand-to-hand options are always available — never skip the picker."
   - What's unclear: The current AttackMode.razor shows a skill selector (step 1), not a weapon picker. The skill list already includes unarmed/hand-to-hand skills via the `attackSkills` list built in `TabCombat.LoadEquipmentAndSkills()` (which includes skills categorized as "Weapon" plus Physicality).
   - Recommendation: Treat the existing skill selector as the "weapon picker" since skills map to weapons (e.g., "Swordsmanship" for swords, "Physicality" or "Hand-to-Hand" for unarmed). The context intent is "player can always attack even without weapons" — this is already supported.

3. **Button arrangement for side-by-side**
   - What we know: Context says "Melee and ranged buttons sit side by side in the same row within the Actions group." Currently there are 4 buttons: Attack, Target, Ranged, Ranged Target.
   - What's unclear: Whether "side by side" means just the solo buttons (Attack + Ranged) or also the targeted variants.
   - Recommendation: Keep all 4 buttons in the same flex row (already the case with `d-flex flex-wrap gap-2`). Ensure Attack and Ranged are adjacent. This already works with current layout.

## Sources

### Primary (HIGH confidence)
- `TabCombat.razor` — Full source read, all button wiring, combat mode state machine, activity logging
- `AttackMode.razor` — Full source read, melee attack setup/roll/result flow
- `RangedAttackMode.razor` — Full source read, ranged attack setup/roll/result flow
- `AttackResolver.cs`, `AttackResult.cs`, `AttackRequest.cs` — Business logic for melee attacks
- `FirearmAttackResolver.cs`, `FirearmAttackRequest.cs`, `FirearmAttackResult.cs` — Business logic for ranged attacks
- `RangedWeaponInfo.cs` — UI model for ranged weapons
- `WeaponSelector.cs` — Melee/ranged weapon detection logic
- `Play.razor` — Parent page, tab navigation, activity log management
- `TargetSelectionModal.razor` — Existing target selection (for reference, not direct reuse)
- `IActivityLogService.cs` — Activity log interface
- `Dice.cs` — Dice rolling (4dF+ static method)
- `themes.css` — Combat tile styling (`.combat-tile`, `.combat-tile-actions`, etc.)
- Phase 32 summaries — Prior phase layout work

### Secondary (MEDIUM confidence)
- `33-CONTEXT.md` — Phase decisions from user discussion

### Tertiary (LOW confidence)
- None

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH — All libraries already in use, no additions needed
- Architecture: HIGH — All patterns established, modifying existing components only
- Pitfalls: HIGH — Identified from direct source code analysis, not speculation

**Research date:** 2026-02-12
**Valid until:** 2026-03-12 (stable project, no external dependency changes expected)
