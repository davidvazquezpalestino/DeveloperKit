namespace DevKit.JWT.Entities;

/// <summary>
/// Entidad que representa un refresh token.
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// Token único generado.
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// Identificador del usuario asociado.
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Fecha de creación del token.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Fecha de expiración del token.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Indica si el token ha sido revocado.
    /// </summary>
    public bool IsRevoked { get; set; }

    /// <summary>
    /// Fecha de revocación del token.
    /// </summary>
    public DateTime? RevokedAt { get; set; }

    /// <summary>
    /// Token que reemplazó a este (en caso de rotación).
    /// </summary>
    public string ReplacedByToken { get; set; }

    /// <summary>
    /// Dirección IP desde donde se creó el token.
    /// </summary>
    public string CreatedByIp { get; set; }

    /// <summary>
    /// Dirección IP desde donde se revocó el token.
    /// </summary>
    public string RevokedByIp { get; set; }

    /// <summary>
    /// Razón de la revocación del token.
    /// </summary>
    public string RevocationReason { get; set; }

    /// <summary>
    /// Indica si el token está activo (no expirado ni revocado).
    /// </summary>
    public bool IsActive => !IsRevoked && DateTime.UtcNow < ExpiresAt;

    /// <summary>
    /// Indica si el token ha expirado.
    /// </summary>
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
}
