# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-01-26)

**Core value:** Players and Game Masters can easily access the system, manage their content securely, and focus on gameplay rather than administration.
**Current focus:** Phase 15 - Dashboard Details (In progress)

## Current Position

Milestone: v1.2
Phase: 15 of 16 (Dashboard Details)
Plan: 1 of 3 in current phase
Status: In progress
Last activity: 2026-01-27 - Completed 15-01-PLAN.md

Progress: v1.2 [#############.......] 64% (5 phases, 43 requirements)

## Performance Metrics

**Velocity:**
- Total plans completed: 34
- Average duration: 11 min
- Total execution time: 6.9 hours

**By Phase (v1.0):**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 01-foundation | 3 | 24 min | 8 min |
| 02-gm-item-management | 3 | 44 min | 15 min |
| 03-character-creation-inventory | 2 | 13 min | 6.5 min |
| 04-gameplay-inventory-core | 2 | 18 min | 9 min |
| 05-container-system | 2 | 27 min | 13.5 min |
| 06-item-bonuses-and-combat | 3 | 108 min | 36 min |
| 07-item-distribution | 1 | 4 min | 4 min |

**By Phase (v1.1):**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 08-registration-foundation | 2/2 | 33 min | 16.5 min |
| 09-password-recovery | 2/2 | 19 min | 9.5 min |
| 10-admin-user-management | 2/2 | 11 min | 5.5 min |
| 11-user-profiles | 2/2 | 29 min | 14.5 min |

**By Phase (v1.2):**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 12-table-foundation | 2/2 | 5 min | 2.5 min |
| 13-join-workflow | 4/4 | 17 min | 4 min |
| 14-dashboard-core | 3/3 | 38 min | 13 min |
| 15-dashboard-details | 1/3 | 6 min | 6 min |

**Recent Trend:**
- Last 5 plans: 15-01 (6 min), 14-03 (21 min), 14-02 (4 min), 14-01 (13 min), 13-03 (5 min)
- Trend: steady execution velocity

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.
Recent decisions affecting current work:

- [v1.2]: CharacterDetailModal opens at 90% screen size
- [v1.2]: GmNotes stored per table-character pair (table-context specific)
- [v1.2]: Character switcher dropdown in modal header
- [v1.2]: Round advance buttons disabled when not in combat mode
- [v1.2]: ITimeEventSubscriber subscription pattern for GM dashboard real-time updates
- [v1.2]: 500ms delay before refreshing character list after time events
- [v1.2]: Use AddEffect() instead of Add() for proper effect stacking
- [v1.2]: CSS specificity: .component.card.border-state for themed borders
- [v1.2]: CharacterStatusCard health state borders: green/yellow/red/dark
- [v1.2]: PendingPoolBar reused for consistent health visualization
- [v1.2]: WoundSummary groups by name with count suffix (Light x2, Serious)
- [v1.2]: EffectSummary includes duration suffix when RoundsRemaining has value
- [v1.2]: Effects loaded via ICharacterEffectDal in TableCharacterList.Fetch
- [v1.2]: JoinRequest DTO with status enum (Pending, Approved, Denied)
- [v1.2]: JoinRequestMessage for real-time notifications via Rx.NET
- [v1.1]: CharacterUpdateMessage reusable for real-time dashboard updates

### Pending Todos

None.

### Blockers/Concerns

None.

**Known Technical Debt (non-blocking, from v1.0):**
- ArmorInfoFactory.cs orphaned (duplicate logic in DamageResolution.razor)
- Weapon filtering logic in UI layer (should move to GameMechanics)
- Case sensitivity inconsistencies in skill/template comparisons
- OnCharacterChanged callback not wired in Play.razor
- Pre-existing failing test: UnequipItemAsync_RemovesEquipEffects

## Session Continuity

Last session: 2026-01-27
Stopped at: Completed 15-01-PLAN.md
Resume file: None

---
*Phase 15 in progress - 1/3 plans complete*
