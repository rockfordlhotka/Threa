---
phase: 27-time-combat-integration
plan: 01
subsystem: Combat/Targeting
tags: [targeting, npc, visibility, disposition]

requires:
  - 25-01 (TableCharacterInfo NPC fields)
  - 26-02 (NPC visibility filtering)
provides:
  - NPCs targetable by player combat actions
  - Disposition-grouped target selection
affects:
  - 27-02 (time advancement with combat)

tech-stack:
  added: []
  patterns:
    - CSLA portal for target list (TableCharacterList)
    - Visibility filtering for player-facing views

key-files:
  created: []
  modified:
    - Threa/Threa.Client/Components/Pages/GamePlay/Play.razor
    - Threa/Threa.Client/Components/Pages/GamePlay/Targeting/TargetSelectionModal.razor

decisions:
  - Tasks combined into single commit due to interdependency

metrics:
  duration: 6 min
  completed: 2026-02-03
---

# Phase 27 Plan 01: NPC Targeting Integration Summary

**One-liner:** Players can now target visible NPCs in combat, with targets grouped by disposition (Hostile/Neutral/Friendly).

## What Was Done

### Play.razor Changes
- Added `IDataPortal<TableCharacterList>` injection for CSLA-based target loading
- Changed `tableCharacters` field from DTO list to `List<TableCharacterInfo>`
- Updated `LoadTableCharactersAsync()` to use CSLA portal fetch
- Modified `GetAvailableTargets()` to:
  - Filter out hidden NPCs (`!c.IsNpc || c.VisibleToPlayers`)
  - Populate `IsNPC` and `Disposition` fields
  - Set `PlayerName` to null for NPCs, "Player" for PCs

### TargetSelectionModal.razor Changes
- Added `Disposition` property to `TargetInfo` class
- Replaced simple foreach with grouped display:
  - NPCs shown first, grouped by disposition (Hostile > Neutral > Friendly)
  - Hostile: skull icon (red)
  - Neutral: circle icon (secondary)
  - Friendly: heart icon (green)
  - Divider between NPCs and PCs section
  - PCs shown in separate section with player names

## Decisions Made

| Decision | Rationale |
|----------|-----------|
| Tasks combined into single commit | Play.razor Disposition usage requires TargetSelectionModal's TargetInfo property - interdependent |

## Deviations from Plan

None - plan executed exactly as written.

## Verification

- Build succeeds: `dotnet build Threa.Client.csproj` passes
- Code follows existing patterns (CSLA portal, Bootstrap styling)
- Icons match GM dashboard disposition styling (skull/circle/heart)

## Files Changed

| File | Change |
|------|--------|
| Play.razor | Use TableCharacterList via CSLA portal; filter hidden NPCs |
| TargetSelectionModal.razor | Add Disposition property; grouped display by disposition |

## Commits

| Hash | Type | Description |
|------|------|-------------|
| bfd4e5d | feat | Integrate NPCs into targeting system |

## Next Phase Readiness

Ready for Plan 27-02 (time advancement integration). NPC targeting is now functional - visible NPCs appear in target list with disposition grouping.
