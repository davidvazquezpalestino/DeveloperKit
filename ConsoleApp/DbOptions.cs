namespace ConsoleNet8;

/// <summary>
/// Configuration options for repository connections.
/// </summary>
public class DbOptions
{
    /// <summary>
    /// The configuration section key.
    /// </summary>
    public const string SectionKey = nameof(DbOptions);

    /// <summary>
    /// Gets or sets the Infomex connection string.
    /// </summary>
    public string ConnectionInfomex { get; set; }

    /// <summary>
    /// Gets or sets the MySQL connection string.
    /// </summary>
    public string MySql { get; set; }

    /// <summary>
    /// Gets or sets the PostgreSQL connection string.
    /// </summary>
    public string PosgreSql { get; set; }
}