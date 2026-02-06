---
phase: 29-batch-action-framework
plan: 01
subsystem: batch-actions
tags: [batch, damage, healing, service, CSLA]
completed: 2026-02-05
duration: 5min
dependency-graph:
  requires: [28-selection-infrastructure]
  provides: [BatchActionService, BatchActionRequest, BatchActionResult]
  affects: [29-02, 29-03]
tech-stack:
  added: []
  patterns: [sequential-batch-processing, error-aggregation, single-notification]
key-files:
  created:
    - GameMechanics/Batch/BatchActionRequest.cs
    - GameMechanics/Batch/BatchActionResult.cs
    - GameMechanics/Batch/BatchActionService.cs
  modified:
    - GameMechanics/ServiceCollectionExtensions.cs
decisions:
  - id: 29-01-D1
    decision: "BatchActionRequest as record type (not class) to support 'with' expression"
    reason: "Service uses 'request with { ActionType = ... }' to enforce action type in Apply methods"
---

# Phase 29 Plan 01: BatchActionService Backend Summary

**One-liner:** Sequential batch damage/healing service with per-character error isolation and single CharactersUpdatedMessage notification, following TimeAdvancementService pattern.

## What Was Done

### Task 1: Create BatchActionRequest and BatchActionResult DTOs
- Created `BatchActionRequest` as a `record` with TableId, CharacterIds, ActionType, Pool, Amount
- Created `BatchActionResult` class with SuccessIds, SuccessNames, FailedIds, Errors lists
- Created `BatchActionType` enum (Damage, Healing)
- Result includes computed properties: TotalCount, HasFailures, AllSucceeded, Summary
- **Commit:** 6eff35d

### Task 2: Create BatchActionService with sequential processing
- Implemented `BatchActionService` with `ApplyDamageAsync` and `ApplyHealingAsync` methods
- Sequential foreach loop per character (CSLA objects are not thread-safe)
- Per-character try/catch for error isolation (partial failure support)
- Applies pending damage/healing to Fatigue or Vitality pool
- Single `CharactersUpdatedMessage` published after batch completes (prevents N refresh cycles)
- **Commit:** c78ae16

### Task 3: Register BatchActionService in DI
- Added `using GameMechanics.Batch` to ServiceCollectionExtensions.cs
- Registered `BatchActionService` as scoped service in `AddGameMechanics()`
- **Commit:** c570778

## Decisions Made

| ID | Decision | Rationale |
|----|----------|-----------|
| 29-01-D1 | BatchActionRequest as `record` (not `class`) | Service uses `with` expression to enforce ActionType in ApplyDamageAsync/ApplyHealingAsync methods |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 1 - Bug] Changed BatchActionRequest from class to record**
- **Found during:** Task 2 implementation planning
- **Issue:** Plan specified `class` for BatchActionRequest, but the service code uses `request with { ActionType = ... }` which requires a `record` type
- **Fix:** Declared BatchActionRequest as `record` instead of `class`
- **Files modified:** GameMechanics/Batch/BatchActionRequest.cs
- **Commit:** 6eff35d

**2. [Rule 3 - Blocking] Added missing using statements**
- **Found during:** Task 1 build verification
- **Issue:** Project does not have implicit usings enabled; System and System.Collections.Generic were missing
- **Fix:** Added explicit using directives to all new files
- **Files modified:** BatchActionRequest.cs, BatchActionResult.cs
- **Commit:** 6eff35d

## Verification

- [x] `dotnet build GameMechanics/GameMechanics.csproj` - passes with 0 errors, 0 warnings
- [x] `dotnet build Threa.Client/Threa.Client.csproj` - passes with 0 errors
- [x] `dotnet build GameMechanics.Test/GameMechanics.Test.csproj` - passes with 0 errors
- [x] Full solution builds (only Threa host fails due to running process file lock, not code issue)
- [x] BatchActionService is injectable via ServiceCollectionExtensions

## Next Phase Readiness

Plan 29-02 (BatchActionBar UI) can proceed. The service is registered and ready for injection into Blazor components. Key integration points:
- Inject `BatchActionService` into component
- Create `BatchActionRequest` with selected character IDs from Phase 28's `HashSet<int>`
- Call `ApplyDamageAsync` or `ApplyHealingAsync`
- Display `BatchActionResult.Summary` for user feedback
