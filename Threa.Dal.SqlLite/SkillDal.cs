using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

public class SkillDal : ISkillDal
{
    private static readonly List<Skill> _skills = GetHardcodedSkills();

    public Task<List<Skill>> GetAllSkillsAsync()
    {
        // TODO: Implement SQLite persistence for skills
        // For now, return hardcoded skills matching MockDb
        return Task.FromResult(_skills);
    }

    public Task<Skill?> GetSkillAsync(string id)
    {
        // TODO: Implement SQLite persistence for skills
        return Task.FromResult(_skills.FirstOrDefault(s => s.Id == id));
    }

    public Task<Skill> SaveSkillAsync(Skill skill)
    {
        // TODO: Implement SQLite persistence for skills
        var existing = _skills.FirstOrDefault(s => s.Id == skill.Id);
        if (existing != null)
        {
            _skills.Remove(existing);
        }
        _skills.Add(skill);
        return Task.FromResult(skill);
    }

    public Task DeleteSkillAsync(string id)
    {
        // TODO: Implement SQLite persistence for skills
        var existing = _skills.FirstOrDefault(s => s.Id == id);
        if (existing != null)
        {
            _skills.Remove(existing);
        }
        return Task.CompletedTask;
    }

    private static List<Skill> GetHardcodedSkills()
    {
        return
        [
            // Standard attribute skills
            new Skill { Id = "physicality", Name = "Physicality", Category = SkillCategory.Standard, Untrained = 3, Trained = 1, PrimaryAttribute = "STR" },
            new Skill { Id = "dodge", Name = "Dodge", Category = SkillCategory.Standard, Untrained = 6, Trained = 4, PrimaryAttribute = "DEX/ITT" },
            new Skill { Id = "drive", Name = "Drive", Category = SkillCategory.Standard, Untrained = 5, Trained = 3, PrimaryAttribute = "WIL/END" },
            new Skill { Id = "reasoning", Name = "Reasoning", Category = SkillCategory.Standard, Untrained = 5, Trained = 3, PrimaryAttribute = "INT" },
            new Skill { Id = "awareness", Name = "Awareness", Category = SkillCategory.Standard, Untrained = 5, Trained = 2, PrimaryAttribute = "ITT" },
            new Skill { Id = "focus", Name = "Focus", Category = SkillCategory.Standard, Untrained = 5, Trained = 2, PrimaryAttribute = "WIL" },
            new Skill { Id = "bearing", Name = "Bearing", Category = SkillCategory.Standard, Untrained = 4, Trained = 2, PrimaryAttribute = "SOC" },
            new Skill { Id = "influence", Name = "Influence", Category = SkillCategory.Standard, Untrained = 4, Trained = 2, PrimaryAttribute = "PHY" },

            // Weapon combat skills
            new Skill { Id = "swords", Name = "Swords", Category = SkillCategory.Combat, IsSpecialized = true, Untrained = 7, Trained = 4, PrimaryAttribute = "DEX" },
            new Skill { Id = "axes", Name = "Axes", Category = SkillCategory.Combat, IsSpecialized = true, Untrained = 7, Trained = 4, PrimaryAttribute = "STR" },
            new Skill { Id = "daggers", Name = "Daggers", Category = SkillCategory.Combat, IsSpecialized = true, Untrained = 6, Trained = 3, PrimaryAttribute = "DEX" },
            new Skill { Id = "bows", Name = "Bows", Category = SkillCategory.Combat, IsSpecialized = true, Untrained = 7, Trained = 4, PrimaryAttribute = "DEX" },
            new Skill { Id = "crossbows", Name = "Crossbows", Category = SkillCategory.Combat, IsSpecialized = true, Untrained = 6, Trained = 3, PrimaryAttribute = "DEX" },
            new Skill { Id = "spears", Name = "Spears", Category = SkillCategory.Combat, IsSpecialized = true, Untrained = 6, Trained = 3, PrimaryAttribute = "DEX" },
            new Skill { Id = "polearms", Name = "Polearms", Category = SkillCategory.Combat, IsSpecialized = true, Untrained = 7, Trained = 4, PrimaryAttribute = "STR" },
            new Skill { Id = "unarmed-combat", Name = "Unarmed Combat", Category = SkillCategory.Combat, IsSpecialized = true, Untrained = 6, Trained = 3, PrimaryAttribute = "DEX" },

            // Movement skills
            new Skill { Id = "sprint", Name = "Sprint", Category = SkillCategory.Movement, Untrained = 5, Trained = 2, PrimaryAttribute = "DEX" },

            // Magic: Mana skills
            new Skill { Id = "fire-mana", Name = "Fire Mana", Category = SkillCategory.Mana, IsMagic = true, Untrained = 7, Trained = 4, PrimaryAttribute = "WIL" },
            new Skill { Id = "water-mana", Name = "Water Mana", Category = SkillCategory.Mana, IsMagic = true, Untrained = 7, Trained = 4, PrimaryAttribute = "WIL" },
            new Skill { Id = "light-mana", Name = "Light Mana", Category = SkillCategory.Mana, IsMagic = true, Untrained = 7, Trained = 4, PrimaryAttribute = "WIL" },
            new Skill { Id = "life-mana", Name = "Life Mana", Category = SkillCategory.Mana, IsMagic = true, Untrained = 7, Trained = 4, PrimaryAttribute = "WIL" },

            // Magic: Spell skills
            new Skill { Id = "fire-bolt", Name = "Fire Bolt", Category = SkillCategory.Spell, IsMagic = true, Untrained = 8, Trained = 5, PrimaryAttribute = "INT" },
            new Skill { Id = "heal", Name = "Heal", Category = SkillCategory.Spell, IsMagic = true, Untrained = 8, Trained = 5, PrimaryAttribute = "WIL" },
            new Skill { Id = "illuminate", Name = "Illuminate", Category = SkillCategory.Spell, IsMagic = true, Untrained = 7, Trained = 4, PrimaryAttribute = "INT" }
        ];
    }
}

