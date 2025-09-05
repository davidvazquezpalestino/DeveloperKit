namespace CoreMailKit.Interfaces;

/// <summary>Define las operaciones para el envío de mensajes de correo electrónico.</summary>
public interface IEmailSender
{
    #region Async Methods
    
    /// <summary>Envía un mensaje de correo electrónico de forma asíncrona usando la configuración inyectada.</summary>
    /// <param name="message">El mensaje de correo electrónico a enviar.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);

    /// <summary>Envía un mensaje de correo electrónico de forma asíncrona con configuración personalizada.</summary>
    /// <param name="message">El mensaje de correo electrónico a enviar.</param>
    /// <param name="settings">Configuración personalizada del servidor SMTP.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task SendEmailAsync(EmailMessage message, MailKitSetting settings, CancellationToken cancellationToken = default);

    #endregion

    #region Sync Methods
    
    /// <summary>Envía un mensaje de correo electrónico de forma síncrona usando la configuración inyectada.</summary>
    /// <param name="message">El mensaje de correo electrónico a enviar.</param>
    void SendEmail(EmailMessage message);

    /// <summary>Envía un mensaje de correo electrónico de forma síncrona con configuración personalizada.</summary>
    /// <param name="message">El mensaje de correo electrónico a enviar.</param>
    /// <param name="settings">Configuración personalizada del servidor SMTP.</param>
    void SendEmail(EmailMessage message, MailKitSetting settings);
    
    #endregion
}
