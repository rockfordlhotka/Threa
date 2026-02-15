using System.Collections.Generic;
using System.Linq;

namespace GameMechanics.Combat;

/// <summary>
/// Resolves damage for attacks with multiple simultaneous damage types.
/// Rolls armor skill check once, then processes each damage type independently
/// through shield/armor absorption.
/// </summary>
public class MultiDamageResolver
{
  private const int ArmorSkillTV = 8;

  private readonly DamageResolver _damageResolver;
  private readonly IDiceRoller _diceRoller;

  public MultiDamageResolver(IDiceRoller diceRoller)
  {
    _diceRoller = diceRoller;
    _damageResolver = new DamageResolver(diceRoller);
  }

  /// <summary>
  /// Resolves multi-damage-type attack. If only one type, delegates to standard DamageResolver.
  /// </summary>
  public MultiDamageResolutionResult Resolve(MultiDamageRequest request)
  {
    var nonZeroTypes = request.WeaponDamage.GetNonZeroTypes().ToList();

    // If no damage types, create a single-type request with just the base SV
    if (nonZeroTypes.Count == 0)
    {
      var singleRequest = CreateDamageRequest(request, DamageType.Bashing, request.BaseSV);
      var singleResult = _damageResolver.Resolve(singleRequest);
      return new MultiDamageResolutionResult
      {
        PerTypeResults = new List<DamageResolutionResult> { singleResult },
        ArmorSkillRoll = singleResult.ArmorSkillRoll,
        ArmorSkillRV = singleResult.ArmorSkillRV,
        ArmorSkillBonus = singleResult.ArmorSkillBonus
      };
    }

    // Roll armor skill check ONCE for all damage types
    int armorRoll = _diceRoller.Roll4dFPlus();
    int armorTotal = request.DefenderArmorAS + armorRoll;
    int armorRV = armorTotal - ArmorSkillTV;
    int armorBonus = CalculateArmorSkillBonus(armorRV);

    // Clone shield/armor so durability consumption is shared across types
    var shieldClone = request.Shield?.Clone();
    var armorClones = request.ArmorPieces.Select(a => a.Clone()).ToList();

    var perTypeResults = new List<DamageResolutionResult>();

    foreach (var typeEntry in nonZeroTypes)
    {
      int effectiveSV = request.BaseSV + typeEntry.Value;
      if (effectiveSV <= 0)
        continue;

      var damageRequest = new DamageRequest
      {
        IncomingSV = effectiveSV,
        DamageType = typeEntry.Key,
        DamageClass = request.DamageClass,
        HitLocation = request.HitLocation,
        DefenderArmorAS = request.DefenderArmorAS,
        ShieldBlockSucceeded = request.ShieldBlockSucceeded,
        ShieldBlockRV = request.ShieldBlockRV,
        Shield = shieldClone,
        ArmorPieces = armorClones
      };

      var result = _damageResolver.Resolve(damageRequest, armorBonus, armorRoll, armorRV);
      perTypeResults.Add(result);

      // Armor skill bonus only applies to first damage type's first armor layer
      // (already handled inside DamageResolver - it zeros it after first layer)
      // Shield/armor durability is consumed cumulatively via the shared clones
    }

    return new MultiDamageResolutionResult
    {
      PerTypeResults = perTypeResults,
      ArmorSkillRoll = armorRoll,
      ArmorSkillRV = armorRV,
      ArmorSkillBonus = armorBonus
    };
  }

  private static DamageRequest CreateDamageRequest(MultiDamageRequest request, DamageType damageType, int sv)
  {
    return new DamageRequest
    {
      IncomingSV = sv,
      DamageType = damageType,
      DamageClass = request.DamageClass,
      HitLocation = request.HitLocation,
      DefenderArmorAS = request.DefenderArmorAS,
      ShieldBlockSucceeded = request.ShieldBlockSucceeded,
      ShieldBlockRV = request.ShieldBlockRV,
      Shield = request.Shield,
      ArmorPieces = request.ArmorPieces
    };
  }

  private static int CalculateArmorSkillBonus(int rv)
  {
    return rv switch
    {
      <= -9 => -3,
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
}
