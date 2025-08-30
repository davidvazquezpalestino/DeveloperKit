namespace DevKit.JWT.Interfaces;

/// <summary>
/// Interfaz para la generación y gestión de tokens JWT.
/// </summary>
public interface IAccessToken
{
    /// <summary>
    /// Genera un token JWT con las configuraciones especificadas.
    /// </summary>
    Task<string> GetTokenAsync(Action<Dictionary<string, string>> configurations);

    /// <summary>
    /// Genera un conjunto completo de tokens (access + refresh).
    /// </summary>
    Task<TokenResponse> GenerateTokenPairAsync(Action<Dictionary<string, string>> configurations, string ipAddress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Renueva un access token usando un refresh token válido.
    /// </summary>
    Task<TokenResponse> RefreshTokenAsync(string refreshToken, string ipAddress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida un token JWT y extrae sus claims.
    /// </summary>
    Task<ClaimsPrincipal> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoca un refresh token específico.
    /// </summary>
    Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoca todos los tokens de un usuario.
    /// </summary>
    Task<int> RevokeAllUserTokensAsync(string userId, string ipAddress = null, CancellationToken cancellationToken = default);
}