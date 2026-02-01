using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameMechanics.Messaging;

/// <summary>
/// Interface for managing targeting interactions between attackers and defenders.
/// </summary>
public interface ITargetingInteractionManager
{
    /// <summary>
    /// Starts a new targeting interaction.
    /// </summary>
    Task<(TargetingInteraction Interaction, int QueuePosition)> StartInteractionAsync(
        Guid tableId,
        int attackerId,
        string attackerName,
        int defenderId,
        string defenderName,
        TargetingAttackerData attackerData);

    /// <summary>
    /// Gets an interaction by ID.
    /// </summary>
    TargetingInteraction? GetInteraction(Guid interactionId);

    /// <summary>
    /// Gets the active interaction for a defender.
    /// </summary>
    TargetingInteraction? GetActiveInteractionForDefender(int defenderId);

    /// <summary>
    /// Gets the active interaction where the character is the attacker.
    /// </summary>
    TargetingInteraction? GetActiveInteractionForAttacker(int attackerId);

    /// <summary>
    /// Gets all pending interactions for a defender (including queued).
    /// </summary>
    IReadOnlyList<TargetingInteraction> GetPendingInteractionsForDefender(int defenderId);

    /// <summary>
    /// Updates attacker data for an interaction.
    /// </summary>
    Task UpdateAttackerDataAsync(Guid interactionId, TargetingAttackerData data, bool isConfirmed = false);

    /// <summary>
    /// Updates defender data for an interaction.
    /// </summary>
    Task UpdateDefenderDataAsync(Guid interactionId, TargetingDefenderData data, bool isConfirmed = false);

    /// <summary>
    /// Marks the attacker as confirmed.
    /// </summary>
    Task ConfirmAttackerAsync(Guid interactionId);

    /// <summary>
    /// Marks the defender as confirmed.
    /// </summary>
    Task ConfirmDefenderAsync(Guid interactionId);

    /// <summary>
    /// Cancels a targeting interaction.
    /// </summary>
    Task CancelAsync(Guid interactionId, string? reason = null);

    /// <summary>
    /// Resolves the targeting interaction and calculates results.
    /// </summary>
    Task<TargetingResolutionData?> ResolveInteractionAsync(Guid interactionId);

    /// <summary>
    /// Marks damage as accepted by the defender.
    /// </summary>
    Task AcceptDamageAsync(Guid interactionId);
}
