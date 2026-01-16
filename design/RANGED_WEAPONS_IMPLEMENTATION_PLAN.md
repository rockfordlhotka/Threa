# Ranged Weapons Implementation Plan

## Overview

Implement ranged weapon combat including firing, reloading, ammunition tracking, and multiple fire modes. Integrates seamlessly into the existing Combat tab.

---

## 1. Data Model Changes

### 1.1 CharacterItem CustomProperties (for weapons)

Add JSON structure to track weapon instance state:

```json
{
  "loadedAmmo": 15,
  "loadedAmmoType": "Standard",
  "chamberLoaded": true
}
```

### 1.2 CharacterItem CustomProperties (for magazines/ammo containers)

```json
{
  "loadedAmmo": 30,
  "maxCapacity": 30,
  "ammoType": "9mm"
}
```

### 1.3 ItemTemplate CustomProperties (for ranged weapons)

```json
{
  "isRangedWeapon": true,
  "rangedWeaponType": "Pistol",
  "shortRange": 10,
  "mediumRange": 25,
  "longRange": 50,
  "extremeRange": 100,
  "capacity": 15,
  "chamberCapacity": 1,
  "reloadType": "Magazine",
  "acceptsLooseAmmo": false,
  "ammoType": "9mm",
  "fireModes": ["Single", "Burst"],
  "burstSize": 3,
  "suppressiveRounds": 10,
  "isDodgeable": false,
  "baseSVModifier": 0
}
```

**ReloadType values**: `Magazine`, `SingleRound`, `Cylinder`, `Belt`, `Battery`

**Fire modes**: `Single`, `Burst`, `Suppression`, `AOE`

### 1.4 ItemTemplate CustomProperties (for ammunition)

```json
{
  "isAmmunition": true,
  "ammoType": "9mm",
  "isContainer": false,
  "damageModifier": 0,
  "penetrationModifier": 0,
  "specialEffect": null
}
```

For magazines:
```json
{
  "isAmmunition": true,
  "ammoType": "9mm",
  "isContainer": true,
  "containerCapacity": 30
}
```

### 1.5 New ItemType Value

Add `Ammunition = 16` to `ItemType` enum.

---

## 2. Helper Classes

### 2.1 RangedWeaponProperties (GameMechanics/Combat/RangedWeaponProperties.cs)

Deserializes weapon CustomProperties into strongly-typed class:

```csharp
public class RangedWeaponProperties
{
    public bool IsRangedWeapon { get; set; }
    public string RangedWeaponType { get; set; } // Pistol, Rifle, Bow, etc.
    public int ShortRange { get; set; }
    public int MediumRange { get; set; }
    public int LongRange { get; set; }
    public int ExtremeRange { get; set; }
    public int Capacity { get; set; }
    public int ChamberCapacity { get; set; } // +1 in chamber
    public string ReloadType { get; set; }
    public bool AcceptsLooseAmmo { get; set; }
    public string AmmoType { get; set; }
    public List<string> FireModes { get; set; }
    public int BurstSize { get; set; } = 3;
    public int SuppressiveRounds { get; set; } = 10;
    public bool IsDodgeable { get; set; }
    public int BaseSVModifier { get; set; }

    public static RangedWeaponProperties? FromJson(string? json);
    public string ToJson();
}
```

### 2.2 WeaponAmmoState (GameMechanics/Combat/WeaponAmmoState.cs)

Tracks current ammo state of a weapon instance:

```csharp
public class WeaponAmmoState
{
    public int LoadedAmmo { get; set; }
    public string? LoadedAmmoType { get; set; }
    public bool ChamberLoaded { get; set; }

    public static WeaponAmmoState FromJson(string? json);
    public string ToJson();
}
```

### 2.3 AmmoContainerState (GameMechanics/Combat/AmmoContainerState.cs)

Tracks ammo in magazines/containers:

```csharp
public class AmmoContainerState
{
    public int LoadedAmmo { get; set; }
    public int MaxCapacity { get; set; }
    public string AmmoType { get; set; }

    public static AmmoContainerState FromJson(string? json);
    public string ToJson();
}
```

---

## 3. Ranged Attack Resolution

### 3.1 RangedAttackRequest (GameMechanics/Combat/RangedAttackRequest.cs)

```csharp
public class RangedAttackRequest
{
    // Attacker info
    public int AttackerSkillAS { get; set; }
    public int WeaponAVModifier { get; set; }
    public bool AttackerIsMoving { get; set; }

    // Target/Environment info
    public RangeCategory Range { get; set; }
    public bool TargetIsMoving { get; set; }
    public CoverType TargetCover { get; set; }
    public TargetSize TargetSize { get; set; }
    public int TVAdjustment { get; set; } // From defender dodge or GM

    // Fire mode
    public FireMode FireMode { get; set; }
    public int BurstSize { get; set; } // For burst mode
    public int SuppressiveRounds { get; set; } // For suppression

    // Weapon info
    public int BaseSVModifier { get; set; }
    public int CurrentLoadedAmmo { get; set; }
}
```

### 3.2 Enums

```csharp
public enum RangeCategory { Short, Medium, Long, Extreme }
public enum CoverType { None, Half, ThreeQuarters, Full }
public enum TargetSize { Tiny, Small, Normal, Large, Huge }
public enum FireMode { Single, Burst, Suppression, AOE }
```

### 3.3 RangedAttackResult

```csharp
public class RangedAttackResult
{
    public int AV { get; set; }
    public int TV { get; set; }
    public int RV { get; set; }
    public bool Hit { get; set; }
    public int AmmoConsumed { get; set; }

    // For single/burst - list of hits with SV
    public List<RangedHitResult> Hits { get; set; }

    // For suppression/AOE - just the SV for GM to distribute
    public int? OutputSV { get; set; }

    public string Description { get; set; }
}

public class RangedHitResult
{
    public int SV { get; set; }
    public int HitNumber { get; set; } // 1st, 2nd, 3rd hit in burst
    public int TVForThisHit { get; set; } // Cumulative +1 for burst
}
```

### 3.4 RangedAttackResolver (GameMechanics/Combat/RangedAttackResolver.cs)

```csharp
public class RangedAttackResolver
{
    private readonly IDiceRoller _diceRoller;

    public RangedAttackResult Resolve(RangedAttackRequest request)
    {
        // 1. Calculate AV
        int av = request.AttackerSkillAS + request.WeaponAVModifier + _diceRoller.Roll4dFPlus();
        if (request.AttackerIsMoving) av -= 2;

        // 2. Calculate base TV
        int tv = GetBaseTVForRange(request.Range);
        if (request.TargetIsMoving) tv += 2;
        tv += GetCoverModifier(request.TargetCover);
        tv += GetSizeModifier(request.TargetSize);
        tv += request.TVAdjustment; // Dodge or GM adjustment

        // 3. Calculate RV
        int rv = av - tv;
        bool hit = rv >= 0;

        // 4. Resolve based on fire mode
        return request.FireMode switch
        {
            FireMode.Single => ResolveSingle(request, av, tv, rv, hit),
            FireMode.Burst => ResolveBurst(request, av, tv, rv, hit),
            FireMode.Suppression => ResolveSuppression(request, av, tv, rv, hit),
            FireMode.AOE => ResolveAOE(request, av, tv, rv, hit),
            _ => throw new ArgumentException("Unknown fire mode")
        };
    }

    private int GetBaseTVForRange(RangeCategory range) => range switch
    {
        RangeCategory.Short => 8,
        RangeCategory.Medium => 10,
        RangeCategory.Long => 12,
        RangeCategory.Extreme => 14,
        _ => 8
    };
}
```

---

## 4. Reload Actions

### 4.1 Quick Reload (Weapon from Magazine)

- **Cost**: 1 AP + 1 FAT
- **Effect**: Swap current magazine for full one from inventory
- **Flow**:
  1. Find compatible magazine in inventory
  2. If current magazine has rounds, return to inventory
  3. Load new magazine into weapon
  4. Update weapon's LoadedAmmo from magazine

### 4.2 Quick Reload (Bow/Single Round Weapons)

- **Cost**: 1 AP + 1 FAT per round
- **Effect**: Load one round from loose ammo
- **Flow**:
  1. Find compatible loose ammo in inventory
  2. Decrement ammo stack
  3. Increment weapon's LoadedAmmo

### 4.3 Reload Magazine (Long-Running Action)

- **Cost**: Concentration-like, spans multiple rounds
- **Effect**: Load loose rounds into empty/partial magazine
- **Flow**:
  1. Player starts action with target magazine and loose ammo source
  2. Each round: load X rounds (based on skill?)
  3. Can be interrupted
  4. On completion: magazine LoadedAmmo updated, loose ammo consumed

**Implementation**: Create `ReloadMagazineEffect` similar to spell concentration:
- Effect type: `CombatStance` or new `Concentration` type
- BehaviorState tracks: magazine ID, ammo source ID, rounds loaded per tick
- OnTick: load rounds, decrement source
- OnExpire/Remove: finalize or cancel

---

## 5. UI Components

### 5.1 RangedAttackMode.razor

New component for Combat tab, similar to AttackMode.razor:

```
┌─────────────────────────────────────────────────────────────┐
│ RANGED ATTACK                                    [Cancel]   │
├─────────────────────────────────────────────────────────────┤
│ Weapon: Glock 17 (15/17 rounds)           [Reload]          │
│ Ammo Type: 9mm Standard                                     │
├─────────────────────────────────────────────────────────────┤
│ Fire Mode:  ○ Single  ○ Burst (3)  ○ Suppression           │
├─────────────────────────────────────────────────────────────┤
│ Range:      ○ Short   ○ Medium   ○ Long   ○ Extreme        │
├─────────────────────────────────────────────────────────────┤
│ Attacker Conditions:                                        │
│   [ ] Moving (-2 AV)                                        │
├─────────────────────────────────────────────────────────────┤
│ Target Conditions:                                          │
│   [ ] Moving (+2 TV)                                        │
│   Cover: [None ▼]  (+0/+1/+2 TV)                           │
│   Size:  [Normal ▼]                                         │
├─────────────────────────────────────────────────────────────┤
│ TV Adjustment: [___] (from dodge roll or GM)                │
│   ℹ️ Weapon does not allow dodge                            │
├─────────────────────────────────────────────────────────────┤
│ Calculated TV: 10                                           │
│ Your AS: 12  Weapon Mod: +1  Moving: -2  = AV Base: 11      │
├─────────────────────────────────────────────────────────────┤
│              [ 🎯 FIRE WEAPON ]                             │
└─────────────────────────────────────────────────────────────┘
```

### 5.2 RangedAttackResult Display

After rolling:

```
┌─────────────────────────────────────────────────────────────┐
│ ATTACK RESULT                                               │
├─────────────────────────────────────────────────────────────┤
│ AV Roll: 11 + 3 (4dF+) = 14                                │
│ TV: 10                                                      │
│ RV: 4 (HIT!)                                               │
├─────────────────────────────────────────────────────────────┤
│ Base SV: 0 + (RV/2 = 2) = 2                                │
│                                                             │
│ ➡️ Target should apply SV 2 via Damage Resolution          │
├─────────────────────────────────────────────────────────────┤
│ Ammo: 14/17 remaining (-1 round)                           │
├─────────────────────────────────────────────────────────────┤
│ [New Attack]  [Return to Combat]                            │
└─────────────────────────────────────────────────────────────┘
```

For Burst mode, show each hit:
```
│ Hit 1: TV 10, RV 4 → SV 2                                  │
│ Hit 2: TV 11, RV 3 → SV 1                                  │
│ Hit 3: TV 12, RV 2 → SV 1                                  │
│                                                             │
│ ➡️ Target should apply total SV 4 via Damage Resolution    │
```

### 5.3 ReloadModal.razor

Modal for reload options:

```
┌─────────────────────────────────────────────────────────────┐
│ RELOAD WEAPON                                    [X]        │
├─────────────────────────────────────────────────────────────┤
│ Current: 3/17 rounds (9mm Standard)                        │
├─────────────────────────────────────────────────────────────┤
│ Available Magazines:                                        │
│   ○ 9mm Magazine (30/30) - Full                            │
│   ○ 9mm Magazine (15/30) - Partial                         │
│   ○ 9mm AP Magazine (30/30) - Armor Piercing               │
├─────────────────────────────────────────────────────────────┤
│ Or load loose rounds:                                       │
│   ○ 9mm Rounds (x47) - Load 1 round (1 AP)                 │
├─────────────────────────────────────────────────────────────┤
│ Cost: 1 AP + 1 FAT                                         │
│              [ RELOAD ]                                     │
└─────────────────────────────────────────────────────────────┘
```

### 5.4 Combat Tab Integration

Add to TabCombat.razor:
- New `CombatMode.RangedAttack` enum value
- "RANGED ATTACK" button in default mode (shown when ranged weapon equipped)
- Ammo display in resource summary
- Quick reload button

---

## 6. Implementation Phases

### Phase 1: Data Model & Helpers
1. Add `Ammunition = 16` to ItemType enum
2. Create `RangedWeaponProperties` helper class
3. Create `WeaponAmmoState` helper class
4. Create `AmmoContainerState` helper class
5. Add extension methods for parsing CustomProperties

### Phase 2: Attack Resolution
1. Create enums (RangeCategory, CoverType, TargetSize, FireMode)
2. Create `RangedAttackRequest` and `RangedAttackResult` classes
3. Implement `RangedAttackResolver` with all fire modes
4. Add unit tests for resolver

### Phase 3: UI - Basic Ranged Attack
1. Create `RangedAttackMode.razor` component
2. Add `CombatMode.RangedAttack` to TabCombat
3. Add "Ranged Attack" button (visible when ranged weapon equipped)
4. Implement single fire mode UI flow
5. Implement ammo consumption on fire

### Phase 4: UI - Fire Modes
1. Add burst mode UI and resolution
2. Add suppression mode UI (outputs SV only)
3. Add AOE mode UI (outputs SV only)

### Phase 5: Reload System
1. Create `ReloadModal.razor` component
2. Implement magazine swap reload
3. Implement single-round reload for bows/revolvers
4. Add reload button to combat UI

### Phase 6: Magazine Reload (Long-Running)
1. Create `MagazineReloadEffect` behavior
2. Implement concentration-like reload action
3. Add UI for starting/monitoring magazine reload

### Phase 7: Sample Data & Testing
1. Add sample ranged weapons to MockDb (Pistol, Rifle, Bow)
2. Add sample ammunition items (bullets, magazines, arrows)
3. End-to-end testing

---

## 7. Files to Create/Modify

### New Files:
- `GameMechanics/Combat/RangedWeaponProperties.cs`
- `GameMechanics/Combat/WeaponAmmoState.cs`
- `GameMechanics/Combat/AmmoContainerState.cs`
- `GameMechanics/Combat/RangedAttackRequest.cs`
- `GameMechanics/Combat/RangedAttackResult.cs`
- `GameMechanics/Combat/RangedAttackResolver.cs`
- `GameMechanics/Combat/RangedEnums.cs`
- `Threa.Client/Components/Pages/GamePlay/RangedAttackMode.razor`
- `Threa.Client/Components/Pages/GamePlay/ReloadModal.razor`
- `GameMechanics/Effects/Behaviors/MagazineReloadBehavior.cs` (Phase 6)
- `GameMechanics.Test/RangedCombatTests.cs`

### Modified Files:
- `Threa.Dal/Dto/ItemType.cs` - Add Ammunition
- `Threa.Client/Components/Pages/GamePlay/TabCombat.razor` - Add ranged mode
- `Threa.Dal.MockDb/MockDb.cs` - Add sample weapons/ammo

---

## 8. TV Reference Table

| Condition | TV Modifier |
|-----------|-------------|
| Short Range | Base 8 |
| Medium Range | Base 10 |
| Long Range | Base 12 |
| Extreme Range | Base 14 |
| Target Moving | +2 |
| Half Cover | +1 |
| 3/4 Cover | +2 |
| Full Cover | Cannot target |
| Tiny Target | +2 |
| Small Target | +1 |
| Large Target | -1 |
| Huge Target | -2 |
| Attacker Moving | -2 AV (not TV) |

---

## 9. Ammo Consumption Reference

| Fire Mode | Rounds Consumed |
|-----------|-----------------|
| Single | 1 |
| Burst | BurstSize (typically 3) |
| Suppression | SuppressiveRounds (typically 10) |
| AOE | 1 (grenade/rocket) |

---

## 10. Open Questions (Resolved)

1. ✅ Magazines track their own LoadedAmmo
2. ✅ Suppression/AOE just output SV for GM distribution
3. ✅ Use CustomProperties for weapon subtypes
4. ✅ Dodge handled via TV Adjustment field, weapon indicates if dodgeable
5. ✅ Option A: All conditions gathered before single roll
