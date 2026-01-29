# Phase 22 Plan 02: ConcentrationBehavior Casting Time Implementation Summary

---
phase: 22-concentration-system
plan: 02
subsystem: effects/concentration
tags: [concentration, casting-time, magazine-reload, spell-casting, behavior, lifecycle]

dependency-graph:
  requires: [22-01]
  provides:
    - ConcentrationBehavior.OnTick with generic casting-time progress
    - ConcentrationBehavior.OnExpire with deferred action execution
    - ConcentrationBehavior.OnRemove with interruption handling
    - CharacterEdit.LastConcentrationResult for behavior-to-UI communication
    - EffectTickResult.CompleteEarly for successful early completion
  affects: [22-03, 22-07]

tech-stack:
  added: []
  patterns:
    - Behavior-to-UI communication via transient result property
    - CompleteEarly vs ExpireEarly distinction for lifecycle hooks

key-files:
  created:
    - GameMechanics.Test/ConcentrationBehaviorCastingTimeTests.cs
  modified:
    - GameMechanics/Effects/Behaviors/ConcentrationBehavior.cs
    - GameMechanics/CharacterEdit.cs
    - GameMechanics/Effects/EffectTickResult.cs
    - GameMechanics/EffectList.cs

decisions:
  - id: 22-02-01
    title: CompleteEarly vs ExpireEarly distinction
    choice: Added new EffectTickResult.CompleteEarly() that sets ExpireAsComplete=true
    rationale: Concentration completion should call OnExpire (success), not OnRemove (interruption)
    alternatives: Could have added a flag to ConcentrationState, but this is more generic for all behaviors

metrics:
  duration: 8 min
  completed: 2026-01-29
---

## One-liner

Casting-time concentration lifecycle with progress tracking, deferred action execution on completion, and interruption handling via CompleteEarly distinction.

## Summary

Implemented the full casting-time concentration lifecycle for magazine reload and spell casting. The key insight was discovering that the existing EffectList treated all "expire early" events as removals (OnRemove), but concentration effects that complete early need to be treated as successful completions (OnExpire). This led to adding the CompleteEarly pattern.

## What Was Done

### Task 1: ConcentrationCompletionResult class
Added a new class to communicate concentration results to the UI/controller layer:
- ActionType: "MagazineReload" or "SpellCast"
- Payload: Serialized action data for processing
- Message: User-facing completion/interruption message
- Success: true for completion, false for interruption

### Task 2: CharacterEdit.LastConcentrationResult
Added transient (non-CSLA) property for behavior-to-UI communication:
- Private field with [NonSerialized] attribute
- Public property with getter/setter
- ClearConcentrationResult() method for post-processing cleanup

### Task 3: OnTick refactoring
Replaced the hardcoded MagazineReload switch with generic casting-time handling:
- IsCastingTimeConcentration() checks for MagazineReload, SpellCasting, RitualPreparation
- TickCastingTime() increments progress, updates description, returns CompleteEarly when done
- GetProgressDescription() provides type-specific progress strings

### Task 4: OnExpire with deferred action execution
Implemented deferred action execution for successful completion:
- ExecuteDeferredAction() dispatches to type-specific handlers
- ExecuteMagazineReload() sets LastConcentrationResult with Success=true
- ExecuteSpellCast() sets LastConcentrationResult (stub for future spell system)

### Task 5: OnRemove for interruption handling
Implemented interruption cleanup without executing deferred action:
- HandleCastingTimeInterruption() sets LastConcentrationResult with Success=false
- Deferred action is NOT executed when interrupted

### Task 6-7: Helper methods updated
Updated CreateMagazineReloadState and added CreateSpellCastingState:
- Full deferred action payload included
- CompletionMessage and InterruptionMessage configured
- RoundsPerTick set to 1 for proper progress tracking

### Task 8: Comprehensive tests
Created 12 tests covering:
- Progress tracking and early completion
- Description updates
- OnExpire result for MagazineReload and SpellCast
- OnRemove interruption result
- Helper method serialization
- Edge cases (invalid state, spell casting progress)

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Fixed CompleteEarly vs ExpireEarly distinction**
- **Found during:** Task 3 implementation
- **Issue:** EffectList.EndOfRound called OnRemove for all ShouldExpireEarly results, but concentration completion needs OnExpire
- **Fix:** Added ExpireAsComplete flag to EffectTickResult and EffectTickResult.CompleteEarly() factory method
- **Files modified:** GameMechanics/Effects/EffectTickResult.cs, GameMechanics/EffectList.cs
- **Commits:** 2ac199b

This was a critical fix - without it, concentration completion would have called OnRemove (which doesn't execute the deferred action) instead of OnExpire (which does).

## Key Links Verified

| From | To | Via | Pattern |
|------|-----|-----|---------|
| ConcentrationBehavior.OnExpire | CharacterEdit.LastConcentrationResult | stores ConcentrationCompletionResult for UI processing | `LastConcentrationResult.*=.*ConcentrationCompletionResult` |
| ConcentrationBehavior.OnTick | EffectTickResult.CompleteEarly | returns CompleteEarly when progress complete | `CompleteEarly.*CompletionMessage` |

## Commits

| Hash | Type | Description |
|------|------|-------------|
| ec7a53e | feat | implement casting-time concentration lifecycle |
| 2ac199b | feat | add CompleteEarly for successful early completion |
| 7b3e500 | feat | add LastConcentrationResult for behavior-to-UI communication |
| e907e1a | test | add casting-time concentration lifecycle tests |

## Next Phase Readiness

**Phase 22-03: Sustained Concentration**
- Prerequisites: ConcentrationState schema (22-01), OnTick/OnExpire/OnRemove lifecycle (22-02)
- Ready: Yes
- Concerns: None

**Phase 22-07: UI Components**
- Prerequisites: LastConcentrationResult property (22-02)
- Ready: Yes, UI can now process concentration completion/interruption results
- Concerns: None

## Test Results

```
Passed!  - Failed: 0, Passed: 12, Skipped: 0, Total: 12
```

All tests pass covering:
- OnTick progress tracking (3 tests)
- OnExpire deferred action execution (2 tests)
- OnRemove interruption handling (2 tests)
- Helper method serialization (2 tests)
- CharacterEdit integration (1 test)
- Edge cases (2 tests)

---

*Summary created: 2026-01-29*
*Duration: 8 minutes*
