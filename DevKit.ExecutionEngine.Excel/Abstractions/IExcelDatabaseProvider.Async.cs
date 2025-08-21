namespace DevKit.ExecutionEngine.Excel.Abstractions;

/// <summary>Define las operaciones asíncronas para trabajar con archivos Excel.</summary>
public partial interface IExcelDatabaseProvider
{
    /// <summary>Obtiene una tabla específica del archivo Excel de forma asíncrona.</summary>
    Task<DataTable> GetTableAsync(string tableName);
    /// <summary>Obtiene una colección de objetos de tipo T desde una tabla del archivo Excel de forma asíncrona.</summary>
    Task<ICollection<T>> GetItemsAsync<T>(string tableName) where T : new();
    /// <summary>Obtiene todas las tablas del archivo Excel de forma asíncrona.</summary>
    Task<List<DataTable>> GetTablesAsync();
    /// <summary>Obtiene los nombres de las hojas disponibles de forma asíncrona.</summary>
    Task<IReadOnlyList<string>> GetSheetNamesAsync();
}