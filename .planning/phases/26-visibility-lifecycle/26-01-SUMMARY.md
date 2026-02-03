---
phase: 26
plan: 01
subsystem: data-model
tags: [npc, visibility, archive, dto, csla, dal]

dependency-graph:
  requires: [phase-25]
  provides: [visibility-filtering, archive-support, archived-npc-query]
  affects: [26-02, 26-03]

tech-stack:
  added: []
  patterns: [soft-delete-archive]

key-files:
  created: []
  modified:
    - GameMechanics/GamePlay/TableCharacterInfo.cs
    - Threa.Dal/Dto/Character.cs
    - GameMechanics/CharacterEdit.cs
    - Threa.Dal/ICharacterDal.cs
    - Threa.Dal.SqlLite/CharacterDal.cs

decisions:
  - "VisibleToPlayers populated from Character DTO in TableCharacterInfo Fetch"
  - "IsArchived property added to both DTO and CharacterEdit for full round-trip"
  - "GetArchivedNpcsAsync uses memory filtering pattern matching GetNpcTemplatesAsync"
  - "MockDb DAL implementation skipped - folder exists but has no source files"

metrics:
  duration: 10 min
  completed: 2026-02-03
---

# Phase 26 Plan 01: Data Model Extensions Summary

Extended data model to support NPC visibility tracking and archive functionality for Phase 26 visibility and lifecycle features.

## One-liner

Added VisibleToPlayers to TableCharacterInfo and IsArchived property chain (DTO -> CharacterEdit -> DAL) for NPC visibility filtering and archive support.

## Commit Log

| Commit | Type | Description |
|--------|------|-------------|
| 157ded3 | feat | Add VisibleToPlayers to TableCharacterInfo |
| 366ace1 | feat | Add IsArchived to Character DTO and CharacterEdit |
| 6f2e730 | feat | Add GetArchivedNpcsAsync to DAL |

## What Was Built

### Task 1: VisibleToPlayers on TableCharacterInfo

Added CSLA property to TableCharacterInfo for dashboard filtering:

```csharp
public static readonly PropertyInfo<bool> VisibleToPlayersProperty = RegisterProperty<bool>(nameof(VisibleToPlayers));
/// <summary>
/// Whether this NPC is visible to players (false = hidden for surprise).
/// Always true for PCs.
/// </summary>
public bool VisibleToPlayers
{
    get => GetProperty(VisibleToPlayersProperty);
    private set => LoadProperty(VisibleToPlayersProperty, value);
}
```

Populated from Character DTO in Fetch method alongside other NPC properties.

### Task 2: IsArchived Property Chain

Added IsArchived to both layers:

**Character DTO (Threa.Dal/Dto/Character.cs):**
```csharp
/// <summary>
/// Whether this character is archived (dismissed but not deleted).
/// Archived characters don't appear in active lists but can be restored.
/// </summary>
public bool IsArchived { get; set; }
```

**CharacterEdit (GameMechanics/CharacterEdit.cs):**
```csharp
public static readonly PropertyInfo<bool> IsArchivedProperty = RegisterProperty<bool>(nameof(IsArchived));
[Display(Name = "Is Archived")]
public bool IsArchived
{
    get => GetProperty(IsArchivedProperty);
    set => SetProperty(IsArchivedProperty, value);
}
```

The property uses SetProperty (not LoadProperty) to enable dirty tracking when archived status changes.

### Task 3: GetArchivedNpcsAsync DAL Method

Added interface method and SQLite implementation:

**ICharacterDal interface:**
```csharp
/// <summary>
/// Gets archived NPCs for the archive browser.
/// </summary>
Task<List<Character>> GetArchivedNpcsAsync();
```

**SqliteCharacterDal implementation:**
```csharp
public async Task<List<Character>> GetArchivedNpcsAsync()
{
    try
    {
        // Reuse existing fetch, filter in memory for JSON storage
        var all = await GetAllCharactersAsync();
        return all.Where(c => c.IsNpc && !c.IsTemplate && c.IsArchived).ToList();
    }
    catch (Exception ex)
    {
        throw new OperationFailedException("Error getting archived NPCs", ex);
    }
}
```

## Deviations from Plan

### Adjusted Implementation

**1. MockDb DAL implementation skipped**
- **Found during:** Task 3 planning
- **Issue:** Plan referenced Threa.Dal.MockDb/MockCharacterDal.cs but MockDb folder exists with only build output (no source files)
- **Resolution:** Implemented only SqliteCharacterDal which is the active DAL implementation
- **Impact:** None - MockDb is not used in the project

## Decisions Made

| Decision | Rationale |
|----------|-----------|
| VisibleToPlayers in Fetch after SourceTemplateName | Groups all NPC-specific properties together |
| IsArchived uses SetProperty | Enables dirty tracking for archive status changes |
| Memory filtering for GetArchivedNpcsAsync | Matches existing pattern in GetNpcTemplatesAsync for JSON storage |

## Testing Notes

- All affected projects build successfully
- Full solution build blocked by file locking (running Threa process) but code compiles correctly
- No unit tests added - plan specified build verification only
- Integration testing will occur in later plans when UI is added

## Next Phase Readiness

Ready for Plan 26-02 (GM visibility toggle) and Plan 26-03 (archive browser UI):
- VisibleToPlayers available on TableCharacterInfo for dashboard filtering
- IsArchived property chain complete for soft-delete pattern
- GetArchivedNpcsAsync available for archive browser data retrieval

## Files Modified

| File | Change |
|------|--------|
| GameMechanics/GamePlay/TableCharacterInfo.cs | Added VisibleToPlayers property and Fetch population |
| Threa.Dal/Dto/Character.cs | Added IsArchived property |
| GameMechanics/CharacterEdit.cs | Added IsArchived CSLA property |
| Threa.Dal/ICharacterDal.cs | Added GetArchivedNpcsAsync method |
| Threa.Dal.SqlLite/CharacterDal.cs | Implemented GetArchivedNpcsAsync |
