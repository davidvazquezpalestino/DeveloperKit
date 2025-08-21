namespace DevKit.ExecutionEngine.Excel.Abstractions;

/// <summary>Interfaz para operaciones con archivos Excel.</summary>
public partial interface IExcelDatabaseProvider : IDisposable
{
    /// <summary>Obtiene la cadena de conexión al archivo Excel.</summary>
    string ConnectionString { get; }

    /// <summary>Obtiene una tabla específica del archivo Excel.</summary>
    /// <param name="tableName">Nombre de la tabla a obtener.</param>
    /// <returns>DataTable con los datos de la tabla.</returns>
    DataTable GetTable(string tableName);

    /// <summary>Obtiene una colección de objetos de tipo T desde una tabla del archivo Excel.</summary>
    /// <typeparam name="T">Tipo de los objetos a crear.</typeparam>
    /// <param name="tableName">Nombre de la tabla a mapear.</param>
    /// <returns>Colección de objetos de tipo T.</returns>
    ICollection<T> GetItems<T>(string tableName) where T : new();

    /// <summary>Obtiene todas las tablas del archivo Excel.</summary>
    /// <returns>Lista de DataTables con todas las tablas del archivo.</returns>
    List<DataTable> GetTables();

    /// <summary>Obtiene los nombres de las hojas disponibles en el archivo Excel.</summary>
    /// <returns>Lista inmutable de nombres de hojas.</returns>
    IReadOnlyList<string> GetSheetNames();

    /// <summary>Configura la conexión al archivo Excel usando una cadena de conexión.</summary>
    /// <param name="connectionString">Cadena de conexión al archivo Excel.</param>
    void SetDatabaseLogon(string connectionString);

    /// <summary>Configura la conexión al archivo Excel usando un Stream.</summary>
    /// <param name="stream">Stream que contiene el archivo Excel.</param>
    void SetDatabaseLogon(Stream stream);
}