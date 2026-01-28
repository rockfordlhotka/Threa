---
phase: 18-wound-management
plan: 02
subsystem: effects
tags: [wounds, effects, gm-tools, ui-integration, blazor]

dependency_graph:
  requires: [18-01]
  provides: [wound-badges, wound-prompt, wound-penalty-display]
  affects: [19-effect-management]

tech_stack:
  added: []
  patterns: [tooltip-display, prompt-workflow, real-penalty-aggregation]

key_files:
  created: []
  modified:
    - Threa/Threa.Client/Components/Shared/CharacterDetailModal.razor
    - Threa/Threa.Client/Components/Shared/CharacterDetailGmActions.razor
    - Threa/Threa.Client/Components/Pages/GamePlay/TabStatus.razor

decisions:
  - key: wound-badge-colors
    choice: Severity-based badge colors (Critical=danger, Severe=orange, Moderate=warning, Minor=secondary)
    reason: Consistent with severity visual hierarchy established in Plan 01
  - key: wound-prompt-optional
    choice: "Apply + Add Wound" is optional alternative to "Apply Anyway"
    reason: GM may choose to apply damage without creating wound for various game reasons
  - key: penalty-aggregation
    choice: Sum actual modifiers from GetAbilityScoreModifiers instead of simple count formula
    reason: Custom AS penalties from severity-based wounds need proper display

metrics:
  duration: 4 min
  completed: 2026-01-28
---

# Phase 18 Plan 02: Wound Display and Prompts Summary

Integrated wound display and prompts into existing UI for complete wound management visibility.

## Objective Achievement

Enhanced CharacterDetailModal header with individual wound badges showing tooltips, added VIT damage wound prompt in damage flow, and fixed AS penalty display to support custom wound penalties.

## Tasks Completed

| Task | Name | Commit | Files |
|------|------|--------|-------|
| 1 | Enhance Header with Individual Wound Badges | fecd381 | CharacterDetailModal.razor |
| 2 | Add VIT Damage Wound Prompt | eccab8a | CharacterDetailGmActions.razor |
| 3 | Verify AS Penalty Display | 97bed1b | TabStatus.razor |

## Implementation Details

### Individual Wound Badges (Task 1)
Replaced single wound count badge in CharacterDetailModal header with per-wound badges:
- Each wound shows as separate badge with location abbreviation (L/R for Left/Right)
- Severity-based badge colors: Critical (danger), Severe (orange), Moderate (warning), Minor (secondary)
- Hover tooltip shows: "{Severity} wound at {Location}: {Description}"
- Added WoundState using directive for BehaviorState deserialization

### VIT Damage Wound Prompt (Task 2)
Added workflow integration when VIT damage exceeds capacity:
- New `woundDamageExcess` state tracks excess VIT damage
- VIT overflow warning now shows additional "Apply + Add Wound" button
- `ApplyAndAddWound` method applies damage then opens WoundFormModal
- Prompt is optional - GM can still "Apply Anyway" or "Cancel"
- State cleared appropriately in SetMode, CancelApply, and finally block

### AS Penalty Display Fix (Task 3)
Updated TabStatus.razor to properly display custom wound penalties:
- Changed `TotalWoundPenalty` from simple `TotalWoundCount * PenaltyPerWound` calculation
- Now sums actual modifiers from each wound's `GetAbilityScoreModifiers()` method
- Properly displays custom AS penalties from severity-based wounds (Plan 01)
- Updated description text from "-2 per wound" to "Applied to all ability scores"

## Key Patterns

### Wound Badge Display
```razor
@foreach (var wound in character.Effects.Where(e => e.EffectType == EffectType.Wound))
{
    var woundState = WoundState.Deserialize(wound.BehaviorState);
    var severity = woundState?.Severity ?? "Unknown";
    // Badge with severity-based styling and tooltip
}
```

### VIT Overflow Prompt
```razor
@if (healthMode == HealthMode.Damage && pendingPool == "VIT" && woundDamageExcess > 0)
{
    <button class="btn btn-danger btn-sm" @onclick="ApplyAndAddWound">
        <i class="bi bi-bandaid me-1"></i>Apply + Add Wound
    </button>
}
```

### Proper Penalty Aggregation
```csharp
private int TotalWoundPenalty => woundEffects.Sum(wound =>
{
    var modifiers = wound.GetAbilityScoreModifiers("AnySkill", "AnyAttr", 0);
    return modifiers.Sum(m => m.Value);
});
```

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed TotalWoundPenalty calculation**
- **Found during:** Task 3
- **Issue:** TabStatus.razor used simple formula `TotalWoundCount * PenaltyPerWound` which doesn't support custom AS penalties from severity-based wounds
- **Fix:** Changed to aggregate actual modifiers from GetAbilityScoreModifiers() for each wound
- **Files modified:** TabStatus.razor
- **Commit:** 97bed1b

## Decisions Made

1. **Badge color mapping**: Used Bootstrap color classes matching severity hierarchy (Critical=danger is most alarming, Minor=secondary is subdued).

2. **Prompt button placement**: Added "Apply + Add Wound" between "Apply Anyway" and "Cancel" for logical flow.

3. **Penalty aggregation method**: Used GetAbilityScoreModifiers with dummy parameters since wound penalties apply to all skills/attributes equally.

## Next Phase Readiness

Phase 18 (Wound Management) is now complete:
- WoundState supports custom severity and damage rates (Plan 01)
- UI enables full CRUD operations on wounds (Plan 01)
- Individual wound badges with tooltips in modal header (Plan 02)
- VIT damage prompt for wound creation (Plan 02)
- Accurate AS penalty display with custom penalties (Plan 02)
- Real-time updates work via CharacterUpdateMessage

Ready to proceed to Phase 19 (Effect Management).

No blockers identified.
