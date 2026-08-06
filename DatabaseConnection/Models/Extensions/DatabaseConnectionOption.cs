namespace DatabaseConnection.Models.Extensions;

internal class DatabaseConnectionOption
{
    /// <summary>
    /// Support multiple connection strings for different databases. The key is the connection string key, and the value is the connection string.
    /// </summary>
    public required Dictionary<string, DatabaseConnectionConfig> ConnectionConfigDictionary { get; set; }
}