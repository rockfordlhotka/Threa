using System;

namespace Threa.Dal.Dto;

/// <summary>
/// Represents a request from a player to join their character to a table.
/// </summary>
public class JoinRequest
{
    /// <summary>
    /// Unique identifier for this join request.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The character being requested to join.
    /// </summary>
    public int CharacterId { get; set; }

    /// <summary>
    /// The table the character wants to join.
    /// </summary>
    public Guid TableId { get; set; }

    /// <summary>
    /// The player who owns the character making the request.
    /// </summary>
    public int PlayerId { get; set; }

    /// <summary>
    /// Current status of the join request.
    /// </summary>
    public JoinRequestStatus Status { get; set; }

    /// <summary>
    /// When the request was submitted.
    /// </summary>
    public DateTime RequestedAt { get; set; }

    /// <summary>
    /// When the request was approved or denied (null if still pending).
    /// </summary>
    public DateTime? ProcessedAt { get; set; }

    /// <summary>
    /// Reason for denial if status is Denied (null otherwise).
    /// </summary>
    public string? DenialReason { get; set; }
}

/// <summary>
/// Status of a join request.
/// </summary>
public enum JoinRequestStatus
{
    /// <summary>Request is awaiting GM review.</summary>
    Pending = 0,
    /// <summary>Request was approved and character was added to table.</summary>
    Approved = 1,
    /// <summary>Request was denied by GM.</summary>
    Denied = 2
}
