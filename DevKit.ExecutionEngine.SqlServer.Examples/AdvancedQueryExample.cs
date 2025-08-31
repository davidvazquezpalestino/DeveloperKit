using DevKit.ExecutionEngine.SQLServer.Extensions;
using DevKit.ExecutionEngine.SQLServer.Query;
using System;
using System.Linq;

namespace DevKit.ExecutionEngine.SQLServer.Examples
{
    public class AdvancedQueryExample
    {
        public static void Run(ISQLServerProvider dbProvider)
        {
            Console.WriteLine("\n=== Ejemplo de Consulta Avanzada ===");
            
            // Página actual y tamaño de página
            int pagina = 1;
            int tamanoPagina = 5;
            
            // Ejemplo 2: Consulta con join, proyección y paginación
            var query = dbProvider.From<Pedido>()
                               .Join<Cliente>((p, c) => p.ClienteId == c.Id)
                               .Where((p, c) => p.Fecha >= DateTime.Today.AddMonths(-3))
                               .OrderByDescending((p, c) => p.Fecha)
                               .Select((p, c) => new 
                               {
                                   p.Id,
                                   p.NumeroPedido,
                                   p.Fecha,
                                   p.Total,
                                   Cliente = c.Nombre,
                                   c.Email
                               });
            
            // Aplicar paginación
            var pagedQuery = query.Skip((pagina - 1) * tamanoPagina)
                                .Take(tamanoPagina);
            
            // Obtener los resultados
            var pedidos = pagedQuery.ToList();
            
            // Mostrar resultados
            Console.WriteLine($"Página {pagina} - Mostrando {pedidos.Count} de {query.Count()} pedidos");
            foreach (var pedido in pedidos)
            {
                Console.WriteLine($"Pedido #{pedido.NumeroPedido} - {pedido.Cliente} - {pedido.Fecha:d} - Total: ${pedido.Total}");
            }
        }
    }

    // Clases de ejemplo
    public class Pedido
    {
        public int Id { get; set; }
        public string NumeroPedido { get; set; }
        public int ClienteId { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
    }

    public class Cliente
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Email { get; set; }
    }
}
