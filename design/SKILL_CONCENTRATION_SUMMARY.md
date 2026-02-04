# Skills with Pre- and Post-Use Concentration Effects - Implementation Summary

## Overview

This implementation adds support for pre- and post-use concentration requirements to the skill system. Skills can now require characters to concentrate before using them (pre-use) or maintain concentration after using them (post-use), with penalties for interrupting post-use concentration.

## What Has Been Implemented

### 1. Data Model (✅ Complete)

**Files Modified:**
- `Threa.Dal/Dto/Skill.cs` - Added 5 new properties
- `GameMechanics/Skills/SkillDefinitionEdit.cs` - Added 5 new CSLA properties
- `GameMechanics/Skills/SkillInfo.cs` - Added 5 new read-only properties

**New Properties:**
```csharp
bool RequiresPreUseConcentration
int PreUseConcentrationRounds
bool RequiresPostUseConcentration
int PostUseConcentrationRounds
int PostUseInterruptionPenaltyRounds
```

**Storage:**
- SQLite DAL uses JSON serialization, so no schema changes required
- Properties automatically persisted through existing save/load mechanisms

### 2. Business Logic (✅ Complete)

**New Files:**
- `GameMechanics/Effects/Behaviors/SkillUsePayload.cs` - Payload for skill concentration data
- `GameMechanics/Effects/Behaviors/ConcentrationInterruptionPenaltyState.cs` - State for penalty effects

**Modified Files:**
- `GameMechanics/Effects/Behaviors/ConcentrationBehavior.cs` - Extended with skill support

**New Concentration Types:**
- `"PreUseSkill"` - Casting-time type for pre-use concentration
- `"PostUseSkill"` - Sustained type for post-use concentration

**New Methods:**
```csharp
ConcentrationBehavior.CreatePreUseSkillState(skillId, skillName, rounds, additionalData)
ConcentrationBehavior.CreatePostUseSkillState(skillId, skillName, rounds, penaltyRounds, additionalData)
```

**Behavior:**
- Pre-use: Character must concentrate before skill executes. Interruption prevents skill use.
- Post-use: Character must maintain concentration after skill executes. Interruption applies -1 AS penalty.
- Penalty duration is configurable by GM

### 3. GM Interface (✅ Complete)

**Files Modified:**
- `Threa/Threa.Client/Components/Pages/GameMaster/SkillEdit.razor`

**New UI Section:**
- "Concentration Requirements" section with checkboxes and numeric inputs
- Displays info tooltips explaining the behavior
- Validates input (rounds must be > 0 when enabled)

**GM Can Configure:**
1. Whether skill requires pre-use concentration
2. Number of rounds for pre-use concentration
3. Whether skill requires post-use concentration
4. Number of rounds for post-use concentration
5. Duration of penalty if post-use is interrupted

### 4. Documentation (✅ Complete)

**New File:**
- `design/SKILL_CONCENTRATION_INTEGRATION_PLAN.md` - Detailed integration guide

**Contents:**
- Complete implementation steps for player UI
- Code examples for checking and applying concentration
- Testing scenarios
- UI indicator requirements

## How It Works

### Pre-Use Concentration

1. **GM Configuration**: GM sets `RequiresPreUseConcentration = true` and `PreUseConcentrationRounds = 2`
2. **Player Action**: Player attempts to use the skill
3. **System Response**:
   - Creates concentration effect using `ConcentrationBehavior.CreatePreUseSkillState()`
   - Applies effect to character
   - Skill does NOT execute yet
   - Character must wait 2 rounds
4. **Concentration Completion**:
   - After 2 rounds, `OnExpire()` is called
   - Sets `Character.LastConcentrationResult` with `ActionType = "SkillUse"`
   - UI can now allow skill execution
5. **If Interrupted**:
   - `OnRemove()` is called
   - Sets `Character.LastConcentrationResult` with `Success = false`
   - Skill never executes

### Post-Use Concentration

1. **GM Configuration**: GM sets `RequiresPostUseConcentration = true`, `PostUseConcentrationRounds = 3`, `PostUseInterruptionPenaltyRounds = 5`
2. **Player Action**: Player uses skill successfully
3. **System Response**:
   - Skill executes normally
   - Creates concentration effect using `ConcentrationBehavior.CreatePostUseSkillState()`
   - Applies effect to character
   - Character must maintain concentration for 3 rounds
4. **Concentration Completion**:
   - After 3 rounds, `OnExpire()` is called
   - No penalty applied
   - Character free to act normally
5. **If Interrupted**:
   - `OnRemove()` is called
   - Sets `Character.LastConcentrationResult` with `ActionType = "PostUseSkillInterrupted"`
   - Payload contains `InterruptionPenaltyRounds = 5`
   - Service layer should apply -1 AS penalty effect for 5 rounds

## What Still Needs Implementation

### Player UI Integration

The player-facing skill usage UI (`TabPlaySkills.razor`) needs to be updated to:

1. **Check Concentration Requirements** (before skill use):
   ```csharp
   var skillInfo = allSkills?.FirstOrDefault(s => s.Name == selectedSkill.Name);
   if (skillInfo?.RequiresPreUseConcentration == true)
   {
       // Apply pre-use concentration instead of executing skill
       ApplyPreUseConcentration(skillInfo);
       return;
   }
   ```

2. **Apply Post-Use Concentration** (after skill use):
   ```csharp
   if (skillInfo?.RequiresPostUseConcentration == true)
   {
       // Apply post-use concentration effect
       ApplyPostUseConcentration(skillInfo);
   }
   ```

3. **Handle Concentration Events**:
   - Monitor `Character.LastConcentrationResult`
   - Show notifications when concentration completes
   - Apply penalties when post-use is interrupted

4. **Visual Indicators**:
   - Show icons for skills with concentration requirements
   - Display active concentration progress
   - Show interruption warnings

See `design/SKILL_CONCENTRATION_INTEGRATION_PLAN.md` for detailed implementation steps.

### Testing

Unit tests should be added to verify:
- Pre-use concentration prevents skill execution
- Pre-use interruption prevents skill execution
- Post-use concentration allows skill execution
- Post-use interruption triggers penalty
- Concentration state serialization/deserialization
- Helper methods create correct states

## Usage Example

### GM Setup
1. Navigate to GM Skills page
2. Edit a skill (e.g., "Powerful Strike")
3. Enable "Requires Pre-Use Concentration"
4. Set "Pre-Use Concentration Rounds" to 1
5. Enable "Requires Post-Use Concentration"
6. Set "Post-Use Concentration Rounds" to 2
7. Set "Post-Use Interruption Penalty Rounds" to 3
8. Save skill

### Player Experience (After UI Integration)
1. Player selects "Powerful Strike" skill
2. System applies 1-round pre-use concentration
3. Player waits 1 round (cannot take other actions)
4. Concentration completes - skill is now ready
5. Player executes "Powerful Strike" successfully
6. System applies 2-round post-use concentration
7. Player maintains concentration for 2 rounds
8. Concentration completes - no penalty
9. Player free to act normally

### If Interrupted
- **Pre-Use**: Skill never executes, wasted 1 AP + 1 FAT
- **Post-Use**: -1 AS penalty for 3 rounds

## Benefits

1. **Flexible Skill Design**: GMs can create skills with strategic tradeoffs
2. **Tactical Depth**: Players must decide when to use concentration-heavy skills
3. **Risk/Reward**: Powerful skills can require concentration investment
4. **No Code Changes Required**: All configuration is data-driven
5. **Reuses Existing System**: Leverages proven concentration mechanics

## Files Changed

### Data Model
- `Threa.Dal/Dto/Skill.cs`
- `GameMechanics/Skills/SkillDefinitionEdit.cs`
- `GameMechanics/Skills/SkillInfo.cs`

### Business Logic
- `GameMechanics/Effects/Behaviors/ConcentrationBehavior.cs` (modified)
- `GameMechanics/Effects/Behaviors/SkillUsePayload.cs` (new)
- `GameMechanics/Effects/Behaviors/ConcentrationInterruptionPenaltyState.cs` (new)

### UI
- `Threa/Threa.Client/Components/Pages/GameMaster/SkillEdit.razor`

### Documentation
- `design/SKILL_CONCENTRATION_INTEGRATION_PLAN.md` (new)
- `design/SKILL_CONCENTRATION_SUMMARY.md` (this file)

## Next Steps

1. Implement player UI integration following `SKILL_CONCENTRATION_INTEGRATION_PLAN.md`
2. Add unit tests for skill concentration behavior
3. Manual testing of complete workflow
4. Update game design documentation with concentration skill examples
5. Consider adding UI indicators and better feedback for players

## Notes

- The concentration system was already well-designed and extensible
- Minimal changes were needed to support skill concentration
- The GM interface is complete and functional
- The main remaining work is player UI integration
- All infrastructure is in place for easy integration
