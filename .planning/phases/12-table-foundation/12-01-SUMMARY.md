---
phase: 12-table-foundation
plan: 01
subsystem: gameplay
tags: [blazor, csla, campaign, theme]
dependency-graph:
  requires: []
  provides: [campaign-creation-page, tableinfo-theme-display]
  affects: [12-02-campaign-list]
tech-stack:
  added: []
  patterns: [theme-preview, epoch-time-selection]
key-files:
  created:
    - Threa/Threa.Client/Components/Pages/GameMaster/CampaignCreate.razor
  modified:
    - GameMechanics/GamePlay/TableInfo.cs
decisions:
  - key: theme-preview-method
    choice: Apply theme via JS interop during preview
    reason: Provides real-time visual feedback using existing theme infrastructure
  - key: epoch-default-value
    choice: "13569465600 for Sci-Fi (Year 2400)"
    reason: Provides meaningful far-future default as specified in CONTEXT.md
metrics:
  duration: 3 min
  completed: 2026-01-27
---

# Phase 12 Plan 01: Campaign Creation Page Summary

Extended TableInfo with Theme and StartTimeSeconds properties for list display, and created dedicated campaign creation page at /campaigns/create.

## Completed Tasks

| # | Task | Commit | Key Changes |
|---|------|--------|-------------|
| 1 | Extend TableInfo with Theme and StartTimeSeconds | afeee2f | Added Theme (string) and StartTimeSeconds (long) CSLA properties with FetchChild mapping |
| 2 | Create CampaignCreate.razor page | b4c5ffe | Created /campaigns/create with name input, theme dropdown, preview card, and epoch time input |

## Technical Implementation

### TableInfo Extension

Added two new CSLA properties following the existing pattern:

```csharp
public static readonly PropertyInfo<string> ThemeProperty = RegisterProperty<string>(nameof(Theme));
public string Theme
{
    get => GetProperty(ThemeProperty);
    private set => LoadProperty(ThemeProperty, value);
}

public static readonly PropertyInfo<long> StartTimeSecondsProperty = RegisterProperty<long>(nameof(StartTimeSeconds));
public long StartTimeSeconds
{
    get => GetProperty(StartTimeSecondsProperty);
    private set => LoadProperty(StartTimeSecondsProperty, value);
}
```

FetchChild maps from GameTable DTO with null-coalescing for Theme: `Theme = table.Theme ?? "fantasy";`

### Campaign Creation Page Features

- **Name input**: Real-time validation with `is-invalid` class when empty after interaction
- **Theme dropdown**: Fantasy (Arcanum) and Sci-Fi (Neon Circuit) options
- **Theme preview card**: Uses CSS variables to show theme styling in real-time
- **Epoch time input**: Accepts full long range including negatives
- **Theme-specific defaults**: 0 for Fantasy, 13569465600 for Sci-Fi (Year 2400)
- **Original theme restoration**: Stores original theme on load, restores on cancel
- **Navigation**: Creates table and navigates to `/gamemaster/table/{id}` on success

### JS Interop Pattern

Uses existing `threaTheme.apply` and `threaTheme.get` methods from GmTable.razor:

```csharp
originalTheme = await JS.InvokeAsync<string>("threaTheme.get");
await JS.InvokeVoidAsync("threaTheme.apply", selectedTheme);
```

## Decisions Made

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Theme preview method | Live JS interop | Provides immediate visual feedback using existing infrastructure |
| Sci-Fi default epoch | 13569465600 | Year 2400 equivalent (2400-1970)*365.25*24*60*60 as specified |
| Validation timing | On interaction | Shows error only after user has interacted (hasInteracted flag) |

## Deviations from Plan

None - plan executed exactly as written.

## Verification

- [x] `dotnet build GameMechanics/GameMechanics.csproj` - Success
- [x] `dotnet build Threa/Threa/Threa.csproj` - Success
- [x] `dotnet build Threa.sln` - Success (pre-existing warnings only)
- [x] TableInfo has Theme and StartTimeSeconds properties
- [x] CampaignCreate.razor exists at /campaigns/create

## Next Phase Readiness

Plan 12-01 provides foundation for 12-02 (campaign list):
- TableInfo.Theme and TableInfo.StartTimeSeconds available for list display
- Theme indicator styling pattern established in preview card

---

*Completed: 2026-01-27 | Duration: 3 min*
