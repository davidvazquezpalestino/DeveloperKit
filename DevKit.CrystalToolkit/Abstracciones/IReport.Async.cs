namespace DevKit.CrystalToolkit.Abstracciones;

public partial interface IReport
{
    /// <summary>Representa el documento del informe.</summary>
    ReportDocument ReportDocument { get; }

    /// <summary>Establece la información de inicio de sesión de la base de datos de forma asincrónica.</summary>
    /// <param name="serverName">Nombre del servidor de la base de datos.</param>
    /// <param name="userID">ID de usuario para la autenticación.</param>
    /// <param name="password">Contraseña del usuario para la autenticación.</param>
    /// <param name="databaseName">Nombre de la base de datos.</param>
    Task SetDatabaseLogonAsync(string serverName, string userID, string password, string databaseName);

    /// <summary>Establece la información de inicio de sesión de la base de datos de forma asincrónica utilizando una cadena de conexión.</summary>
    /// <param name="connectionString">Cadena de conexión para la base de datos.</param>
    Task SetDatabaseLogonAsync(string connectionString);

    /// <summary>Exporta el informe al disco en un formato específico de manera asincrónica.</summary>
    /// <param name="formatType">Tipo de formato en el que se exportará el informe.</param>
    /// <param name="filePath">Ruta del archivo donde se guardará el informe exportado.</param>
    /// <param name="parameter">Diccionario de parámetros opcionales para el informe.</param>
    Task ExportToDiskAsync(FormatType formatType, string filePath, IDictionary<string, object> parameter = null);

    /// <summary>Exporta el informe al disco en formato de flujo de datos de manera asincrónica.</summary>
    /// <param name="parameter">Diccionario de parámetros opcionales para el informe.</param>
    /// <returns>Flujo de datos que contiene el informe exportado.</returns>
    Task<Stream> ExportToDiskAsync(IDictionary<string, object> parameter = null);

    /// <summary>Imprime el informe de forma asincrónica.</summary>
    /// <param name="parameter">Diccionario de parámetros opcionales para el informe.</param>
    /// <param name="copies">Número de copias a imprimir.</param>
    /// <param name="printerName">Nombre de la impresora a utilizar.</param>
    Task PrintAsync(IDictionary<string, object> parameter = null, int copies = 1, string printerName = null);
}