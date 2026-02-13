# Phase 34: New Action Types - Research

**Researched:** 2026-02-12
**Domain:** Blazor UI + GameMechanics action resolution for non-attack actions
**Confidence:** HIGH

## Summary

Phase 34 adds two new standalone actions in the Combat tab's Actions group: **Anonymous Action** (attribute-only roll vs TV) and **Use Skill** (combat skill check via modal, then inline roll). Neither triggers the attack workflow. Both share the same cost model (1AP+1FAT or 2AP) and follow the established inline panel pattern used by existing modes (AttackMode, RangedAttackMode, MedicalMode).

The codebase already has all the foundational pieces needed. The `ActionResolver` in `GameMechanics/Actions/` handles generic skill-based action resolution with AS calculation, TV comparison, and SV output. The `TabPlaySkills.razor` component already demonstrates a complete skill check flow (select skill, enter TV, choose cost, roll 4dF+, display SV result) but lives on the Skills tab, not the Combat tab, and does not include attribute-only actions.

**Primary recommendation:** Build two new Razor combat mode components (`AnonymousActionMode.razor` and `SkillCheckMode.razor`) following the exact pattern of existing mode components, integrate them into `TabCombat.razor`'s mode switcher, and reuse the existing `ActionCostSelector`, `BoostSelector`, and `ActionResolver` infrastructure. No new resolver classes are needed.

## Standard Stack

### Core (already in project)
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Blazor InteractiveServer | .NET 10 | Component rendering | Existing framework |
| CSLA.NET | 9.1.0 | Business object persistence | Existing data pattern |
| Radzen.Blazor | 8.4.2 | Dialog/modal service | Existing modal pattern |
| GameMechanics.Actions | N/A | ActionResolver, ActionResult, AbilityScore | Existing action resolution |
| GameMechanics.Dice | N/A | Roll4dFPlus() static method | Existing dice roller |
| Bootstrap Icons | N/A | Icon library | Existing UI convention |

### Supporting (already in project)
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| ActionCostSelector | Shared component | 1AP+1FAT / 2AP toggle | Both new action panels |
| BoostSelector | Shared component | AP/FAT boost spending | Both new action panels |
| IActivityLogService | Messaging | Activity feed logging | Result logging |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| New inline panels | Reuse TabPlaySkills | Context says Actions group in Combat tab, not Skills tab |
| ActionResolver | Manual roll logic | ActionResolver exists but uses Skill DTO; for anonymous actions we need attribute-only roll which is simpler |
| Radzen modal for skill picker | Custom component | Radzen DialogService already used for modals; consistent pattern |

**Installation:** No new packages needed.

## Architecture Patterns

### Existing Combat Mode Pattern (HIGH confidence)

The `TabCombat.razor` uses an enum-based mode switcher (`CombatMode`) to swap between inline panels. Each mode is a separate Razor component that replaces the default view.

**Source:** `Threa/Threa.Client/Components/Pages/GamePlay/TabCombat.razor`

```
TabCombat.razor
├── CombatMode.Default     → Shows 3 action groups + activity feed
├── CombatMode.Attack      → <AttackMode />
├── CombatMode.RangedAttack → <RangedAttackMode />
├── CombatMode.Defend      → <DefendMode />
├── CombatMode.TakeDamage  → <DamageResolution />
├── CombatMode.Reload      → <ReloadMode />
├── CombatMode.Unload      → <UnloadMode />
├── CombatMode.Medical     → <MedicalMode />
├── CombatMode.ActivateImplant → <ActivateImplantMode />
├── CombatMode.SelectTarget → <TargetSelectionModal />
├── (NEW) CombatMode.AnonymousAction → <AnonymousActionMode />
└── (NEW) CombatMode.SkillCheck → <SkillCheckMode />
```

### Mode Component Standard Pattern (HIGH confidence)

Every combat mode component follows the same structure:

1. **Header** with title + Cancel button
2. **Setup phase** (left column: inputs; right column: summary card)
3. **Roll button** in summary card footer
4. **Result phase** with breakdown table + Done/Reset buttons
5. **Complete callback** passes result message string to parent
6. **Parent logs** the message to activity feed via `LogActivity()`

Parameters always include:
- `Character` (GameMechanics.CharacterEdit)
- `Table` (GameMechanics.GamePlay.TableEdit)
- `OnCancel` (EventCallback)
- `On{Action}Complete` (EventCallback<string> or EventCallback<{Data}>)

### Cost Deduction Pattern (HIGH confidence)

Every action mode deducts costs from the character before rolling:

```csharp
// From AttackMode.razor, MedicalMode.razor, RangedAttackMode.razor:
var baseAPCost = costType == ActionCostType.TwoAP ? 2 : 1;
var baseFATCost = costType == ActionCostType.OneAPOneFat ? 1 : 0;

var totalAPCost = baseAPCost + boostAPCost;
var totalFATCost = baseFATCost + boostFATCost;

Character.ActionPoints.Available -= totalAPCost;
Character.ActionPoints.ActionsTakenThisRound += 1;
if (totalFATCost > 0)
    Character.Fatigue.Value -= totalFATCost;
```

Then saves via CSLA:
```csharp
if (Character is Csla.Core.ISavable savable)
{
    await savable.SaveAsync();
}
```

### Concentration Break Pattern (HIGH confidence)

Every action that costs AP/FAT checks for active concentration effects and prompts before breaking:

```csharp
var concentrationEffect = Character.GetConcentrationEffect();
if (concentrationEffect != null)
{
    var state = ConcentrationState.FromJson(concentrationEffect.BehaviorState);
    if (state != null)
    {
        var confirmed = await ConcentrationBreakDialog.ShowAsync(
            DialogService, state, concentrationEffect.Name);
        if (!confirmed) return;
        ConcentrationBehavior.BreakConcentration(Character, "Took action");
    }
}
```

### Activity Log Pattern (HIGH confidence)

Activity log entries are published through `IActivityLogService.Publish()` from the parent `TabCombat.razor`:

```csharp
private void LogActivity(string message, ActivityCategory category)
{
    if (Table != null && Character != null)
    {
        ActivityLog.Publish(Table.Id, message, Character.Name, category);
    }
}
```

The activity feed displays in `TabCombat.razor`'s default mode with timestamp, source, and message.

### Skill Picker Modal Pattern (HIGH confidence)

The existing `SkillPickerModal.razor` uses Radzen's `DialogService` for modal display. It fetches all skills from `IDataPortal<SkillList>`, allows search and category filtering, and returns the selected skill ID via `DialogService.Close(skill.Id)`.

For Phase 34's "Use Skill" modal, a new modal component will be needed that:
- Takes the character's actual skills (from `Character.Skills`) rather than all game skills
- Groups by governing attribute (PrimaryAttribute)
- Shows computed AS values for each skill
- Returns the selected `SkillEdit` object (not just ID)

### Attribute Access Pattern (HIGH confidence)

Character attributes are accessed through `Character.AttributeList`:

```csharp
// Get attribute value by name
var attr = Character.AttributeList.FirstOrDefault(a => a.Name == "STR");
int value = attr?.Value ?? 10;

// Get effective attribute (with item bonuses + effects)
int effective = Character.GetEffectiveAttribute("STR");
```

The 7 core attributes are: STR, DEX, END, INT, ITT, WIL, PHY.

### Ability Score Calculation Pattern (HIGH confidence)

For skill checks, AS is calculated as: `Attribute + SkillLevel - 5 + Modifiers`

For anonymous actions (attribute-only), there is no skill, so AS is simply the attribute value itself. The roll is: `AttributeValue + 4dF+ vs TV`.

**Important distinction:** Anonymous actions use raw attribute value (not AS formula), because there is no skill involved. The context says "selects an attribute from a dropdown, enters a TV, rolls 4dF+." This means: `Roll = AttributeValue + 4dF+`, `SV = Roll - TV`.

### Existing TabPlaySkills Pattern (MEDIUM confidence)

`TabPlaySkills.razor` already implements a full skill check flow on the Skills tab. It:
- Lists non-combat, non-spell skills
- Has TV input, cost selector, boost selector
- Rolls with `Dice.Roll4dFPlus()`
- Shows results with full breakdown

The new Phase 34 skill check in the Combat tab will be different because:
- It shows ALL character skills (per CONTEXT.md: "All character skills shown, not just combat-tagged ones")
- It uses a modal picker grouped by attribute, not a table
- It transitions to an inline panel after selection (not a side panel)
- It lives in the Combat tab, not the Skills tab

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Cost selection UI | Custom radio buttons | `ActionCostSelector` component | Already handles disabled states, auto-selection |
| Boost spending UI | Custom +/- controls | `BoostSelector` component | Handles AP/FAT budget, max calculation |
| Dice rolling | Custom random | `GameMechanics.Dice.Roll4dFPlus()` | Handles exploding dice correctly |
| Cost affordability | Manual AP/FAT checks | Same pattern from existing modes | Well-tested in 6+ components |
| Modal display | Custom overlay | `Radzen.DialogService` | Consistent with existing modals |
| Activity logging | Console.Write | `IActivityLogService.Publish()` | Broadcasts to all subscribers |
| Concentration checks | Custom check | `ConcentrationBreakDialog.ShowAsync()` | Existing pattern in all action modes |

**Key insight:** Every building block for these two new actions already exists. This phase is purely about composing existing pieces into two new mode components and one new modal component.

## Common Pitfalls

### Pitfall 1: Forgetting ActionsTakenThisRound Increment
**What goes wrong:** Multiple action penalty not applied to subsequent actions
**Why it happens:** Developers forget to increment `ActionsTakenThisRound` after deducting AP/FAT
**How to avoid:** Always include `Character.ActionPoints.ActionsTakenThisRound += 1;` in the cost deduction block
**Warning signs:** Multi-action penalty showing as 0 after performing an anonymous action

### Pitfall 2: Not Checking Concentration Before Action
**What goes wrong:** Action executes while character is concentrating, causing silent state corruption
**Why it happens:** New mode components skip the concentration check pattern
**How to avoid:** Copy the concentration break pattern from existing modes into both new modes
**Warning signs:** Character takes action without concentration prompt appearing

### Pitfall 3: Anonymous Action AS Calculation
**What goes wrong:** Using `Attribute + SkillLevel - 5` formula instead of raw attribute value
**Why it happens:** Reusing skill-based AS calculation for attribute-only rolls
**How to avoid:** Anonymous actions use only `Character.GetEffectiveAttribute(attrName)` as the base value, with no skill level or -5 offset
**Warning signs:** Anonymous action results being too low (by 5 or more) compared to expected

### Pitfall 4: Skill AS Must Use Modified Values
**What goes wrong:** Skill AS shown in modal doesn't include effect modifiers or item bonuses
**Why it happens:** Using raw `Attribute + Level - 5` instead of `skill.AbilityScore` which already accounts for effects
**How to avoid:** Use `skill.AbilityScore` from `SkillEdit` which is a computed property that includes all modifiers
**Warning signs:** Skill AS in picker doesn't match the value shown on the Skills tab

### Pitfall 5: Not Saving After Cost Deduction
**What goes wrong:** AP/FAT changes lost if user navigates away before completing action
**Why it happens:** Forgetting the CSLA save pattern after modifying character state
**How to avoid:** Always call `savable.SaveAsync()` after deducting costs
**Warning signs:** AP/FAT values revert after page refresh

### Pitfall 6: CombatMode Enum Not Updated
**What goes wrong:** New modes cannot be entered because the enum lacks values
**Why it happens:** Adding new Razor components without updating the enum and switch in TabCombat
**How to avoid:** Add `AnonymousAction` and `SkillCheck` to the `CombatMode` enum in TabCombat.razor first
**Warning signs:** Buttons exist but clicking them does nothing

### Pitfall 7: Cost Selector Affordability Not Disabling Unaffordable Options
**What goes wrong:** Player selects 2AP cost when they only have 1AP, causing negative AP
**Why it happens:** The existing `ActionCostSelector` already handles this, but the inline panel must pass correct current AP/FAT values
**How to avoid:** Pass `Character.ActionPoints.Available` and `Character.Fatigue.Value` to ActionCostSelector
**Warning signs:** ActionCostSelector shows both options enabled when one should be grayed out

## Code Examples

### Example 1: AnonymousActionMode Structure (Recommended)

```razor
@* AnonymousActionMode.razor - Attribute-only roll vs TV *@

<div class="anonymous-action-mode">
    <div class="d-flex justify-content-between align-items-center mb-3">
        <h4 class="mb-0"><i class="bi bi-dice-5 text-primary"></i> Anonymous Action</h4>
        <button class="btn btn-outline-secondary btn-sm" @onclick="OnCancel">
            <i class="bi bi-x-lg"></i> Cancel
        </button>
    </div>

    @if (!hasRolled)
    {
        <div class="row">
            <div class="col-md-6">
                <!-- 1. Attribute dropdown -->
                <!-- 2. Cost choice (ActionCostSelector) -->
                <!-- 3. TV input (number) -->
                <!-- 4. Boost (BoostSelector) -->
            </div>
            <div class="col-md-6">
                <!-- Summary card with Roll button -->
            </div>
        </div>
    }
    else
    {
        <!-- Result card with breakdown + Done button -->
    }
</div>
```

### Example 2: Attribute Dropdown Population

```csharp
// All 7 attributes available for selection
private static readonly string[] AttributeNames = { "STR", "DEX", "END", "INT", "ITT", "WIL", "PHY" };
private static readonly Dictionary<string, string> AttributeDisplayNames = new()
{
    { "STR", "Physicality (STR)" },
    { "DEX", "Dodge (DEX)" },
    { "END", "Drive (END)" },
    { "INT", "Reasoning (INT)" },
    { "ITT", "Awareness (ITT)" },
    { "WIL", "Focus (WIL)" },
    { "PHY", "Influence (PHY)" }
};

private int GetSelectedAttributeValue()
{
    if (Character == null || string.IsNullOrEmpty(selectedAttribute)) return 0;
    return Character.GetEffectiveAttribute(selectedAttribute);
}
```

### Example 3: Anonymous Action Roll Logic

```csharp
private async Task ExecuteAction()
{
    // ... concentration check, cost deduction (same pattern as AttackMode) ...

    int attributeValue = Character.GetEffectiveAttribute(selectedAttribute);
    int diceRoll = GameMechanics.Dice.Roll4dFPlus();
    int rollResult = attributeValue + diceRoll;
    int sv = rollResult - targetValue;

    // No skill involved, no AS formula - just attribute + dice vs TV
    actionResult = new AnonymousActionResult
    {
        AttributeName = selectedAttribute,
        AttributeValue = attributeValue,
        DiceRoll = diceRoll,
        RollResult = rollResult,
        TargetValue = targetValue,
        SuccessValue = sv,
        IsSuccess = sv >= 0
    };
    hasRolled = true;
}
```

### Example 4: Skill Check Modal (Radzen Dialog)

```csharp
// In SkillCheckMode.razor - open the modal to select a skill
private async Task OpenSkillPicker()
{
    var result = await DialogService.OpenAsync<CombatSkillPickerModal>(
        "Select Skill",
        new Dictionary<string, object>
        {
            { "Character", Character! }
        },
        new Radzen.DialogOptions
        {
            Width = "500px",
            Height = "600px"
        });

    if (result is GameMechanics.SkillEdit selectedSkill)
    {
        this.selectedSkill = selectedSkill;
        skillSelected = true;
    }
}
```

### Example 5: CombatSkillPickerModal Grouping by Attribute

```csharp
// Group character skills by their governing attribute
private IEnumerable<IGrouping<string, GameMechanics.SkillEdit>> GetGroupedSkills()
{
    if (Character?.Skills == null) return Enumerable.Empty<IGrouping<string, GameMechanics.SkillEdit>>();

    return Character.Skills
        .OrderBy(s => s.PrimaryAttribute)
        .ThenBy(s => s.Name)
        .GroupBy(s => s.PrimaryAttribute);
}

// Display: "Stealth (AS 12)" where AS includes all modifiers
// Use skill.AbilityScore which is already computed with effects
```

### Example 6: Activity Log Message Format (Per CONTEXT.md)

```csharp
// Anonymous action log:
// "STR 7 + [+1,+1,0,-1] = 8 vs TV 10 -> SV -2"
var message = $"{Character.Name}: {AttributeDisplayNames[selectedAttribute]} " +
              $"{attributeValue} + 4dF+ ({(diceRoll >= 0 ? "+" : "")}{diceRoll}) = " +
              $"{rollResult} vs TV {targetValue} -> SV {(sv >= 0 ? "+" : "")}{sv}";

// Skill check log:
// "AS 12 + [+1,0,+1,-1] = 13 vs TV 10 -> SV +3"
var message = $"{Character.Name}: {skill.Name} " +
              $"AS {effectiveAS} + 4dF+ ({(diceRoll >= 0 ? "+" : "")}{diceRoll}) = " +
              $"{totalRoll} vs TV {targetValue} -> SV {(sv >= 0 ? "+" : "")}{sv}";
```

### Example 7: TabCombat Integration

```csharp
// Add to CombatMode enum:
private enum CombatMode
{
    Default, Attack, Defend, TakeDamage, RangedAttack,
    Reload, Unload, Medical, SelectTarget, ActivateImplant,
    AnonymousAction, SkillCheck  // NEW
}

// Add buttons to Actions group in default mode:
<button class="btn combat-tile combat-tile-actions"
        @onclick="StartAnonymousAction"
        disabled="@(!CanAct())"
        title="Attribute-only roll vs TV (1 AP + 1 FAT)">
    <i class="bi bi-dice-5"></i>
    <span>Action</span>
</button>
<button class="btn combat-tile combat-tile-actions"
        @onclick="StartSkillCheck"
        disabled="@(!CanAct())"
        title="Skill check from any skill">
    <i class="bi bi-journal-check"></i>
    <span>Use Skill</span>
</button>

// Add mode rendering in the else-if chain:
else if (combatMode == CombatMode.AnonymousAction)
{
    <AnonymousActionMode Character="Character" Table="Table"
                          OnCancel="ReturnToDefault"
                          OnActionComplete="OnAnonymousActionComplete" />
}
else if (combatMode == CombatMode.SkillCheck)
{
    <SkillCheckMode Character="Character" Table="Table"
                     OnCancel="ReturnToDefault"
                     OnSkillCheckComplete="OnSkillCheckComplete" />
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Skills tab for all skill checks | Combat tab for combat-relevant actions | Phase 34 | Players can roll skill checks without leaving Combat tab |
| No attribute-only rolls | Anonymous actions for quick attribute checks | Phase 34 | Supports GM-called-for attribute checks during combat |

**Note:** The Skills tab (`TabPlaySkills.razor`) will continue to exist for non-combat skill use. Phase 34 adds combat-context access to skill checks, not a replacement.

## Open Questions

1. **Boost interaction with anonymous actions**
   - What we know: Context says "AP bonus works exactly like other skills -- extra AP spent for +1 bonus each"
   - What's unclear: Whether boost applies to the attribute value or is a separate modifier
   - Recommendation: Treat boost as a separate modifier added to attribute value, same as all other actions (this is consistent with how BoostSelector works)

2. **Effect modifiers on anonymous actions**
   - What we know: Effect modifiers use `Character.Effects.GetAbilityScoreModifier(skillName, primaryAttribute, baseAS)` which requires a skill name
   - What's unclear: What skill name to pass for an anonymous attribute-only action
   - Recommendation: For anonymous actions, skip the skill-specific effect modifier but still apply wound penalties and multi-action penalty. The `GetEffectiveAttribute()` already includes attribute-level modifiers from effects and items. Wounds should be applied separately (wounds affect all actions per game rules: -2 AS per wound).

3. **Parry mode interaction**
   - What we know: All other actions in the Actions group end parry mode
   - What's unclear: Whether anonymous actions and skill checks should also end parry mode
   - Recommendation: Yes, end parry mode for both (consistent with existing pattern: "Parry mode ends when the character takes any non-parry action")

## Sources

### Primary (HIGH confidence)
- `Threa/Threa.Client/Components/Pages/GamePlay/TabCombat.razor` - Combat mode switching pattern
- `Threa/Threa.Client/Components/Pages/GamePlay/AttackMode.razor` - Inline panel pattern, cost deduction
- `Threa/Threa.Client/Components/Pages/GamePlay/RangedAttackMode.razor` - Anonymous target, TV input
- `Threa/Threa.Client/Components/Pages/GamePlay/MedicalMode.razor` - Non-attack action mode
- `Threa/Threa.Client/Components/Pages/GamePlay/TabPlaySkills.razor` - Existing skill check flow
- `Threa/Threa.Client/Components/Shared/ActionCostSelector.razor` - Cost toggle component
- `Threa/Threa.Client/Components/Shared/BoostSelector.razor` - Boost spending component
- `GameMechanics/Actions/ActionResolver.cs` - Action resolution engine
- `GameMechanics/Actions/ActionResult.cs` - Result structure with SV, quality
- `GameMechanics/SkillEdit.cs` - Skill business object with AbilityScore computed property
- `GameMechanics/CharacterEdit.cs` - Character with AttributeList, GetEffectiveAttribute()
- `GameMechanics/Dice.cs` - Roll4dFPlus() static method
- `GameMechanics/Messaging/IActivityLogService.cs` - Activity log interface
- `design/ACTIONS.md` - Universal action resolution framework
- `design/ACTION_POINTS.md` - AP cost model
- `.planning/phases/34-new-action-types/34-CONTEXT.md` - User decisions

### Secondary (MEDIUM confidence)
- `.planning/codebase/CONVENTIONS.md` - Coding conventions analysis
- `.planning/codebase/INTEGRATIONS.md` - Messaging/Rx.NET pattern

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All components already exist in codebase
- Architecture: HIGH - Pattern is clearly established with 8+ existing mode components
- Pitfalls: HIGH - Identified from actual code patterns in existing modes
- Code examples: HIGH - Derived directly from existing codebase patterns

**Research date:** 2026-02-12
**Valid until:** 2026-03-12 (stable domain, internal project patterns)
