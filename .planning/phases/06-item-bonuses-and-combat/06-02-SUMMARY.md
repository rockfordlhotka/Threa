---
phase: "06"
plan: "02"
subsystem: combat
tags: [item-bonuses, character-stats, ui-integration, stat-breakdown]

dependency-graph:
  requires: [06-01]
  provides: [CharacterEdit-item-integration, equipped-items-loading, attribute-breakdown-ui]
  affects: [07-01, 07-02]

tech-stack:
  added: []
  patterns: [layered-calculation, non-serialized-field, helper-method-injection]

key-files:
  created: []
  modified:
    - Threa.Dal/IItemDal.cs
    - Threa.Dal.MockDb/CharacterItemDal.cs
    - Threa.Dal.SqlLite/CharacterItemDal.cs
    - GameMechanics/CharacterEdit.cs
    - Threa/Threa.Client/Components/Pages/GamePlay/Play.razor
    - Threa/Threa.Client/Components/Pages/GamePlay/TabStatus.razor
    - GameMechanics/Combat/WeaponSelector.cs

decisions:
  - id: layered-attribute-calculation
    choice: GetEffectiveAttribute uses base + items + effects layering
    rationale: Per CONTEXT.md layered calculation approach
  - id: non-serialized-equipped-items
    choice: Use [NonSerialized] for _equippedItems field
    rationale: Equipped items loaded on-demand, not persisted with CSLA object
  - id: effect-modifier-after-items
    choice: Effect modifier applied to (base + itemBonus) sum
    rationale: Effects may depend on modified attribute value

metrics:
  duration: 10 min
  completed: 2026-01-26
---

# Phase 6 Plan 02: Combat Integration Summary

**One-liner:** CharacterEdit now calculates effective attributes with item bonuses, and the Play page displays stat breakdowns with color-coded item/effect modifiers.

## What Was Built

### DAL Layer Changes

1. **ICharacterItemDal.GetEquippedItemsWithTemplatesAsync** - New method added to interface
   - Returns equipped items with templates pre-populated
   - Filters to only items with valid templates (prevents null reference issues)

2. **MockCharacterItemDal** - Implementation added
   - Uses existing GetEquippedItemsAsync and PopulateTemplate

3. **SqliteCharacterItemDal** - Implementation added
   - Delegates to GetEquippedItemsAsync (already populates templates)
   - Filters to items with non-null templates

### CharacterEdit Integration

1. **Static calculator instance** - `ItemBonusCalculator` shared across all instances (stateless)

2. **Equipped items field** - Non-serialized `List<EquippedItemInfo>?` for on-demand loading

3. **SetEquippedItems/ClearEquippedItems** - Methods to manage equipped items cache

4. **GetEffectiveAttribute** - Modified to include layered calculation:
   - Base attribute value
   - Item bonuses from equipped items
   - Effect modifiers (applied to base + items sum)

5. **GetAttributeBreakdown** - Returns `AttributeBonusBreakdown` with full breakdown

6. **GetAttributeItemBreakdown** - Returns per-item breakdown list

7. **GetSkillItemBonus** - Returns total skill bonus from items (for Ability Score calculations)

8. **GetSkillItemBreakdown** - Returns per-item skill bonus breakdown

### UI Changes

1. **Play.razor**
   - Added `ICharacterItemDal` injection
   - Added `LoadEquippedItemsAsync` helper method
   - Calls `LoadEquippedItemsAsync` after every `characterPortal.FetchAsync`:
     - `SelectCharacterAsync` (when character attached to table)
     - `OnCharacterUpdateReceived` (real-time updates)
     - `PlayWithoutTable` (standalone play mode)
     - `ConfirmTableSelection` (joining a table)

2. **TabStatus.razor**
   - Added Attributes card below Quick Status
   - Displays each attribute with:
     - Attribute name (bold)
     - Total value (color-coded: green if boosted, red if reduced)
     - Breakdown showing: (base +/- N items +/- N effects)
   - Positive bonuses in green, negative in red

## Bug Fix During Execution

**[Rule 3 - Blocking] WeaponSelector.cs missing imports**
- File was missing `using System.Collections.Generic;` and `using System.Linq;`
- Fixed to unblock build
- (Note: IDE/linter may have auto-fixed before manual edit)

## Files Changed

| File | Change |
|------|--------|
| Threa.Dal/IItemDal.cs | Added GetEquippedItemsWithTemplatesAsync method |
| Threa.Dal.MockDb/CharacterItemDal.cs | Implemented GetEquippedItemsWithTemplatesAsync |
| Threa.Dal.SqlLite/CharacterItemDal.cs | Implemented GetEquippedItemsWithTemplatesAsync |
| GameMechanics/CharacterEdit.cs | Added item bonus integration (102 lines added) |
| Play.razor | Added equipped items loading (52 lines added) |
| TabStatus.razor | Added Attributes breakdown card |
| GameMechanics/Combat/WeaponSelector.cs | Added missing using statements |

## Verification Results

- Solution builds without errors (client project verified)
- LoadEquippedItemsAsync: 5 occurrences (1 definition + 4 calls)
- GetSkillItemBonus exists in CharacterEdit.cs
- SetEquippedItems called in Play.razor

## Success Criteria Met

- BONUS-01: GetSkillItemBonus method available for Ability Score calculations
- BONUS-02: GetEffectiveAttribute includes item bonuses
- BONUS-03: Attribute modifiers cascade via GetEffectiveAttribute (used by skills)
- BONUS-04: Multiple items stack additively (via ItemBonusCalculator)
- BONUS-05: Multiple attribute items stack additively
- BONUS-06: ClearEquippedItems + SetEquippedItems allows refresh on equip/unequip
- UI shows inline stat breakdown per CONTEXT.md

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] WeaponSelector.cs missing imports**
- **Found during:** Task 1 build verification
- **Issue:** File compiled in isolation but not with full solution due to missing System.Collections.Generic and System.Linq
- **Fix:** Added using statements
- **Files modified:** GameMechanics/Combat/WeaponSelector.cs
- **Commit:** d97e8a3 (included with Task 1)

## Next Phase Readiness

Ready for Phase 7 (Polish & Edge Cases) which will:
- Handle edge cases in item bonus calculations
- Add UI polish for stat displays
- Test combat integration end-to-end

---

*Completed: 2026-01-26 | Duration: 10 min*
