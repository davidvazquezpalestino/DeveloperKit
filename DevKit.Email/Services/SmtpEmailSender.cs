namespace CoreMailKit.Services;

/// <summary>Implementación simple de IEmailSender usando SmtpClient.</summary>
public class SmtpEmailSender : IEmailSender, IDisposable
{
    private readonly SmtpClient SmtpClient;
    private readonly MailKitSetting Settings;

    /// <summary>Inicializa una nueva instancia de SmtpEmailSender.</summary>
    public SmtpEmailSender(IOptions<MailKitSetting> options)
    {
        Settings = options?.Value ?? throw new ArgumentNullException(nameof(options));

        SmtpClient = new SmtpClient(Settings.Servidor, Settings.Puerto)
        {
            EnableSsl = Settings.HabilitarSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(Settings.Cuenta, Settings.Contrasena)
        };
    }


    /// <summary>Envía un mensaje de correo electrónico de forma asíncrona usando la configuración inyectada.</summary>
    public async Task SendEmailAsync(EmailMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        await SendEmailAsync(message, Settings);
    }

    /// <summary>Envía un mensaje de correo electrónico de forma asíncrona con configuración personalizada.</summary>
    public async Task SendEmailAsync(EmailMessage message, MailKitSetting settings)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        using (MailMessage mailMessage = CreateMailMessage(message))
        {
            await SmtpClient.SendMailAsync(mailMessage);
        }
    }


    /// <summary>Envía un mensaje de correo electrónico de forma síncrona usando la configuración inyectada.</summary>
    public void SendEmail(EmailMessage message)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        SendEmail(message, Settings);
    }

    /// <summary>Envía un mensaje de correo electrónico de forma síncrona con configuración personalizada.</summary>
    public void SendEmail(EmailMessage message, MailKitSetting settings)
    {
        if (message == null) throw new ArgumentNullException(nameof(message));
        if (settings == null) throw new ArgumentNullException(nameof(settings));

        using (MailMessage mailMessage = CreateMailMessage(message))
        {
            SmtpClient.Send(mailMessage);
        }
    }


    private static MailMessage CreateMailMessage(EmailMessage message)
    {
        MailMessage mailMessage = new MailMessage(
            from: message.From,
            to: message.To,
            subject: message.Subject,
            body: message.Body)
        {
            IsBodyHtml = message.IsBodyHtml
        };

        if (!string.IsNullOrEmpty(message.Cc))
            mailMessage.CC.Add(message.Cc);

        if (!string.IsNullOrEmpty(message.Bcc))
            mailMessage.Bcc.Add(message.Bcc);

        if (message.Attachments?.Any() == true)
        {
            foreach (Attachment attachment in message.Attachments)
            {
                mailMessage.Attachments.Add(attachment);
            }
        }

        return mailMessage;
    }

    /// <summary>Libera los recursos utilizados por el SmtpEmailSender.</summary>
    public void Dispose()
    {
        SmtpClient?.Dispose();
        GC.SuppressFinalize(this);
    }
}
