# Phase 29: Batch Action Framework - Research

**Researched:** 2026-02-05
**Domain:** Batch damage/healing operations with contextual action bar UI
**Confidence:** HIGH

## Summary

This phase builds on Phase 28's selection infrastructure to implement batch damage and healing operations. The research focused on four key areas: (1) extending the SelectionBar component into a contextual action bar with batch operation buttons, (2) implementing a BatchActionService following the proven TimeAdvancementService sequential processing pattern, (3) designing structured result handling for partial success scenarios, and (4) creating feedback UI for batch operation outcomes.

The project already has all necessary infrastructure: Phase 28 delivered `HashSet<int> selectedCharacterIds` selection state, the `SelectionBar` component, and selection methods in GmTable.razor. The existing `CharacterDetailGmActions` component provides the exact damage/healing logic to extract and adapt for batch operations. The `TimeAdvancementService` provides a proven pattern for sequential character processing with error aggregation. No new dependencies are required.

**Primary recommendation:** Create a `BatchActionService` class following the `TimeAdvancementService` pattern (sequential loop, error aggregation, single `CharactersUpdatedMessage` after completion). Extend `SelectionBar` into a `BatchActionBar` component with damage/healing action buttons that open a simple modal for amount input. Display batch results via inline feedback in the action bar with expandable details for partial failures.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CSLA.NET | 9.1.0 | Business object operations (fetch, modify, save) | Already in use; CharacterEdit business object handles damage/healing |
| Radzen.Blazor | 8.4.2 | UI components (dialogs, buttons, inputs) | Already in use; RadzenDialog for modal, RadzenNumeric for input |
| System.Reactive | 6.0.1 | Message publishing via InMemoryMessageBus | Already in use; CharactersUpdatedMessage broadcasting |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Bootstrap Icons | 1.11+ | Action button icons | Already in use; bi-heart-pulse (damage), bi-bandaid (healing) |
| Bootstrap CSS | 5.x | Alert components for feedback | Already in use; alert-success, alert-warning for results |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Custom BatchActionService | CSLA CommandBase | CommandBase is for stateless operations; batch needs result tracking across multiple saves |
| Sequential processing | Parallel (Task.WhenAll) | CSLA business objects are NOT thread-safe; sequential is required |
| Inline feedback | Toast notifications | Toasts disappear; batch results need persistent display with details |
| Single CharactersUpdatedMessage | Individual CharacterUpdateMessage per character | Single message prevents N refresh cycles; proven in TimeAdvancementService |

**Installation:** No additional packages required - all dependencies already present.

## Architecture Patterns

### Recommended Project Structure
```
GameMechanics/
  Batch/
    BatchActionService.cs           # NEW: Orchestration service
    BatchActionRequest.cs           # NEW: Request with action type, pool, amount
    BatchActionResult.cs            # NEW: Result with successes, failures, messages

Threa.Client/Components/
  Shared/
    SelectionBar.razor              # MODIFY: Rename to BatchActionBar, add action buttons
    BatchDamageHealingModal.razor   # NEW: Simple modal for amount/pool input
    BatchResultDisplay.razor        # NEW: Inline result display component
```

### Pattern 1: Sequential Processing with Error Aggregation
**What:** Process characters one at a time in a loop, catch errors per character, aggregate results
**When to use:** Batch operations where partial success is acceptable
**Example:**
```csharp
// Source: Existing pattern from TimeAdvancementService.cs lines 137-176
public async Task<BatchActionResult> ApplyDamageAsync(BatchActionRequest request)
{
    var result = new BatchActionResult { ActionType = request.ActionType };

    foreach (var characterId in request.CharacterIds)
    {
        try
        {
            var character = await _characterPortal.FetchAsync(characterId);

            // Apply damage to pending pool (same as CharacterDetailGmActions)
            if (request.Pool == "FAT")
                character.Fatigue.PendingDamage += request.Amount;
            else
                character.Vitality.PendingDamage += request.Amount;

            await _characterPortal.UpdateAsync(character);
            result.SuccessIds.Add(characterId);
            result.SuccessNames.Add(character.Name);
        }
        catch (Exception ex)
        {
            result.FailedIds.Add(characterId);
            result.Errors.Add($"{characterId}: {ex.Message}");
        }
    }

    // Single notification after all updates complete
    if (result.SuccessIds.Count > 0)
    {
        await _timeEventPublisher.PublishCharactersUpdatedAsync(
            new CharactersUpdatedMessage
            {
                TableId = request.TableId,
                CharacterIds = result.SuccessIds,
                EventType = TimeEventType.EndOfRound, // Reuse existing type
                SourceId = "BatchActionService"
            });
    }

    return result;
}
```

### Pattern 2: Contextual Action Bar (Extending SelectionBar)
**What:** Expand existing SelectionBar to include action buttons when characters are selected
**When to use:** Batch action UI that appears contextually based on selection state
**Example:**
```razor
<!-- Source: Extend existing SelectionBar.razor pattern -->
<div class="selection-bar @(SelectedCount > 0 ? "visible" : "")">
    <div class="d-flex justify-content-between align-items-center">
        <span class="selection-count">
            <i class="bi bi-check2-square me-2"></i>
            <strong>@SelectedCount</strong> selected
        </span>

        <!-- Batch Action Buttons - NEW -->
        <div class="d-flex gap-2 align-items-center">
            <button class="btn btn-sm btn-danger" @onclick="OpenDamageModal">
                <i class="bi bi-heart-pulse me-1"></i>Damage
            </button>
            <button class="btn btn-sm btn-success" @onclick="OpenHealingModal">
                <i class="bi bi-bandaid me-1"></i>Heal
            </button>
            <button class="btn btn-sm btn-outline-secondary" @onclick="HandleDeselectAll">
                <i class="bi bi-x-lg me-1"></i>Clear
            </button>
        </div>
    </div>

    <!-- Inline Result Display - NEW -->
    @if (LastResult != null)
    {
        <BatchResultDisplay Result="@LastResult" OnDismiss="ClearResult" />
    }
</div>
```

### Pattern 3: Structured Batch Result
**What:** Result object with clear separation of successes, failures, and human-readable messages
**When to use:** Batch operations that need partial success reporting
**Example:**
```csharp
// Source: Inspired by TimeAdvancementResult pattern
public class BatchActionResult
{
    public BatchActionType ActionType { get; set; }
    public string Pool { get; set; } = "";  // "FAT" or "VIT"
    public int Amount { get; set; }

    public List<int> SuccessIds { get; } = new();
    public List<string> SuccessNames { get; } = new();
    public List<int> FailedIds { get; } = new();
    public List<string> Errors { get; } = new();

    public int TotalCount => SuccessIds.Count + FailedIds.Count;
    public bool HasFailures => FailedIds.Count > 0;
    public bool AllSucceeded => FailedIds.Count == 0;

    // Human-readable summary
    public string Summary => HasFailures
        ? $"Applied {Amount} {Pool} {ActionType.ToString().ToLower()} to {SuccessIds.Count} of {TotalCount} characters"
        : $"Applied {Amount} {Pool} {ActionType.ToString().ToLower()} to {SuccessIds.Count} characters";
}

public enum BatchActionType { Damage, Healing }
```

### Anti-Patterns to Avoid
- **Parallel CSLA operations:** Using `Task.WhenAll` with CharacterEdit objects causes threading issues; CSLA business objects are not thread-safe
- **Individual CharacterUpdateMessage per character:** Publishing N messages causes N UI refresh cycles; use single CharactersUpdatedMessage
- **Complex modal forms:** Batch damage/healing only needs amount and pool (FAT/VIT); don't replicate full CharacterDetailGmActions UI
- **Clearing selection on partial failure:** If 4 of 5 succeed and 1 fails, keep selection so GM can retry or investigate; clear only on full success

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Character fetch and update | Custom DAL calls | IDataPortal<CharacterEdit> | CSLA handles dirty tracking, validation, save |
| Message broadcasting | Manual observer pattern | ITimeEventPublisher.PublishCharactersUpdatedAsync | Existing Rx.NET infrastructure |
| Pending damage/healing | Direct Value modification | Fatigue.PendingDamage, Vitality.PendingHealing | Existing properties handle pool cascading |
| Modal dialogs | Custom modal implementation | DialogService.OpenAsync<T> | Radzen handles overlay, focus trapping, animation |
| Input validation | Manual null checks | RadzenNumeric with Min/Max | Built-in validation with visual feedback |

**Key insight:** The damage/healing logic already exists in `CharacterDetailGmActions.ApplyToPool()`. Extract and loop over selected characters; don't reinvent the health modification logic.

## Common Pitfalls

### Pitfall 1: Thread Safety with CSLA Business Objects
**What goes wrong:** Using Task.WhenAll to parallelize updates causes data corruption or exceptions
**Why it happens:** CharacterEdit business objects are not thread-safe; DataPortal is designed for sequential use
**How to avoid:** Always use sequential foreach loop (TimeAdvancementService pattern)
**Warning signs:** Sporadic exceptions during batch operations, intermittent data inconsistencies

### Pitfall 2: Message Flooding
**What goes wrong:** Publishing CharacterUpdateMessage for each character in batch causes N refresh cycles
**Why it happens:** Each message triggers RefreshCharacterListAsync in GmTable
**How to avoid:** Publish single CharactersUpdatedMessage with all affected IDs after batch completes
**Warning signs:** UI flickering during batch, performance degradation with large batches

### Pitfall 3: Stale Selection State After Batch
**What goes wrong:** Selection still contains characters that were dismissed or failed
**Why it happens:** Selection HashSet not cleaned up after batch action
**How to avoid:** Clear selection on full success; on partial success, remove successful IDs from selection
**Warning signs:** Selection count mismatch, clicking "Apply Again" affects wrong characters

### Pitfall 4: Modal Callback Missing TableId
**What goes wrong:** BatchActionService can't publish CharactersUpdatedMessage without TableId
**Why it happens:** Modal component doesn't pass TableId through to service
**How to avoid:** Include TableId in BatchActionRequest; pass from GmTable through modal
**Warning signs:** CharactersUpdatedMessage has empty TableId, other clients don't refresh

### Pitfall 5: Amount Validation Edge Cases
**What goes wrong:** GM enters 0 or negative amount, batch "succeeds" but nothing happens
**Why it happens:** No validation on amount before processing
**How to avoid:** Validate amount > 0 before opening modal or executing batch; RadzenNumeric Min="1"
**Warning signs:** "Applied 0 FAT damage to 5 characters" success message

## Code Examples

Verified patterns from official sources and existing codebase:

### Extracting Damage/Healing Logic (from CharacterDetailGmActions)
```csharp
// Source: CharacterDetailGmActions.razor lines 515-568 (simplified for batch)
// This is the single-character pattern to adapt for batch

private async Task ExecuteApply()
{
    var character = await characterPortal.FetchAsync(CharacterId);

    if (healthMode == HealthMode.Damage)
    {
        if (pendingPool == "FAT")
            character.Fatigue.PendingDamage += healthAmount;
        else
            character.Vitality.PendingDamage += healthAmount;
    }
    else // Healing
    {
        if (pendingPool == "FAT")
            character.Fatigue.PendingHealing += healthAmount;
        else
            character.Vitality.PendingHealing += healthAmount;
    }

    await characterPortal.UpdateAsync(character);

    await TimeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
    {
        CharacterId = CharacterId,
        UpdateType = healthMode == HealthMode.Damage
            ? CharacterUpdateType.Damage
            : CharacterUpdateType.Healing,
        CampaignId = TableId.ToString()
    });
}
```

### BatchActionService Registration
```csharp
// Source: Pattern from ServiceCollectionExtensions.cs
// Register in Threa/Program.cs or GameMechanics/ServiceCollectionExtensions.cs

services.AddScoped<BatchActionService>();
```

### BatchDamageHealingModal Component
```razor
@* Source: Pattern from existing modals (EffectFormModal, WoundFormModal) *@
@inject DialogService DialogService

<div class="p-3">
    <div class="mb-3">
        <label class="form-label">Amount</label>
        <RadzenNumeric @bind-Value="amount" Min="1" Max="100" class="w-100" />
    </div>

    <div class="mb-3">
        <label class="form-label">Pool</label>
        <div class="btn-group w-100" role="group">
            <button class="btn @(pool == "FAT" ? "btn-primary" : "btn-outline-primary")"
                    @onclick='() => pool = "FAT"'>FAT</button>
            <button class="btn @(pool == "VIT" ? "btn-primary" : "btn-outline-primary")"
                    @onclick='() => pool = "VIT"'>VIT</button>
        </div>
    </div>

    <div class="d-flex justify-content-end gap-2">
        <button class="btn btn-secondary" @onclick="Cancel">Cancel</button>
        <button class="btn @(Mode == BatchActionType.Damage ? "btn-danger" : "btn-success")"
                @onclick="Confirm" disabled="@(amount < 1)">
            Apply to @SelectedCount Characters
        </button>
    </div>
</div>

@code {
    [Parameter] public BatchActionType Mode { get; set; }
    [Parameter] public int SelectedCount { get; set; }

    private int amount = 1;
    private string pool = "FAT";

    private void Cancel() => DialogService.Close(null);
    private void Confirm() => DialogService.Close(new BatchActionInput(amount, pool));
}
```

### Inline Result Feedback
```razor
@* Source: Pattern from CharacterDetailGmActions feedback display *@
@if (Result != null)
{
    <div class="alert @GetAlertClass() py-2 mb-0 mt-2 d-flex justify-content-between align-items-center">
        <div>
            <i class="bi @GetIcon() me-1"></i>
            @Result.Summary
        </div>
        <div class="d-flex gap-2">
            @if (Result.HasFailures)
            {
                <button class="btn btn-sm btn-link" @onclick="ToggleDetails">
                    @(showDetails ? "Hide" : "Show") Details
                </button>
            }
            <button class="btn btn-sm btn-link" @onclick="OnDismiss">
                <i class="bi bi-x"></i>
            </button>
        </div>
    </div>

    @if (showDetails && Result.HasFailures)
    {
        <div class="alert alert-secondary py-2 mt-1">
            <strong>Failed:</strong>
            <ul class="mb-0 small">
                @foreach (var error in Result.Errors)
                {
                    <li>@error</li>
                }
            </ul>
        </div>
    }
}

@code {
    [Parameter] public BatchActionResult? Result { get; set; }
    [Parameter] public EventCallback OnDismiss { get; set; }

    private bool showDetails;

    private string GetAlertClass() => Result?.AllSucceeded == true
        ? "alert-success"
        : "alert-warning";

    private string GetIcon() => Result?.AllSucceeded == true
        ? "bi-check-circle"
        : "bi-exclamation-triangle";

    private void ToggleDetails() => showDetails = !showDetails;
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Individual refresh per character | Single CharactersUpdatedMessage with ID list | Phase 27 (TimeAdvancementService) | Batch operations scale without UI thrashing |
| Dialog-based results | Inline feedback in action bar | Current patterns | Faster GM workflow, results visible without modal dismissal |
| Full CharacterDetailGmActions in modal | Simplified amount+pool input | This phase | Batch-specific UI, faster interaction |

**Deprecated/outdated:**
- Creating new message types per operation: Reuse CharactersUpdatedMessage with appropriate SourceId
- Full character reload after each change: PendingDamage/PendingHealing modify without reload

## Open Questions

Things that couldn't be fully resolved:

1. **Selection clearing behavior on partial success**
   - What we know: Clear on full success, keep selection on failure for retry
   - What's unclear: On partial success (3 succeed, 2 fail), should selection contain only the 2 failed?
   - Recommendation: Remove succeeded IDs from selection, leaving only failed for retry or investigation

2. **Result feedback persistence**
   - What we know: Need to show results after batch completes
   - What's unclear: How long to show results before auto-dismiss? Or keep until manual dismiss?
   - Recommendation: Keep until user clicks dismiss or starts new batch; auto-clear when selection changes

3. **Overflow handling in batch**
   - What we know: CharacterDetailGmActions warns about FAT overflow to VIT
   - What's unclear: Should batch silently allow overflow or warn before applying?
   - Recommendation: Apply without warning (batch is for speed); GM can check individual characters if needed

## Sources

### Primary (HIGH confidence)
- Existing codebase: `GmTable.razor`, `CharacterDetailGmActions.razor` - verified single-character damage/healing patterns
- Existing codebase: `TimeAdvancementService.cs` - verified batch processing with error aggregation pattern
- Existing codebase: `SelectionBar.razor` - Phase 28 delivered selection count display
- Existing codebase: `Fatigue.cs`, `Vitality.cs` - verified PendingDamage/PendingHealing property patterns
- CSLA MCP documentation - CommandBase vs service patterns for batch operations

### Secondary (MEDIUM confidence)
- Radzen.Blazor documentation (https://blazor.radzen.com/docs/api/) - DialogService, RadzenNumeric components
- v1.6 research documents: `ARCHITECTURE.md`, `FEATURES.md`, `STACK.md` - project-specific decisions

### Tertiary (LOW confidence)
- None - All patterns verified against existing codebase

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - No new dependencies, all patterns verified in codebase
- Architecture: HIGH - Direct adaptation of TimeAdvancementService pattern
- Pitfalls: HIGH - Based on CSLA threading constraints and existing messaging patterns

**Research date:** 2026-02-05
**Valid until:** 2026-03-05 (30 days - stable stack, internal patterns)
