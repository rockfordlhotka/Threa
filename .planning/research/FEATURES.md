# Feature Landscape: NPC Management System

**Domain:** TTRPG NPC Management for GM Dashboard
**Researched:** 2026-02-01
**Confidence:** MEDIUM (based on VTT ecosystem survey and existing codebase patterns)

---

## Table Stakes

Features users expect from any NPC management system. Missing = product feels incomplete or broken.

| Feature | Why Expected | Complexity | Dependencies | Notes |
|---------|--------------|------------|--------------|-------|
| **NPC Template Library** | GMs need pre-built NPCs for encounter prep; D&D Beyond, Foundry, Roll20 all provide this | Medium | ItemTemplate pattern exists | Similar to existing ItemTemplate infrastructure |
| **Quick NPC Creation from Template** | GMs instantiate NPCs during play without building from scratch | Low | NPC Template Library | Spawn instance from template, like Items from ItemTemplates |
| **NPC Status Cards in GM Dashboard** | NPCs must appear alongside PCs with same visibility (FAT/VIT/AP/wounds/effects) | Low | CharacterStatusCard already exists | Reuse existing card component, add NPC flag |
| **NPC Detail Modal** | GM needs same manipulation powers for NPCs as PCs (damage, healing, effects, wounds) | Low | CharacterDetailModal exists | Same modal, detect NPC vs PC context |
| **NPC Visibility Toggle (Hide/Reveal)** | GMs hide NPCs before combat, reveal for surprise; standard VTT feature per Foundry modules | Medium | New field on NPC instance | Combat Tracker Extensions module shows this is expected |
| **Initiative Tracking Integration** | NPCs participate in combat rounds with other characters | Medium | Table already has combat mode | NPCs need to appear in round tracking |
| **NPC Removal/Dismiss** | NPCs can be removed from table when defeated or no longer needed | Low | Existing table character removal | Same pattern as removing PC from table |
| **Basic NPC Persistence** | NPCs survive across sessions if not dismissed; session state matters | Low | Character persistence exists | NPCs attached to table like PCs |

---

## Differentiators

Features that set Threa apart. Not universally expected, but valued when present.

| Feature | Value Proposition | Complexity | Dependencies | Notes |
|---------|-------------------|------------|--------------|-------|
| **Full Character Stats for NPCs** | NPCs use same character model as PCs - full attributes, skills, equipment, effects | Medium | Character model already supports this | Unlike D&D 5e simplified stat blocks, Threa NPCs are full characters |
| **NPC Grouping with Batch Actions** | Apply damage/effects to multiple NPCs at once ("all goblins take 3 FAT damage") | High | New batch action infrastructure | Roll20 encounter builder hints at this; rare in competition |
| **NPC Template Categories/Tags** | Filter templates by type (humanoid, beast, undead) or CR-equivalent | Low | Tag system from ItemTemplate | Existing tag infrastructure applies |
| **GM Notes Per NPC Instance** | Each NPC instance can have session-specific notes separate from template | Low | GM Notes pattern exists for PCs | TableCharacter already has GmNotes field |
| **NPC Initiative Helpers** | Roll initiative for all hostile NPCs at once; group identical NPCs | Medium | New initiative roll logic | Foundry's "Roll NPCs" button; Group Initiative module |
| **Smart NPC Naming** | Auto-generate names: "Goblin 1", "Goblin 2" or use name table | Low | None | Quality-of-life feature from The Alexandrian's templates |
| **NPC Combat Disposition** | Mark NPCs as Hostile/Neutral/Friendly for visual differentiation | Low | New disposition field | Foundry Combat Tracker Extensions pattern |

---

## Anti-Features

Features to explicitly NOT build. Common mistakes or over-engineering traps.

| Anti-Feature | Why Avoid | What to Do Instead |
|--------------|-----------|-------------------|
| **Simplified NPC Stat Blocks** | Threa's character model is already designed; creating a second model doubles maintenance and loses system integration | NPCs use full Character model with same stats, skills, equipment |
| **AI-Powered NPC Generation** | Scope creep; not core to management workflow; adds complexity and external dependencies | Provide template library and manual creation tools |
| **NPC-to-PC Conversion** | Edge case that complicates data model; "converted NPCs" often have weird edge cases | Keep NPCs and PCs separate; copy manually if needed |
| **Automatic CR Calculation** | D&D 5e concept; Threa has different power scaling; complex to implement correctly | Let GM judge difficulty narratively; provide stat comparisons |
| **NPC Loot Tables** | Feature creep into encounter management; Threa already has item templates | GM grants items manually using existing distribution system |
| **Voice/Personality AI** | Far outside core scope; NPC Reactor-style features are external tools | GM manages roleplay manually; provide notes field |
| **NPC Memory/Conversation State** | AI-adjacent feature; scope creep | Simple notes field suffices |
| **Map/Token Integration** | Threa is not a VTT; no map system exists | Focus on character management, not spatial combat |

---

## Feature Dependencies

```
NPC Template Library
    |
    +---> Quick NPC Creation from Template
              |
              +---> NPC Status Cards in GM Dashboard
              |         |
              |         +---> NPC Detail Modal (reuses CharacterDetailModal)
              |         |
              |         +---> NPC Visibility Toggle
              |
              +---> Initiative Tracking Integration
              |
              +---> NPC Removal/Dismiss
              |
              +---> NPC Grouping with Batch Actions (differentiator, optional)

Existing Infrastructure Used:
    - CharacterStatusCard (visual display)
    - CharacterDetailModal (manipulation)
    - CharacterEdit (full character model)
    - TableCharacterList (table membership)
    - ItemTemplate pattern (for NPC templates)
    - GmNotes infrastructure
```

---

## MVP Recommendation

For MVP NPC management, prioritize:

### Must Have (Phase 1)
1. **NPC Template Library** - GM creates reusable NPC definitions
2. **Quick NPC Creation** - Spawn NPC instance to table from template
3. **NPC Status Cards** - NPCs appear in GM dashboard alongside PCs
4. **NPC Detail Modal** - Same manipulation powers as PCs
5. **NPC Removal** - Remove defeated/dismissed NPCs from table

### Should Have (Phase 2)
1. **NPC Visibility Toggle** - Hide/reveal for surprise encounters
2. **NPC Disposition** - Hostile/Neutral/Friendly visual markers
3. **Template Categories/Tags** - Filter NPC library by type

### Nice to Have (Phase 3)
1. **Batch Actions** - Apply damage/effects to NPC groups
2. **Initiative Helpers** - Roll for all hostile NPCs at once
3. **Smart Naming** - Auto-increment "Goblin 1", "Goblin 2"

### Defer to Post-MVP
- NPC Grouping (complex batch action infrastructure)
- Initiative roll groups (requires initiative system redesign)

---

## Architecture Implications

### Data Model Decision: NPC as Character

**Recommendation:** NPCs use the existing `Character` model with an `IsNpc` flag.

**Rationale:**
- Same full stats, skills, equipment, effects as PCs
- Reuses all existing business objects (CharacterEdit, effects, wounds)
- Same GM manipulation tools work without modification
- No parallel maintenance of simplified stat block system
- Consistent with Threa's design philosophy (full character model)

**Template Pattern:**
- `NpcTemplate` table (similar to `ItemTemplate`)
- Contains full character definition as JSON or normalized tables
- Instantiation creates a `Character` record with `IsNpc = true`

### UI Reuse

| Existing Component | NPC Reuse |
|-------------------|-----------|
| CharacterStatusCard | Same card, add NPC visual marker |
| CharacterDetailModal | Same modal, same tabs |
| CharacterDetailGmActions | Same health/wound/effect controls |
| PendingPoolBar | Same health visualization |
| EffectIcon | Same effect display |

### New Components Needed

| Component | Purpose |
|-----------|---------|
| NpcPlaceholder (exists) | Replace with NpcSection |
| NpcSection | Collapsible section in GM dashboard showing NPCs |
| NpcTemplateList | GM page for managing NPC templates |
| NpcTemplateEdit | Create/edit NPC template |
| NpcSpawnModal | Quick-spawn NPC from template to table |

---

## Competitive Analysis Summary

| Platform | NPC Approach | Key Features | Threa Opportunity |
|----------|--------------|--------------|-------------------|
| **Foundry VTT** | Actor system, compendium bestiaries | Visibility toggle, disposition, group initiative | Match visibility/disposition; skip map integration |
| **Roll20** | Handouts + tokens | Journal entries, GM layer | Focus on dashboard integration, not spatial |
| **D&D Beyond** | Homebrew monsters (simplified) | Encounter builder CR balance | Full character model is stronger for custom systems |
| **Chronica** | NPC Codex | Contact list, relationships | Dashboard focus over relationship mapping |
| **Fantasy Grounds** | Full automation | NPC turns handled automatically | Consider automation later; start with display |

**Key Insight:** Most VTTs focus on map/token integration. Threa's advantage is the existing dashboard with real-time updates. NPCs should integrate into this flow seamlessly rather than requiring map-based interaction.

---

## Risk Factors

| Risk | Mitigation |
|------|------------|
| Scope creep into VTT features | Explicit anti-features list; no map/token scope |
| Character model complexity for NPCs | NPCs use same model; no simplification needed |
| Performance with many NPCs | Lazy loading; pagination if >20 NPCs on table |
| Template vs instance confusion | Clear UI separation (library vs active table) |

---

## Sources

**VTT Ecosystem:**
- [Foundry VTT Combat Tracker](https://foundryvtt.com/article/combat/)
- [Combat Tracker Extensions Module](https://foundryvtt.com/packages/combat-tracker-extensions/)
- [Your Tokens Visible Module](https://foundryvtt.com/packages/TokensVisible)
- [D&D Beyond NPC Creation Forums](https://www.dndbeyond.com/forums/dungeons-dragons-discussion/dungeon-masters-only/106885-resources-for-npc-creation)

**NPC Design Patterns:**
- [The Alexandrian Quick NPC Template](https://thealexandrian.net/wordpress/50635/roleplaying-games/quick-npc-template)
- [3 Line NPC Method](https://www.roleplayingtips.com/rptn/the-3-line-npc-method-how-to-create-story-full-npcs-fast/)
- [Running NPCs: Statblocks vs No Stats](https://illusoryscript.com/npc-statblocks/)

**Encounter Management:**
- [HeroMuster Encounters Builder](https://encounters.heromuster.com/)
- [D&D Beyond Encounter Builder](https://www.dndbeyond.com/encounter-builder)
- [TTRPG Games Initiative Trackers](https://www.ttrpg-games.com/blog/top-10-initiative-trackers-for-ttrpgs/)

**Existing Codebase:**
- `GmTable.razor` - GM dashboard with character cards
- `CharacterStatusCard.razor` - Compact character display
- `CharacterDetailModal.razor` - Full character manipulation
- `NpcPlaceholder.razor` - Placeholder for NPC section
- `DATABASE_DESIGN.md` - Character/Item data model patterns
