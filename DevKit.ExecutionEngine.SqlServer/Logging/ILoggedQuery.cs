namespace DevKit.ExecutionEngine.SQLServer.Logging
{
    /// <summary>
    /// Interface for queries that can be logged
    /// </summary>
    public interface ILoggedQuery
    {
        /// <summary>
        /// SQL that was executed
        /// </summary>
        string ExecutedSql { get; }
        
        /// <summary>
        /// Query parameters
        /// </summary>
        IDictionary<string, object> Parameters { get; }
    }
}
