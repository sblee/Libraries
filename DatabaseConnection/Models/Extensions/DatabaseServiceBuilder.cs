using Microsoft.Extensions.DependencyInjection;

namespace DatabaseConnection.Models.Extensions;

public sealed class DatabaseServiceBuilder(IServiceCollection services)
{
    public IServiceCollection Services { get; } = services;
}