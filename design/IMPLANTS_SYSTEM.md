# Implants System

## Overview

Implants are cybernetic or biotech enhancements surgically installed into a character's body. Unlike regular equipment, implants cannot be easily removed and provide persistent effects. This system supports cyberpunk, transhuman, and sci-fi settings.

**Related Documents**:
- [Item Effects System](ITEM_EFFECTS_SYSTEM.md) - Effect triggers, curse handling (implant rejection)
- [Effects System](EFFECTS_SYSTEM.md) - Buff/debuff mechanics
- [Equipment System](EQUIPMENT_SYSTEM.md) - Base equipment slots

---

## Implant Slots

Each character has a fixed number of implant slots representing where cybernetics can be installed.

| Slot | Count | Description |
|------|-------|-------------|
| `ImplantNeural` | 1 | Brain/neural interface, cognitive enhancement |
| `ImplantOpticLeft` | 1 | Left eye replacement/enhancement |
| `ImplantOpticRight` | 1 | Right eye replacement/enhancement |
| `ImplantAuralLeft` | 1 | Left ear replacement/enhancement |
| `ImplantAuralRight` | 1 | Right ear replacement/enhancement |
| `ImplantCardiac` | 1 | Heart/circulatory enhancement |
| `ImplantSpine` | 1 | Spinal enhancement, reflex boosters |
| `ImplantArmLeft` | 1 | Left arm cybernetic replacement |
| `ImplantArmRight` | 1 | Right arm cybernetic replacement |
| `ImplantLegLeft` | 1 | Left leg cybernetic replacement |
| `ImplantLegRight` | 1 | Right leg cybernetic replacement |
| `ImplantHandLeft` | 1 | Left hand enhancement (compatible with arm implant) |
| `ImplantHandRight` | 1 | Right hand enhancement (compatible with arm implant) |
| `ImplantSubdermal` | 3 | Under-skin armor, sensors, storage |
| `ImplantOrgan` | 2 | Internal organ replacement/enhancement |

**Total Maximum Implants**: 19 (17 unique slots + 3 subdermal + 2 organ)

### Slot Compatibility

Some implants work together, others conflict:

| Combination | Result |
|-------------|--------|
| Cyber Arm + Hand Enhancement | Compatible (hand enhances cyber arm) |
| Full Cyber Arm + Organic Hand | Incompatible (arm replaces hand) |
| Dual Optic Implants | Each eye independent |
| Neural + Spinal Interface | Often synergize (+1 AS to reflex skills) |
| Multiple Subdermal | Stack (up to 3 slots) |
| Multiple Organ | Stack (up to 2 slots) |

---

## Installation and Removal

### Installation Requirements

| Implant Grade | Required Skill | Facility | Time | Risk |
|---------------|----------------|----------|------|------|
| **Basic** | Medicine AS 10+ | Clinic | 1 hour | Low |
| **Standard** | Medicine AS 12+ | Hospital | 2-4 hours | Medium |
| **Advanced** | Medicine AS 14+ | Surgical Suite | 4-8 hours | Medium |
| **Military/Experimental** | Medicine AS 16+ | Specialized Lab | 8+ hours | High |

### Installation Procedure

1. **Pre-Surgery Check**: Medicine skill check vs implant difficulty
2. **Surgery**: Extended action (time varies by grade)
3. **Recovery**: Character gains "Recovering from Surgery" effect
4. **Activation**: Implant effects become active after recovery

### Installation Failure

| Failure Margin | Result |
|----------------|--------|
| Fail by 1-2 | Installation succeeds, +1 day recovery |
| Fail by 3-4 | Installation succeeds, minor complication (debuff 1 week) |
| Fail by 5-6 | Installation fails, implant damaged, retry needed |
| Fail by 7+ | Installation fails, patient takes wound, implant destroyed |

### Removal

Implants can be removed, but it's not trivial:

| Removal Type | Skill Check | Recovery | Notes |
|--------------|-------------|----------|-------|
| **Clean Removal** | Same as install | 1-3 days | Implant preserved for reinstall |
| **Emergency Removal** | -4 AS penalty | 1 week | Implant may be damaged |
| **Forced Removal** | N/A (combat) | Severe wound | Implant destroyed, major trauma |

### Recovery Effects

**Recovering from Surgery** (Standard):
```
Duration: 1-3 days (based on implant grade)
Effects:
  - -2 to all physical skills
  - -1 FAT recovery rate
  - Cannot install additional implants
Removal: Time, accelerated with medical care
```

---

## Implant Activation Modes

Implants can be configured with different activation behaviors.

### Always-On Implants

These implants are permanently active once installed. They cannot be disabled without removal.

- **Trigger**: `WhileEquipped` (always active)
- **Examples**: Subdermal armor, bone lacing, organ replacements
- **Power**: Either passive or has internal power source

### Toggleable Implants

These implants can be activated or deactivated by the user.

| Toggle Cost | Description | Example Implants |
|-------------|-------------|------------------|
| **Free Action** | Toggle instantly, no cost | Thermographic vision, audio filter |
| **1 AP** | Requires conscious activation | Wired reflexes, adrenal boost |
| **Mental Command** | Neural interface required, free action | Smart-link, memory playback |

### Conditional Activation

Some implants activate automatically based on conditions:

| Trigger | Description | Example |
|---------|-------------|---------|
| **On Damage** | Activates when character takes damage | Trauma damper, emergency sedative |
| **Low Health** | Activates when FAT or VIT drops below threshold | Adrenaline surge, emergency beacon |
| **Combat Start** | Activates at combat initiative | Combat drugs, targeting systems |
| **Environmental** | Activates based on environment | Low-light vision in darkness |

---

## Power Requirements

### Powered Implants

Many implants require power to function:

| Power Source | Duration | Recharge | Notes |
|--------------|----------|----------|-------|
| **Bio-Electric** | Unlimited | Passive | Uses body's electricity |
| **Internal Battery** | 24-72 hours active use | 8 hours rest | Most common |
| **External Power Pack** | 8-12 hours | Battery swap | High-drain implants |
| **Induction Charging** | N/A | Charging pad/port | Convenient for frequent use |

### Power Failure

When a powered implant runs out of power:

- Implant effects deactivate
- Some implants have graceful degradation (reduced effect at low power)
- Critical implants (cardiac, neural) may have emergency reserves
- Unpowered cyber limbs have reduced function (-4 AS)

### EMP Vulnerability

Cybernetic implants are vulnerable to EMP attacks:

| Implant Type | EMP Effect |
|--------------|------------|
| **Basic Electronics** | Disabled for SV rounds |
| **Hardened/Military** | -2 AS for SV rounds |
| **Bioware** | Immune |
| **Hybrid (Bio+Cyber)** | -1 AS for SV rounds |

Characters can install **EMP Shielding** (Subdermal slot) to protect implants.

---

## Implant Categories

### Neural Implants

Cognitive and interface enhancements.

| Implant | Effect | Toggle | Grade |
|---------|--------|--------|-------|
| **Neural Interface** | Control devices mentally, smart-link compatible | Always-On | Standard |
| **Memory Enhancement** | +2 to memory-related INT checks, record/playback | Mental Command | Standard |
| **Cognitive Processor** | +1 INT attribute | Always-On | Advanced |
| **Tactical Computer** | +2 AS to tactical/planning actions, combat analysis | 1 AP | Advanced |
| **Reflex Booster** | +1 DEX attribute, +2 Initiative | Always-On | Military |
| **Skillwire** | Load skill chips (see Skills section) | Mental Command | Advanced |

### Optic Implants

Vision enhancements.

| Implant | Effect | Toggle | Grade |
|---------|--------|--------|-------|
| **Low-Light Vision** | See in dim light | Free Action | Basic |
| **Thermographic** | See heat signatures | Free Action | Standard |
| **Telescopic** | +2 AS at Long/Extreme range | Free Action | Standard |
| **Recording** | Record/playback visual data | Mental Command | Basic |
| **Targeting System** | +1 AS to ranged attacks | Always-On | Advanced |
| **Flash Compensation** | Immune to flash blindness | Always-On | Basic |
| **Full Cyber Eyes** | Replacements with multiple modes | Mode Select | Advanced |

### Aural Implants

Hearing enhancements.

| Implant | Effect | Toggle | Grade |
|---------|--------|--------|-------|
| **Enhanced Hearing** | +2 to hearing-based awareness | Free Action | Basic |
| **Audio Filter** | Filter out specific sounds, reduce noise damage | Free Action | Standard |
| **Recording** | Record/playback audio | Mental Command | Basic |
| **Ultrasonic** | Hear ultrasonic frequencies | Free Action | Standard |
| **Dampener** | Immune to sonic attacks | Always-On | Standard |
| **Directional Mic** | Hear distant conversations | Free Action | Standard |

### Spinal Implants

Reflex and physical enhancements.

| Implant | Effect | Toggle | Grade |
|---------|--------|--------|-------|
| **Wired Reflexes** | +1 DEX, +1 to defense rolls | 1 AP to activate | Advanced |
| **Wired Reflexes II** | +2 DEX, +2 to defense rolls | 1 AP to activate | Military |
| **Pain Editor** | Ignore wound penalties (still take damage) | 1 AP to activate | Advanced |
| **Reflex Trigger** | +3 Initiative, act first in surprise | Always-On | Advanced |

### Limb Implants

Cyber limbs and enhancements.

| Implant | Effect | Toggle | Grade |
|---------|--------|--------|-------|
| **Cyber Arm** | +2 STR for that arm, can mount weapons | Always-On | Standard |
| **Cyber Arm (Enhanced)** | +3 STR, +1 DEX for that arm, 2 weapon mounts | Always-On | Advanced |
| **Cyber Leg** | +2 movement, +1 STR for kicks | Always-On | Standard |
| **Cyber Leg (Enhanced)** | +3 movement, +2 jump distance | Always-On | Advanced |
| **Cyber Hand** | +1 DEX for fine motor, hidden compartment | Always-On | Basic |

### Hand Modifications

Enhancements to hands (organic or cyber).

| Implant | Effect | Toggle | Grade |
|---------|--------|--------|-------|
| **Retractable Blade** | Concealed melee weapon (SV +2) | Free Action | Standard |
| **Shock Hand** | Melee attack, stun effect (SV rounds) | Free Action | Standard |
| **Grapple Hand** | Launch grappling line (10m range) | Free Action | Standard |
| **Tool Hand** | Built-in toolkit (+2 to technical skills) | Always-On | Basic |

### Subdermal Implants

Under-skin enhancements (3 slots available).

| Implant | Effect | Toggle | Grade |
|---------|--------|--------|-------|
| **Subdermal Armor** | +2 to armor absorption | Always-On | Standard |
| **Subdermal Armor II** | +4 to armor absorption | Always-On | Advanced |
| **EMP Shielding** | Protect all implants from EMP | Always-On | Advanced |
| **Subdermal Pocket** | Hidden storage compartment | Always-On | Basic |
| **Biomonitor** | Track health stats, alert on status changes | Always-On | Basic |
| **Dermal Plating** | +1 to all armor, visible modification | Always-On | Standard |

### Cardiac Implants

Heart and circulatory enhancements.

| Implant | Effect | Toggle | Grade |
|---------|--------|--------|-------|
| **Synthetic Heart** | +2 END, +2 FAT pool | Always-On | Advanced |
| **Adrenaline Surge** | +3 to all physical skills for 3 rounds, then -2 for 3 | 1 AP | Advanced |
| **Toxin Filter** | +4 to resist poisons, +2 to resist drugs | Always-On | Standard |
| **Cardio Boost** | +1 FAT recovery rate | Always-On | Standard |

### Organ Implants

Internal organ replacements (2 slots available).

| Implant | Effect | Toggle | Grade |
|---------|--------|--------|-------|
| **Synthetic Lungs** | Breathe in hostile environments, +1 END | Always-On | Standard |
| **Digestive Enhancement** | Eat anything organic, resist ingested poisons | Always-On | Basic |
| **Nanosurgeon Suite** | +2 VIT recovery, auto-stabilize at 0 VIT | Always-On | Military |
| **Drug Gland** | Produce combat drugs internally (see Drugs) | Conditional | Advanced |

---

## Implant Weapons

Some implants function as built-in weapons.

### Arm-Mounted Weapons

Require Cyber Arm with weapon mount.

| Weapon | Type | Capacity | Notes |
|--------|------|----------|-------|
| **Pop-Up Gun** | Pistol | 10 rounds | Concealed, -2 AS |
| **SMG Mount** | SMG | 20 rounds | Visible, no concealment penalty |
| **Shotgun Mount** | Shotgun | 4 rounds | Close range devastating |
| **Laser Mount** | Energy Pistol | 15 shots | Built-in power cell |

### Hand Weapons

Require Cyber Hand or Hand Enhancement slot.

| Weapon | Damage | Notes |
|--------|--------|-------|
| **Retractable Blade** | SV +2 | Monofilament optional (+3 SV, Military grade) |
| **Shock Hand** | SV +0 | Stun effect for SV rounds |
| **Injector** | Poison/Drug delivery | No damage, delivers payload |

---

## Implant Grades and Costs

### Grade Progression

| Grade | Availability | Quality | Lifespan |
|-------|-------------|---------|----------|
| **Basic** | Common | Functional, minimal bonuses | 10+ years |
| **Standard** | Licensed dealers | Good quality, standard bonuses | 15+ years |
| **Advanced** | Specialists | High quality, enhanced bonuses | 20+ years |
| **Military** | Black market/military | Top quality, max bonuses | 25+ years |
| **Experimental** | Rare | Cutting edge, may have side effects | Variable |

### Maintenance

Implants require periodic maintenance:

- **Basic/Standard**: Annual checkup
- **Advanced/Military**: Bi-annual checkup
- **Experimental**: Monthly monitoring

Neglected implants may malfunction or cause rejection effects.

---

## Complications and Rejection

### Implant Rejection

Some characters may reject implants, especially experimental ones.

**Rejection Check**: END + 4dF+ vs Implant Rejection TV

| Implant Type | Rejection TV |
|--------------|--------------|
| Basic | 4 |
| Standard | 6 |
| Advanced | 8 |
| Military | 10 |
| Experimental | 12 |

**Rejection Effects** (on failed check):
```
Mild Rejection (fail by 1-3):
  - -1 AS to related skills
  - Occasional pain (1 FAT/day)
  - Requires immunosuppressants

Moderate Rejection (fail by 4-6):
  - -2 AS to related skills
  - 1 FAT damage per day
  - Medical intervention needed within 1 week

Severe Rejection (fail by 7+):
  - Implant non-functional
  - 2 VIT damage per day
  - Requires immediate removal
```

### Bioware Alternative

Bioware implants use cloned tissue and organic materials:

- **No Rejection**: Body accepts organic material
- **EMP Immune**: No electronics
- **Slower Effects**: Buffs take 1 round to activate
- **Higher Cost**: 2× to 3× cybernetic equivalent
- **Limited Options**: Not all implants available as bioware

---

## Example Implant Loadouts

### Street Samurai

Combat-focused cybernetics:

| Slot | Implant |
|------|---------|
| Neural | Neural Interface + Tactical Computer |
| Optic (Both) | Targeting System, Low-Light, Thermographic |
| Aural (Both) | Enhanced Hearing, Audio Filter |
| Spine | Wired Reflexes II |
| Arm (Right) | Cyber Arm (Enhanced) with SMG Mount |
| Hand (Left) | Retractable Blade |
| Subdermal (×3) | Subdermal Armor II, EMP Shielding, Biomonitor |
| Cardiac | Adrenaline Surge |

### Netrunner / Decker

Interface and cognitive focus:

| Slot | Implant |
|------|---------|
| Neural | Neural Interface + Cognitive Processor + Memory Enhancement |
| Optic (Both) | Recording, Display Overlay |
| Hand (Both) | Tool Hands |
| Subdermal | Biomonitor |
| Cardiac | Toxin Filter |

### Infiltrator

Stealth and adaptability:

| Slot | Implant |
|------|---------|
| Neural | Neural Interface |
| Optic (Both) | Low-Light, Recording, Flash Compensation |
| Aural (Both) | Enhanced Hearing, Directional Mic |
| Spine | Reflex Trigger |
| Hand (Right) | Retractable Blade |
| Hand (Left) | Grapple Hand |
| Subdermal (×3) | Subdermal Pocket (×2), EMP Shielding |
| Organ | Synthetic Lungs |

---

## Integration with Item Effects System

Implants use the Item Effects System with these specifics:

### Trigger Mapping

| Implant Behavior | Effect Trigger |
|------------------|----------------|
| Always-On | `WhileEquipped` |
| Toggleable (Free) | `WhileEquipped` with `IsActive` flag |
| Toggleable (1 AP) | `OnUse` with duration |
| Conditional | Custom trigger condition |

### Implant as Equipment

```json
{
  "Name": "Wired Reflexes II",
  "ItemType": "Implant",
  "EquipmentSlot": "ImplantSpine",
  "RequiresSurgery": true,
  "SurgeryDifficulty": 14,
  "Grade": "Military",
  "PowerSource": "InternalBattery",
  "BatteryLife": 48,
  "Effects": [
    {
      "Name": "Boosted Reflexes",
      "EffectType": "Buff",
      "Trigger": "OnUse",
      "ActivationCost": "1 AP",
      "DurationRounds": null,
      "IsToggleable": true,
      "BehaviorState": {
        "Modifiers": [
          { "Type": "Attribute", "Target": "DEX", "Value": 2 },
          { "Type": "AbilityScore", "Target": "Defense", "Value": 2 }
        ]
      }
    }
  ]
}
```

---

## Skillwire & Skill Chips

The **Skillwire** is a neural implant (`EquipmentSlot = ImplantNeural`) that acts as a container for **skill chips** — small data chips loaded mentally that grant the character access to a skill.

### Container Mechanic

- The Skillwire has `IsContainer = true` and `MaxChipSlots = N` (commonly 4).
- Only items with `ItemType = SkillChip` (18) may be placed into a Skillwire.
- Slot capacity is enforced by `ItemManagementService.MoveToContainerAsync()`.
- Chips are loaded/unloaded via the standard `MoveToContainerAsync` call (not the Reload button).

### Skill Grant Behavior

Each skill chip has a `SkillBonuses` entry with `BonusType = GrantSkill` (3):

| Field | Purpose |
|-------|---------|
| `BonusType = GrantSkill` | Identifies this as a skill-grant (not a flat bonus) |
| `BonusValue` | The skill level the chip provides |
| `SkillName` | The name of the skill granted |
| `CustomProperties` JSON: `{"chipPrimaryAttribute":"INT"}` | Primary attribute for ability score calculation |

**Override Rule**: A chip overrides the character's native skill level **only if the chip level is higher**. If the character has Hacking 5 natively and loads a Hacking 3 chip, the chip has no effect. This is implemented via `CharacterEdit.GetChipSkillLevelBonus(skillName, nativeSkillLevel)`.

### Ability Score Calculation

For skills the character has natively, `SkillEdit.AbilityScore` automatically applies the chip bonus:

```
AS = Bonus(nativeLevel) + ChipBonus + AttributeBase + EffectModifier
```

where `ChipBonus = max(0, chipLevel - nativeLevel)`.

For skills the character does **not** have natively, use `CharacterEdit.GetChipGrantedSkills()` to enumerate them. The UI computes AS as:

```
AS = AttributeBase(chipPrimaryAttribute) + chipLevel
```

### Data Format

```json
// Skill chip ItemTemplate
{
  "ItemType": 18,
  "CustomProperties": "{\"chipPrimaryAttribute\":\"INT\"}",
  "SkillBonuses": [
    { "SkillName": "Hacking", "BonusType": 3, "BonusValue": 3 }
  ]
}

// Skillwire ItemTemplate
{
  "ItemType": 17,
  "EquipmentSlot": "ImplantNeural",
  "IsContainer": true,
  "MaxChipSlots": 4,
  "ContainerAllowedTypes": "18"
}
```

### Loading Chips in Code

```csharp
// After loading equipped items, also load chip data
var allItems = await CharacterItemDal.GetCharacterItemsAsync(character.Id);
character.SetChipItems(allItems);

// Access chip-granted skills for non-native skills
foreach (var granted in character.GetChipGrantedSkills())
{
    // granted.SkillName, granted.SkillLevel, granted.PrimaryAttribute, granted.ChipName
}
```

---

## Related Documents

- [Item Effects System](ITEM_EFFECTS_SYSTEM.md) - Core effect mechanics
- [Effects System](EFFECTS_SYSTEM.md) - Buff/debuff definitions
- [Ranged Weapons (Sci-Fi)](RANGED_WEAPONS_SCIFI.md) - Smart-link weapons, cyber targeting
- [Combat System](COMBAT_SYSTEM.md) - Combat resolution with implant bonuses
