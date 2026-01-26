# Project State

## Project Reference

See: .planning/PROJECT.md (updated 2026-01-24)

**Core value:** Players can equip weapons and armor that directly affect their combat effectiveness, and Game Masters can create and distribute items that integrate seamlessly with the existing combat system.
**Current focus:** Phase 6 - Item Bonuses & Combat - In Progress

## Current Position

Phase: 6 of 7 (Item Bonuses & Combat)
Plan: 1 of 2 in current phase
Status: In progress
Last activity: 2026-01-25 - Completed 06-01-PLAN.md

Progress: [#############---] 76%

## Performance Metrics

**Velocity:**
- Total plans completed: 13
- Average duration: 10 min
- Total execution time: 2.2 hours

**By Phase:**

| Phase | Plans | Total | Avg/Plan |
|-------|-------|-------|----------|
| 01-foundation | 3 | 24 min | 8 min |
| 02-gm-item-management | 3 | 44 min | 15 min |
| 03-character-creation-inventory | 2 | 13 min | 6.5 min |
| 04-gameplay-inventory-core | 2 | 18 min | 9 min |
| 05-container-system | 2 | 27 min | 13.5 min |
| 06-item-bonuses-and-combat | 1 | 4 min | 4 min |

**Recent Trend:**
- Last 5 plans: 06-01 (4 min), 05-02 (15 min), 05-01 (12 min), 04-02 (15 min), 04-01 (3 min)
- Trend: steady execution velocity

*Updated after each plan completion*

## Accumulated Context

### Decisions

Decisions are logged in PROJECT.md Key Decisions table.
Recent decisions affecting current work:

- Build on existing DAL interfaces (IItemTemplateDal, ICharacterItemDal) - no refactoring
- GM manages templates, players get instances - separation of concerns
- Include container support in v1 - user requested
- Bonus calculation on equip/unequip only - performance optimization
- Use WeaponType.Polearm for spears (no dedicated Spear enum) - 01-03
- Ranged weapon CustomProperties JSON pattern established (fire modes, range bands, capacity) - 01-03
- Ammunition CustomProperties JSON pattern established (ammoType, damageModifier, isLooseAmmo) - 01-03
- CSLA CommonRules + custom rules for ItemTemplate validation - 01-01
- Container capacity validation is warning, not error (GM flexibility) - 01-01
- Used [RunLocal] + sync Create for CharacterItemEdit (test compatibility) - 01-02
- String properties initialized to empty by CSLA - tests check IsNullOrEmpty - 01-02
- Used IEnumerable instead of IQueryable for client-side filtering - 02-02
- RadzenDataGrid with RowSelect for click-to-edit navigation pattern - 02-02
- 300ms debounce pattern for search input - 02-02
- Tags stored as comma-separated string for simple filtering - 02-01
- RadzenTabs with @key for dynamic tab visibility on enum change - 02-03
- Radzen JS/CSS must be in App.razor for component functionality - 02-03
- Split-view layout with Bootstrap grid for browse-and-select interfaces - 03-01
- Single-click row adds item (no confirmation dialog) - 03-01
- Unsaved characters (Id=0) must save before managing inventory - 03-01
- Quantity 0 triggers item deletion (immediate removal) - 03-02
- Carrying capacity formula: 50 lbs * 1.15^(STR-10) - 03-02
- Overweight warning is informational only (no enforcement) - 03-02
- CSS Grid for inventory tiles (not RadzenDataGrid) per CONTEXT.md - 04-01
- Text-based item type icons for MVP (no IconUrl field exists) - 04-01
- Implant slots excluded from equipment display (deferred) - 04-01
- SlotCategory record pattern for grouping equipment slots - 04-01
- Two-step equip flow: select item then click slot (prevents accidents) - 04-02
- IsEquippableType() check prevents ammunition/consumables from being equipped - 04-02
- TwoHand weapons ONLY equip to TwoHand slot (rifles must use TwoHand) - 04-02
- OneHand weapons (MainHand/OffHand) can equip to any weapon slot - 04-02
- Always confirm before dropping items (per CONTEXT.md) - 04-02
- Stackable drops prompt for quantity before confirmation - 04-02
- All item operations use ItemManagementService (curse handling, effects) - 04-02
- Radzen DialogService registered in Program.cs for confirmation dialogs - 04-02
- Container panel replaces equipment slots in right column when open - 05-01
- Two-phase container move flow (select item then click container) - 05-01
- Click container without selection opens panel - 05-01
- Client-side filtering for container contents (items already loaded) - 05-01
- Dashed border visual indicator for container tiles - 05-01
- Capacity/type warnings are non-blocking (placement succeeds with warning) - 05-02
- Nesting validation is blocking (one-level only per CONTEXT.md) - 05-02
- Empty containers CAN be nested, non-empty containers cannot - 05-02
- Drop container dialog has Cancel/Empty First/Drop All options - 05-02
- Fill indicator colors: gray (empty), green (<75%), yellow (75-99%), red (100%+) - 05-02
- ContainerWeightReduction multiplier applied to contents weight - 05-02
- Only FlatBonus type bonuses are calculated (PercentageBonus ignored per CONTEXT.md) - 06-01
- Case-insensitive comparison for attribute/skill names in bonus calculation - 06-01

### Pending Todos

None yet.

### Blockers/Concerns

None.

## Session Continuity

Last session: 2026-01-25
Stopped at: Completed 06-01-PLAN.md
Resume file: None

## Phase 6 Status - In Progress

1 of 2 plans complete:
- 06-01-PLAN.md: Complete - ItemBonusCalculator, EquippedItemInfo, AttributeBonusBreakdown, unit tests
- 06-02-PLAN.md: Pending - Combat integration (pending)

**Phase 6 Deliverables (Completed so far):**
- ItemBonusCalculator service computes flat bonuses from equipped items
- EquippedItemInfo value object combines CharacterItem + ItemTemplate
- AttributeBonusBreakdown shows base + item + effect contributions
- 17 unit tests covering bonus calculation scenarios

Next: Plan 06-02 (Combat Integration - apply item bonuses to attack/defense)
