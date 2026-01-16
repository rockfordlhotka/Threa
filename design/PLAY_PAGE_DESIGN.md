# Play Page Design

## Overview

The Play Page is the primary interface for players during active gameplay. It provides quick access to character status, combat actions, skill usage, and magic casting in a streamlined, action-oriented layout.

**Related Documents**:
- [Actions](ACTIONS.md) - Universal action resolution framework
- [Combat System](COMBAT_SYSTEM.md) - Combat mechanics and workflows
- [Magic System Implementation](MAGIC_SYSTEM_IMPLEMENTATION.md) - Spell casting and mana
- [Effects System](EFFECTS_SYSTEM.md) - Wounds, conditions, buffs, debuffs

---

## Page Structure

### Top Status Bar

A fixed bar across the top displaying critical character information at a glance:

| Element | Display | Notes |
|---------|---------|-------|
| **FAT** | Current / Max | Current fatigue with pending indicator |
| **VIT** | Current / Max | Current vitality with pending indicator |
| **Pending Pool** | Dual progress bar | Split bar showing pending damage/healing |
| **Wounds** | Count badge | Total number of active wounds |
| **Effect Icons** | Icon row | Visual indicators for active effects |

#### Pending Pool Progress Bar Component

A **reusable shared Razor component** (`PendingPoolBar.razor`) displaying pending damage and healing:

```
Structure:
[Red (damage) ← | Center | → Green (healing)]

Visual behavior:
- Center represents "neutral" (no pending changes)
- Pending damage fills RED from center toward LEFT
- Pending healing fills GREEN from center toward RIGHT
- Scale based on character's max pool value
- Show numeric values on hover/click
```

**Component Parameters**:
- `CurrentValue` (int) - Current pool value
- `MaxValue` (int) - Maximum pool value
- `PendingDamage` (int) - Pending damage amount (fills left, red)
- `PendingHealing` (int) - Pending healing amount (fills right, green)
- `ShowLabels` (bool) - Show numeric labels
- `Height` (string) - CSS height (default "8px")

#### Effect Icons

Display icons for all active effects in a compact, multi-row layout:

**Layout**:
- Icons sized small enough to fit many (approximately 16-20px)
- Allow up to 2 rows of icons in the header
- Click/tap on any icon opens an info overlay with effect details

**Icon Overlay** (on click/tap):
```
+---------------------------+
| [Icon] Effect Name        |
+---------------------------+
| Source: Poison Dart       |
| Duration: 5 rounds        |
| Effect: -2 AS, 1 FAT/round|
| [Close]                   |
+---------------------------+
```

**Standard Effect Icons**:

| Effect Type | Icon | Color |
|-------------|------|-------|
| Wound | Cross/bandage | Red |
| Poison | Skull/droplet | Green |
| Disease | Virus | Yellow-green |
| Invisibility | Eye-slash | Purple |
| Hidden | Person-dash | Gray |
| Stunned | Stars | Yellow |
| Unconscious | Moon | Dark gray |
| Burning | Flame | Orange |
| Frozen | Snowflake | Light blue |
| Blessed/Buff | Arrow-up | Blue |
| Cursed/Debuff | Arrow-down | Dark red |
| Concentrating | Brain | Purple |
| Casting | Wand | Gold |
| Gathering Mana | Sparkles | School color |

---

## Tab Organization

### Tab 1: Status (Health Overview)

**Purpose**: Provide overall character status at a glance.

**Content**:
- **Health Pools**: FAT and VIT with detailed pending damage/healing display
- **Damage Pools**: Visual representation of pending damage queues
- **Action Points**: Available AP with recovery rate
- **Active Effects**: Complete list of all active effects with:
  - Effect name and icon
  - Source (spell, poison, item, wound)
  - Duration remaining
  - Effect magnitude/description
- **Wounds**: Location-specific wound display with severity indicators
  - Head, Torso, Left Arm, Right Arm, Left Leg, Right Leg
  - Light vs Serious wounds
  - Crippled/Disabled status

---

### Tab 2: Combat

**Purpose**: Streamlined interface for combat actions with equipped gear.

#### Skill Display Rules

Show skills based on equipment and usability:

| Skill Category | Display Rule |
|----------------|--------------|
| **Attribute Skills** | Always show (Physicality, Dodge, Drive, Awareness, etc.) |
| **Equipped Weapon Skills** | Show only if weapon of that type is equipped |
| **Armor Skills** | Show only if armor/shield of that type is equipped |
| **Movement Skills** | Always show (Sprint, etc.) |
| **Perception Skills** | Always show (Awareness-based) |

#### Combat Tab Layout

The Combat tab has three modes:

1. **Default Mode** - Skill list with Attack/Defend buttons
2. **Attack Mode** - Full attack workflow interface
3. **Defend Mode** - Full defense workflow interface

##### Default Mode

```
+---------------------------+---------------------------+
|     Combat Skills         |        Actions            |
+---------------------------+---------------------------+
| [Skill] [AS] [dice icon]  |  [ATTACK] (large button)  |
| [Skill] [AS] [dice icon]  |                           |
| [Skill] [AS] [dice icon]  |  [DEFEND] (large button)  |
| ...                       |                           |
|                           |  [REST] (button)          |
+---------------------------+---------------------------+
```

**Compact Skill Display**:
- Skill name (truncated if needed)
- Ability Score (AS) in bold
- Dice icon button to "use" the skill (for non-combat skill checks)

##### Rest Action

A character can choose to rest to recover fatigue more quickly.

- **Cost**: 1 AP
- **Effect**: Adds 1 FAT to the character's **pending healing** pool. This is in addition to any automatic end-of-round recovery.
- **UI**: A "REST" button is available in the Combat tab's default mode. Clicking it immediately deducts the AP and applies the healing effect. No new mode is needed.
- **Log**: "[Character] rests, recovering some fatigue."

##### Attack Mode

Triggered when user clicks "ATTACK" button. Replaces the main content area.

**Attack Workflow UI**:

```
+-------------------------------------------------------+
| ATTACK MODE                              [Cancel]     |
+-------------------------------------------------------+
| 1. Select Weapon/Skill                                |
|    [Dropdown: Sword (AS 14), Dagger (AS 12), ...]    |
+-------------------------------------------------------+
| 2. Power the Action                                   |
|    ( ) Standard: 1 AP + 1 FAT                        |
|    ( ) Fatigue-Free: 2 AP                            |
+-------------------------------------------------------+
| 3. Boost Attack (optional)                            |
|    [+] [-] AP Boost: 0  (+0 AS)                      |
|    [+] [-] FAT Boost: 0  (+0 AS)                     |
|    Total Boost: +0 AS                                 |
+-------------------------------------------------------+
| 4. Add Physicality Bonus                              |
|    [ ] Include Physicality check (+SV bonus)         |
+-------------------------------------------------------+
| Summary                                               |
|    Base AS: 14                                        |
|    Boost: +2                                          |
|    Final AS: 16                                       |
|    Cost: 1 AP, 3 FAT                                  |
+-------------------------------------------------------+
|              [ROLL ATTACK]                            |
+-------------------------------------------------------+
```

**Attack Result**:
- Roll 4dF+ and display result
- Calculate AV = AS + 4dF+ result
- If Physicality included, roll separately and show SV modifier
- Display final AV prominently
- Log to Activity: "[Character] attacks with [Weapon]. AV: [value]"

##### Defend Mode

Triggered when user clicks "DEFEND" button. Replaces the main content area.

**Defense Types**:
- **Dodge**: Use Dodge skill (DEX-based) to generate TV
- **Parry**: Use equipped weapon's skill to generate TV instead of Dodge (requires weapon equipped)
- **Shield Block**: Use Shield skill to block (requires shield equipped)
- **Passive**: No roll, TV = Dodge AS - 1 (for when out of resources or choosing not to actively defend)

**Note**: Parry is NOT a "mode" - it's simply choosing to use your weapon skill for defense instead of Dodge. All active defenses (Dodge, Parry, Shield Block) cost the same: 1 AP + 1 FAT (or 2 AP).

**Defense Workflow UI**:

```
+-------------------------------------------------------+
| DEFEND MODE                              [Cancel]     |
+-------------------------------------------------------+
| 1. Select Defense Type                                |
|    ( ) Dodge (AS: 12)                                |
|    ( ) Parry with [Sword] (AS: 14)  [if equipped]    |
|    ( ) Shield Block (AS: 10)        [if equipped]    |
|    ( ) Passive Defense (TV = 11, no cost)            |
+-------------------------------------------------------+
| 2. Power the Defense (if active defense selected)     |
|    ( ) Standard: 1 AP + 1 FAT                        |
|    ( ) Fatigue-Free: 2 AP                            |
+-------------------------------------------------------+
| 3. Boost Defense (optional)                           |
|    [+] [-] AP Boost: 0  (+0 AS)                      |
|    [+] [-] FAT Boost: 0  (+0 AS)                     |
+-------------------------------------------------------+
| Summary                                               |
|    Defense: Parry with Sword                          |
|    Base AS: 14                                        |
|    Boost: +1                                          |
|    Final AS: 15                                       |
|    Cost: 1 AP, 2 FAT                                  |
+-------------------------------------------------------+
|              [ROLL DEFENSE]                           |
+-------------------------------------------------------+
```

**Defense Result**:
- Roll 4dF+ and display result
- Calculate TV = AS + 4dF+ result (or passive TV if no roll)
- Display final TV prominently
- Log to Activity: "[Character] defends with [Method]. TV: [value]"

##### Armor/Damage Resolution (Post-Defense)

After defense is rolled, if attack hit (SV >= 0), show damage resolution:

```
+-------------------------------------------------------+
| RESOLVE DAMAGE                           [Skip]       |
+-------------------------------------------------------+
| Enter Attacker's AV: [    ]                           |
|                                                       |
| Your TV: 13                                           |
| SV = AV - TV = [calculated]                           |
+-------------------------------------------------------+
| Shield Defense (optional)                             |
|    [Use Shield Block]                                 |
|    Shield AS: 10, Absorbs up to [X] SV               |
+-------------------------------------------------------+
| Armor Absorption                                      |
|    Location: [Roll: Torso]                           |
|    Armor at location: Chainmail (absorbs 3)          |
|    Final SV after armor: [calculated]                |
+-------------------------------------------------------+
| Damage Result                                         |
|    Final SV: 4                                        |
|    Damage Roll: 1d10 = 7                             |
|    FAT Damage: 7                                      |
|    VIT Damage: 4                                      |
|    Wounds: 1                                          |
+-------------------------------------------------------+
|              [APPLY DAMAGE]                           |
+-------------------------------------------------------+
```

---

### Tab 3: Skills

**Purpose**: General skill usage outside of combat. Shows ALL non-magic skills.

**Layout**:
- Search/filter box
- Compact skill list with all skills
- Skill check panel

**Skill Categories Displayed**:
- Attribute skills (Physicality, Dodge, Drive, Reasoning, Awareness, Focus, Influence, Bearing)
- Weapon skills (all known, regardless of equipment)
- Crafting skills
- Social skills
- Perception skills
- Movement skills

**Compact Skill Row**:
```
[Skill Name] [Level] [AS: ##] [🎲]
```

**Skill Check Panel** (when skill selected):
```
+---------------------------------------+
| [Skill Name]                          |
+---------------------------------------+
| AS: 14                                |
+---------------------------------------+
| Target Value (TV): [  8  ]            |
+---------------------------------------+
| Power Action:                         |
|   ( ) 1 AP + 1 FAT                    |
|   ( ) 2 AP                            |
+---------------------------------------+
| Boost: [+] 0 [-]  (+0 AS)             |
+---------------------------------------+
| Final AS: 14                          |
+---------------------------------------+
|        [ROLL CHECK]                   |
+---------------------------------------+
```

**Result Display**:
- Show roll, AS, TV, and SV
- Interpret result (Success/Failure with quality)
- Log to Activity

---

### Tab 4: Magic

**Purpose**: Mana collection and spell casting.

#### Section 1: Mana Pools

Display current mana for each school:

```
+---------------------------------------+
| MANA POOLS                            |
+---------------------------------------+
| Fire    [████████░░] 8/10             |
| Water   [██████░░░░] 6/10             |
| Light   [██████████] 10/10            |
| Life    [████░░░░░░] 4/10             |
+---------------------------------------+
```

#### Section 2: Mana Collection

Separate from spell casting. When collecting mana:

```
+---------------------------------------+
| GATHER MANA                           |
+---------------------------------------+
| Select School: [Fire ▼]               |
|                                       |
| Mana Skill AS: 12                     |
|                                       |
| Target Value (from GM): [  6  ]       |
| (Default: 6, may vary by narrative)   |
+---------------------------------------+
| Power Action:                         |
|   ( ) 1 AP + 1 FAT                    |
|   ( ) 2 AP                            |
+---------------------------------------+
|        [BEGIN GATHERING]              |
+---------------------------------------+
```

**Mana Collection Mechanics**:
1. Roll skill check against TV (default 6)
2. Calculate SV
3. Look up mana recovered from result table
4. **Create a "Gathering [School] Mana" Effect** on the character:
   - Duration: X minutes (based on mana to recover, typically 1 minute per mana)
   - Effect indicates character is concentrating
   - Character cannot take other actions while gathering
   - Mana is granted when effect expires
5. Log to Activity: "[Character] begins gathering [School] mana. Will recover [X] mana in [Y] minutes."

**Interruption Rules**:
- If the character takes **any action** while gathering, the effect is canceled
- If the character takes **damage** while gathering, the effect is canceled
- No mana is recovered on cancellation
- Log on cancel: "[Character]'s mana gathering was interrupted."

#### Section 3: Spell Casting

When casting spells, the tab enters **Casting Mode**:

**Spell List** (default view):
```
+-----------------------------------------------+
| KNOWN SPELLS                      [Filter ▼]  |
+-----------------------------------------------+
| [Spell Name] [School] [AS: ##] [Cost] [🎲]    |
| Fire Bolt     Fire     AS: 14    2     [🎲]   |
| Heal          Life     AS: 12    3     [🎲]   |
| Shield        Light    AS: 10    1     [🎲]   |
+-----------------------------------------------+
```

**Casting Mode** (when spell selected):

```
+-------------------------------------------------------+
| CASTING: Fire Bolt                       [Cancel]     |
+-------------------------------------------------------+
| Spell Type: Targeted | Range: Medium | School: Fire   |
+-------------------------------------------------------+
| 1. Power the Skill Action                             |
|    ( ) Standard: 1 AP + 1 FAT                        |
|    ( ) Fatigue-Free: 2 AP                            |
+-------------------------------------------------------+
| 2. Power the Spell                                    |
|    Base Cost: 2 Fire Mana                             |
|    ( ) Pay with Mana (2 mana)                        |
|    ( ) Pay with FAT (2 FAT)                          |
|    ( ) Mixed: [1] mana + [1] FAT                     |
+-------------------------------------------------------+
| 3. Boost Spell (optional)                             |
|    [+] [-] Extra Mana: 0  (+0 effect)                |
|    [+] [-] Extra FAT: 0   (+0 effect)                |
+-------------------------------------------------------+
| 4. Target                                             |
|    [Select target or location]                        |
+-------------------------------------------------------+
| Summary                                               |
|    Spell AS: 14                                       |
|    Cost: 1 AP, 1 FAT, 2 Mana                         |
|    Boost: +0                                          |
|    Cast Time: Instant                                 |
+-------------------------------------------------------+
|              [CAST SPELL]                             |
+-------------------------------------------------------+
```

**Spells with Cast Time**:
- Some spells take time to cast (multiple rounds or minutes)
- Create a "Casting [Spell Name]" Effect
- Character is focused on casting
- Spell triggers when effect expires
- Log: "[Character] begins casting [Spell]. Will complete in [X rounds/minutes]."

**Cast Time Interruption**:
- If the character takes **any action** while casting, the spell is canceled
- If the character takes **damage** while casting, the spell is canceled
- **Refund**: Only the boost cost (extra mana/FAT) is refunded; base casting cost is lost
- Log on cancel: "[Character]'s casting of [Spell] was interrupted. [X mana/FAT] refunded."

**Concentration Spells**:
- Some spells require ongoing concentration to maintain their effect
- Create a "Maintaining [Spell Name]" Effect
- **Per-Spell Cost**: Each concentration spell defines its own AP/FAT drain per round
- **Limited Actions**: Character can take limited other actions at GM's discretion
- **Ending Concentration**:
  - Character can choose to end concentration at any time
  - GM can cancel the effect from the GM screen
  - When concentration ends, the spell effect also ends
- Log: "[Character] is maintaining concentration on [Spell] ([X] FAT/round)."
- Log on end: "[Character] stops concentrating on [Spell]."

---

## Combat Flow Summary

The complete combat exchange flow:

### Attacker's Turn

1. **Declare Attack**: Attacker clicks "ATTACK" in Combat tab
2. **Configure Attack**:
   - Select weapon/skill
   - Choose power method (1 AP + 1 FAT or 2 AP)
   - Apply boosts (optional)
   - Include Physicality check (optional)
3. **Roll Attack**: System rolls 4dF+, calculates AV
4. **Broadcast**: Activity log shows: "[Attacker] attacks [Target] with [Weapon]. AV: [value]"

### Defender's Turn

1. **Acknowledge Attack**: Defender clicks "DEFEND" in Combat tab
2. **Configure Defense**:
   - Select defense type (Dodge, Parry, Shield Block, Passive)
   - Choose power method
   - Apply boosts (optional)
3. **Roll Defense**: System rolls 4dF+ (if active), calculates TV
4. **Broadcast**: Activity log shows: "[Defender] defends with [Method]. TV: [value]"

### Damage Resolution (Defender)

1. **Calculate SV**: Defender enters attacker's AV
   - SV = AV - TV
   - Broadcast: "Attack [succeeds/fails]. SV: [value]"

2. **Shield Defense** (if SV >= 0 and shield equipped):
   - Roll shield skill check
   - Shield absorbs damage based on rating
   - Shield durability reduced
   - Calculate modified SV

3. **Armor Defense** (if SV >= 0):
   - Determine hit location (1d12)
   - Apply armor absorption at location
   - Armor durability reduced
   - Calculate final SV

4. **Apply Damage** (if final SV >= 0):
   - Look up damage roll from SV
   - Roll damage dice
   - Convert to FAT/VIT/Wounds using damage table
   - Apply to pending damage pools
   - Broadcast: "Final SV: [X]. [Defender] takes [Y] FAT, [Z] VIT, [W] wound(s)."

---

## Reusable Components

### PendingPoolBar.razor

Shared component for displaying pending damage/healing:

**Parameters**:
```csharp
[Parameter] public int CurrentValue { get; set; }
[Parameter] public int MaxValue { get; set; }
[Parameter] public int PendingDamage { get; set; }
[Parameter] public int PendingHealing { get; set; }
[Parameter] public bool ShowLabels { get; set; } = false;
[Parameter] public string Height { get; set; } = "8px";
```

**Usage Examples**:
- Top status bar (compact)
- Status tab (detailed)
- Any pool display needing pending visualization

### SkillRow.razor

Compact skill display component:

**Parameters**:
```csharp
[Parameter] public string SkillName { get; set; }
[Parameter] public int Level { get; set; }
[Parameter] public int AbilityScore { get; set; }
[Parameter] public EventCallback OnUse { get; set; }
[Parameter] public bool CanUse { get; set; } = true;
```

### EffectIcon.razor

Effect indicator icon:

**Parameters**:
```csharp
[Parameter] public string EffectType { get; set; }
[Parameter] public string Tooltip { get; set; }
[Parameter] public int? Stacks { get; set; }
```

### ActionCostSelector.razor

Reusable action cost selection:

**Parameters**:
```csharp
[Parameter] public int AvailableAP { get; set; }
[Parameter] public int AvailableFAT { get; set; }
[Parameter] public EventCallback<ActionCost> OnCostSelected { get; set; }
```

### BoostSelector.razor

Reusable boost selection with AP/FAT controls:

**Parameters**:
```csharp
[Parameter] public int MaxAPBoost { get; set; }
[Parameter] public int MaxFATBoost { get; set; }
[Parameter] public EventCallback<int> OnBoostChanged { get; set; }
```

---

## Activity Log Integration

All combat and skill actions should publish to the Activity Log for table visibility:

| Action | Log Format |
|--------|------------|
| Attack declared | "[Name] attacks with [Weapon]. AV: [X]" |
| Defense rolled | "[Name] defends with [Method]. TV: [X]" |
| Attack result | "Attack [succeeds/fails]. SV: [X]" |
| Shield used | "[Name] blocks with shield, absorbs [X] SV" |
| Armor absorbs | "Armor absorbs [X] SV at [Location]" |
| Damage taken | "[Name] takes [X] FAT, [Y] VIT, [Z] wound(s)" |
| Rest | "[Name] rests to recover fatigue." |
| Skill check | "[Name] uses [Skill]. Result: [Success/Fail], SV: [X]" |
| Mana gathering | "[Name] begins gathering [School] mana ([X] in [Y] min)" |
| Spell cast | "[Name] casts [Spell]. [Effect description]" |
| Effect applied | "[Name] gains effect: [Effect Name]" |
| Effect expired | "[Name] loses effect: [Effect Name]" |

---

## UI/UX Considerations

### Responsive Design
- Mobile-friendly layout
- Tabs collapse to icons on small screens
- Compact skill rows stack vertically on mobile

### Accessibility
- Keyboard navigation for all actions
- Screen reader support for status indicators
- High contrast mode support

### Performance
- Lazy-load spell definitions
- Cache skill calculations
- Debounce boost slider changes

### Visual Feedback
- Roll animations for dice
- Progress indicators for pending damage/healing
- Highlight active effects
- Flash on damage/healing received

---

## Implementation Priorities

### Phase 1: Core Infrastructure
1. PendingPoolBar shared component
2. Updated top status bar with pending pools and effect icons
3. SkillRow compact component

### Phase 2: Status Tab Enhancement
1. Detailed health pool displays with pending bars
2. Active effects list with full details
3. Wound tracking by location

### Phase 3: Combat Tab Overhaul
1. Equipped-only skill filtering
2. Attack Mode workflow
3. Defend Mode workflow
4. Damage resolution workflow

### Phase 4: Skills Tab
1. All-skills list with filtering
2. Generic skill check workflow
3. Activity log integration

### Phase 5: Magic Tab
1. Mana pool displays
2. Mana gathering workflow with Effect creation
3. Spell casting workflow
4. Concentration spell management

---

## Related Documents

- [PLAY_PAGE_IMPLEMENTATION_PLAN.md](PLAY_PAGE_IMPLEMENTATION_PLAN.md) - Detailed implementation checklist
- [ACTIONS.md](ACTIONS.md) - Action resolution framework
- [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md) - Combat mechanics
- [EFFECTS_SYSTEM.md](EFFECTS_SYSTEM.md) - Effect types and behaviors
- [MAGIC_SYSTEM_IMPLEMENTATION.md](MAGIC_SYSTEM_IMPLEMENTATION.md) - Magic system details
