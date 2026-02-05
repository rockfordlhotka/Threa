# Architecture Research: Batch Character Actions

**Domain:** TTRPG Assistant - Batch Character Operations
**Researched:** 2026-02-04
**Confidence:** HIGH (based on direct codebase analysis of existing patterns)

## System Overview

```
+-------------------------------------------------------------------------+
|                           PRESENTATION LAYER                             |
+-------------------------------------------------------------------------+
|  GmTable.razor (Dashboard)                                               |
|  +------------------+  +------------------+  +------------------+        |
|  | CharacterStatus  |  | NpcStatusCard    |  | BatchActionPanel | [NEW]  |
|  | Card             |  |                  |  | (selection UI)   |        |
|  +--------+---------+  +--------+---------+  +--------+---------+        |
|           |                     |                     |                  |
|           +-------- OnClick ----+                     |                  |
|                        |                              |                  |
|           +------------v------------+                 |                  |
|           | CharacterDetailModal    |<----------------+ [multi-select]   |
|           | (single character)      |                                    |
|           +------------+------------+                                    |
|                        |                                                 |
|           +------------v------------+                                    |
|           | CharacterDetailGmActions|      +------------------+          |
|           | (damage/heal/effects)   |      | BatchActionModal | [NEW]    |
|           +-------------------------+      | (damage/heal/etc)|          |
|                                            +------------------+          |
+-------------------------------------------------------------------------+
|                       SERVICE / ORCHESTRATION LAYER                      |
+-------------------------------------------------------------------------+
|  +----------------------------+    +---------------------------+         |
|  | TimeAdvancementService     |    | BatchActionService [NEW]  |         |
|  | (existing batch pattern)   |    | (orchestrates multi-char) |         |
|  +-------------+--------------+    +-------------+-------------+         |
|                |                                 |                        |
|                +-----------+---------------------+                        |
|                            |                                              |
|                  +---------v---------+                                    |
|                  | IDataPortal<T>     |                                    |
|                  | (CSLA data portal) |                                    |
|                  +---------+---------+                                    |
+-------------------------------------------------------------------------+
|                      BUSINESS OBJECT LAYER (CSLA)                        |
+-------------------------------------------------------------------------+
|  +------------------+    +------------------+    +------------------+     |
|  | CharacterEdit    |    | EffectList       |    | Fatigue/Vitality |     |
|  | (BusinessBase<T>)|    | (child list)     |    | (child objects)  |     |
|  +------------------+    +------------------+    +------------------+     |
+-------------------------------------------------------------------------+
|                           MESSAGING LAYER                                |
+-------------------------------------------------------------------------+
|  +------------------+    +------------------+    +------------------+     |
|  | InMemoryMessageBus|   | CharacterUpdate  |    | CharactersUpdated|     |
|  | (Rx.NET Subject) |    | Message          |    | Message [batch]  |     |
|  +------------------+    +------------------+    +------------------+     |
+-------------------------------------------------------------------------+
|                          DATA ACCESS LAYER                               |
+-------------------------------------------------------------------------+
|  +------------------+    +------------------+    +------------------+     |
|  | ICharacterDal    |    | ITableDal        |    | SQLite/MockDb    |     |
|  | (interface)      |    | (interface)      |    | (implementation) |     |
|  +------------------+    +------------------+    +------------------+     |
+-------------------------------------------------------------------------+
```

## Component Responsibilities

| Component | Responsibility | Existing/New |
|-----------|----------------|--------------|
| GmTable.razor | Dashboard, character list display, selection state | Existing - MODIFY |
| CharacterStatusCard | Individual character display, click handling | Existing - MODIFY |
| NpcStatusCard | NPC wrapper with disposition | Existing - MODIFY |
| BatchSelectionState | Track selected character IDs | NEW - Component State |
| BatchActionPanel | Selection toolbar (count, actions) | NEW - Component |
| BatchActionModal | Modal for batch operation input | NEW - Component |
| BatchActionService | Execute batch operations | NEW - Service |
| CharactersUpdatedMessage | Notify clients of batch update | Existing - REUSE |

## Integration Points with Existing Architecture

### 1. Selection State Location

**Decision:** Selection state lives in GmTable.razor as component state.

**Rationale:**
- GmTable already manages `selectedCharacter` for detail modal
- Selection is transient UI state, not persisted
- Component-level state avoids unnecessary service complexity
- Pattern matches existing `selectedCharacter` single-select

**Implementation Pattern:**
```csharp
// GmTable.razor @code block
private HashSet<int> selectedCharacterIds = new();
private bool isMultiSelectMode = false;

// Toggle selection on card click (when multi-select active)
private void ToggleSelection(TableCharacterInfo character)
{
    if (selectedCharacterIds.Contains(character.CharacterId))
        selectedCharacterIds.Remove(character.CharacterId);
    else
        selectedCharacterIds.Add(character.CharacterId);
}

// Clear on mode exit or batch action completion
private void ClearSelection()
{
    selectedCharacterIds.Clear();
    isMultiSelectMode = false;
}
```

### 2. Integration with CharacterStatusCard

**Current pattern (GmTable.razor lines 176-183):**
```csharp
<CharacterStatusCard Character="@character"
                    IsSelected="@(selectedCharacter?.CharacterId == character.CharacterId)"
                    OnClick="OpenCharacterDetails" />
```

**Batch pattern modification:**
```csharp
<CharacterStatusCard Character="@character"
                    IsSelected="@(selectedCharacterIds.Contains(character.CharacterId))"
                    ShowCheckbox="@isMultiSelectMode"
                    OnClick="HandleCardClick"
                    OnCheckboxToggle="ToggleSelection" />

private void HandleCardClick(TableCharacterInfo character)
{
    if (isMultiSelectMode)
        ToggleSelection(character);
    else
        OpenCharacterDetails(character);
}
```

**CharacterStatusCard.razor modification (add checkbox visual):**
```razor
@if (ShowCheckbox)
{
    <div class="position-absolute" style="top: 4px; left: 4px; z-index: 1;">
        <input type="checkbox" class="form-check-input"
               checked="@IsSelected"
               @onclick="OnCheckboxClicked"
               @onclick:stopPropagation />
    </div>
}

@code {
    [Parameter] public bool ShowCheckbox { get; set; }
    [Parameter] public EventCallback<TableCharacterInfo> OnCheckboxToggle { get; set; }

    private async Task OnCheckboxClicked()
    {
        await OnCheckboxToggle.InvokeAsync(Character);
    }
}
```

### 3. Integration with CharacterDetailModal

**No changes needed.** Single-character operations continue through existing modal.

Batch operations use a separate, simpler modal (BatchActionModal) that:
- Takes array of character IDs
- Shows simplified action UI (no tabs, just the action)
- Returns aggregate results

### 4. Integration with CharacterDetailGmActions

**Existing actions to batch-enable (from CharacterDetailGmActions.razor):**

| Action | Current Implementation | Batch Approach |
|--------|------------------------|----------------|
| Apply Damage | `ApplyToPool("FAT/VIT")` lines 448-499 | Extract to service, loop over selected |
| Apply Healing | `ApplyToPool("FAT/VIT")` lines 448-499 | Same service, healing mode |
| Add Effect | `OpenEffectManagement()` lines 596-616 | Simplified batch effect picker |
| Add Wound | `OpenWoundManagement()` lines 626-644 | Too complex for batch (location varies) |

**Shared logic extraction pattern:**
```csharp
// Extract from CharacterDetailGmActions into BatchActionService
public async Task<BatchActionResult> ApplyPoolChangeAsync(
    IEnumerable<int> characterIds,
    int amount,
    string pool, // "FAT" or "VIT"
    bool isDamage, // true for damage, false for healing
    Guid tableId)
{
    var result = new BatchActionResult();
    foreach (var charId in characterIds)
    {
        try
        {
            var character = await _characterPortal.FetchAsync(charId);
            if (isDamage)
            {
                if (pool == "FAT")
                    character.Fatigue.PendingDamage += amount;
                else
                    character.Vitality.PendingDamage += amount;
            }
            else
            {
                if (pool == "FAT")
                    character.Fatigue.PendingHealing += amount;
                else
                    character.Vitality.PendingHealing += amount;
            }
            await _characterPortal.UpdateAsync(character);
            result.SuccessIds.Add(charId);
        }
        catch (Exception ex)
        {
            result.FailedIds.Add(charId);
            result.Errors.Add($"Character {charId}: {ex.Message}");
        }
    }
    return result;
}
```

## Batch Operation Execution Pattern

### Parallel vs Sequential

**Recommendation:** Sequential execution with error aggregation.

**Rationale:**
1. **CSLA business objects are not thread-safe** - CharacterEdit cannot be used from multiple threads
2. **SQLite has limited concurrent write performance** - parallel gains minimal benefit
3. **Error isolation** - one failure doesn't abort entire batch
4. **Matches TimeAdvancementService pattern** - proven approach already in codebase

**TimeAdvancementService reference pattern (lines 137-176):**
```csharp
// From S:\src\rdl\threa\GameMechanics\Time\TimeAdvancementService.cs
foreach (var kvp in loadedCharacters)
{
    try
    {
        var character = kvp.Value;
        // ... process character ...
        await _characterPortal.UpdateAsync(character);
        result.UpdatedCharacterIds.Add(kvp.Key);
    }
    catch (Exception ex)
    {
        result.FailedCharacterIds.Add(kvp.Key);
        result.Errors.Add($"Character {kvp.Key}: {ex.Message}");
    }
}
```

### BatchActionService Pattern

```csharp
// New service: GameMechanics/Batch/BatchActionService.cs
public class BatchActionService
{
    private readonly IDataPortal<CharacterEdit> _characterPortal;
    private readonly ITimeEventPublisher _timeEventPublisher;

    public BatchActionService(
        IDataPortal<CharacterEdit> characterPortal,
        ITimeEventPublisher timeEventPublisher)
    {
        _characterPortal = characterPortal;
        _timeEventPublisher = timeEventPublisher;
    }

    public async Task<BatchActionResult> ExecuteAsync(BatchActionRequest request)
    {
        var result = new BatchActionResult { ActionType = request.ActionType };

        foreach (var characterId in request.CharacterIds)
        {
            try
            {
                var character = await _characterPortal.FetchAsync(characterId);
                ApplyAction(character, request);
                await _characterPortal.UpdateAsync(character);
                result.SuccessIds.Add(characterId);
            }
            catch (Exception ex)
            {
                result.FailedIds.Add(characterId);
                result.Errors.Add($"Character {characterId}: {ex.Message}");
            }
        }

        // Publish batch update notification
        if (result.SuccessIds.Count > 0)
        {
            await _timeEventPublisher.PublishCharactersUpdatedAsync(
                new CharactersUpdatedMessage
                {
                    TableId = request.TableId,
                    CharacterIds = result.SuccessIds,
                    EventType = TimeEventType.EndOfRound // or dedicated batch type
                });
        }

        return result;
    }

    private void ApplyAction(CharacterEdit character, BatchActionRequest request)
    {
        switch (request.ActionType)
        {
            case BatchActionType.Damage:
                if (request.Pool == "FAT")
                    character.Fatigue.PendingDamage += request.Amount;
                else
                    character.Vitality.PendingDamage += request.Amount;
                break;
            case BatchActionType.Healing:
                if (request.Pool == "FAT")
                    character.Fatigue.PendingHealing += request.Amount;
                else
                    character.Vitality.PendingHealing += request.Amount;
                break;
            case BatchActionType.ToggleVisibility:
                character.VisibleToPlayers = !character.VisibleToPlayers;
                break;
            // Additional action types...
        }
    }
}
```

## Messaging Pattern for Batch Updates

### Existing Infrastructure

**CharactersUpdatedMessage (already exists - TimeMessages.cs lines 387-403):**
```csharp
public class CharactersUpdatedMessage : TimeMessageBase
{
    public Guid TableId { get; init; }
    public List<int> CharacterIds { get; init; } = new();
    public Time.TimeEventType EventType { get; init; }
}
```

**Decision:** Reuse CharactersUpdatedMessage for batch operations.

**Rationale:**
- Message already carries list of character IDs
- Player clients already subscribe and refresh on receipt
- No new message type needed
- Consistent with existing time advancement pattern

**Alternative considered:** Create BatchActionResultMessage with action details
- Rejected: Adds complexity without clear benefit
- Player clients just need to know "refresh these characters"

### Client Refresh Pattern

**Existing pattern in GmTable.razor (lines 643-676):**
```csharp
private void OnCharacterUpdateReceived(object? sender, CharacterUpdateMessage e)
{
    // Refresh character list when a character is updated
    _ = InvokeAsync(async () =>
    {
        await RefreshCharacterListAsync();
        StateHasChanged();
    });
}
```

**Enhancement for batch:** The same handler already handles updates. CharactersUpdatedMessage triggers same refresh.

## Recommended Project Structure

```
Threa.Client/Components/
+-- Pages/GamePlay/
|   +-- GmTable.razor                    # MODIFY: add selection state, batch UI
|
+-- Shared/
    +-- CharacterStatusCard.razor        # MODIFY: add checkbox/multi-select visual
    +-- CharacterStatusCard.razor.cs     # MODIFY: add ShowCheckbox parameter
    +-- NpcStatusCard.razor              # No change (wraps CharacterStatusCard)
    +-- BatchActionPanel.razor           # NEW: selection toolbar component
    +-- BatchActionModal.razor           # NEW: batch action input modal

GameMechanics/
+-- Batch/
    +-- BatchActionService.cs            # NEW: orchestration service
    +-- BatchActionRequest.cs            # NEW: request DTO
    +-- BatchActionResult.cs             # NEW: result with partial success
    +-- BatchActionType.cs               # NEW: enum of supported actions
```

## Architectural Patterns

### Pattern 1: Component-Level Selection State

**What:** Selection state managed as component field in GmTable.razor
**When to use:** Transient UI state not needed by other components
**Trade-offs:**
- Pro: Simple, no DI overhead, matches existing selectedCharacter pattern
- Con: Can't share selection across routes (acceptable for this use case)

**Example:**
```csharp
@code {
    private HashSet<int> selectedCharacterIds = new();

    private void ToggleSelection(TableCharacterInfo character)
    {
        if (!selectedCharacterIds.Add(character.CharacterId))
            selectedCharacterIds.Remove(character.CharacterId);
        StateHasChanged();
    }
}
```

### Pattern 2: Service with Error Aggregation

**What:** Service method that processes items sequentially, collecting errors
**When to use:** Batch operations where partial success is acceptable
**Trade-offs:**
- Pro: Clear success/failure per item, proven pattern in TimeAdvancementService
- Con: Sequential is slower than parallel (acceptable for typical batch sizes)

**Reference:** TimeAdvancementService.cs lines 137-189

### Pattern 3: Reuse Existing Messages

**What:** Use CharactersUpdatedMessage for batch notifications
**When to use:** Clients only need to know "refresh these characters"
**Trade-offs:**
- Pro: No new message infrastructure, clients already handle it
- Con: Less semantic clarity about what changed (acceptable)

**Reference:** TimeMessages.cs lines 387-403

## Data Flow

### Selection Flow

```
[User clicks "Multi-Select" button]
    |
    v
[GmTable sets isMultiSelectMode = true]
    |
    v
[User clicks CharacterStatusCard]
    |
    +-- isMultiSelectMode? --+
    |                        |
   NO                       YES
    |                        |
    v                        v
[OpenCharacterDetails]   [ToggleSelection]
                             |
                             v
                    [selectedCharacterIds.Add/Remove]
                             |
                             v
                    [StateHasChanged - cards re-render with checkbox]
```

### Batch Action Flow

```
[User clicks "Apply Action" in BatchActionPanel]
    |
    v
[BatchActionModal opens with selected count]
    |
    v
[User configures action (amount, pool type)]
    |
    v
[User clicks "Apply to N Characters"]
    |
    v
[BatchActionService.ExecuteAsync called]
    |
    +-- For each characterId:
    |   |
    |   v
    |   [Fetch CharacterEdit via CSLA portal]
    |   |
    |   v
    |   [Apply action (damage/heal/effect)]
    |   |
    |   v
    |   [Update CharacterEdit via CSLA portal]
    |   |
    |   +-- Success? --+
    |       |          |
    |      YES        NO
    |       |          |
    |   [Add to      [Add to
    |    SuccessIds]  FailedIds + error msg]
    |
    v
[Publish CharactersUpdatedMessage with SuccessIds]
    |
    v
[Show result toast (N succeeded, M failed)]
    |
    v
[Clear selection, exit multi-select mode]
    |
    v
[RefreshCharacterListAsync triggered by message]
```

## Scaling Considerations

| Scale | Architecture Adjustments |
|-------|--------------------------|
| 1-10 characters | Current approach is fine - no user-perceivable delay |
| 10-50 characters | Add progress indicator during batch execution |
| 50+ characters | Consider chunked execution with intermediate progress updates |

### Performance Notes

- **Typical batch size:** 5-15 characters (party + NPCs in single encounter)
- **Sequential save time:** ~50-100ms per character with SQLite
- **User-perceivable delay threshold:** ~500ms for 5 characters
- **Mitigation strategies:**
  - Show spinner/progress bar for batches > 10 characters
  - Consider optimistic UI updates (show result, refresh in background)
  - Single `RefreshCharacterListAsync()` call after all updates complete

## Anti-Patterns to Avoid

### Anti-Pattern 1: Parallel CSLA Operations

**What people do:** Use `Task.WhenAll` to update multiple CharacterEdit objects simultaneously
**Why it's wrong:** CSLA business objects are not thread-safe; data portal is not designed for parallel use
**Do this instead:** Sequential loop with error aggregation (TimeAdvancementService pattern)

### Anti-Pattern 2: Global Selection Service

**What people do:** Create injectable service to hold selection state
**Why it's wrong:** Overengineering for simple transient UI state; adds unnecessary DI complexity
**Do this instead:** Component-level `HashSet<int>` in GmTable.razor

### Anti-Pattern 3: Batch-Specific Message Types

**What people do:** Create new message type per batch action (BatchDamageMessage, BatchHealMessage)
**Why it's wrong:** Adds infrastructure without benefit - clients just need "refresh these characters"
**Do this instead:** Reuse CharactersUpdatedMessage with list of affected character IDs

### Anti-Pattern 4: Individual CharacterUpdateMessage per Character

**What people do:** Publish CharacterUpdateMessage for each character in batch
**Why it's wrong:** N messages = N refresh cycles = poor performance
**Do this instead:** Single CharactersUpdatedMessage with all IDs, single refresh

## Suggested Build Order

Based on dependencies and incremental delivery:

### Phase 1: Selection Infrastructure (Foundation)
1. Add `selectedCharacterIds` HashSet to GmTable.razor
2. Add `isMultiSelectMode` flag and toggle button
3. Modify CharacterStatusCard to show checkbox when multi-select active
4. Update card click handler to respect multi-select mode
5. Add selection count badge in UI

**Deliverable:** GM can select/deselect multiple characters, see selection count
**Test:** Toggle multi-select mode, click cards, verify selection state

### Phase 2: BatchActionService (Backend)
1. Create `BatchActionRequest` / `BatchActionResult` DTOs in `GameMechanics/Batch/`
2. Create `BatchActionService` with DI registration
3. Implement `ApplyPoolChange` batch method (damage and healing)
4. Add `CharactersUpdatedMessage` publishing after batch
5. Unit test batch logic with DeterministicDiceRoller pattern

**Deliverable:** Service can execute batch damage/healing with error aggregation
**Test:** Unit tests for batch service, verify partial success handling

### Phase 3: Batch UI (User-Facing)
1. Create `BatchActionPanel` component (appears when selection exists)
2. Create `BatchActionModal` component for action configuration
3. Wire modal open from action panel
4. Implement damage/healing mode toggle (reuse CharacterDetailGmActions pattern)
5. Show result toast (N succeeded, M failed)
6. Auto-clear selection on success

**Deliverable:** GM can apply batch damage/healing via modal
**Test:** E2E flow from selection to batch apply to result display

### Phase 4: Additional Actions (Extension)
1. Add batch visibility toggle for NPCs
2. Add batch dismiss/archive for NPCs
3. Consider batch effect application (simpler subset of effects)

**Deliverable:** Full batch action support for table stakes features
**Test:** All batch actions working, edge cases handled

## Integration with Existing Components Summary

| Existing Component | Integration Point | Change Type |
|--------------------|-------------------|-------------|
| GmTable.razor | Add selection state, toggle mode, action panel | MODIFY |
| CharacterStatusCard.razor | Add checkbox, multi-select styling | MODIFY |
| CharacterStatusCard.razor.cs | Add ShowCheckbox, OnCheckboxToggle params | MODIFY |
| NpcStatusCard.razor | No change (wraps CharacterStatusCard) | NONE |
| CharacterDetailModal.razor | No change (single-char operations) | NONE |
| CharacterDetailGmActions.razor | Extract logic for batch reuse | REFERENCE |
| TimeAdvancementService.cs | Pattern reference only | REFERENCE |
| InMemoryMessageBus.cs | Reuse CharactersUpdatedMessage | REUSE |
| TimeMessages.cs | Reuse message types | REUSE |

## Sources

- **Direct codebase analysis:**
  - `S:\src\rdl\threa\Threa\Threa.Client\Components\Pages\GamePlay\GmTable.razor` - Dashboard structure
  - `S:\src\rdl\threa\Threa\Threa.Client\Components\Shared\CharacterDetailGmActions.razor` - Single character action patterns
  - `S:\src\rdl\threa\GameMechanics\Time\TimeAdvancementService.cs` - Batch processing pattern
  - `S:\src\rdl\threa\GameMechanics\Messaging\TimeMessages.cs` - Message infrastructure
  - `S:\src\rdl\threa\GameMechanics.Messaging.InMemory\InMemoryMessageBus.cs` - Pub/sub infrastructure
  - `S:\src\rdl\threa\GameMechanics\CharacterEdit.cs` - Business object structure
- **CSLA .NET documentation** - Thread safety constraints for business objects

---
*Architecture research for: Batch Character Actions (v1.6)*
*Researched: 2026-02-04*
