namespace DevKit.Extensions.Security;

/// <summary>Proporciona métodos para cifrar y descifrar texto usando el algoritmo AES.</summary>
public static class SecurityAes
{
    private static readonly string CypherPattern = "T9dWq7FhJ2nPxYB6zKcLmVrX5uG8QeAt";
    /// <summary>Cifra una cadena de texto usando el algoritmo AES.</summary>
    public static string Encrypt(string plainText)
    {
        byte[] key = GetKeyBytes();
        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.GenerateIV(); // IV aleatorio
            byte[] iv = aes.IV;

            using (MemoryStream memoryStream = new MemoryStream())
            {
                memoryStream.Write(iv, 0, iv.Length); // Guardamos el IV al principio

                using (CryptoStream cryptoStream =
                       new CryptoStream(memoryStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    {
                        streamWriter.Write(plainText);
                        streamWriter.Close();

                        return Convert.ToBase64String(memoryStream.ToArray());
                    }
                }
            }
        }
    }

    /// <summary>Descifra una cadena de texto previamente cifrada con AES.</summary>
    public static string Decrypt(string encryptedText)
    {
        byte[] fullCipher = Convert.FromBase64String(encryptedText);
        byte[] key = GetKeyBytes();

        if (fullCipher.Length < 16)
        {
            throw new ArgumentException("El texto cifrado es inválido o está corrupto.");
        }

        byte[] iv = new byte[16];
        byte[] cipherText = new byte[fullCipher.Length - iv.Length];
        Array.Copy(fullCipher, 0, iv, 0, iv.Length);
        Array.Copy(fullCipher, iv.Length, cipherText, 0, cipherText.Length);

        using (Aes aes = Aes.Create())
        {
            aes.Key = key;
            aes.IV = iv;

            using (MemoryStream memoryStream = new MemoryStream(cipherText))
            {
                using (CryptoStream cryptoStream =
                       new CryptoStream(memoryStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    using (StreamReader streamReader = new StreamReader(cryptoStream))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }
    }

    /// <summary>Obtiene los bytes de la clave de cifrado.</summary>
    private static byte[] GetKeyBytes()
    {
        byte[] key = Encoding.UTF8.GetBytes(CypherPattern);
        if (key.Length != 16 && key.Length != 24 && key.Length != 32)
        {
            throw new ArgumentException("La clave debe tener 16, 24 o 32 caracteres.");
        }

        return key;
    }
}

