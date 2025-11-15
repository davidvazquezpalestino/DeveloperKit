namespace DevKit.ExecutionEngine.Oracle.Extensions;

/// <summary>Proporciona métodos de extensión para trabajar con parámetros Oracle.</summary>
public static class OracleParametersExtensions
{
    /// <summary>Crea un nuevo parámetro Oracle con las características especificadas.</summary>
    /// <param name="parameterName">Nombre del parámetro</param>
    /// <param name="value">Valor del parámetro</param>
    /// <param name="oracleDbType">Tipo de dato Oracle (opcional)</param>
    /// <param name="size">Tamaño del parámetro (opcional)</param>
    /// <param name="precision">Precisión (opcional)</param>
    /// <param name="scale">Escala (opcional)</param>
    /// <param name="direction">Dirección del parámetro (Input/Output/ReturnValue) (opcional)</param>
    /// <param name="log">Delegado para registrar mensajes (opcional)</param>
    /// <returns>Parámetro Oracle configurado</returns>
    public static OracleParameter CreateOracleParameter(string parameterName, object value, OracleDbType? oracleDbType = null,
        int? size = null, byte? precision = null, byte? scale = null, ParameterDirection? direction = null,
        Action<string> log = null)
    {
        try
        {
            log?.Invoke($"Creando parámetro Oracle: {parameterName}");

            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException("El nombre del parámetro no puede estar vacío", nameof(parameterName));
            }

            object parameterValue = value ?? DBNull.Value;
            log?.Invoke($"Valor del parámetro {parameterName}: {parameterValue} (Tipo: {value?.GetType().Name ?? "null"})");

            OracleParameter parameter = new OracleParameter
            {
                ParameterName = parameterName,
                Value = parameterValue
            };

            if (oracleDbType.HasValue)
            {
                parameter.OracleDbType = oracleDbType.Value;
                log?.Invoke($"Tipo de dato Oracle para {parameterName}: {oracleDbType.Value}");
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
            throw;
        }
    }

    /// <param name="collection">Colección de parámetros a la que se agregará el nuevo parámetro</param>
    extension(IDataParameterCollection collection)
    {
        /// <summary>Agrega un parámetro Oracle a la colección especificada.</summary>
        /// <param name="parameterName">Nombre del parámetro</param>
        /// <param name="value">Valor del parámetro</param>
        /// <param name="oracleDbType">Tipo de dato Oracle (opcional)</param>
        /// <param name="size">Tamaño del parámetro (opcional)</param>
        /// <param name="precision">Precisión (opcional)</param>
        /// <param name="scale">Escala (opcional)</param>
        /// <param name="direction">Dirección del parámetro (opcional)</param>
        /// <param name="log">Delegado para registrar mensajes (opcional)</param>
        /// <returns>La misma colección de parámetros para permitir el encadenamiento de métodos</returns>
        public IDataParameterCollection AddOracleParameter(string parameterName, object value,
            OracleDbType? oracleDbType = null, int? size = null, byte? precision = null, byte? scale = null,
            ParameterDirection? direction = null, Action<string> log = null)
        {
            try
            {
                log?.Invoke($"Agregando parámetro a la colección: {parameterName}");

                if (collection == null)
                {
                    throw new ArgumentNullException(nameof(collection));
                }

                OracleParameter parameter = CreateOracleParameter(parameterName, value, oracleDbType, size, precision, scale, direction, log);
                collection.Add(parameter);

                log?.Invoke($"Parámetro {parameterName} agregado exitosamente a la colección");
                return collection;
            }
            catch (Exception ex)
            {
                log?.Invoke($"ERROR al agregar el parámetro {parameterName} a la colección: {ex.Message}");
                throw;
            }
        }

        /// <summary>Convierte las propiedades de un objeto en una colección de parámetros Oracle.</summary>
        /// <typeparam name="T">Tipo del objeto a convertir</typeparam>
        /// <param name="item">Objeto cuyas propiedades se convertirán en parámetros</param>
        /// <param name="log">Delegado para registrar mensajes (opcional)</param>
        /// <returns>La misma colección de parámetros para permitir el encadenamiento de métodos</returns>
        public IDataParameterCollection AsOracleParameters<T>(T item, Action<string> log = null)
        {
            try
            {
                log?.Invoke("Iniciando conversión de objeto a parámetros Oracle");

                if (collection == null)
                {
                    throw new ArgumentNullException(nameof(collection));
                }

                if (item == null)
                {
                    log?.Invoke("El objeto no puede ser nulo.");
                    return collection;
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
                        OracleDbType dbType = GetOracleDbType(property.PropertyType);
                        string paramName = NormalizeOracleParamName(propertyName);

                        log?.Invoke($"Tipo Oracle inferido para {propertyName}: {dbType}");

                        collection.AddOracleParameter(paramName, value ?? DBNull.Value, dbType, log: log);
                    }
                    catch (Exception ex)
                    {
                        string errorMsg = $"Error al procesar la propiedad '{property.Name}': {ex.Message}";
                        log?.Invoke($"ERROR: {errorMsg}");
                        if (log == null) // Solo lanzar Console si no hay logger configurado
                        {
                            Console.Error.WriteLine(errorMsg);
                        }
                    }
                }

                log?.Invoke("Conversión de objeto a parámetros Oracle completada exitosamente");
                return collection;
            }
            catch (Exception ex)
            {
                log?.Invoke($"ERROR en AsOracleParameters: {ex.Message}");
                throw;
            }
        }

        /// <summary>Convierte un diccionario en una colección de parámetros Oracle.</summary>
        /// <param name="dictionary">Diccionario con los parámetros a convertir</param>
        /// <param name="log">Delegado para registrar mensajes (opcional)</param>
        /// <returns>La misma colección de parámetros para permitir el encadenamiento de métodos</returns>
        public IDataParameterCollection AsOracleParameters(Dictionary<string, object> dictionary, Action<string> log = null)
        {
            try
            {
                log?.Invoke("Iniciando conversión de diccionario a parámetros Oracle");

                if (collection == null)
                {
                    throw new ArgumentNullException(nameof(collection));
                }

                if (dictionary == null)
                {
                    return collection;
                }

                List<KeyValuePair<string, object>> validParameters = dictionary.Where(pair => string.IsNullOrWhiteSpace(pair.Key) == false).ToList();
                log?.Invoke($"Procesando {validParameters.Count} parámetros del diccionario");

                foreach (KeyValuePair<string, object> kvp in validParameters)
                {
                    try
                    {
                        log?.Invoke($"Procesando parámetro: {kvp.Key}");

                        Type type = kvp.Value?.GetType() ?? typeof(string);
                        OracleDbType dbType = GetOracleDbType(type);
                        string paramName = NormalizeOracleParamName(kvp.Key);

                        log?.Invoke($"Tipo Oracle inferido para {kvp.Key}: {dbType}");

                        collection.AddOracleParameter(paramName, kvp.Value ?? DBNull.Value, dbType, log: log);
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

                log?.Invoke("Conversión de diccionario a parámetros Oracle completada exitosamente");
                return collection;
            }
            catch (Exception ex)
            {
                log?.Invoke($"ERROR en AsOracleParamsFromDict: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>Obtiene el tipo de dato Oracle correspondiente al tipo .NET especificado.</summary>
    /// <param name="type">Tipo .NET a convertir</param>
    /// <returns>Tipo de dato Oracle correspondiente</returns>
    private static OracleDbType GetOracleDbType(Type type)
    {
        if (type == null)
        {
            return OracleDbType.Varchar2;
        }

        type = Nullable.GetUnderlyingType(type) ?? type;

        if (type == typeof(string))
        {
            return OracleDbType.Varchar2;
        }

        if (type == typeof(int))
        {
            return OracleDbType.Int32;
        }

        if (type == typeof(long))
        {
            return OracleDbType.Int64;
        }

        if (type == typeof(short))
        {
            return OracleDbType.Int16;
        }

        if (type == typeof(byte))
        {
            return OracleDbType.Byte;
        }

        if (type == typeof(bool))
        {
            return OracleDbType.Byte; // No Boolean en OracleDbType
        }

        if (type == typeof(DateTime))
        {
            return OracleDbType.Date;
        }

        if (type == typeof(decimal))
        {
            return OracleDbType.Decimal;
        }

        if (type == typeof(double))
        {
            return OracleDbType.Double;
        }

        if (type == typeof(float))
        {
            return OracleDbType.Single;
        }

        if (type == typeof(Guid))
        {
            return OracleDbType.Raw;
        }

        if (type == typeof(byte[]))
        {
            return OracleDbType.Blob;
        }

        if (type == typeof(char))
        {
            return OracleDbType.Char;
        }

        return OracleDbType.Varchar2;
    }
    /// <summary>Normaliza el nombre de un parámetro Oracle.</summary>
    /// <param name="name">Nombre de la propiedad a normalizar</param>
    /// <returns>Nombre del parámetro Oracle normalizado</returns>
    private static string NormalizeOracleParamName(string name)
    {
        return name.StartsWith(":") ? name : ":" + name;
    }
}

