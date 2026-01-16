# Shared Components

This document provides documentation for the reusable Blazor components located in `Threa.Client/Components/Shared`.

---

## 1. PendingPoolBar

A reusable component for displaying a resource pool (like health or energy) with pending damage and healing. It renders a split progress bar.

**Location**: `Threa.Client/Components/Shared/PendingPoolBar.razor`

### Parameters

| Name           | Type    | Default | Description                                             |
|----------------|---------|---------|---------------------------------------------------------|
| `CurrentValue` | `int`   |         | The current value of the pool.                          |
| `MaxValue`     | `int`   |         | The maximum value of the pool.                          |
| `PendingDamage`| `int`   |         | The amount of damage that is pending.                   |
| `PendingHealing`| `int`  |         | The amount of healing that is pending.                  |
| `ShowLabels`   | `bool`  | `false` | If true, displays a text label over the bar.            |
| `Height`       | `string`| `"8px"` | The CSS height of the progress bar.                     |

### Example Usage

```razor
<PendingPoolBar 
    CurrentValue="75" 
    MaxValue="100" 
    PendingDamage="10" 
    PendingHealing="5" 
    ShowLabels="true" 
    Height="12px" />
```

---

## 2. SkillRow

A compact component to display a single skill in a list, with a button to use it.

**Location**: `Threa.Client/Components/Shared/SkillRow.razor`

### Parameters

| Name               | Type            | Default | Description                                             |
|--------------------|-----------------|---------|---------------------------------------------------------|
| `SkillName`        | `string`        |         | The name of the skill.                                  |
| `Level`            | `int`           |         | The level of the skill.                                 |
| `AbilityScore`     | `int`           |         | The character's ability score for this skill.           |
| `PrimaryAttribute` | `string`        |         | The primary attribute associated with the skill.        |
| `OnUse`            | `EventCallback` |         | The callback invoked when the "Use" button is clicked.  |
| `CanUse`           | `bool`          | `true`  | If false, the component is styled as disabled.          |

### Example Usage

```razor
<SkillRow 
    SkillName="Swords" 
    Level="5" 
    AbilityScore="12" 
    PrimaryAttribute="DEX" 
    OnUse="@(() => Console.WriteLine("Used Swords skill"))" 
    CanUse="true" />
```

---

## 3. EffectIcon

A component to display a small icon representing an active status effect.

**Location**: `Threa.Client/Components/Shared/EffectIcon.razor`

### Parameters

| Name         | Type     | Default           | Description                                                        |
|--------------|----------|-------------------|--------------------------------------------------------------------|
| `EffectType` | `string` |                   | The type of the effect (e.g., "poison", "stun"). Used to map to an icon. |
| `EffectName` | `string` |                   | The name of the effect.                                            |
| `Tooltip`    | `string` |                   | The tooltip text to display on hover.                              |
| `Stacks`     | `int?`   | `null`            | If the effect is stackable, this shows the number of stacks.       |
| `Color`      | `string` | `"black"`         | The color of the icon.                                             |

### Example Usage

```razor
<EffectIcon 
    EffectType="poison" 
    EffectName="Venom" 
    Tooltip="You are poisoned." 
    Stacks="3" 
    Color="green" />
```

---

## 4. ActionCostSelector

A component that allows a user to select between two different costs for performing an action (e.g., AP vs AP+FAT).

**Location**: `Threa.Client/Components/Shared/ActionCostSelector.razor`

### Parameters

| Name                 | Type                   | Default | Description                                             |
|----------------------|------------------------|---------|---------------------------------------------------------|
| `AP`                 | `int`                  |         | The character's available Action Points.                |
| `Fat`                | `int`                  |         | The character's available Fatigue.                      |
| `OnCostTypeSelected` | `EventCallback<ActionCostType>` |         | The callback invoked when the selection changes.        |

### Example Usage

```razor
<ActionCostSelector 
    AP="3" 
    Fat="5" 
    OnCostTypeSelected="@((cost) => selectedCost = cost)" />
```

---

## 5. BoostSelector

A component that allows a user to spend AP and/or FAT to get a boost on an action.

**Location**: `Threa.Client/Components/Shared/BoostSelector.razor`

### Parameters

| Name           | Type          | Default | Description                                             |
|----------------|---------------|---------|---------------------------------------------------------|
| `MaxAP`        | `int`         |         | The maximum AP that can be spent for a boost.           |
| `MaxFAT`       | `int`         |         | The maximum FAT that can be spent for a boost.          |
| `OnBoostChanged`| `EventCallback<int>` |         | The callback invoked when the boost value changes.      |

### Example Usage

```razor
<BoostSelector 
    MaxAP="2" 
    MaxFAT="4" 
    OnBoostChanged="@((boost) => totalBoost = boost)" />
```
