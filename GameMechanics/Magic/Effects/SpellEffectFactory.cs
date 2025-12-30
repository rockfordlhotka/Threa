using System;
using System.Collections.Generic;
using System.Linq;

namespace GameMechanics.Magic.Effects;

/// <summary>
/// Factory for getting the appropriate spell effect handler for a spell.
/// Spell effects can be registered by spell ID or can handle multiple spells.
/// </summary>
public class SpellEffectFactory
{
    private readonly Dictionary<string, ISpellEffect> _effectsBySpellId = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<ISpellEffect> _registeredEffects = [];

    /// <summary>
    /// Creates a new spell effect factory with optional pre-registered effects.
    /// </summary>
    /// <param name="effects">Effects to register.</param>
    public SpellEffectFactory(IEnumerable<ISpellEffect>? effects = null)
    {
        if (effects != null)
        {
            foreach (var effect in effects)
            {
                RegisterEffect(effect);
            }
        }
    }

    /// <summary>
    /// Registers a spell effect handler.
    /// </summary>
    /// <param name="effect">The effect to register.</param>
    public void RegisterEffect(ISpellEffect effect)
    {
        _registeredEffects.Add(effect);
        foreach (var spellId in effect.HandledSpellIds)
        {
            _effectsBySpellId[spellId] = effect;
        }
    }

    /// <summary>
    /// Gets the spell effect handler for a given spell ID.
    /// </summary>
    /// <param name="spellId">The spell skill ID.</param>
    /// <returns>The effect handler, or null if none registered.</returns>
    public ISpellEffect? GetEffect(string spellId)
    {
        return _effectsBySpellId.TryGetValue(spellId, out var effect) ? effect : null;
    }

    /// <summary>
    /// Checks if a spell has a registered effect handler.
    /// </summary>
    /// <param name="spellId">The spell skill ID.</param>
    /// <returns>True if an effect handler exists.</returns>
    public bool HasEffect(string spellId)
    {
        return _effectsBySpellId.ContainsKey(spellId);
    }

    /// <summary>
    /// Gets all registered effect handlers.
    /// </summary>
    public IReadOnlyList<ISpellEffect> RegisteredEffects => _registeredEffects.AsReadOnly();

    /// <summary>
    /// Creates a factory with all standard spell effects pre-registered.
    /// </summary>
    public static SpellEffectFactory CreateWithStandardEffects()
    {
        var factory = new SpellEffectFactory();

        // Register all standard spell effects
        factory.RegisterEffect(new PhysicalDamageSpellEffect());
        factory.RegisterEffect(new EnergyDamageSpellEffect());
        factory.RegisterEffect(new HealingSpellEffect());
        factory.RegisterEffect(new AreaLightSpellEffect());
        factory.RegisterEffect(new WallOfFireSpellEffect());

        return factory;
    }
}
