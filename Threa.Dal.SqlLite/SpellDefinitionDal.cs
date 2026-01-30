using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

/// <summary>
/// SQLite implementation of ISpellDefinitionDal.
/// </summary>
public class SpellDefinitionDal : ISpellDefinitionDal
{
    private readonly SqliteConnection Connection;

    public SpellDefinitionDal(SqliteConnection connection)
    {
        Connection = connection;
        InitializeTable();
    }

    private void InitializeTable()
    {
        try
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS SpellDefinitions (
                    SkillId TEXT NOT NULL PRIMARY KEY,
                    MagicSchool INTEGER NOT NULL,
                    Json TEXT NOT NULL
                );
                CREATE INDEX IF NOT EXISTS IX_SpellDefinitions_MagicSchool ON SpellDefinitions(MagicSchool);
            ";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error creating SpellDefinitions table", ex);
        }
    }

    public async Task<SpellDefinition?> GetSpellBySkillIdAsync(string skillId)
    {
        try
        {
            var sql = "SELECT Json FROM SpellDefinitions WHERE SkillId = @SkillId COLLATE NOCASE";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@SkillId", skillId);
            using var reader = await command.ExecuteReaderAsync();
            if (!reader.Read())
                return null;
            string json = reader.GetString(0);
            return JsonSerializer.Deserialize<SpellDefinition>(json);
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error getting spell definition for skill {skillId}", ex);
        }
    }

    public async Task<List<SpellDefinition>> GetSpellsBySchoolAsync(MagicSchool school)
    {
        try
        {
            var sql = "SELECT Json FROM SpellDefinitions WHERE MagicSchool = @MagicSchool";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@MagicSchool", (int)school);
            using var reader = await command.ExecuteReaderAsync();
            var spells = new List<SpellDefinition>();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var spell = JsonSerializer.Deserialize<SpellDefinition>(json);
                if (spell != null)
                    spells.Add(spell);
            }
            return spells;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error getting spells for school {school}", ex);
        }
    }

    public async Task<List<SpellDefinition>> GetAllSpellsAsync()
    {
        try
        {
            var sql = "SELECT Json FROM SpellDefinitions";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            using var reader = await command.ExecuteReaderAsync();
            var spells = new List<SpellDefinition>();
            while (reader.Read())
            {
                string json = reader.GetString(0);
                var spell = JsonSerializer.Deserialize<SpellDefinition>(json);
                if (spell != null)
                    spells.Add(spell);
            }
            return spells;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting all spell definitions", ex);
        }
    }

    public async Task SaveSpellAsync(SpellDefinition spell)
    {
        try
        {
            var existing = await GetSpellBySkillIdAsync(spell.SkillId);
            string sql;

            if (existing == null)
            {
                sql = @"INSERT INTO SpellDefinitions (SkillId, MagicSchool, Json)
                        VALUES (@SkillId, @MagicSchool, @Json)";
            }
            else
            {
                sql = @"UPDATE SpellDefinitions SET MagicSchool = @MagicSchool, Json = @Json
                        WHERE SkillId = @SkillId COLLATE NOCASE";
            }

            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@SkillId", spell.SkillId);
            command.Parameters.AddWithValue("@MagicSchool", (int)spell.MagicSchool);
            command.Parameters.AddWithValue("@Json", JsonSerializer.Serialize(spell));
            await command.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error saving spell definition {spell.SkillId}", ex);
        }
    }

    public async Task<bool> IsSpellAsync(string skillId)
    {
        try
        {
            var sql = "SELECT COUNT(*) FROM SpellDefinitions WHERE SkillId = @SkillId COLLATE NOCASE";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@SkillId", skillId);
            var count = (long?)await command.ExecuteScalarAsync() ?? 0;
            return count > 0;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException($"Error checking if skill {skillId} is a spell", ex);
        }
    }
}
