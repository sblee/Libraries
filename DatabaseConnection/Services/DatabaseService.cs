using DatabaseConnection.Interfaces.Services;
using DatabaseConnection.Models;
using DatabaseConnection.Models.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DatabaseConnection.Services;

internal class DatabaseService(IServiceProvider serviceProvider, IOptions<DatabaseConnectionOption> options, ILogger<DatabaseService> logger) : IDatabaseService
{
    #region Publics

    public async Task<IEnumerable<TReturn>> ExecuteStoredProcedureAsync<TReturn>(string storedProcedureName, Func<SqlDataReader, TReturn> mapRow, string? connectionConfigKey = default, Dictionary<string, object>? parameters = null, int sqlTimeout = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            DatabaseConnectionConfig connectionConfig = GetDatabaseConnectionConfig(connectionConfigKey) ?? throw new InvalidOperationException("Database connection configuration not found.");

            IDatabaseProviderService databaseProviderService = serviceProvider.GetRequiredKeyedService<IDatabaseProviderService>(connectionConfig.DatabaseProviderType);

            return await databaseProviderService.ExecuteStoredProcedureAsync(connectionConfig.ConnectionString, storedProcedureName, mapRow, parameters, sqlTimeout, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing stored procedure {StoredProcedureName}", storedProcedureName);
            throw;
        }
    }

    #endregion Publics

    #region Privates

    private DatabaseConnectionConfig? GetDatabaseConnectionConfig(string? connectionConfigKey) 
    {
        if(connectionConfigKey == null) 
        {
            return options.Value.ConnectionConfigDictionary.First(f => f.Value.IsDefault).Value;
        }

        return options.Value.ConnectionConfigDictionary.TryGetValue(connectionConfigKey, out DatabaseConnectionConfig? config) ? config : null;
    }

    #endregion Privates
}