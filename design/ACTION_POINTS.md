# Action Points System

## Overview

Action Points (AP) represent a character's capacity to perform actions during combat and other time-critical situations. AP provides a resource management layer that interacts with Fatigue (FAT) to create tactical decisions about action economy.

---

## Maximum Action Points

A character's maximum AP is calculated from their total accumulated skill levels across all skills:

| Total Skill Levels | Max AP |
|--------------------|--------|
| 0-10 | 1 |
| 11-20 | 2 |
| 21-30 | 3 |
| 31-40 | 4 |
| etc. | +1 per 10 levels |

**Examples**:
- New character with 10 total skill levels: Max AP = 1
- Developing character with 15 total skill levels: Max AP = 2
- Experienced character with 47 total skill levels: Max AP = 5
- Veteran character with 120 total skill levels: Max AP = 12

**Note**: Total Skill Levels is the sum of all individual skill levels the character has, not the count of skills. A character with Swords 3, Dodge 2, and Focus 4 has 9 total skill levels.

---

## Starting Action Points

At the beginning of combat or any action-based encounter:

- Characters start with **AP = Max AP**
- All AP is considered "Available" for use

---

## Action Costs

Every action a character takes requires **1 AP** plus an additional cost that can be paid with either FAT or AP:

| Payment Option | AP Cost | FAT Cost | Total Resources |
|----------------|---------|----------|-----------------|
| Standard Action | 1 AP | 1 FAT | 1 AP + 1 FAT |
| Fatigue-Free Action | 2 AP | 0 FAT | 2 AP |

### When to Use Each Option

**Standard Action (1 AP + 1 FAT)**:
- Most common choice during combat
- Preserves AP for more actions
- Gradually depletes stamina over extended encounters

**Fatigue-Free Action (2 AP)**:
- Useful when FAT is critically low
- Preserves stamina for escape or recovery
- Limits total actions per round but avoids exhaustion penalties

### Action Examples

All of the following actions require the standard action cost:

- **Attack** (melee or ranged)
- **Defend** (dodge or parry)
- **Cast a spell**
- **Use a skill** (in time-critical situations)
- **Use an item** (potions, scrolls, etc.)
- **Move** (significant movement, not minor repositioning)
- **Special maneuvers** (knockback, disarm, etc.)

---

## Action Point Recovery

At the **end of each round**, characters recover AP based on their current Fatigue:

```
AP Recovery = Current FAT / 4 (minimum 1)
```

**Recovery Rules**:
- Recovery is calculated from **current FAT**, not maximum FAT
- At least 1 AP is always recovered, even at very low FAT
- Recovered AP cannot exceed Max AP
- Recovery happens automatically at round end

**Examples**:
- Character with FAT 16: Recovers 4 AP per round
- Character with FAT 8: Recovers 2 AP per round
- Character with FAT 3: Recovers 1 AP per round (minimum)
- Character with FAT 0: Recovers 1 AP per round (minimum, but likely cannot act due to exhaustion)

---

## Action Point States

AP exists in three states during a round:

| State | Description |
|-------|-------------|
| **Available** | AP that can be spent on actions |
| **Spent** | AP used for actions this round |
| **Locked** | AP committed to ongoing effects (rare) |

### End of Round Processing

1. Calculate recovery: `FAT / 4` (minimum 1)
2. Add recovery to Available
3. Return Locked AP to Available
4. Reset Spent to 0
5. Cap Available at Max AP

---

## Interaction with Low Health States

### Low Fatigue Effects

When FAT is critically low, the character may need to pass Focus checks to act (see [GAME_RULES_SPECIFICATION.md](GAME_RULES_SPECIFICATION.md)):

| Current FAT | Effect |
|-------------|--------|
| FAT = 3 | Focus check TV 5 required to act |
| FAT = 2 | Focus check TV 7 required to act |
| FAT = 1 | Focus check TV 12 required to act |
| FAT = 0 | Cannot act; suffer 2 VIT damage |

**Note**: Failed Focus checks do not consume the action cost (no FAT spent on failed attempt).

### Low Vitality Effects

Severe injury affects both FAT recovery and action capability:

| Current VIT | Effect on Actions |
|-------------|-------------------|
| VIT = 3 | Focus check TV 7 required to attempt any action |
| VIT = 2 | Focus check TV 12 required to attempt any action |
| VIT = 1 | Cannot perform actions |
| VIT = 0 | Death |

---

## Rest Action

Characters can choose to rest instead of taking combat actions:

**Rest Mechanics**:
- Costs AP (variable amount chosen by player)
- Adds equivalent FAT healing to pending healing pool
- Locks remaining Available AP for the round
- Character cannot take other actions that round

**Example**: A character with 4 Available AP chooses to rest with 2 AP:
- 2 pending FAT healing is queued
- Remaining 2 AP is locked
- No other actions possible until next round
- At round end: recovers normal AP + locked AP returns

---

## Strategic Considerations

### Resource Management

- **Early Combat**: Spend AP freely, FAT is abundant
- **Extended Combat**: Monitor FAT, consider fatigue-free actions
- **Escape Situations**: Preserve FAT for low-health Focus checks
- **Multiple Opponents**: Balance offense and defense AP allocation

### AP vs FAT Trade-offs

| Situation | Recommended Approach |
|-----------|---------------------|
| High FAT, Low AP | Standard actions (1 AP + 1 FAT) |
| Low FAT, High AP | Fatigue-free actions (2 AP) |
| Low Both | Rest action or retreat |
| High Both | Aggressive standard actions |

---

## Implementation Notes

### Calculation Summary

```
Max AP = ceiling(TotalSkillLevels / 10), minimum 1
         (0-10 levels = 1 AP, 11-20 = 2 AP, 21-30 = 3 AP, etc.)
Recovery = CurrentFAT / 4 (minimum 1)
Standard Action = 1 AP + 1 FAT
Fatigue-Free Action = 2 AP
```

### Related Systems

- **Actions**: See [ACTIONS.md](ACTIONS.md) for the universal action resolution framework
- **Fatigue**: See [GAME_RULES_SPECIFICATION.md](GAME_RULES_SPECIFICATION.md) for FAT calculation and effects
- **Combat**: See [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md) for combat-specific action rules
- **Time**: See [TIME_SYSTEM.md](TIME_SYSTEM.md) for rounds and timing
- **Movement**: See [MOVEMENT.md](MOVEMENT.md) for movement action rules
