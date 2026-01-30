# Server-Authoritative Time Processing

## Overview

Currently, when the GM advances time (rounds, minutes, hours, etc.), the time events are published to player clients, and each player's browser processes `EndOfRound` effects for their own character. This creates issues:

1. **Disconnected players** - Characters not currently logged in never process time effects
2. **Race conditions** - GM fetches character data before player saves complete
3. **Inconsistency** - Different clients may process effects differently

## Proposed Architecture

Move character time processing to the server (GM context) so all characters at a table are processed consistently.

### Current Flow

```
GM clicks "Advance Round"
    → GM publishes TimeEventMessage
    → Each connected player receives event
    → Player processes EndOfRound for their character
    → Player saves character
    → Player publishes CharacterUpdateMessage
    → GM receives update, fetches character list
```

### Proposed Flow

```
GM clicks "Advance Round"
    → GM fetches all characters at table (full CharacterEdit objects)
    → GM processes EndOfRound for EACH character
    → GM saves all characters
    → GM publishes CharacterUpdateMessage to notify player clients
    → Player clients refresh their display (no processing, just display)
```

## Implementation Steps

### Step 1: Create Table Character Processor

Create a new command object `TableTimeAdvancer` that:
- Fetches all characters attached to a table
- Processes time effects (EndOfRound, EndOfMinute, etc.) for each character
- Saves all modified characters
- Returns summary of what changed

```csharp
// GameMechanics/GamePlay/TableTimeAdvancer.cs
[Serializable]
public class TableTimeAdvancer : CommandBase<TableTimeAdvancer>
{
    public Guid TableId { get; private set; }
    public TimeEventType EventType { get; private set; }
    public int Count { get; private set; }
    
    // Results
    public List<CharacterUpdateSummary> UpdatedCharacters { get; private set; }
    
    [Execute]
    private async Task ExecuteAsync(
        [Inject] ITableDal tableDal,
        [Inject] IDataPortal<CharacterEdit> characterPortal,
        [Inject] IChildDataPortal<EffectRecord> effectPortal)
    {
        var tableCharacters = await tableDal.GetTableCharactersAsync(TableId);
        UpdatedCharacters = new List<CharacterUpdateSummary>();
        
        foreach (var tc in tableCharacters)
        {
            var character = await characterPortal.FetchAsync(tc.CharacterId);
            var beforeFat = character.Fatigue.Value;
            var beforeVit = character.Vitality.Value;
            
            // Process time effects
            for (int i = 0; i < Count; i++)
            {
                switch (EventType)
                {
                    case TimeEventType.EndOfRound:
                        character.EndOfRound(effectPortal);
                        break;
                    case TimeEventType.EndOfMinute:
                        character.EndOfMinute(effectPortal);
                        break;
                    // ... other time types
                }
            }
            
            // Save the character
            await characterPortal.UpdateAsync(character);
            
            // Track changes for notification
            UpdatedCharacters.Add(new CharacterUpdateSummary
            {
                CharacterId = tc.CharacterId,
                FatigueBefore = beforeFat,
                FatigueAfter = character.Fatigue.Value,
                VitalityBefore = beforeVit,
                VitalityAfter = character.Vitality.Value
            });
        }
    }
}
```

### Step 2: Update GmTable.razor

Modify `AdvanceRound()` and `AdvanceTime()` to use the new command:

```csharp
private async Task AdvanceRound()
{
    if (table == null) return;
    
    // Update table state
    table.AdvanceRound();
    await SaveTable();
    AddLogEntry($"Advanced to round {table.CurrentRound}", ActivityCategory.Time);
    
    // Process all characters server-side
    var advancer = await timeAdvancerPortal.CreateAsync();
    advancer.TableId = table.Id;
    advancer.EventType = TimeEventType.EndOfRound;
    advancer.Count = 1;
    advancer = await timeAdvancerPortal.ExecuteAsync(advancer);
    
    // Log results
    foreach (var update in advancer.UpdatedCharacters)
    {
        if (update.FatigueBefore != update.FatigueAfter)
        {
            AddLogEntry($"{update.CharacterName}: FAT {update.FatigueBefore} → {update.FatigueAfter}", 
                ActivityCategory.Combat);
        }
    }
    
    // Refresh local display
    await RefreshCharacterListAsync();
    StateHasChanged();
    
    // Notify player clients to refresh their displays
    await TimeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
    {
        CampaignId = table.Id.ToString(),
        UpdateType = CharacterUpdateType.TimeAdvanced,
        Description = $"Round {table.CurrentRound}"
    });
}
```

### Step 3: Update Play.razor

Simplify the time event handling to just refresh display (no processing):

```csharp
private void OnTimeEventReceived(object? sender, TimeEventMessage e)
{
    if (_disposed || table == null || e.CampaignId != table.Id.ToString()) return;

    _ = InvokeAsync(async () =>
    {
        // Just refresh - server already processed the character
        if (table != null)
        {
            table = await tablePortal.FetchAsync(table.Id);
        }
        if (character != null)
        {
            character = await characterPortal.FetchAsync(character.Id);
            await LoadEquippedItemsAsync();
        }
        StateHasChanged();
    });
}
```

## Benefits

1. **Consistency** - All characters processed the same way
2. **Offline support** - Characters react to time even when player disconnected
3. **No race conditions** - GM processes and saves before refreshing
4. **Simpler client code** - Player clients just display, don't process
5. **Authoritative server** - Game state is controlled by GM/server

## Migration Considerations

- Keep existing `EndOfRound`, `EndOfMinute` methods on `CharacterEdit`
- Remove processing logic from `Play.razor` time event handlers
- Add appropriate error handling for partial failures
- Consider batch saves for performance with many characters

## Future Enhancements

- NPC characters processed automatically (no player connection needed)
- Background time progression for sandbox play
- Undo/redo for time advancement
- Time advancement preview showing what will happen
