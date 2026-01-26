# Milestone v1: Threa Inventory & Equipment System

**Status:** ✅ SHIPPED 2026-01-26
**Phases:** 1-7
**Total Plans:** 16

## Overview

This roadmap delivers a complete inventory and equipment management system for the Threa TTRPG assistant. Starting from CSLA business object foundation and seed data, it progresses through GM item management, player inventory (character creation and gameplay), container support, and culminates with item bonuses integrating into the existing combat system. Each phase builds on the previous, creating a functional item system that Game Masters can populate and players can use in combat.

## Phases

### Phase 1: Foundation
**Goal**: ItemTemplate and CharacterItem business objects exist with full CSLA data portal operations, and the database contains example items for testing
**Depends on**: Nothing (first phase)
**Requirements**: DATA-01, DATA-02, DATA-03, DATA-04, DATA-05, DATA-06, DATA-07
**Success Criteria** (what must be TRUE):
  1. ItemTemplateEdit business object can be fetched, saved, and deleted via data portal
  2. CharacterItemEdit business object can be fetched, saved, and deleted via data portal
  3. Database contains at least 15 example items across weapons, armor, ammo, containers, and consumables
  4. Unit tests verify business object CRUD operations work correctly
**Plans:** 3 plans

Plans:
- [x] 01-01: ItemTemplate Validation & Tests
- [x] 01-02: CharacterItem Business Objects
- [x] 01-03: Seed Data Augmentation

**Completed:** 2026-01-24

### Phase 2: GM Item Management
**Goal**: Game Masters can create, edit, browse, search, filter, and manage item templates through the web UI
**Depends on**: Phase 1
**Requirements**: GM-01, GM-02, GM-03, GM-04, GM-05, GM-06, GM-07, GM-08, GM-09, GM-10, GM-11, GM-12
**Success Criteria** (what must be TRUE):
  1. GM can navigate to Item Template management page and see existing templates
  2. GM can create a new item template with all properties (basic, combat, bonuses, effects)
  3. GM can edit any existing template and save changes
  4. GM can filter templates by type and tags, and search by name
  5. GM can deactivate or delete templates from the library
**Plans:** 3 plans

Plans:
- [x] 02-01: Tags Data Layer - Add Tags property to DTO, business objects, and seed data
- [x] 02-02: Item List Page Enhancement - RadzenDataGrid with type filter and debounced search
- [x] 02-03: Edit Page Tabbed Layout - RadzenTabs, Tags input, sticky action bar, search by tags

**Completed:** 2026-01-25

### Phase 3: Character Creation Inventory
**Goal**: Players can browse available items and add them to their starting inventory during character creation
**Depends on**: Phase 2
**Requirements**: INV-01, INV-02, INV-03, INV-04, INV-05, INV-06
**Success Criteria** (what must be TRUE):
  1. Player sees item browser on character creation page
  2. Player can filter items by type and search by name
  3. Player can add items to starting inventory and set quantities for stackable items
  4. Player can remove items from starting inventory before finalizing
**Plans:** 2 plans

Plans:
- [x] 03-01: Split-View Item Browser - RadzenDataGrid browser with type filter and debounced search, single-click to add
- [x] 03-02: Inventory Enhancements - Inline quantity editing for stackable items, STR-based weight display with warnings

**Completed:** 2026-01-25

### Phase 4: Gameplay Inventory Core
**Goal**: Players can view their inventory on the Play page and manage equipped items
**Depends on**: Phase 3
**Requirements**: INV-07, INV-08, INV-09, INV-10, INV-11, INV-12
**Success Criteria** (what must be TRUE):
  1. Inventory tab on Play page shows all character items
  2. Equipment slots display shows currently equipped items
  3. Player can equip items from inventory to appropriate slots
  4. Player can unequip items back to inventory
  5. Player can drop or destroy items from inventory
**Plans:** 2 plans

Plans:
- [x] 04-01: Inventory Grid and Equipment Slots Display - CSS grid inventory tiles, categorized equipment slot list
- [x] 04-02: Selection, Equip, Unequip, and Drop Actions - Two-step equip flow, curse blocking, drop with confirmation

**Completed:** 2026-01-25

### Phase 5: Container System
**Goal**: Players can organize items inside containers with weight and volume limits enforced
**Depends on**: Phase 4
**Requirements**: INV-13, INV-14, INV-15, INV-16, INV-17, INV-18
**Success Criteria** (what must be TRUE):
  1. Player can place items inside container items in their inventory
  2. Player can view contents of any container
  3. Player can remove items from containers back to main inventory
  4. System warns when exceeding container weight/volume capacity (but allows placement per CONTEXT.md) and blocks nesting violations (items in nested containers, non-empty containers into containers)
**Plans:** 2 plans

Plans:
- [x] 05-01: Container UI and Item Placement - Container contents panel, move-to/remove-from container functionality
- [x] 05-02: Container Limit Enforcement - Visual fill indicators, capacity warnings, nesting enforcement, drop confirmation

**Completed:** 2026-01-25

### Phase 6: Item Bonuses & Combat
**Goal**: Equipped items provide stat bonuses and integrate with the combat system
**Depends on**: Phase 4
**Requirements**: BONUS-01, BONUS-02, BONUS-03, BONUS-04, BONUS-05, BONUS-06, BONUS-07, BONUS-08, BONUS-09, BONUS-10
**Success Criteria** (what must be TRUE):
  1. Equipped items with skill bonuses increase character Ability Scores
  2. Equipped items with attribute modifiers increase base attributes (cascading to skills)
  3. Multiple item bonuses stack additively
  4. Unequipping removes all bonuses from calculations
  5. Combat attack resolution uses equipped weapon properties (damage class, SV/AV modifiers)
  6. Equipped armor provides absorption values during damage resolution
  7. Equipped weapons appear in appropriate combat mode weapon selection (melee/ranged)
**Plans:** 3 plans

Plans:
- [x] 06-01: ItemBonusCalculator service with unit tests
- [x] 06-02: CharacterEdit integration and UI stat display with breakdowns
- [x] 06-03: Combat system integration (weapon selection, armor absorption)

**Completed:** 2026-01-25

### Phase 7: Item Distribution
**Goal**: Game Masters can grant items to players during gameplay sessions
**Depends on**: Phase 4
**Requirements**: DIST-01, DIST-02, DIST-03
**Success Criteria** (what must be TRUE):
  1. GM can see list of players at current game table
  2. GM can select item template and quantity to grant
  3. GM can grant item to specific player character
  4. Granted items appear immediately in player inventory
**Plans:** 1 plan

Plans:
- [x] 07-01: Add Item Distribution panel to GM Table with real-time player notification

**Completed:** 2026-01-26

## Progress

**Execution Order:**
Phases execute in numeric order: 1 -> 2 -> 3 -> 4 -> 5 -> 6 -> 7

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Foundation | 3/3 | Complete | 2026-01-24 |
| 2. GM Item Management | 3/3 | Complete | 2026-01-25 |
| 3. Character Creation Inventory | 2/2 | Complete | 2026-01-25 |
| 4. Gameplay Inventory Core | 2/2 | Complete | 2026-01-25 |
| 5. Container System | 2/2 | Complete | 2026-01-25 |
| 6. Item Bonuses & Combat | 3/3 | Complete | 2026-01-25 |
| 7. Item Distribution | 1/1 | Complete | 2026-01-26 |

---

## Milestone Summary

**Key Decisions:**

- Build on existing DAL interfaces (no refactoring) - proven approach
- GM manages templates, players get instances - clear separation
- Container support in v1 - foundational for UX
- Bonus calculation on equip/unequip only - performance optimization
- Tags as comma-separated string - simple and effective
- CSS Grid for inventory tiles - better than RadzenDataGrid for this UI
- Two-step equip flow - prevents accidents
- Container nesting: one level, empty only - enforced with validation
- ItemBonusCalculator service - clean separation, testable
- CharacterUpdateMessage for inventory changes - reuse infrastructure

**Issues Resolved:**

- CSLA business object validation patterns established
- Radzen component integration (RadzenDataGrid, RadzenTabs)
- Real-time messaging for inventory changes
- Container capacity and nesting rules implemented
- Item bonus calculation and combat integration

**Technical Debt Incurred:**

- ArmorInfoFactory.cs orphaned (duplicate logic in DamageResolution.razor)
- Weapon filtering logic in UI layer (should move to GameMechanics)
- Case sensitivity inconsistencies in skill/template comparisons
- OnCharacterChanged callback not wired in Play.razor

---

*For current project status, see .planning/PROJECT.md and .planning/MILESTONES.md*

---

*Created: 2026-01-24*
*Shipped: 2026-01-26*
*Archived: 2026-01-26*
