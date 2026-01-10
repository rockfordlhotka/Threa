using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb;

public class SkillDal : ISkillDal
{
    public Task<List<Skill>> GetAllSkillsAsync()
    {
        return Task.FromResult(MockDb.Skills);
    }

    public Task<Skill?> GetSkillAsync(string id)
    {
        return Task.FromResult(MockDb.Skills.FirstOrDefault(s => s.Id == id));
    }

    public Task<Skill> SaveSkillAsync(Skill skill)
    {
        var existing = MockDb.Skills.FirstOrDefault(s => s.Id == skill.Id);
        if (existing != null)
        {
            MockDb.Skills.Remove(existing);
        }
        MockDb.Skills.Add(skill);
        return Task.FromResult(skill);
    }

    public Task DeleteSkillAsync(string id)
    {
        var existing = MockDb.Skills.FirstOrDefault(s => s.Id == id);
        if (existing != null)
        {
            MockDb.Skills.Remove(existing);
        }
        return Task.CompletedTask;
    }
}

