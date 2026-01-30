# Phase 22: Concentration System - Context

**Phase Goal:** Implement both casting-time and sustained effect concentration mechanics

**Depends on:** Phase 21 (Stat Editing)

**Requirements:** CONC-01 through CONC-15

---

## Problem Statement

The game system requires two distinct types of concentration mechanics:

1. **Casting Time Concentration**: Characters must concentrate for a fixed duration (e.g., 2 rounds) to complete an action like casting a spell or reloading a magazine. If concentration completes normally, the deferred action executes (spell is cast, magazine is loaded). If interrupted, the action fails and never happens.

2. **Sustained Effect Concentration**: Characters actively concentrate to maintain an already-active effect (e.g., Invisibility spell on an ally). The effect continues indefinitely as long as concentration is maintained, typically with FAT/VIT drain per round. Breaking concentration immediately removes all linked effects from all targets.

Both types share common rules:
- Only one concentration at a time per character
- Concentration breaks when taking other actions
- Concentration check required when defending passively under attack
- Automatic break on incapacitation

The critical distinction is in the `OnExpire()` vs `OnRemove()` lifecycle hooks:
- **OnExpire**: Duration ended naturally → execute deferred action (casting-time only)
- **OnRemove**: Interrupted/dropped → clean up without executing

---

## Current State

### What Exists

1. **EffectRecord and EffectList** (GameMechanics):
   - Full effect lifecycle support with OnTick, OnExpire, OnRemove hooks
   - Epoch-based expiration for O(1) performance
   - BehaviorState JSON storage for custom data

2. **ConcentrationBehavior** (GameMechanics/Effects/Behaviors):
   - Partial implementation for magazine reload (casting-time)
   - OnTick progress tracking
   - OnExpire/OnRemove hooks but incomplete execution logic
   - ConcentrationState schema (missing sustained effect fields)

3. **EffectType.Concentration** (Threa.Dal.Dto):
   - Enum value exists in EffectType

4. **Time Management** (Phase 16):
   - Round-based time tracking
   - ProcessEndOfRound triggers OnTick for all effects

### What's Missing

1. **Complete ConcentrationState Schema**:
   - Missing: LinkedEffectIds, FatDrainPerRound, VitDrainPerRound, SourceCasterId
   - Missing: DeferredActionType, DeferredActionPayload, CompletionMessage

2. **Deferred Action Execution**:
   - OnExpire doesn't execute stored actions (spell cast, magazine reload)
   - No action payload classes (SpellCastPayload, ReloadPayload)
   - No execution methods

3. **Sustained Effect Linking**:
   - EffectRecord missing SourceEffectId and SourceCasterId properties
   - No mechanism to find and remove linked effects when concentration breaks
   - No FAT/VIT drain implementation

4. **Character Methods**:
   - CharacterEdit missing IsConcentrating(), GetConcentrationType(), BreakConcentration()
   - No CheckConcentration() method for defense integration

5. **Action Integration**:
   - No check before actions to warn about breaking concentration
   - No confirmation dialog

6. **Defense Integration**:
   - DefenseResolver doesn't trigger concentration checks on passive defense
   - No damage penalty calculation

7. **UI Components**:
   - No concentration indicator in CharacterStatusCard
   - No display of concentration type/progress
   - No linked effects display for sustained concentration

---

## Technical Approach

### Architecture Decision: Event-Driven vs Direct Execution

**Question**: Should deferred action execution be:
- A) Direct method calls in ConcentrationBehavior.OnExpire() (tight coupling)
- B) Event publishing with handlers executing actions (loose coupling)

**Recommendation**: Start with **A (direct execution)** for simplicity, refactor to B if needed.

**Rationale**:
- Magazine reload is straightforward: update CharacterItem.CurrentRounds
- Spell casting will need spell system integration (future work)
- Event-driven adds complexity without immediate benefit
- Can refactor when spell system is mature

### Data Model Strategy

**EffectRecord Extensions**:
```csharp
// Add to EffectRecord.cs
public Guid? SourceEffectId { get; set; }      // Links spell effect back to concentration
public Guid? SourceCasterId { get; set; }      // Character ID of concentrating caster
```

**ConcentrationState Expansion**:
```csharp
// Extend existing ConcentrationState class
// Casting-time fields (already exist):
- ConcentrationType, TotalRequired, CurrentProgress, RoundsPerTick
- TargetItemId, SourceItemId

// ADD casting-time fields:
- DeferredActionType, DeferredActionPayload
- CompletionMessage, InterruptionMessage

// ADD sustained effect fields:
- SpellName, LinkedEffectIds
- FatDrainPerRound, VitDrainPerRound
```

**Database Migration**:
```sql
ALTER TABLE CharacterEffects ADD SourceEffectId TEXT NULL;
ALTER TABLE CharacterEffects ADD SourceCasterId TEXT NULL;
CREATE INDEX IX_CharacterEffects_SourceEffectId ON CharacterEffects(SourceEffectId);
```

### Implementation Order

**Phase 22-01**: Data layer foundation
- ConcentrationState complete schema
- EffectRecord property additions
- Database migration
- DTO updates

**Phase 22-02**: Casting-time concentration
- OnTick progress tracking (already exists, verify)
- OnExpire deferred action execution
- Magazine reload completion logic
- Spell casting payload (stub for future)

**Phase 22-03**: Sustained concentration
- FAT/VIT drain in OnTick
- Linked effect tracking
- OnRemove cleanup (remove all linked effects)
- Multi-target support

**Phase 22-04**: Character integration
- IsConcentrating(), GetConcentrationType()
- BreakConcentration() method
- CheckConcentration() with Focus skill
- Damage penalty calculation

**Phase 22-05**: Action system integration
- Pre-action concentration check
- Confirmation dialog component
- Single-concentration rule enforcement

**Phase 22-06**: Defense system integration
- DefenseResolver concentration check trigger
- Passive defense concentration check flow
- Damage penalty application
- Result logging

**Phase 22-07**: UI components
- Concentration indicator in CharacterStatusCard
- Progress display for casting-time
- Linked effects display for sustained
- FAT/VIT drain rate display
- Confirmation dialogs

---

## Success Criteria

### Casting-Time Concentration

1. ✅ Character can start magazine reload concentration (2-3 rounds)
2. ✅ Each round, progress increments and displays
3. ✅ On completion (OnExpire), magazine CurrentRounds updates
4. ✅ If interrupted (OnRemove), magazine does NOT update
5. ✅ UI shows "Reloading Magazine (1 of 3 rounds)"

### Sustained Concentration

1. ✅ Character can create sustained concentration + linked spell effect on target
2. ✅ Each round drains FAT/VIT from caster
3. ✅ Breaking concentration removes spell effect from target immediately
4. ✅ Target death removes their effect but caster continues (multi-target)
5. ✅ Caster FAT reaching 0 auto-breaks concentration
6. ✅ UI shows "Sustaining Invisibility (1 FAT/round)" with linked targets

### Concentration Rules

1. ✅ Only one concentration effect per character at a time
2. ✅ Starting new concentration prompts to break existing
3. ✅ Taking action prompts to break concentration
4. ✅ Passive defense triggers Focus concentration check
5. ✅ Damage applies penalty: -1 per 2 damage dealt
6. ✅ Incapacitation auto-breaks concentration

---

## Design References

**Primary Document**: `design/CONCENTRATION_SYSTEM.md` (1,303 lines, comprehensive spec)

**Key Sections**:
- Lines 10-51: Two Types of Concentration (casting vs sustained)
- Lines 115-291: Casting Time Concentration (deferred actions, OnExpire vs OnRemove)
- Lines 293-531: Sustained Effect Concentration (linking, FAT/VIT drain, multi-target)
- Lines 533-685: Spell Casting Integration (instant, casting-time, combined)
- Lines 687-835: Edge Cases (target death, caster death, dispel magic)
- Lines 937-1150: Data Model (ConcentrationState, EffectRecord, CharacterEdit)
- Lines 1152-1303: Implementation Checklist (13 phases of work)

**Related Code**:
- `GameMechanics/Effects/Behaviors/ConcentrationBehavior.cs` (partial implementation)
- `GameMechanics/EffectRecord.cs` (needs SourceEffectId/SourceCasterId)
- `GameMechanics/EffectList.cs` (RemoveEffect, ProcessEndOfRound)
- `GameMechanics/CharacterEdit.cs` (needs concentration methods)

---

## Risk Assessment

### High Risk

1. **Spell System Integration**: Spell casting completion requires spell system that doesn't fully exist yet
   - **Mitigation**: Stub out spell casting execution, focus on magazine reload for Phase 22
   - **Future**: Spell system in v1.4+ will implement spell casting payloads

2. **Cross-Character Effect Removal**: Sustained concentration on target requires finding target character
   - **Mitigation**: Use SourceEffectId to query effects across all table characters
   - **Consider**: Performance if table has many characters (100+)

### Medium Risk

1. **UI Complexity**: Displaying concentration state with progress, linked effects, drain rate
   - **Mitigation**: Start with simple text indicator, enhance in 22-07

2. **Defense Integration**: Concentration check in combat flow adds complexity
   - **Mitigation**: DefenseResolver already has hooks, add concentration check after damage

### Low Risk

1. **Database Migration**: Adding two GUID columns is straightforward
2. **Single Concentration Rule**: EffectType.Concentration already distinct, easy to enforce

---

## Open Questions

### Q1: Spell Casting Execution - How much to implement now?

**Context**: SpellCastPayload needs spell system integration that doesn't exist.

**Options**:
- A) Stub out spell casting entirely (log message, no execution)
- B) Implement simple spell effect creation (no damage/targeting logic)
- C) Wait for full spell system before implementing

**Recommendation**: **A** - Focus on magazine reload (complete implementation), stub spell casting with TODO comments.

### Q2: Multi-Table Concentration - How to find target characters?

**Context**: Sustained spell on target requires finding target character to remove effect.

**Options**:
- A) Store target character ID in ConcentrationState, fetch via CharacterPortal
- B) Query all table characters for effects with matching SourceEffectId
- C) Use messaging to broadcast concentration break to all table characters

**Recommendation**: **B** - Query approach is simplest and most reliable. Performance is fine for <100 characters.

### Q3: Concentration Check Timing - Before or after defense roll?

**Context**: Design says check happens "regardless of hit/miss" but needs clarification.

**Flow**:
```
1. Attack declared
2. Defense choice (active breaks immediately, passive continues)
3. Defense roll (hit/miss determined)
4. If hit, damage calculated
5. Concentration check (includes damage penalty if applicable)
```

**Recommendation**: Check **after** defense resolution but **during** same round. Damage penalty applies if hit landed.

---

## Dependencies

### Blocking Dependencies

None - all required infrastructure exists from Phases 17-21.

### Soft Dependencies

- **Spell System** (future): Required for full spell casting execution
  - Phase 22 can stub this out

- **Action System** (exists): Need to add concentration check before actions
  - ActionResolver pattern exists, just add check

- **Defense System** (exists): Need to add concentration check on passive defense
  - DefenseResolver pattern exists, just add check

---

## Performance Considerations

1. **Effect Queries**: Finding effects by SourceEffectId requires table scan
   - **Solution**: Added index in migration
   - **Impact**: Negligible for <1000 effects

2. **OnTick Processing**: Every effect processes every round
   - **Existing**: Already optimized with epoch-based expiration
   - **Addition**: ConcentrationBehavior.OnTick adds minimal cost

3. **Cross-Character Lookups**: Sustained concentration needs to find target characters
   - **Solution**: Query by table ID limits scope
   - **Impact**: O(n) where n = characters at table (typically <10)

---

## Testing Strategy

### Unit Tests

**ConcentrationBehavior**:
- OnTick progress tracking (casting-time)
- OnExpire deferred action execution (casting-time)
- OnRemove cleanup without execution (casting-time)
- OnTick FAT/VIT drain (sustained)
- OnRemove linked effect removal (sustained)

**CharacterEdit**:
- IsConcentrating() detection
- GetConcentrationType() returns correct type
- CheckConcentration() calculates correctly
- BreakConcentration() calls RemoveEffect

**Magazine Reload**:
- Complete concentration updates CurrentRounds
- Interrupted concentration doesn't update CurrentRounds
- Progress visible in Description

### Integration Tests

**Casting-Time Flow**:
1. Create concentration effect with magazine reload
2. Advance 3 rounds (OnTick each round)
3. Verify magazine updated on round 3
4. Verify concentration effect removed

**Sustained Flow**:
1. Create concentration on caster + spell effect on target
2. Advance 1 round
3. Verify FAT drained from caster
4. Break concentration on caster
5. Verify spell effect removed from target

**Concentration Rules**:
1. Create concentration effect on character
2. Attempt to create second concentration
3. Verify first broken, second applied
4. Verify UI prompts for confirmation

**Defense Integration**:
1. Character concentrating, passive defense
2. Attack hits, deals damage
3. Verify concentration check triggered
4. Verify damage penalty applied
5. If failed, verify concentration broken

---

*Context prepared: 2026-01-29 after CONCENTRATION_SYSTEM.md design completion*
