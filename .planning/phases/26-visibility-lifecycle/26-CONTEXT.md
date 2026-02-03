# Phase 26: Visibility & Lifecycle - Context

**Gathered:** 2026-02-03
**Status:** Ready for planning

<domain>
## Phase Boundary

GMs can hide NPCs for surprise encounters and manage NPC persistence across sessions. This includes visibility toggling (hidden NPCs don't appear to players), NPC dismissal with archive/delete options, saving active NPCs as templates, and full session persistence. NPCs use the existing CharacterEdit model, so persistence is already handled — this phase adds visibility state and lifecycle management UI.

</domain>

<decisions>
## Implementation Decisions

### Visibility mechanics
- Hidden NPCs move to a dedicated "Hidden" section (not grouped by disposition)
- Hidden section is collapsed by default, shows count only, expands on click
- Minimized/collapsed card representation for hidden NPCs (not full status card)
- Crossed-out eye icon indicates hidden status
- Visibility toggle accessible from both minimized card AND CharacterDetailModal
- Toggle is instant, no confirmation required
- Newly spawned NPCs start **hidden** by default (GM reveals when ready to introduce)

### Dismiss vs archive
- Two options when removing NPC: Delete (permanent) or Archive (retrievable)
- Archive is instant, no confirmation
- Delete requires confirmation dialog ("Permanently delete [NPC]?")
- Archived NPCs accessible via separate archive page/modal (not on main dashboard)
- Restored NPCs return in hidden state (GM reveals when ready)

### Save as template
- "Save as Template" action in CharacterDetailModal GM Actions tab only (not on cards)
- Copies full current state: attributes, skills, equipment, wounds, effects, health, notes
- Template name pre-filled with current NPC name, editable before saving
- Modal prompts for category and tags before saving

### Session persistence
- NPCs save automatically on every change (same as PCs — already implemented)
- No automatic cleanup between sessions — NPCs persist until explicitly dismissed
- Everything persists: wounds, effects, health, inventory, notes, visibility state
- NPCs load automatically with table (same as PCs), including hidden NPCs

### Claude's Discretion
- Exact styling for minimized hidden NPC cards
- Archive page/modal layout and search/filter capabilities
- Icon choices beyond the crossed-out eye (e.g., archive, restore, delete icons)
- Error state handling

</decisions>

<specifics>
## Specific Ideas

- Visibility is a "currently active in scene" toggle — hidden means "off-screen" or "not yet introduced"
- Long-running campaign NPCs (bosses, narrative characters) may stay hidden for many sessions
- One-off combat NPCs exist just for a few rounds, then dismissed
- Hidden section collapsed by default keeps dashboard clean for the common "only active NPCs" view

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 26-visibility-lifecycle*
*Context gathered: 2026-02-03*
