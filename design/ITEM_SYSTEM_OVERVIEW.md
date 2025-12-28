# Item System Overview

## Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                         ITEM SYSTEM                              │
└─────────────────────────────────────────────────────────────────┘

┌─────────────────┐         ┌──────────────────┐
│  ItemTemplate   │◄───────┤  ItemSkillBonus   │
│  (Blueprint)    │         └──────────────────┘
│                 │         ┌──────────────────┐
│ - Name          │◄───────┤ ItemAttribute-   │
│ - Weight        │         │ Modifier         │
│ - Volume        │         └──────────────────┘
│ - IsContainer   │
└────────┬────────┘
         │ 1:N
         │
         ▼
┌─────────────────┐
│   Item          │         ┌──────────────────┐
│   (Instance)    │◄───────┤  Character       │
│                 │ N:1     │                  │
│ - StackSize     │         │ Inventory:       │
│ - Durability    │         │  MaxWeight       │
│ - IsEquipped    │         │  MaxVolume       │
│ - Location:     │◄──┐     └──────────────────┘
│   • Character   │   │
│   • Container   │   │
└─────────────────┘   │
         │            │
         │ Self-ref   │
         │ (nesting)  │
         └────────────┘
```

## Item Location States

```
┌──────────────┐
│  In Inventory│  OwnerCharacterId = abc
└──────┬───────┘
       │ equip
       ▼
┌──────────────┐
│   Equipped   │  IsEquipped = true, EquippedSlot = MainHand
└──────┬───────┘
       │ unequip
       ▼
┌──────────────┐
│  In Inventory│
└──────┬───────┘
       │ put in container
       ▼
┌──────────────┐
│ In Container │  ContainerItemId = xyz
└──────┬───────┘
       │ take from container
       ▼
┌──────────────┐
│  In Inventory│
└──────────────┘
```

## Container Hierarchy Example

```
Character (Physicality = 12, MaxWeight = ~66 lbs, MaxVolume = ~13 cu.ft.)
│
├─ Large Backpack (weight: 5 lbs, capacity: 50 lbs / 10 cu.ft.)
│  ├─ Small Pouch (weight: 0.5 lbs, capacity: 5 lbs / 1 cu.ft.)
│  │  ├─ Gold Coins (×100, weight: 2 lbs, volume: 0.1 cu.ft.)
│  │  └─ Gems (×3, weight: 0.3 lbs, volume: 0.05 cu.ft.)
│  ├─ Rope (50 ft, weight: 10 lbs, volume: 2 cu.ft.)
│  ├─ Torch (×5, weight: 5 lbs, volume: 1 cu.ft.)
│  └─ Rations (×10, weight: 10 lbs, volume: 2 cu.ft.)
│
├─ Belt Pouch (weight: 0.3 lbs, capacity: 3 lbs / 0.5 cu.ft.)
│  ├─ Lockpicks (weight: 0.2 lbs)
│  └─ Small Key (weight: 0.1 lbs)
│
├─ Quiver (weight: 1 lb, capacity: 5 lbs, restriction: "Arrow,Bolt")
│  └─ Arrows (×30, weight: 3 lbs, volume: 0.3 cu.ft.)
│
└─ Equipped Items (not in containers)
   ├─ Longsword (MainHand, weight: 4 lbs)
   ├─ Shield (OffHand, weight: 6 lbs)
   ├─ Leather Armor (Chest, weight: 10 lbs)
   └─ Boots (Feet, weight: 2 lbs)

Total Inventory Weight:
  Backpack: 5 + (0.5 + 2.3) + 10 + 5 + 10 = 32.8 lbs
  Belt Pouch: 0.3 + 0.3 = 0.6 lbs
  Quiver: 1 + 3 = 4 lbs
  Equipped: 4 + 6 + 10 + 2 = 22 lbs
  TOTAL: 59.4 lbs / ~66 lbs (90% capacity)
```

## Capacity Calculations

### Character Base Capacity

Uses exponential scaling to reflect 4dF bell curve distribution:

```
Physicality = 12

Max Weight = 50 × (1.15 ^ (12 - 10)) = 50 × 1.3225 = ~66.1 lbs
Max Volume = 10 × (1.15 ^ (12 - 10)) = 10 × 1.3225 = ~13.2 cu.ft.

Comparison:
  PHY  6: ~29 lbs, ~6 cu.ft.  (very weak, rare roll)
  PHY 10:  50 lbs, 10 cu.ft.  (average, most common)
  PHY 14: ~87 lbs, ~17 cu.ft. (very strong, rare roll)
```

### Magical Containers

Magical containers can reduce effective weight/volume:

```
Bag of Holding (ContainerWeightReduction = 0.1)
  Contains: 100 lbs of items
  Effective weight: bag weight + (100 × 0.1) = bag + 10 lbs
```

## Item Type Use Cases

| Type | Examples | Stackable? | Has Durability? | Can Equip? |
|------|----------|------------|-----------------|------------|
| Weapon | Sword, Bow, Dagger | ❌ | ✅ | ✅ |
| Armor | Helmet, Chest Plate | ❌ | ✅ | ✅ |
| Container | Backpack, Quiver | ❌ | ❌ | Maybe¹ |
| Consumable | Potion, Scroll | ✅ | ❌ | ❌ |
| Treasure | Gold, Gems | ✅ | ❌ | ❌ |
| Key | Door Key, Chest Key | ❌ | ❌ | ❌ |
| Magic | Wand, Ring | ❌ | ✅² | ✅ |
| Food/Drink | Bread, Water | ✅ | ❌ | ❌ |
| Tool | Lockpick, Torch | Maybe | ✅ | Maybe¹ |

¹ Some containers/tools may be wearable (backpacks on Back slot)
² Magic items may or may not have durability/charges

## Equipment Bonuses Flow

```
Character
  Physicality = 10
  Swords Skill Level = 5
  
  ↓ equips
  
Magic Longsword
  ItemSkillBonus:
    - Skill: Swords
    - BonusType: FlatBonus
    - BonusValue: 2
  
  ItemAttributeModifier:
    - Attribute: STR
    - ModifierType: FlatBonus
    - ModifierValue: 1
  
  ↓ result
  
Character (with bonuses)
  Effective Physicality = 10 + 1 = 11
  Effective Swords Skill = 5 + 2 = 7
  Ability Score = 11 + 7 - 5 = 13
```

---

**Related Documents**:
- [Equipment System](EQUIPMENT_SYSTEM.md) - Detailed equipment mechanics
- [Item Bonuses and Cascading Effects](ITEM_BONUSES_AND_CASCADING_EFFECTS.md) - Bonus calculations
- [Carrying Capacity Analysis](CARRYING_CAPACITY_ANALYSIS.md) - Weight/volume system
- [Database Design](DATABASE_DESIGN.md) - Schema details
