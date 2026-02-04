# Project Research Summary

**Project:** Threa TTRPG Assistant - v1.6 Batch Character Actions
**Domain:** TTRPG GM Tools - Batch Character Management
**Researched:** 2026-02-04
**Confidence:** HIGH

## Executive Summary

Batch character actions enable GMs to apply operations (damage, healing, effects, visibility toggles, dismissal) to multiple selected characters simultaneously. Research shows this is a **UI/pattern implementation task, not a technology challenge**. The existing stack (Radzen.Blazor 8.4.2, CSLA.NET 9.1.0, Rx.NET 6.0.1) provides all necessary capabilities with no new dependencies required.

The recommended approach follows the proven `TimeAdvancementService` pattern already in the codebase: sequential processing with error aggregation, component-level selection state in `GmTable.razor`, and reuse of the existing `CharactersUpdatedMessage` for real-time notifications. Batch operations should be simple wrappers around existing single-character actions extracted from `CharacterDetailGmActions.razor`.

The primary risk is **message flooding** from rapid individual updates causing UI churn and activity log spam. This is mitigated by publishing a single batch notification after all characters are processed. Secondary risks include partial failure obscuring errors and selection state diverging from server state, both addressed through structured result types and execution-time validation patterns already proven in `TimeAdvancementService`.

## Key Findings

### Recommended Stack

**No new dependencies required.** The existing stack fully supports batch operations through established patterns. Radzen.Blazor already provides multi-select capabilities via `SelectionMode.Multiple` on DataGrid or custom checkbox patterns on cards. CSLA.NET handles individual saves in loops with error aggregation (proven pattern in `TimeAdvancementService`). Rx.NET's `InMemoryMessageBus` broadcasts batch updates via the existing `CharactersUpdatedMessage` type.

**Core technologies:**
- **Radzen.Blazor 8.4.2**: Multi-select UI components via DataGrid or custom card checkboxes — proven in existing codebase
- **CSLA.NET 9.1.0**: Business objects with sequential save pattern and dirty tracking — matches TimeAdvancementService approach
- **System.Reactive 6.0.1**: Real-time batch notifications via existing CharactersUpdatedMessage — no new message types needed

**What NOT to add:**
- State management libraries (Fluxor) — HashSet selection state is sufficient
- MediatR/CQRS — direct async/await with CSLA portal is simpler
- Background job frameworks — batch ops are synchronous UI operations
- Bulk update stored procedures — gaming data volumes don't warrant database-level batching

### Expected Features

Research into VTT ecosystem (Foundry VTT, Roll20, D&D Beyond) and SaaS bulk action UX patterns reveals clear feature expectations.

**Must have (table stakes):**
- Multi-character selection with checkboxes/click-to-select — core premise of batch operations
- Selection count display ("3 selected") — standard bulk action pattern per Eleken guidelines
- Select All / Deselect All per section — expected in any bulk UI per PatternFly patterns
- Batch damage application — primary use case ("fireball hits everyone")
- Batch healing application — symmetric with damage, equally expected
- Batch visibility toggle for NPCs — reveal/hide for dramatic encounters
- Clear success/failure feedback — GMs must know what happened, partial success common

**Should have (competitive differentiators):**
- Batch effect add/remove — rare in VTTs, Foundry Multi Token Status module provides this
- Cross-type selection (NPCs + PCs together) — most tools separate these
- Partial success reporting with per-character details — "4/5 applied; Goblin 3 at max FAT"
- Batch dismiss/archive NPCs — combat cleanup workflow

**Defer (v2+):**
- Selection persistence across tabs — prevents frustration but not essential for MVP
- Keyboard shortcuts (Ctrl+A, Escape) — power user efficiency, add after validation
- Batch combat actions (attack/defend) — too complex, requires individual rolls/targets
- Undo batch operations — requires transaction log, complex state management
- Saved selection groups — feature creep, disposition groups suffice

### Architecture Approach

The architecture leverages existing patterns throughout. Selection state lives as component-level `HashSet<int>` in `GmTable.razor`, matching the existing `selectedCharacter` single-select pattern. Batch execution follows the sequential loop with error aggregation pattern proven in `TimeAdvancementService` (lines 137-176), where each character is fetched, modified, saved, and results aggregated into success/failure lists.

**Major components:**
1. **BatchActionService (NEW)** — Orchestrates batch operations, loops through selected characters, applies actions, aggregates results. Mirrors TimeAdvancementService pattern exactly.
2. **BatchActionPanel (NEW)** — Contextual action toolbar appears when selection exists, contains action buttons and selection count display.
3. **CharacterStatusCard (MODIFY)** — Add checkbox overlay for multi-select mode, selection state styling. No changes to existing click-to-open behavior.
4. **GmTable.razor (MODIFY)** — Add `selectedCharacterIds` HashSet, `isMultiSelectMode` flag, handle selection toggling and batch operation initiation.
5. **CharactersUpdatedMessage (REUSE)** — Existing message type already supports list of character IDs, perfect for batch notifications.

**Key architectural decisions:**
- **Sequential vs parallel execution**: Sequential only. CSLA business objects are not thread-safe, SQLite has limited concurrent write performance. Matches proven TimeAdvancementService pattern.
- **Selection state location**: Component-level in GmTable.razor. Simple, no DI overhead, matches existing selectedCharacter pattern.
- **Messaging pattern**: Single CharactersUpdatedMessage after batch completes with all affected IDs. Avoids N individual messages causing UI flood.

### Critical Pitfalls

Research identified six critical pitfalls that require architectural decisions to avoid rewrites.

1. **Message flooding from rapid batch updates** — Publishing CharacterUpdateMessage per character (10+ selected) floods Rx.NET message bus, causes cascading UI re-renders, activity log spam, browser performance degradation. **Prevention**: Publish single CharactersUpdatedMessage with all IDs after batch completes. TimeAdvancementService already demonstrates this pattern (lines 294-300).

2. **Partial failure obscuring errors** — Batch operations that continue after failures can leave inconsistent state. User doesn't know which characters succeeded/failed or why. **Prevention**: Return structured BatchActionResult with success list, failure list, per-character error messages. Mirror TimeAdvancementResult pattern (lines 16-57). UI must display: "Applied to 3 of 5 characters. Failed: [names] - [reasons]".

3. **CSLA validation running per-character without aggregation** — Validation errors appear one at a time as you process each character instead of upfront. **Prevention**: Implement pre-validation step that checks all characters BEFORE applying changes. Create BatchActionValidator that accepts action and character list, returns all validation failures upfront. Only proceed after validation passes or user explicitly overrides.

4. **UI selection state diverging from server state** — Selected characters may be removed by other clients during batch processing. Selection state becomes stale, subsequent operations target phantom characters. **Prevention**: Validate selection against current server state at execution time, not selection time. Clear/refresh selection when character list updates. Resolve selected IDs to current characters at execution.

5. **Mixed PC/NPC handling breaking authorization** — Batch selection includes both PCs and NPCs with different authorization rules (players can only modify their own PCs, GM can modify any). Batch operations may bypass per-character authorization checks. **Prevention**: Check authorization per-character in batch loop using CSLA's authorization rules. Filter selection based on current user's permissions BEFORE showing available characters.

6. **Blazor circuit timeout during long batch operations** — Processing 20+ characters takes several seconds. Circuit may timeout, StateHasChanged never reaches client, user sees frozen UI and clicks again creating duplicate operations. **Prevention**: Use progress indication for batches ("Processing 3 of 10..."), process in chunks with UI updates between chunks, set appropriate timeouts, never block UI thread for >2 seconds without feedback.

## Implications for Roadmap

Based on research, suggested three-phase structure with clear incremental delivery and risk mitigation.

### Phase 1: Selection Infrastructure (Foundation)
**Rationale:** All batch operations depend on selection. This phase delivers visible progress (GM can select characters) while establishing the foundation for all subsequent work. Addresses critical Pitfall #4 (selection state management) and Pitfall #5 (authorization foundation) early.

**Delivers:**
- Multi-character selection via checkboxes on CharacterStatusCard/NpcStatusCard
- Selection count display and visual feedback
- Select All / Deselect All per section (PCs, Hostile NPCs, Neutral NPCs, etc.)
- Multi-select mode toggle
- Selection state management in GmTable.razor

**Technical approach:**
- Add `HashSet<int> selectedCharacterIds` to GmTable.razor
- Add `bool isMultiSelectMode` flag
- Modify CharacterStatusCard to accept ShowCheckbox parameter, render checkbox overlay
- Update card click handler to respect multi-select mode (toggle selection vs open detail modal)

**Addresses features:**
- Multi-character selection (table stakes)
- Selection count display (table stakes)
- Select All / Deselect All (table stakes)

**Avoids pitfalls:**
- Pitfall #4 (selection state divergence) by establishing validation pattern upfront
- Pitfall #5 (authorization) by filtering selectable characters based on permissions

**Research flag:** SKIP — Standard UI pattern, well-documented in Radzen docs and existing codebase

---

### Phase 2: Batch Action Framework (Backend)
**Rationale:** Build the service layer and result aggregation pattern before adding UI complexity. This phase establishes the sequential processing with error aggregation pattern (mirroring TimeAdvancementService) and addresses critical Pitfalls #1 (message flooding) and #2 (partial failure handling) through architectural decisions rather than retrofitting.

**Delivers:**
- BatchActionService with DI registration
- BatchActionRequest / BatchActionResult DTOs
- Sequential processing with error aggregation
- Single CharactersUpdatedMessage publishing after batch
- Unit tests for batch logic

**Technical approach:**
- Create `GameMechanics/Batch/BatchActionService.cs`
- Implement sequential loop pattern from TimeAdvancementService
- Extract damage/healing logic from CharacterDetailGmActions for reuse
- Publish single CharactersUpdatedMessage with all affected character IDs
- Use DeterministicDiceRoller pattern for unit tests

**Addresses features:**
- Backend infrastructure for all batch operations
- Partial success reporting foundation (differentiator)

**Avoids pitfalls:**
- Pitfall #1 (message flooding) by publishing single batch notification
- Pitfall #2 (partial failure) through structured BatchActionResult type
- Pitfall #6 (circuit timeout) foundation via chunked processing capability

**Research flag:** SKIP — Pattern directly mirrors TimeAdvancementService, proven in codebase

---

### Phase 3: Batch Action UI (User-Facing)
**Rationale:** With selection and backend working, add the user-facing UI for batch operations. This phase delivers complete end-to-end batch damage and healing, the highest-value features per VTT research. Addresses Pitfall #3 (validation) through pre-validation UI patterns.

**Delivers:**
- BatchActionPanel component (contextual action bar)
- BatchActionModal for damage/healing configuration
- Result toast/dialog showing success/failure with details
- Complete batch damage/healing workflow
- End-to-end testing

**Technical approach:**
- Create BatchActionPanel as slide-in bar at bottom of character section
- Create BatchActionModal reusing CharacterDetailGmActions patterns
- Wire modal open from action panel when selection exists
- Show result feedback: "Applied 3 FAT damage to 5 characters" or detailed partial success
- Auto-clear selection after successful batch operation

**Addresses features:**
- Batch damage application (table stakes)
- Batch healing application (table stakes)
- Contextual action bar (table stakes)
- Clear success/failure feedback (table stakes)
- Partial success reporting with details (differentiator)

**Avoids pitfalls:**
- Pitfall #3 (validation) by adding pre-validation preview before apply
- Pitfall #6 (timeout) by showing progress indicator for large batches

**Research flag:** SKIP — UI patterns well-documented in Eleken and PatternFly sources

---

### Phase 4: Additional Batch Actions (Extension)
**Rationale:** Once core damage/healing workflow is validated, extend to other batch operations with simpler implementations. These actions have lower complexity but high GM value for workflow efficiency.

**Delivers:**
- Batch visibility toggle for NPCs (reveal/hide for surprise encounters)
- Batch dismiss/archive for NPCs (combat cleanup)
- Batch effect add/remove (differentiator, if resources permit)

**Technical approach:**
- Reuse BatchActionService infrastructure
- Add new action types to BatchActionType enum
- Implement simpler actions (visibility toggle is single boolean flip)
- Consider effect operations as optional stretch goal

**Addresses features:**
- Batch visibility toggle (table stakes)
- Batch dismiss/archive (table stakes)
- Batch effect add/remove (differentiator, optional)

**Research flag:** SKIP — Simple extensions of established patterns

---

### Phase Ordering Rationale

1. **Foundation-first approach**: Selection infrastructure (Phase 1) is prerequisite for all batch operations. Delivers visible progress while establishing patterns.

2. **Risk mitigation early**: Phase 2 addresses the two critical architectural pitfalls (message flooding, partial failure handling) through service design before UI complexity is added. Retrofitting these patterns later would require rework.

3. **Highest-value features first**: Phases 3-4 prioritize batch damage/healing (primary use case per VTT research) before niche operations like effect management.

4. **Pattern reuse throughout**: Each phase mirrors existing codebase patterns (TimeAdvancementService, CharacterDetailGmActions, existing message types), minimizing risk and learning curve.

### Research Flags

**Phases with standard patterns (skip research-phase):**
- **Phase 1**: Selection patterns well-documented in Radzen docs, existing codebase has card selection patterns
- **Phase 2**: TimeAdvancementService provides exact pattern, CSLA batch approach proven
- **Phase 3**: SaaS bulk action UX extensively researched (Eleken, PatternFly, Google AIP), Radzen UI components documented
- **Phase 4**: Simple extensions, no new research needed

**No phases require deeper research.** All patterns are proven in existing codebase or well-documented in consulted sources.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | No new dependencies, all capabilities verified in existing codebase and official docs |
| Features | MEDIUM | VTT ecosystem research from multiple sources (Foundry, Roll20), SaaS UX patterns from authoritative sources (Eleken, PatternFly), but Threa's card-based approach differs from token-based VTT selection |
| Architecture | HIGH | Direct pattern reuse from TimeAdvancementService (verified working code), CharacterDetailGmActions extraction straightforward, message types already exist |
| Pitfalls | HIGH | Critical pitfalls identified through codebase analysis (message flooding pattern visible in TimeAdvancementService fix), SaaS bulk action research (partial failure handling), Blazor circuit behavior documented |

**Overall confidence:** HIGH

### Gaps to Address

**Selection UX testing needed:** Research shows VTTs use token-based map selection while Threa uses card-based dashboard selection. The checkbox pattern is assumed from SaaS apps but needs validation with actual GM users. Consider usability testing of:
- Checkbox placement (corner overlay vs dedicated column)
- Multi-select mode toggle discoverability
- Visual feedback for selection state

**Performance at scale unknown:** Typical encounter size is 5-15 characters, but edge cases (mass combat with 50+ NPCs) are untested. Sequential processing at ~50-100ms per character means 5-second latency for 50 characters. Monitor during beta:
- Add progress indicator at threshold (recommend 10+ characters)
- Consider chunked processing if timeout issues emerge
- Single refresh after batch is critical (verified this pattern works in TimeAdvancementService)

**Effect batch operations complexity unclear:** Batch effect add/remove is marked as differentiator but may have hidden complexity in effect parameter configuration. Some effects require parameters (duration, intensity) that don't fit simple batch modal. Plan during Phase 4:
- Start with parameter-free effects only (Blessed, Stunned)
- Consider effect templates for parameterized effects
- May defer complex effects to individual application

## Sources

### Primary (HIGH confidence)
- **Threa codebase** (direct analysis):
  - `S:\src\rdl\threa\GameMechanics\Time\TimeAdvancementService.cs` — Batch processing pattern with error aggregation (lines 137-176), CharactersUpdatedMessage usage (lines 294-300)
  - `S:\src\rdl\threa\Threa\Threa.Client\Components\Pages\GamePlay\GmTable.razor` — Dashboard structure, selection patterns, character list management
  - `S:\src\rdl\threa\Threa\Threa.Client\Components\Shared\CharacterDetailGmActions.razor` — Single character action patterns for damage/healing extraction
  - `S:\src\rdl\threa\GameMechanics.Messaging.InMemory\InMemoryMessageBus.cs` — Rx.NET pub/sub infrastructure
  - `S:\src\rdl\threa\GameMechanics\Messaging\TimeMessages.cs` — CharactersUpdatedMessage definition (lines 387-403)
- **Radzen.Blazor documentation**:
  - [DataGrid Multiple Selection](https://blazor.radzen.com/datagrid-multiple-selection) — Official multi-select examples
  - [DataGrid API Reference](https://blazor.radzen.com/docs/api/Radzen.Blazor.RadzenDataGrid-1.html) — SelectionMode.Multiple, Value binding
- **SaaS UX Pattern Research**:
  - [Eleken: Bulk Actions UX Guidelines](https://www.eleken.co/blog-posts/bulk-actions-ux) — 8 design guidelines for bulk operations
  - [PatternFly: Bulk Selection Patterns](https://www.patternfly.org/patterns/bulk-selection/) — Enterprise UI selection patterns, scope hierarchy
  - [Google AIP-233: Batch Create](https://google.aip.dev/233) — Partial success API patterns
  - [Google AIP-234: Batch Update](https://google.aip.dev/234) — Batch operation response standards

### Secondary (MEDIUM confidence)
- **VTT Ecosystem**:
  - [Foundry VTT Multi Token Status Module](https://foundryvtt.com/packages/multistatus) — Batch status effect management patterns
  - [Foundry VTT Combat Tracker](https://foundryvtt.com/article/tokens/) — Token selection patterns
  - [Roll20 API Scripts](https://wiki.roll20.net/Mod:Script_Index) — TokenMod, ApplyDamage batch patterns
  - [D&D Beyond Encounter Builder](https://www.dndbeyond.com/posts/1135-tutorial-how-to-build-encounters-and-run-them-on-d) — Encounter management workflow
- **Additional UX Patterns**:
  - [HashiCorp Helios: Table Multi-Select](https://helios.hashicorp.design/patterns/table-multi-select) — Selection scope hierarchy
  - [SaaS Interface: Bulk Actions Examples](https://saasinterface.com/components/bulk-actions/) — 19 real-world SaaS examples

### Tertiary (LOW confidence)
- **Performance considerations**:
  - [Microsoft Learn: Blazor Server State Management](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/server?view=aspnetcore-10.0) — Circuit lifetime, timeout behavior
  - [Inngest: Rate Limiting, Debouncing, Throttling](https://www.inngest.com/blog/rate-limit-debouncing-throttling-explained) — Message flood prevention strategies
  - [CSLA Forum: Transaction Scope on Multiple Object Saves](https://cslanet.com/old-forum/10293.html) — CSLA-specific batch update discussions

---
*Research completed: 2026-02-04*
*Ready for roadmap: yes*
