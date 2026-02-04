# Feature Research: Batch Character Actions

**Domain:** GM Batch Operations for TTRPG Character Management
**Researched:** 2026-02-04
**Confidence:** MEDIUM (based on VTT ecosystem patterns, SaaS bulk action UX research, existing Threa codebase)

---

## Feature Landscape

### Table Stakes (Users Expect These)

Features GMs assume exist when told "batch operations" are available. Missing = feature feels broken or incomplete.

| Feature | Why Expected | Complexity | Dependencies | Notes |
|---------|--------------|------------|--------------|-------|
| **Multi-Character Selection** | Core premise of batch operations; GMs must select who to affect | LOW | Existing CharacterStatusCard, NpcStatusCard | Checkbox or click-to-select pattern; selection state in parent component |
| **Selection Count Display** | GMs need to know how many characters selected before acting | LOW | Multi-Character Selection | "3 selected" badge; standard bulk action pattern per [Eleken](https://www.eleken.co/blog-posts/bulk-actions-ux) |
| **Select All / Deselect All** | Fast selection for large groups; expected in any bulk UI per [PatternFly](https://www.patternfly.org/patterns/bulk-selection/) | LOW | Multi-Character Selection | Header checkbox or button; scope to visible section (NPCs/PCs) |
| **Batch Damage Application** | Apply same damage to all selected; primary use case ("fireball hits everyone") | MEDIUM | Selection + CharacterDetailGmActions patterns | Reuse existing ApplyToPool logic, loop over selected |
| **Batch Healing Application** | Apply same healing to all selected; symmetric with damage | MEDIUM | Selection + CharacterDetailGmActions patterns | Same infrastructure as damage |
| **Batch Visibility Toggle** | Reveal/hide multiple NPCs at once for dramatic encounters | LOW | Selection + existing VisibleToPlayers field | NPCs only; single button in action bar |
| **Clear Feedback on Success/Failure** | GMs must know what happened; partial success common with batch ops | MEDIUM | Batch action results | Toast/alert showing "Applied 3 FAT damage to 5 characters" |
| **Contextual Action Bar** | Actions appear when selection exists; disappear when cleared | LOW | Multi-Character Selection | Slide-in bar pattern; [Eleken bulk actions UX](https://www.eleken.co/blog-posts/bulk-actions-ux) |

### Differentiators (Competitive Advantage)

Features that set Threa apart from basic VTT batch implementations. Valuable but not expected.

| Feature | Value Proposition | Complexity | Dependencies | Notes |
|---------|-------------------|------------|--------------|-------|
| **Batch Effect Add** | Apply same effect (Poisoned, Blessed, etc.) to group | MEDIUM | Selection + EffectManagementModal patterns | Rare in VTTs; [Multi Token Status](https://foundryvtt.com/packages/multistatus) Foundry module provides this |
| **Batch Effect Remove** | Remove specific effect from all selected | MEDIUM | Selection + Effects system | "Remove Stunned from all selected"; cleanup after combat phase |
| **Cross-Type Selection** | Select both NPCs and PCs together for universal effects | LOW | Unified selection state across sections | AoE damage affects everyone; most tools separate PC/NPC |
| **Batch Dismiss/Archive NPCs** | Remove multiple defeated NPCs at once | LOW | Selection + existing NPC removal | Combat cleanup; "dismiss all dead goblins" |
| **Partial Success Reporting** | Show exactly which characters succeeded/failed and why | MEDIUM | Detailed batch result tracking | "4/5 applied; Goblin 3 already at max FAT" per [Google AIP-233](https://google.aip.dev/233) |
| **Selection Persistence Across Tabs** | Selection survives GM switching between dashboard sections | LOW | Global selection state | Prevents frustration of losing selection |
| **Keyboard Shortcuts** | Ctrl+A select all, Escape to deselect | LOW | JavaScript keybindings | Power user efficiency |

### Anti-Features (Deliberately NOT Building)

Features that seem useful but create problems or scope creep.

| Anti-Feature | Why Requested | Why Problematic | Alternative |
|--------------|---------------|-----------------|-------------|
| **Individual Amount Per Character** | "Different damage to each target" | Defeats purpose of batch; complex UI; slow workflow | Apply batch, then adjust individuals via modal |
| **Batch Combat Actions (Attack/Defend)** | "All goblins attack" | Combat resolution requires individual rolls, targets, defenses; too complex for batch | Roll initiative helpers instead; individual actions |
| **Undo Batch Operations** | "Oops, wrong group" | Requires transaction log, complex state management; increases technical debt | Confirmation dialog before apply; individual corrections after |
| **Batch Wound Management** | "Add wound to all selected" | Wounds require location, severity, source; too many parameters for batch | Open wound modal individually |
| **Batch Inventory Changes** | "Give item to all" | Item templates, stacking rules, container logic; different domain | Use item distribution separately |
| **Smart Grouping (Auto-Select Same Type)** | "Select all Goblins" | Requires categorization logic; name parsing unreliable; scope creep | Manual selection; disposition grouping sufficient |
| **Saved Selection Groups** | "Save this selection for later" | Feature creep; rarely used; adds complexity | Use disposition groups (Hostile/Neutral/Friendly) as implicit groups |
| **Batch AP Modification** | "Give 2 AP to all" | AP follows strict rules; direct manipulation breaks game model | AP recovers via time system; not a batch operation |

---

## Feature Dependencies

```
Multi-Character Selection (Foundation)
    |
    +---> Selection Count Display
    |
    +---> Select All / Deselect All
    |
    +---> Contextual Action Bar
              |
              +---> Batch Damage Application
              |         |
              |         +---> Partial Success Reporting
              |
              +---> Batch Healing Application
              |         |
              |         +---> Partial Success Reporting
              |
              +---> Batch Visibility Toggle (NPCs only)
              |
              +---> Batch Dismiss/Archive (NPCs only)
              |
              +---> Batch Effect Add (differentiator)
              |
              +---> Batch Effect Remove (differentiator)

Cross-Type Selection ──enhances──> Multi-Character Selection

Selection Persistence ──enhances──> Multi-Character Selection

Keyboard Shortcuts ──enhances──> Selection workflow
```

### Dependency Notes

- **All batch actions require Multi-Character Selection:** No batch without selection infrastructure
- **Contextual Action Bar requires selection:** Actions only show when characters selected
- **Partial Success Reporting enhances all batch actions:** Optional but valuable for feedback
- **Effect operations require base damage/healing working:** Build simpler actions first

---

## Existing Infrastructure Analysis

Based on `GmTable.razor` and `CharacterDetailGmActions.razor`:

### What Already Exists

| Component/Pattern | Current State | Batch Reuse Potential |
|-------------------|---------------|----------------------|
| `CharacterStatusCard` | Click opens modal | Add checkbox; add selected state styling |
| `NpcStatusCard` | Click opens modal | Same modification needed |
| `CharacterDetailGmActions.ApplyToPool()` | Single character damage/healing | Extract logic for batch loop |
| `ToggleVisibility()` | Single NPC show/hide | Simple batch adaptation |
| `RefreshCharacterListAsync()` | Reloads character list | Call once after batch completes |
| `TimeEventPublisher.PublishCharacterUpdateAsync()` | Notifies clients of changes | Batch publish or loop |
| Disposition groups (Hostile/Neutral/Friendly) | Visual grouping | Natural selection groups |
| Hidden NPC section | Collapsible section | Selection should include hidden NPCs |

### New Components Needed

| Component | Purpose | Complexity |
|-----------|---------|------------|
| `BatchSelectionState` | Track selected character IDs | LOW |
| `BatchActionBar` | Floating/fixed bar with batch actions | MEDIUM |
| `BatchDamageModal` | Simple form for amount + pool type | LOW |
| `BatchEffectModal` | Select effect to add/remove from list | MEDIUM |
| `BatchResultToast` | Display success/partial/failure feedback | LOW |

---

## MVP Definition

### Launch With (v1.6)

Minimum viable batch operations:

- [ ] **Multi-Character Selection** - Checkboxes on status cards
- [ ] **Selection Count Display** - "X selected" in UI
- [ ] **Select All/Deselect All** - Per section (PCs, Hostile NPCs, etc.)
- [ ] **Batch Damage Application** - Apply FAT or VIT damage to all selected
- [ ] **Batch Healing Application** - Apply FAT or VIT healing to all selected
- [ ] **Batch Visibility Toggle** - Reveal/hide selected NPCs
- [ ] **Batch Dismiss/Archive** - Remove selected NPCs from table
- [ ] **Clear Success Feedback** - Toast showing action result

### Add After Validation (v1.6.x)

Features to add once core batch operations are working:

- [ ] **Batch Effect Add** - Apply predefined effect to all selected
- [ ] **Batch Effect Remove** - Remove specific effect from all selected
- [ ] **Partial Success Reporting** - Detailed per-character results
- [ ] **Keyboard Shortcuts** - Ctrl+A, Escape bindings
- [ ] **Cross-Type Selection** - Select PCs and NPCs together

### Future Consideration (v2+)

Features to defer until product-market fit is established:

- [ ] **Selection Persistence** - Maintain selection across page navigation
- [ ] **Custom Quick Actions** - GM-defined batch macros
- [ ] **Batch Initiative Roll** - Roll initiative for all selected

---

## Feature Prioritization Matrix

| Feature | User Value | Implementation Cost | Priority |
|---------|------------|---------------------|----------|
| Multi-Character Selection | HIGH | LOW | P1 |
| Selection Count Display | MEDIUM | LOW | P1 |
| Select All/Deselect All | HIGH | LOW | P1 |
| Batch Damage Application | HIGH | MEDIUM | P1 |
| Batch Healing Application | HIGH | MEDIUM | P1 |
| Batch Visibility Toggle | HIGH | LOW | P1 |
| Batch Dismiss/Archive | MEDIUM | LOW | P1 |
| Clear Success Feedback | HIGH | LOW | P1 |
| Contextual Action Bar | MEDIUM | MEDIUM | P1 |
| Batch Effect Add | MEDIUM | MEDIUM | P2 |
| Batch Effect Remove | MEDIUM | MEDIUM | P2 |
| Partial Success Reporting | LOW | MEDIUM | P2 |
| Cross-Type Selection | MEDIUM | LOW | P2 |
| Keyboard Shortcuts | LOW | LOW | P3 |
| Selection Persistence | LOW | MEDIUM | P3 |

**Priority key:**
- P1: Must have for v1.6 launch
- P2: Should have, add in v1.6.x
- P3: Nice to have, future consideration

---

## Competitor Feature Analysis

| Feature | Foundry VTT | Roll20 | D&D Beyond | Threa Approach |
|---------|-------------|--------|------------|----------------|
| **Multi-select tokens** | Ctrl+click or drag box | Ctrl+click or drag box | N/A (character sheets) | Checkbox on cards |
| **Batch damage** | Via modules (TokenMod) | Via API scripts (ApplyDamage) | Not available | Native first-class feature |
| **Batch status effects** | Multi Token Status module | Not native | Not available | Native batch effect modal |
| **Batch visibility** | GM Layer toggle | GM Layer toggle | Not available | Batch visibility button |
| **Selection UI** | Token-based (map) | Token-based (map) | N/A | Card-based (dashboard) |
| **Partial success** | Module-dependent | Script-dependent | N/A | Built-in reporting |

**Key Insight:** VTTs handle batch operations through map/token selection. Threa's card-based dashboard is different - we need checkbox selection rather than spatial selection. This is actually an advantage for accessibility and mobile use.

### Sources for Competitor Analysis

- [Multi Token Status Module](https://foundryvtt.com/packages/multistatus) - Foundry VTT
- [Roll20 API Scripts](https://wiki.roll20.net/Mod:Script_Index) - TokenMod, ApplyDamage, GroupCheck
- [D&D Beyond Encounter Builder Tutorial](https://www.dndbeyond.com/posts/1135-tutorial-how-to-build-encounters-and-run-them-on-d)
- [Game Master's Grimoire](https://extensions.owlbear.rodeo/hp-tracker) - Owlbear Rodeo extension

---

## UX Patterns from SaaS Research

### Bulk Action Bar Placement

Per [Eleken UX guidelines](https://www.eleken.co/blog-posts/bulk-actions-ux):
- Action bar appears at stable, obvious position
- Tied visually to selected items
- Same interactive pattern across all pages

**Recommendation for Threa:** Fixed position bar at bottom of character section when selection active. Contains action buttons and selection count.

### Selection Feedback

Per [PatternFly bulk selection](https://www.patternfly.org/patterns/bulk-selection/):
- Display selected count
- Support page-level and global scope
- Provide "Select all" and "Reset selection" options

**Recommendation for Threa:**
- Section-level selection (PCs, Hostile NPCs, Neutral NPCs, Friendly NPCs, Hidden NPCs)
- "Select All in Section" checkbox in section header
- Global "Deselect All" button in action bar

### Partial Success Handling

Per [Google AIP patterns](https://google.aip.dev/233):
- Batch operations may succeed partially
- Response should include both successes and failures
- Failed items include reason for failure

**Recommendation for Threa:**
- Return `BatchResult` with success count, failure count, and optional details
- Toast message: "Applied 3 FAT damage to 4 of 5 characters"
- Optional expansion to see which failed and why

---

## Implementation Considerations

### State Management

Selection state should live in `GmTable.razor` parent component:

```csharp
private HashSet<int> selectedCharacterIds = new();

private bool IsSelected(int characterId) => selectedCharacterIds.Contains(characterId);

private void ToggleSelection(int characterId)
{
    if (selectedCharacterIds.Contains(characterId))
        selectedCharacterIds.Remove(characterId);
    else
        selectedCharacterIds.Add(characterId);
}

private void SelectAllInSection(IEnumerable<int> characterIds)
{
    foreach (var id in characterIds)
        selectedCharacterIds.Add(id);
}

private void DeselectAll() => selectedCharacterIds.Clear();
```

### Batch Operation Pattern

```csharp
public class BatchOperationResult
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<BatchItemResult> Details { get; set; } = new();
}

public class BatchItemResult
{
    public int CharacterId { get; set; }
    public string CharacterName { get; set; }
    public bool Success { get; set; }
    public string? FailureReason { get; set; }
}
```

### Real-Time Updates

After batch operation completes:
1. Refresh character list once (not per character)
2. Publish single aggregate notification or batch of updates
3. Clear selection (optional - user preference)

---

## Risk Assessment

| Risk | Likelihood | Impact | Mitigation |
|------|------------|--------|------------|
| Scope creep into complex actions | HIGH | MEDIUM | Strict anti-features list; defer complex ops |
| Performance with many selections | LOW | MEDIUM | Batch database operations; single refresh |
| UI clutter with selection checkboxes | MEDIUM | LOW | Hide checkboxes until hover or selection mode |
| Partial failure confusion | MEDIUM | MEDIUM | Clear feedback; optional detail expansion |
| Selection state lost on navigation | MEDIUM | LOW | Accept for MVP; add persistence later |

---

## Sources

### VTT Ecosystem Research
- [Foundry VTT Multi Token Status](https://foundryvtt.com/packages/multistatus) - Batch status effect management
- [Foundry VTT Combat Tracker](https://foundryvtt.com/article/tokens/) - Token selection patterns
- [Roll20 API Scripts](https://wiki.roll20.net/Mod:Script_Index) - TokenMod, ApplyDamage
- [Roll20 Batch Operations Forum](https://app.roll20.net/forum/post/10766392/script-autobuttons-automatically-generate-damage-and-healing-buttons-for-your-clicking-pleasure)
- [D&D Beyond Encounter Builder](https://www.dndbeyond.com/posts/1135-tutorial-how-to-build-encounters-and-run-them-on-d)
- [Owlbear Rodeo Game Master's Grimoire](https://extensions.owlbear.rodeo/hp-tracker)

### UX Patterns Research
- [Eleken: Bulk Actions UX Guidelines](https://www.eleken.co/blog-posts/bulk-actions-ux) - 8 design guidelines for SaaS
- [PatternFly: Bulk Selection](https://www.patternfly.org/patterns/bulk-selection/) - Enterprise UI patterns
- [HashiCorp Helios: Table Multi-Select](https://helios.hashicorp.design/patterns/table-multi-select) - Selection scope hierarchy
- [SaaS Interface: Bulk Actions Examples](https://saasinterface.com/components/bulk-actions/) - 19 SaaS examples

### Partial Success Patterns
- [Google AIP-233: Batch Create](https://google.aip.dev/233) - Partial success API patterns
- [Google AIP-234: Batch Update](https://google.aip.dev/234) - Batch operation response patterns
- [API Catalyst: Mixed Success/Failure Responses](https://medium.com/api-catalyst/design-patterns-for-handling-mixed-success-and-failure-scenarios-in-http-200-ok-responses-07e26684f1ec)

### Existing Codebase
- `S:\src\rdl\threa\Threa\Threa.Client\Components\Pages\GamePlay\GmTable.razor` - GM dashboard
- `S:\src\rdl\threa\Threa\Threa.Client\Components\Shared\CharacterDetailGmActions.razor` - Single character actions
- `S:\src\rdl\threa\Threa\Threa.Client\Components\Shared\CharacterDetailModal.razor` - Character manipulation modal

---

*Feature research for: Batch Character Actions (v1.6)*
*Researched: 2026-02-04*
