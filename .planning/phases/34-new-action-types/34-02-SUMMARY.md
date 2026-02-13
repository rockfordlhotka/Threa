---
phase: 34
plan: 02
subsystem: combat-ui
tags: [blazor, combat, skill-check, modal, inline-panel]
dependency-graph:
  requires: [34-01]
  provides: [skill-check-mode, combat-skill-picker-modal, combat-tab-use-skill-button]
  affects: []
tech-stack:
  added: []
  patterns: [modal-then-inline-panel, radzen-dialog-skill-picker]
file-tracking:
  key-files:
    created:
      - Threa/Threa.Client/Components/Pages/GamePlay/CombatSkillPickerModal.razor
      - Threa/Threa.Client/Components/Pages/GamePlay/SkillCheckMode.razor
    modified:
      - Threa/Threa.Client/Components/Pages/GamePlay/TabCombat.razor
decisions: []
metrics:
  duration: 5 min
  completed: 2026-02-13
---

# Phase 34 Plan 02: Skill Check Mode Summary

**One-liner:** Modal skill picker grouped by attribute with inline roll panel using pre-computed AbilityScore, integrated into Combat tab Actions group

## What Was Done

### Task 1: Create CombatSkillPickerModal and SkillCheckMode components (281c3c6)
Created two new Blazor components:

**CombatSkillPickerModal.razor (91 lines):**
- Modal component opened via Radzen DialogService.OpenAsync
- Shows all character skills grouped by PrimaryAttribute (STR, DEX, END, INT, ITT, WIL, PHY)
- Each attribute group has a card header with display name (e.g., "Physicality (STR)")
- Each skill row shows name + "AS {skill.AbilityScore}" badge using the pre-computed property
- Search/filter input at top filters by skill name (case-insensitive)
- Clicking a skill calls DialogService.Close(skill) to return the SkillEdit object
- Cancel button calls DialogService.Close() with no argument
- Scrollable skill list (max-height 460px) for characters with many skills

**SkillCheckMode.razor (489 lines):**
- Inline panel following the exact AnonymousActionMode pattern from 34-01
- Opens CombatSkillPickerModal in OnAfterRenderAsync(firstRender) with modalOpened guard
- If modal cancelled, invokes OnCancel to return to default mode
- If skill selected, shows setup phase with 4 input cards:
  1. Selected Skill card with name, attribute, AS, and "Change" button to re-open modal
  2. ActionCostSelector (1AP+1FAT or 2AP)
  3. Target Value numeric input
  4. BoostSelector with available AP/FAT calculated after base cost deduction
- Summary card shows skill name, AS, TV, cost, boost; footer has Roll Check button
- Resource summary card shows current AP/FAT/VIT
- ExecuteSkillCheck flow: concentration break check, parry mode end, cost deduction with ActionsTakenThisRound increment, wound penalty (-2 per wound), multi-action penalty (-2 per prior action), 4dF+ roll, SV calculation, CSLA save
- Uses selectedSkill.AbilityScore directly (already includes attribute + skill level - 5 + item bonuses + effect modifiers)
- Result phase with full breakdown table and Done button
- Result message formatted per CONTEXT.md: "{Name}: {Skill} AS {effectiveAS} + 4dF+ ({diceRoll}) = {total} vs TV {tv} -> SV {sv}"

### Task 2: Integrate SkillCheckMode into TabCombat (cebfc19)
Wired the new components into the Combat tab's mode system:

- Added `SkillCheck` to `CombatMode` enum (after `AnonymousAction`)
- Added "Use Skill" button tile with `bi-journal-check` icon in the Actions group
- Added `else if (combatMode == CombatMode.SkillCheck)` rendering block with SkillCheckMode component
- Added `StartSkillCheck()` method that ends parry mode and sets combat mode
- Added `OnSkillCheckComplete()` handler that logs result to activity feed, returns to default mode, and notifies parent of character changes

## Deviations from Plan

None - plan executed exactly as written.

## Verification Results

| Check | Status |
|-------|--------|
| `dotnet build Threa.sln` compiles | Pass (0 errors) |
| CombatSkillPickerModal.razor exists and groups skills by PrimaryAttribute | Pass |
| SkillCheckMode.razor exists with modal-first flow, then inline panel | Pass |
| TabCombat has SkillCheck in CombatMode enum | Pass |
| TabCombat has "Use Skill" button in Actions group | Pass |
| TabCombat renders SkillCheckMode when in SkillCheck mode | Pass |
| Uses skill.AbilityScore (pre-computed with modifiers) for AS | Pass |
| Does NOT trigger attack workflow - purely standalone roll | Pass |
| Deducts costs, increments ActionsTakenThisRound, saves character | Pass |
| Result message follows CONTEXT.md format | Pass |

## Commits

| Hash | Message |
|------|---------|
| 281c3c6 | feat(34-02): create CombatSkillPickerModal and SkillCheckMode components |
| cebfc19 | feat(34-02): integrate SkillCheckMode into Combat tab |

## Next Phase Readiness

Phase 34 (New Action Types) is now complete. Both plans delivered:
- Plan 01: AnonymousActionMode (attribute-only roll)
- Plan 02: SkillCheckMode (modal skill picker + skill roll)

No blockers for future phases.
