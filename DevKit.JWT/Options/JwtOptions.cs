namespace DevKit.JWT.Options;

/// <summary>
/// Opciones de configuración para JWT.
/// </summary>
public class JwtOptions
{
    /// <summary>
    /// Nombre de la sección de configuración en appsettings.
    /// </summary>
    public const string SectionKey = nameof(JwtOptions);

    /// <summary>
    /// Clave secreta para firmar el token.
    /// </summary>
    public string SecurityKey { get; set; }

    /// <summary>
    /// Emisor válido del token.
    /// </summary>
    public string ValidIssuer { get; set; }

    /// <summary>
    /// Receptor válido del token.
    /// </summary>
    public string ValidAudience { get; set; }

    /// <summary>
    /// Tiempo de expiración del token en minutos.
    /// </summary>
    public int ExpireInMinutes { get; set; }
}