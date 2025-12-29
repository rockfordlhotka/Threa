# Skill Progression System

## Overview

Threa uses an experience point (XP) based skill progression system. Characters earn XP through gameplay and spend it to advance individual skills. The cost to advance depends on both the skill's current level and the skill's difficulty rating.

## Core Mechanics

### Experience Point Cost Table

Skill advancement costs are determined by a lookup table based on:

1. **Current Skill Level**: The skill's level before advancement (0-9)
2. **Skill Difficulty**: A rating from 1 (easiest) to 14 (hardest) assigned to each skill

### XP Cost Lookup Table

| Level | Diff 1 | Diff 2 | Diff 3 | Diff 4 | Diff 5 | Diff 6 | Diff 7 | Diff 8 | Diff 9 | Diff 10 | Diff 11 | Diff 12 | Diff 13 | Diff 14 |
|-------|--------|--------|--------|--------|--------|--------|--------|--------|--------|---------|---------|---------|---------|---------|
| 0→1 | 0.1 | 0.3 | 0.5 | 1 | 1 | 1 | 2 | 2 | 3 | 4 | 4 | 5 | 6 | 6 |
| 1→2 | 0.3 | 0.5 | 1 | 2 | 3 | 3 | 4 | 5 | 5 | 6 | 6 | 7 | 8 | 8 |
| 2→3 | 0.5 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 7 | 8 | 9 | 10 | 10 | 11 |
| 3→4 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 | 10 | 11 | 12 | 13 | 14 |
| 4→5 | 2 | 3 | 4 | 5 | 6 | 8 | 9 | 10 | 12 | 13 | 14 | 16 | 17 | 18 |
| 5→6 | 3 | 5 | 7 | 10 | 12 | 15 | 17 | 20 | 22 | 24 | 27 | 29 | 32 | 34 |
| 6→7 | 4 | 7 | 11 | 14 | 18 | 21 | 25 | 28 | 32 | 35 | 39 | 43 | 46 | 50 |
| 7→8 | 8 | 17 | 25 | 33 | 41 | 50 | 58 | 66 | 74 | 83 | 91 | 99 | 107 | 116 |
| 8→9 | 22 | 44 | 65 | 87 | 109 | 131 | 152 | 174 | 196 | 218 | 240 | 261 | 283 | 305 |
| 9→10 | 37 | 74 | 111 | 148 | 185 | 222 | 259 | 296 | 333 | 370 | 407 | 444 | 481 | 518 |

**Note**: Fractional XP costs (0.1, 0.3, 0.5) allow multiple low-level skills to be advanced with a single XP.

### Skill Level Bonus

Each skill level provides a bonus to skill checks:

```
Skill Bonus = Skill Level - 5
```

| Level | Bonus |
|-------|-------|
| 0 | -5 |
| 1 | -4 |
| 2 | -3 |
| 3 | -2 |
| 4 | -1 |
| 5 | 0 |
| 6 | +1 |
| 7 | +2 |
| 8 | +3 |
| 9 | +4 |
| 10 | +5 |

## Skill Difficulty Ratings

**Game Balance Note**: Skills are not intended to progress significantly beyond level 10. Progression becomes prohibitively expensive after level 5 to maintain balanced gameplay.

### Suggested Difficulty Ratings by Category

| Skill Category | Difficulty Range | Typical |
|----------------|------------------|---------|
| Core Attribute Skills | 4-6 | 5 |
| Simple Weapons | 3-5 | 4 |
| Martial Weapons | 5-8 | 6 |
| Exotic Weapons | 8-11 | 9 |
| Cantrips | 2-4 | 3 |
| Standard Spells | 5-7 | 6 |
| Advanced Spells | 8-10 | 9 |
| Master Spells | 11-14 | 12 |
| Mana Recovery | 5-7 | 6 |
| Crafting | 6-9 | 7 |
| Social Skills | 3-5 | 4 |

## Progression Examples

**Sword Skill** (Difficulty 6, Martial Weapon):

| Level | XP Cost | Cumulative XP |
|-------|---------|---------------|
| 0→1 | 1 | 1 |
| 1→2 | 3 | 4 |
| 2→3 | 5 | 9 |
| 3→4 | 6 | 15 |
| 4→5 | 8 | 23 |
| 5→6 | 15 | 38 |
| 6→7 | 21 | 59 |
| 7→8 | 50 | 109 |

**Fireball Spell** (Difficulty 9, Advanced Spell):

| Level | XP Cost | Cumulative XP |
|-------|---------|---------------|
| 0→1 | 3 | 3 |
| 1→2 | 5 | 8 |
| 2→3 | 7 | 15 |
| 3→4 | 9 | 24 |
| 4→5 | 12 | 36 |
| 5→6 | 22 | 58 |
| 6→7 | 32 | 90 |
| 7→8 | 74 | 164 |

## Earning Experience Points

XP is awarded by the Game Master based on:

| Activity | Typical XP Award |
|----------|------------------|
| Minor encounter/challenge | 1-2 XP |
| Standard encounter | 3-5 XP |
| Significant challenge | 6-10 XP |
| Major story milestone | 10-20 XP |
| Campaign arc completion | 20-50 XP |

**Session Guidelines**: A typical session awards 5-15 XP depending on accomplishments.

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

The character sheet should:
- Track current level and available XP for each character
- Display skill level with appropriate rank label
- Show XP cost to advance each skill to the next level
- Allow spending XP to advance skills
- Display skill difficulty rating for reference

## Benefits of This System

1. **Player Agency**: Players choose which skills to advance
2. **Meaningful Differentiation**: Each level represents significant improvement
3. **Natural Plateaus**: Progress slows appropriately at higher levels
4. **Build Diversity**: Players choose breadth vs depth
5. **Simple Mechanics**: Lookup table is easy for GMs and players to use
6. **Balanced Progression**: Difficulty ratings ensure complex skills cost more

---

**Related Documents**:
- [Game Rules Specification](GAME_RULES_SPECIFICATION.md) - Full skill system details
- [Database Design](DATABASE_DESIGN.md) - Schema for tracking progression
