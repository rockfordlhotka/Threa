# SQLite Database Migrations

## Overview

The SQLite DAL implementations now include a migration system to handle schema changes without losing existing data.

## How It Works

### Schema Versioning

A `SchemaVersion` table tracks which migrations have been applied:

```sql
CREATE TABLE IF NOT EXISTS SchemaVersion (
    Version INTEGER NOT NULL PRIMARY KEY,
    AppliedAt TEXT NOT NULL
);
```

### Migration Execution

When a DAL class initializes:

1. Creates the `SchemaVersion` table if it doesn't exist
2. Creates base tables using `CREATE TABLE IF NOT EXISTS`
3. Runs `RunMigrations()` which checks the current version and applies any pending migrations
4. Each migration is idempotent (safe to run multiple times)

### Current Migrations

#### Migration 1: Add PlayerId to TableCharacters

**Applied to:** `TableDal`

**Purpose:** Adds the `PlayerId` column to the `TableCharacters` table to support player associations.

**Changes:**
```sql
ALTER TABLE TableCharacters ADD COLUMN PlayerId INTEGER NOT NULL DEFAULT 0
```

**Note:** Uses a default value of 0 to handle existing rows. Application code should update these values as needed.

## Adding New Migrations

To add a new migration to a DAL class:

1. **Add the migration logic in `RunMigrations()`:**

```csharp
// Migration 2: Your migration description
if (currentVersion < 2)
{
    try
    {
        // Check if change already exists (idempotent)
        // Apply the migration
        var migrationSql = "ALTER TABLE ...";
        using var migrationCommand = Connection.CreateCommand();
        migrationCommand.CommandText = migrationSql;
        migrationCommand.ExecuteNonQuery();

        SetSchemaVersion(2);
    }
    catch (Exception ex)
    {
        throw new OperationFailedException("Error running migration 2", ex);
    }
}
```

2. **Make migrations idempotent** by checking if the change already exists before applying
3. **Increment the version number** sequentially
4. **Document the migration** in this file

## Best Practices

- **Always preserve existing data** - use `ALTER TABLE ADD COLUMN` rather than `DROP`/`CREATE`
- **Use default values** for new NOT NULL columns to handle existing rows
- **Check before modifying** - verify a column/table doesn't exist before adding it
- **Wrap in try-catch** - provide clear error messages
- **Test with existing data** - verify migrations work on databases with actual data
- **Sequential versioning** - use consecutive integers (1, 2, 3, ...)
- **One-way migrations** - we don't support rollback, only forward migration

## SQLite Limitations

SQLite has limited `ALTER TABLE` support:
- ✅ Can add columns
- ❌ Cannot drop columns (requires table recreation)
- ❌ Cannot modify column types (requires table recreation)
- ❌ Cannot rename columns in older SQLite versions

For unsupported operations, you must:
1. Create a new table with the desired schema
2. Copy data from the old table
3. Drop the old table
4. Rename the new table

## Testing Migrations

To test a migration:

1. Create a database with the old schema
2. Add some test data
3. Run the application (which will trigger migrations)
4. Verify the data is preserved and the new schema is correct

## Future Improvements

Consider implementing:
- Migration rollback support
- SQL script-based migrations (separate .sql files)
- Migration testing utilities
- Automatic backup before migration
- Schema validation after migration
