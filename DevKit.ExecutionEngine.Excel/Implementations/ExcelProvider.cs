
namespace DevKit.ExecutionEngine.Excel.Implementations;

/// <summary>Implementación de operaciones con archivos Excel.</summary>
public partial class ExcelProvider : IExcelProvider
{
    /// <inheritdoc/>
    public string ConnectionString { get; private set; }
    private Stream FileStream;
    private readonly IExcelReader Reader;

    /// <inheritdoc/>
    public void SetDatabaseLogon(string connectionString)
    {
        ConnectionString = connectionString;
        FileStream = Stream.Null;
    }

    /// <inheritdoc/>
    public void SetDatabaseLogon(Stream stream)
    {
        FileStream = stream;
        ConnectionString = null;
    }

    /// <inheritdoc/>
    public ICollection<T> GetItems<T>(string tableName) where T : new()
    {
        DataTable table = GetTable(tableName);
        return DataTableMapper.MapRowsToItems<T>(table);
    }

    /// <inheritdoc/>
    public DataTable GetTable(string tableName) => GetTables().FirstOrDefault(table => table.TableName == tableName);

    /// <inheritdoc/>
    public List<DataTable> GetTables()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        // La sobrecarga de SetDatabaseLogon usada determina la fuente de lectura:
        // - Stream  -> IExcelReader.ReadTables(Stream)
        // - string  -> IExcelReader.ReadTables(string)
        if (string.IsNullOrWhiteSpace(ConnectionString) && FileStream is not null && FileStream != Stream.Null)
        {
            return Reader.ReadTables(FileStream);
        }

        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            throw new InvalidOperationException(
                "No se ha configurado un origen para el archivo Excel. Use SetDatabaseLogon antes de leer.");
        }

        return Reader.ReadTables(ConnectionString);
    }

    /// <summary>Inicializa una nueva instancia predeterminada de <see cref="ExcelProvider"/>.</summary>
    public ExcelProvider() : this((IExcelReader)null) { }

    /// <summary>Inicializa una nueva instancia de <see cref="ExcelProvider"/> usando una cadena de conexión al archivo Excel.</summary>
    /// <param name="connectionString">Cadena de conexión que apunta al archivo Excel que se leerá.</param>
    public ExcelProvider(string connectionString) : this(connectionString, null) { }

    /// <summary>Inicializa una nueva instancia de <see cref="ExcelProvider"/> usando un flujo que contiene el archivo Excel.</summary>
    /// <param name="stream">Stream con el contenido del archivo Excel.</param>
    public ExcelProvider(Stream stream) : this(stream, null) { }

    /// <summary>Inicializa una nueva instancia de <see cref="ExcelProvider"/> con un lector personalizado.</summary>
    /// <param name="reader">Implementación de <see cref="IExcelReader"/> a usar. Si es <c>null</c>, se usa <see cref="ExcelReader"/>.</param>
    public ExcelProvider(IExcelReader reader)
    {
        Reader = reader ?? new ExcelReader();
    }

    /// <summary>Inicializa una nueva instancia de <see cref="ExcelProvider"/> usando una cadena de conexión y un lector personalizado.</summary>
    /// <param name="connectionString">Cadena de conexión que apunta al archivo Excel que se leerá.</param>
    /// <param name="reader">Implementación de <see cref="IExcelReader"/> a usar. Si es <c>null</c>, se usa <see cref="ExcelReader"/>.</param>
    public ExcelProvider(string connectionString, IExcelReader reader) : this(reader)
        => SetDatabaseLogon(connectionString);

    /// <summary>Inicializa una nueva instancia de <see cref="ExcelProvider"/> usando un flujo y un lector personalizado.</summary>
    /// <param name="stream">Stream con el contenido del archivo Excel.</param>
    /// <param name="reader">Implementación de <see cref="IExcelReader"/> a usar. Si es <c>null</c>, se usa <see cref="ExcelReader"/>.</param>
    public ExcelProvider(Stream stream, IExcelReader reader) : this(reader)
        => SetDatabaseLogon(stream);

    /// <inheritdoc/>
    public IReadOnlyList<string> GetSheetNames() =>
        GetTables().Select(t => t.TableName).ToList();

    /// <summary>Libera los recursos administrados utilizados por la instancia de forma asíncrona.</summary>
    public ValueTask DisposeAsync()
    {
        if (FileStream != null)
        {
            FileStream.Dispose();
            FileStream = null;
        }
        return new ValueTask();
    }


    /// <summary>Libera los recursos administrados utilizados por la instancia.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            FileStream?.Dispose();
            FileStream = null;
        }
    }

    /// <inheritdoc/>
    ~ExcelProvider() => Dispose(false);
}