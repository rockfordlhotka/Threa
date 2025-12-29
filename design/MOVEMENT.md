# Movement System

## Overview

Movement in Threa covers how characters navigate the battlefield and the world. Movement interacts with the Action Points system and affects combat positioning. Movement uses the same **power-of-2 range system** as ranged combat.

---

## Range and Distance

```
Distance (meters) = Range Value²
```

| Range Value | Distance (meters) | Description |
|-------------|-------------------|-------------|
| 0 | 0 | Touch/contact |
| 1 | 1 | Adjacent/reach |
| 2 | 4 | Short distance |
| 3 | 9 | Across a room |
| 4 | 16 | Across a large room |
| 5 | 25 | Maximum combat sprint |

---

## Combat Movement (Per Round)

Within a combat round (3 seconds), movement is categorized by range and action cost.

### Free Positioning (No Action Cost)

Characters can adjust position up to **Range 2 (4 meters)** without spending an action:

- Stepping aside, shifting stance
- Circling an opponent
- Minor tactical repositioning
- Moving within melee engagement

This free movement can occur at any point during the character's turn.

### Sprint Action

**Cost**: 1 AP + 1 FAT (or 2 AP)

Move up to **Range 3 (9 meters)** as an action:

- Can be performed before or after another action in the same round
- Subject to the -1 AS multiple action penalty if combined with other actions
- Useful for closing distance or tactical repositioning

### Full-Round Sprint

**Cost**: All actions for the round

Move up to **Range 5 (25 meters)**:

- Character dedicates entire round to movement
- No other actions possible
- Maximum distance a human can cover in one round
- Used for fleeing, charging across open ground, or pursuit

### Combat Movement Summary (Human)

| Movement Type | Range | Distance | Cost |
|---------------|-------|----------|------|
| Free positioning | 0-2 | 0-4m | None |
| Sprint action | 3 | 9m | 1 AP + 1 FAT |
| Full-round sprint | 5 | 25m | Full round |

---

## Travel Movement (Sustained)

For extended travel over time, movement rates are measured per round but calculated for long-distance travel with associated fatigue costs.

### Human Travel Rates

| Movement Type | Speed (per round) | Speed (per hour) | FAT Cost |
|---------------|-------------------|------------------|----------|
| Walking | 4m | 4.8 km | 1 FAT per 2 km |
| Endurance running | 10m | 12 km | 1 FAT per km |
| Burst running | 12m | 14.4 km | 1 FAT per 6m |
| Fast sprinting | 16m | 19.2 km | 1 FAT per 5m |

### Travel Fatigue

- **Walking**: Sustainable for long periods with minimal fatigue
- **Endurance running**: Sustainable pace for trained characters
- **Burst running**: Short-term speed boost, fatiguing
- **Fast sprinting**: Maximum effort, extremely fatiguing

**Example**: A human with FAT 15 walking 2 km:
- Distance: 2,000m
- FAT cost: 1 FAT per 2 km = 1 FAT
- Time: 2,000m ÷ 4m per round = 500 rounds = 25 minutes

**Example**: A human with FAT 15 sprinting 80m:
- Distance: 80m
- FAT cost: 1 FAT per 5m = 16 FAT (would exhaust the character)
- Time: 80m ÷ 16m per round = 5 rounds = 15 seconds

---

## Movement Modifiers

### Encumbrance

*To be documented: movement penalties based on carried weight relative to carrying capacity*

### Difficult Terrain

| Terrain Type | Effect |
|--------------|--------|
| Rough ground | -1 Range category |
| Dense vegetation | -1 Range category |
| Steep slope (up) | -1 Range category |
| Steep slope (down) | No penalty |
| Water (shallow) | -1 Range category |
| Water (deep) | Special rules (swimming) |

### Species Differences

*To be documented: movement rate modifiers for non-human species*

---

## Special Movement

### Climbing

*To be documented*

### Swimming

*To be documented*

### Jumping

*To be documented*

### Falling

*To be documented*

---

## Leaving Melee Range

Moving from Range 1 (melee) to Range 2+ from an opponent:

- Requires at least free positioning (Range 2)
- May allow opportunity attack from opponent (GM discretion)
- Defensive movement (backing away) may avoid opportunity attacks

---

## Implementation Notes

The movement system should track:
- Character position relative to others (range values)
- Movement taken this round (free vs action)
- Terrain effects on movement
- Cumulative travel fatigue for extended journeys

Movement is primarily narrative in tabletop play, with the assistant tracking relative distances when needed for ranged combat and area effects.
