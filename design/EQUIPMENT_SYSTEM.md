# Equipment System

## Overview

The equipment system allows characters to manage their inventory, equip and unequip items, and benefit from item bonuses that affect skills and attributes.

## Equipment Slots

Characters can equip items in the following slots:

### Body Slots
- Head, Face, Ears, Neck
- Shoulders, Back, Chest
- ArmLeft, ArmRight
- WristLeft, WristRight
- HandLeft, HandRight
- Waist, Legs
- AnkleLeft, AnkleRight
- FootLeft, FootRight

### Weapon Slots
- MainHand
- OffHand
- TwoHand (uses both MainHand and OffHand)

### Jewelry Slots
- FingerLeft1-5 (5 ring slots on left hand)
- FingerRight1-5 (5 ring slots on right hand)

## Equipment Rules

### Two-Handed Weapons
- When equipping a two-handed weapon, both MainHand and OffHand are used
- Equipping to MainHand or OffHand automatically unequips two-handed weapons

### Auto-Unequip Logic
- When equipping an item to an occupied slot, the existing item is automatically unequipped
- Items cannot be equipped from inside containers (must be in direct inventory)

## Item Bonuses

### Skill Bonuses (ItemSkillBonus)

Direct bonuses to specific skills:

- **FlatBonus**: Added directly to the skill's effective level
- **PercentageBonus**: Multiplies the skill's effectiveness
- **CooldownReduction**: Reduces the skill's cooldown time

**Example**: Magic Sword with +2 Swords Bonus
- Base: STR 12 + Swords 5 - 5 = AS 12
- With item: STR 12 + (Swords 5 + 2) - 5 = AS **14**

### Attribute Modifiers (ItemAttributeModifier)

Bonuses to base attributes (these cascade to ALL skills using that attribute):

- **FlatBonus**: Added to the base attribute
- **PercentageBonus**: Multiplies the attribute

**Example**: Belt of Giants (+3 Physicality)
- Effective Physicality: 10 + 3 = **13**
- Swords AS: 13 + 5 - 5 = **13** (+3)
- Axes AS: 13 + 3 - 5 = **11** (+3)
- Athletics AS: 13 + 4 - 5 = **12** (+3)
- **Every STR-based skill improved by +3!**

## Stacking Multiple Bonuses

### Example: Equipped Items

Character has:
- Base Physicality: 10
- Swords skill level: 6

**Equipped items**:
1. **Enchanted Longsword** (+2 to Swords skill)
2. **Gauntlets of Strength** (+2 to Physicality)
3. **Belt of the Warrior** (+1 to Physicality)

**Calculation**:
```
Effective Physicality = 10 + 2 (gauntlets) + 1 (belt) = 13

Swords Ability Score:
  = Effective Physicality + (Swords Level + Sword Bonus) - 5
  = 13 + (6 + 2) - 5
  = 13 + 8 - 5
  = 16

Base (no items): 10 + 6 - 5 = 11
With items: 16 (Total improvement: +5)
```

## Strategic Implications

### Attribute Items are More Valuable

**One +3 Physicality item** affects:
- All weapon skills (Swords, Axes, Maces, Polearms, etc.)
- Physical utility skills (Athletics, Climb, Swim, etc.)
- Carrying capacity (exponential scaling!)

**One +3 Swords skill item** affects:
- Only the Swords skill

### Build Diversity

**Specialized Build** (Swordmaster):
- Sword with +5 Swords
- Armor with +3 Swords
- Ring with +2 Swords
- **Total**: +10 to Swords only

**Generalist Build** (Warrior):
- Belt with +3 Physicality
- Gauntlets with +2 Physicality
- **Total**: +5 to ALL Physicality-based skills

## Carrying Capacity Impact

Don't forget: Physicality bonuses also affect carrying capacity (exponential)!

```
Base PHY 10: 50 lbs capacity
With +3 from belt: PHY 13 = ~76 lbs capacity (+52% capacity!)
```

## Negative Modifiers (Cursed/Poor Quality)

Items can have negative bonuses:

- **Rusty Sword**: -2 to Swords skill
- **Cursed Gauntlets**: -3 to Physicality (affects ALL STR-based skills!)

## Conditional Bonuses

Both bonus types support optional conditions:

- **Day Blade**: +5 to Swords (only during daytime)
- **Vampire Amulet**: +3 to Physicality (only at night)

## Ability Score Calculation

When calculating effective ability scores:

1. Get base attribute
2. Apply attribute modifiers from equipped items
3. Get base skill level
4. Apply skill bonuses from equipped items
5. Calculate final AS: Effective Attribute + Effective Skill - 5

---

**Related Documents**:
- [Game Rules Specification](GAME_RULES_SPECIFICATION.md) - Combat and skill mechanics
- [Item Bonuses and Cascading Effects](ITEM_BONUSES_AND_CASCADING_EFFECTS.md) - Detailed bonus mechanics
- [Database Design](DATABASE_DESIGN.md) - Schema details
