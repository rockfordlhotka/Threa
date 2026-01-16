using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

/// <summary>
/// SQLite implementation of ISkillDal.
/// </summary>
public class SkillDal : ISkillDal
{
    private readonly SqliteConnection Connection;

    public SkillDal(SqliteConnection connection)
    {
        Connection = connection;
        InitializeTable();
    }

    private void InitializeTable()
    {
        try
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS Skills (
                    Id TEXT NOT NULL PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Json TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_Skills_Name ON Skills(Name);
            ";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error creating Skills table", ex);
        }
    }

    public async Task<List<Skill>> GetAllSkillsAsync()
    {
        try
        {
            var sql = "SELECT Json FROM Skills ORDER BY Name";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            using var reader = await command.ExecuteReaderAsync();
            var skills = new List<Skill>();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var skill = JsonSerializer.Deserialize<Skill>(json);
                if (skill != null)
                    skills.Add(skill);
            }
            return skills;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting skills", ex);
        }
    }

    public async Task<Skill?> GetSkillAsync(string id)
    {
        try
        {
            var sql = "SELECT Json FROM Skills WHERE Id = @Id COLLATE NOCASE";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id);
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                return null;
            string json = reader.GetString(0);
            return JsonSerializer.Deserialize<Skill>(json);
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error getting skill {id}", ex);
        }
    }

    public async Task<Skill> SaveSkillAsync(Skill skill)
    {
        try
        {
            // Check if skill exists
            var existing = await GetSkillAsync(skill.Id);
            string sql;
            if (existing == null)
            {
                sql = "INSERT INTO Skills (Id, Name, Json) VALUES (@Id, @Name, @Json)";
            }
            else
            {
                sql = "UPDATE Skills SET Name = @Name, Json = @Json WHERE Id = @Id COLLATE NOCASE";
            }

            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", skill.Id);
            command.Parameters.AddWithValue("@Name", skill.Name);
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(skill));
            await command.ExecuteNonQueryAsync();

            return skill;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error saving skill {skill.Id}", ex);
        }
    }

    public async Task DeleteSkillAsync(string id)
    {
        try
        {
            var sql = "DELETE FROM Skills WHERE Id = @Id COLLATE NOCASE";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id);
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error deleting skill {id}", ex);
        }
    }
}
