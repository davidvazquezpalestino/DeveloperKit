namespace DevKit.ExecutionEngine.SQLServer.Factories;

/// <summary>
/// Interfaz para fábrica de conexiones SQL Server.
/// </summary>
public interface ISqlConnectionFactory
{
    /// <summary>
    /// Crea una nueva conexión SQL Server.
    /// </summary>
    /// <param name="connectionString">Cadena de conexión.</param>
    /// <returns>Nueva instancia de SqlConnection.</returns>
    SqlConnection CreateConnection(string connectionString);
    
    /// <summary>
    /// Crea una nueva conexión SQL Server de forma asíncrona.
    /// </summary>
    /// <param name="connectionString">Cadena de conexión.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Nueva instancia de SqlConnection abierta.</returns>
    Task<SqlConnection> CreateAndOpenConnectionAsync(string connectionString, CancellationToken cancellationToken = default);
}

/// <summary>
/// Implementación por defecto de fábrica de conexiones SQL Server.
/// </summary>
public class DefaultSqlConnectionFactory : ISqlConnectionFactory
{
    /// <summary>
    /// Crea una nueva conexión SQL Server.
    /// </summary>
    /// <param name="connectionString">Cadena de conexión.</param>
    /// <returns>Nueva instancia de SqlConnection.</returns>
    public SqlConnection CreateConnection(string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("La cadena de conexión no puede ser nula o vacía.", nameof(connectionString));
        }
        
        return new SqlConnection(connectionString);
    }
    
    /// <summary>
    /// Crea una nueva conexión SQL Server de forma asíncrona.
    /// </summary>
    /// <param name="connectionString">Cadena de conexión.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Nueva instancia de SqlConnection abierta.</returns>
    public async Task<SqlConnection> CreateAndOpenConnectionAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        SqlConnection connection = CreateConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }
}

/// <summary>
/// Fábrica de conexiones para testing que permite mockear conexiones.
/// </summary>
public class TestSqlConnectionFactory : ISqlConnectionFactory
{
    private readonly Func<string, SqlConnection> _connectionFactory;
    private readonly Func<string, CancellationToken, Task<SqlConnection>> _asyncConnectionFactory;
    
    /// <summary>
    /// Inicializa una nueva instancia de TestSqlConnectionFactory.
    /// </summary>
    /// <param name="connectionFactory">Función para crear conexiones síncronas.</param>
    /// <param name="asyncConnectionFactory">Función para crear conexiones asíncronas.</param>
    public TestSqlConnectionFactory(
        Func<string, SqlConnection> connectionFactory = null,
        Func<string, CancellationToken, Task<SqlConnection>> asyncConnectionFactory = null)
    {
        _connectionFactory = connectionFactory ?? (cs => new SqlConnection(cs));
        _asyncConnectionFactory = asyncConnectionFactory ?? DefaultAsyncFactory;
    }
    
    /// <summary>
    /// Crea una nueva conexión SQL Server.
    /// </summary>
    /// <param name="connectionString">Cadena de conexión.</param>
    /// <returns>Nueva instancia de SqlConnection.</returns>
    public SqlConnection CreateConnection(string connectionString)
    {
        return _connectionFactory(connectionString);
    }
    
    /// <summary>
    /// Crea una nueva conexión SQL Server de forma asíncrona.
    /// </summary>
    /// <param name="connectionString">Cadena de conexión.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>Nueva instancia de SqlConnection abierta.</returns>
    public Task<SqlConnection> CreateAndOpenConnectionAsync(string connectionString, CancellationToken cancellationToken = default)
    {
        return _asyncConnectionFactory(connectionString, cancellationToken);
    }
    
    private static async Task<SqlConnection> DefaultAsyncFactory(string connectionString, CancellationToken cancellationToken)
    {
        var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        return connection;
    }
}
