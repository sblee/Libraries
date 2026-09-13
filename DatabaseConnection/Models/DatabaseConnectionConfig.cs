namespace DatabaseConnection.Models;

internal class DatabaseConnectionConfig 
{
    public DatabaseProviderTypes DatabaseProviderType { get; set; }

    public required Type DatabaseProviderInterfaceType { get; set; }

    public required string ConnectionString { get; set; }

    public bool IsDefault { get; set; }
}