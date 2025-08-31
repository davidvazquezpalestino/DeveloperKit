namespace DevKit.ExecutionEngine.SQLServer.Logging
{
    /// <summary>
    /// Interfaz para el logger de consultas SQL
    /// </summary>
    public interface IQueryLogger
    {
        /// <summary>
        /// Niveles de log disponibles
        /// </summary>
        public enum LogLevel
        {
            /// <summary>
            /// Nivel de depuración
            /// </summary>
            Debug,

            /// <summary>
            /// Nivel de información
            /// </summary>
            Information,

            /// <summary>
            /// Nivel de advertencia
            /// </summary>
            Warning,

            /// <summary>
            /// Nivel de error
            /// </summary>
            Error
        }

        /// <summary>
        /// Registra una consulta SQL
        /// </summary>
        /// <param name="sql">Consulta SQL</param>
        /// <param name="parameters">Parámetros de la consulta</param>
        /// <param name="level">Nivel de log</param>
        /// <param name="message">Mensaje descriptivo (opcional)</param>
        void LogQuery(string sql, IDictionary<string, object> parameters = null, LogLevel level = LogLevel.Debug, string message = null);
    }
}
