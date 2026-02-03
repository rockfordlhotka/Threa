# Phase 24: NPC Template System - Context

**Gathered:** 2026-02-02
**Status:** Ready for planning

<domain>
## Phase Boundary

GMs can build and organize a library of NPC templates for pre-session encounter prep. Templates store reusable NPC configurations that can be spawned during play (spawning is Phase 25). This phase covers creation, editing, organization, and browsing of templates.

</domain>

<decisions>
## Implementation Decisions

### Template creation flow
- Two creation paths: create from scratch OR clone from existing character/template
- Cloning copies stats only (attributes, skills) — not equipment or notes
- Create action lives on dedicated Template Library page (not on GM dashboard)
- After creating, GM stays in editor to continue working — saves when ready

### Library organization
- Two-level organization: categories (folders) plus tags within
- Categories are GM-created (no predefined system categories)
- Simple list view in library: name, category, tags — click to expand/edit
- Full search capabilities: name search + category filter + tag filter

### Template content
- Full content: stats + notes + equipment
- Single free-text notes field (not structured sections)
- Auto-calculated difficulty rating from AS values and equipment
- Default disposition (Hostile/Neutral/Friendly) stored on template, overridable at spawn

### Template editing
- Dedicated page editor (same as creation) — not modal or inline
- Spawned NPCs have no connection to template — edits don't affect existing instances
- Deactivated templates show as inactive in list (dimmed/strikethrough), can reactivate
- No duplicate action — clone from scratch covers variant creation

### Claude's Discretion
- Exact difficulty calculation algorithm
- Tag UI implementation (chips, autocomplete, etc.)
- Category management UI
- Field validation rules

</decisions>

<specifics>
## Specific Ideas

No specific references — open to standard approaches.

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope.

</deferred>

---

*Phase: 24-npc-template-system*
*Context gathered: 2026-02-02*
