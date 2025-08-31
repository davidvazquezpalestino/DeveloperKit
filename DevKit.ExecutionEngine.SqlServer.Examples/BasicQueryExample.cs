using DevKit.ExecutionEngine.SQLServer.Extensions;
using DevKit.ExecutionEngine.SQLServer.Query;
using System;
using System.Linq;

namespace DevKit.ExecutionEngine.SQLServer.Examples
{
    public class BasicQueryExample
    {
        public static void Run(ISQLServerProvider dbProvider)
        {
            Console.WriteLine("=== Ejemplo de Consulta Básica ===");
            
            // Ejemplo 1: Consulta simple con filtrado y ordenamiento
            var query = dbProvider.From<Producto>()
                               .Where(p => p.Precio > 100 && p.EnStock)
                               .OrderBy(p => p.Nombre)
                               .Take(10);

            // Obtener los resultados
            var productos = query.ToList();
            
            // Mostrar resultados
            Console.WriteLine($"Productos encontrados: {productos.Count}");
            foreach (var producto in productos)
            {
                Console.WriteLine($"- {producto.Nombre} (${producto.Precio})");
            }
        }
    }

    // Clase de ejemplo
    public class Producto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public decimal Precio { get; set; }
        public bool EnStock { get; set; }
    }
}
