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
        System.Globalization.CultureInfo culture = options.Culture ?? System.Globalization.CultureInfo.InvariantCulture;
        DataTable table = new DataTable(tableName) { Locale = culture };

        using StreamReader reader = new StreamReader(stream, encoding, detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: leaveOpen);

        List<string> headers = null;
        List<List<string>> dataRecords = new List<List<string>>();
        int maxColumns = 0;

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

            if (options.TrimFields)
            {
                for (int i = 0; i < record.Count; i++)
                {
                    if (record[i] is not null)
                    {
                        record[i] = record[i].Trim();
                    }
                }
            }

            if (headers is null && options.HasHeader)
            {
                headers = record;
                if (record.Count > maxColumns)
                {
                    maxColumns = record.Count;
                }
                continue;
            }

            dataRecords.Add(record);
            if (record.Count > maxColumns)
            {
                maxColumns = record.Count;
            }
        }

        Type[] columnTypes = new Type[maxColumns];
        for (int c = 0; c < maxColumns; c++)
        {
            columnTypes[c] = options.InferColumnTypes
                ? InferColumnType(dataRecords, c, culture)
                : typeof(string);
        }

        for (int c = 0; c < maxColumns; c++)
        {
            string name;
            if (headers is not null && c < headers.Count && !string.IsNullOrWhiteSpace(headers[c]))
            {
                name = headers[c];
            }
            else
            {
                name = $"Column{c + 1}";
            }
            name = MakeUniqueColumnName(table, name);
            table.Columns.Add(name, columnTypes[c]);
        }

        foreach (List<string> record in dataRecords)
        {
            DataRow row = table.NewRow();
            for (int i = 0; i < record.Count && i < table.Columns.Count; i++)
            {
                row[i] = ConvertValue(record[i], columnTypes[i], culture);
            }
            table.Rows.Add(row);
        }

        return table;
    }

    private static Type InferColumnType(IReadOnlyList<List<string>> rows, int columnIndex, System.Globalization.CultureInfo culture)
    {
        bool any = false;
        bool allBool = true;
        bool allInt = true;
        bool allLong = true;
        bool allDecimal = true;
        bool allDate = true;

        const System.Globalization.NumberStyles integerStyles = System.Globalization.NumberStyles.Integer;
        const System.Globalization.NumberStyles decimalStyles = System.Globalization.NumberStyles.Number | System.Globalization.NumberStyles.AllowExponent;

        foreach (List<string> record in rows)
        {
            if (columnIndex >= record.Count)
            {
                continue;
            }
            string value = record[columnIndex];
            if (string.IsNullOrWhiteSpace(value))
            {
                continue;
            }
            any = true;

            if (allBool && !bool.TryParse(value, out _))
            {
                allBool = false;
            }
            if (allInt && !int.TryParse(value, integerStyles, culture, out _))
            {
                allInt = false;
            }
            if (allLong && !long.TryParse(value, integerStyles, culture, out _))
            {
                allLong = false;
            }
            if (allDecimal && !decimal.TryParse(value, decimalStyles, culture, out _))
            {
                allDecimal = false;
            }
            if (allDate && !DateTime.TryParse(value, culture, System.Globalization.DateTimeStyles.None, out _))
            {
                allDate = false;
            }

            if (!allBool && !allInt && !allLong && !allDecimal && !allDate)
            {
                break;
            }
        }

        if (!any) return typeof(string);
        if (allBool) return typeof(bool);
        if (allInt) return typeof(int);
        if (allLong) return typeof(long);
        if (allDecimal) return typeof(decimal);
        if (allDate) return typeof(DateTime);
        return typeof(string);
    }

    private static object ConvertValue(string value, Type targetType, System.Globalization.CultureInfo culture)
    {
        if (string.IsNullOrEmpty(value))
        {
            return DBNull.Value;
        }
        if (targetType == typeof(string))
        {
            return value;
        }

        try
        {
            if (targetType == typeof(bool))
            {
                return bool.Parse(value);
            }
            if (targetType == typeof(int))
            {
                return int.Parse(value, System.Globalization.NumberStyles.Integer, culture);
            }
            if (targetType == typeof(long))
            {
                return long.Parse(value, System.Globalization.NumberStyles.Integer, culture);
            }
            if (targetType == typeof(decimal))
            {
                return decimal.Parse(value, System.Globalization.NumberStyles.Number | System.Globalization.NumberStyles.AllowExponent, culture);
            }
            if (targetType == typeof(DateTime))
            {
                return DateTime.Parse(value, culture, System.Globalization.DateTimeStyles.None);
            }
        }
        catch
        {
            // Si el valor no puede convertirse, se trata como nulo para preservar la tipificación de la columna.
            return DBNull.Value;
        }

        return value;
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
