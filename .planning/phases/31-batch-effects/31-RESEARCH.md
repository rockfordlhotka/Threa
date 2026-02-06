# Phase 31: Batch Effects - Research

**Researched:** 2026-02-05
**Domain:** Batch effect add/remove operations for multiple selected characters
**Confidence:** HIGH

## Summary

This phase adds batch effect operations (add and remove) to the existing SelectionBar, extending the proven batch action framework from Phases 29-30. The research focused on four areas: (1) understanding the existing single-character effect management flow (EffectFormModal, EffectTemplatePickerModal, EffectManagementModal), (2) mapping how the BatchActionService sequential processing pattern applies to effect operations, (3) analyzing the EffectRecord creation lifecycle and CSLA child object patterns required for batch add, and (4) identifying the data query pattern needed for the batch remove union list.

The codebase has mature, well-tested infrastructure for all required pieces. The `EffectFormModal` (Shared component, ~735 lines) handles effect creation with template pre-fill, duration types, modifiers, and "Save as Template" functionality. The `EffectTemplatePickerModal` (Shared component, ~405 lines) provides searchable/filterable template browsing. The `BatchActionService` provides the sequential processing loop with error aggregation, and the `SelectionBar` provides the action bar with result feedback. The `EffectList.AddEffect()` method handles stacking logic via `IEffectBehavior.OnAdding()`, returning `EffectAddOutcome` (Add, Reject, Replace, AddWithSideEffects).

The key architectural insight is that batch effect add requires creating an `EffectRecord` child object via `IChildDataPortal<EffectRecord>.CreateChildAsync()` for each character individually (because the EffectRecord's `[CreateChild]` method references `Parent.Parent as CharacterEdit` for game time). The effect configuration (name, type, duration, modifiers) is captured once in the UI, but the actual EffectRecord is created per-character in the processing loop. For batch remove, the union list of effect names must be computed from `TableCharacterInfo.EffectSummary` or by fetching individual characters -- the latter is more accurate since EffectSummary is a display string.

**Primary recommendation:** Extend `BatchActionService` with `AddEffectAsync` and `RemoveEffectsAsync` methods. Add `BatchActionType.EffectAdd` and `BatchActionType.EffectRemove` enum values. Extend `BatchActionRequest` with effect-specific fields (EffectName, EffectType, DurationSeconds, BehaviorStateJson for add; EffectNamesToRemove for remove). Reuse existing `EffectFormModal` for the batch add flow by parameterizing it with a "batch mode" flag that skips the per-character save and returns the effect configuration. Create a new `BatchEffectRemoveModal` for the remove flow with a checkbox list of union effect names.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CSLA.NET | 9.1.0 | CharacterEdit fetch, EffectRecord child creation, save | Already in use; proven pattern in EffectFormModal |
| Radzen.Blazor | 8.4.2 | DialogService for add/remove modals | Already in use; Phases 29-30 established modal pattern |
| System.Reactive | 6.0.1 | CharactersUpdatedMessage broadcasting after batch | Already in use; BatchActionService uses this |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| System.Text.Json | built-in | EffectState serialization/deserialization | Already in use; EffectState.Serialize()/Deserialize() |
| Bootstrap Icons | 1.11+ | Button icons (bi-stars, bi-x-circle) | Already in use for effect type icons |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Extending BatchActionService | New EffectBatchService | Unnecessary proliferation; same sequential pattern applies |
| Extending BatchActionRequest | Separate EffectBatchRequest type | Would require separate result handling; unified approach keeps SelectionBar simple |
| Reusing EffectFormModal in batch | New BatchEffectAddModal | EffectFormModal has 700+ lines of form logic that would be duplicated; parameterize instead |
| Fetching all characters for union list | Using TableCharacterInfo.EffectSummary | EffectSummary is a display string (comma-separated), not structured data; fetching gives accurate effect names |

**Installation:** No additional packages required -- all dependencies already present.

## Architecture Patterns

### Recommended Project Structure
```
GameMechanics/
  Batch/
    BatchActionService.cs           # MODIFY: Add AddEffectAsync, RemoveEffectsAsync
    BatchActionRequest.cs           # MODIFY: Add effect-specific fields
    BatchActionResult.cs            # MODIFY: Add EffectAdd/EffectRemove summary cases
    BatchActionType.cs              # MODIFY: Add EffectAdd, EffectRemove enum values

Threa.Client/Components/
  Shared/
    SelectionBar.razor              # MODIFY: Add Effect Add/Remove buttons
    EffectFormModal.razor            # MODIFY: Add batch mode (IsBatchMode parameter)
    BatchEffectRemoveModal.razor     # NEW: Checkbox list of union effect names
```

### Pattern 1: Batch Effect Add via EffectFormModal Reuse
**What:** Parameterize the existing EffectFormModal with an `IsBatchMode` flag. In batch mode, the modal skips the per-character fetch/save and instead returns the effect configuration data for the caller to process.
**When to use:** Batch effect add flow
**Example:**
```csharp
// In SelectionBar.razor - opens EffectFormModal in batch mode
private async Task OpenBatchEffectAdd()
{
    // Two entry paths per CONTEXT decision:
    // 1. "From Template" -> EffectTemplatePickerModal -> EffectFormModal (pre-filled)
    // 2. "Custom Effect" -> EffectFormModal (blank)

    var result = await DialogService.OpenAsync<BatchEffectAddModal>(
        "Add Effect to Selected Characters",
        new Dictionary<string, object>
        {
            { "SelectedCount", SelectedCount }
        },
        new DialogOptions { Width = "600px", CloseDialogOnOverlayClick = true });

    if (result is BatchEffectConfig config)
    {
        await ExecuteBatchEffectAdd(config);
    }
}
```

### Pattern 2: Batch Effect Add Processing Loop
**What:** Sequential processing creating EffectRecord per character, sharing a captured game time
**When to use:** Inside BatchActionService.AddEffectAsync
**Example:**
```csharp
// Source: Adaptation of EffectFormModal.SaveEffect() and BatchActionService.ProcessBatchAsync
public async Task<BatchActionResult> AddEffectAsync(BatchActionRequest request)
{
    var result = new BatchActionResult
    {
        ActionType = BatchActionType.EffectAdd,
        EffectName = request.EffectName
    };

    // CONTEXT decision: capture game time once before loop
    // All characters get same CreatedAt/ExpiresAt
    long? sharedGameTime = null;

    foreach (var characterId in request.CharacterIds)
    {
        try
        {
            var character = await _characterPortal.FetchAsync(characterId);

            // Capture game time from first character (all should be synchronized)
            sharedGameTime ??= character.CurrentGameTimeSeconds;

            var effect = await _effectPortal.CreateChildAsync(
                request.EffectType,
                request.EffectName,
                (string?)null,  // no location for batch effects
                request.DurationRounds,
                request.BehaviorStateJson
            );

            // Apply shared timestamp
            effect.CreatedAtEpochSeconds = sharedGameTime.Value;
            if (request.DurationSeconds.HasValue)
            {
                effect.ExpiresAtEpochSeconds = sharedGameTime.Value + request.DurationSeconds.Value;
            }

            effect.Description = request.EffectDescription;
            effect.Source = "GM (Batch)";

            // CONTEXT decision: defer to EffectList.AddEffect stacking logic
            var added = character.Effects.AddEffect(effect);
            // Note: even if stacking rejects, we still save (AddEffect handles it)

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

### Pattern 3: Batch Effect Remove with Union List
**What:** Collect unique effect names across all selected characters, present as multi-select checkbox list, remove matching effects
**When to use:** Batch effect remove flow
**Example:**
```csharp
// Source: Adaptation of EffectList.RemoveEffect pattern
public async Task<BatchActionResult> RemoveEffectsAsync(BatchActionRequest request)
{
    var result = new BatchActionResult
    {
        ActionType = BatchActionType.EffectRemove
    };

    int totalEffectsRemoved = 0;

    foreach (var characterId in request.CharacterIds)
    {
        try
        {
            var character = await _characterPortal.FetchAsync(characterId);
            int removedFromThis = 0;

            foreach (var effectName in request.EffectNamesToRemove)
            {
                // Find matching effects by name (there may be multiple instances)
                var matching = character.Effects
                    .Where(e => e.Name == effectName && e.EffectType != EffectType.Wound)
                    .ToList();

                foreach (var effect in matching)
                {
                    character.Effects.RemoveEffect(effect.Id);
                    removedFromThis++;
                }
            }

            // CONTEXT decision: silently skip characters that don't have the effect
            if (removedFromThis > 0)
            {
                await _characterPortal.UpdateAsync(character);
                result.SuccessIds.Add(characterId);
                result.SuccessNames.Add(character.Name);
                totalEffectsRemoved += removedFromThis;
            }
            // Characters with no matching effects are silently skipped (not counted)
        }
        catch (Exception ex)
        {
            result.FailedIds.Add(characterId);
            result.Errors.Add($"Character {characterId}: {ex.Message}");
        }
    }

    result.TotalEffectsRemoved = totalEffectsRemoved;

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

### Pattern 4: Building the Union Effect Name List
**What:** Fetch each selected character to collect unique effect names for the remove modal
**When to use:** Before opening the BatchEffectRemoveModal
**Example:**
```csharp
// In SelectionBar or a helper method
private async Task<List<EffectNameInfo>> GetUnionEffectNames()
{
    var effectNames = new Dictionary<string, EffectNameInfo>();

    foreach (var characterId in SelectedCharacterIds)
    {
        try
        {
            var character = await characterPortal.FetchAsync(characterId);
            foreach (var effect in character.Effects
                .Where(e => e.EffectType != EffectType.Wound && e.IsActive))
            {
                if (!effectNames.ContainsKey(effect.Name))
                {
                    effectNames[effect.Name] = new EffectNameInfo
                    {
                        Name = effect.Name,
                        EffectType = effect.EffectType,
                        CharacterCount = 0
                    };
                }
                effectNames[effect.Name].CharacterCount++;
            }
        }
        catch { /* skip inaccessible characters */ }
    }

    return effectNames.Values
        .OrderBy(e => e.EffectType)
        .ThenBy(e => e.Name)
        .ToList();
}
```

### Pattern 5: EffectFormModal Batch Mode Adaptation
**What:** Add `IsBatchMode` parameter to EffectFormModal that changes its behavior from "fetch character, add effect, save character" to "collect effect config, return to caller"
**When to use:** When the EffectFormModal is opened from the batch add flow
**Example:**
```csharp
// In EffectFormModal.razor @code block

// New parameter
[Parameter] public bool IsBatchMode { get; set; }
[Parameter] public int BatchSelectedCount { get; set; }

// Modified SaveEffect method
private async Task SaveEffect()
{
    if (!IsFormValid) return;

    if (IsBatchMode)
    {
        // In batch mode, return config instead of saving
        var config = new BatchEffectConfig
        {
            EffectName = effectName,
            EffectType = selectedEffectType,
            Description = description,
            DurationType = durationType,
            DurationValue = durationValue,
            DurationSeconds = GetDurationSeconds(),
            BehaviorStateJson = BuildEffectState().Serialize()
        };
        DialogService.Close(config);
        return;
    }

    // ... existing single-character save logic ...
}
```

### Anti-Patterns to Avoid
- **Creating EffectRecord outside the character context:** EffectRecord's `[CreateChild]` accesses `Parent.Parent as CharacterEdit` for game time. Always create after fetching the character. For batch, capture the shared time separately and override.
- **Using TableCharacterInfo.EffectSummary for remove list:** EffectSummary is a formatted display string (e.g., "Poison (3 rnd), Blessed"). Parse it for structured data and you get fragile, broken code. Fetch CharacterEdit objects instead.
- **Parallel processing of characters:** CSLA business objects are NOT thread-safe. Always process sequentially (same pattern as all other batch operations).
- **Saving characters that had no effects removed:** If a character didn't have any of the selected effects, skip the UpdateAsync call entirely. Unnecessary saves create pointless DB writes and dirty tracking overhead.
- **Removing wound effects in batch:** The remove union list should exclude EffectType.Wound -- wounds have special lifecycle management through the wound system.
- **Trying to reuse batch effects with concentration links:** CONTEXT decision explicitly states batch effects are always standalone (no SourceCasterId/SourceEffectId).

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Effect creation with stacking | Custom effect duplication check | EffectList.AddEffect() + IEffectBehavior.OnAdding() | Stacking rules are complex (Add/Reject/Replace/AddWithSideEffects); existing behavior handles all cases |
| Effect state serialization | Manual JSON construction | EffectState class with Serialize()/Deserialize() | Handles nulls, empty collections, and all modifier types consistently |
| Duration calculation (seconds) | Custom time math | EffectFormModal.GetDurationSeconds() pattern | Already handles Rounds/Turns/Minutes/Hours/Days conversion |
| Template pre-fill flow | Custom template-to-form mapping | EffectFormModal.ApplyTemplate() | Already maps DurationType enum, handles modifier dictionaries, shows feedback |
| Batch notification | Per-character CharacterUpdateMessage | Single CharactersUpdatedMessage after batch | Prevents N refresh cycles; proven in BatchActionService |
| Effect type icons/colors | Custom icon mapping | GetEffectTypeIcon/GetEffectTypeAlertClass from EffectFormModal | Already maps all EffectType values to Bootstrap icons and colors |
| Confirmation modals | Custom modal HTML | DialogService.OpenAsync<T> from Radzen | Handles overlay, focus, animation; consistent with Phase 29-30 patterns |

**Key insight:** The EffectFormModal already contains all the form logic for creating effects -- name, type, duration, modifiers, template picker, template save. The batch add flow should reuse this component rather than building a parallel form. The only difference is that batch mode returns the configuration instead of saving directly to a character.

## Common Pitfalls

### Pitfall 1: EffectRecord CreatedAt/ExpiresAt Divergence Across Characters
**What goes wrong:** Each character gets a slightly different CreatedAtEpochSeconds because game time is read from each character individually, and characters may not be perfectly synchronized.
**Why it happens:** EffectRecord's CreateChild method reads `Parent.Parent as CharacterEdit`.CurrentGameTimeSeconds, which could differ between characters if they processed different time events.
**How to avoid:** CONTEXT decision: "capture game time once before processing loop." Read CurrentGameTimeSeconds from the first character and use it for all. Override CreatedAtEpochSeconds and ExpiresAtEpochSeconds after CreateChildAsync, before calling AddEffect.
**Warning signs:** Effects on different characters showing different remaining durations despite being applied in the same batch.

### Pitfall 2: EffectList.AddEffect Returns False (Stacking Rejection)
**What goes wrong:** A character already has the same effect and the behavior's stacking logic rejects the duplicate. The batch result doesn't account for this case.
**Why it happens:** EffectList.AddEffect calls behavior.OnAdding() which may return EffectAddOutcome.Reject if the effect can't stack.
**How to avoid:** Check the return value of AddEffect. If false, the character still needs saving (the effect state may have been modified by a merge), but the batch result should note that the effect was "already present" rather than "failed." Consider counting rejections separately or noting them in the result.
**Warning signs:** Batch says "Added Bless to 5 characters" but some characters already had it and got merged/rejected silently.

### Pitfall 3: Union List Fetches All Characters Individually
**What goes wrong:** Building the union effect name list for remove requires fetching every selected character via DataPortal, creating N network calls before the modal even opens.
**Why it happens:** TableCharacterInfo doesn't have structured effect data -- only EffectSummary (display string) and EffectCount (int).
**How to avoid:** Accept the N fetch calls as necessary but show a loading indicator. For large selections (>10), consider fetching in the service layer and caching briefly. The modal should show a spinner while loading. Alternatively, consider adding structured effect data to TableCharacterInfo in a future phase.
**Warning signs:** Long delay when clicking "Remove Effects" with many characters selected; no loading feedback.

### Pitfall 4: BatchActionResult.Summary Not Handling Effect Types
**What goes wrong:** Summary says "Applied 0 FAT effectadd to 3 characters" instead of "Added Bless to 3 characters"
**Why it happens:** Current Summary property uses damage/healing-specific format with Pool and Amount fields.
**How to avoid:** Extend the Summary switch expression with cases for EffectAdd and EffectRemove. Use EffectName property for add summary and TotalEffectsRemoved for remove summary.
**Warning signs:** Confusing feedback messages mentioning health pools for effect operations.

### Pitfall 5: Forgetting IChildDataPortal<EffectRecord> Injection
**What goes wrong:** BatchActionService cannot create EffectRecord child objects for the add operation.
**Why it happens:** Current service only injects IDataPortal<CharacterEdit>, ITimeEventPublisher, and ITableDal. EffectRecord creation requires IChildDataPortal<EffectRecord>.
**How to avoid:** Add IChildDataPortal<EffectRecord> as a constructor dependency to BatchActionService. CSLA child portals are already registered in DI by the CSLA framework configuration.
**Warning signs:** Compile error or runtime DI resolution failure when calling CreateChildAsync.

### Pitfall 6: Removing Effects Without Calling OnRemove Behavior
**What goes wrong:** Effects are removed from the list but their OnRemove behavior (which may revert stat changes) is never called.
**Why it happens:** Calling `Remove(effect)` directly on the list instead of `RemoveEffect(effectId)` which calls behavior.OnRemove first.
**How to avoid:** Always use `EffectList.RemoveEffect(effectId)` instead of direct list manipulation. The RemoveEffect method calls `effect.Behavior.OnRemove(effect, Character)` before removing.
**Warning signs:** Character stats remain modified even after effects are "removed."

### Pitfall 7: Batch Effect Remove Doesn't Save Characters with No Changes
**What goes wrong:** Characters that had no matching effects still get an unnecessary UpdateAsync call.
**Why it happens:** Processing loop calls UpdateAsync for every character regardless of whether any effects were actually removed.
**How to avoid:** Track removedFromThis count per character. Only call UpdateAsync and add to SuccessIds if removedFromThis > 0. Characters with zero removals are silently skipped per CONTEXT decision.
**Warning signs:** Unnecessary database writes, potentially triggering cascade effects or dirty tracking noise.

## Code Examples

Verified patterns from existing codebase:

### Existing Single-Character Effect Add (EffectFormModal.SaveEffect)
```csharp
// Source: EffectFormModal.razor lines 462-546
private async Task SaveEffect()
{
    if (!IsFormValid) return;
    isProcessing = true;
    errorMessage = null;

    try
    {
        var character = await characterPortal.FetchAsync(CharacterId);
        var state = BuildEffectState();
        var durationSeconds = GetDurationSeconds();

        // Create new effect with epoch-based expiration
        var currentTime = character.CurrentGameTimeSeconds;
        var effect = await effectPortal.CreateChildAsync(
            selectedEffectType,
            effectName,
            (string?)null, // location
            durationSeconds.HasValue ? (int?)(durationSeconds.Value / 3) : null,
            state.Serialize()
        );

        // Set epoch-based expiration
        if (currentTime > 0)
        {
            effect.CreatedAtEpochSeconds = currentTime;
            if (durationSeconds.HasValue)
            {
                effect.ExpiresAtEpochSeconds = currentTime + durationSeconds.Value;
            }
        }

        effect.Description = description;
        effect.Source = "GM";
        character.Effects.AddEffect(effect);

        await characterPortal.UpdateAsync(character);
        DialogService.Close(true);
    }
    catch (Exception ex)
    {
        errorMessage = $"Error saving effect: {ex.Message}";
    }
    finally
    {
        isProcessing = false;
    }
}
```

### Existing EffectList.AddEffect Stacking Logic
```csharp
// Source: GameMechanics/EffectList.cs lines 28-72
public bool AddEffect(EffectRecord effect)
{
    var behavior = effect.Behavior;
    var result = behavior.OnAdding(effect, Character);

    switch (result.Outcome)
    {
        case EffectAddOutcome.Add:
            Add(effect);
            behavior.OnApply(effect, Character);
            return true;

        case EffectAddOutcome.Reject:
            return false;

        case EffectAddOutcome.Replace:
            if (result.ReplaceEffectId.HasValue)
            {
                var existing = this.FirstOrDefault(e => e.Id == result.ReplaceEffectId.Value);
                if (existing != null)
                {
                    existing.Behavior.OnRemove(existing, Character);
                    Remove(existing);
                }
            }
            Add(effect);
            behavior.OnApply(effect, Character);
            return true;

        case EffectAddOutcome.AddWithSideEffects:
            Add(effect);
            behavior.OnApply(effect, Character);
            return true;

        default:
            return false;
    }
}
```

### Existing EffectList.RemoveEffect with Behavior Callback
```csharp
// Source: GameMechanics/EffectList.cs lines 79-88
public bool RemoveEffect(Guid effectId)
{
    var effect = this.FirstOrDefault(e => e.Id == effectId);
    if (effect == null)
        return false;

    effect.Behavior.OnRemove(effect, Character);
    Remove(effect);
    return true;
}
```

### Existing EffectRecord CreateChild with Epoch Time
```csharp
// Source: GameMechanics/EffectRecord.cs lines 348-369
[CreateChild]
private void CreateWithEpochTime(EffectType effectType, string name, string? location,
    long? durationSeconds, string? behaviorState, long currentGameTimeSeconds)
{
    using (BypassPropertyChecks)
    {
        Id = Guid.NewGuid();
        EffectType = effectType;
        Name = name;
        Location = location;
        BehaviorState = behaviorState;
        CurrentStacks = 1;
        IsActive = true;

        CreatedAtEpochSeconds = currentGameTimeSeconds;
        if (durationSeconds.HasValue)
        {
            ExpiresAtEpochSeconds = currentGameTimeSeconds + durationSeconds.Value;
        }
    }
}
```

### Existing SelectionBar Batch Action Execution Pattern
```csharp
// Source: SelectionBar.razor lines 150-200
private async Task OpenDamageModal()
{
    var result = await DialogService.OpenAsync<BatchDamageHealingModal>(
        "Apply Damage",
        new Dictionary<string, object>
        {
            { "Mode", BatchActionType.Damage },
            { "SelectedCount", SelectedCount }
        },
        new DialogOptions { Width = "350px", CloseDialogOnOverlayClick = true });

    if (result is BatchInputResult input)
    {
        await ExecuteBatchAction(BatchActionType.Damage, input.Amount, input.Pool);
    }
}
```

### Existing BatchActionResult Summary Switch
```csharp
// Source: GameMechanics/Batch/BatchActionResult.cs lines 68-79
public string Summary => ActionType switch
{
    BatchActionType.Visibility => HasFailures
        ? $"{SuccessIds.Count} of {TotalCount} NPC(s) {(VisibilityAction ? "revealed" : "hidden")}"
        : $"{SuccessIds.Count} NPC(s) {(VisibilityAction ? "revealed" : "hidden")}",
    BatchActionType.Dismiss => HasFailures
        ? $"Dismissed {SuccessIds.Count} of {TotalCount} NPC(s)"
        : $"Dismissed {SuccessIds.Count} NPC(s)",
    _ => HasFailures
        ? $"Applied {Amount} {Pool} {ActionType.ToString().ToLower()} to {SuccessIds.Count} of {TotalCount} characters"
        : $"Applied {Amount} {Pool} {ActionType.ToString().ToLower()} to {SuccessIds.Count} character(s)"
};
```

### Existing HandleBatchActionCompleted in GmTable
```csharp
// Source: GmTable.razor lines 1494-1530
private async Task HandleBatchActionCompleted(BatchActionResult result)
{
    switch (result.ActionType)
    {
        case BatchActionType.Visibility:
            // Visibility: keep selection intact
            break;
        case BatchActionType.Dismiss:
            // Dismiss: remove only dismissed NPC IDs
            foreach (var successId in result.SuccessIds)
                selectedCharacterIds.Remove(successId);
            break;
        default:
            // Damage/Healing: clear all on full success
            if (result.AllSucceeded)
                selectedCharacterIds.Clear();
            else if (result.HasFailures && result.SuccessIds.Count > 0)
                foreach (var successId in result.SuccessIds)
                    selectedCharacterIds.Remove(successId);
            break;
    }

    await RefreshCharacterListAsync();
    StateHasChanged();
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Per-character effect add via CharacterDetailModal | Batch effect add from SelectionBar | This phase | GM can buff/debuff entire group at once |
| Per-character effect remove via EffectManagementModal | Batch effect remove from SelectionBar | This phase | GM can strip effects from group at once |
| Damage/healing-only BatchActionService | Fully generalized batch service with effects | This phase | Service handles all batch operation types |

**Deprecated/outdated:**
- BatchActionResult.Summary with only damage/healing/visibility/dismiss formats: Must add EffectAdd and EffectRemove cases
- BatchActionRequest without effect fields: Must be extended with EffectName, EffectType, BehaviorStateJson, etc.
- EffectFormModal with implicit single-character assumption: Must accept IsBatchMode parameter

## Open Questions

Things that couldn't be fully resolved:

1. **EffectFormModal reuse strategy: new wrapper component vs parameterized existing**
   - What we know: EffectFormModal has Character and CharacterId parameters used for single-character save. In batch mode, these aren't applicable.
   - What's unclear: Should we add IsBatchMode directly to EffectFormModal (simpler, slightly messier), or create a BatchEffectAddModal wrapper that hosts the form fields without the save logic?
   - Recommendation: Create a `BatchEffectAddModal` wrapper component that provides the two entry paths ("From Template" and "Custom Effect") and hosts form logic for collecting effect configuration. This avoids polluting EffectFormModal with batch concerns and keeps the single-character flow untouched. The wrapper can reuse `EffectTemplatePickerModal` for template selection and share form field layout via extraction or duplication of the simpler form parts. Per CONTEXT "Claude's Discretion" this is a planner decision.

2. **Union list performance with many selected characters**
   - What we know: Building the union list requires fetching each selected character individually via DataPortal.
   - What's unclear: With 20+ characters selected, this could take several seconds. Should we add structured effect data to TableCharacterInfo?
   - Recommendation: Accept the N fetch cost for now with a loading spinner. TableCharacterInfo already has EffectCount and EffectSummary but these are display-only. Adding structured effect names to TableCharacterInfo is a separate optimization concern. For this phase, the sequential fetch in the remove flow is acceptable since GMs typically don't select more than 10-15 characters.

3. **Stacking rejection feedback in batch add**
   - What we know: EffectList.AddEffect() returns false when stacking rejects the effect. The batch result currently only tracks success/failure at the character level.
   - What's unclear: Should a stacking rejection be counted as a "success" (character was processed), a "failure" (effect wasn't added), or something else?
   - Recommendation: Count it as a success with a note. The character was successfully processed; the stacking system made the right decision. If the GM needs to know, the result summary could optionally note "X already had this effect" but per CONTEXT the add summary is count-only format ("Added Bless to 5 characters"). Keep it simple.

4. **Selection behavior after effect operations**
   - What we know: CONTEXT says "Selection stays intact after both add and remove operations (GM may want to apply multiple effects to same group)."
   - What's unclear: HandleBatchActionCompleted in GmTable currently handles this per ActionType. Need to add EffectAdd and EffectRemove cases.
   - Recommendation: Add cases to the switch that keep selection intact (same as Visibility behavior). This is straightforward.

## Sources

### Primary (HIGH confidence)
- Existing codebase: `EffectFormModal.razor` - complete effect creation form with template picker, duration, modifiers
- Existing codebase: `EffectTemplatePickerModal.razor` - searchable/filterable template browsing
- Existing codebase: `EffectManagementModal.razor` - single-character effect management with add/edit/remove
- Existing codebase: `EffectList.cs` - AddEffect stacking logic, RemoveEffect with behavior callbacks
- Existing codebase: `EffectRecord.cs` - CreateChild methods, epoch-based expiration, property definitions
- Existing codebase: `EffectState.cs` - serialization/deserialization, modifier structures
- Existing codebase: `EffectAddResult.cs` - stacking outcomes (Add/Reject/Replace/AddWithSideEffects)
- Existing codebase: `BatchActionService.cs` - sequential processing pattern, error aggregation
- Existing codebase: `BatchActionRequest.cs` - record type with extensible fields
- Existing codebase: `BatchActionResult.cs` - success/failure tracking, Summary generation
- Existing codebase: `SelectionBar.razor` - action buttons, modal opening, result feedback
- Existing codebase: `GmTable.razor` - HandleBatchActionCompleted selection cleanup per ActionType
- Existing codebase: `TableCharacterInfo.cs` - EffectCount, EffectSummary display properties
- Existing codebase: `CharacterEdit.cs` - CurrentGameTimeSeconds, Effects property
- Existing codebase: `EffectType.cs` (Threa.Dal.Dto) - all effect type enum values
- Phase 31 CONTEXT.md - all user decisions on add flow, remove behavior, result feedback, stacking

### Secondary (MEDIUM confidence)
- Phase 30 RESEARCH.md - established patterns for extending BatchActionService and SelectionBar
- Phase 29 plan summaries - batch action framework architectural decisions

### Tertiary (LOW confidence)
- None - All patterns verified against existing codebase

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - No new dependencies; all patterns verified in existing codebase
- Architecture: HIGH - Direct extension of Phases 29-30 BatchActionService and SelectionBar patterns
- Pitfalls: HIGH - Based on analysis of EffectRecord lifecycle, stacking logic, and existing batch patterns
- Code examples: HIGH - All sourced from actual codebase files

**Research date:** 2026-02-05
**Valid until:** 2026-03-05 (30 days - stable stack, internal patterns)
