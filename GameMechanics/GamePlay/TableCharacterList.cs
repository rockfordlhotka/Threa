using Csla;
using System;
using System.Threading.Tasks;
using Threa.Dal;

namespace GameMechanics.GamePlay;

/// <summary>
/// Read-only list of characters connected to a table.
/// </summary>
[Serializable]
public class TableCharacterList : ReadOnlyListBase<TableCharacterList, TableCharacterInfo>
{
    /// <summary>
    /// Fetches all characters at a specific table.
    /// </summary>
    [Fetch]
    private async Task FetchAsync(Guid tableId,
        [Inject] ITableDal tableDal,
        [Inject] ICharacterDal characterDal,
        [Inject] ICharacterEffectDal effectDal,
        [Inject] IChildDataPortal<TableCharacterInfo> characterPortal)
    {
        var tableCharacters = await tableDal.GetTableCharactersAsync(tableId);

        using (LoadListMode)
        {
            foreach (var tc in tableCharacters)
            {
                // Load the character details
                Threa.Dal.Dto.Character? character = null;
                try
                {
                    character = await characterDal.GetCharacterAsync(tc.CharacterId);

                    // Populate effects for wound/effect counting in TableCharacterInfo
                    if (character != null)
                    {
                        character.Effects = await effectDal.GetCharacterEffectsAsync(tc.CharacterId);
                    }
                }
                catch
                {
                    // Character might have been deleted
                }

                Add(characterPortal.FetchChild(tc, character));
            }
        }
    }
}
