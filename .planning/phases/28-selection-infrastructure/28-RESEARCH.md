# Phase 28: Selection Infrastructure - Research

**Researched:** 2026-02-04
**Domain:** Blazor multi-select UI state management
**Confidence:** HIGH

## Summary

This phase implements multi-character selection infrastructure for the GM dashboard, enabling batch operations in subsequent phases (29-31). The research focused on three key areas: (1) UI components for checkboxes and selection indicators, (2) component-level state management using `HashSet<int>`, and (3) accessibility patterns including keyboard support.

The project already uses Radzen.Blazor 8.4.2 which provides `RadzenCheckBox` - a well-documented component supporting two-way binding and keyboard accessibility. The existing CharacterStatusCard and NpcStatusCard components provide clear extension points for adding selection checkboxes. The codebase follows established CSS theming patterns via `themes.css` with CSS variables that support both fantasy and sci-fi themes.

**Primary recommendation:** Use `RadzenCheckBox` with component-level `HashSet<int>` state in GmTable.razor, extend existing card components with optional checkbox overlay, and implement a sticky selection bar using CSS `position: sticky` with transition animations.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Radzen.Blazor | 8.4.2 | RadzenCheckBox component | Already in use, provides accessible checkbox with theming |
| .NET HashSet<int> | N/A | Selection state storage | O(1) lookup, built-in duplicate prevention |
| CSS Variables | N/A | Theme-aware styling | Existing pattern in themes.css |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Bootstrap Icons | 1.11+ | Selection indicators | Already in use for bi-check-square icons |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| RadzenCheckBox | Native HTML checkbox | Radzen provides theme integration and accessibility for free |
| HashSet<int> | List<int> | HashSet prevents duplicates automatically, O(1) lookup vs O(n) |
| Component-level state | Service/DI state | Component state is simpler, clears automatically on navigation |

**Installation:** No additional packages required - all dependencies already present.

## Architecture Patterns

### Recommended Project Structure
```
Threa.Client/Components/
  Shared/
    CharacterStatusCard.razor      # Add IsSelectable, OnSelectionChanged
    CharacterStatusCard.razor.cs   # Extend base with selection support
    NpcStatusCard.razor            # Wraps CharacterStatusCard, inherits selection
    HiddenNpcCard.razor            # Add selection support
    SelectionBar.razor             # NEW: Sticky selection count/controls
  Pages/
    GamePlay/
      GmTable.razor                # Add HashSet<int> selectedCharacterIds
```

### Pattern 1: HashSet Selection State
**What:** Store selected character IDs in a `HashSet<int>` at the GmTable component level
**When to use:** Multi-select scenarios where duplicates are impossible and O(1) lookup is valuable
**Example:**
```csharp
// Source: Existing pattern in codebase + HashSet best practices
@code {
    private HashSet<int> selectedCharacterIds = new();

    private void ToggleSelection(int characterId)
    {
        if (!selectedCharacterIds.Add(characterId))
        {
            // Already existed, so remove it (toggle off)
            selectedCharacterIds.Remove(characterId);
        }
        StateHasChanged();
    }

    private bool IsCharacterSelected(int characterId)
        => selectedCharacterIds.Contains(characterId);

    private void SelectAllInSection(IEnumerable<TableCharacterInfo> characters)
    {
        foreach (var c in characters)
            selectedCharacterIds.Add(c.CharacterId);
        StateHasChanged();
    }

    private void DeselectAll()
    {
        selectedCharacterIds.Clear();
        StateHasChanged();
    }
}
```

### Pattern 2: Card Selection Checkbox Overlay
**What:** Position checkbox in top-left corner of existing card components
**When to use:** Adding selection to existing card layouts without restructuring
**Example:**
```razor
// Source: Existing CharacterStatusCard pattern + CONTEXT.md decisions
<div class="character-status-card card @CardBorderClass @(IsMultiSelected ? "multi-selected" : "") mb-2"
     style="cursor: pointer; position: relative;">
    @if (IsSelectable)
    {
        <div class="selection-checkbox" @onclick:stopPropagation="true">
            <RadzenCheckBox TValue="bool"
                           Value="@IsMultiSelected"
                           Change="@OnCheckboxToggle" />
        </div>
    }
    <!-- Rest of card content unchanged -->
</div>
```

### Pattern 3: Sticky Selection Bar
**What:** A bar that appears when selections exist, showing count and controls
**When to use:** Providing persistent selection feedback during scrolling
**Example:**
```razor
// Source: CSS-Tricks sticky patterns + CONTEXT.md decisions
@if (selectedCharacterIds.Count > 0)
{
    <div class="selection-bar @(selectedCharacterIds.Count > 0 ? "visible" : "")">
        <span class="selection-count">
            <strong>@selectedCharacterIds.Count</strong> selected
        </span>
        <button class="btn btn-sm btn-outline-secondary" @onclick="DeselectAll">
            Deselect All
        </button>
    </div>
}
```
```css
.selection-bar {
    position: sticky;
    top: 0;
    z-index: 100;
    background: var(--color-card-bg);
    padding: 0.5rem 1rem;
    border-bottom: 1px solid var(--color-card-border);
    transform: translateY(-100%);
    opacity: 0;
    transition: transform 150ms ease, opacity 150ms ease;
}

.selection-bar.visible {
    transform: translateY(0);
    opacity: 1;
}
```

### Pattern 4: Keyboard Event Handler for Escape
**What:** Clear selections when user presses Escape key
**When to use:** Providing quick keyboard shortcut for deselection
**Example:**
```razor
// Source: Microsoft Blazor event handling docs
<div @onkeydown="HandleKeyDown" tabindex="0">
    <!-- Page content -->
</div>

@code {
    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape" && selectedCharacterIds.Count > 0)
        {
            DeselectAll();
        }
    }
}
```

### Anti-Patterns to Avoid
- **Storing selection in individual card components:** Creates synchronization nightmare; keep state centralized in parent
- **Using List instead of HashSet:** Risks duplicates and slower lookups
- **Persisting selection to database:** Selections are transient UI state, not business data
- **Custom checkbox implementation:** RadzenCheckBox already handles accessibility and theming

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Checkbox component | Custom HTML checkbox with styling | RadzenCheckBox | Accessibility, keyboard nav, theme integration |
| Selection state container | Custom class with Add/Remove | HashSet<int> | Built-in duplicate prevention, O(1) operations |
| Slide animation | JavaScript animation | CSS transitions | Simpler, better performance, no JS interop |
| Keyboard shortcuts | Custom key listener service | @onkeydown directive | Built into Blazor, handles focus correctly |

**Key insight:** The Blazor/Radzen stack already provides all the primitives needed. The challenge is composition, not implementation.

## Common Pitfalls

### Pitfall 1: Click Event Propagation
**What goes wrong:** Clicking checkbox also triggers card click (opens details modal)
**Why it happens:** Events bubble up by default in DOM
**How to avoid:** Use `@onclick:stopPropagation="true"` on checkbox container
**Warning signs:** Details modal opens when toggling checkbox

### Pitfall 2: Stale Selection After Character Removal
**What goes wrong:** Selection count includes characters no longer at table
**Why it happens:** Character dismissed/archived but ID still in HashSet
**How to avoid:** Filter selectedCharacterIds against current tableCharacters when displaying count; clean up on RefreshCharacterListAsync
**Warning signs:** Selection count exceeds visible character count

### Pitfall 3: Focus Loss Breaking Keyboard Events
**What goes wrong:** Escape key stops working after clicking various UI elements
**Why it happens:** Focus moves away from the element with @onkeydown handler
**How to avoid:** Add keydown handler at container level, ensure container has tabindex="0"
**Warning signs:** Escape works initially but stops after interaction

### Pitfall 4: Checkbox Touch Targets Too Small
**What goes wrong:** Mobile users struggle to tap checkboxes accurately
**Why it happens:** Default checkbox sizing optimized for mouse
**How to avoid:** Wrap checkbox in larger clickable container (44x44px minimum per WCAG)
**Warning signs:** User complaints about mobile usability

### Pitfall 5: Selection Bar Covers Content
**What goes wrong:** Sticky bar overlaps important UI when it appears
**Why it happens:** Incorrect z-index or missing top offset calculation
**How to avoid:** Ensure proper stacking context, add padding to content below sticky bar
**Warning signs:** Content hidden behind selection bar

## Code Examples

Verified patterns from official sources and existing codebase:

### RadzenCheckBox Usage
```razor
// Source: https://blazor.radzen.com/docs/guides/components/checkbox.html
<RadzenCheckBox @bind-Value="@isChecked"
               TValue="bool"
               Change="@(args => OnCheckChanged(args))" />

@code {
    bool isChecked;

    void OnCheckChanged(bool? value)
    {
        // Handle change
    }
}
```

### Keyboard Event with Specific Key Check
```razor
// Source: https://learn.microsoft.com/en-us/aspnet/core/blazor/components/event-handling
<div @onkeydown="HandleKeyDown" tabindex="0">
    <!-- Content -->
</div>

@code {
    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            // Clear selections
        }
    }
}
```

### Theme-Aware Selection Styling
```css
/* Source: Existing themes.css patterns */
.character-status-card.multi-selected {
    border-color: var(--color-accent-primary) !important;
    background-color: var(--color-bg-tertiary);
    box-shadow: 0 0 0 2px var(--color-accent-primary);
}

[data-theme="scifi"] .character-status-card.multi-selected {
    box-shadow: 0 0 10px var(--color-accent-primary);
}

.selection-checkbox {
    position: absolute;
    top: 4px;
    left: 4px;
    z-index: 10;
    padding: 8px; /* Touch target expansion */
}
```

### Section-Level Select All
```razor
// Source: Existing GmTable.razor patterns
<div class="card-header d-flex justify-content-between align-items-center">
    <strong>Hostile NPCs</strong>
    <div class="d-flex gap-2 align-items-center">
        <span class="badge bg-secondary">@hostileNpcs.Count()</span>
        <button class="btn btn-outline-secondary btn-sm"
                @onclick="() => SelectAllInSection(hostileNpcs)">
            Select All
        </button>
        <button class="btn btn-outline-secondary btn-sm"
                @onclick="() => DeselectAllInSection(hostileNpcs)"
                disabled="@(!hostileNpcs.Any(n => IsCharacterSelected(n.CharacterId)))">
            Deselect
        </button>
    </div>
</div>
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| JavaScript click handlers | Blazor @onclick directives | Blazor 3.0+ | No JS interop needed for basic interactions |
| Redux/Fluxor for all state | Component-level state for UI-only | Always true | Simpler architecture for transient state |
| jQuery animations | CSS transitions | CSS3 | Better performance, simpler code |

**Deprecated/outdated:**
- Manual focus management for keyboard events: Use native tabindex attribute instead
- Two-way binding with `@bind` for checkboxes in lists: Prefer explicit `Value` + `Change` for control

## Open Questions

Things that couldn't be fully resolved:

1. **Exact selection bar positioning**
   - What we know: Should be sticky at top, appear when 1+ selected
   - What's unclear: Exact offset if page has other sticky headers
   - Recommendation: Test positioning after implementation, may need `top` offset adjustment

2. **Animation timing preferences**
   - What we know: CONTEXT.md specifies ~150ms for selection toggle
   - What's unclear: Whether selection bar slide should match or be different
   - Recommendation: Start with 150ms for consistency, adjust based on feel

## Sources

### Primary (HIGH confidence)
- Existing codebase: GmTable.razor, CharacterStatusCard.razor, themes.css - Verified current patterns
- Radzen Blazor docs (https://blazor.radzen.com/checkbox) - CheckBox API and usage
- Microsoft Blazor docs (https://learn.microsoft.com/en-us/aspnet/core/blazor/components/event-handling) - Keyboard event handling

### Secondary (MEDIUM confidence)
- CSS-Tricks sticky patterns - Verified with MDN documentation
- HashSet best practices for multi-select - Verified with .NET API docs

### Tertiary (LOW confidence)
- None - All patterns verified with authoritative sources

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All components already in use in codebase
- Architecture: HIGH - Patterns match existing codebase conventions
- Pitfalls: HIGH - Based on common Blazor event handling issues documented by Microsoft

**Research date:** 2026-02-04
**Valid until:** 2026-03-04 (30 days - stable Blazor/Radzen stack)
