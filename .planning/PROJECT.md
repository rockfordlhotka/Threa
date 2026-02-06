# Threa TTRPG Assistant

## What This Is

A web-based TTRPG assistant for the Threa game system that helps players manage characters and Game Masters run games. Features include character creation, combat resolution with 4dF+ dice mechanics, inventory and equipment management, user authentication with role-based access control, campaign table management, real-time GM dashboard with comprehensive time control, full GM character manipulation powers, concentration system for spell casting, NPC management for running combat encounters, and batch character actions for efficient multi-character management.

## Core Value

Players and Game Masters can easily access the system, manage their content securely, and focus on gameplay rather than administration.

## Previous Achievements

**v1.6 Batch Character Actions** (shipped 2026-02-05)
- Multi-character selection with checkbox overlays, per-section Select All, and theme-aware highlighting
- Batch damage/healing via BatchActionService with sequential CSLA-safe processing
- Batch visibility toggle and dismiss/archive for NPC lifecycle management
- Batch effect add via template picker and remove via union name list
- Inline result feedback with expandable error details and smart selection cleanup
- 20/20 requirements shipped (100%), 4 phases, 9 plans complete

**v1.5 NPC Management System** (shipped 2026-02-03)
- NPC template library with search, filter, category tags, and difficulty badges
- Quick-spawn NPCs from templates with auto-naming and disposition selection
- Dashboard NPC section with disposition grouping and full GM manipulation powers
- Visibility toggle for surprise encounters with hidden NPC section
- NPC lifecycle management (archive, restore, delete, save-as-template)
- Combat integration with NPCs in targeting and time advancement
- 23/23 requirements shipped (100%), 5 phases, 20 plans complete

**v1.4 GM Character Manipulation + Concentration System** (shipped 2026-01-29)
- Health management with theme-aware color-coded health bars and pending pool damage/healing
- Wound management with four severity levels, common wound templates, and header badges
- Effect management with dictionary-based modifiers, template library, and searchable picker
- Inventory manipulation with item template picker, quantity prompts, and currency editing
- Stat editing with inline attribute/skill editing, validation warnings, and dependent stat recalculation
- Complete concentration system with casting-time progress, sustained FAT/VIT drain, Focus skill checks, linked effect removal, and UI indicators
- 52/52 requirements shipped (100%), 6 phases, 21 plans complete

**v1.2 GM Table & Campaign Management** (shipped 2026-01-28)
**v1.1 User Management & Authentication** (shipped 2026-01-26)
**v1.0 Inventory & Equipment System** (shipped 2026-01-26)

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

**Delivered in v1.5 (2026-02-03):**
- ✓ GM can create NPC templates with full character stats (attributes, skills, equipment) — v1.5
- ✓ GM can edit existing NPC templates — v1.5
- ✓ GM can browse/search NPC templates (filter by category/tags) — v1.5
- ✓ GM can delete/deactivate NPC templates — v1.5
- ✓ NPC templates support category tags for organization — v1.5
- ✓ GM can quick-create NPC from template (spawn instance to active table) — v1.5
- ✓ NPCs have full character stats (same model as PCs) — v1.5
- ✓ Smart naming auto-generates unique names ("Goblin 1", "Goblin 2") — v1.5
- ✓ GM can add session-specific notes to individual NPC instances — v1.5
- ✓ NPCs appear in GM dashboard in separate section from PCs — v1.5
- ✓ NPC status cards show same info as PC cards (FAT/VIT/wounds, AP, effects) — v1.5
- ✓ NPC detail modal provides same manipulation powers as PCs — v1.5
- ✓ NPCs display disposition marker (Hostile/Neutral/Friendly) — v1.5
- ✓ GM can toggle NPC visibility (hide/reveal for surprise encounters) — v1.5
- ✓ Hidden NPCs do not appear in player-visible views — v1.5
- ✓ Visibility toggle accessible from both status card and detail modal — v1.5
- ✓ GM can remove/dismiss NPCs from active table — v1.5
- ✓ NPCs persist across sessions until explicitly dismissed — v1.5
- ✓ GM can save active NPC as new template — v1.5
- ✓ Dismissed NPCs can be optionally deleted or archived — v1.5
- ✓ NPCs participate in round/time advancement (effects expire, AP recovers) — v1.5
- ✓ NPCs can be targeted by combat actions — v1.5
- ✓ Time advancement applies to NPCs same as PCs — v1.5

**Delivered in v1.4 (2026-01-29):**
- ✓ GM can apply damage/healing to characters (add to pending pools) — v1.4
- ✓ GM can add/remove effects to/from characters — v1.4
- ✓ GM can edit effect properties (duration, modifiers, expiration) — v1.4
- ✓ GM can manipulate character inventory (add/remove items) — v1.4
- ✓ GM can edit character stats/properties outside normal restrictions — v1.4
- ✓ Casting-time concentration tracks progress and executes deferred actions — v1.4
- ✓ Sustained concentration drains FAT/VIT and links to spell effects — v1.4
- ✓ Concentration checks on damage with Focus skill vs attacker AV — v1.4
- ✓ Breaking concentration removes linked effects immediately — v1.4
- ✓ Active actions prompt to break concentration with confirmation — v1.4
- ✓ UI displays concentration status with type, progress, and drain — v1.4

**Delivered in v1.2 (2026-01-28):**
- ✓ GM can create campaign tables with name, theme (fantasy/sci-fi), and start epoch time — v1.2
- ✓ Players can request to join tables with a character — v1.2
- ✓ GM can review character details and approve/deny join requests — v1.2
- ✓ GM can remove characters from active table — v1.2
- ✓ GM dashboard displays compact character status (FAT/VIT/wounds with pending, AP, effects) — v1.2
- ✓ GM can advance time by multiple increments (rounds, minutes, turns, hours, days, weeks) — v1.2
- ✓ System tracks "in rounds" vs normal state — v1.2
- ✓ GM dashboard updates in real-time when character state changes — v1.2
- ✓ GM can view detailed character information (sheet, inventory, appearance) — v1.2
- ✓ Characters are campaign-specific (not shared across tables) — v1.2

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

**Delivered in v1.6 (2026-02-05):**
- ✓ GM can multi-select characters from dashboard — v1.6
- ✓ GM can apply damage/healing to selected characters in batch — v1.6
- ✓ GM can add/remove effects to selected characters in batch — v1.6
- ✓ GM can toggle visibility on selected characters in batch — v1.6
- ✓ GM can dismiss/archive selected characters in batch — v1.6
- ✓ Batch operations show clear feedback (partial success: what succeeded/failed) — v1.6

### Active

**Future milestones (to be defined):**
- [ ] Initiative tracking with automatic turn order
- [ ] Automated encounter balancing based on party composition

### Out of Scope

Explicitly excluded (may be considered for future milestones):

**Deferred to future milestones:**
- Initiative tracking automation — Round advancement exists; initiative order is future enhancement
- Automated encounter balancing — Complex feature requiring game balance data
- Session logs/history within campaign — Focus on real-time management first
- Campaign sharing between GMs — Single GM per campaign simplifies permissions

**Previously deferred:**
- Item crafting system — Complex, separate feature set
- Durability degradation mechanics — Field exists but auto-decay not needed yet
- Marketplace/economy/vendor system — Trading focus is player-to-player initially
- Item enchanting/upgrading — Future enhancement
- Item sets or combos — Single-item bonuses sufficient for v1.0
- Visual item icons/sprites — Text-based UI works well
- Player-to-player trading — Deferred from v1.0 scope

## Context

**Current State (v1.6 Shipped):**
The Threa TTRPG Assistant now has batch character action capabilities enabling GMs to select multiple characters and apply actions to all at once for efficient encounter management. Key components:

**v1.6 Batch Character Actions (2026-02-05):**
- Multi-character selection with checkbox overlays on all card types (PC, NPC, Hidden NPC)
- SelectionBar with per-section Select All, Deselect All, and Escape key support
- BatchActionService with 6 operations: damage, heal, visibility, dismiss, effect add, effect remove
- Batch modals: damage/healing amount, dismiss confirmation, effect add (template picker), effect remove (union list)
- Inline result feedback with expandable error details for partial failures
- Smart selection cleanup per action type and two-layer NPC protection
- Codebase: +11,097 lines across 56 files modified

**v1.5 NPC Management System (2026-02-03):**
- NPC template library with search, filter, category tags, difficulty badges, and clone functionality
- Quick-spawn from templates with auto-naming ("Goblin 1", "Goblin 2") and disposition selection
- Dashboard NPC section with disposition grouping (Hostile/Neutral/Friendly)
- Full GM manipulation powers on NPCs (same as PCs: health, wounds, effects, inventory, stats)
- Visibility toggle for surprise encounters with hidden NPC section
- NPC lifecycle: archive, restore, delete, save-as-template
- Combat integration: NPCs in targeting dropdown and time advancement processing
- Codebase: +8,380 lines across 48 files modified

**v1.4 GM Character Manipulation + Concentration (2026-01-29):**
- Health management with theme-aware color-coded health bars (green/yellow/red)
- Wound management with four severity levels and common wound templates
- Effect management with dictionary-based modifiers and template library
- Inventory manipulation with item template picker and quantity prompts
- Stat editing with inline attribute/skill editing and validation
- Concentration system: casting-time progress, sustained FAT/VIT drain, Focus checks

**v1.2 GM Table & Campaign Management (2026-01-28):**
- Campaign table creation with theme (Fantasy/Sci-Fi) and epoch-based world time
- Player request-to-join workflow with character selection and GM approval/denial
- Real-time GM dashboard with character status cards showing health, AP, wounds, effects
- Detailed character modal with five tabbed views (GM Actions, Sheet, Inventory, Grant Items, Narrative)
- Context-aware time management: combat rounds and calendar time (minutes, hours, days, weeks)

**v1.1 User Management & Authentication (2026-01-26):**
- Self-service user registration with first-user-as-admin
- Password recovery via secret Q&A with brute-force protection
- Admin user management with last-admin protection safety feature
- User profiles with Gravatar avatars and profanity filtering

**v1.0 Inventory & Equipment System (2026-01-26):**
- 52 seed items (weapons, armor, ammo, containers, consumables) available for testing
- GM can create and manage item templates with full CRUD operations
- Players can manage inventory during character creation and gameplay
- Equipment slots with equip/unequip functionality
- Container system with nesting rules and capacity tracking
- Item bonuses (skill bonuses, attribute modifiers) integrated with combat

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

**Technical Environment:**
- .NET 10, C# 12+
- CSLA.NET 9.1.0 business objects
- Blazor Web App (SSR + Interactive Server)
- Radzen.Blazor 8.4.2 for UI components
- SQLite database (Microsoft.Data.Sqlite 10.0.1)
- Nullable reference types enabled project-wide
- ~88,400 lines of C#/Razor code

**Known Technical Debt (from v1.0):**
- ArmorInfoFactory.cs orphaned (duplicate logic in DamageResolution.razor)
- Weapon filtering logic in UI layer (should move to GameMechanics)
- Case sensitivity inconsistencies in skill/template comparisons

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
| Epoch-based world time tracking | Flexible time management, supports long-running campaigns | ✓ Good (v1.2) |
| Context-aware time buttons | Show only relevant buttons for current mode (combat vs exploration) | ✓ Good (v1.2) |
| "In Rounds: Round X" combined badge | Single badge clearer than separate Round/Combat indicators | ✓ Good (v1.2) |
| Time skip cap at 100 iterations | Prevents UI freeze on large calendar time skips (e.g., 1 week = 100,800 rounds) | ✓ Good (v1.2) |
| Five-tab modal for character details | Prioritizes GM workflow: Actions, Sheet, Inventory, Grant Items, Narrative | ✓ Good (v1.2) |
| Two-button FAT/VIT layout | Eliminates dropdown step for faster damage/healing application | ✓ Good (v1.4) |
| RadzenDialog in interactive context | Required for modal rendering with DialogService | ✓ Good (v1.2) |
| Character-specific GM notes | Notes stored per table-character pair for campaign context | ✓ Good (v1.2) |
| Real-time updates via CharacterUpdateMessage | Reuse existing infrastructure, no new message types | ✓ Good (v1.2) |
| ITimeEventSubscriber pattern | Consistent subscription pattern for time-based updates | ✓ Good (v1.2) |
| 500ms delay before character list refresh | Debounce rapid time events to reduce UI thrashing | ✓ Good (v1.2) |
| JoinRequest with status enum | Clean state management for Pending/Approved/Denied workflow | ✓ Good (v1.2) |
| NPC placeholder in v1.2 | Foundation laid, full NPC management deferred to future | ✓ Good (v1.2) |
| Four fixed wound severity levels | Matches design, provides clear progression (Minor/Moderate/Severe/Critical) | ✓ Good (v1.4) |
| Dictionary-based effect modifiers | Flexible extensibility for attributes/skills without schema changes | ✓ Good (v1.4) |
| JSON blob storage for concentration state | No schema migration needed for SQLite | ✓ Good (v1.4) |
| Effect linking via SourceEffectId | Enables cascade removal when concentration breaks | ✓ Good (v1.4) |
| Focus skill check for concentration | Matches game design with damage penalty (-1 per 2 damage) | ✓ Good (v1.4) |
| Same concentration check pattern for all actions | Consistent UX across melee, ranged, and reload actions | ✓ Good (v1.4) |
| NPCs use existing CharacterEdit model | 100% feature parity with PCs, no parallel model maintenance | ✓ Good (v1.5) |
| Template pattern follows ItemTemplate approach | Proven pattern, consistent UX | ✓ Good (v1.5) |
| Spawn hidden by default | Surprise encounters are common GM need | ✓ Good (v1.5) |
| Global auto-naming counter | Avoids confusion between templates | ✓ Good (v1.5) |
| Disposition grouping in dashboard | Visual organization for combat management | ✓ Good (v1.5) |
| VisibleToPlayers filter in targeting | Prevents hidden NPCs leaking to players | ✓ Good (v1.5) |
| Archive vs delete option | Preserves useful NPCs while cleaning dashboard | ✓ Good (v1.5) |
| Save-as-template resets health | Fresh NPC definition, not combat state | ✓ Good (v1.5) |
| HashSet<int> for selection state | O(1) toggle and lookup performance for multi-select | ✓ Good (v1.6) |
| Sequential batch processing | CSLA objects not thread-safe; mirrors TimeAdvancementService pattern | ✓ Good (v1.6) |
| Single CharactersUpdatedMessage per batch | Prevents N refresh cycles from flooding dashboard | ✓ Good (v1.6) |
| BatchActionRequest as record type | Supports 'with' expression for action type enforcement | ✓ Good (v1.6) |
| Two-layer NPC protection | UI pre-filter + service-level skip ensures PCs never sent to NPC-only ops | ✓ Good (v1.6) |
| Action-type-aware selection cleanup | Clear on damage/heal, preserve on visibility/effects, remove dismissed | ✓ Good (v1.6) |
| Shared game timestamp for batch effects | All effects in batch have identical CreatedAt/ExpiresAt for consistency | ✓ Good (v1.6) |
| Name-based effect removal | Allows same effect removed from all characters in one pass | ✓ Good (v1.6) |

---
*Last updated: 2026-02-05 after v1.6 milestone complete*
