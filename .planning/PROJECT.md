# Threa TTRPG Assistant

## What This Is

A web-based TTRPG assistant for the Threa game system that helps players manage characters and Game Masters run games. Features include character creation, combat resolution with 4dF+ dice mechanics, inventory and equipment management, and user authentication with role-based access control.

## Core Value

Players and Game Masters can easily access the system, manage their content securely, and focus on gameplay rather than administration.

## Current Milestone: Ready for Next Milestone

**v1.1 User Management & Authentication** shipped on 2026-01-26. See `.planning/MILESTONES.md` for full details.

**Status:** No active milestone. Run `/gsd:new-milestone` to start the next phase of development.

**Recent Achievement (v1.1):**
- Complete user management and authentication system
- Self-service registration, password recovery, admin controls, user profiles
- 19/19 requirements satisfied, 100% integration verified

## Requirements

### Validated

These capabilities exist in the codebase:

**Pre-existing (before v1.0):**
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

**Delivered in v1.1 (2026-01-26):**
- ✓ New users can self-register with username, password (min 6 chars), and secret Q&A — v1.1
- ✓ First registered user automatically becomes Admin — v1.1
- ✓ Users can reset password using secret Q&A (case-insensitive, trimmed answer) — v1.1
- ✓ Admin can view list of all users — v1.1
- ✓ Admin can enable/disable users (preserves data) — v1.1
- ✓ Admin can change user roles (User/GameMaster/Admin) — v1.1
- ✓ Users can set display name (shown in UI, separate from login username) — v1.1
- ✓ Users can optionally provide email address — v1.1
- ✓ User avatars display via Gravatar (based on email if provided) — v1.1

**Delivered in v1.0 (2026-01-26):**
- ✓ GM can create new item templates via web UI — v1.0
- ✓ GM can edit existing item templates (all properties: basic, combat stats, bonuses) — v1.0
- ✓ GM can browse/search item templates (filter by type, search by name, tags) — v1.0
- ✓ GM can deactivate/delete item templates — v1.0
- ✓ Players can browse available item templates during character creation — v1.0
- ✓ Players can add items to character starting inventory — v1.0
- ✓ Players can view inventory on Play page — v1.0
- ✓ Players can equip items to equipment slots — v1.0
- ✓ Players can unequip items back to inventory — v1.0
- ✓ Players can drop/destroy items from inventory — v1.0
- ✓ Items stored in containers (bags) are tracked with parent-child relationships — v1.0
- ✓ GM can grant items to players during gameplay — v1.0
- ✓ Equipped items apply skill bonuses to character Ability Scores — v1.0
- ✓ Equipped items apply attribute modifiers to character attributes — v1.0
- ✓ Multiple item bonuses stack correctly according to design rules — v1.0
- ✓ Seed database with 10-20 example items for testing (52 items shipped) — v1.0

### Active

No active requirements. Next milestone will define new requirements via `/gsd:new-milestone`.

### Out of Scope

Explicitly excluded (may be considered for future milestones):

- Item crafting system — Complex, separate feature set
- Durability degradation mechanics — Field exists but auto-decay not needed yet
- Marketplace/economy/vendor system — Trading focus is player-to-player initially
- Item enchanting/upgrading — Future enhancement
- Item sets or combos — Single-item bonuses sufficient for v1.0
- Visual item icons/sprites — Text-based UI works well
- Player-to-player trading — Deferred from v1.0 scope

## Context

**Current State (v1.1 Shipped):**
The Threa TTRPG Assistant now has complete user management, authentication, and inventory systems. Key components:

**v1.1 User Management & Authentication (2026-01-26):**
- Self-service user registration with first-user-as-admin
- Password recovery via secret Q&A with brute-force protection
- Admin user management with last-admin protection safety feature
- User profiles with Gravatar avatars and profanity filtering
- 25 unit tests for authentication business logic
- Codebase: ~2,300 lines of user management code across 49 files

**v1.0 Inventory & Equipment System (2026-01-26):**
- 52 seed items (weapons, armor, ammo, containers, consumables) available for testing
- GM can create and manage item templates with full CRUD operations
- Players can manage inventory during character creation and gameplay
- Equipment slots with equip/unequip functionality
- Container system with nesting rules and capacity tracking
- Item bonuses (skill bonuses, attribute modifiers) integrated with combat
- Real-time item distribution from GM to players via messaging
- 38 passing unit tests across ItemTemplate, CharacterItem, and ItemBonusCalculator
- Codebase: ~16,000+ lines of inventory/equipment code across 80 files

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
Items provide the `+ Modifiers` portion of the Ability Score formula via ItemBonusCalculator:
- Skill bonuses: Add directly to skill level before AS calculation
- Attribute modifiers: Add to base attribute, which cascades to ALL skills using that attribute
- Stacking rules: Multiple items with same bonus type stack additively
- Combat integration: Equipped weapons provide damage class, SV/AV modifiers; armor provides absorption

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
- Item management workflows from v1.0 release

**Known Technical Debt (from v1.0):**
- ArmorInfoFactory.cs orphaned (duplicate logic in DamageResolution.razor)
- Weapon filtering logic in UI layer (should move to GameMechanics)
- Case sensitivity inconsistencies in skill/template comparisons
- OnCharacterChanged callback not wired in Play.razor (minor UX improvement)

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
| Build on existing DAL, don't refactor | DAL interfaces are well-designed and match needs exactly | ✓ Good (v1.0) |
| GM manages item templates, players get instances | Separation of concerns: GM curates library, players don't spawn arbitrary items | ✓ Good (v1.0) |
| Include container support in v1 | User explicitly requested it; foundational for inventory UX | ✓ Good (v1.0) |
| Bonus calculation on equip/unequip only | Performance: Don't recalculate on every property access | ✓ Good (v1.0) |
| Seed data with test items (52 items) | User requested it; critical for testing combat integration | ✓ Good (v1.0) |
| CSLA CommonRules + custom rules for validation | Standard pattern for ItemTemplate validation | ✓ Good (v1.0) |
| Container capacity warnings not errors | GM flexibility per design (magic bags allowed) | ✓ Good (v1.0) |
| Tags as comma-separated string | Simple filtering without complex data structure | ✓ Good (v1.0) |
| RadzenDataGrid with debounced search (300ms) | Responsive UI without performance issues | ✓ Good (v1.0) |
| CSS Grid for inventory tiles | Flexible layout, better than RadzenDataGrid for this UI | ✓ Good (v1.0) |
| Two-step equip flow (select then click slot) | Prevents accidental equips, better UX | ✓ Good (v1.0) |
| Container nesting: one level, empty only | Prevents complexity, enforced with blocking validation | ✓ Good (v1.0) |
| Capacity warnings non-blocking | Allows placement with warning per design | ✓ Good (v1.0) |
| ItemBonusCalculator service | Clean separation, easily testable (17 unit tests) | ✓ Good (v1.0) |
| CharacterUpdateMessage for inventory changes | Reuse existing infrastructure, no new message types | ✓ Good (v1.0) |
| Defer player-to-player trading to v2 | Scope management, GM distribution sufficient for v1 | ✓ Good (v1.0) |

---
*Last updated: 2026-01-26 after v1.1 milestone completion*
