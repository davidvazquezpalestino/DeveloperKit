namespace DevKit.CrystalToolkit.Abstracciones;

/// <summary>Define la interfaz para la generación y manipulación de informes con Crystal Reports.</summary>
public partial interface IReport : IDisposable
{
    /// <summary>Establece la conexión a la base de datos para el informe.</summary>
    /// <param name="serverName">Nombre del servidor de la base de datos.</param>
    /// <param name="userID">Nombre de usuario para la conexión a la base de datos.</param>
    /// <param name="password">Contraseña para la conexión a la base de datos.</param>
    /// <param name="databaseName">Nombre de la base de datos.</param>
    void SetDatabaseLogon(string serverName, string userID, string password, string databaseName);

    /// <summary>Establece la conexión a la base de datos usando una cadena de conexión.</summary>
    void SetDatabaseLogon(string connectionString);
    /// <summary>Carga un informe desde la ruta especificada.</summary>
    void LoadReport(string reportPath);
    /// <summary>Exporta el informe al disco en el formato especificado.</summary>
    void ExportToDisk(FormatType formatType, string filePath, IDictionary<string, object> parameter = null);
    /// <summary>Exporta el informe a un stream.</summary>
    Stream ExportToDisk(IDictionary<string, object> parameter = null);
    /// <summary>Imprime el informe con los parámetros especificados.</summary>
    void Print(IDictionary<string, object> parameter = null, int copies = 1, string printerName = null);
}