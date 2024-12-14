using Microsoft.Data.Sqlite;

namespace Threa.Dal.Sqlite;

public class ImageDal : IImageDal
{
    private readonly SqliteConnection Connection;

    public ImageDal(SqliteConnection connection)
    {
        Connection = connection;
        try
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS 
                Images (
                    Id INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                    Image TEXT
                );
                ";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error creating image table", ex);
        }
    }

    public async Task<int> AddImage(string data)
    {
        try
        {
            var sql = "INSERT INTO Images (Image) VALUES (@Image)";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Image", data);
            await command.ExecuteNonQueryAsync();

            sql = "SELECT last_insert_rowid()";
            using var idCommand = Connection.CreateCommand();
            {
                long? lastInsertId = (long?)idCommand.ExecuteScalar();
                if (lastInsertId.HasValue)
                {
                    return (int)lastInsertId.Value;
                }
                else
                {
                    throw new OperationFailedException("Error getting last insert id");
                }
            }
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error adding image", ex);
        }
    }

    public async Task DeleteImage(int id)
    {
        try
        {
            var sql = "DELETE FROM Images WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error deleting image {id}", ex);
        }
    }

    public async Task<string> GetImage(int id)
    {
        try
        {
            var sql = "SELECT Image FROM Images WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id);
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                throw new NotFoundException($"Image {id}");
            return reader.GetString(0);
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error getting image {id}", ex);
        }
    }

    public async Task UpdateImage(int id, string data)
    {
        try
        {
            var sql = "UPDATE Images SET Image = @Image WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@Image", data);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error updating image {id}", ex);
        }
}
}