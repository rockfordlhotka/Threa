namespace GameMechanics.Batch;

/// <summary>
/// Result returned from BatchDamageHealingModal when user confirms.
/// </summary>
public record BatchInputResult(int Amount, string Pool);
