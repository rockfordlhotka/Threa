# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-01-28)

**Core value:** Players and Game Masters can easily access the system, manage their content securely, and focus on gameplay rather than administration.
**Current focus:** Phase 20 - Inventory Manipulation in progress

## Current Position

Milestone: v1.3 GM Character Manipulation
Phase: 20 of 21 (Inventory Manipulation)
Plan: 01 of 02 complete
Status: In progress
Last activity: 2026-01-29 - Completed 20-01-PLAN.md (Item Template Picker)

Progress: v1.3 [█████████████████░░░] 85% (4.5/5 phases complete)

## Performance Metrics

**Velocity:**
- Total plans completed: 47
- Average duration: 10.5 min
- Total execution time: 8.9 hours

**By Milestone:**

| Milestone | Phases | Plans | Total Time | Avg/Plan |
|-----------|--------|-------|------------|----------|
| v1.0 | 7 | 16 | 238 min | 15 min |
| v1.1 | 4 | 8 | 92 min | 11.5 min |
| v1.2 | 5 | 14 | 121 min | 8.6 min |
| v1.3 | 5 | 9 | 53.6 min | 6.0 min |

**Recent Trend:**
- Last 5 plans: 20-01 (2.0 min), 19-04 (5.0 min), 19-03 (6.3 min), 19-02 (3.8 min), 19-01 (4.5 min)
- Trend: Excellent - Phase 20 Plan 01 in 2 min

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

### Pending Todos

**From previous milestones:**
- Fix AP Max calculation bug (recalculate when skills change)

**Completed this phase (20):**
- ItemTemplatePickerModal for browsing item templates

### Blockers/Concerns

None.

**Known Technical Debt (non-blocking, from v1.0):**
- ArmorInfoFactory.cs orphaned
- Weapon filtering logic in UI layer
- Case sensitivity inconsistencies
- OnCharacterChanged callback not wired in Play.razor

## Session Continuity

Last session: 2026-01-29
Stopped at: Completed 20-01-PLAN.md (Item Template Picker)
Resume file: None

---
*v1.3 milestone in progress - Phase 20 Plan 01 complete, Plan 02 next*
