namespace DevKit.ExecutionEngine.SQLServer.Validation;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Validador para SqlOptions usando validadores de .NET 10.
/// </summary>
public class SqlOptionsValidator : IValidateOptions<SqlOptions>
{
    /// <summary>
    /// Valida las opciones de SQL Server.
    /// </summary>
    /// <param name="name">Nombre de las opciones.</param>
    /// <param name="options">Opciones a validar.</param>
    /// <returns>Resultado de la validación.</returns>
    public ValidateOptionsResult Validate(string name, SqlOptions options)
    {
        var failures = new List<string>();

        // Validación básica
        if (options == null)
        {
            failures.Add("SqlOptions no puede ser nulo.");
            return ValidateOptionsResult.Fail(failures);
        }

        // Validar cadena de conexión o SqlAuth
        if (string.IsNullOrWhiteSpace(options.ConnectionString) && !options.SqlAuth.IsConfigured())
        {
            failures.Add("Debe especificar ConnectionString o configurar SqlAuth con servidor, base de datos, usuario y contraseña.");
        }

        // Validar timeouts
        if (options.CommandTimeout <= 0)
        {
            failures.Add("CommandTimeout debe ser mayor que 0.");
        }

        if (options.ConnectionTimeout <= 0)
        {
            failures.Add("ConnectionTimeout debe ser mayor que 0.");
        }

        if (options.CommandTimeout > 3600) // Máximo 1 hora
        {
            failures.Add("CommandTimeout no debe exceder 3600 segundos (1 hora).");
        }

        if (options.ConnectionTimeout > 300) // Máximo 5 minutos
        {
            failures.Add("ConnectionTimeout no debe exceder 300 segundos (5 minutos).");
        }

        // Validar configuración de pooling
        if (options.ConnectionPooling != null)
        {
            if (options.ConnectionPooling.MinPoolSize < 0)
            {
                failures.Add("MinPoolSize no puede ser negativo.");
            }

            if (options.ConnectionPooling.MaxPoolSize <= 0)
            {
                failures.Add("MaxPoolSize debe ser mayor que 0.");
            }

            if (options.ConnectionPooling.MinPoolSize > options.ConnectionPooling.MaxPoolSize)
            {
                failures.Add("MinPoolSize no puede ser mayor que MaxPoolSize.");
            }

            if (options.ConnectionPooling.MaxPoolSize > 1000)
            {
                failures.Add("MaxPoolSize no debe exceder 1000 para evitar agotamiento de recursos.");
            }
        }

        // Validar configuración de BulkCopy
        if (options.BulkCopy != null)
        {
            if (options.BulkCopy.BatchSize < 0)
            {
                failures.Add("BulkCopy.BatchSize no puede ser negativo.");
            }

            if (options.BulkCopy.BulkCopyTimeout <= 0)
            {
                failures.Add("BulkCopy.BulkCopyTimeout debe ser mayor que 0.");
            }

            if (options.BulkCopy.BulkCopyTimeout > 3600) // Máximo 1 hora
            {
                failures.Add("BulkCopy.BulkCopyTimeout no debe exceder 3600 segundos (1 hora).");
            }

            if (options.BulkCopy.NotifyAfter < 0)
            {
                failures.Add("BulkCopy.NotifyAfter no puede ser negativo.");
            }
        }

        // Validar SqlAuth si está configurado
        if (options.SqlAuth.IsConfigured())
        {
            if (options.SqlAuth.Server?.Length > 128)
            {
                failures.Add("SqlAuth.Server no debe exceder 128 caracteres.");
            }

            if (options.SqlAuth.Database?.Length > 128)
            {
                failures.Add("SqlAuth.Database no debe exceder 128 caracteres.");
            }

            if (options.SqlAuth.UserId?.Length > 128)
            {
                failures.Add("SqlAuth.UserId no debe exceder 128 caracteres.");
            }

            if (string.IsNullOrWhiteSpace(options.SqlAuth.Password))
            {
                failures.Add("SqlAuth.Password no puede estar vacío cuando SqlAuth está configurado.");
            }
        }

        // Validar que la cadena de conexión sea válida si se proporciona
        if (!string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            try
            {
                var builder = new SqlConnectionStringBuilder(options.ConnectionString);

                // Validar componentes básicos de la cadena de conexión
                if (string.IsNullOrWhiteSpace(builder.InitialCatalog) && string.IsNullOrWhiteSpace(builder.AttachDBFilename))
                {
                    failures.Add("La cadena de conexión debe especificar una base de datos (InitialCatalog o AttachDBFilename).");
                }

                if (string.IsNullOrWhiteSpace(builder.DataSource))
                {
                    failures.Add("La cadena de conexión debe especificar un servidor (DataSource).");
                }

                // Validar timeouts en la cadena de conexión
                if (builder.ConnectTimeout <= 0)
                {
                    failures.Add("Connect Timeout en la cadena de conexión debe ser mayor que 0.");
                }
            }
            catch (ArgumentException ex)
            {
                failures.Add($"Cadena de conexión inválida: {ex.Message}");
            }
            catch (Exception ex)
            {
                failures.Add($"Error al validar cadena de conexión: {ex.Message}");
            }
        }

        return failures.Count > 0 ? ValidateOptionsResult.Fail(failures) : ValidateOptionsResult.Success;
    }
}

/// <summary>
/// Atributo de validación personalizado para SqlOptions.
/// </summary>
public class SqlOptionsValidationAttribute : ValidationAttribute
{
    /// <inheritdoc/>
    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        if (value is SqlOptions options)
        {
            var validator = new SqlOptionsValidator();
            ValidateOptionsResult result = validator.Validate(validationContext.DisplayName, options);

            if (!result.Succeeded)
            {
                return new ValidationResult(string.Join(", ", result.Failures));
            }
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Extensiones para registrar validación de SqlOptions en DI container.
/// </summary>
public static class SqlOptionsValidationExtensions
{
    /// <summary>
    /// Registra la validación de SqlOptions en el contenedor de DI.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <returns>Colección de servicios con validación registrada.</returns>
    public static IServiceCollection AddSqlOptionsValidation(this IServiceCollection services)
    {
        services.AddSingleton<IValidateOptions<SqlOptions>, SqlOptionsValidator>();
        return services;
    }

    /// <summary>
    /// Configura y valida SqlOptions en el contenedor de DI.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configureAction">Acción de configuración.</param>
    /// <returns>Colección de servicios con SqlOptions configurado y validado.</returns>
    public static IServiceCollection ConfigureAndValidateSqlOptions(
        this IServiceCollection services,
        Action<SqlOptions> configureAction)
    {
        services.Configure(configureAction);
        services.AddSqlOptionsValidation();
        return services;
    }
}
