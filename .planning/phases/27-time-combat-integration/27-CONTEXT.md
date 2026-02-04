# Phase 27: Time & Combat Integration - Context

**Gathered:** 2026-02-03
**Status:** Ready for planning

<domain>
## Phase Boundary

NPCs participate in combat rounds and time advancement identically to PCs. Players can target visible NPCs with attacks. Time processing (round advancement, effect expiration, AP recovery) applies to all NPCs at a table. The architectural decision to reuse CharacterEdit means NPC combat mechanics are inherited from PC implementation.

</domain>

<decisions>
## Implementation Decisions

### Targeting list
- NPCs appear first in target dropdown, then PCs below
- NPCs grouped by disposition within the NPC section (Hostile/Neutral/Friendly subheadings)
- Each NPC shows disposition icon (skull/circle/heart) matching GM dashboard styling
- Only visible NPCs (VisibleToPlayers = true) appear in target list

### Visibility filtering
- Hidden NPCs completely excluded from player-visible target lists
- If player is mid-action when GM hides target, action is invalidated — player picks new target
- When GM reveals an NPC, activity log entry: "[NPC Name] appears!" (name only, no disposition text)

### Combat feedback
- NPC damage logs show generic hit messages: "Goblin Scout is hit" (exact damage hidden from players)
- NPC defeat messages differ by pool:
  - VIT = 0: "[NPC Name] goes down!"
  - FAT = 0: "[NPC Name] is stunned!"
- Wound badges (Laceration, Fracture, etc.) visible to players on visible NPCs

### Time processing
- ALL NPCs processed during time advancement (including hidden ones) — maintains simulation integrity
- Time advancement summary uses single total: "Processed 1 round for 5 characters" (no NPC/PC breakdown)
- NPC effect changes (expiration, wound progression) are GM-only — not logged to player activity feed

### Claude's Discretion
- Exact implementation of target dropdown component
- How target invalidation is communicated to player (toast vs inline message)
- Reveal activity message formatting and category

</decisions>

<specifics>
## Specific Ideas

- TimeAdvancementService already processes all TableCharacters without filtering — NPCs already work once attached to table
- GetAvailableTargets() in Play.razor needs to include visible NPCs from TableCharacterInfo
- TargetSelectionModal.TargetInfo already has IsNPC property — use it for grouping/icons

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 27-time-combat-integration*
*Context gathered: 2026-02-03*
