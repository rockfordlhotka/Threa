# Phase 5: Container System - Context

**Gathered:** 2026-01-25
**Status:** Ready for planning

<domain>
## Phase Boundary

Players organize items inside container items with capacity limits enforced. This phase adds hierarchical item organization - items going inside other items with physical constraints (weight, volume) tracked and enforced through warnings. Items in equipped containers are accessible during combat, while items in unequipped containers are not.

</domain>

<decisions>
## Implementation Decisions

### Container UI pattern
- Side panel layout: main inventory on left, selected container contents on right
- Move items into containers: select item(s) in main inventory, then click container tile
- Remove items from containers: select item(s) in container panel, click "Remove" button to move back to main inventory
- Visual fill indicator: container icon changes appearance based on fill level (empty/partial/full) - no numeric count badge

### Capacity enforcement
- Weight limit violations: warn but allow placement (same pattern as character carrying capacity)
- Volume system: both weight AND volume tracked (no slot count system)
- Removing items: no special handling for over-limit containers after item removal
- Fill status display: color-coded status only (green/yellow/red) matching visual fill indicator

### Container types & restrictions
- Nesting: one level only (containers can hold containers, but nested containers cannot hold more containers)
- Item type restrictions: warn only (quiver designed for ammunition, but allows other items with warning)
- Valid containers: ItemType.Container OR items with container-related EquipmentSlot (belt pouch, backpack)
- Weight calculation: container weight + contents weight both count toward character carrying capacity

### Equipped container behavior
- Containers with EquipmentSlot can be equipped (backpack to Back slot, belt pouch to Belt slot)
- Combat access: only equipped containers accessible during combat; unequipped containers cannot be accessed in combat
- Dropping containers: ask player each time via confirmation dialog ("Drop backpack with 8 items inside, or empty first?")
- No quick access system in Phase 5 (deferred to future phase)

### Claude's Discretion
- Exact color scheme for fill indicators (green/yellow/red thresholds)
- Container panel layout and styling details
- Warning message text and styling
- Animation/transitions for container state changes

</decisions>

<specifics>
## Specific Ideas

- Side panel design should feel similar to existing split-view patterns from character creation inventory (Phase 3)
- Visual fill indicator should integrate smoothly with existing tile-based inventory grid from Phase 4
- Container access restriction during combat sets up future action point cost system

</specifics>

<deferred>
## Deferred Ideas

- Quick access/favorites system for frequently used items - future phase
- Action point costs for accessing containers during combat - noted for Phase 6 integration
- Equipped containers providing capacity bonuses (backpack adds storage) - could be separate phase or enhancement

</deferred>

---

*Phase: 05-container-system*
*Context gathered: 2026-01-25*
