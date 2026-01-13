# Play Page Implementation Plan

## Overview

This document provides a detailed implementation plan and checklist for the Play Page redesign. The implementation is organized into phases, with each phase building on the previous.

**Design Reference**: See [PLAY_PAGE_DESIGN.md](PLAY_PAGE_DESIGN.md) for full design specifications.

---

## Phase 1: Shared Components and Infrastructure

### 1.1 PendingPoolBar Component

Create a reusable component for displaying pending damage/healing as a split progress bar.

**Location**: `Threa.Client/Components/Shared/PendingPoolBar.razor`

- [ ] Create component file and code-behind
- [ ] Implement split progress bar layout (damage left, healing right)
- [ ] Add color coding (red for damage, green for healing)
- [ ] Add scale calculation based on max value
- [ ] Add tooltip/label support for numeric values
- [ ] Add configurable height parameter
- [ ] Add unit tests for calculation logic
- [ ] Document component usage with examples

**Component Interface**:
```csharp
[Parameter] public int CurrentValue { get; set; }
[Parameter] public int MaxValue { get; set; }
[Parameter] public int PendingDamage { get; set; }
[Parameter] public int PendingHealing { get; set; }
[Parameter] public bool ShowLabels { get; set; } = false;
[Parameter] public string Height { get; set; } = "8px";
```

### 1.2 SkillRow Component

Compact skill display component for skill lists.

**Location**: `Threa.Client/Components/Shared/SkillRow.razor`

- [ ] Create component file
- [ ] Implement compact layout (name, AS, dice button)
- [ ] Add click handler for "use" action
- [ ] Add disabled state styling
- [ ] Add tooltip with full skill info
- [ ] Document component usage

**Component Interface**:
```csharp
[Parameter] public string SkillName { get; set; }
[Parameter] public int Level { get; set; }
[Parameter] public int AbilityScore { get; set; }
[Parameter] public string PrimaryAttribute { get; set; }
[Parameter] public EventCallback OnUse { get; set; }
[Parameter] public bool CanUse { get; set; } = true;
```

### 1.3 EffectIcon Component

Icon component for displaying active effect indicators.

**Location**: `Threa.Client/Components/Shared/EffectIcon.razor`

- [ ] Create component file
- [ ] Implement icon mapping for effect types
- [ ] Add color coding by effect category
- [ ] Add stack count badge
- [ ] Add tooltip with effect details
- [ ] Document component usage

**Component Interface**:
```csharp
[Parameter] public string EffectType { get; set; }
[Parameter] public string EffectName { get; set; }
[Parameter] public string Tooltip { get; set; }
[Parameter] public int? Stacks { get; set; }
[Parameter] public string Color { get; set; }
```

### 1.4 ActionCostSelector Component

Reusable component for selecting action cost (1 AP + 1 FAT vs 2 AP).

**Location**: `Threa.Client/Components/Shared/ActionCostSelector.razor`

- [ ] Create component file
- [ ] Implement radio button selection
- [ ] Add validation for available resources
- [ ] Add visual feedback for insufficient resources
- [ ] Emit selected cost type
- [ ] Document component usage

### 1.5 BoostSelector Component

Reusable component for selecting boost amounts.

**Location**: `Threa.Client/Components/Shared/BoostSelector.razor`

- [ ] Create component file
- [ ] Implement +/- controls for AP boost
- [ ] Implement +/- controls for FAT boost
- [ ] Show total boost value (+X AS)
- [ ] Validate against available resources
- [ ] Emit total boost value
- [ ] Document component usage

---

## Phase 2: Top Status Bar Enhancement

### 2.1 Update Play.razor Header

Enhance the character status header with new displays.

**Location**: `Threa.Client/Components/Pages/GamePlay/Play.razor`

- [ ] Replace current FAT/VIT progress bars with PendingPoolBar components
- [ ] Add wound count badge with icon
- [ ] Add effect icons row for active effects
- [ ] Create helper method to get significant effects for icons
- [ ] Style the header for better visual hierarchy
- [ ] Ensure responsive layout for mobile

**Header Content**:
- FAT: current/max with PendingPoolBar
- VIT: current/max with PendingPoolBar
- Wounds: count badge
- Effects: icon row (poison, invisibility, stunned, etc.)
- Table info (if attached)

### 2.2 Effect Icons Logic

Add logic to display all effect icons with info overlay.

- [ ] Create effect categorization helper
- [ ] Map effect types to icons (Bootstrap Icons)
- [ ] Size icons small enough for 2 rows (~16-20px)
- [ ] Display ALL active effects (no limit)
- [ ] Create info overlay component for effect details
- [ ] Add click/tap handler to show info overlay
- [ ] Info overlay shows: name, source, duration, effect description

---

## Phase 3: Status Tab Enhancement

### 3.1 Health Pools Section

Update health pool displays with pending bars and detailed info.

**Location**: `Threa.Client/Components/Pages/GamePlay/TabStatus.razor`

- [ ] Replace current progress bars with PendingPoolBar components
- [ ] Add detailed pending damage/healing display
- [ ] Show time until next pending pool tick
- [ ] Add color-coded health thresholds
- [ ] Add low health warning indicators

### 3.2 Active Effects List

Implement full active effects display.

- [ ] Load actual effects from character
- [ ] Display effect name, icon, and source
- [ ] Show duration remaining (rounds/minutes/hours)
- [ ] Show effect magnitude/description
- [ ] Group by effect type (buffs, debuffs, conditions)
- [ ] Add effect detail expansion

### 3.3 Wounds Display

Implement location-specific wound tracking.

- [ ] Create body location diagram or list
- [ ] Show wounds per location (Head, Torso, Arms, Legs)
- [ ] Distinguish light vs serious wounds
- [ ] Show disabled/crippled status
- [ ] Calculate and show total AS penalty from wounds
- [ ] Show wound-related FAT drain info

---

## Phase 4: Combat Tab Overhaul

### 4.1 Combat Skill Filtering

Implement equipment-based skill filtering.

**Location**: `Threa.Client/Components/Pages/GamePlay/TabActions.razor` (rename to TabCombat.razor)

- [ ] Rename TabActions.razor to TabCombat.razor
- [ ] Update Play.razor tab references
- [ ] Create equipped gear detection logic
- [ ] Filter weapon skills to equipped weapons only
- [ ] Always show attribute skills (Physicality, Dodge, etc.)
- [ ] Always show movement skills (Sprint)
- [ ] Create skill categorization (attack, defense, utility)

### 4.2 Default Combat Mode

Implement the default combat view.

- [ ] Create compact skill list using SkillRow components
- [ ] Add large "ATTACK" button
- [ ] Add large "DEFEND" button
- [ ] Add "REST" button
- [ ] Implement Rest action logic:
  - [ ] On click, verify character has at least 1 AP.
  - [ ] Deduct 1 AP.
  - [ ] Add 1 FAT to the character's pending healing pool.
  - [ ] Log to Activity Log: "[Character] rests, recovering some fatigue."
- [ ] Track current mode state (default/attack/defend)
- [ ] Add mode switching logic

### 4.3 Attack Mode Implementation

Create the attack workflow interface.

**New File**: `Threa.Client/Components/Pages/GamePlay/AttackMode.razor`

- [ ] Create attack mode component
- [ ] Weapon/skill selection dropdown
- [ ] ActionCostSelector integration
- [ ] BoostSelector integration
- [ ] Physicality bonus checkbox
- [ ] Attack summary calculation
- [ ] Roll button with dice animation
- [ ] 4dF+ roll implementation
- [ ] AV calculation and display
- [ ] Activity log integration
- [ ] Cancel button to return to default mode

### 4.4 Defend Mode Implementation

Create the defense workflow interface.

**New File**: `Threa.Client/Components/Pages/GamePlay/DefendMode.razor`

**Note**: Parry is NOT a "mode" - it's simply using weapon skill for defense instead of Dodge. All active defenses (Dodge, Parry, Shield Block) cost the same: 1 AP + 1 FAT (or 2 AP).

- [ ] Create defend mode component
- [ ] Defense type selection:
  - [ ] Dodge (uses Dodge skill)
  - [ ] Parry (uses equipped weapon skill - show only if weapon equipped)
  - [ ] Shield Block (uses Shield skill - show only if shield equipped)
  - [ ] Passive (TV = Dodge AS - 1, no cost)
- [ ] ActionCostSelector integration (for active defenses only)
- [ ] BoostSelector integration
- [ ] Defense summary calculation (show selected skill's AS)
- [ ] Roll button with dice animation
- [ ] TV calculation and display
- [ ] Activity log integration
- [ ] Cancel button to return to default mode

### 4.5 Damage Resolution Workflow

Create damage resolution interface (after successful attack).

**New File**: `Threa.Client/Components/Pages/GamePlay/DamageResolution.razor`

- [ ] Create damage resolution component
- [ ] AV input field (enter attacker's AV)
- [ ] SV calculation display (AV - TV)
- [ ] Shield block option (if equipped)
  - [ ] Shield skill check
  - [ ] Shield absorption calculation
  - [ ] Durability tracking
- [ ] Hit location roll (1d12)
- [ ] Armor absorption at location
  - [ ] Armor layer processing
  - [ ] Durability tracking
- [ ] Final SV calculation
- [ ] Damage roll lookup from SV
- [ ] Damage dice roll
- [ ] FAT/VIT/Wound calculation from damage table
- [ ] Damage application to pending pools
- [ ] Wound creation if applicable
- [ ] Activity log integration
- [ ] Skip button for taking hit without armor

---

## Phase 5: Skills Tab

### 5.1 Skills Tab Redesign

Update skills tab for general gameplay.

**Location**: `Threa.Client/Components/Pages/GamePlay/TabPlaySkills.razor`

- [ ] Show ALL non-magic skills (not just combat)
- [ ] Implement search/filter functionality
- [ ] Add category filtering (Attribute, Weapon, Craft, Social, etc.)
- [ ] Use SkillRow components for compact display
- [ ] Improve skill check panel layout

### 5.2 Generic Skill Check Workflow

Implement standard skill check workflow.

- [ ] TV input (with common value presets)
- [ ] ActionCostSelector integration
- [ ] BoostSelector integration
- [ ] Roll execution with 4dF+
- [ ] Result calculation and display
- [ ] Result interpretation (quality of success/failure)
- [ ] Activity log integration

---

## Phase 6: Magic Tab

### 6.1 Mana Pool Display

Implement mana pool visualization.

**Location**: `Threa.Client/Components/Pages/GamePlay/TabPlayMagic.razor`

- [ ] Load mana pools from character data
- [ ] Display pools by magic school
- [ ] Color-code by school (Fire=red, Water=blue, etc.)
- [ ] Show current/max for each pool
- [ ] Add progress bar visualization

### 6.2 Mana Gathering Workflow

Implement mana collection system.

**New File**: `Threa.Client/Components/Pages/GamePlay/ManaGathering.razor`

- [ ] Create mana gathering component
- [ ] School selection dropdown
- [ ] TV input (default 6, GM can adjust)
- [ ] ActionCostSelector integration
- [ ] Skill check execution
- [ ] SV to mana recovery lookup
- [ ] Time calculation (minutes per mana)
- [ ] Create "Gathering [School] Mana" effect on character
  - [ ] Effect duration = recovery time (typically 1 min/mana)
  - [ ] Effect indicates concentration
  - [ ] Effect blocks other actions
  - [ ] Mana granted on effect expiration
- [ ] Interruption handling:
  - [ ] If character takes any action while gathering: cancel effect, no mana
  - [ ] If character takes damage while gathering: cancel effect, no mana
  - [ ] Log interruption to activity log
- [ ] Activity log integration
- [ ] Visual indicator of active gathering

### 6.3 Spell List Display

Implement spell list view.

- [ ] Load spells from character's spell skills
- [ ] Filter by school (optional)
- [ ] Display: name, school, AS, cost, type
- [ ] Use SkillRow components
- [ ] Click to enter casting mode

### 6.4 Spell Casting Workflow

Implement spell casting interface.

**New File**: `Threa.Client/Components/Pages/GamePlay/SpellCasting.razor`

- [ ] Create spell casting component
- [ ] Display spell details (name, school, type, range)
- [ ] Skill action cost selection (1 AP + 1 FAT or 2 AP)
- [ ] Spell cost selection:
  - [ ] Pay with mana
  - [ ] Pay with FAT
  - [ ] Mixed payment
- [ ] Boost options (extra mana/FAT)
- [ ] Target selection (if targeted spell)
- [ ] Cast time handling:
  - [ ] Instant: execute immediately
  - [ ] Delayed: create "Casting [Spell]" effect, spell triggers on expiration
- [ ] Cast time interruption:
  - [ ] If character takes any action while casting: cancel, refund boost only
  - [ ] If character takes damage while casting: cancel, refund boost only
  - [ ] Base casting cost (mana/FAT for spell itself) is NOT refunded
  - [ ] Log interruption to activity log
- [ ] Concentration handling:
  - [ ] Create "Maintaining [Spell]" effect
  - [ ] Per-spell AP/FAT drain per round (defined in spell definition)
  - [ ] Character can take limited other actions (GM discretion)
  - [ ] Character can end concentration at any time
  - [ ] GM can cancel effect from GM screen
  - [ ] When concentration ends, spell effect also ends
- [ ] Roll execution with 4dF+
- [ ] Result calculation
- [ ] Effect application
- [ ] Activity log integration

---

## Phase 7: Integration and Polish

### 7.1 Activity Log Enhancement

Improve activity log for combat flow.

- [ ] Add structured message formatting
- [ ] Color-code by category (combat, magic, skill, etc.)
- [ ] Add icons for message types
- [ ] Implement collapsible detail view
- [ ] Add timestamp formatting options
- [ ] Ensure all actions log appropriately

### 7.2 Real-time Updates

Implement real-time synchronization.

- [ ] Subscribe to table activity log
- [ ] Update character state on server changes
- [ ] Show incoming attack notifications
- [ ] Update effect timers in real-time
- [ ] Handle round/time progression events

### 7.3 Dice Rolling System

Implement proper dice rolling with visuals.

- [ ] Create DiceRoller service
- [ ] Implement 4dF+ with exploding logic
- [ ] Add visual dice animation component
- [ ] Support for other dice types (d6, d8, d10, d12)
- [ ] Roll history display

### 7.4 Result Tables Integration

Connect to game mechanics result tables.

- [ ] SV to damage roll lookup
- [ ] Damage to FAT/VIT/Wound conversion
- [ ] Mana recovery result table
- [ ] Skill check quality interpretation
- [ ] Combat special action results

### 7.5 Testing and Validation

Ensure quality and correctness.

- [ ] Unit tests for calculation logic
- [ ] Unit tests for component behavior
- [ ] Integration tests for workflows
- [ ] Manual testing of complete combat flow
- [ ] Manual testing of skill check flow
- [ ] Manual testing of magic flow
- [ ] Accessibility testing
- [ ] Mobile responsiveness testing

### 7.6 Documentation

Update documentation.

- [ ] Update design docs with implementation notes
- [ ] Add usage documentation for new components
- [ ] Document activity log message formats
- [ ] Update README with feature description

---

## Implementation Order Summary

### Quick Wins (Can be done independently)
1. PendingPoolBar component
2. SkillRow component
3. EffectIcon component

### Core Flow (Sequential)
1. Top status bar update
2. Status tab enhancement
3. Combat tab restructure
4. Attack mode workflow
5. Defend mode workflow
6. Damage resolution workflow

### Extended Features (After core)
1. Skills tab redesign
2. Mana gathering workflow
3. Spell casting workflow
4. Real-time updates
5. Dice rolling visuals

---

## Dependencies

### External Dependencies (Already Satisfied)
- GameMechanics library (actions, combat, effects)
- CSLA data portal
- Activity log service
- Bootstrap Icons

### Internal Dependencies
- Character effects list must be populated from Effects system
- Equipped gear detection requires inventory/equipment system
- Mana pools require magic system integration
- Spell definitions require spell skill data

---

## Notes

### Questions Resolved

| Question | Resolution |
|----------|------------|
| **Parry Mode** | Parry is NOT a mode - it's simply using weapon skill for defense instead of Dodge. All active defenses cost 1 AP + 1 FAT (or 2 AP). |
| **Equipment Detection** | Still needs implementation - detect from character inventory/equipment slots |
| **Effect Icons** | Display ALL effects with small icons (16-20px), allow 2 rows. Click/tap shows info overlay with details. |
| **Mana Gathering Interruption** | If interrupted by action or damage, gathering is canceled with no mana recovery. |
| **Concentration Drain** | Per-spell - each concentration spell defines its own AP/FAT cost per round in the spell definition. |
| **Cast Time Interruption** | If interrupted by action or damage: spell fails, effect ends. Boost cost is refunded, but base casting cost is lost. |
| **Concentration Actions** | Character can take limited other actions at GM's discretion. GM can cancel the effect from the GM screen. |

### Design Decisions Made

| Topic | Decision |
|-------|----------|
| Combat modes | Tab content switches between default/attack/defend modes instead of popups |
| Pending pool bar | Split from center, damage left (red), healing right (green) |
| Skill display | Compact rows with name, AS, dice button |
| Parry defense | Uses weapon skill for TV instead of Dodge; same cost as other active defenses |
| Effect icons | Small icons (~16-20px), allow 2 rows, click/tap for info overlay |
| Mana gathering | Creates Effect to track; blocks other actions; interrupted by action or damage |
| Cast time spells | Creates Effect; interrupted by action or damage; boost refunded, base cost lost |
| Concentration spells | Creates Effect with per-spell drain; allows limited actions; GM can cancel |
| Activity log | All actions broadcast to table log for visibility |

---

## Related Documents

- [PLAY_PAGE_DESIGN.md](PLAY_PAGE_DESIGN.md) - Design specification
- [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md) - Combat mechanics reference
- [ACTIONS.md](ACTIONS.md) - Action resolution reference
- [EFFECTS_SYSTEM.md](EFFECTS_SYSTEM.md) - Effects reference
- [MAGIC_SYSTEM_IMPLEMENTATION.md](MAGIC_SYSTEM_IMPLEMENTATION.md) - Magic system reference
