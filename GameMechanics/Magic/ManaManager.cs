using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameMechanics.Actions;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.Magic;

/// <summary>
/// Manages mana pools for characters across all magic schools.
/// Handles mana spending, recovery actions, and pool calculations.
/// </summary>
public class ManaManager
{
    private readonly IManaDal _manaDal;
    private readonly IDiceRoller _diceRoller;

    /// <summary>
    /// Default Target Value for mana recovery skill checks.
    /// </summary>
    public const int DefaultRecoveryTV = 6;

    public ManaManager(IManaDal manaDal, IDiceRoller diceRoller)
    {
        _manaDal = manaDal;
        _diceRoller = diceRoller;
    }

    #region Mana Pool Operations

    /// <summary>
    /// Gets a character's mana pool for a specific magic school.
    /// </summary>
    public Task<CharacterMana?> GetManaPoolAsync(int characterId, MagicSchool school)
        => _manaDal.GetManaPoolAsync(characterId, school);

    /// <summary>
    /// Gets all mana pools for a character.
    /// </summary>
    public Task<List<CharacterMana>> GetAllManaPoolsAsync(int characterId)
        => _manaDal.GetAllManaPoolsAsync(characterId);

    /// <summary>
    /// Checks if a character has sufficient mana in a school.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="school">The magic school.</param>
    /// <param name="amount">The amount of mana needed.</param>
    /// <returns>True if the character has enough mana.</returns>
    public async Task<bool> HasSufficientManaAsync(int characterId, MagicSchool school, int amount)
    {
        var pool = await _manaDal.GetManaPoolAsync(characterId, school);
        return pool != null && pool.CurrentMana >= amount;
    }

    /// <summary>
    /// Spends mana from a character's pool.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="school">The magic school.</param>
    /// <param name="amount">The amount of mana to spend.</param>
    /// <returns>Result of the spend operation.</returns>
    public async Task<SpendManaResult> SpendManaAsync(int characterId, MagicSchool school, int amount)
    {
        var pool = await _manaDal.GetManaPoolAsync(characterId, school);
        
        if (pool == null)
        {
            return new SpendManaResult
            {
                Success = false,
                ErrorMessage = $"Character has no mana pool for {school} magic."
            };
        }

        if (pool.CurrentMana < amount)
        {
            return new SpendManaResult
            {
                Success = false,
                ErrorMessage = $"Insufficient {school} mana. Have {pool.CurrentMana}, need {amount}.",
                CurrentMana = pool.CurrentMana,
                ManaSpent = 0
            };
        }

        pool.CurrentMana -= amount;
        pool.LastUpdated = DateTime.UtcNow;
        await _manaDal.UpdateCurrentManaAsync(characterId, school, pool.CurrentMana);

        return new SpendManaResult
        {
            Success = true,
            ManaSpent = amount,
            CurrentMana = pool.CurrentMana
        };
    }

    /// <summary>
    /// Adds mana to a character's pool (up to maximum).
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="school">The magic school.</param>
    /// <param name="amount">The amount of mana to add.</param>
    /// <param name="maxMana">The maximum mana for this pool (skill level).</param>
    /// <returns>The actual amount of mana recovered.</returns>
    public async Task<int> RecoverManaAsync(int characterId, MagicSchool school, int amount, int maxMana)
    {
        var pool = await _manaDal.GetManaPoolAsync(characterId, school);
        
        if (pool == null)
        {
            return 0;
        }

        var previousMana = pool.CurrentMana;
        pool.CurrentMana = Math.Min(pool.CurrentMana + amount, maxMana);
        pool.LastUpdated = DateTime.UtcNow;
        
        await _manaDal.UpdateCurrentManaAsync(characterId, school, pool.CurrentMana);

        return pool.CurrentMana - previousMana;
    }

    /// <summary>
    /// Restores all mana pools to maximum (e.g., after extended rest).
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="manaSkillLevels">Dictionary of school -> skill level (max mana).</param>
    public async Task RestoreAllManaAsync(int characterId, Dictionary<MagicSchool, int> manaSkillLevels)
    {
        var pools = await _manaDal.GetAllManaPoolsAsync(characterId);
        
        foreach (var pool in pools)
        {
            if (manaSkillLevels.TryGetValue(pool.MagicSchool, out var maxMana))
            {
                pool.CurrentMana = maxMana;
                pool.LastUpdated = DateTime.UtcNow;
                await _manaDal.UpdateCurrentManaAsync(characterId, pool.MagicSchool, pool.CurrentMana);
            }
        }
    }

    #endregion

    #region Mana Recovery Action

    /// <summary>
    /// Performs a mana recovery action for a character.
    /// This is an active skill check using the school's mana skill.
    /// Each point of mana recovered takes 1 minute.
    /// </summary>
    /// <param name="characterId">The character ID.</param>
    /// <param name="school">The magic school to recover.</param>
    /// <param name="manaSkillLevel">The character's skill level in this school's mana skill.</param>
    /// <param name="attributeBonus">The relevant attribute bonus for the skill.</param>
    /// <param name="tv">Target Value for the recovery check (default 6).</param>
    /// <returns>Result of the recovery attempt.</returns>
    public async Task<ManaRecoveryResult> AttemptManaRecoveryAsync(
        int characterId,
        MagicSchool school,
        int manaSkillLevel,
        int attributeBonus,
        int tv = DefaultRecoveryTV)
    {
        var pool = await _manaDal.GetManaPoolAsync(characterId, school);
        
        if (pool == null)
        {
            return new ManaRecoveryResult
            {
                Success = false,
                ErrorMessage = $"Character has no mana pool for {school} magic."
            };
        }

        // Max mana equals skill level
        int maxMana = manaSkillLevel;
        
        if (pool.CurrentMana >= maxMana)
        {
            return new ManaRecoveryResult
            {
                Success = false,
                ErrorMessage = $"{school} mana pool is already full.",
                CurrentMana = pool.CurrentMana,
                MaxMana = maxMana
            };
        }

        // Calculate AS (Ability Score) = skill bonus + attribute
        int skillBonus = SkillCost.GetBonus(manaSkillLevel);
        int abilityScore = skillBonus + attributeBonus;

        // Roll 4dF+ and calculate result
        int roll = _diceRoller.Roll4dFPlus();
        int av = abilityScore + roll;
        int sv = av - tv;

        // Get result from mana recovery table
        var interpretation = ResultTables.GetResult(sv, ResultTableType.ManaRecovery);

        var result = new ManaRecoveryResult
        {
            Success = interpretation.IsSuccess,
            AV = av,
            TV = tv,
            SV = sv,
            Roll = roll,
            ResultDescription = interpretation.Description,
            MaxMana = maxMana
        };

        if (interpretation.IsSuccess && interpretation.EffectValue > 0)
        {
            // Capture current mana before recovery modifies the pool object
            int currentManaBefore = pool.CurrentMana;
            
            // Recover mana (capped by max and what's needed)
            int toRecover = Math.Min(interpretation.EffectValue, maxMana - currentManaBefore);
            result.ManaRecovered = await RecoverManaAsync(characterId, school, toRecover, maxMana);
            result.MinutesSpent = result.ManaRecovered; // 1 minute per mana
            result.CurrentMana = currentManaBefore + result.ManaRecovered;
        }
        else if (interpretation.EffectValue < 0)
        {
            // Negative effect - could be FAT loss or mana loss depending on severity
            result.ManaRecovered = 0;
            result.NegativeEffect = interpretation.EffectValue;
            result.CurrentMana = pool.CurrentMana;
        }
        else
        {
            result.ManaRecovered = 0;
            result.CurrentMana = pool.CurrentMana;
        }

        return result;
    }

    #endregion

    #region Mana Pool Initialization

    /// <summary>
    /// Creates or updates a mana pool for a character based on their mana skill.
    /// </summary>
    public async Task<CharacterMana> EnsureManaPoolAsync(
        int characterId,
        MagicSchool school,
        string manaSkillId,
        int skillLevel)
    {
        var pool = await _manaDal.GetManaPoolAsync(characterId, school);
        
        if (pool == null)
        {
            pool = new CharacterMana
            {
                CharacterId = characterId,
                MagicSchool = school,
                ManaSkillId = manaSkillId,
                CurrentMana = skillLevel, // Start full
                LastUpdated = DateTime.UtcNow
            };
        }
        else
        {
            // Update skill ID if changed
            pool.ManaSkillId = manaSkillId;
            pool.LastUpdated = DateTime.UtcNow;
        }

        return await _manaDal.SaveManaPoolAsync(pool);
    }

    #endregion

    #region Utility Methods

    /// <summary>
    /// Gets the mana skill ID for a magic school.
    /// </summary>
    public static string GetManaSkillId(MagicSchool school)
    {
        return school switch
        {
            MagicSchool.Fire => "fire-mana",
            MagicSchool.Water => "water-mana",
            MagicSchool.Light => "light-mana",
            MagicSchool.Life => "life-mana",
            _ => throw new ArgumentException($"Unknown magic school: {school}")
        };
    }

    /// <summary>
    /// Gets the magic school for a mana skill ID.
    /// </summary>
    public static MagicSchool? GetSchoolFromManaSkillId(string skillId)
    {
        return skillId switch
        {
            "fire-mana" => MagicSchool.Fire,
            "water-mana" => MagicSchool.Water,
            "light-mana" => MagicSchool.Light,
            "life-mana" => MagicSchool.Life,
            _ => null
        };
    }

    #endregion
}

/// <summary>
/// Result of a mana spend operation.
/// </summary>
public class SpendManaResult
{
    public bool Success { get; set; }
    public int ManaSpent { get; set; }
    public int CurrentMana { get; set; }
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Result of a mana recovery action.
/// </summary>
public class ManaRecoveryResult
{
    public bool Success { get; set; }
    public int AV { get; set; }
    public int TV { get; set; }
    public int SV { get; set; }
    public int Roll { get; set; }
    public int ManaRecovered { get; set; }
    public int CurrentMana { get; set; }
    public int MaxMana { get; set; }
    public int MinutesSpent { get; set; }
    public int NegativeEffect { get; set; }
    public string? ResultDescription { get; set; }
    public string? ErrorMessage { get; set; }
}
