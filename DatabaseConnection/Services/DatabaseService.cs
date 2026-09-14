using DatabaseConnection.Interfaces.Services;
using DatabaseConnection.Models;
using DatabaseConnection.Models.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Text.RegularExpressions;

namespace DatabaseConnection.Services;

internal partial class DatabaseService(IServiceProvider serviceProvider, IOptions<DatabaseConnectionOption> options, ILogger<DatabaseService> logger) : IDatabaseService
{
    #region Publics

    /// <summary>
    /// Executes a stored procedure asynchronously and maps the result set to a collection of TReturn objects.
    /// </summary>
    /// <typeparam name="TReturn">The type of objects to return.</typeparam>
    /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
    /// <param name="mapRow">A function to map each row of the result set to a TReturn object.</param>
    /// <param name="connectionConfigKey">An optional key to select a specific database connection string.</param>
    /// <param name="parameters">The parameters for the stored procedure.</param>
    /// <param name="sqlTimeout">The timeout for the SQL command.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns></returns>
    public async Task<IEnumerable<TReturn>> ExecuteStoredProcedureAsync<TReturn>(string storedProcedureName, Func<IDataReader, TReturn> mapRow, string? connectionConfigKey = default, Dictionary<string, object>? parameters = null, int sqlTimeout = 30, CancellationToken cancellationToken = default)
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

    /// <summary>
    /// Execute queries in sql files
    /// </summary>
    /// <param name="filePath">The file path</param>
    /// <param name="connectionConfigKey"></param>
    /// <param name="parameters"></param>
    /// <param name="sqlTimeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<IEnumerable<(bool isSuccess, string batchQuery, Exception? exception)>> ExecuteSqlFileAsync(string filePath, string? connectionConfigKey = default, Dictionary<string, object>? parameters = null, int sqlTimeout = 30, CancellationToken cancellationToken = default) 
    {
        string script = await File.ReadAllTextAsync(filePath, cancellationToken);

        DatabaseConnectionConfig connectionConfig = GetDatabaseConnectionConfig(connectionConfigKey) ?? throw new InvalidOperationException("Database connection configuration not found.");

        var databaseQueryProviderService = serviceProvider.GetRequiredKeyedService(connectionConfig.DatabaseProviderInterfaceType, connectionConfig.DatabaseProviderType);

        IEnumerable<string> batches = SqlBatchesSplitRegex().Split(script);

        List<(bool isSuccess, string batchQuery, Exception? exception)> results = [];
        foreach (string batch in batches)
        {
            if (string.IsNullOrWhiteSpace(batch))
            {
                continue;
            }

            try 
            {
                await ((IDatabaseQueryProviderService)databaseQueryProviderService).ExecuteNonQueryAsync(connectionConfig.ConnectionString, batch, parameters, sqlTimeout, cancellationToken);

                results.Add((true, batch, null));
            }
            catch(Exception ex) 
            {
                logger.LogError(ex, "Error executing the following query in the file {filePath}: {batch}", filePath, batch);
                results.Add((false, batch, ex));
            }
        }

        return results;
    }

    public async Task<(bool isSuccess, Exception? exception)> ExecuteNonQueryAsync(string query, string? connectionConfigKey = default, Dictionary<string, object>? parameters = null, int sqlTimeout = 30, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(query))
        {
            return (false, new ArgumentException("Invalid null or empty query."));
        }

        DatabaseConnectionConfig? connectionConfig = GetDatabaseConnectionConfig(connectionConfigKey);
        if(connectionConfig == null)
        {
            return (false, new ArgumentException("Database connection configuration not found."));
        }

        try
        {
            var databaseQueryProviderService = serviceProvider.GetRequiredKeyedService(connectionConfig.DatabaseProviderInterfaceType, connectionConfig.DatabaseProviderType);

            await ((IDatabaseQueryProviderService)databaseQueryProviderService).ExecuteNonQueryAsync(connectionConfig.ConnectionString, query, parameters, sqlTimeout, cancellationToken);

            return (true, null);
        }
        catch(Exception ex)
        {
            logger.LogError(ex, "Error executing the following query: {query}", query);
            return (false, ex);
        }
    }

    public async Task<(bool isSuccess, DataTable? dataTable, Exception? exception)> ExecuteDataTableAsync(string query, string? connectionConfigKey = default, Dictionary<string, object>? parameters = null, int sqlTimeout = 30, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(query))
        {
            return (false, null, new ArgumentException("Invalid null or empty query."));
        }

        DatabaseConnectionConfig? connectionConfig = GetDatabaseConnectionConfig(connectionConfigKey);
        if (connectionConfig == null)
        {
            return (false, null, new ArgumentException("Database connection configuration not found."));
        }

        try
        {
            var databaseQueryProviderService = serviceProvider.GetRequiredKeyedService(connectionConfig.DatabaseProviderInterfaceType, connectionConfig.DatabaseProviderType);

            DataTable dataTable = await ((IDatabaseQueryProviderService)databaseQueryProviderService).ExecuteDataTableAsync(connectionConfig.ConnectionString, query, parameters, sqlTimeout, cancellationToken);

            return (true, dataTable, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error executing the following query: {query}", query);
            return (false, null, ex);
        }
    }

    #endregion Publics

    #region Privates

    private DatabaseConnectionConfig? GetDatabaseConnectionConfig(string? connectionConfigKey) 
    {
        if(connectionConfigKey == null) 
        {
            return options.Value.ConnectionConfigDictionary.FirstOrDefault(f => f.Value.IsDefault).Value;
        }

        return options.Value.ConnectionConfigDictionary.TryGetValue(connectionConfigKey, out DatabaseConnectionConfig? config) ? config : null;
    }

    [GeneratedRegex(@"^\s*GO\s*(?:--.*)?$", RegexOptions.IgnoreCase | RegexOptions.Multiline, "en-CA")]
    private static partial Regex SqlBatchesSplitRegex();

    #endregion Privates
}