using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Threa.Dal.Dto;

namespace Threa.Dal.Sqlite;

/// <summary>
/// SQLite implementation of IEffectTemplateDal.
/// </summary>
public class EffectTemplateDal : IEffectTemplateDal
{
    private readonly SqliteConnection Connection;
    private bool _initialized;

    public EffectTemplateDal(SqliteConnection connection)
    {
        Connection = connection;
    }

    private async Task InitializeTableAsync()
    {
        if (_initialized) return;

        try
        {
            var sql = @"
                CREATE TABLE IF NOT EXISTS EffectTemplates (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    EffectType INTEGER NOT NULL,
                    Description TEXT,
                    IconName TEXT,
                    Color TEXT,
                    DefaultDurationValue INTEGER,
                    DurationType INTEGER NOT NULL DEFAULT 0,
                    StateJson TEXT,
                    Tags TEXT,
                    IsSystem INTEGER NOT NULL DEFAULT 0,
                    IsActive INTEGER NOT NULL DEFAULT 1,
                    CreatedAt TEXT NOT NULL,
                    UpdatedAt TEXT
                )
            ";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();

            // Check if table is empty and seed if needed
            using var countCommand = Connection.CreateCommand();
            countCommand.CommandText = "SELECT COUNT(*) FROM EffectTemplates";
            var count = (long?)await countCommand.ExecuteScalarAsync() ?? 0;

            if (count == 0)
            {
                await SeedTemplatesAsync();
            }

            _initialized = true;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error creating EffectTemplates table", ex);
        }
    }

    private async Task SeedTemplatesAsync()
    {
        var templates = new List<EffectTemplateDto>
        {
            new()
            {
                Name = "Stunned",
                EffectType = EffectType.Condition,
                Description = "Unable to take actions, severely impaired.",
                IconName = "bi-lightning",
                Color = "#ffc107",
                DefaultDurationValue = 1,
                DurationType = DurationType.Rounds,
                StateJson = "{\"ASModifier\":-4,\"BehaviorTags\":[\"condition\",\"end-of-turn-remove\"]}",
                Tags = "combat,condition,debilitating",
                IsSystem = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new()
            {
                Name = "Blessed",
                EffectType = EffectType.Buff,
                Description = "Divine favor enhances all actions.",
                IconName = "bi-star-fill",
                Color = "#ffd700",
                DefaultDurationValue = 10,
                DurationType = DurationType.Rounds,
                StateJson = "{\"ASModifier\":2,\"BehaviorTags\":[\"modifier\",\"magic\"]}",
                Tags = "magic,buff,divine",
                IsSystem = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new()
            {
                Name = "Poisoned",
                EffectType = EffectType.Poison,
                Description = "Toxin causes ongoing damage.",
                IconName = "bi-droplet-fill",
                Color = "#28a745",
                DefaultDurationValue = 5,
                DurationType = DurationType.Rounds,
                StateJson = "{\"FatDamagePerTick\":2,\"VitDamagePerTick\":1,\"BehaviorTags\":[\"poison\",\"end-of-round-trigger\"]}",
                Tags = "poison,damage-over-time",
                IsSystem = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new()
            {
                Name = "Haste",
                EffectType = EffectType.Buff,
                Description = "Supernatural speed enhances reflexes.",
                IconName = "bi-speedometer2",
                Color = "#17a2b8",
                DefaultDurationValue = 3,
                DurationType = DurationType.Rounds,
                StateJson = "{\"AttributeModifiers\":{\"DEX\":2},\"BehaviorTags\":[\"modifier\",\"magic\"]}",
                Tags = "magic,buff,speed",
                IsSystem = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new()
            {
                Name = "Weakened",
                EffectType = EffectType.Debuff,
                Description = "Physical strength is sapped.",
                IconName = "bi-person-dash",
                Color = "#6c757d",
                DefaultDurationValue = 5,
                DurationType = DurationType.Rounds,
                StateJson = "{\"AttributeModifiers\":{\"STR\":-2},\"BehaviorTags\":[\"modifier\",\"debuff\"]}",
                Tags = "debuff,physical",
                IsSystem = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new()
            {
                Name = "Regenerating",
                EffectType = EffectType.Buff,
                Description = "Wounds heal over time.",
                IconName = "bi-heart-pulse",
                Color = "#dc3545",
                DefaultDurationValue = 10,
                DurationType = DurationType.Rounds,
                StateJson = "{\"FatHealingPerTick\":1,\"VitHealingPerTick\":1,\"BehaviorTags\":[\"healing\",\"end-of-round-trigger\"]}",
                Tags = "healing,regeneration,buff",
                IsSystem = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            },
            new()
            {
                Name = "Blinded",
                EffectType = EffectType.Condition,
                Description = "Cannot see, severely impacting combat and awareness.",
                IconName = "bi-eye-slash",
                Color = "#343a40",
                DefaultDurationValue = 3,
                DurationType = DurationType.Rounds,
                StateJson = "{\"ASModifier\":-6,\"BehaviorTags\":[\"condition\",\"sensory\"]}",
                Tags = "condition,sensory,debilitating",
                IsSystem = true,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-30)
            }
        };

        foreach (var template in templates)
        {
            await InsertTemplateAsync(template);
        }
    }

    private async Task InsertTemplateAsync(EffectTemplateDto template)
    {
        var sql = @"
            INSERT INTO EffectTemplates
            (Name, EffectType, Description, IconName, Color, DefaultDurationValue,
             DurationType, StateJson, Tags, IsSystem, IsActive, CreatedAt, UpdatedAt)
            VALUES
            (@Name, @EffectType, @Description, @IconName, @Color, @DefaultDurationValue,
             @DurationType, @StateJson, @Tags, @IsSystem, @IsActive, @CreatedAt, @UpdatedAt)
        ";

        using var command = Connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@Name", template.Name);
        command.Parameters.AddWithValue("@EffectType", (int)template.EffectType);
        command.Parameters.AddWithValue("@Description", template.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IconName", template.IconName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Color", template.Color ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@DefaultDurationValue", template.DefaultDurationValue ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@DurationType", (int)template.DurationType);
        command.Parameters.AddWithValue("@StateJson", template.StateJson ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Tags", template.Tags ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IsSystem", template.IsSystem ? 1 : 0);
        command.Parameters.AddWithValue("@IsActive", template.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("@CreatedAt", template.CreatedAt.ToString("o"));
        command.Parameters.AddWithValue("@UpdatedAt", template.UpdatedAt?.ToString("o") ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    private async Task InsertTemplateWithIdAsync(EffectTemplateDto template)
    {
        var sql = @"
            INSERT INTO EffectTemplates
            (Id, Name, EffectType, Description, IconName, Color, DefaultDurationValue,
             DurationType, StateJson, Tags, IsSystem, IsActive, CreatedAt, UpdatedAt)
            VALUES
            (@Id, @Name, @EffectType, @Description, @IconName, @Color, @DefaultDurationValue,
             @DurationType, @StateJson, @Tags, @IsSystem, @IsActive, @CreatedAt, @UpdatedAt)
        ";

        using var command = Connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@Id", template.Id);
        command.Parameters.AddWithValue("@Name", template.Name);
        command.Parameters.AddWithValue("@EffectType", (int)template.EffectType);
        command.Parameters.AddWithValue("@Description", template.Description ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IconName", template.IconName ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Color", template.Color ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@DefaultDurationValue", template.DefaultDurationValue ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@DurationType", (int)template.DurationType);
        command.Parameters.AddWithValue("@StateJson", template.StateJson ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@Tags", template.Tags ?? (object)DBNull.Value);
        command.Parameters.AddWithValue("@IsSystem", template.IsSystem ? 1 : 0);
        command.Parameters.AddWithValue("@IsActive", template.IsActive ? 1 : 0);
        command.Parameters.AddWithValue("@CreatedAt", template.CreatedAt.ToString("o"));
        command.Parameters.AddWithValue("@UpdatedAt", template.UpdatedAt?.ToString("o") ?? (object)DBNull.Value);

        await command.ExecuteNonQueryAsync();
    }

    public async Task<List<EffectTemplateDto>> GetAllTemplatesAsync()
    {
        await InitializeTableAsync();

        try
        {
            var sql = "SELECT * FROM EffectTemplates WHERE IsActive = 1";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            using var reader = await command.ExecuteReaderAsync();

            var templates = new List<EffectTemplateDto>();
            while (await reader.ReadAsync())
            {
                templates.Add(ReadTemplate(reader));
            }
            return templates;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting effect templates", ex);
        }
    }

    public async Task<List<EffectTemplateDto>> GetTemplatesByTypeAsync(EffectType type)
    {
        await InitializeTableAsync();

        try
        {
            var sql = "SELECT * FROM EffectTemplates WHERE IsActive = 1 AND EffectType = @EffectType";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@EffectType", (int)type);
            using var reader = await command.ExecuteReaderAsync();

            var templates = new List<EffectTemplateDto>();
            while (await reader.ReadAsync())
            {
                templates.Add(ReadTemplate(reader));
            }
            return templates;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting effect templates by type", ex);
        }
    }

    public async Task<List<EffectTemplateDto>> SearchTemplatesAsync(string searchTerm)
    {
        await InitializeTableAsync();

        try
        {
            var term = $"%{searchTerm}%";
            var sql = @"
                SELECT * FROM EffectTemplates
                WHERE IsActive = 1
                AND (Name LIKE @Term OR Tags LIKE @Term OR Description LIKE @Term)
            ";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Term", term);
            using var reader = await command.ExecuteReaderAsync();

            var templates = new List<EffectTemplateDto>();
            while (await reader.ReadAsync())
            {
                templates.Add(ReadTemplate(reader));
            }
            return templates;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error searching effect templates", ex);
        }
    }

    public async Task<EffectTemplateDto?> GetTemplateAsync(int id)
    {
        await InitializeTableAsync();

        try
        {
            var sql = "SELECT * FROM EffectTemplates WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id);
            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return ReadTemplate(reader);
            }
            return null;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error getting effect template", ex);
        }
    }

    public async Task<EffectTemplateDto> SaveTemplateAsync(EffectTemplateDto template)
    {
        await InitializeTableAsync();

        try
        {
            if (template.Id <= 0)
            {
                // Insert
                template.CreatedAt = DateTime.UtcNow;
                await InsertTemplateAsync(template);

                using var idCommand = Connection.CreateCommand();
                idCommand.CommandText = "SELECT last_insert_rowid()";
                long? lastInsertId = (long?)await idCommand.ExecuteScalarAsync();
                if (lastInsertId.HasValue)
                {
                    template.Id = (int)lastInsertId.Value;
                }
            }
            else
            {
                // Check if exists
                using var checkCommand = Connection.CreateCommand();
                checkCommand.CommandText = "SELECT COUNT(*) FROM EffectTemplates WHERE Id = @Id";
                checkCommand.Parameters.AddWithValue("@Id", template.Id);
                var exists = (long)(await checkCommand.ExecuteScalarAsync() ?? 0) > 0;

                if (exists)
                {
                    // Update
                    template.UpdatedAt = DateTime.UtcNow;

                    var sql = @"
                        UPDATE EffectTemplates SET
                            Name = @Name,
                            EffectType = @EffectType,
                            Description = @Description,
                            IconName = @IconName,
                            Color = @Color,
                            DefaultDurationValue = @DefaultDurationValue,
                            DurationType = @DurationType,
                            StateJson = @StateJson,
                            Tags = @Tags,
                            IsSystem = @IsSystem,
                            IsActive = @IsActive,
                            UpdatedAt = @UpdatedAt
                        WHERE Id = @Id
                    ";

                    using var command = Connection.CreateCommand();
                    command.CommandText = sql;
                    command.Parameters.AddWithValue("@Id", template.Id);
                    command.Parameters.AddWithValue("@Name", template.Name);
                    command.Parameters.AddWithValue("@EffectType", (int)template.EffectType);
                    command.Parameters.AddWithValue("@Description", template.Description ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@IconName", template.IconName ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Color", template.Color ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DefaultDurationValue", template.DefaultDurationValue ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@DurationType", (int)template.DurationType);
                    command.Parameters.AddWithValue("@StateJson", template.StateJson ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Tags", template.Tags ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@IsSystem", template.IsSystem ? 1 : 0);
                    command.Parameters.AddWithValue("@IsActive", template.IsActive ? 1 : 0);
                    command.Parameters.AddWithValue("@UpdatedAt", template.UpdatedAt?.ToString("o") ?? (object)DBNull.Value);

                    await command.ExecuteNonQueryAsync();
                }
                else
                {
                    // Insert with explicit Id
                    template.CreatedAt = DateTime.UtcNow;
                    await InsertTemplateWithIdAsync(template);
                }
            }

            return template;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error saving effect template", ex);
        }
    }

    public async Task DeleteTemplateAsync(int id)
    {
        await InitializeTableAsync();

        try
        {
            // Check if it's a system template
            var template = await GetTemplateAsync(id);
            if (template == null)
                throw new NotFoundException($"EffectTemplate {id}");
            if (template.IsSystem)
                throw new InvalidOperationException("Cannot delete system templates");

            var sql = "UPDATE EffectTemplates SET IsActive = 0, UpdatedAt = @UpdatedAt WHERE Id = @Id";
            using var command = Connection.CreateCommand();
            command.CommandText = sql;
            command.Parameters.AddWithValue("@Id", id);
            command.Parameters.AddWithValue("@UpdatedAt", DateTime.UtcNow.ToString("o"));

            await command.ExecuteNonQueryAsync();
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (InvalidOperationException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new OperationFailedException("Error deleting effect template", ex);
        }
    }

    private static EffectTemplateDto ReadTemplate(SqliteDataReader reader)
    {
        return new EffectTemplateDto
        {
            Id = reader.GetInt32(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            EffectType = (EffectType)reader.GetInt32(reader.GetOrdinal("EffectType")),
            Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),
            IconName = reader.IsDBNull(reader.GetOrdinal("IconName")) ? null : reader.GetString(reader.GetOrdinal("IconName")),
            Color = reader.IsDBNull(reader.GetOrdinal("Color")) ? null : reader.GetString(reader.GetOrdinal("Color")),
            DefaultDurationValue = reader.IsDBNull(reader.GetOrdinal("DefaultDurationValue")) ? null : reader.GetInt32(reader.GetOrdinal("DefaultDurationValue")),
            DurationType = (DurationType)reader.GetInt32(reader.GetOrdinal("DurationType")),
            StateJson = reader.IsDBNull(reader.GetOrdinal("StateJson")) ? null : reader.GetString(reader.GetOrdinal("StateJson")),
            Tags = reader.IsDBNull(reader.GetOrdinal("Tags")) ? null : reader.GetString(reader.GetOrdinal("Tags")),
            IsSystem = reader.GetInt32(reader.GetOrdinal("IsSystem")) == 1,
            IsActive = reader.GetInt32(reader.GetOrdinal("IsActive")) == 1,
            CreatedAt = DateTime.Parse(reader.GetString(reader.GetOrdinal("CreatedAt"))),
            UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : DateTime.Parse(reader.GetString(reader.GetOrdinal("UpdatedAt")))
        };
    }
}
