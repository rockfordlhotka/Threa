namespace Threa.Client.Components.Pages.GamePlay;

/// <summary>
/// Data returned when a medical treatment is initiated.
/// </summary>
public class MedicalCompleteData
{
    /// <summary>
    /// Message to log.
    /// </summary>
    public string Message { get; set; } = "";

    /// <summary>
    /// Whether the skill check succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Amount of healing that will be applied when concentration completes.
    /// </summary>
    public int HealingAmount { get; set; }

    /// <summary>
    /// Number of rounds of concentration required.
    /// </summary>
    public int ConcentrationRounds { get; set; }
}
