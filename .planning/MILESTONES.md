# Project Milestones: Threa TTRPG Assistant

## v1.7 Combat Tab Rework (Shipped: 2026-02-13)

**Delivered:** Consolidated Combat and Defense tabs into a single compact three-group tab with icon+label buttons, added anonymous action and skill check features, and implemented defensive stance system with cost-aware dimming.

**Phases completed:** 32-36 (10 plans total)

**Key accomplishments:**

- Consolidated two-tab Combat/Defense layout into single compact tab with three button groups (Actions, Defense, Other) using icon+label tiles
- Added anonymous target support for both melee and ranged attacks with AV display and simplified TV/SV workflow
- Created anonymous action mode for attribute-only rolls with cost selection, boost, and concentration handling
- Created combat skill picker modal and skill check mode (AS + 4dF+ vs TV) as standalone resolution
- Implemented defensive stance system with toggleable chips (Normal, Parry Mode, Dodge Focus, Block with Shield) and DefendMode pre-selection
- Applied consistent cost-aware dimming across all 12 combat buttons with cost-explaining tooltips

**Stats:**

- 50 files modified
- +9,076 lines of C#/Razor (9,076 insertions, 606 deletions)
- 5 phases, 10 plans, ~18 tasks
- 2 days from start to ship (2026-02-11 to 2026-02-13)
- 18/18 requirements shipped (100%)

**Git range:** abcc91b → bd9f692

**What's next:** Consider initiative tracking, encounter balancing, or other combat enhancements

---

## v1.6 Batch Character Actions (Shipped: 2026-02-05)

**Delivered:** Batch character action system enabling GMs to select multiple characters and apply damage, healing, visibility toggles, dismiss/archive, and effect add/remove operations to all at once with partial-success feedback.

**Phases completed:** 28-31 (9 plans total)

**Key accomplishments:**

- Multi-character selection with checkbox overlays on all card types, per-section Select All, and theme-aware highlighting
- BatchActionService with sequential CSLA-safe processing for damage/healing batch operations
- Batch visibility toggle and dismiss/archive for efficient NPC lifecycle management
- Batch effect add via template picker and batch effect remove via union name list with character counts
- Inline result feedback with expandable error details and smart selection cleanup per action type
- Two-layer NPC protection (UI pre-filter + service-level skip) ensuring PCs never sent to NPC-only operations

**Stats:**

- 56 files modified
- +11,097 lines of C#/Razor (11,097 insertions, 561 deletions)
- 4 phases, 9 plans
- 1 day from start to ship (2026-02-05)
- 20/20 requirements shipped (100%)

**Git range:** ae1207a → b7da5d7

**What's next:** Consider initiative tracking, encounter balancing, or session logs

---

## v1.5 NPC Management System (Shipped: 2026-02-03)

**Delivered:** Complete NPC management system enabling GMs to create, manage, and run NPCs in combat encounters alongside player characters.

**Phases completed:** 23-27 (20 plans total)

**Key accomplishments:**

- NPC template library with search, filter, category tags, difficulty badges, and clone functionality
- Quick-spawn NPCs from templates with auto-naming ("Goblin 1", "Goblin 2") and disposition selection
- Dashboard NPC section with disposition grouping (Hostile/Neutral/Friendly) and full GM manipulation powers
- Visibility toggle for surprise encounters with hidden NPC section and reveal activity log
- NPC lifecycle management (archive, restore, delete, save-as-template)
- Combat integration with NPCs in targeting dropdown and time advancement processing

**Stats:**

- 48 files modified
- +8,380 lines of C#/Razor (8,445 insertions, 65 deletions)
- 5 phases, 20 plans
- 3 days from start to ship (2026-02-01 to 2026-02-03)
- 23/23 requirements shipped (100%)

**Git range:** 50c21ed → 219043f

**What's next:** Consider group management (batch NPC actions), initiative tracking, or other GM capabilities

---

## v1.4 GM Character Manipulation + Concentration System (Shipped: 2026-01-29)

**Delivered:** Complete GM character manipulation powers with health management, wound tracking, effect application, inventory control, stat editing, and full concentration system mechanics.

**Phases completed:** 17-22 (21 plans total)

**Key accomplishments:**

- Health management with theme-aware color-coded health bars and pending pool damage/healing application
- Wound management with four severity levels, common wound templates, and header badge indicators
- Effect management with dictionary-based modifiers, template library, and searchable picker UI
- Inventory manipulation with item template picker, quantity prompts, and currency editing
- Stat editing with inline attribute/skill editing, validation warnings, and dependent stat recalculation
- Complete concentration system with casting-time progress, sustained FAT/VIT drain, Focus skill checks, linked effect removal, and UI indicators

**Stats:**

- 322 files modified
- +41,092 lines of C#/Razor (47,664 insertions, 6,572 deletions)
- 6 phases, 21 plans
- 2 days from start to ship (2026-01-28 to 2026-01-29)
- 52/52 requirements shipped (100%)

**Git range:** ef4981c → ca216d8

**What's next:** Consider NPC management system, initiative tracking, or other GM capabilities

---

## v1.2 GM Table & Campaign Management (Shipped: 2026-01-28)

**Delivered:** Complete campaign table and character management system with real-time GM dashboard and comprehensive time control

**Phases completed:** 12-16 (14 plans total)

**Key accomplishments:**

- GM campaign table creation with theme selection (Fantasy/Sci-Fi) and epoch-based world time
- Player request-to-join workflow with character selection, GM review, and approval/denial system
- Information-dense GM dashboard with compact character status cards showing health pools, wounds, AP, and effects
- Detailed character modal with five tabbed views (GM Actions, Character Sheet, Inventory, Grant Items, Narrative)
- Real-time dashboard updates via Rx.NET messaging when character state changes
- Context-aware time management system with combat rounds and calendar time (minutes, hours, days, weeks)
- "In Rounds" combat mode with toggle for detailed tracking
- NPC placeholder foundation for future expansion

**Stats:**

- 83 files modified
- +12,093 lines of C#/Razor (12,624 insertions, 531 deletions)
- 5 phases, 14 plans, ~30 tasks
- 2 days from start to ship (2026-01-26 to 2026-01-27)

**Git range:** `feat(12-01)` (afeee2f) → `feat(16-02)` (290cbef)

**What's next:** Continue building GM capabilities with character manipulation, effect management, and NPC system

---

## v1.1 User Management & Authentication (Shipped: 2026-01-26)

**Delivered:** Complete user management and authentication system with self-service registration, password recovery, admin controls, and user profiles.

**Phases completed:** 8-11 (8 plans total)

**Key accomplishments:**

- Self-service user registration with automatic first-user-as-admin convenience feature
- Password recovery via secret Q&A with brute-force protection (3 attempts, 15-minute lockout)
- Admin user management with last-admin protection preventing system lockout
- User profiles with Gravatar avatars and profanity filtering (handles Scunthorpe problem)
- Complete authentication and authorization system without email dependency
- 19/19 requirements satisfied (100%), 5/5 E2E flows verified, 18+ cross-phase integrations working

**Stats:**

- 49 files created/modified
- +7,955 lines of C#/Razor (8,041 additions, 86 deletions)
- 4 phases, 8 plans, ~20 tasks
- 1 day from start to ship (2026-01-26)
- 25 new unit tests (all passing)

**Git range:** `feat(08-01)` (aa6a010) → `feat(11-02)` (755913a)

**What's next:** Plan next milestone focus (consider v1.2 for feature enhancements or v2.0 for major initiative)

---

## v1.0 Inventory & Equipment System (Shipped: 2026-01-26)

**Delivered:** Complete inventory and equipment management system for GMs and players with real-time item distribution and combat integration.

**Phases completed:** 1-7 (16 plans total)

**Key accomplishments:**

- Complete CSLA business object foundation with 38 passing unit tests (ItemTemplate, CharacterItem, ItemBonusCalculator)
- GM item template management with full CRUD, tag-based categorization, and search/filter capabilities
- Player inventory system spanning character creation and gameplay with equipment slot management
- Container system with nesting enforcement (one level, empty containers only) and capacity tracking
- Item bonuses fully integrated with combat system via ItemBonusCalculator (attribute modifiers, skill bonuses)
- Real-time GM item distribution with CharacterUpdateMessage infrastructure
- 52 diverse seed items across weapons, armor, ammo, containers, and consumables (247% over minimum)

**Stats:**

- 80 files created/modified
- +16,145 lines of C#/Razor
- 7 phases, 16 plans, 21 tasks
- 3 days from start to ship (2026-01-24 to 2026-01-26)

**Git range:** `feat(01-03)` (4d2fd71) → `feat(07-01)` (f2a4fc7)

**What's next:** Technical debt cleanup and v2 enhancements (player trading, durability, vendors)

---
