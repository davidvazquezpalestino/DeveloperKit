using DevKit.CrystalToolkit.Abstracciones;
using DevKit.CrystalToolkit.Enumeraciones;
using DevKit.CrystalToolkit.Servicios;
using DevKit.ExecutionEngine.Abstractions.Interfaces.SqlServer;
using DevKit.ExecutionEngine.SqlServer.Settings;
using Microsoft.Extensions.Options;

namespace ConsoleNet48;

using System;

internal class Program
{
    static readonly ISQLServerDatabaseProvider Repository;
    static readonly IReport Report;
    static readonly Stopwatch Stopwatch;
    static readonly string[] Connections = new string[]
    {
        "Data Source=209.46.121.6;Initial Catalog=IERP_SOTRES_NOMINA;User ID=SA;Password=Mssql2025;Encrypt=False",//sotres
        "Data Source=192.168.8.2;Initial Catalog=IERP_CAZ;User ID=SA;Password=cz1023027*;encrypt=false;", //caz
        "Data Source=74.208.168.84;Initial Catalog=FacturacionIntelix;User ID=SA;Password=RkQglhn#1tl;TrustServerCertificate=True", //caz
        "Server=209.46.121.6;Database=Infosoft;User Id=Infosoft;Password=Infosoft2025;encrypt=false;"
    };
    static readonly string[] Reports = new string[]
    {
        "C:\\GITHUB\\SOLUCIONES\\DOCUMENTACION\\FORMATOS CRYSTAL\\NOMINA\\fmtComprobanteNomina.rpt",//sotres
        "C:\\Github\\Documentacion\\Formatos Crystal\\CAZ\\fmtComprobanteFiscal.rpt",//caz
        "C:\\GitHub\\Soluciones\\Documentacion\\Formatos Crystal\\Constancia de retenciones\\fmtConstanciaRetencionesDividendos.rpt",
        "C:\\GitHub\\Soluciones\\Documentacion\\Formatos Crystal\\Constancia de retenciones\\fmtConstanciaRetencionesIntereses.rpt",
        "C:\\Infosoft\\Formatos\\fmtCotizacion.rpt"
    };

    static Program()
    {
        string report = Reports[4];
        Repository = new SQLServerDatabaseProvider(Options.Create(new SqlOptions()));
        Report = new Report();
        Report.LoadReport(report);
        Stopwatch = new Stopwatch();
    }

    static async Task Main(string[] args)
    {
        await ConstanciaRetenciones();
    }

    private static async Task Sotres()
    {

        ICollection<Comprobante> comprobantes = await Repository
            .ExecuteQueryAsListAsync(
                query: @"SELECT cf.IdComprobante,
                                   NombreArchivoGenerado = CONCAT(cf.RFCEmisor,'.', cf.Fecha,'.', cf.Serie,cf.Folio),
                                   Folder = Nomina.Concepto
                            FROM dbo.tIMPcomprobantesFiscales cf
                            INNER JOIN dbo.tNOMnominas Nomina ON Nomina.IdNomina = cf.IdNomina
                            WHERE cf.IdComprobante IN (76475, 76476, 76477, 76478);

                        ",
                expression: reader => reader.GetItem<Comprobante>());

        await Report.SetDatabaseLogonAsync(connectionString: Repository.ConnectionString);

        try
        {
            Console.WriteLine("iniciando");
            Stopwatch.Start();

            List<Task> tareas = comprobantes
                .Select(async comprobante =>
                {
                    if (Directory.Exists($"C:\\Intelix\\Facturacion\\ArchivosGenerados\\SSE840120QD3\\{comprobante.Folder}") == false)
                    {
                        Directory.CreateDirectory($"C:\\Intelix\\Facturacion\\ArchivosGenerados\\SSE840120QD3\\{comprobante.Folder}");
                    }
                    string rutacfdi = $@"C:\Intelix\Facturacion\ArchivosGenerados\SSE840120QD3\{comprobante.Folder}\{comprobante.NombreArchivoGenerado}.pdf";

                    await Report.ExportToDiskAsync(formatType: FormatType.Pdf,
                        filePath: rutacfdi,
                        parameter: new Dictionary<string, object>
                        {
                            { "IdComprobante", comprobante.IdComprobante }
                        });

                    Console.WriteLine($"Procesando {comprobante.NombreArchivoGenerado} - Completado");
                })
                .ToList();

            await Task.WhenAll(tareas);

            Stopwatch.Stop();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }

        Console.WriteLine($"El método tardó {Stopwatch.Elapsed.Seconds} segundos en ejecutarse.");
    }
    private static async Task Caz()
    {
        string directorioArchivosGenerados = string.Empty;

        ICollection<Comprobante> comprobantes = await Repository.ExecuteQueryAsListAsync(
            query: "SELECT comprobante.IdComprobante, Bitacora.NombreArchivoGenerado\r\nFROM dbo.tIMPcomprobantesFiscales comprobante\r\nINNER JOIN dbo.tFELestadoCuentaBancario edo ON edo.IdComprobante = comprobante.IdComprobante\r\nINNER JOIN dbo.tCTLbitacoraCfdi Bitacora ON Bitacora.IdComprobante = comprobante.IdComprobante\r\nWHERE comprobante.RFCReceptor <> 'XAXX010101000' AND edo.IdPeriodo = 391;",
            expression: reader => reader.GetItem<Comprobante>());


        Repository.ExecuteQueryAsSingle(
            "SELECT DirectorioArchivosGenerados FROM dbo.tCTLcertificadoCFDi  WHERE Id <> 0 AND EsActivo = 1", reader =>
                directorioArchivosGenerados = reader.GetValue<string>("DirectorioArchivosGenerados"));

        await Report.SetDatabaseLogonAsync(connectionString: Repository.ConnectionString);

        try
        {
            Console.WriteLine("iniciando");
            Stopwatch.Start();

            List<Task> tareas = comprobantes
                .Select(async comprobante =>
                {
                    await Report.ExportToDiskAsync(formatType: FormatType.Pdf,
                        filePath: $"{Path.Combine(directorioArchivosGenerados, comprobante.NombreArchivoGenerado)}.pdf",
                        parameter: new Dictionary<string, object>
                        {
                            { "IdComprobante", comprobante.IdComprobante }
                        });

                    Console.WriteLine($"Procesando {comprobante.NombreArchivoGenerado} - Completado");
                })
                .ToList();

            await Task.WhenAll(tareas);

            Stopwatch.Stop();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }

        Console.WriteLine($"El método tardó {Stopwatch.Elapsed.Seconds} segundos en ejecutarse.");
    }
    private static async Task ConstanciaRetenciones()
    {
        //string directorioArchivosGenerados = string.Empty;

        //const string query = "SELECT Constancia.IdConstanciaRetencion, Constancia.Ejercicio, NombreArchivoGenerado = CONCAT(Constancia.ReceptorRfc,'.',Constancia.IdConstanciaRetencion), hst.ArchivoXML  " +
        //                     "FROM dbo.tFELconstanciaRetenciones Constancia " +
        //                     "LEFT JOIN dbo.tCTLhistoricoConstanciaRetencion hst ON hst.IdConstanciaRetencion = Constancia.IdConstanciaRetencion " +
        //                     "WHERE hst.IdHistoricoConstanciaRetencion IS NOT NULL AND " +
        //                     "Constancia.IdConstanciaRetencion IN (1274,1283,1299) ;";
        //Debug.WriteLine(query);


        //ICollection<Constancia> constancias = await Repository.FetchItemsFromQueryAsync(
        //    query: query,
        //    expression: reader => reader.GetItem<Constancia>());

        //Repository.FetchItemFromQuery("SELECT DirectorioArchivosGenerados FROM dbo.tCTLcertificadoCFDi WHERE EsActivo = 1", reader =>
        //      directorioArchivosGenerados = reader.GetString("DirectorioArchivosGenerados"));

        await Report.SetDatabaseLogonAsync(connectionString: Connections[3]);
        await Report.ExportToDiskAsync(formatType: FormatType.Pdf,
            filePath: "1.pdf",
            parameter: new Dictionary<string, object>
            {
                { "CotizacionID",3 }
            });

        try
        {
            Console.WriteLine("iniciando");
            Stopwatch.Start();

            //List<Task> tareas = constancias.Select(async (constancia, index) =>
            //    {
            //        await Report.ExportToDiskAsync(formatType: FormatType.Pdf,
            //            filePath: $"{Path.Combine(directorioArchivosGenerados, constancia.Ejercicio.ToString(), constancia.NombreArchivoGenerado)}.pdf",
            //            collection: new Dictionary<string, object>
            //            {
            //                { "IdConstanciaRetencion", constancia.IdConstanciaRetencion }
            //            });

            //        File.WriteAllText($"{Path.Combine(directorioArchivosGenerados, constancia.Ejercicio.ToString(), constancia.NombreArchivoGenerado)}.xml",
            //            constancia.ArchivoXml,
            //            Encoding.UTF8);


            //        Console.WriteLine($"Procesando {constancia.NombreArchivoGenerado} - Completado {index}");
            //    })
            //    .ToList();

            //await Task.WhenAll(tareas);

            Stopwatch.Stop();
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }

        Console.WriteLine($"El método tardó {Stopwatch.Elapsed.Seconds} segundos en ejecutarse.");
    }
}

public class Comprobante
{
    public int IdComprobante { get; set; }
    public string NombreArchivoGenerado { get; set; }
    public string Folder { get; set; }
}

public class Constancia
{
    public int IdConstanciaRetencion { get; set; }
    public int Ejercicio { get; set; }
    public string NombreArchivoGenerado { get; set; }
    public string ArchivoXml { get; set; }
}