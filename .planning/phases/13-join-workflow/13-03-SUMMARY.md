---
phase: 13-join-workflow
plan: 03
subsystem: ui-player
tags: [blazor, player-ui, navigation, join-workflow]
completed: 2026-01-27
duration: 5 min

dependency-graph:
  requires: [13-01, 13-02]
  provides: [BrowseCampaigns-page, MyJoinRequests-page, join-navigation]
  affects: []

tech-stack:
  added: []
  patterns: [character-first-flow, csla-data-portal-ui]

key-files:
  created:
    - Threa/Threa.Client/Components/Pages/Player/BrowseCampaigns.razor
    - Threa/Threa.Client/Components/Pages/Player/MyJoinRequests.razor
  modified:
    - Threa/Threa/Components/Layout/NavMenu.razor

decisions: []

metrics:
  tasks: 3/3
  files-created: 2
  files-modified: 1
---

# Phase 13 Plan 03: Player Join UI Summary

**One-liner:** Character-first campaign browser and join request tracking pages with NavMenu integration for player workflow.

## What Was Built

### BrowseCampaigns Page (/player/browse-campaigns)
Created `Threa/Threa.Client/Components/Pages/Player/BrowseCampaigns.razor`:
- **Two-step character-first flow**: Player must select a character before seeing available campaigns
- **No-character handling**: Players with no characters see redirect to character creation
- **Available character filtering**: Only shows activated characters not already in a campaign
- **Campaign cards display**: Name, ThemeIndicator badge, truncated description, GM, and status
- **Immediate join submission**: Click "Join with [character]" calls JoinRequestSubmitter directly
- **Post-submit navigation**: Automatically navigates to /player/join-requests after successful submission
- **Filtering**: Excludes player's own GM tables and ended campaigns

### MyJoinRequests Page (/player/join-requests)
Created `Threa/Threa.Client/Components/Pages/Player/MyJoinRequests.razor`:
- **Pending requests**: Shows with hourglass badge (bi-hourglass-split)
- **Approved requests**: Shows with success badge and "Go to Campaign" button linking to /play/{tableId}
- **Denied requests**: Not displayed (per CONTEXT.md - deleted on denial)
- **Empty state**: Friendly message with link to browse campaigns
- **Request info**: Character name, table name, requested timestamp
- **Note**: Informational footer explaining denied requests are automatically removed

### NavMenu Navigation
Updated `Threa/Threa/Components/Layout/NavMenu.razor`:
- Added "Browse Campaigns" link (bi-search icon) to /player/browse-campaigns
- Added "My Requests" link (bi-hourglass icon) to /player/join-requests
- Placed in player section after "My characters" and before "My profile"

## Commits

| Commit | Description |
|--------|-------------|
| 456752d | feat(13-03): add BrowseCampaigns page with character-first flow |
| cce0d81 | feat(13-03): add MyJoinRequests page for tracking request status |
| 665346a | feat(13-03): add browse campaigns and my requests links to nav menu |

## Deviations from Plan

None - plan executed exactly as written.

## Testing Notes

- `dotnet build Threa.sln` succeeds with no new warnings
- All verification criteria met:
  - BrowseCampaigns.razor exists at /player/browse-campaigns
  - MyJoinRequests.razor exists at /player/join-requests
  - NavMenu shows new navigation links
  - Character selection enforced before showing campaigns
  - Join navigates to MyJoinRequests after submission

## Next Phase Readiness

Ready for 13-04 (GM Review UI):
- Player can browse campaigns and submit join requests
- Player can track request status on MyJoinRequests page
- Navigation integrated into app menu

Phase 13 complete - all join workflow UI implemented:
- Plan 01: Data layer (DTO, DAL)
- Plan 02: Business logic (submitter, processor, read-only objects)
- Plan 03: Player UI (browse, requests, navigation)

Note: GM review UI (pending requests section in GmTable, badges in Campaigns.razor) was already implemented in plan 13-02 as part of that plan's scope.
