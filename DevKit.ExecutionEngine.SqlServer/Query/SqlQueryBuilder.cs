namespace DevKit.ExecutionEngine.SQLServer.Query;

/// <summary>
/// Constructor de consultas SQL para SQL Server
/// </summary>
/// <typeparam name="T">Tipo de entidad</typeparam>
public class SqlQueryBuilder<T> where T : class, new()
{
    private readonly List<Expression<Func<T, bool>>> WhereExpressionsField = new();
    private readonly List<(string column, bool isAscending)> OrderByField = new();
    private int? TakeField;
    private int? SkipField;

    /// <summary>
    /// Agrega una condición WHERE a la consulta
    /// </summary>
    public SqlQueryBuilder<T> Where(Expression<Func<T, bool>> predicate)
    {
        if (predicate != null)
        {
            WhereExpressionsField.Add(predicate);
        }
        return this;
    }

    /// <summary>
    /// Agrega un ordenamiento a la consulta
    /// </summary>
    public SqlQueryBuilder<T> OrderBy(string column, bool ascending = true)
    {
        if (!string.IsNullOrWhiteSpace(column))
        {
            OrderByField.Add((column, ascending));
        }

        return this;
    }

    /// <summary>
    /// Limita el número de resultados
    /// </summary>
    public SqlQueryBuilder<T> Take(int count)
    {
        TakeField = count > 0 ? count : TakeField;
        return this;
    }

    /// <summary>
    /// Salta un número específico de resultados
    /// </summary>
    public SqlQueryBuilder<T> Skip(int count)
    {
        SkipField = count >= 0 ? count : SkipField;
        return this;
    }

    /// <summary>
    /// Especifica que la consulta debe devolver resultados distintos
    /// </summary>
    public SqlQueryBuilder<T> Distinct()
    {
        return this;
    }
}