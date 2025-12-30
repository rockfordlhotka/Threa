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
      var absorptionSteps = new List<AbsorptionRecord>();
      int remainingSV = request.IncomingSV;

      // Step 1: Armor skill check (free action, like Physicality)
      int armorRoll = _diceRoller.Roll4dFPlus();
      int armorTotal = request.DefenderArmorAS + armorRoll;
      int armorRV = armorTotal - ArmorSkillTV;
      int armorBonus = CalculateArmorSkillBonus(armorRV);

      // Step 2: Shield absorption (if block succeeded)
      if (request.ShieldBlockSucceeded && request.Shield != null && request.Shield.IsIntact)
      {
        var shieldRecord = AbsorbWithShield(
          request.Shield,
          request.DamageType,
          request.DamageClass,
          remainingSV,
          request.ShieldBlockRV ?? 0);

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
            armorBonus);

          absorptionSteps.Add(armorRecord);
          remainingSV = armorRecord.RemainingAfter;

          // Armor skill bonus only applies to first armor layer
          armorBonus = 0;
        }
      }

      // Step 4: Calculate penetrating damage
      int totalAbsorbed = request.IncomingSV - remainingSV;
      
      // If all damage was absorbed, no damage is dealt (not even a glancing blow)
      var finalDamage = remainingSV > 0
        ? CombatResultTables.GetDamage(remainingSV)
        : DamageResult.None;

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
        FinalDamage = finalDamage,
        Summary = BuildSummary(request, absorptionSteps, totalAbsorbed, remainingSV, finalDamage, armorRV)
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
      int shieldBlockRV)
    {
      int baseAbsorption = GetEffectiveAbsorption(
        shield.GetAbsorption(damageType),
        shield.DamageClass,
        attackDamageClass);

      // Shield block RV provides bonus absorption (similar to armor skill)
      int bonus = shieldBlockRV > 0 ? Math.Min(shieldBlockRV / 2, 4) : 0;
      int totalAbsorption = Math.Max(0, baseAbsorption + bonus);

      // Can only absorb up to incoming SV and remaining durability
      int maxAbsorption = Math.Min(totalAbsorption, shield.CurrentDurability);
      int actualAbsorbed = Math.Min(maxAbsorption, incomingSV);
      int remaining = incomingSV - actualAbsorbed;

      // Durability loss = damage absorbed
      int durabilityLost = shield.ReduceDurability(actualAbsorbed);
      bool destroyed = !shield.IsIntact;

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
        Description = destroyed
          ? $"{shield.Name} absorbed {actualAbsorbed} SV and was DESTROYED"
          : $"{shield.Name} absorbed {actualAbsorbed} SV (base {baseAbsorption} + bonus {bonus})"
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
      int armorSkillBonus)
    {
      int baseAbsorption = GetEffectiveAbsorption(
        armor.GetAbsorption(damageType),
        armor.DamageClass,
        attackDamageClass);

      int totalAbsorption = Math.Max(0, baseAbsorption + armorSkillBonus);

      // Can only absorb up to incoming SV and remaining durability
      int maxAbsorption = Math.Min(totalAbsorption, armor.CurrentDurability);
      int actualAbsorbed = Math.Min(maxAbsorption, incomingSV);
      int remaining = incomingSV - actualAbsorbed;

      // Durability loss = damage absorbed
      int durabilityLost = armor.ReduceDurability(actualAbsorbed);
      bool destroyed = !armor.IsIntact;

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
        Description = destroyed
          ? $"{armor.Name} absorbed {actualAbsorbed} SV and was DESTROYED"
          : armorSkillBonus != 0
            ? $"{armor.Name} absorbed {actualAbsorbed} SV (base {baseAbsorption} + skill {armorSkillBonus})"
            : $"{armor.Name} absorbed {actualAbsorbed} SV"
      };
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
      DamageResult finalDamage,
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

      if (penetratingSV < 0)
      {
        sb.Append("All damage absorbed!");
      }
      else if (penetratingSV == 0)
      {
        sb.Append($"Glancing blow: {finalDamage.Description}");
      }
      else
      {
        sb.Append($"Penetrating SV {penetratingSV}: {finalDamage.Description}");
      }

      return sb.ToString();
    }
  }
}
