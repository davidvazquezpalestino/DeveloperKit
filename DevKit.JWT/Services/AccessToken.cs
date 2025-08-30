namespace DevKit.JWT.Services;

/// <summary>
/// Servicio para la generación y gestión completa de tokens JWT.
/// </summary>
public class AccessToken(
    IOptions<JwtOptions> options,
    IRefreshTokenService refreshTokenService,
    ILogger<AccessToken> logger)
    : IAccessToken
{
    private readonly JwtOptions Options = options.Value;

    /// <summary>
    /// Genera un token JWT con las configuraciones proporcionadas.
    /// </summary>
    public Task<string> GetTokenAsync(Action<Dictionary<string, string>> configurations)
    {
        SigningCredentials signingCredentials = GetSigningCredentials();
        List<Claim> claims = BuildClaims(configurations);
        SecurityTokenDescriptor tokenDescriptor = CreateTokenDescriptor(signingCredentials, claims);

        JsonWebTokenHandler tokenHandler = new JsonWebTokenHandler();
        string token = tokenHandler.CreateToken(tokenDescriptor);
        return Task.FromResult(token);
    }

    /// <summary>
    /// Genera un conjunto completo de tokens (access + refresh).
    /// </summary>
    public async Task<TokenResponse> GenerateTokenPairAsync(Action<Dictionary<string, string>> configurations, string ipAddress = null, CancellationToken cancellationToken = default)
    {
        string accessToken = await GetTokenAsync(configurations).ConfigureAwait(false);

        // Extraer UserId de las configuraciones para el refresh token
        Dictionary<string, string> configDict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        configurations?.Invoke(configDict);

        string userId = configDict.TryGetValue("IdentifierId", out string id) ? id :
                    configDict.TryGetValue("UserId", out string uid) ? uid :
                    throw new InvalidOperationException("UserId o IdentifierId debe estar presente en las configuraciones");

        RefreshToken refreshToken = await refreshTokenService.GenerateRefreshTokenAsync(userId, ipAddress, cancellationToken).ConfigureAwait(false);

        DateTime expiresAt = DateTime.UtcNow.AddMinutes(Options.ExpireInMinutes);

        logger.LogInformation("Par de tokens generado para usuario {UserId} desde IP {IpAddress}", userId, ipAddress);

        return new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresIn = Options.ExpireInMinutes * 60,
            ExpiresAt = expiresAt,
            TokenType = "Bearer"
        };
    }

    /// <summary>
    /// Renueva un access token usando un refresh token válido.
    /// </summary>
    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, string ipAddress = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token no puede estar vacío", nameof(refreshToken));
        }

        RefreshToken storedToken = await refreshTokenService.ValidateRefreshTokenAsync(refreshToken, cancellationToken).ConfigureAwait(false);
        if (storedToken == null)
        {
            logger.LogWarning("Intento de renovación con refresh token inválido desde IP {IpAddress}", ipAddress);
            throw new SecurityTokenValidationException("Refresh token inválido o expirado");
        }

        RefreshToken newRefreshToken;
        if (Options.EnableRefreshTokenRotation)
        {
            newRefreshToken = await refreshTokenService.RotateRefreshTokenAsync(storedToken, ipAddress, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            newRefreshToken = storedToken;
        }

        // Recrear access token con los mismos claims del usuario
        string newAccessToken = await GetTokenAsync(config =>
        {
            config["IdentifierId"] = storedToken.UserId;
            // Aquí podrías agregar lógica para recuperar roles/claims actualizados desde BD
        }).ConfigureAwait(false);

        DateTime expiresAt = DateTime.UtcNow.AddMinutes(Options.ExpireInMinutes);

        logger.LogInformation("Token renovado para usuario {UserId} desde IP {IpAddress}", storedToken.UserId, ipAddress);

        return new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresIn = Options.ExpireInMinutes * 60,
            ExpiresAt = expiresAt,
            TokenType = "Bearer"
        };
    }

    /// <summary>
    /// Valida un token JWT y extrae sus claims.
    /// </summary>
    public async Task<ClaimsPrincipal> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        try
        {
            JsonWebTokenHandler tokenHandler = new JsonWebTokenHandler();
            TokenValidationParameters validationParameters = GetTokenValidationParameters();

            TokenValidationResult result = await tokenHandler.ValidateTokenAsync(token, validationParameters).ConfigureAwait(false);

            if (result.IsValid)
            {
                return new ClaimsPrincipal(result.ClaimsIdentity);
            }

            logger.LogWarning("Token JWT inválido: {Error}", result.Exception?.Message);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error validando token JWT: {Error}", ex.Message);
            return null;
        }
    }

    /// <summary>
    /// Revoca un refresh token específico.
    /// </summary>
    public async Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress = null, CancellationToken cancellationToken = default)
    {
        bool result = await refreshTokenService.RevokeRefreshTokenAsync(refreshToken, ipAddress, "Manual revocation", cancellationToken).ConfigureAwait(false);

        if (result)
        {
            logger.LogInformation("Refresh token revocado manualmente desde IP {IpAddress}", ipAddress);
        }

        return result;
    }

    /// <summary>
    /// Revoca todos los tokens de un usuario.
    /// </summary>
    public async Task<int> RevokeAllUserTokensAsync(string userId, string ipAddress = null, CancellationToken cancellationToken = default)
    {
        int count = await refreshTokenService.RevokeAllUserRefreshTokensAsync(userId, ipAddress, "Bulk user revocation", cancellationToken).ConfigureAwait(false);

        logger.LogInformation("Revocados {Count} tokens para usuario {UserId} desde IP {IpAddress}", count, userId, ipAddress);

        return count;
    }

    /// <summary>
    /// Construye la lista de claims a partir de las configuraciones.
    /// </summary>
    private static List<Claim> BuildClaims(Action<Dictionary<string, string>> configurations)
    {
        Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        configurations?.Invoke(values);

        List<Claim> claims = new List<Claim>();
        foreach (KeyValuePair<string, string> pair in values)
        {
            if (string.Equals(pair.Key, "IdentifierId", StringComparison.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, pair.Value));
                continue;
            }

            if (string.Equals(pair.Key, "RolName", StringComparison.OrdinalIgnoreCase))
            {
                IEnumerable<Claim> roleClaims = (pair.Value ?? string.Empty)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Select(role => new Claim(ClaimTypes.Role, role));
                claims.AddRange(roleClaims);
                continue;
            }

            if (string.Equals(pair.Key, "Email", StringComparison.OrdinalIgnoreCase))
            {
                claims.Add(new Claim(ClaimTypes.Email, pair.Value));
                continue;
            }

            claims.Add(new Claim(pair.Key, pair.Value ?? string.Empty));
        }

        return claims;
    }

    /// <summary>
    /// Crea el descriptor de token con las configuraciones necesarias.
    /// </summary>
    private SecurityTokenDescriptor CreateTokenDescriptor(SigningCredentials signingCredentials, List<Claim> claims)
    {
        return new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = Options.ValidIssuer,
            Audience = Options.ValidAudience,
            Expires = DateTime.UtcNow.AddMinutes(Options.ExpireInMinutes),
            SigningCredentials = signingCredentials
        };
    }

    /// <summary>
    /// Obtiene los parámetros de validación para tokens JWT.
    /// </summary>
    private TokenValidationParameters GetTokenValidationParameters()
    {
        byte[] key = Encoding.UTF8.GetBytes(Options.SecurityKey);

        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = Options.ValidIssuer,
            ValidateAudience = true,
            ValidAudience = Options.ValidAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5),
            RequireExpirationTime = true
        };
    }

    /// <summary>
    /// Obtiene las credenciales de firma para el token.
    /// </summary>
    private SigningCredentials GetSigningCredentials()
    {
        string securityKey = Options.SecurityKey;
        if (string.IsNullOrWhiteSpace(securityKey))
        {
            throw new InvalidOperationException("SecurityKey no está configurado en JwtOptions.");
        }

        byte[] key = Encoding.UTF8.GetBytes(securityKey);
        SymmetricSecurityKey secret = new SymmetricSecurityKey(key);
        return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
    }
}