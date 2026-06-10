namespace DevKit.ExecutionEngine.Excel.Implementations;

/// <summary>Implementación de operaciones asíncronas con archivos CSV.</summary>
public partial class CsvProvider
{
    /// <inheritdoc/>
    public Task<DataTable> GetTableAsync(string tableName = null, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(GetTable(tableName));
    }

    /// <inheritdoc/>
    public Task<ICollection<T>> GetItemsAsync<T>(string tableName = null, CancellationToken cancellationToken = default) where T : new()
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(GetItems<T>(tableName));
    }

    /// <inheritdoc/>
    public Task<IReadOnlyList<string>> GetColumnNamesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(GetColumnNames());
    }
}
