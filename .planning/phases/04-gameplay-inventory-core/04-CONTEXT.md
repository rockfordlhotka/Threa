# Phase 4: Gameplay Inventory Core - Context

**Gathered:** 2026-01-25
**Status:** Ready for planning

<domain>
## Phase Boundary

Players manage their inventory during active gameplay sessions - viewing items in a grid interface, equipping them to specific equipment slots, unequipping back to inventory, and dropping/destroying items permanently. This phase focuses on the Play page inventory tab, distinct from the character creation flow.

</domain>

<decisions>
## Implementation Decisions

### Inventory Display Layout
- Grid-based display with icons (not list/table)
- Each item shows: icon + name + quantity (for stackable items)
- Single scrollable list - no grouping or filtering by type
- Equipped items appear in both equipment slots AND inventory grid with visual indicator
- Visual badge/icon on inventory items to show equipped status

### Equipment Slot Interaction
- Equipment slots displayed as list/grid (not paper doll/character outline)
- Show all equipment slots always, including empty ones with placeholder
- Equipped items in slots show: icon + name (no bonuses displayed in slot)
- Two-step interaction: click item in inventory, then click target equipment slot

### Equip/Unequip Flow
- Auto-swap when equipping to filled slot (old item automatically unequipped)
- **Important constraint:** Cursed items cannot be unequipped (must block swap if target slot has cursed item)
- Animation/transition visual feedback during equip/unequip
- Equip restrictions: validate slot type compatibility + attribute/class requirements
- Unequipped items always return to inventory

### Drop/Destroy Actions
- Select item first, then click action button (not per-item icons or right-click menu)
- Always confirm before dropping ("Drop [item]?" dialog)
- For stackable items: prompt for quantity ("Drop how many?")
- Drop and Destroy are the same action - items removed permanently (no recovery)

### Claude's Discretion
- Exact grid layout (columns, spacing, icon size)
- Animation implementation details
- Error message wording
- Loading states and transitions

</decisions>

<specifics>
## Specific Ideas

- Cursed items constraint is critical - must prevent unequipping cursed items even during auto-swap
- Visual indicator for equipped items should be clear enough to see at a glance in grid view

</specifics>

<deferred>
## Deferred Ideas

None - discussion stayed within phase scope

</deferred>

---

*Phase: 04-gameplay-inventory-core*
*Context gathered: 2026-01-25*
