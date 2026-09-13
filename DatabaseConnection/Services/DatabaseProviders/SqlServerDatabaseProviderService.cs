using DatabaseConnection.Interfaces.Services;
using Microsoft.Data.SqlClient;
using System.Data;

namespace DatabaseConnection.Services.DatabaseProviders;

internal class SqlServerDatabaseProviderService : IDatabaseProviderService
{
    #region Publics

    public async Task<IEnumerable<TReturn>> ExecuteStoredProcedureAsync<TReturn>(string connectionString, string storedProcedureName, Func<SqlDataReader, TReturn> mapRow, Dictionary<string, object>? parameters, int sqlTimeout, CancellationToken cancellationToken)
    {
        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using SqlCommand command = new(storedProcedureName, connection);
        command.CommandType = CommandType.StoredProcedure;
        command.CommandTimeout = sqlTimeout;

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }

        await using SqlDataReader reader = await command.ExecuteReaderAsync(cancellationToken);

        List<TReturn> results = [];
        while (await reader.ReadAsync(cancellationToken))
        {
            results.Add(mapRow(reader));
        }

        return results;
    }

    public async Task ExecuteNonQueryAsync(string connectionString, string sql, Dictionary<string, object>? parameters, int sqlTimeout, CancellationToken cancellationToken = default) 
    {
        await using SqlConnection connection = new(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using SqlCommand command = connection.CreateCommand();

        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                command.Parameters.AddWithValue(param.Key, param.Value ?? DBNull.Value);
            }
        }

        command.CommandText = sql;
        command.CommandType = CommandType.Text;
        command.CommandTimeout = sqlTimeout;

        await command.ExecuteNonQueryAsync(cancellationToken);
    }

    #endregion Publics
}