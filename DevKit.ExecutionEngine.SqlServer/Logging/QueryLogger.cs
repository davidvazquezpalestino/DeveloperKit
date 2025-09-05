namespace DevKit.ExecutionEngine.SQLServer.Logging
{
    /// <summary>
    /// Clase estática para el logging de consultas SQL
    /// </summary>
    public static class QueryLogger
    {
        private static IQueryLogger Logger = new DefaultQueryLogger();

        /// <summary>
        /// Establece el logger personalizado
        /// </summary>
        public static void SetLogger(IQueryLogger logger)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Establece el nivel mínimo de log
        /// </summary>
        public static void SetMinimumLevel(IQueryLogger.LogLevel level)
        {
            if (Logger is DefaultQueryLogger defaultLogger)
            {
                defaultLogger.MinimumLevel = level;
            }
        }

        /// <summary>
        /// Registra una consulta SQL
        /// </summary>
        public static void LogQuery(string sql, IDictionary<string, object> parameters = null,
                                  IQueryLogger.LogLevel level = IQueryLogger.LogLevel.Debug, string message = null)
        {
            Logger?.LogQuery(sql, parameters, level, message);
        }
    }
}
