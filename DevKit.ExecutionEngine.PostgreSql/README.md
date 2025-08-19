# DevKit.ExecutionEngine.PostgreSql

Proveedor de acceso a datos para PostgreSQL basado en Npgsql, con API síncrona y asíncrona, utilidades de ejecución (query, stored procedure), operaciones a diccionario y soporte de inserción masiva vía COPY binario.

## Características

- API consistente: `ExecuteQueryAsTable`, `ExecuteQueryAsList<T>`, `ExecuteNonQuery`, y variantes `*Async`.
- Soporte para procedimientos almacenados (`ExecuteProcedure*`).
- Inserción masiva a través de `COPY ... FROM STDIN (FORMAT BINARY)`.
- Creación/Eliminación de tablas en runtime para cargas temporales.
- Configuración con Options Pattern (`PostgreOptions`).

## Namespaces y tipos

- Namespace principal: `DevKit.ExecutionEngine.PostgreSql`
- Clase principal: `PostgreSqlDatabaseProvider`
- Interfaz: `DevKit.ExecutionEngine.Abstractions.Interfaces.Postgre.IPostgreSqlDatabaseProvider`
- Options: `DevKit.ExecutionEngine.PostgreSql.Settings.PostgreOptions`

## Uso con Dependency Injection

```csharp
using DevKit.ExecutionEngine.Abstractions.Interfaces.Postgre;
using DevKit.ExecutionEngine.PostgreSql;
using DevKit.ExecutionEngine.PostgreSql.Settings;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.Configure<PostgreOptions>(opt =>
{
    opt.ConnectionString = "Host=localhost;Database=app;Username=user;Password=***;";
    opt.CommandTimeout = 60; // opcional
    opt.ConnectionPooling = new PostgreOptions.ConnectionPoolingOptions { Pooling = true, MinPoolSize = 1, MaxPoolSize = 100 };
});

services.AddSingleton<IPostgreSqlDatabaseProvider, PostgreSqlDatabaseProvider>();

var sp = services.BuildServiceProvider();
var db = sp.GetRequiredService<IPostgreSqlDatabaseProvider>();

// Consulta como lista tipada
var items = db.ExecuteQueryAsList(
    "SELECT id, name FROM customers WHERE active = @p0",
    r => new { Id = r.GetInt32(0), Name = r.GetString(1) },
    p => { var prm = (Npgsql.NpgsqlParameterCollection)p; prm.AddWithValue("@p0", true); }
);
```

## Inserción masiva

```csharp
// Inserta un DataTable en una tabla existente usando COPY binario
provider.ExecuteBulkInsert(dt, "public.customers");

// Recrea la tabla destino con el esquema del DataTable y luego inserta
provider.ExecuteBulkInsertToTable(dt, "temp.customers_load");
```

## Métodos clave

- Síncronos: `ExecuteQueryAsTable`, `ExecuteQueryAsList<T>`, `ExecuteQueryAsDictionary`, `ExecuteNonQuery`, `ExecuteProcedure*`, `BeginTransaction`, `CommitTransaction`, `RollbackTransaction`, `ExecuteBulkInsert*`.
- Asíncronos: `ExecuteQueryAsTableAsync`, `ExecuteQueryAsListAsync<T>`, `ExecuteQueryAsDictionaryAsync`, `ExecuteNonQueryAsync`, `ExecuteProcedure*Async`, `ExecuteBulkInsertAsync`, `ExecuteBulkInsertToTableAsync`.

## Requisitos

- .NET 8.0+
- [Npgsql](https://www.npgsql.org/)

## Licencia

MIT. Ver `LICENSE` en el repositorio raíz.
