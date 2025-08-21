//IOracleRepository apex = new OracleRepository("Server=172.19.221.90;Database=MX_ATM_SPI;User Id=SpiConnectSQL;Password=59iC0@n3C75Ql1;encrypt=false;");

//cargar DI

using DevKit.ExecutionEngine.MySQL;
using DevKit.ExecutionEngine.MySQL.Abstractions;
using DevKit.ExecutionEngine.MySQL.Settings;
using DevKit.ExecutionEngine.PostgreSQL;
using DevKit.ExecutionEngine.PostgreSQL.Abstractions;
using DevKit.ExecutionEngine.PostgreSQL.Settings;
using DevKit.ExecutionEngine.SQLServer.Abstractions;
using DevKit.ExecutionEngine.SQLServer.Implementations;
using DevKit.ExecutionEngine.SQLServer.Settings;
using Microsoft.Extensions.Options;
using System.Data;

IHost host = CreateHostBuilder().Build();


ISQLServerDatabaseProvider infomexDataBase = host.Services.GetRequiredKeyedService<ISQLServerDatabaseProvider>("Infomex");

DateTime currentDateTime = await infomexDataBase.GetCurrentDateTimeAsync();
Console.WriteLine(currentDateTime);

Console.WriteLine("Consultando SQL Sever");

DataTable table = await infomexDataBase.ExecuteQueryAsTableAsync("SELECT * FROM Sepomex.Asentamientos");
table.TableName = "Asentamientos";

IMySQLDatabaseProvider mySqlDatabase = host.Services.GetRequiredService<IMySQLDatabaseProvider>();
await mySqlDatabase.ExecuteBulkInsertToTableAsync(table, table.TableName);

Console.WriteLine("consultando MySQL");
mySqlDatabase.ExecuteBulkInsertToTable(table, table.TableName);

DataTable asTable = mySqlDatabase.ExecuteQueryAsTable("SELECT * FROM Sepomex.Asentamientos");
asTable.TableName = "AsentamientosV2";

IPostgreSqlDatabaseProvider postgreDatabase = host.Services.GetRequiredService<IPostgreSqlDatabaseProvider>();

await postgreDatabase.ExecuteBulkInsertToTableAsync(table, table.TableName);

table.TableName = "AsentamientosV3";
postgreDatabase.ExecuteBulkInsertToTable(table, table.TableName);

DataTable tablaPostgres = await postgreDatabase.ExecuteQueryAsTableAsync("SELECT * FROM public.\"AsentamientosV3\";");
Console.WriteLine("Consultando PostgreSQL");

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

            services.AddKeyedScoped<ISQLServerDatabaseProvider>("Infomex", (provider, _) =>
            {
                RepositoryOptions repositoryOptions = provider.GetRequiredService<IOptions<RepositoryOptions>>().Value;

                SqlOptions options = new SqlOptions
                {
                    ConnectionString = repositoryOptions.ConnectionStringInfomex
                };

                return new SQLServerDatabaseProvider(Options.Create(options));
            });

            services.AddScoped<ISQLServerDatabaseProvider>(provider =>
            {
                RepositoryOptions repositoryOptions = provider.GetRequiredService<IOptions<RepositoryOptions>>().Value;

                SqlOptions options = new SqlOptions
                {
                    ConnectionString = repositoryOptions.ConnectionStringInfomex
                };

                return new SQLServerDatabaseProvider(Options.Create(options));
            });

            services.AddScoped<IMySQLDatabaseProvider>(provider =>
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
                return new MySqlDatabaseProvider(Options.Create(options));
            });

            services.AddScoped<IPostgreSqlDatabaseProvider>(provider =>
            {
                RepositoryOptions repositoryOptions = provider.GetRequiredService<IOptions<RepositoryOptions>>().Value;
                PostgreOptions options = new PostgreOptions
                {
                    ConnectionString = repositoryOptions.PosgreSql
                };
                return new PostgreSqlDatabaseProvider(Options.Create(options));
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