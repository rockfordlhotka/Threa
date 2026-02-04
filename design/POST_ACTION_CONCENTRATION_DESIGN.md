# Post-Action Concentration System Design

**Status: Implemented**

## Overview

Post-action concentration represents maintaining focus after using a skill. While the user concentrates, linked effects are maintained on targets. When concentration ends, those effects end. If concentration is interrupted, the user also receives a configurable debuff penalty.

## Use Cases

### Spell Example: Blindness
1. Caster uses Blindness spell on target
2. Target receives "Blinded" effect
3. Caster enters post-use concentration
4. While concentrating: Target remains blinded
5. When concentration ends (any reason): Target's Blind effect removed
6. If interrupted: Caster also receives debuff (e.g., -1 AS for 3 rounds)

### Combat Example: Grapple
1. Attacker grapples defender
2. Defender receives "Grappled" effect
3. Attacker enters post-use concentration
4. While concentrating: Defender remains grappled
5. When concentration ends (any reason): Grappled effect removed
6. If interrupted: Attacker receives debuff penalty

## Data Model Changes

### ConcentrationState (existing, enhanced)

```csharp
// NEW: Linked effects to remove when concentration ends
[JsonPropertyName("linkedEffects")]
public List<LinkedEffectInfo>? LinkedEffects { get; set; }

// NEW: Debuff configuration for interruption penalty
[JsonPropertyName("interruptionDebuff")]
public InterruptionDebuffConfig? InterruptionDebuff { get; set; }
```

### LinkedEffectInfo (new class)

```csharp
public class LinkedEffectInfo
{
    /// <summary>
    /// The effect ID on the target.
    /// </summary>
    public Guid EffectId { get; set; }

    /// <summary>
    /// The character ID who has this effect.
    /// </summary>
    public int TargetCharacterId { get; set; }

    /// <summary>
    /// Display name for logging (e.g., "Blinded on Goblin").
    /// </summary>
    public string? Description { get; set; }
}
```

### InterruptionDebuffConfig (new class)

```csharp
public class InterruptionDebuffConfig
{
    /// <summary>
    /// Name of the debuff effect (e.g., "Blindness Concentration Broken").
    /// </summary>
    public string Name { get; set; } = "Concentration Broken";

    /// <summary>
    /// Description for the debuff effect.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Duration in rounds.
    /// </summary>
    public int DurationRounds { get; set; }

    /// <summary>
    /// Global AS penalty (applies to all skills). Default: -1
    /// </summary>
    public int GlobalAsPenalty { get; set; } = -1;

    /// <summary>
    /// Optional: Specific skill penalties (skill ID -> penalty value).
    /// </summary>
    public Dictionary<string, int>? SkillPenalties { get; set; }

    /// <summary>
    /// Optional: Attribute penalties (attribute name -> penalty value).
    /// </summary>
    public Dictionary<string, int>? AttributePenalties { get; set; }
}
```

### SkillUsePayload (enhanced)

```csharp
// EXISTING
public string SkillId { get; set; }
public string SkillName { get; set; }
public int InterruptionPenaltyRounds { get; set; }
public string? AdditionalData { get; set; }

// NEW: Linked effects created by this skill use
public List<LinkedEffectInfo>? LinkedEffects { get; set; }

// NEW: Full debuff configuration (overrides simple InterruptionPenaltyRounds)
public InterruptionDebuffConfig? InterruptionDebuff { get; set; }
```

## Behavior Changes

### ConcentrationBehavior.OnExpire (normal completion)

When PostUseSkill concentration completes normally:
1. Prepare linked effect removal (store in LastConcentrationResult)
2. NO debuff applied
3. Message: "Maintained concentration on {skill} - effect ended"

### ConcentrationBehavior.OnRemove (interruption)

When PostUseSkill concentration is interrupted:
1. Prepare linked effect removal (store in LastConcentrationResult)
2. Include debuff configuration in result
3. Message: "Concentration interrupted! {debuff description}"

### TimeAdvancementService Changes

When processing concentration results:

```csharp
case "PostUseSkillEnded":  // Normal completion
    RemoveLinkedEffects(tableCharacters, result.Payload);
    return result.Message;

case "PostUseSkillInterrupted":  // Interruption
    RemoveLinkedEffects(tableCharacters, result.Payload);
    ApplyInterruptionDebuff(character, result.Payload);
    return result.Message;
```

New method for linked effect removal:
```csharp
private void RemoveLinkedEffects(IEnumerable<CharacterEdit> tableCharacters, string? payloadJson)
{
    var payload = PostUseConcentrationPayload.FromJson(payloadJson);
    if (payload?.LinkedEffects == null) return;

    foreach (var linkedEffect in payload.LinkedEffects)
    {
        var targetCharacter = tableCharacters
            .FirstOrDefault(c => c.Id == linkedEffect.TargetCharacterId);
        targetCharacter?.Effects.RemoveEffect(linkedEffect.EffectId);
    }
}
```

## Factory Method Changes

### CreatePostUseSkillState (enhanced)

```csharp
public static string CreatePostUseSkillState(
    string skillId,
    string skillName,
    int concentrationRounds,
    List<LinkedEffectInfo>? linkedEffects = null,
    InterruptionDebuffConfig? interruptionDebuff = null,
    string? additionalData = null)
{
    // If no explicit debuff config, use legacy behavior (simple -1 AS)
    interruptionDebuff ??= new InterruptionDebuffConfig
    {
        Name = $"{skillName} Concentration Broken",
        DurationRounds = 3,  // Default
        GlobalAsPenalty = -1
    };

    var payload = new SkillUsePayload
    {
        SkillId = skillId,
        SkillName = skillName,
        LinkedEffects = linkedEffects,
        InterruptionDebuff = interruptionDebuff,
        AdditionalData = additionalData
    };

    return new ConcentrationState
    {
        ConcentrationType = "PostUseSkill",
        TotalRequired = concentrationRounds,
        CurrentProgress = 0,
        RoundsPerTick = 1,
        LinkedEffects = linkedEffects,
        InterruptionDebuff = interruptionDebuff,
        DeferredActionPayload = payload.Serialize(),
        CompletionMessage = $"Concentration on {skillName} complete",
        InterruptionMessage = $"{skillName} concentration interrupted!"
    }.Serialize();
}
```

## Integration Flow

### Player UI Flow (TabPlaySkills.razor)

1. Player selects skill with post-use concentration
2. Player selects target(s) if applicable
3. Player performs skill check
4. If successful AND skill creates effect on target:
   a. Create effect on target character
   b. Capture the effect ID
   c. Create LinkedEffectInfo with effect ID and target character ID
5. Create post-use concentration with linked effects
6. Store concentration effect on character

### GM Interface

The GM can configure debuff penalties through:
1. **Skill definition** (database): Default penalty for the skill
2. **At runtime**: When manually breaking concentration, GM can specify/override debuff

## Database Schema Changes (Optional)

Add to `Skills` table:
```sql
-- Debuff configuration JSON (nullable, uses defaults if null)
InterruptionDebuffConfig NVARCHAR(MAX) NULL
```

This allows per-skill customization of interruption penalties in the database, while falling back to defaults if not specified.

## Migration Path

1. Existing `PostUseSkill` concentration continues to work (backwards compatible)
2. New `LinkedEffects` field is optional
3. New `InterruptionDebuff` field is optional, falls back to legacy `-1 AS` behavior
4. UI can be updated incrementally to support linked effects

## Test Scenarios

1. **Post-use with linked effect, normal completion**
   - Concentration completes → linked effect removed → no debuff

2. **Post-use with linked effect, interrupted**
   - Concentration interrupted → linked effect removed → debuff applied

3. **Post-use without linked effect (legacy)**
   - Concentration interrupted → debuff applied (backwards compatible)

4. **Post-use with custom debuff config**
   - Different penalties, durations, skill-specific effects

5. **Multiple linked effects**
   - Single concentration maintains multiple target effects
   - All removed when concentration ends

## Implementation Files

| File | Changes |
|------|---------|
| `GameMechanics/Effects/Behaviors/SkillUsePayload.cs` | Added `LinkedEffectInfo`, `InterruptionDebuffConfig` classes |
| `GameMechanics/Effects/Behaviors/ConcentrationBehavior.cs` | Updated `ConcentrationState`, `CreatePostUseSkillState`, `OnExpire`, `OnRemove` |
| `GameMechanics/Effects/Behaviors/DebuffBehavior.cs` | Added `SkillPenalties`, `AttributePenalties` support |
| `GameMechanics/Time/TimeAdvancementService.cs` | Added `RemoveLinkedEffects`, updated `ApplySkillInterruptionPenalty` |
| `Threa.Client/Components/Pages/GamePlay/Play.razor` | Added `PostUseSkillEnded` handling |
| `Threa.Client/Components/Pages/GamePlay/TabPlaySkills.razor` | Updated to use new API |
| `GameMechanics.Test/SkillConcentrationTests.cs` | Added linked effects tests |
