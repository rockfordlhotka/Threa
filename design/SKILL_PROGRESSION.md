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
| 0→1 | 1 | 3 | 5 | 10 | 10 | 10 | 20 | 20 | 30 | 40 | 40 | 50 | 60 | 60 |
| 1→2 | 3 | 5 | 10 | 20 | 30 | 30 | 40 | 50 | 50 | 60 | 60 | 70 | 80 | 80 |
| 2→3 | 5 | 10 | 20 | 30 | 40 | 50 | 60 | 70 | 70 | 80 | 90 | 100 | 100 | 110 |
| 3→4 | 10 | 20 | 30 | 40 | 50 | 60 | 70 | 80 | 90 | 100 | 110 | 120 | 130 | 140 |
| 4→5 | 20 | 30 | 40 | 50 | 60 | 80 | 90 | 100 | 120 | 130 | 140 | 160 | 170 | 180 |
| 5→6 | 30 | 50 | 70 | 100 | 120 | 150 | 170 | 200 | 220 | 240 | 270 | 290 | 320 | 340 |
| 6→7 | 40 | 70 | 110 | 140 | 180 | 210 | 250 | 280 | 320 | 350 | 390 | 430 | 460 | 500 |
| 7→8 | 80 | 170 | 250 | 330 | 410 | 500 | 580 | 660 | 740 | 830 | 910 | 990 | 1070 | 1160 |
| 8→9 | 220 | 440 | 650 | 870 | 1090 | 1310 | 1520 | 1740 | 1960 | 2180 | 2400 | 2610 | 2830 | 3050 |
| 9→10 | 370 | 740 | 1110 | 1480 | 1850 | 2220 | 2590 | 2960 | 3330 | 3700 | 4070 | 4440 | 4810 | 5180 |

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
| 0→1 | 10 | 10 |
| 1→2 | 30 | 40 |
| 2→3 | 50 | 90 |
| 3→4 | 60 | 150 |
| 4→5 | 80 | 230 |
| 5→6 | 150 | 380 |
| 6→7 | 210 | 590 |
| 7→8 | 500 | 1090 |

**Fireball Spell** (Difficulty 9, Advanced Spell):

| Level | XP Cost | Cumulative XP |
|-------|---------|---------------|
| 0→1 | 30 | 30 |
| 1→2 | 50 | 80 |
| 2→3 | 70 | 150 |
| 3→4 | 90 | 240 |
| 4→5 | 120 | 360 |
| 5→6 | 220 | 580 |
| 6→7 | 320 | 900 |
| 7→8 | 740 | 1640 |

## Earning Experience Points

XP is awarded by the Game Master based on:

| Activity | Typical XP Award |
|----------|------------------|
| Minor encounter/challenge | 10-20 XP |
| Standard encounter | 30-50 XP |
| Significant challenge | 60-100 XP |
| Major story milestone | 100-200 XP |
| Campaign arc completion | 200-500 XP |

**Session Guidelines**: A typical session awards 50-150 XP depending on accomplishments.

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
