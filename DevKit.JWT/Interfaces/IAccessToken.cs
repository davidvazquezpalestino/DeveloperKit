namespace DevKit.JWT.Interfaces;

/// <summary>
/// Interfaz para la generación de tokens JWT.
/// </summary>
public interface IAccessToken
{
    /// <summary>
    /// Genera un token JWT con las configuraciones especificadas.
    /// </summary>
    Task<string> GetTokenAsync(Action<Dictionary<string, string>> configurations);
}