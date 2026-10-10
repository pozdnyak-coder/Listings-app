using System.Data;
using Microsoft.EntityFrameworkCore;

namespace ListingsApi.Data;

/// <summary>
/// Applies EF Core migrations on startup. Databases previously created with
/// EnsureCreated are adopted only when their required columns match the initial schema.
/// Back up a production SQLite file before deploying schema changes.
/// </summary>
public static class DatabaseBootstrap
{
    public static void Apply(AppDbContext db)
    {
        var migrations = db.Database.GetMigrations().ToArray();
        if (migrations.Length == 0)
            throw new InvalidOperationException(
                "EF Core migrations were not found. Add a migration before starting the application.");

        // Compatibility path for databases created by the previous EnsureCreated startup.
        // Never mark an unknown/partial schema as migrated.
        if (TableExists(db, "Districts") || TableExists(db, "Listings"))
        {
            if (!TableExists(db, "Districts") || !TableExists(db, "Listings"))
                throw new InvalidOperationException(
                    "The SQLite database has an incomplete schema (Districts/Listings). Back it up and repair it before startup.");

            if (!TableExists(db, "__EFMigrationsHistory"))
            {
                RequireColumns(db, "Districts", "Id", "Name");
                RequireColumns(db, "Listings", "Id", "Title", "Price", "Rooms", "Address", "CreatedAt", "DistrictId");

                db.Database.ExecuteSqlRaw(
                    "CREATE TABLE \"__EFMigrationsHistory\" (" +
                    "\"MigrationId\" TEXT NOT NULL CONSTRAINT \"PK___EFMigrationsHistory\" PRIMARY KEY, " +
                    "\"ProductVersion\" TEXT NOT NULL);");

                // The schema was created from the same current EF model by EnsureCreated.
                // Adopt only the known initial migration; keep all existing rows untouched.
                var productVersion = "10.0.0";
                db.Database.ExecuteSqlInterpolated(
                    $"INSERT INTO \"__EFMigrationsHistory\" (\"MigrationId\", \"ProductVersion\") VALUES ({migrations[0]}, {productVersion});");
            }
        }

        db.Database.Migrate();
    }

    private static void RequireColumns(AppDbContext db, string table, params string[] required)
    {
        var columns = GetColumns(db, table);
        var missing = required.Where(column => !columns.Contains(column, StringComparer.OrdinalIgnoreCase)).ToArray();
        if (missing.Length > 0)
            throw new InvalidOperationException(
                $"SQLite table '{table}' is missing required column(s): {string.Join(", ", missing)}. " +
                "The database was not modified; back it up and create a reviewed migration for this schema.");
    }

    private static HashSet<string> GetColumns(AppDbContext db, string table)
    {
        var connection = db.Database.GetDbConnection();
        var wasClosed = connection.State != ConnectionState.Open;
        if (wasClosed) connection.Open();
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = $"PRAGMA table_info(\"{table}\")";
            using var reader = command.ExecuteReader();
            var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            while (reader.Read()) columns.Add(reader.GetString(1));
            return columns;
        }
        finally
        {
            if (wasClosed) connection.Close();
        }
    }

    private static bool TableExists(AppDbContext db, string name)
    {
        var connection = db.Database.GetDbConnection();
        var wasClosed = connection.State != ConnectionState.Open;
        if (wasClosed) connection.Open();
        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type = 'table' AND name = $name";
            var parameter = command.CreateParameter();
            parameter.ParameterName = "$name";
            parameter.Value = name;
            command.Parameters.Add(parameter);
            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
        finally
        {
            if (wasClosed) connection.Close();
        }
    }
}
