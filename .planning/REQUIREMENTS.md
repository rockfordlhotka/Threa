# Requirements

## v1 Requirements

### GM Item Management

- [ ] **GM-01**: GM can create new item template with basic properties (name, description, type, equipment slot, weight, volume, value)
- [ ] **GM-02**: GM can set combat properties on item templates (damage class, damage type, SV modifier, AV modifier, range, fire modes, ammo capacity)
- [ ] **GM-03**: GM can add skill bonuses to item templates (specify skill name and bonus amount)
- [ ] **GM-04**: GM can add attribute modifiers to item templates (specify attribute and modifier amount)
- [ ] **GM-05**: GM can add special effects to item templates (equipped effects, on-use effects, trigger conditions)
- [ ] **GM-06**: GM can edit existing item templates and save changes
- [ ] **GM-07**: GM can browse item template library filtered by item type (weapon, armor, ammo, consumable, container, misc)
- [ ] **GM-08**: GM can search item templates by name
- [ ] **GM-09**: GM can add custom tags to item templates for categorization
- [ ] **GM-10**: GM can filter item templates by tags
- [ ] **GM-11**: GM can deactivate item templates to hide them from player selection
- [ ] **GM-12**: GM can delete item templates that have never been instantiated

### Player Inventory - Character Creation

- [ ] **INV-01**: Player can browse available item templates during character creation
- [ ] **INV-02**: Player can filter item templates by type during character creation
- [ ] **INV-03**: Player can search item templates by name during character creation
- [ ] **INV-04**: Player can add item instance to character starting inventory from template
- [ ] **INV-05**: Player can remove item from starting inventory before finalizing character
- [ ] **INV-06**: Player can set initial quantity for stackable items

### Player Inventory - Gameplay

- [ ] **INV-07**: Player can view all items in their inventory on Play page
- [ ] **INV-08**: Player can view all equipped items in equipment slots on Play page
- [ ] **INV-09**: Player can equip item from inventory to appropriate equipment slot
- [ ] **INV-10**: Player can unequip item from equipment slot back to inventory
- [ ] **INV-11**: Player can drop item from inventory (permanently removes from character)
- [ ] **INV-12**: Player can destroy item from inventory (permanently removes from character)
- [ ] **INV-13**: Player can place item inside container item in inventory
- [ ] **INV-14**: Player can remove item from container back to main inventory
- [ ] **INV-15**: Player can view items contained within a container
- [ ] **INV-16**: Container weight limits are enforced (cannot exceed container max weight)
- [ ] **INV-17**: Container volume limits are enforced (cannot exceed container max volume)
- [ ] **INV-18**: Container type restrictions are enforced (only allowed item types can be stored)

### Item Distribution

- [ ] **DIST-01**: GM can grant item instance to player character by selecting template and quantity
- [ ] **DIST-02**: GM can see which characters are in the current game table for item distribution
- [ ] **DIST-03**: Granted items appear in player inventory immediately

### Item Bonuses & Integration

- [ ] **BONUS-01**: When item with skill bonuses is equipped, bonuses apply to character's Ability Score calculations
- [ ] **BONUS-02**: When item with attribute modifiers is equipped, modifiers apply to character's base attributes
- [ ] **BONUS-03**: Attribute modifiers cascade to all skills using that attribute
- [ ] **BONUS-04**: Multiple items with same skill bonus stack additively
- [ ] **BONUS-05**: Multiple items with same attribute modifier stack additively
- [ ] **BONUS-06**: When item is unequipped, bonuses are removed from character calculations
- [ ] **BONUS-07**: Combat system uses equipped weapon properties (damage class, SV/AV modifiers) in attack resolution
- [ ] **BONUS-08**: Equipped armor provides absorption values for damage resolution
- [ ] **BONUS-09**: Equipped ranged weapons appear in ranged attack mode weapon selection
- [ ] **BONUS-10**: Equipped melee weapons appear in melee attack mode weapon selection

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
| GM-01 | Phase 2: GM Item Management | Pending |
| GM-02 | Phase 2: GM Item Management | Pending |
| GM-03 | Phase 2: GM Item Management | Pending |
| GM-04 | Phase 2: GM Item Management | Pending |
| GM-05 | Phase 2: GM Item Management | Pending |
| GM-06 | Phase 2: GM Item Management | Pending |
| GM-07 | Phase 2: GM Item Management | Pending |
| GM-08 | Phase 2: GM Item Management | Pending |
| GM-09 | Phase 2: GM Item Management | Pending |
| GM-10 | Phase 2: GM Item Management | Pending |
| GM-11 | Phase 2: GM Item Management | Pending |
| GM-12 | Phase 2: GM Item Management | Pending |
| INV-01 | Phase 3: Character Creation Inventory | Pending |
| INV-02 | Phase 3: Character Creation Inventory | Pending |
| INV-03 | Phase 3: Character Creation Inventory | Pending |
| INV-04 | Phase 3: Character Creation Inventory | Pending |
| INV-05 | Phase 3: Character Creation Inventory | Pending |
| INV-06 | Phase 3: Character Creation Inventory | Pending |
| INV-07 | Phase 4: Gameplay Inventory Core | Pending |
| INV-08 | Phase 4: Gameplay Inventory Core | Pending |
| INV-09 | Phase 4: Gameplay Inventory Core | Pending |
| INV-10 | Phase 4: Gameplay Inventory Core | Pending |
| INV-11 | Phase 4: Gameplay Inventory Core | Pending |
| INV-12 | Phase 4: Gameplay Inventory Core | Pending |
| INV-13 | Phase 5: Container System | Pending |
| INV-14 | Phase 5: Container System | Pending |
| INV-15 | Phase 5: Container System | Pending |
| INV-16 | Phase 5: Container System | Pending |
| INV-17 | Phase 5: Container System | Pending |
| INV-18 | Phase 5: Container System | Pending |
| BONUS-01 | Phase 6: Item Bonuses & Combat | Pending |
| BONUS-02 | Phase 6: Item Bonuses & Combat | Pending |
| BONUS-03 | Phase 6: Item Bonuses & Combat | Pending |
| BONUS-04 | Phase 6: Item Bonuses & Combat | Pending |
| BONUS-05 | Phase 6: Item Bonuses & Combat | Pending |
| BONUS-06 | Phase 6: Item Bonuses & Combat | Pending |
| BONUS-07 | Phase 6: Item Bonuses & Combat | Pending |
| BONUS-08 | Phase 6: Item Bonuses & Combat | Pending |
| BONUS-09 | Phase 6: Item Bonuses & Combat | Pending |
| BONUS-10 | Phase 6: Item Bonuses & Combat | Pending |
| DIST-01 | Phase 7: Item Distribution | Pending |
| DIST-02 | Phase 7: Item Distribution | Pending |
| DIST-03 | Phase 7: Item Distribution | Pending |

**Coverage Summary:** 50/50 v1 requirements mapped

---

## Requirements Validation Criteria

Each requirement must be:
- **Testable**: Can verify through UI interaction or automated test
- **User-centric**: Describes what user can do, not how system implements it
- **Atomic**: Single capability per requirement
- **Independent**: Minimal dependencies on other requirements

---

*Last updated: 2026-01-24*
