using DatabaseConnection.Interfaces.Services;
using DatabaseConnection.Models;
using DatabaseConnection.Models.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DatabaseConnection.Services;

internal class DatabaseService<TKey>(IServiceProvider serviceProvider, IOptions<DatabaseConnectionOption<TKey>> options, ILogger<DatabaseService<TKey>> logger) : IDatabaseService<TKey> where TKey : notnull
{
    #region Publics

    public Task<IEnumerable<TReturn>> ExecuteStoredProcedureAsync<TReturn>(string storedProcedureName, Func<SqlDataReader, TReturn> mapRow, TKey? connectionConfigKey = default, Dictionary<string, object>? parameters = null, int sqlTimeout = 30, CancellationToken cancellationToken = default)
    {
        try
        {
            DatabaseConnectionConfig connectionConfig = GetDatabaseConnectionConfig(connectionConfigKey) ?? throw new InvalidOperationException("Database connection configuration not found.");

            IDatabaseProviderService databaseProviderService = serviceProvider.GetRequiredKeyedService<IDatabaseProviderService>(connectionConfig.DatabaseProviderType);

            return databaseProviderService.ExecuteStoredProcedureAsync(connectionConfig.ConnectionString, storedProcedureName, mapRow, parameters, sqlTimeout, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing stored procedure {StoredProcedureName}", storedProcedureName);
            throw;
        }
    }

    #endregion Publics

    #region Privates

    private DatabaseConnectionConfig? GetDatabaseConnectionConfig(TKey? connectionConfigKey) 
    {
        if(connectionConfigKey == null) 
        {
            return options.Value.ConnectionConfigDictionary.First(f => f.Value.IsDefault).Value;
        }

        return options.Value.ConnectionConfigDictionary.TryGetValue(connectionConfigKey, out DatabaseConnectionConfig? config) ? config : null;
    }

    #endregion Privates
}