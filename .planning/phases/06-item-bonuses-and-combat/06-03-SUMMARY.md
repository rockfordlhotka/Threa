# Plan 06-03 Summary: Combat System Integration

**Status:** Complete  
**Duration:** ~90 minutes (including extensive bug fixing)  
**Checkpoint:** Human verification - Approved

## Objective

Integrate equipped weapons and armor into the combat system so that combat uses equipped weapon properties (damage class, SV/AV modifiers) and equipped armor for damage absorption.

## What Was Built

### Core Services Created

1. **WeaponSelector** (`GameMechanics/Combat/WeaponSelector.cs`)
   - Filters equipped weapons by combat mode (melee/ranged)
   - Checks both simple Range property and RangedWeaponProperties JSON
   - Supports advanced sci-fi weapons (plasma rifles, etc.)

2. **EquipmentLocationMapper** (`GameMechanics/Combat/EquipmentLocationMapper.cs`)
   - Maps equipment slots to hit locations they cover
   - Bidirectional mapping (slot→locations, location→slots)
   - Supports all armor slots (Head, Chest, Back, Shoulders, etc.)

3. **ArmorInfoFactory** (`GameMechanics/Items/ArmorInfoFactory.cs`)
   - Converts EquippedItemInfo to ArmorInfo for DamageResolver
   - Parses ArmorAbsorption JSON from item template
   - Filters armor by hit location
   - Orders armor by layer (outer to inner)

### UI Integration

**TabCombat.razor:**
- Detects equipped weapons using WeaponSelector
- Collects weapon skills from both RelatedSkill (melee) and RangedWeaponProperties.RangedSkill (ranged)
- Routes skills to appropriate attack mode (ranged vs melee)
- Shows weapon skills when weapons are equipped

**AttackMode.razor:**
- Uses weapon's AV/SV modifiers in attack calculations
- Weapon modifiers apply to attack rolls

**DamageResolution.razor:**
- Uses equipped armor for damage absorption
- Armor skill modifier applies to absorption values
- Multi-layer armor support

## Key Commits

### Initial Implementation
- `6516699` - Create WeaponSelector and EquipmentLocationMapper
- `d7a39bc` - Create ArmorInfoFactory
- `49a6a21` - Update combat UI to use weapon/armor from equipped items

### Bug Fixes (Orchestrator-Applied)

**Weapon Detection Issues:**
- `77119b2` - Check RangedWeaponProperties.IsRangedWeapon for advanced ranged weapons
- `7d003ed` - Use WeaponSelector for ranged weapon detection
- `0449200` - Check both melee and ranged weapon skills

**Skill Matching Issues:**
- `19f6605` - Use RelatedSkill instead of item Name for filtering
- `12ab1df` - Include TwoHand slot in weapon skill filtering
- `ce814cb` - Show skills matching equipped weapon RelatedSkill
- `5a26de4` - Use skill.Id instead of skill.SkillId (correct property)
- `ce1a82b` - Compare against skill.Id not skill.Name
- `a91d1ef` - Use case-insensitive skill name matching
- `e49f808` - Lookup ranged weapon skill by Id not Name

**Routing Issues:**
- `3802599` - Set hasWeaponEquipped for both melee and ranged
- `f40c0da` - Route ranged weapon skills to ranged attack mode

## Verification Results

✓ **Weapon Skills Display**
- Short sword (melee) → "Light blades" skill appears
- Plasma rifle (ranged) → "Rifle" skill appears

✓ **Ranged Attack Mode**
- Rifle skill routes to ranged attack mode
- No physicality bonus checkbox (correct for ranged)

✓ **Melee Attack Mode**
- Light blades skill routes to melee attack mode
- Physicality bonus checkbox present (correct for melee)

✓ **Armor Absorption**
- Chainmail provides absorption values
- Armor skill modifier applies correctly (-3 in test case)
- Multi-damage-type support working

## Issues Encountered & Resolutions

### Issue 1: Skill/Name vs Skill/Id Confusion
**Problem:** Code mixed up skill.Name (display name like "Light blades") with skill.Id (identifier like "light-blades") and item.Template.RelatedSkill (which stores the ID).

**Root Cause:** Inconsistent property usage across codebase.

**Resolution:** Standardized on using skill.Id for all comparisons with RelatedSkill and RangedSkill.

### Issue 2: Ranged Weapon Detection
**Problem:** Plasma rifle not detected as ranged weapon.

**Root Cause:** WeaponSelector only checked Range property, but sci-fi weapons store properties in RangedWeaponProperties JSON.

**Resolution:** Updated WeaponSelector.IsRangedWeapon() to check BOTH Range.HasValue and RangedWeaponProperties.IsRangedWeapon.

### Issue 3: TwoHand Slot Exclusion
**Problem:** Two-handed weapons (rifles) didn't show skills.

**Root Cause:** Skill filtering only checked MainHand/OffHand slots, excluded TwoHand.

**Resolution:** Added TwoHand to slot filter.

### Issue 4: Ranged Weapons Routed to Melee Mode
**Problem:** Clicking "Use" on ranged skill went to melee attack mode (with physicality checkbox).

**Root Cause:** StartAttackWithSkill always routed to CombatMode.Attack.

**Resolution:** Check if skill matches ranged weapon, route to CombatMode.RangedAttack if true.

## Deviations from Plan

**Deviation Type:** Auto-fix blockers (Rule 3)

The plan's Task 3 was completed by the executor agent, but had multiple bugs that blocked user verification:
- Skill detection logic had numerous ID/Name mismatches
- Ranged weapon detection incomplete
- Slot filtering incomplete
- Attack mode routing incorrect

All bugs were fixed by orchestrator during checkpoint verification phase. This was necessary to unblock verification and complete the plan.

**Architectural Concern (Not Fixed):**
User noted that weapon/skill filtering logic should be in GameMechanics business layer, not in UI (TabCombat.razor). This is correct per CSLA architecture, but refactoring is out of scope for this plan. Recommend future refactoring to move this logic to CharacterEdit or a dedicated service.

## Technical Debt

1. **Business Logic in UI:** TabCombat.razor contains weapon filtering and skill matching logic that should be in GameMechanics layer
2. **Duplicate Code:** DamageResolution.razor has its own armor parsing instead of using ArmorInfoFactory
3. **Case Sensitivity:** Multiple case-insensitive comparisons suggest data inconsistency between templates and skills

## Success Criteria Met

- ✓ BONUS-07: Combat uses equipped weapon properties (damage class, SV/AV modifiers)
- ✓ BONUS-08: Equipped armor provides absorption values for damage resolution
- ✓ BONUS-09: Equipped ranged weapons appear in ranged attack mode
- ✓ BONUS-10: Equipped melee weapons appear in melee attack mode

## Files Created

- `GameMechanics/Combat/WeaponSelector.cs` (72 lines)
- `GameMechanics/Combat/EquipmentLocationMapper.cs` (95 lines)
- `GameMechanics/Items/ArmorInfoFactory.cs` (94 lines)

## Files Modified

- `Threa/Threa.Client/Components/Pages/GamePlay/TabCombat.razor` (extensive changes)
- `Threa/Threa.Client/Components/Pages/GamePlay/AttackMode.razor` (parameter additions)
- `Threa/Threa.Client/Components/Pages/GamePlay/DamageResolution.razor` (armor integration)

## Lessons Learned

1. **ID vs Name confusion is pervasive:** Need consistent naming conventions (SkillId vs Id vs Name)
2. **Multiple weapon storage patterns:** Simple weapons use Range property, advanced weapons use JSON - both must be supported
3. **Equipment slot coverage:** Must check ALL relevant slots (MainHand, OffHand, TwoHand) when filtering weapons
4. **Attack mode routing:** Different combat modes (melee vs ranged) require different UI/logic

## Next Steps

Phase 6 complete. Ready for phase verification and roadmap update.
