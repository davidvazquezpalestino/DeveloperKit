namespace DevKit.ExecutionEngine.Excel.Abstractions;

/// <summary>Define las operaciones de lectura de tablas desde un archivo Excel.</summary>
/// <remarks>
/// Una implementación de <see cref="IExcelReader"/> debe ser capaz de leer las hojas (tablas)
/// del archivo Excel ya sea a partir de un <see cref="Stream"/> ya abierto o a partir de la
/// ruta a un archivo en disco. La selección del método se determina por la sobrecarga de
/// <c>SetDatabaseLogon</c> usada en el proveedor.
/// </remarks>
public interface IExcelReader
{
    /// <summary>Lee todas las hojas del archivo Excel a partir de un <see cref="Stream"/>.</summary>
    /// <param name="stream">Stream con el contenido del archivo Excel. No se cierra al terminar.</param>
    /// <returns>Lista de <see cref="DataTable"/> con el contenido de cada hoja.</returns>
    List<DataTable> ReadTables(Stream stream);

    /// <summary>Lee todas las hojas del archivo Excel a partir de la ruta a un archivo en disco.</summary>
    /// <param name="filePath">Ruta absoluta o relativa al archivo Excel a leer.</param>
    /// <returns>Lista de <see cref="DataTable"/> con el contenido de cada hoja.</returns>
    List<DataTable> ReadTables(string filePath);
}
