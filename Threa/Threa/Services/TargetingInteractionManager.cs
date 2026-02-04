using System.Collections.Concurrent;
using GameMechanics;
using GameMechanics.Combat;
using GameMechanics.Messaging;

namespace Threa.Services;

/// <summary>
/// Singleton service to manage active targeting interactions.
/// </summary>
public class TargetingInteractionManager : ITargetingInteractionManager
{
    private readonly ConcurrentDictionary<Guid, TargetingInteraction> _interactions = new();
    private readonly ConcurrentDictionary<int, Queue<Guid>> _defenderQueues = new();
    private readonly ConcurrentDictionary<int, Guid> _activeDefenderInteractions = new();
    private readonly ITimeEventPublisher _publisher;
    private readonly ILogger<TargetingInteractionManager> _logger;
    private readonly object _queueLock = new();

    public TargetingInteractionManager(
        ITimeEventPublisher publisher,
        ILogger<TargetingInteractionManager> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    /// <summary>
    /// Starts a new targeting interaction.
    /// </summary>
    public async Task<(TargetingInteraction Interaction, int QueuePosition)> StartInteractionAsync(
        Guid tableId,
        int attackerId,
        string attackerName,
        int defenderId,
        string defenderName,
        TargetingAttackerData attackerData)
    {
        var interaction = new TargetingInteraction
        {
            TableId = tableId,
            AttackerId = attackerId,
            AttackerName = attackerName,
            DefenderId = defenderId,
            DefenderName = defenderName,
            AttackerData = attackerData
        };

        _interactions[interaction.InteractionId] = interaction;

        int queuePosition;

        lock (_queueLock)
        {
            // Check if defender already has an active interaction
            if (_activeDefenderInteractions.TryGetValue(defenderId, out _))
            {
                // Add to queue
                var queue = _defenderQueues.GetOrAdd(defenderId, _ => new Queue<Guid>());
                queue.Enqueue(interaction.InteractionId);
                queuePosition = queue.Count;
            }
            else
            {
                // Set as active
                _activeDefenderInteractions[defenderId] = interaction.InteractionId;
                queuePosition = 0;
            }
        }

        _logger.LogInformation(
            "Started targeting interaction {InteractionId}: {AttackerName} -> {DefenderName}, Queue position: {QueuePosition}",
            interaction.InteractionId, attackerName, defenderName, queuePosition);

        // Publish targeting request
        var message = new TargetingRequestMessage
        {
            InteractionId = interaction.InteractionId,
            TableId = tableId,
            AttackerId = attackerId,
            AttackerName = attackerName,
            DefenderId = defenderId,
            DefenderName = defenderName,
            AttackerData = attackerData
        };

        await _publisher.PublishTargetingRequestAsync(message);

        return (interaction, queuePosition);
    }

    /// <summary>
    /// Gets an interaction by ID.
    /// </summary>
    public TargetingInteraction? GetInteraction(Guid interactionId)
    {
        return _interactions.TryGetValue(interactionId, out var interaction) ? interaction : null;
    }

    /// <summary>
    /// Gets the active interaction for a defender.
    /// </summary>
    public TargetingInteraction? GetActiveInteractionForDefender(int defenderId)
    {
        if (_activeDefenderInteractions.TryGetValue(defenderId, out var interactionId))
        {
            return GetInteraction(interactionId);
        }
        return null;
    }

    /// <summary>
    /// Gets the active interaction where the character is the attacker.
    /// </summary>
    public TargetingInteraction? GetActiveInteractionForAttacker(int attackerId)
    {
        return _interactions.Values.FirstOrDefault(i =>
            i.AttackerId == attackerId &&
            i.State != TargetingState.Resolved &&
            i.State != TargetingState.Cancelled);
    }

    /// <summary>
    /// Gets all pending interactions for a defender (queued).
    /// </summary>
    public IReadOnlyList<TargetingInteraction> GetPendingInteractionsForDefender(int defenderId)
    {
        var result = new List<TargetingInteraction>();

        // Add active interaction first
        var active = GetActiveInteractionForDefender(defenderId);
        if (active != null)
        {
            result.Add(active);
        }

        // Add queued interactions
        if (_defenderQueues.TryGetValue(defenderId, out var queue))
        {
            lock (_queueLock)
            {
                foreach (var id in queue)
                {
                    if (_interactions.TryGetValue(id, out var interaction))
                    {
                        result.Add(interaction);
                    }
                }
            }
        }

        return result;
    }

    /// <summary>
    /// Updates attacker data for an interaction.
    /// </summary>
    public async Task UpdateAttackerDataAsync(Guid interactionId, TargetingAttackerData data, bool isConfirmed = false)
    {
        if (!_interactions.TryGetValue(interactionId, out var interaction))
        {
            _logger.LogWarning("Attempted to update non-existent interaction {InteractionId}", interactionId);
            return;
        }

        interaction.AttackerData = data;
        interaction.AttackerConfirmed = isConfirmed;
        interaction.UpdatedAt = DateTime.UtcNow;

        if (isConfirmed)
        {
            interaction.State = TargetingState.AttackerConfirmed;
        }

        _logger.LogDebug("Updated attacker data for interaction {InteractionId}, Confirmed: {Confirmed}",
            interactionId, isConfirmed);

        var message = new TargetingUpdateMessage
        {
            InteractionId = interactionId,
            TableId = interaction.TableId,
            IsFromAttacker = true,
            AttackerData = data,
            IsConfirmed = isConfirmed
        };

        await _publisher.PublishTargetingUpdateAsync(message);

        // Check if both parties confirmed
        if (interaction.AttackerConfirmed && interaction.DefenderConfirmed)
        {
            await ResolveInteractionAsync(interactionId);
        }
    }

    /// <summary>
    /// Updates defender data for an interaction.
    /// </summary>
    public async Task UpdateDefenderDataAsync(Guid interactionId, TargetingDefenderData data, bool isConfirmed = false)
    {
        if (!_interactions.TryGetValue(interactionId, out var interaction))
        {
            _logger.LogWarning("Attempted to update non-existent interaction {InteractionId}", interactionId);
            return;
        }

        interaction.DefenderData = data;
        interaction.DefenderConfirmed = isConfirmed;
        interaction.UpdatedAt = DateTime.UtcNow;

        if (interaction.State == TargetingState.Initiated)
        {
            interaction.State = TargetingState.DefenderViewing;
        }

        if (isConfirmed)
        {
            interaction.State = TargetingState.DefenderConfirmed;
        }

        _logger.LogDebug("Updated defender data for interaction {InteractionId}, Confirmed: {Confirmed}",
            interactionId, isConfirmed);

        var message = new TargetingUpdateMessage
        {
            InteractionId = interactionId,
            TableId = interaction.TableId,
            IsFromAttacker = false,
            DefenderData = data,
            IsConfirmed = isConfirmed
        };

        await _publisher.PublishTargetingUpdateAsync(message);

        // Check if both parties confirmed
        if (interaction.AttackerConfirmed && interaction.DefenderConfirmed)
        {
            await ResolveInteractionAsync(interactionId);
        }
    }

    /// <summary>
    /// Marks the attacker as confirmed.
    /// </summary>
    public async Task ConfirmAttackerAsync(Guid interactionId)
    {
        if (!_interactions.TryGetValue(interactionId, out var interaction))
        {
            return;
        }

        await UpdateAttackerDataAsync(interactionId, interaction.AttackerData, isConfirmed: true);
    }

    /// <summary>
    /// Marks the defender as confirmed.
    /// </summary>
    public async Task ConfirmDefenderAsync(Guid interactionId)
    {
        if (!_interactions.TryGetValue(interactionId, out var interaction) || interaction.DefenderData == null)
        {
            return;
        }

        await UpdateDefenderDataAsync(interactionId, interaction.DefenderData, isConfirmed: true);
    }

    /// <summary>
    /// Cancels a targeting interaction.
    /// </summary>
    public async Task CancelAsync(Guid interactionId, string? reason = null)
    {
        if (!_interactions.TryGetValue(interactionId, out var interaction))
        {
            return;
        }

        interaction.State = TargetingState.Cancelled;
        interaction.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Cancelled targeting interaction {InteractionId}: {Reason}",
            interactionId, reason ?? "No reason provided");

        // Clean up queues
        RemoveFromQueues(interaction);

        var message = new TargetingCancelledMessage
        {
            InteractionId = interactionId,
            TableId = interaction.TableId,
            AttackerId = interaction.AttackerId,
            DefenderId = interaction.DefenderId,
            Reason = reason
        };

        await _publisher.PublishTargetingCancelledAsync(message);

        // Process next in queue
        await ProcessNextInQueueAsync(interaction.DefenderId);
    }

    /// <summary>
    /// Resolves the targeting interaction and calculates results.
    /// </summary>
    public async Task<TargetingResolutionData?> ResolveInteractionAsync(Guid interactionId)
    {
        if (!_interactions.TryGetValue(interactionId, out var interaction))
        {
            return null;
        }

        if (interaction.DefenderData == null)
        {
            _logger.LogWarning("Cannot resolve interaction {InteractionId}: Defender data is null", interactionId);
            return null;
        }

        var attackerData = interaction.AttackerData;
        var defenderData = interaction.DefenderData;

        // Use existing resolvers based on action type
        var diceRoller = new RandomDiceRoller();
        TargetingResolutionData resolution;

        if (attackerData.ActionType == TargetingActionType.RangedAttack)
        {
            resolution = ResolveRangedAttack(attackerData, defenderData, diceRoller);
        }
        else if (attackerData.ActionType == TargetingActionType.Medical)
        {
            resolution = ResolveMedicalAction(attackerData, diceRoller);
        }
        else
        {
            resolution = ResolveMeleeAttack(attackerData, defenderData, diceRoller);
        }

        interaction.Resolution = resolution;
        interaction.State = TargetingState.Resolved;
        interaction.UpdatedAt = DateTime.UtcNow;

        _logger.LogInformation("Resolved targeting interaction {InteractionId}: Hit={IsHit}, SV={SV}",
            interactionId, resolution.IsHit, resolution.SuccessValue);

        // Clean up queues
        RemoveFromQueues(interaction);

        var message = new TargetingResultMessage
        {
            InteractionId = interactionId,
            TableId = interaction.TableId,
            AttackerId = interaction.AttackerId,
            DefenderId = interaction.DefenderId,
            Resolution = resolution
        };

        await _publisher.PublishTargetingResultAsync(message);

        // Process next in queue
        await ProcessNextInQueueAsync(interaction.DefenderId);

        return resolution;
    }

    private TargetingResolutionData ResolveMeleeAttack(
        TargetingAttackerData attackerData,
        TargetingDefenderData defenderData,
        IDiceRoller diceRoller)
    {
        var defenseResolver = new DefenseResolver(diceRoller);
        var hitLocationCalculator = new HitLocationCalculator(diceRoller);

        // Calculate attacker AV
        int avBase = attackerData.SkillAS + attackerData.WeaponAVModifier;
        if (attackerData.IsMoving)
            avBase -= 2;
        if (attackerData.IsCalledShot)
            avBase -= 2;
        avBase += attackerData.APBoost + attackerData.FATBoost;

        int attackRoll = diceRoller.Roll4dFPlus();
        int av = avBase + attackRoll;

        // Calculate defender TV based on defense type
        int tv;
        int defenseRoll = 0;
        string defenseBreakdown;

        switch (defenderData.DefenseType)
        {
            case DefenseType.Passive:
                tv = defenderData.DodgeAS - 1;
                defenseBreakdown = $"Passive Defense: Dodge AS {defenderData.DodgeAS} - 1 = TV {tv}";
                break;
            case DefenseType.Dodge:
                defenseRoll = diceRoller.Roll4dFPlus();
                tv = defenderData.DodgeAS + defenseRoll + defenderData.APBoost + defenderData.FATBoost;
                defenseBreakdown = $"Active Dodge: {defenderData.DodgeAS} + {defenseRoll} (roll) + {defenderData.APBoost + defenderData.FATBoost} (boost) = TV {tv}";
                break;
            case DefenseType.Parry:
                defenseRoll = diceRoller.Roll4dFPlus();
                tv = defenderData.ParryAS + defenseRoll + defenderData.APBoost + defenderData.FATBoost;
                defenseBreakdown = $"Parry: {defenderData.ParryAS} + {defenseRoll} (roll) + {defenderData.APBoost + defenderData.FATBoost} (boost) = TV {tv}";
                break;
            case DefenseType.ShieldBlock:
                defenseRoll = diceRoller.Roll4dFPlus();
                tv = defenderData.ShieldAS + defenseRoll + defenderData.APBoost + defenderData.FATBoost;
                defenseBreakdown = $"Shield Block: {defenderData.ShieldAS} + {defenseRoll} (roll) + {defenderData.APBoost + defenderData.FATBoost} (boost) = TV {tv}";
                break;
            default:
                tv = defenderData.DodgeAS - 1;
                defenseBreakdown = $"Default Passive: TV {tv}";
                break;
        }

        int sv = av - tv;
        bool isHit = sv >= 0;

        // Determine hit location
        HitLocation hitLocation;
        if (isHit && attackerData.IsCalledShot && attackerData.CalledShotLocation.HasValue)
        {
            // Called shot: SV >= 2 hits intended location, SV 0-1 hits random
            hitLocation = sv >= 2 ? attackerData.CalledShotLocation.Value : hitLocationCalculator.DetermineHitLocation();
        }
        else if (isHit)
        {
            hitLocation = hitLocationCalculator.DetermineHitLocation();
        }
        else
        {
            hitLocation = HitLocation.Torso; // Default for miss
        }

        // Calculate damage
        int fatDamage = 0;
        int vitDamage = 0;
        int damageClass = attackerData.WeaponDamageClass;

        if (isHit)
        {
            // Add physicality bonus if used
            if (attackerData.UsePhysicality)
            {
                int physRoll = diceRoller.Roll4dFPlus();
                int physRV = physRoll - 8; // Physicality TV is 8
                if (physRV >= 0)
                {
                    var physBonus = CombatResultTables.GetPhysicalityBonus(physRV);
                    sv += physBonus.SVModifier;
                }
            }

            // Apply weapon SV modifier (e.g., unarmed attacks: Punch +2, Kick +4)
            int effectiveSV = sv + attackerData.WeaponSVModifier;
            var damage = CombatResultTables.GetDamage(effectiveSV);
            fatDamage = damage.FatigueDamage * (damageClass > 0 ? damageClass : 1);
            vitDamage = damage.VitalityDamage * (damageClass > 0 ? damageClass : 1);
        }

        string attackBreakdown = $"Attack: {attackerData.SkillAS} (skill) + {attackerData.WeaponAVModifier} (weapon)";
        if (attackerData.IsMoving) attackBreakdown += " - 2 (moving)";
        if (attackerData.IsCalledShot) attackBreakdown += " - 2 (called shot)";
        if (attackerData.APBoost + attackerData.FATBoost > 0)
            attackBreakdown += $" + {attackerData.APBoost + attackerData.FATBoost} (boost)";
        attackBreakdown += $" + {attackRoll} (roll) = AV {av}";

        string attackerNarrative = isHit
            ? GenerateHitNarrative(sv, hitLocation, attackerData.WeaponName ?? "weapon")
            : GenerateMissNarrative();

        string defenderDetails = $"{attackBreakdown}\n{defenseBreakdown}\nSV = {av} - {tv} = {sv}";
        if (isHit)
        {
            if (attackerData.WeaponSVModifier != 0)
            {
                int effectiveSVDisplay = sv + attackerData.WeaponSVModifier;
                defenderDetails += $" + {attackerData.WeaponSVModifier} (weapon) = {effectiveSVDisplay}";
            }
            defenderDetails += $"\nHit Location: {hitLocation}\nDamage: {fatDamage} FAT, {vitDamage} VIT";
        }

        return new TargetingResolutionData
        {
            AttackerAV = av,
            DefenderTV = tv,
            DiceRoll = attackRoll,
            SuccessValue = sv,
            IsHit = isHit,
            HitLocation = hitLocation,
            DamageClass = damageClass,
            FATDamage = fatDamage,
            VITDamage = vitDamage,
            AttackerNarrative = attackerNarrative,
            DefenderDetails = defenderDetails,
            AttackBreakdown = attackBreakdown,
            DefenseBreakdown = defenseBreakdown
        };
    }

    private TargetingResolutionData ResolveRangedAttack(
        TargetingAttackerData attackerData,
        TargetingDefenderData defenderData,
        IDiceRoller diceRoller)
    {
        var hitLocationCalculator = new HitLocationCalculator(diceRoller);

        // Calculate base TV from range and conditions
        int baseTV = attackerData.Range.HasValue ? RangeModifiers.GetBaseTV(attackerData.Range.Value) : 8;

        // Add defender modifiers to TV
        if (defenderData.IsMoving) baseTV += 2;
        if (defenderData.IsProne) baseTV += 2;
        if (defenderData.IsCrouching) baseTV += 2;
        baseTV += RangeModifiers.GetCoverModifier(defenderData.Cover);
        baseTV += RangeModifiers.GetSizeModifier(defenderData.Size);

        // Calculate attacker AV
        int avBase = attackerData.SkillAS + attackerData.WeaponAVModifier + attackerData.AimBonus;
        if (attackerData.IsMoving)
            avBase -= 2;
        if (attackerData.IsCalledShot)
            avBase -= 2;
        avBase += attackerData.APBoost + attackerData.FATBoost;

        int attackRoll = diceRoller.Roll4dFPlus();
        int av = avBase + attackRoll;

        // For dodgeable weapons, defender can dodge
        int tv = baseTV;
        int defenseRoll = 0;
        string defenseBreakdown;

        if (attackerData.IsDodgeable && defenderData.DefenseType == DefenseType.Dodge)
        {
            defenseRoll = diceRoller.Roll4dFPlus();
            int dodgeTV = defenderData.DodgeAS + defenseRoll + defenderData.APBoost + defenderData.FATBoost;
            tv = Math.Max(baseTV, dodgeTV);
            defenseBreakdown = $"Active Dodge vs Dodgeable: max({baseTV} base, {dodgeTV} dodge) = TV {tv}";
        }
        else
        {
            defenseBreakdown = $"Ranged TV: {attackerData.Range} range ({RangeModifiers.GetBaseTV(attackerData.Range ?? RangeCategory.Medium)})";
            if (defenderData.IsMoving) defenseBreakdown += " + 2 (moving)";
            if (defenderData.IsProne) defenseBreakdown += " + 2 (prone)";
            if (defenderData.IsCrouching) defenseBreakdown += " + 2 (crouching)";
            if (defenderData.Cover != CoverType.None) defenseBreakdown += $" + {RangeModifiers.GetCoverModifier(defenderData.Cover)} ({defenderData.Cover} cover)";
            defenseBreakdown += $" = TV {tv}";
        }

        int sv = av - tv;
        bool isHit = sv >= 0;

        // Determine hit location
        HitLocation hitLocation;
        if (isHit && attackerData.IsCalledShot && attackerData.CalledShotLocation.HasValue)
        {
            hitLocation = sv >= 2 ? attackerData.CalledShotLocation.Value : hitLocationCalculator.DetermineHitLocation();
        }
        else if (isHit)
        {
            hitLocation = hitLocationCalculator.DetermineHitLocation();
        }
        else
        {
            hitLocation = HitLocation.Torso;
        }

        // Calculate damage
        int fatDamage = 0;
        int vitDamage = 0;
        int damageClass = attackerData.WeaponDamageClass + attackerData.AmmoDamageModifier;

        if (isHit)
        {
            int effectiveSV = sv + attackerData.WeaponSVModifier;
            var damage = CombatResultTables.GetDamage(effectiveSV);
            fatDamage = damage.FatigueDamage * (damageClass > 0 ? damageClass : 1);
            vitDamage = damage.VitalityDamage * (damageClass > 0 ? damageClass : 1);
        }

        string attackBreakdown = $"Attack: {attackerData.SkillAS} (skill) + {attackerData.WeaponAVModifier} (weapon)";
        if (attackerData.AimBonus > 0) attackBreakdown += $" + {attackerData.AimBonus} (aim)";
        if (attackerData.IsMoving) attackBreakdown += " - 2 (moving)";
        if (attackerData.IsCalledShot) attackBreakdown += " - 2 (called shot)";
        if (attackerData.APBoost + attackerData.FATBoost > 0)
            attackBreakdown += $" + {attackerData.APBoost + attackerData.FATBoost} (boost)";
        attackBreakdown += $" + {attackRoll} (roll) = AV {av}";

        string attackerNarrative = isHit
            ? GenerateHitNarrative(sv, hitLocation, attackerData.WeaponName ?? "weapon")
            : GenerateMissNarrative();

        string defenderDetails = $"{attackBreakdown}\n{defenseBreakdown}\nSV = {av} - {tv} = {sv}";
        if (isHit)
        {
            if (attackerData.WeaponSVModifier != 0)
            {
                int effectiveSVDisplay = sv + attackerData.WeaponSVModifier;
                defenderDetails += $" + {attackerData.WeaponSVModifier} (weapon) = {effectiveSVDisplay}";
            }
            defenderDetails += $"\nHit Location: {hitLocation}\nDamage: {fatDamage} FAT, {vitDamage} VIT";
        }

        return new TargetingResolutionData
        {
            AttackerAV = av,
            DefenderTV = tv,
            DiceRoll = attackRoll,
            SuccessValue = sv,
            IsHit = isHit,
            HitLocation = hitLocation,
            DamageClass = damageClass,
            FATDamage = fatDamage,
            VITDamage = vitDamage,
            AttackerNarrative = attackerNarrative,
            DefenderDetails = defenderDetails,
            AttackBreakdown = attackBreakdown,
            DefenseBreakdown = defenseBreakdown
        };
    }

    private TargetingResolutionData ResolveMedicalAction(
        TargetingAttackerData attackerData,
        IDiceRoller diceRoller)
    {
        // Medical actions resolve against a fixed TV (typically 8)
        const int medicalTV = 8;

        int avBase = attackerData.SkillAS;
        avBase += attackerData.APBoost + attackerData.FATBoost;

        int roll = diceRoller.Roll4dFPlus();
        int av = avBase + roll;
        int sv = av - medicalTV;
        bool isSuccess = sv >= 0;

        string attackBreakdown = $"Medical: {attackerData.SkillAS} (skill)";
        if (attackerData.APBoost + attackerData.FATBoost > 0)
            attackBreakdown += $" + {attackerData.APBoost + attackerData.FATBoost} (boost)";
        attackBreakdown += $" + {roll} (roll) = {av} vs TV {medicalTV}";

        string narrative = isSuccess
            ? $"Successfully treated with {attackerData.SkillName}."
            : $"Treatment with {attackerData.SkillName} was unsuccessful.";

        return new TargetingResolutionData
        {
            AttackerAV = av,
            DefenderTV = medicalTV,
            DiceRoll = roll,
            SuccessValue = sv,
            IsHit = isSuccess,
            HitLocation = HitLocation.Torso,
            DamageClass = 0,
            FATDamage = 0,
            VITDamage = 0,
            AttackerNarrative = narrative,
            DefenderDetails = $"{attackBreakdown}\nResult: {(isSuccess ? "Success" : "Failure")} (SV {sv})",
            AttackBreakdown = attackBreakdown,
            DefenseBreakdown = $"Medical TV: {medicalTV}"
        };
    }

    private static string GenerateHitNarrative(int sv, HitLocation location, string weaponName)
    {
        string severity = sv switch
        {
            >= 6 => "devastating",
            >= 4 => "solid",
            >= 2 => "clean",
            >= 0 => "glancing",
            _ => "weak"
        };

        string locationText = location switch
        {
            HitLocation.Head => "head",
            HitLocation.Torso => "torso",
            HitLocation.LeftArm => "left arm",
            HitLocation.RightArm => "right arm",
            HitLocation.LeftLeg => "left leg",
            HitLocation.RightLeg => "right leg",
            _ => "body"
        };

        return $"A {severity} hit to the {locationText} with {weaponName}.";
    }

    private static string GenerateMissNarrative()
    {
        return "The attack misses.";
    }

    private void RemoveFromQueues(TargetingInteraction interaction)
    {
        lock (_queueLock)
        {
            // Remove from active if it's the active one
            if (_activeDefenderInteractions.TryGetValue(interaction.DefenderId, out var activeId) &&
                activeId == interaction.InteractionId)
            {
                _activeDefenderInteractions.TryRemove(interaction.DefenderId, out _);
            }

            // Remove from queue if it's in queue
            if (_defenderQueues.TryGetValue(interaction.DefenderId, out var queue))
            {
                var newQueue = new Queue<Guid>(queue.Where(id => id != interaction.InteractionId));
                _defenderQueues[interaction.DefenderId] = newQueue;
            }
        }

        // Remove from interactions dictionary after a delay to allow result retrieval
        _ = Task.Run(async () =>
        {
            await Task.Delay(TimeSpan.FromMinutes(5));
            _interactions.TryRemove(interaction.InteractionId, out _);
        });
    }

    private async Task ProcessNextInQueueAsync(int defenderId)
    {
        TargetingInteraction? nextInteraction = null;

        lock (_queueLock)
        {
            // Check if there's already an active interaction
            if (_activeDefenderInteractions.ContainsKey(defenderId))
            {
                return;
            }

            // Get next from queue
            if (_defenderQueues.TryGetValue(defenderId, out var queue) && queue.Count > 0)
            {
                var nextId = queue.Dequeue();
                if (_interactions.TryGetValue(nextId, out nextInteraction))
                {
                    _activeDefenderInteractions[defenderId] = nextId;
                }
            }
        }

        if (nextInteraction != null)
        {
            _logger.LogInformation("Processing next in queue for defender {DefenderId}: {InteractionId}",
                defenderId, nextInteraction.InteractionId);

            // Re-publish the request for the next interaction
            var message = new TargetingRequestMessage
            {
                InteractionId = nextInteraction.InteractionId,
                TableId = nextInteraction.TableId,
                AttackerId = nextInteraction.AttackerId,
                AttackerName = nextInteraction.AttackerName,
                DefenderId = nextInteraction.DefenderId,
                DefenderName = nextInteraction.DefenderName,
                AttackerData = nextInteraction.AttackerData
            };

            await _publisher.PublishTargetingRequestAsync(message);
        }
    }

    /// <summary>
    /// Marks damage as accepted by the defender.
    /// </summary>
    public async Task AcceptDamageAsync(Guid interactionId)
    {
        if (!_interactions.TryGetValue(interactionId, out var interaction))
        {
            return;
        }

        if (interaction.Resolution == null)
        {
            return;
        }

        _logger.LogInformation("Damage accepted for interaction {InteractionId}", interactionId);

        // Publish updated result with damage accepted
        var message = new TargetingResultMessage
        {
            InteractionId = interactionId,
            TableId = interaction.TableId,
            AttackerId = interaction.AttackerId,
            DefenderId = interaction.DefenderId,
            Resolution = interaction.Resolution,
            DamageAccepted = true
        };

        await _publisher.PublishTargetingResultAsync(message);
    }
}
