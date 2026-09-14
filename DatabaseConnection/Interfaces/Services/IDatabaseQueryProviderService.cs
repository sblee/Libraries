using System.Data;

namespace DatabaseConnection.Interfaces.Services;

internal interface IDatabaseQueryProviderService
{
    Task ExecuteNonQueryAsync(string connectionString, string sql, Dictionary<string, object>? parameters, int sqlTimeout, CancellationToken cancellationToken = default);

    Task<DataTable> ExecuteDataTableAsync(string connectionString, string sql, Dictionary<string, object>? parameters, int sqlTimeout, CancellationToken cancellationToken = default);
}