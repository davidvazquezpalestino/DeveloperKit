# DevKit.ExecutionEngine.Abstractions

Contrato común (interfaces) para los proveedores de base de datos y otros motores de ejecución del DeveloperKit. Define una API uniforme para acceso a datos sin depender de un motor específico (SQL Server, Oracle, PostgreSQL, MySQL, Excel).

## Objetivo

Permitir que el código de aplicación dependa de interfaces estables (`IDatabaseProvider` y derivados) y que la implementación concreta sea reemplazable vía inyección de dependencias.

## Namespaces y principales interfaces

- `DevKit.ExecutionEngine.Abstractions.Interfaces.DatabaseProvider`
  - `IDatabaseProvider` (sincrónico)
  - `IDatabaseProvider` partial async en `IDatabaseProvider.Async.cs`
  - utilidades de tabla temporal en `IDatabaseProvider.TempDb.cs`
- `DevKit.ExecutionEngine.Abstractions.Interfaces.SqlServer`
  - `ISqlServerDatabaseProvider`
- `DevKit.ExecutionEngine.Abstractions.Interfaces.Oracle`
  - `IOracleDatabaseProvider`
- `DevKit.ExecutionEngine.Abstractions.Interfaces.Postgre`
  - `IPostgreSqlDatabaseProvider`
- `DevKit.ExecutionEngine.Abstractions.Interfaces.MySql`
  - `IMySqlDatabaseProvider`
- `DevKit.ExecutionEngine.Abstractions.Interfaces.Excel`
  - `IExcelDatabaseProvider`

## Resumen de la API (`IDatabaseProvider`)

Operaciones comunes disponibles (y sus variantes `*Async` en el archivo async):

- `BeginTransaction()`, `CommitTransaction()`, `RollbackTransaction()`
- `ExecuteQueryAsTable(string query, Action<IDataParameterCollection>?)`
- `ExecuteProcedureAsTable(string procedimiento, Action<IDataParameterCollection>?)`
- `ExecuteQueryAsSingle<T>(string, Func<IDataReader,T>, Action<IDataParameterCollection>?)`
- `ExecuteProcedureAsSingle<T>(string, Func<IDataReader,T>, Action<IDataParameterCollection>?)`
- `ExecuteQueryAsDictionary(string, Action<IDataParameterCollection>?)`
- `ExecuteProcedureAsDictionary(string, Action<IDataParameterCollection>?)`
- `ExecuteQueryAsList<T>(string, Func<IDataReader,T>, Action<IDataParameterCollection>?)`
- `ExecuteProcedureAsList<T>(string, Func<IDataReader,T>, Action<IDataParameterCollection>?)`
- `ExecuteNonQuery(string, Action<IDataParameterCollection>?)`
- `ExecuteProcedureCommand(string, Action<IDataParameterCollection>?)`
- `ExecuteBulkInsert(DataTable, string)` y `ExecuteBulkInsertToTable(DataTable, string)`

Propiedades comunes:

- `ConnectionState` y `ConnectionString`

## Uso recomendado

- Referenciar este proyecto en tiempo de compilación desde las implementaciones concretas y desde las capas de aplicación que dependan de interfaces.
- Registrar en DI la implementación concreta adecuada, por ejemplo:

```csharp
// Ejemplo (en proyecto de implementación)
services.AddSingleton<DevKit.ExecutionEngine.Abstractions.Interfaces.Postgre.IPostgreSqlDatabaseProvider,
                      DevKit.ExecutionEngine.PostgreSql.PostgreSqlDatabaseProvider>();
```

## Licencia

MIT. Ver `LICENSE` en el repositorio raíz.
