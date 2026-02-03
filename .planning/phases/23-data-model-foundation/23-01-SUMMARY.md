---
phase: 23-data-model-foundation
plan: 01
subsystem: data-model
tags: [npc, character, dto, csla, business-object]
dependency-graph:
  requires: []
  provides: [npc-character-flags]
  affects: [23-02, 24-gm-ui, 25-template-spawning]
tech-stack:
  added: []
  patterns: [csla-property-info]
key-files:
  created: []
  modified:
    - Threa.Dal/Dto/Character.cs
    - GameMechanics/CharacterEdit.cs
decisions:
  - key: visible-default
    choice: true
    reason: Backward compatibility - existing characters remain visible
metrics:
  duration: 4min
  completed: 2026-02-01
---

# Phase 23 Plan 01: Add NPC and Template Flags Summary

**One-liner:** Added IsNpc, IsTemplate, and VisibleToPlayers boolean flags to Character DTO and CharacterEdit business object using CSLA PropertyInfo pattern.

## What Was Built

Three new boolean properties added to the character data model to support NPC management:

| Property | Purpose | Default |
|----------|---------|---------|
| `IsNpc` | Identifies non-player characters | false |
| `IsTemplate` | Identifies template characters for spawning | false |
| `VisibleToPlayers` | Controls player visibility of NPCs | true |

### Character DTO (Threa.Dal/Dto/Character.cs)

```csharp
public bool IsNpc { get; set; }
public bool IsTemplate { get; set; }
public bool VisibleToPlayers { get; set; } = true;
```

### CharacterEdit Business Object (GameMechanics/CharacterEdit.cs)

Full CSLA PropertyInfo pattern implementation:

```csharp
public static readonly PropertyInfo<bool> IsNpcProperty = RegisterProperty<bool>(nameof(IsNpc));
[Display(Name = "Is NPC")]
public bool IsNpc
{
    get => GetProperty(IsNpcProperty);
    set => SetProperty(IsNpcProperty, value);
}
// ... similar for IsTemplate and VisibleToPlayers
```

Properties use `SetProperty` (not `LoadProperty`) for dirty tracking since these are user-editable fields.

## Decisions Made

| Decision | Choice | Rationale |
|----------|--------|-----------|
| VisibleToPlayers default | `true` | Backward compatibility - existing characters remain visible; templates in GM library browsable; NPCs hidden explicitly for surprise encounters |
| Property setter | `SetProperty` | These are user-editable properties requiring dirty tracking |

## Verification

- Threa.Dal.csproj builds successfully
- GameMechanics.csproj builds successfully
- Full solution builds with 0 errors
- Properties follow existing CSLA patterns in codebase

## Deviations from Plan

None - plan executed exactly as written.

## Commits

| Task | Commit | Description |
|------|--------|-------------|
| 1 | `6ff34c2` | Add NPC flag properties to Character DTO |
| 2 | `98d4620` | Add NPC flag properties to CharacterEdit business object |

## Next Phase Readiness

Ready for 23-02 (SQLite schema migration). The DTO and business object changes provide the foundation for:
- Database schema migration to add columns
- GM UI for managing NPCs
- Template spawning system
