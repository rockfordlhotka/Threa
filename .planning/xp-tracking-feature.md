# XP Spend Tracking Feature

## Overview
This feature adds XP spend tracking during character creation, allowing players to see exactly where they've allocated their XP across different skills.

## User Story
During character creation, players want to track where XP was spent - on which skills and how much per skill. This helps them make informed decisions about skill allocation before activating their character.

## Implementation

### Backend Changes

#### 1. SkillEdit Business Object (`GameMechanics/SkillEdit.cs`)
- Added `XPSpent` property (int) to track cumulative XP spent on each skill
- Property is automatically saved/loaded through CSLA data portal operations
- Incremented when skill level increases, decremented when skill level decreases

#### 2. CharacterSkill DTO (`Threa.Dal/Dto/CharacterSkill.cs`)
- Added `XPSpent` field to persist the value in database

#### 3. Database Schema
- Added `XPSpent` column to `CharacterSkill` table (default: 0)
- Created migration script: `Sql/migrations/003_add_xpspent_to_character_skill.sql`
- Updated base table definition: `Sql/dbo.CharacterSkill.sql`

### UI Changes

#### 1. TabSkills.razor
- **Header Section**: Added "XP Spent on Skills" display showing total across all skills
- **Skills Table**: Added "XP Spent" column (visible only during character creation with `!vm.Model.IsPlayable`)
- **Info Alert**: Updated to show total XP spent during both creation and play
- **Method Updates**:
  - `IncreaseSkillLevel()`: Increments `skill.XPSpent` when leveling up
  - `DecreaseSkillLevel()`: Decrements `skill.XPSpent` when leveling down
  - `GetTotalXPSpentOnSkills()`: Calculates sum of XPSpent across all skills

#### 2. TabMagic.razor
- Applied identical changes for consistency across magic skills (mana, spell, meta-magic)
- All three skill tables show XP spent during character creation

## User Experience

### During Character Creation (IsPlayable = false)
1. **Visibility**: Players see an "XP Spent" column for each skill showing cumulative XP invested
2. **Header Summary**: Total XP spent across all skills is prominently displayed
3. **Real-time Updates**: XP spent updates immediately as skills are leveled up or down
4. **Decision Support**: Players can see exactly how much XP they've committed to each skill

### After Character Activation (IsPlayable = true)
1. **XP Spent Column**: Hidden from skill tables (no longer relevant for gameplay)
2. **Total XP Spent**: Still visible in header to track character progression
3. **Historical Record**: XP spent values are persisted and continue to accumulate as skills advance

## Technical Notes

### Backwards Compatibility
- Existing characters will have `XPSpent = 0` until skills are modified and saved
- No migration required for existing data - new field defaults to 0
- When skills are next modified, XPSpent will be correctly calculated going forward

### XP Calculation
- Uses `SkillCost.GetLevelUpCost(level, difficulty)` for each level increase
- Cumulative XP for a skill at level N = sum of costs from level 0 to N
- XP is tracked per-skill, not globally, for granular visibility

### Data Flow
1. User clicks "Level Up" → `IncreaseSkillLevel()` called
2. Method validates XP availability and level cap
3. Calls `vm.Model.SpendXP(xpCost)` to deduct from bank
4. Increments `skill.Level` and `skill.XPSpent`
5. UI refreshes via `StateHasChanged()`

## Testing
- All 995 unit tests pass
- Build succeeds with no warnings
- No breaking changes to existing functionality

## Future Enhancements
- Could add XP spending history/log
- Could show XP efficiency metrics (XP per skill level, etc.)
- Could add visual charts for XP allocation
