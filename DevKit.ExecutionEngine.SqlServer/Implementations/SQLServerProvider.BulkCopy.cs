

namespace DevKit.ExecutionEngine.SQLServer.Implementations
{
    public partial class SQLServerProvider
    {

        /// <summary>
        /// Realiza una inserción masiva de datos desde un DataTable a la tabla de destino de forma síncrona.
        /// </summary>
        /// <param name="source">DataTable que contiene los datos a insertar.</param>
        /// <param name="target">Nombre de la tabla de destino.</param>
        public void ExecuteBulkInsert(DataTable source, string target)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (string.IsNullOrWhiteSpace(target))
            {
                throw new ArgumentException("El nombre de la tabla de destino no puede estar vacío.", nameof(target));
            }

            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(Connection, SqlBulkCopyOptions.Default, Transaccion))
            {
                int defaultBatchSize = source.Rows.Count;

                bulkCopy.DestinationTableName = target;
                bulkCopy.BatchSize = defaultBatchSize;
                bulkCopy.NotifyAfter = defaultBatchSize;
                bulkCopy.BulkCopyTimeout = SqlOptions.BulkCopy.BulkCopyTimeout;

                if (SqlOptions.BulkCopy.BatchSize > 0)
                {
                    bulkCopy.BatchSize = SqlOptions.BulkCopy.BatchSize;
                }

                if (SqlOptions.BulkCopy.NotifyAfter > 0)
                {
                    bulkCopy.NotifyAfter = SqlOptions.BulkCopy.NotifyAfter;
                }

                foreach (DataColumn column in source.Columns)
                {
                    bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                }

                if (Connection.State == ConnectionState.Closed)
                {
                    Connection.Open();
                }

                bulkCopy.WriteToServer(source);
            }
        }

        /// <summary>
        /// Realiza una copia masiva de un DataTable a la tabla destino de forma asíncrona.
        /// </summary>
        public async Task ExecuteBulkInsertToTableAsync(DataTable source, string target, CancellationToken cancellationToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (string.IsNullOrWhiteSpace(target))
            {
                throw new ArgumentException("El nombre de la tabla de destino no puede estar vacío.", nameof(target));
            }

            bool isConnectionOwner = false;
            SqlTransaction transaction = null;

            try
            {
                // Use existing connection if it's open and not in a transaction
                if (Connection.State != ConnectionState.Open)
                {
                    await Connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                    isConnectionOwner = true;
                }

                // Only create a new transaction if we're not already in one
                if (Transaccion == null)
                {
                    transaction = await Task.Run(() => (SqlTransaction)Connection.BeginTransaction(), cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    transaction = Transaccion;
                }

                // Create the table if it doesn't exist
                using (SqlCommand command = Connection.CreateCommand())
                {
                    command.Transaction = transaction;
                    command.CommandText = CreateTableScriptSQL(source, target);
                    command.CommandTimeout = SqlOptions.CommandTimeout;
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }

                // Perform the bulk insert
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(Connection, SqlBulkCopyOptions.Default, transaction))
                {
                    bulkCopy.DestinationTableName = target;
                    bulkCopy.BatchSize = SqlOptions.BulkCopy.BatchSize > 0
                        ? SqlOptions.BulkCopy.BatchSize
                        : source.Rows.Count;
                    bulkCopy.BulkCopyTimeout = SqlOptions.BulkCopy.BulkCopyTimeout;
                    bulkCopy.NotifyAfter = SqlOptions.BulkCopy.NotifyAfter;

                    foreach (DataColumn column in source.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }

                    await bulkCopy.WriteToServerAsync(source, cancellationToken).ConfigureAwait(false);
                }

                // Only commit if we created the transaction
                if (transaction != null && transaction == Transaccion)
                {
                    await Task.Run(() => transaction.Commit(), cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception)
            {
                // Only rollback if we created the transaction
                if (transaction != null && transaction == Transaccion)
                {
                    await Task.Run(() => transaction.Rollback(), cancellationToken).ConfigureAwait(false);
                }
                throw;
            }
            finally
            {
                if (isConnectionOwner && Connection?.State == ConnectionState.Open)
                {
                    Connection.Close();
                }
            }
        }

        /// <summary>
        /// Realiza una inserción masiva de datos desde un DataTable a la tabla de destino de forma asíncrona.
        /// </summary>
        public async Task ExecuteBulkInsertAsync(DataTable source, string target, CancellationToken cancellationToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (string.IsNullOrWhiteSpace(target))
            {
                throw new ArgumentException("El nombre de la tabla de destino no puede estar vacío.", nameof(target));
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, null))
                {
                    bulkCopy.DestinationTableName = target;
                    int defaultBatchSize = source.Rows.Count;
                    bulkCopy.BatchSize = defaultBatchSize;

                    if (SqlOptions.BulkCopy.BatchSize > 0)
                    {
                        bulkCopy.BatchSize = SqlOptions.BulkCopy.BatchSize;
                    }

                    bulkCopy.BulkCopyTimeout = SqlOptions.BulkCopy.BulkCopyTimeout;

                    foreach (DataColumn column in source.Columns)
                    {
                        bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                    }

                    await bulkCopy.WriteToServerAsync(source, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Realiza una inserción masiva de datos con configuración personalizada de forma asíncrona.
        /// </summary>
        public async Task ExecuteBulkInsertAsync(DataTable source, BulkOperationsConfiguration configuration, CancellationToken cancellationToken = default)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (string.IsNullOrWhiteSpace(configuration.DestinationTableName))
            {
                throw new ArgumentException("DestinationTableName no puede estar vacío.");
            }

            using (SqlConnection connection = new SqlConnection(ConnectionString))
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, null))
                {
                    bulkCopy.DestinationTableName = configuration.DestinationTableName;
                    bulkCopy.BatchSize = configuration.BatchSize;
                    bulkCopy.BulkCopyTimeout = configuration.BulkCopyTimeout;
                    bulkCopy.NotifyAfter = configuration.NotifyAfter;

                    // Configurar mapeos de columnas
                    if (configuration.ColumnMappings?.Any() == true)
                    {
                        foreach (ColumnMapping mapping in configuration.ColumnMappings)
                        {
                            bulkCopy.ColumnMappings.Add(mapping.SourceColumn, mapping.DestinationColumn);
                        }
                    }
                    else
                    {
                        // Mapeo automático por nombre de columna
                        foreach (DataColumn column in source.Columns)
                        {
                            bulkCopy.ColumnMappings.Add(column.ColumnName, column.ColumnName);
                        }
                    }

                    await bulkCopy.WriteToServerAsync(source, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        /// <summary>
        /// Copia masivamente una colección de entidades con configuración avanzada.
        /// </summary>
        public async Task ExecuteBulkInsertAsync<T>(IEnumerable<T> entities, BulkOperationsConfiguration configuration, CancellationToken cancellationToken = default) where T : class
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            using (DataTable dataTable = entities.ToDataTable())
            {
                await ExecuteBulkInsertAsync(dataTable, configuration, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Copia masivamente una colección de entidades con configuración fluida.
        /// </summary>
        public async Task ExecuteBulkInsertAsync<T>(IEnumerable<T> entities, Action<BulkOperationsConfigurationBuilder> configure, CancellationToken cancellationToken = default) where T : class
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            BulkOperationsConfigurationBuilder builder = new BulkOperationsConfigurationBuilder();
            configure(builder);
            BulkOperationsConfiguration configuration = builder.Build();

            using (DataTable dataTable = entities.ToDataTable())
            {
                await ExecuteBulkInsertAsync(dataTable, configuration, cancellationToken).ConfigureAwait(false);
            }
        }

        private string CreateTableScriptSQL(DataTable source, string targetTableName)
        {
            StringBuilder sb = new StringBuilder($"IF OBJECT_ID('{targetTableName}') IS NULL CREATE TABLE {targetTableName} (", 1024);

            foreach (DataColumn column in source.Columns)
            {
                string sqlType = GetSqlType(column.DataType);
                sb.Append($"\n    [{column.ColumnName}] {sqlType},");
            }

            // Remove trailing comma and close the statement
            if (source.Columns.Count > 0)
            {
                sb.Length--; // Remove last comma
            }

            sb.Append("\n)");
            return sb.ToString();
        }

        private string GetSqlType(Type type)
        {
            if (type == typeof(int))
            {
                return "INT";
            }

            if (type == typeof(string))
            {
                return "NVARCHAR(MAX)";
            }

            if (type == typeof(DateTime))
            {
                return "DATETIME";
            }

            if (type == typeof(decimal))
            {
                return "DECIMAL(18,2)";
            }

            if (type == typeof(bool))
            {
                return "BIT";
            }

            if (type == typeof(Guid))
            {
                return "UNIQUEIDENTIFIER";
            }

            if (type == typeof(byte[]))
            {
                return "VARBINARY(MAX)";
            }

            if (type == typeof(double))
            {
                return "FLOAT";
            }

            if (type == typeof(float))
            {
                return "REAL";
            }

            return "NVARCHAR(MAX)";
        }
    }
}
