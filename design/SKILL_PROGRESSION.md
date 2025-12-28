# Skill Progression System

## Overview

Threa uses a usage-based skill progression system where skills improve through actual practice. Each skill tracks usage events and converts them into skill level increases based on progression parameters.

## Core Mechanics

### Base Cost and Multiplier System

Each skill is defined with two key progression parameters:

1. **Base Cost**: The number of usage events required to advance from level 0 to level 1
2. **Multiplier**: A real number that compounds the cost for each subsequent level

### Cost Calculation Formula

```
Cost (N → N+1) = Base Cost × (Multiplier^N)
```

Where:
- **Base Cost**: Skill-specific starting difficulty (typically 10-100 usage events)
- **Multiplier**: Progression difficulty scaling (typically 2.0-3.5)
- **N**: Current skill level

## Skill Category Parameters

**Game Balance Note**: Skills are not intended to progress significantly beyond level 10. Progression becomes prohibitively expensive after level 5 to maintain balanced gameplay.

### Core Attribute Skills

- **Base Cost**: 15
- **Multiplier**: 2.5
- **Rationale**: Fundamental abilities that become exponentially harder to master

### Weapon Skills

- **Base Cost**: 25
- **Multiplier**: 2.2
- **Rationale**: Combat skills require practice but plateau at higher levels

### Individual Spell Skills

| Spell Type | Base Cost | Multiplier |
|------------|-----------|------------|
| Cantrips | 20 | 2.0 |
| Standard spells | 40 | 2.3 |
| Advanced spells | 80 | 2.8 |
| Master spells | 150 | 3.5 |

### Other Skills

| Category | Base Cost | Multiplier |
|----------|-----------|------------|
| Mana Recovery | 30 | 2.1 |
| Crafting | 35 | 2.4 |
| Social | 20 | 2.0 |

## Progression Examples

**Weapon Skill** (Base Cost: 25, Multiplier: 2.2):

| Level | Uses Required | Cumulative |
|-------|---------------|------------|
| 0→1 | 25 | 25 |
| 1→2 | 55 | 80 |
| 2→3 | 121 | 201 |
| 3→4 | 266 | 467 |
| 4→5 | 585 | 1,052 |
| 5→6 | 1,287 | 2,339 |
| 6→7 | 2,831 | 5,170 |

## Usage Event Types

Not all skill usage generates the same advancement potential:

| Event Type | Multiplier | Description |
|------------|------------|-------------|
| Routine Use | 1.0x | Standard skill application |
| Challenging Use | 1.5x | Difficult circumstances |
| Critical Success | 2.0x | Exceptional performance |
| Teaching Others | 0.8x | Instructing other characters |
| Training Practice | 0.5x | Safe, deliberate practice |

## Skill Level Meanings

| Level Range | Rank | Description |
|-------------|------|-------------|
| 1-3 | Novice | Learning fundamentals |
| 4-5 | Competent | Reliable skill use |
| 6-7 | Expert | Advanced techniques |
| 8-10 | Master | Exceptional ability |
| 11+ | Legendary | Virtually impossible |

## Practical Expectations

- **Most Characters**: Will reach levels 3-6 in primary skills
- **Dedicated Specialists**: May achieve levels 7-8 in 1-2 skills
- **Legendary Masters**: Levels 9-10+ are extremely rare achievements

## Character Sheet Integration

The assistant should:
- Track current level and usage points for each skill
- Calculate progress percentage toward next level
- Display skill level with appropriate rank label
- Show advancement notifications
- Support adding usage events with appropriate multipliers

## Benefits of This System

1. **Meaningful Differentiation**: Each level represents significant improvement
2. **Natural Plateaus**: Progress slows appropriately at higher levels
3. **Build Diversity**: Players choose breadth vs depth
4. **Long-term Goals**: High-level skills remain aspirational

---

**Related Documents**:
- [Game Rules Specification](GAME_RULES_SPECIFICATION.md) - Full skill system details
- [Database Design](DATABASE_DESIGN.md) - Schema for tracking progression
