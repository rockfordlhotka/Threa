# Roadmap: Threa TTRPG Assistant

## Milestones

- [x] **v1.0 Inventory & Equipment System** - Phases 1-7 (shipped 2026-01-26) - [Archive](milestones/v1.0-ROADMAP.md)
- [x] **v1.1 User Management & Authentication** - Phases 8-11 (shipped 2026-01-26) - [Archive](milestones/v1.1-ROADMAP.md)
- [ ] **v1.2 GM Table & Campaign Management** - Phases 12-16 (in progress)

## Phases

<details>
<summary>v1.0 Inventory & Equipment System (Phases 1-7) - SHIPPED 2026-01-26</summary>

See MILESTONES.md for v1.0 details.

**Key accomplishments:**
- Complete CSLA business object foundation with 38 passing unit tests
- GM item template management with full CRUD
- Player inventory system with equipment slot management
- Container system with nesting enforcement
- Item bonuses integrated with combat system
- Real-time GM item distribution
- 52 seed items across all categories

</details>

<details>
<summary>v1.1 User Management & Authentication (Phases 8-11) - SHIPPED 2026-01-26</summary>

See [milestones/v1.1-ROADMAP.md](milestones/v1.1-ROADMAP.md) for full details.

**Key accomplishments:**
- Self-service user registration with first-user-as-admin
- Password recovery via secret Q&A with brute-force protection
- Admin user management with last-admin protection
- User profiles with Gravatar avatars and profanity filtering

**Phases:**
- Phase 8: Registration Foundation (2/2 plans)
- Phase 9: Password Recovery (2/2 plans)
- Phase 10: Admin User Management (2/2 plans)
- Phase 11: User Profiles (2/2 plans)

</details>

### v1.2 GM Table & Campaign Management (In Progress)

**Milestone Goal:** Enable Game Masters to create and manage campaign tables with player character joining, real-time status monitoring, and comprehensive time control.

**Phase Numbering:**
- Integer phases (12, 13, 14...): Planned milestone work
- Decimal phases (12.1, 12.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [x] **Phase 12: Table Foundation** - GM creates and manages campaign tables
- [x] **Phase 13: Join Workflow** - Players request to join, GM reviews and approves
- [x] **Phase 14: Dashboard Core** - Character status cards with health, AP, effects
- [ ] **Phase 15: Dashboard Details** - Detailed views, real-time updates, NPC placeholder
- [ ] **Phase 16: Time Management** - Round-based and calendar time control

## Phase Details

### Phase 12: Table Foundation
**Goal**: GMs can create campaign tables and navigate to manage them
**Depends on**: Nothing (first phase of v1.2)
**Requirements**: TBL-01, TBL-02, TBL-03, TBL-04, TBL-05
**Success Criteria** (what must be TRUE):
  1. GM can create a new campaign table with a name they choose
  2. GM can select Fantasy or Sci-Fi theme when creating campaign
  3. GM can set a starting epoch time for the campaign world
  4. GM can see all campaigns they have created in a list
  5. GM can open a campaign to access its management dashboard
**Plans**: 2 plans

Plans:
- [x] 12-01-PLAN.md - TableInfo extension and CampaignCreate page
- [x] 12-02-PLAN.md - Campaign list view and theme infrastructure

### Phase 13: Join Workflow
**Goal**: Players can request to join campaigns with characters, GMs can review and approve/deny
**Depends on**: Phase 12
**Requirements**: JOIN-01, JOIN-02, JOIN-03, JOIN-04, JOIN-05, JOIN-06, JOIN-07, JOIN-08, JOIN-09, JOIN-10, JOIN-11
**Success Criteria** (what must be TRUE):
  1. Player can browse available campaign tables and select one to join
  2. Player can choose one of their characters and submit a join request
  3. GM sees pending join requests and can view full character details (sheet, inventory, narrative)
  4. GM can approve a request (character becomes active at table) or deny it (character remains unattached)
  5. GM can remove a character from an active table
  6. Character cannot be active in more than one campaign simultaneously
  7. Player receives notification when their join request is approved or denied
**Plans**: 4 plans

Plans:
- [x] 13-01-PLAN.md - Data layer foundation (JoinRequest DTO, DAL, Description field, messaging)
- [x] 13-02-PLAN.md - Business logic (JoinRequestSubmitter, JoinRequestProcessor, JoinRequestList)
- [x] 13-03-PLAN.md - Player UI (BrowseCampaigns, MyJoinRequests pages)
- [x] 13-04-PLAN.md - GM UI (pending badges, review section, character removal)

### Phase 14: Dashboard Core
**Goal**: GM dashboard displays compact status cards for all active characters
**Depends on**: Phase 13
**Requirements**: DASH-01, DASH-02, DASH-03, DASH-04, DASH-05, DASH-06, DASH-07
**Success Criteria** (what must be TRUE):
  1. GM dashboard shows compact cards for all active characters at the table
  2. Each card displays Fatigue and Vitality health pools
  3. Each card displays current wounds and pending damage/healing pools
  4. Each card displays current Action Points
  5. Each card displays count of active effects on the character
**Plans**: 3 plans

Plans:
- [x] 14-01-PLAN.md - Extend TableCharacterInfo with pending pools and status counts
- [x] 14-02-PLAN.md - Create CharacterStatusCard component
- [x] 14-03-PLAN.md - Integrate CharacterStatusCard into GmTable dashboard

### Phase 15: Dashboard Details
**Goal**: GM can view detailed character information and dashboard updates in real-time
**Depends on**: Phase 14
**Requirements**: DASH-08, DASH-09, DASH-10, DASH-11, DASH-12, DASH-13
**Success Criteria** (what must be TRUE):
  1. GM can click a character card to view detailed information
  2. Detailed view shows character sheet (attributes, skills, levels)
  3. Detailed view shows inventory and equipped items
  4. Detailed view shows narrative information (appearance, backstory)
  5. Dashboard automatically updates when any character's state changes (no manual refresh needed)
  6. Dashboard includes labeled placeholder area for future NPC functionality
**Plans**: 3 plans

Plans:
- [ ] 15-01-PLAN.md - Modal foundation, data layer GmNotes, CharacterStatusCard onclick
- [ ] 15-02-PLAN.md - Tab content components (CharacterDetailSheet, Inventory, Narrative)
- [ ] 15-03-PLAN.md - Real-time updates and NPC placeholder

### Phase 16: Time Management
**Goal**: GM can control time flow with multiple increments and round-based mode
**Depends on**: Phase 15
**Requirements**: TIME-01, TIME-02, TIME-03, TIME-04, TIME-05, TIME-06, TIME-07, TIME-08, TIME-09, TIME-10, TIME-11, TIME-12, TIME-13, TIME-14
**Success Criteria** (what must be TRUE):
  1. GM can advance time by round (6 seconds), minute, turn (10 minutes), hour, day, or week
  2. GM can enter and exit "in rounds" mode for detailed combat tracking
  3. Round-by-round advancement is only available when in "in rounds" mode
  4. Time advancement propagates to all characters at table via messaging
  5. Characters automatically process time changes (pending damage/healing, effect expiration, AP recovery)
  6. GM dashboard reflects character state changes after time processing
  7. Player Play page displays "in rounds" indicator when table is in round mode, hidden otherwise
**Plans**: TBD

Plans:
- [ ] 16-01: TBD
- [ ] 16-02: TBD

## Progress

**Execution Order:**
Phases execute in numeric order: 12 -> 12.1 -> 12.2 -> 13 -> etc.

| Phase | Milestone | Plans Complete | Status | Completed |
|-------|-----------|----------------|--------|-----------|
| 1-7 | v1.0 | 16/16 | Complete | 2026-01-26 |
| 8. Registration Foundation | v1.1 | 2/2 | Complete | 2026-01-26 |
| 9. Password Recovery | v1.1 | 2/2 | Complete | 2026-01-26 |
| 10. Admin User Management | v1.1 | 2/2 | Complete | 2026-01-26 |
| 11. User Profiles | v1.1 | 2/2 | Complete | 2026-01-26 |
| 12. Table Foundation | v1.2 | 2/2 | Complete | 2026-01-26 |
| 13. Join Workflow | v1.2 | 4/4 | Complete | 2026-01-27 |
| 14. Dashboard Core | v1.2 | 3/3 | Complete | 2026-01-27 |
| 15. Dashboard Details | v1.2 | 0/3 | Planned | - |
| 16. Time Management | v1.2 | 0/? | Not started | - |

---
*Roadmap created: 2026-01-26*
*Last updated: 2026-01-27 - Phase 15 planned (3 plans)*
