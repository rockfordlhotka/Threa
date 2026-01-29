---
phase: 22-concentration-system
plan: 05
subsystem: ui-components
tags: [concentration, dialog, radzen, blazor]

dependency-graph:
  requires: ["22-04"]
  provides: ["ConcentrationBreakDialog component", "ShowAsync static helper"]
  affects: ["22-07", "action-integration"]

tech-stack:
  added: []
  patterns: ["Radzen DialogService", "static ShowAsync helper pattern"]

key-files:
  created:
    - Threa/Threa.Client/Components/Shared/ConcentrationBreakDialog.razor
  modified:
    - Threa/Threa/wwwroot/app.css

decisions:
  - id: conc-05-01
    choice: "Inline @code block instead of separate .razor.cs file"
    rationale: "Follows existing codebase pattern (EffectFormModal.razor)"
  - id: conc-05-02
    choice: "Bootstrap icons (bi-*) instead of Radzen icons (rzi-*)"
    rationale: "Project uses Bootstrap icons throughout; consistent with other components"

metrics:
  duration: 2 min
  completed: 2026-01-29
---

# Phase 22 Plan 05: ConcentrationBreakDialog Summary

**One-liner:** Radzen dialog component for confirming concentration break with type-specific details display

## What Was Built

### ConcentrationBreakDialog.razor

A confirmation dialog component that warns players before breaking concentration:

**Features:**
1. **Concentration name display** - Shows spell name, effect name, or concentration type
2. **Casting-time details** - Progress indicator (X/Y rounds) with interruption warning
3. **Sustained details** - Linked effect count and FAT/VIT drain rate
4. **Action buttons** - Cancel (returns false) and Break & Continue (returns true)
5. **Static ShowAsync helper** - Simplified invocation with ConcentrationState parameter

**Usage example:**
```csharp
// Check if user wants to break concentration
var confirmed = await ConcentrationBreakDialog.ShowAsync(
    DialogService,
    concentrationState,
    "Reloading Magazine");

if (confirmed)
{
    character.BreakConcentration();
    // Execute the action that would break concentration
}
```

### CSS Styling

Added `.concentration-break-dialog` styles to app.css:
- Minimum width 350px for consistent sizing
- Warning-colored left border on details card
- Proper text colors using theme variables

## Commits

| Hash | Description |
|------|-------------|
| 42356c6 | feat(22-05): add ConcentrationBreakDialog component |

## Deviations from Plan

### Structural Deviation

**Plan specified:** Separate ConcentrationBreakDialog.razor and ConcentrationBreakDialog.razor.cs files (Tasks 1-4)

**Actual implementation:** Single ConcentrationBreakDialog.razor file with inline @code block

**Rationale:** This matches the existing codebase pattern. EffectFormModal.razor, WoundFormModal.razor, and other modal components in the Shared folder use inline @code blocks. Following established conventions improves maintainability.

**Result:** All 4 tasks completed in a single file - component UI, parameters, button handlers, and static ShowAsync helper are all present.

## Verification

1. **Build succeeds:** `dotnet build Threa.sln` passes (0 errors)
2. **Component contains required elements:**
   - RadzenButton with "Cancel" text calling DialogService.Close(false)
   - RadzenButton with "Break & Continue" text calling DialogService.Close(true)
   - Static ShowAsync helper method accepting DialogService and ConcentrationState
3. **CSS styling present:** `.concentration-break-dialog` class in app.css

## Next Phase Readiness

**Ready for 22-06:** Defense system integration

The ConcentrationBreakDialog component is complete and ready to be invoked from action/defense code when concentration would be broken. The static ShowAsync helper simplifies integration.

**Ready for 22-07:** UI components

The dialog pattern established here (static ShowAsync with ConcentrationState parameter) can be referenced when building the concentration indicator UI.

---

*Summary created: 2026-01-29*
