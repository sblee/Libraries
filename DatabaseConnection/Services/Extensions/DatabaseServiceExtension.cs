using DatabaseConnection.Interfaces.Services;
using DatabaseConnection.Models;
using DatabaseConnection.Models.Extensions;
using DatabaseConnection.Services.DatabaseProviders;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseConnection.Services.Extensions;

public static class DatabaseServiceExtension
{
    public static DatabaseServiceBuilder<TKey> AddDatabaseService<TKey>(this IServiceCollection services) where TKey : notnull
    {
        services.Configure<DatabaseConnectionOption<TKey>>(config =>
        {
            config.ConnectionConfigDictionary = [];
        });

        services.AddTransient<IDatabaseService, DatabaseService<TKey>>();
        services.AddKeyedTransient<IDatabaseProviderService, SqlServerDatabaseProviderService>(DatabaseProviderTypes.SqlServer);

        DatabaseServiceBuilder<TKey> builder = new(services);

        return builder;
    }

    public static DatabaseServiceBuilder<TKey> AddConnection<TKey>(this DatabaseServiceBuilder<TKey> databaseServiceBuilder, TKey key, Action<DatabaseConnectionExtensionConfig> action) where TKey : notnull
    {
        databaseServiceBuilder.Services.Configure<DatabaseConnectionOption<TKey>>(options =>
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