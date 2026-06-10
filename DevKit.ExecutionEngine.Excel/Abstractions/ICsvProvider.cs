namespace DevKit.ExecutionEngine.Excel.Abstractions;

/// <summary>Interfaz para operaciones con archivos CSV.</summary>
public partial interface ICsvProvider : IAsyncDisposable, IDisposable
{
    /// <summary>Obtiene la ruta o cadena de conexión al archivo CSV.</summary>
    string ConnectionString { get; }

    /// <summary>Opciones de lectura del archivo CSV.</summary>
    CsvOptions Options { get; }

    /// <summary>Obtiene el <see cref="DataTable"/> con los datos del archivo CSV.</summary>
    /// <param name="tableName">Nombre de la tabla. Si es <c>null</c> o vacío, se usa el nombre por defecto.</param>
    /// <returns><see cref="DataTable"/> con los datos del archivo CSV.</returns>
    DataTable GetTable(string tableName = null);

    /// <summary>Obtiene una colección de objetos de tipo <typeparamref name="T"/> a partir del archivo CSV.</summary>
    /// <typeparam name="T">Tipo de los objetos a crear.</typeparam>
    /// <param name="tableName">Nombre de la tabla. Si es <c>null</c> o vacío, se usa el nombre por defecto.</param>
    /// <returns>Colección de objetos de tipo <typeparamref name="T"/>.</returns>
    ICollection<T> GetItems<T>(string tableName = null) where T : new();

    /// <summary>Obtiene los nombres de columnas del archivo CSV.</summary>
    /// <returns>Lista inmutable de nombres de columnas.</returns>
    IReadOnlyList<string> GetColumnNames();

    /// <summary>Configura la conexión al archivo CSV usando una ruta de archivo.</summary>
    /// <param name="connectionString">Ruta al archivo CSV.</param>
    void SetDatabaseLogon(string connectionString);

    /// <summary>Configura la conexión al archivo CSV usando un <see cref="Stream"/>.</summary>
    /// <param name="stream">Stream que contiene el archivo CSV.</param>
    void SetDatabaseLogon(Stream stream);

    /// <summary>Establece las opciones de lectura del archivo CSV.</summary>
    /// <param name="options">Opciones de configuración.</param>
    void SetOptions(CsvOptions options);
}
