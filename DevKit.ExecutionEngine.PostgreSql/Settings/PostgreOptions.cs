namespace DevKit.ExecutionEngine.PostgreSql.Settings;

/// <summary>
/// Provides configuration options for the PostgreSQL database provider.
/// </summary>
public class PostgreOptions
{
    /// <summary>
    /// Gets or sets the connection string for the PostgreSQL database.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the command timeout in seconds. Defaults to 30.
    /// </summary>
    public int CommandTimeout { get; set; } = 30;

    /// <summary>
    /// Gets or sets a function to configure the application name for the connection.
    /// </summary>
    public Func<string> ConfigureApplication { get; set; }

    /// <summary>
    /// Gets or sets the connection pooling options.
    /// </summary>
    public ConnectionPoolingOptions ConnectionPooling { get; set; }

    /// <summary>
    /// Gets or sets the advanced options for bulk copy operations.
    /// </summary>
    public BulkCopyAdvancedOptions BulkCopy { get; set; }

    /// <summary>
    /// Provides configuration for connection pooling.
    /// </summary>
    public class ConnectionPoolingOptions
    {
        /// <summary>
        /// Gets or sets a value indicating whether connection pooling is enabled. Defaults to true.
        /// </summary>
        public bool Pooling { get; set; } = true;

        /// <summary>
        /// Gets or sets the minimum pool size. Defaults to 1.
        /// </summary>
        public int MinPoolSize { get; set; } = 1;

        /// <summary>
        /// Gets or sets the maximum pool size. Defaults to 100.
        /// </summary>
        public int MaxPoolSize { get; set; } = 100;
    }

    /// <summary>
    /// Provides advanced options for bulk copy operations.
    /// </summary>
    public class BulkCopyAdvancedOptions
    {
        /// <summary>
        /// Gets or sets the batch size for bulk copy operations. Defaults to 5000.
        /// </summary>
        public int BatchSize { get; set; } = 5000;

        /// <summary>
        /// Gets or sets the timeout for bulk copy operations in seconds. Defaults to 300.
        /// </summary>
        public int BulkCopyTimeout { get; set; } = 300;
    }
}
