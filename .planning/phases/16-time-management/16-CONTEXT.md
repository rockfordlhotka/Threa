# Phase 16: Time Management - Context

**Gathered:** 2026-01-27
**Status:** Ready for planning

<domain>
## Phase Boundary

GM controls time flow with multiple increments (round, minute, turn, hour, day, week) and a special "in rounds" mode for detailed combat tracking. Time changes propagate to all characters at the table, triggering automatic processing of pending damage/healing, effect expiration, and AP recovery. Dashboard reflects character state changes after time processing.

</domain>

<decisions>
## Implementation Decisions

### Time Control UI Layout
- Right panel (always visible) dedicated to time controls
- Panel contains only time advancement buttons - no additional info displays
- Current time and mode indicator remain in main header (existing pattern)
- Panel is persistent and non-collapsible - time control is core GM function

### Round Mode Transitions
- Explicit "Start Combat" button to enter round mode (not automatic)
- One-click activation: mode ON, Round 1 begins immediately (no confirmation dialog)
- Same button becomes "End Combat" when in round mode - toggle behavior
- One-click exit from round mode (no confirmation)
- Red "In Rounds: Round X" badge in header when active - obvious combat state indicator

### Time Increment Presentation
- Context-aware display: Show ONLY relevant buttons for current mode
  - In round mode: ONLY "+1 Round" button visible
  - In calendar mode: ONLY "+1 Min", "+10 Min" (Turn), "+1 Hour", "+1 Day", "+1 Week" visible
- Buttons hidden (not disabled) when unavailable for current mode
- Short label format: "+1 Round", "+1 Min", "+10 Min", "+1 Hour", "+1 Day", "+1 Week"
- Full-width buttons stacked vertically in right panel

### Processing Feedback
- Silent auto-update: character cards update automatically with no loading indicator or toast
- Per-character change badges show significant changes:
  - Pending damage/healing applied
  - Effects expired
  - Major AP recovery (not minor ticks)
- Badges fade in with animation, auto-dismiss after 3-5 seconds
- Temporary subtle notification - non-blocking

### Claude's Discretion
- Exact button styling and colors (except red for "In Rounds" badge)
- Badge animation timing and easing
- Definition of "significant" AP recovery threshold
- Badge content format (icon + text vs text only)
- Panel width sizing

</decisions>

<specifics>
## Specific Ideas

- "In round mode time moves by rounds, otherwise by minute/turn/hour/day/week"
- Combat mode badge must be red to clearly indicate combat is active
- Context-aware button display keeps UI clean - show only what's relevant

</specifics>

<deferred>
## Deferred Ideas

None - discussion stayed within phase scope

</deferred>

---

*Phase: 16-time-management*
*Context gathered: 2026-01-27*
