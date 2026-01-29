# Roadmap: Threa TTRPG Assistant - v1.3 GM Character Manipulation

## Overview

This milestone completes GM character manipulation powers by enabling direct health management, wound tracking, effect application, inventory control, and stat editing from the GM dashboard. Building on v1.2's dashboard foundation, GMs gain full control over character state during gameplay.

## Milestones

- v1.0 MVP - Phases 1-7 (shipped 2026-01-26)
- v1.1 User Management & Authentication - Phases 8-11 (shipped 2026-01-26)
- v1.2 GM Table & Campaign Management - Phases 12-16 (shipped 2026-01-28)
- **v1.3 GM Character Manipulation** - Phases 17-21 (in progress)

## Phases

**Phase Numbering:**
- Integer phases (17, 18, 19...): Planned milestone work
- Decimal phases (17.1, 17.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [x] **Phase 17: Health Management** - GM applies damage/healing to character health pools
- [x] **Phase 18: Wound Management** - GM tracks and manages character wounds
- [x] **Phase 19: Effect Management** - GM applies, edits, and templates character effects
- [x] **Phase 20: Inventory Manipulation** - GM controls character inventory directly
- [x] **Phase 21: Stat Editing** - GM modifies character attributes and skills

## Phase Details

### Phase 17: Health Management
**Goal**: GM can apply damage and healing to character FAT/VIT pools through the dashboard
**Depends on**: Phase 16 (v1.2 dashboard foundation)
**Requirements**: HLTH-01, HLTH-02, HLTH-03, HLTH-04, HLTH-05, HLTH-06, HLTH-07
**Success Criteria** (what must be TRUE):
  1. GM can apply numeric damage to a character's FAT pending pool and see the update immediately
  2. GM can apply numeric damage to a character's VIT pending pool and see the update immediately
  3. GM can apply numeric healing to both FAT and VIT pending pools
  4. GM can view current pool values and pending values before applying changes
  5. Dashboard updates in real-time when damage/healing is applied
**Plans**: 2 plans

Plans:
- [x] 17-01-PLAN.md — Enhance PendingPoolBar with color thresholds and overheal support
- [x] 17-02-PLAN.md — Refactor CharacterDetailGmActions to mode toggle with warnings

### Phase 18: Wound Management
**Goal**: GM can add, remove, and edit wounds on characters with severity tracking
**Depends on**: Phase 17
**Requirements**: WOND-01, WOND-02, WOND-03, WOND-04
**Success Criteria** (what must be TRUE):
  1. GM can add a wound to a character with severity level (minor, moderate, severe, critical) and description
  2. GM can remove a wound from a character
  3. GM can edit an existing wound's description and severity
  4. GM can view all active wounds on a character with their severities
**Plans**: 2 plans

Plans:
- [x] 18-01-PLAN.md — Extend WoundState and create wound CRUD modals
- [x] 18-02-PLAN.md — Header wound badges and VIT damage wound prompt

### Phase 19: Effect Management
**Goal**: GM can create, apply, edit, and template effects on characters
**Depends on**: Phase 18
**Requirements**: EFCT-01, EFCT-02, EFCT-03, EFCT-04, EFCT-05, EFCT-06, EFCT-07, EFCT-08, EFCT-09, EFCT-10
**Success Criteria** (what must be TRUE):
  1. GM can add a custom effect to a character with name, description, duration, and modifiers
  2. GM can set effect duration in rounds or turns and specify attribute/skill modifiers
  3. GM can remove an active effect from a character
  4. GM can edit an existing effect's duration and modifiers
  5. GM can save an effect as a template and apply saved templates to characters
**Plans**: 4 plans

Plans:
- [x] 19-01-PLAN.md — EffectState model and DAL layer for effect templates
- [x] 19-02-PLAN.md — CSLA business objects (EffectTemplate, EffectTemplateList, GenericEffectBehavior)
- [x] 19-03-PLAN.md — Effect management UI (EffectManagementModal, EffectFormModal)
- [x] 19-04-PLAN.md — Template picker and Effects tab integration

### Phase 20: Inventory Manipulation
**Goal**: GM can directly add, remove, and manage items in character inventory
**Depends on**: Phase 19
**Requirements**: INVT-01, INVT-02, INVT-03, INVT-04, INVT-05, INVT-06, INVT-07, INVT-08
**Success Criteria** (what must be TRUE):
  1. GM can add items from the template library to a character's inventory with quantity support
  2. GM can remove items from a character's inventory
  3. GM can equip/unequip items to/from character equipment slots
  4. GM can view character inventory before making changes
  5. Inventory changes trigger real-time dashboard updates
**Plans**: 3 plans

Plans:
- [x] 20-01-PLAN.md — ItemTemplatePickerModal for browsing/selecting item templates
- [x] 20-02-PLAN.md — CharacterDetailInventory GM actions and currency editing
- [x] 20-03-PLAN.md — (Gap closure) Fix CharacterDetailModal integration

### Phase 21: Stat Editing
**Goal**: GM can directly modify character attributes and skills
**Depends on**: Phase 20
**Requirements**: STAT-01, STAT-02, STAT-03, STAT-04, STAT-05, STAT-06
**Success Criteria** (what must be TRUE):
  1. GM can edit character attribute values (STR, DEX, END, INT, ITT, WIL, PHY)
  2. GM can edit character skill levels
  3. Attribute changes automatically recalculate dependent stats (health pools, Ability Scores)
  4. Skill changes automatically recalculate Ability Scores
  5. Stat changes trigger real-time dashboard updates
**Plans**: 2 plans

Plans:
- [x] 21-01-PLAN.md — Attribute editing with edit mode toggle in CharacterDetailSheet
- [x] 21-02-PLAN.md — Skill editing and CharacterDetailModal coordination

## Progress

**Execution Order:**
Phases execute in numeric order: 17 -> 17.1 -> 18 -> 18.1 -> 19 -> ...

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 17. Health Management | 2/2 | Complete | 2026-01-28 |
| 18. Wound Management | 2/2 | Complete | 2026-01-28 |
| 19. Effect Management | 4/4 | Complete | 2026-01-28 |
| 20. Inventory Manipulation | 3/3 | Complete | 2026-01-29 |
| 21. Stat Editing | 2/2 | Complete | 2026-01-29 |

---
*Created: 2026-01-28 for v1.3 milestone*
