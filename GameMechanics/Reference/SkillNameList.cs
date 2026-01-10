using System;
using System.Threading.Tasks;
using Csla;
using Threa.Dal;

namespace GameMechanics.Reference;

[Serializable]
public class SkillNameList : ReadOnlyListBase<SkillNameList, SkillName>
{
    [Fetch]
    private async Task Fetch([Inject] ISkillDal dal, [Inject] IChildDataPortal<SkillName> childPortal)
    {
        var skills = await dal.GetAllSkillsAsync();
        using (LoadListMode)
        {
            foreach (var skill in skills)
            {
                Add(childPortal.FetchChild(skill.Id, skill.Name));
            }
        }
    }
}

[Serializable]
public class SkillName : ReadOnlyBase<SkillName>
{
    public static readonly PropertyInfo<string> IdProperty = RegisterProperty<string>(nameof(Id));
    public string Id
    {
        get => GetProperty(IdProperty);
        private set => LoadProperty(IdProperty, value);
    }

    public static readonly PropertyInfo<string> NameProperty = RegisterProperty<string>(nameof(Name));
    public string Name
    {
        get => GetProperty(NameProperty);
        private set => LoadProperty(NameProperty, value);
    }

    [FetchChild]
    private void Fetch(string id, string name)
    {
        Id = id;
        Name = name;
    }
}
