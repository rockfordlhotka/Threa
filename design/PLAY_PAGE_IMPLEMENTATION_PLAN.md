# Play Page Implementation Plan

## Overview

This document provides a detailed implementation plan and checklist for the Play Page redesign. The implementation is organized into phases, with each phase building on the previous.

**Design Reference**: See [PLAY_PAGE_DESIGN.md](PLAY_PAGE_DESIGN.md) for full design specifications.

---

## Phase 1: Shared Components and Infrastructure

### 1.1 PendingPoolBar Component

Create a reusable component for displaying pending damage/healing as a split progress bar.

**Location**: `Threa.Client/Components/Shared/PendingPoolBar.razor`

- [x] Create component file and code-behind
- [x] Implement split progress bar layout (damage left, healing right)
- [x] Add color coding (red for damage, green for healing)
- [x] Add scale calculation based on max value
- [x] Add tooltip/label support for numeric values
- [x] Add configurable height parameter
- [ ] Add unit tests for calculation logic
- [x] Document component usage with examples

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

- [x] Create component file
- [x] Implement compact layout (name, AS, dice button)
- [x] Add click handler for "use" action
- [x] Add disabled state styling
- [x] Add tooltip with full skill info
- [x] Document component usage

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

- [x] Create component file
- [x] Implement icon mapping for effect types
- [x] Add color coding by effect category
- [x] Add stack count badge
- [x] Add tooltip with effect details
- [x] Document component usage

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

- [x] Create component file
- [x] Implement radio button selection
- [x] Add validation for available resources
- [x] Add visual feedback for insufficient resources
- [x] Emit selected cost type
- [x] Document component usage

### 1.5 BoostSelector Component

Reusable component for selecting boost amounts.

**Location**: `Threa.Client/Components/Shared/BoostSelector.razor`

- [x] Create component file
- [x] Implement +/- controls for AP boost
- [x] Implement +/- controls for FAT boost
- [x] Show total boost value (+X AS)
- [x] Validate against available resources
- [x] Emit total boost value
- [x] Document component usage

---

## Phase 2: Top Status Bar Enhancement

### 2.1 Update Play.razor Header

Enhance the character status header with new displays.

**Location**: `Threa.Client/Components/Pages/GamePlay/Play.razor`

- [x] Replace current FAT/VIT progress bars with PendingPoolBar components
- [x] Add wound count badge with icon
- [x] Add effect icons row for active effects
- [x] Create helper method to get significant effects for icons
- [x] Style the header for better visual hierarchy
- [x] Ensure responsive layout for mobile

**Header Content**:
- FAT: current/max with PendingPoolBar
- VIT: current/max with PendingPoolBar
- Wounds: count badge
- Effects: icon row (poison, invisibility, stunned, etc.)
- Table info (if attached)

### 2.2 Effect Icons Logic

Add logic to display all effect icons with info overlay.

- [x] Create effect categorization helper
- [x] Map effect types to icons (Bootstrap Icons)
- [x] Size icons small enough for 2 rows (~16-20px)
- [x] Display ALL active effects (no limit)
- [ ] Create info overlay component for effect details
- [ ] Add click/tap handler to show info overlay
- [x] Info overlay shows: name, source, duration, effect description (via tooltip)

---

## Phase 3: Status Tab Enhancement

### 3.1 Health Pools Section

Update health pool displays with pending bars and detailed info.

**Location**: `Threa.Client/Components/Pages/GamePlay/TabStatus.razor`

- [x] Replace current progress bars with PendingPoolBar components
- [x] Add detailed pending damage/healing display
- [x] Show time until next pending pool tick
- [x] Add color-coded health thresholds
- [x] Add low health warning indicators

### 3.2 Active Effects List

Implement full active effects display.

- [x] Load actual effects from character
- [x] Display effect name, icon, and source
- [x] Show duration remaining (rounds/minutes/hours)
- [x] Show effect magnitude/description
- [x] Group by effect type (buffs, debuffs, conditions)
- [x] Add effect detail expansion

### 3.3 Wounds Display

Implement location-specific wound tracking.

- [x] Create body location diagram or list
- [x] Show wounds per location (Head, Torso, Arms, Legs)
- [x] Distinguish light vs serious wounds
- [x] Show disabled/crippled status
- [x] Calculate and show total AS penalty from wounds
- [x] Show wound-related FAT drain info

---

## Phase 4: Combat Tab Overhaul

### 4.1 Combat Skill Filtering

Implement equipment-based skill filtering.

**Location**: `Threa.Client/Components/Pages/GamePlay/TabCombat.razor`

- [x] Rename TabActions.razor to TabCombat.razor
- [x] Update Play.razor tab references
- [x] Create equipped gear detection logic
- [x] Filter weapon skills to equipped weapons only
- [x] Always show attribute skills (Physicality, Dodge, etc.)
- [x] Always show movement skills (Sprint)
- [x] Create skill categorization (attack, defense, utility)

### 4.2 Default Combat Mode

Implement the default combat view.

- [x] Create compact skill list using SkillRow components
- [x] Add large "ATTACK" button
- [x] Add large "DEFEND" button
- [x] Add "REST" button
- [x] Implement Rest action logic:
  - [x] On click, verify character has at least 1 AP.
  - [x] Deduct 1 AP.
  - [x] Add 1 FAT to the character's pending healing pool.
  - [x] Log to Activity Log: "[Character] rests, recovering some fatigue."
- [x] Track current mode state (default/attack/defend)
- [x] Add mode switching logic

### 4.3 Attack Mode Implementation

Create the attack workflow interface.

**New File**: `Threa.Client/Components/Pages/GamePlay/AttackMode.razor`

- [x] Create attack mode component
- [x] Weapon/skill selection dropdown
- [x] ActionCostSelector integration
- [x] BoostSelector integration
- [x] Physicality bonus checkbox
- [x] Attack summary calculation
- [ ] Roll button with dice animation (uses existing GameMechanics.Dice, animation deferred)
- [x] 4dF+ roll implementation (uses existing GameMechanics.Dice.Roll4dFPlus())
- [x] AV calculation and display
- [x] Activity log integration
- [x] Cancel button to return to default mode

### 4.4 Defend Mode Implementation

Create the defense workflow interface.

**New File**: `Threa.Client/Components/Pages/GamePlay/DefendMode.razor`

**Note**: Parry Mode is implemented as a CombatStance Effect:
- Entering Parry Mode costs 1 AP + 1 FAT (or 2 AP)
- While in Parry Mode, parries using the equipped weapon skill are FREE
- Parry Mode ends when the character takes any non-parry action (attack, rest, etc.)
- Shield Block rolls vs fixed TV 8 (not AS-based)
- Multi-action penalty (-1 AS) applies after the first action each round

- [x] Create defend mode component
- [x] Defense type selection:
  - [x] Dodge (uses Dodge skill)
  - [x] Parry (uses equipped weapon skill - show only if weapon equipped)
  - [x] Shield Block (uses Shield skill - show only if shield equipped)
  - [x] Passive (TV = Dodge AS - 1, no cost)
- [x] ActionCostSelector integration (for active defenses only)
- [x] BoostSelector integration
- [x] Defense summary calculation (show selected skill's AS)
- [ ] Roll button with dice animation (uses existing GameMechanics.Dice, animation deferred)
- [x] TV calculation and display
- [x] Activity log integration
- [x] Cancel button to return to default mode

### 4.5 Damage Resolution Workflow

Create damage resolution interface (after successful attack).

**New File**: `Threa.Client/Components/Pages/GamePlay/DamageResolution.razor`

- [x] Create damage resolution component
- [x] SV input field (accepts SV from defense or GM for falls/spells)
- [x] Damage type selection (Bashing, Cutting, Piercing, Projectile, Energy)
- [x] Damage class selection (1-4)
- [x] Shield block option (if equipped, layered defense)
  - [x] Shield skill check vs TV 8
  - [x] Shield absorption calculation
  - [x] Durability tracking
- [x] Hit location roll (using HitLocationCalculator with proper 1d24 probability)
- [x] Armor absorption at location
  - [x] Armor layer processing (outer first)
  - [x] Equipment slot to hit location mapping
  - [x] Durability tracking
- [x] Penetrating SV calculation after absorption
- [x] Damage dice roll from SV (new dice-based tables)
- [x] FAT/VIT/Wound calculation from damage table
- [x] Damage application to pending pools
- [x] Wound creation as Effect
- [x] Activity log integration
- [x] Auto-transition from DefendMode when hit
- [x] "Take Damage" button in TabCombat for non-combat damage

**New Files Created**:
- `GameMechanics/Combat/DamageTables.cs` - SV to dice roll, damage to FAT/VIT/wounds conversion
- `GameMechanics/Combat/EquipmentLocationMapper.cs` - Maps equipment slots to hit locations
- `Threa.Client/Components/Pages/GamePlay/DefenseHitData.cs` - Data transfer for hit transition

### 4.6 Attack Mode Details (Implemented)

These features are already implemented in AttackMode.razor:

- [x] Physicality check uses CombatResultTables.GetPhysicalityBonus()
- [x] Negative Physicality results create temporary Debuff Effects
  - Effect tracks AttackPenalty in BehaviorState
  - Duration based on severity of failure
  - Description shows penalty amount and reason
- [x] Multi-action penalty (-1 AS after first action) tracked in ActionPoints
- [x] Boost calculation: 1 AP = +1 AS, 1 FAT = +1 AS (equal value)

### 4.7 Future: Ranged Combat (Phase TBD)

Ranged combat will be implemented in a future phase. Key differences from melee:

- [ ] Range modifiers based on distance
- [ ] Parry Mode does NOT work against ranged attacks
- [ ] Cover and concealment modifiers
- [ ] Ammunition tracking
- [ ] Reload actions (cost varies by weapon)
- [ ] Aim action for accuracy bonus

### 4.8 Future: Special Combat Actions (Phase TBD)

Additional combat options to implement in a future phase:

- [ ] Disarm (opposed check, weapon knocked away)
- [ ] Grapple (initiates grapple combat)
- [ ] Trip/Knockdown (target falls prone)
- [ ] Called Shot (target specific location, higher TV)
- [ ] Charge (move + attack, bonus damage, penalty to defense)
- [ ] Defensive Stance (bonus to defense, penalty to attack)
- [ ] All-Out Attack (bonus to attack, no defense until next turn)

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
| **Parry Mode** | Parry Mode IS a mode - implemented as a CombatStance Effect. Costs 1 AP + 1 FAT to enter, then parries are FREE. Ends on non-parry action. |
| **Shield Block** | Rolls Shield AS + 4dF+ vs fixed TV 8 (not against attacker's AV). Can layer with other defenses. |
| **Multi-Action Penalty** | -1 AS penalty for all actions after the first in a round. Tracked in ActionPoints.ActionsTakenThisRound. |
| **Boost Value** | 1 AP = +1 AS, 1 FAT = +1 AS (equal value, not FAT/2). |
| **Physicality Negative Results** | Create temporary Debuff Effects when roll fails. Duration and penalty from CombatResultTables. |
| **Equipment Detection** | Implemented - detects equipped weapons/shields from inventory slots |
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
| Parry Mode | Creates CombatStance Effect on enter; FREE parries while active; ends on non-parry action |
| Shield Block | Rolls vs fixed TV 8; can layer with other defenses; uses Shield ItemType |
| Boost calculation | 1 AP = +1 AS, 1 FAT = +1 AS (equal value) |
| Multi-action penalty | -1 AS after first action per round; tracked in ActionPoints |
| Physicality debuffs | Negative results create Debuff Effects with duration from combat table |
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
