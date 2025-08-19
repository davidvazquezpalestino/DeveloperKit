namespace DevKit.JWT.Services;

/// <summary>
/// Servicio para la generación de tokens JWT.
/// </summary>
public class AccessToken(IOptions<JwtOptions> options) : IAccessToken
{

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
            Issuer = options.Value.ValidIssuer,
            Audience = options.Value.ValidAudience,
            Expires = DateTime.UtcNow.AddMinutes(options.Value.ExpireInMinutes),
            SigningCredentials = signingCredentials
        };
    }

    /// <summary>
    /// Obtiene las credenciales de firma para el token.
    /// </summary>
    private SigningCredentials GetSigningCredentials()
    {
        string securityKey = options.Value.SecurityKey;
        if (string.IsNullOrWhiteSpace(securityKey))
        {
            throw new InvalidOperationException("SecurityKey no está configurado en JwtOptions.");
        }

        byte[] key = Encoding.UTF8.GetBytes(securityKey);
        SymmetricSecurityKey secret = new SymmetricSecurityKey(key);
        return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
    }
}