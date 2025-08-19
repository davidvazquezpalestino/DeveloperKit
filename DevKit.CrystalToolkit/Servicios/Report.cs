namespace DevKit.CrystalToolkit.Servicios;

/// <summary>Implementación concreta de IReport para la generación y manipulación de informes con Crystal Reports.</summary>
public partial class Report : IReport
{
    /// <summary>Obtiene la información de conexión a la base de datos.</summary>
    private ConnectionInfo ConnectionInfo { get; } = new ConnectionInfo();
    /// <summary>Obtiene el documento del informe de Crystal Reports.</summary>
    public ReportDocument ReportDocument { get; } = new ReportDocument();

    /// <summary>Establece la conexión a la base de datos para el informe.</summary>
    /// <param name="serverName">Nombre del servidor de base de datos.</param>
    /// <param name="userID">Nombre de usuario para la autenticación.</param>
    /// <param name="password">Contraseña para la autenticación.</param>
    /// <param name="databaseName">Nombre de la base de datos.</param>
    public void SetDatabaseLogon(string serverName, string userID, string password, string databaseName)
    {
        ConnectionInfo.ServerName = serverName;
        ConnectionInfo.UserID = userID;
        ConnectionInfo.Password = password;
        ConnectionInfo.DatabaseName = databaseName;

        SetReportLogon();
    }

    /// <summary>Establece la conexión a la base de datos usando una cadena de conexión.</summary>
    /// <param name="connectionString">Cadena de conexión a la base de datos.</param>
    public void SetDatabaseLogon(string connectionString)
    {
        SqlConnectionStringBuilder connection = new SqlConnectionStringBuilder(connectionString);
        ConnectionInfo.ServerName = connection.DataSource;
        ConnectionInfo.UserID = connection.UserID;
        ConnectionInfo.Password = connection.Password;
        ConnectionInfo.DatabaseName = connection.InitialCatalog;

        SetReportLogon();
    }

    /// <summary>Carga un informe desde la ruta especificada.</summary>
    /// <param name="reportPath">Ruta completa al archivo de informe.</param>
    public void LoadReport(string reportPath) => ReportDocument.Load(reportPath);

    /// <summary>Configura la información de inicio de sesión para todas las tablas y subtablas del informe.</summary>
    private void SetReportLogon()
    {
        foreach (Table table in ReportDocument.Database.Tables)
        {
            TableLogOnInfo logOnInfo = table.LogOnInfo;
            logOnInfo.ConnectionInfo = ConnectionInfo;
            table.ApplyLogOnInfo(logOnInfo);
            table.Location = logOnInfo.TableName;
            Console.WriteLine($"Tabla: {logOnInfo.TableName}");
        }

        foreach (Section section in ReportDocument.ReportDefinition.Sections)
        {
            foreach (ReportObject reportObject in section.ReportObjects)
            {
                if (reportObject.Kind == ReportObjectKind.SubreportObject)
                {
                    SubreportObject subreportObject = (SubreportObject)reportObject;
                    ReportDocument subreportDocument = subreportObject.OpenSubreport(subreportObject.SubreportName);

                    foreach (Table table in subreportDocument.Database.Tables)
                    {
                        TableLogOnInfo logOnInfo = table.LogOnInfo;
                        logOnInfo.ConnectionInfo = ConnectionInfo;
                        table.ApplyLogOnInfo(logOnInfo);
                        table.Location = logOnInfo.TableName;
                        Console.WriteLine($"SubTabla: {logOnInfo.TableName}");
                    }
                }
            }
        }
    }
}