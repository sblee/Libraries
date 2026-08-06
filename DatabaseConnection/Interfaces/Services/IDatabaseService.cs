using Microsoft.Data.SqlClient;

namespace DatabaseConnection.Interfaces.Services;

public interface IDatabaseService
{
    /// <summary>
    /// Executes a stored procedure asynchronously and maps the result set to a collection of TReturn objects.
    /// </summary>
    /// <typeparam name="TReturn">The type of objects to return.</typeparam>
    /// <param name="storedProcedureName">The name of the stored procedure to execute.</param>
    /// <param name="mapRow">A function to map each row of the result set to a TReturn object.</param>
    /// <param name="databaseConnectionKey">An optional key to select a specific database connection string.</param>
    /// <param name="parameters">The parameters for the stored procedure.</param>
    /// <param name="sqlTimeout">The timeout for the SQL command.</param>
    /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
    /// <returns></returns>
    Task<IEnumerable<TReturn>> ExecuteStoredProcedureAsync<TReturn>(string storedProcedureName, Func<SqlDataReader, TReturn> mapRow, string? databaseConnectionKey = default, Dictionary<string, object>? parameters = null, int sqlTimeout = 30, CancellationToken cancellationToken = default);
}