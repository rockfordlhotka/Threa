# Roadmap: Threa TTRPG Assistant

## Milestones

- [x] **v1.0 Inventory & Equipment** - Phases 1-7 (shipped 2026-01-26)
- [x] **v1.1 User Management** - Phases 8-11 (shipped 2026-01-26)
- [x] **v1.2 GM Table & Campaign** - Phases 12-16 (shipped 2026-01-28)
- [x] **v1.4 GM Manipulation & Concentration** - Phases 17-22 (shipped 2026-01-29)
- [x] **v1.5 NPC Management** - Phases 23-27 (shipped 2026-02-03)
- [x] **v1.6 Batch Character Actions** - Phases 28-31 (shipped 2026-02-05)
- [ ] **v1.7 Combat Tab Rework** - Phases 32-36 (in progress)

## Phases

- [x] **Phase 32: Layout Restructuring** - Replace two-tab combat/defense layout with single compact three-group tab
- [ ] **Phase 33: Attack Actions** - Wire melee and ranged attack buttons into Actions group with anonymous target verification
- [ ] **Phase 34: New Action Types** - Add anonymous action and combat skill check modal
- [ ] **Phase 35: Defense Group** - Wire defend, take damage, and defensive stances into Defense group
- [ ] **Phase 36: Other Group** - Wire medical, rest, implants, reload, and unload into Other group

## Phase Details

### v1.7 Combat Tab Rework (In Progress)

**Milestone Goal:** Consolidate Combat and Defense tabs into a single compact tab with icon+label buttons organized in three groups (Actions, Defense, Other), add anonymous action and skill check features, and verify anonymous target attacks.

#### Phase 32: Layout Restructuring
**Goal**: Player sees a single Combat tab with three clearly labeled button groups instead of the old two-tab layout with large buttons and a skills list
**Depends on**: Nothing (first phase of v1.7)
**Requirements**: LAY-01, LAY-02, LAY-03, LAY-04, LAY-05
**Success Criteria** (what must be TRUE):
  1. Combat tab shows three visually distinct groups labeled Actions, Defense, and Other
  2. All buttons in the three groups use compact icon + short label style (no large buttons remain)
  3. The combat skills list no longer appears on the Combat tab (empty space or placeholder where it was)
  4. Defense tab is gone from the play page tab bar
  5. AP, FAT, and VIT resource summary remains visible on the tab
**Plans**: 2 plans

Plans:
- [x] 32-01-PLAN.md — Add combat tile CSS and restructure TabCombat Default mode (three groups, left panel, compact tiles)
- [x] 32-02-PLAN.md — Remove Defense tab from Play.razor, wire activity log to TabCombat, visual verification

#### Phase 33: Attack Actions
**Goal**: Player can initiate melee and ranged attacks from the Actions group, with verified AV display for solo melee and TV modifier support for solo ranged
**Depends on**: Phase 32
**Requirements**: ACT-01, ACT-02, VER-01, VER-02
**Success Criteria** (what must be TRUE):
  1. Player clicks the melee attack button in Actions group and enters the existing attack mode flow
  2. Player clicks the ranged attack button in Actions group (visible only when ranged weapon equipped) and enters the existing ranged attack mode flow
  3. Solo melee attack against anonymous target displays the Attack Value (AV) to the player
  4. Solo ranged attack against anonymous target allows entering TV modifiers and displays the resulting Success Value (SV)
**Plans**: 2 plans

Plans:
- [ ] 33-01-PLAN.md — Add ranged button tooltip and wire anonymous target selection into melee AttackMode
- [ ] 33-02-PLAN.md — Add anonymous target support to RangedAttackMode with simplified TV input and SV-only result

#### Phase 34: New Action Types
**Goal**: Player can perform attribute-only anonymous actions and skill checks from the Actions group without using the attack workflow
**Depends on**: Phase 32
**Requirements**: ACT-03, ACT-04
**Success Criteria** (what must be TRUE):
  1. Player clicks Anonymous Action, selects an attribute from a dropdown, enters a TV, rolls 4dF+, and sees the result with cost applied (1AP+1FAT or 2AP)
  2. Player clicks Use Skill, sees a modal listing combat skills, selects one, enters a TV, rolls AS + 4dF+ vs TV, and sees the success/failure result
  3. The skill check modal does not trigger an attack -- it is a standalone skill resolution
**Plans**: TBD

Plans:
- [ ] 34-01: TBD

#### Phase 35: Defense Group
**Goal**: Player can defend, take damage, and set stances from the Defense group on the Combat tab
**Depends on**: Phase 32
**Requirements**: DEF-01, DEF-02, DEF-03
**Success Criteria** (what must be TRUE):
  1. Player clicks Defend in the Defense group and enters the existing defense mode flow (passive/dodge/parry/shield block options)
  2. Player clicks Take Damage in the Defense group and enters the existing take-damage mode flow
  3. Player can select a defensive stance (Normal, Parry Mode, Dodge Focus, Block with Shield) from the Defense group
**Plans**: TBD

Plans:
- [ ] 35-01: TBD

#### Phase 36: Other Group
**Goal**: Player can access all utility actions (medical, rest, implants, reload, unload) from the Other group
**Depends on**: Phase 32
**Requirements**: OTH-01, OTH-02, OTH-03, OTH-04, OTH-05
**Success Criteria** (what must be TRUE):
  1. Player clicks Medical in the Other group and enters the existing medical mode flow
  2. Player clicks Rest in the Other group and spends 1 AP to recover 1 FAT
  3. Player clicks Implants in the Other group (visible only when toggleable implants equipped) and enters the existing implant activation flow
  4. Player clicks Reload in the Other group (visible only when ranged weapon equipped) and enters the existing reload flow
  5. Player clicks Unload in the Other group (visible only when weapon has ammo loaded) and enters the existing unload flow
**Plans**: TBD

Plans:
- [ ] 36-01: TBD

## Progress

**Execution Order:**
Phases execute in numeric order: 32 -> 33 -> 34 -> 35 -> 36

Note: Phases 33, 34, 35, 36 all depend on Phase 32 but are independent of each other. Ordering is for consistency, not hard dependency between them.

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|----------------|--------|-----------|
| 32. Layout Restructuring | v1.7 | 2/2 | ✓ Complete | 2026-02-12 |
| 33. Attack Actions | v1.7 | 1/2 | In progress | - |
| 34. New Action Types | v1.7 | 0/TBD | Not started | - |
| 35. Defense Group | v1.7 | 0/TBD | Not started | - |
| 36. Other Group | v1.7 | 0/TBD | Not started | - |

---
*Roadmap created: 2026-02-11*
*Milestone: v1.7 Combat Tab Rework (Issues #104, #105)*
