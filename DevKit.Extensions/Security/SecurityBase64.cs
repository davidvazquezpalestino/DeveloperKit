namespace DevKit.Extensions.Security;

/// <summary>Proporciona métodos para codificar y decodificar cadenas en Base64.</summary>
public class SecurityBase64
{
    /// <summary>Codifica una cadena a Base64.</summary>
    public static string Encrypting(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text));
    /// <summary>Decodifica una cadena desde Base64.</summary>
    public static string Decrypting(string text) => Encoding.UTF8.GetString(Convert.FromBase64String(text));
}