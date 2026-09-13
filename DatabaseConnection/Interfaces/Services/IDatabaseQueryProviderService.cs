namespace DatabaseConnection.Interfaces.Services;

internal interface IDatabaseQueryProviderService
{
    Task ExecuteNonQueryAsync(string connectionString, string sql, Dictionary<string, object>? parameters, int sqlTimeout, CancellationToken cancellationToken = default);
}