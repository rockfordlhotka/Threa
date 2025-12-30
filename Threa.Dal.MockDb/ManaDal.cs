using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb;

/// <summary>
/// Mock database implementation for character mana pools.
/// </summary>
public class ManaDal : IManaDal
{
    private readonly List<CharacterMana> _pools;
    private readonly bool _useStaticStorage;

    /// <summary>
    /// Creates a ManaDal using instance-level storage (for test isolation).
    /// </summary>
    public ManaDal()
    {
        _pools = new List<CharacterMana>();
        _useStaticStorage = false;
    }

    /// <summary>
    /// Creates a ManaDal that uses the static MockDb storage.
    /// </summary>
    public static ManaDal WithStaticStorage()
    {
        return new ManaDal(useStatic: true);
    }

    private ManaDal(bool useStatic)
    {
        _pools = MockDb.CharacterManaPools;
        _useStaticStorage = useStatic;
    }

    private List<CharacterMana> Pools => _pools;

    public Task<CharacterMana?> GetManaPoolAsync(int characterId, MagicSchool school)
    {
        var pool = Pools
            .FirstOrDefault(m => m.CharacterId == characterId && m.MagicSchool == school);
        return Task.FromResult(pool);
    }

    public Task<List<CharacterMana>> GetAllManaPoolsAsync(int characterId)
    {
        var pools = Pools
            .Where(m => m.CharacterId == characterId)
            .ToList();
        return Task.FromResult(pools);
    }

    public Task<CharacterMana> SaveManaPoolAsync(CharacterMana manaPool)
    {
        var existing = Pools
            .FirstOrDefault(m => m.CharacterId == manaPool.CharacterId && m.MagicSchool == manaPool.MagicSchool);

        if (existing != null)
        {
            Pools.Remove(existing);
            manaPool.Id = existing.Id;
        }
        else if (manaPool.Id == 0)
        {
            manaPool.Id = Pools.Count == 0
                ? 1
                : Pools.Max(m => m.Id) + 1;
        }

        Pools.Add(manaPool);
        return Task.FromResult(manaPool);
    }

    public Task UpdateCurrentManaAsync(int characterId, MagicSchool school, int currentMana)
    {
        var pool = Pools
            .FirstOrDefault(m => m.CharacterId == characterId && m.MagicSchool == school);

        if (pool != null)
        {
            pool.CurrentMana = currentMana;
            pool.LastUpdated = DateTime.UtcNow;
        }

        return Task.CompletedTask;
    }

    public Task InitializeManaPoolsAsync(int characterId)
    {
        // In a real implementation, this would look up character's mana skills
        // and create pools for each school they have skills in.
        // For mock, we just ensure the character has empty pools if needed.
        return Task.CompletedTask;
    }
}
