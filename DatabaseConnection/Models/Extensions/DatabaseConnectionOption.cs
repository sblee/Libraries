namespace DatabaseConnection.Models.Extensions;

internal class DatabaseConnectionOption<TKey> where TKey : notnull
{
    /// <summary>
    /// Support multiple connection strings for different databases. The key is the connection string key, and the value is the connection string.
    /// </summary>
    public required Dictionary<TKey, DatabaseConnectionConfig> ConnectionConfigDictionary { get; set; }
}