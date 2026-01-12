using Csla;
using System;
using System.Threading.Tasks;
using Threa.Dal;

namespace GameMechanics.GamePlay;

/// <summary>
/// Command object to detach a character from a table.
/// A character can only be attached to one table at a time, so this removes them from their current table.
/// </summary>
[Serializable]
public class TableCharacterDetacher : CommandBase<TableCharacterDetacher>
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
    private async Task ExecuteAsync(int characterId, [Inject] ITableDal tableDal)
    {
        try
        {
            // Find the table the character is attached to
            var table = await tableDal.GetTableForCharacterAsync(characterId);

            if (table == null)
            {
                LoadProperty(SuccessProperty, false);
                LoadProperty(ErrorMessageProperty, "Character is not attached to any table.");
                return;
            }

            // Store the table name for confirmation message
            LoadProperty(TableNameProperty, table.Name);

            // Remove the character from the table
            await tableDal.RemoveCharacterFromTableAsync(table.Id, characterId);

            LoadProperty(SuccessProperty, true);
        }
        catch (Exception ex)
        {
            LoadProperty(SuccessProperty, false);
            LoadProperty(ErrorMessageProperty, $"Failed to detach character: {ex.Message}");
        }
    }
}
