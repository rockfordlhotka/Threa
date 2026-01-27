# Phase 15: Dashboard Details - Research

**Researched:** 2026-01-27
**Domain:** Blazor Server Modal Dialogs, Real-time Data Binding, Rx.NET Subscriptions
**Confidence:** HIGH

## Summary

This phase extends the existing GM dashboard (GmTable.razor) with detailed character drill-down capabilities via modal dialogs, comprehensive character views across three tabs (Character Sheet, Inventory, Narrative), and ensures real-time updates propagate to the detail modal while open.

The codebase already has well-established patterns for all required features:
- **Modal dialogs:** Radzen DialogService with established usage patterns in UserEditModal and TabPlayInventory
- **Tab-based interfaces:** Existing tab components in Play.razor (Status, Combat, Skills, etc.) with themed styling
- **Real-time subscriptions:** ITimeEventSubscriber pattern with multiple message types (CharacterUpdateMessage, TimeEventMessage, etc.)
- **Character data:** Full CharacterEdit business object with all narrative, attribute, skill, and inventory data

**Primary recommendation:** Leverage existing DialogService + ITimeEventSubscriber patterns; create a new CharacterDetailModal component with three tabs reusing existing character display patterns.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Radzen.Blazor | 8.4.2 | Dialog service for modals | Already in use project-wide |
| System.Reactive | (via Rx.NET) | Real-time pub/sub messaging | Existing InMemoryMessageBus pattern |
| CSLA.NET | 9.1.0 | Business objects (CharacterEdit) | Core project architecture |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Bootstrap 5 | (bundled) | Modal and tab CSS classes | Existing theme integration |
| Bootstrap Icons | (bundled) | UI icons | Existing usage throughout |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Radzen DialogService | Custom modal component | More control but reinvents wheel; DialogService already integrated |
| CharacterUpdateMessage | Separate DetailUpdateMessage | Unnecessary complexity; existing message type covers all use cases |

**Installation:**
No new packages required - all dependencies already present.

## Architecture Patterns

### Recommended Project Structure
```
Threa.Client/Components/
  Pages/GamePlay/
    GmTable.razor              # Existing - add onclick to CharacterStatusCard
  Shared/
    CharacterDetailModal.razor # NEW - main modal component
    CharacterDetailSheet.razor # NEW - Character Sheet tab content
    CharacterDetailInventory.razor # NEW - Inventory tab content
    CharacterDetailNarrative.razor # NEW - Narrative tab content
    NpcPlaceholder.razor       # NEW - Expandable NPC placeholder section
```

### Pattern 1: Radzen Dialog with Parameters
**What:** Open modal with DialogService.OpenAsync<TComponent>, pass parameters via Dictionary
**When to use:** Displaying detailed information that needs full CharacterEdit data
**Example:**
```csharp
// Source: Existing pattern in Users.razor:82
var result = await DialogService.OpenAsync<CharacterDetailModal>(
    "Character Details",
    new Dictionary<string, object>
    {
        { "CharacterId", characterId },
        { "TableId", TableId },
        { "AllCharacters", tableCharacters }  // For character switcher dropdown
    },
    new DialogOptions
    {
        Width = "90%",
        Height = "90%",
        CloseDialogOnOverlayClick = false,
        ShowClose = true
    });
```

### Pattern 2: Tab-Based Content Switching
**What:** Use tab-buttons CSS class with active state toggling
**When to use:** Multi-section content within a modal or page
**Example:**
```csharp
// Source: Existing pattern in Play.razor:361-397
<div class="tab-buttons">
    @foreach (var tabName in new[] { "Character Sheet", "Inventory", "Narrative" })
    {
        <button class="tab-link @(activeTab == tabName ? "active" : "")"
                @onclick="() => activeTab = tabName">
            @tabName
        </button>
    }
</div>

<div class="tab-content">
    @if (activeTab == "Character Sheet")
    {
        <CharacterDetailSheet Character="character" />
    }
    // ... other tabs
</div>
```

### Pattern 3: Real-time Subscription with InvokeAsync
**What:** Subscribe to ITimeEventSubscriber events, use InvokeAsync for thread-safe UI updates
**When to use:** Any component needing live data refresh
**Example:**
```csharp
// Source: Existing pattern in GmTable.razor:592-628
private void OnCharacterUpdateReceived(object? sender, CharacterUpdateMessage e)
{
    // Filter for relevant characters
    if (character == null || e.CharacterId != character.Id) return;

    InvokeAsync(async () =>
    {
        // Refresh data
        character = await characterPortal.FetchAsync(character.Id);
        StateHasChanged();
    });
}
```

### Pattern 4: Character Switcher Dropdown in Modal
**What:** Dropdown in modal header allowing navigation between characters
**When to use:** GM needs to compare/switch between characters without closing modal
**Example:**
```csharp
<div class="modal-header">
    <select class="form-select" style="max-width: 250px;" @bind="selectedCharacterId"
            @bind:after="OnCharacterChanged">
        @foreach (var char in allCharacters)
        {
            <option value="@char.CharacterId">@char.CharacterName</option>
        }
    </select>
    <button type="button" class="btn-close" @onclick="Close"></button>
</div>
```

### Anti-Patterns to Avoid
- **Direct DOM manipulation:** Use Blazor data binding, not JS for modal show/hide
- **Polling for updates:** Use Rx.NET subscriptions, not periodic refresh timers
- **Passing CharacterEdit to modal:** Fetch fresh data in modal to ensure current state
- **Modal without IDisposable:** Always unsubscribe from events in DisposeAsync

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Modal overlay | Custom CSS backdrop | Radzen DialogService | Handles keyboard, focus trap, accessibility |
| Tab styling | Custom tab CSS | Existing .tab-buttons/.tab-content | Already themed for fantasy/scifi |
| Real-time messaging | Custom SignalR hub | InMemoryMessageBus + ITimeEventSubscriber | Existing infrastructure, proven patterns |
| Character data loading | Inline SQL/HTTP | IDataPortal<CharacterEdit> | CSLA pattern, handles security |
| Event subscription cleanup | Manual dispose | IDisposable/IAsyncDisposable pattern | Prevents memory leaks |

**Key insight:** The codebase has comprehensive infrastructure for all required features. Implementation is primarily composition of existing patterns, not creation of new infrastructure.

## Common Pitfalls

### Pitfall 1: Stale Data in Modal During Updates
**What goes wrong:** Modal shows outdated character state after GM applies damage elsewhere
**Why it happens:** Modal fetched character once on open, doesn't subscribe to updates
**How to avoid:**
- Subscribe to CharacterUpdateMessage in modal's OnInitializedAsync
- Filter by CharacterId to refresh only when relevant
- Call StateHasChanged() after refreshing data
**Warning signs:** Character stats in modal don't match dashboard cards

### Pitfall 2: Memory Leak from Event Subscriptions
**What goes wrong:** Event handlers accumulate, causing multiple refreshes per update
**Why it happens:** Component subscribes in OnInitializedAsync but doesn't unsubscribe
**How to avoid:**
- Implement IAsyncDisposable on modal component
- Store subscription flags (e.g., `timeEventSubscribed`)
- Unsubscribe in DisposeAsync before modal closes
**Warning signs:** Console shows multiple "Received CharacterUpdateMessage" logs per event

### Pitfall 3: Race Condition with Character Switcher
**What goes wrong:** Switching characters rapidly causes display flicker or wrong data
**Why it happens:** Multiple async fetches racing, last one doesn't necessarily win
**How to avoid:**
- Use a loading flag to disable switcher during fetch
- Cancel pending operations when new selection made
- Consider debounce for rapid switching
**Warning signs:** Character name shows "Alice" but attributes show Bob's values

### Pitfall 4: Modal Not Closing on Escape Key
**What goes wrong:** User expects Escape to close modal, nothing happens
**Why it happens:** Custom modal doesn't implement keyboard handling
**How to avoid:**
- Use DialogService which handles Escape automatically
- Or add @onkeydown handler with e.Key == "Escape" check
**Warning signs:** User feedback about modal being "stuck"

### Pitfall 5: GM Notes Not Persisting
**What goes wrong:** GM writes notes, switches characters, notes are lost
**Why it happens:** Notes stored in component state, not persisted to database
**How to avoid:**
- Add GmNotes property to TableCharacter entity (table-character relationship)
- Save on blur/change, not just on modal close
- Consider auto-save timer for longer edits
**Warning signs:** Notes disappear after any character switch or page navigation

## Code Examples

Verified patterns from the codebase:

### Opening a Modal with DialogService
```csharp
// Source: Threa.Client/Components/Pages/Admin/Users.razor:82-90
var result = await DialogService.OpenAsync<UserEditModal>(
    "Edit User",
    new Dictionary<string, object> { { "UserId", userId } },
    new DialogOptions
    {
        Width = "500px",
        CloseDialogOnOverlayClick = false,
        ShowClose = true
    });

// Handle result
if (result is bool saved && saved)
{
    await RefreshListAsync();
}
```

### Subscribing to Time Events
```csharp
// Source: Threa.Client/Components/Pages/GamePlay/GmTable.razor:592-603
private async Task SubscribeToTimeEventsAsync()
{
    if (timeEventSubscribed) return;

    await TimeEventSubscriber.ConnectAsync();
    await TimeEventSubscriber.SubscribeAsync();

    TimeEventSubscriber.TimeEventReceived += OnTimeEventReceived;
    TimeEventSubscriber.CharacterUpdateReceived += OnCharacterUpdateReceived;
    TimeEventSubscriber.JoinRequestReceived += OnJoinRequestReceived;
    timeEventSubscribed = true;
}
```

### Handling Character Updates
```csharp
// Source: Threa.Client/Components/Pages/GamePlay/GmTable.razor:621-629
private void OnCharacterUpdateReceived(object? sender, CharacterUpdateMessage e)
{
    // Refresh character list when any character at this table is updated
    InvokeAsync(async () =>
    {
        await RefreshCharacterListAsync();
        StateHasChanged();
    });
}
```

### Tab Navigation Pattern
```csharp
// Source: Threa.Client/Components/Pages/GamePlay/Play.razor:361-397
private static readonly string[] tabNames = new[] { "Status", "Combat", "Skills", "Magic", "Defense", "Inventory" };
private string activeTab = tabNames[0];

<div class="tab-buttons">
    @foreach (var tabName in tabNames)
    {
        <button class="tab-link @(activeTab == tabName ? "active" : "")"
                @onclick="() => SelectTab(tabName)">
            @tabName
        </button>
    }
</div>

<div class="tab-content">
    @if (activeTab == "Status")
    {
        <TabStatus Character="character" Table="table" />
    }
    // ... other tab content
</div>
```

### Existing Character Data Properties (Narrative)
```csharp
// Source: GameMechanics/CharacterEdit.cs:105-162
// Available narrative properties:
- SkinDescription (string) - Physical appearance
- HairDescription (string) - Physical appearance
- Height (string) - Physical stats
- Weight (string) - Physical stats
- Description (string) - Main description/backstory
- Notes (string) - Player notes
- ImageUrl (string) - Character portrait
```

### NPC Placeholder Collapsible Section
```csharp
// Pattern: Bootstrap collapse with custom state
<div class="card mt-3">
    <div class="card-header d-flex justify-content-between align-items-center"
         style="cursor: pointer;"
         @onclick="() => npcSectionExpanded = !npcSectionExpanded">
        <strong><i class="bi bi-people me-1"></i>NPCs</strong>
        <i class="bi @(npcSectionExpanded ? "bi-chevron-up" : "bi-chevron-down")"></i>
    </div>
    @if (npcSectionExpanded)
    {
        <div class="card-body text-center text-muted">
            <img src="/images/npc-placeholder.png" alt="NPC feature coming soon"
                 style="max-width: 200px; opacity: 0.5;" />
            <p class="mt-2"><em>NPC management coming in a future update</em></p>
        </div>
    }
</div>
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Manual modal show/hide | DialogService.OpenAsync | Project standard | Consistent UX, accessibility |
| Polling for updates | Rx.NET subscriptions | Phase 13+ | Real-time without server load |
| Inline character fetch | IDataPortal pattern | Project standard | Security, caching, consistency |

**Deprecated/outdated:**
- Custom modal CSS/JS: Use Radzen DialogService
- Timer-based refresh: Use message subscriptions

## Open Questions

Things that could not be fully resolved:

1. **GM Notes Storage Location**
   - What we know: CharacterEdit has player Notes, but no GM-specific notes
   - What's unclear: Should GM notes be on TableCharacter (per-table) or Character (global)?
   - Recommendation: Add GmNotes column to TableCharacter table - notes are table-context specific

2. **NPC Placeholder Visual Design**
   - What we know: Collapsible section with mockup/wireframe image
   - What's unclear: Exact visual design of placeholder image
   - Recommendation: Use simple SVG or text description; defer actual graphic design

3. **Character Switcher Behavior on Update**
   - What we know: Modal should update in place when character data changes
   - What's unclear: If viewing Character A and Character B updates, should dropdown indicator change?
   - Recommendation: Keep simple - only refresh data for currently viewed character

## Sources

### Primary (HIGH confidence)
- GmTable.razor (lines 1-1373) - Existing GM dashboard patterns
- Play.razor (lines 1-869) - Tab and subscription patterns
- UserEditModal.razor - Modal component pattern
- TimeMessages.cs - Message types for real-time updates
- InMemoryTimeEventSubscriber.cs - Subscription interface and implementation

### Secondary (MEDIUM confidence)
- themes.css (lines 332-385, 748-768) - Tab and modal styling
- TabPlayInventory.razor - Inventory display patterns
- CharacterEdit.cs - Character business object properties

### Tertiary (LOW confidence)
None - all findings verified against codebase

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Using only existing project dependencies
- Architecture: HIGH - Following established codebase patterns
- Pitfalls: HIGH - Based on existing code and Blazor Server fundamentals

**Research date:** 2026-01-27
**Valid until:** 60 days (stable patterns, no external dependencies changing)
