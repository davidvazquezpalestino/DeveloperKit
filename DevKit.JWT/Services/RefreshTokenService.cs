namespace DevKit.JWT.Services;

/// <summary>
/// Implementación del servicio de gestión de refresh tokens.
/// </summary>
public class RefreshTokenService(IOptions<JwtOptions> options) : IRefreshTokenService
{
    private readonly JwtOptions Options = options?.Value;
    private readonly ConcurrentDictionary<string, RefreshToken> TokenStore = new();

    /// <summary>
    /// Genera un nuevo refresh token para el usuario especificado.
    /// </summary>
    public Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string ipAddress = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("UserId no puede estar vacío", nameof(userId));
        }

        RefreshToken refreshToken = new RefreshToken
        {
            Token = GenerateSecureToken(),
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(Options.RefreshTokenExpireInDays),
            CreatedByIp = ipAddress
        };

        TokenStore.TryAdd(refreshToken.Token, refreshToken);

        return Task.FromResult(refreshToken);
    }

    /// <summary>
    /// Valida un refresh token y devuelve la información asociada.
    /// </summary>
    public Task<RefreshToken> ValidateRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult<RefreshToken>(null);
        }

        if (!TokenStore.TryGetValue(token, out RefreshToken refreshToken))
        {
            return Task.FromResult<RefreshToken>(null);
        }

        if (!refreshToken.IsActive)
        {
            return Task.FromResult<RefreshToken>(null);
        }

        return Task.FromResult(refreshToken);
    }

    /// <summary>
    /// Revoca un refresh token específico.
    /// </summary>
    public Task<bool> RevokeRefreshTokenAsync(string token, string ipAddress = null, string reason = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return Task.FromResult(false);
        }

        if (!TokenStore.TryGetValue(token, out RefreshToken refreshToken))
        {
            return Task.FromResult(false);
        }

        refreshToken.IsRevoked = true;
        refreshToken.RevokedAt = DateTime.UtcNow;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.RevocationReason = reason ?? "Manual revocation";

        return Task.FromResult(true);
    }

    /// <summary>
    /// Revoca todos los refresh tokens de un usuario.
    /// </summary>
    public Task<int> RevokeAllUserRefreshTokensAsync(string userId, string ipAddress = null, string reason = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(0);
        }

        List<RefreshToken> userTokens = TokenStore.Values.Where(t => t.UserId == userId && t.IsActive).ToList();
        int revokedCount = 0;

        foreach (RefreshToken token in userTokens)
        {
            token.IsRevoked = true;
            token.RevokedAt = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.RevocationReason = reason ?? "Bulk user revocation";
            revokedCount++;
        }

        return Task.FromResult(revokedCount);
    }

    /// <summary>
    /// Rota un refresh token (revoca el actual y genera uno nuevo).
    /// </summary>
    public async Task<RefreshToken> RotateRefreshTokenAsync(RefreshToken oldToken, string ipAddress = null, CancellationToken cancellationToken = default)
    {
        if (oldToken == null)
        {
            throw new ArgumentNullException(nameof(oldToken));
        }

        // Generar nuevo token
        RefreshToken newToken = await GenerateRefreshTokenAsync(oldToken.UserId, ipAddress, cancellationToken).ConfigureAwait(false);

        // Revocar el token anterior
        oldToken.IsRevoked = true;
        oldToken.RevokedAt = DateTime.UtcNow;
        oldToken.RevokedByIp = ipAddress;
        oldToken.RevocationReason = "Token rotation";
        oldToken.ReplacedByToken = newToken.Token;

        return newToken;
    }

    /// <summary>
    /// Limpia refresh tokens expirados.
    /// </summary>
    public Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        List<RefreshToken> expiredTokens = TokenStore.Values
            .Where(t => t.IsExpired || (t.IsRevoked && t.RevokedAt < DateTime.UtcNow.AddDays(-1)))
            .ToList();

        int removedCount = 0;
        foreach (RefreshToken token in expiredTokens)
        {
            if (TokenStore.TryRemove(token.Token, out _))
            {
                removedCount++;
            }
        }

        return Task.FromResult(removedCount);
    }

    /// <summary>
    /// Genera un token seguro aleatorio.
    /// </summary>
    private string GenerateSecureToken()
    {
        byte[] randomBytes = new byte[Options.RefreshTokenLength];
        using RandomNumberGenerator rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
