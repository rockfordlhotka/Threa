# Technology Stack - v1.6 Batch Character Actions

**Project:** Threa TTRPG Assistant - Batch Character Actions
**Researched:** 2026-02-04
**Overall Confidence:** HIGH

---

## Executive Summary

Batch character selection and action execution requires **NO new dependencies**. The existing stack (Radzen.Blazor 8.4.2, CSLA.NET 9.1.0, Rx.NET 6.0.1) provides all necessary capabilities. This is a UI/pattern implementation task, not a technology addition.

---

## Recommended Stack (No Changes)

The existing stack is sufficient:

| Technology | Current Version | Purpose | Status |
|------------|-----------------|---------|--------|
| Radzen.Blazor | 8.4.2 | Multi-select UI components | **Sufficient** |
| CSLA.NET | 9.1.0 | Business objects, data portal | **Sufficient** |
| System.Reactive | 6.0.1 | Real-time messaging (InMemoryMessageBus) | **Sufficient** |
| Microsoft.AspNetCore.Components.QuickGrid | 10.0.1 | Alternative grid (in Client) | **Not needed** |

### Why No New Dependencies

1. **Radzen.Blazor already supports multi-select**: `SelectionMode="DataGridSelectionMode.Multiple"` with `@bind-Value` for `IList<TItem>` binding
2. **CSLA handles individual saves**: Batch = loop through selected items, apply action, save each
3. **Rx.NET handles notifications**: Existing `PublishCharacterUpdateAsync` pattern broadcasts updates

---

## Existing Capabilities Mapped to Batch Requirements

### Multi-Select UI

**Radzen DataGrid** (already in use - verified via [official docs](https://blazor.radzen.com/datagrid-multiple-selection)):

```csharp
// Existing pattern in codebase (e.g., UserEditModal, Items pages)
<RadzenDataGrid @ref="grid"
                Data="@characters"
                SelectionMode="DataGridSelectionMode.Multiple"
                @bind-Value="@selectedCharacters">
    <Columns>
        <RadzenDataGridColumn Width="40px">
            <Template>
                <RadzenCheckBox @bind-Value="@context.Selected" />
            </Template>
        </RadzenDataGridColumn>
        <!-- other columns -->
    </Columns>
</RadzenDataGrid>

// Selection state
IList<TableCharacterInfo> selectedCharacters = new List<TableCharacterInfo>();
```

**Alternative: Card-based multi-select** (better fits existing CharacterStatusCard pattern):

```csharp
// Already have @onclick on CharacterStatusCard
// Add selection tracking via HashSet<int> selectedCharacterIds
// Add visual indicator (CSS class) when character.CharacterId in selectedCharacterIds
```

### Batch Action Execution

**Existing pattern** from CharacterDetailGmActions.razor:

```csharp
// Single character action (lines 448-549)
private async Task ExecuteApply()
{
    // 1. Modify character
    // 2. Save via data portal
    await characterPortal.UpdateAsync(Character);

    // 3. Publish update for real-time sync
    await TimeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
    {
        CharacterId = Character.Id,
        UpdateType = CharacterUpdateType.HealthChanged,
        CampaignId = TableId.ToString(),
        Description = "Damage applied"
    });
}
```

**Batch extension** (new pattern to implement):

```csharp
private async Task ExecuteBatchAction(Func<CharacterEdit, Task> action)
{
    var results = new BatchActionResult();

    foreach (var charInfo in selectedCharacters)
    {
        try
        {
            var character = await characterPortal.FetchAsync(charInfo.CharacterId);
            await action(character);
            await characterPortal.UpdateAsync(character);

            await TimeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
            {
                CharacterId = character.Id,
                UpdateType = CharacterUpdateType.General,
                CampaignId = TableId.ToString(),
                Description = "Batch action applied"
            });

            results.Succeeded.Add(charInfo.CharacterName);
        }
        catch (Exception ex)
        {
            results.Failed.Add((charInfo.CharacterName, ex.Message));
        }
    }

    ShowBatchFeedback(results);
}
```

### Real-Time Notifications

**Existing InMemoryMessageBus** handles broadcasting:

```csharp
// Already wired up (GameMechanics.Messaging.InMemory)
// Subject<CharacterUpdateMessage> _characterUpdates
// GmTable subscribes via OnCharacterUpdateReceived
// No changes needed - just call PublishCharacterUpdateAsync per character
```

---

## What NOT to Add (and Why)

| Don't Add | Why Not |
|-----------|---------|
| **State management library** (Fluxor, Redux) | Overkill - HashSet<int> for selection state is sufficient |
| **MediatR / CQRS** | Overkill - direct async/await with existing data portal is simpler |
| **Background job framework** (Hangfire) | Batch ops are synchronous UI operations, not background jobs |
| **New SignalR features** | Existing Rx.NET InMemoryMessageBus is sufficient for single-server |
| **Bulk update stored procedures** | CSLA pattern is per-object saves; gaming data volumes don't warrant bulk ops |

---

## Integration Points with Existing Stack

### GmTable.razor Integration

Current structure (verified from codebase):

```
GmTable.razor
├── playerCharacters collection (IEnumerable<TableCharacterInfo>)
├── tableNpcs collection (filtered by IsNpc, Disposition, Visibility)
├── CharacterStatusCard rendering (per character)
├── CharacterDetailModal (single character detail/actions)
└── TimeEventPublisher (real-time updates)
```

Batch integration adds:

```
GmTable.razor (enhanced)
├── selectedCharacterIds (HashSet<int>) - NEW
├── BatchActionToolbar component - NEW
│   ├── Selected count display
│   ├── Select All / Clear buttons
│   └── Action buttons (damage, heal, effect, visibility, dismiss)
├── CharacterStatusCard (enhanced with selection checkbox)
└── BatchActionModal / BatchActionFeedback components - NEW
```

### CharacterStatusCard Enhancement

Current:
- Takes `Character`, `OnClick`, `IsSelected` parameters
- `IsSelected` determines border highlight

Enhanced:
- Add `OnSelectionChanged` EventCallback for checkbox
- Checkbox in corner for multi-select mode
- Maintain existing click behavior (opens detail modal)

### CSLA Data Portal Pattern

**Verified pattern** (from CharacterEdit.cs, CharacterDetailGmActions.razor):

```csharp
// Fetch
var character = await characterPortal.FetchAsync(characterId);

// Modify
character.Fatigue.PendingDamage += amount;

// Save
await characterPortal.UpdateAsync(character);

// Notify
await TimeEventPublisher.PublishCharacterUpdateAsync(...);
```

**Batch = iterate this pattern**. No transactional batch save needed - each character save is independent. Partial success is acceptable per requirements.

---

## Batch Action Result Handling

### Result Tracking (New Pattern)

```csharp
public class BatchActionResult
{
    public List<string> Succeeded { get; } = new();
    public List<(string CharacterName, string Error)> Failed { get; } = new();

    public int Total => Succeeded.Count + Failed.Count;
    public bool HasFailures => Failed.Any();
    public bool AllSucceeded => !HasFailures;
}
```

### Feedback Display Pattern

Radzen's existing notification/dialog can show results:

```csharp
// Use DialogService (already injected in GmTable/modals)
if (result.AllSucceeded)
{
    // Toast notification
    NotificationService.Notify(NotificationSeverity.Success,
        $"Applied to {result.Total} characters");
}
else
{
    // Modal with details
    await DialogService.OpenAsync<BatchResultModal>(
        "Batch Action Results",
        new Dictionary<string, object> { ["Result"] = result });
}
```

---

## Verified Library Features

### Radzen.Blazor Multi-Select (HIGH Confidence)

Source: [Official documentation](https://blazor.radzen.com/datagrid-multiple-selection), [API docs](https://blazor.radzen.com/docs/api/Radzen.Blazor.RadzenDataGrid-1.html)

| Feature | Support | Notes |
|---------|---------|-------|
| SelectionMode.Multiple | Yes | `DataGridSelectionMode.Multiple` enum |
| @bind-Value for IList<T> | Yes | Two-way binding to selection collection |
| Header checkbox (select all) | Yes | Built-in with checkbox column template |
| SelectedItemsChanged event | Yes | EventCallback<IList<TItem>> |
| Programmatic selection | Yes | SelectRow(item) method |

### CSLA.NET Batch Pattern (HIGH Confidence)

Source: Existing codebase patterns, [CSLA docs](https://github.com/MarimerLLC/csla)

| Operation | Pattern | Notes |
|-----------|---------|-------|
| Fetch multiple | Loop with FetchAsync | No bulk fetch needed for small counts |
| Modify | Direct property sets | Dirty tracking automatic |
| Save individual | UpdateAsync per object | Standard CSLA pattern |
| Error handling | Try/catch per save | Partial success supported |

### Rx.NET Messaging (HIGH Confidence)

Source: Existing InMemoryMessageBus.cs in codebase

| Feature | Support | Notes |
|---------|---------|-------|
| Publish per character | Yes | `PublishCharacterUpdate(message)` |
| Multiple rapid publishes | Yes | Subject handles sequential OnNext calls |
| Subscriber filtering | Yes | Subscribers filter by CampaignId/CharacterId |

---

## Summary: Stack Decision

| Aspect | Decision | Rationale |
|--------|----------|-----------|
| UI multi-select | Use Radzen DataGrid OR enhance CharacterStatusCard | Both patterns proven in codebase |
| Selection state | HashSet<int> in component | Simple, no library needed |
| Batch execution | Loop + existing CSLA pattern | Per-object save is idiomatic CSLA |
| Real-time sync | Existing PublishCharacterUpdateAsync | One call per character |
| Result feedback | Radzen DialogService/NotificationService | Already in use |
| Error handling | Try/catch with aggregated results | Partial success acceptable |

**No new packages. No version changes. Implementation only.**

---

## Sources

- [Radzen DataGrid Multiple Selection](https://blazor.radzen.com/datagrid-multiple-selection) - Official documentation
- [Radzen DataGrid API](https://blazor.radzen.com/docs/api/Radzen.Blazor.RadzenDataGrid-1.html) - SelectionMode, Value binding
- Existing codebase: `CharacterDetailGmActions.razor`, `GmTable.razor`, `InMemoryMessageBus.cs`
- Project files: `Threa.csproj`, `Threa.Client.csproj`, `GameMechanics.Messaging.InMemory.csproj`
