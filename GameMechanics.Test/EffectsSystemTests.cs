using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameMechanics.Effects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Threa.Dal;
using Threa.Dal.Dto;
using Threa.Dal.MockDb;

namespace GameMechanics.Test
{
    [TestClass]
    public class EffectsSystemTests
    {
        private IEffectDefinitionDal _definitionDal;
        private ICharacterEffectDal _characterEffectDal;
        private IItemEffectDal _itemEffectDal;
        private ICharacterItemDal _itemDal;
        private EffectCalculator _calculator;

        [TestInitialize]
        public void Setup()
        {
            _definitionDal = new EffectDefinitionDal();
            _itemDal = new CharacterItemDal();
            _characterEffectDal = new CharacterEffectDal(_definitionDal);
            _itemEffectDal = new ItemEffectDal(_definitionDal, _itemDal);
            _calculator = new EffectCalculator();
        }

        #region Effect Definition Tests

        [TestMethod]
        public async Task GetAllDefinitions_ReturnsPresetEffects()
        {
            var definitions = await _definitionDal.GetAllDefinitionsAsync();

            Assert.IsTrue(definitions.Count > 0, "Should return preset effects");
            Assert.IsTrue(definitions.Any(d => d.Name == "Wound"), "Should contain Wound effect");
            Assert.IsTrue(definitions.Any(d => d.Name == "Stunned"), "Should contain Stunned effect");
            Assert.IsTrue(definitions.Any(d => d.Name == "Strength Boost"), "Should contain Strength Boost effect");
        }

        [TestMethod]
        public async Task GetDefinitionByName_ReturnsCorrectEffect()
        {
            var wound = await _definitionDal.GetDefinitionByNameAsync("Wound");

            Assert.IsNotNull(wound);
            Assert.AreEqual(EffectType.Wound, wound.EffectType);
            Assert.AreEqual(DurationType.UntilRemoved, wound.DurationType);
            Assert.IsTrue(wound.IsStackable);
        }

        [TestMethod]
        public async Task GetDefinitionsByType_FiltersByType()
        {
            var buffs = await _definitionDal.GetDefinitionsByTypeAsync(EffectType.Buff);

            Assert.IsTrue(buffs.Count > 0, "Should return buff effects");
            Assert.IsTrue(buffs.All(b => b.EffectType == EffectType.Buff), "All should be Buff type");
        }

        #endregion

        #region Character Effect Tests

        [TestMethod]
        public async Task ApplyEffect_CreatesNewEffect()
        {
            var strengthBoost = await _definitionDal.GetDefinitionByNameAsync("Strength Boost");
            Assert.IsNotNull(strengthBoost);

            var effect = new CharacterEffect
            {
                CharacterId = 1,
                EffectDefinitionId = strengthBoost.Id,
                RoundsRemaining = 10
            };

            var applied = await _characterEffectDal.ApplyEffectAsync(effect);

            Assert.AreNotEqual(Guid.Empty, applied.Id);
            Assert.AreEqual(1, applied.CharacterId);
            Assert.AreEqual(strengthBoost.Id, applied.EffectDefinitionId);
        }

        [TestMethod]
        public async Task GetCharacterEffects_ReturnsAppliedEffects()
        {
            // Clear existing effects
            var existingEffects = await _characterEffectDal.GetCharacterEffectsAsync(2);
            foreach (var e in existingEffects)
            {
                await _characterEffectDal.RemoveEffectAsync(e.Id);
            }

            var strengthBoost = await _definitionDal.GetDefinitionByNameAsync("Strength Boost");
            Assert.IsNotNull(strengthBoost);

            var effect = new CharacterEffect
            {
                CharacterId = 2,
                EffectDefinitionId = strengthBoost.Id
            };
            await _characterEffectDal.ApplyEffectAsync(effect);

            var effects = await _characterEffectDal.GetCharacterEffectsAsync(2);

            Assert.AreEqual(1, effects.Count);
            Assert.IsNotNull(effects[0].Definition);
            Assert.AreEqual("Strength Boost", effects[0].Definition!.Name);
        }

        [TestMethod]
        public async Task HasEffect_ReturnsTrueWhenPresent()
        {
            // Clear existing effects
            var existingEffects = await _characterEffectDal.GetCharacterEffectsAsync(3);
            foreach (var e in existingEffects)
            {
                await _characterEffectDal.RemoveEffectAsync(e.Id);
            }

            var stunned = await _definitionDal.GetDefinitionByNameAsync("Stunned");
            Assert.IsNotNull(stunned);

            await _characterEffectDal.ApplyEffectAsync(new CharacterEffect
            {
                CharacterId = 3,
                EffectDefinitionId = stunned.Id
            });

            Assert.IsTrue(await _characterEffectDal.HasEffectAsync(3, "Stunned"));
            Assert.IsFalse(await _characterEffectDal.HasEffectAsync(3, "Invisible"));
        }

        [TestMethod]
        public async Task RemoveEffect_RemovesFromCharacter()
        {
            var strengthBoost = await _definitionDal.GetDefinitionByNameAsync("Strength Boost");
            Assert.IsNotNull(strengthBoost);

            var effect = new CharacterEffect
            {
                CharacterId = 4,
                EffectDefinitionId = strengthBoost.Id
            };
            var applied = await _characterEffectDal.ApplyEffectAsync(effect);

            await _characterEffectDal.RemoveEffectAsync(applied.Id);

            var retrieved = await _characterEffectDal.GetEffectAsync(applied.Id);
            Assert.IsNull(retrieved);
        }

        [TestMethod]
        public async Task StackBehavior_Replace_ReplacesExistingEffect()
        {
            // Clear existing effects
            var existingEffects = await _characterEffectDal.GetCharacterEffectsAsync(5);
            foreach (var e in existingEffects)
            {
                await _characterEffectDal.RemoveEffectAsync(e.Id);
            }

            // Strength Boost has Replace behavior
            var strengthBoost = await _definitionDal.GetDefinitionByNameAsync("Strength Boost");
            Assert.IsNotNull(strengthBoost);
            Assert.AreEqual(StackBehavior.Replace, strengthBoost.StackBehavior);

            // Apply twice
            await _characterEffectDal.ApplyEffectAsync(new CharacterEffect
            {
                CharacterId = 5,
                EffectDefinitionId = strengthBoost.Id,
                RoundsRemaining = 5
            });

            await _characterEffectDal.ApplyEffectAsync(new CharacterEffect
            {
                CharacterId = 5,
                EffectDefinitionId = strengthBoost.Id,
                RoundsRemaining = 10
            });

            var effects = await _characterEffectDal.GetEffectsByNameAsync(5, "Strength Boost");

            // Should only be one
            Assert.AreEqual(1, effects.Count);
        }

        [TestMethod]
        public async Task StackBehavior_Intensify_IncreaseStacks()
        {
            // Clear existing effects
            var existingEffects = await _characterEffectDal.GetCharacterEffectsAsync(6);
            foreach (var e in existingEffects)
            {
                await _characterEffectDal.RemoveEffectAsync(e.Id);
            }

            // Weak Poison has Intensify behavior
            var weakPoison = await _definitionDal.GetDefinitionByNameAsync("Weak Poison");
            Assert.IsNotNull(weakPoison);
            Assert.AreEqual(StackBehavior.Intensify, weakPoison.StackBehavior);

            // Apply once
            await _characterEffectDal.ApplyEffectAsync(new CharacterEffect
            {
                CharacterId = 6,
                EffectDefinitionId = weakPoison.Id
            });

            // Apply again - should intensify
            var intensified = await _characterEffectDal.ApplyEffectAsync(new CharacterEffect
            {
                CharacterId = 6,
                EffectDefinitionId = weakPoison.Id
            });

            Assert.AreEqual(2, intensified.CurrentStacks);
        }

        [TestMethod]
        public async Task ProcessEndOfRound_DecrementsRoundsRemaining()
        {
            // Clear existing effects
            var existingEffects = await _characterEffectDal.GetCharacterEffectsAsync(7);
            foreach (var e in existingEffects)
            {
                await _characterEffectDal.RemoveEffectAsync(e.Id);
            }

            var stunned = await _definitionDal.GetDefinitionByNameAsync("Stunned");
            Assert.IsNotNull(stunned);

            await _characterEffectDal.ApplyEffectAsync(new CharacterEffect
            {
                CharacterId = 7,
                EffectDefinitionId = stunned.Id,
                RoundsRemaining = 3
            });

            await _characterEffectDal.ProcessEndOfRoundAsync(7);

            var effects = await _characterEffectDal.GetCharacterEffectsAsync(7);
            Assert.AreEqual(1, effects.Count);
            Assert.AreEqual(2, effects[0].RoundsRemaining);
        }

        [TestMethod]
        public async Task ProcessEndOfRound_RemovesExpiredEffects()
        {
            // Clear existing effects
            var existingEffects = await _characterEffectDal.GetCharacterEffectsAsync(8);
            foreach (var e in existingEffects)
            {
                await _characterEffectDal.RemoveEffectAsync(e.Id);
            }

            var stunned = await _definitionDal.GetDefinitionByNameAsync("Stunned");
            Assert.IsNotNull(stunned);

            await _characterEffectDal.ApplyEffectAsync(new CharacterEffect
            {
                CharacterId = 8,
                EffectDefinitionId = stunned.Id,
                RoundsRemaining = 1
            });

            var expired = await _characterEffectDal.ProcessEndOfRoundAsync(8);

            Assert.AreEqual(1, expired.Count);
            var remainingEffects = await _characterEffectDal.GetCharacterEffectsAsync(8);
            Assert.AreEqual(0, remainingEffects.Count);
        }

        #endregion

        #region Effect Calculator Tests

        [TestMethod]
        public void GetTotalAttributeModifier_CalculatesFromEffects()
        {
            var effects = new List<CharacterEffect>
            {
                CreateEffectWithImpact(
                    "Strength Boost",
                    EffectImpactType.AttributeModifier,
                    "STR",
                    2)
            };

            var modifier = _calculator.GetTotalAttributeModifier(effects, new List<ItemEffect>(), "STR");

            Assert.AreEqual(2, modifier);
        }

        [TestMethod]
        public void GetTotalAttributeModifier_MultiplyByStacks()
        {
            var effects = new List<CharacterEffect>
            {
                CreateEffectWithImpact(
                    "Weakened",
                    EffectImpactType.AttributeModifier,
                    "STR",
                    -2,
                    stacks: 3)
            };

            var modifier = _calculator.GetTotalAttributeModifier(effects, new List<ItemEffect>(), "STR");

            Assert.AreEqual(-6, modifier);
        }

        [TestMethod]
        public void GetTotalASModifier_SumsAllModifiers()
        {
            var effects = new List<CharacterEffect>
            {
                CreateEffectWithImpact("Wound", EffectImpactType.ASModifier, "All", -2),
                CreateEffectWithImpact("Wound", EffectImpactType.ASModifier, "All", -2),
                CreateEffectWithImpact("Battle Focus", EffectImpactType.ASModifier, "All", 2)
            };

            var modifier = _calculator.GetTotalASModifier(effects, new List<ItemEffect>());

            Assert.AreEqual(-2, modifier);
        }

        [TestMethod]
        public void GetTotalTVModifier_CalculatesSelfModifiers()
        {
            var effects = new List<CharacterEffect>
            {
                CreateEffectWithImpact("Magic Shield", EffectImpactType.TVModifier, "Self", 2),
                CreateEffectWithImpact("Invisibility", EffectImpactType.TVModifier, "Self", 4)
            };

            var modifier = _calculator.GetTotalTVModifier(effects, new List<ItemEffect>());

            Assert.AreEqual(6, modifier);
        }

        [TestMethod]
        public void GetDamageOverTime_ReturnsCorrectDamage()
        {
            var effects = new List<CharacterEffect>
            {
                CreateDotEffect("Burning", "FAT", 2, tickReady: true),
                CreateDotEffect("Strong Poison", "VIT", 1, tickReady: true)
            };

            var (fatDamage, vitDamage) = _calculator.GetDamageOverTime(effects);

            Assert.AreEqual(2, fatDamage);
            Assert.AreEqual(1, vitDamage);
        }

        [TestMethod]
        public void GetDamageOverTime_IgnoresNotReadyTicks()
        {
            var effects = new List<CharacterEffect>
            {
                CreateDotEffect("Burning", "FAT", 2, tickReady: false)
            };

            var (fatDamage, vitDamage) = _calculator.GetDamageOverTime(effects);

            Assert.AreEqual(0, fatDamage);
            Assert.AreEqual(0, vitDamage);
        }

        [TestMethod]
        public void HasSpecialAbility_DetectsAbilities()
        {
            var effects = new List<CharacterEffect>
            {
                CreateEffectWithImpact("Invisibility", EffectImpactType.SpecialAbility, "Visibility", 0)
            };

            Assert.IsTrue(_calculator.HasSpecialAbility(effects, new List<ItemEffect>(), "Visibility"));
            Assert.IsFalse(_calculator.HasSpecialAbility(effects, new List<ItemEffect>(), "Flight"));
        }

        [TestMethod]
        public void GetEffectsBrokenByAction_IdentifiesBreakableEffects()
        {
            var invisDef = new EffectDefinition
            {
                Id = 100,
                Name = "Invisibility",
                BreakConditions = "Attack,CastSpell,TakeDamage"
            };

            var effects = new List<CharacterEffect>
            {
                new CharacterEffect
                {
                    Id = Guid.NewGuid(),
                    CharacterId = 1,
                    EffectDefinitionId = 100,
                    Definition = invisDef
                }
            };

            var brokenByAttack = _calculator.GetEffectsBrokenByAction(effects, "Attack");
            var brokenByMove = _calculator.GetEffectsBrokenByAction(effects, "Move");

            Assert.AreEqual(1, brokenByAttack.Count);
            Assert.AreEqual(0, brokenByMove.Count);
        }

        #endregion

        #region Effect Manager Integration Tests

        [TestMethod]
        public async Task EffectManager_ApplyAndCalculateModifiers()
        {
            var manager = new EffectManager(_characterEffectDal, _itemEffectDal, _definitionDal);

            // Clear existing
            var existingEffects = await _characterEffectDal.GetCharacterEffectsAsync(9);
            foreach (var e in existingEffects)
            {
                await _characterEffectDal.RemoveEffectAsync(e.Id);
            }

            // Apply strength boost
            await manager.ApplyEffectAsync(9, "Strength Boost");

            var strModifier = await manager.GetTotalAttributeModifierAsync(9, "STR");
            Assert.AreEqual(2, strModifier);
        }

        [TestMethod]
        public async Task EffectManager_GetEffectSummary()
        {
            var manager = new EffectManager(_characterEffectDal, _itemEffectDal, _definitionDal);

            // Clear existing
            var existingEffects = await _characterEffectDal.GetCharacterEffectsAsync(10);
            foreach (var e in existingEffects)
            {
                await _characterEffectDal.RemoveEffectAsync(e.Id);
            }

            // Apply some effects
            await manager.ApplyEffectAsync(10, "Battle Focus");
            await manager.ApplyEffectAsync(10, "Magic Shield");

            var summary = await manager.GetEffectSummaryAsync(10);

            Assert.AreEqual(2, summary.CharacterEffects.Count);
            Assert.AreEqual(2, summary.TotalAVModifier); // Battle Focus gives +2 AV
            Assert.AreEqual(2, summary.TotalTVModifier); // Magic Shield gives +2 TV
        }

        #endregion

        #region Helper Methods

        private CharacterEffect CreateEffectWithImpact(
            string name,
            EffectImpactType impactType,
            string target,
            decimal value,
            int stacks = 1)
        {
            return new CharacterEffect
            {
                Id = Guid.NewGuid(),
                CharacterId = 1,
                EffectDefinitionId = 1,
                CurrentStacks = stacks,
                Definition = new EffectDefinition
                {
                    Id = 1,
                    Name = name,
                    Impacts = new List<EffectImpact>
                    {
                        new EffectImpact
                        {
                            ImpactType = impactType,
                            Target = target,
                            Value = value
                        }
                    }
                }
            };
        }

        private CharacterEffect CreateDotEffect(string name, string target, decimal value, bool tickReady)
        {
            return new CharacterEffect
            {
                Id = Guid.NewGuid(),
                CharacterId = 1,
                EffectDefinitionId = 1,
                CurrentStacks = 1,
                RoundsUntilTick = tickReady ? 1 : 5,
                Definition = new EffectDefinition
                {
                    Id = 1,
                    Name = name,
                    Impacts = new List<EffectImpact>
                    {
                        new EffectImpact
                        {
                            ImpactType = EffectImpactType.DamageOverTime,
                            Target = target,
                            Value = value,
                            DamageInterval = 1
                        }
                    }
                }
            };
        }

        #endregion
    }
}
