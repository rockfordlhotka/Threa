using System;
using System.Collections.Generic;

namespace GameMechanics.Combat;

/// <summary>
/// Resolves firearm attacks with fire mode support.
/// Returns SV for targets to use in damage resolution.
/// </summary>
public class FirearmAttackResolver
{
    private readonly IDiceRoller _diceRoller;

    public FirearmAttackResolver(IDiceRoller diceRoller)
    {
        _diceRoller = diceRoller;
    }

    /// <summary>
    /// Resolves a firearm attack and returns result with SV for targets.
    /// </summary>
    public FirearmAttackResult Resolve(FirearmAttackRequest request)
    {
        // Check ammo
        int ammoRequired = request.GetAmmoConsumption();
        if (!request.HasEnoughAmmo())
        {
            return FirearmAttackResult.NotEnoughAmmo(request.CurrentLoadedAmmo, ammoRequired);
        }

        // AOE is determined by weapon/ammo properties, not fire mode
        if (request.IsAOEAttack)
        {
            return ResolveAOE(request);
        }

        return request.FireMode switch
        {
            FireMode.Single => ResolveSingle(request),
            FireMode.Burst => ResolveBurst(request),
            FireMode.Suppression => ResolveSuppression(request),
            _ => ResolveSingle(request)
        };
    }

    private FirearmAttackResult ResolveSingle(FirearmAttackRequest request)
    {
        int avBase = request.CalculateAVBase();
        int diceRoll = _diceRoller.Roll4dFPlus();
        int av = avBase + diceRoll;

        int baseTV = request.CalculateBaseTV();
        int finalTV = baseTV + request.TVAdjustment;

        int rv = av - finalTV;
        bool hit = rv >= 0;

        var result = new FirearmAttackResult
        {
            FireMode = FireMode.Single,
            AVBase = avBase,
            DiceRoll = diceRoll,
            AV = av,
            BaseTV = baseTV,
            TVAdjustment = request.TVAdjustment,
            TV = finalTV,
            RV = rv,
            Hit = hit,
            AmmoConsumed = 1,
            AmmoRemaining = request.CurrentLoadedAmmo - 1
        };

        if (hit)
        {
            // SV = base + (RV / 2) + ammo modifier
            int sv = request.BaseSVModifier + (rv / 2) + request.AmmoDamageModifier;
            result.Hits.Add(new ProjectileHitResult
            {
                ShotNumber = 1,
                TVForShot = finalTV,
                RVForShot = rv,
                Hit = true,
                SV = sv
            });
            result.Description = $"Hit! Target should apply SV {sv} via Damage Resolution";
        }
        else
        {
            result.Description = $"Miss! RV {rv} (needed 0+)";
        }

        return result;
    }

    private FirearmAttackResult ResolveBurst(FirearmAttackRequest request)
    {
        int avBase = request.CalculateAVBase();
        int diceRoll = _diceRoller.Roll4dFPlus();
        int av = avBase + diceRoll;

        int baseTV = request.CalculateBaseTV();
        int baseFinalTV = baseTV + request.TVAdjustment;

        // First shot at base TV
        int rv = av - baseFinalTV;
        bool firstHit = rv >= 0;

        var result = new FirearmAttackResult
        {
            FireMode = FireMode.Burst,
            AVBase = avBase,
            DiceRoll = diceRoll,
            AV = av,
            BaseTV = baseTV,
            TVAdjustment = request.TVAdjustment,
            TV = baseFinalTV,
            RV = rv,
            Hit = firstHit,
            AmmoConsumed = request.BurstSize,
            AmmoRemaining = request.CurrentLoadedAmmo - request.BurstSize
        };

        // Resolve each shot in burst
        int totalSV = 0;
        int hitsCount = 0;

        for (int shot = 1; shot <= request.BurstSize; shot++)
        {
            // Each shot after first gets +1 TV cumulative
            int tvForShot = baseFinalTV + (shot - 1);
            int rvForShot = av - tvForShot;
            bool hitThisShot = rvForShot >= 0;

            var shotResult = new ProjectileHitResult
            {
                ShotNumber = shot,
                TVForShot = tvForShot,
                RVForShot = rvForShot,
                Hit = hitThisShot
            };

            if (hitThisShot)
            {
                // SV = base + (RV / 2) + ammo modifier
                int sv = request.BaseSVModifier + (rvForShot / 2) + request.AmmoDamageModifier;
                shotResult.SV = sv;
                totalSV += sv;
                hitsCount++;
            }

            result.Hits.Add(shotResult);
        }

        if (hitsCount > 0)
        {
            result.Description = $"{hitsCount} hit(s)! Target should apply total SV {totalSV} via Damage Resolution";
        }
        else
        {
            result.Description = "All shots missed!";
        }

        return result;
    }

    private FirearmAttackResult ResolveSuppression(FirearmAttackRequest request)
    {
        int avBase = request.CalculateAVBase();
        int diceRoll = _diceRoller.Roll4dFPlus();
        int av = avBase + diceRoll;

        int baseTV = request.CalculateBaseTV();
        int finalTV = baseTV + request.TVAdjustment;

        int rv = av - finalTV;
        bool hit = rv >= 0;

        var result = new FirearmAttackResult
        {
            FireMode = FireMode.Suppression,
            AVBase = avBase,
            DiceRoll = diceRoll,
            AV = av,
            BaseTV = baseTV,
            TVAdjustment = request.TVAdjustment,
            TV = finalTV,
            RV = rv,
            Hit = hit,
            AmmoConsumed = request.SuppressiveRounds,
            AmmoRemaining = request.CurrentLoadedAmmo - request.SuppressiveRounds
        };

        if (hit)
        {
            // SV = base + (RV / 2) + ammo modifier
            int sv = request.BaseSVModifier + (rv / 2) + request.AmmoDamageModifier;
            result.OutputSV = sv;
            result.Description = $"Suppression successful! GM determines targets hit. Each target applies SV {sv}";
        }
        else
        {
            result.Description = "Suppression failed! No targets hit.";
        }

        return result;
    }

    private FirearmAttackResult ResolveAOE(FirearmAttackRequest request)
    {
        int avBase = request.CalculateAVBase();
        int diceRoll = _diceRoller.Roll4dFPlus();
        int av = avBase + diceRoll;

        int baseTV = request.CalculateBaseTV();
        int finalTV = baseTV + request.TVAdjustment;

        int rv = av - finalTV;
        bool hit = rv >= 0;

        var result = new FirearmAttackResult
        {
            IsAOE = true,
            AVBase = avBase,
            DiceRoll = diceRoll,
            AV = av,
            BaseTV = baseTV,
            TVAdjustment = request.TVAdjustment,
            TV = finalTV,
            RV = rv,
            Hit = hit,
            AmmoConsumed = 1, // AOE typically one grenade/rocket
            AmmoRemaining = request.CurrentLoadedAmmo - 1,
            BlastRadius = request.EffectiveBlastRadius,
            BlastFalloff = request.EffectiveFalloff
        };

        if (hit)
        {
            // SV = base + (RV / 2) + ammo modifier + direct hit bonus (for primary target)
            int baseSV = request.BaseSVModifier + (rv / 2) + request.AmmoDamageModifier;
            int directHitSV = baseSV + request.DirectHitBonus;

            result.OutputSV = baseSV;
            result.DirectHitSV = directHitSV;

            string falloffDesc = request.EffectiveFalloff ?? "Linear";
            result.Description = $"AOE hit! Blast radius: {request.EffectiveBlastRadius}m ({falloffDesc} falloff). " +
                                 $"Direct hit target: SV {directHitSV}. Other targets in area: SV {baseSV} (reduced by distance)";
        }
        else
        {
            result.Description = "AOE missed target area!";
        }

        return result;
    }
}
