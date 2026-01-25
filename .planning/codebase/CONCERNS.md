# Codebase Concerns

**Analysis Date:** 2026-01-24

## Tech Debt

### Incomplete SQLite DAL Implementations

**Issue:** Multiple data access layer implementations use hardcoded in-memory data instead of persisting to SQLite.

**Files:**
- `Threa.Dal.SqlLite\SpeciesDal.cs` (line 15)
- `Threa.Dal.SqlLite\MagicSchoolDal.cs` (lines 15, 21, 27, 39)

**Impact:**
- Species and magic school modifications are not persisted across application restarts
- Data loss risk if application crashes
- Future scaling to real database will require significant refactoring

**Fix approach:**
- Replace hardcoded species/school lists with actual SQLite queries
- Implement proper entity mapping for persistence
- Add migration strategy to seed core data on first run
- Ensure transactions and error handling for writes

### Incomplete Play Page UI Implementation

**Issue:** Multiple game play tabs contain TODO comments and stub implementations with placeholder data only.

**Files:**
- `Threa.Client\Components\Pages\GamePlay\TabPlayMagic.razor` (lines 144, 150)
- `Threa.Client\Components\Pages\GamePlay\TabPlayInventory.razor` (lines 172, 190, 195)
- `Threa.Client\Components\Pages\GamePlay\TabDefense.razor` (lines 198, 201, 208, 219, 233, 242)

**Impact:**
- Magic tab loads empty mana pools and spells - players cannot cast spells in actual gameplay
- Inventory tab shows equipped items but cannot actually use items or activate combat items
- Defense tab cannot track stance changes or calculate defense appropriately
- Features advertised in design docs are not functional

**Fix approach:**
- `TabPlayMagic.razor`: Load mana pools from character's mana skills, load spells from spell definitions, implement gather mana and cast spell actions
- `TabPlayInventory.razor`: Load actual character inventory items, implement item usage server calls, implement activation mechanics
- `TabDefense.razor`: Load equipped armor from character items, calculate total absorption, implement stance changes, connect to server

### Surgery Requirements Not Implemented

**Issue:** Implant equipment and removal reference surgery requirements that are not actually checked or enforced.

**Files:**
- `GameMechanics\Items\ItemManagementService.cs` (lines 135, 186)

**Impact:**
- Characters can equip/unequip implants without any surgery skill check or validation
- Implant system design is incomplete - surgery requirements exist in docs but not in code
- Combat edge case: players could equip high-level implants instantly, breaking balance

**Fix approach:**
- Check if character has surgery skill at sufficient level when equipping/unequipping implants
- Look up implant definition to determine surgery requirements
- Block equip/unequip if requirements not met, return error with details
- Consider cooldown between implant swaps to prevent rapid equipping/removal

### Incomplete Time System Event Processing

**Issue:** Turn-specific and week-specific event processing is stubbed out with TODO comments.

**Files:**
- `GameMechanics\Time\TimeManager.cs` (lines 401, 476)

**Impact:**
- Turn-level effects (e.g., 10-minute cooldowns, short-term buffs) are not processed correctly
- Week-level events (e.g., long-term healing, status recoveries) are skipped
- Time system appears complete but critical game loop mechanics are missing

**Fix approach:**
- Implement turn-specific processing for cooldown decrements, short-duration effects
- Implement week-specific processing for healing recovery, long-term conditions
- Add corresponding handler methods for both event types

## Known Bugs

### No Known Critical Bugs Documented

**Note:** The codebase contains recent commits addressing format/theme issues and ammo container refactoring, suggesting active maintenance and bug fixing. However, no explicit bug tracking document was found. Consider maintaining a KNOWN_BUGS.md file in the design/ directory.

## Security Considerations

### Nullable Reference Types Enabled But Inconsistent Null Handling

**Risk:** While nullable reference types are enabled project-wide (positive), the codebase shows inconsistent null handling patterns with multiple uses of null coalescing (`??`) and null-conditional operators (`?.`).

**Files:** Throughout GameMechanics/
- `GameMechanics/Combat/AmmoContainerProperties.cs`: Multiple null returns (lines showing `return null`)
- `GameMechanics/Combat/RangedWeaponProperties.cs`: Chains of null-coalescing operators
- `GameMechanics/Effects/ItemEffectService.cs`: ItemActionCheckResult and ItemEffectApplicationResult with nullable error messages

**Current mitigation:**
- Nullable reference types enabled
- CSLA business objects use property getters/setters with GetProperty/LoadProperty

**Recommendations:**
- Add explicit null checks before calling methods on complex objects (weapons, armor, effects)
- Consider creating wrapper/extension methods for common null-safety patterns
- Add input validation in all DAL read methods before deserialization
- Ensure JsonDeserialize() calls have proper null checks on results

### No Explicit Input Validation on User Actions

**Risk:** UI components send user actions to game mechanics but no explicit validation layer visible between UI and business logic.

**Files:**
- `Threa.Client\Components\Pages\GamePlay\*.razor` - Multiple files call server actions without pre-validation
- `GameMechanics\Actions\ActionResolver.cs` - Accepts requests but validation logic not immediately obvious

**Current mitigation:**
- CSLA business rules engine provides post-action validation
- Server-side processing ensures validation before state changes

**Recommendations:**
- Add explicit pre-flight checks in UI for impossible actions (e.g., casting spell with insufficient AP)
- Validate skill AS values and equipment before sending attack/defense requests
- Add rate limiting to prevent action spam from UI

### Potential JSON Deserialization Vulnerabilities in DAL

**Risk:** TableDal and other SQLite DALs deserialize JSON from database directly without schema validation.

**Files:**
- `Threa.Dal.SqlLite\TableDal.cs` (lines 77-79, 101)
- Pattern: `JsonSerializer.Deserialize<T>(json)` followed by null checks but no schema validation

**Recommendations:**
- Add JSON schema validation before deserialization
- Use JsonSerializerOptions with restrictive settings (ignore unknown properties set to error, not ignore)
- Consider adding audit logging for failed deserializations

## Performance Bottlenecks

### Large Test Classes and Behavior Files

**Problem:** Several files exceed 1000 lines, indicating complex, monolithic implementations that could impact maintainability and test performance.

**Files:**
- `GameMechanics.Test\CombatTests.cs` (1,895 lines)
- `GameMechanics.Test\EffectsTests.cs` (1,696 lines)
- `Threa.Dal.MockDb\MockDb.cs` (2,047 lines)
- `GameMechanics\Effects\Behaviors\DrugBehavior.cs` (1,086 lines)
- `GameMechanics\Effects\Behaviors\SpellBuffBehavior.cs` (690 lines)
- `GameMechanics\Effects\Behaviors\PoisonBehavior.cs` (463 lines)

**Impact:**
- Test suite execution time likely increased due to file size
- Code review and modification difficult in large files
- Harder to locate specific test cases or effect behaviors

**Improvement path:**
- Split CombatTests.cs into focused test classes: WeaponAttackTests, RangedAttackTests, DamageResolutionTests, etc.
- Extract reusable test fixtures and data factories
- Consider splitting effect behaviors by category (StatusEffects, DamageEffects, UtilityEffects)
- MockDb could move reference data to factory classes

### Effect System Complexity

**Problem:** Effect application and management spread across multiple services with cascading calculations.

**Files:**
- `GameMechanics\Effects\EffectManager.cs` (437 lines)
- `GameMechanics\Effects\EffectCalculator.cs` (434 lines)
- `GameMechanics\Effects\ItemEffectService.cs` (426 lines)
- `GameMechanics\Effects\EffectRecord.cs` (413 lines)
- `GameMechanics\EffectList.cs` (672 lines)

**Impact:**
- Effect interactions could be difficult to trace (which service handles curse blocks? which handles damage reduction?)
- Risk of double-applying effects or missing cleanup
- Difficult to add new effect types without understanding entire system

**Improvement path:**
- Document effect lifecycle: creation → application → condition check → expiration
- Create effect registry for all effect types and their behaviors
- Add tracing/logging for effect application decisions

## Fragile Areas

### Ammo Container and Ammunition Properties

**Files:**
- `GameMechanics\Combat\AmmoContainerProperties.cs`
- `GameMechanics\Combat\AmmunitionProperties.cs`
- `GameMechanics\Combat\WeaponAmmoState.cs`
- `GameMechanics\Combat\AmmoContainerState.cs`

**Why fragile:**
- Recent refactor commit "72d339b Major change to ammo - now discrete support for containers" indicates this was recently overhauled
- Multiple null checks and property parsing patterns suggest complexity
- Pattern: Properties stored in JSON CustomProperties dict, parsed on each access
- No validation that ammo matches weapon type until usage

**Safe modification:**
- Add comprehensive ammo-weapon compatibility tests before modifying
- Create AmmoCompatibilityValidator tests covering all ammo types vs weapon types
- Don't modify CustomProperties structure without migration strategy

**Test coverage:**
- `GameMechanics.Test\RangedCombatTests.cs` (915 lines) appears to cover this, but fragility remains

### Character State Initialization

**Files:**
- `GameMechanics\CharacterEdit.cs` (607 lines)

**Why fragile:**
- Central business object that many systems depend on
- Skill initialization, attribute modifiers, health pool calculations all happen here
- Species modifiers applied during initialization could cause cascading errors if species data is wrong

**Safe modification:**
- Verify species exists before applying modifiers
- Add tests for each species to ensure modifiers calculate correctly
- Test health pool calculations with extreme attribute values (very low END)

**Test coverage:**
- Tests exist but concentrate on specific workflows rather than comprehensive state initialization

### Item Inventory and Equipment Slot Management

**Files:**
- `GameMechanics\Items\ItemManagementService.cs`

**Why fragile:**
- Handles both item addition and equipment slot assignment
- Effects applied during equip but removal/swap logic complex
- Container relationships (items in bags) not fully validated

**Safe modification:**
- Before modifying slot assignment logic, run ItemManagementServiceTests completely
- Add tests for edge cases: equip → damage → unequip, unequip cursed items
- Test container containment logic (item in bag, bag in other bag)

## Missing Critical Features

### Magic System UI Not Functional

**Problem:** Mana pools and spell casting shown in design but not implemented in play page tabs.

**Impact:**
- Players cannot use primary magic mechanic in actual gameplay
- Core feature blocks testing of spell effects and mana recovery

**Blocks:**
- Magic system end-to-end testing
- Player gameplay for magic-using characters
- Spell effect validation

### Defense System Incomplete in UI

**Problem:** Defense stance tracking and armor absorption not connected to server.

**Impact:**
- Players cannot change defensive stances during combat
- Armor calculation for display only, not enforced

**Blocks:**
- Defense mechanics testing
- Combat workflow testing for defender characters

### Inventory Item Usage Incomplete

**Problem:** TabPlayInventory shows items but cannot use consumable items or activate combat items.

**Impact:**
- Consumable system untestable from UI
- Item effects cannot be triggered by players

**Blocks:**
- End-to-end consumable testing
- Item usage workflow

## Test Coverage Gaps

### UI Component Integration Tests Missing

**What's not tested:** Connection between Razor components and server-side game mechanics.

**Files:** `Threa.Client/Components/Pages/GamePlay/TabPlay*.razor` - No corresponding .Test files

**Risk:** UI state changes might not properly reflect server state, or server changes not reflected in UI. This is especially risky for:
- Pending damage/healing display accuracy
- Effect icon freshness
- Stance/defense mode synchronization

**Priority:** High - UI is primary interface for players

### Ammo Consumption and Reload Logic Edge Cases

**What's not tested:** Specific scenarios around ammo depletion, weapon switching mid-combat, reload interruption.

**Files:**
- `GameMechanics.Test\RangedCombatTests.cs` (915 lines) - extensive but may have gaps
- `GameMechanics\Combat\WeaponAmmoState.cs` - state machine for reload

**Priority:** Medium - critical for ranged combat but ranged systems relatively new

### Effect Interaction Edge Cases

**What's not tested:**
- Multiple poison sources stacking and degrading
- Curse blocking removal attempts while multiple curses active
- Wound location conflicts (contradictory wound states)
- Effect expiration during effect application (timing edge case)

**Files:**
- `GameMechanics.Test\EffectsTests.cs` (1,696 lines) - substantial, but might need expansion
- `GameMechanics\Effects\` - Multiple services with complex interactions

**Priority:** Medium - affects game balance and state consistency

### Time System Boundary Transitions

**What's not tested:** Behavior when crossing major time boundaries (minute → hour, day → week).

**Files:**
- No dedicated TimeManagerTests.cs found
- TimeManager.cs (540 lines) has TODO comments for turn/week processing

**Risk:** Effects expiring at wrong times, scheduled events firing multiple times or not at all

**Priority:** High - affects all time-dependent mechanics

---

*Concerns audit: 2026-01-24*
