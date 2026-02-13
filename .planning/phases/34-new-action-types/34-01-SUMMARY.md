---
phase: 34
plan: 01
subsystem: combat-ui
tags: [blazor, combat, anonymous-action, attribute-roll, inline-panel]
dependency-graph:
  requires: [33]
  provides: [anonymous-action-mode, combat-tab-action-button]
  affects: [34-02]
tech-stack:
  added: []
  patterns: [combat-mode-component, attribute-only-roll]
file-tracking:
  key-files:
    created:
      - Threa/Threa.Client/Components/Pages/GamePlay/AnonymousActionMode.razor
    modified:
      - Threa/Threa.Client/Components/Pages/GamePlay/TabCombat.razor
decisions: []
metrics:
  duration: 5 min
  completed: 2026-02-13
---

# Phase 34 Plan 01: Anonymous Action Mode Summary

**One-liner:** Attribute-only roll inline panel with cost/boost/concentration handling integrated into Combat tab Actions group

## What Was Done

### Task 1: Create AnonymousActionMode.razor component (c1cf0d9)
Created a new Blazor component following the established combat mode pattern (matching MedicalMode, AttackMode structure). The component provides:

- **Setup phase** with 4 input cards (attribute dropdown, cost selector, TV input, boost selector) and a summary card with Roll Check button
- **Attribute dropdown** with all 7 attributes displayed as "Physicality (STR)", "Dodge (DEX)", etc.
- **ActionCostSelector** for 1AP+1FAT or 2AP cost choice
- **BoostSelector** with available AP/FAT calculated after base cost deduction
- **CanRoll()** validation checking character state, attribute selection, TV > 0, and sufficient resources for base cost + boost
- **ExecuteAction** flow: concentration break check, parry mode end, cost deduction with ActionsTakenThisRound increment, wound penalty (-2 per wound), multi-action penalty (-2 per prior action), 4dF+ roll, SV calculation
- **Result phase** with full breakdown table showing attribute value, boost, wound penalty, multi-action penalty, effective value, dice roll, total, TV, and SV
- **Result message** formatted per CONTEXT.md: `"{Name}: {Attribute} {effectiveValue} + 4dF+ ({diceRoll}) = {total} vs TV {tv} -> SV {sv}"`
- Character saved via CSLA ISavable after rolling

### Task 2: Integrate AnonymousActionMode into TabCombat (f8d1c16)
Wired the new component into the Combat tab's mode system:

- Added `AnonymousAction` to `CombatMode` enum
- Added "Action" button tile with `bi-dice-5` icon in the Actions group
- Added `else if (combatMode == CombatMode.AnonymousAction)` rendering block with AnonymousActionMode component
- Added `StartAnonymousAction()` method that ends parry mode and sets combat mode
- Added `OnAnonymousActionComplete()` handler that logs result to activity feed, returns to default mode, and notifies parent of character changes

## Deviations from Plan

None - plan executed exactly as written.

## Verification Results

| Check | Status |
|-------|--------|
| `dotnet build Threa.sln` compiles | Pass (0 errors) |
| AnonymousActionMode.razor exists with setup + result phases | Pass |
| TabCombat has AnonymousAction in CombatMode enum | Pass |
| TabCombat has "Action" button tile in Actions group | Pass |
| TabCombat renders AnonymousActionMode when in AnonymousAction mode | Pass |
| Uses GetEffectiveAttribute (not AS formula) | Pass |
| Deducts costs, increments ActionsTakenThisRound, saves character | Pass |
| Checks concentration before rolling | Pass |
| Handles boost via BoostSelector | Pass |
| Result message follows CONTEXT.md format | Pass |

## Commits

| Hash | Message |
|------|---------|
| c1cf0d9 | feat(34-01): create AnonymousActionMode inline panel component |
| f8d1c16 | feat(34-01): integrate AnonymousActionMode into Combat tab |

## Next Phase Readiness

Plan 34-02 (Use Skill mode) can proceed. The anonymous action pattern establishes the inline panel template that the skill check mode will follow. No blockers.
