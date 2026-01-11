using System;

namespace GameMechanics.GamePlay;

// Notification DTOs for SignalR communication between server and clients

public record RoundAdvanceResult(
    int NewRound,
    int RoundsAdvanced,
    DateTime Timestamp);

public record TimeSkipResult(
    string Duration,
    bool FullRestoration,
    DateTime Timestamp);

public record CharacterStatusUpdate(
    int CharacterId,
    int FatValue,
    int FatMax,
    int VitValue,
    int VitMax,
    int ActionPoints,
    int ActionPointMax,
    string[] ActiveEffects);

public record DamageNotification(
    int CharacterId,
    int FatDamage,
    int VitDamage,
    string Source,
    string? DamageType);

public record HealingNotification(
    int CharacterId,
    int FatHealing,
    int VitHealing,
    string Source);

public record EffectNotification(
    int CharacterId,
    string EffectId,
    string EffectName,
    int? Duration,
    string Source);

public record ActionResultNotification(
    int CharacterId,
    string ActionType,
    bool Success,
    string Narrative,
    int? AbilityScore,
    int? TargetValue,
    int? SuccessValue);

public record SkillCheckNotification(
    int CharacterId,
    string SkillName,
    int AbilityScore,
    int TargetValue,
    int SuccessValue,
    string Outcome,
    string? Narrative);

public record SpellCastNotification(
    int CasterId,
    string SpellName,
    bool Success,
    int[]? TargetIds,
    string Narrative);

public record PlayerJoinedNotification(
    int PlayerId,
    int CharacterId,
    string CharacterName);

public record PlayerLeftNotification(
    int PlayerId,
    int CharacterId,
    string CharacterName);
