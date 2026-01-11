using System.Collections.Generic;
using Threa.Dal.Dto;

namespace GameMechanics.Effects;

/// <summary>
/// Defines the behavior for a specific type of effect.
/// Implementations handle the lifecycle events and modifier calculations
/// for their effect type.
/// </summary>
public interface IEffectBehavior
{
  /// <summary>
  /// The effect type this behavior handles.
  /// </summary>
  EffectType EffectType { get; }

  /// <summary>
  /// Called when an effect is about to be added to a character.
  /// Allows the behavior to check for stacking, immunity, or other conditions.
  /// </summary>
  /// <param name="effect">The effect being added.</param>
  /// <param name="character">The character receiving the effect.</param>
  /// <returns>Result indicating how the add should proceed.</returns>
  EffectAddResult OnAdding(EffectRecord effect, CharacterEdit character);

  /// <summary>
  /// Called immediately after an effect is added to a character.
  /// Use for one-time effects that happen on application.
  /// </summary>
  /// <param name="effect">The effect that was added.</param>
  /// <param name="character">The character with the effect.</param>
  void OnApply(EffectRecord effect, CharacterEdit character);

  /// <summary>
  /// Called each round while the effect is active.
  /// Use for periodic damage, healing, or other recurring effects.
  /// </summary>
  /// <param name="effect">The active effect.</param>
  /// <param name="character">The character with the effect.</param>
  /// <returns>Result indicating if the effect should continue or expire early.</returns>
  EffectTickResult OnTick(EffectRecord effect, CharacterEdit character);

  /// <summary>
  /// Called when an effect's duration expires naturally.
  /// Use for effects that do something when they wear off.
  /// </summary>
  /// <param name="effect">The expiring effect.</param>
  /// <param name="character">The character losing the effect.</param>
  void OnExpire(EffectRecord effect, CharacterEdit character);

  /// <summary>
  /// Called when an effect is removed before expiration (dispelled, cured, etc.).
  /// </summary>
  /// <param name="effect">The effect being removed.</param>
  /// <param name="character">The character losing the effect.</param>
  void OnRemove(EffectRecord effect, CharacterEdit character);

  /// <summary>
  /// Gets attribute modifiers from this effect.
  /// Called whenever an attribute value is queried.
  /// </summary>
  /// <param name="effect">The active effect.</param>
  /// <param name="attributeName">The attribute being queried.</param>
  /// <param name="baseValue">The base attribute value before this effect.</param>
  /// <returns>Modifiers to apply to the attribute, or empty if none.</returns>
  IEnumerable<EffectModifier> GetAttributeModifiers(EffectRecord effect, string attributeName, int baseValue);

  /// <summary>
  /// Gets ability score modifiers from this effect.
  /// Called during AS calculation for any skill check.
  /// </summary>
  /// <param name="effect">The active effect.</param>
  /// <param name="skillName">The skill being used.</param>
  /// <param name="attributeName">The attribute(s) being used.</param>
  /// <param name="currentAS">The current AS value before this effect.</param>
  /// <returns>Modifiers to apply to the ability score, or empty if none.</returns>
  IEnumerable<EffectModifier> GetAbilityScoreModifiers(EffectRecord effect, string skillName, string attributeName, int currentAS);

  /// <summary>
  /// Gets success value modifiers from this effect.
  /// Called after AS-TV calculation to determine final SV.
  /// </summary>
  /// <param name="effect">The active effect.</param>
  /// <param name="actionType">The type of action (attack, defend, skill check, etc.).</param>
  /// <param name="currentSV">The current SV before this effect.</param>
  /// <returns>Modifiers to apply to the success value, or empty if none.</returns>
  IEnumerable<EffectModifier> GetSuccessValueModifiers(EffectRecord effect, string actionType, int currentSV);
}
