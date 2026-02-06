# Phase 30: Batch Visibility & Lifecycle - Research

**Researched:** 2026-02-05
**Domain:** Batch NPC visibility toggling and dismiss/archive operations
**Confidence:** HIGH

## Summary

This phase extends the batch action framework built in Phase 29 with two new operation types: visibility toggling (reveal/hide NPCs) and dismiss/archive (remove NPCs from table). The research focused on four key areas: (1) understanding the existing single-NPC visibility and archive patterns already in the codebase, (2) mapping how the BatchActionService pattern from Phase 29 extends to non-damage/healing operations, (3) analyzing the SelectionBar component extension points for new buttons, and (4) identifying NPC-vs-PC filtering requirements.

The codebase already contains all needed infrastructure. The `CharacterDetailModal.ToggleVisibility()` method (lines 300-327) shows the exact single-NPC visibility toggle pattern: fetch CharacterEdit, set `VisibleToPlayers`, save via UpdateAsync, publish CharacterUpdateMessage. The `CharacterDetailAdmin.ArchiveNpc()` method (lines 198-224) shows the dismiss/archive pattern: set `IsArchived = true`, save, then call `RemoveCharacterFromTableAsync`. The `BatchActionService` provides the proven sequential processing loop with error aggregation. The `SelectionBar` component already has the action button row, result feedback display, and `HandleBatchResult` public method.

**Primary recommendation:** Extend `BatchActionService` with `ToggleVisibilityAsync` and `DismissAsync` methods following the existing `ProcessBatchAsync` sequential pattern. Add new `BatchActionType` enum values (Visibility, Dismiss). Extend `SelectionBar` with Toggle Visibility and Dismiss buttons, with PC-filtering logic that counts only NPC IDs before executing. Use `DialogService.OpenAsync` for dismiss confirmation (same pattern as damage/heal modals). Create a generalized `BatchActionResult` summary that works for visibility/dismiss (not just damage/healing).

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CSLA.NET | 9.1.0 | CharacterEdit fetch, modify VisibleToPlayers/IsArchived, save | Already in use; proven pattern in CharacterDetailModal |
| Radzen.Blazor | 8.4.2 | DialogService for dismiss confirmation modal | Already in use; Phase 29 established modal pattern |
| System.Reactive | 6.0.1 | CharactersUpdatedMessage broadcasting after batch | Already in use; BatchActionService uses this |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Bootstrap Icons | 1.11+ | Button icons (bi-eye, bi-eye-slash, bi-archive, bi-x-lg) | Already in use for action buttons |
| Bootstrap CSS | 5.x | Alert components for batch result feedback | Already in use; reuse Phase 29 result patterns |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Extending BatchActionService | New VisibilityService + DismissService | Unnecessary proliferation; same sequential pattern applies |
| Extending BatchActionType enum | Separate enum per operation | Would break existing result handling; unified enum keeps SelectionBar simple |
| DialogService for dismiss confirm | Browser window.confirm | Browser confirm blocks rendering thread; DialogService matches existing patterns |
| Single CharactersUpdatedMessage | Individual CharacterUpdateMessage per NPC | N messages = N refresh cycles; proven in Phase 29 |

**Installation:** No additional packages required - all dependencies already present.

## Architecture Patterns

### Recommended Project Structure
```
GameMechanics/
  Batch/
    BatchActionService.cs           # MODIFY: Add ToggleVisibilityAsync, DismissAsync
    BatchActionRequest.cs           # MODIFY: Remove Pool/Amount dependency for new types
    BatchActionResult.cs            # MODIFY: Generalize Summary for visibility/dismiss
    BatchActionType.cs              # MODIFY: Add Visibility, Dismiss enum values

Threa.Client/Components/
  Shared/
    SelectionBar.razor              # MODIFY: Add visibility/dismiss buttons, PC filtering
    BatchDismissConfirmModal.razor   # NEW: Simple confirm modal for dismiss
```

### Pattern 1: Extending BatchActionService for Non-Damage Operations
**What:** Add new async methods following the same sequential processing loop with error aggregation
**When to use:** Any batch operation on characters that follows fetch-modify-save pattern
**Example:**
```csharp
// Source: Adaptation of existing BatchActionService.ProcessBatchAsync pattern
public async Task<BatchActionResult> ToggleVisibilityAsync(BatchActionRequest request)
{
    var result = new BatchActionResult { ActionType = BatchActionType.Visibility };

    foreach (var characterId in request.CharacterIds)
    {
        try
        {
            var character = await _characterPortal.FetchAsync(characterId);

            // Skip non-NPCs silently (PCs should be filtered before calling)
            if (!character.IsNpc)
                continue;

            // CONTEXT decision: reveal all (not toggle) when mixed visible/hidden
            character.VisibleToPlayers = request.VisibilityTarget; // true = reveal, false = hide
            await _characterPortal.UpdateAsync(character);
            result.SuccessIds.Add(characterId);
            result.SuccessNames.Add(character.Name);
        }
        catch (Exception ex)
        {
            result.FailedIds.Add(characterId);
            result.Errors.Add($"Character {characterId}: {ex.Message}");
        }
    }

    // Single notification after batch completes
    if (result.SuccessIds.Count > 0)
    {
        await _timeEventPublisher.PublishCharactersUpdatedAsync(
            new CharactersUpdatedMessage
            {
                TableId = request.TableId,
                CharacterIds = result.SuccessIds,
                EventType = TimeEventType.EndOfRound,
                SourceId = "BatchActionService"
            });
    }

    return result;
}
```

### Pattern 2: Dismiss with Archive and Detach
**What:** Set IsArchived = true, save character, then remove from table - two-step operation per character
**When to use:** Batch dismiss/archive operations
**Example:**
```csharp
// Source: Adaptation of CharacterDetailAdmin.ArchiveNpc() pattern (lines 198-224)
public async Task<BatchActionResult> DismissAsync(BatchActionRequest request)
{
    var result = new BatchActionResult { ActionType = BatchActionType.Dismiss };

    foreach (var characterId in request.CharacterIds)
    {
        try
        {
            var character = await _characterPortal.FetchAsync(characterId);

            if (!character.IsNpc)
                continue;

            // Step 1: Archive the character
            character.IsArchived = true;
            await _characterPortal.UpdateAsync(character);

            // Step 2: Remove from table
            await _tableDal.RemoveCharacterFromTableAsync(request.TableId, characterId);

            result.SuccessIds.Add(characterId);
            result.SuccessNames.Add(character.Name);
        }
        catch (Exception ex)
        {
            result.FailedIds.Add(characterId);
            result.Errors.Add($"Character {characterId}: {ex.Message}");
        }
    }

    // Single notification after batch
    if (result.SuccessIds.Count > 0)
    {
        await _timeEventPublisher.PublishCharactersUpdatedAsync(
            new CharactersUpdatedMessage
            {
                TableId = request.TableId,
                CharacterIds = result.SuccessIds,
                EventType = TimeEventType.EndOfRound,
                SourceId = "BatchActionService"
            });
    }

    return result;
}
```

### Pattern 3: PC Filtering in SelectionBar (NPC-Only Button Visibility)
**What:** Compute NPC count from selected characters and conditionally render buttons
**When to use:** Buttons that should only apply to NPCs
**Example:**
```razor
@* Source: Pattern derived from CONTEXT.md decisions *@
@code {
    // Access to table character list needed for NPC filtering
    [Parameter]
    public IEnumerable<TableCharacterInfo>? AllCharacters { get; set; }

    private int SelectedNpcCount => AllCharacters?
        .Where(c => c.IsNpc && SelectedCharacterIds.Contains(c.CharacterId))
        .Count() ?? 0;

    private bool HasSelectedNpcs => SelectedNpcCount > 0;

    private List<int> GetSelectedNpcIds() => AllCharacters?
        .Where(c => c.IsNpc && SelectedCharacterIds.Contains(c.CharacterId))
        .Select(c => c.CharacterId)
        .ToList() ?? new();
}

@* Only render NPC-specific buttons when NPCs are selected *@
@if (HasSelectedNpcs)
{
    <button class="btn btn-sm btn-outline-info" @onclick="ToggleVisibility">
        <i class="bi bi-eye me-1"></i>Toggle Vis
    </button>
}
```

### Pattern 4: Contextual Visibility Button Label
**What:** Button label changes based on current visibility state of selected NPCs
**When to use:** Visibility toggle button
**Example:**
```csharp
// CONTEXT decision: reveal all when mixed visible/hidden NPCs
// So the button label reflects "Reveal" when ANY selected NPC is hidden
private string VisibilityButtonLabel
{
    get
    {
        var selectedNpcs = AllCharacters?
            .Where(c => c.IsNpc && SelectedCharacterIds.Contains(c.CharacterId));
        if (selectedNpcs == null || !selectedNpcs.Any()) return "Toggle Vis";

        bool anyHidden = selectedNpcs.Any(c => !c.VisibleToPlayers);
        return anyHidden ? "Reveal" : "Hide";
    }
}
```

### Pattern 5: Dismiss Confirmation via DialogService
**What:** Simple confirmation modal opened via DialogService before executing dismiss
**When to use:** Destructive batch operations requiring user confirmation
**Example:**
```razor
@* Source: Same DialogService.OpenAsync pattern as BatchDamageHealingModal *@
private async Task OpenDismissConfirmation()
{
    var npcCount = SelectedNpcCount;
    var result = await DialogService.OpenAsync<BatchDismissConfirmModal>(
        "Dismiss NPCs",
        new Dictionary<string, object>
        {
            { "NpcCount", npcCount }
        },
        new DialogOptions { Width = "350px", CloseDialogOnOverlayClick = true });

    if (result is bool confirmed && confirmed)
    {
        await ExecuteDismiss();
    }
}
```

### Anti-Patterns to Avoid
- **Separate visibility service class:** Reuse BatchActionService; the pattern is identical (sequential fetch-modify-save)
- **Passing full CharacterEdit objects to SelectionBar:** Use TableCharacterInfo (read-only) for NPC filtering; only fetch CharacterEdit in the service layer
- **True toggle semantics with mixed visibility:** CONTEXT explicitly says "reveal all" when mixed visible/hidden -- do not implement per-NPC toggle logic
- **Showing "skipped X PCs" in feedback:** CONTEXT says feedback shows only successes -- "3 NPCs revealed", not "3 NPCs revealed, 2 PCs skipped"
- **Clearing entire selection after dismiss:** CONTEXT says only dismissed NPCs are removed from selection; remaining selected characters stay selected

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Character visibility toggle | Direct DAL calls | CharacterEdit.VisibleToPlayers + DataPortal.UpdateAsync | CSLA handles dirty tracking, validation, concurrency |
| Character archive + detach | Custom archive logic | IsArchived + UpdateAsync, then RemoveCharacterFromTableAsync | Proven pattern from CharacterDetailAdmin.ArchiveNpc() |
| Confirmation dialog | Custom modal HTML | DialogService.OpenAsync<T> from Radzen | Handles overlay, focus, animation; consistent with Phase 29 |
| Batch notification | Per-NPC CharacterUpdateMessage | Single CharactersUpdatedMessage after batch | Prevents N refresh cycles; proven in BatchActionService |
| NPC filtering from selection | Manual ID-to-character lookup | LINQ on AllCharacters parameter with IsNpc check | TableCharacterInfo already has IsNpc, VisibleToPlayers |
| Selection cleanup after batch | Manual per-ID removal | HashSet.RemoveWhere with result.SuccessIds | Proven in GmTable.HandleBatchActionCompleted |

**Key insight:** Both operations (visibility and dismiss) follow the exact same fetch-modify-save pattern already in BatchActionService. The only difference is which property is modified and whether a table detach step follows. Reuse the pattern; do not create new service classes.

## Common Pitfalls

### Pitfall 1: Dismiss Without Table Detach
**What goes wrong:** NPC is archived (IsArchived = true) but still appears in the table character list
**Why it happens:** Forgetting the second step: `RemoveCharacterFromTableAsync`. CharacterDetailAdmin.ArchiveNpc does both steps.
**How to avoid:** Always pair `IsArchived = true; UpdateAsync` with `RemoveCharacterFromTableAsync`. If the detach fails after archive, catch and attempt to revert IsArchived.
**Warning signs:** Archived NPCs still showing in GmTable after dismiss

### Pitfall 2: Dismiss Partial Failure Leaves Orphaned State
**What goes wrong:** Character is archived but table detach fails, leaving inconsistent state
**Why it happens:** Two-step operation (archive + detach) is not atomic
**How to avoid:** If detach fails after archive, add to failed list with descriptive error. Consider reversing IsArchived on detach failure. Log the inconsistency for GM awareness.
**Warning signs:** NPC shows as archived but still appears in table character list

### Pitfall 3: Forgetting to Pass AllCharacters to SelectionBar
**What goes wrong:** SelectionBar cannot determine which selected IDs are NPCs vs PCs
**Why it happens:** SelectionBar only has `SelectedCharacterIds` (int set) with no character metadata
**How to avoid:** Add `AllCharacters` (or NPC-specific filtered list) parameter to SelectionBar
**Warning signs:** Cannot implement PC filtering; visibility/dismiss buttons always show or always hidden

### Pitfall 4: Stale Selection After Dismiss
**What goes wrong:** Selection contains IDs of NPCs that no longer exist in the table
**Why it happens:** Dismissed NPCs are removed from table but their IDs stay in selectedCharacterIds
**How to avoid:** CONTEXT says dismissed NPC IDs should be removed from selection. Use `result.SuccessIds` to remove from HashSet. The existing `RefreshCharacterListAsync` also calls `selectedCharacterIds.RemoveWhere` for stale IDs, providing a safety net.
**Warning signs:** Selection count stays high after dismiss; next batch action targets nonexistent characters

### Pitfall 5: BatchActionResult.Summary Hardcoded to Damage/Healing Format
**What goes wrong:** Summary says "Applied 0 FAT visibility to 3 characters" instead of "3 NPCs revealed"
**Why it happens:** Current Summary property uses damage/healing-specific format: `Applied {Amount} {Pool} {ActionType}...`
**How to avoid:** Make Summary format conditional on ActionType. For Visibility: "{count} NPC(s) revealed/hidden". For Dismiss: "Dismissed {count} NPC(s)".
**Warning signs:** Confusing feedback messages mentioning "FAT" or "VIT" for non-health operations

### Pitfall 6: Not Injecting ITableDal into BatchActionService
**What goes wrong:** DismissAsync cannot call RemoveCharacterFromTableAsync
**Why it happens:** Current BatchActionService only injects IDataPortal<CharacterEdit> and ITimeEventPublisher
**How to avoid:** Add ITableDal as a constructor dependency to BatchActionService
**Warning signs:** Compile error when calling RemoveCharacterFromTableAsync; or creating separate dismiss service

### Pitfall 7: Visibility Toggle with All-Hidden Selection Hides Instead of Reveals
**What goes wrong:** GM selects hidden NPCs, clicks toggle, and they get hidden again (no-op)
**Why it happens:** True toggle logic would flip each NPC's current state individually
**How to avoid:** CONTEXT decision: "reveal all" when mixed visible/hidden. Determine target visibility before processing: if any selected NPC is hidden, target = true (reveal). If all are visible, target = false (hide).
**Warning signs:** Inconsistent behavior depending on current NPC visibility states

## Code Examples

Verified patterns from existing codebase:

### Existing Single-NPC Visibility Toggle (CharacterDetailModal)
```csharp
// Source: CharacterDetailModal.razor lines 300-327
private async Task ToggleVisibility()
{
    if (character == null || !character.IsNpc) return;

    var wasVisible = character.VisibleToPlayers;
    character.VisibleToPlayers = !character.VisibleToPlayers;

    try
    {
        await characterPortal.UpdateAsync(character);

        await TimeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
        {
            CharacterId = character.Id,
            UpdateType = CharacterUpdateType.General,
            CampaignId = TableId.ToString(),
            Description = character.VisibleToPlayers ? "Revealed to players" : "Hidden from players"
        });
    }
    catch
    {
        character.VisibleToPlayers = wasVisible; // Revert on failure
    }

    StateHasChanged();
}
```

### Existing Single-NPC Archive (CharacterDetailAdmin)
```csharp
// Source: CharacterDetailAdmin.razor lines 198-224
private async Task ArchiveNpc()
{
    if (Character == null || !Character.IsNpc) return;
    isProcessing = true;

    try
    {
        // 1. Set IsArchived flag on character
        Character.IsArchived = true;
        await characterPortal.UpdateAsync(Character);

        // 2. Detach from table
        await tableDal.RemoveCharacterFromTableAsync(TableId, CharacterId);

        // 3. Notify parent that character was removed
        await OnCharacterRemoved.InvokeAsync();
    }
    catch (Exception ex)
    {
        errorMessage = $"Failed to archive: {ex.Message}";
        Character.IsArchived = false; // Revert on failure
    }
    finally
    {
        isProcessing = false;
    }
}
```

### Existing Batch Processing Pattern (BatchActionService.ProcessBatchAsync)
```csharp
// Source: GameMechanics/Batch/BatchActionService.cs lines 48-104
private async Task<BatchActionResult> ProcessBatchAsync(BatchActionRequest request)
{
    var result = new BatchActionResult
    {
        ActionType = request.ActionType,
        Pool = request.Pool,
        Amount = request.Amount
    };

    // Sequential processing - CSLA business objects are NOT thread-safe
    foreach (var characterId in request.CharacterIds)
    {
        try
        {
            var character = await _characterPortal.FetchAsync(characterId);
            // ... modify character ...
            await _characterPortal.UpdateAsync(character);
            result.SuccessIds.Add(characterId);
            result.SuccessNames.Add(character.Name);
        }
        catch (Exception ex)
        {
            result.FailedIds.Add(characterId);
            result.Errors.Add($"Character {characterId}: {ex.Message}");
        }
    }

    // Single notification after all updates
    if (result.SuccessIds.Count > 0)
    {
        await _timeEventPublisher.PublishCharactersUpdatedAsync(
            new CharactersUpdatedMessage
            {
                TableId = request.TableId,
                CharacterIds = result.SuccessIds,
                EventType = TimeEventType.EndOfRound,
                SourceId = "BatchActionService"
            });
    }

    return result;
}
```

### Existing SelectionBar Button Layout (Current State)
```razor
// Source: SelectionBar.razor lines 9-23
<div class="selection-bar @(SelectedCount > 0 ? "visible" : "")">
    <div class="d-flex justify-content-between align-items-center">
        <span class="selection-count">
            <i class="bi bi-check2-square me-2"></i>
            <strong>@SelectedCount</strong> selected
        </span>

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
    // ... result feedback section ...
</div>
```

### GmTable NPC Filtering Properties (Current State)
```csharp
// Source: GmTable.razor lines 606-625
// These show how NPC vs PC filtering is already done
private IEnumerable<TableCharacterInfo> playerCharacters =>
    tableCharacters?.Where(c => !c.IsNpc) ?? Enumerable.Empty<TableCharacterInfo>();

private IEnumerable<TableCharacterInfo> tableNpcs =>
    tableCharacters?.Where(c => c.IsNpc) ?? Enumerable.Empty<TableCharacterInfo>();

private IEnumerable<TableCharacterInfo> hiddenNpcs =>
    tableNpcs.Where(c => !c.VisibleToPlayers);
```

### Existing GmTable Batch Action Cleanup (HandleBatchActionCompleted)
```csharp
// Source: GmTable.razor lines 1494-1514
private async Task HandleBatchActionCompleted(BatchActionResult result)
{
    if (result.AllSucceeded)
    {
        selectedCharacterIds.Clear();
    }
    else if (result.HasFailures && result.SuccessIds.Count > 0)
    {
        foreach (var successId in result.SuccessIds)
        {
            selectedCharacterIds.Remove(successId);
        }
    }

    await RefreshCharacterListAsync();
    StateHasChanged();
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Per-NPC visibility toggle in modal | Batch visibility from SelectionBar | This phase | GM can reveal/hide all at once |
| Per-NPC archive in Admin tab | Batch dismiss from SelectionBar | This phase | GM can dismiss group in one action |
| Damage/healing-only BatchActionService | Generalized batch service supporting visibility/dismiss | This phase | Service becomes extensible for future batch ops |
| BatchActionResult with damage-specific Summary | Action-type-aware Summary | This phase | Feedback messages make sense for all operations |

**Deprecated/outdated:**
- Creating separate service classes per batch operation: BatchActionService handles all batch types
- Damage-specific BatchActionResult.Summary format: Must be generalized for new action types

## Open Questions

Things that couldn't be fully resolved:

1. **BatchActionRequest generalization approach**
   - What we know: Current request has Pool and Amount fields specific to damage/healing; new operations don't need these
   - What's unclear: Should we add a new `VisibilityTarget` (bool) property to the existing record, or create a more generic approach?
   - Recommendation: Add `VisibilityTarget` bool property (nullable) to BatchActionRequest. Pool and Amount are already only used by damage/healing; adding VisibilityTarget for visibility operations keeps the record flat and simple. Dismiss needs no extra data beyond CharacterIds.

2. **ITableDal injection into BatchActionService**
   - What we know: DismissAsync needs RemoveCharacterFromTableAsync from ITableDal
   - What's unclear: Should BatchActionService take ITableDal as constructor dependency, or should a new service handle dismiss?
   - Recommendation: Add ITableDal to BatchActionService constructor. The service already handles batch character operations; dismiss is a natural extension. Creating a separate service would fragment the batch processing pattern unnecessarily.

3. **Activity log entries for batch operations**
   - What we know: Single-NPC reveal creates activity log entry `"{name} appears!"` (GmTable.RevealNpc line 1473)
   - What's unclear: Should batch reveal create individual activity entries per NPC or a single batch entry?
   - Recommendation: Single batch entry: "3 NPCs revealed" or "Dismissed 5 NPCs". Individual entries would flood the activity log. GmTable already has `AddLogEntry` available.

4. **Selection cleanup behavior difference for visibility vs dismiss**
   - What we know: CONTEXT says visibility keeps selection, dismiss removes only dismissed NPCs
   - What's unclear: The current HandleBatchActionCompleted clears ALL selection on success; this needs different behavior per action type
   - Recommendation: Modify HandleBatchActionCompleted to check `result.ActionType`. For Visibility: don't clear any selection. For Dismiss: remove only dismissed NPC IDs. For Damage/Healing: keep current behavior (clear on full success).

## Sources

### Primary (HIGH confidence)
- Existing codebase: `SelectionBar.razor` - current action bar with damage/heal buttons and result feedback
- Existing codebase: `BatchActionService.cs` - sequential processing pattern with error aggregation
- Existing codebase: `BatchActionRequest.cs`, `BatchActionResult.cs` - current DTOs
- Existing codebase: `CharacterDetailModal.razor` lines 300-327 - single-NPC visibility toggle pattern
- Existing codebase: `CharacterDetailAdmin.razor` lines 198-224 - single-NPC archive pattern
- Existing codebase: `GmTable.razor` lines 606-625 - NPC/PC filtering, selection state management
- Existing codebase: `TableCharacterInfo.cs` - read-only info with IsNpc, VisibleToPlayers, Disposition
- Existing codebase: `CharacterEdit.cs` lines 379-393 - VisibleToPlayers and IsArchived CSLA properties
- Existing codebase: `ITableDal.cs` line 56 - RemoveCharacterFromTableAsync interface
- Phase 30 CONTEXT.md - all user decisions on layout, confirmation, mixed selection, post-action state

### Secondary (MEDIUM confidence)
- Phase 29 RESEARCH.md - batch action patterns and pitfalls documentation
- Phase 29 plan summaries (01, 02, 03) - established patterns and architectural decisions

### Tertiary (LOW confidence)
- None - All patterns verified against existing codebase

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - No new dependencies; all patterns verified in existing codebase
- Architecture: HIGH - Direct extension of Phase 29 BatchActionService pattern
- Pitfalls: HIGH - Based on existing code analysis (two-step dismiss, stale selection, summary format)
- Code examples: HIGH - All sourced from actual codebase files with line numbers

**Research date:** 2026-02-05
**Valid until:** 2026-03-05 (30 days - stable stack, internal patterns)
