
namespace DevKit.ExecutionEngine.Excel.Implementations;

/// <summary>Implementación de operaciones con archivos Excel.</summary>
public partial class ExcelProvider : IExcelProvider
{
    /// <inheritdoc/>
    public string ConnectionString { get; private set; }
    private Stream FileStream;

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
        return GetTable(tableName).Rows.Cast<DataRow>()
                                       .Select(row => row.GetItem<T>())
                                       .ToList();
    }

    /// <inheritdoc/>
    public DataTable GetTable(string tableName) => this.GetTables().FirstOrDefault(table => table.TableName == tableName);

    /// <inheritdoc/>
    public List<DataTable> GetTables()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        if (string.IsNullOrWhiteSpace(ConnectionString) && FileStream is not null)
        {
            return ReadWorksheetTables(FileStream);
        }
        return GetWorksheetTables();
    }

    /// <summary>Inicializa una nueva instancia de la clase ExcelDatabaseProvider.</summary>
    public ExcelProvider() { }

    /// <summary>Inicializa una nueva instancia de la clase ExcelDatabaseProvider con la ruta del archivo Excel.</summary>
    public ExcelProvider(string connectionString) => SetDatabaseLogon(connectionString);

    /// <summary>Inicializa una nueva instancia de la clase ExcelDatabaseProvider con un Stream que contiene el archivo Excel.</summary>
    public ExcelProvider(Stream stream) => SetDatabaseLogon(stream);

    /// <inheritdoc/>
    public IReadOnlyList<string> GetSheetNames() =>
        GetTables().Select(t => t.TableName).ToList();
    private List<DataTable> GetWorksheetTables()
    {
        using (FileStream stream = new FileStream(
                   ConnectionString,
                   FileMode.Open,
                   FileAccess.Read,
                   FileShare.ReadWrite | FileShare.Delete,
                   4096,
                   FileOptions.SequentialScan))
        {
            return ReadWorksheetTables(stream);
        }
    }

    private static List<DataTable> ReadWorksheetTables(Stream stream)
    {
        using (IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream, new ExcelReaderConfiguration { FallbackEncoding = Encoding.UTF8 }))
        {
            DataSet dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
            {
                ConfigureDataTable = (_) => new ExcelDataTableConfiguration { UseHeaderRow = true }
            });
            return dataSet.Tables
                          .Cast<DataTable>()
                          .ToList();
        }
    }

    /// <inheritdoc/>
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

    ~ExcelProvider() => Dispose(false);


}