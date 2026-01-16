# XP System Refinement

## Overview

This document describes refinements to the XP (Experience Points) system to improve usability and fix tracking issues.

**Changes**:
1. Convert XP costs from decimal to integer values (10x multiplier)
2. Fix XPTotal tracking to correctly accumulate spent XP
3. Migration strategy for existing characters

**Related Documents**:
- [Skill Progression](SKILL_PROGRESSION.md) - Original skill progression system

---

## Problem Statement

### Issue 1: Fractional XP Values

The current system uses fractional XP costs (0.1, 0.3, 0.5), which:
- Are confusing for players ("I have 2.7 XP")
- Require floating-point math with potential precision issues
- Make the UI harder to display cleanly

### Issue 2: XPTotal Not Maintained

The `XPTotal` property on CharacterEdit exists but is never updated when XP is spent on skills. It always shows 0, making it useless for tracking character advancement.

---

## Solution

### 1. Integer XP Costs (10x Multiplier)

Multiply all XP costs by 10 and store as integers. This makes the minimum cost 1 XP.

**New XP Cost Table**:

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

**Note**: Difficulty 1 is retained for future use, even if currently unused.

### 2. XP Award Scaling

Since costs are 10x higher, XP awards should also be 10x higher:

| Activity | Old Award | New Award |
|----------|-----------|-----------|
| Minor encounter/challenge | 1-2 XP | 10-20 XP |
| Standard encounter | 3-5 XP | 30-50 XP |
| Significant challenge | 6-10 XP | 60-100 XP |
| Major story milestone | 10-20 XP | 100-200 XP |
| Campaign arc completion | 20-50 XP | 200-500 XP |

**Session Guidelines**: A typical session awards 50-150 XP.

### 3. XPTotal Tracking

Fix `XPTotal` to correctly track cumulative XP spent across all skills.

**Behavior**:
- When XP is spent to advance a skill, add that amount to `XPTotal`
- `XPTotal` is read-only (calculated from skill advancement history)
- `XPTotal` never decreases (represents lifetime XP investment)

**Implementation Options**:

**Option A: Calculate on demand**
```csharp
// XPTotal = sum of XP spent on all skills to reach current levels
public int XPTotal => Skills.Sum(s => GetCumulativeXPCost(s.Id, s.Level));
```

**Option B: Track incrementally**
```csharp
// When skill advances, add cost to running total
public void SpendXP(string skillId, int cost)
{
    XPBanked -= cost;
    XPTotal += cost;
}
```

**Recommendation**: Option B (incremental tracking) is simpler and matches user expectations. The total reflects exactly what was spent, even if skill costs change in future updates.

### 4. Data Type Changes

| Property | Old Type | New Type |
|----------|----------|----------|
| `CharacterEdit.XPTotal` | `double` | `int` |
| `CharacterEdit.XPBanked` | `double` | `int` |
| `SkillEdit.XPBanked` | `double` | `int` |
| `SkillCost.GetLevelUpCost()` | `double` | `int` |
| `SkillCost.Costs` array | `double[,]` | `int[,]` |

---

## Migration Strategy

### Existing Character Conversion

For characters with existing XP values:

1. **XPBanked (Character)**: Multiply by 10, round up (ceiling)
   - Example: `2.7 → ceiling(27) = 27`
   - Example: `0.3 → ceiling(3) = 3`

2. **XPBanked (per-Skill)**: Multiply by 10, round up (ceiling)
   - Preserves partial progress toward next level
   - Rounding up is player-friendly

3. **XPTotal**: Calculate from current skill levels
   - Sum the cumulative cost to reach each skill's current level
   - This gives an accurate "XP spent" value

### Migration SQL Example

```sql
-- Migrate character XP
UPDATE Characters
SET XPBanked = CEILING(XPBanked * 10),
    XPTotal = (
        SELECT SUM(GetCumulativeXPCost(s.SkillId, s.Level))
        FROM CharacterSkills s
        WHERE s.CharacterId = Characters.Id
    );

-- Migrate skill XP
UPDATE CharacterSkills
SET XPBanked = CEILING(XPBanked * 10);
```

### Database Schema Changes

```sql
-- Change column types from REAL/FLOAT to INTEGER
ALTER TABLE Characters ALTER COLUMN XPBanked INTEGER;
ALTER TABLE Characters ALTER COLUMN XPTotal INTEGER;
ALTER TABLE CharacterSkills ALTER COLUMN XPBanked INTEGER;
```

---

## Code Changes Required

### 1. SkillCost.cs

```csharp
public static class SkillCost
{
    public static int GetBonus(int level)
    {
        if (level < -1 || level > 10)
            throw new ArgumentException(nameof(level));
        return level - 5;
    }

    public static int GetLevelUpCost(int startLevel, int difficulty)
    {
        if (startLevel < -1 || startLevel >= 10)
            throw new ArgumentException(nameof(startLevel));
        if (difficulty < 1 || difficulty > 14)
            throw new ArgumentException(nameof(difficulty));
        if (startLevel < 0)
            return 0;
        return Costs[startLevel, difficulty - 1];
    }

    /// <summary>
    /// Gets the cumulative XP cost to reach a given level from level 0.
    /// </summary>
    public static int GetCumulativeCost(int targetLevel, int difficulty)
    {
        int total = 0;
        for (int level = 0; level < targetLevel; level++)
        {
            total += GetLevelUpCost(level, difficulty);
        }
        return total;
    }

    private static readonly int[,] Costs =
    {
        { 1, 3, 5, 10, 10, 10, 20, 20, 30, 40, 40, 50, 60, 60 },
        { 3, 5, 10, 20, 30, 30, 40, 50, 50, 60, 60, 70, 80, 80 },
        { 5, 10, 20, 30, 40, 50, 60, 70, 70, 80, 90, 100, 100, 110 },
        { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140 },
        { 20, 30, 40, 50, 60, 80, 90, 100, 120, 130, 140, 160, 170, 180 },
        { 30, 50, 70, 100, 120, 150, 170, 200, 220, 240, 270, 290, 320, 340 },
        { 40, 70, 110, 140, 180, 210, 250, 280, 320, 350, 390, 430, 460, 500 },
        { 80, 170, 250, 330, 410, 500, 580, 660, 740, 830, 910, 990, 1070, 1160 },
        { 220, 440, 650, 870, 1090, 1310, 1520, 1740, 1960, 2180, 2400, 2610, 2830, 3050 },
        { 370, 740, 1110, 1480, 1850, 2220, 2590, 2960, 3330, 3700, 4070, 4440, 4810, 5180 }
    };
}
```

### 2. CharacterEdit.cs

Change property types and add XPTotal update logic:

```csharp
public static readonly PropertyInfo<int> XPTotalProperty = RegisterProperty<int>(nameof(XPTotal));
[Display(Name = "Total XP")]
public int XPTotal
{
    get => GetProperty(XPTotalProperty);
    private set => SetProperty(XPTotalProperty, value);
}

public static readonly PropertyInfo<int> XPBankedProperty = RegisterProperty<int>(nameof(XPBanked));
[Display(Name = "Banked XP")]
public int XPBanked
{
    get => GetProperty(XPBankedProperty);
    set => SetProperty(XPBankedProperty, value);
}

/// <summary>
/// Spends XP from the banked pool to advance a skill.
/// Updates XPTotal to track cumulative spending.
/// </summary>
public void SpendXP(int amount)
{
    if (amount > XPBanked)
        throw new InvalidOperationException("Insufficient XP");
    XPBanked -= amount;
    XPTotal += amount;
}
```

### 3. SkillEdit.cs

Change property type:

```csharp
public static readonly PropertyInfo<int> XPBankedProperty = RegisterProperty<int>(nameof(XPBanked));
public int XPBanked
{
    get => GetProperty(XPBankedProperty);
    set => SetProperty(XPBankedProperty, value);
}
```

### 4. DTO Changes

Update `Character` and `CharacterSkill` DTOs:

```csharp
// Character.cs
public int XPTotal { get; set; }
public int XPBanked { get; set; }

// CharacterSkill.cs
public int XPBanked { get; set; }
```

### 5. UI Updates

- Update any displays showing XP values (no more decimals)
- Update GM XP award interface (suggest values in 10s: 10, 20, 50, 100)
- Update skill cost displays

---

## Progression Examples (Updated)

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

---

## Implementation Checklist

### Phase 1: Core Changes
- [ ] Update `SkillCost.cs` with integer costs and 10x values
- [ ] Add `GetCumulativeCost()` method to SkillCost
- [ ] Update unit tests for SkillCost

### Phase 2: Data Model Changes
- [ ] Update `CharacterEdit.cs` XP properties to `int`
- [ ] Add `SpendXP()` method to CharacterEdit
- [ ] Update `SkillEdit.cs` XPBanked to `int`
- [ ] Update `Character` DTO
- [ ] Update `CharacterSkill` DTO

### Phase 3: Database Migration
- [ ] Create migration script for existing data
- [ ] Test migration on sample data
- [ ] Update database schema (if not using EF migrations)

### Phase 4: UI Updates
- [ ] Update character sheet XP display
- [ ] Update skill advancement UI
- [ ] Update GM XP award interface
- [ ] Remove decimal formatting from XP displays

### Phase 5: Documentation
- [ ] Update SKILL_PROGRESSION.md with new values
- [ ] Update any player-facing documentation

---

## Backward Compatibility Notes

- Existing save files will need migration
- API responses will change from `double` to `int` (may affect external tools)
- Session award guidelines change (GMs need to award 10x more XP)

---

## Related Documents

- [Skill Progression](SKILL_PROGRESSION.md) - Will be updated with new values
- [Database Design](DATABASE_DESIGN.md) - Schema changes needed
