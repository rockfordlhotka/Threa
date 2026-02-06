# Requirements: Threa TTRPG Assistant

**Defined:** 2026-02-04
**Core Value:** Players and Game Masters can easily access the system, manage their content securely, and focus on gameplay rather than administration.

## v1.6 Requirements

Requirements for Batch Character Actions milestone. Each maps to roadmap phases.

### Selection Infrastructure

- [x] **SEL-01**: GM can multi-select characters via checkboxes on status cards
- [x] **SEL-02**: Dashboard displays selection count ("3 selected")
- [x] **SEL-03**: GM can Select All characters within a section (PCs, Hostile NPCs, etc.)
- [x] **SEL-04**: GM can Deselect All to clear selection
- [x] **SEL-05**: Selected characters have visual selection indicator

### Batch Damage/Healing

- [x] **DMG-01**: GM can apply damage to all selected characters at once
- [x] **DMG-02**: GM can apply healing to all selected characters at once
- [x] **DMG-03**: Batch damage/healing shows success count and any failures
- [x] **DMG-04**: Partial success allowed — characters that succeed are updated, failures reported

### Batch Visibility

- [ ] **VIS-01**: GM can toggle visibility on all selected NPCs at once (reveal/hide)
- [ ] **VIS-02**: Batch visibility shows success count and any failures

### Batch Lifecycle

- [ ] **LIFE-01**: GM can dismiss/archive all selected NPCs at once
- [ ] **LIFE-02**: Batch dismiss shows success count and any failures

### Batch Effects

- [ ] **EFF-01**: GM can add an effect to all selected characters at once
- [ ] **EFF-02**: GM can remove an effect from all selected characters at once
- [ ] **EFF-03**: Batch effect operations show success count and any failures

### Feedback & UX

- [x] **UX-01**: Contextual action bar appears when characters are selected
- [x] **UX-02**: Action bar shows available batch actions
- [x] **UX-03**: Batch results display clear feedback (success/failure with details)
- [x] **UX-04**: Selection clears after successful batch operation (or user can retain)

## Future Requirements

Deferred to later milestones. Tracked but not in current roadmap.

### Selection Enhancements

- **SEL-F01**: Selection persists across tab changes
- **SEL-F02**: Keyboard shortcuts for selection (Ctrl+A, Escape to clear)
- **SEL-F03**: Saved selection groups for recurring encounters

### Batch Actions

- **ACT-F01**: Batch combat actions (attack/defend) — requires individual rolls/targets
- **ACT-F02**: Undo batch operations — requires transaction log

## Out of Scope

Explicitly excluded. Documented to prevent scope creep.

| Feature | Reason |
|---------|--------|
| Batch combat actions (attack/defend) | Too complex — requires individual rolls, targets, defenses per character |
| Undo batch operations | Requires transaction log and complex state management |
| Saved selection groups | Feature creep — disposition grouping already organizes NPCs |
| Individual amounts per character | Defeats purpose of batch — use individual actions instead |
| Batch wound management | Wounds require individual details (location, severity) |
| Batch inventory manipulation | Too granular for batch — use individual actions |

## Traceability

Which phases cover which requirements. Updated during roadmap creation.

| Requirement | Phase | Status |
|-------------|-------|--------|
| SEL-01 | Phase 28 | Complete |
| SEL-02 | Phase 28 | Complete |
| SEL-03 | Phase 28 | Complete |
| SEL-04 | Phase 28 | Complete |
| SEL-05 | Phase 28 | Complete |
| DMG-01 | Phase 29 | Complete |
| DMG-02 | Phase 29 | Complete |
| DMG-03 | Phase 29 | Complete |
| DMG-04 | Phase 29 | Complete |
| VIS-01 | Phase 30 | Pending |
| VIS-02 | Phase 30 | Pending |
| LIFE-01 | Phase 30 | Pending |
| LIFE-02 | Phase 30 | Pending |
| EFF-01 | Phase 31 | Pending |
| EFF-02 | Phase 31 | Pending |
| EFF-03 | Phase 31 | Pending |
| UX-01 | Phase 29 | Complete |
| UX-02 | Phase 29 | Complete |
| UX-03 | Phase 29 | Complete |
| UX-04 | Phase 29 | Complete |

**Coverage:**
- v1.6 requirements: 20 total
- Mapped to phases: 20
- Unmapped: 0

---
*Requirements defined: 2026-02-04*
*Last updated: 2026-02-04 after roadmap creation*
