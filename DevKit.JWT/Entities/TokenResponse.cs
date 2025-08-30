namespace DevKit.JWT.Entities;

/// <summary>
/// Respuesta que contiene el access token y refresh token.
/// </summary>
public class TokenResponse
{
    /// <summary>
    /// Access token JWT.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token para renovar el access token.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de token (siempre "Bearer").
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Tiempo de expiración del access token en segundos.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Fecha de expiración del access token.
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// Scopes o permisos asociados al token.
    /// </summary>
    public string[] Scopes { get; set; }
}
