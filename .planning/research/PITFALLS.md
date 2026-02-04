# Pitfalls Research: Batch Character Actions

**Domain:** Adding batch character operations to existing TTRPG assistant
**Researched:** 2026-02-04
**Confidence:** HIGH (based on existing codebase patterns and batch operation best practices)

## Critical Pitfalls

Mistakes that cause rewrites or major issues if not addressed early.

### Pitfall 1: Message Flooding from Rapid Batch Updates

**What goes wrong:**
When applying batch actions to multiple characters, each individual character update triggers a `PublishCharacterUpdateAsync` call. With 10+ characters selected, this floods the Rx.NET message bus, causing:
- UI re-renders cascading across all connected clients
- Activity log spam making it unreadable
- Potential race conditions as multiple StateHasChanged calls overlap
- Browser performance degradation from excessive DOM updates

**Why it happens:**
The existing individual operation pattern (seen in `TimeAdvancementService.cs`) was designed for single-character updates. When iterating through multiple characters, developers naturally call the same publish method per character without considering the aggregate message volume.

**How to avoid:**
- Implement batch-aware messaging: publish a single `BatchUpdateMessage` containing all affected character IDs
- Use the existing `CharactersUpdatedMessage` pattern (lines 294-300 in TimeAdvancementService.cs) which already supports multiple character IDs
- Add debouncing/throttling on the subscriber side for activity log display
- Consider coalescing updates: delay UI refresh until batch completes, then refresh once

**Warning signs:**
- Activity log shows rapid-fire individual entries during batch operations
- UI flickers or stutters during batch actions
- Network traffic spikes during batch operations
- Test logs show many sequential publish calls instead of single batched publish

**Phase to address:**
Phase 1 - Foundation. Define the batch messaging contract before implementing any batch operations.

---

### Pitfall 2: Partial Failure Obscuring Errors

**What goes wrong:**
Batch operations that continue after individual failures can leave the system in an inconsistent state. For example:
- Apply damage to 5 characters: 3 succeed, 2 fail due to validation
- UI shows "operation complete" but 2 characters weren't updated
- User doesn't know which characters failed or why
- Subsequent actions based on assumed state cause confusion

**Why it happens:**
The existing `TimeAdvancementResult` pattern (lines 16-57 in TimeAdvancementService.cs) shows the correct approach with separate `UpdatedCharacterIds` and `FailedCharacterIds` lists. However, developers often implement simpler try/catch that swallows errors and continues.

**How to avoid:**
- Mirror the `TimeAdvancementResult` pattern for all batch operations
- Always return a structured result with: success list, failure list, error messages per failure
- UI must display partial failure states clearly: "Applied to 3 of 5 characters. Failed: [names] - [reasons]"
- Never commit partial transactions if atomicity is required for the operation
- Log all failures even if operation "mostly succeeded"

**Warning signs:**
- Batch methods return simple bool or void instead of structured results
- No per-character error tracking in batch loops
- UI only shows "success" or "failure" without details
- Integration tests only check happy path, not partial failure scenarios

**Phase to address:**
Phase 1 - Foundation. Establish the batch result pattern before any batch operations are implemented.

---

### Pitfall 3: CSLA Validation Running Per-Character Without Aggregation

**What goes wrong:**
CSLA business objects run validation rules when properties change. In a batch operation:
- Select 10 characters
- Apply "-5 FAT damage" to each
- 3 characters have FAT < 5, so validation fails
- Validation errors appear one at a time as you process each character
- User sees confusing sequence of errors instead of upfront validation

**Why it happens:**
CSLA's validation model is designed for individual object editing, not batch previewing. The `CharacterEdit.AddBusinessRules()` method (line 902) sets up rules that trigger during property modification, not before.

**How to avoid:**
- Implement a "pre-validation" step that checks all characters BEFORE applying changes
- Create a `BatchActionValidator` that accepts the action and character list, returns all validation failures upfront
- Only proceed to apply changes after pre-validation passes (or user explicitly overrides)
- For mixed success scenarios, show which characters will succeed/fail before committing

**Warning signs:**
- Batch operations start applying changes before checking all characters
- Validation errors appear mid-operation instead of upfront
- Users ask "why did it stop halfway through?"
- No way to preview which characters an action will affect

**Phase to address:**
Phase 2 - Batch Action Framework. Build validation preview into the core batch action infrastructure.

---

### Pitfall 4: UI Selection State Diverging from Server State

**What goes wrong:**
User selects 5 characters, applies action, but:
- During processing, another client removes one character from the table
- Server processes 4 characters successfully
- UI still shows 5 selected
- Character list refreshes, selected character gone but selection state stale
- Subsequent batch actions target phantom character

**Why it happens:**
Blazor Server maintains UI state in circuits. The `GmTable.razor` (line 509) tracks `tableCharacters` and refreshes via `RefreshCharacterListAsync()`, but `selectedCharacter` is maintained separately. In batch mode with multiple selections, this divergence compounds.

**How to avoid:**
- Validate selection against current server state at execution time, not selection time
- Clear/refresh selection when character list updates (extend the pattern at lines 709-711)
- Use optimistic UI carefully: show immediate feedback but reconcile with server state on completion
- Consider "selection is a filter, not a state" pattern: resolve selected IDs to current characters at execution

**Warning signs:**
- Selected character count doesn't match actual characters in batch result
- Users report actions applied to wrong characters
- UI shows selected state for characters no longer at table
- Race conditions in tests when adding/removing characters during batch operations

**Phase to address:**
Phase 2 - Batch Action Framework. Selection management is part of the batch infrastructure.

---

### Pitfall 5: Mixed PC/NPC Handling Breaking Authorization

**What goes wrong:**
Batch selection includes both PCs (player characters) and NPCs. Authorization rules differ:
- Players can only modify their own PCs
- GM can modify any character
- Batch applies GM-level action but includes player's PC
- Player's PC modified without proper authorization check

**Why it happens:**
Individual character operations check authorization at the CSLA business object level or route level. Batch operations that iterate through characters may bypass these checks if they use an elevated service context.

**How to avoid:**
- Check authorization per-character, not per-batch
- Use CSLA's authorization rules even in batch loops
- Filter selection based on current user's permissions BEFORE showing available characters
- For GM actions, clearly indicate "this action will affect X PCs and Y NPCs"
- Consider separate batch action sets for "my characters" vs "GM actions"

**Warning signs:**
- Batch operations use raw DAL calls instead of going through CSLA portal
- No per-character authorization checks in batch loops
- Players can select NPCs they shouldn't modify
- Tests don't cover mixed PC/NPC batch scenarios with different user contexts

**Phase to address:**
Phase 1 - Foundation. Authorization model must be established before any batch operations.

---

### Pitfall 6: Blazor Circuit Timeout During Long Batch Operations

**What goes wrong:**
Batch operation processing 20+ characters takes several seconds. During this time:
- Blazor Server circuit has no activity from user
- If processing takes too long, circuit may timeout
- StateHasChanged never reaches the client
- User sees frozen UI, clicks again, creates duplicate operations

**Why it happens:**
The `GmTable.razor` processes time advancement synchronously (lines 884-899). For batch operations with more characters or more complex actions, this pattern doesn't scale.

**How to avoid:**
- Use progress indication for batch operations (show "Processing 3 of 10...")
- Process in chunks with UI updates between chunks
- Consider background processing with SignalR notification on completion
- Set appropriate timeouts and show clear feedback during long operations
- Never block UI thread for more than ~2 seconds without feedback

**Warning signs:**
- No loading indicator during batch operations
- Users report frozen UI
- Duplicate operations in logs (user clicked multiple times)
- Tests timeout when batch size exceeds certain threshold

**Phase to address:**
Phase 3 - Batch Action UI. Progress feedback is part of the UI implementation.

---

## Technical Debt Patterns

Shortcuts that seem reasonable but create long-term problems.

| Shortcut | Immediate Benefit | Long-term Cost | When Acceptable |
|----------|-------------------|----------------|-----------------|
| Looping individual saves in batch | Simple implementation | N+1 database round-trips, slow at scale | Prototyping only; never in production |
| Single error message for batch failures | Less code | Users can't identify which items failed | Never for user-facing operations |
| Reusing single-item UI for batch preview | No new UI needed | Confusing UX, can't see full batch scope | Early alpha only |
| Publishing one message per character | Existing pattern works | Message flooding, UI churn | Never for batch > 3 items |
| Ignoring selection validation | Faster batch execution | Phantom selections cause confusing failures | Never |

## Integration Gotchas

Common mistakes when connecting to existing Threa systems.

| Integration | Common Mistake | Correct Approach |
|-------------|----------------|------------------|
| Activity Log Service | Publishing N individual messages for N characters | Publish single batch message: "Applied [action] to [count] characters: [list]" |
| Time Event Publisher | Calling `PublishCharacterUpdateAsync` in loop | Use `PublishCharactersUpdatedAsync` with ID list (already exists in TimeAdvancementService!) |
| CSLA Data Portal | Fetching all characters sequentially before batch | Pre-load in parallel using `Task.WhenAll` for character fetches |
| Rx.NET Message Bus | No backpressure consideration | Use `Buffer` or `Throttle` operators on subscriber side for UI updates |
| Character Status Cards | Re-rendering entire list on each update | Use proper `@key` directives (already done at line 178) and batch refresh |

## Performance Traps

Patterns that work at small scale but fail as usage grows.

| Trap | Symptoms | Prevention | When It Breaks |
|------|----------|------------|----------------|
| Sequential character fetches | Batch of 10 takes 10x single operation time | Parallel fetch with `Task.WhenAll`, or use batch fetch DAL method | > 5 characters |
| Full character reload after batch | High latency between action and UI update | Use partial state updates, only refresh affected properties | > 3 characters |
| Unbounded message history in activity log | Memory grows, scroll performance degrades | Already has 100-item cap (line 746), ensure batch messages count as 1 | > 50 batch operations/session |
| Individual validation per character | O(n * rules) validation time | Batch validation with early termination on fatal errors | > 10 characters with complex validation |

## Security Mistakes

Domain-specific security issues beyond general web security.

| Mistake | Risk | Prevention |
|---------|------|------------|
| Batch action bypasses per-character authorization | Player could modify other players' characters | Check authorization for each character in batch, filter before execution |
| GM actions applied to PCs without logging | Accountability lost for GM modifications | Log all GM-initiated batch actions with explicit "GM modified [PC name]" entries |
| Batch selection includes hidden NPCs player shouldn't see | Information disclosure | Filter selection list based on `VisibleToPlayers` flag for non-GM users |

## UX Pitfalls

Common user experience mistakes in batch operations.

| Pitfall | User Impact | Better Approach |
|---------|-------------|-----------------|
| No confirmation for destructive batch actions | Accidental mass damage/kills | Show confirmation: "Apply 10 damage to 8 characters? This will kill 2 of them." |
| Success message without details | User doesn't know what happened | "Applied 5 FAT damage to: Warrior, Mage, Thief. Skipped: Paladin (already at 0 FAT)" |
| No undo for batch actions | Mistakes are permanent | At minimum, provide detailed log for manual reversal; ideally implement batch undo |
| Batch toolbar appears/disappears unpredictably | Disorienting, users lose context | Keep batch toolbar persistent while any characters selected; show selection count |
| No visual distinction between selected PCs and NPCs | GM applies wrong action scope | Color-code or group selection: "2 PCs, 5 NPCs selected" |

## "Looks Done But Isn't" Checklist

Things that appear complete but are missing critical pieces.

- [ ] **Batch selection:** Often missing keyboard shortcuts (Shift+click for range, Ctrl+click for toggle) -- verify full selection patterns work
- [ ] **Batch preview:** Often missing validation failures shown upfront -- verify all characters checked before "Apply"
- [ ] **Batch result:** Often missing per-character breakdown -- verify failure details shown, not just count
- [ ] **Batch messages:** Often missing message coalescing -- verify activity log shows one entry, not N entries
- [ ] **Batch progress:** Often missing progress indicator for >5 characters -- verify UI shows "Processing 3 of 10"
- [ ] **Batch authorization:** Often missing per-character auth checks -- verify player can't batch-modify NPCs they don't own
- [ ] **Batch undo/recovery:** Often missing recovery path -- verify user can identify and manually fix partial failures

## Recovery Strategies

When pitfalls occur despite prevention, how to recover.

| Pitfall | Recovery Cost | Recovery Steps |
|---------|---------------|----------------|
| Message flooding | LOW | Add throttling to subscriber, deploy without full rollback |
| Partial failure not shown | MEDIUM | Add detailed logging immediately, backfill UI reporting |
| UI state divergence | MEDIUM | Force full refresh on batch complete, add selection validation |
| Authorization bypass | HIGH | Audit logs for unauthorized modifications, add retroactive permission checks, notify affected players |
| Circuit timeout | LOW | Add loading indicator, reduce batch processing time per chunk |
| Validation errors mid-operation | MEDIUM | Roll back applied changes if possible, show clear status of what succeeded |

## Pitfall-to-Phase Mapping

How roadmap phases should address these pitfalls.

| Pitfall | Prevention Phase | Verification |
|---------|------------------|--------------|
| Message flooding | Phase 1 - Foundation | Single `CharactersUpdatedMessage` published per batch operation |
| Partial failure obscuring | Phase 1 - Foundation | Batch result type returns success/failure lists with reasons |
| CSLA validation aggregation | Phase 2 - Batch Framework | Pre-validation step checks all characters before any changes |
| UI selection divergence | Phase 2 - Batch Framework | Selection validated against server state at execution time |
| Mixed PC/NPC authorization | Phase 1 - Foundation | Per-character authorization checks in batch loop |
| Circuit timeout | Phase 3 - Batch UI | Progress indicator and chunked processing for large batches |

## Sources

- Threa codebase analysis: `TimeAdvancementService.cs` (existing batch pattern), `GmTable.razor` (existing UI patterns), `InMemoryMessageBus.cs` (messaging infrastructure), `CharacterEdit.cs` (CSLA business rules)
- [Microsoft Learn - Blazor Server state management](https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management/server?view=aspnetcore-10.0) - Circuit lifetime and state preservation
- [Eleken - Bulk action UX guidelines](https://www.eleken.co/blog-posts/bulk-actions-ux) - Partial failure feedback and confirmation patterns
- [Adidas API Guidelines - Batch Operations](https://adidas.gitbook.io/api-guidelines/rest-api-guidelines/execution/batch-operations) - Error handling and partial commits
- [Inngest - Rate limiting, debouncing, throttling](https://www.inngest.com/blog/rate-limit-debouncing-throttling-explained) - Message flood prevention strategies
- [CSLA Forum - Transaction Scope on multiple object saves](https://cslanet.com/old-forum/10293.html) - CSLA-specific batch update patterns
- [PatternFly - Bulk selection](https://www.patternfly.org/patterns/bulk-selection/) - Selection state reflection patterns

---
*Pitfalls research for: Batch Character Actions (Threa v1.6)*
*Researched: 2026-02-04*
