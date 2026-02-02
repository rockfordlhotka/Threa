# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-02-01)

**Core value:** Players and Game Masters can easily access the system, manage their content securely, and focus on gameplay rather than administration.
**Current focus:** v1.5 NPC Management System

## Current Position

Milestone: v1.5 NPC Management System
Phase: 24 - NPC Template System (in progress)
Plan: 02 of 5
Status: In progress
Last activity: 2026-02-02 -- Completed 24-02-PLAN.md (NPC Template Business Objects)

Progress: [████░░░░░░░░░░░░░░░░] 20%

## Performance Metrics

**Velocity:**
- Total plans completed: 62
- Average duration: 9.0 min
- Total execution time: 10 hours 7 min

**By Milestone:**

| Milestone | Phases | Plans | Total Time | Avg/Plan |
|-----------|--------|-------|------------|----------|
| v1.0 | 7 | 16 | 238 min | 15 min |
| v1.1 | 4 | 8 | 92 min | 11.5 min |
| v1.2 | 5 | 14 | 121 min | 8.6 min |
| v1.3/v1.4 | 6 | 21 | 48 min | 6 min |
| v1.5 | 2 | 4 | 16 min | 4.0 min |

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
Stopped at: Completed 24-02-PLAN.md
Resume file: None
Next action: Execute 24-03-PLAN.md (NPC Template Library Grid View)

---
*v1.5 Milestone -- Phase 24 plan 02 complete, ready for plan 03*
