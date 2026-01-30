---
phase: 22-concentration-system
plan: 04
subsystem: game-mechanics
tags: [concentration, focus-skill, skill-check, csla, character-api]

# Dependency graph
requires:
  - phase: 22-03
    provides: ConcentrationBehavior with BreakConcentration static method, ConcentrationState schema
provides:
  - CharacterEdit.GetConcentrationEffect() for concentration effect queries
  - CharacterEdit.GetConcentrationType() for concentration type string queries
  - CharacterEdit.CheckConcentration() with Focus skill check and damage penalty
  - ConcentrationCheckResult class for structured check outcomes
affects: [22-05, 22-06, 22-07, defense-system, ui-concentration-display]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "IDiceRoller injection for testable skill checks"
    - "Damage penalty formula: -1 per 2 damage dealt (integer division)"
    - "AbilityScore property on SkillEdit includes attribute + bonus"

key-files:
  created:
    - GameMechanics.Test/ConcentrationCheckTests.cs
  modified:
    - GameMechanics/CharacterEdit.cs

key-decisions:
  - "Use existing SkillEdit.AbilityScore property which already includes attribute + skill bonus"
  - "Damage penalty uses integer division (damageDealt / 2) for natural rounding down"
  - "No Focus skill = automatic failure (not default to attribute-only check)"
  - "RandomDiceRoller for production, DeterministicDiceRoller for tests"

patterns-established:
  - "Concentration check result class pattern for structured outcomes"
  - "Overloaded methods with IDiceRoller parameter for testability"

# Metrics
duration: 6min
completed: 2026-01-29
---

# Phase 22 Plan 04: Character Concentration API Summary

**Focus skill check with damage penalty for passive defense concentration verification**

## Performance

- **Duration:** 6 min
- **Started:** 2026-01-29T22:47:02Z
- **Completed:** 2026-01-29T22:52:44Z
- **Tasks:** 6
- **Files modified:** 2

## Accomplishments
- ConcentrationCheckResult class with Success, AS, Roll, Result, TV, DamagePenalty, Reason
- GetConcentrationEffect() returns active concentration effect or null
- GetConcentrationType() returns concentration type string or null
- CheckConcentration() calculates Focus AS + 4dF+ vs attackerAV
- Damage penalty: -1 per 2 damage dealt (rounds down via integer division)
- Failed concentration check automatically calls ConcentrationBehavior.BreakConcentration()
- Convenience overload uses RandomDiceRoller for production code

## Task Commits

All 6 tasks committed atomically:

1. **All tasks: Character concentration check API** - `b12e930` (feat)
   - ConcentrationCheckResult class (Task 1)
   - GetConcentrationEffect method (Task 2)
   - GetConcentrationType method (Task 3)
   - CheckConcentration with IDiceRoller (Task 4)
   - CheckConcentration convenience overload (Task 5)
   - 14 unit tests (Task 6)

## Files Created/Modified
- `GameMechanics/CharacterEdit.cs` - Added ConcentrationCheckResult class, GetConcentrationEffect, GetConcentrationType, CheckConcentration methods
- `GameMechanics.Test/ConcentrationCheckTests.cs` - 14 tests covering concentration check mechanics

## Decisions Made

1. **Use SkillEdit.AbilityScore:** The Focus skill's AbilityScore property already includes the attribute value (WIL) plus skill bonus. No need to recalculate.

2. **Integer division for damage penalty:** Using `damageDealt / 2` (integer division) naturally rounds down as required by the spec. No explicit Math.Floor needed.

3. **No Focus skill = automatic failure:** If a character doesn't have the Focus skill, concentration automatically fails. This encourages characters to invest in Focus if they plan to concentrate during combat.

4. **Separate RandomDiceRoller class:** Rather than wrapping the static Dice class, use the existing RandomDiceRoller which implements IDiceRoller.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- CharacterEdit concentration API complete
- Ready for 22-05 (Action System Integration) to check concentration before actions
- Ready for 22-06 (Defense Integration) to call CheckConcentration after passive defense
- UI integration (22-07) can display concentration check results

---
*Phase: 22-concentration-system*
*Completed: 2026-01-29*
