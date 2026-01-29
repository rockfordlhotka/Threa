# Phase 21: Stat Editing - Context

**Gathered:** 2026-01-29
**Status:** Ready for planning

<domain>
## Phase Boundary

GM directly modifies character attributes (STR, DEX, END, INT, ITT, WIL, PHY) and skills through the dashboard. This phase enables editing of **base attribute values** (before species adjustments) and skill levels, with automatic recalculation of dependent stats like health pools, AS, and AP recovery.

Scope: Direct editing of existing stats only. Creation of new custom attributes or skill trees belongs in other phases.

</domain>

<decisions>
## Implementation Decisions

### Edit Interface Style
- **Edit mode toggle** - Single "Edit Stats" button switches entire stats section into edit mode (consistent with other GM actions)
- **All editable at once** - All 7 attributes and all skills become editable simultaneously (no tabs/sections)
- **Number input fields** - Standard `<input type='number'>` fields for direct typing with increment/decrement arrows
- **Save/Cancel buttons** - Standard button pair at bottom; changes aren't applied until Save is clicked

### Validation & Constraints
- **Base attribute editing** - GM edits base attribute values (pre-species adjustment)
- **Display effective values** - Show both base value being edited AND final effective value (base + species modifier)
- **Attribute validation**:
  - Minimum value: 1 (blocked - cannot save)
  - Warn for values > 14 (typical human range is 6-14 before species adjustments)
  - Inline alert warnings below the input field
- **Skill validation**:
  - Minimum value: 0 (blocked)
  - Warn for skill level > 10
  - Inline alert warnings
- **Impossible states** - Block save if changes create invalid character state (e.g., negative max health)
- **Health pool auto-adjustment**:
  - When attribute changes affect FAT/VIT max, automatically adjust current pools
  - Cap at new max (if current > new max, set current = new max)

### Recalculation Feedback
- **Recalculate on blur** - Derived stats update when GM moves away from a field (not live as typing)
- **Update derived values immediately** - FAT/VIT max, AS scores, AP recovery update instantly and visibly when field loses focus
- **Visible derived values during editing**:
  - Health pools (FAT/VIT max) - update when END/STR blur
  - AP recovery rate - update when END blurs
  - Effective attribute values - show final value (base + species) alongside base being edited
- **Ability Score updates** - AS values recalculate when GM finishes editing any attribute (on attribute blur)

### Bulk Operations
- **No reset feature** - GM edits manually, no bulk reset to defaults
- **Cancel button reverts all** - Cancel exits edit mode and discards all pending changes (standard form behavior)
- **One character at a time** - Save/cancel closes edit mode, GM reopens for next character (standard flow)
- **No additional shortcuts** - Direct field editing is sufficient

### Claude's Discretion
- Exact layout and spacing of edit controls
- Error message wording for validation failures
- Transition animations between view/edit mode
- Loading states during save operations

</decisions>

<specifics>
## Specific Ideas

- Must ensure dependent values recalculate correctly: FAT/VIT max, AP recovery, all skill AS scores
- Human attribute base values typically range 6-14, then species adjustments are applied
- GM needs to see both base and final (effective) attribute values during editing
- Validation should prevent impossible states but warn (not block) for unusual but valid values

</specifics>

<deferred>
## Deferred Ideas

None - discussion stayed within phase scope

</deferred>

---

*Phase: 21-stat-editing*
*Context gathered: 2026-01-29*
