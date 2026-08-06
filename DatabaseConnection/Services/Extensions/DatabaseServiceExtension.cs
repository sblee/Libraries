using DatabaseConnection.Interfaces.Services;
using DatabaseConnection.Models;
using DatabaseConnection.Models.Extensions;
using DatabaseConnection.Services.DatabaseProviders;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseConnection.Services.Extensions;

public static class DatabaseServiceExtension
{
    public static DatabaseServiceBuilder AddDatabaseService(this IServiceCollection services)
    {
        services.Configure<DatabaseConnectionOption>(config =>
        {
            config.ConnectionConfigDictionary = [];
        });

        services.AddTransient<IDatabaseService, DatabaseService>();
        services.AddKeyedTransient<IDatabaseProviderService, SqlServerDatabaseProviderService>(DatabaseProviderTypes.SqlServer);

        DatabaseServiceBuilder builder = new(services);

        return builder;
    }

    public static DatabaseServiceBuilder AddConnection(this DatabaseServiceBuilder databaseServiceBuilder, string key, Action<DatabaseConnectionExtensionConfig> action)
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
                DatabaseProviderType = tempConfig.DatabaseProviderType,
                ConnectionString = tempConfig.ConnectionString,
                IsDefault = tempConfig.IsDefault
            };
        });

        return databaseServiceBuilder;
    }
}