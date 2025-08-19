# DevKit.ExecutionEngine.MySql

Proveedor de acceso a datos para MySQL basado en MySqlConnector, con API síncrona y asíncrona, utilidades de ejecución (query, stored procedure), operaciones en diccionario y soporte de inserción masiva (MySqlBulkCopy) y tablas temporales.

## Características

- API consistente: `ExecuteQueryAsTable`, `ExecuteQueryAsList<T>`, `ExecuteNonQuery`, y variantes asíncronas `*Async`.
- Soporte para procedimientos almacenados (`ExecuteProcedure*`).
- Inserción masiva: `ExecuteBulkInsert` y `ExecuteBulkInsertToTable`.
- Creación/Eliminación de tablas temporales en runtime.
- Configuración por Options Pattern (`MySqlOptions`).

## Instalación

- Referencia el proyecto `DevKit.ExecutionEngine.MySql` o el paquete NuGet correspondiente cuando esté disponible.

## Namespaces y tipos

- Namespace principal: `DevKit.ExecutionEngine.MySql`
- Clase principal: `MySqlDatabaseProvider`
- Interfaz: `DevKit.ExecutionEngine.Abstractions.Interfaces.MySql.IMySqlDatabaseProvider`
- Options: `DevKit.ExecutionEngine.MySql.Settings.MySqlOptions`

## Uso con Dependency Injection

```csharp
using DevKit.ExecutionEngine.Abstractions.Interfaces.MySql;
using DevKit.ExecutionEngine.MySql;
using DevKit.ExecutionEngine.MySql.Settings;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.Configure<MySqlOptions>(opt =>
{
    opt.ConnectionString = "Server=localhost;Database=app;User Id=user;Password=***;";
    opt.CommandTimeout = 60; // opcional
    opt.ConnectionPooling = new ConnectionPoolingOptions { Pooling = true, MinPoolSize = 10, MaxPoolSize = 100 };
    opt.BulkCopy = new BulkCopyAdvancedOptions { AllowLoadLocalInfile = true };
});

services.AddSingleton<IMySqlDatabaseProvider, MySqlDatabaseProvider>();

var provider = services.BuildServiceProvider();
var db = provider.GetRequiredService<IMySqlDatabaseProvider>();

// Ejemplo: consulta como lista tipada
var items = db.ExecuteQueryAsList(
    "SELECT id, name FROM customers WHERE active = @p0",
    r => new { Id = r.GetInt32(0), Name = r.GetString(1) },
    p => { var prm = (MySqlConnector.MySqlParameterCollection)p; prm.AddWithValue("@p0", true); }
);
```

## Inserción masiva

```csharp
// Inserta un DataTable en una tabla existente
provider.ExecuteBulkInsert(dt, "schema.Customers");

// Crea una tabla temporal con el esquema del DataTable y luego inserta
provider.ExecuteBulkInsertToTable(dt, "temp.Customers_Load");
```

## Métodos clave

- Conjunto síncrono: `ExecuteQueryAsTable`, `ExecuteQueryAsList<T>`, `ExecuteQueryAsDictionary`, `ExecuteNonQuery`, `ExecuteProcedure*`, `BeginTransaction`, `CommitTransaction`, `RollbackTransaction`.
- Conjunto asíncrono: `ExecuteQueryAsTableAsync`, `ExecuteQueryAsListAsync<T>`, `ExecuteQueryAsDictionaryAsync`, `ExecuteNonQueryAsync`, `ExecuteProcedure*Async`, `ExecuteBulkInsertAsync`, `ExecuteBulkInsertToTableAsync`.

## Requisitos

- .NET 8.0+
- [MySqlConnector](https://mysqlconnector.net/)

## Licencia

MIT. Ver `LICENSE` en el repositorio raíz.
