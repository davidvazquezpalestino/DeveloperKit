namespace DevKit.ExecutionEngine.SQLServer.Abstractions;

/// <summary>
/// Interface for bulk copy operations in SQL Server.
/// </summary>
public partial interface ISQLServerProvider
{
    /// <summary>
    /// Performs a bulk insert from a DataTable to the target table synchronously.
    /// </summary>
    /// <param name="source">The source DataTable containing data to insert.</param>
    /// <param name="target">The target table name.</param>
    void ExecuteBulkInsert(DataTable source, string target);

    /// <summary>
    /// Performs a bulk insert from a DataTable to the target table asynchronously.
    /// </summary>
    /// <param name="source">The source DataTable containing data to insert.</param>
    /// <param name="target">The target table name.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    Task ExecuteBulkInsertAsync(DataTable source, string target, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a bulk insert from a DataTable to a new or existing table with the specified configuration asynchronously.
    /// </summary>
    /// <param name="source">The source DataTable containing data to insert.</param>
    /// <param name="configuration">The bulk copy configuration.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    Task ExecuteBulkInsertAsync(DataTable source, BulkOperationsConfiguration configuration, CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a bulk insert of a collection of entities with advanced configuration asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of entities to insert.</typeparam>
    /// <param name="entities">The collection of entities to insert.</param>
    /// <param name="configuration">The bulk copy configuration.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    Task ExecuteBulkInsertAsync<T>(IEnumerable<T> entities, BulkOperationsConfiguration configuration, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Performs a bulk insert of a collection of entities with a fluent configuration asynchronously.
    /// </summary>
    /// <typeparam name="T">The type of entities to insert.</typeparam>
    /// <param name="entities">The collection of entities to insert.</param>
    /// <param name="configure">Action to configure the bulk copy operation.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    Task ExecuteBulkInsertAsync<T>(IEnumerable<T> entities, Action<BulkOperationsConfigurationBuilder> configure, CancellationToken cancellationToken = default) where T : class;


}

