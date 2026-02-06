using Threa.Dal.Dto;

namespace GameMechanics.Batch;

/// <summary>
/// Effect configuration captured by BatchEffectAddModal for batch application.
/// </summary>
public record BatchEffectConfig
{
    public string EffectName { get; init; } = "";
    public EffectType EffectType { get; init; }
    public string? Description { get; init; }
    public long? DurationSeconds { get; init; }
    public int? DurationRounds { get; init; }
    public string? BehaviorStateJson { get; init; }
}
