---
phase: 22-concentration-system
plan: 08
type: gap_closure
status: complete
subsystem: game-play-ui
tags: [concentration, action-integration, dialog, blazor]

dependency_graph:
  requires: ["22-04", "22-05", "22-07"]
  provides:
    - "Action execution prompts for concentration break confirmation"
    - "Melee attack concentration check integration"
    - "Ranged attack concentration check integration"
    - "Reload action concentration check integration"
  affects: []

tech_stack:
  added: []
  patterns:
    - "Static dialog helper pattern for Radzen dialogs"
    - "Concentration check at action entry point"
    - "Early return pattern for user cancellation"

files:
  created: []
  modified:
    - path: "Threa/Threa.Client/Components/Pages/GamePlay/AttackMode.razor"
      changes: "Added DialogService injection and concentration check in ExecuteAttack"
    - path: "Threa/Threa.Client/Components/Pages/GamePlay/RangedAttackMode.razor"
      changes: "Added Effects.Behaviors using, DialogService injection, and concentration check in ExecuteAttack"
    - path: "Threa/Threa.Client/Components/Pages/GamePlay/ReloadMode.razor"
      changes: "Added Effects.Behaviors using, DialogService injection, and concentration check in ExecuteReload"

decisions: []

metrics:
  duration: "6 min"
  completed: "2026-01-29"
---

# Phase 22 Plan 08: Action Integration with ConcentrationBreakDialog Summary

Concentration checks integrated into all active action execution flows, closing Phase 22 gap.

## What Was Built

Integrated ConcentrationBreakDialog into the action execution flow for three components:

1. **AttackMode.razor** - Melee attack actions
   - Added `@inject Radzen.DialogService DialogService`
   - Added concentration check at start of `ExecuteAttack()`
   - Shows dialog if character has active concentration effect
   - Cancels action if user clicks "Cancel"
   - Breaks concentration if user clicks "Break & Continue"

2. **RangedAttackMode.razor** - Ranged/firearm attack actions
   - Added `@using GameMechanics.Effects.Behaviors`
   - Added `@inject Radzen.DialogService DialogService`
   - Added concentration check at start of `ExecuteAttack()`
   - Same dialog flow as melee attacks

3. **ReloadMode.razor** - Weapon reload actions
   - Added `@using GameMechanics.Effects.Behaviors`
   - Added `@inject Radzen.DialogService DialogService`
   - Added concentration check at start of `ExecuteReload()`
   - Same dialog flow as attacks

## Integration Pattern

All three components use the same pattern:

```csharp
// At start of action execution method
var concentrationEffect = Character.GetConcentrationEffect();
if (concentrationEffect != null)
{
    var state = ConcentrationState.FromJson(concentrationEffect.BehaviorState);
    if (state != null)
    {
        var confirmed = await ConcentrationBreakDialog.ShowAsync(
            DialogService,
            state,
            concentrationEffect.Name);
        if (!confirmed)
            return; // User cancelled - don't execute action

        // User confirmed - break concentration before proceeding
        ConcentrationBehavior.BreakConcentration(Character, "Took [action] action");
    }
}
// Continue with normal action execution...
```

## Gap Closed

This plan closes the Phase 22 gap: **"Taking active actions prompts to break concentration with confirmation"**

The concentration system is now fully integrated:
- Casting-time concentration tracks progress across rounds
- Sustained concentration drains FAT/VIT and maintains linked effects
- Concentration checks occur on damage taken (DefenseResolver)
- Health pool depletion auto-breaks concentration
- **Active actions now prompt before breaking concentration** (this plan)

## Deviations from Plan

None - plan executed exactly as written.

## Commits

| Task | Commit | Description |
|------|--------|-------------|
| 1 | 92ae6ef | feat(22-08): add concentration check to AttackMode.razor |
| 2 | 14b872b | feat(22-08): add concentration check to RangedAttackMode.razor |
| 3 | bd768e4 | feat(22-08): add concentration check to ReloadMode.razor |

## Next Phase Readiness

Phase 22 (Concentration System) is now complete with all gaps closed:
- Data layer foundation (22-01)
- Casting-time lifecycle (22-02)
- Sustained concentration (22-03)
- Character concentration API (22-04)
- ConcentrationBreakDialog component (22-05)
- Defense integration (22-06)
- UI indicators (22-07)
- Action integration (22-08) - this plan

The v1.4 Concentration System milestone is complete.
