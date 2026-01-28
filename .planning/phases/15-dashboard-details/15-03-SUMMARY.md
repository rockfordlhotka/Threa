---
phase: 15-dashboard-details
plan: 03
subsystem: ui
tags: [blazor, modal, real-time, messaging, ux]

# Dependency graph
requires:
  - phase: 15-01
    provides: CharacterDetailModal foundation
  - phase: 15-02
    provides: Tab content components
provides:
  - Real-time modal updates via CharacterUpdateMessage subscription
  - NPC placeholder section in dashboard
  - GM Actions tab with streamlined damage/healing UI
  - Grant Items tab for item distribution
  - Character summary in modal header
  - GetGmNotesAsync method in ITableDal
affects: [16-time-management]

# Tech tracking
tech-stack:
  added: []
  patterns:
    - ITimeEventSubscriber for modal real-time updates
    - Two-button layout for FAT/VIT pool selection
    - PendingPoolBar in modal header for at-a-glance health

key-files:
  created:
    - Threa/Threa.Client/Components/Shared/NpcPlaceholder.razor
    - Threa/Threa.Client/Components/Shared/CharacterDetailGmActions.razor
    - Threa/Threa.Client/Components/Shared/CharacterDetailItemDistribution.razor
  modified:
    - Threa/Threa.Client/Components/Shared/CharacterDetailModal.razor
    - Threa/Threa.Client/Components/Pages/GamePlay/GmTable.razor
    - Threa.Dal/ITableDal.cs
    - Threa.Dal.MockDb/TableDal.cs
    - Threa.Dal.SqlLite/TableDal.cs

key-decisions:
  - "Tab order: GM Actions, Character Sheet, Inventory, Grant Items, Narrative"
  - "Modal header shows character summary with health bars and status badges"
  - "Two-button layout (FAT/VIT) for damage and healing actions"
  - "2-column dashboard layout after center panel removal"
  - "RadzenDialog must be in interactive context for DialogService to work"

patterns-established:
  - "Modal subscribes to CharacterUpdateMessage for real-time refresh"
  - "GM actions moved from dashboard to modal for cleaner UX"
  - "Click-outside-to-close for touch device accessibility"

# Metrics
duration: 45min
completed: 2026-01-27
---

# Phase 15 Plan 03: Real-time Updates and NPC Placeholder Summary

**Real-time modal updates, NPC placeholder, and significant UX improvements based on user feedback**

## Performance

- **Duration:** ~45 min (extensive user feedback iterations)
- **Started:** 2026-01-27
- **Completed:** 2026-01-27
- **Tasks:** 3 (2 auto + 1 checkpoint)
- **Additional improvements:** 6 (user feedback driven)

## Accomplishments

- CharacterDetailModal subscribes to CharacterUpdateMessage for real-time updates
- NPC placeholder section with collapsible UI indicating future functionality
- GM Actions tab with streamlined two-button FAT/VIT layout
- Grant Items tab for item distribution (moved from dashboard)
- Character summary in modal header (species, health bars, AP, wounds, effects)
- Fixed GM notes persistence issue
- Fixed RadzenDialog context issue for modal rendering
- Enabled click-outside-to-close for touch accessibility
- Removed center panel for cleaner 2-column dashboard layout

## Task Commits

Each task was committed atomically:

1. **Task 1: Real-time updates** - `eb141ac` (feat)
2. **Task 2: NPC placeholder** - `8101538` (feat)
3. **Fix: RadzenDialog context** - `1d8ced9` (fix)
4. **Fix: Click-outside-to-close** - `f7d754e` (fix)
5. **Fix: GM notes persistence** - `b135901` (fix)
6. **Refactor: Move GM actions to modal** - `d71dccb` (refactor)
7. **Feat: Item distribution in modal** - `43cf325` (feat)
8. **Feat: Header summary and streamlined actions** - `ac89efd` (feat)

## Files Created/Modified

**Created:**
- `Threa/Threa.Client/Components/Shared/NpcPlaceholder.razor` - Collapsible NPC placeholder section
- `Threa/Threa.Client/Components/Shared/CharacterDetailGmActions.razor` - GM actions with damage/heal/effect/remove
- `Threa/Threa.Client/Components/Shared/CharacterDetailItemDistribution.razor` - Item granting functionality

**Modified:**
- `Threa/Threa.Client/Components/Shared/CharacterDetailModal.razor` - Real-time subscription, 5 tabs, header summary
- `Threa/Threa.Client/Components/Pages/GamePlay/GmTable.razor` - 2-column layout, RadzenDialog, removed center panel
- `Threa.Dal/ITableDal.cs` - Added GetGmNotesAsync method
- `Threa.Dal.MockDb/TableDal.cs` - Implemented GetGmNotesAsync
- `Threa.Dal.SqlLite/TableDal.cs` - Implemented GetGmNotesAsync

## Decisions Made

- Tab order prioritizes GM workflow: GM Actions first, Narrative last
- Modal header provides at-a-glance character status without tab switching
- Two-button FAT/VIT layout eliminates dropdown step for faster actions
- Center panel removed entirely - all character actions in modal
- RadzenDialog must be placed within interactive render mode context
- Click-outside-to-close enabled for touch device accessibility

## Deviations from Plan

### User Feedback Driven Improvements

**1. [Modal not opening] Added RadzenDialog to interactive context**
- Issue: Modal wasn't rendering when clicking character cards
- Fix: Added RadzenDialog component within GmTable.razor's interactive context

**2. [Touch accessibility] Enabled click-outside-to-close**
- User couldn't close modal on touch devices without ESC key
- Changed CloseDialogOnOverlayClick to true

**3. [GM notes not persisting] Added GetGmNotesAsync to DAL**
- Notes were loading from stale AllCharacters collection
- Now loads directly from DAL on each modal open

**4. [Redundant center panel] Moved all actions to modal**
- Center panel duplicated info shown in modal
- Moved damage/healing/effects to new GM Actions tab
- Moved item distribution to new Grant Items tab
- Changed to 2-column layout

**5. [Tab order] Reordered for GM workflow**
- GM Actions moved to first position (most common action)
- Grant Items added between Inventory and Narrative

**6. [Header summary] Added character status to modal header**
- Species, health bars, AP, wounds, effects visible at all times
- Uses PendingPoolBar component for consistent health visualization

**7. [Streamlined actions] Two-button FAT/VIT layout**
- Eliminated dropdown for pool selection
- Direct FAT and VIT buttons for faster interaction

## Issues Encountered

- Pre-existing bug discovered: Play.razor doesn't subscribe to TimeSkipReceived
  - Non-combat time advancement doesn't trigger pending pool flow
  - Documented in STATE.md for Phase 16 to address

## User Setup Required

None - no external service configuration required.

## Next Phase Readiness

- Phase 15 Dashboard Details complete
- All DASH requirements satisfied:
  - DASH-08: Click character card for details
  - DASH-09: Character sheet tab
  - DASH-10: Inventory tab
  - DASH-11: Narrative tab
  - DASH-12: Real-time updates
  - DASH-13: NPC placeholder
- Ready for Phase 16: Time Management

---
*Phase: 15-dashboard-details*
*Plan: 03 of 03*
*Completed: 2026-01-27*
