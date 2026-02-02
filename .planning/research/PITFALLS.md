# Domain Pitfalls: NPC Management System

**Domain:** Adding NPC management to existing TTRPG character assistant
**Researched:** 2026-02-01
**Confidence:** HIGH (based on existing codebase analysis + industry patterns)

## Critical Pitfalls

Mistakes that cause rewrites or major issues if not addressed early.

### Pitfall 1: Template/Instance Identity Confusion

**What goes wrong:** The system conflates NPC templates (blueprints in the library) with NPC instances (active copies in a session). This leads to:
- Editing a template accidentally modifies all active instances
- Deleting a template orphans or corrupts active instances
- No way to track which template an instance came from
- Changes to an instance bleed back into the template

**Why it happens:** The existing `Character` and `ItemTemplates`/`Items` patterns work differently. Characters are unique entities, while items follow a clear template-instance separation. NPCs need the item pattern but developers may instinctively follow the character pattern since NPCs "feel" like characters.

**Consequences:**
- GM edits goblin stats mid-combat; all goblins in the campaign change
- GM deletes "Bandit" template; active bandits in encounters crash or vanish
- No audit trail of "this orc was created from template X but modified"
- Template versioning nightmares when templates evolve

**Prevention:**
1. Follow the existing `ItemTemplates` -> `Items` pattern exactly:
   - `NpcTemplates` table: blueprints with stats, abilities, equipment
   - `NpcInstances` table: runtime copies with `TemplateId`, `SessionId`, current state
2. Instance creation must deep-copy all mutable state (health, effects, inventory)
3. Template deletion requires validation: block if active instances exist, or orphan instances gracefully with a "template deleted" flag
4. Add `CreatedFromTemplateId` and `CreatedAt` fields to instances for traceability

**Detection:**
- Code review: Any direct modification of template properties after instance creation
- Test: Create instance, modify instance, verify template unchanged
- Test: Delete template, verify instance still functions

**Phase to address:** Phase 1 (Data Model) - establish the separation in the database schema from the start

---

### Pitfall 2: Dashboard Performance Collapse with Many NPCs

**What goes wrong:** The GM dashboard becomes unresponsive when displaying 20+ NPCs alongside player characters. The existing real-time update pattern (Rx.NET `Subject<T>` broadcasts) causes:
- Full re-renders on any NPC change
- N+1 query patterns fetching NPC details
- SignalR message flooding when multiple NPCs update simultaneously

**Why it happens:** The current `CharacterUpdateMessage` and dashboard subscription model works well for 4-6 player characters. NPCs can easily reach 30-50 entities in a large combat. The pattern doesn't scale.

**Consequences:**
- Dashboard freezes during large combats
- Browser memory exhaustion from excessive DOM nodes
- Real-time updates lag by seconds
- GM abandons the tool mid-session

**Prevention:**
1. **Virtualization:** Render only visible NPC cards (Blazor `Virtualize<T>` component)
2. **Batched updates:** Debounce NPC updates to 100-200ms intervals; batch multiple changes into single render cycles
3. **Separate message streams:** Add `NpcUpdateMessage` separate from `CharacterUpdateMessage` to enable selective subscription
4. **Summary-first pattern:** Dashboard shows NPC counts and summary; expand to details on demand
5. **Pagination or grouping:** NPCs organized by group/encounter with collapse/expand

**Detection:**
- Load test with 50+ NPCs active
- Monitor render times in browser dev tools
- Profile SignalR message frequency during batch operations

**Phase to address:** Phase 3 (Dashboard Integration) - design for scale from the start, not retrofitted

---

### Pitfall 3: Visibility State Leakage

**What goes wrong:** Hidden NPCs leak information to players through:
- Activity log messages mentioning hidden NPCs
- Targeting lists including hidden NPCs
- Combat initiative showing "???"-type entries that reveal ambush existence
- Real-time updates broadcasting hidden NPC changes to player clients

**Why it happens:** The existing architecture broadcasts updates to all connected clients via the `InMemoryMessageBus`. No concept of visibility filtering exists. Adding NPC visibility as an afterthought means retrofitting every broadcast point.

**Consequences:**
- Surprise encounters ruined by UI hints
- Players meta-game around hidden NPC presence
- GM loses trust in the system for dramatic reveals
- Inconsistent behavior: some features respect visibility, others don't

**Prevention:**
1. **Visibility as first-class concept:** Add `IsVisibleToPlayers` flag to NPC instances
2. **Message filtering layer:** Introduce client-filtering in message publishing:
   ```csharp
   // Instead of broadcasting to all
   _messageBus.PublishNpcUpdate(npc);
   // Filter by visibility
   _messageBus.PublishNpcUpdate(npc, visibleToClientIds: GetClientsWhoCanSee(npc));
   ```
3. **GM-only message stream:** Create `GmOnlyMessages` subject for hidden NPC updates
4. **Audit all broadcast points:** Systematically review every `Publish*` call for NPC awareness
5. **Activity log filtering:** Add `VisibleTo` property to `ActivityLogMessage`

**Detection:**
- Test: Create hidden NPC, act with it, verify player client receives no updates
- Test: Reveal hidden NPC, verify player client receives update
- Review: Search codebase for all `Publish` calls and verify NPC visibility handling

**Phase to address:** Phase 2 (Core CRUD) - bake visibility into the NPC model from day one

---

### Pitfall 4: Group Operations Partial Failure Chaos

**What goes wrong:** Batch operations (damage all goblins, heal all allies) fail partially with no clear recovery path:
- 3 of 5 NPCs updated, 2 fail due to validation
- No rollback mechanism leaves inconsistent state
- Error messages don't identify which NPCs failed
- UI shows success while some operations silently failed

**Why it happens:** The existing single-character manipulation pattern doesn't handle batch operations. Developers apply damage one-by-one in a loop without transaction boundaries or error aggregation.

**Consequences:**
- GM applies "10 damage to all orcs" - some orcs take damage, some don't, unclear which
- Partial state requires manual cleanup
- GM trust erodes; they stop using batch features
- Combat slows down as GM applies changes individually "to be safe"

**Prevention:**
1. **All-or-nothing operations:** Wrap batch operations in database transactions
2. **Error aggregation:** Collect all failures, report them together:
   ```csharp
   var results = await ApplyDamageToGroup(npcs, damage);
   // results.Succeeded: [npc1, npc2, npc3]
   // results.Failed: [(npc4, "Already dead"), (npc5, "Effect immunity")]
   ```
3. **Preview mode:** Show what would happen before committing batch action
4. **Detailed feedback UI:** List succeeded/failed items clearly; don't just show "partial success"
5. **Undo support:** Enable reverting batch operations (especially important for mistakes)

**Detection:**
- Test: Batch operation with one invalid target; verify correct behavior for valid targets
- Test: Batch operation with all failures; verify graceful handling
- UX review: Can GM understand what happened after partial failure?

**Phase to address:** Phase 4 (Group Management) - design the batch pattern before implementing features

---

## Moderate Pitfalls

Mistakes that cause delays or technical debt.

### Pitfall 5: Keep vs Dismiss Persistence Ambiguity

**What goes wrong:** The NPC persistence model (keep for future sessions vs dismiss after encounter) is unclear to GMs:
- "Dismiss" deletes the NPC entirely when GM expected to archive it
- "Keep" creates clutter; GM has hundreds of "kept" NPCs with no organization
- State at dismissal not preserved for resurrection scenarios
- No distinction between "dead and gone" vs "retreated and may return"

**Why it happens:** The feature seems simple but has subtle UX requirements that aren't specified upfront.

**Prevention:**
1. **Three-state model:** Active (in session), Archived (kept for later), Dismissed (gone but logged)
2. **Clear UI language:**
   - "Archive" = remove from active play, keep all state, can restore
   - "Dismiss" = remove from session, keep minimal record for logs
   - "Delete" = permanent removal (GM confirmation required)
3. **State preservation:** Archived NPCs retain health, effects, inventory exactly as archived
4. **Archive organization:** Tags, folders, or encounter groupings for archived NPCs
5. **Resurrection workflow:** Clear path to "bring back" an archived NPC

**Detection:**
- UX testing: Ask GM to "save this NPC for later" - do they find the right action?
- Test: Archive NPC, restore it - is state identical?

**Phase to address:** Phase 5 (Persistence) - define the mental model before building UI

---

### Pitfall 6: Initiative Integration Collision

**What goes wrong:** NPCs in combat don't integrate cleanly with the existing initiative and round management:
- `RoundManager` expects `CharacterId` but NPCs have `NpcInstanceId`
- Initiative messages broadcast NPC details to players (visibility leak)
- Multiple NPCs with same initiative score create ordering ambiguity
- NPC removal mid-combat doesn't clean up initiative state

**Why it happens:** The existing `CombatState` and `CombatStateManager` use `string combatantId` which is generic, but downstream code may assume these are character IDs.

**Prevention:**
1. **Combatant abstraction:** Introduce `ICombatant` interface or discriminated ID type:
   ```csharp
   public record CombatantId(string Type, string Id); // ("Character", "123") or ("Npc", "456")
   ```
2. **Visibility-aware initiative:** Initiative display filters NPCs by visibility
3. **Tie-breaking rules:** Define and implement NPC initiative ties (e.g., DEX tiebreaker, then alphabetical)
4. **Cleanup hooks:** NPC removal triggers `CombatStateManager.RemoveCombatant`

**Detection:**
- Test: Add NPC to combat, verify initiative works
- Test: Hide NPC, verify player doesn't see it in initiative
- Test: Kill/remove NPC mid-combat, verify no dangling state

**Phase to address:** Phase 3 (Dashboard Integration) - when NPCs appear alongside characters

---

### Pitfall 7: Template Library Discovery Nightmare

**What goes wrong:** GM can't find the NPC template they need:
- Hundreds of templates with no organization
- Search only matches exact names
- No preview of template stats before instantiating
- Custom templates mixed with system templates

**Why it happens:** Template management is treated as a secondary concern; focus goes to instance management.

**Prevention:**
1. **Hierarchical categories:** Monster Type > Subtype > Specific (e.g., Humanoid > Goblinoid > Goblin Warrior)
2. **Tagging system:** CR/level, environment, faction, custom tags
3. **Full-text search:** Search name, description, abilities, equipment
4. **Quick preview:** Hover or click to see full stat block before instantiation
5. **Favorites/recents:** Track frequently used templates for quick access
6. **User vs system templates:** Clear visual distinction; user templates editable, system templates not

**Detection:**
- UX test: "Find and spawn a medium-difficulty fire-based creature" - how long does it take?
- Test: 100+ templates in library - is navigation still reasonable?

**Phase to address:** Phase 1 (Data Model) for structure, Phase 2 (CRUD) for UI

---

### Pitfall 8: Equipment and Effect System Divergence

**What goes wrong:** NPC equipment and effects don't use the same systems as characters:
- NPC weapons don't follow `ItemTemplates` pattern
- NPC effects don't integrate with `CharacterEffects` table
- Damage calculation bypasses standard resolver because NPC data is structured differently
- Spell effects on NPCs don't expire correctly via time skip

**Why it happens:** Pressure to ship quickly leads to NPC-specific shortcuts that diverge from the established character systems.

**Consequences:**
- Two code paths for damage calculation (character vs NPC)
- Effects work differently on NPCs than characters
- Bug fixes need to be applied in multiple places
- Eventually, systems drift apart and become unmaintainable

**Prevention:**
1. **Single source of truth:** NPCs use the same `Item`, `CharacterEffect` (renamed to just `Effect`), and skill systems
2. **Composition over duplication:** If NPCs need simpler data, create simplified views or DTOs, not separate tables
3. **Resolver reuse:** `AttackResolver`, `DamageResolver` etc. work with interfaces, not concrete character types
4. **Time system integration:** NPCs register with `RoundManager` for effect expiration

**Detection:**
- Code review: Grep for NPC-specific damage/effect handling
- Test: Apply same effect to character and NPC; verify identical behavior
- Test: Time skip expires NPC effects correctly

**Phase to address:** Phase 2 (Core CRUD) - establish pattern of reuse, not duplication

---

## Minor Pitfalls

Mistakes that cause annoyance but are fixable.

### Pitfall 9: Naming Collision with Player Characters

**What goes wrong:** GM creates NPC named "Elara" when a player character is also named "Elara":
- Activity log becomes confusing ("Elara attacks Elara")
- Targeting lists show duplicate names
- Search returns both without clear distinction

**Prevention:**
1. **Warn on collision:** Alert GM when creating NPC with name matching a character
2. **Visual distinction:** NPCs have different icon/badge than characters everywhere
3. **Qualified names in logs:** "[NPC] Elara attacks [PC] Elara" or "Elara (Goblin) attacks Elara (Elf)"
4. **Unique identifier fallback:** System can reference by ID if names collide

**Phase to address:** Phase 2 (Core CRUD) - validation rule during NPC creation

---

### Pitfall 10: Bulk Spawn Floods Session

**What goes wrong:** GM spawns "10 Goblins" and dashboard shows 10 nearly-identical cards cluttering the UI:
- Can't tell goblins apart (Goblin 1, Goblin 2, ... Goblin 10)
- Each has full stat block visible, wasting space
- Batch actions require selecting each individually

**Prevention:**
1. **Auto-naming:** Goblin Alpha, Goblin Beta, or Goblin 1, Goblin 2 with easy bulk-rename
2. **Collapsed group view:** "Goblins (10)" expands to show individuals
3. **Shared stat display:** One stat block for identical NPCs, individual health/effects only
4. **Bulk selection:** Checkbox to select entire group for batch actions

**Phase to address:** Phase 4 (Group Management) - group display patterns

---

### Pitfall 11: Real-Time Update Race Conditions

**What goes wrong:** Rapid NPC updates create race conditions:
- GM applies damage while another update is in flight
- UI shows stale health briefly, then corrects
- Optimistic updates conflict with server state

**Prevention:**
1. **Sequence numbers:** Each NPC update includes sequence; ignore out-of-order updates
2. **Server-authoritative state:** Don't optimistically update; wait for server confirmation
3. **Debounced UI updates:** Coalesce rapid changes into single render

**Phase to address:** Phase 3 (Dashboard Integration) - as part of real-time infrastructure

---

### Pitfall 12: Missing Audit Trail for NPC Actions

**What goes wrong:** After session, GM can't review what happened to NPCs:
- No record of damage dealt/taken by NPCs
- Effect applications not logged
- Can't reconstruct combat for dispute resolution

**Prevention:**
1. **Activity log integration:** All NPC actions logged exactly like character actions
2. **NPC-specific log filtering:** View "actions by/to Goblin Chief"
3. **Session summary:** NPC participation stats (damage dealt, taken, kills, deaths)

**Phase to address:** Phase 3 (Dashboard Integration) - activity log integration

---

## Phase-Specific Warnings

| Phase Topic | Likely Pitfall | Mitigation |
|-------------|---------------|------------|
| Data Model | Template/Instance confusion | Follow ItemTemplates pattern exactly |
| Data Model | Equipment divergence | Reuse existing Item/Effect tables |
| Core CRUD | Visibility leakage | Add visibility to model on day one |
| Core CRUD | Name collisions | Validation rule warning |
| Dashboard Integration | Performance collapse | Virtualization + batching from start |
| Dashboard Integration | Initiative collision | Combatant abstraction |
| Group Management | Partial failure chaos | Transaction + error aggregation pattern |
| Group Management | Bulk spawn UI flood | Collapsed group view |
| Persistence | Keep/dismiss ambiguity | Three-state model with clear UX |

## Integration Warnings Specific to Threa

Based on analysis of existing codebase:

### InMemoryMessageBus Expansion

The `InMemoryMessageBus` currently has 15 subject types. Adding NPC support will require:
- `NpcUpdateMessage` (separate from `CharacterUpdateMessage`)
- `NpcGroupUpdateMessage` (for batch operations)
- Visibility filtering capability

**Risk:** Bus becomes unwieldy. Consider message inheritance or generic `EntityUpdateMessage<T>`.

### CSLA Business Object Pattern

NPCs should follow CSLA patterns:
- `NpcTemplateEdit` extends `BusinessBase<T>` for templates
- `NpcInstanceEdit` extends `BusinessBase<T>` for instances
- `NpcTemplateList` / `NpcInstanceList` for collections
- DAL interfaces: `INpcTemplateDal`, `INpcInstanceDal`

**Risk:** Shortcutting CSLA patterns to ship faster creates technical debt.

### Existing Character Model Reuse

The `CharacterEdit` has 7 attributes, skills, equipment, effects. NPCs should:
- Share attribute definitions (`STR`, `DEX`, etc.)
- Share skill system (`SkillDefinitions`)
- Use same `Item` table for equipment
- Use same `CharacterEffects` table (rename to `EntityEffects`?)

**Risk:** Creating parallel NPC-specific tables instead of reusing character tables.

## Sources

Research compiled from:
- [Mythcreants - Eight Tips For Managing NPCs](https://mythcreants.com/blog/eight-tips-for-managing-npcs/) - naming and management advice
- [Microsoft Learn - Blazor Performance Best Practices](https://learn.microsoft.com/en-us/aspnet/core/blazor/performance/) - virtualization and rendering optimization
- [Eleken - Bulk Actions UX Guidelines](https://www.eleken.co/blog-posts/bulk-actions-ux) - batch operation error handling patterns
- [NN/g - Cancel vs Close](https://www.nngroup.com/articles/cancel-vs-close/) - dismiss/archive UX patterns
- [N3 Game Database Structure](https://floooh.github.io/2009/02/21/n3s-game-database-structure.html) - template vs instance database design
- [World Anvil - Visibility Toggle](https://blog.worldanvil.com/2020/02/12/introducing-the-visibility-toggle-show-to-your-players-only-what-you-want-them-to-see) - TTRPG visibility patterns
- Existing Threa codebase analysis: `DATABASE_DESIGN.md`, `InMemoryMessageBus.cs`, `CombatState.cs`

---

*This pitfalls document should be reviewed during phase planning to ensure each phase addresses relevant warnings.*
