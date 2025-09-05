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

    /// <summary>
    /// Tiempo de expiración del refresh token en días.
    /// </summary>
    public int RefreshTokenExpireInDays { get; set; } = 7;

    /// <summary>
    /// Longitud del refresh token generado.
    /// </summary>
    public int RefreshTokenLength { get; set; } = 64;

    /// <summary>
    /// Habilitar rotación de refresh tokens (más seguro).
    /// </summary>
    public bool EnableRefreshTokenRotation { get; set; } = true;

    /// <summary>
    /// Tiempo de gracia para refresh tokens rotados en minutos.
    /// </summary>
    public int RefreshTokenGraceTimeMinutes { get; set; } = 5;

    /// <summary>
    /// Tolerancia de tiempo para validación de tokens (ClockSkew) en minutos.
    /// </summary>
    public int ClockSkewMinutes { get; set; } = 5;

    /// <summary>
    /// Requiere HTTPS para metadatos (true en producción, false en desarrollo).
    /// </summary>
    public bool RequireHttpsMetadata { get; set; } = false;
}