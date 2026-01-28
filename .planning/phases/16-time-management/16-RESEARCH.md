# Phase 16: Time Management - Research

**Researched:** 2026-01-27
**Domain:** GM time control, round mode, character time processing, real-time UI updates
**Confidence:** HIGH

## Summary

Phase 16 implements GM time controls with multiple increments and a "rounds mode" toggle. The existing codebase already has substantial infrastructure: `TimeManager`, `TimeEventType` enum, messaging infrastructure via Rx.NET (`ITimeEventPublisher`/`ITimeEventSubscriber`), and the `GmTable.razor` page with basic time controls.

The primary work involves:
1. **UI Refactoring**: Reorganizing time controls into a dedicated right panel with context-aware button display
2. **Mode Toggle Enhancement**: Adding explicit "Start Combat" / "End Combat" toggle with visual indicators
3. **Character Processing Integration**: Ensuring time advancement properly triggers character updates via messaging
4. **Player-side Indicator**: Adding "In Rounds" badge to the Play page

The architecture already supports most requirements. Key existing components:
- `TableEdit.cs`: Tracks `IsInCombat`, `CurrentRound`, `CurrentTimeSeconds`, `StartTimeSeconds`
- `TimeEventMessage`, `CombatStateMessage`: Message types for broadcasting time events
- `InMemoryMessageBus`: Rx.NET-based pub/sub for real-time updates
- `CharacterEdit.EndOfRound()`: Processes pending damage/healing, effect expiration, AP recovery

**Primary recommendation:** Refactor the existing `GmTable.razor` time controls section into the decided layout pattern, leveraging the established messaging infrastructure.

## Standard Stack

The existing codebase defines the technology choices. No new libraries needed.

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Rx.NET | Existing | Pub/sub messaging | Already in use for `InMemoryMessageBus` |
| Radzen.Blazor | 8.4.2 | UI components | Already in use for dialogs and UI |
| CSLA.NET | 9.1.0 | Business objects | Core architecture pattern |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Bootstrap 5 | Existing | CSS/layout | Grid system, badges, buttons |
| Bootstrap Icons | Existing | Icons | Visual indicators |

### Alternatives Considered
None - the stack is established by the project.

**Installation:**
No new packages required.

## Architecture Patterns

### Existing Time System Components

```
GameMechanics/
├── Time/
│   ├── TimeManager.cs           # Orchestrates time advancement
│   ├── RoundManager.cs          # Combat round tracking
│   ├── TimeEventType.cs         # Enum: EndOfRound, EndOfMinute, etc.
│   ├── GameTimeFormatter.cs     # Display formatting
│   └── [Cooldown classes]       # Action cooldowns

GameMechanics/Messaging/
├── TimeMessages.cs              # Message DTOs
├── ITimeEventPublisher.cs       # Publisher interface
└── ITimeEventSubscriber.cs      # Subscriber interface

GameMechanics.Messaging.InMemory/
├── InMemoryMessageBus.cs        # Rx.NET subjects
├── InMemoryTimeEventPublisher.cs
└── InMemoryTimeEventSubscriber.cs

Threa.Client/Components/Pages/GamePlay/
├── GmTable.razor                # GM dashboard (existing)
└── Play.razor                   # Player page (existing)
```

### Time Flow Pattern

```
GM Action (GmTable.razor)
    |
    v
TableEdit.AdvanceRound() / AdvanceTime() / EnterCombat()
    |
    v
Save via Data Portal
    |
    v
Publish via ITimeEventPublisher
    |
    v
InMemoryMessageBus (Rx.NET Subject)
    |
    v
ITimeEventSubscriber (all connected clients)
    |
    v
OnTimeEventReceived handler
    |
    v
CharacterEdit.EndOfRound() + Save
    |
    v
StateHasChanged() - UI refresh
```

### Recommended UI Structure for Time Panel

```
Right Panel (col-md-3 or similar width)
├── Header: "TIME CONTROLS"
├── Current Time Display
│   └── GameTimeFormatter.FormatCompact(table.CurrentTimeSeconds)
├── Combat Mode Section
│   ├── Badge: "In Rounds: Round X" (red, when active)
│   └── Toggle Button: "Start Combat" / "End Combat"
└── Time Buttons Section (context-aware)
    ├── [If InCombat]: "+1 Round" only
    └── [If NOT InCombat]: "+1 Min", "+10 Min", "+1 Hour", "+1 Day", "+1 Week"
```

### Anti-Patterns to Avoid
- **Disabled Buttons Instead of Hidden**: The decision specifies buttons should be hidden when not applicable for current mode, not disabled. This keeps the UI clean.
- **Processing in UI Thread**: All character processing should be async with proper `InvokeAsync` wrapping for state changes.
- **Missing Message Propagation**: Every time change must publish a message. The existing code already does this correctly.

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Time formatting | Custom string formatting | `GameTimeFormatter.FormatCompact()` | Already handles days/hours/minutes consistently |
| Message broadcasting | Custom event system | `InMemoryMessageBus` + `ITimeEventPublisher` | Rx.NET provides proper pub/sub with disposal |
| Round/time calculations | Manual math | `TimeState` class | Constants like `RoundsPerMinute` are defined |
| Character state updates | Direct property access | `CharacterEdit.EndOfRound()` | Handles all pools and effects correctly |

**Key insight:** The existing infrastructure handles the hard parts. The phase is primarily UI reorganization and proper message publishing.

## Common Pitfalls

### Pitfall 1: Race Condition on Character Updates
**What goes wrong:** GM advances time, characters process locally, but UI shows stale data.
**Why it happens:** The 500ms delay before refreshing character list (existing pattern from v1.2 decision) may not be sufficient.
**How to avoid:** The current code has a 500ms delay in `OnTimeEventReceived`. This is acceptable. If issues arise, could increase delay or add retry logic.
**Warning signs:** Character cards show old values after time advancement.

### Pitfall 2: Missing StateHasChanged in Event Handlers
**What goes wrong:** UI doesn't update after receiving time events.
**Why it happens:** Rx.NET callbacks don't automatically trigger Blazor re-render.
**How to avoid:** Always wrap event handler logic in `InvokeAsync()` and call `StateHasChanged()`.
**Warning signs:** UI only updates on manual interaction.

### Pitfall 3: Combat Mode Button State Mismatch
**What goes wrong:** "Start Combat" button shows when already in combat.
**Why it happens:** Local state diverges from server state after network operations.
**How to avoid:** Always re-read `table.IsInCombat` from the business object after save operations.
**Warning signs:** Button label doesn't match header badge.

### Pitfall 4: Time Advancement Without Message Publishing
**What goes wrong:** GM advances time but players don't see effects.
**Why it happens:** Forgetting to call `TimeEventPublisher.PublishTimeEventAsync()` after saving.
**How to avoid:** Every time-changing operation must: (1) modify table, (2) save, (3) publish message.
**Warning signs:** Activity log shows advancement but character states don't change.

### Pitfall 5: Incorrect Time Event Types
**What goes wrong:** Using `EndOfRound` when advancing by minutes results in wrong processing.
**Why it happens:** Mapping time increments to event types incorrectly.
**How to avoid:** Use the existing mapping in `AdvanceTime()`:
```csharp
60 => TimeEventType.EndOfMinute
600 => TimeEventType.EndOfTurn
3600 => TimeEventType.EndOfHour
86400 => TimeEventType.EndOfDay
604800 => TimeEventType.EndOfWeek
```
**Warning signs:** Players report effects not expiring correctly.

## Code Examples

Verified patterns from the existing codebase:

### Publishing Time Events (from GmTable.razor)
```csharp
// Source: GmTable.razor lines 600-609
await TimeEventPublisher.PublishTimeEventAsync(new TimeEventMessage
{
    EventType = GameMechanics.Time.TimeEventType.EndOfRound,
    Count = 1,
    CampaignId = table.Id.ToString(),
    SourceId = "GM",
    Reason = $"Advanced to round {table.CurrentRound}"
});
```

### Combat State Toggle (from GmTable.razor)
```csharp
// Source: GmTable.razor lines 647-678
private async Task EnterCombat()
{
    if (table == null) return;
    table.EnterCombat();
    await SaveTable();
    AddLogEntry("Combat started!", ActivityCategory.Combat);

    await TimeEventPublisher.PublishCombatStateAsync(new CombatStateMessage
    {
        EnteringCombat = true,
        CampaignId = table.Id.ToString(),
        SourceId = "GM",
        EncounterName = "Combat"
    });
}
```

### Handling Time Events on Player Side (from Play.razor)
```csharp
// Source: Play.razor lines 624-659
private void OnTimeEventReceived(object? sender, TimeEventMessage e)
{
    if (table == null || e.CampaignId != table.Id.ToString()) return;

    InvokeAsync(async () =>
    {
        try
        {
            if (character != null && e.EventType == GameMechanics.Time.TimeEventType.EndOfRound)
            {
                for (int i = 0; i < e.Count; i++)
                {
                    character.EndOfRound(effectPortal);
                }
                await characterPortal.UpdateAsync(character);
                if (table != null)
                {
                    table = await tablePortal.FetchAsync(table.Id);
                }
            }
            StateHasChanged();
        }
        catch (Exception ex)
        {
            errorMessage = $"Error processing time event: {ex.Message}";
            StateHasChanged();
        }
    });
}
```

### Context-Aware Button Display Pattern
```razor
@* Recommended pattern for hiding vs. disabling *@
@if (table.IsInCombat)
{
    <button class="btn btn-primary w-100" @onclick="AdvanceRound">
        +1 Round
    </button>
}
else
{
    <button class="btn btn-outline-primary w-100" @onclick="() => AdvanceTime(60)">
        +1 Min
    </button>
    <button class="btn btn-outline-primary w-100 mt-1" @onclick="() => AdvanceTime(600)">
        +10 Min
    </button>
    @* ... etc *@
}
```

### Combat Mode Badge in Header
```razor
@* Per CONTEXT.md: Red badge for combat mode *@
@if (table.IsInCombat)
{
    <span class="badge bg-danger">In Rounds: Round @table.CurrentRound</span>
}
```

### Combat Toggle Button Pattern
```razor
@* Single button that toggles - per CONTEXT.md decision *@
@if (table.IsInCombat)
{
    <button class="btn btn-outline-success w-100" @onclick="ExitCombat">
        <i class="bi bi-peace"></i> End Combat
    </button>
}
else
{
    <button class="btn btn-danger w-100" @onclick="EnterCombat">
        <i class="bi bi-shield-exclamation"></i> Start Combat
    </button>
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Separate round/time buttons always shown | Context-aware display based on mode | Phase 16 (new) | Cleaner UI |
| Combat mode as secondary status | Explicit toggle with visible indicator | Phase 16 (new) | Clear state |

**Deprecated/outdated:**
- The existing layout shows round controls even when not in combat. Phase 16 changes this to context-aware display.
- The existing combat badge in header is small. Phase 16 makes it more prominent with "In Rounds: Round X" format.

## Key Implementation Considerations

### TIME-01 through TIME-06: Time Increment Buttons
The existing code already supports all time increments. The work is UI reorganization:
- Move buttons to dedicated right panel
- Use `w-100` for full-width stacking
- Label format: "+1 Round", "+1 Min", "+10 Min", "+1 Hour", "+1 Day", "+1 Week"

### TIME-07, TIME-08: Combat Mode Toggle
The existing `EnterCombat()` and `ExitCombat()` methods handle state changes. The work is:
- Create single toggle button that shows correct label based on `IsInCombat`
- One-click activation (no confirmation dialog per decision)
- Publish `CombatStateMessage` on state change

### TIME-09: Round Button Only in Combat Mode
Use conditional rendering: `@if (table.IsInCombat)` to show round button.

### TIME-10: Message Propagation
Already implemented. Every time operation calls the publisher. Verify all paths include publishing.

### TIME-11: Character Processing
`CharacterEdit.EndOfRound()` handles:
- `Fatigue.EndOfRound()` - Pending damage/healing application
- `Vitality.EndOfRound()` - Pending damage/healing application
- `Effects.EndOfRound()` - Effect expiration
- `ActionPoints.EndOfRound()` - AP recovery

### TIME-12: Dashboard Updates
The existing 500ms delay pattern with `RefreshCharacterListAsync()` handles this.

### TIME-13, TIME-14: Player "In Rounds" Indicator
Add badge to `Play.razor` header section:
```razor
@if (table?.IsInCombat == true)
{
    <span class="badge bg-danger">In Rounds</span>
}
```

## Open Questions

Things that couldn't be fully resolved:

1. **Change Badge Animation Timing**
   - What we know: CONTEXT.md mentions badges should "fade in with animation, auto-dismiss after 3-5 seconds"
   - What's unclear: Exact CSS animation or JavaScript implementation preference
   - Recommendation: Use CSS transitions with `opacity` and `setTimeout` for auto-dismiss

2. **"Significant" AP Recovery Threshold**
   - What we know: CONTEXT.md says show badges for "major AP recovery (not minor ticks)"
   - What's unclear: What constitutes "major" vs "minor"
   - Recommendation: Consider 3+ AP recovery as "significant" (half or more of typical max AP)

## Sources

### Primary (HIGH confidence)
- `GmTable.razor` - Existing GM dashboard implementation
- `Play.razor` - Existing player page implementation
- `TableEdit.cs` - Business object with time/combat methods
- `TimeManager.cs` - Core time management logic
- `TimeMessages.cs` - Message type definitions
- `InMemoryMessageBus.cs` - Rx.NET messaging implementation

### Secondary (MEDIUM confidence)
- `16-CONTEXT.md` - User decisions from discussion phase

### Tertiary (LOW confidence)
- None

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All libraries already in use
- Architecture: HIGH - Patterns established in codebase
- Pitfalls: HIGH - Observed from existing code patterns
- UI Layout: MEDIUM - Based on CONTEXT.md decisions (may need adjustment)

**Research date:** 2026-01-27
**Valid until:** 60 days (stable codebase, well-established patterns)
