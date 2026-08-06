using Microsoft.Extensions.DependencyInjection;

namespace DatabaseConnection.Models.Extensions;

public sealed class DatabaseServiceBuilder<TKey>(IServiceCollection services) where TKey : notnull
{
    public IServiceCollection Services { get; } = services;
}