namespace DevKit.ExecutionEngine.Abstractions.Interfaces.SqlServer;

/// <summary>
/// Interfaz que extiende IOracleRepository con operaciones asíncronas.
/// Proporciona métodos asíncronos para ejecutar consultas y procedimientos almacenados.
/// </summary>
public partial interface ISQLServerDatabaseProvider
{
    /// <summary>
    /// Ejecuta una consulta y devuelve los resultados como lista de listas de diccionarios.
    /// </summary>
    Task<IList<IList<Dictionary<string, object>>>> ExecuteMultiResultQueryAsync(
        string query,
        Action<IDataParameterCollection> parametros = null,
        Action<string> logger = null);



    /// <summary>
    /// Copia masivamente datos desde un IDataReader a la tabla destino.
    /// </summary>
    /// <summary>
    /// Copia masivamente datos con configuración avanzada.
    /// </summary>
    Task ExecuteBulkInsertAsync(DataTable source, BulkOperationsConfiguration configuration);

    /// <summary>
    /// Copia masivamente una colección de entidades con configuración avanzada.
    /// </summary>
    Task ExecuteBulkInsertAsync<T>(IEnumerable<T> entities, BulkOperationsConfiguration configuration) where T : class;
    /// <summary>
    /// Copia masivamente una colección de entidades con configuración fluida.
    /// </summary>
    Task ExecuteBulkInsertAsync<T>(IEnumerable<T> entities, Action<BulkOperationsConfigurationBuilder> configure) where T : class;
    /// <summary>
    /// Inserta una entidad en la tabla especificada.
    /// </summary>
    Task ExecuteInsertAsync<T>(string tableName, T entity);
    /// <summary>
    /// Inserta una colección de entidades en la tabla especificada con configuración de lote.
    /// </summary>
    Task ExecuteInsertAsync<T>(string tableName, ICollection<T> entities, int batchSize = 1000) where T : class, new();

    /// <summary>
    /// Obtiene la fecha y hora actuales del servidor de forma asíncrona.
    /// </summary>
    Task<DateTime> GetCurrentDateTimeAsync();
}