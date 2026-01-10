using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace Threa.Dal.MockDb;

public class MagicSchoolDal : IMagicSchoolDal
{
    public Task<List<MagicSchoolDefinition>> GetAllSchoolsAsync()
    {
        return Task.FromResult(MockDb.MagicSchools);
    }

    public Task<MagicSchoolDefinition?> GetSchoolAsync(string id)
    {
        return Task.FromResult(MockDb.MagicSchools.FirstOrDefault(s => s.Id == id));
    }

    public Task<MagicSchoolDefinition> SaveSchoolAsync(MagicSchoolDefinition school)
    {
        var existing = MockDb.MagicSchools.FirstOrDefault(s => s.Id == school.Id);
        if (existing != null)
        {
            MockDb.MagicSchools.Remove(existing);
        }
        MockDb.MagicSchools.Add(school);
        return Task.FromResult(school);
    }

    public Task DeleteSchoolAsync(string id)
    {
        var existing = MockDb.MagicSchools.FirstOrDefault(s => s.Id == id);
        if (existing != null)
        {
            if (existing.IsCore)
            {
                throw new InvalidOperationException("Cannot delete core magic schools.");
            }
            MockDb.MagicSchools.Remove(existing);
        }
        return Task.CompletedTask;
    }
}
