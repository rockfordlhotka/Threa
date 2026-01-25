# Threa Inventory & Equipment System

## What This Is

A complete inventory and equipment management system for the Threa TTRPG assistant that allows Game Masters to create and manage item templates (weapons, armor, ammo, consumables), and players to equip items, manage inventory, and benefit from item bonuses during character creation and gameplay.

## Core Value

Players can equip weapons and armor that directly affect their combat effectiveness, and Game Masters can create and distribute items that integrate seamlessly with the existing combat system.

## Requirements

### Validated

These capabilities already exist in the codebase:

- ✓ CSLA.NET business objects with data portal operations — existing
- ✓ ItemTemplate DTO with properties for weapons, armor, damage, bonuses — existing
- ✓ CharacterItem DTO for character inventory instances — existing
- ✓ IItemTemplateDal and ICharacterItemDal interfaces defined — existing
- ✓ MockDb and SQLite DAL implementations — existing
- ✓ Combat system expects equipped items to provide stat bonuses — existing
- ✓ Blazor Web App with SSR + Interactive Server rendering — existing
- ✓ Character creation and edit pages — existing
- ✓ Play page with combat, skills, magic, defense tabs — existing
- ✓ Equipment slots defined (MainHand, OffHand, Head, Chest, etc.) — existing

### Active

New capabilities being built:

- [ ] GM can create new item templates via web UI
- [ ] GM can edit existing item templates (all properties: basic, combat stats, bonuses)
- [ ] GM can browse/search item templates (filter by type, search by name, tags)
- [ ] GM can deactivate/delete item templates
- [ ] Players can browse available item templates during character creation
- [ ] Players can add items to character starting inventory
- [ ] Players can view inventory on Play page
- [ ] Players can equip items to equipment slots
- [ ] Players can unequip items back to inventory
- [ ] Players can drop/destroy items from inventory
- [ ] Items stored in containers (bags) are tracked with parent-child relationships
- [ ] GM can grant items to players during gameplay
- [ ] Players can give items to other players
- [ ] Equipped items apply skill bonuses to character Ability Scores
- [ ] Equipped items apply attribute modifiers to character attributes
- [ ] Multiple item bonuses stack correctly according to design rules
- [ ] Seed database with 10-20 example items for testing (swords, guns, armor, ammo)

### Out of Scope

Explicitly excluded from this milestone:

- Item crafting system — Complex, separate feature set
- Durability degradation mechanics — Field exists but auto-decay not needed yet
- Marketplace/economy/vendor system — Trading focus is player-to-player
- Item enchanting/upgrading — Future enhancement
- Item sets or combos — Single-item bonuses only for v1
- Visual item icons/sprites — Text-based for now

## Context

**Existing System:**
The codebase already has a working TTRPG combat system with:
- 4dF+ dice mechanics (exploding Fudge dice)
- Melee combat (Physicality + skill)
- Ranged combat (bows, thrown weapons)
- Firearms combat (single, burst, suppression fire modes)
- Defense mechanics (dodge, parry, shield block)
- Damage resolution with hit locations and armor
- Character stats with attributes (STR, DEX, END, INT, ITT, WIL, PHY) and skills
- Ability Score calculation: `AS = Attribute + Skill Level - 5 + Modifiers`

**Integration Point:**
Items need to provide the `+ Modifiers` portion of the Ability Score formula. The EQUIPMENT_SYSTEM.md design doc specifies:
- Skill bonuses: Add directly to skill level before AS calculation
- Attribute modifiers: Add to base attribute, which cascades to ALL skills using that attribute
- Stacking rules: Multiple items with same bonus type stack additively

**Technical Environment:**
- .NET 10, C# 12+
- CSLA.NET 9.1.0 business objects
- Blazor Web App (SSR + Interactive Server)
- Radzen.Blazor 8.4.2 for UI components
- SQLite database (Microsoft.Data.Sqlite 10.0.1)
- Nullable reference types enabled project-wide

**User Knowledge:**
Users are already familiar with:
- TTRPG concepts (equipment slots, weapons, armor, inventory)
- The existing combat system (they've been testing melee/ranged attacks)
- The Play page layout (tabs for Status, Combat, Skills, Magic, Defense, Inventory)

## Constraints

- **Tech Stack**: Must use existing CSLA.NET patterns — All domain entities extend BusinessBase<T>, use PropertyInfo pattern
- **Data Layer**: Must use existing DAL interfaces (IItemTemplateDal, ICharacterItemDal) — Don't rebuild, extend if needed
- **UI Framework**: Radzen.Blazor components for consistency — Match existing page styles
- **Equipment Slots**: Use existing EquipmentSlot enum from Threa.Dal.Dto — Don't invent new slots
- **Bonus Calculation**: Follow EQUIPMENT_SYSTEM.md rules exactly — Skill bonuses vs. attribute modifiers, stacking behavior
- **Performance**: Keep item bonus calculations efficient — Recalculate on equip/unequip only, cache results

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Build on existing DAL, don't refactor | DAL interfaces are well-designed and match needs exactly | — Pending |
| GM manages item templates, players get instances | Separation of concerns: GM curates library, players don't spawn arbitrary items | — Pending |
| Include container support in v1 | User explicitly requested it; foundational for inventory UX | — Pending |
| Include player-to-player trading in v1 | User explicitly requested it; enables collaborative play | — Pending |
| Bonus calculation on equip/unequip only | Performance: Don't recalculate on every property access | — Pending |
| Seed data with test items | User explicitly requested it; critical for testing combat integration | — Pending |

---
*Last updated: 2026-01-24 after initialization*
