# Phase 19: Effect Management - Context

**Gathered:** 2026-01-28
**Status:** Ready for planning

<domain>
## Phase Boundary

GM creates, applies, edits, and templates character effects (buffs, debuffs, conditions) that modify attributes, skills, and other character stats during gameplay. This phase delivers the effect lifecycle management system from the GM dashboard.

**In scope:**
- CRUD operations for character effects
- Effect templates (system-wide library)
- Duration tracking (rounds, turns, real-time, permanent)
- Modifier system with behavior tags
- Equipment-triggered effects integration
- Dedicated Effects tab in character detail

**Out of scope:**
- Spell casting UI (separate phase)
- Automated effect triggers from combat actions (separate phase)
- Player-side effect visibility (separate phase)
- Effect animations or visual feedback beyond status display

</domain>

<decisions>
## Implementation Decisions

### Effect Data Model

- **Hybrid storage**: Core properties as table columns + flexible properties in JSON
  - **Table columns**: `description`, `duration`, `icon`, `color`
  - **JSON blob**: All modifiers (AS, FAT, VIT, attribute, skill, etc.) since some effects are purely narrative while others modify stats
- **Migrate wounds to effects**: Wounds become a type of effect with severity property - unified effect system
- **Behavior tags system**: Effects have tags that activate different handlers (e.g., `["modifier", "end-of-round-trigger"]`, `["narrative"]`, `["equipment-driven"]`)
  - Tags support both game-time triggers (end-of-round) and real-world time triggers (epoch-based)
- **All modifiers stack**: Simple additive stacking - multiple effects on same stat sum together

### Duration and Timing

- **Four duration types supported**:
  - **Rounds (combat time)**: Effect lasts N combat rounds
  - **Turns (character-specific)**: Effect lasts N of the character's turns
  - **Real-world time**: Effect lasts until epoch timestamp (minutes/hours/days for long-term effects)
  - **Permanent/until removed**: No expiration - GM must manually remove
- **Hybrid storage**: `durationType` + `durationValue` + `computedExpiry` fields
  - Store input format plus computed expiry timestamp for efficient queries
- **Event-driven expiration**: Any time advancement (round advance, time flow) triggers effect evaluation
  - Per-round effects expire/trigger at end-of-round (combat is fast)
  - Other effects check against time system advancement
- **Notification on trigger/expiry**: Toast or alert when effects activate, trigger behavior, or expire

### Template System

- **System-wide template library**: Global effect templates - all GMs can access common effects
- **Two creation paths**:
  - Save from existing applied effect ("Save as template" button)
  - Create template directly in template builder
- **Everything editable on apply**: Template provides defaults - GM can modify any property before applying to character
- **Tag-based organization**: Templates have tags (combat, magic, poison, cyberware, etc.) for flexible filtering and grouping
- **Equipment integration**: Items reference effect templates via `effectTemplateId`
  - Equipping item auto-applies template to character
  - Unequipping auto-removes the effect
  - System automatically manages effect lifecycle with equipment state

### UI and Workflow

- **Dedicated Effects tab**: Separate tab in character detail for effect management (not inline in Actions)
- **Card grid display**: Active effects shown as cards (like wounds) with icon, name, duration, quick remove button
- **Single form for create/edit**: All properties visible on one screen (name, description, icon, color, duration, tags, modifiers)
- **Quick-apply system**:
  - Favorite/common templates shown as quick-apply buttons
  - "More templates" opens full template picker
  - Template picker pre-fills form with selected template

### Claude's Discretion

- Exact JSON schema for modifiers (as long as it's extensible and supports typed modifiers)
- Icon library or upload mechanism
- Color picker implementation
- Specific toast/notification styling
- Tag autocomplete vs predefined tag list
- Template favoriting mechanism
- Effect card visual design details

</decisions>

<specifics>
## Specific Ideas

- **Behavior tag examples from discussion**:
  - Stunned effect: Has `["end-of-round-trigger"]` tag, active behavior sets character's FAT to 0 at end-of-round
  - Cyberware skill chip: Equipment-driven effect with `["modifier"]` tag, provides specific skill at level 3 when equipped
  - Narrative effects: Pure flavor with `["narrative"]` tag, no mechanical modifiers

- **Integration with existing systems**:
  - Time system already exists in codebase - hook into time advancement events
  - Wounds already implemented - migrate to use unified effect system with severity as special property
  - Equipment effects discussed in previous milestone - don't lose this capability

</specifics>

<deferred>
## Deferred Ideas

None - discussion stayed within phase scope.

</deferred>

---

*Phase: 19-effect-management*
*Context gathered: 2026-01-28*
