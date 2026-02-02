# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-02-01)

**Core value:** Players and Game Masters can easily access the system, manage their content securely, and focus on gameplay rather than administration.
**Current focus:** v1.5 NPC Management System

## Current Position

Milestone: v1.5 NPC Management System
Phase: 25 - NPC Creation & Dashboard
Plan: 06 of 6 (complete)
Status: Phase complete
Last activity: 2026-02-02 -- Completed 25-06-PLAN.md (Spawn NPC Integration)

Progress: [████████████████████] 100%

## Performance Metrics

**Velocity:**
- Total plans completed: 71
- Average duration: 8.3 min
- Total execution time: 11 hours 1 min

**By Milestone:**

| Milestone | Phases | Plans | Total Time | Avg/Plan |
|-----------|--------|-------|------------|----------|
| v1.0 | 7 | 16 | 238 min | 15 min |
| v1.1 | 4 | 8 | 92 min | 11.5 min |
| v1.2 | 5 | 14 | 121 min | 8.6 min |
| v1.3/v1.4 | 6 | 21 | 48 min | 6 min |
| v1.5 | 3 | 13 | 70 min | 5.4 min |

## Accumulated Context

### Decisions

All decisions are logged in PROJECT.md Key Decisions table.

**v1.5 Architecture Decision (from research):**
- NPCs use existing CharacterEdit model with IsNpc flag (not parallel NPC model)
- Template pattern follows proven ItemTemplate approach
- Dashboard reuses CharacterStatusCard and CharacterDetailModal
- Visibility filtering prevents hidden NPCs from leaking to players

**Plan 23-01 Decision:**
- VisibleToPlayers defaults to true for backward compatibility

**Plan 23-02 Decision:**
- GetNpcTemplatesAsync uses memory filtering (JSON storage pattern)
- GetTableNpcsAsync stubbed until Phase 25 table integration

**Plan 24-01 Decision:**
- DifficultyRating uses average combat AS + health modifier - 10 normalization
- GetNpcCategoriesAsync reuses GetNpcTemplatesAsync with memory filtering

**Plan 24-02 Decision:**
- IsActive property on NpcTemplateInfo maps from VisibleToPlayers (true = active)
- TagList computed property returns IEnumerable<string> for LINQ flexibility

**Plan 24-03 Decision:**
- Extract categories and tags from data rather than separate DAL call
- Difficulty badge colors: 1-5=Easy/green, 6-10=Moderate/yellow, 11+=Hard/red

**Plan 24-04 Decision:**
- Reuse existing TabAttributes, TabSkills, TabItems components for template editing
- Clone route copies attributes and skills but NOT equipment or notes per CONTEXT.md
- HTML5 datalist used for category autocomplete (simpler than RadzenAutoComplete)

**Plan 24-05 Decision:**
- Clone modal uses Bootstrap modal with list-group for character selection
- GmCharacterInfo extended with IsNpc/IsTemplate/CharacterType properties
- Difficulty tooltips use HTML title attribute for simplicity

**Plan 25-01 Decision:**
- SourceTemplateId/Name on both DTO and CharacterEdit for full round-trip support
- TableCharacterInfo Fetch populates NPC fields from Character data for dashboard display

**Plan 25-02 Decision:**
- Global counter across all NPCs (not per-template) to avoid confusion
- Service registered as Scoped for per-session counter state
- Prefix extraction from customized names updates future spawns

**Plan 25-03 Decision:**
- NpcSpawner uses SaveCharacterAsync (Id=0 triggers insert)
- Spawned NPCs get IsPlayable=true immediately (no activation needed)
- Health pools and AP start at max (full health, full AP)

**Plan 25-04 Decision:**
- Modal component in Shared folder for reuse across template library and GM dashboard
- NpcSpawnRequest nested class provides typed spawn parameters
- NpcAutoNamingService moved to Threa.Client/Services for compile-time access

**Plan 25-05 Decision:**
- NPCs filtered from tableCharacters using IsNpc property
- Empty disposition groups hidden via @if conditional rendering
- Disposition icons: skull (Hostile), circle (Neutral), heart (Friendly)

**Plan 25-06 Decision:**
- Template selector modal loads templates lazily on first open, caches for session
- Spawn button in NPC section header alongside count badge
- Auto-generated name shown in spawn modal with customization option

### Pending Todos

None.

### Blockers/Concerns

None.

**Known Technical Debt (non-blocking, from v1.0):**
- ArmorInfoFactory.cs orphaned
- Weapon filtering logic in UI layer
- Case sensitivity inconsistencies

## Session Continuity

Last session: 2026-02-02
Stopped at: Completed 25-06-PLAN.md (Spawn NPC Integration)
Resume file: None
Next action: Phase 25 complete. Ready for next milestone planning.

---
*v1.5 Milestone -- Phase 25 complete*
