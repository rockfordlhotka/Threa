# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-01-26)

**Core value:** Players and Game Masters can easily access the system, manage their content securely, and focus on gameplay rather than administration.
**Current focus:** Phase 16 - Time Management (IN PROGRESS)

## Current Position

Milestone: v1.2
Phase: 16 of 16 (Time Management)
Plan: 2 of 2 in current phase
Status: Phase complete
Last activity: 2026-01-27 - Completed 16-02-PLAN.md

Progress: v1.2 [####################] 100% (5 phases, 43 requirements)

## Performance Metrics

**Velocity:**
- Total plans completed: 38
- Average duration: 12 min
- Total execution time: 7.9 hours

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
| 15-dashboard-details | 3/3 | 56 min | 19 min |
| 16-time-management | 2/2 | 5 min | 2.5 min |

**Recent Trend:**
- Last 5 plans: 16-02 (5 min), 15-03 (45 min), 15-02 (5 min), 15-01 (6 min), 14-03 (21 min)
- Trend: 16-02 quick execution with minimal code changes

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.
Recent decisions affecting current work:

- [v1.2]: Tab order: GM Actions, Character Sheet, Inventory, Grant Items, Narrative
- [v1.2]: Modal header shows character summary with health bars and status badges
- [v1.2]: Two-button layout (FAT/VIT) for damage and healing actions
- [v1.2]: 2-column dashboard layout after center panel removal
- [v1.2]: RadzenDialog must be in interactive context for DialogService
- [v1.2]: Click-outside-to-close enabled for touch accessibility
- [v1.2]: Skills grouped by PrimaryAttribute in CharacterDetailSheet
- [v1.2]: GM notes auto-save on blur with loading indicator
- [v1.2]: Read-only view components for GM character inspection
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
- [v1.2]: Time skip processing capped at 100 iterations to avoid UI freeze
- [v1.2]: "In Rounds" badge shown only when in combat, hidden otherwise
- [v1.1]: CharacterUpdateMessage reusable for real-time dashboard updates

### Pending Todos

**Future Phase - Character Management:**
- Add FAT recovery warning in TabStatus.razor when VIT is low (VIT 1-4 affects FAT recovery rate per design/GAME_RULES_SPECIFICATION.md)
- Fix AP Max calculation bug: ActionPoints.Max should recalculate when skills change, not just use cached DB value (currently only calculates on character creation)
- Implement effect management: edit/remove effects (requirements GMCHAR-05, GMCHAR-06, GMCHAR-07 exist but not assigned to a phase)

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
Stopped at: Completed 16-02-PLAN.md (Phase 16 complete, v1.2 milestone complete)
Resume file: None

---
*Phase 16 complete - v1.2 milestone complete*
