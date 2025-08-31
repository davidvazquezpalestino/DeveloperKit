using DevKit.ExecutionEngine.SQLServer.Extensions;
using DevKit.ExecutionEngine.SQLServer.Query;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DevKit.ExecutionEngine.SQLServer.Examples
{
    public class AsyncQueryExample
    {
        public static async Task RunAsync(ISQLServerProvider dbProvider)
        {
            Console.WriteLine("\n=== Ejemplo de Consulta Asíncrona ===");
            
            try
            {
                // Ejemplo 3: Consulta asíncrona con múltiples condiciones
                var query = dbProvider.From<Producto>()
                                   .Where(p => p.Precio > 50 && p.CategoriaId == 1)
                                   .OrderByDescending(p => p.Precio);

                // Contar total de registros (asíncrono)
                int totalProductos = await query.CountAsync();
                
                // Obtener los 5 productos más caros (asíncrono)
                var productosCaros = await query
                    .Take(5)
                    .ToListAsync();
                
                // Mostrar resultados
                Console.WriteLine($"Total de productos en la categoría: {totalProductos}");
                Console.WriteLine("\nLos 5 productos más caros:");
                foreach (var producto in productosCaros)
                {
                    Console.WriteLine($"- {producto.Nombre}: ${producto.Precio:N2}");
                }

                // Ejemplo 4: Obtener un solo producto (asíncrono)
                var productoEspecial = await dbProvider.From<Producto>()
                                                    .Where(p => p.EsDestacado)
                                                    .FirstOrDefaultAsync();

                if (productoEspecial != null)
                {
                    Console.WriteLine($"\nProducto destacado: {productoEspecial.Nombre} - ${productoEspecial.Precio:N2}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al ejecutar la consulta: {ex.Message}");
            }
        }
    }

    // Clase de ejemplo extendida
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int CategoriaId { get; set; }
        public bool EnStock { get; set; }
        public bool EsDestacado { get; set; }
    }
}
