using Microsoft.Data.Sqlite;

namespace Backend.Data;

public sealed class Db
{
    private readonly string _connectionString;

    public Db(IConfiguration configuration, IWebHostEnvironment env)
    {
        var configured = configuration.GetConnectionString("Default");
        if (string.IsNullOrWhiteSpace(configured))
        {
            var dbPath = Path.GetFullPath(Path.Combine(env.ContentRootPath, "..", "Database.db"));
            _connectionString = $"Data Source={dbPath}";
        }
        else
        {
            _connectionString = configured;
        }
    }

    public SqliteConnection OpenConnection()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();

        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();

        return connection;
    }
}
