# Roadmap: Threa TTRPG Assistant

## Milestones

- v1.0 MVP - Phases 1-7 (shipped 2026-01-26)
- v1.1 User Management & Authentication - Phases 8-11 (shipped 2026-01-26)
- v1.2 GM Table & Campaign Management - Phases 12-16 (shipped 2026-01-28)
- v1.4 GM Character Manipulation + Concentration - Phases 17-22 (shipped 2026-01-29)
- v1.5 NPC Management System - Phases 23-27 (shipped 2026-02-03)
- **v1.6 Batch Character Actions** - Phases 28-31 (shipped 2026-02-05)

## Phases

<details>
<summary>v1.0-v1.5 (Phases 1-27) - SHIPPED</summary>

See .planning/milestones/ for archived roadmaps and requirements from previous milestones.

- v1.0 (Phases 1-7): Inventory & Equipment System
- v1.1 (Phases 8-11): User Management & Authentication
- v1.2 (Phases 12-16): GM Table & Campaign Management
- v1.4 (Phases 17-22): GM Character Manipulation + Concentration System
- v1.5 (Phases 23-27): NPC Management System

</details>

### v1.6 Batch Character Actions (Complete)

**Milestone Goal:** Enable GMs to select multiple characters (NPCs and/or PCs) and apply actions to all at once for efficient encounter management.

- [x] **Phase 28: Selection Infrastructure** - Multi-character selection UI and state management
- [x] **Phase 29: Batch Action Framework** - Backend service and damage/healing batch operations
- [x] **Phase 30: Batch Visibility & Lifecycle** - NPC visibility toggle and dismiss/archive operations
- [x] **Phase 31: Batch Effects** - Effect add/remove batch operations

## Phase Details

### Phase 28: Selection Infrastructure
**Goal**: GMs can select multiple characters from the dashboard with clear visual feedback
**Depends on**: Phase 27 (v1.5 shipped)
**Requirements**: SEL-01, SEL-02, SEL-03, SEL-04, SEL-05
**Success Criteria** (what must be TRUE):
  1. GM can click checkbox on any character status card to toggle selection
  2. Dashboard displays "X selected" count when characters are selected
  3. GM can click "Select All" to select all characters within a section (PCs, Hostile NPCs, etc.)
  4. GM can click "Deselect All" to clear all selections
  5. Selected characters have visible highlight/indicator distinguishing them from unselected
**Plans**: 2 plans

Plans:
- [x] 28-01-PLAN.md - Selection state management and checkbox UI on all card components
- [x] 28-02-PLAN.md - Selection controls (SelectionBar, per-section Select All, Escape key handler)

### Phase 29: Batch Action Framework
**Goal**: GMs can apply damage or healing to all selected characters at once with clear feedback
**Depends on**: Phase 28
**Requirements**: DMG-01, DMG-02, DMG-03, DMG-04, UX-01, UX-02, UX-03, UX-04
**Success Criteria** (what must be TRUE):
  1. Contextual action bar appears when one or more characters are selected
  2. GM can enter damage amount and apply to all selected characters in one action
  3. GM can enter healing amount and apply to all selected characters in one action
  4. Batch results show success count ("Applied 3 FAT damage to 5 characters")
  5. When some characters fail (e.g., already at max health), feedback shows both successes and failures with reasons
**Plans**: 3 plans

Plans:
- [x] 29-01-PLAN.md - BatchActionService backend with sequential processing
- [x] 29-02-PLAN.md - SelectionBar action buttons and damage/healing modal
- [x] 29-03-PLAN.md - Result feedback display and selection cleanup

### Phase 30: Batch Visibility & Lifecycle
**Goal**: GMs can toggle visibility or dismiss/archive multiple NPCs at once
**Depends on**: Phase 29
**Requirements**: VIS-01, VIS-02, LIFE-01, LIFE-02
**Success Criteria** (what must be TRUE):
  1. GM can reveal or hide all selected NPCs in one action
  2. GM can dismiss/archive all selected NPCs in one action
  3. Visibility batch shows success/failure feedback
  4. Dismiss batch shows success/failure feedback
**Plans**: 2 plans

Plans:
- [x] 30-01-PLAN.md - Backend: extend BatchActionService with visibility toggle and dismiss methods
- [x] 30-02-PLAN.md - Frontend: SelectionBar visibility/dismiss buttons, confirmation modal, GmTable wiring

### Phase 31: Batch Effects
**Goal**: GMs can add or remove effects to/from multiple characters at once
**Depends on**: Phase 30
**Requirements**: EFF-01, EFF-02, EFF-03
**Success Criteria** (what must be TRUE):
  1. GM can select an effect from the effect picker and add it to all selected characters
  2. GM can select an effect type and remove it from all selected characters that have it
  3. Effect batch operations show success count and any failures with reasons
**Plans**: 2 plans

Plans:
- [x] 31-01-PLAN.md - Backend effect add service + BatchEffectAddModal + SelectionBar wiring
- [x] 31-02-PLAN.md - Backend effect remove service + BatchEffectRemoveModal + SelectionBar wiring

## Progress

**Execution Order:**
Phases execute in numeric order: 28 -> 29 -> 30 -> 31

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|----------------|--------|-----------|
| 28. Selection Infrastructure | v1.6 | 2/2 | Complete | 2026-02-04 |
| 29. Batch Action Framework | v1.6 | 3/3 | Complete | 2026-02-05 |
| 30. Batch Visibility & Lifecycle | v1.6 | 2/2 | Complete | 2026-02-05 |
| 31. Batch Effects | v1.6 | 2/2 | Complete | 2026-02-05 |

---
*Roadmap created: 2026-02-04*
*Milestone v1.6: 4 phases, 9 plans, 20 requirements*
