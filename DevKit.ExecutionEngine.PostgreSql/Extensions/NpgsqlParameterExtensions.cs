namespace DevKit.ExecutionEngine.PostgreSQL.Extensions
{
    /// <summary>
    /// Provides extension methods for working with NpgsqlParameter objects.
    /// </summary>
    public static class NpgsqlParameterExtensions
    {
        /// <summary>
        /// Creates a new NpgsqlParameter with the specified characteristics.
        /// </summary>
        public static NpgsqlParameter CreatePosgreParameter(
            string parameterName,
            object value,
            NpgsqlDbType? npgsqlDbType = null,
            int? size = null,
            byte? precision = null,
            byte? scale = null,
            ParameterDirection? direction = null,
            Action<string> log = null)
        {
            try
            {
                log?.Invoke($"Creating NpgSQL parameter: {parameterName}");

                if (string.IsNullOrWhiteSpace(parameterName))
                {
                    throw new ArgumentException("Parameter name cannot be empty", nameof(parameterName));
                }

                // Normalize parameter name
                string normalizedName = NormalizeNpgsqlParamName(parameterName);

                // Create parameter with value
                NpgsqlParameter parameter = new NpgsqlParameter(normalizedName, value ?? DBNull.Value);

                // Set type if specified
                if (npgsqlDbType.HasValue)
                {
                    parameter.NpgsqlDbType = npgsqlDbType.Value;
                    log?.Invoke($"Type for {normalizedName}: {npgsqlDbType.Value}");
                }
                else if (value != null && value != DBNull.Value)
                {
                    // Infer type from value if not specified
                    parameter.NpgsqlDbType = GetNpgsqlDbType(value.GetType());
                    log?.Invoke($"Inferred type for {normalizedName}: {parameter.NpgsqlDbType}");
                }

                // Set size if specified
                if (size.HasValue)
                {
                    parameter.Size = size.Value;
                    log?.Invoke($"Size for {normalizedName}: {size.Value}");
                }

                // Set precision if specified
                if (precision.HasValue)
                {
                    parameter.Precision = precision.Value;
                    log?.Invoke($"Precision for {normalizedName}: {precision.Value}");
                }

                // Set scale if specified
                if (scale.HasValue)
                {
                    parameter.Scale = scale.Value;
                    log?.Invoke($"Scale for {normalizedName}: {scale.Value}");
                }

                // Set direction if specified
                if (direction.HasValue)
                {
                    parameter.Direction = direction.Value;
                    log?.Invoke($"Direction for {normalizedName}: {direction.Value}");
                }

                log?.Invoke($"Parameter {normalizedName} created successfully");
                return parameter;
            }
            catch (Exception ex)
            {
                log?.Invoke($"ERROR creating parameter {parameterName}: {ex.Message}");
                throw;
            }
        }

        extension(IDataParameterCollection parameterCollection)
        {
            /// <summary>
            /// Adds an NpgsqlParameter to the specified parameter collection.
            /// </summary>
            public IDataParameterCollection AddPosgreParameter(string parameterName,
                object value,
                NpgsqlDbType? npgsqlDbType = null,
                int? size = null,
                byte? precision = null,
                byte? scale = null,
                ParameterDirection? direction = null,
                Action<string> log = null)
            {
                try
                {
                    log?.Invoke($"Adding parameter to collection: {parameterName}");

                    if (parameterCollection == null)
                    {
                        throw new ArgumentNullException(nameof(parameterCollection));
                    }

                    NpgsqlParameter parameter = CreatePosgreParameter(
                        parameterName, value, npgsqlDbType, size, precision, scale, direction, log);

                    parameterCollection.Add(parameter);

                    log?.Invoke($"Parameter {parameterName} added to collection successfully");
                    return parameterCollection;
                }
                catch (Exception ex)
                {
                    log?.Invoke($"ERROR adding parameter {parameterName} to collection: {ex.Message}");
                    throw;
                }
            }

            /// <summary>
            /// Converts an object's properties to a collection of NpgsqlParameters.
            /// </summary>
            public IDataParameterCollection AsPosgreParameters<T>(T item,
                Action<string> log = null)
            {
                try
                {
                    log?.Invoke("Starting object to NpgsqlParameters conversion");

                    if (parameterCollection == null)
                    {
                        throw new ArgumentNullException(nameof(parameterCollection));
                    }

                    if (item == null)
                    {
                        throw new ArgumentNullException(nameof(item), "Object cannot be null");
                    }

                    Type type = item.GetType();
                    PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

                    log?.Invoke($"Processing {properties.Length} properties");

                    foreach (PropertyInfo property in properties)
                    {
                        try
                        {
                            string propertyName = property.Name;
                            log?.Invoke($"Processing property: {propertyName}");

                            object value = property.GetValue(item);
                            NpgsqlDbType dbType = GetNpgsqlDbType(property.PropertyType);
                            string paramName = NormalizeNpgsqlParamName(propertyName);

                            log?.Invoke($"Creating parameter: {paramName} with type {dbType}");

                            parameterCollection.AddPosgreParameter(
                                paramName,
                                value,
                                dbType,
                                log: log);
                        }
                        catch (Exception ex)
                        {
                            string errorMsg = $"Error processing property '{property.Name}': {ex.Message}";
                            log?.Invoke($"ERROR: {errorMsg}");
                            if (log == null)
                            {
                                Console.WriteLine(errorMsg);
                            }
                        }
                    }

                    log?.Invoke("Object to NpgsqlParameters conversion completed successfully");
                    return parameterCollection;
                }
                catch (Exception ex)
                {
                    log?.Invoke($"ERROR in AsNpgsqlParameters: {ex.Message}");
                    throw;
                }
            }

            /// <summary>
            /// Converts a dictionary to a collection of NpgsqlParameters.
            /// </summary>
            public IDataParameterCollection AsPosgreParameters(Dictionary<string, object> parameters,
                Action<string> log = null)
            {
                try
                {
                    log?.Invoke("Starting dictionary to NpgsqlParameters conversion");

                    if (parameterCollection == null)
                    {
                        throw new ArgumentNullException(nameof(parameterCollection));
                    }

                    if (parameters == null)
                    {
                        throw new ArgumentNullException(nameof(parameters));
                    }

                    log?.Invoke($"Processing {parameters.Count} parameters");

                    foreach (KeyValuePair<string, object> param in parameters)
                    {
                        try
                        {
                            string paramName = NormalizeNpgsqlParamName(param.Key);
                            log?.Invoke($"Adding parameter: {paramName}");

                            parameterCollection.AddPosgreParameter(
                                paramName,
                                param.Value,
                                log: log);
                        }
                        catch (Exception ex)
                        {
                            string errorMsg = $"Error processing parameter '{param.Key}': {ex.Message}";
                            log?.Invoke($"ERROR: {errorMsg}");
                            if (log == null)
                            {
                                Console.Error.WriteLine(errorMsg);
                            }
                        }
                    }

                    log?.Invoke("Dictionary to NpgsqlParameters conversion completed successfully");
                    return parameterCollection;
                }
                catch (Exception ex)
                {
                    log?.Invoke($"ERROR in AsNpgsqlParameters: {ex.Message}");
                    throw;
                }
            }

            /// <summary>
            /// Adds a range of IDataParameter objects to an existing IDataParameterCollection.
            /// </summary>
            public IDataParameterCollection AddRange(params IEnumerable<IDataParameter>[] parameters)
            {
                foreach (IEnumerable<IDataParameter> paramList in parameters)
                {
                    foreach (IDataParameter parameter in paramList)
                    {
                        parameterCollection.Add(parameter);
                    }
                }
                return parameterCollection;
            }
        }

        /// <summary>
        /// Gets the NpgsqlDbType corresponding to the specified .NET type.
        /// </summary>
        private static NpgsqlDbType GetNpgsqlDbType(Type type)
        {
            if (type == null)
            {
                return NpgsqlDbType.Unknown;
            }

            type = Nullable.GetUnderlyingType(type) ?? type;

            if (type == typeof(string))
            {
                return NpgsqlDbType.Text;
            }

            if (type == typeof(int) || type == typeof(int?))
            {
                return NpgsqlDbType.Integer;
            }

            if (type == typeof(long) || type == typeof(long?))
            {
                return NpgsqlDbType.Bigint;
            }

            if (type == typeof(short) || type == typeof(short?))
            {
                return NpgsqlDbType.Smallint;
            }

            if (type == typeof(byte) || type == typeof(byte?))
            {
                return NpgsqlDbType.Smallint;
            }

            if (type == typeof(bool) || type == typeof(bool?))
            {
                return NpgsqlDbType.Boolean;
            }

            if (type == typeof(DateTime) || type == typeof(DateTime?))
            {
                return NpgsqlDbType.TimestampTz;
            }

            if (type == typeof(DateTimeOffset) || type == typeof(DateTimeOffset?))
            {
                return NpgsqlDbType.TimestampTz;
            }

            if (type == typeof(decimal) || type == typeof(decimal?))
            {
                return NpgsqlDbType.Numeric;
            }

            if (type == typeof(double) || type == typeof(double?))
            {
                return NpgsqlDbType.Double;
            }

            if (type == typeof(float) || type == typeof(float?))
            {
                return NpgsqlDbType.Real;
            }

            if (type == typeof(Guid) || type == typeof(Guid?))
            {
                return NpgsqlDbType.Uuid;
            }

            if (type == typeof(byte[]))
            {
                return NpgsqlDbType.Bytea;
            }

            if (type == typeof(char) || type == typeof(char?))
            {
                return NpgsqlDbType.Char;
            }

            if (type == typeof(TimeSpan) || type == typeof(TimeSpan?))
            {
                return NpgsqlDbType.Interval;
            }

            if (type.IsEnum)
            {
                return NpgsqlDbType.Text; // Store enums as text by default
            }

            return NpgsqlDbType.Unknown;
        }

        /// <summary>
        /// Normalizes a parameter name by adding the ':' prefix if it doesn't exist.
        /// </summary>
        private static string NormalizeNpgsqlParamName(string paramName)
        {
            if (string.IsNullOrEmpty(paramName))
            {
                return paramName;
            }

            // PostgreSQL uses :paramName format for parameters
            return paramName.StartsWith(":") ? paramName : ":" + paramName;
        }
    }
}
