# Ejemplos de Consultas MySQL - MySqlQueryBuilder

Esta guía muestra cómo usar `MySqlQueryBuilder` para construir consultas MySQL de forma segura y parametrizada.

## Configuración Inicial

```csharp
// Inyección de dependencias
services.AddScoped<IMySQLDatabaseProvider, MySQLDatabaseProvider>();

// Uso en un servicio
public class UsuarioService
{
    private readonly IMySQLDatabaseProvider _provider;
    
    public UsuarioService(IMySQLDatabaseProvider provider)
    {
        _provider = provider;
    }
}
```

## Consultas Básicas

### 1. SELECT Simple
```csharp
// Obtener todos los usuarios
var query = MySqlQueryBuilder.Create()
    .Select("Id", "Nombre", "Email")
    .From("usuarios");

var resultado = _provider.ExecuteQuery(query);

// SQL generado: SELECT `Id`, `Nombre`, `Email` FROM `usuarios`
```

### 2. WHERE con Parámetros
```csharp
// Usuarios activos
var query = MySqlQueryBuilder.Create()
    .Select("*")
    .From("usuarios")
    .Where("Activo", true);

// SQL generado: SELECT * FROM `usuarios` WHERE `Activo` = @p0
```

### 3. Múltiples Condiciones WHERE
```csharp
// Usuarios activos de un departamento
var query = MySqlQueryBuilder.Create()
    .Select("*")
    .From("usuarios")
    .Where("Activo", true)
    .Where("DepartamentoId", 5);

// SQL generado: SELECT * FROM `usuarios` WHERE `Activo` = @p0 AND `DepartamentoId` = @p1
```

## Búsquedas de Texto

### 4. LIKE para Búsquedas
```csharp
// Buscar por nombre
var query = MySqlQueryBuilder.Create()
    .Select("*")
    .From("usuarios")
    .WhereLike("Nombre", "%Juan%");

// SQL generado: SELECT * FROM `usuarios` WHERE `Nombre` LIKE @p0
```

### 5. IN para Múltiples Valores
```csharp
// Usuarios por IDs específicos
int[] ids = {1, 2, 3, 4, 5};
var query = MySqlQueryBuilder.Create()
    .Select("*")
    .From("usuarios")
    .WhereIn("Id", ids.Cast<object>());

// SQL generado: SELECT * FROM `usuarios` WHERE `Id` IN (@p0, @p1, @p2, @p3, @p4)
```

## Ordenamiento

### 6. ORDER BY Ascendente
```csharp
var query = MySqlQueryBuilder.Create()
    .Select("*")
    .From("usuarios")
    .Where("Activo", true)
    .OrderBy("Nombre");

// SQL generado: SELECT * FROM `usuarios` WHERE `Activo` = @p0 ORDER BY `Nombre` ASC
```

### 7. ORDER BY Descendente
```csharp
var query = MySqlQueryBuilder.Create()
    .Select("*")
    .From("usuarios")
    .OrderByDescending("FechaCreacion");

// SQL generado: SELECT * FROM `usuarios` ORDER BY `FechaCreacion` DESC
```

## Paginación

### 8. LIMIT para Limitar Resultados
```csharp
var query = MySqlQueryBuilder.Create()
    .Select("*")
    .From("usuarios")
    .OrderBy("Id")
    .Limit(10);

// SQL generado: SELECT * FROM `usuarios` ORDER BY `Id` ASC LIMIT 10
```

### 9. LIMIT y OFFSET para Paginación
```csharp
int pagina = 3;
int tamanoPagina = 20;
int offset = (pagina - 1) * tamanoPagina;

var query = MySqlQueryBuilder.Create()
    .Select("*")
    .From("usuarios")
    .OrderBy("Id")
    .Limit(tamanoPagina)
    .Offset(offset);

// SQL generado: SELECT * FROM `usuarios` ORDER BY `Id` ASC LIMIT 20 OFFSET 40
```

## JOINs

### 10. INNER JOIN
```csharp
var query = MySqlQueryBuilder.Create()
    .Select("u.*", "d.Nombre as DepartamentoNombre")
    .From("usuarios", "u")
    .Join("departamentos", "u.DepartamentoId = d.Id", "d", "INNER")
    .Where("u.Activo", true);

// SQL generado: SELECT u.*, d.Nombre as DepartamentoNombre FROM `usuarios` AS `u` 
//               INNER JOIN `departamentos` AS `d` ON u.DepartamentoId = d.Id 
//               WHERE u.Activo = @p0
```

### 11. LEFT JOIN
```csharp
var query = MySqlQueryBuilder.Create()
    .Select("u.*", "d.Nombre as DepartamentoNombre")
    .From("usuarios", "u")
    .LeftJoin("departamentos", "u.DepartamentoId = d.Id", "d");

// SQL generado: SELECT u.*, d.Nombre as DepartamentoNombre FROM `usuarios` AS `u` 
//               LEFT JOIN `departamentos` AS `d` ON u.DepartamentoId = d.Id
```

## Agrupación

### 12. GROUP BY
```csharp
var query = MySqlQueryBuilder.Create()
    .Select("DepartamentoId", "COUNT(*) as Total")
    .From("usuarios")
    .Where("Activo", true)
    .GroupBy("DepartamentoId");

// SQL generado: SELECT `DepartamentoId`, COUNT(*) as Total FROM `usuarios` 
//               WHERE `Activo` = @p0 GROUP BY `DepartamentoId`
```

### 13. GROUP BY con HAVING
```csharp
var query = MySqlQueryBuilder.Create()
    .Select("DepartamentoId", "COUNT(*) as Total")
    .From("usuarios")
    .GroupBy("DepartamentoId")
    .Having("COUNT(*) > 5");

// SQL generado: SELECT `DepartamentoId`, COUNT(*) as Total FROM `usuarios` 
//               GROUP BY `DepartamentoId` HAVING COUNT(*) > 5
```

## Valores Especiales

### 14. DISTINCT
```csharp
var query = MySqlQueryBuilder.Create()
    .Select("DepartamentoId")
    .From("usuarios")
    .WhereNull("DepartamentoId")
    .Distinct();

// SQL generado: SELECT DISTINCT `DepartamentoId` FROM `usuarios` WHERE `DepartamentoId` IS NULL
```

### 15. IS NULL
```csharp
var query = MySqlQueryBuilder.Create()
    .Select("*")
    .From("usuarios")
    .WhereNull("DepartamentoId");

// SQL generado: SELECT * FROM `usuarios` WHERE `DepartamentoId` IS NULL
```

### 16. BETWEEN para Rangos
```csharp
var query = MySqlQueryBuilder.Create()
    .Select("*")
    .From("productos")
    .WhereBetween("Precio", 100.00m, 500.00m)
    .OrderBy("Precio");

// SQL generado: SELECT * FROM `productos` WHERE `Precio` BETWEEN @p0 AND @p1 ORDER BY `Precio` ASC
```

## Consultas Asíncronas

### 17. Async/Await
```csharp
public async Task<DataTable> ObtenerUsuariosAsync(CancellationToken cancellationToken = default)
{
    var query = MySqlQueryBuilder.Create()
        .Select("*")
        .From("usuarios")
        .Where("Activo", true)
        .OrderBy("Nombre");

    return await _provider.ExecuteQueryAsync(query, cancellationToken);
}
```

## Construcción Dinámica

### 18. Consultas Condicionales
```csharp
public DataTable BuscarUsuarios(string nombre = null, int? departamentoId = null, bool? activo = null)
{
    var query = MySqlQueryBuilder.Create()
        .Select("*")
        .From("usuarios");

    // Agregar condiciones dinámicamente
    if (!string.IsNullOrEmpty(nombre))
        query.WhereLike("Nombre", $"%{nombre}%");

    if (departamentoId.HasValue)
        query.Where("DepartamentoId", departamentoId.Value);

    if (activo.HasValue)
        query.Where("Activo", activo.Value);

    return _provider.ExecuteQuery(query.OrderBy("Nombre"));
}
```

## Debugging

### 19. Ver SQL Generado
```csharp
var query = MySqlQueryBuilder.Create()
    .Select("*")
    .From("usuarios")
    .Where("Activo", true)
    .WhereLike("Email", "%@empresa.com")
    .OrderBy("Nombre")
    .Limit(10);

// Obtener SQL y parámetros
string sql = query.Build();
var parametros = query.GetParameters();

Console.WriteLine($"SQL: {sql}");
foreach (var param in parametros)
{
    Console.WriteLine($"  {param.Key} = {param.Value}");
}

// Salida:
// SQL: SELECT * FROM `usuarios` WHERE `Activo` = @p0 AND `Email` LIKE @p1 ORDER BY `Nombre` ASC LIMIT 10
//   @p0 = True
//   @p1 = %@empresa.com
```

## Usando Extensiones del Provider

### 20. Métodos de Conveniencia
```csharp
// Usando extensión Query() del provider
var query = _provider.Query("usuarios")
    .Select("Id", "Nombre", "Email")
    .Where("Activo", true)
    .OrderBy("Nombre")
    .Limit(20);

var resultado = _provider.ExecuteQuery(query);
```

## Modelos de Ejemplo

```csharp
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
```

## Características Principales

✅ **Consultas parametrizadas** - Prevención automática de SQL injection  
✅ **Sintaxis MySQL nativa** - Usa backticks (`) para identificadores  
✅ **Construcción fluida** - API intuitiva y encadenable  
✅ **Soporte async/await** - Operaciones asíncronas completas  
✅ **Paginación integrada** - LIMIT y OFFSET nativos de MySQL  
✅ **JOINs flexibles** - INNER, LEFT, RIGHT JOIN  
✅ **Debugging fácil** - Ver SQL generado y parámetros  

## Diferencias con SQL Server

- **Identificadores**: MySQL usa backticks (\`) vs SQL Server corchetes ([])
- **Paginación**: MySQL usa LIMIT/OFFSET vs SQL Server TOP/OFFSET
- **Sintaxis**: Algunas funciones específicas de MySQL
- **Tipos de datos**: Compatibilidad con tipos específicos de MySQL
