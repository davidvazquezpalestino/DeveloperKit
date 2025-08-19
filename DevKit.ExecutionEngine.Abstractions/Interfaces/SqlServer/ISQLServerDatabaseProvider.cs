namespace DevKit.ExecutionEngine.Abstractions.Interfaces.SqlServer;

/// <summary>
/// Interfaz principal para el repositorio de Oracle que define operaciones básicas de base de datos.
/// Proporciona métodos para ejecutar consultas, procedimientos almacenados y operaciones de transacción.
/// </summary>
public partial interface ISQLServerDatabaseProvider : IDatabaseProvider, IDisposable
{

    /// <summary>
    /// Inserta una entidad en la tabla especificada.
    /// </summary>
    void ExecuteInsert<T>(string tableName, T entity) where T : class, new();
    /// <summary>
    /// Inserta una colección de entidades en la tabla especificada.
    /// </summary>
    void ExecuteInsert<T>(string tableName, ICollection<T> collection) where T : class, new();

    /// <summary>
    /// Obtiene la fecha y hora actuales del servidor.
    /// </summary>
    DateTime GetCurrentDateTime();

}