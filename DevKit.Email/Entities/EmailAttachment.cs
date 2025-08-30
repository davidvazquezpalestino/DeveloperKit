namespace CoreMailKit.Entities;

/// <summary>
/// Representa un archivo adjunto para correos electrónicos compatible con MailKit.
/// </summary>
public class EmailAttachment
{
    /// <summary>
    /// Nombre del archivo adjunto.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Tipo MIME del archivo.
    /// </summary>
    public string ContentType { get; set; } = "application/octet-stream";

    /// <summary>
    /// Contenido del archivo como stream.
    /// </summary>
    public Stream ContentStream { get; set; }

    /// <summary>
    /// Contenido del archivo como array de bytes.
    /// </summary>
    public byte[]? ContentBytes { get; set; }

    /// <summary>
    /// Ruta del archivo en disco.
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// Crea un attachment desde un stream.
    /// </summary>
    public static EmailAttachment FromStream(string fileName, Stream stream, string contentType = "application/octet-stream")
    {
        return new EmailAttachment
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName)),
            ContentStream = stream ?? throw new ArgumentNullException(nameof(stream)),
            ContentType = contentType
        };
    }

    /// <summary>
    /// Crea un attachment desde un array de bytes.
    /// </summary>
    public static EmailAttachment FromBytes(string fileName, byte[] bytes, string contentType = "application/octet-stream")
    {
        return new EmailAttachment
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName)),
            ContentBytes = bytes ?? throw new ArgumentNullException(nameof(bytes)),
            ContentType = contentType
        };
    }

    /// <summary>
    /// Crea un attachment desde un archivo en disco.
    /// </summary>
    public static EmailAttachment FromFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Archivo no encontrado: {filePath}");
        }

        return new EmailAttachment
        {
            FileName = Path.GetFileName(filePath),
            FilePath = filePath,
            ContentType = GetContentType(filePath)
        };
    }

    /// <summary>
    /// Obtiene el tipo MIME basado en la extensión del archivo.
    /// </summary>
    private static string GetContentType(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".txt" => "text/plain",
            ".html" => "text/html",
            ".zip" => "application/zip",
            _ => "application/octet-stream"
        };
    }
}
