using System;
using System.Collections.Generic;
using GameMechanics.Effects;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Magic.Resolvers;

/// <summary>
/// Factory for creating spell resolvers based on spell type.
/// </summary>
public class SpellResolverFactory
{
    private readonly Dictionary<SpellType, ISpellResolver> _resolvers;

    /// <summary>
    /// Creates a SpellResolverFactory with the required dependencies.
    /// </summary>
    public SpellResolverFactory(
        EffectManager effectManager,
        ILocationEffectDal? locationEffectDal = null)
    {
        _resolvers = new Dictionary<SpellType, ISpellResolver>
        {
            { SpellType.SelfBuff, new SelfBuffResolver(effectManager) },
            { SpellType.Targeted, new TargetedSpellResolver(effectManager) },
            { SpellType.AreaEffect, new AreaEffectResolver(effectManager) },
            { SpellType.Environmental, new EnvironmentalSpellResolver(effectManager, locationEffectDal) }
        };
    }

    /// <summary>
    /// Gets the resolver for a specific spell type.
    /// </summary>
    /// <param name="spellType">The type of spell.</param>
    /// <returns>The appropriate resolver.</returns>
    /// <exception cref="ArgumentException">If no resolver exists for the spell type.</exception>
    public ISpellResolver GetResolver(SpellType spellType)
    {
        if (_resolvers.TryGetValue(spellType, out var resolver))
        {
            return resolver;
        }

        throw new ArgumentException($"No resolver registered for spell type: {spellType}", nameof(spellType));
    }

    /// <summary>
    /// Registers a custom resolver for a spell type (for extensibility).
    /// </summary>
    /// <param name="spellType">The spell type.</param>
    /// <param name="resolver">The resolver implementation.</param>
    public void RegisterResolver(SpellType spellType, ISpellResolver resolver)
    {
        _resolvers[spellType] = resolver ?? throw new ArgumentNullException(nameof(resolver));
    }
}
