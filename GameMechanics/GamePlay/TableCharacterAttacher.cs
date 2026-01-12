using Csla;
using System;
using System.Threading.Tasks;
using Threa.Dal;
using Threa.Dal.Dto;

namespace GameMechanics.GamePlay;

/// <summary>
/// Command object to attach a character to a table.
/// A character can only be attached to one table at a time.
/// If the character is already attached to another table, that attachment is removed first.
/// </summary>
[Serializable]
public class TableCharacterAttacher : CommandBase<TableCharacterAttacher>
{
    public static readonly PropertyInfo<bool> SuccessProperty = RegisterProperty<bool>(nameof(Success));
    public bool Success
    {
        get => ReadProperty(SuccessProperty);
        private set => LoadProperty(SuccessProperty, value);
    }

    public static readonly PropertyInfo<string?> ErrorMessageProperty = RegisterProperty<string?>(nameof(ErrorMessage));
    public string? ErrorMessage
    {
        get => ReadProperty(ErrorMessageProperty);
        private set => LoadProperty(ErrorMessageProperty, value);
    }

    public static readonly PropertyInfo<string?> TableNameProperty = RegisterProperty<string?>(nameof(TableName));
    public string? TableName
    {
        get => ReadProperty(TableNameProperty);
        private set => LoadProperty(TableNameProperty, value);
    }

    [Execute]
    private async Task ExecuteAsync(
        int characterId, 
        Guid tableId,
        int playerId,
        [Inject] ITableDal tableDal,
        [Inject] ICharacterDal characterDal)
    {
        try
        {
            // Verify the character exists and belongs to the player
            var character = await characterDal.GetCharacterAsync(characterId);
            if (character == null)
            {
                LoadProperty(SuccessProperty, false);
                LoadProperty(ErrorMessageProperty, "Character not found.");
                return;
            }

            if (character.PlayerId != playerId)
            {
                LoadProperty(SuccessProperty, false);
                LoadProperty(ErrorMessageProperty, "You do not own this character.");
                return;
            }

            if (!character.IsPlayable)
            {
                LoadProperty(SuccessProperty, false);
                LoadProperty(ErrorMessageProperty, "This character is not activated for play.");
                return;
            }

            // Verify the table exists
            GameTable table;
            try
            {
                table = await tableDal.GetTableAsync(tableId);
            }
            catch (NotFoundException)
            {
                LoadProperty(SuccessProperty, false);
                LoadProperty(ErrorMessageProperty, "Table not found.");
                return;
            }

            // Check if character is already attached to another table
            var existingTable = await tableDal.GetTableForCharacterAsync(characterId);
            if (existingTable != null)
            {
                // Remove from existing table first
                await tableDal.RemoveCharacterFromTableAsync(existingTable.Id, characterId);
            }

            // Attach to the new table
            var tableCharacter = new TableCharacter
            {
                TableId = tableId,
                CharacterId = characterId,
                PlayerId = playerId,
                JoinedAt = DateTime.UtcNow,
                ConnectionStatus = ConnectionStatus.Connected,
                LastActivity = DateTime.UtcNow
            };

            await tableDal.AddCharacterToTableAsync(tableCharacter);

            LoadProperty(SuccessProperty, true);
            LoadProperty(TableNameProperty, table.Name);
        }
        catch (Exception ex)
        {
            LoadProperty(SuccessProperty, false);
            LoadProperty(ErrorMessageProperty, $"Failed to attach character to table: {ex.Message}");
        }
    }
}
