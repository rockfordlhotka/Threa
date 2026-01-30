---
phase: 22-concentration-system
plan: 03
subsystem: game-mechanics
tags: [concentration, sustained-spells, fudge-dice, effect-behavior, csla]

# Dependency graph
requires:
  - phase: 22-02
    provides: ConcentrationBehavior with casting-time lifecycle, LastConcentrationResult, ConcentrationState schema
provides:
  - Sustained concentration mechanics with FAT/VIT drain per round
  - Exhaustion detection that breaks concentration automatically
  - Linked effect removal preparation for multi-target spells
  - CreateSustainedConcentrationState helper for spell system integration
affects: [22-04, 22-05, 22-06, 22-07, spell-system, ui-concentration-display]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Pending damage pattern for sustained drain (adds to Fatigue.PendingDamage, processed at EndOfRound)"
    - "Effective pool calculation (Value - PendingDamage) for exhaustion detection"

key-files:
  created:
    - GameMechanics.Test/ConcentrationBehaviorSustainedTests.cs
  modified:
    - GameMechanics/Effects/Behaviors/ConcentrationBehavior.cs

key-decisions:
  - "Drain adds to PendingDamage rather than directly reducing Value (matches existing health pool pattern)"
  - "Exhaustion checked against effective pool (Value - PendingDamage) for immediate break detection"
  - "LinkedEffectIds stored in LastConcentrationResult for UI/controller layer to process"

patterns-established:
  - "IsSustainedConcentration helper for type checking (SustainedSpell, SustainedAbility, MentalControl)"
  - "PrepareLinkedEffectRemoval stores result for cross-character effect cleanup"

# Metrics
duration: 8min
completed: 2026-01-29
---

# Phase 22 Plan 03: Sustained Concentration Summary

**FAT/VIT drain per round with exhaustion-based auto-break and linked effect removal for multi-target sustained spells**

## Performance

- **Duration:** 8 min
- **Started:** 2026-01-29T22:38:33Z
- **Completed:** 2026-01-29T22:46:00Z
- **Tasks:** 8
- **Files modified:** 2

## Accomplishments
- Sustained concentration drains FAT/VIT per round via PendingDamage
- Concentration auto-breaks when caster's effective FAT or VIT reaches 0
- Multi-target spell effects prepared for removal via LastConcentrationResult
- Helper method creates properly configured sustained concentration state

## Task Commits

All 8 tasks committed atomically:

1. **All tasks: Sustained concentration implementation** - `c2206b9` (feat)
   - IsSustainedConcentration helper (Task 1)
   - ApplyDrain helper (Task 2)
   - TickSustainedConcentration (Task 3)
   - OnTick dispatch (Task 4)
   - PrepareLinkedEffectRemoval (Task 5)
   - OnRemove sustained handling (Task 6)
   - CreateSustainedConcentrationState (Task 7)
   - 21 unit tests (Task 8)

## Files Created/Modified
- `GameMechanics/Effects/Behaviors/ConcentrationBehavior.cs` - Added sustained concentration logic (IsSustainedConcentration, ApplyDrain, TickSustainedConcentration, PrepareLinkedEffectRemoval, CreateSustainedConcentrationState)
- `GameMechanics.Test/ConcentrationBehaviorSustainedTests.cs` - 21 tests covering drain, exhaustion, description updates, and linked effect removal

## Decisions Made

1. **Drain via PendingDamage:** Rather than directly reducing Fatigue.Value, drain is added to Fatigue.PendingDamage. This matches the existing health pool pattern where damage is "pending" until applied during EndOfRound processing.

2. **Effective pool for exhaustion:** Exhaustion is checked against effective pool (Value - PendingDamage) rather than just Value. This ensures concentration breaks immediately when drain would cause exhaustion, rather than waiting for EndOfRound to apply the damage.

3. **LinkedEffectIds in LastConcentrationResult:** Like casting-time concentration, sustained concentration stores linked effect IDs in LastConcentrationResult rather than directly removing effects from target characters. This is necessary because ConcentrationBehavior only has access to the caster's CharacterEdit, not other table characters.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Sustained concentration mechanics complete
- Ready for 22-04 (Concentration Check Triggers) and 22-05 (Concentration Check Resolution)
- UI integration (22-07) can now process sustained spell breaks via LastConcentrationResult

---
*Phase: 22-concentration-system*
*Completed: 2026-01-29*
