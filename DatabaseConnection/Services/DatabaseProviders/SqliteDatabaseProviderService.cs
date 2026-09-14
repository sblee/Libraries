using DatabaseConnection.Interfaces.Services;
using Microsoft.Data.Sqlite;
using System.Data;

namespace DatabaseConnection.Services.DatabaseProviders;

internal class SqliteDatabaseProviderService : IDatabaseQueryProviderService
{
    public async Task<DataTable> ExecuteDataTableAsync(string connectionString, string sql, Dictionary<string, object>? parameters, int sqlTimeout, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }

        command.CommandText = sql;
        command.CommandTimeout = sqlTimeout;

        DataTable dataTable = new();

        using SqliteDataReader reader = await command.ExecuteReaderAsync(cancellationToken);
        dataTable.Load(reader);

        return dataTable;
    }

    public async Task ExecuteNonQueryAsync(string connectionString, string sql, Dictionary<string, object>? parameters, int sqlTimeout, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }

        command.CommandText = sql;
        command.CommandTimeout = sqlTimeout;
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}