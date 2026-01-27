---
phase: 13-join-workflow
plan: 02
subsystem: business-logic
tags: [csla, command, read-only, messaging]
completed: 2026-01-27
duration: 2 min

dependency-graph:
  requires: [13-01]
  provides: [JoinRequestSubmitter, JoinRequestProcessor, JoinRequestInfo, JoinRequestList, PendingRequestCountFetcher]
  affects: [13-03]

tech-stack:
  added: []
  patterns: [csla-command-pattern, csla-readonly-pattern, repository-injection]

key-files:
  created:
    - GameMechanics/GamePlay/JoinRequestSubmitter.cs
    - GameMechanics/GamePlay/JoinRequestProcessor.cs
    - GameMechanics/GamePlay/JoinRequestInfo.cs
    - GameMechanics/GamePlay/JoinRequestList.cs
    - GameMechanics/GamePlay/PendingRequestCountFetcher.cs
  modified: []

decisions: []

metrics:
  tasks: 3/3
  files-created: 5
  files-modified: 0
---

# Phase 13 Plan 02: Join Request Business Logic Summary

**One-liner:** CSLA commands for join request submission with ownership/playability validation, approve/deny processing with notification publishing, and read-only objects for UI display.

## What Was Built

### JoinRequestSubmitter Command
Created `GameMechanics/GamePlay/JoinRequestSubmitter.cs` following TableCharacterAttacher pattern:
- Validates character exists and belongs to requesting player
- Validates character is activated for play (IsPlayable)
- Checks single-campaign constraint (JOIN-09): blocks if character already active elsewhere
- Prevents duplicate pending requests for same character/table
- Returns Success, ErrorMessage, and RequestId properties

### JoinRequestProcessor Command
Created `GameMechanics/GamePlay/JoinRequestProcessor.cs` for GM approve/deny workflow:
- Validates GM owns the target table
- Validates request is still pending (not already processed)
- On **approval**:
  1. Validates character still exists
  2. Double-checks character not in another campaign
  3. Attaches character to table via AddCharacterToTableAsync
  4. Updates request status to Approved
  5. Publishes JoinRequestMessage via ITimeEventPublisher
- On **denial**:
  1. Publishes JoinRequestMessage BEFORE delete (order matters for notification)
  2. Deletes request (per CONTEXT.md - immediate deletion)
- Returns Success, ErrorMessage, and CharacterName properties

### JoinRequestInfo Read-Only Object
Created `GameMechanics/GamePlay/JoinRequestInfo.cs` for display:
- Id, CharacterId, CharacterName, Species
- TableId, TableName, PlayerId
- Status, RequestedAt
- Helper properties: IsPending, IsApproved

### JoinRequestList Read-Only Collection
Created `GameMechanics/GamePlay/JoinRequestList.cs` with two fetch modes:
- **FetchForPlayer(playerId)**: Gets all non-denied requests for player's "My Requests" view
- **FetchForTable(tableId)**: Gets pending requests for GM review

### PendingRequestCountFetcher Command
Created `GameMechanics/GamePlay/PendingRequestCountFetcher.cs`:
- Returns Count property for badge display ("3 pending")
- Simple command that calls IJoinRequestDal.GetPendingCountForTableAsync

## Commits

| Commit | Description |
|--------|-------------|
| 86dd9ed | JoinRequestSubmitter with ownership/playability/single-campaign validation |
| d8c3aac | JoinRequestProcessor with approve/deny and notification publishing |
| 61057b8 | JoinRequestInfo, JoinRequestList, and PendingRequestCountFetcher |

## Deviations from Plan

None - plan executed exactly as written.

## Testing Notes

- `dotnet build GameMechanics/GameMechanics.csproj` succeeds with no new warnings
- All verification criteria met:
  - JoinRequestSubmitter validates all constraints per CONTEXT.md
  - JoinRequestProcessor handles approve (attach + notify) and deny (notify + delete)
  - JoinRequestProcessor injects ITimeEventPublisher and calls PublishJoinRequestAsync
  - JoinRequestInfo has all display properties
  - JoinRequestList has both Fetch overloads
  - PendingRequestCountFetcher returns integer count

## Next Phase Readiness

Ready for 13-03 (UI Integration):
- JoinRequestSubmitter ready for "Join Campaign" button
- JoinRequestProcessor ready for GM approve/deny buttons
- JoinRequestList ready for player "My Requests" page
- JoinRequestList ready for GM "Pending Requests" section
- PendingRequestCountFetcher ready for badge display

Dependencies satisfied:
- All business logic validation in place
- Notification publishing integrated
- Read-only display objects ready for Blazor binding
