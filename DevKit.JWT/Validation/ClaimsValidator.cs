using System.Net.Mail;

namespace DevKit.JWT.Validation;

/// <summary>
/// Validador personalizable de claims JWT.
/// </summary>
public class ClaimsValidator
{
    private readonly Dictionary<string, Func<string, bool>> Validators;
    private readonly ILogger<ClaimsValidator> Logger;

    public ClaimsValidator(ILogger<ClaimsValidator> logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        Validators = new Dictionary<string, Func<string, bool>>(StringComparer.OrdinalIgnoreCase);

        // Validadores predeterminados
        RegisterDefaultValidators();
    }

    /// <summary>
    /// Registra un validador personalizado para un claim específico.
    /// </summary>
    public ClaimsValidator RegisterValidator(string claimType, Func<string, bool> validator)
    {
        if (string.IsNullOrWhiteSpace(claimType))
        {
            throw new ArgumentException("Claim type no puede estar vacío", nameof(claimType));
        }

        Validators[claimType] = validator ?? throw new ArgumentNullException(nameof(validator));
        Logger.LogDebug("Validador registrado para claim type: {ClaimType}", claimType);

        return this;
    }

    /// <summary>
    /// Valida todos los claims de un ClaimsPrincipal.
    /// </summary>
    public ValidationResult ValidateClaims(ClaimsPrincipal principal)
    {
        if (principal?.Identity == null || !principal.Identity.IsAuthenticated)
        {
            return ValidationResult.Failed("Principal no autenticado");
        }

        List<string> errors = new List<string>();
        List<string> validatedClaims = new List<string>();

        foreach (Claim claim in principal.Claims)
        {
            if (Validators.TryGetValue(claim.Type, out Func<string, bool> validator))
            {
                try
                {
                    if (!validator(claim.Value))
                    {
                        errors.Add($"Claim '{claim.Type}' con valor '{claim.Value}' no es válido");
                        Logger.LogWarning("Validación fallida para claim {ClaimType}: {ClaimValue}", claim.Type, claim.Value);
                    }
                    else
                    {
                        validatedClaims.Add(claim.Type);
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Error validando claim '{claim.Type}': {ex.Message}");
                    Logger.LogError(ex, "Error validando claim {ClaimType}", claim.Type);
                }
            }
        }

        if (errors.Any())
        {
            return ValidationResult.Failed(errors.ToArray());
        }

        Logger.LogDebug("Claims validados exitosamente: {ValidatedClaims}", string.Join(", ", validatedClaims));
        return ValidationResult.Success();
    }

    /// <summary>
    /// Valida claims específicos con reglas personalizadas.
    /// </summary>
    public ValidationResult ValidateSpecificClaims(ClaimsPrincipal principal, params string[] requiredClaimTypes)
    {
        if (principal?.Identity == null || !principal.Identity.IsAuthenticated)
        {
            return ValidationResult.Failed("Principal no autenticado");
        }

        List<string> errors = new List<string>();

        foreach (string requiredClaimType in requiredClaimTypes)
        {
            Claim claim = principal.FindFirst(requiredClaimType);
            if (claim == null)
            {
                errors.Add($"Claim requerido '{requiredClaimType}' no encontrado");
                continue;
            }

            if (Validators.TryGetValue(requiredClaimType, out Func<string, bool> validator))
            {
                if (!validator(claim.Value))
                {
                    errors.Add($"Claim '{requiredClaimType}' no es válido");
                }
            }
        }

        return errors.Any() ? ValidationResult.Failed(errors.ToArray()) : ValidationResult.Success();
    }

    /// <summary>
    /// Registra validadores predeterminados para claims comunes.
    /// </summary>
    private void RegisterDefaultValidators()
    {
        // Validador para email
        RegisterValidator(ClaimTypes.Email, email =>
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                MailAddress addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        });

        // Validador para roles (no vacío)
        RegisterValidator(ClaimTypes.Role,
            role => !string.IsNullOrWhiteSpace(role));

        // Validador para NameIdentifier (GUID o número)
        RegisterValidator(ClaimTypes.NameIdentifier, id =>
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            return Guid.TryParse(id, out _) || long.TryParse(id, out _);
        });

        // Validador para fechas de expiración
        RegisterValidator("exp", exp =>
        {
            if (long.TryParse(exp, out long expTime))
            {
                DateTimeOffset expDateTime = DateTimeOffset.FromUnixTimeSeconds(expTime);
                return expDateTime > DateTimeOffset.UtcNow;
            }
            return false;
        });
    }
}

/// <summary>
/// Resultado de validación de claims.
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string[] Errors { get; private set; } = Array.Empty<string>();

    private ValidationResult(bool isValid, params string[] errors)
    {
        IsValid = isValid;
        Errors = errors ?? Array.Empty<string>();
    }

    public static ValidationResult Success() => new(true);
    public static ValidationResult Failed(params string[] errors) => new(false, errors);
}
