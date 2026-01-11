using Csla;
using System;
using System.Threading.Tasks;
using Threa.Dal;

namespace GameMechanics
{
  /// <summary>
  /// Command object to activate a character, making it playable.
  /// This is a one-way operation - once activated, certain properties like attributes become read-only.
  /// </summary>
  [Serializable]
  public class CharacterActivator : CommandBase<CharacterActivator>
  {
    // Output property
    public static readonly PropertyInfo<bool> SuccessProperty = RegisterProperty<bool>(nameof(Success));
    public bool Success
    {
      get => ReadProperty(SuccessProperty);
      private set => LoadProperty(SuccessProperty, value);
    }

    [Execute]
    private async Task ExecuteAsync(int characterId, [Inject] ICharacterDal dal)
    {
      // Fetch the character
      var character = await dal.GetCharacterAsync(characterId);
      
      if (character != null && !character.IsPlayable)
      {
        // Activate the character
        character.IsPlayable = true;
        
        // Save the updated character
        await dal.SaveCharacterAsync(character);
        
        LoadProperty(SuccessProperty, true);
      }
      else
      {
        LoadProperty(SuccessProperty, false);
      }
    }
  }
}

