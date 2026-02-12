# Requirements: Threa TTRPG Assistant

**Defined:** 2026-02-11
**Core Value:** Players and Game Masters can easily access the system, manage their content securely, and focus on gameplay rather than administration.

## v1.7 Requirements

Requirements for Combat Tab Rework milestone (Issues #104, #105).

### Layout

- [ ] **LAY-01**: Combat tab displays three distinct button groups: Actions, Defense, and Other
- [ ] **LAY-02**: All action buttons use compact style with icon + short label (replacing current large buttons)
- [ ] **LAY-03**: Combat skills list is removed from permanent display on Combat tab (replaced by Use Skill modal)
- [ ] **LAY-04**: Defense tab is removed from the play page tab list
- [ ] **LAY-05**: Resource summary (AP/FAT/VIT) remains visible on the combined tab

### Actions Group

- [ ] **ACT-01**: Player can initiate a melee attack from the Actions group
- [ ] **ACT-02**: Player can initiate a ranged attack from the Actions group (when ranged weapon equipped)
- [ ] **ACT-03**: Player can take an anonymous action by selecting an attribute, entering a TV, and rolling 4dF+ (costs 1AP+1FAT or 2AP, no skill applied)
- [ ] **ACT-04**: Player can open a combat skill picker modal to perform a skill check (AS + 4dF+ vs TV entered by player)

### Defense Group

- [ ] **DEF-01**: Player can initiate defense (passive/dodge/parry/shield block) from the Defense group
- [ ] **DEF-02**: Player can take arbitrary damage from the Defense group
- [ ] **DEF-03**: Player can set defensive stance (Normal, Parry Mode, Dodge Focus, Block with Shield) from the Defense group

### Other Group

- [ ] **OTH-01**: Player can initiate medical actions from the Other group
- [ ] **OTH-02**: Player can rest (spend 1 AP to recover 1 FAT) from the Other group
- [ ] **OTH-03**: Player can activate/deactivate implants from the Other group (when toggleable implants equipped)
- [ ] **OTH-04**: Player can reload ranged weapons from the Other group (when ranged weapon equipped)
- [ ] **OTH-05**: Player can unload ammo from weapons from the Other group (when weapon has ammo)

### Verification (Issue #105)

- [ ] **VER-01**: Solo melee attack against anonymous target displays the Attack Value (AV)
- [ ] **VER-02**: Solo ranged attack against anonymous target allows entering TV modifiers and displays the Success Value (SV)

## Future Requirements

Deferred to later milestones.

### Combat Enhancements

- **INIT-01**: Initiative tracking with automatic turn order
- **ENC-01**: Automated encounter balancing based on party composition

## Out of Scope

Explicitly excluded from this milestone.

| Feature | Reason |
|---------|--------|
| Armor summary display | Defense tab armor info is placeholder/TODO — defer to future milestone |
| Incoming attack response UI | Defense tab attack response is placeholder — defer to future milestone |
| Batch combat actions | Too complex, requires individual rolls/targets |
| Keyboard shortcuts for actions | Power user feature, add after core rework validated |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| LAY-01 | — | Pending |
| LAY-02 | — | Pending |
| LAY-03 | — | Pending |
| LAY-04 | — | Pending |
| LAY-05 | — | Pending |
| ACT-01 | — | Pending |
| ACT-02 | — | Pending |
| ACT-03 | — | Pending |
| ACT-04 | — | Pending |
| DEF-01 | — | Pending |
| DEF-02 | — | Pending |
| DEF-03 | — | Pending |
| OTH-01 | — | Pending |
| OTH-02 | — | Pending |
| OTH-03 | — | Pending |
| OTH-04 | — | Pending |
| OTH-05 | — | Pending |
| VER-01 | — | Pending |
| VER-02 | — | Pending |

**Coverage:**
- v1.7 requirements: 18 total
- Mapped to phases: 0
- Unmapped: 18

---
*Requirements defined: 2026-02-11*
*Last updated: 2026-02-11 after initial definition*
