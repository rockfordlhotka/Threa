# Phase 18: Wound Management - Context

**Gathered:** 2026-01-28
**Status:** Ready for planning

<domain>
## Phase Boundary

GM can add, remove, and edit wounds on characters with severity tracking through the dashboard. This phase focuses on wound CRUD operations, severity levels, and basic wound effects (AS penalties, ongoing damage). Wound recovery mechanics and complex wound interactions are separate concerns.

Builds on Phase 17's health management system for damage tracking and integration.

</domain>

<decisions>
## Implementation Decisions

### Wound Entry and Editing
- **Add Wound button** lives in CharacterDetailGmActions (character detail modal)
- **Modal-over-modal pattern**: "Add Wound" opens a new modal on top of character detail modal
- **Common wound dropdown**: Quick-select from preset wounds (broken arm, concussion, bleeding, etc.) then customize
- **Edit workflow**: Optimized for changing duration/severity as primary use case — choose simplest UX approach

### Wound Form Fields
- **Required fields**: Severity, Description, Location/Body Part
- **Effect fields with defaults**: AS penalty (numeric), FAT damage/min, VIT damage/min
- **Default behavior**: Form pre-fills suggested values based on severity selection, GM can override all fields
- **Location**: Dropdown or text field for body part (head, torso, arm, leg, etc.)

### Severity System
- **Fixed four levels**: Minor, Moderate, Severe, Critical (hardcoded, not customizable)
- **Visual presentation**: Bootstrap badge style with severity text labels
- **Mechanical meaning**: Each severity suggests default AS penalty and FAT/VIT damage rates, but GM sets final values
- **Effect on gameplay**: Wounds affect all ability scores (AS) with negative modifier, cause ongoing FAT/VIT damage per minute depending on severity

### Wound List Display
- **Main display**: Compact table in character detail screen showing Severity + Description columns
- **Title bar icons**: One icon/badge per wound, hover shows tooltip with wound details
- **Table sorting**: Sort by severity option (shows critical wounds first)
- **Actions**: Edit/remove available per wound in table row

### Integration with Health (Phase 17)
- **Damage-to-wound prompt**: When VIT damage exceeds threshold (e.g., >5 points), prompt GM to add wound (optional, dismissible)
- **Automatic damage ticks**: Wound FAT/VIT damage applies automatically based on time system integration
- **AS penalty display**: Show wound penalties in AS calculation breakdown (e.g., "Base: 5, Wounds: -2, Total: 3")
- **Unified healing UI**: Healing mode allows GM to apply healing to pools OR reduce wound severity in one interface

### Claude's Discretion
- Exact threshold for VIT damage wound prompt
- Tooltip content and formatting for title bar wound badges
- Table column widths and responsive behavior
- Modal sizing and layout for wound form
- Common wound template list (specific preset wounds to include)
- Time integration mechanism for automatic damage ticks

</decisions>

<specifics>
## Specific Ideas

- Title bar should show wound icons/badges with tooltip on hover (one per wound)
- Wound effects automatically tick damage based on time system (requires integration with Phase 12-16 time tracking)
- Healing workflow unified with health management from Phase 17 (same card/mode, different targets)
- Edit optimized for changing duration and severity as most common operations

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 18-wound-management*
*Context gathered: 2026-01-28*
