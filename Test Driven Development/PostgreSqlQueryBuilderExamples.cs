using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DevKit.ExecutionEngine.PostgreSQL.Extensions;
using DevKit.ExecutionEngine.PostgreSQL.QueryBuilder;

namespace TestDrivenDevelopment
{
    /// <summary>
    /// Example class demonstrating the usage of the PostgreSQL query builder.
    /// </summary>
    public static class PostgreSqlQueryBuilderExamples
    {
        // Sample entity classes for demonstration
        public class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }
            public int CategoryId { get; set; }
            public bool IsActive { get; set; }
            public DateTime CreatedDate { get; set; }
        }

        public class Category
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        /// <summary>
        /// Example of a service class that uses the PostgreSQL query builder.
        /// </summary>
        public class ProductService
        {
            private readonly IPostgreSqlDatabaseProvider _dbProvider;

            /// <summary>
            /// Inicializa la clase de servicio con el proveedor de base de datos PostgreSQL.
            /// </summary>
            /// <param name="dbProvider">Proveedor utilizado para ejecutar las consultas.</param>
            public ProductService(IPostgreSqlDatabaseProvider dbProvider)
            {
                _dbProvider = dbProvider ?? throw new ArgumentNullException(nameof(dbProvider));
            }

            /// <summary>
            /// Example: Get active products with price greater than the specified amount.
            /// </summary>
            public async Task<List<Product>> GetActiveProductsAbovePriceAsync(decimal minPrice)
            {
                var query = _dbProvider.Query<Product>()
                    .Where(p => p.IsActive && p.Price > minPrice)
                    .OrderBy(p => p.Price);

                return await _dbProvider.ToListAsync(query);
            }

            /// <summary>
            /// Example: Get products by category with pagination.
            /// </summary>
            public async Task<List<Product>> GetProductsByCategoryAsync(int categoryId, int pageNumber, int pageSize)
            {
                var query = _dbProvider.Query<Product>()
                    .Where(p => p.CategoryId == categoryId && p.IsActive)
                    .OrderBy(p => p.Name)
                    .Offset((pageNumber - 1) * pageSize)
                    .Limit(pageSize);

                return await _dbProvider.ToListAsync(query);
            }

            /// <summary>
            /// Example: Search products by name with case-insensitive contains.
            /// </summary>
            public async Task<List<Product>> SearchProductsAsync(string searchTerm)
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                    return new List<Product>();

                var query = _dbProvider.Query<Product>()
                    .Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()) && p.IsActive)
                    .OrderBy(p => p.Name);

                return await _dbProvider.ToListAsync(query);
            }

            /// <summary>
            /// Example: Get product count by category.
            /// </summary>
            public async Task<Dictionary<int, int>> GetProductCountByCategoryAsync()
            {
                // This is a simplified example - in a real implementation, you would use GROUP BY
                // For complex aggregations, you might want to use a raw SQL query or a view
                var products = await _dbProvider.Query<Product>()
                    .Where(p => p.IsActive)
                    .ToListAsync(_dbProvider);

                var result = new Dictionary<int, int>();
                foreach (var product in products)
                {
                    if (result.ContainsKey(product.CategoryId))
                        result[product.CategoryId]++;
                    else
                        result[product.CategoryId] = 1;
                }

                return result;
            }

            /// <summary>
            /// Example: Get products with their category names using a join.
            /// Note: This is a simplified example - in a real implementation, you would use a proper join
            /// or a view that handles the relationship.
            /// </summary>
            public async Task<List<(Product Product, string CategoryName)>> GetProductsWithCategoriesAsync()
            {
                // In a real implementation, you would use a proper join
                // This is a simplified example that makes separate queries
                var products = await _dbProvider.Query<Product>()
                    .Where(p => p.IsActive)
                    .OrderBy(p => p.Name)
                    .ToListAsync(_dbProvider);

                var categoryIds = new HashSet<int>();
                foreach (var product in products)
                {
                    categoryIds.Add(product.CategoryId);
                }

                var categories = await _dbProvider.Query<Category>()
                    .Where(c => categoryIds.Contains(c.Id))
                    .ToListAsync(_dbProvider);

                var categoryLookup = new Dictionary<int, string>();
                foreach (var category in categories)
                {
                    categoryLookup[category.Id] = category.Name;
                }

                var result = new List<(Product, string)>(products.Count);
                foreach (var product in products)
                {
                    categoryLookup.TryGetValue(product.CategoryId, out var categoryName);
                    result.Add((product, categoryName ?? "Uncategorized"));
                }

                return result;
            }
        }
    }

    // Extension methods to make the examples more concise
    public static class PostgreSqlQueryBuilderExtensions
    {
        /// <summary>
        /// Ejecuta el generador de consultas y devuelve la lista de resultados.
        /// </summary>
        /// <typeparam name="T">Tipo de entidad que se proyectará.</typeparam>
        /// <param name="queryBuilder">Constructor de consultas que se desea materializar.</param>
        /// <param name="provider">Proveedor encargado de ejecutar la consulta.</param>
        public static async Task<List<T>> ToListAsync<T>(this PostgreSqlQueryBuilder<T> queryBuilder, IPostgreSqlDatabaseProvider provider) 
            where T : class, new()
        {
            return await provider.ToListAsync(queryBuilder);
        }
    }
}
