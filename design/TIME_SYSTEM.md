# Time System

## Overview

The Threa time system provides structure for action resolution, recovery mechanics, and long-term effect processing. Time is measured in discrete units that scale from combat-focused rounds to narrative-focused days and weeks.

---

## Time Units

| Unit | Duration | Primary Use |
|------|----------|-------------|
| **Round** | 3 seconds | Combat, action resolution |
| **Minute** | 20 rounds | Short-term effects, exploration |
| **Turn** | 10 minutes | Dungeon exploration, travel segments |
| **Hour** | 6 turns | Rest, vitality recovery, travel |
| **Day** | 24 hours | Long rest, major healing, narrative time |
| **Week** | 7 days | Long-term effects, training, crafting |

---

## Round Structure

A round represents 3 seconds of game time and is the primary unit for combat and action resolution.

### Round Phases

1. **Initiative Determination**
2. **Action Phase** (characters act in initiative order)
3. **End of Round Processing**

### Initiative

Initiative is determined by **Available AP** (not maximum AP):

- Characters/NPCs with the **highest Available AP** act first
- Ties are resolved by GM discretion (or secondary factors like Awareness)
- Initiative is recalculated each round based on current Available AP

**Strategic Implication**: Characters who pass or take fewer actions may act earlier in subsequent rounds due to higher Available AP.

### Action Phase

During their turn, a character may:

- **Act**: Spend AP (and optionally FAT) to perform actions
- **Pass**: Take no action, preserving Available AP for later rounds
- **Delay**: Wait for a specific trigger (uses reaction mechanics)

**Multiple Actions Per Round**: Many actions (like melee attacks) can occur multiple times within a round. The limiting factor is Available AP, not an arbitrary action count.

### End of Round Processing

At the end of each round, the following occur in order:

1. **Fatigue Recovery**
   - Recover 1 FAT to pending healing pool (baseline recovery)
   - Modified by VIT level (see [GAME_RULES_SPECIFICATION.md](GAME_RULES_SPECIFICATION.md))
   - This recovery is added to the pending healing pool BEFORE applying pools
   - This allows natural recovery to offset pending damage in the same round

2. **Pending Damage/Healing Application**
   - Apply half of each pending pool (FAT and VIT)
   - Round up to ensure pools eventually reach zero

3. **Action Point Recovery**
   - Recover FAT / 4 AP (minimum 1)
   - Return Locked AP to Available
   - Reset Spent AP to 0
   - Cap at Max AP

4. **Wound Damage** (every 2 rounds)
   - Each wound deals 1 FAT damage
   - Tracked separately per wound

5. **Cooldown Advancement**
   - Active cooldowns progress by 1 round (3 seconds)
   - Completed cooldowns make their actions available

6. **Duration Tracking**
   - Decrement active spell durations
   - Process environmental effects
   - Check for minute/turn/hour boundaries

---

## Action Timing

### Instant Actions

Most combat actions complete within a round and can be performed multiple times per round (AP permitting):

- Melee attacks
- Dodging/Defending
- Parrying
- Movement (minor repositioning)
- Casting most spells

### Cooldown Actions

Some actions require preparation time before they can be performed:

| Action | Typical Cooldown | Can Be Prepped? |
|--------|------------------|-----------------|
| Ready arrow (bow) | Skill-based (see below) | Yes (limited quantity) |
| Reload crossbow | 1-2 rounds | Partially (pre-wound) |
| Reload firearm | Varies by weapon | Yes (magazines) |
| Retrieve item from backpack | 1 round | Yes (move to accessible slot) |
| Drink potion | Instant if in hand | 1 round if stored |
| Cast ritual spell | Multiple rounds | No |

#### Ranged Weapon Cooldowns

Cooldown between shots varies by skill level:

| Skill Level | Cooldown |
|-------------|----------|
| 0 | 6 seconds (2 rounds) |
| 1 | 5 seconds |
| 2 | 4 seconds |
| 3 | 3 seconds (1 round) |
| 4-5 | 2 seconds |
| 6-7 | 1 second |
| 8-9 | 0.5 seconds |
| 10+ | No cooldown |

**Note**: Sub-round cooldowns (< 3 seconds) allow multiple shots per round.

### Cooldown Interruption

Cooldowns can be interrupted by:
- Taking damage
- Performing a different action
- Being knocked down or stunned

**Interruption Effects**:

| Cooldown Type | On Interrupt |
|---------------|--------------|
| **Resettable** | Progress resets to 0% (e.g., concentration spells) |
| **Pausable** | Progress freezes, resumes when action continues (e.g., reloading) |

The specific behavior is defined per action/item.

---

## Long-Term Time Events

Certain game effects are processed at longer intervals. The GM triggers these time events through the assistant, affecting all tracked characters and NPCs.

### End of Minute (20 rounds)

- Check for short-duration spell expirations
- Process poison/toxin effects
- Update environmental hazard exposure

### End of Turn (10 minutes)

- Dungeon exploration timekeeping
- Torch/light source consumption
- Wandering monster checks (if applicable)
- Short-term buff expirations

### End of Hour

- **Vitality Recovery**: Restore 1 VIT (if VIT > 0)
- Extended rest benefits
- Travel distance calculations
- Long-duration spell checks

### End of Day

- Full rest recovery (if applicable)
- Daily spell/ability resets
- Narrative time advancement
- Condition/disease progression

### End of Week

- Long-term healing
- Training progression
- Crafting project advancement
- Major world event processing

---

## Time Event System (Application)

The character assistant tracks time and triggers events when the GM advances time:

### Event Types

```
EndOfRound      - Every 3 seconds during active time
EndOfMinute     - Every 20 rounds
EndOfTurn       - Every 10 minutes (200 rounds)
EndOfHour       - Every 60 minutes
EndOfDay        - Every 24 hours
EndOfWeek       - Every 7 days
```

### Event Propagation

When a time event is triggered:

1. GM initiates time advancement through the assistant
2. Event propagates to all tracked characters/NPCs
3. Each entity processes relevant effects for that time unit
4. All shorter time units are also processed as needed
   - Example: EndOfHour triggers 60 EndOfMinute events and 1200 EndOfRound events (or summarized equivalent)

### Combat vs Narrative Time

| Mode | Time Tracking | Event Processing |
|------|---------------|------------------|
| **Combat** | Round-by-round | Full detail, per-round events |
| **Exploration** | Turn-by-turn | Summarized, skip to turn boundaries |
| **Travel** | Hour-by-hour | Highly summarized, key events only |
| **Downtime** | Day/Week | Narrative summary, major events |

---

## Recovery Summary

| Recovery Type | Timing | Amount | Conditions |
|---------------|--------|--------|------------|
| FAT (baseline) | End of round | 1 FAT | No pending FAT damage |
| FAT (low VIT) | Varies | 1 FAT | Modified by VIT level |
| VIT (baseline) | End of hour | 1 VIT | VIT > 0 |
| AP | End of round | FAT/4 (min 1) | Always |
| Wounds (natural) | Per 4 hours | 1 wound | Resting |

### VIT-Modified FAT Recovery

| Current VIT | FAT Recovery Rate |
|-------------|-------------------|
| 5+ | 1 per round (3 seconds) |
| 4 | 1 per minute |
| 3 | 1 per 30 minutes |
| 2 | 1 per hour |
| 1 | None |
| 0 | Dead |

---

## Game Time and Fictional Date/Time

### Elapsed Game Time

Game time is tracked as a `long` value representing the number of seconds since the start of the table (time 0). This value is incremented as the GM advances time through various time units:

| Time Advancement | Seconds Added |
|------------------|---------------|
| End of Round | 3 |
| End of Minute | 60 |
| End of Turn | 600 (10 minutes) |
| End of Hour | 3600 |
| End of Day | 86400 |
| End of Week | 604800 |

**Storage**: Add `ElapsedSeconds` (long) to the `GameTable` entity to track total elapsed game time.

### Fictional Date/Time Mapping (Future Feature)

Different game scenarios may use different calendar systems:
- **Fantasy**: "The 10th day of Mongroth in the year 123"
- **Sci-Fi**: Stardate 47634.2
- **Modern**: Standard Gregorian calendar
- **Post-Apocalyptic**: "Day 347 After the Fall"

**Planned Implementation**:

1. Each table/campaign defines a `TimeFormat` configuration that includes:
   - Calendar system type (Gregorian, custom fantasy, stardate, etc.)
   - Starting date/time in that system (mapping ElapsedSeconds = 0 to a specific date)
   - Format string for display

2. A `TimeFormatter` service converts `ElapsedSeconds` to displayable date/time strings based on the table's configuration

3. The player and GM screens display the formatted fictional time

**Note**: For initial implementation, display raw elapsed time as days/hours/minutes/seconds until the calendar system is built.

---

## Implementation Notes

### Time Tracking Requirements

The application needs to track:

- Current round number (within combat/encounter)
- Elapsed rounds since last minute/turn/hour boundary
- Active cooldowns per character (seconds remaining)
- Active durations per effect (rounds/minutes remaining)
- Pending damage/healing pools per character

### Event Handler Interface

Each tracked entity should respond to time events:

```
OnEndOfRound()    - AP recovery, pending damage, cooldowns
OnEndOfMinute()   - Short-term effect processing
OnEndOfTurn()     - Exploration-scale processing
OnEndOfHour()     - VIT recovery, long-term effects
OnEndOfDay()      - Daily resets, condition updates
OnEndOfWeek()     - Training, crafting, major progression
```

### Interrupt Capability

The assistant app must provide a way for the GM (or player) to **interrupt** a long-running action:

- Interrupt cancels or pauses an in-progress cooldown action
- Effect depends on action type (resettable vs pausable)
- Beyond the mechanical interrupt, narrative consequences are determined by the GM
- Examples: interrupting an aim action, stopping a reload mid-progress

### Related Systems

- **Action Points**: See [ACTION_POINTS.md](ACTION_POINTS.md) for AP mechanics
- **Health Pools**: See [GAME_RULES_SPECIFICATION.md](GAME_RULES_SPECIFICATION.md) for FAT/VIT
- **Combat**: See [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md) for combat actions and resolution
- **Effects**: See [EFFECTS_SYSTEM.md](EFFECTS_SYSTEM.md) for wounds, conditions, buffs, spell effects
