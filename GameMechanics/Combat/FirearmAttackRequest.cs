namespace GameMechanics.Combat;

/// <summary>
/// Request for a firearm/ranged weapon attack with fire modes.
/// Simplified for UI flow where player inputs conditions and gets SV for targets.
/// </summary>
public class FirearmAttackRequest
{
    // Attacker info
    /// <summary>Attacker's weapon skill Ability Score.</summary>
    public int AttackerSkillAS { get; set; }

    /// <summary>Weapon's AV modifier (quality, magic, etc.).</summary>
    public int WeaponAVModifier { get; set; }

    /// <summary>Whether the attacker is moving (-2 AV).</summary>
    public bool AttackerIsMoving { get; set; }

    // Target/Environment info
    /// <summary>Range category to target.</summary>
    public RangeCategory Range { get; set; }

    /// <summary>Whether the target is moving (+2 TV).</summary>
    public bool TargetIsMoving { get; set; }

    /// <summary>Whether the target is prone (+2 TV).</summary>
    public bool TargetIsProne { get; set; }

    /// <summary>Whether the target is crouching (+2 TV).</summary>
    public bool TargetIsCrouching { get; set; }

    /// <summary>Target's cover type.</summary>
    public CoverType TargetCover { get; set; } = CoverType.None;

    /// <summary>Target's size category.</summary>
    public TargetSize TargetSize { get; set; } = TargetSize.Normal;

    /// <summary>
    /// TV adjustment from defender's dodge roll or GM.
    /// Only applicable for dodgeable weapons (thrown, arrows).
    /// </summary>
    public int TVAdjustment { get; set; }

    // Fire mode
    /// <summary>Fire mode being used.</summary>
    public FireMode FireMode { get; set; } = FireMode.Single;

    /// <summary>Number of rounds in a burst (for burst mode).</summary>
    public int BurstSize { get; set; } = 3;

    /// <summary>Number of rounds for suppressive fire.</summary>
    public int SuppressiveRounds { get; set; } = 10;

    // Weapon info
    /// <summary>Weapon's base SV modifier added to successful hits.</summary>
    public int BaseSVModifier { get; set; }

    /// <summary>Current loaded ammo in weapon.</summary>
    public int CurrentLoadedAmmo { get; set; }

    /// <summary>Ammo damage modifier (from special ammo types).</summary>
    public int AmmoDamageModifier { get; set; }

    /// <summary>Whether the weapon allows dodge (thrown, arrows = yes; bullets = no).</summary>
    public bool IsDodgeable { get; set; }

    // ========== AOE Properties (computed from weapon + ammo) ==========

    /// <summary>Whether this attack is an AOE attack (from weapon or ammo).</summary>
    public bool IsAOEAttack { get; set; }

    /// <summary>Effective blast radius in meters (ammo overrides weapon).</summary>
    public int EffectiveBlastRadius { get; set; }

    /// <summary>Effective blast falloff type: "Linear", "Steep", or "Flat".</summary>
    public string? EffectiveFalloff { get; set; }

    /// <summary>Direct hit bonus SV for the primary target.</summary>
    public int DirectHitBonus { get; set; }

    /// <summary>
    /// Gets the ammo consumption for this attack.
    /// </summary>
    public int GetAmmoConsumption()
    {
        // AOE attacks (grenades, HE rounds) consume 1 round
        if (IsAOEAttack)
            return 1;

        return FireMode switch
        {
            FireMode.Single => 1,
            FireMode.Burst => BurstSize,
            FireMode.Suppression => SuppressiveRounds,
            _ => 1
        };
    }

    /// <summary>
    /// Checks if there is enough ammo for this attack.
    /// </summary>
    public bool HasEnoughAmmo()
    {
        return CurrentLoadedAmmo >= GetAmmoConsumption();
    }

    /// <summary>
    /// Gets the TV penalty for the selected fire mode.
    /// Burst: +1 TV, Suppression: +3 TV, Single: 0
    /// </summary>
    public int GetFireModeTVPenalty()
    {
        return FireMode switch
        {
            FireMode.Burst => 1,
            FireMode.Suppression => 3,
            _ => 0
        };
    }

    /// <summary>
    /// Calculates the base TV from range and conditions.
    /// </summary>
    public int CalculateBaseTV()
    {
        int tv = RangeModifiers.GetBaseTV(Range);
        tv += RangeModifiers.GetTargetMovementModifier(TargetIsMoving, TargetIsProne, TargetIsCrouching);
        tv += RangeModifiers.GetCoverModifier(TargetCover);
        tv += RangeModifiers.GetSizeModifier(TargetSize);
        tv += GetFireModeTVPenalty();
        return tv;
    }

    /// <summary>
    /// Calculates the final TV including dodge adjustment.
    /// </summary>
    public int CalculateFinalTV()
    {
        return CalculateBaseTV() + TVAdjustment;
    }

    /// <summary>
    /// Calculates the effective AV base (before dice roll).
    /// </summary>
    public int CalculateAVBase()
    {
        int av = AttackerSkillAS + WeaponAVModifier;
        if (AttackerIsMoving)
            av -= 2;
        return av;
    }
}
