using Csla;
using System;
using System.Threading.Tasks;
using Threa.Dal;

namespace GameMechanics.Player
{
  [Serializable]
  public class CharacterInfo : ReadOnlyBase<CharacterInfo>
  {
    public static readonly PropertyInfo<int> IdProperty = RegisterProperty<int>(nameof(Id));
    public int Id
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
