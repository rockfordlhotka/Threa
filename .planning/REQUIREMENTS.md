# Requirements: Threa TTRPG Assistant

**Defined:** 2026-01-26
**Core Value:** Players and Game Masters can easily access the system, manage their content securely, and focus on gameplay rather than administration.

## v1.2 Requirements

Requirements for milestone v1.2: GM Table & Campaign Management

### Table Management

- [x] **TBL-01**: GM can create new campaign table with name
- [x] **TBL-02**: GM can select theme for campaign (Fantasy or Sci-Fi)
- [x] **TBL-03**: GM can set epoch start time when creating campaign
- [x] **TBL-04**: GM can view list of all their campaigns
- [x] **TBL-05**: GM can open/access a specific campaign to manage it

### Character Joining

- [x] **JOIN-01**: Player can browse available campaign tables
- [x] **JOIN-02**: Player can select character to join a campaign table
- [x] **JOIN-03**: Player can submit join request for character to table
- [x] **JOIN-04**: GM can view list of pending join requests for their table
- [x] **JOIN-05**: GM can view full character details from join request (sheet, inventory, narrative)
- [x] **JOIN-06**: GM can approve join request (character becomes active at table)
- [x] **JOIN-07**: GM can deny join request (character remains unattached)
- [x] **JOIN-08**: GM can remove character from active table
- [x] **JOIN-09**: Character can only be active in one campaign at a time
- [x] **JOIN-10**: Player receives notification when join request is approved
- [x] **JOIN-11**: Player receives notification when join request is denied

### Dashboard Display

- [ ] **DASH-01**: GM dashboard displays all active characters at table in compact cards
- [ ] **DASH-02**: Character card shows current Fatigue and Vitality
- [ ] **DASH-03**: Character card shows current wounds
- [ ] **DASH-04**: Character card shows pending damage pool
- [ ] **DASH-05**: Character card shows pending healing pool
- [ ] **DASH-06**: Character card shows current Action Points
- [ ] **DASH-07**: Character card shows count of active effects
- [ ] **DASH-08**: GM can click character card to view detailed character information
- [ ] **DASH-09**: Detailed view displays character sheet (attributes, skills, levels)
- [ ] **DASH-10**: Detailed view displays character inventory and equipped items
- [ ] **DASH-11**: Detailed view displays character narrative (appearance, backstory)
- [ ] **DASH-12**: Dashboard automatically updates when character state changes
- [ ] **DASH-13**: Dashboard includes placeholder for NPCs (labeled for future implementation)

### Time Management

- [ ] **TIME-01**: GM can advance time by 1 round (6 seconds)
- [ ] **TIME-02**: GM can advance time by 1 minute
- [ ] **TIME-03**: GM can advance time by 1 turn (10 minutes)
- [ ] **TIME-04**: GM can advance time by 1 hour
- [ ] **TIME-05**: GM can advance time by 1 day
- [ ] **TIME-06**: GM can advance time by 1 week
- [ ] **TIME-07**: GM can enter "in rounds" mode for combat/detailed tracking
- [ ] **TIME-08**: GM can exit "in rounds" mode to return to normal time flow
- [ ] **TIME-09**: Round-by-round button is only enabled when in "in rounds" mode
- [ ] **TIME-10**: Time advancement triggers message to all characters at table via RabbitMQ
- [ ] **TIME-11**: Characters process time advancement (apply pending damage/healing, expire effects, recover AP)
- [ ] **TIME-12**: GM dashboard reflects character updates after time processing completes
- [ ] **TIME-13**: Player Play page displays "in rounds" indicator when table is in round mode
- [ ] **TIME-14**: Player Play page hides "in rounds" indicator when table is in normal mode

## v1.3+ Future Requirements

Deferred to future milestones. Tracked but not in current roadmap.

### GM Character Manipulation

- **GMCHAR-01**: GM can apply damage to character (add to pending damage pool)
- **GMCHAR-02**: GM can apply healing to character (add to pending healing pool)
- **GMCHAR-03**: GM can apply damage/healing to multiple characters at once
- **GMCHAR-04**: GM can add effect to character
- **GMCHAR-05**: GM can configure effect properties (duration, modifiers, expiration)
- **GMCHAR-06**: GM can add effect to multiple characters at once
- **GMCHAR-07**: GM can remove effect from character
- **GMCHAR-08**: GM can add item to character inventory
- **GMCHAR-09**: GM can remove item from character inventory
- **GMCHAR-10**: GM can edit item properties (damage, bonuses, durability)
- **GMCHAR-11**: GM can edit character attributes outside normal restrictions
- **GMCHAR-12**: GM can grant/remove XP from character

### NPC Management

- **NPC-01**: GM can create NPC with stats and abilities
- **NPC-02**: GM can edit NPC properties
- **NPC-03**: GM can add NPC to active table
- **NPC-04**: GM can remove NPC from table
- **NPC-05**: NPC appears in GM dashboard with same status display as characters
- **NPC-06**: GM can assign inventory to NPC
- **NPC-07**: GM can apply damage/healing/effects to NPC

## Out of Scope

Explicitly excluded. Documented to prevent scope creep.

| Feature | Reason |
|---------|--------|
| Session logs/history within campaign | Focus on real-time management first; historical tracking is future enhancement |
| Campaign sharing between GMs | Single GM per campaign simplifies permissions and ownership |
| Player chat/messaging within table | Out-of-game communication tools already exist (Discord, etc.) |
| Automated encounter balancing | Complex feature requiring significant game balance data |
| Initiative tracking automation | Current system has round advancement; initiative order is v1.3+ |
| Map/grid integration | Visual tactical features are separate major initiative |
| Dice rolling from GM dashboard | Players roll dice; GM uses resolver tools for NPCs (future) |
| Character templates/archetypes | Character creation feature, not table management |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| TBL-01 | Phase 12 | Complete |
| TBL-02 | Phase 12 | Complete |
| TBL-03 | Phase 12 | Complete |
| TBL-04 | Phase 12 | Complete |
| TBL-05 | Phase 12 | Complete |
| JOIN-01 | Phase 13 | Complete |
| JOIN-02 | Phase 13 | Complete |
| JOIN-03 | Phase 13 | Complete |
| JOIN-04 | Phase 13 | Complete |
| JOIN-05 | Phase 13 | Complete |
| JOIN-06 | Phase 13 | Complete |
| JOIN-07 | Phase 13 | Complete |
| JOIN-08 | Phase 13 | Complete |
| JOIN-09 | Phase 13 | Complete |
| JOIN-10 | Phase 13 | Complete |
| JOIN-11 | Phase 13 | Complete |
| DASH-01 | Phase 14 | Complete |
| DASH-02 | Phase 14 | Complete |
| DASH-03 | Phase 14 | Complete |
| DASH-04 | Phase 14 | Complete |
| DASH-05 | Phase 14 | Complete |
| DASH-06 | Phase 14 | Complete |
| DASH-07 | Phase 14 | Complete |
| DASH-08 | Phase 15 | Pending |
| DASH-09 | Phase 15 | Pending |
| DASH-10 | Phase 15 | Pending |
| DASH-11 | Phase 15 | Pending |
| DASH-12 | Phase 15 | Pending |
| DASH-13 | Phase 15 | Pending |
| TIME-01 | Phase 16 | Pending |
| TIME-02 | Phase 16 | Pending |
| TIME-03 | Phase 16 | Pending |
| TIME-04 | Phase 16 | Pending |
| TIME-05 | Phase 16 | Pending |
| TIME-06 | Phase 16 | Pending |
| TIME-07 | Phase 16 | Pending |
| TIME-08 | Phase 16 | Pending |
| TIME-09 | Phase 16 | Pending |
| TIME-10 | Phase 16 | Pending |
| TIME-11 | Phase 16 | Pending |
| TIME-12 | Phase 16 | Pending |
| TIME-13 | Phase 16 | Pending |
| TIME-14 | Phase 16 | Pending |

**Coverage:**
- v1.2 requirements: 43 total
- Mapped to phases: 43
- Unmapped: 0

---
*Requirements defined: 2026-01-26*
*Last updated: 2026-01-26 - Traceability updated for v1.2 roadmap*
