//IOracleRepository apex = new OracleRepository("Server=172.19.221.90;Database=MX_ATM_SPI;User Id=SpiConnectSQL;Password=59iC0@n3C75Ql1;encrypt=false;");

//cargar DI

using DevKit.ExecutionEngine.Excel.Abstractions;
using DevKit.ExecutionEngine.Excel.Implementations;
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
using DevKit.Extensions;
using DevKit.Extensions.DataTableExtension;
using Microsoft.Extensions.Options;
using System.Data;


IHost host = CreateHostBuilder().Build();

IExcelProvider excel =
    new ExcelProvider("C:\\Users\\vazqu\\Downloads\\PLANTILLA NOMINA 31 oct 2025.xlsx");

DataTable dataTable = excel.GetTable("Datos");
DataTable table1 = dataTable
    .Where(row =>
    {
        string value = row.GetValue<string>("Codigo");
        return value != "0" && string.IsNullOrWhiteSpace(value) == false;
    });

dataTable.RemoveAll(row =>
{
    string value = row.GetValue<string>("Codigo");
    return value == "0" || string.IsNullOrWhiteSpace(value);
});

ISQLServerProvider infomex = host.Services.GetRequiredKeyedService<ISQLServerProvider>("Infomex");

DateTime currentDateTime = await infomex.GetCurrentDateTimeAsync();
Console.WriteLine(currentDateTime);

Console.WriteLine("Consultando SQL Sever");

DataTable table = await infomex.ExecuteQueryAsTableAsync("SELECT * FROM Sepomex.Asentamientos");
table.TableName = "Asentamientos";


List<Asentamientos> asentamientosList = await infomex
    .From<Asentamientos>("Comprobante", "VW_Asentamientos")
    .Where(u => u.Estado == "puebla" && u.Asentamiento.StartsWith("santa maria"))
    .OrderBy(u => u.Asentamiento)
    .ToListAsync();


List<Asentamientos> asentamientosList2 = await infomex
    .From<Asentamientos>("Comprobante", "VW_Asentamientos")
    .ToListAsync();



int registros = infomex
    .From<Asentamientos>("Comprobante", "VW_Asentamientos")
    .Where(u => u.Estado == "VERACRUZ")
    .Count();


int pageSize = 5;
int totalPages = (int)Math.Ceiling((double)registros / pageSize);

List<object> queryState = new();
for (int pageNumber = 0; pageNumber < totalPages; pageNumber++)
{
    var select = infomex
        .From<Asentamientos>("Comprobante", "VW_Asentamientos")
        .Where(u => u.Estado == "VERACRUZ")
        .OrderBy(u => u.Asentamiento)
        .Skip(pageNumber * pageSize)
        .Take(pageSize)
        .Select(u => new { u.ColoniaID, u.Asentamiento })
        .ToList();

    Console.WriteLine($"Página {pageNumber}");
    queryState.Add(select);
}






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
    public class RepositoryOptions
    {
        public const string SectionKey = nameof(RepositoryOptions);
        public string ConnectionStringInfomex { get; set; }
        public string MySql { get; set; }
        public string PosgreSql { get; set; }
    }
}

public class Asentamientos
{
    public int ColoniaID { get; set; }
    public string CodigoPostal { get; set; }
    public string NumeroAsentamiento { get; set; }
    public string Asentamiento { get; set; }
    public string NumeroMunicipio { get; set; }
    public string Municipio { get; set; }
    public string NumeroLocalidad { get; set; }
    public string Localidad { get; set; }
    public string NumeroEstado { get; set; }
    public string Estado { get; set; }
    public string NumeroPais { get; set; }
    public string Pais { get; set; }
}