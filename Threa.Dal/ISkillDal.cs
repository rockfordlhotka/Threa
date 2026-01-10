using System.Collections.Generic;
using System.Threading.Tasks;
using Threa.Dal.Dto;

namespace Threa.Dal;

public interface ISkillDal
{
    Task<List<Skill>> GetAllSkillsAsync();
    Task<Skill?> GetSkillAsync(string id);
    Task<Skill> SaveSkillAsync(Skill skill);
    Task DeleteSkillAsync(string id);
}

