using DevKit.ExecutionEngine.MySQL.Abstractions;
using DevKit.ExecutionEngine.MySQL.Extensions;
using DevKit.ExecutionEngine.MySQL.QueryBuilder;

namespace DevKit.ExecutionEngine.MySQL.Examples;

/// <summary>
/// Ejemplos de uso de MySqlQueryBuilder para construir consultas MySQL.
/// </summary>
public class MySqlQueryBuilderExamples
{
    private readonly IMySQLDatabaseProvider _provider;

    /// <summary>
    /// Inicializa una nueva instancia con el proveedor de base de datos especificado.
    /// </summary>
    /// <param name="provider">Proveedor utilizado para ejecutar las consultas de ejemplo.</param>
    public MySqlQueryBuilderExamples(IMySQLDatabaseProvider provider)
    {
        _provider = provider;
    }

    #region Modelos de Ejemplo

    public class Usuario
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
        public DateTime FechaCreacion { get; set; }
        public bool Activo { get; set; }
        public int? DepartamentoId { get; set; }
    }

    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public string Categoria { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    #endregion

    #region Ejemplos Básicos

    /// <summary>
    /// Ejemplo 1: Consulta SELECT básica
    /// </summary>
    public DataTable ObtenerTodosLosUsuarios()
    {
        // SQL generado: SELECT `Id`, `Nombre`, `Email`, `FechaCreacion`, `Activo`, `DepartamentoId` FROM `usuarios`
        var query = MySqlQueryBuilder.Create()
            .Select("Id", "Nombre", "Email", "FechaCreacion", "Activo", "DepartamentoId")
            .From("usuarios");

        return _provider.ExecuteQuery(query);
    }

    /// <summary>
    /// Ejemplo 2: Consulta con condición WHERE
    /// </summary>
    public DataTable ObtenerUsuariosActivos()
    {
        // SQL generado: SELECT `Id`, `Nombre`, `Email` FROM `usuarios` WHERE `Activo` = @p0
        var query = MySqlQueryBuilder.Create()
            .Select("Id", "Nombre", "Email")
            .From("usuarios")
            .Where("Activo", true);

        return _provider.ExecuteQuery(query);
    }

    /// <summary>
    /// Ejemplo 3: Consulta con múltiples condiciones WHERE
    /// </summary>
    public DataTable ObtenerUsuariosPorDepartamento(int departamentoId)
    {
        // SQL generado: SELECT * FROM `usuarios` WHERE `Activo` = @p0 AND `DepartamentoId` = @p1
        var query = MySqlQueryBuilder.Create()
            .Select("*")
            .From("usuarios")
            .Where("Activo", true)
            .Where("DepartamentoId", departamentoId);

        return _provider.ExecuteQuery(query);
    }

    /// <summary>
    /// Ejemplo 4: Consulta con LIKE para búsqueda de texto
    /// </summary>
    public DataTable BuscarUsuariosPorNombre(string nombre)
    {
        // SQL generado: SELECT * FROM `usuarios` WHERE `Nombre` LIKE @p0
        var query = MySqlQueryBuilder.Create()
            .Select("*")
            .From("usuarios")
            .WhereLike("Nombre", $"%{nombre}%");

        return _provider.ExecuteQuery(query);
    }

    /// <summary>
    /// Ejemplo 5: Consulta con IN para múltiples valores
    /// </summary>
    public DataTable ObtenerUsuariosPorIds(int[] ids)
    {
        // SQL generado: SELECT * FROM `usuarios` WHERE `Id` IN (@p0, @p1, @p2, ...)
        var query = MySqlQueryBuilder.Create()
            .Select("*")
            .From("usuarios")
            .WhereIn("Id", ids.Cast<object>());

        return _provider.ExecuteQuery(query);
    }

    #endregion

    #region Ejemplos con Ordenamiento

    /// <summary>
    /// Ejemplo 6: Consulta con ORDER BY ascendente
    /// </summary>
    public DataTable ObtenerUsuariosOrdenadosPorNombre()
    {
        // SQL generado: SELECT * FROM `usuarios` WHERE `Activo` = @p0 ORDER BY `Nombre` ASC
        var query = MySqlQueryBuilder.Create()
            .Select("*")
            .From("usuarios")
            .Where("Activo", true)
            .OrderBy("Nombre");

        return _provider.ExecuteQuery(query);
    }

    /// <summary>
    /// Ejemplo 7: Consulta con ORDER BY descendente
    /// </summary>
    public DataTable ObtenerUsuariosRecientes()
    {
        // SQL generado: SELECT * FROM `usuarios` ORDER BY `FechaCreacion` DESC
        var query = MySqlQueryBuilder.Create()
            .Select("*")
            .From("usuarios")
            .OrderByDescending("FechaCreacion");

        return _provider.ExecuteQuery(query);
    }

    #endregion

    #region Ejemplos con LIMIT y OFFSET

    /// <summary>
    /// Ejemplo 8: Consulta con LIMIT para paginación
    /// </summary>
    public DataTable ObtenerPrimeros10Usuarios()
    {
        // SQL generado: SELECT * FROM `usuarios` ORDER BY `Id` ASC LIMIT 10
        var query = MySqlQueryBuilder.Create()
            .Select("*")
            .From("usuarios")
            .OrderBy("Id")
            .Limit(10);

        return _provider.ExecuteQuery(query);
    }

    /// <summary>
    /// Ejemplo 9: Consulta con LIMIT y OFFSET para paginación
    /// </summary>
    public DataTable ObtenerUsuariosPaginados(int pagina, int tamanoPagina)
    {
        int offset = (pagina - 1) * tamanoPagina;
        
        // SQL generado: SELECT * FROM `usuarios` ORDER BY `Id` ASC LIMIT 20 OFFSET 40
        var query = MySqlQueryBuilder.Create()
            .Select("*")
            .From("usuarios")
            .OrderBy("Id")
            .Limit(tamanoPagina)
            .Offset(offset);

        return _provider.ExecuteQuery(query);
    }

    #endregion

    #region Ejemplos con JOIN

    /// <summary>
    /// Ejemplo 10: Consulta con INNER JOIN
    /// </summary>
    public DataTable ObtenerUsuariosConDepartamento()
    {
        // SQL generado: SELECT u.*, d.Nombre as DepartamentoNombre FROM `usuarios` AS `u` INNER JOIN `departamentos` AS `d` ON u.DepartamentoId = d.Id WHERE u.Activo = @p0
        var query = MySqlQueryBuilder.Create()
            .Select("u.*", "d.Nombre as DepartamentoNombre")
            .From("usuarios", "u")
            .Join("departamentos", "u.DepartamentoId = d.Id", "d", "INNER")
            .Where("u.Activo", true);

        return _provider.ExecuteQuery(query);
    }

    /// <summary>
    /// Ejemplo 11: Consulta con LEFT JOIN
    /// </summary>
    public DataTable ObtenerTodosLosUsuariosConDepartamento()
    {
        // SQL generado: SELECT u.*, d.Nombre as DepartamentoNombre FROM `usuarios` AS `u` LEFT JOIN `departamentos` AS `d` ON u.DepartamentoId = d.Id
        var query = MySqlQueryBuilder.Create()
            .Select("u.*", "d.Nombre as DepartamentoNombre")
            .From("usuarios", "u")
            .LeftJoin("departamentos", "u.DepartamentoId = d.Id", "d");

        return _provider.ExecuteQuery(query);
    }

    #endregion

    #region Ejemplos con GROUP BY y HAVING

    /// <summary>
    /// Ejemplo 12: Consulta con GROUP BY
    /// </summary>
    public DataTable ContarUsuariosPorDepartamento()
    {
        // SQL generado: SELECT `DepartamentoId`, COUNT(*) as Total FROM `usuarios` WHERE `Activo` = @p0 GROUP BY `DepartamentoId`
        var query = MySqlQueryBuilder.Create()
            .Select("DepartamentoId", "COUNT(*) as Total")
            .From("usuarios")
            .Where("Activo", true)
            .GroupBy("DepartamentoId");

        return _provider.ExecuteQuery(query);
    }

    /// <summary>
    /// Ejemplo 13: Consulta con GROUP BY y HAVING
    /// </summary>
    public DataTable ObtenerDepartamentosConMasDe5Usuarios()
    {
        // SQL generado: SELECT `DepartamentoId`, COUNT(*) as Total FROM `usuarios` GROUP BY `DepartamentoId` HAVING COUNT(*) > 5
        var query = MySqlQueryBuilder.Create()
            .Select("DepartamentoId", "COUNT(*) as Total")
            .From("usuarios")
            .GroupBy("DepartamentoId")
            .Having("COUNT(*) > 5");

        return _provider.ExecuteQuery(query);
    }

    #endregion

    #region Ejemplos con DISTINCT

    /// <summary>
    /// Ejemplo 14: Consulta con DISTINCT
    /// </summary>
    public DataTable ObtenerDepartamentosUnicos()
    {
        // SQL generado: SELECT DISTINCT `DepartamentoId` FROM `usuarios` WHERE `DepartamentoId` IS NOT NULL
        var query = MySqlQueryBuilder.Create()
            .Select("DepartamentoId")
            .From("usuarios")
            .WhereNull("DepartamentoId")
            .Distinct();

        return _provider.ExecuteQuery(query);
    }

    #endregion

    #region Ejemplos con Valores Nulos

    /// <summary>
    /// Ejemplo 15: Consulta con IS NULL
    /// </summary>
    public DataTable ObtenerUsuariosSinDepartamento()
    {
        // SQL generado: SELECT * FROM `usuarios` WHERE `DepartamentoId` IS NULL
        var query = MySqlQueryBuilder.Create()
            .Select("*")
            .From("usuarios")
            .WhereNull("DepartamentoId");

        return _provider.ExecuteQuery(query);
    }

    #endregion

    #region Ejemplos con BETWEEN

    /// <summary>
    /// Ejemplo 16: Consulta con BETWEEN para rangos
    /// </summary>
    public DataTable ObtenerProductosPorRangoPrecio(decimal precioMin, decimal precioMax)
    {
        // SQL generado: SELECT * FROM `productos` WHERE `Precio` BETWEEN @p0 AND @p1 ORDER BY `Precio` ASC
        var query = MySqlQueryBuilder.Create()
            .Select("*")
            .From("productos")
            .WhereBetween("Precio", precioMin, precioMax)
            .OrderBy("Precio");

        return _provider.ExecuteQuery(query);
    }

    /// <summary>
    /// Ejemplo 17: Consulta con rango de fechas
    /// </summary>
    public DataTable ObtenerUsuariosRecientes(DateTime fechaInicio, DateTime fechaFin)
    {
        // SQL generado: SELECT * FROM `usuarios` WHERE `FechaCreacion` BETWEEN @p0 AND @p1 ORDER BY `FechaCreacion` DESC
        var query = MySqlQueryBuilder.Create()
            .Select("*")
            .From("usuarios")
            .WhereBetween("FechaCreacion", fechaInicio, fechaFin)
            .OrderByDescending("FechaCreacion");

        return _provider.ExecuteQuery(query);
    }

    #endregion

    #region Ejemplos Asíncronos

    /// <summary>
    /// Ejemplo 18: Consulta asíncrona básica
    /// </summary>
    public async Task<DataTable> ObtenerUsuariosActivosAsync(CancellationToken cancellationToken = default)
    {
        var query = MySqlQueryBuilder.Create()
            .Select("*")
            .From("usuarios")
            .Where("Activo", true)
            .OrderBy("Nombre");

        return await _provider.ExecuteQueryAsync(query, cancellationToken);
    }

    /// <summary>
    /// Ejemplo 19: Búsqueda asíncrona con múltiples condiciones
    /// </summary>
    public async Task<DataTable> BuscarUsuariosAsync(string nombre, int? departamentoId, CancellationToken cancellationToken = default)
    {
        var query = MySqlQueryBuilder.Create()
            .Select("*")
            .From("usuarios")
            .Where("Activo", true);

        if (!string.IsNullOrEmpty(nombre))
        {
            query.WhereLike("Nombre", $"%{nombre}%");
        }

        if (departamentoId.HasValue)
        {
            query.Where("DepartamentoId", departamentoId.Value);
        }

        query.OrderBy("Nombre");

        return await _provider.ExecuteQueryAsync(query, cancellationToken);
    }

    #endregion

    #region Ejemplos de Construcción Dinámica

    /// <summary>
    /// Ejemplo 20: Construcción dinámica de consulta
    /// </summary>
    public DataTable BuscarUsuariosDinamico(string nombre = null, int? departamentoId = null, bool? activo = null, string ordenarPor = "Id")
    {
        var query = MySqlQueryBuilder.Create()
            .Select("*")
            .From("usuarios");

        // Agregar condiciones dinámicamente
        if (!string.IsNullOrEmpty(nombre))
        {
            query.WhereLike("Nombre", $"%{nombre}%");
        }

        if (departamentoId.HasValue)
        {
            query.Where("DepartamentoId", departamentoId.Value);
        }

        if (activo.HasValue)
        {
            query.Where("Activo", activo.Value);
        }

        // Ordenamiento dinámico
        switch (ordenarPor.ToLower())
        {
            case "nombre":
                query.OrderBy("Nombre");
                break;
            case "fecha":
                query.OrderByDescending("FechaCreacion");
                break;
            default:
                query.OrderBy("Id");
                break;
        }

        return _provider.ExecuteQuery(query);
    }

    #endregion

    #region Ejemplos de Debugging

    /// <summary>
    /// Ejemplo 21: Ver SQL generado y parámetros
    /// </summary>
    public void MostrarSQLGenerado()
    {
        var query = MySqlQueryBuilder.Create()
            .Select("*")
            .From("usuarios")
            .Where("Activo", true)
            .WhereLike("Email", "%@empresa.com")
            .OrderBy("Nombre")
            .Limit(10);

        // Obtener el SQL generado
        string sql = query.Build();
        var parametros = query.GetParameters();

        Console.WriteLine($"SQL: {sql}");
        Console.WriteLine("Parámetros:");
        foreach (var param in parametros)
        {
            Console.WriteLine($"  {param.Key} = {param.Value}");
        }

        // Salida esperada:
        // SQL: SELECT * FROM `usuarios` WHERE `Activo` = @p0 AND `Email` LIKE @p1 ORDER BY `Nombre` ASC LIMIT 10
        // Parámetros:
        //   @p0 = True
        //   @p1 = %@empresa.com
    }

    #endregion

    #region Ejemplos de Uso con Extensions

    /// <summary>
    /// Ejemplo 22: Usando extensiones del provider
    /// </summary>
    public DataTable EjemploConExtensiones()
    {
        // Usando el método de extensión Query() del provider
        var query = _provider.Query("usuarios")
            .Select("Id", "Nombre", "Email")
            .Where("Activo", true)
            .OrderBy("Nombre")
            .Limit(20);

        return _provider.ExecuteQuery(query);
    }

    #endregion
}
