namespace DevKit.ExecutionEngine.MySQL.Extensions;

/// <summary>
/// Extension methods for working with MySql parameters
/// </summary>
public static class MySqlParameterExtensions
{
    private static readonly Dictionary<Type, MySqlDbType> TypeMappings = new()
    {
        // String types
        [typeof(string)] = MySqlDbType.VarString,
        [typeof(char)] = MySqlDbType.String,

        // Boolean
        [typeof(bool)] = MySqlDbType.Bool,

        // Integer types
        [typeof(byte)] = MySqlDbType.Byte,
        [typeof(sbyte)] = MySqlDbType.Byte,
        [typeof(short)] = MySqlDbType.Int16,
        [typeof(ushort)] = MySqlDbType.UInt16,
        [typeof(int)] = MySqlDbType.Int32,
        [typeof(uint)] = MySqlDbType.UInt32,
        [typeof(long)] = MySqlDbType.Int64,
        [typeof(ulong)] = MySqlDbType.UInt64,

        // Floating point types
        [typeof(float)] = MySqlDbType.Float,
        [typeof(double)] = MySqlDbType.Double,
        [typeof(decimal)] = MySqlDbType.Decimal,

        // Date and time types
        [typeof(DateTime)] = MySqlDbType.DateTime,
        [typeof(DateTimeOffset)] = MySqlDbType.Timestamp,
        [typeof(TimeSpan)] = MySqlDbType.Time,

        // Other types
        [typeof(Guid)] = MySqlDbType.Guid,
        [typeof(byte[])] = MySqlDbType.LongBlob,
        [typeof(Stream)] = MySqlDbType.LongBlob
    };

    /// <summary>
    /// Creates a new MySqlParameter with the specified characteristics.
    /// </summary>
    public static MySqlParameter CreateMySqlParameter(
        string parameterName,
        object value,
        MySqlDbType? mySqlDbType = null,
        int? size = null,
        byte? precision = null,
        byte? scale = null,
        ParameterDirection direction = ParameterDirection.Input,
        Action<string> log = null)
    {
        try
        {
            string normalizedName = NormalizeMySqlParamName(parameterName);
            log?.Invoke($"Creating parameter: {normalizedName} with value: {value}");

            // Create parameter with value
            MySqlParameter parameter = new MySqlParameter(normalizedName, value ?? DBNull.Value);

            // Set type if specified
            if (mySqlDbType.HasValue)
            {
                parameter.MySqlDbType = mySqlDbType.Value;
            }

            // Set size if specified
            if (size.HasValue)
            {
                parameter.Size = size.Value;
            }

            // Set precision and scale for decimal types
            if (precision.HasValue)
            {
                parameter.Precision = precision.Value;
                if (scale.HasValue)
                {
                    parameter.Scale = scale.Value;
                }
            }

            parameter.Direction = direction;
            return parameter;
        }
        catch (Exception ex)
        {
            log?.Invoke($"Error creating parameter {parameterName}: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Adds a MySqlParameter to the specified parameter collection.
    /// </summary>
    extension(IDataParameterCollection parameterCollection)
    {
        /// <summary>
        /// Adds a MySqlParameter to the specified parameter collection.
        /// </summary>
        public IDataParameterCollection AddMySqlParameter(string parameterName,
            object value,
            MySqlDbType? mySqlDbType = null,
            int? size = null,
            byte? precision = null,
            byte? scale = null,
            ParameterDirection direction = ParameterDirection.Input,
            Action<string> log = null)
        {
            try
            {
                MySqlParameter parameter = CreateMySqlParameter(
                    parameterName, value, mySqlDbType, size, precision, scale, direction, log);

                parameterCollection.Add(parameter);
                return parameterCollection;
            }
            catch (Exception ex)
            {
                log?.Invoke($"Error adding parameter {parameterName}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Converts an object's properties to a collection of MySqlParameters.
        /// </summary>
        public IDataParameterCollection AsMySqlParameters<T>(T item,
            Action<string> log = null)
        {
            if (item == null)
            {
                return parameterCollection;
            }

            try
            {
                PropertyInfo[] properties = typeof(T).GetProperties();
                foreach (PropertyInfo property in properties)
                {
                    try
                    {
                        // Skip indexers and other non-serializable properties
                        if (property.GetIndexParameters().Length > 0)
                        {
                            continue;
                        }

                        string paramName = $"@{property.Name}";
                        object value = property.GetValue(item);

                        // Get the appropriate MySqlDbType for the property
                        MySqlDbType dbType = GetMySqlDbType(property.PropertyType);

                        log?.Invoke($"Creating parameter: {paramName} with type {dbType}");

                        parameterCollection.AddMySqlParameter(
                            paramName,
                            value,
                            dbType,
                            log: log);
                    }
                    catch (Exception ex)
                    {
                        log?.Invoke($"Error processing property {property.Name}: {ex.Message}");
                        // Continue with next property
                    }
                }

                return parameterCollection;
            }
            catch (Exception ex)
            {
                log?.Invoke($"Error converting object to parameters: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Converts a dictionary to a collection of MySqlParameters.
        /// </summary>
        public IDataParameterCollection AsMySqlParameters(Dictionary<string, object> parameters,
            Action<string> log = null)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return parameterCollection;
            }

            try
            {
                foreach (KeyValuePair<string, object> param in parameters)
                {
                    try
                    {
                        string paramName = NormalizeMySqlParamName(param.Key);
                        log?.Invoke($"Adding parameter: {paramName}");

                        parameterCollection.AddMySqlParameter(
                            paramName,
                            param.Value,
                            log: log);
                    }
                    catch (Exception ex)
                    {
                        log?.Invoke($"Error adding parameter {param.Key}: {ex.Message}");
                        // Continue with next parameter
                    }
                }

                return parameterCollection;
            }
            catch (Exception ex)
            {
                log?.Invoke($"Error converting dictionary to parameters: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// Normalizes parameter names by ensuring they start with '@'.
    /// </summary>
    private static string NormalizeMySqlParamName(string parameterName)
    {
        if (string.IsNullOrEmpty(parameterName))
        {
            throw new ArgumentException("Parameter name cannot be null or empty", nameof(parameterName));
        }

        return parameterName.StartsWith("@") ? parameterName : $"@{parameterName}";
    }

    /// <summary>
    /// Maps a .NET type to the corresponding MySqlDbType.
    /// </summary>
    /// <param name="type">The .NET type to map</param>
    /// <returns>The corresponding MySqlDbType, or VarString if no direct mapping is found</returns>
    /// <exception cref="ArgumentNullException">Thrown when type is null</exception>
    /// <remarks>
    /// This method handles both nullable and non-nullable types, including enums.
    /// For unmapped types, it defaults to MySqlDbType.VarString.
    /// </remarks>
    private static MySqlDbType GetMySqlDbType(Type type)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        // Handle nullable types
        Type underlyingType = Nullable.GetUnderlyingType(type) ?? type;

        // Handle enums as their underlying type (usually int)
        if (underlyingType.IsEnum)
        {
            return MySqlDbType.Int32;
        }

        // Look up the type in our mapping dictionary
        return TypeMappings.GetValueOrDefault(underlyingType, MySqlDbType.VarString);

        // Default to string for unmapped types
    }
}