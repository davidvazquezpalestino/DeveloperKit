namespace CoreMailKit.Entities;

/// <summary>Contiene la configuración necesaria para la conexión con el servidor de correo.</summary>
public class MailKitSetting
{
    /// <summary>Identificador único de la cuenta de correo.</summary>
    public int CuentaID { get; set; }
    /// <summary>Dirección del servidor SMTP.</summary>
    public string Servidor { get; set; }
    /// <summary>Puerto del servidor SMTP.</summary>
    public int Puerto { get; set; }
    /// <summary>Dirección de correo electrónico del remitente.</summary>
    public string Cuenta { get; set; }
    /// <summary>Contraseña de la cuenta de correo.</summary>
    public string Contrasena { get; set; }
    /// <summary>Indica si se debe usar SSL para la conexión segura.</summary>
    public bool HabilitarSsl { get; set; }
}