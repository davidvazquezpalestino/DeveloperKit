# JWT Authentication for DeveloperKit

[![NuGet](https://img.shields.io/nuget/v/DevKit.JWT.svg?style=flat-square)](https://www.nuget.org/packages/DevKit.JWT/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-blue)](https://dotnet.microsoft.com/)

A robust and easy-to-implement JWT (JSON Web Token) authentication library for ASP.NET Core applications. Part of the DeveloperKit ecosystem, this component provides secure token generation, validation, and management with minimal configuration.

## ✨ Features

- **Secure Token Generation**
  - HMAC, RSA, and ECDSA signing algorithms
  - Configurable token expiration and validation parameters
  - Support for custom claims and token metadata

- **Flexible Configuration**
  - Simple setup with sensible defaults
  - Support for multiple issuers and audiences
  - Custom token validation parameters
  - Seamless integration with ASP.NET Core Identity

- **Security First**
  - Secure key management
  - Automatic token validation
  - Protection against common JWT vulnerabilities
  - Support for token refresh mechanisms

- **Developer Experience**
  - Clean, intuitive API
  - Comprehensive logging
  - Detailed error messages
  - Async support throughout

## 🚀 Getting Started

### Prerequisites

- .NET 6.0+ or .NET Standard 2.0+
- ASP.NET Core 6.0+
- Visual Studio 2022 or VS Code with C# Dev Kit (recommended)

### Installation

```bash
dotnet add package DevKit.JWT
```

## 🔧 Configuration

### Basic Setup

```csharp
public void ConfigureServices(IServiceCollection services)
{
    // Add JWT Authentication
    services.AddJwtAuthentication(Configuration.GetSection("Jwt"));
    
    // Add Controllers and other services
    services.AddControllers();
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    // Other middleware...
    
    app.UseAuthentication();
    app.UseAuthorization();
    
    // Other middleware...
}
```

### appsettings.json

```json
{
  "Jwt": {
    "Issuer": "https://your-issuer.com",
    "Audience": "your-audience",
    "SecretKey": "your-256-bit-secret-key-at-least-32-characters-long",
    "ExpireMinutes": 60,
    "RefreshTokenExpireDays": 7
  }
}
```

### Advanced Configuration

```csharp
services.AddJwtAuthentication(options =>
{
    // Required
    options.Issuer = "https://your-issuer.com";
    options.Audience = "your-audience";
    options.SecretKey = Configuration["Jwt:SecretKey"];
    
    // Optional with defaults shown
    options.ExpireMinutes = 60;
    options.ClockSkew = TimeSpan.FromMinutes(5);
    options.ValidateIssuer = true;
    options.ValidateAudience = true;
    options.ValidateLifetime = true;
    options.ValidateIssuerSigningKey = true;
    
    // Refresh token options
    options.RefreshTokenExpireDays = 7;
    
    // Custom token validation parameters
    options.TokenValidationParameters = new TokenValidationParameters
    {
        // Override any parameters as needed
        NameClaimType = JwtRegisteredClaimNames.Sub,
        RoleClaimType = ClaimTypes.Role
    };
});
services.AddJsonWebToken(options =>
{
    // Clave de seguridad (mínimo 16 caracteres)
    options.SecurityKey = "your-very-long-secret-key-here";
    
    // Emisor del token
    options.ValidIssuer = "https://api.yourdomain.com";
    
    // Cliente que puede usar el token
    options.ValidAudience = "client-123";
    
    // Duración del token en minutos
    options.ExpireInMinutes = 120;
}, ServiceLifetime.Scoped);
}

## 💻 Usage Examples

### 1. Basic Token Generation

```csharp
public class AuthService
{
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ILogger<AuthService> _logger;

    public AuthService(IJwtTokenGenerator tokenGenerator, ILogger<AuthService> logger)
    {
        _tokenGenerator = tokenGenerator;
        _logger = logger;
    }

    public async Task<AuthResponse> AuthenticateAsync(LoginRequest request)
    {
        // 1. Validate user credentials (replace with your actual user validation logic)
        var user = await _userService.ValidateCredentialsAsync(request.Username, request.Password);
        if (user == null)
        {
            _logger.LogWarning("Authentication failed for user {Username}", request.Username);
            return AuthResponse.Failed("Invalid username or password");
        }

        // 2. Generate access token
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Name, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("custom_claim", "custom_value") // Add custom claims as needed
        };

        // Add roles as claims
        foreach (var role in user.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        // 3. Generate access token
        var accessToken = _tokenGenerator.GenerateAccessToken(claims);
        
        // 4. Generate refresh token
        var refreshToken = _tokenGenerator.GenerateRefreshToken();
        
        // 5. Store refresh token (implementation depends on your storage)
        await _tokenStore.StoreRefreshTokenAsync(user.Id, refreshToken, 
            DateTime.UtcNow.AddDays(7)); // Match with RefreshTokenExpireDays

        _logger.LogInformation("User {UserId} authenticated successfully", user.Id);
        
        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = TimeSpan.FromMinutes(60).TotalSeconds,
            TokenType = "Bearer"
        };
    }
}

### 2. Token Refresh Flow

```csharp
public class TokenService
{
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ILogger<TokenService> _logger;

    public TokenService(
        IJwtTokenGenerator tokenGenerator,
        IRefreshTokenService refreshTokenService,
        ILogger<TokenService> logger)
    {
        _tokenGenerator = tokenGenerator;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
    }

    public async Task<AuthResponse> RefreshTokenAsync(string accessToken, string refreshToken)
    {
        // 1. Validate the expired access token (without lifetime validation)
        var principal = _tokenGenerator.GetPrincipalFromExpiredToken(accessToken);
        var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("Invalid access token format");
            return AuthResponse.Failed("Invalid token");
        }

        // 2. Validate refresh token
        var storedToken = await _refreshTokenService.GetRefreshTokenAsync(userId, refreshToken);
        if (storedToken == null || storedToken.IsUsed || storedToken.IsRevoked || 
            storedToken.ExpiryDate <= DateTime.UtcNow)
        {
            _logger.LogWarning("Invalid refresh token for user {UserId}", userId);
            return AuthResponse.Failed("Invalid refresh token");
        }

        // 3. Mark token as used to prevent reuse
        await _refreshTokenService.MarkAsUsedAsync(storedToken.Id);

        // 4. Generate new tokens
        var newAccessToken = _tokenGenerator.GenerateAccessToken(principal.Claims);
        var newRefreshToken = _tokenGenerator.GenerateRefreshToken();
        
        // 5. Store the new refresh token
        await _refreshTokenService.StoreRefreshTokenAsync(
            userId, 
            newRefreshToken, 
            DateTime.UtcNow.AddDays(7));

        _logger.LogInformation("Refreshed tokens for user {UserId}", userId);
        
        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = TimeSpan.FromMinutes(60).TotalSeconds,
            TokenType = "Bearer"
        };
    }
}

### 3. Protected API Endpoints

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires authentication by default
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var user = await _userService.GetUserByIdAsync(userId);
        
        if (user == null)
        {
            _logger.LogWarning("User {UserId} not found", userId);
            return NotFound();
        }

        _logger.LogInformation("Retrieved profile for user {UserId}", userId);
        return Ok(user);
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
    {
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        var result = await _userService.UpdateProfileAsync(userId, request);
        
        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed to update profile for user {UserId}", userId);
            return BadRequest(result.Errors);
        }

        _logger.LogInformation("Updated profile for user {UserId}", userId);
        return NoContent();
    }

    [HttpGet("admin-only")]
    [Authorize(Roles = "Admin")] // Requires Admin role
    public IActionResult AdminOnly()
    {
        return Ok("This is an admin-only endpoint");
    }

    [HttpGet("custom-policy")]
    [Authorize(Policy = "MinimumAge")] // Requires custom policy
    public IActionResult MinimumAge()
    {
        return Ok("You meet the minimum age requirement");
    }
}

## 🔐 Security Best Practices

### 1. Secure Token Storage

```csharp
// In your token storage implementation
public class RefreshTokenStore : IRefreshTokenService
{
    private readonly IDataProtector _protector;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RefreshTokenStore> _logger;

    public RefreshTokenStore(
        IDataProtectionProvider dataProtectionProvider,
        ApplicationDbContext context,
        ILogger<RefreshTokenStore> logger)
    {
        _protector = dataProtectionProvider.CreateProtector("RefreshTokenPurpose");
        _context = context;
        _logger = logger;
    }

    public async Task<string> StoreRefreshTokenAsync(string userId, string token, DateTime expiryDate)
    {
        // Encrypt the token before storing
        var protectedToken = _protector.Protect(token);
        
        var refreshToken = new RefreshToken
        {
            UserId = userId,
            Token = protectedToken,
            ExpiryDate = expiryDate,
            CreatedAt = DateTime.UtcNow,
            IsRevoked = false,
            IsUsed = false
        };

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Stored refresh token for user {UserId}", userId);
        
        // Return the original (unencrypted) token to the client
        return token;
    }

    public async Task<RefreshToken> GetRefreshTokenAsync(string userId, string token)
    {
        // Find all non-expired, non-revoked, non-used tokens for the user
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && 
                       !t.IsRevoked && 
                       !t.IsUsed && 
                       t.ExpiryDate > DateTime.UtcNow)
            .ToListAsync();

        // Try to find a matching token
        foreach (var storedToken in tokens)
        {
            try
            {
                var unprotectedToken = _protector.Unprotect(storedToken.Token);
                if (unprotectedToken == token)
                {
                    return storedToken;
                }
            }
            catch (CryptographicException ex)
            {
                _logger.LogError(ex, "Failed to unprotect refresh token {TokenId}", storedToken.Id);
                continue;
            }
        }

        return null;
    }
}

### 2. Token Validation

```csharp
// In your startup configuration
services.AddJwtAuthentication(options =>
{
    // ... other options ...
    
    // Enable token validation events for additional security checks
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            // Additional validation can be performed here
            var userId = context.Principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            
            // Example: Check if user exists and is active
            var userService = context.HttpContext.RequestServices
                .GetRequiredService<IUserService>();
                
            var user = userService.GetUserByIdAsync(userId).GetAwaiter().GetResult();
            
            if (user == null || !user.IsActive)
            {
                context.Fail("User is not active or does not exist");
            }
            
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Add("Token-Expired", "true");
            }
            
            return Task.CompletedTask;
        }
    };
});

## 📚 API Reference

### Interfaces

#### `IJwtTokenGenerator`
Main interface for JWT token generation and validation.

**Methods:**
- `string GenerateAccessToken(IEnumerable<Claim> claims)` - Generates a JWT access token
- `string GenerateRefreshToken()` - Generates a secure refresh token
- `ClaimsPrincipal GetPrincipalFromExpiredToken(string token)` - Gets principal from expired token (for refresh flow)
- `bool ValidateToken(string token, out ClaimsPrincipal principal)` - Validates a token

#### `IRefreshTokenService`
Interface for managing refresh tokens.

**Methods:**
- `Task<string> StoreRefreshTokenAsync(string userId, string token, DateTime expiryDate)`
- `Task<RefreshToken> GetRefreshTokenAsync(string userId, string token)`
- `Task MarkAsUsedAsync(string tokenId)`
- `Task RevokeTokenAsync(string tokenId)`
- `Task RevokeAllTokensAsync(string userId)`

### Configuration

#### `JwtOptions`
Configuration options for JWT authentication.

**Properties:**
- `string Issuer` - Token issuer (required)
- `string Audience` - Token audience (required)
- `string SecretKey` - Secret key for signing tokens (required)
- `int ExpireMinutes` - Token expiration in minutes (default: 60)
- `int RefreshTokenExpireDays` - Refresh token expiration in days (default: 7)
- `TimeSpan ClockSkew` - Clock skew for token validation (default: 5 minutes)
- `bool ValidateIssuer` - Whether to validate the issuer (default: true)
- `bool ValidateAudience` - Whether to validate the audience (default: true)
- `bool ValidateLifetime` - Whether to validate the lifetime (default: true)
- `bool ValidateIssuerSigningKey` - Whether to validate the signing key (default: true)
- `TokenValidationParameters TokenValidationParameters` - Advanced token validation parameters

## 🔒 Security Considerations

1. **Key Management**
   - Never hardcode secrets in source code
   - Use Azure Key Vault, AWS Secrets Manager, or environment variables in production
   - Rotate signing keys periodically

2. **Token Security**
   - Use HTTPS in production
   - Set appropriate token expiration times
   - Implement token revocation
   - Use secure, random refresh tokens

3. **Best Practices**
   - Implement rate limiting on authentication endpoints
   - Log security events (failed logins, token refreshes, etc.)
   - Use the latest security patches for all dependencies

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🤝 Contributing

Contributions are welcome! Please read our [contributing guidelines](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## 📫 Support

For support, please open an issue in our [issue tracker](https://github.com/davidvazquezpalestino/DeveloperKit/issues).

---

<div align="center">
  Made with ❤️ by the DeveloperKit Team
</div>
