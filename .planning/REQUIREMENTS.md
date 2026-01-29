# Requirements: Threa TTRPG Assistant - v1.3 GM Character Manipulation

**Defined:** 2026-01-28
**Core Value:** Players and Game Masters can easily access the system, manage their content securely, and focus on gameplay rather than administration.

## v1.3 Requirements

Requirements for GM character manipulation milestone. Each maps to roadmap phases.

### Health Management

- [ ] **HLTH-01**: GM can apply damage to character FAT pool (adds to pending damage)
- [ ] **HLTH-02**: GM can apply damage to character VIT pool (adds to pending damage)
- [ ] **HLTH-03**: GM can apply healing to character FAT pool (adds to pending healing)
- [ ] **HLTH-04**: GM can apply healing to character VIT pool (adds to pending healing)
- [ ] **HLTH-05**: GM can specify damage/healing amount via numeric input
- [ ] **HLTH-06**: GM can view current and pending FAT/VIT values before applying changes
- [ ] **HLTH-07**: Damage/healing applies immediately to pending pools and triggers real-time dashboard update

### Wound Management

- [ ] **WOND-01**: GM can add wound to character with severity level and description
- [ ] **WOND-02**: GM can remove wound from character
- [ ] **WOND-03**: GM can edit wound description and severity
- [ ] **WOND-04**: GM can view all active wounds on character with severities

### Effect Management

- [ ] **EFCT-01**: GM can add custom effect to character with name, description, and duration
- [ ] **EFCT-02**: GM can specify effect modifiers (attribute/skill bonuses or penalties)
- [ ] **EFCT-03**: GM can set effect duration in rounds or turns
- [ ] **EFCT-04**: GM can remove active effect from character
- [ ] **EFCT-05**: GM can edit effect duration (extend/reduce time remaining)
- [ ] **EFCT-06**: GM can edit effect modifiers on existing effect
- [ ] **EFCT-07**: GM can view all active effects on character with durations
- [ ] **EFCT-08**: GM can create effect template for reuse (saved preset)
- [ ] **EFCT-09**: GM can apply effect from saved template to character
- [ ] **EFCT-10**: Effect changes trigger real-time dashboard update

### Inventory Manipulation

- [ ] **INVT-01**: GM can add item from template library to character inventory
- [ ] **INVT-02**: GM can specify quantity when adding stackable items
- [ ] **INVT-03**: GM can remove item from character inventory
- [ ] **INVT-04**: GM can modify quantity of stackable items in inventory
- [ ] **INVT-05**: GM can equip item to character equipment slot
- [ ] **INVT-06**: GM can unequip item from character equipment slot
- [ ] **INVT-07**: Inventory changes trigger real-time dashboard update
- [ ] **INVT-08**: GM can view character inventory before making changes

### Stat Editing

- [ ] **STAT-01**: GM can edit character attribute values (STR, DEX, END, INT, ITT, WIL, PHY)
- [ ] **STAT-02**: GM can edit character skill levels
- [ ] **STAT-03**: Attribute changes recalculate dependent stats automatically
- [ ] **STAT-04**: Skill changes recalculate Ability Scores automatically
- [ ] **STAT-05**: Stat changes trigger real-time dashboard update
- [ ] **STAT-06**: GM can view character sheet with current values before editing

### Concentration System

- [ ] **CONC-01**: Casting time concentration effects track progress and execute deferred actions on completion
- [ ] **CONC-02**: Casting time concentration effects fail without executing action when interrupted
- [ ] **CONC-03**: Magazine reload concentration completes and updates weapon when concentration expires naturally
- [ ] **CONC-04**: Magazine reload concentration fails and loses progress when interrupted
- [ ] **CONC-05**: Sustained concentration effects drain FAT/VIT per round while active
- [ ] **CONC-06**: Sustained concentration effects link to spell effects on target(s)
- [ ] **CONC-07**: Breaking sustained concentration removes all linked effects from targets immediately
- [ ] **CONC-08**: Character can only concentrate on one effect at a time
- [ ] **CONC-09**: Taking active actions automatically breaks concentration with confirmation
- [ ] **CONC-10**: Character failing concentration check breaks concentration and removes effect
- [ ] **CONC-11**: Passive defense triggers concentration check using Focus skill vs attacker's AV
- [ ] **CONC-12**: Damage dealt applies penalty to concentration check (-1 per 2 damage)
- [ ] **CONC-13**: Character becoming unconscious/incapacitated automatically breaks concentration
- [ ] **CONC-14**: Concentration effects expire automatically when caster's FAT/VIT reaches 0
- [ ] **CONC-15**: UI displays concentration status with type, progress, and linked effects

## Future Requirements

Deferred to future milestones.

### NPC Management

- **NPC-01**: GM can create NPC with stats and abilities
- **NPC-02**: GM can edit NPC stats and properties
- **NPC-03**: GM can add NPC to campaign table
- **NPC-04**: NPC status appears in dashboard alongside player characters
- **NPC-05**: GM can apply damage/healing to NPCs
- **NPC-06**: GM can add/remove effects on NPCs

### Advanced Features

- **ADV-01**: Initiative tracking with automatic turn order
- **ADV-02**: Automated encounter balancing based on party composition
- **ADV-03**: Session logs and history within campaign
- **ADV-04**: Campaign sharing between multiple GMs

## Out of Scope

Explicitly excluded. Documented to prevent scope creep.

| Feature | Reason |
|---------|--------|
| Player-initiated health manipulation | GM-only power; players manage via combat/rest |
| Direct current pool editing | Pending pools are the safe GM interface; avoids bypassing game rules |
| Bulk effect application (to multiple characters) | Single-character focus for v1.3; batch operations are future enhancement |
| Custom effect formulas/scripting | Predefined modifiers sufficient; scripting adds complexity |
| Item creation from GM panel | Item templates managed separately; GM uses existing templates |
| Character deletion by GM | Characters owned by players; GM can remove from table only |
| Automated effect expiration notifications | Effect durations auto-expire on time skip; notifications are future UX enhancement |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| HLTH-01 | Phase 17 | Complete |
| HLTH-02 | Phase 17 | Complete |
| HLTH-03 | Phase 17 | Complete |
| HLTH-04 | Phase 17 | Complete |
| HLTH-05 | Phase 17 | Complete |
| HLTH-06 | Phase 17 | Complete |
| HLTH-07 | Phase 17 | Complete |
| WOND-01 | Phase 18 | Complete |
| WOND-02 | Phase 18 | Complete |
| WOND-03 | Phase 18 | Complete |
| WOND-04 | Phase 18 | Complete |
| EFCT-01 | Phase 19 | Pending |
| EFCT-02 | Phase 19 | Pending |
| EFCT-03 | Phase 19 | Pending |
| EFCT-04 | Phase 19 | Pending |
| EFCT-05 | Phase 19 | Pending |
| EFCT-06 | Phase 19 | Pending |
| EFCT-07 | Phase 19 | Pending |
| EFCT-08 | Phase 19 | Pending |
| EFCT-09 | Phase 19 | Pending |
| EFCT-10 | Phase 19 | Pending |
| INVT-01 | Phase 20 | Pending |
| INVT-02 | Phase 20 | Pending |
| INVT-03 | Phase 20 | Pending |
| INVT-04 | Phase 20 | Pending |
| INVT-05 | Phase 20 | Pending |
| INVT-06 | Phase 20 | Pending |
| INVT-07 | Phase 20 | Pending |
| INVT-08 | Phase 20 | Pending |
| STAT-01 | Phase 21 | Pending |
| STAT-02 | Phase 21 | Pending |
| STAT-03 | Phase 21 | Pending |
| STAT-04 | Phase 21 | Pending |
| STAT-05 | Phase 21 | Pending |
| STAT-06 | Phase 21 | Pending |
| CONC-01 | Phase 22 | Complete |
| CONC-02 | Phase 22 | Complete |
| CONC-03 | Phase 22 | Complete |
| CONC-04 | Phase 22 | Complete |
| CONC-05 | Phase 22 | Complete |
| CONC-06 | Phase 22 | Complete |
| CONC-07 | Phase 22 | Complete |
| CONC-08 | Phase 22 | Complete |
| CONC-09 | Phase 22 | Complete |
| CONC-10 | Phase 22 | Complete |
| CONC-11 | Phase 22 | Complete |
| CONC-12 | Phase 22 | Complete |
| CONC-13 | Phase 22 | Complete |
| CONC-14 | Phase 22 | Complete |
| CONC-15 | Phase 22 | Complete |

**Coverage:**
- v1.3 requirements: 52 total (37 original + 15 concentration)
- Mapped to phases: 52
- Unmapped: 0

---
*Requirements defined: 2026-01-28*
*Last updated: 2026-01-28 after roadmap creation*
