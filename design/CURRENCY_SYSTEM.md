# Currency System

## Overview

The Threa game economy uses a four-tier currency system. All currency is tracked at the character level and item costs are stored as copper pieces.

## Currency Denominations

| Currency | Symbol | Exchange Rate |
|----------|--------|---------------|
| Copper   | cp     | Base unit (1 cp) |
| Silver   | sp     | 20 cp = 1 sp |
| Gold     | gp     | 20 sp = 400 cp = 1 gp |
| Platinum | pp     | 20 gp = 8,000 cp = 1 pp |

## Currency Properties

### Coin Weight

- 100 coins of any type = 1 pound
- Weight affects character carrying capacity
- Total coin weight = (Copper + Silver + Gold + Platinum) / 100

### Calculated Values

```
Total Copper Value = CopperCoins + (SilverCoins × 20) + (GoldCoins × 400) + (PlatinumCoins × 8000)
```

## Currency Economy Rules

### Currency Rarity

- **Copper**: Most common, everyday transactions
- **Silver**: Quality goods, professional services
- **Gold**: Rare items, enchanted equipment
- **Platinum**: Exceedingly rare (1-3 coins from high-value treasures)

### Pricing Guidelines

| Category | Price Range |
|----------|-------------|
| Food and Drink | 1-50 cp |
| Basic Supplies | 5-100 cp |
| Common Weapons/Armor | 50 cp - 10 sp |
| Quality Weapons/Armor | 10 sp - 5 gp |
| Masterwork Equipment | 5-20 gp |
| Enchanted Items (minor) | 20-100 gp |
| Enchanted Items (major) | 100+ gp |

### Merchant Types

- **Common Merchants**: Deal primarily in copper and silver
- **Specialty Merchants**: Deal in silver and gold
- **Wealthy Merchants**: Deal primarily in gold
- **Black Market Traders**: Accept any currency

## Coin Optimization

When converting copper to coins, optimize to minimize total coin count:

**Example**:
```
Input: 850 copper
Output: 2 gold, 2 silver, 10 copper
```

This is important for:
- Reducing weight (fewer total coins)
- Realistic transactions
- Player convenience

## Character Sheet Integration

The character sheet assistant should:
- Display individual coin counts (cp, sp, gp, pp)
- Calculate and display total wealth in copper
- Calculate and display total coin weight
- Support adding/removing currency
- Support automatic coin optimization

---

**Related Documents**:
- [Game Rules Specification](GAME_RULES_SPECIFICATION.md) - Full economy design
- [Database Design](DATABASE_DESIGN.md) - Schema details
