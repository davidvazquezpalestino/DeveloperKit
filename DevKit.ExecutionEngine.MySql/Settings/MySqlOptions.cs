namespace DevKit.ExecutionEngine.MySQL.Settings;

/// <summary>
/// Provides configuration options for the MySQL database provider.
/// </summary>
public class MySqlOptions
{
    public MySqlOptions()
    {
        ConnectionPooling = new ConnectionPoolingOptions();
        BulkCopy = new BulkCopyAdvancedOptions();
    }
    /// <summary>
    /// Gets or sets the connection string for the MySQL database.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Gets or sets the command timeout in seconds. Defaults to 30.
    /// </summary>
    public int CommandTimeout { get; set; } = 0;

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
    public BulkCopyAdvancedOptions BulkCopy { get; set; } = new BulkCopyAdvancedOptions();


}
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
    public int MinPoolSize { get; set; } = 10;

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

    /// <summary>
    /// When true, appends AllowLoadLocalInfile=true to the MySQL connection string to enable LOCAL INFILE operations.
    /// Default is true because MySqlBulkCopy relies on LOCAL INFILE under the hood.
    /// </summary>
    public bool AllowLoadLocalInfile { get; set; } = true;

}