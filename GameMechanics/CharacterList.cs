using Csla;
using System;
using System.Threading.Tasks;
using Threa.Dal;

namespace GameMechanics
{
  [Serializable]
  public class CharacterList : ReadOnlyListBase<CharacterList, CharacterInfo>
  {
    [Fetch]
    private async Task Fetch(string playerEmail, [Inject] ICharacterDal dal)
    {
      var items = await dal.GetCharactersAsync(playerEmail);
      using (LoadListMode)
      {
        foreach (var item in items)
          Add(DataPortal.FetchChild<CharacterInfo>(item));
      }
    }
  }

  [Serializable]
  public class CharacterInfo : ReadOnlyBase<CharacterInfo>
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

    public static readonly PropertyInfo<string> SpeciesProperty = RegisterProperty<string>(nameof(Species));
    public string Species
    {
      get => GetProperty(SpeciesProperty);
      private set => LoadProperty(SpeciesProperty, value);
    }

    [FetchChild]
    private void Fetch(ICharacter character)
    {
      Id = character.Id;
      Name = character.Name;
      Species = character.Species;
    }
  }
}
