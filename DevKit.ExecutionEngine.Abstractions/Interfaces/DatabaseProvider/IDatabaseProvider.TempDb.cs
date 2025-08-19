namespace DevKit.ExecutionEngine.Abstractions.Interfaces.DatabaseProvider;

public partial interface IDatabaseProvider
{
    /// <summary>
    /// Elimina la tabla temporal indicada.
    /// </summary>
    void DropTable(string tableName);

    /// <summary>
    /// Crea una tabla temporal a partir de un DataTable.
    /// </summary>
    void CreateTable(DataTable source, string target);

}