# Project Research Summary

**Project:** Threa TTRPG Assistant v1.5 - NPC Management System
**Domain:** TTRPG Game Master Tools
**Researched:** 2026-02-01
**Confidence:** HIGH

## Executive Summary

The NPC Management System extends Threa's existing TTRPG character assistant to support GM-controlled non-player characters alongside player characters. Research reveals that the codebase already has substantial NPC infrastructure in place (TableNpc DTO, DAL operations, SQLite tables), but the current approach uses a simplified stat block model that will create maintenance nightmares and feature limitations.

**Recommended approach:** Reuse the existing `CharacterEdit` business object with an `IsNpc` flag instead of maintaining parallel NPC-specific models. This provides 100% feature parity (NPCs get same stats, skills, equipment, effects, wounds as PCs), allows all existing GM manipulation tools to work unchanged, and maintains a single code path for combat, time advancement, and effects processing. The trade-off is slightly more storage per NPC and more complex creation, but templates solve the creation speed concern while preserving full capabilities.

**Key risks:** The primary risks are performance collapse with 20+ NPCs (mitigated by virtualization and batched updates), visibility state leakage to players (mitigated by first-class visibility filtering in messaging), and template/instance confusion (mitigated by following the proven ItemTemplate pattern exactly). All critical pitfalls can be avoided by architectural decisions in Phase 1 and 2.

## Key Findings

### Recommended Stack

**No new external packages needed.** The existing stack is fully capable of implementing all NPC management features. The key insight is that Threa already has a proven template pattern (ItemTemplate/EffectTemplate), real-time messaging infrastructure (Rx.NET InMemoryMessageBus), table-scoped character management, and comprehensive UI components (Radzen DataGrid with batch selection).

**Core technologies (existing):**
- **CSLA.NET 9.1.0**: Business object framework - NPCs use same CharacterEdit model with IsNpc flag
- **Radzen.Blazor 8.4.2**: DataGrid with SelectionMode.Multiple for batch NPC selection and manipulation
- **System.Reactive 6.0.1**: Rx.NET messaging for real-time NPC updates (add NpcUpdateMessage to InMemoryMessageBus)
- **SQLite with JSON**: TableNpc DTO extensible via JSON blob; add IsVisible, GroupName, NpcTemplateId columns

**What NOT to add:**
- SignalR (Rx.NET + Blazor Server already handles real-time updates)
- Entity Framework (SQLite with JSON serialization simpler and working)
- Separate NPC database (NPCs are table-scoped like PCs)

### Expected Features

Research shows VTT ecosystem (Foundry, Roll20, D&D Beyond) and TTRPG community expect certain baseline features for NPC management. Threa's differentiator is the existing dashboard with real-time updates and full character model support.

**Must have (table stakes):**
- NPC template library for encounter prep
- Quick NPC creation from template during play
- NPC status cards in GM dashboard (same visibility as PCs for health/effects/wounds)
- NPC detail modal with same GM manipulation powers (damage, healing, effects)
- NPC visibility toggle for surprise encounters (hide/reveal)
- Initiative tracking integration (NPCs in combat rounds)
- NPC removal/dismiss when defeated or no longer needed

**Should have (competitive differentiators):**
- Full character stats for NPCs (not simplified stat blocks) - consistent with Threa's design
- NPC grouping with batch actions (apply damage/effects to multiple NPCs)
- Template categories/tags for filtering library
- GM notes per NPC instance
- Smart auto-naming (Goblin 1, Goblin 2)
- Combat disposition markers (Hostile/Neutral/Friendly)

**Defer (v2+ or anti-features):**
- AI-powered NPC generation (scope creep)
- NPC-to-PC conversion (edge case complexity)
- Automatic CR calculation (D&D-specific, not applicable to Threa's system)
- Map/token integration (Threa is not a VTT)
- NPC memory/conversation AI (far outside scope)

### Architecture Approach

The codebase already has NPC foundation (TableNpc DTO, ITableDal with NPC CRUD, TableNpcs SQLite table, NpcPlaceholder component), but the lightweight TableNpc approach creates divergence from the character model. **Recommendation: Use CharacterEdit with IsNpc flag for full feature parity.**

**Major components:**
1. **Data Model (enhanced)** - Add IsNpc, IsTemplate, VisibleToPlayers flags to Character table; NPCs use same model as PCs
2. **Templates** - NpcTemplate follows ItemTemplate pattern (ReadOnly library), NpcTemplateEdit (editable), instantiation creates CharacterEdit with IsNpc=true
3. **Dashboard Integration** - NpcSection.razor replaces NpcPlaceholder; NpcStatusCard reuses CharacterStatusCard pattern; CharacterDetailModal works unchanged
4. **Time & Combat** - TimeAdvancementService extends to process NPCs alongside PCs; single code path for effects, wounds, health
5. **Messaging** - Add NpcUpdateMessage to InMemoryMessageBus; visibility filtering layer for player clients
6. **Group Management** - Batch operations use LINQ with transaction boundaries and error aggregation

**Key pattern:** Composition over duplication. NPCs use same Item, Effect, Skill, Attribute systems. AttackResolver, DamageResolver, ActionResolver work unchanged.

### Critical Pitfalls

Research identified 12 pitfalls across critical/moderate/minor severity. Top 5 to address early:

1. **Template/Instance Identity Confusion** - Avoid conflating NPC templates (blueprints) with instances (active copies). Follow ItemTemplate pattern exactly: deep-copy on instantiation, instances independent from templates, template deletion doesn't affect active NPCs. Address in Phase 1 data model.

2. **Dashboard Performance Collapse** - With 20+ NPCs, dashboard freezes without virtualization. Use Blazor Virtualize component, batch NPC updates (100-200ms debounce), separate NpcUpdateMessage from CharacterUpdateMessage, summary-first display with expand-on-demand. Address in Phase 3 dashboard integration.

3. **Visibility State Leakage** - Hidden NPCs leak info to players via activity log, targeting lists, real-time updates. Add IsVisibleToPlayers flag to NPC model on day one, filter message publishing by visibility, create GmOnlyMessages stream, audit all broadcast points. Address in Phase 2 core CRUD.

4. **Group Operations Partial Failure** - Batch damage to 5 NPCs where 2 fail leaves inconsistent state. Wrap batch operations in transactions, aggregate errors (list succeeded/failed NPCs separately), provide preview mode, clear detailed feedback UI. Address in Phase 4 group management.

5. **Equipment and Effect System Divergence** - Shortcutting with NPC-specific damage/effect handling creates two code paths. NPCs must use same Item, CharacterEffect, Skill systems; resolvers work with interfaces; time system integration for effect expiration. Address in Phase 2 by establishing reuse pattern.

## Implications for Roadmap

Based on research, suggested phase structure with dependencies and rationale:

### Phase 1: Data Model Foundation
**Rationale:** Everything depends on the data model. Establishing CharacterEdit reuse and template/instance separation prevents rework.
**Delivers:** Database schema with NPC flags, template pattern foundation, DAL operations
**Addresses Features:**
- Infrastructure for NPC template library
- Foundation for NPC instances at tables
**Avoids Pitfalls:**
- Template/Instance confusion (use ItemTemplate pattern exactly)
- Equipment/Effect divergence (NPCs use same tables from start)

**Tasks:**
- Add IsNpc, IsTemplate, VisibleToPlayers, GroupName to Character table
- Create NpcTemplates table (or IsTemplate flag on Characters)
- Update ICharacterDal with GetNpcTemplatesAsync, GetTableNpcsAsync
- Migration for new columns
- Update CharacterEdit business object with new properties

### Phase 2: Template System & Core CRUD
**Rationale:** Can't test dashboard without NPCs to display; templates enable quick creation during play.
**Delivers:** NPC template library, template creation/editing, NPC instantiation
**Addresses Features:**
- NPC template library (table stakes)
- Quick NPC creation from template (table stakes)
- Template categories/tags (competitive)
**Avoids Pitfalls:**
- Visibility leakage (bake VisibleToPlayers into model now)
- Name collisions (validation during creation)
- Template library discovery nightmare (categories/search from start)

**Tasks:**
- Create NpcTemplate ReadOnly business object
- Create NpcTemplateEdit for GM template creation
- Create NpcTemplateList with filtering/search
- Implement template instantiation (deep copy to CharacterEdit with IsNpc=true)
- Build NpcTemplateLibrary.razor UI
- Build NpcQuickCreate.razor modal

### Phase 3: Dashboard Integration
**Rationale:** NPCs visible and manipulable using existing tools; demonstrates value immediately.
**Delivers:** NPCs appear in GM dashboard, full manipulation via existing modal, real-time updates
**Addresses Features:**
- NPC status cards in GM dashboard (table stakes)
- NPC detail modal (table stakes)
- NPC removal (table stakes)
**Avoids Pitfalls:**
- Dashboard performance collapse (virtualization, batching from start)
- Initiative collision (combatant abstraction)
- Real-time update race conditions (sequencing, server-authoritative)

**Tasks:**
- Create NpcSection.razor component (replace NpcPlaceholder)
- Create NpcStatusCard.razor (reuse CharacterStatusCard pattern)
- Wire NPCs to CharacterDetailModal (detect NPC vs PC context)
- Add NpcUpdateMessage to InMemoryMessageBus
- Implement visibility filtering for player clients
- Add NPCs to GmTable.razor display

### Phase 4: Time & Combat Integration
**Rationale:** NPCs participate in full combat loop alongside PCs; validates architectural decision.
**Delivers:** NPCs in initiative, time advancement processes NPCs, effects expire correctly
**Addresses Features:**
- Initiative tracking integration (table stakes)
- NPC visibility toggle for surprise (table stakes)
**Avoids Pitfalls:**
- Initiative integration collision (use combatant abstraction)
- Missing audit trail (activity log integration)

**Tasks:**
- Extend TimeAdvancementService to process TableNpcs
- Add NPCs to CombatStateManager as ICombatants
- Implement visibility toggle with NpcVisibilityMessage
- Add NPCs to targeting system (filter by visibility)
- Integrate NPC actions into activity log

### Phase 5: Group Management & Polish
**Rationale:** Quality-of-life features after core workflow validated; batch operations high value for large encounters.
**Delivers:** Batch actions, group organization, persistence management, encounter cleanup
**Addresses Features:**
- NPC grouping with batch actions (differentiator)
- Smart auto-naming (differentiator)
- Combat disposition markers (differentiator)
**Avoids Pitfalls:**
- Group operations partial failure (transaction + error aggregation)
- Keep/dismiss ambiguity (three-state model: Active/Archived/Dismissed)
- Bulk spawn floods session (collapsed group view)

**Tasks:**
- Implement batch damage/healing/effects with transaction boundaries
- Build error aggregation and detailed feedback UI
- Add group organization (GroupName field, visual grouping)
- Implement auto-naming for bulk spawns (Goblin 1, Goblin 2)
- Add disposition field and visual markers
- Build archive/dismiss workflow with state preservation

### Phase Ordering Rationale

- **Phase 1 first** because changing data model later requires migrations and refactoring
- **Phase 2 before 3** because can't test dashboard without NPCs; templates essential for usability
- **Phase 3 before 4** to validate UI/UX before adding combat complexity
- **Phase 4 before 5** because batch operations depend on combat integration working
- **Phase 5 last** as polish features that enhance but aren't required for core workflow

**Dependencies:**
- Phase 2 depends on Phase 1 (data model)
- Phase 3 depends on Phase 2 (NPCs exist to display)
- Phase 4 depends on Phase 3 (dashboard shows NPCs)
- Phase 5 depends on Phase 4 (batch actions on combat-ready NPCs)

### Research Flags

**Phases with standard patterns (skip /gsd:research-phase):**
- **Phase 1:** Data model extension follows existing CSLA patterns
- **Phase 2:** Template system follows proven ItemTemplate pattern
- **Phase 3:** Dashboard components follow CharacterStatusCard/Modal patterns
- **Phase 4:** Time advancement and combat integration well-documented in design/

**Phases unlikely to need deeper research:**
- **Phase 5:** Group management uses standard LINQ patterns, UI iteration based on testing

**All phases can proceed with existing research.** The codebase analysis was thorough (reviewed DTOs, DAL interfaces, business objects, UI components, messaging infrastructure, design docs). No external API integrations or niche domains involved.

## Confidence Assessment

| Area | Confidence | Notes |
|------|------------|-------|
| Stack | HIGH | No new packages needed; existing stack validated through codebase analysis |
| Features | MEDIUM | Based on VTT ecosystem survey (Foundry, Roll20, D&D Beyond) and community patterns; validated against existing Threa design philosophy |
| Architecture | HIGH | Detailed codebase analysis; CharacterEdit reuse approach aligns with CSLA patterns; proven template pattern exists |
| Pitfalls | HIGH | Derived from existing codebase patterns, industry best practices (Microsoft Blazor performance docs, bulk action UX guidelines), and Threa-specific integration points |

**Overall confidence:** HIGH

All recommendations are based on direct codebase analysis rather than external research. The CharacterEdit reuse decision is strongly supported by existing code patterns (ItemTemplate/Item separation, CSLA business object architecture, resolver pattern with interfaces). The only medium-confidence area is feature prioritization, which will be validated through GM testing.

### Gaps to Address

Minor gaps that need attention during planning/implementation:

- **Performance testing:** Actual performance thresholds for NPC count unknown; suggested 20+ based on typical component limits. Validate with load testing in Phase 3.
- **Visibility filtering implementation:** Message filtering layer is new pattern for InMemoryMessageBus; may require refactoring existing Subject<T> pattern. Address during Phase 3 design.
- **Group operations transaction boundaries:** SQLite transaction semantics for CSLA business objects needs validation. May need to explore batch save patterns. Address during Phase 5.
- **NPC template seeding:** Decision needed on whether to ship with system templates or empty library. Consider community contribution workflow.

These gaps are implementation details, not architectural risks. The core approach (CharacterEdit reuse, template pattern, visibility filtering, batch operations) is sound based on research.

## Sources

### Primary (HIGH confidence - codebase analysis)
- `S:/src/rdl/threa/Threa.Dal/Dto/GameTable.cs` - Existing TableNpc DTO
- `S:/src/rdl/threa/Threa.Dal/ITableDal.cs` - NPC DAL interface
- `S:/src/rdl/threa/Threa.Dal.SqlLite/TableDal.cs` - NPC SQLite implementation
- `S:/src/rdl/threa/GameMechanics/CharacterEdit.cs` - Character business object
- `S:/src/rdl/threa/GameMechanics/EffectTemplate.cs` - Template pattern
- `S:/src/rdl/threa/GameMechanics/Items/ItemTemplateEdit.cs` - Editable template pattern
- `S:/src/rdl/threa/GameMechanics.Messaging.InMemory/InMemoryMessageBus.cs` - Messaging infrastructure
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Pages/GamePlay/GmTable.razor` - GM dashboard
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Shared/CharacterStatusCard.razor` - Status card pattern
- `S:/src/rdl/threa/Threa/Threa.Client/Components/Shared/CharacterDetailModal.razor` - Detail modal pattern
- `S:/src/rdl/threa/.planning/PROJECT.md` - v1.5 requirements
- `S:/src/rdl/threa/design/GAME_RULES_SPECIFICATION.md` - Game mechanics

### Secondary (MEDIUM confidence - industry patterns)
- [Foundry VTT Combat Tracker](https://foundryvtt.com/article/combat/) - VTT NPC management patterns
- [Combat Tracker Extensions Module](https://foundryvtt.com/packages/combat-tracker-extensions/) - Visibility toggle, disposition
- [D&D Beyond Forums - NPC Creation](https://www.dndbeyond.com/forums/dungeons-dragons-discussion/dungeon-masters-only/106885-resources-for-npc-creation) - GM workflow patterns
- [The Alexandrian Quick NPC Template](https://thealexandrian.net/wordpress/50635/roleplaying-games/quick-npc-template) - Template design patterns
- [Microsoft Learn - Blazor Performance](https://learn.microsoft.com/en-us/aspnet/core/blazor/performance/) - Virtualization and rendering optimization
- [Eleken - Bulk Actions UX](https://www.eleken.co/blog-posts/bulk-actions-ux) - Batch operation error handling
- [World Anvil - Visibility Toggle](https://blog.worldanvil.com/2020/02/12/introducing-the-visibility-toggle-show-to-your-players-only-what-you-want-them-to-see) - TTRPG visibility patterns

### Tertiary (LOW confidence - referenced but not critical)
- [Mythcreants - Managing NPCs](https://mythcreants.com/blog/eight-tips-for-managing-npcs/) - General NPC management tips
- [NN/g - Cancel vs Close](https://www.nngroup.com/articles/cancel-vs-close/) - Dismiss/archive UX patterns

---
*Research completed: 2026-02-01*
*Ready for roadmap: yes*
