namespace DevKit.ExecutionEngine.Excel.Implementations;

/// <summary>Implementación de operaciones con archivos CSV.</summary>
public partial class CsvProvider : ICsvProvider
{
    /// <inheritdoc/>
    public string ConnectionString { get; private set; }

    /// <inheritdoc/>
    public CsvOptions Options { get; private set; } = new CsvOptions();

    private Stream FileStream;
    private bool Disposed;

    /// <summary>Inicializa una nueva instancia predeterminada de <see cref="CsvProvider"/>.</summary>
    public CsvProvider() { }

    /// <summary>Inicializa una nueva instancia de <see cref="CsvProvider"/> usando una ruta al archivo CSV.</summary>
    /// <param name="connectionString">Ruta al archivo CSV.</param>
    public CsvProvider(string connectionString) => SetDatabaseLogon(connectionString);

    /// <summary>Inicializa una nueva instancia de <see cref="CsvProvider"/> usando una ruta al archivo CSV y opciones de lectura.</summary>
    /// <param name="connectionString">Ruta al archivo CSV.</param>
    /// <param name="options">Opciones de lectura.</param>
    public CsvProvider(string connectionString, CsvOptions options)
    {
        SetOptions(options);
        SetDatabaseLogon(connectionString);
    }

    /// <summary>Inicializa una nueva instancia de <see cref="CsvProvider"/> usando un <see cref="Stream"/>.</summary>
    /// <param name="stream">Stream con el contenido del archivo CSV.</param>
    public CsvProvider(Stream stream) => SetDatabaseLogon(stream);

    /// <summary>Inicializa una nueva instancia de <see cref="CsvProvider"/> usando un <see cref="Stream"/> y opciones de lectura.</summary>
    /// <param name="stream">Stream con el contenido del archivo CSV.</param>
    /// <param name="options">Opciones de lectura.</param>
    public CsvProvider(Stream stream, CsvOptions options)
    {
        SetOptions(options);
        SetDatabaseLogon(stream);
    }

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
    public void SetOptions(CsvOptions options) =>
        Options = options ?? throw new ArgumentNullException(nameof(options));

    /// <inheritdoc/>
    public DataTable GetTable(string tableName = null)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        string name = ResolveTableName(tableName);

        if (string.IsNullOrWhiteSpace(ConnectionString) && FileStream is not null && FileStream != Stream.Null)
        {
            return ReadTable(FileStream, name, leaveOpen: true);
        }

        if (string.IsNullOrWhiteSpace(ConnectionString))
        {
            throw new InvalidOperationException("No se ha configurado un origen para el archivo CSV. Use SetDatabaseLogon antes de leer.");
        }

        using FileStream stream = new FileStream(
            ConnectionString,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            4096,
            FileOptions.SequentialScan);
        return ReadTable(stream, name, leaveOpen: false);
    }

    /// <inheritdoc/>
    public ICollection<T> GetItems<T>(string tableName = null) where T : new()
    {
        DataTable table = GetTable(tableName);
        return DataTableMapper.MapRowsToItems<T>(table);
    }

    /// <inheritdoc/>
    public IReadOnlyList<string> GetColumnNames()
    {
        DataTable table = GetTable();
        return table.Columns
                    .Cast<DataColumn>()
                    .Select(c => c.ColumnName)
                    .ToList();
    }

    private string ResolveTableName(string tableName)
    {
        if (!string.IsNullOrWhiteSpace(tableName))
        {
            return tableName;
        }
        if (!string.IsNullOrWhiteSpace(ConnectionString))
        {
            try
            {
                return Path.GetFileNameWithoutExtension(ConnectionString);
            }
            catch
            {
                // ignore and fall back to default
            }
        }
        return Options?.DefaultTableName ?? "Csv";
    }

    private DataTable ReadTable(Stream stream, string tableName, bool leaveOpen)
    {
        CsvOptions options = Options ?? new CsvOptions();
        Encoding encoding = options.Encoding ?? Encoding.UTF8;
        DataTable table = new DataTable(tableName) { Locale = options.Culture ?? System.Globalization.CultureInfo.InvariantCulture };

        using StreamReader reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: leaveOpen);

        bool headerInitialized = false;
        foreach (List<string> record in ParseRecords(reader, options))
        {
            if (record.Count == 0)
            {
                continue;
            }
            if (options.SkipEmptyLines && record.Count == 1 && string.IsNullOrEmpty(record[0]))
            {
                continue;
            }

            if (!headerInitialized)
            {
                BuildColumns(table, record, options);
                headerInitialized = true;
                if (options.HasHeader)
                {
                    continue;
                }
            }

            EnsureColumns(table, record.Count);
            DataRow row = table.NewRow();
            for (int i = 0; i < record.Count && i < table.Columns.Count; i++)
            {
                string value = record[i];
                if (options.TrimFields && value is not null)
                {
                    value = value.Trim();
                }
                row[i] = (object)value ?? DBNull.Value;
            }
            table.Rows.Add(row);
        }

        return table;
    }

    private static void BuildColumns(DataTable table, IReadOnlyList<string> firstRecord, CsvOptions options)
    {
        if (options.HasHeader)
        {
            for (int i = 0; i < firstRecord.Count; i++)
            {
                string name = options.TrimFields ? firstRecord[i]?.Trim() : firstRecord[i];
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = $"Column{i + 1}";
                }
                name = MakeUniqueColumnName(table, name);
                table.Columns.Add(name, typeof(string));
            }
        }
        else
        {
            for (int i = 0; i < firstRecord.Count; i++)
            {
                table.Columns.Add($"Column{i + 1}", typeof(string));
            }
        }
    }

    private static void EnsureColumns(DataTable table, int requiredCount)
    {
        while (table.Columns.Count < requiredCount)
        {
            string name = $"Column{table.Columns.Count + 1}";
            name = MakeUniqueColumnName(table, name);
            table.Columns.Add(name, typeof(string));
        }
    }

    private static string MakeUniqueColumnName(DataTable table, string baseName)
    {
        if (!table.Columns.Contains(baseName))
        {
            return baseName;
        }
        int suffix = 1;
        string candidate;
        do
        {
            candidate = $"{baseName}_{suffix++}";
        } while (table.Columns.Contains(candidate));
        return candidate;
    }

    /// <summary>Parser RFC 4180: soporta campos entrecomillados, comillas escapadas y saltos de línea dentro de campos.</summary>
    private static IEnumerable<List<string>> ParseRecords(TextReader reader, CsvOptions options)
    {
        char delimiter = options.Delimiter;
        char quote = options.Quote;
        List<string> fields = new();
        StringBuilder current = new StringBuilder();
        bool inQuotes = false;
        bool fieldStarted = false;

        int read;
        while ((read = reader.Read()) != -1)
        {
            char ch = (char)read;

            if (inQuotes)
            {
                if (ch == quote)
                {
                    if (reader.Peek() == quote)
                    {
                        current.Append(quote);
                        reader.Read();
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(ch);
                }
                continue;
            }

            if (ch == quote && !fieldStarted)
            {
                inQuotes = true;
                fieldStarted = true;
                continue;
            }

            if (ch == delimiter)
            {
                fields.Add(current.ToString());
                current.Clear();
                fieldStarted = false;
                continue;
            }

            if (ch == '\r')
            {
                if (reader.Peek() == '\n')
                {
                    reader.Read();
                }
                fields.Add(current.ToString());
                current.Clear();
                fieldStarted = false;
                yield return fields;
                fields = new List<string>();
                continue;
            }

            if (ch == '\n')
            {
                fields.Add(current.ToString());
                current.Clear();
                fieldStarted = false;
                yield return fields;
                fields = new List<string>();
                continue;
            }

            current.Append(ch);
            fieldStarted = true;
        }

        if (current.Length > 0 || fields.Count > 0 || fieldStarted)
        {
            fields.Add(current.ToString());
            yield return fields;
        }
    }

    /// <summary>Libera los recursos administrados utilizados por la instancia de forma asíncrona.</summary>
    public ValueTask DisposeAsync()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
        return new ValueTask();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (Disposed) return;
        if (disposing)
        {
            FileStream?.Dispose();
            FileStream = null;
        }
        Disposed = true;
    }

    /// <inheritdoc/>
    ~CsvProvider() => Dispose(false);
}
