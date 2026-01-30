# Phase 20: Inventory Manipulation - Context

**Gathered:** 2026-01-29
**Status:** Ready for planning

<domain>
## Phase Boundary

GM directly controls character inventory during gameplay - add items from templates with quantity/container selection, remove items from inventory, equip/unequip items to/from equipment slots, and edit character currency values. This gives the GM full inventory control through the character detail modal's Inventory tab.

</domain>

<decisions>
## Implementation Decisions

### Item Addition Workflow
- GM accesses item addition from **character modal Inventory tab** (not GM Actions tab, not Grant Items tab)
- Item selection via **modal with searchable item template grid** (similar to EffectTemplatePickerModal pattern)
- GM **prompted for quantity** during item selection - after picking template, ask "How many?" before adding
- GM **can choose target container** during add - option to select which container (or none for main inventory)

### Item Removal and Equipment
- Item removal via **context menu or action dropdown per item** (not direct remove buttons, not bulk select mode)
- When removing equipped items: **auto-unequip then remove** (silent unequip, no confirmation needed)
- GM **can directly equip/unequip items** - include equip/unequip in item action menu
- Removal confirmation: **confirm only for equipped or rare items** (skip confirmation for common items)

### Inventory View and Context
- Inventory tab shows **all four contexts**:
  - Current equipment slots with equipped items
  - Container organization (items grouped by container)
  - Item stats and bonuses (modifiers, damage, armor, weight)
  - Quantity and stackable items (clear quantity display for ammo, etc.)
- Inventory tab is **fully interactive** - all add/remove/equip/move actions inline, no separate modals for basic operations
- Equipped items shown with **equipped badge on items in inventory** (not separate equipment section, not both)
- GM sees **simplified GM view focused on manipulation** - different from player view, optimized for quick GM actions over aesthetics

### Real-Time Feedback
- GM confirmation: **inline success message in inventory view** (green alert bar, not toast notification)
- Dashboard updates: **Yes, trigger CharacterUpdateMessage** to refresh dashboard character card immediately
- Player notification: **Silent on player side** - no toast, no notification, player discovers changes organically (supports stealth scenarios like pickpocketing)
- Error handling: **error alert in inventory view** - display inline error explaining what went wrong

### Currency Editing
- **Included in Phase 20** - GM can edit character currency values from Inventory tab (currency is inventory-related)
- Currency editing interface integrated with inventory management workflow

</decisions>

<specifics>
## Specific Ideas

- Use EffectTemplatePickerModal as the pattern for ItemTemplatePickerModal - card grid with search and filters
- Silent player-side updates support GM scenarios like pickpocketing or surprise loot removal without alerting the player
- Inline feedback keeps GM focused on the Inventory tab - no context switching to chase toast notifications

</specifics>

<deferred>
## Deferred Ideas

None - discussion stayed within phase scope

</deferred>

---

*Phase: 20-inventory-manipulation*
*Context gathered: 2026-01-29*
