# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-01-28)

**Core value:** Players and Game Masters can easily access the system, manage their content securely, and focus on gameplay rather than administration.
**Current focus:** v1.4 Concentration System - In progress

## Current Position

Milestone: v1.4 Concentration System
Phase: 22 (Concentration System) - In progress
Plan: 03 of 07 complete
Status: Phase incomplete - 4 gaps remain
Last activity: 2026-01-29 - Completed 22-03 (Sustained Concentration)

Progress: Phase 22 [████████░░░░░░░░░░░░] 43% (3/7 plans complete)

## Performance Metrics

**Velocity:**
- Total plans completed: 53
- Average duration: 9.8 min
- Total execution time: 9.4 hours

**By Milestone:**

| Milestone | Phases | Plans | Total Time | Avg/Plan |
|-----------|--------|-------|------------|----------|
| v1.0 | 7 | 16 | 238 min | 15 min |
| v1.1 | 4 | 8 | 92 min | 11.5 min |
| v1.2 | 5 | 14 | 121 min | 8.6 min |
| v1.3 | 6 | 13 | 66.5 min | 5.1 min |
| v1.4 | 1 | 3 | 18 min | 6 min |

**Recent Trend:**
- Last 5 plans: 22-03 (8 min), 22-02 (8 min), 22-01 (2 min), 21-02 (2.4 min), 21-01 (3.5 min)
- Trend: Excellent - Phase 22 progressing well

## Accumulated Context

### Decisions

All decisions are logged in PROJECT.md Key Decisions table.

Recent decisions affecting current work:
- v1.2: Two-button FAT/VIT layout for faster damage/healing application
- v1.2: Real-time updates via CharacterUpdateMessage infrastructure
- v1.3: Color thresholds use effective value (current - damage + healing)
- v1.3: Theme-aware colors via CSS variables for fantasy/scifi support
- v1.3: Single card with mode toggle replaces two separate damage/healing cards
- v1.3: Inline alert warnings (alert-warning for damage, alert-info for healing)
- v1.3 (18-01): Four fixed severity levels (Minor/Moderate/Severe/Critical)
- v1.3 (18-01): Severity auto-fills AS penalty and FAT/VIT rates with GM override option
- v1.3 (18-02): Wound badge colors match severity (Critical=danger, Minor=secondary)
- v1.3 (18-02): "Apply + Add Wound" is optional prompt, not required
- v1.3 (19-01): Dictionary-based modifiers for attribute/skill flexibility
- v1.3 (19-01): Behavior tags as list for multi-behavior effects
- v1.3 (19-02): GenericEffectBehavior for Disease, ObjectEffect, Environmental only
- v1.3 (19-02): State caching in EffectTemplate for performance
- v1.3 (19-03): Card grid layout for effect display (col-md-6, col-lg-4)
- v1.3 (19-03): Collapsible advanced modifiers section in effect form
- v1.3 (19-04): Debounced template search (300ms)
- v1.3 (19-04): Table layout for Effects tab (compact read-only view)
- v1.3 (20-01): Rarity colors for items (secondary/success/primary/purple/warning)
- v1.3 (20-02): Inline quantity prompt for stackable items
- v1.3 (20-02): Dropdown context menus for item actions
- v1.3 (20-02): Currency editing via edit mode toggle
- v1.3 (21-01): Pencil button on Attributes card header for inline edit mode
- v1.3 (21-01): IsEditMode two-way binding with EventCallback for parent coordination
- v1.3 (21-01): Health pools capped at new max on save
- v1.3 (21-02): All skills visible in edit mode for complete editing
- v1.3 (21-02): Combined SaveAllChanges for attributes and skills together
- v1.3 (21-02): AP capped at new max when skills reduced
- v1.4 (22-01): JSON blob storage continues (no schema migration for SQLite)
- v1.4 (22-01): Effect linking via SourceEffectId/SourceCasterId
- v1.4 (22-01): Nested payload JSON for deferred actions
- v1.4 (22-02): CompleteEarly vs ExpireEarly distinction for OnExpire vs OnRemove
- v1.4 (22-03): Drain via PendingDamage pattern (matches health pool processing)
- v1.4 (22-03): Effective pool check (Value - PendingDamage) for exhaustion detection
- v1.4 (22-03): LinkedEffectIds in LastConcentrationResult for cross-character cleanup

### Pending Todos

**From previous milestones:**
- (RESOLVED) Fix AP Max calculation bug - Now addressed via skill editing with AP capping

**Completed this milestone (v1.3):**
- Pending damage/healing with real-time updates (Phase 17)
- Wound management with severity levels (Phase 18)
- Effect management with template picker (Phase 19)
- Inventory manipulation with item grants (Phase 20)
- Stat editing for attributes and skills (Phase 21)

**Completed (v1.4 Concentration System):**
- Phase 22 Plan 01: Data layer foundation with ConcentrationState schema, effect linking, payload classes
- Phase 22 Plan 02: Casting-time concentration lifecycle with OnTick/OnExpire/OnRemove implementation
- Phase 22 Plan 03: Sustained concentration with FAT/VIT drain and linked effect removal preparation

### Blockers/Concerns

None.

**Known Technical Debt (non-blocking, from v1.0):**
- ArmorInfoFactory.cs orphaned
- Weapon filtering logic in UI layer
- Case sensitivity inconsistencies
- OnCharacterChanged callback not wired in Play.razor

## Session Continuity

Last session: 2026-01-29
Stopped at: Completed 22-03-PLAN.md (Sustained Concentration)
Resume file: None

---
*v1.4 Concentration System in progress - 3/7 plans complete*
