namespace DevKit.ExecutionEngine.SQLServer.Logging
{
    /// <summary>
    /// Implementación por defecto de IQueryLogger que escribe en la consola de depuración
    /// </summary>
    public class DefaultQueryLogger : IQueryLogger
    {
        /// <summary>
        /// Nivel mínimo de log
        /// </summary>
        public IQueryLogger.LogLevel MinimumLevel { get; set; } = IQueryLogger.LogLevel.Debug;

        /// <summary>
        /// Registra una consulta SQL
        /// </summary>
        public void LogQuery(string sql, IDictionary<string, object> parameters = null, IQueryLogger.LogLevel level = IQueryLogger.LogLevel.Debug, string message = null)
        {
            if (level < MinimumLevel)
            {
                return;
            }

            StringBuilder logMessage = new();

            if (!string.IsNullOrEmpty(message))
            {
                logMessage.AppendLine($"{message}:");
            }

            logMessage.AppendLine("SQL:");
            logMessage.AppendLine(sql);

            if (parameters != null && parameters.Count > 0)
            {
                logMessage.AppendLine();
                logMessage.AppendLine("Parameters:");

                foreach (KeyValuePair<string, object> param in parameters)
                {
                    logMessage.AppendLine($"{param.Key} = {param.Value}");
                }
            }

            // Escribir en la salida de depuración y en la consola
            string formattedMessage = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}] [{level}] {logMessage}";
            Debug.WriteLine(formattedMessage);
            Console.WriteLine(formattedMessage);
        }
    }
}
