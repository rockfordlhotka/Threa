using System.Collections.Generic;

namespace GameMechanics.Batch;

/// <summary>
/// Result from a batch action operation, with success/failure tracking and summary.
/// </summary>
public class BatchActionResult
{
    /// <summary>
    /// The type of action that was applied.
    /// </summary>
    public BatchActionType ActionType { get; set; }

    /// <summary>
    /// The health pool that was targeted: "FAT" or "VIT".
    /// </summary>
    public string Pool { get; set; } = "";

    /// <summary>
    /// The amount of damage or healing applied per character.
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// IDs of characters that were successfully updated.
    /// </summary>
    public List<int> SuccessIds { get; } = new();

    /// <summary>
    /// Names of characters that were successfully updated.
    /// </summary>
    public List<string> SuccessNames { get; } = new();

    /// <summary>
    /// IDs of characters that failed to update.
    /// </summary>
    public List<int> FailedIds { get; } = new();

    /// <summary>
    /// Error messages for failed character updates.
    /// </summary>
    public List<string> Errors { get; } = new();

    /// <summary>
    /// Total number of characters processed (success + failed).
    /// </summary>
    public int TotalCount => SuccessIds.Count + FailedIds.Count;

    /// <summary>
    /// Whether any characters failed to update.
    /// </summary>
    public bool HasFailures => FailedIds.Count > 0;

    /// <summary>
    /// Whether all characters were successfully updated.
    /// </summary>
    public bool AllSucceeded => FailedIds.Count == 0 && SuccessIds.Count > 0;

    /// <summary>
    /// Human-readable summary of the batch operation result.
    /// </summary>
    public string Summary => HasFailures
        ? $"Applied {Amount} {Pool} {ActionType.ToString().ToLower()} to {SuccessIds.Count} of {TotalCount} characters"
        : $"Applied {Amount} {Pool} {ActionType.ToString().ToLower()} to {SuccessIds.Count} character(s)";
}
