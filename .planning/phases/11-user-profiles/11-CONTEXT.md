# Phase 11: User Profiles - Context

**Gathered:** 2026-01-26
**Status:** Ready for planning

<domain>
## Phase Boundary

Users can customize their profiles with display names, email, and Gravatar-based avatars. This includes profile editing, avatar display throughout the UI, and privacy controls. New features like bio/about sections, activity feeds, or social features are out of scope.

</domain>

<decisions>
## Implementation Decisions

### Profile editing UI
- Dedicated /profile page (full page at /profile route)
- Editable fields: display name and email only (no bio, no avatar upload)
- Live Gravatar preview on profile page that updates when email changes
- Real-time inline validation matching existing form patterns

### Avatar display patterns
- Avatars appear in: navigation bar (current user), user lists (admin panel), game/character contexts
- Size varies by context: larger in nav (48px), medium in lists (32px), small in inline contexts (24px)
- Loading states: show loading spinner briefly, then fall back to initials if slow/failed
- Avatars are decorative only (not clickable)

### Display name vs username
- When display name is set: show display name with username tooltip on hover (e.g., hovering "Rocky Lhotka" reveals "rocky")
- When no display name: show username as fallback
- Display names can be duplicates (username is the unique identifier)
- Display name restrictions: 1-50 characters with profanity filter

### Email privacy and visibility
- Email is never visible to other users (only to the user themselves and admins)
- Avatar updates immediately when email changes (new Gravatar lookup)
- Users can opt out of Gravatar (toggle to always use initials instead)
- If user removes email (sets to empty): fallback to initials from display name

### Claude's Discretion
- Exact profanity filter implementation (library choice, word list)
- Specific tooltip styling and hover delay
- Loading spinner duration before fallback
- Exact validation message wording

</decisions>

<specifics>
## Specific Ideas

- Profile page should follow the same form pattern as registration/admin pages
- Gravatar integration using RadzenGravatar component (available in Radzen.Blazor 8.4.2)
- Initials fallback when no email or Gravatar disabled (already decided in v1.1 decisions)

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 11-user-profiles*
*Context gathered: 2026-01-26*
