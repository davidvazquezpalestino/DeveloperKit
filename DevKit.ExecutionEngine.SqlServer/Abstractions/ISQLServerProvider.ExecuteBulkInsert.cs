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
    /// Realiza una copia masiva de un DataTable a la tabla destino de forma asíncrona.
    /// </summary>
    /// <param name="source">DataTable que contiene los datos a insertar.</param>
    /// <param name="target">Nombre de la tabla de destino.</param>
    /// <param name="cancellationToken">Token de cancelación para la operación asíncrona.</param>
    /// <returns>Tarea asíncrona que representa la operación.</returns>
    Task ExecuteBulkInsertToTableAsync(DataTable source, string target, CancellationToken cancellationToken = default);

    /// <summary>
    /// Realiza una copia masiva de un DataTable a la tabla destino de forma asíncrona.
    /// </summary>
    /// <param name="source">DataTable que contiene los datos a insertar.</param>
    /// <param name="target">Nombre de la tabla de destino.</param>
    /// <returns>Tarea asíncrona que representa la operación.</returns>
    void ExecuteBulkInsertToTable(DataTable source, string target);
}

