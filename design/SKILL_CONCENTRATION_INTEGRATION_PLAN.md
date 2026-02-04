# Skill Concentration Integration Plan

## Overview
This document outlines how to integrate pre- and post-use concentration requirements into the skill usage flow.

## Data Model (✅ Complete)
- Added `RequiresPreUseConcentration`, `PreUseConcentrationRounds` to Skill DTO
- Added `RequiresPostUseConcentration`, `PostUseConcentrationRounds`, `PostUseInterruptionPenaltyRounds` to Skill DTO
- Created `SkillUsePayload` class for storing skill concentration data
- Created `ConcentrationInterruptionPenaltyState` for penalty tracking
- Extended `ConcentrationBehavior` with "PreUseSkill" and "PostUseSkill" types
- Added `CreatePreUseSkillState()` and `CreatePostUseSkillState()` helper methods

## GM Interface (✅ Complete)
- Added "Concentration Requirements" section to SkillEdit.razor
- GMs can configure pre-use and post-use concentration settings

## Player Interface Integration (🔄 Remaining Work)

### Checking for Concentration Requirements

When a skill is selected in `TabPlaySkills.razor`:

1. **Load Skill Definition**: Get the full skill definition including concentration properties
   - Current code only has access to `SkillEdit` (character skill level data)
   - Need to load full `SkillDefinitionEdit` or add concentration properties to `SkillInfo`

2. **Pre-Use Check**: Before executing skill in `PerformCheckAsync()`:
   ```csharp
   if (skillDefinition.RequiresPreUseConcentration)
   {
       // Apply pre-use concentration effect
       var state = ConcentrationBehavior.CreatePreUseSkillState(
           skillDefinition.Id,
           skillDefinition.Name,
           skillDefinition.PreUseConcentrationRounds
       );
       
       // Create concentration effect on character
       var concentrationEffect = CreateConcentrationEffect(state);
       Character.Effects.Add(concentrationEffect);
       
       // Don't execute skill yet - wait for concentration to complete
       await OnCharacterChanged.InvokeAsync();
       return;
   }
   ```

3. **Post-Use Application**: After successful skill execution:
   ```csharp
   if (skillDefinition.RequiresPostUseConcentration)
   {
       // Apply post-use concentration effect
       var state = ConcentrationBehavior.CreatePostUseSkillState(
           skillDefinition.Id,
           skillDefinition.Name,
           skillDefinition.PostUseConcentrationRounds,
           skillDefinition.PostUseInterruptionPenaltyRounds
       );
       
       // Create concentration effect on character
       var concentrationEffect = CreateConcentrationEffect(state);
       Character.Effects.Add(concentrationEffect);
   }
   ```

### Handling Concentration Completion

When pre-use concentration completes:

1. **Listen for LastConcentrationResult**: Check `Character.LastConcentrationResult`
2. **If ActionType == "SkillUse"**: Skill is ready to execute
3. **Show notification**: "Concentration complete - {SkillName} ready to use"
4. **Auto-select skill**: Optionally pre-select the skill for immediate use

### Handling Interruptions

When concentration is interrupted:

1. **Pre-Use Interruption**:
   - Skill does not execute
   - Show message: "Concentration interrupted - {SkillName} not usable"

2. **Post-Use Interruption**:
   - Check `Character.LastConcentrationResult.ActionType == "PostUseSkillInterrupted"`
   - Parse `SkillUsePayload` from payload
   - Apply penalty effect: -1 AS for `InterruptionPenaltyRounds`
   - Show message: "Post-use concentration interrupted - penalty applied"

### UI Indicators

Add visual indicators for concentration:

1. **Skill List**:
   - Show icon for skills requiring pre-use concentration
   - Show icon for skills requiring post-use concentration
   - Disable skills while concentrating (unless it's the skill being concentrated on)

2. **Active Concentration Display**:
   - Show concentration progress in effects list
   - Add button to interrupt concentration voluntarily
   - Show warning when attempting other actions

3. **Skill Details Panel**:
   - Display concentration requirements
   - Show estimated rounds before skill is usable
   - Show penalty for interrupting post-use concentration

## Implementation Steps

### Step 1: Extend SkillInfo to Include Concentration Properties
Add the concentration properties to `SkillInfo.cs` so they're available in the player UI:

```csharp
public bool RequiresPreUseConcentration { get; set; }
public int PreUseConcentrationRounds { get; set; }
public bool RequiresPostUseConcentration { get; set; }
public int PostUseConcentrationRounds { get; set; }
public int PostUseInterruptionPenaltyRounds { get; set; }
```

### Step 2: Update TabPlaySkills.razor
Modify the skill usage flow to handle concentration:

1. Add skill definition lookup
2. Check for pre-use concentration before executing
3. Apply pre-use concentration effect if required
4. Apply post-use concentration effect after execution
5. Handle concentration completion events

### Step 3: Add UI Indicators
Create visual indicators for concentration requirements and active concentration.

### Step 4: Handle Penalty Application
When post-use concentration is interrupted, create a debuff effect that applies -1 AS.

## Testing Scenarios

1. **Pre-Use Concentration**:
   - Configure skill with 2 rounds pre-use concentration
   - Attempt to use skill
   - Verify concentration effect is applied
   - Wait 2 rounds
   - Verify skill executes after concentration completes
   - Test interruption (skill should not execute)

2. **Post-Use Concentration**:
   - Configure skill with 3 rounds post-use concentration
   - Use skill successfully
   - Verify concentration effect is applied
   - Test completion (no penalty)
   - Test interruption (penalty should be applied)

3. **Combined Concentration**:
   - Configure skill with both pre- and post-use concentration
   - Test full flow from pre-use through execution to post-use

4. **Multiple Concentration Attempts**:
   - Verify only one concentration can be active at a time
   - Test switching between different skill concentrations

## Notes

- The concentration system is already implemented in `ConcentrationBehavior`
- The GM interface is already complete
- The main work is integrating into the player skill usage flow
- Consider using the existing `ConcentrationIndicator.razor` component for displaying active concentration
- The penalty effect should use the existing effect system (similar to wounds)
