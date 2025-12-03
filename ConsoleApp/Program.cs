//IOracleRepository apex = new OracleRepository("Server=172.19.221.90;Database=MX_ATM_SPI;User Id=SpiConnectSQL;Password=59iC0@n3C75Ql1;encrypt=false;");

//cargar DI

using DevKit.ExecutionEngine.MySQL.Abstractions;
using DevKit.ExecutionEngine.MySQL.Implementations;
using DevKit.ExecutionEngine.MySQL.Settings;
using DevKit.ExecutionEngine.PostgreSQL;
using DevKit.ExecutionEngine.PostgreSQL.Abstractions;
using DevKit.ExecutionEngine.PostgreSQL.Settings;
using DevKit.ExecutionEngine.SQLServer.Abstractions;
using DevKit.ExecutionEngine.SQLServer.Extensions;
using DevKit.ExecutionEngine.SQLServer.Implementations;
using DevKit.ExecutionEngine.SQLServer.Settings;
using DevKit.Extensions.DataTableExtension;
using Microsoft.Extensions.Options;
using System.Data;


IHost host = CreateHostBuilder().Build();


ISQLServerProvider infomex = host.Services.GetRequiredKeyedService<ISQLServerProvider>("Infomex");
IMySqlProvider mySqlProvider = host.Services.GetRequiredService<IMySqlProvider>();

DateTime currentDateTime = await infomex.GetCurrentDateTimeAsync();
Console.WriteLine(currentDateTime);

Console.WriteLine("Consultando SQL Sever");

//DataTable table = await infomex.ExecuteQueryAsTableAsync("SELECT * FROM Sepomex.Asentamientos");
//table.TableName = "Asentamientos";



List<Asentamientos> asentamientosList = await infomex
    .From<Asentamientos>("Comprobante", "VW_Asentamientos")
    .Where(u => u.Estado == "puebla" && u.Asentamiento.StartsWith("santa maria"))
    .OrderBy(u => u.Asentamiento)
    .ToListAsync();


Asentamientos asentamiento = await infomex
    .From<Asentamientos>("Comprobante", "VW_Asentamientos")
    .Where(u => u.Estado == "puebla" && u.Asentamiento.StartsWith("santa maria"))
    .FirstOrDefaultAsync();


Console.WriteLine("Fin");

static IHostBuilder CreateHostBuilder()
{
    return Host.CreateDefaultBuilder()
        .ConfigureAppConfiguration(configurationBuilder =>
        {
            configurationBuilder.AddJsonFile("appsettings.json");
        }).ConfigureServices((builder, services) =>
        {
            services.Configure<RepositoryOptions>(builder.Configuration.GetSection(RepositoryOptions.SectionKey));

            services.AddKeyedScoped<ISQLServerProvider>("Infomex", (provider, _) =>
            {
                RepositoryOptions repositoryOptions = provider.GetRequiredService<IOptions<RepositoryOptions>>().Value;

                SqlOptions options = new SqlOptions
                {
                    ConnectionString = repositoryOptions.ConnectionStringInfomex
                };

                return new SQLServerProvider(Options.Create(options));
            });

            services.AddScoped<ISQLServerProvider>(provider =>
            {
                RepositoryOptions repositoryOptions = provider.GetRequiredService<IOptions<RepositoryOptions>>().Value;

                SqlOptions options = new SqlOptions
                {
                    ConnectionString = repositoryOptions.ConnectionStringInfomex
                };

                return new SQLServerProvider(Options.Create(options));
            });

            services.AddScoped<IMySqlProvider>(provider =>
            {
                RepositoryOptions repositoryOptions = provider.GetRequiredService<IOptions<RepositoryOptions>>().Value;
                MySqlOptions options = new MySqlOptions
                {
                    ConnectionString = repositoryOptions.MySql,
                    BulkCopy =
                    {
                        AllowLoadLocalInfile = true
                    }
                };
                return new MySqlProvider(Options.Create(options));
            });

            services.AddScoped<IPostgreSqlProvider>(provider =>
            {
                RepositoryOptions repositoryOptions = provider.GetRequiredService<IOptions<RepositoryOptions>>().Value;
                PostgreOptions options = new PostgreOptions
                {
                    ConnectionString = repositoryOptions.PosgreSql
                };
                return new PostgreSqlProvider(Options.Create(options));
            });

        });
}

namespace ConsoleNet8
{
    /// <summary>
    /// Configuration options for repository connections.
    /// </summary>
    public class RepositoryOptions
    {
        /// <summary>
        /// The configuration section key.
        /// </summary>
        public const string SectionKey = nameof(RepositoryOptions);

        /// <summary>
        /// Gets or sets the Infomex connection string.
        /// </summary>
        public string ConnectionStringInfomex { get; set; }

        /// <summary>
        /// Gets or sets the MySQL connection string.
        /// </summary>
        public string MySql { get; set; }

        /// <summary>
        /// Gets or sets the PostgreSQL connection string.
        /// </summary>
        public string PosgreSql { get; set; }
    }
}

/// <summary>
/// Represents a settlement or locality with postal and geographic information.
/// </summary>
public class Asentamientos
{
    /// <summary>
    /// Gets or sets the colony ID.
    /// </summary>
    /// <summary>
    /// Gets or sets the colony ID.
    /// </summary>
    public int ColoniaID { get; set; }

    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    /// <summary>
    /// Gets or sets the postal code.
    /// </summary>
    public string CodigoPostal { get; set; }

    /// <summary>
    /// Gets or sets the settlement number.
    /// </summary>
    /// <summary>
    /// Gets or sets the settlement number.
    /// </summary>
    public string NumeroAsentamiento { get; set; }

    /// <summary>
    /// Gets or sets the settlement name.
    /// </summary>
    /// <summary>
    /// Gets or sets the settlement name.
    /// </summary>
    public string Asentamiento { get; set; }

    /// <summary>
    /// Gets or sets the municipality number.
    /// </summary>
    /// <summary>
    /// Gets or sets the municipality number.
    /// </summary>
    public string NumeroMunicipio { get; set; }

    /// <summary>
    /// Gets or sets the municipality name.
    /// </summary>
    /// <summary>
    /// Gets or sets the municipality name.
    /// </summary>
    public string Municipio { get; set; }

    /// <summary>
    /// Gets or sets the locality number.
    /// </summary>
    /// <summary>
    /// Gets or sets the locality number.
    /// </summary>
    public string NumeroLocalidad { get; set; }

    /// <summary>
    /// Gets or sets the locality name.
    /// </summary>
    /// <summary>
    /// Gets or sets the locality name.
    /// </summary>
    public string Localidad { get; set; }

    /// <summary>
    /// Gets or sets the state number.
    /// </summary>
    /// <summary>
    /// Gets or sets the state number.
    /// </summary>
    public string NumeroEstado { get; set; }

    /// <summary>
    /// Gets or sets the state name.
    /// </summary>
    /// <summary>
    /// Gets or sets the state name.
    /// </summary>
    public string Estado { get; set; }

    /// <summary>
    /// Gets or sets the country number.
    /// </summary>
    /// <summary>
    /// Gets or sets the country number.
    /// </summary>
    public string NumeroPais { get; set; }

    /// <summary>
    /// Gets or sets the country name.
    /// </summary>
    public string Pais { get; set; }
}