# Threa TTRPG Assistant - Design Documentation

## Overview

This folder contains the game rules and design specifications for the Threa tabletop role-playing game system. The Threa TTRPG Character Sheet Assistant uses these rules to help players manage their characters during in-person tabletop play.

## Core Documents

### Game Rules

| Document | Description |
|----------|-------------|
| [Game Rules Specification](GAME_RULES_SPECIFICATION.md) | **Primary reference** - Complete game mechanics including dice system, attributes, skills, combat, damage, equipment, magic, and currency |
| [Actions](ACTIONS.md) | **Universal action framework** - The core action resolution flow that all skill uses follow |
| [Combat System](COMBAT_SYSTEM.md) | Combat resolution with AP integration, initiative, defense options, melee/ranged attacks, and damage |
| [Movement](MOVEMENT.md) | Movement categories, distances, and battlefield positioning |
| [Action Points](ACTION_POINTS.md) | Action economy system - AP calculation, costs, recovery, and interaction with Fatigue |
| [Time System](TIME_SYSTEM.md) | Rounds, initiative, cooldowns, and long-term time events (minute, turn, hour, day, week) |
| [Skill Progression](SKILL_PROGRESSION.md) | Usage-based skill advancement system with base costs, multipliers, and level meanings |

### Character Systems

| Document | Description |
|----------|-------------|
| [Effects System](EFFECTS_SYSTEM.md) | Wounds, conditions, buffs, debuffs, spell effects - anything that persists over time |
| [Equipment System](EQUIPMENT_SYSTEM.md) | Equipment slots, item bonuses, attribute modifiers, and stacking rules |
| [Item System Overview](ITEM_SYSTEM_OVERVIEW.md) | Architecture diagram, container hierarchy, weight/volume calculations |
| [Item Bonuses and Cascading Effects](ITEM_BONUSES_AND_CASCADING_EFFECTS.md) | Detailed explanation of how item bonuses affect skills and cascade through attributes |
| [Carrying Capacity Analysis](CARRYING_CAPACITY_ANALYSIS.md) | Exponential scaling formula for weight/volume based on Physicality |

### Reference Data

| Document | Description |
|----------|-------------|
| [Item List](ITEM_LIST.md) | Sample items with values and categories |
| [Currency System](CURRENCY_SYSTEM.md) | Four-tier currency (cp/sp/gp/pp), exchange rates, and pricing guidelines |

### User Interface

| Document | Description |
|----------|-------------|
| [Play Page Design](PLAY_PAGE_DESIGN.md) | Play page layout, combat workflows, skill usage, and magic casting UI |
| [Play Page Implementation Plan](PLAY_PAGE_IMPLEMENTATION_PLAN.md) | Detailed implementation checklist for play page features |

### Technical

| Document | Description |
|----------|-------------|
| [Database Design](DATABASE_DESIGN.md) | Schema design for characters, skills, items, effects, and inventory |
| [XP System Refinement](XP_SYSTEM_REFINEMENT.md) | Converting XP from decimals to integers, fixing XPTotal tracking |

## Quick Reference

### Core Mechanics

- **Dice System**: 4dF (Fudge dice) with exploding mechanics (4dF+)
- **Skill Check**: `Ability Score + 4dF+ vs Target Value`
- **Ability Score (AS)**: `Related Attribute + Skill Level - 5`

### Seven Core Attributes

1. **Physicality (STR)** - Physical strength
2. **Dodge (DEX)** - Agility and evasion
3. **Drive (END)** - Endurance and stamina
4. **Reasoning (INT)** - Intelligence and logic
5. **Awareness (ITT)** - Intuition and perception
6. **Focus (WIL)** - Willpower and concentration
7. **Bearing (PHY)** - Physical presence

### Health Pools

- **Fatigue (FAT)**: `(END × 2) - 5` - Stamina/exhaustion
- **Vitality (VIT)**: `(STR + END) - 5` - Life force

### Available Species

| Species | Modifiers |
|---------|-----------|
| Human | None (baseline) |
| Elf | INT +1, STR -1 |
| Dwarf | STR +1, DEX -1 |
| Halfling | DEX +1, ITT +1, STR -2 |
| Orc | STR +2, END +1, INT -1, PHY -1 |

### Currency

| Coin | Value |
|------|-------|
| Copper (cp) | Base unit |
| Silver (sp) | 20 cp |
| Gold (gp) | 400 cp |
| Platinum (pp) | 8,000 cp |

### Carrying Capacity

```
Max Weight = 50 lbs × (1.15 ^ (Physicality - 10))
Max Volume = 10 cu.ft. × (1.15 ^ (Physicality - 10))
```

## Using These Documents

1. **New to Threa?** Start with [Game Rules Specification](GAME_RULES_SPECIFICATION.md)
2. **Building a character?** Reference the attributes, species, and skill sections
3. **Managing equipment?** See [Equipment System](EQUIPMENT_SYSTEM.md) and [Item System Overview](ITEM_SYSTEM_OVERVIEW.md)
4. **Implementing features?** Check [Database Design](DATABASE_DESIGN.md) for schema details

## Document Status

These documents define the game mechanics for the Threa TTRPG system and serve as the specification for the character sheet assistant application.

