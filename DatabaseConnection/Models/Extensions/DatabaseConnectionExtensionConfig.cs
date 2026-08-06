namespace DatabaseConnection.Models.Extensions;

public class DatabaseConnectionExtensionConfig
{
    public DatabaseProviderTypes DatabaseProviderType { get; set; }

    public string ConnectionString { get; set; } = default!;

    public bool IsDefault { get; set; } = false;
}