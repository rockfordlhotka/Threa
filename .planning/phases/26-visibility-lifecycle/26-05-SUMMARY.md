---
phase: 26-visibility-lifecycle
plan: 05
subsystem: npc-management
tags: [npc, templates, csla, blazor]

dependency_graph:
  requires:
    - 26-01 (data model extensions)
    - 24-04 (NpcTemplates page)
  provides:
    - NpcTemplateCreator CSLA command
    - SaveAsTemplateModal component
    - Save as Template button in CharacterDetailAdmin
  affects:
    - Phase 27+ (template usage in campaigns)

tech_stack:
  added: []
  patterns:
    - CSLA command pattern for template creation
    - Modal component for template metadata input
    - HTML5 datalist for category autocomplete

key_files:
  created:
    - GameMechanics/NpcTemplateCreator.cs
    - Threa/Threa.Client/Components/Shared/SaveAsTemplateModal.razor
  modified:
    - Threa/Threa.Client/Components/Shared/CharacterDetailAdmin.razor

decisions:
  - name: "Item copying uses CharacterItem template reference"
    rationale: "CharacterItem references ItemTemplate by ID; copies instance data not template data"
  - name: "Health pools reset to full in template"
    rationale: "Templates represent fresh NPC state, not battle-damaged state"
  - name: "Effects cleared in template"
    rationale: "Wounds, buffs, debuffs are combat state, not permanent NPC definition"

metrics:
  duration: 6 min
  completed: 2026-02-03
---

# Phase 26 Plan 05: Save as Template Summary

**One-liner:** NpcTemplateCreator command and SaveAsTemplateModal for saving active NPCs as reusable templates.

## What Was Built

### 1. NpcTemplateCreator CSLA Command (`GameMechanics/NpcTemplateCreator.cs`)

A CSLA CommandBase that creates a new NPC template from an active NPC instance:

- **Input:** Source NPC ID, template name, category, tags
- **Process:**
  - Fetches source NPC via ICharacterDal
  - Creates new Character DTO as template
  - Copies attributes, skills, and equipment (item instances)
  - Resets health pools to full (FatValue/VitValue = base values)
  - Clears all effects (wounds, buffs, debuffs)
  - Sets flags: IsTemplate=true, IsNpc=true, IsPlayable=false
  - Saves via SaveCharacterAsync
- **Output:** CreatedTemplateId, Success, ErrorMessage

### 2. SaveAsTemplateModal Component (`Threa/Threa.Client/Components/Shared/SaveAsTemplateModal.razor`)

Bootstrap modal for template creation:

- **Inputs:**
  - Template name (pre-filled with NPC name)
  - Category (with datalist autocomplete from existing categories)
  - Tags (comma-separated)
- **Features:**
  - Info box explaining what gets saved
  - Loading spinner during save
  - Error message display
- **Actions:** Executes NpcTemplateCreator via IDataPortal

### 3. CharacterDetailAdmin Integration

Added to NPC Lifecycle section:

- **"Save as Template" button** - Opens SaveAsTemplateModal
- Positioned before Archive button with separator
- Only visible for NPCs (Character.IsNpc == true)

## Technical Decisions

| Decision | Rationale |
|----------|-----------|
| Health reset to full | Templates represent fresh NPCs ready for spawning |
| Effects cleared | Combat state (wounds, conditions) is instance-specific |
| Items copied as instances | CharacterItem references ItemTemplate by ID |
| Notes preserved as TemplateNotes | Existing GM notes become template documentation |

## Files Changed

| File | Change Type | Lines |
|------|-------------|-------|
| `GameMechanics/NpcTemplateCreator.cs` | Created | 270 |
| `Threa/Threa.Client/Components/Shared/SaveAsTemplateModal.razor` | Created | 147 |
| `Threa/Threa.Client/Components/Shared/CharacterDetailAdmin.razor` | Modified | +32 |

## Commits

| Hash | Message |
|------|---------|
| 22d41c6 | feat(26-05): add NpcTemplateCreator CSLA command |
| 1281545 | feat(26-05): add SaveAsTemplateModal component |
| 78349d1 | feat(26-05): add Save as Template button to CharacterDetailAdmin |

## Deviations from Plan

### Auto-fixed Issues

**1. [Rule 3 - Blocking] Fixed CharacterItem property mapping**

- **Found during:** Task 1
- **Issue:** Plan specified old CharacterItem structure (ItemId, Name, etc.) but actual DTO uses ItemTemplateId, StackSize, EquippedSlot pattern
- **Fix:** Updated item copying to use correct CharacterItem properties
- **Files modified:** GameMechanics/NpcTemplateCreator.cs
- **Commit:** 22d41c6

**2. [Rule 3 - Blocking] Added missing System.Collections.Generic using**

- **Found during:** Task 1
- **Issue:** List<CharacterEffect>() required System.Collections.Generic
- **Fix:** Added using directive
- **Files modified:** GameMechanics/NpcTemplateCreator.cs
- **Commit:** 22d41c6

## Verification Checklist

- [x] Solution builds without errors (individual projects verified)
- [x] NpcTemplateCreator command exists and compiles
- [x] SaveAsTemplateModal component exists
- [x] CharacterDetailAdmin shows "Save as Template" button for NPCs
- [x] Modal pre-fills name with NPC name
- [x] Modal shows category autocomplete with existing categories
- [x] Template creation resets health and clears effects
- [x] IsTemplate=true set on created template

## Next Phase Readiness

**Blockers:** None

**Ready for:**
- Phase 27+ can use templates created from active NPCs
- v1.5 milestone wrap-up

---
*Plan 26-05 complete - Phase 26 finished*
