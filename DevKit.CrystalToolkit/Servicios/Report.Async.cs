namespace DevKit.CrystalToolkit.Servicios;

public partial class Report
{
    /// <summary>Establece la conexión a la base de datos para el informe de forma asíncrona.</summary>
    /// <param name="serverName">Nombre del servidor de base de datos.</param>
    /// <param name="userID">Nombre de usuario para la autenticación.</param>
    /// <param name="password">Contraseña para la autenticación.</param>
    /// <param name="databaseName">Nombre de la base de datos.</param>
    /// <returns>Tarea que representa la operación asíncrona.</returns>
    public Task SetDatabaseLogonAsync(string serverName, string userID, string password, string databaseName)
    {
        ConnectionInfo.ServerName = serverName;
        ConnectionInfo.UserID = userID;
        ConnectionInfo.Password = password;
        ConnectionInfo.DatabaseName = databaseName;

        return SetReportLogonAsync();
    }

    /// <summary>Establece la conexión a la base de datos usando una cadena de conexión de forma asíncrona.</summary>
    /// <param name="connectionString">Cadena de conexión a la base de datos.</param>
    /// <returns>Tarea que representa la operación asíncrona.</returns>
    public Task SetDatabaseLogonAsync(string connectionString)
    {
        SqlConnectionStringBuilder connection = new SqlConnectionStringBuilder(connectionString);
        ConnectionInfo.ServerName = connection.DataSource;
        ConnectionInfo.UserID = connection.UserID;
        ConnectionInfo.Password = connection.Password;
        ConnectionInfo.DatabaseName = connection.InitialCatalog;

        return SetReportLogonAsync();
    }
    /// <summary>Configura la información de inicio de sesión para todas las tablas y subtablas del informe de forma asíncrona.</summary>
    /// <returns>Tarea que representa la operación asíncrona.</returns>
    private Task SetReportLogonAsync()
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
        return Task.CompletedTask;
    }
    /// <summary>Libera los recursos administrados utilizados por la instancia.</summary>
    public void Dispose()
    {
        ReportDocument?.Dispose();
    }
}