using System.Collections.Generic;
using System.Threading.Tasks;
using GameMechanics.Effects;
using GameMechanics.Magic;
using GameMechanics.Magic.Resolvers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Test;

/// <summary>
/// Unit tests for Phase 2 spell resolvers.
/// </summary>
[TestClass]
public class SpellResolverTests : TestBase
{
    private EffectManager? _effectManager;
    private ILocationEffectDal? _locationEffectDal;

    [TestInitialize]
    public void Setup()
    {
        var provider = InitServices();
        var effectDefinitionDal = provider.GetRequiredService<IEffectDefinitionDal>();
        var characterItemDal = provider.GetRequiredService<ICharacterItemDal>();
        var characterEffectDal = provider.GetRequiredService<ICharacterEffectDal>();
        var itemEffectDal = provider.GetRequiredService<IItemEffectDal>();
        _locationEffectDal = provider.GetRequiredService<ILocationEffectDal>();
        _effectManager = new EffectManager(characterEffectDal, itemEffectDal, effectDefinitionDal);
    }

    #region Test Helpers

    private EffectManager CreateEffectManager()
    {
        return _effectManager!;
    }

    private static SpellDefinition CreateSelfBuffSpell(string skillId = "flame-shield")
    {
        return new SpellDefinition
        {
            SkillId = skillId,
            MagicSchool = MagicSchool.Fire,
            SpellType = SpellType.SelfBuff,
            ManaCost = 2,
            Range = 0,
            DefaultDuration = 10,
            ResistanceType = SpellResistanceType.None,
            EffectDescription = "A protective flame shield."
        };
    }

    private static SpellDefinition CreateTargetedSpell(string skillId = "fire-bolt")
    {
        return new SpellDefinition
        {
            SkillId = skillId,
            MagicSchool = MagicSchool.Fire,
            SpellType = SpellType.Targeted,
            ManaCost = 1,
            Range = 2,
            ResistanceType = SpellResistanceType.Fixed,
            FixedResistanceTV = 8,
            EffectDescription = "A bolt of fire."
        };
    }

    private static SpellDefinition CreateAreaSpell(string skillId = "fireball")
    {
        return new SpellDefinition
        {
            SkillId = skillId,
            MagicSchool = MagicSchool.Fire,
            SpellType = SpellType.AreaEffect,
            ManaCost = 3,
            Range = 3,
            AreaRadius = 2,
            ResistanceType = SpellResistanceType.Fixed,
            FixedResistanceTV = 8,
            EffectDescription = "An explosive ball of fire."
        };
    }

    private static SpellDefinition CreateEnvironmentalSpell(string skillId = "wall-of-fire")
    {
        return new SpellDefinition
        {
            SkillId = skillId,
            MagicSchool = MagicSchool.Fire,
            SpellType = SpellType.Environmental,
            ManaCost = 4,
            Range = 2,
            DefaultDuration = 5,
            ResistanceType = SpellResistanceType.Fixed,
            FixedResistanceTV = 8,
            EffectDescription = "A wall of flames."
        };
    }

    #endregion

    #region SelfBuffResolver Tests

    [TestMethod]
    public void SelfBuffResolver_SpellType_IsSelfBuff()
    {
        var effectManager = CreateEffectManager();
        var resolver = new SelfBuffResolver(effectManager);

        Assert.AreEqual(SpellType.SelfBuff, resolver.SpellType);
    }

    [TestMethod]
    public void SelfBuffResolver_ValidateRequest_AlwaysValid()
    {
        var effectManager = CreateEffectManager();
        var resolver = new SelfBuffResolver(effectManager);
        var spell = CreateSelfBuffSpell();
        var request = new SpellCastRequest { CasterId = 1, SpellSkillId = "flame-shield" };

        var result = resolver.ValidateRequest(request, spell);

        Assert.IsTrue(result.IsValid);
        Assert.AreEqual(0, result.Errors.Count);
    }

    [TestMethod]
    public async Task SelfBuffResolver_AlwaysSucceeds()
    {
        var effectManager = CreateEffectManager();
        var resolver = new SelfBuffResolver(effectManager);
        var spell = CreateSelfBuffSpell();
        var request = new SpellCastRequest { CasterId = 1, SpellSkillId = "flame-shield" };
        var context = new SpellResolutionContext
        {
            Request = request,
            Spell = spell,
            CasterAV = 5,
            Roll = 3,
            ManaCost = 2
        };

        var result = await resolver.ResolveAsync(context);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.TargetResults.Count);
        Assert.IsTrue(result.TargetResults[0].Success);
        Assert.AreEqual(1, result.TargetResults[0].TargetCharacterId); // Targets self
    }

    [TestMethod]
    public async Task SelfBuffResolver_SV_EqualsAV_NoResistance()
    {
        var effectManager = CreateEffectManager();
        var resolver = new SelfBuffResolver(effectManager);
        var spell = CreateSelfBuffSpell();
        var request = new SpellCastRequest { CasterId = 1 };
        var context = new SpellResolutionContext
        {
            Request = request,
            Spell = spell,
            CasterAV = 10,
            Roll = 4,
            ManaCost = 2
        };

        var result = await resolver.ResolveAsync(context);

        Assert.AreEqual(10, result.TargetResults[0].SV);
        Assert.AreEqual(0, result.TargetResults[0].TV);
    }

    #endregion

    #region TargetedSpellResolver Tests

    [TestMethod]
    public void TargetedSpellResolver_SpellType_IsTargeted()
    {
        var effectManager = CreateEffectManager();
        var resolver = new TargetedSpellResolver(effectManager);

        Assert.AreEqual(SpellType.Targeted, resolver.SpellType);
    }

    [TestMethod]
    public void TargetedSpellResolver_ValidateRequest_RequiresTarget()
    {
        var effectManager = CreateEffectManager();
        var resolver = new TargetedSpellResolver(effectManager);
        var spell = CreateTargetedSpell();
        var request = new SpellCastRequest { CasterId = 1, SpellSkillId = "fire-bolt" };

        var result = resolver.ValidateRequest(request, spell);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors[0].Contains("target"));
    }

    [TestMethod]
    public void TargetedSpellResolver_ValidateRequest_ValidWithCharacterTarget()
    {
        var effectManager = CreateEffectManager();
        var resolver = new TargetedSpellResolver(effectManager);
        var spell = CreateTargetedSpell();
        var request = new SpellCastRequest
        {
            CasterId = 1,
            SpellSkillId = "fire-bolt",
            TargetCharacterId = 2
        };

        var result = resolver.ValidateRequest(request, spell);

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task TargetedSpellResolver_HitsWhenAV_GreaterThanTV()
    {
        var effectManager = CreateEffectManager();
        var resolver = new TargetedSpellResolver(effectManager);
        var spell = CreateTargetedSpell(); // Fixed TV = 8
        var request = new SpellCastRequest
        {
            CasterId = 1,
            TargetCharacterId = 2
        };
        var context = new SpellResolutionContext
        {
            Request = request,
            Spell = spell,
            CasterAV = 12, // > 8
            Roll = 4,
            ManaCost = 1
        };

        var result = await resolver.ResolveAsync(context);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.TargetResults[0].Success);
        Assert.AreEqual(12, result.TargetResults[0].AV);
        Assert.AreEqual(8, result.TargetResults[0].TV);
        Assert.AreEqual(4, result.TargetResults[0].SV);
    }

    [TestMethod]
    public async Task TargetedSpellResolver_MissesWhenAV_LessThanTV()
    {
        var effectManager = CreateEffectManager();
        var resolver = new TargetedSpellResolver(effectManager);
        var spell = CreateTargetedSpell(); // Fixed TV = 8
        var request = new SpellCastRequest
        {
            CasterId = 1,
            TargetCharacterId = 2
        };
        var context = new SpellResolutionContext
        {
            Request = request,
            Spell = spell,
            CasterAV = 5, // < 8
            Roll = 0,
            ManaCost = 1
        };

        var result = await resolver.ResolveAsync(context);

        Assert.IsFalse(result.Success);
        Assert.IsFalse(result.TargetResults[0].Success);
        Assert.AreEqual(-3, result.TargetResults[0].SV);
    }

    [TestMethod]
    public async Task TargetedSpellResolver_UsesWillpowerResistance()
    {
        var effectManager = CreateEffectManager();
        var resolver = new TargetedSpellResolver(effectManager);
        var spell = new SpellDefinition
        {
            SkillId = "blind",
            MagicSchool = MagicSchool.Light,
            SpellType = SpellType.Targeted,
            ManaCost = 2,
            ResistanceType = SpellResistanceType.Willpower
        };
        var request = new SpellCastRequest
        {
            CasterId = 1,
            TargetCharacterId = 2,
            TargetDefenseValue = 10 // High willpower
        };
        var context = new SpellResolutionContext
        {
            Request = request,
            Spell = spell,
            CasterAV = 12,
            Roll = 4,
            ManaCost = 2
        };

        var result = await resolver.ResolveAsync(context);

        Assert.AreEqual(10, result.TargetResults[0].TV);
        Assert.AreEqual(2, result.TargetResults[0].SV);
    }

    #endregion

    #region AreaEffectResolver Tests

    [TestMethod]
    public void AreaEffectResolver_SpellType_IsAreaEffect()
    {
        var effectManager = CreateEffectManager();
        var resolver = new AreaEffectResolver(effectManager);

        Assert.AreEqual(SpellType.AreaEffect, resolver.SpellType);
    }

    [TestMethod]
    public void AreaEffectResolver_ValidateRequest_RequiresTargets()
    {
        var effectManager = CreateEffectManager();
        var resolver = new AreaEffectResolver(effectManager);
        var spell = CreateAreaSpell();
        var request = new SpellCastRequest { CasterId = 1, SpellSkillId = "fireball" };

        var result = resolver.ValidateRequest(request, spell);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors[0].Contains("target"));
    }

    [TestMethod]
    public void AreaEffectResolver_ValidateRequest_ValidWithTargetList()
    {
        var effectManager = CreateEffectManager();
        var resolver = new AreaEffectResolver(effectManager);
        var spell = CreateAreaSpell();
        var request = new SpellCastRequest
        {
            CasterId = 1,
            SpellSkillId = "fireball",
            TargetCharacterIds = new List<int> { 2, 3, 4 }
        };

        var result = resolver.ValidateRequest(request, spell);

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task AreaEffectResolver_AllTargetsHit_WhenAVBeatsAllDefenses()
    {
        var effectManager = CreateEffectManager();
        var resolver = new AreaEffectResolver(effectManager);
        var spell = CreateAreaSpell(); // Fixed TV = 8
        var request = new SpellCastRequest
        {
            CasterId = 1,
            TargetCharacterIds = new List<int> { 2, 3, 4 }
        };
        var context = new SpellResolutionContext
        {
            Request = request,
            Spell = spell,
            CasterAV = 12, // Beats TV 8 for all
            Roll = 4,
            ManaCost = 3
        };

        var result = await resolver.ResolveAsync(context);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.TargetResults.Count);
        Assert.IsTrue(result.TargetResults.TrueForAll(r => r.Success));
    }

    [TestMethod]
    public async Task AreaEffectResolver_SomeTargetsResist_WithVariedDefenses()
    {
        var effectManager = CreateEffectManager();
        var resolver = new AreaEffectResolver(effectManager);
        var spell = new SpellDefinition
        {
            SkillId = "fireball",
            MagicSchool = MagicSchool.Fire,
            SpellType = SpellType.AreaEffect,
            ManaCost = 3,
            ResistanceType = SpellResistanceType.Willpower // Uses individual defenses
        };
        var request = new SpellCastRequest
        {
            CasterId = 1,
            TargetCharacterIds = new List<int> { 2, 3, 4 },
            TargetDefenseValues = new Dictionary<int, int>
            {
                { 2, 6 },  // Low defense - will be hit
                { 3, 10 }, // Medium defense - will be hit
                { 4, 15 }  // High defense - will resist
            }
        };
        var context = new SpellResolutionContext
        {
            Request = request,
            Spell = spell,
            CasterAV = 12,
            Roll = 4,
            ManaCost = 3
        };

        var result = await resolver.ResolveAsync(context);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.TargetResults.Count);

        // Target 2 (TV 6) should be hit
        Assert.IsTrue(result.TargetResults[0].Success);
        Assert.AreEqual(6, result.TargetResults[0].SV);

        // Target 3 (TV 10) should be hit
        Assert.IsTrue(result.TargetResults[1].Success);
        Assert.AreEqual(2, result.TargetResults[1].SV);

        // Target 4 (TV 15) should resist
        Assert.IsFalse(result.TargetResults[2].Success);
        Assert.AreEqual(-3, result.TargetResults[2].SV);
    }

    [TestMethod]
    public async Task AreaEffectResolver_NoTargetsHit_AllResist()
    {
        var effectManager = CreateEffectManager();
        var resolver = new AreaEffectResolver(effectManager);
        var spell = CreateAreaSpell(); // Fixed TV = 8
        var request = new SpellCastRequest
        {
            CasterId = 1,
            TargetCharacterIds = new List<int> { 2, 3 }
        };
        var context = new SpellResolutionContext
        {
            Request = request,
            Spell = spell,
            CasterAV = 5, // Below TV 8
            Roll = -2,
            ManaCost = 3
        };

        var result = await resolver.ResolveAsync(context);

        Assert.IsFalse(result.Success);
        Assert.AreEqual(2, result.TargetResults.Count);
        Assert.IsTrue(result.TargetResults.TrueForAll(r => !r.Success));
    }

    #endregion

    #region EnvironmentalSpellResolver Tests

    [TestMethod]
    public void EnvironmentalSpellResolver_SpellType_IsEnvironmental()
    {
        var effectManager = CreateEffectManager();
        var resolver = new EnvironmentalSpellResolver(effectManager);

        Assert.AreEqual(SpellType.Environmental, resolver.SpellType);
    }

    [TestMethod]
    public void EnvironmentalSpellResolver_ValidateRequest_RequiresLocation()
    {
        var effectManager = CreateEffectManager();
        var resolver = new EnvironmentalSpellResolver(effectManager);
        var spell = CreateEnvironmentalSpell();
        var request = new SpellCastRequest { CasterId = 1, SpellSkillId = "wall-of-fire" };

        var result = resolver.ValidateRequest(request, spell);

        Assert.IsFalse(result.IsValid);
        Assert.IsTrue(result.Errors[0].Contains("location"));
    }

    [TestMethod]
    public void EnvironmentalSpellResolver_ValidateRequest_ValidWithLocation()
    {
        var effectManager = CreateEffectManager();
        var resolver = new EnvironmentalSpellResolver(effectManager);
        var spell = CreateEnvironmentalSpell();
        var request = new SpellCastRequest
        {
            CasterId = 1,
            SpellSkillId = "wall-of-fire",
            TargetLocation = "The doorway"
        };

        var result = resolver.ValidateRequest(request, spell);

        Assert.IsTrue(result.IsValid);
    }

    [TestMethod]
    public async Task EnvironmentalSpellResolver_AlwaysSucceeds()
    {
        var effectManager = CreateEffectManager();
        var resolver = new EnvironmentalSpellResolver(effectManager);
        var spell = CreateEnvironmentalSpell();
        var request = new SpellCastRequest
        {
            CasterId = 1,
            TargetLocation = "The doorway",
            CampaignId = 1
        };
        var context = new SpellResolutionContext
        {
            Request = request,
            Spell = spell,
            CasterAV = 5,
            Roll = 2,
            ManaCost = 4
        };

        var result = await resolver.ResolveAsync(context);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.AffectedLocation);
        Assert.AreEqual("The doorway", result.AffectedLocation.Name);
    }

    [TestMethod]
    public async Task EnvironmentalSpellResolver_CreatesLocationWithDuration()
    {
        var effectManager = CreateEffectManager();
        var resolver = new EnvironmentalSpellResolver(effectManager, _locationEffectDal!);
        var spell = CreateEnvironmentalSpell(); // Base duration = 5
        var request = new SpellCastRequest
        {
            CasterId = 1,
            TargetLocation = "Center of the room",
            CampaignId = 1
        };
        var context = new SpellResolutionContext
        {
            Request = request,
            Spell = spell,
            CasterAV = 10, // High SV should add bonus rounds
            Roll = 4,
            ManaCost = 4
        };

        var result = await resolver.ResolveAsync(context);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.AffectedLocation);

        // Verify the location effect was created with duration
        var effects = await _locationEffectDal!.GetActiveEffectsAtLocationAsync(result.AffectedLocation.Id);
        Assert.AreEqual(1, effects.Count);
        Assert.IsTrue(effects[0].RoundsRemaining >= 5); // At least base duration
    }

    [TestMethod]
    public async Task EnvironmentalSpellResolver_AffectsCharactersAtLocation()
    {
        var effectManager = CreateEffectManager();
        var resolver = new EnvironmentalSpellResolver(effectManager);
        // Use Willpower resistance so individual defenses are used
        var spell = new SpellDefinition
        {
            SkillId = "fear-zone",
            MagicSchool = MagicSchool.Light,
            SpellType = SpellType.Environmental,
            ManaCost = 4,
            Range = 2,
            DefaultDuration = 5,
            ResistanceType = SpellResistanceType.Willpower,
            EffectDescription = "An area of supernatural fear."
        };
        var request = new SpellCastRequest
        {
            CasterId = 1,
            TargetLocation = "The hallway",
            TargetCharacterIds = new List<int> { 2, 3 }, // Characters in the area
            TargetDefenseValues = new Dictionary<int, int>
            {
                { 2, 6 },  // Low willpower - will be affected
                { 3, 12 }  // High willpower - will resist
            },
            CampaignId = 1
        };
        var context = new SpellResolutionContext
        {
            Request = request,
            Spell = spell,
            CasterAV = 10,
            Roll = 4,
            ManaCost = 4
        };

        var result = await resolver.ResolveAsync(context);

        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.AffectedLocation);
        Assert.AreEqual(2, result.TargetResults.Count);

        // First target (low defense) should be affected
        Assert.IsTrue(result.TargetResults[0].Success);

        // Second target (high defense) should resist
        Assert.IsFalse(result.TargetResults[1].Success);
    }

    #endregion

    #region SpellResolverFactory Tests

    [TestMethod]
    public void SpellResolverFactory_ReturnsCorrectResolver_ForEachType()
    {
        var effectManager = CreateEffectManager();
        var factory = new SpellResolverFactory(effectManager);

        Assert.IsInstanceOfType(factory.GetResolver(SpellType.SelfBuff), typeof(SelfBuffResolver));
        Assert.IsInstanceOfType(factory.GetResolver(SpellType.Targeted), typeof(TargetedSpellResolver));
        Assert.IsInstanceOfType(factory.GetResolver(SpellType.AreaEffect), typeof(AreaEffectResolver));
        Assert.IsInstanceOfType(factory.GetResolver(SpellType.Environmental), typeof(EnvironmentalSpellResolver));
    }

    [TestMethod]
    public void SpellResolverFactory_RegisterResolver_OverridesDefault()
    {
        var effectManager = CreateEffectManager();
        var factory = new SpellResolverFactory(effectManager);
        var customResolver = new SelfBuffResolver(effectManager);

        factory.RegisterResolver(SpellType.Targeted, customResolver);

        // Now Targeted returns our custom (wrong type but valid for test)
        var resolver = factory.GetResolver(SpellType.Targeted);
        Assert.AreSame(customResolver, resolver);
    }

    #endregion
}
