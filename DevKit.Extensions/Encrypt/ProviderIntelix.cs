namespace DevKit.Extensions.Encrypt;

/// <summary>Proporciona métodos para cifrar y descifrar cadenas usando un algoritmo personalizado.</summary>
public static class ProviderIntelix
{
    private static readonly string FindPattern = "0gzHaYIV2J>XñDxkA1mw3BiEZbKpervL6M5hPn<fÑN7C@#|9OS4TWo8FsQjdRGtycql";
    private static readonly string CypherPattern = "Yc@8IkMgFqoB2DjyPvA1Gl9ñ<C#St5Ha0xRN6LTbn7OXmKd3ZQVzWÑ4>rEsfw|ieJhp";

    /// <summary>Cifra una cadena de caracteres usando un algoritmo personalizado.</summary>
    /// <param name="cadenaACifrar">Cadena de caracteres que desea cifrarse</param>
    /// <returns>Cadena Cifrada, o Cadena Vacia en caso de excepción</returns>
    public static string CifrarCadena(string cadenaACifrar)
    {
        string result = "";

        for (int idx = 0; idx < cadenaACifrar.Length; idx++)
        {
            result += CifrarCaracter(cadenaACifrar.Substring(idx, 1), cadenaACifrar.Length, idx);
        }
        return result;

    }

    /// <summary>Cifra un carácter individual usando el patrón de cifrado.</summary>
    /// <param name="caracterACifrar">Caracter que se desea cifrar</param>
    /// <param name="length">Longitud de la cadena que se desea cifrar</param>
    /// <param name="index">Posición del caracter en la cadena original</param>
    /// <returns>Caracter cifrado, o el mismo caracter si no se encuentra en el patrón</returns>
    public static string CifrarCaracter(string caracterACifrar, int length, int index)
    {
        if (FindPattern.IndexOf(caracterACifrar, StringComparison.Ordinal) != -1)
        {
            int indice = (FindPattern.IndexOf(caracterACifrar, StringComparison.Ordinal) + length + index) % FindPattern.Length;
            return CypherPattern.Substring(indice, 1);
        }
        return caracterACifrar;
    }

    /// <summary>Descifra una cadena previamente cifrada con el método CifrarCadena.</summary>
    /// <param name="cadenaADecifrar">Cadena a descifrar</param>
    /// <returns>Cadena descifrada</returns>
    public static string DescrifrarCadena(string cadenaADecifrar)
    {
        string result = "";

        for (int index = 0; index < cadenaADecifrar.Length; index++)
        {
            result += DecifrarCaracter(cadenaADecifrar.Substring(index, 1), cadenaADecifrar.Length, index);
        }
        return result;
    }

    /// <summary>Descifra un carácter individual usando el patrón de cifrado.</summary>
    /// <param name="caracterADecifrar">Caracter que se desea descifrar</param>
    /// <param name="length">Longitud de la cadena original</param>
    /// <param name="index">Posición del caracter en la cadena cifrada</param>
    /// <returns>Caracter descifrado, o el mismo caracter si no se encuentra en el patrón</returns>
    public static string DecifrarCaracter(string caracterADecifrar, int length, int index)
    {
        int pos = CypherPattern.IndexOf(caracterADecifrar, StringComparison.Ordinal);
        if (pos == -1)
        {
            return caracterADecifrar;
        }

        int originalPos = pos - length - index;
        originalPos = (originalPos % FindPattern.Length + FindPattern.Length) % FindPattern.Length;
        return FindPattern.Substring(originalPos, 1);
    }
}