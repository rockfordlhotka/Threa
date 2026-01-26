# Phase 3: Character Creation Inventory - Context

**Gathered:** 2026-01-24
**Status:** Ready for planning

<domain>
## Phase Boundary

Players can browse available items and add them to their starting inventory during character creation. This is about selecting and acquiring items during the character setup flow — not about using them in gameplay (that's Phase 4).

</domain>

<decisions>
## Implementation Decisions

### Item Selection Flow
- **Primary interaction:** Single click adds one copy of the item to inventory
- **Quantity adjustment:** Players can edit quantity directly in their inventory list (click to edit number)
- **Item removal:** Each inventory row has a Delete/X button (explicit removal)
- **Visual feedback:** Immediate visual update only — item appears in inventory list instantly, no toast or animation

### Starting Inventory Constraints
- **No budget system:** Players can add items without credit/point limits
- **Weight limits:** Warning only, not enforced — show warning if over capacity based on STR, but allow creation
- **Item type restrictions:** No restrictions — all items in template library are available
- **Container availability:** Yes, containers are available during character creation (players can select backpacks, quivers, etc.)

### Integration with Character Creation
- **Placement in flow:** Its own tab/section — available anytime during creation, player chooses when to handle it
- **Requirement level:** Optional — players can start with empty inventory if they want
- **Live preview:** Yes, split view — item browser on left, current inventory list on right (always visible)
- **Edit window:** Inventory is editable until the character is "activated" (enters gameplay)

### Claude's Discretion
- Exact layout of split view (responsive behavior, panel sizing)
- Item browser filtering and search implementation
- Handling of container organization during creation (flat list vs nested view)
- Validation messages and warning display patterns

</decisions>

<specifics>
## Specific Ideas

No specific product references or interaction patterns provided — open to standard approaches.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 03-character-creation-inventory*
*Context gathered: 2026-01-24*
