# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-02-11)

**Core value:** Players and Game Masters can easily access the system, manage their content securely, and focus on gameplay rather than administration.
**Current focus:** v1.7 Combat Tab Rework - Phase 36 Other Group complete

## Current Position

Milestone: v1.7 Combat Tab Rework (Issues #104, #105)
Phase: 36 of 36 (Other Group)
Plan: 2 of 2 in current phase
Status: Phase complete - Milestone complete
Last activity: 2026-02-13 — Completed 36-02-PLAN.md

Progress: [████████████████████] 100% (5/5 phases complete)

## Performance Metrics

**Velocity:**
- Total plans completed: 97
- Average duration: 7.2 min
- Total execution time: ~11.6 hours

**By Milestone:**

| Milestone | Phases | Plans | Total Time | Avg/Plan |
|-----------|--------|-------|------------|----------|
| v1.0 | 7 | 16 | 238 min | 15 min |
| v1.1 | 4 | 8 | 92 min | 11.5 min |
| v1.2 | 5 | 14 | 121 min | 8.6 min |
| v1.4 | 6 | 21 | 48 min | 6 min |
| v1.5 | 5 | 20 | 105 min | 5.3 min |
| v1.6 | 4 | 9 | 55 min | 6.1 min |
| v1.7 | 5 | 10 | 42 min | 4.2 min |

## Accumulated Context

### Decisions

All decisions are logged in PROJECT.md Key Decisions table.

| Decision | Phase | Rationale |
|----------|-------|-----------|
| Used FirearmAttackResolver with TVAdjustment offset for anonymous targets | 33-02 | Preserves resolver logic (ammo, hit detection, SV calc) while giving player TV control |
| OnAdding replaces ANY existing CombatStance (not just same-name) | 35-01 | Enforces single-stance mutual exclusivity across all stance types |
| Pre-selection guarded by !hasRolled to avoid overriding player choice | 35-02 | Don't change defense type after roll; only set initial state |
| StartRest checks only IsPassedOut, not CanRest(), to allow panel open with insufficient AP | 36-01 | Matches how other combat modes work; Confirm button handles validation |
| Disabled attribute reserved for hard blocks only; CSS dimming for resource insufficiency | 36-02 | Keeps buttons clickable for feedback; two-tier visual system |

### Pending Todos

None.

### Blockers/Concerns

None.

**Known Technical Debt (non-blocking, from v1.0):**
- ArmorInfoFactory.cs orphaned
- Weapon filtering logic in UI layer
- Case sensitivity inconsistencies

## Session Continuity

Last session: 2026-02-13
Stopped at: Completed 36-02-PLAN.md (Phase 36 complete, Milestone v1.7 complete)
Resume file: None
Next action: None - v1.7 Combat Tab Rework milestone complete

---
*v1.7 Combat Tab Rework started 2026-02-11*
*Previous milestone archives: .planning/milestones/*
