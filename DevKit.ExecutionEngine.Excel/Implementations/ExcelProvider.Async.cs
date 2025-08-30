namespace DevKit.ExecutionEngine.Excel.Implementations;

/// <summary>Implementación de operaciones asíncronas con archivos Excel.</summary>
public partial class ExcelProvider
{
    /// <inheritdoc/>
    public Task<DataTable> GetTableAsync(string tableName)
    {
        return Task.FromResult(GetTable(tableName));
    }

    /// <inheritdoc/>
    public Task<ICollection<T>> GetItemsAsync<T>(string tableName) where T : new()
    {
        return Task.FromResult(GetItems<T>(tableName));
    }

    /// <inheritdoc/>
    public Task<List<DataTable>> GetTablesAsync()
    {
        return Task.FromResult(GetTables());
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> GetSheetNamesAsync()
    {
        return Task.FromResult(GetSheetNames());
    }
}
