# UI Changes for XP Tracking Feature

## Skills Tab - During Character Creation

### Header Section (Before & After)

**BEFORE:**
```
Character: John Smith    Species: Human    Total XP: 1000    Banked XP: 500
```

**AFTER:**
```
Character: John Smith    Species: Human    Total XP: 1000    Banked XP: 500    XP Spent on Skills: 500
```

### Skills Table (Before & After)

**BEFORE:**
| Skill Name | Primary Attribute | Level | Bonus | Ability Score | XP Cost to Next Level | Actions |
|------------|-------------------|-------|-------|---------------|----------------------|---------|
| Swordsmanship | STR | 3 | -2 | 8 | 20 | [Level Up (20 XP)] [Decrease] |
| Dodge | DEX | 2 | -3 | 7 | 10 | [Level Up (10 XP)] [Decrease] |

**AFTER (During Creation):**
| Skill Name | Primary Attribute | Level | Bonus | Ability Score | **XP Spent** | XP Cost to Next Level | Actions |
|------------|-------------------|-------|-------|---------------|--------------|----------------------|---------|
| Swordsmanship | STR | 3 | -2 | 8 | **15** | 20 | [Level Up (20 XP)] [Decrease] |
| Dodge | DEX | 2 | -3 | 7 | **8** | 10 | [Level Up (10 XP)] [Decrease] |

**AFTER (After Activation):**
| Skill Name | Primary Attribute | Level | Bonus | Ability Score | XP Cost to Next Level | Actions |
|------------|-------------------|-------|-------|---------------|----------------------|---------|
| Swordsmanship | STR | 3 | -2 | 8 | 20 | [Level Up (20 XP)] |
| Dodge | DEX | 2 | -3 | 7 | 10 | [Level Up (10 XP)] |

*Note: XP Spent column is hidden after activation*

### Info Alert Box

**During Character Creation:**
```
ℹ️ During character creation, you can spend 500 XP to advance your skills.
   You can also decrease skill levels to return XP to your bank if you change your mind.
   Once the character is activated, you can only advance skills with earned XP (no decreasing).
```

**After Character Activation:**
```
ℹ️ Your character is active. Spend XP from your bank to improve your skills.
   You have 500 XP available to spend.
   Total XP spent on skills: 500
```

## Magic Tab - Same Changes Applied

All three magic skill categories (Mana Skills, Spell Skills, Meta-Magic Skills) have the same XP tracking:

### Mana Skills Table Example

| Skill Name | Primary Attribute | Level | Bonus | Ability Score | Max Mana | **XP Spent** | XP Cost to Next Level | Actions |
|------------|-------------------|-------|-------|---------------|----------|--------------|----------------------|---------|
| Fire Mana | WIL | 2 | -3 | 7 | **2** | **8** | 10 | [Level Up (10 XP)] [Decrease] |

## Visual Styling

- **XP Spent values**: Displayed in **bold blue** (#0066cc) for emphasis
- **Column visibility**: Conditional - only shown when `!vm.Model.IsPlayable`
- **Total XP Spent**: Displayed in header and alert boxes
- **Real-time updates**: Values update immediately with `StateHasChanged()`

## User Workflows

### Leveling Up a Skill
1. User sees current XP Spent: 8
2. User clicks "Level Up (10 XP)"
3. XP Spent updates to: 18
4. Banked XP decreases by 10
5. Total XP Spent in header increases by 10

### Decreasing a Skill Level
1. User sees current XP Spent: 18
2. User clicks "Decrease"
3. XP Spent updates to: 8
4. Banked XP increases by 10
5. Total XP Spent in header decreases by 10

### Character Activation
1. During creation: XP Spent column visible, can increase/decrease
2. After activation: XP Spent column hidden, can only increase
3. Total XP Spent still visible in header and alert for reference
