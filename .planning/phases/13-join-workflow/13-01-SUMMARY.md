---
phase: 13-join-workflow
plan: 01
subsystem: data-layer
tags: [dto, dal, csla, messaging, blazor]
completed: 2026-01-27
duration: 6 min

dependency-graph:
  requires: [12-table-foundation]
  provides: [JoinRequest-DTO, IJoinRequestDal, JoinRequestMessage]
  affects: [13-02, 13-03]

tech-stack:
  added: []
  patterns: [repository-pattern, csla-property-pattern, rx-messaging]

key-files:
  created:
    - Threa.Dal/Dto/JoinRequest.cs
    - Threa.Dal/IJoinRequestDal.cs
    - Threa.Dal.MockDb/JoinRequestDal.cs
  modified:
    - Threa.Dal/Dto/GameTable.cs
    - Threa.Dal.MockDb/MockDb.cs
    - Threa.Dal.MockDb/ConfigurationExtensions.cs
    - GameMechanics/GamePlay/TableEdit.cs
    - GameMechanics/GamePlay/TableInfo.cs
    - GameMechanics/Messaging/TimeMessages.cs
    - GameMechanics/Messaging/ITimeEventPublisher.cs
    - GameMechanics.Messaging.InMemory/InMemoryTimeEventPublisher.cs
    - GameMechanics.Messaging.InMemory/InMemoryMessageBus.cs
    - Threa/Threa.Client/Components/Pages/GameMaster/CampaignCreate.razor

decisions: []

metrics:
  tasks: 3/3
  files-created: 3
  files-modified: 10
---

# Phase 13 Plan 01: Data Layer Foundation Summary

**One-liner:** JoinRequest DTO with status enum, DAL interface/MockDb, campaign Description field, and JoinRequestMessage for real-time notifications.

## What Was Built

### JoinRequest DTO and Status Enum
Created `Threa.Dal/Dto/JoinRequest.cs` with complete join request model:
- `Id` (Guid) - unique request identifier
- `CharacterId` (int) - character being joined
- `TableId` (Guid) - target table
- `PlayerId` (int) - character owner
- `Status` (JoinRequestStatus) - Pending=0, Approved=1, Denied=2
- `RequestedAt` (DateTime) - submission timestamp
- `ProcessedAt` (DateTime?) - when GM processed
- `DenialReason` (string?) - optional denial explanation

### IJoinRequestDal Interface and MockDb Implementation
Created `Threa.Dal/IJoinRequestDal.cs` with CRUD operations:
- `GetBlank()` - create new request with defaults
- `GetRequestsByPlayerAsync(playerId)` - player's active requests (excludes denied)
- `GetPendingRequestsForTableAsync(tableId)` - GM's pending queue
- `GetPendingRequestAsync(characterId, tableId)` - check for duplicate
- `GetRequestAsync(id)` - fetch by ID
- `SaveRequestAsync(request)` - insert/update
- `DeleteRequestAsync(id)` - remove request
- `GetPendingCountForTableAsync(tableId)` - badge count for GM
- `DeleteRequestsByCharacterAsync(characterId)` - cleanup on character delete

MockDb implementation uses thread-safe list operations with locking.

### GameTable Description Field
Added `Description` property to GameTable DTO, TableEdit, and TableInfo:
- Used for campaign browsing and player discovery
- Follows existing CSLA property pattern
- Mapped in MapFromDto/MapToDto methods

### Campaign Creation Description Requirement
Updated CampaignCreate.razor:
- Added required description textarea
- Added `showDescriptionError` validation
- Updated Create button disabled condition
- Saves description to table on creation

### JoinRequestMessage for Notifications
Added `JoinRequestMessage` to TimeMessages.cs:
- Extends `TimeMessageBase` for consistency
- Contains `RequestId`, `CharacterId`, `PlayerId`, `TableId`, `Status`
- Includes `CharacterName` and `TableName` for display

Added `PublishJoinRequestAsync` to ITimeEventPublisher and InMemoryTimeEventPublisher.
Added `JoinRequests` Subject to InMemoryMessageBus.

## Commits

| Commit | Description |
|--------|-------------|
| 0f0bca0 | JoinRequest DTO and Description to GameTable |
| 632b791 | IJoinRequestDal interface and MockDb implementation |
| fe542d0 | Description to Table and JoinRequestMessage for notifications |

## Deviations from Plan

None - plan executed exactly as written.

## Testing Notes

- `dotnet build Threa.sln` succeeds with no new warnings
- All verification criteria met:
  - JoinRequest.cs exists with all properties
  - JoinRequestStatus enum has Pending=0, Approved=1, Denied=2
  - IJoinRequestDal.cs has all method signatures
  - JoinRequestDal.cs implements all methods
  - TableEdit and TableInfo have Description property
  - TimeMessages.cs contains JoinRequestMessage
  - ITimeEventPublisher has PublishJoinRequestAsync
  - CampaignCreate.razor has description textarea with validation

## Next Phase Readiness

Ready for 13-02 (JoinRequest Business Logic):
- JoinRequest DTO provides data model
- IJoinRequestDal provides data access abstraction
- JoinRequestMessage enables real-time notification
- Description field allows campaign discovery

Dependencies satisfied:
- TableEdit has Description for campaign discovery
- Messaging infrastructure ready for join notifications
