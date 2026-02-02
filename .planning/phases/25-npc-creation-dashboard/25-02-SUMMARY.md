---
phase: 25-npc-creation-dashboard
plan: 02
subsystem: services
tags: [npc, auto-naming, session-scoped, di]

# Dependency graph
requires:
  - phase: 25-01
    provides: NPC Dashboard wireframe requirements
provides:
  - NpcAutoNamingService for generating unique NPC names
  - Global counter pattern across all NPCs
  - Template prefix memory within session
affects: [25-03, 25-04]

# Tech tracking
tech-stack:
  added: []
  patterns: [session-scoped service, global counter, prefix memory]

key-files:
  created:
    - Threa/Threa/Services/NpcAutoNamingService.cs
  modified:
    - Threa/Threa/Program.cs

key-decisions:
  - "Global counter across all NPCs (not per-template) to avoid confusion"
  - "Template prefixes remembered within session for consistency"
  - "Prefix extraction from customized names updates future spawns"

patterns-established:
  - "NPC naming: Global counter + template prefix memory"

# Metrics
duration: 2min
completed: 2026-02-02
---

# Phase 25 Plan 02: NPC Auto-Naming Service Summary

**Session-scoped NpcAutoNamingService with global counter and template prefix memory for unique NPC names**

## Performance

- **Duration:** 2 min 5 sec
- **Started:** 2026-02-02T06:59:28Z
- **Completed:** 2026-02-02T07:01:33Z
- **Tasks:** 3
- **Files modified:** 2

## Accomplishments
- Created NpcAutoNamingService with GenerateName method using global counter
- Implemented GetOrSetPrefix for template prefix memory
- Added UpdatePrefixFromName for extracting prefixes from customized names
- Registered service as Scoped in DI container

## Task Commits

Each task was committed atomically:

1. **Task 1: Create NpcAutoNamingService** - `47a0fa4` (feat)
2. **Task 2: Register service in Program.cs** - `4165f6a` (feat)
3. **Task 3: Verify service integration** - (verification only, no commit)

## Files Created/Modified
- `Threa/Threa/Services/NpcAutoNamingService.cs` - Session-scoped service for generating unique NPC names with global counter
- `Threa/Threa/Program.cs` - Added NpcAutoNamingService registration as Scoped

## Decisions Made
- Global counter across all NPCs (not per-template) per CONTEXT.md to avoid "which Goblin 2?" confusion
- Service registered as Scoped (not Singleton) so each user session has its own counter
- Prefix extraction uses regex to handle names like "Goblin Chief 3" -> "Goblin Chief"

## Deviations from Plan
None - plan executed exactly as written.

## Issues Encountered
None

## User Setup Required
None - no external service configuration required.

## Next Phase Readiness
- Auto-naming service ready for integration with NPC spawning UI
- Service can be injected into components that spawn NPCs from templates
- No blockers for Phase 25-03 (Spawn NPC modal)

---
*Phase: 25-npc-creation-dashboard*
*Completed: 2026-02-02*
