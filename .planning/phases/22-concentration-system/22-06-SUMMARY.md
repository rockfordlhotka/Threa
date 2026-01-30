---
phase: 22-concentration-system
plan: 06
subsystem: combat
tags: [concentration, defense, passive-defense, active-defense, focus-skill]

# Dependency graph
requires:
  - phase: 22-04
    provides: CharacterEdit.CheckConcentration() with Focus skill check and damage penalty
provides:
  - DefenseResult with ConcentrationCheck and ConcentrationBroken properties
  - DefenseRequest with optional Defender, AttackerAV, and DamageDealt
  - DefenseResolver integration with concentration system
affects: [22-07, ui-combat-resolution, combat-controller]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - "Optional Defender in DefenseRequest for concentration integration"
    - "Backward compatible extension - legacy usage without Defender still works"
    - "Health depletion auto-break uses effective pool calculation (Value - PendingDamage)"

key-files:
  created:
    - GameMechanics.Test/DefenseConcentrationTests.cs
  modified:
    - GameMechanics/Combat/DefenseResult.cs
    - GameMechanics/Combat/DefenseRequest.cs
    - GameMechanics/Combat/DefenseResolver.cs

key-decisions:
  - "Optional Defender property allows backward compatibility with existing defense resolution code"
  - "Active defense breaks concentration BEFORE the defense roll (per design spec)"
  - "Passive defense triggers concentration check AFTER damage is determined"
  - "Health depletion check uses effective pool (Value - PendingDamage) for pending damage awareness"
  - "ConcentrationBroken flag always set even when ConcentrationCheck is null (for active defense)"

patterns-established:
  - "Defense resolution with optional character integration for side effects"
  - "Concentration check result propagation through DefenseResult"

# Metrics
duration: 5min
completed: 2026-01-29
---

# Phase 22 Plan 06: Defense Integration Summary

**DefenseResolver integrated with concentration checks: active defense breaks concentration before roll, passive defense triggers Focus check with damage penalty**

## Performance

- **Duration:** 5 min
- **Started:** 2026-01-29T22:55:53Z
- **Completed:** 2026-01-29T23:00:27Z
- **Tasks:** 5
- **Files modified:** 4

## Accomplishments
- DefenseResult extended with ConcentrationCheck and ConcentrationBroken properties
- DefenseRequest extended with optional Defender, AttackerAV, and DamageDealt properties
- Active dodge breaks concentration before roll when Defender is concentrating
- Active parry breaks concentration before roll when Defender is concentrating
- Passive defense triggers CheckConcentration with attacker AV and damage dealt
- Health depletion auto-break when FAT or VIT effective value reaches 0
- 13 comprehensive unit tests covering all scenarios
- Backward compatible - existing defense resolution code without Defender still works

## Task Commits

| Task | Name | Commit | Type |
|------|------|--------|------|
| 1 | Add ConcentrationCheck to DefenseResult | 844377f | feat |
| 2 | Break concentration on active defense | 79d0ab4 | feat |
| 3 | Add concentration check for passive defense | 7353778 | feat |
| 4 | Add auto-break on health depletion | 3c3eb23 | feat |
| 5 | Create defense concentration tests | 8556cd6 | test |

## Files Created/Modified
- `GameMechanics/Combat/DefenseResult.cs` - Added ConcentrationCheck and ConcentrationBroken properties
- `GameMechanics/Combat/DefenseRequest.cs` - Added Defender, AttackerAV, DamageDealt properties
- `GameMechanics/Combat/DefenseResolver.cs` - Integrated concentration checks into Resolve methods
- `GameMechanics.Test/DefenseConcentrationTests.cs` - 13 tests covering defense-triggered concentration

## Decisions Made

1. **Optional Defender property:** Rather than changing the method signatures, DefenseRequest was extended with an optional Defender property. This maintains backward compatibility with all existing code.

2. **Health depletion uses effective pool:** The auto-break check uses `Value <= PendingDamage` to account for pending damage that hasn't been applied yet. This ensures concentration breaks when the character is effectively incapacitated.

3. **ConcentrationBroken always set:** For active defense, ConcentrationBroken is set to true even though ConcentrationCheck is null. This allows UI to detect concentration break without checking the check result.

4. **Concentration check reuses dice roller:** The same IDiceRoller instance passed to DefenseResolver is used for concentration checks, ensuring deterministic testing.

## Deviations from Plan

None - plan executed exactly as written.

## Issues Encountered

None.

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness
- Defense integration complete
- UI (22-07) can now display ConcentrationCheck result from DefenseResult
- Combat flow can use ConcentrationBroken flag for messaging
- Ready for full combat resolution integration

---
*Phase: 22-concentration-system*
*Completed: 2026-01-29*
