# Roadmap: Threa v1.5 NPC Management System

**Created:** 2026-02-01
**Depth:** Standard
**Phases:** 5 (Phases 23-27)
**Requirements:** 23 total

## Overview

This roadmap enables GMs to create, manage, and run NPCs in combat encounters alongside player characters. NPCs use the existing CharacterEdit model with an IsNpc flag, providing 100% feature parity with PCs. The template pattern follows the proven ItemTemplate approach for pre-session prep, while quick-spawn enables improvisation during play.

## Phases

### Phase 23: Data Model Foundation

**Goal:** Character model supports NPC and template flags with database schema ready for NPC features.

**Dependencies:** None (first phase)

**Plans:** 2 plans

Plans:
- [x] 23-01-PLAN.md — Add NPC flags to Character DTO and CharacterEdit business object
- [x] 23-02-PLAN.md — Add DAL query methods and unit tests for NPC flags

**Requirements:**
- CRTN-02 (partial): NPCs have full character stats (same model as PCs)

**Success Criteria:**
1. Character table has IsNpc, IsTemplate, and VisibleToPlayers columns with migration applied
2. CharacterEdit business object exposes IsNpc, IsTemplate, and VisibleToPlayers properties
3. ICharacterDal has GetNpcTemplatesAsync and GetTableNpcsAsync methods defined
4. Unit tests verify new properties persist through save/fetch cycle

**Rationale:** Everything depends on the data model. Establishing CharacterEdit reuse with NPC flags prevents rework and enables all existing GM manipulation tools to work unchanged on NPCs.

---

### Phase 24: NPC Template System

**Goal:** GMs can build and organize a library of NPC templates for pre-session encounter prep.

**Dependencies:** Phase 23 (data model)

**Plans:** 5 plans

Plans:
- [x] 24-01-PLAN.md — Data model extensions (NpcDisposition enum, template properties, DAL methods)
- [x] 24-02-PLAN.md — Template list business objects (NpcTemplateInfo, NpcTemplateList)
- [x] 24-03-PLAN.md — Template library UI page with search/filter
- [x] 24-04-PLAN.md — Template editor UI page with clone functionality
- [x] 24-05-PLAN.md — Polish: category autocomplete, inactive styling, difficulty badges

**Requirements:**
- TMPL-01: GM can create NPC templates with full character stats
- TMPL-02: GM can edit existing NPC templates
- TMPL-03: GM can browse/search NPC templates (filter by category/tags)
- TMPL-04: GM can delete/deactivate NPC templates
- TMPL-05: NPC templates support category tags for organization

**Success Criteria:**
1. GM can create a new NPC template with attributes, skills, and equipment via web UI
2. GM can edit an existing NPC template and changes persist
3. GM can search templates by name and filter by category tags
4. GM can deactivate a template (soft delete) and it no longer appears in active library
5. Template library page shows organized view with tag-based filtering

**Rationale:** Templates enable pre-session prep and quick spawning. Following the proven ItemTemplate pattern provides consistency and reduces implementation risk.

---

### Phase 25: NPC Creation & Dashboard

**Goal:** GMs can spawn NPCs from templates during play and see them in the dashboard with full manipulation powers.

**Dependencies:** Phase 24 (templates exist to spawn from)

**Plans:** 6 plans

Plans:
- [x] 25-01-PLAN.md — Data model extensions (SourceTemplateId/Name, TableCharacterInfo NPC fields)
- [x] 25-02-PLAN.md — NPC auto-naming service with global counter and prefix memory
- [x] 25-03-PLAN.md — NPC spawner CSLA command (clone template to new character)
- [x] 25-04-PLAN.md — Spawn modal UI with template library integration
- [x] 25-05-PLAN.md — Dashboard NPC section with disposition grouping
- [x] 25-06-PLAN.md — Integration: dashboard spawn button and end-to-end workflow

**Requirements:**
- CRTN-01: GM can quick-create NPC from template
- CRTN-02: NPCs have full character stats (same model as PCs)
- CRTN-03: Smart naming auto-generates unique names
- CRTN-04: GM can add session-specific notes to individual NPC instances
- DASH-01: NPCs appear in GM dashboard in separate section from PCs
- DASH-02: NPC status cards show same info as PC cards
- DASH-03: NPC detail modal provides same manipulation powers as PCs
- DASH-04: NPCs display disposition marker with visual differentiation

**Success Criteria:**
1. GM can select a template and spawn an NPC instance to the active table
2. Spawning multiple from same template auto-generates unique names (e.g., "Goblin 1", "Goblin 2")
3. GM dashboard shows NPCs in a separate section below player characters
4. NPC status cards display FAT/VIT bars, wound badges, AP, and active effects
5. Clicking an NPC status card opens CharacterDetailModal with full GM Actions tab functionality
6. GM can set and view session-specific notes on individual NPC instances
7. NPCs display disposition marker (Hostile/Neutral/Friendly) with distinct visual styling

**Rationale:** This phase delivers the core workflow: spawn NPCs and manipulate them using existing tools. Dashboard integration validates the CharacterEdit reuse approach.

---

### Phase 26: Visibility & Lifecycle

**Goal:** GMs can hide NPCs for surprise encounters and manage NPC persistence across sessions.

**Dependencies:** Phase 25 (NPCs exist to hide/manage)

**Plans:** 5 plans

Plans:
- [x] 26-01-PLAN.md — Data model extensions (TableCharacterInfo.VisibleToPlayers, IsArchived, DAL methods)
- [x] 26-02-PLAN.md — Visibility toggle (spawn hidden, Hidden section, HiddenNpcCard, modal toggle)
- [x] 26-03-PLAN.md — NPC lifecycle - dismiss/archive/delete in CharacterDetailAdmin
- [x] 26-04-PLAN.md — NPC archive browser page with restore flow
- [x] 26-05-PLAN.md — Save as template (NpcTemplateCreator command, modal integration)

**Requirements:**
- VSBL-01: GM can toggle NPC visibility
- VSBL-02: Hidden NPCs do not appear in player-visible views
- VSBL-03: Visibility toggle is accessible from both status card and detail modal
- LIFE-01: GM can remove/dismiss NPCs from active table
- LIFE-02: NPCs persist across sessions until explicitly dismissed
- LIFE-03: GM can save active NPC as new template
- LIFE-04: Dismissed NPCs can be optionally deleted or archived

**Success Criteria:**
1. GM can toggle NPC visibility from both status card and detail modal
2. Hidden NPCs show visual indicator (eye icon, dimmed styling) in GM dashboard
3. Hidden NPCs do not appear in any player-visible targeting lists or activity feeds
4. GM can dismiss an NPC from the active table (removes from dashboard)
5. NPCs persist across browser refresh and session reconnect until explicitly dismissed
6. GM can save an active NPC (with current wounds, effects, inventory) as a new template
7. Dismiss action offers choice between delete (permanent) and archive (retrievable)

**Rationale:** Visibility enables surprise encounters. Lifecycle management prevents stale NPC accumulation while preserving useful NPCs for recurring encounters.

---

### Phase 27: Time & Combat Integration

**Goal:** NPCs participate fully in combat rounds with time advancement affecting them identically to PCs.

**Dependencies:** Phase 26 (NPCs fully manageable)

**Plans:** 2 plans

Plans:
- [x] 27-01-PLAN.md — NPC target loading and selection UI (TableCharacterList, disposition grouping)
- [x] 27-02-PLAN.md — Combat polish (target invalidation, reveal activity log)

**Requirements:**
- CMBT-01: NPCs participate in round/time advancement
- CMBT-02: NPCs can be targeted by combat actions
- CMBT-03: Time advancement applies to NPCs same as PCs

**Success Criteria:**
1. Advancing time (rounds, minutes, hours) processes NPC effects same as PC effects (expiration, stacking)
2. NPC AP recovers on round advancement according to standard rules
3. NPCs appear in targeting dropdown when player initiates attack action
4. Visible NPCs (not hidden) are available as valid targets in combat
5. NPC wounds, effects, and health pools update in real-time during time advancement

**Rationale:** Full combat integration validates the architectural decision to reuse CharacterEdit. NPCs behave identically to PCs in combat, enabling GMs to run encounters without learning separate mechanics.

---

## Progress

| Phase | Name | Requirements | Status |
|-------|------|--------------|--------|
| 23 | Data Model Foundation | 1 | Complete |
| 24 | NPC Template System | 5 | Complete |
| 25 | NPC Creation & Dashboard | 8 | Complete |
| 26 | Visibility & Lifecycle | 7 | Complete |
| 27 | Time & Combat Integration | 3 | Complete |

**Total:** 23 requirements mapped / 23 total = 100% coverage

## Requirement Coverage

| Requirement | Phase | Description |
|-------------|-------|-------------|
| CRTN-02 | 23, 25 | Full character stats (foundation in 23, completion in 25) |
| TMPL-01 | 24 | Create NPC templates |
| TMPL-02 | 24 | Edit NPC templates |
| TMPL-03 | 24 | Browse/search templates |
| TMPL-04 | 24 | Delete/deactivate templates |
| TMPL-05 | 24 | Template category tags |
| CRTN-01 | 25 | Quick-create NPC from template |
| CRTN-03 | 25 | Smart auto-naming |
| CRTN-04 | 25 | Session-specific notes |
| DASH-01 | 25 | NPCs in separate dashboard section |
| DASH-02 | 25 | NPC status cards |
| DASH-03 | 25 | NPC detail modal |
| DASH-04 | 25 | Disposition markers |
| VSBL-01 | 26 | Toggle NPC visibility |
| VSBL-02 | 26 | Hidden NPCs not visible to players |
| VSBL-03 | 26 | Visibility toggle accessible from card and modal |
| LIFE-01 | 26 | Remove/dismiss NPCs |
| LIFE-02 | 26 | NPCs persist across sessions |
| LIFE-03 | 26 | Save NPC as new template |
| LIFE-04 | 26 | Delete or archive dismissed NPCs |
| CMBT-01 | 27 | NPCs in round/time advancement |
| CMBT-02 | 27 | NPCs as combat targets |
| CMBT-03 | 27 | Time advancement applies to NPCs |

---
*Roadmap created: 2026-02-01*
*Phase 23 planned: 2026-02-01*
*Phase 23 complete: 2026-02-01*
*Phase 24 planned: 2026-02-02*
*Phase 24 complete: 2026-02-02*
*Phase 25 planned: 2026-02-02*
*Phase 25 complete: 2026-02-02*
*Phase 26 planned: 2026-02-03*
*Phase 26 complete: 2026-02-03*
*Phase 27 planned: 2026-02-03*
*Phase 27 complete: 2026-02-03*
