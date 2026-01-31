# Concentration System

## Overview

**Concentration** is a mechanic where a character must maintain exclusive focus on a specific effect or activity, preventing them from taking other actions. This primarily applies to spell casting (both during casting and maintaining active spells) but can also apply to other activities requiring sustained mental focus.

While concentrating, a character cannot use weapons, skills, or take any active actions without breaking concentration and ending the effect immediately.

**Key principle**: Concentration represents mental commitment. The character's attention is wholly devoted to the task at hand, and any distraction or competing action breaks that focus.

### Two Types of Concentration

Concentration serves two distinct purposes in the game system:

| Type | Duration | Completion | Use Cases |
|------|----------|------------|-----------|
| **Casting Time** | Fixed (rounds/minutes) | Executes deferred action | Spell casting, magazine reload, ritual preparation |
| **Sustained Effect** | Indefinite (until dropped) | Removes active effect | Active spells, mental control, telekinetic holds |

**Casting Time Concentration**:
- Has a predetermined duration (e.g., "2 rounds to cast Fireball")
- When duration completes normally, the deferred action executes (spell is cast, magazine is loaded)
- If interrupted before completion, the action fails and never happens
- Example: "Concentrating on Casting Fireball (1 of 2 rounds complete)"

**Sustained Effect Concentration**:
- No fixed duration; continues as long as character maintains it
- The effect is already active (spell already cast, ability already activated)
- Breaking concentration immediately ends the active effect
- Usually causes ongoing FAT/VIT drain, creating a natural time limit
- Example: "Concentrating on Invisibility spell (active on self)"

### Quick Implementation Guide

**For developers implementing concentration:**

1. **Casting Time Concentration**:
   - Store deferred action in `ConcentrationState.DeferredActionPayload`
   - Track progress in `OnTick()`
   - Execute action in `OnExpire()` (natural completion)
   - Do NOT execute in `OnRemove()` (interruption)

2. **Sustained Effect Concentration**:
   - Create concentration effect on caster + spell effect on target(s)
   - Link effects via `SourceEffectId` and `LinkedEffectIds`
   - Apply FAT/VIT drain in `OnTick()`
   - Remove all linked effects in `OnRemove()` (any break)

3. **Key Principle**: `OnExpire` vs `OnRemove`
   - `OnExpire`: Duration ended naturally → execute deferred action
   - `OnRemove`: Interrupted/dropped → clean up without executing

---

## Core Mechanics

### What Requires Concentration?

All concentration effects use `EffectType.Concentration`, but the behavior differs based on `ConcentrationType`:

**Casting Time Concentration** (fixed duration, executes deferred action):

| Activity | Duration | On Completion | On Interruption |
|----------|----------|---------------|-----------------|
| **Spell Casting** | 1-10 rounds | Spell is cast | Spell fails, wasted |
| **Magazine Reload** | 3-5 rounds | Magazine loaded | Partial progress lost |
| **Ritual Preparation** | 10+ rounds/minutes | Ritual executes | Ritual fails |
| **Complex Action** | Variable | Action executes | Action fails |

**Sustained Concentration** (indefinite, maintains active effect):

| Activity | Drain Cost | Active While... | On Break |
|----------|------------|-----------------|----------|
| **Invisibility** | 1 FAT/round | Caster concentrates | Target becomes visible |
| **Mental Domination** | 1 FAT + 1 VIT/round | Caster concentrates | Target freed |
| **Telekinetic Hold** | 2 FAT/round | Caster concentrates | Targets released |
| **Force Field** | 1 FAT/round | Caster concentrates | Barrier drops |
| **Levitation** | 1 FAT/round | Caster concentrates | Target falls |

### Concentration State

A character is in a **concentrating state** when they have one or more active effects with `RequiresConcentration = true`.

```
IsConcentrating = HasActiveEffect(RequiresConcentration: true)
```

### Concentration Limits

**Single Concentration Rule**: A character can only maintain concentration on **one effect at a time**.

- Attempting to concentrate on a new effect while already concentrating automatically breaks the previous concentration
- The character is prompted to confirm before breaking existing concentration
- Some powerful casters might have special abilities that allow multiple concentrations (future enhancement)

---

## Breaking Concentration

### Voluntary Breaking

Concentration breaks immediately when the character:

1. **Takes any active action** that requires skill use:
   - Using a weapon (melee or ranged attack)
   - Activating armor or equipment abilities
   - Using any skill (except passive defense)
   - Casting a different spell
   - Taking any action that costs AP

2. **Chooses to end concentration**:
   - Explicitly drops concentration via UI
   - No action cost to drop concentration voluntarily

### Involuntary Breaking

Concentration breaks when:

1. **Effect duration expires**:
   - Natural expiration ends concentration cleanly
   - No concentration check required

2. **Effect is dispelled or removed**:
   - External removal (dispel magic, counterspell)
   - No concentration check; effect simply ends

3. **Character is incapacitated**:
   - Unconscious, stunned, or paralyzed
   - Automatically breaks all concentration

4. **Failed concentration check** (see below)

### Concentration Checks

When a concentrating character is attacked and must defend, they risk breaking concentration.

**Active Defense**: Using active defense (Dodge, Parry) is an action and automatically breaks concentration before the defense roll.

**Passive Defense**: If the character chooses passive defense, they attempt to maintain concentration:

```
Concentration Check = Focus Skill AS + 4dF+ vs Attacker's AV
```

| Check Components | Value |
|------------------|-------|
| **AS** | WIL + Focus Skill Level - 5 + modifiers |
| **TV** | The attacker's Attack Value (AV) roll |
| **Success** | Concentration maintained |
| **Failure** | Concentration broken, effect ends |

**Important**: The concentration check happens regardless of whether the attack hits or misses. The character must maintain focus while under threat.

**Damage Modifier**: If the attack successfully hits and deals damage, apply an additional penalty to the concentration check:

```
Concentration Penalty = -1 per 2 points of damage dealt (round down)
```

Example:
- Attack deals 5 damage → -2 to concentration check
- Attack deals 8 damage → -4 to concentration check

---

## Casting Time Concentration

**Casting Time Concentration** represents the focus required to complete a complex action over multiple rounds. The action doesn't execute until the concentration duration completes successfully.

### How It Works

1. **Action Initiated**: Player starts an action (cast spell, reload magazine, prepare ritual)
2. **Concentration Effect Created**: System creates a Concentration effect with:
   - Fixed duration (e.g., 2 rounds for spell, 3 rounds for magazine reload)
   - Progress tracking (0 of 2 rounds complete)
   - Deferred action payload (what will happen on completion)
3. **Each Round**: Progress increments, but action hasn't executed yet
4. **Completion**: When duration expires naturally, the deferred action executes
5. **Interruption**: If concentration breaks early, action fails and never happens

### Deferred Action Storage

The concentration effect stores the pending action in its BehaviorState:

```json
{
  "type": "SpellCasting",
  "totalRequired": 2,
  "currentProgress": 1,
  "deferredActionType": "SpellCast",
  "deferredActionPayload": "{\"spellId\":42,\"targetId\":\"abc-123\"}",
  "completionMessage": "Fireball spell unleashed!"
}
```

### Completion vs Interruption

**On Natural Completion** (OnExpire):
- Execute the deferred action (cast the spell, reload the magazine)
- Apply spell effects to targets
- Update item states (magazine now loaded)
- Display completion message
- Remove concentration effect

**On Interruption** (OnRemove):
- Do NOT execute the deferred action
- The spell is wasted (if consumable)
- Partial progress is lost
- Display interruption message ("Concentration broken! Spell fizzled.")
- Remove concentration effect

### Deferred Action Types

| Action Type | Payload | On Completion |
|-------------|---------|---------------|
| **SpellCast** | Spell ID, target(s), parameters | Create spell effect(s) on target(s) |
| **MagazineReload** | Weapon ID, magazine ID, rounds | Update magazine CurrentRounds property |
| **RitualComplete** | Ritual ID, participants | Apply ritual effects to all participants |
| **ItemActivate** | Item ID, activation parameters | Trigger item's one-time effect |
| **SkillPrepare** | Skill ID, action parameters | Execute prepared skill action |

### Examples

**Spell Casting**:
```
Round 1: "Concentrating on Casting Fireball (0 of 2 rounds)"
  - Character cannot attack, use skills, or cast other spells
  - Can move, speak, use passive defense
Round 2: "Concentrating on Casting Fireball (1 of 2 rounds)"
  - Still concentrating, cannot take other actions
Round 3: Concentration expires naturally
  - Fireball spell is cast at target
  - Spell effect created on target
  - Concentration effect removed
```

**Magazine Reload**:
```
Round 1: "Reloading Magazine (0 of 3 rounds, 0/30 rounds loaded)"
  - Cannot fire weapon or take other combat actions
Round 2: "Reloading Magazine (1 of 3 rounds, 10/30 rounds loaded)"
  - Partial progress visible
Round 3: "Reloading Magazine (2 of 3 rounds, 20/30 rounds loaded)"
  - Almost complete
Round 4: Concentration expires
  - Magazine CurrentRounds set to 30
  - Magazine is usable
  - Concentration effect removed
```

**Interrupted Example**:
```
Round 1: "Concentrating on Casting Teleport (0 of 3 rounds)"
Round 2: Character is attacked, fails concentration check
  - Concentration broken
  - Teleport spell does NOT cast
  - Spell slot/component consumed (if applicable)
  - Concentration effect removed
```

### Implementation Notes

**ConcentrationState Structure**:
```csharp
public class ConcentrationState
{
    // Progress tracking
    public string ConcentrationType { get; set; }      // "SpellCasting", "MagazineReload"
    public int TotalRequired { get; set; }              // Total rounds needed
    public int CurrentProgress { get; set; }            // Rounds completed
    public int RoundsPerTick { get; set; }              // Progress per round (usually 1)

    // Deferred action
    public string? DeferredActionType { get; set; }     // "SpellCast", "Reload", etc.
    public string? DeferredActionPayload { get; set; }  // JSON-serialized action data
    public string? CompletionMessage { get; set; }      // Display on success
    public string? InterruptionMessage { get; set; }    // Display on failure

    // Item references (if applicable)
    public Guid? TargetItemId { get; set; }             // Magazine being reloaded
    public Guid? SourceItemId { get; set; }             // Weapon/spell focus used
}
```

**ConcentrationBehavior.OnExpire**:
```csharp
public void OnExpire(EffectRecord effect, CharacterEdit character)
{
    var state = ConcentrationState.FromJson(effect.BehaviorState);
    if (state?.DeferredActionType == null)
        return;

    // Execute the deferred action
    switch (state.DeferredActionType)
    {
        case "SpellCast":
            ExecuteSpellCast(character, state.DeferredActionPayload);
            break;
        case "MagazineReload":
            ExecuteMagazineReload(character, state.DeferredActionPayload);
            break;
        // ... other action types
    }

    // Publish completion event
    PublishConcentrationCompleted(character, state.CompletionMessage);
}
```

**ConcentrationBehavior.OnRemove**:
```csharp
public void OnRemove(EffectRecord effect, CharacterEdit character)
{
    var state = ConcentrationState.FromJson(effect.BehaviorState);
    if (state?.DeferredActionType != null)
    {
        // Action never executes - publish interruption event
        string message = state.InterruptionMessage
            ?? $"{state.ConcentrationType} interrupted!";
        PublishConcentrationBroken(character, message);
    }
}
```

---

## Sustained Effect Concentration

**Sustained Effect Concentration** maintains an already-active effect that requires ongoing focus. Breaking concentration immediately ends the active effect and removes it from all targets.

### How It Works

1. **Effect Activated**: Player casts a sustained spell or activates a sustained ability
2. **Two Effects Created**:
   - **Concentration Effect** (on caster): Tracks that caster is concentrating
   - **Active Effect** (on target(s)): The actual spell effect (invisibility, domination, etc.)
3. **Linked Effects**: Active effect has `SourceEffectId` pointing to concentration effect
4. **Ongoing Cost**: Concentration effect usually causes FAT/VIT drain per round
5. **Sustaining**: Caster maintains concentration indefinitely (or until drained)
6. **Breaking**: If concentration breaks for any reason, all linked active effects end immediately

### Effect Linking

The sustained spell effect and concentration effect are linked:

**Concentration Effect** (on caster):
```json
{
  "type": "SustainedSpell",
  "spellName": "Invisibility",
  "linkedEffectIds": ["effect-guid-1", "effect-guid-2"],
  "fatDrainPerRound": 1,
  "vitDrainPerRound": 0,
  "durationSeconds": null,  // Indefinite
  "completionMessage": null  // No deferred action
}
```

**Active Spell Effect** (on target):
```json
{
  "effectType": "SpellEffect",
  "name": "Invisibility",
  "sourceEffectId": "concentration-guid",  // Links back to concentration
  "sourceCasterId": "caster-character-id",
  "description": "Invisible while caster concentrates",
  "behaviorState": { ... invisibility modifiers ... }
}
```

### Breaking Sustained Concentration

**On Natural Drop** (OnRemove with voluntary flag):
- Caster voluntarily drops concentration
- Remove all linked active effects from targets
- Display: "[Caster] stops concentrating. Invisibility ends."

**On Interruption** (OnRemove):
- Concentration broken by attack, action, or incapacitation
- Remove all linked active effects from targets immediately
- Display: "[Caster] lost concentration! Invisibility ends."

**On Target Death**:
- If target dies but caster still concentrates, effect just ends on that target
- Concentration continues for other targets (if multi-target spell)

**On Caster Death**:
- All concentration effects break
- All linked active effects removed from all targets

### Ongoing Costs

Most sustained concentration causes gradual resource drain:

```
Base Drain per Round:
- Minor spells: 1 FAT per round
- Moderate spells: 2 FAT per round
- Major spells: 1 FAT + 1 VIT per round
- Epic spells: 2 FAT + 2 VIT per round
```

When FAT/VIT reaches 0, character becomes Exhausted/Incapacitated and concentration breaks.

### Examples

**Invisibility Spell**:
```
Cast Time: Instant (no casting concentration)
Sustain: Requires concentration
Effect: Target becomes invisible while caster concentrates
Cost: 1 FAT per round to sustain

Round 1: Caster casts Invisibility on Ally
  - Concentration effect created on Caster
  - Invisibility effect created on Ally (linked to concentration)
  - Caster is now concentrating, cannot take other actions
Round 2-10: Ally remains invisible, caster loses 1 FAT per round
Round 11: Caster attacked, fails concentration check
  - Concentration breaks
  - Invisibility effect removed from Ally immediately
  - Ally becomes visible
```

**Telekinetic Hold** (multi-target):
```
Cast Time: 1 round (casting concentration)
Sustain: Requires concentration
Effect: Up to 3 targets are paralyzed while caster concentrates
Cost: 2 FAT per round

Round 1: Caster concentrates on casting (casting concentration)
Round 2: Spell completes
  - Casting concentration removed
  - Sustained concentration created on Caster
  - Paralyzed effects created on 3 enemies (linked to concentration)
  - Caster must now maintain concentration
Round 3-8: Enemies remain paralyzed, caster loses 2 FAT per round
Round 9: Caster voluntarily drops concentration
  - Concentration removed
  - All 3 Paralyzed effects removed immediately
  - Enemies can act again
```

**Mental Domination**:
```
Cast Time: 2 rounds (casting concentration)
Sustain: Requires concentration
Effect: Target is dominated while caster concentrates
Cost: 1 FAT + 1 VIT per round

Round 1-2: Casting concentration (preparing spell)
Round 3: Spell completes
  - Sustained concentration created on Caster
  - Dominated effect created on Target (linked)
Round 4-12: Target is dominated, caster drains FAT/VIT
Round 13: Caster's VIT reaches 0
  - Caster becomes Incapacitated (automatic)
  - Concentration breaks automatically
  - Dominated effect removed from Target
  - Target regains control
```

### Implementation Notes

**Creating Sustained Concentration**:
```csharp
// Step 1: Create concentration effect on caster
var concentrationState = new ConcentrationState
{
    ConcentrationType = "SustainedSpell",
    SpellName = "Invisibility",
    LinkedEffectIds = new List<Guid>(),
    FatDrainPerRound = 1,
    VitDrainPerRound = 0,
    DurationSeconds = null  // Indefinite
};

var concentration = effectPortal.CreateChild(
    EffectType.Concentration,
    "Sustaining Invisibility",
    null,
    null,  // No fixed duration
    concentrationState.Serialize());

caster.Effects.AddEffect(concentration);

// Step 2: Create active spell effect on target(s)
var spellEffect = effectPortal.CreateChild(
    EffectType.SpellEffect,
    "Invisibility",
    null,
    300,  // 5 minutes max (safety limit)
    invisibilityState.Serialize());

spellEffect.SourceEffectId = concentration.Id;  // Link to concentration
spellEffect.SourceCasterId = caster.Id;

target.Effects.AddEffect(spellEffect);

// Step 3: Update concentration with linked effect ID
concentrationState.LinkedEffectIds.Add(spellEffect.Id);
concentration.BehaviorState = concentrationState.Serialize();
```

**ConcentrationBehavior.OnTick** (for sustained effects):
```csharp
public EffectTickResult OnTick(EffectRecord effect, CharacterEdit character)
{
    var state = ConcentrationState.FromJson(effect.BehaviorState);

    // Apply ongoing drain for sustained effects
    if (state.ConcentrationType == "SustainedSpell")
    {
        if (state.FatDrainPerRound > 0)
            character.ApplyFatigueDamage(state.FatDrainPerRound);

        if (state.VitDrainPerRound > 0)
            character.ApplyVitalityDamage(state.VitDrainPerRound);

        // Check if caster is still able to concentrate
        if (character.CurrentFatigue <= 0 || character.CurrentVitality <= 0)
        {
            return EffectTickResult.ExpireEarly(
                $"Too exhausted to maintain {state.SpellName}");
        }
    }

    return EffectTickResult.Continue();
}
```

**ConcentrationBehavior.OnRemove** (for sustained effects):
```csharp
public void OnRemove(EffectRecord effect, CharacterEdit character)
{
    var state = ConcentrationState.FromJson(effect.BehaviorState);

    // If sustained spell, remove all linked effects from targets
    if (state?.LinkedEffectIds != null)
    {
        foreach (var linkedEffectId in state.LinkedEffectIds)
        {
            // Find target character/NPC with this effect
            var target = FindCharacterWithEffect(linkedEffectId);
            if (target != null)
            {
                target.Effects.RemoveEffect(linkedEffectId);
            }
        }

        PublishConcentrationBroken(
            character,
            $"{state.SpellName} concentration ended");
    }
}
```

### Sustained Concentration Properties

Add to `ConcentrationState`:

```csharp
public class ConcentrationState
{
    // ... existing fields ...

    // For sustained effects
    public string? SpellName { get; set; }              // Name of sustained spell
    public List<Guid>? LinkedEffectIds { get; set; }    // Effects on target(s)
    public int FatDrainPerRound { get; set; }           // FAT cost per round
    public int VitDrainPerRound { get; set; }           // VIT cost per round
    public Guid? SourceCasterId { get; set; }           // Who is concentrating
}
```

Add to `EffectRecord`:

```csharp
public Guid? SourceEffectId { get; set; }               // Links back to concentration effect
public Guid? SourceCasterId { get; set; }               // Character ID of caster
```

---

## Integration with Actions

### Before Action Processing

Before any action is processed, check if the character is concentrating:

```
1. User attempts action
2. Check IsConcentrating()
3. If concentrating:
   a. Display warning modal
   b. Show current concentration effect details
   c. Prompt: "Taking this action will break concentration. Continue?"
   d. If user confirms, break concentration and proceed
   e. If user cancels, abort action
4. If not concentrating, proceed normally
```

### Allowed Actions While Concentrating

These actions do NOT break concentration:

| Action Type | Examples | Notes |
|-------------|----------|-------|
| **Movement** | Walking, sprinting | Free movement or using AP |
| **Communication** | Speaking, gestures | Brief communication only |
| **Passive Defense** | Taking hits without dodging | Requires concentration check |
| **Drop Item** | Releasing an object | Free action |
| **Perception** | Passive awareness | No active searching |
| **Drive (VIT to FAT)** | Converting Vitality to Fatigue | Character taxes themselves to maintain focus |

**Note on Drive Skill**: A character may use the Drive skill to convert VIT to FAT while concentrating. This is an intentional exception to the "no skill use" rule, as it represents the character pushing through exhaustion to maintain their focus. Sometimes a caster will truly tax themselves to keep a spell active.

### Blocked Actions While Concentrating

These actions WILL break concentration:

| Action Type | Examples | Why Blocked |
|-------------|----------|-------------|
| **Weapon Use** | Attack with sword, shoot bow | Requires active coordination |
| **Skill Use** | Lockpicking, climbing, healing | Demands mental focus |
| **Equipment Activation** | Activate shield, use item ability | Diverts attention |
| **Spell Casting** | Cast another spell | Mental interference |
| **Active Defense** | Dodge, parry, block | Physical action |

---

## Integration with Defense Resolution

When a concentrating character is attacked, the defense flow changes:

### Standard Defense Flow (Modified)

```
1. Attack declared against concentrating character
2. Prompt defense choice:
   - "Use passive defense (concentration check required)"
   - "Use active defense (breaks concentration)"
   - Show current concentration effect name
3. If passive defense chosen:
   a. Resolve attack normally (passive TV = AS - 1)
   b. If attack succeeds, calculate damage
   c. Roll concentration check (Focus AS + 4dF+ vs Attacker's AV)
   d. Apply damage penalty if hit: -1 per 2 damage dealt
   e. If concentration check fails, break concentration
   f. Notify player and GM of result
4. If active defense chosen:
   a. Break concentration immediately
   b. Notify player and GM
   c. Resolve active defense normally
```

### DefenseResolver Integration

The `DefenseResolver` should:

1. Check if defender is concentrating
2. If concentrating and using passive defense, store AV for concentration check
3. After damage resolution, trigger concentration check
4. Break concentration if check fails

---

## Data Model

### ConcentrationState (BehaviorState JSON)

Complete state structure for concentration effects:

```csharp
public class ConcentrationState
{
    // Common properties
    [JsonPropertyName("type")]
    public string ConcentrationType { get; set; } = "";  // "SpellCasting", "SustainedSpell", "MagazineReload"

    // Progress tracking (for casting-time concentration)
    [JsonPropertyName("totalRequired")]
    public int TotalRequired { get; set; }              // Total rounds needed

    [JsonPropertyName("currentProgress")]
    public int CurrentProgress { get; set; }            // Rounds completed

    [JsonPropertyName("roundsPerTick")]
    public int RoundsPerTick { get; set; } = 1;         // Progress per round

    // Deferred action (for casting-time concentration)
    [JsonPropertyName("deferredActionType")]
    public string? DeferredActionType { get; set; }     // "SpellCast", "Reload", "SkillUse"

    [JsonPropertyName("deferredActionPayload")]
    public string? DeferredActionPayload { get; set; }  // JSON-serialized action parameters

    [JsonPropertyName("completionMessage")]
    public string? CompletionMessage { get; set; }      // Display on completion

    [JsonPropertyName("interruptionMessage")]
    public string? InterruptionMessage { get; set; }    // Display on interruption

    // Sustained effect properties (for sustained concentration)
    [JsonPropertyName("spellName")]
    public string? SpellName { get; set; }              // Name of sustained spell

    [JsonPropertyName("linkedEffectIds")]
    public List<Guid>? LinkedEffectIds { get; set; }    // Active effects on target(s)

    [JsonPropertyName("fatDrainPerRound")]
    public int FatDrainPerRound { get; set; }           // FAT cost per round

    [JsonPropertyName("vitDrainPerRound")]
    public int VitDrainPerRound { get; set; }           // VIT cost per round

    // Item references (for both types)
    [JsonPropertyName("targetItemId")]
    public Guid? TargetItemId { get; set; }             // Magazine being reloaded, etc.

    [JsonPropertyName("sourceItemId")]
    public Guid? SourceItemId { get; set; }             // Weapon/spell focus used

    [JsonPropertyName("sourceCasterId")]
    public Guid? SourceCasterId { get; set; }           // Character concentrating

    public string Serialize() => JsonSerializer.Serialize(this);
    public static ConcentrationState? FromJson(string? json) =>
        string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<ConcentrationState>(json);
}
```

### EffectRecord

Add properties for linking sustained effects:

```csharp
public static readonly PropertyInfo<Guid?> SourceEffectIdProperty = RegisterProperty<Guid?>(nameof(SourceEffectId));
/// <summary>
/// Links an active spell effect back to the concentration effect on the caster.
/// When concentration breaks, all effects with this SourceEffectId are removed.
/// </summary>
public Guid? SourceEffectId
{
    get => GetProperty(SourceEffectIdProperty);
    set => SetProperty(SourceEffectIdProperty, value);
}

public static readonly PropertyInfo<Guid?> SourceCasterIdProperty = RegisterProperty<Guid?>(nameof(SourceCasterId));
/// <summary>
/// The character ID of the caster who is concentrating on this effect.
/// Used to find the caster when displaying effect information.
/// </summary>
public Guid? SourceCasterId
{
    get => GetProperty(SourceCasterIdProperty);
    set => SetProperty(SourceCasterIdProperty, value);
}
```

### CharacterEdit

Add methods:

```csharp
// Check if character is concentrating
public bool IsConcentrating()
{
    return Effects.Any(e => e.IsActive && e.EffectType == EffectType.Concentration);
}

// Get the concentration effect
public EffectRecord? GetConcentrationEffect()
{
    return Effects.FirstOrDefault(e => e.IsActive && e.EffectType == EffectType.Concentration);
}

// Get concentration type (casting or sustaining)
public string? GetConcentrationType()
{
    var effect = GetConcentrationEffect();
    if (effect == null) return null;

    var state = ConcentrationState.FromJson(effect.BehaviorState);
    return state?.ConcentrationType;
}

// Break concentration (removes effect, which triggers OnRemove)
public void BreakConcentration(string reason = "Concentration broken")
{
    var concentrationEffect = GetConcentrationEffect();
    if (concentrationEffect != null)
    {
        Effects.RemoveEffect(concentrationEffect.Id);
        // OnRemove will handle cleanup of linked effects
        // Publish ConcentrationBroken event
    }
}

// Perform concentration check
public ConcentrationCheckResult CheckConcentration(int attackerAV, int damageDealt)
{
    var focusSkill = Skills.FirstOrDefault(s => s.Name == "Focus");
    if (focusSkill == null)
        return new ConcentrationCheckResult { Success = false, Reason = "No Focus skill" };

    int damagePenalty = -(damageDealt / 2);
    int as = GetAbilityScore(focusSkill.Id) + damagePenalty;
    int roll = DiceRoller.Roll4dFPlus();
    int result = as + roll;

    bool success = result >= attackerAV;

    if (!success)
        BreakConcentration($"Failed concentration check (rolled {result} vs {attackerAV})");

    return new ConcentrationCheckResult
    {
        Success = success,
        AS = as,
        Roll = roll,
        Result = result,
        TV = attackerAV,
        DamagePenalty = damagePenalty
    };
}
```

### Database Migration

```sql
-- Add SourceEffectId and SourceCasterId columns to CharacterEffects
ALTER TABLE CharacterEffects
ADD SourceEffectId TEXT NULL;

ALTER TABLE CharacterEffects
ADD SourceCasterId TEXT NULL;

-- Index for finding effects by source
CREATE INDEX IF NOT EXISTS IX_CharacterEffects_SourceEffectId
ON CharacterEffects(SourceEffectId);

CREATE INDEX IF NOT EXISTS IX_CharacterEffects_SourceCasterId
ON CharacterEffects(SourceCasterId);
```

---

## UI/UX Considerations

### Visual Indicators

**Character Status Display**:
- Show concentration icon/badge when character is concentrating
- Display the name of the effect being concentrated on
- Highlight in Status tab and character modal

**Action Buttons**:
- Dim or mark action buttons that would break concentration
- Show tooltip: "This action will break concentration on [Effect Name]"

### Confirmation Dialogs

**Before Breaking Concentration**:
```
┌─────────────────────────────────────┐
│ Break Concentration?                │
├─────────────────────────────────────┤
│ You are concentrating on:           │
│ "Invisibility" (3 rounds remaining) │
│                                     │
│ Taking this action will end the     │
│ effect immediately.                 │
│                                     │
│ [Cancel] [Break & Continue]         │
└─────────────────────────────────────┘
```

### Defense Choice Dialog

**When Under Attack While Concentrating**:
```
┌─────────────────────────────────────┐
│ Choose Defense                      │
├─────────────────────────────────────┤
│ You are concentrating on:           │
│ "Levitation"                        │
│                                     │
│ [Passive Defense]                   │
│   Requires Focus check to maintain  │
│   concentration                     │
│                                     │
│ [Active Defense (Dodge)]            │
│   Breaks concentration immediately  │
└─────────────────────────────────────┘
```

### Notifications

**Concentration Broken**:
- Toast notification to player: "Concentration broken! [Effect Name] has ended."
- Combat log entry: "[Character] lost concentration on [Effect]"
- GM notification: "[Character] can no longer maintain [Effect]"

**Concentration Check Success**:
- Combat log entry: "[Character] maintained concentration (Focus check: [result] vs [TV])"
- Brief visual feedback (checkmark, success color flash)

---

## Edge Cases and Special Scenarios

### Multiple Concentration Effects

**Problem**: What if a character somehow has multiple effects requiring concentration?

**Solution**:
- The system should prevent this during effect application
- If it occurs (data corruption, migration, etc.), `BreakConcentration()` should end ALL concentration effects
- Log a warning for debugging

### Concentration During Effect Application

**Problem**: Applying a new concentration effect while already concentrating.

**Solution**:
1. Check if character is already concentrating
2. If yes, prompt: "You are already concentrating on [Effect]. Replace with [New Effect]?"
3. If confirmed, end previous effect and apply new effect
4. If cancelled, abort new effect application

### Unconsciousness

**Problem**: What happens to concentration when character is knocked unconscious?

**Solution**:
- Apply "Unconscious" effect
- During application, check for concentration
- Automatically break concentration (no check)
- Log: "[Character] lost consciousness and can no longer concentrate"

### Death

**Problem**: What happens to concentration when character dies?

**Solution**:
- All effects end on death, including concentration effects
- No special handling needed beyond normal effect cleanup

### Spell Completion

**Problem**: Spell that requires concentration during casting (before effect is created).

**Solution** (future enhancement):
- Create a temporary "Casting [Spell Name]" effect with `RequiresConcentration = true`
- On successful cast, remove casting effect and apply spell effect
- On concentration break during casting, spell fails and is wasted

### Long-Duration Concentration

**Problem**: Concentration spells that last hours or days.

**Solution**:
- Effect continues as long as concentration is maintained
- Character cannot participate in combat without breaking concentration
- Consider "downtime concentration" flag for effects that don't need active attention

### Environmental Distractions

**Problem**: Should environmental effects (loud noises, sudden light) break concentration?

**Solution** (GM discretion):
- Not automatically implemented in the system
- GM can manually force a concentration check
- Use standard Focus check against appropriate TV

---

## Implementation Checklist

### Phase 1: Core Data Model
- [ ] Update `ConcentrationState` class with all properties (casting + sustained)
- [ ] Add `SourceEffectId` property to `EffectRecord`
- [ ] Add `SourceCasterId` property to `EffectRecord`
- [ ] Create database migration for new columns
- [ ] Update `CharacterEffect` DTO with new properties

### Phase 2: ConcentrationBehavior - Casting Time
- [ ] Implement deferred action storage in `OnAdding`
- [ ] Implement progress tracking in `OnTick`
- [ ] Implement action execution in `OnExpire`
- [ ] Implement interruption handling in `OnRemove`
- [ ] Create action payload classes (SpellCastPayload, ReloadPayload, etc.)
- [ ] Create action executor methods (ExecuteSpellCast, ExecuteMagazineReload, etc.)

### Phase 3: ConcentrationBehavior - Sustained Effects
- [ ] Implement linked effect tracking in `ConcentrationState`
- [ ] Implement FAT/VIT drain in `OnTick`
- [ ] Implement linked effect removal in `OnRemove`
- [ ] Add exhaustion check (auto-break when FAT/VIT reaches 0)
- [ ] Support multi-target spell linking

### Phase 4: Character Methods
- [ ] Implement `IsConcentrating()` in `CharacterEdit`
- [ ] Implement `GetConcentrationEffect()` in `CharacterEdit`
- [ ] Implement `GetConcentrationType()` in `CharacterEdit`
- [ ] Implement `BreakConcentration()` in `CharacterEdit`
- [ ] Implement `CheckConcentration()` in `CharacterEdit`
- [ ] Create `ConcentrationCheckResult` class

### Phase 5: Spell/Action System Integration
- [ ] Add "casting time" field to spell definitions
- [ ] Create concentration effect when starting spell with casting time
- [ ] Create sustained concentration for spells that require it
- [ ] Link spell effects to concentration effects
- [ ] Handle spell failure when casting interrupted

### Phase 6: Action Integration
- [ ] Add concentration check to action processing flow
- [ ] Create confirmation dialog for breaking concentration
- [ ] Block actions when concentrating (with override)
- [ ] Update `ActionResolver` to check concentration

### Phase 7: Defense Integration
- [ ] Modify defense choice dialog to show concentration warning
- [ ] Update `DefenseResolver` to trigger concentration checks
- [ ] Calculate damage penalty for concentration checks
- [ ] Apply concentration check after damage resolution

### Phase 8: Effect Application
- [ ] Check for existing concentration when applying new concentration effect
- [ ] Prompt to replace existing concentration
- [ ] Automatically break concentration when incapacitated
- [ ] Remove sustained effects when concentration breaks

### Phase 9: UI Components
- [ ] Add concentration indicator to Status tab (show type and progress)
- [ ] Add concentration indicator to character modal
- [ ] Update `EffectFormModal` to include concentration options
- [ ] Create concentration broken notification/toast
- [ ] Add concentration check results to combat log
- [ ] Show linked effects (e.g., "Sustaining Invisibility on Ally")
- [ ] Display FAT/VIT drain rate for sustained effects

### Phase 10: Messaging/Events
- [ ] Create `ConcentrationBrokenEvent`
- [ ] Create `ConcentrationCompletedEvent` (for casting time)
- [ ] Create `ConcentrationCheckEvent`
- [ ] Publish events to notify GM and player
- [ ] Subscribe to events in UI components
- [ ] Notify targets when sustained effect ends

### Phase 11: Testing - Casting Time
- [ ] Unit test: Create casting-time concentration with deferred action
- [ ] Unit test: Progress tracking per round
- [ ] Unit test: Successful completion executes action
- [ ] Unit test: Interruption does NOT execute action
- [ ] Unit test: Magazine reload completion updates item
- [ ] Integration test: Spell cast after concentration completes
- [ ] Integration test: Spell fails when concentration broken

### Phase 12: Testing - Sustained Effects
- [ ] Unit test: Create sustained concentration with linked effects
- [ ] Unit test: FAT/VIT drain per round
- [ ] Unit test: Breaking concentration removes linked effects
- [ ] Unit test: Auto-break when FAT reaches 0
- [ ] Unit test: Multi-target spell linking
- [ ] Integration test: Invisibility spell sustained and broken
- [ ] Integration test: Target dies but caster continues concentrating

### Phase 13: Testing - Concentration Checks
- [ ] Unit tests for concentration checks with various modifiers
- [ ] Unit tests for automatic concentration breaking
- [ ] Integration tests for action blocking
- [ ] Integration tests for defense-triggered concentration checks
- [ ] UI tests for confirmation dialogs

---

## Related Documents

- [EFFECTS_SYSTEM.md](EFFECTS_SYSTEM.md) - Effect properties, types, and management
- [ACTIONS.md](ACTIONS.md) - Action resolution flow and integration
- [COMBAT_SYSTEM.md](COMBAT_SYSTEM.md) - Defense options and damage resolution
- [GAME_RULES_SPECIFICATION.md](GAME_RULES_SPECIFICATION.md) - Core mechanics and skill checks
- [TIME_SYSTEM.md](TIME_SYSTEM.md) - Effect duration and timing

---

## Spell Casting Integration

When integrating concentration with the spell system, consider:

### Instant Spells
- No concentration during casting
- May require sustained concentration (e.g., Invisibility)
- Cast immediately, then create sustained concentration

### Casting Time Spells
- Require casting-time concentration (1-10 rounds typical)
- Deferred action payload stores spell parameters
- Spell effect created on completion
- May also require sustained concentration after casting

### Ritual Spells
- Long casting time (minutes to hours)
- May require multiple casters concentrating together
- Interruption of any caster breaks the ritual
- Complex deferred action with multiple effects

### Spell Definition Schema

Add to spell database/definitions:

```csharp
public class SpellDefinition
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int? CastingTimeRounds { get; set; }        // Null = instant
    public bool RequiresSustainedConcentration { get; set; }
    public int? SustainFatCostPerRound { get; set; }   // For sustained spells
    public int? SustainVitCostPerRound { get; set; }
    public int? MaxSustainRounds { get; set; }         // Safety limit
    // ... other spell properties
}
```

### Casting Flow

**Instant spell with sustained concentration**:
```
1. Player casts Invisibility (instant)
2. Spell effect immediately created on target
3. Sustained concentration effect created on caster
4. Caster must maintain concentration to keep spell active
5. Breaking concentration removes spell effect from target
```

**Casting time with no sustained concentration**:
```
1. Player begins casting Fireball (2 rounds)
2. Casting concentration effect created on caster
3. Round 1: Progress 0 of 2
4. Round 2: Progress 1 of 2
5. Round 3: Concentration completes, Fireball spell executes
6. Fireball damage applied to target
7. No sustained concentration needed
```

**Casting time + sustained concentration**:
```
1. Player begins casting Mental Domination (2 rounds)
2. Casting concentration effect created
3. Round 1-2: Casting in progress
4. Round 3: Casting completes
   - Casting concentration removed
   - Dominated effect created on target
   - Sustained concentration created on caster
5. Rounds 4+: Caster sustains, draining FAT/VIT per round
6. Breaking concentration removes Dominated effect from target
```

---

## Edge Cases and Special Scenarios

### Concentration During Effect Application

**Problem**: Applying a new concentration effect while already concentrating.

**Solution**:
1. Check if character is already concentrating
2. If yes, prompt: "You are already concentrating on [Effect]. Replace with [New Effect]?"
3. If confirmed, call `BreakConcentration()` which triggers `OnRemove` for current effect
4. Apply new concentration effect
5. If cancelled, abort new effect application

**Important**: Breaking casting-time concentration means the deferred action never executes. Breaking sustained concentration immediately removes all linked effects.

### Target Death During Sustained Concentration

**Problem**: Target of sustained spell dies while caster still concentrates.

**Solution** (single-target spell):
- Remove spell effect from dead target
- Auto-break caster's concentration (spell has no valid target)
- Display: "[Target] died. [Spell] concentration ends."

**Solution** (multi-target spell):
- Remove spell effect from dead target only
- Caster continues concentrating on remaining targets
- If all targets die, auto-break concentration

### Caster Death or Incapacitation

**Problem**: What happens to sustained effects when caster dies?

**Solution**:
- All concentration effects break immediately (on caster's death/incapacitation)
- All linked effects removed from all targets
- `OnRemove` is called for cleanup
- Display: "[Caster] lost consciousness. All sustained spells end."

### Concentration in Different Time Scales

**Problem**: Casting a 2-round spell when not in combat (no round tracking).

**Solution**:
- Convert rounds to real-time seconds (1 round = 3 seconds)
- Use epoch-based expiration for casting-time concentration
- When combat starts mid-casting, convert remaining time to rounds

**Problem**: Sustaining a spell for hours outside combat.

**Solution**:
- Sustained concentration continues indefinitely
- Apply FAT/VIT drain at appropriate intervals (per minute or per hour)
- When combat starts, switch to per-round drain
- Character cannot participate in combat while concentrating on sustained spell

### Dispel Magic on Sustained Spell

**Problem**: Someone casts Dispel on a sustained spell effect.

**Solution** (dispel targets the effect):
- Remove the spell effect from target (normal dispel)
- Caster's concentration remains active but has no linked effects
- Caster should be notified: "Your Invisibility spell was dispelled."
- Concentration auto-breaks if all linked effects are removed

**Solution** (dispel targets the concentration):
- Remove concentration effect from caster
- `OnRemove` removes all linked spell effects from all targets
- More powerful dispel variant

### Unconsciousness

**Problem**: What happens to concentration when character is knocked unconscious?

**Solution**:
- Apply "Unconscious" condition effect
- During application, check `IsConcentrating()`
- Auto-call `BreakConcentration()` (no check)
- This triggers `OnRemove` which handles cleanup
- Log: "[Character] lost consciousness and can no longer concentrate"

### Simultaneous Casting + Sustaining

**Problem**: Can I sustain a spell while casting a new one?

**Answer**: No, only one concentration at a time.
- Attempting to cast a new spell with casting time breaks sustained concentration
- Must choose: keep sustaining current spell OR start casting new spell
- Instant spells don't require concentration to cast (unless they require sustaining)

---

## Future Enhancements

### Multiple Concentration (Advanced)

Some powerful casters might gain abilities to maintain multiple concentrations:

- Add `MaxConcentrations` property to character
- Default is 1, some abilities/items might increase this
- Track concentration effects as a list rather than single effect
- Apply concentration check penalty: -2 per additional concentration
- More complex UI to show multiple concurrent concentrations

### Concentration Difficulty Tiers

Different effects might be easier or harder to maintain:

- Add `ConcentrationDifficulty` to effect templates (Easy, Moderate, Hard)
- Apply bonus/penalty to concentration checks
- Easy: +2 to checks, Moderate: +0, Hard: -2
- Higher difficulty spells might have higher FAT/VIT drain

### Partial Concentration

Allow characters to take limited actions while concentrating:

- Add `AllowedActionTypes` list to concentration effects
- Some spells might allow movement but not attacks
- Others might allow simple actions but not complex skills
- "Light concentration" vs "deep concentration"

### Concentration Practice

Skill progression for maintaining concentration:

- Track successful concentration checks
- Grant bonuses after multiple successes
- "Concentration Mastery" feat or meta-skill
- Reduces FAT/VIT drain for sustained spells over time

### Shared Concentration (Rituals)

For multi-caster rituals:

- Multiple casters concentrating on same effect
- If any caster breaks concentration, ritual fails
- Distribute casting time across multiple casters
- Combine caster stats for concentration checks
