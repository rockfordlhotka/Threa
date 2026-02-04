# Phase 27: Time & Combat Integration - Research

**Researched:** 2026-02-03
**Domain:** NPC time/combat integration, targeting system, visibility filtering
**Confidence:** HIGH

## Summary

This phase integrates NPCs into the time advancement and combat systems so they participate identically to PCs. The key finding is that **NPCs already use CharacterEdit (same as PCs) with IsNpc flag**, meaning most time/combat mechanics already apply to NPCs once they're attached to a table. The work is primarily UI integration for targeting.

Three main areas require implementation:
1. **Time advancement already processes NPCs** - TimeAdvancementService.AdvanceRoundsAsync() loads all characters via GetTableCharactersAsync() which includes NPCs (IsNpc flag on CharacterEdit). No backend changes needed.
2. **Target selection needs NPC inclusion** - Play.razor's GetAvailableTargets() currently only returns tableCharacters (raw DAL DTOs that don't include NPC visibility). Must use TableCharacterList (CSLA) which has IsNpc and VisibleToPlayers properties.
3. **TargetSelectionModal needs enhancement** - Add disposition grouping, icons, and visibility filtering.

**Primary recommendation:** Modify Play.razor to load targets via TableCharacterList (CSLA business object) instead of raw DAL calls, filter by VisibleToPlayers, group by disposition. Add disposition icons matching GM dashboard styling.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CSLA.NET | 9.1.0 | Business objects | CharacterEdit, TableCharacterInfo, TableCharacterList |
| TimeAdvancementService | existing | Time processing | Already processes all table characters |
| Radzen.Blazor | 8.4.2 | UI components | DialogService for invalidation messages |
| Bootstrap Icons | bundled | Disposition icons | skull, circle, heart icons |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| N/A | | | No new libraries needed |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| TableCharacterList | ITableDal.GetTableCharactersAsync | CSLA list has IsNpc, VisibleToPlayers, Disposition |
| Reload via CSLA | Raw DAL calls | CSLA objects maintain consistency, handle rules |

**Installation:**
No new packages needed - all dependencies in place.

## Architecture Patterns

### Recommended Project Structure

```
GameMechanics/
  Time/TimeAdvancementService.cs    # NO CHANGES - already processes all table characters
  GamePlay/TableCharacterInfo.cs    # NO CHANGES - has IsNpc, Disposition, VisibleToPlayers
  GamePlay/TableCharacterList.cs    # NO CHANGES - fetches all characters including NPCs

Threa.Client/Components/
  Pages/GamePlay/
    Play.razor                      # MODIFY: Load targets via TableCharacterList, filter, group
    TabCombat.razor                 # NO CHANGES - receives AvailableTargets from Play.razor
    Targeting/
      TargetSelectionModal.razor    # MODIFY: Add grouping, disposition icons, subheadings
```

### Pattern 1: Target Loading via TableCharacterList

**What:** Use CSLA TableCharacterList instead of raw DAL for targets
**When to use:** Play.razor GetAvailableTargets()
**Example:**
```csharp
// Current implementation (only PCs, uses raw DAL):
private List<TableCharacter> tableCharacters = new();

private async Task LoadTableCharactersAsync()
{
    if (table == null) return;
    tableCharacters = await TableDal.GetTableCharactersAsync(table.Id);
}

public List<TargetSelectionModal.TargetInfo> GetAvailableTargets()
{
    return tableCharacters
        .Where(c => c.CharacterId != character.Id)
        .Select(c => new TargetSelectionModal.TargetInfo
        {
            CharacterId = c.CharacterId,
            CharacterName = c.CharacterName,
            PlayerName = c.PlayerName,
            IsNPC = false // BUG: Always false
        })
        .ToList();
}

// REQUIRED implementation (includes NPCs with visibility filtering):
private IEnumerable<TableCharacterInfo>? tableCharacters;

private async Task LoadTableCharactersAsync()
{
    if (table == null) return;
    var charList = await characterListPortal.FetchAsync(table.Id);
    tableCharacters = charList.ToList();
}

public List<TargetSelectionModal.TargetInfo> GetAvailableTargets()
{
    if (table == null || character == null || tableCharacters == null)
        return new();

    // Filter: exclude self, include visible NPCs only
    var targets = tableCharacters
        .Where(c => c.CharacterId != character.Id)
        .Where(c => !c.IsNpc || c.VisibleToPlayers) // PCs always visible, NPCs only if revealed
        .Select(c => new TargetSelectionModal.TargetInfo
        {
            CharacterId = c.CharacterId,
            CharacterName = c.CharacterName,
            PlayerName = c.IsNpc ? null : "Player", // NPCs have no player
            IsNPC = c.IsNpc,
            Disposition = c.Disposition
        })
        .ToList();

    return targets;
}
```

### Pattern 2: NPC-First Grouped Target List

**What:** NPCs appear first, grouped by disposition with subheadings
**When to use:** TargetSelectionModal.razor
**Example:**
```razor
// Source: CONTEXT.md decision - NPCs first, grouped by disposition
@if (AvailableTargets?.Any() == true)
{
    <div class="list-group">
        @* NPCs Section - appears first *@
        @{
            var npcs = AvailableTargets.Where(t => t.IsNPC).ToList();
            var pcs = AvailableTargets.Where(t => !t.IsNPC).ToList();
        }

        @if (npcs.Any())
        {
            @* Hostile NPCs *@
            @foreach (var target in npcs.Where(n => n.Disposition == NpcDisposition.Hostile))
            {
                <TargetListItem Target="@target"
                               IsSelected="@(selectedTargetId == target.CharacterId)"
                               OnSelect="SelectTarget" />
            }

            @* Neutral NPCs *@
            @foreach (var target in npcs.Where(n => n.Disposition == NpcDisposition.Neutral))
            {
                <TargetListItem Target="@target"
                               IsSelected="@(selectedTargetId == target.CharacterId)"
                               OnSelect="SelectTarget" />
            }

            @* Friendly NPCs *@
            @foreach (var target in npcs.Where(n => n.Disposition == NpcDisposition.Friendly))
            {
                <TargetListItem Target="@target"
                               IsSelected="@(selectedTargetId == target.CharacterId)"
                               OnSelect="SelectTarget" />
            }
        }

        @* PCs Section - below NPCs *@
        @if (pcs.Any() && npcs.Any())
        {
            <div class="list-group-item bg-secondary text-white py-1">
                <small>Players</small>
            </div>
        }
        @foreach (var target in pcs)
        {
            <TargetListItem Target="@target"
                           IsSelected="@(selectedTargetId == target.CharacterId)"
                           OnSelect="SelectTarget" />
        }
    </div>
}
```

### Pattern 3: Disposition Icon Mapping

**What:** Consistent icons matching GM dashboard
**When to use:** TargetSelectionModal, any NPC display
**Example:**
```razor
// Source: CONTEXT.md + GM dashboard styling from GmTable.razor
@switch (target.Disposition)
{
    case NpcDisposition.Hostile:
        <i class="bi bi-skull text-danger me-1" title="Hostile"></i>
        break;
    case NpcDisposition.Neutral:
        <i class="bi bi-circle text-secondary me-1" title="Neutral"></i>
        break;
    case NpcDisposition.Friendly:
        <i class="bi bi-heart text-success me-1" title="Friendly"></i>
        break;
}
```

### Pattern 4: Target Invalidation Handling

**What:** Handle case where target is hidden mid-action
**When to use:** When GM hides NPC during targeting
**Example:**
```csharp
// Source: CONTEXT.md decision - action invalidated, player picks new target
// In Play.razor, monitor for table character updates

private void OnTableCharacterUpdate(...)
{
    // Reload table characters
    await LoadTableCharactersAsync();

    // Check if current targeting interaction has valid target
    if (currentTargetingInteraction != null)
    {
        var targetId = currentTargetingInteraction.DefenderId;
        var target = tableCharacters?.FirstOrDefault(c => c.CharacterId == targetId);

        // If target no longer visible (hidden) or removed
        if (target == null || (target.IsNpc && !target.VisibleToPlayers))
        {
            // Invalidate targeting
            await TargetingManager.CancelAsync(
                currentTargetingInteraction.InteractionId,
                "Target is no longer available");

            // Show notification (Claude's discretion: toast)
            ShowToast("Target Hidden", "Your target is no longer available. Please select a new target.", "warning");

            showTargetingModal = false;
            currentTargetingInteraction = null;
        }
    }
}
```

### Pattern 5: Reveal Activity Log Message

**What:** Activity log entry when NPC is revealed
**When to use:** GM reveals hidden NPC
**Example:**
```csharp
// Source: CONTEXT.md - "[NPC Name] appears!" (name only, no disposition)
// In GmTable.razor RevealNpc method

private async Task RevealNpc(TableCharacterInfo npc)
{
    // Set visible
    var character = await characterPortal.FetchAsync(npc.CharacterId);
    character.VisibleToPlayers = true;
    await characterPortal.UpdateAsync(character);

    // Log reveal - name only, no disposition mentioned
    ActivityLog.Publish(table.Id, $"{npc.CharacterName} appears!", "GM", ActivityCategory.Announcement);

    // Notify player clients
    await TimeEventPublisher.PublishCharacterUpdateAsync(new CharacterUpdateMessage
    {
        CharacterId = npc.CharacterId,
        UpdateType = CharacterUpdateType.StatusChange,
        CampaignId = table.Id.ToString()
    });
}
```

### Anti-Patterns to Avoid

- **Filtering time processing by visibility:** ALL NPCs must process time (CONTEXT.md decision). Hidden NPCs still have effects expire, AP recover.
- **Showing NPC damage amounts to players:** Combat feedback shows generic "is hit", not damage numbers (CONTEXT.md decision).
- **Separate NPC/PC counts in time summary:** Use single total "Processed X round(s) for Y character(s)" (CONTEXT.md decision).
- **Logging NPC effect changes to player feed:** NPC effect changes are GM-only (CONTEXT.md decision).

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Loading characters with NPC flag | Raw DAL query | TableCharacterList via CSLA portal | Has all properties populated |
| Time processing for NPCs | Separate NPC processing | TimeAdvancementService | Already processes all TableCharacters |
| NPC combat mechanics | New NPC combat resolver | Existing AttackResolver/DefenseResolver | CharacterEdit handles both PC and NPC |
| Target validation | Custom visibility check | TableCharacterInfo.VisibleToPlayers | Property already exists |

**Key insight:** The architectural decision in v1.5 to use CharacterEdit for NPCs (with IsNpc flag) means almost all combat/time infrastructure already works. This phase is UI integration, not new mechanics.

## Common Pitfalls

### Pitfall 1: Using Raw DAL Instead of CSLA for Targets
**What goes wrong:** GetTableCharactersAsync returns TableCharacter DTOs which don't have IsNpc, Disposition, VisibleToPlayers
**Why it happens:** Copy-paste from existing code that predates NPC system
**How to avoid:** Always use TableCharacterList (CSLA) for anything needing NPC properties
**Warning signs:** IsNPC always false, no disposition icons

### Pitfall 2: Filtering NPCs from Time Processing
**What goes wrong:** Hidden NPCs don't have effects expire, AP recover
**Why it happens:** Mistakenly thinking hidden means inactive
**How to avoid:** TimeAdvancementService processes ALL TableCharacters - no changes needed
**Warning signs:** Hidden NPCs frozen in state when revealed

### Pitfall 3: Exposing NPC Damage to Players
**What goes wrong:** Players see exact damage dealt to NPCs
**Why it happens:** Reusing PC damage display logic
**How to avoid:** Combat feedback must check target.IsNpc and show generic messages
**Warning signs:** Activity log shows "Goblin takes 5 FAT damage"

### Pitfall 4: Not Refreshing Targets on Visibility Change
**What goes wrong:** Hidden NPC remains in target list after GM hides
**Why it happens:** Target list cached, not refreshed on character update
**How to avoid:** Subscribe to CharacterUpdateReceived, reload targets
**Warning signs:** Player can target NPC that GM just hid

### Pitfall 5: Disposition Change Not Reflected
**What goes wrong:** NPC shows old disposition in target list after GM changes it
**Why it happens:** Target list not refreshed on character update
**How to avoid:** Reload TableCharacterList when CharacterUpdateReceived fires
**Warning signs:** Hostile NPC shows friendly icon after disposition change

## Code Examples

Verified patterns from existing codebase:

### TimeAdvancementService Already Processes NPCs
```csharp
// Source: GameMechanics/Time/TimeAdvancementService.cs lines 118-126
// No filtering - processes ALL characters at table (PCs and NPCs)
var tableCharacters = await _tableDal.GetTableCharactersAsync(tableId);

foreach (var tc in tableCharacters)
{
    // Load the character (CharacterEdit - works for both PC and NPC)
    var character = await _characterPortal.FetchAsync(tc.CharacterId);

    // Process end-of-round for each round
    for (int i = 0; i < roundCount; i++)
    {
        character.EndOfRound(_effectPortal);
    }

    // Save the updated character
    await _characterPortal.UpdateAsync(character);
    result.UpdatedCharacterIds.Add(tc.CharacterId);
}
```

### TableCharacterInfo Has NPC Properties
```csharp
// Source: GameMechanics/GamePlay/TableCharacterInfo.cs lines 203-253
public static readonly PropertyInfo<bool> IsNpcProperty = RegisterProperty<bool>(nameof(IsNpc));
public bool IsNpc
{
    get => GetProperty(IsNpcProperty);
    private set => LoadProperty(IsNpcProperty, value);
}

public static readonly PropertyInfo<NpcDisposition> DispositionProperty = RegisterProperty<NpcDisposition>(nameof(Disposition));
public NpcDisposition Disposition
{
    get => GetProperty(DispositionProperty);
    private set => LoadProperty(DispositionProperty, value);
}

public static readonly PropertyInfo<bool> VisibleToPlayersProperty = RegisterProperty<bool>(nameof(VisibleToPlayers));
public bool VisibleToPlayers
{
    get => GetProperty(VisibleToPlayersProperty);
    private set => LoadProperty(VisibleToPlayersProperty, value);
}
```

### TargetInfo Already Has IsNPC Property
```csharp
// Source: Threa.Client/Components/Pages/GamePlay/Targeting/TargetSelectionModal.razor lines 97-103
public class TargetInfo
{
    public int CharacterId { get; init; }
    public string CharacterName { get; init; } = string.Empty;
    public string? PlayerName { get; init; }
    public bool IsNPC { get; init; }
    // NEEDS: Add Disposition property
}
```

### GM Dashboard NPC Grouping Pattern
```csharp
// Source: GmTable.razor lines 511-530 - Use same pattern for targeting
private IEnumerable<TableCharacterInfo> hostileNpcs =>
    tableNpcs.Where(c => c.VisibleToPlayers && c.Disposition == NpcDisposition.Hostile);

private IEnumerable<TableCharacterInfo> neutralNpcs =>
    tableNpcs.Where(c => c.VisibleToPlayers && c.Disposition == NpcDisposition.Neutral);

private IEnumerable<TableCharacterInfo> friendlyNpcs =>
    tableNpcs.Where(c => c.VisibleToPlayers && c.Disposition == NpcDisposition.Friendly);
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Separate NPC entity | CharacterEdit with IsNpc flag | v1.5 architecture | NPCs inherit all PC mechanics |
| TableNpc DTO | TableCharacterInfo | Phase 25 | NPCs in same list as PCs |
| Manual NPC time processing | TimeAdvancementService | Phase 23 | Automatic via table attachment |

**Deprecated/outdated:**
- TableNpc DTO still exists but not used for spawned NPCs (they use Character/CharacterEdit)
- Old NPC stat-block approach replaced by full character objects

## Open Questions

Things that couldn't be fully resolved:

1. **Target invalidation timing**
   - What we know: GM can hide NPC, player's targeting should invalidate
   - What's unclear: Exact timing of CharacterUpdateReceived delivery
   - Recommendation: Check target validity on every render cycle, not just event

2. **Combat feedback message formatting**
   - What we know: Generic messages for NPC damage ("is hit")
   - What's unclear: Where exactly to implement filtering (Play.razor vs TabCombat vs TargetingModal)
   - Recommendation: Filter in ActivityLog publish or subscribe based on IsNpc

## Sources

### Primary (HIGH confidence)
- GameMechanics/Time/TimeAdvancementService.cs - Time processing implementation
- GameMechanics/GamePlay/TableCharacterInfo.cs - NPC properties (IsNpc, Disposition, VisibleToPlayers)
- GameMechanics/GamePlay/TableCharacterList.cs - CSLA list with all characters
- Threa.Client/Components/Pages/GamePlay/Play.razor - Current targeting implementation
- Threa.Client/Components/Pages/GamePlay/Targeting/TargetSelectionModal.razor - Current TargetInfo class
- Threa.Client/Components/Pages/GamePlay/GmTable.razor - NPC grouping pattern

### Secondary (MEDIUM confidence)
- 27-CONTEXT.md - User decisions on targeting, visibility, combat feedback
- design/COMBAT_SYSTEM.md - Combat rules (same for PC/NPC)
- design/TIME_SYSTEM.md - Time processing rules (same for PC/NPC)

### Tertiary (LOW confidence)
- None - all findings verified from codebase

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All components exist in codebase
- Architecture: HIGH - Clear patterns from GmTable.razor and existing code
- Pitfalls: HIGH - Derived from codebase analysis and CONTEXT.md decisions

**Research date:** 2026-02-03
**Valid until:** Stable - core architecture unlikely to change
