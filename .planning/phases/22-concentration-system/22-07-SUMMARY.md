---
phase: 22-concentration-system
plan: 07
subsystem: ui
tags: [blazor, radzen, concentration, magazine-reload, sustained-effects]

dependency_graph:
  requires: ["22-02", "22-03", "22-05", "22-06"]
  provides:
    - ConcentrationIndicator component
    - Concentration status display in TabStatus
    - Concentration badge in CharacterStatusCard
    - ProcessConcentrationResult for deferred actions
    - Magazine reload completion handler
    - Sustained break linked effect removal
  affects: []

tech-stack:
  added: []
  patterns:
    - Component code-behind pattern
    - Compact/Full mode component variants
    - DAL-based item updates
    - WeaponAmmoState JSON merge pattern

key-files:
  created:
    - Threa/Threa.Client/Components/Shared/ConcentrationIndicator.razor
    - Threa/Threa.Client/Components/Shared/ConcentrationIndicator.razor.cs
  modified:
    - Threa/Threa/wwwroot/app.css
    - Threa/Threa.Client/Components/Pages/GamePlay/TabStatus.razor
    - Threa/Threa.Client/Components/Shared/CharacterStatusCard.razor
    - GameMechanics/GamePlay/TableCharacterInfo.cs
    - Threa/Threa.Client/Components/Pages/GamePlay/Play.razor

decisions:
  - title: "Compact mode via Radzen-qualified enums"
    rationale: "Radzen enums need full qualification (Radzen.BadgeStyle) in Blazor components"
    alternatives_considered: ["Global using directive", "Inheriting base component"]

  - title: "IsConcentrating in TableCharacterInfo"
    rationale: "CharacterStatusCard uses read-only summary; added IsConcentrating and ConcentrationName properties"
    alternatives_considered: ["Pass full CharacterEdit", "Separate query for concentration"]

  - title: "WeaponAmmoState.LoadedAmmo for magazine reload"
    rationale: "Existing weapon ammo system uses CustomProperties with WeaponAmmoState; LoadedAmmo is the correct field"
    alternatives_considered: ["Adding CurrentRounds to CharacterItem DTO"]

metrics:
  duration: 11 min
  completed: 2026-01-29
---

# Phase 22 Plan 07: ConcentrationIndicator UI Summary

**One-liner:** ConcentrationIndicator component for casting-time progress and sustained drain display, plus LastConcentrationResult processing for magazine reload and linked effect removal.

## Changes Made

### 1. ConcentrationIndicator Component (Tasks 1-2)

Created reusable Blazor component for displaying concentration status:

**ConcentrationIndicator.razor:**
- Badge with "Concentrating" label
- Concentration name display (spell name or effect name)
- Casting-time mode: progress bar (X/Y rounds)
- Sustained mode: linked effect count and drain rate display
- Drop Concentration button with callback
- Compact mode for GM dashboard (badge only)

**ConcentrationIndicator.razor.cs:**
- Character parameter for concentration source
- Compact and ShowDropButton parameters
- OnConcentrationDropped callback
- Computed properties: IsConcentrating, IsCastingTime, IsSustained, ProgressPercent, DrainDisplay

### 2. Component Styling (Task 3)

Added CSS to app.css:
- Warning border accent for concentration indicator
- Compact mode (transparent, borderless)
- Progress bar background styling
- Sustained section flexbox alignment

### 3. TabStatus Integration (Task 4)

Integrated ConcentrationIndicator into player's Status tab:
- Added after health status alerts in Quick Status card
- Full mode with progress/drain details
- Drop Concentration button enabled
- OnCharacterChanged parameter for save callback
- Wired to SaveCharacterAsync in Play.razor

### 4. CharacterStatusCard Integration (Task 5)

Added concentration badge for GM dashboard:
- Added IsConcentrating and ConcentrationName to TableCharacterInfo
- Parse spell name from CustomProperties (BehaviorState equivalent)
- Compact "Conc" badge with tooltip showing concentration name
- ConcentrationStateMinimal file-scoped class for minimal JSON deserialization

### 5. ProcessConcentrationResult (Tasks 6-7)

Implemented concentration result processing in Play.razor:

**ProcessConcentrationResult:**
- Handles MagazineReload, SustainedBreak, SpellCast action types
- Logs messages via AddLogEntry
- Always clears result after processing

**ExecuteMagazineReload:**
- Fetches weapon via CharacterItemDal
- Updates LoadedAmmo via WeaponAmmoState pattern
- Merges ammo state into CustomProperties
- Reloads equipped items after update

**ProcessSustainedBreak:**
- Parses LinkedEffectIds from payload
- Removes effects matching ID or SourceEffectId
- Saves character if effects removed
- SustainedBreakPayload class for deserialization

### 6. Time Advancement Wiring (Task 8)

Connected concentration result processing to time events:
- OnTimeEventReceived: call ProcessConcentrationResult after EndOfRound
- OnTimeSkipReceived: call ProcessConcentrationResult after ProcessTimeSkip
- Handles concentration completion/interruption when time advances

## Decisions Made

1. **Radzen enum qualification**: Used `Radzen.BadgeStyle.Warning` instead of importing Radzen namespace

2. **TableCharacterInfo extension**: Added IsConcentrating/ConcentrationName instead of passing full CharacterEdit

3. **WeaponAmmoState for reload**: Used existing LoadedAmmo field in WeaponAmmoState rather than adding new DTO property

4. **Single-character sustained break**: ProcessSustainedBreak only processes current character; multi-character cleanup requires table-level orchestration

## Commits

1. `c7d9276` - feat(22-07): add ConcentrationIndicator component
2. `c504e77` - style(22-07): add ConcentrationIndicator CSS styling
3. `8cd54c4` - feat(22-07): integrate ConcentrationIndicator into TabStatus
4. `79af540` - feat(22-07): integrate concentration badge into CharacterStatusCard
5. `e199e7a` - feat(22-07): add ProcessConcentrationResult for magazine reload and sustained break
6. `0029b44` - feat(22-07): wire ProcessConcentrationResult to time advancement

## Verification

- [x] Build succeeds: `dotnet build Threa.sln`
- [x] All 69 concentration tests pass
- [x] ConcentrationIndicator displays badge and progress/drain info
- [x] Compact mode shows only badge and name
- [x] TabStatus shows full concentration indicator
- [x] CharacterStatusCard shows compact badge
- [x] ProcessConcentrationResult handles MagazineReload
- [x] ProcessConcentrationResult handles SustainedBreak
- [x] Time advancement triggers concentration result processing

## Next Phase Readiness

**Phase 22 Complete:** All 7 plans executed successfully.

The concentration system is now fully implemented:
- Data layer with ConcentrationState schema (22-01)
- Casting-time concentration lifecycle (22-02)
- Sustained concentration with drain (22-03)
- Character concentration API with Focus checks (22-04)
- ConcentrationBreakDialog for confirmation (22-05)
- Defense integration with auto-break (22-06)
- UI components and result processing (22-07)

---
*Summary created: 2026-01-29*
