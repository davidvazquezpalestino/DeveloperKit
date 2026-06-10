namespace DevKit.ExecutionEngine.Excel.Abstractions;

/// <summary>Define las operaciones asíncronas para trabajar con archivos CSV.</summary>
public partial interface ICsvProvider
{
    /// <summary>Obtiene el <see cref="DataTable"/> con los datos del archivo CSV de forma asíncrona.</summary>
    Task<DataTable> GetTableAsync(string tableName = null, CancellationToken cancellationToken = default);

    /// <summary>Obtiene una colección de objetos de tipo <typeparamref name="T"/> de forma asíncrona.</summary>
    Task<ICollection<T>> GetItemsAsync<T>(string tableName = null, CancellationToken cancellationToken = default) where T : new();

    /// <summary>Obtiene los nombres de columnas del archivo CSV de forma asíncrona.</summary>
    Task<IReadOnlyList<string>> GetColumnNamesAsync(CancellationToken cancellationToken = default);
}
