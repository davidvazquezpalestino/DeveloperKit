//IOracleRepository apex = new OracleRepository("Server=172.19.221.90;Database=MX_ATM_SPI;User Id=SpiConnectSQL;Password=59iC0@n3C75Ql1;encrypt=false;");

//cargar DI

using ConsoleNet8;
using DevKit.ExecutionEngine.MySQL.Abstractions;
using DevKit.ExecutionEngine.MySQL.Implementations;
using DevKit.ExecutionEngine.MySQL.Settings;
using DevKit.ExecutionEngine.PostgreSQL;
using DevKit.ExecutionEngine.PostgreSQL.Abstractions;
using DevKit.ExecutionEngine.PostgreSQL.Settings;
using DevKit.ExecutionEngine.SQLServer.Abstractions;
using DevKit.ExecutionEngine.SQLServer.Implementations;
using DevKit.ExecutionEngine.SQLServer.Settings;
using Microsoft.Extensions.Options;
using System.Diagnostics;


internal class Program
{
    private static readonly HttpClient Client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
    static async Task Main(string[] args)
    {
        string url = "https://api-cat-cfdi.infosoft.mx/api/cfdi/services";
        var stopwatch = Stopwatch.StartNew(); // iniciar cronómetro

        var tasks = new List<Task>();
        var semaphore = new SemaphoreSlim(50); // máximo 10 en paralelo

        for (int i = 0; i < 10000; i++)
        {
            int requestNumber = i + 1;
            await semaphore.WaitAsync(); // esperar turno

            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await SendRequestAsync(url, requestNumber);
                }
                finally
                {
                    semaphore.Release(); // liberar slot
                }
            }));
        }


        // Ejecutar todas en paralelo y esperar a que terminen
        await Task.WhenAll(tasks);

        stopwatch.Stop(); // detener cronómetro
        Console.WriteLine($"✅ Todas las peticiones han finalizado en {stopwatch.Elapsed.TotalSeconds:F2} segundos.");

    }

    static async Task SendRequestAsync(string url, int requestNumber)
    {
        try
        {
            HttpResponseMessage response = await Client.GetAsync(url);

            response.EnsureSuccessStatusCode();
            await response.Content.ReadAsStringAsync();

            // Mostrar en consola cuando cada tarea se complete
            Console.WriteLine($"[{requestNumber}] Completada con éxito");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[{requestNumber}] Error: {ex.Message}");
        }
    }


    static IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(configurationBuilder =>
            {
                configurationBuilder.AddJsonFile("appsettings.json");
            }).ConfigureServices((builder, services) =>
            {
                services.Configure<DbOptions>(builder.Configuration.GetSection(DbOptions.SectionKey));

                services.AddKeyedScoped<ISQLServerProvider>("Infomex",
                    (provider, _) =>
                    {
                        DbOptions dbOptions = provider.GetRequiredService<IOptions<DbOptions>>().Value;

                        SqlOptions options = new SqlOptions
                        {
                            ConnectionString = dbOptions.ConnectionInfomex
                        };

                        return new SQLServerProvider(Options.Create(options));
                    });

                services.AddScoped<ISQLServerProvider>(provider =>
                {
                    DbOptions dbOptions = provider.GetRequiredService<IOptions<DbOptions>>().Value;

                    SqlOptions options = new SqlOptions
                    {
                        ConnectionString = dbOptions.ConnectionInfomex
                    };

                    return new SQLServerProvider(Options.Create(options));
                });

                services.AddScoped<IMySqlProvider>(provider =>
                {
                    DbOptions dbOptions = provider.GetRequiredService<IOptions<DbOptions>>().Value;
                    MySqlOptions options = new MySqlOptions
                    {
                        ConnectionString = dbOptions.MySql,
                        BulkCopy =
                        {
                                AllowLoadLocalInfile = true
                        }
                    };
                    return new MySqlProvider(Options.Create(options));
                });

                services.AddScoped<IPostgreSqlProvider>(provider =>
                {
                    DbOptions dbOptions = provider.GetRequiredService<IOptions<DbOptions>>().Value;
                    PostgreOptions options = new PostgreOptions
                    {
                        ConnectionString = dbOptions.PosgreSql
                    };
                    return new PostgreSqlProvider(Options.Create(options));
                });

                services.AddMcpServer()
                        .WithStdioServerTransport()
                        .WithToolsFromAssembly();

            });
    }
}
