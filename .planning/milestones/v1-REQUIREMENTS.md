# Requirements Archive: v1.0 Threa Inventory & Equipment System

**Archived:** 2026-01-26
**Status:** ✅ SHIPPED

This is the archived requirements specification for v1.0.
For current requirements, see `.planning/REQUIREMENTS.md` (will be created for next milestone).

---

# Requirements

## v1 Requirements

### GM Item Management

- [x] **GM-01**: GM can create new item template with basic properties (name, description, type, equipment slot, weight, volume, value)
- [x] **GM-02**: GM can set combat properties on item templates (damage class, damage type, SV modifier, AV modifier, range, fire modes, ammo capacity)
- [x] **GM-03**: GM can add skill bonuses to item templates (specify skill name and bonus amount)
- [x] **GM-04**: GM can add attribute modifiers to item templates (specify attribute and modifier amount)
- [x] **GM-05**: GM can add special effects to item templates (equipped effects, on-use effects, trigger conditions)
- [x] **GM-06**: GM can edit existing item templates and save changes
- [x] **GM-07**: GM can browse item template library filtered by item type (weapon, armor, ammo, consumable, container, misc)
- [x] **GM-08**: GM can search item templates by name
- [x] **GM-09**: GM can add custom tags to item templates for categorization
- [x] **GM-10**: GM can filter item templates by tags
- [x] **GM-11**: GM can deactivate item templates to hide them from player selection
- [x] **GM-12**: GM can delete item templates that have never been instantiated

### Player Inventory - Character Creation

- [x] **INV-01**: Player can browse available item templates during character creation
- [x] **INV-02**: Player can filter item templates by type during character creation
- [x] **INV-03**: Player can search item templates by name during character creation
- [x] **INV-04**: Player can add item instance to character starting inventory from template
- [x] **INV-05**: Player can remove item from starting inventory before finalizing character
- [x] **INV-06**: Player can set initial quantity for stackable items

### Player Inventory - Gameplay

- [x] **INV-07**: Player can view all items in their inventory on Play page
- [x] **INV-08**: Player can view all equipped items in equipment slots on Play page
- [x] **INV-09**: Player can equip item from inventory to appropriate equipment slot
- [x] **INV-10**: Player can unequip item from equipment slot back to inventory
- [x] **INV-11**: Player can drop item from inventory (permanently removes from character)
- [x] **INV-12**: Player can destroy item from inventory (permanently removes from character)
- [x] **INV-13**: Player can place item inside container item in inventory
- [x] **INV-14**: Player can remove item from container back to main inventory
- [x] **INV-15**: Player can view items contained within a container
- [x] **INV-16**: Container weight limits are enforced (cannot exceed container max weight)
- [x] **INV-17**: Container volume limits are enforced (cannot exceed container max volume)
- [x] **INV-18**: Container type restrictions are enforced (only allowed item types can be stored)

### Item Distribution

- [x] **DIST-01**: GM can grant item instance to player character by selecting template and quantity
- [x] **DIST-02**: GM can see which characters are in the current game table for item distribution
- [x] **DIST-03**: Granted items appear in player inventory immediately

### Item Bonuses & Integration

- [x] **BONUS-01**: When item with skill bonuses is equipped, bonuses apply to character's Ability Score calculations
- [x] **BONUS-02**: When item with attribute modifiers is equipped, modifiers apply to character's base attributes
- [x] **BONUS-03**: Attribute modifiers cascade to all skills using that attribute
- [x] **BONUS-04**: Multiple items with same skill bonus stack additively
- [x] **BONUS-05**: Multiple items with same attribute modifier stack additively
- [x] **BONUS-06**: When item is unequipped, bonuses are removed from character calculations
- [x] **BONUS-07**: Combat system uses equipped weapon properties (damage class, SV/AV modifiers) in attack resolution
- [x] **BONUS-08**: Equipped armor provides absorption values for damage resolution
- [x] **BONUS-09**: Equipped ranged weapons appear in ranged attack mode weapon selection
- [x] **BONUS-10**: Equipped melee weapons appear in melee attack mode weapon selection

### Test Data

- [x] **DATA-01**: Database includes 3-5 example melee weapons (swords, axes, daggers) with varied damage classes
- [x] **DATA-02**: Database includes 3-5 example ranged weapons (bows, crossbows) with range and damage properties
- [x] **DATA-03**: Database includes 3-5 example firearms (pistols, rifles, automatic weapons) with fire modes and ammo capacity
- [x] **DATA-04**: Database includes 2-3 example armor pieces (helmets, chest armor, shields) with absorption values
- [x] **DATA-05**: Database includes 2-3 example ammo types with damage modifiers
- [x] **DATA-06**: Database includes 1-2 example containers (backpack, pouch) with weight/volume limits
- [x] **DATA-07**: Database includes 1-2 example consumables (potions, grenades)

## v2 Requirements (Deferred)

- [ ] **TRADE-01**: Player can initiate trade with another player in same game table
- [ ] **TRADE-02**: Player can select items to offer in trade
- [ ] **TRADE-03**: Player can accept or reject trade offer
- [ ] **TRADE-04**: Items transfer between player inventories when trade is accepted
- [ ] **CRAFT-01**: Player can combine items to create new items via crafting recipes
- [ ] **DUR-01**: Item durability degrades with use during combat
- [ ] **DUR-02**: Player can repair damaged items to restore durability
- [ ] **VENDOR-01**: GM can create vendor NPCs with item inventories
- [ ] **VENDOR-02**: Player can buy items from vendors
- [ ] **VENDOR-03**: Player can sell items to vendors for currency

## Out of Scope

- **Item enchanting/upgrading** — Future enhancement; single-tier items sufficient for v1
- **Item set bonuses** — Complex bonus calculation; individual item bonuses only
- **Visual item icons/sprites** — Text-based UI sufficient; icons are cosmetic enhancement
- **Currency/economy system** — Item value fields exist but no transaction system needed yet
- **Item identification/lore** — Simple name/description sufficient; no hidden properties
- **Quest items or unique items** — Template system handles all items; no special item logic
- **Weight encumbrance penalties** — Weight tracked but no movement/stat penalties yet
- **Automatic loot distribution** — GM manually grants items; no dice-based loot tables

## Traceability

### Phase Mapping

| Requirement | Phase | Status |
|-------------|-------|--------|
| DATA-01 | Phase 1: Foundation | Complete |
| DATA-02 | Phase 1: Foundation | Complete |
| DATA-03 | Phase 1: Foundation | Complete |
| DATA-04 | Phase 1: Foundation | Complete |
| DATA-05 | Phase 1: Foundation | Complete |
| DATA-06 | Phase 1: Foundation | Complete |
| DATA-07 | Phase 1: Foundation | Complete |
| GM-01 | Phase 2: GM Item Management | Complete |
| GM-02 | Phase 2: GM Item Management | Complete |
| GM-03 | Phase 2: GM Item Management | Complete |
| GM-04 | Phase 2: GM Item Management | Complete |
| GM-05 | Phase 2: GM Item Management | Complete |
| GM-06 | Phase 2: GM Item Management | Complete |
| GM-07 | Phase 2: GM Item Management | Complete |
| GM-08 | Phase 2: GM Item Management | Complete |
| GM-09 | Phase 2: GM Item Management | Complete |
| GM-10 | Phase 2: GM Item Management | Complete |
| GM-11 | Phase 2: GM Item Management | Complete |
| GM-12 | Phase 2: GM Item Management | Complete |
| INV-01 | Phase 3: Character Creation Inventory | Complete |
| INV-02 | Phase 3: Character Creation Inventory | Complete |
| INV-03 | Phase 3: Character Creation Inventory | Complete |
| INV-04 | Phase 3: Character Creation Inventory | Complete |
| INV-05 | Phase 3: Character Creation Inventory | Complete |
| INV-06 | Phase 3: Character Creation Inventory | Complete |
| INV-07 | Phase 4: Gameplay Inventory Core | Complete |
| INV-08 | Phase 4: Gameplay Inventory Core | Complete |
| INV-09 | Phase 4: Gameplay Inventory Core | Complete |
| INV-10 | Phase 4: Gameplay Inventory Core | Complete |
| INV-11 | Phase 4: Gameplay Inventory Core | Complete |
| INV-12 | Phase 4: Gameplay Inventory Core | Complete |
| INV-13 | Phase 5: Container System | Complete |
| INV-14 | Phase 5: Container System | Complete |
| INV-15 | Phase 5: Container System | Complete |
| INV-16 | Phase 5: Container System | Complete |
| INV-17 | Phase 5: Container System | Complete |
| INV-18 | Phase 5: Container System | Complete |
| BONUS-01 | Phase 6: Item Bonuses & Combat | Complete |
| BONUS-02 | Phase 6: Item Bonuses & Combat | Complete |
| BONUS-03 | Phase 6: Item Bonuses & Combat | Complete |
| BONUS-04 | Phase 6: Item Bonuses & Combat | Complete |
| BONUS-05 | Phase 6: Item Bonuses & Combat | Complete |
| BONUS-06 | Phase 6: Item Bonuses & Combat | Complete |
| BONUS-07 | Phase 6: Item Bonuses & Combat | Complete |
| BONUS-08 | Phase 6: Item Bonuses & Combat | Complete |
| BONUS-09 | Phase 6: Item Bonuses & Combat | Complete |
| BONUS-10 | Phase 6: Item Bonuses & Combat | Complete |
| DIST-01 | Phase 7: Item Distribution | Complete |
| DIST-02 | Phase 7: Item Distribution | Complete |
| DIST-03 | Phase 7: Item Distribution | Complete |

**Coverage Summary:** 50/50 v1 requirements complete (100%)

---

## Milestone Summary

**Shipped:** 50 of 50 v1 requirements
**Adjusted:** None (all requirements delivered as originally specified)
**Dropped:** Player-to-player trading (TRADE-01 to TRADE-04) deferred to v2

**Implementation Highlights:**
- 52 seed items (247% over minimum 21 items)
- 38 unit tests across ItemTemplate, CharacterItem, and ItemBonusCalculator
- Real-time messaging infrastructure for inventory updates
- Container system with nesting enforcement and capacity tracking
- Full combat integration with weapon selection and armor absorption

---

*Archived: 2026-01-26 as part of v1.0 milestone completion*
