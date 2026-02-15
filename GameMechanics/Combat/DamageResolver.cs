using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameMechanics.Combat
{
  /// <summary>
  /// Resolves damage through the defense sequence: Shield → Armor → Character.
  /// </summary>
  public class DamageResolver
  {
    private const int ArmorSkillTV = 8;

    private readonly IDiceRoller _diceRoller;

    public DamageResolver(IDiceRoller diceRoller)
    {
      _diceRoller = diceRoller;
    }

    /// <summary>
    /// Resolves damage through the full defense sequence.
    /// </summary>
    /// <param name="request">The damage request with all relevant info.</param>
    /// <returns>The damage resolution result.</returns>
    public DamageResolutionResult Resolve(DamageRequest request)
    {
      return Resolve(request, null, null, null);
    }

    /// <summary>
    /// Resolves damage using pre-rolled armor skill values.
    /// Used by MultiDamageResolver so the armor skill check is only rolled once
    /// across multiple damage types.
    /// </summary>
    public DamageResolutionResult Resolve(DamageRequest request, int preRolledArmorBonus, int preRolledArmorRoll, int preRolledArmorRV)
    {
      return Resolve(request, (int?)preRolledArmorBonus, (int?)preRolledArmorRoll, (int?)preRolledArmorRV);
    }

    private DamageResolutionResult Resolve(DamageRequest request, int? preRolledArmorBonus, int? preRolledArmorRoll, int? preRolledArmorRV)
    {
      var absorptionSteps = new List<AbsorptionRecord>();
      int remainingSV = request.IncomingSV;

      // Step 1: Armor skill check (free action, like Physicality)
      // Use pre-rolled values if provided (for multi-damage-type resolution)
      int armorRoll;
      int armorRV;
      int armorBonus;
      if (preRolledArmorBonus.HasValue)
      {
        armorRoll = preRolledArmorRoll!.Value;
        armorRV = preRolledArmorRV!.Value;
        armorBonus = preRolledArmorBonus.Value;
      }
      else
      {
        armorRoll = _diceRoller.Roll4dFPlus();
        int armorTotal = request.DefenderArmorAS + armorRoll;
        armorRV = armorTotal - ArmorSkillTV;
        armorBonus = CalculateArmorSkillBonus(armorRV);
      }

      // Step 2: Shield absorption (if block succeeded)
      if (request.ShieldBlockSucceeded && request.Shield != null && request.Shield.IsIntact)
      {
        var shieldRecord = AbsorbWithShield(
          request.Shield,
          request.DamageType,
          request.DamageClass,
          remainingSV,
          request.ShieldBlockRV ?? 0,
          request.ApOffset,
          request.SvMax);

        absorptionSteps.Add(shieldRecord);
        remainingSV = shieldRecord.RemainingAfter;
      }

      // Step 3: Armor absorption (by layer, outer first)
      if (remainingSV > 0)
      {
        var applicableArmor = request.ArmorPieces
          .Where(a => a.IsIntact && a.CoversLocation(request.HitLocation))
          .OrderBy(a => a.LayerOrder)
          .ToList();

        foreach (var armor in applicableArmor)
        {
          if (remainingSV <= 0)
            break;

          var armorRecord = AbsorbWithArmor(
            armor,
            request.DamageType,
            request.DamageClass,
            remainingSV,
            armorBonus,
            request.ApOffset,
            request.SvMax);

          absorptionSteps.Add(armorRecord);
          remainingSV = armorRecord.RemainingAfter;

          // Armor skill bonus only applies to first armor layer
          armorBonus = 0;
        }
      }

      // Step 4: Calculate penetrating damage
      int totalAbsorbed = request.IncomingSV - remainingSV;

      // Roll damage dice if there's penetrating damage
      DamageRollResult? damageRoll = null;
      DamageResult legacyDamage = DamageResult.None;

      if (remainingSV > 0)
      {
        // Use new dice-based damage system
        damageRoll = DamageTables.RollDamage(_diceRoller, remainingSV, request.DamageClass);

        // Also populate legacy DamageResult for backwards compatibility
        legacyDamage = new DamageResult
        {
          FatigueDamage = damageRoll.FatigueDamage,
          VitalityDamage = damageRoll.VitalityDamage,
          CausesWound = damageRoll.Wounds > 0,
          Description = damageRoll.Summary
        };
      }

      return new DamageResolutionResult
      {
        IncomingSV = request.IncomingSV,
        HitLocation = request.HitLocation,
        DamageType = request.DamageType,
        DamageClass = request.DamageClass,
        ArmorSkillRoll = armorRoll,
        ArmorSkillRV = armorRV,
        ArmorSkillBonus = CalculateArmorSkillBonus(armorRV), // Store original bonus
        AbsorptionSteps = absorptionSteps,
        TotalAbsorbed = totalAbsorbed,
        PenetratingSV = remainingSV,
        DamageRoll = damageRoll,
        FinalDamage = legacyDamage,
        Summary = BuildSummary(request, absorptionSteps, totalAbsorbed, remainingSV, damageRoll, armorRV)
      };
    }

    /// <summary>
    /// Calculates bonus absorption from armor skill RV.
    /// Uses same RVS pattern as Physicality bonus.
    /// </summary>
    private int CalculateArmorSkillBonus(int rv)
    {
      return rv switch
      {
        <= -9 => -3, // Very bad roll reduces absorption
        -8 or -7 => -2,
        -6 or -5 => -2,
        -4 or -3 => -1,
        >= -2 and <= 1 => 0,
        2 or 3 => 1,
        >= 4 and <= 7 => 2,
        >= 8 and <= 11 => 3,
        >= 12 => 4
      };
    }

    /// <summary>
    /// Processes shield absorption.
    /// </summary>
    private AbsorptionRecord AbsorbWithShield(
      ShieldInfo shield,
      DamageType damageType,
      int attackDamageClass,
      int incomingSV,
      int shieldBlockRV,
      int apOffset = 0,
      int? svMax = null)
    {
      // AP offset reduces raw absorption before DC scaling
      int rawAbsorption = shield.GetAbsorption(damageType);
      int reducedAbsorption = Math.Max(0, rawAbsorption - apOffset);
      int baseAbsorption = GetEffectiveAbsorption(
        reducedAbsorption,
        shield.DamageClass,
        attackDamageClass);

      // Shield block RV provides bonus absorption (similar to armor skill)
      int bonus = shieldBlockRV > 0 ? Math.Min(shieldBlockRV / 2, 4) : 0;
      int totalAbsorption = Math.Max(0, baseAbsorption + bonus);

      // SV max check: if total absorption exceeds svMax, cap effective SV
      bool svMaxTriggered = false;
      if (svMax.HasValue && totalAbsorption > svMax.Value)
      {
        svMaxTriggered = true;
        // Cap: armor absorbs up to svMax worth, remaining SV becomes 0
        totalAbsorption = svMax.Value;
      }

      // Can only absorb up to incoming SV and remaining durability
      int maxAbsorption = Math.Min(totalAbsorption, shield.CurrentDurability);
      int actualAbsorbed = Math.Min(maxAbsorption, incomingSV);
      int remaining = svMaxTriggered ? 0 : incomingSV - actualAbsorbed;

      // Durability loss: when svMax triggered, shield takes svMax damage
      int durabilityDamage = svMaxTriggered ? Math.Min(svMax!.Value, shield.CurrentDurability) : actualAbsorbed;
      int durabilityLost = shield.ReduceDurability(durabilityDamage);
      bool destroyed = !shield.IsIntact;

      var desc = BuildAbsorptionDescription(shield.Name, actualAbsorbed, baseAbsorption, bonus,
        apOffset, svMax, svMaxTriggered, destroyed, "bonus");

      return new AbsorptionRecord
      {
        ItemId = shield.ItemId,
        ItemName = shield.Name,
        IsShield = true,
        DamageType = damageType,
        BaseAbsorption = baseAbsorption,
        SkillBonus = bonus,
        TotalAbsorbed = actualAbsorbed,
        RemainingAfter = remaining,
        DurabilityLost = durabilityLost,
        ItemDestroyed = destroyed,
        ApOffsetApplied = apOffset,
        SvMaxApplied = svMax,
        SvMaxTriggered = svMaxTriggered,
        Description = desc
      };
    }

    /// <summary>
    /// Processes armor absorption.
    /// </summary>
    private AbsorptionRecord AbsorbWithArmor(
      ArmorInfo armor,
      DamageType damageType,
      int attackDamageClass,
      int incomingSV,
      int armorSkillBonus,
      int apOffset = 0,
      int? svMax = null)
    {
      // AP offset reduces raw absorption before DC scaling
      int rawAbsorption = armor.GetAbsorption(damageType);
      int reducedAbsorption = Math.Max(0, rawAbsorption - apOffset);
      int baseAbsorption = GetEffectiveAbsorption(
        reducedAbsorption,
        armor.DamageClass,
        attackDamageClass);

      int totalAbsorption = Math.Max(0, baseAbsorption + armorSkillBonus);

      // SV max check: if total absorption exceeds svMax, cap effective SV
      bool svMaxTriggered = false;
      if (svMax.HasValue && totalAbsorption > svMax.Value)
      {
        svMaxTriggered = true;
        totalAbsorption = svMax.Value;
      }

      // Can only absorb up to incoming SV and remaining durability
      int maxAbsorption = Math.Min(totalAbsorption, armor.CurrentDurability);
      int actualAbsorbed = Math.Min(maxAbsorption, incomingSV);
      int remaining = svMaxTriggered ? 0 : incomingSV - actualAbsorbed;

      // Durability loss: when svMax triggered, armor takes svMax damage
      int durabilityDamage = svMaxTriggered ? Math.Min(svMax!.Value, armor.CurrentDurability) : actualAbsorbed;
      int durabilityLost = armor.ReduceDurability(durabilityDamage);
      bool destroyed = !armor.IsIntact;

      var desc = BuildAbsorptionDescription(armor.Name, actualAbsorbed, baseAbsorption, armorSkillBonus,
        apOffset, svMax, svMaxTriggered, destroyed, "skill");

      return new AbsorptionRecord
      {
        ItemId = armor.ItemId,
        ItemName = armor.Name,
        IsShield = false,
        DamageType = damageType,
        BaseAbsorption = baseAbsorption,
        SkillBonus = armorSkillBonus,
        TotalAbsorbed = actualAbsorbed,
        RemainingAfter = remaining,
        DurabilityLost = durabilityLost,
        ItemDestroyed = destroyed,
        ApOffsetApplied = apOffset,
        SvMaxApplied = svMax,
        SvMaxTriggered = svMaxTriggered,
        Description = desc
      };
    }

    private static string BuildAbsorptionDescription(
      string itemName, int absorbed, int baseAbsorption, int bonus,
      int apOffset, int? svMax, bool svMaxTriggered, bool destroyed, string bonusLabel)
    {
      if (destroyed)
      {
        var desc = $"{itemName} absorbed {absorbed} SV and was DESTROYED";
        if (apOffset > 0) desc += $" (AP -{apOffset})";
        return desc;
      }

      var parts = new List<string>();
      parts.Add($"base {baseAbsorption}");
      if (bonus != 0) parts.Add($"{bonusLabel} {bonus}");
      if (apOffset > 0) parts.Add($"AP -{apOffset}");

      var result = $"{itemName} absorbed {absorbed} SV";
      if (parts.Count > 1 || apOffset > 0)
        result += $" ({string.Join(" + ", parts)})";

      if (svMaxTriggered)
        result += $" [SvMax {svMax} triggered - remaining SV nullified]";

      return result;
    }

    /// <summary>
    /// Calculates effective absorption considering damage class differences.
    /// Higher class armor absorbs lower class damage as 1 SV.
    /// </summary>
    private int GetEffectiveAbsorption(int baseAbsorption, int armorClass, int attackClass)
    {
      if (armorClass > attackClass)
      {
        // Higher class armor trivially absorbs lower class damage
        // Each class difference means the damage is 10x smaller
        return baseAbsorption * 10 * (armorClass - attackClass);
      }
      else if (attackClass > armorClass)
      {
        // Lower class armor is less effective against higher class damage
        // Damage penetrates more easily
        int classDiff = attackClass - armorClass;
        return Math.Max(0, baseAbsorption / (10 * classDiff));
      }

      return baseAbsorption;
    }

    /// <summary>
    /// Builds a human-readable summary of the damage resolution.
    /// </summary>
    private string BuildSummary(
      DamageRequest request,
      List<AbsorptionRecord> steps,
      int totalAbsorbed,
      int penetratingSV,
      DamageRollResult? damageRoll,
      int armorRV)
    {
      var sb = new StringBuilder();
      sb.Append($"Incoming {request.IncomingSV} {request.DamageType} SV to {request.HitLocation}. ");

      if (armorRV >= 2)
        sb.Append($"Armor skill: +{CalculateArmorSkillBonus(armorRV)} bonus. ");
      else if (armorRV <= -3)
        sb.Append($"Armor skill: {CalculateArmorSkillBonus(armorRV)} penalty. ");

      foreach (var step in steps)
      {
        sb.Append(step.Description).Append(". ");
      }

      if (penetratingSV <= 0)
      {
        sb.Append("All damage absorbed!");
      }
      else if (damageRoll != null)
      {
        sb.Append($"Penetrating SV {penetratingSV}: {damageRoll.Summary}");
      }
      else
      {
        sb.Append($"Penetrating SV {penetratingSV}");
      }

      return sb.ToString();
    }
  }
}
