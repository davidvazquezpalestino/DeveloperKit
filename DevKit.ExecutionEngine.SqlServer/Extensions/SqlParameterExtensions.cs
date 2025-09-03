namespace DevKit.ExecutionEngine.SQLServer.Extensions;

/// <summary>Proporciona métodos de extensión para trabajar con parámetros SQL.</summary>
public static class SqlParameterExtensions
{
    /// <summary>Crea un nuevo parámetro SQL con las características especificadas.</summary>
    public static SqlParameter CreateSqlParameter(string parameterName, object value, SqlDbType? sqlDbType = null, int? size = null, byte? precision = null, byte? scale = null, ParameterDirection? direction = null, Action<string> log = null)
    {
        try
        {
            log?.Invoke($"Creando parámetro SQL: {parameterName}");

            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException("El nombre del parámetro no puede estar vacío", nameof(parameterName));
            }

            object parameterValue = value ?? DBNull.Value;
            log?.Invoke($"Valor del parámetro {parameterName}: {parameterValue} (Tipo: {value?.GetType().Name ?? "null"})");

            SqlParameter parameter = new()
            {
                ParameterName = parameterName,
                Value = parameterValue
            };

            if (sqlDbType.HasValue)
            {
                parameter.SqlDbType = sqlDbType.Value;
                log?.Invoke($"Tipo de dato SQL para {parameterName}: {sqlDbType.Value}");
            }

            if (size.HasValue)
            {
                parameter.Size = size.Value;
                log?.Invoke($"Tamaño para {parameterName}: {size.Value}");
            }

            if (precision.HasValue)
            {
                parameter.Precision = precision.Value;
                log?.Invoke($"Precisión para {parameterName}: {precision.Value}");
            }

            if (scale.HasValue)
            {
                parameter.Scale = scale.Value;
                log?.Invoke($"Escala para {parameterName}: {scale.Value}");
            }

            if (direction.HasValue)
            {
                parameter.Direction = direction.Value;
                log?.Invoke($"Dirección para {parameterName}: {direction.Value}");
            }

            log?.Invoke($"Parámetro {parameterName} creado exitosamente");
            return parameter;
        }
        catch (Exception ex)
        {
            log?.Invoke($"ERROR al crear el parámetro {parameterName}: {ex.Message}");
            throw; // Relanzar la excepción para manejo posterior
        }
    }
    /// <summary>Agrega un parámetro SQL a la colección especificada.</summary>
    public static IDataParameterCollection AddSqlParameter(this IDataParameterCollection parameterCollection, string parameterName, object value,
        SqlDbType? sqlDbType = null, int? size = null, byte? precision = null, byte? scale = null,
        ParameterDirection? direction = null, Action<string> log = null)
    {
        try
        {
            log?.Invoke($"Agregando parámetro a la colección: {parameterName}");

            if (parameterCollection == null)
            {
                throw new ArgumentNullException(nameof(parameterCollection));
            }

            SqlParameter parameter = CreateSqlParameter(parameterName, value, sqlDbType, size, precision, scale, direction, log);
            parameterCollection.Add(parameter);

            log?.Invoke($"Parámetro {parameterName} agregado exitosamente a la colección");
            return parameterCollection;
        }
        catch (Exception ex)
        {
            log?.Invoke($"ERROR al agregar el parámetro {parameterName} a la colección: {ex.Message}");
            throw;
        }
    }
    /// <summary>Convierte las propiedades de un objeto en una colección de parámetros SQL.</summary>
    public static IDataParameterCollection AddSqlParameters<T>(this IDataParameterCollection parameterCollection, T item, Action<string> log = null)
    {
        try
        {
            log?.Invoke("Iniciando conversión de objeto a parámetros SQL");

            if (parameterCollection == null)
            {
                throw new ArgumentNullException(nameof(parameterCollection));
            }

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item), "El objeto no puede ser nulo.");
            }

            PropertyInfo[] properties = item.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            log?.Invoke($"Procesando {properties.Length} propiedades del objeto");

            foreach (PropertyInfo property in properties)
            {
                try
                {
                    string propertyName = property.Name;
                    log?.Invoke($"Procesando propiedad: {propertyName}");

                    object value = property.GetValue(item);
                    SqlDbType dbType = GetSqlDbType(property.PropertyType);
                    string paramName = NormalizeSqlParamName(propertyName);

                    log?.Invoke($"Tipo SQL inferido para {propertyName}: {dbType}");

                    parameterCollection.AddSqlParameter(paramName, value ?? DBNull.Value, dbType, log: log);
                }
                catch (Exception ex)
                {
                    string errorMsg = $"Error al procesar la propiedad '{property.Name}': {ex.Message}";
                    log?.Invoke($"ERROR: {errorMsg}");
                    if (log == null) // Solo lanzar Console si no hay logger configurado
                    {
                        Console.WriteLine(errorMsg);
                    }
                }
            }

            log?.Invoke("Conversión de objeto a parámetros SQL completada exitosamente");
            return parameterCollection;
        }
        catch (Exception ex)
        {
            log?.Invoke($"ERROR en AsSqlParameters: {ex.Message}");
            throw;
        }
    }
    /// <summary>Convierte un diccionario en una colección de parámetros SQL.</summary>
    public static IDataParameterCollection AddSqlParameters(this IDataParameterCollection parameterCollection,
        Dictionary<string, object> parameters, Action<string> log = null)
    {
        try
        {
            log?.Invoke("Iniciando conversión de diccionario a parámetros SQL");

            if (parameterCollection == null)
            {
                throw new ArgumentNullException(nameof(parameterCollection));
            }

            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters), "El diccionario no puede ser nulo.");
            }

            List<KeyValuePair<string, object>> validParameters = parameters.Where(pair => string.IsNullOrWhiteSpace(pair.Key) == false).ToList();
            log?.Invoke($"Procesando {validParameters.Count} parámetros del diccionario");

            foreach (KeyValuePair<string, object> kvp in validParameters)
            {
                try
                {
                    log?.Invoke($"Procesando parámetro: {kvp.Key}");

                    Type type = kvp.Value?.GetType() ?? typeof(string);
                    SqlDbType dbType = GetSqlDbType(type);
                    string paramName = NormalizeSqlParamName(kvp.Key);

                    log?.Invoke($"Tipo SQL inferido para {kvp.Key}: {dbType}");

                    parameterCollection.AddSqlParameter(paramName, kvp.Value ?? DBNull.Value, dbType, log: log);
                }
                catch (Exception ex)
                {
                    string errorMsg = $"Error al procesar el parámetro '{kvp.Key}': {ex.Message}";
                    log?.Invoke($"ERROR: {errorMsg}");
                    if (log == null) // Solo lanzar Console si no hay logger configurado
                    {
                        Console.Error.WriteLine(errorMsg);
                    }
                }
            }

            log?.Invoke("Conversión de diccionario a parámetros SQL completada exitosamente");
            return parameterCollection;
        }
        catch (Exception ex)
        {
            log?.Invoke($"ERROR en AsSqlParamsFromDictionary: {ex.Message}");
            throw;
        }
    }

    /// <summary>Agrega una colección de objetos <see cref="IDataParameter"/> a una instancia existente de <see cref="IDataParameterCollection"/>.</summary>
    public static IDataParameterCollection AddRange(this IDataParameterCollection parameterCollection, params IEnumerable<IDataParameter> parameters)
    {
        foreach (IDataParameter parameter in parameters)
        {
            parameterCollection.Add(parameter);
        }
        return parameterCollection;
    }

    /// <summary>Obtiene el tipo de dato SQL correspondiente al tipo .NET especificado.</summary>
    private static SqlDbType GetSqlDbType(Type type)
    {
        if (type == null)
        {
            return SqlDbType.Variant;
        }

        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(string))
        {
            return SqlDbType.NVarChar;
        }

        if (type == typeof(int))
        {
            return SqlDbType.Int;
        }

        if (type == typeof(long))
        {
            return SqlDbType.BigInt;
        }

        if (type == typeof(short))
        {
            return SqlDbType.SmallInt;
        }

        if (type == typeof(byte))
        {
            return SqlDbType.TinyInt;
        }

        if (type == typeof(bool))
        {
            return SqlDbType.Bit;
        }

        if (type == typeof(DateTime))
        {
            return SqlDbType.DateTime;
        }

        if (type == typeof(decimal))
        {
            return SqlDbType.Decimal;
        }

        if (type == typeof(double))
        {
            return SqlDbType.Float;
        }

        if (type == typeof(float))
        {
            return SqlDbType.Real;
        }

        if (type == typeof(Guid))
        {
            return SqlDbType.UniqueIdentifier;
        }

        if (type == typeof(byte[]))
        {
            return SqlDbType.VarBinary;
        }

        if (type == typeof(char))
        {
            return SqlDbType.NChar;
        }

        return SqlDbType.Variant;
    }
    /// <summary>Normaliza el nombre de un parámetro SQL agregando el prefijo '@' si no existe.</summary>
    private static string NormalizeSqlParamName(string propertyName)
    {
        return propertyName.StartsWith("@") ? propertyName : "@" + propertyName;
    }
}
