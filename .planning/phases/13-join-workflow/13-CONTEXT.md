# Phase 13: Join Workflow - Context

**Gathered:** 2026-01-26
**Status:** Ready for planning

<domain>
## Phase Boundary

Players request to join campaigns with their characters. GMs review pending requests and approve or deny them. Character can only be active in one campaign at a time. Character removal from active tables is supported.

This phase handles the complete joining lifecycle from discovery through activation. Creating campaigns and gameplay features are separate phases.

</domain>

<decisions>
## Implementation Decisions

### Campaign Discovery & Browsing
- Player selects character first, then sees available campaigns for that character
- Each campaign shows: name, theme, GM name, player count, **and description**
- Campaign description is **required during campaign creation** (update Phase 12's CampaignCreate page to include description field)
- Empty state shows friendly message only: "No campaigns available. Check back later!"

### Join Request Submission
- **Immediate submission** when player clicks "Join with this character" (no confirmation dialog)
- After submitting, **navigate to pending requests view** showing player's active join requests
- If character is already in another campaign: **block with error message** - "[Character] is already active in [Campaign]. Leave that campaign first."
- If player has no characters: **block campaign browsing entirely**, redirect to character creation with message about needing a character first

### GM Review & Decision Interface
- Pending requests appear in **both locations**:
  - Badge/indicator on Campaigns.razor list page showing "3 pending" per campaign
  - Full review interface on campaign dashboard (GmTable page)
- GM sees **full character details**: complete sheet, inventory, and narrative for each pending request
- **Individual buttons per request**: each pending request has 'Approve' and 'Deny' buttons (no batch operations)
- **No confirmation for deny** - deny immediately when clicked, clean rejection

### Post-Decision Experience
- **Pending requests page updates** - player's requests view shows status changes from 'pending' to 'approved'
- After approval, **player navigates to campaign** - 'approved' status shows 'Go to Campaign' button on pending requests view
- Denied requests: **immediate deletion** - removed from list as soon as GM denies
- Character removal: **confirm before removing** - GM removal action requires confirmation to prevent accidents

### Claude's Discretion
- Exact layout and styling of campaign browse cards
- Pending requests view design and navigation placement
- Character detail display format in GM review interface
- Character removal button placement on dashboard
- Error message wording and styling

</decisions>

<specifics>
## Specific Ideas

**Phase 12 Integration:**
- CampaignCreate.razor needs description field added (required, multiline text input)
- TableEdit and TableInfo need Description property
- GameTable DTO needs Description field

**Data Model Extensions:**
- Join request entity with: character ID, campaign ID, status (pending/approved/denied), timestamp
- Character needs "active campaign ID" field to enforce one-campaign-at-a-time rule

**Player Flow:**
1. Player navigates to "Join Campaign" (from character sheet or main nav)
2. Character selector (if multiple) → Browse campaigns screen
3. Click campaign card → Shows join button
4. Submit → Navigate to "My Join Requests" page
5. See pending/approved status → "Go to Campaign" button when approved

**GM Flow:**
1. GM sees pending count badge on Campaigns.razor
2. Click campaign → Dashboard shows "Pending Requests" section
3. Each request shows full character card (expandable details)
4. Approve → Character immediately active at table
5. Deny → Request deleted, player's requests view updates

</specifics>

<deferred>
## Deferred Ideas

None - discussion stayed within phase scope

</deferred>

---

*Phase: 13-join-workflow*
*Context gathered: 2026-01-26*
