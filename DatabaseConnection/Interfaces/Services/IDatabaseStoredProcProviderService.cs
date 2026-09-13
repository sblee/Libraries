using Microsoft.Data.SqlClient;

namespace DatabaseConnection.Interfaces.Services;

internal interface IDatabaseStoredProcProviderService
{
    Task<IEnumerable<TReturn>> ExecuteStoredProcedureAsync<TReturn>(string connectionString, string storedProcedureName, Func<SqlDataReader, TReturn> mapRow, Dictionary<string, object>? parameters, int sqlTimeout, CancellationToken cancellationToken);
}