using ExcelDataReader;

namespace DevKit.ExecutionEngine.Excel.Implementations;

/// <summary>
/// Implementación predeterminada de <see cref="IExcelReader"/> basada en
/// <see cref="ExcelReaderFactory"/> de <c>ExcelDataReader</c>.
/// </summary>
public sealed class ExcelReader : IExcelReader
{
    /// <inheritdoc/>
    public List<DataTable> ReadTables(Stream stream)
    {
        if (stream is null)
        {
            throw new ArgumentNullException(nameof(stream));
        }

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        using IExcelDataReader reader = ExcelReaderFactory.CreateReader(stream);
        DataSet dataSet = reader.AsDataSet(new ExcelDataSetConfiguration
        {
            ConfigureDataTable = _ => new ExcelDataTableConfiguration
            {
                UseHeaderRow = true,
            },
        });

        return dataSet.Tables.Cast<DataTable>().ToList();
    }

    /// <inheritdoc/>
    public List<DataTable> ReadTables(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("La ruta al archivo Excel no puede estar vacía.", nameof(filePath));
        }

        using FileStream stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            4096,
            FileOptions.SequentialScan);
        return ReadTables(stream);
    }
}
