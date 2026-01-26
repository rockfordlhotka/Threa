# Phase 6: Item Bonuses & Combat - Research

**Researched:** 2026-01-25
**Domain:** CSLA.NET computed properties, item bonus calculation, combat system integration
**Confidence:** HIGH

## Summary

This phase adds item bonus calculations to character stats and integrates equipped items with the existing combat system. The research focused on understanding the existing codebase patterns for effects, modifiers, combat resolution, and CSLA property patterns to ensure consistent implementation.

The codebase already has robust support for effect-based modifiers through `EffectCalculator`, modifier stacking through `ModifierStack` and `AsModifier`, and combat resolution through `AttackResolver`/`DamageResolver`. The primary work is:
1. Creating an `ItemBonusCalculator` service that computes bonuses from equipped items
2. Integrating item bonuses into the existing `CharacterEdit.GetEffectiveAttribute()` pattern
3. Extending combat resolvers to use equipped weapon properties
4. Updating UI to show bonus breakdowns

**Primary recommendation:** Implement a stateless `ItemBonusCalculator` service following the existing `EffectCalculator` pattern, computing bonuses on-demand from equipped items with template lookups, then integrate this into character stat calculations and combat resolution.

## Standard Stack

The established libraries/tools for this domain:

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| CSLA.NET | 9.1.0 | Business objects, data portal, rules | Already used throughout, provides computed property support |
| System.Text.Json | (built-in) | Parsing ArmorAbsorption JSON | Used for CustomProperties parsing already |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| Radzen.Blazor | 8.4.2 | UI components | Tooltips, badges for bonus display |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Service pattern | CSLA child property | Service is simpler for read-only calculation, child property would add persistence complexity |
| On-demand calculation | Cached bonuses | Caching adds state management; on-demand is simpler per CONTEXT.md decision |

**No new packages required** - all functionality uses existing dependencies.

## Architecture Patterns

### Recommended Project Structure
```
GameMechanics/
├── Items/
│   ├── ItemBonusCalculator.cs         # NEW: Computes bonuses from equipped items
│   ├── ItemBonusResult.cs             # NEW: Result with attribute/skill breakdowns
│   └── [existing Item classes]
├── Combat/
│   ├── WeaponSelector.cs              # NEW: Filters weapons by combat mode
│   ├── ArmorLocationMapper.cs         # NEW: Maps EquipmentSlot to HitLocation
│   └── [existing Combat classes]
└── CharacterEdit.cs                   # MODIFY: Add GetEffectiveAttribute with item bonuses
```

### Pattern 1: Stateless Calculator Service
**What:** Calculator class with no state that takes equipped items and returns computed bonuses
**When to use:** For on-demand bonus calculation without caching
**Why:** Matches existing `EffectCalculator` pattern exactly; simple, testable, no state management

```csharp
// Source: Existing EffectCalculator pattern
public class ItemBonusCalculator
{
    /// <summary>
    /// Calculates total attribute modifier from equipped items.
    /// </summary>
    public int GetAttributeBonus(
        IEnumerable<EquippedItemInfo> equippedItems,
        string attributeName)
    {
        int total = 0;
        foreach (var item in equippedItems)
        {
            var modifiers = item.Template.AttributeModifiers
                .Where(m => m.AttributeName.Equals(attributeName, StringComparison.OrdinalIgnoreCase));

            foreach (var modifier in modifiers)
            {
                // Per CONTEXT.md: flat bonuses only
                if (modifier.ModifierType == BonusType.Flat)
                    total += (int)modifier.ModifierValue;
            }
        }
        return total;
    }

    /// <summary>
    /// Calculates total skill bonus from equipped items.
    /// </summary>
    public int GetSkillBonus(
        IEnumerable<EquippedItemInfo> equippedItems,
        string skillName)
    {
        int total = 0;
        foreach (var item in equippedItems)
        {
            var bonuses = item.Template.SkillBonuses
                .Where(b => b.SkillName.Equals(skillName, StringComparison.OrdinalIgnoreCase));

            foreach (var bonus in bonuses)
            {
                // Per CONTEXT.md: flat bonuses only
                if (bonus.BonusType == BonusType.Flat)
                    total += (int)bonus.BonusValue;
            }
        }
        return total;
    }
}
```

### Pattern 2: EquippedItemInfo Value Object
**What:** Lightweight object containing CharacterItem + loaded ItemTemplate
**When to use:** When computing bonuses to avoid repeated template lookups
**Why:** Templates are separate from instances; need both for calculations

```csharp
public class EquippedItemInfo
{
    public CharacterItem Item { get; init; } = null!;
    public ItemTemplate Template { get; init; } = null!;

    // Convenience properties
    public bool IsWeapon => Template.ItemType == ItemType.Weapon;
    public bool IsArmor => Template.ItemType == ItemType.Armor;
    public bool IsMelee => Template.WeaponType != WeaponType.None &&
                           Template.Range == null;
    public bool IsRanged => Template.Range.HasValue && Template.Range > 0;
}
```

### Pattern 3: Layered Bonus Calculation
**What:** Item bonuses applied first, then effect bonuses, for final stat
**When to use:** All stat calculations
**Why:** Per CONTEXT.md decision: "item bonuses applied to base stats first, then effect bonuses layer on top"

```csharp
// In CharacterEdit.cs
public int GetEffectiveAttribute(string attributeName)
{
    var baseValue = GetAttribute(attributeName);

    // Layer 1: Item bonuses (from ItemBonusCalculator)
    var itemBonus = _itemBonusCalculator.GetAttributeBonus(equippedItems, attributeName);

    // Layer 2: Effect bonuses (existing EffectCalculator)
    var effectModifier = Effects.GetAttributeModifier(attributeName, baseValue + itemBonus);

    return baseValue + itemBonus + effectModifier;
}
```

### Pattern 4: Combat Mode Weapon Filtering
**What:** Filter equipped weapons by combat mode (melee/ranged)
**When to use:** When presenting weapon choices in combat UI
**Why:** Per CONTEXT.md: "Combat mode automatically filters to show only equipped weapons valid for that mode"

```csharp
public class WeaponSelector
{
    public IEnumerable<EquippedItemInfo> GetMeleeWeapons(
        IEnumerable<EquippedItemInfo> equippedItems)
    {
        return equippedItems.Where(i =>
            i.Template.ItemType == ItemType.Weapon &&
            (i.Item.EquippedSlot == EquipmentSlot.MainHand ||
             i.Item.EquippedSlot == EquipmentSlot.OffHand ||
             i.Item.EquippedSlot == EquipmentSlot.TwoHand) &&
            i.Template.Range == null); // No range = melee
    }

    public IEnumerable<EquippedItemInfo> GetRangedWeapons(
        IEnumerable<EquippedItemInfo> equippedItems)
    {
        return equippedItems.Where(i =>
            i.Template.ItemType == ItemType.Weapon &&
            (i.Item.EquippedSlot == EquipmentSlot.MainHand ||
             i.Item.EquippedSlot == EquipmentSlot.OffHand ||
             i.Item.EquippedSlot == EquipmentSlot.TwoHand) &&
            i.Template.Range.HasValue);
    }
}
```

### Pattern 5: Hit Location to Armor Slot Mapping
**What:** Map HitLocation enum to EquipmentSlots that cover it
**When to use:** During damage resolution to find applicable armor
**Why:** Per COMBAT_SYSTEM.md, different armor slots cover different locations

```csharp
public static class ArmorLocationMapper
{
    private static readonly Dictionary<HitLocation, EquipmentSlot[]> _locationToSlots = new()
    {
        [HitLocation.Head] = [EquipmentSlot.Head, EquipmentSlot.Face, EquipmentSlot.Ears, EquipmentSlot.Neck],
        [HitLocation.Torso] = [EquipmentSlot.Neck, EquipmentSlot.Shoulders, EquipmentSlot.Back, EquipmentSlot.Chest, EquipmentSlot.Waist],
        [HitLocation.LeftArm] = [EquipmentSlot.Shoulders, EquipmentSlot.ArmLeft, EquipmentSlot.WristLeft, EquipmentSlot.HandLeft],
        [HitLocation.RightArm] = [EquipmentSlot.Shoulders, EquipmentSlot.ArmRight, EquipmentSlot.WristRight, EquipmentSlot.HandRight],
        [HitLocation.LeftLeg] = [EquipmentSlot.Waist, EquipmentSlot.Legs, EquipmentSlot.AnkleLeft, EquipmentSlot.FootLeft],
        [HitLocation.RightLeg] = [EquipmentSlot.Waist, EquipmentSlot.Legs, EquipmentSlot.AnkleRight, EquipmentSlot.FootRight]
    };

    public static EquipmentSlot[] GetSlotsForLocation(HitLocation location)
    {
        return _locationToSlots.TryGetValue(location, out var slots) ? slots : [];
    }
}
```

### Anti-Patterns to Avoid
- **Caching bonuses in CharacterEdit:** Per CONTEXT.md, calculate on-demand every time. Caching adds complexity and staleness risk.
- **Percentage bonuses:** Per CONTEXT.md, "Flat bonuses only - no percentage-based bonuses"
- **Bonus caps:** Per CONTEXT.md, "No caps or limits on total bonuses from items"
- **Storing computed bonuses in DB:** Bonuses are computed, not persisted

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Modifier aggregation | Custom sum logic | `ModifierStack` class | Already handles bonuses/penalties, breakdown display |
| Combat damage lookup | Manual table lookup | `CombatResultTables` | Already has SV-to-damage conversion |
| Armor absorption | Per-item math | `ArmorInfo`/`DamageResolver` | Already handles DC comparison, durability |
| Effect modifiers | Custom calculation | `EffectCalculator` | Already handles all effect types |

**Key insight:** The codebase has excellent abstractions for modifiers, effects, and combat. The item bonus system should plug INTO these existing patterns, not duplicate them.

## Common Pitfalls

### Pitfall 1: Forgetting Template Lookups
**What goes wrong:** CharacterItem only has TemplateId, not the actual bonuses. Code tries to access bonuses from CharacterItem directly.
**Why it happens:** CharacterItem is lightweight by design; templates are separate.
**How to avoid:** Always load templates along with items when computing bonuses. Use `EquippedItemInfo` pattern.
**Warning signs:** NullReferenceException on bonus access, bonuses always zero.

### Pitfall 2: Not Handling Cursed Items
**What goes wrong:** Unequip logic skips cursed items, but bonus calculation also skips them.
**Why it happens:** Per CONTEXT.md, cursed items "continue to provide their bonuses (positive or negative) even when unequippable"
**How to avoid:** Bonus calculation includes ALL equipped items regardless of curse status. Only equip/unequip logic checks curses.
**Warning signs:** Cursed item penalties disappear while item is still equipped.

### Pitfall 3: UI Not Showing Breakdown
**What goes wrong:** Player sees final stat but doesn't understand why.
**Why it happens:** Per CONTEXT.md, players want "STR 12 (10 base + 2 items)" inline display.
**How to avoid:** Always return bonus breakdowns, not just totals. Use `AttributeBonusBreakdown` class.
**Warning signs:** User confusion, bug reports about "wrong" stats.

### Pitfall 4: Combat Not Using Weapon Properties
**What goes wrong:** Attack uses character's base skill without weapon SV/AV modifiers.
**Why it happens:** Existing `AttackRequest` has hardcoded values, not dynamic weapon lookup.
**How to avoid:** Modify attack flow to look up equipped weapon, merge its modifiers.
**Warning signs:** All weapons deal same damage, weapon modifiers ignored.

### Pitfall 5: Armor Location Mismatch
**What goes wrong:** Hit to head, but chest armor absorbs damage.
**Why it happens:** ArmorInfo.CoveredLocations not populated from template, or location mapping wrong.
**How to avoid:** Parse ArmorAbsorption JSON properly; use ArmorLocationMapper consistently.
**Warning signs:** Full-body absorption regardless of hit location.

### Pitfall 6: Effect Order Matters
**What goes wrong:** Some bonuses double-counted or missed due to calculation order.
**Why it happens:** Item bonuses and effect bonuses calculated in wrong order.
**How to avoid:** Per CONTEXT.md, strict order: base -> item bonuses -> effect bonuses
**Warning signs:** Stats change when no equipment changes; cascading errors.

## Code Examples

Verified patterns from existing codebase:

### Character Attribute with Bonuses
```csharp
// Source: CharacterEdit.cs existing pattern, extended
public int GetEffectiveAttribute(string attributeName)
{
    var baseValue = GetAttribute(attributeName);

    // Item bonuses (NEW)
    var itemBonus = GetItemAttributeBonus(attributeName);

    // Effect modifiers (existing)
    var effectModifier = Effects.GetAttributeModifier(attributeName, baseValue + itemBonus);

    return baseValue + itemBonus + effectModifier;
}

public AttributeBonusBreakdown GetAttributeBreakdown(string attributeName)
{
    return new AttributeBonusBreakdown
    {
        AttributeName = attributeName,
        BaseValue = GetAttribute(attributeName),
        ItemBonus = GetItemAttributeBonus(attributeName),
        EffectBonus = Effects.GetAttributeModifier(attributeName, baseValue),
        // Provides: Total, FormattedString like "STR 12 (10 base + 2 items)"
    };
}
```

### ModifierStack Integration
```csharp
// Source: AbilityScore.cs existing pattern
public class AbilityScore
{
    public ModifierStack Modifiers { get; } = new();

    // Add item bonuses using existing ModifierSource.Equipment
    public void AddEquipmentBonus(string itemName, int bonus)
    {
        if (bonus != 0)
        {
            Modifiers.Add(ModifierSource.Equipment, itemName, bonus);
        }
    }
}
```

### Weapon Property Integration for Combat
```csharp
// Source: TabCombat.razor existing weapon lookup pattern
// Extending AttackRequest to use weapon properties
public static AttackRequest CreateWithWeapon(
    int attackerBaseAS,
    int attackerPhysicalityAS,
    int defenderDodgeAS,
    EquippedItemInfo weapon)
{
    return new AttackRequest
    {
        AttackerAS = attackerBaseAS + weapon.Template.AVModifier,
        AttackerPhysicalityAS = attackerPhysicalityAS,
        DefenderDodgeAS = defenderDodgeAS,
        // SVModifier applied to final SV, not AS
        // DamageClass determines damage table
    };
}
```

### ArmorInfo from Equipped Items
```csharp
// Source: DamageResolver.cs existing ArmorInfo pattern
public static ArmorInfo FromEquippedItem(EquippedItemInfo item, HitLocation hitLocation)
{
    // Parse ArmorAbsorption JSON
    var absorption = string.IsNullOrEmpty(item.Template.ArmorAbsorption)
        ? new Dictionary<DamageType, int>()
        : JsonSerializer.Deserialize<Dictionary<DamageType, int>>(item.Template.ArmorAbsorption);

    return new ArmorInfo
    {
        ItemId = item.Item.Id.ToString(),
        Name = item.Template.Name,
        CoveredLocations = ArmorLocationMapper.GetLocationsForSlot(item.Item.EquippedSlot),
        DamageClass = item.Template.DamageClass,
        Absorption = absorption ?? new(),
        CurrentDurability = item.Item.CurrentDurability ?? item.Template.MaxDurability ?? 100,
        MaxDurability = item.Template.MaxDurability ?? 100,
        LayerOrder = GetLayerOrder(item.Item.EquippedSlot)
    };
}
```

### UI Bonus Display Pattern
```csharp
// Blazor component pattern for inline breakdown
@if (ShowBreakdown)
{
    <span class="@(TotalBonus >= 0 ? "text-success" : "text-danger")">
        @AttributeName @TotalValue
        <small class="text-muted">
            (@BaseValue base
            @if (ItemBonus != 0)
            {
                <text>@(ItemBonus >= 0 ? "+" : "")@ItemBonus items</text>
            }
            @if (EffectBonus != 0)
            {
                <text>@(EffectBonus >= 0 ? "+" : "")@EffectBonus effects</text>
            })
        </small>
    </span>
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Cached bonus state | On-demand calculation | CONTEXT.md decision | Simpler, always accurate |
| Percentage bonuses | Flat bonuses only | CONTEXT.md decision | Predictable stacking |

**Deprecated/outdated:**
- None - this is a new feature building on existing patterns

## Open Questions

Things that couldn't be fully resolved:

1. **How to inject ItemBonusCalculator into CharacterEdit**
   - What we know: CharacterEdit is a CSLA BusinessBase, uses [Inject] for DAL
   - What's unclear: Whether to inject calculator or make it static/singleton
   - Recommendation: Make ItemBonusCalculator static with no dependencies, or inject via CSLA [Inject] in methods that need it

2. **Where to load equipped items with templates**
   - What we know: CharacterItemDal returns CharacterItem without Template populated
   - What's unclear: Best place to do the join (DAL or business layer)
   - Recommendation: Add GetEquippedItemsWithTemplatesAsync to DAL interface

3. **Validation during integration testing**
   - What we know: Unit tests can use DeterministicDiceRoller
   - What's unclear: How to verify UI correctly shows breakdown
   - Recommendation: Manual testing checklist plus component-level tests if Blazor testing is set up

## Sources

### Primary (HIGH confidence)
- **Existing codebase files** - Direct code analysis:
  - `GameMechanics/Effects/EffectCalculator.cs` - Pattern for stateless calculators
  - `GameMechanics/Actions/ModifierStack.cs` - Modifier aggregation pattern
  - `GameMechanics/Actions/AsModifier.cs` - ModifierSource.Equipment exists
  - `GameMechanics/Combat/DamageResolver.cs` - ArmorInfo, damage resolution
  - `GameMechanics/Combat/ArmorInfo.cs` - Location-based armor structure
  - `GameMechanics/CharacterEdit.cs` - GetEffectiveAttribute pattern
  - `Threa.Dal/Dto/ItemTemplate.cs` - SkillBonuses, AttributeModifiers structure
  - `design/EQUIPMENT_SYSTEM.md` - Bonus stacking rules
  - `design/COMBAT_SYSTEM.md` - Hit locations, damage resolution

### Secondary (MEDIUM confidence)
- **06-CONTEXT.md** - User decisions constraining implementation

### Tertiary (LOW confidence)
- None - all findings verified against codebase

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - uses existing CSLA and project patterns
- Architecture: HIGH - extends verified existing patterns
- Pitfalls: HIGH - derived from actual code analysis

**Research date:** 2026-01-25
**Valid until:** 60 days (stable .NET/CSLA patterns)
