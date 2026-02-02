# Requirements: Threa NPC Management System

**Defined:** 2026-02-01
**Core Value:** Players and Game Masters can easily access the system, manage their content securely, and focus on gameplay rather than administration.

## v1.5 Requirements

Requirements for NPC Management System. Each maps to roadmap phases.

### NPC Templates

- [x] **TMPL-01**: GM can create NPC templates with full character stats (attributes, skills, equipment)
- [x] **TMPL-02**: GM can edit existing NPC templates
- [x] **TMPL-03**: GM can browse/search NPC templates (filter by category/tags)
- [x] **TMPL-04**: GM can delete/deactivate NPC templates
- [x] **TMPL-05**: NPC templates support category tags for organization (humanoid, beast, undead, etc.)

### NPC Creation

- [ ] **CRTN-01**: GM can quick-create NPC from template (spawn instance to active table)
- [ ] **CRTN-02**: NPCs have full character stats (same model as PCs — attributes, skills, equipment, effects, wounds)
- [ ] **CRTN-03**: Smart naming auto-generates unique names ("Goblin 1", "Goblin 2")
- [ ] **CRTN-04**: GM can add session-specific notes to individual NPC instances

### Dashboard Display

- [ ] **DASH-01**: NPCs appear in GM dashboard in separate section from PCs
- [ ] **DASH-02**: NPC status cards show same info as PC cards (FAT/VIT/wounds, AP, effects)
- [ ] **DASH-03**: NPC detail modal provides same manipulation powers as PCs (health, wounds, effects, inventory, stats)
- [ ] **DASH-04**: NPCs display disposition marker (Hostile/Neutral/Friendly) with visual differentiation

### Visibility & Surprise

- [ ] **VSBL-01**: GM can toggle NPC visibility (hide/reveal for surprise encounters)
- [ ] **VSBL-02**: Hidden NPCs do not appear in player-visible views
- [ ] **VSBL-03**: Visibility toggle is accessible from both status card and detail modal

### NPC Lifecycle

- [ ] **LIFE-01**: GM can remove/dismiss NPCs from active table
- [ ] **LIFE-02**: NPCs persist across sessions until explicitly dismissed
- [ ] **LIFE-03**: GM can save active NPC as new template (capture current state for reuse)
- [ ] **LIFE-04**: Dismissed NPCs can be optionally deleted or archived

### Time & Combat Integration

- [ ] **CMBT-01**: NPCs participate in round/time advancement (effects expire, AP recovers)
- [ ] **CMBT-02**: NPCs can be targeted by combat actions
- [ ] **CMBT-03**: Time advancement applies to NPCs same as PCs

## Future Requirements

Deferred to later milestones.

### Group Management (v1.6+)

- **GRPM-01**: GM can select multiple NPCs for batch actions
- **GRPM-02**: GM can apply damage/healing to multiple NPCs at once
- **GRPM-03**: GM can apply effects to multiple NPCs at once
- **GRPM-04**: NPCs can be assigned to named groups

### Initiative System (v1.6+)

- **INIT-01**: Initiative tracking with automatic turn order
- **INIT-02**: Roll initiative for all hostile NPCs at once
- **INIT-03**: Group identical NPCs in initiative order

## Out of Scope

Explicitly excluded. Documented to prevent scope creep.

| Feature | Reason |
|---------|--------|
| Simplified NPC stat blocks | Full character model is already designed; creating second model doubles maintenance |
| AI-powered NPC generation | Scope creep; not core to management workflow; adds external dependencies |
| NPC-to-PC conversion | Edge case that complicates data model; copy manually if needed |
| Automatic CR calculation | Threa has different power scaling than D&D; complex to implement correctly |
| NPC loot tables | Feature creep; GM grants items manually using existing distribution system |
| Map/token integration | Threa is not a VTT; no map system exists |
| Voice/personality AI | Far outside core scope; GM manages roleplay manually |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| TMPL-01 | Phase 24 | Complete |
| TMPL-02 | Phase 24 | Complete |
| TMPL-03 | Phase 24 | Complete |
| TMPL-04 | Phase 24 | Complete |
| TMPL-05 | Phase 24 | Complete |
| CRTN-01 | Phase 25 | Pending |
| CRTN-02 | Phase 23, 25 | Pending |
| CRTN-03 | Phase 25 | Pending |
| CRTN-04 | Phase 25 | Pending |
| DASH-01 | Phase 25 | Pending |
| DASH-02 | Phase 25 | Pending |
| DASH-03 | Phase 25 | Pending |
| DASH-04 | Phase 25 | Pending |
| VSBL-01 | Phase 26 | Pending |
| VSBL-02 | Phase 26 | Pending |
| VSBL-03 | Phase 26 | Pending |
| LIFE-01 | Phase 26 | Pending |
| LIFE-02 | Phase 26 | Pending |
| LIFE-03 | Phase 26 | Pending |
| LIFE-04 | Phase 26 | Pending |
| CMBT-01 | Phase 27 | Pending |
| CMBT-02 | Phase 27 | Pending |
| CMBT-03 | Phase 27 | Pending |

**Coverage:**
- v1.5 requirements: 23 total
- Mapped to phases: 23
- Unmapped: 0

---
*Requirements defined: 2026-02-01*
*Last updated: 2026-02-02 after Phase 24 completion*
