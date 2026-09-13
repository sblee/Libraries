using DatabaseConnection.Interfaces.Services;
using DatabaseConnection.Models;
using DatabaseConnection.Models.Extensions;
using DatabaseConnection.Services.DatabaseProviders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DatabaseConnection.Services.Extensions;

public static class DatabaseServiceExtension
{
    /// <summary>
    /// Add database service
    /// </summary>
    /// <param name="services">service collection</param>
    /// <returns></returns>
    public static DatabaseServiceBuilder AddDatabaseService(this IServiceCollection services)
    {
        services.AddOptions<DatabaseConnectionOption>()
            .Configure(config => config.ConnectionConfigDictionary = [])
            .ValidateOnStart();

        services.AddTransient<IDatabaseService, DatabaseService>();

        DatabaseServiceBuilder builder = new(services);

        return builder;
    }

    /// <summary>
    /// Add database connection information
    /// </summary>
    /// <param name="databaseServiceBuilder">database service builder</param>
    /// <param name="key">database connection key</param>
    /// <param name="action">database connection information action</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public static DatabaseServiceBuilder AddSqlServerConnection(this DatabaseServiceBuilder databaseServiceBuilder, string key, Action<DatabaseConnectionExtensionConfig> action)
    {
        databaseServiceBuilder.Services.Configure<DatabaseConnectionOption>(options =>
        {
            if (options.ConnectionConfigDictionary.TryGetValue(key, out _))
            {
                throw new InvalidOperationException($"A connection with the key '{key}' already exists.");
            }

            DatabaseConnectionExtensionConfig tempConfig = new();
            action(tempConfig);

            if (string.IsNullOrWhiteSpace(tempConfig.ConnectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.");
            }

            options.ConnectionConfigDictionary[key] = new() 
            {
                DatabaseProviderType = DatabaseProviderTypes.SqlServer,
                DatabaseProviderInterfaceType = typeof(IDatabaseProviderService),
                ConnectionString = tempConfig.ConnectionString,
                IsDefault = tempConfig.IsDefault
            };
        });

        databaseServiceBuilder.Services.TryAddKeyedTransient<IDatabaseProviderService, SqlServerDatabaseProviderService>(DatabaseProviderTypes.SqlServer);

        return databaseServiceBuilder;
    }

    public static DatabaseServiceBuilder AddSqliteConnection(this DatabaseServiceBuilder databaseServiceBuilder, string key, Action<DatabaseConnectionExtensionConfig> action)
    {
        databaseServiceBuilder.Services.Configure<DatabaseConnectionOption>(options =>
        {
            if (options.ConnectionConfigDictionary.TryGetValue(key, out _))
            {
                throw new InvalidOperationException($"A connection with the key '{key}' already exists.");
            }

            DatabaseConnectionExtensionConfig tempConfig = new();
            action(tempConfig);

            if (string.IsNullOrWhiteSpace(tempConfig.ConnectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.");
            }

            options.ConnectionConfigDictionary[key] = new()
            {
                DatabaseProviderType = DatabaseProviderTypes.Sqlite,
                DatabaseProviderInterfaceType = typeof(IDatabaseQueryProviderService),
                ConnectionString = tempConfig.ConnectionString,
                IsDefault = tempConfig.IsDefault
            };
        });

        databaseServiceBuilder.Services.TryAddKeyedTransient<IDatabaseQueryProviderService, SqliteDatabaseProviderService>(DatabaseProviderTypes.Sqlite);

        SQLitePCL.Batteries_V2.Init();

        return databaseServiceBuilder;
    }
}