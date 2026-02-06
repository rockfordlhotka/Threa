# Phase 31: Batch Effects - Context

**Gathered:** 2026-02-05
**Status:** Ready for planning

<domain>
## Phase Boundary

GMs can add or remove effects (buffs, debuffs, conditions, etc.) to/from multiple selected characters at once. Applies to all selected characters (PCs and NPCs alike). Effect templates, the effect form, and single-character effect management already exist — this phase adds batch operations only.

</domain>

<decisions>
## Implementation Decisions

### Effect picker flow (Add)
- Two entry paths: "From Template" opens template picker then full form pre-populated; "Custom Effect" opens blank EffectFormModal directly
- After template selection, show the full EffectFormModal with all fields editable (name, type, description, duration, modifiers)
- GM can modify any field before batch-applying
- "Save as Template" button available in the batch form (same as single-character flow)

### Remove behavior
- GM picks effect(s) to remove by name from a union list of all unique effect names across selected characters
- Multi-select supported — GM can check multiple effect names to remove in one operation
- Characters that don't have the selected effect are silently skipped (not counted in results, not an error)

### Result feedback
- Add summary: count-only format — "Added Bless to 5 characters" (matches existing batch pattern)
- Remove summary: aggregate format — "Removed 2 effects from 5 characters" (combined count, not per-effect breakdown)
- Errors use existing expandable error detail pattern from damage/healing batch results
- Selection stays intact after both add and remove operations (GM may want to apply multiple effects to same group)

### Duplicate/conflict handling
- Defer to existing EffectList.AddEffect stacking logic — no special batch override
- All selected characters receive the effect regardless of type (no NPC-only filtering like visibility/dismiss)
- Batch effects are always standalone — no concentration links (SourceCasterId/SourceEffectId)
- Shared timestamp: capture game time once before processing loop; all characters get same CreatedAt/ExpiresAt

### Claude's Discretion
- SelectionBar button placement and styling for effect operations
- Modal sizing and layout
- Union list presentation for remove (sorting, grouping by type, etc.)
- How to handle the EffectFormModal reuse (new component vs parameterized existing)

</decisions>

<specifics>
## Specific Ideas

- Reuse existing EffectTemplatePickerModal and EffectFormModal components — batch flow chains them together
- Follow the same BatchActionService sequential processing pattern used by damage/healing/visibility/dismiss
- BatchActionResult summary should feel consistent with existing batch result messages

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 31-batch-effects*
*Context gathered: 2026-02-05*
