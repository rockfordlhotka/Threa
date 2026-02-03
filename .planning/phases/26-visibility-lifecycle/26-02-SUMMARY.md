---
phase: 26-visibility-lifecycle
plan: 02
subsystem: npc-management
tags: [visibility, ui, blazor, real-time]

dependency-graph:
  requires: [26-01]
  provides: [hidden-section-ui, visibility-toggle]
  affects: [26-03]

tech-stack:
  added: []
  patterns: [collapsible-section, toggle-button, real-time-sync]

key-files:
  created:
    - Threa/Threa.Client/Components/Shared/HiddenNpcCard.razor
  modified:
    - GameMechanics/NpcSpawner.cs
    - Threa/Threa.Client/Components/Pages/GamePlay/GmTable.razor
    - Threa/Threa.Client/Components/Shared/CharacterDetailModal.razor

decisions:
  - id: hidden-by-default
    choice: "Spawned NPCs start hidden (VisibleToPlayers = false)"
    rationale: "GMs control when NPCs are revealed for surprise encounters"
  - id: update-type
    choice: "Use CharacterUpdateType.General for visibility changes"
    rationale: "StatusChange not in enum; General covers all refresh scenarios"

metrics:
  duration: 8 min
  completed: 2026-02-03
---

# Phase 26 Plan 02: GM Visibility Toggle Summary

Hidden section UI with collapsible display for surprise encounters, plus visibility toggle from card and modal.

## What Was Built

### 1. NPCs Spawn Hidden by Default
Modified NpcSpawner.cs to set `VisibleToPlayers = false` on newly created NPCs:
```csharp
VisibleToPlayers = false, // Spawned NPCs start hidden for surprise encounters
```

### 2. HiddenNpcCard Component
Created minimized card component for hidden NPCs with:
- Eye-slash icon indicating hidden status
- NPC name display
- Disposition indicator (skull/circle/heart)
- Reveal button to toggle visibility
- Click handler to open CharacterDetailModal

### 3. Hidden Section in GmTable
Added collapsible section above disposition groups:
- Collapsed by default (`showHiddenSection = false`)
- Shows count: "Hidden (N)"
- Chevron toggle icon
- Grid layout (2-3 columns) with HiddenNpcCard components
- RevealNpc method updates character and publishes real-time sync

### 4. Visibility Toggle in CharacterDetailModal
Added toggle button in modal header for NPCs:
- Shows "Hide" or "Reveal" based on current state
- Warning/success button styling
- Instant toggle (no confirmation)
- Publishes CharacterUpdateMessage for real-time sync

## Key Code Changes

**NpcSpawner.cs** (line 184):
```csharp
VisibleToPlayers = false, // Spawned NPCs start hidden for surprise encounters
```

**GmTable.razor** - Hidden NPCs filter:
```csharp
private IEnumerable<TableCharacterInfo> hiddenNpcs =>
    tableNpcs.Where(c => !c.VisibleToPlayers);
```

**CharacterDetailModal.razor** - Toggle method:
```csharp
private async Task ToggleVisibility()
{
    character.VisibleToPlayers = !character.VisibleToPlayers;
    await characterPortal.UpdateAsync(character);
    await TimeEventPublisher.PublishCharacterUpdateAsync(...);
}
```

## Commits

| Hash | Description |
|------|-------------|
| c950112 | feat(26-02): spawn NPCs as hidden by default |
| 0fc4b24 | feat(26-02): add hidden section and HiddenNpcCard component |
| 09e790e | feat(26-02): add visibility toggle to CharacterDetailModal |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Changed CharacterUpdateType from StatusChange to General**
- **Found during:** Task 2 and Task 3
- **Issue:** CharacterUpdateType.StatusChange does not exist in the enum
- **Fix:** Used CharacterUpdateType.General instead
- **Files modified:** GmTable.razor, CharacterDetailModal.razor
- **Commit:** 0fc4b24, 09e790e

## Verification Results

All verification criteria met:
- [x] Solution builds without errors
- [x] NpcSpawner sets VisibleToPlayers = false
- [x] GmTable shows Hidden section when hidden NPCs exist
- [x] Hidden section is collapsed by default
- [x] HiddenNpcCard displays NPC name, disposition icon, and reveal button
- [x] Clicking reveal button moves NPC to disposition group
- [x] CharacterDetailModal shows visibility toggle for NPCs
- [x] Toggle is instant (no confirmation)

## Next Phase Readiness

Plan 26-02 provides the visibility toggle workflow. Plan 26-03 (NPC Lifecycle) can now implement archive/delete functionality, building on the visibility infrastructure established here.

---

*Phase: 26-visibility-lifecycle*
*Plan: 02*
*Completed: 2026-02-03*
