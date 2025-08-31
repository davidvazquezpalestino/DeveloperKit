using DevKit.ExecutionEngine.SQLServer.Examples;
using DevKit.ExecutionEngine.SQLServer.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace DevKit.ExecutionEngine.Examples
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Configurar el host
            using var host = CreateHostBuilder(args).Build();
            
            // Obtener el proveedor de base de datos
            var dbProvider = host.Services.GetRequiredService<ISQLServerProvider>();

            try
            {
                // Ejecutar ejemplos
                BasicQueryExample.Run(dbProvider);
                AdvancedQueryExample.Run(dbProvider);
                await AsyncQueryExample.RunAsync(dbProvider);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Detalles: {ex.InnerException.Message}");
                }
            }

            Console.WriteLine("\nPresione cualquier tecla para salir...");
            Console.ReadKey();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    // Configurar la conexión a la base de datos
                    services.AddSQLServerProvider(provider => 
                    {
                        // Reemplaza con tu cadena de conexión
                        return new SQLServerOptions
                        {
                            ConnectionString = "Server=tu_servidor;Database=tu_base_de_datos;User Id=tu_usuario;Password=tu_contraseña;TrustServerCertificate=True;",
                            CommandTimeout = 30
                        };
                    });
                });
    }
}
