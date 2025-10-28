namespace DevKit.ExecutionEngine.SQLServer.Query;

/// <summary>
/// Representa el estado de una consulta proyectada en construcción.
/// </summary>
/// <typeparam name="T">El tipo de entidad que se está consultando</typeparam>
/// <typeparam name="TResult">El tipo del resultado proyectado</typeparam>
public class ProjectedQueryState<T, TResult> : ILoggedQuery where T : class, new()
{
    internal ISQLServerProvider DbProvider { get; set; }

    /// <summary>
    /// SQL que se ejecutó (para logging)
    /// </summary>
    public string ExecutedSql { get; private set; }

    /// <summary>
    /// Parámetros de la consulta (para logging)
    /// </summary>
    public IDictionary<string, object> Parameters { get; private set; } = new Dictionary<string, object>();

    /// <summary>
    /// Obtiene o establece el nombre del esquema para la consulta.
    /// </summary>
    internal string Schema { get; private set; }

    /// <summary>
    /// Obtiene o establece el nombre de la tabla para la consulta.
    /// </summary>
    internal string TableName { get; }

    internal List<Expression<Func<T, bool>>> Where { get; } = new();
    internal List<(string column, bool isAscending)> OrderBy { get; } = new();
    internal int? TakeField { get; set; }
    internal int? SkipField { get; set; }
    internal bool DistinctField { get; set; }

    /// <summary>
    /// La expresión de selección que define la proyección
    /// </summary>
    internal Expression<Func<T, TResult>> SelectExpression { get; set; }

    internal ProjectedQueryState(ISQLServerProvider dbProvider, string schema, string tableName)
    {
        DbProvider = dbProvider ?? throw new ArgumentNullException(nameof(dbProvider));
        Schema = schema;
        TableName = tableName ?? typeof(T).Name;
    }

    /// <summary>
    /// Construye la consulta SQL
    /// </summary>
    public string Build()
    {
        QueryResult queryInfo = BuildQueryInternal();

        // Registrar la consulta SQL generada
        QueryLogger.LogQuery(
            queryInfo.SQL,
            queryInfo.Parameters,
            IQueryLogger.LogLevel.Debug,
            "Consulta SQL proyectada generada");

        return queryInfo.SQL;
    }

    /// <summary>
    /// Obtiene los parámetros de la consulta
    /// </summary>
    public IReadOnlyDictionary<string, object> GetParameters()
    {
        QueryResult queryInfo = BuildQueryInternal();
        return new System.Collections.ObjectModel.ReadOnlyDictionary<string, object>(
            queryInfo.Parameters ?? new Dictionary<string, object>());
    }

    /// <summary>
    /// Construye la consulta SQL y sus parámetros
    /// </summary>
    private QueryResult BuildQueryInternal()
    {
        // Usar el método BuildQuery existente que maneja las proyecciones
        return SqlQueryBuilderExtensions.BuildQuery(this);
    }
}
