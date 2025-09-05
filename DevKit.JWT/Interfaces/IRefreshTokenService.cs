namespace DevKit.JWT.Interfaces;

/// <summary>
/// Interfaz para el servicio de gestión de refresh tokens.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Genera un nuevo refresh token para el usuario especificado.
    /// </summary>
    Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string ipAddress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida un refresh token y devuelve la información asociada.
    /// </summary>
    Task<RefreshToken> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoca un refresh token específico.
    /// </summary>
    Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress = null, string reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revoca todos los refresh tokens de un usuario.
    /// </summary>
    Task<int> RevokeAllUserRefreshTokensAsync(string userId, string ipAddress = null, string reason = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Rota un refresh token (revoca el actual y genera uno nuevo).
    /// </summary>
    Task<RefreshToken> RotateRefreshTokenAsync(RefreshToken oldToken, string ipAddress = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Limpia refresh tokens expirados.
    /// </summary>
    Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);
}
