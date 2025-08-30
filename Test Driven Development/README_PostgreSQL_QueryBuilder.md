# PostgreSQL Query Builder

This document provides an overview and examples of using the PostgreSQL query builder in the DeveloperKit library.

## Features

- Type-safe query building using expression trees
- Support for common SQL operations (SELECT, WHERE, ORDER BY, etc.)
- Parameterized queries to prevent SQL injection
- Fluent API for building complex queries
- Async/await support
- PostgreSQL-specific features like LIMIT/OFFSET for pagination

## Basic Usage

### Creating a Query

```csharp
// Create a query builder instance
var query = dbProvider.Query<Product>()
    .Where(p => p.IsActive && p.Price > 100)
    .OrderBy(p => p.Name)
    .Limit(10);
```

### Executing a Query

```csharp
// Get results as a list
var products = await dbProvider.ToListAsync(query);

// Get first result
var product = await dbProvider.FirstOrDefaultAsync(query);

// Get results as a DataTable
var dataTable = await dbProvider.ToDataTableAsync(query);

// Get a scalar value
var total = await dbProvider.ExecuteScalarAsync<Product, int>(
    dbProvider.Query<Product>().Select(p => p.Price * p.Quantity).Sum()
);
```

## Advanced Examples

### Pagination

```csharp
int pageNumber = 1;
int pageSize = 10;

var query = dbProvider.Query<Product>()
    .Where(p => p.CategoryId == 1)
    .OrderBy(p => p.Name)
    .Offset((pageNumber - 1) * pageSize)
    .Limit(pageSize);

var pageOfProducts = await dbProvider.ToListAsync(query);
```

### Complex Conditions

```csharp
var query = dbProvider.Query<Product>()
    .Where(p => (p.Name.Contains("Premium") || p.Price > 100) && p.IsActive)
    .OrderByDescending(p => p.Price);
```

### Using Joins (Conceptual)

```csharp
// Note: This is a conceptual example - actual join implementation would depend on your data model
var products = await dbProvider.Query<Product>()
    .Where(p => p.IsActive)
    .OrderBy(p => p.Name)
    .ToListAsync();

// Get related categories in a second query
var categoryIds = products.Select(p => p.CategoryId).Distinct().ToList();
var categories = await dbProvider.Query<Category>()
    .Where(c => categoryIds.Contains(c.Id))
    .ToDictionaryAsync(c => c.Id);
```

## Supported Operations

- **Filtering**: `Where()`, `And()`, `Or()`
- **Sorting**: `OrderBy()`, `OrderByDescending()`, `ThenBy()`, `ThenByDescending()`
- **Pagination**: `Skip()`, `Take()`, `Limit()`, `Offset()`
- **Selection**: `Select()`
- **Aggregation**: `Count()`, `Sum()`, `Average()`, `Min()`, `Max()`
- **Distinct**: `Distinct()`

## Best Practices

1. **Use parameters**: Always use the query builder's parameterization to prevent SQL injection.
2. **Project only needed fields**: Use `Select()` to retrieve only the columns you need.
3. **Use pagination**: For large result sets, always implement pagination.
4. **Handle exceptions**: Always wrap database calls in try-catch blocks.
5. **Dispose resources**: Ensure proper disposal of database connections and commands.

## Limitations

- Complex joins might be better handled with raw SQL or stored procedures
- Some advanced PostgreSQL features may require custom SQL
- Performance tuning might be needed for very complex queries

## See Also

- [PostgreSQL Documentation](https://www.postgresql.org/docs/)
- [Npgsql Documentation](https://www.npgsql.org/doc/)
