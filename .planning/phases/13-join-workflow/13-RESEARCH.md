# Phase 13: Join Workflow - Research

**Researched:** 2026-01-27
**Domain:** CSLA.NET business objects, Blazor workflow pages, join request state management
**Confidence:** HIGH

## Summary

Phase 13 implements the join workflow allowing players to request joining campaigns with their characters, and GMs to approve or deny those requests. The existing codebase has substantial infrastructure: `TableCharacterAttacher`/`TableCharacterDetacher` command objects for table membership, `ITableDal` with character-table relationships, `TableCharacter` DTO tracking join timestamps, and a messaging infrastructure (`ITimeEventPublisher`/`ITimeEventSubscriber`) for real-time notifications.

The primary work involves creating a new `JoinRequest` data model to track pending/approved/denied requests, extending the DAL with join request operations, creating player-facing pages (campaign browse, join request submission, pending requests view), creating GM-facing components (pending request badges on Campaigns.razor, review interface on GmTable.razor), and adding join request notification messages.

**Primary recommendation:** Create `JoinRequest` DTO and corresponding DAL interface/implementation, extend existing messaging infrastructure for join request notifications, build workflow pages following existing Blazor patterns (Campaigns.razor, CampaignCreate.razor), and integrate pending request display into GmTable.razor.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CSLA.NET | 9.1.0 | Business object framework | Already used throughout, handles validation, data access |
| Blazor (InteractiveServer) | in SDK | Server-rendered interactive components | Already used for all interactive pages |
| Bootstrap | 5.x | CSS framework | Already imported, used for layout and components |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Bootstrap Icons | 1.11.3 | Icon library | Visual indicators for status (bi-check-circle, bi-x-circle, bi-hourglass) |
| Rx.NET (InMemory) | 6.x | In-memory message bus | Real-time notifications via existing infrastructure |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| JoinRequest DTO | Extend TableCharacter | TableCharacter is for active membership; join requests are a separate lifecycle |
| Real-time via Rx.NET | Polling | Real-time already implemented for other features, consistent UX |
| Bootstrap table | RadzenDataGrid | Bootstrap simpler for small lists (per Phase 12 decision) |

**Installation:**
No new packages required. All dependencies already present.

## Architecture Patterns

### Recommended Project Structure
```
Threa.Dal/
├── IJoinRequestDal.cs              # NEW: DAL interface for join requests
├── Dto/
│   └── JoinRequest.cs              # NEW: Join request entity

Threa.Dal.MockDb/
└── JoinRequestDal.cs               # NEW: In-memory implementation

GameMechanics/
├── GamePlay/
│   ├── JoinRequestSubmitter.cs     # NEW: Command to submit join request
│   ├── JoinRequestProcessor.cs     # NEW: Command to approve/deny (GM)
│   ├── JoinRequestInfo.cs          # NEW: Read-only info for lists
│   └── JoinRequestList.cs          # NEW: Read-only list of requests
└── Messaging/
    └── TimeMessages.cs             # EXTEND: Add JoinRequestMessage

Threa/Threa.Client/Components/Pages/
├── Player/
│   ├── BrowseCampaigns.razor       # NEW: Character-first campaign browser
│   └── MyJoinRequests.razor        # NEW: Player's pending/approved requests
└── GameMaster/
    └── Campaigns.razor             # MODIFY: Add pending request badge
    └── GmTable.razor               # MODIFY: Add pending requests section
```

### Pattern 1: CSLA CommandBase for State Transitions
**What:** Use CommandBase for operations that change state (submit, approve, deny)
**When to use:** Join request submission and processing
**Example:**
```csharp
// Source: Existing TableCharacterAttacher.cs pattern
[Serializable]
public class JoinRequestSubmitter : CommandBase<JoinRequestSubmitter>
{
    public static readonly PropertyInfo<bool> SuccessProperty = RegisterProperty<bool>(nameof(Success));
    public bool Success
    {
        get => ReadProperty(SuccessProperty);
        private set => LoadProperty(SuccessProperty, value);
    }

    public static readonly PropertyInfo<string?> ErrorMessageProperty = RegisterProperty<string?>(nameof(ErrorMessage));
    public string? ErrorMessage
    {
        get => ReadProperty(ErrorMessageProperty);
        private set => LoadProperty(ErrorMessageProperty, value);
    }

    public static readonly PropertyInfo<Guid?> RequestIdProperty = RegisterProperty<Guid?>(nameof(RequestId));
    public Guid? RequestId
    {
        get => ReadProperty(RequestIdProperty);
        private set => LoadProperty(RequestIdProperty, value);
    }

    [Execute]
    private async Task ExecuteAsync(
        int characterId,
        Guid tableId,
        int playerId,
        [Inject] IJoinRequestDal joinRequestDal,
        [Inject] ICharacterDal characterDal,
        [Inject] ITableDal tableDal)
    {
        // Validate character exists, belongs to player, is playable
        // Check character not already in another campaign
        // Check for existing pending request
        // Create join request
    }
}
```

### Pattern 2: ReadOnlyListBase for Request Lists
**What:** Use ReadOnlyListBase for fetching lists of join requests
**When to use:** Player's pending requests, GM's review queue
**Example:**
```csharp
// Source: Existing CharacterList.cs pattern
[Serializable]
public class JoinRequestList : ReadOnlyListBase<JoinRequestList, JoinRequestInfo>
{
    [Fetch]
    private async Task FetchForPlayer(
        int playerId,
        [Inject] IJoinRequestDal dal,
        [Inject] IChildDataPortal<JoinRequestInfo> requestPortal)
    {
        var items = await dal.GetRequestsByPlayerAsync(playerId);
        using (LoadListMode)
        {
            foreach (var item in items)
            {
                Add(requestPortal.FetchChild(item));
            }
        }
    }

    [Fetch]
    private async Task FetchForTable(
        Guid tableId,
        [Inject] IJoinRequestDal dal,
        [Inject] IChildDataPortal<JoinRequestInfo> requestPortal)
    {
        var items = await dal.GetPendingRequestsForTableAsync(tableId);
        using (LoadListMode)
        {
            foreach (var item in items)
            {
                Add(requestPortal.FetchChild(item));
            }
        }
    }
}
```

### Pattern 3: Messaging for Notifications
**What:** Extend existing messaging infrastructure for join request notifications
**When to use:** Notify player when request approved/denied
**Example:**
```csharp
// Source: Existing TimeMessages.cs CharacterUpdateMessage pattern
public class JoinRequestMessage : TimeMessageBase
{
    public Guid RequestId { get; init; }
    public int CharacterId { get; init; }
    public int PlayerId { get; init; }
    public Guid TableId { get; init; }
    public JoinRequestStatus Status { get; init; }
    public string? CharacterName { get; init; }
    public string? TableName { get; init; }
}

public enum JoinRequestStatus
{
    Pending,
    Approved,
    Denied
}
```

### Pattern 4: Character-First Navigation
**What:** Player selects character first, then browses campaigns available for that character
**When to use:** Campaign browsing page per CONTEXT.md decision
**Example:**
```razor
@* Source: Pattern from existing Play.razor character selection *@
@if (selectedCharacter == null)
{
    <h3>Select a Character</h3>
    <div class="list-group" style="max-width: 500px;">
        @foreach (var charInfo in availableCharacters)
        {
            <div class="list-group-item d-flex justify-content-between align-items-center">
                <div @onclick="() => SelectCharacter(charInfo)">
                    <strong>@charInfo.Name</strong>
                    <small class="text-muted">@charInfo.Species</small>
                </div>
            </div>
        }
    </div>
}
else
{
    <h3>Join a Campaign with @selectedCharacter.Name</h3>
    @* Campaign cards here *@
}
```

### Anti-Patterns to Avoid
- **Using TableCharacter for pending requests:** TableCharacter is for active membership only; separate JoinRequest entity tracks the request lifecycle
- **Confirmation dialogs everywhere:** Decision specifies immediate submission, no confirm for deny
- **Batch operations:** Individual approve/deny buttons per request, no bulk actions
- **Polling for notifications:** Use existing Rx.NET infrastructure for real-time updates

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Real-time notifications | Custom WebSocket handling | `ITimeEventPublisher.PublishJoinRequestAsync` | Existing Rx.NET infrastructure handles pub/sub |
| Character-campaign validation | Manual checks | Extend `TableCharacterAttacher` logic | Already validates character ownership, playability |
| Request list display | Custom HTML tables | Bootstrap table (like Campaigns.razor) | Consistent with Phase 12 decision |
| Status transitions | If/else chains | Enum-based state machine | Clear, testable state management |

**Key insight:** The existing codebase has complete messaging, character validation, and table membership infrastructure. The work is primarily adding a new request state entity and workflow pages, not building new systems.

## Common Pitfalls

### Pitfall 1: Character Already in Another Campaign
**What goes wrong:** Player submits join request, but character is already active elsewhere
**Why it happens:** Missing validation before request submission
**How to avoid:** Check `GetTableForCharacterAsync` in `JoinRequestSubmitter`, return error if active
**Warning signs:** Error message when approving: "Character is already attached to another table"

```csharp
// In JoinRequestSubmitter.Execute:
var existingTable = await tableDal.GetTableForCharacterAsync(characterId);
if (existingTable != null)
{
    LoadProperty(SuccessProperty, false);
    LoadProperty(ErrorMessageProperty,
        $"{character.Name} is already active in {existingTable.Name}. Leave that campaign first.");
    return;
}
```

### Pitfall 2: Duplicate Pending Requests
**What goes wrong:** Player submits multiple requests to same campaign
**Why it happens:** No check for existing pending request
**How to avoid:** Check for existing pending request before creating new one
**Warning signs:** GM sees duplicate entries in pending list

```csharp
var existing = await joinRequestDal.GetPendingRequestAsync(characterId, tableId);
if (existing != null)
{
    LoadProperty(SuccessProperty, false);
    LoadProperty(ErrorMessageProperty, "You already have a pending request for this campaign.");
    return;
}
```

### Pitfall 3: Stale Pending Request Count
**What goes wrong:** GM sees "3 pending" badge but list shows different count
**Why it happens:** Badge not updating when requests approved/denied
**How to avoid:** Refresh badge after any request state change; consider using messaging
**Warning signs:** Badge count doesn't match actual list length

### Pitfall 4: No Redirect After Player Has No Characters
**What goes wrong:** Player with no characters lands on empty campaign browse page
**Why it happens:** CONTEXT.md decision says block campaign browsing entirely for characterless players
**How to avoid:** Check character count at page load, redirect to character creation
**Warning signs:** Empty campaign browse with no helpful message

```razor
@if (availableCharacters == null || !availableCharacters.Any())
{
    NavigationManager.NavigateTo("/player/character?message=create-to-join");
}
```

### Pitfall 5: Approved Request Not Activating Character
**What goes wrong:** GM approves request but character doesn't appear at table
**Why it happens:** Approval only updates request status, doesn't call `AddCharacterToTableAsync`
**How to avoid:** Approval must both update request status AND add character to table
**Warning signs:** Approved status but character not in table character list

## Code Examples

Verified patterns from existing codebase:

### JoinRequest DTO
```csharp
// Source: Pattern from GameTable.cs and TableCharacter.cs
public class JoinRequest
{
    public Guid Id { get; set; }
    public int CharacterId { get; set; }
    public Guid TableId { get; set; }
    public int PlayerId { get; set; }
    public JoinRequestStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public string? DenialReason { get; set; }
}

public enum JoinRequestStatus
{
    Pending = 0,
    Approved = 1,
    Denied = 2
}
```

### IJoinRequestDal Interface
```csharp
// Source: Pattern from ITableDal.cs
public interface IJoinRequestDal
{
    JoinRequest GetBlank();
    Task<List<JoinRequest>> GetRequestsByPlayerAsync(int playerId);
    Task<List<JoinRequest>> GetPendingRequestsForTableAsync(Guid tableId);
    Task<JoinRequest?> GetPendingRequestAsync(int characterId, Guid tableId);
    Task<JoinRequest> GetRequestAsync(Guid id);
    Task<JoinRequest> SaveRequestAsync(JoinRequest request);
    Task DeleteRequestAsync(Guid id);
    Task<int> GetPendingCountForTableAsync(Guid tableId);
}
```

### Campaign Card with Description
```razor
@* Source: Pattern from Campaigns.razor table, extended with cards *@
<div class="row row-cols-1 row-cols-md-2 row-cols-lg-3 g-4">
    @foreach (var campaign in availableCampaigns)
    {
        <div class="col">
            <div class="card h-100 campaign-card" @onclick="() => SelectCampaign(campaign)">
                <div class="card-body">
                    <div class="d-flex justify-content-between align-items-start mb-2">
                        <h5 class="card-title mb-0">@campaign.Name</h5>
                        <ThemeIndicator Theme="@campaign.Theme" />
                    </div>
                    <p class="card-text text-muted small">@campaign.Description</p>
                    <div class="d-flex justify-content-between align-items-center mt-3">
                        <small class="text-muted">
                            <i class="bi bi-person"></i> GM: @campaign.GmName
                        </small>
                        <small class="text-muted">
                            <i class="bi bi-people"></i> @campaign.PlayerCount players
                        </small>
                    </div>
                </div>
                <div class="card-footer bg-transparent">
                    <button class="btn btn-primary w-100"
                            @onclick:stopPropagation="true"
                            @onclick="() => SubmitJoinRequest(campaign.Id)">
                        Join with @selectedCharacter.Name
                    </button>
                </div>
            </div>
        </div>
    }
</div>
```

### Pending Requests Badge on Campaigns.razor
```razor
@* Source: Pattern from badge usage in GmTable.razor *@
<tr style="cursor: pointer;" @onclick="() => OpenCampaign(table.Id)">
    <td class="fw-medium">
        @table.Name
        @if (GetPendingCount(table.Id) > 0)
        {
            <span class="badge bg-warning text-dark ms-2">
                @GetPendingCount(table.Id) pending
            </span>
        }
    </td>
    <td><ThemeIndicator Theme="@table.Theme" /></td>
    <td class="text-muted">@table.StartTimeSeconds</td>
    <td class="text-muted">@table.CreatedAt.ToString("g")</td>
</tr>
```

### GM Review Section on Dashboard
```razor
@* Source: Pattern from GmTable.razor character list panel *@
@if (pendingRequests != null && pendingRequests.Any())
{
    <div class="card mb-3">
        <div class="card-header d-flex justify-content-between align-items-center bg-warning text-dark">
            <strong>Pending Join Requests</strong>
            <span class="badge bg-dark">@pendingRequests.Count()</span>
        </div>
        <div class="card-body" style="max-height: 400px; overflow-y: auto;">
            @foreach (var request in pendingRequests)
            {
                <div class="card mb-2">
                    <div class="card-body py-2">
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <div>
                                <strong>@request.CharacterName</strong>
                                <small class="text-muted d-block">@request.Species</small>
                            </div>
                            <small class="text-muted">@request.RequestedAt.ToString("g")</small>
                        </div>

                        @* Character details accordion *@
                        <div class="mb-2">
                            <button class="btn btn-sm btn-outline-secondary"
                                    @onclick="() => ToggleCharacterDetails(request.CharacterId)">
                                <i class="bi bi-eye"></i> View Character
                            </button>
                        </div>

                        @if (expandedCharacterId == request.CharacterId)
                        {
                            @* Full character sheet, inventory, narrative *@
                        }

                        <div class="d-flex gap-2">
                            <button class="btn btn-success btn-sm"
                                    @onclick="() => ApproveRequest(request.Id)">
                                <i class="bi bi-check-lg"></i> Approve
                            </button>
                            <button class="btn btn-danger btn-sm"
                                    @onclick="() => DenyRequest(request.Id)">
                                <i class="bi bi-x-lg"></i> Deny
                            </button>
                        </div>
                    </div>
                </div>
            }
        </div>
    </div>
}
```

### Player Pending Requests View
```razor
@* Source: Pattern from Play.razor and Characters.razor *@
@page "/player/join-requests"

<h3>My Join Requests</h3>

@if (requests == null)
{
    <p>Loading...</p>
}
else if (!requests.Any())
{
    <div class="alert alert-info">
        <p>You have no pending or recent join requests.</p>
        <a href="/player/browse-campaigns" class="btn btn-primary">Browse Campaigns</a>
    </div>
}
else
{
    <div class="list-group" style="max-width: 600px;">
        @foreach (var request in requests.OrderByDescending(r => r.RequestedAt))
        {
            <div class="list-group-item">
                <div class="d-flex justify-content-between align-items-center">
                    <div>
                        <strong>@request.CharacterName</strong>
                        <small class="text-muted d-block">@request.CampaignName</small>
                    </div>
                    <div>
                        @if (request.Status == JoinRequestStatus.Pending)
                        {
                            <span class="badge bg-warning text-dark">
                                <i class="bi bi-hourglass-split"></i> Pending
                            </span>
                        }
                        else if (request.Status == JoinRequestStatus.Approved)
                        {
                            <span class="badge bg-success">
                                <i class="bi bi-check-circle"></i> Approved
                            </span>
                            <a href="/play/@request.TableId" class="btn btn-sm btn-success ms-2">
                                Go to Campaign
                            </a>
                        }
                        @* Denied requests are deleted, so no display needed *@
                    </div>
                </div>
            </div>
        }
    </div>
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Direct table attachment | Request -> Approval workflow | This phase | GM control over who joins |
| Immediate join from Play.razor | Character-first browse, then join | This phase | Better campaign discovery |

**Deprecated/outdated:**
- Direct attachment via `TableCharacterAttacher` in Play.razor: Still exists for backward compatibility but join workflow should use request system
- Modal table selection in Play.razor: Will coexist with new browse page

## Open Questions

Things that couldn't be fully resolved:

1. **Description Field on CampaignCreate.razor**
   - What we know: CONTEXT.md says "CampaignCreate.razor needs description field added"
   - What's unclear: This is a Phase 12 modification; should it be done as part of Phase 13 or as Phase 12 followup?
   - Recommendation: Include as first task in Phase 13 since it's required for campaign browsing to work

2. **Navigation Placement for "My Join Requests"**
   - What we know: Player needs to see their pending/approved requests
   - What's unclear: Where in nav should this appear?
   - Recommendation: Add to player dropdown menu alongside "My Characters", or as tab on Characters page

3. **Character Removal UI Placement**
   - What we know: GM can remove character from active table with confirmation
   - What's unclear: Best placement on GmTable.razor
   - Recommendation: Add "Remove" button next to each character in "Characters at Table" card, or in character detail view

4. **Messaging Extension**
   - What we know: Need to notify players of approval/denial
   - What's unclear: Should this extend `ITimeEventPublisher` or be separate interface?
   - Recommendation: Extend existing infrastructure since it already handles table-scoped notifications

## Sources

### Primary (HIGH confidence)
- `S:/src/rdl/threa/GameMechanics/GamePlay/TableCharacterAttacher.cs` - Command pattern for table membership
- `S:/src/rdl/threa/GameMechanics/GamePlay/TableCharacterDetacher.cs` - Command pattern for removal
- `S:/src/rdl/threa/Threa.Dal/ITableDal.cs` - DAL interface pattern
- `S:/src/rdl/threa/Threa.Dal/Dto/GameTable.cs` - DTO with TableCharacter
- `S:/src/rdl/threa/GameMechanics/Player/CharacterList.cs` - ReadOnlyListBase pattern
- `S:/src/rdl/threa/GameMechanics/Messaging/TimeMessages.cs` - Messaging DTOs
- `S:/src/rdl/threa/GameMechanics/Messaging/ITimeEventPublisher.cs` - Publisher interface
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Pages/GameMaster/Campaigns.razor` - GM list page
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Pages/GamePlay/GmTable.razor` - Campaign dashboard
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Pages/GamePlay/Play.razor` - Player flow patterns

### Secondary (MEDIUM confidence)
- `S:/src/rdl/threa/.planning/phases/12-table-foundation/12-RESEARCH.md` - Phase 12 patterns and decisions

### Tertiary (LOW confidence)
- None - all findings verified from codebase

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - verified from existing project dependencies and patterns
- Architecture: HIGH - verified from existing CSLA patterns and Blazor components
- Pitfalls: HIGH - derived from existing validation patterns in TableCharacterAttacher

**Research date:** 2026-01-27
**Valid until:** 2026-02-26 (30 days - stable domain, existing patterns)
