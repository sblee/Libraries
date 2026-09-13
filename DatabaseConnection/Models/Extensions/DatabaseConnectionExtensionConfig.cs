namespace DatabaseConnection.Models.Extensions;

public class DatabaseConnectionExtensionConfig
{
    public string? ConnectionString { get; set; }

    public bool IsDefault { get; set; } = false;
}