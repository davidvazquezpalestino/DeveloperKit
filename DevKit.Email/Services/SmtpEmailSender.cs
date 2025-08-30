namespace CoreMailKit.Services;

/// <summary>Implementación moderna de IEmailSender usando MailKit con patrones async.</summary>
public class SmtpEmailSender : IEmailSender, IDisposable
{
    private readonly MailKitSetting Settings;
    private readonly ILogger<SmtpEmailSender> Logger;
    private readonly SmtpClient SmtpClient;

    /// <summary>Inicializa una nueva instancia de SmtpEmailSender.</summary>
    public SmtpEmailSender(IOptions<MailKitSetting> options, ILogger<SmtpEmailSender> logger = null)
    {
        Settings = options?.Value ?? throw new ArgumentNullException(nameof(options));
        Logger = logger;
        SmtpClient = new SmtpClient();
    }


    /// <summary>Envía un mensaje de correo electrónico de forma asíncrona usando la configuración inyectada.</summary>
    public async Task SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        await SendEmailAsync(message, Settings, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>Envía un mensaje de correo electrónico de forma asíncrona con configuración personalizada.</summary>
    public async Task SendEmailAsync(EmailMessage message, MailKitSetting settings, CancellationToken cancellationToken = default)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        try
        {
            Logger?.LogInformation("Enviando email a {To} con asunto: {Subject}", message.To, message.Subject);

            using MimeMessage mimeMessage = CreateMimeMessage(message);

            if (!SmtpClient.IsConnected)
            {
                await SmtpClient.ConnectAsync(settings.Servidor, settings.Puerto, settings.HabilitarSsl, cancellationToken).ConfigureAwait(false);
            }

            if (!SmtpClient.IsAuthenticated)
            {
                await SmtpClient.AuthenticateAsync(settings.Cuenta, settings.Contrasena, cancellationToken).ConfigureAwait(false);
            }

            await SmtpClient.SendAsync(mimeMessage, cancellationToken).ConfigureAwait(false);

            Logger?.LogInformation("Email enviado exitosamente a {To}", message.To);
        }
        catch (Exception ex)
        {
            Logger?.LogError(ex, "Error enviando email a {To}: {Error}", message.To, ex.Message);
            throw;
        }
    }


    /// <summary>Envía un mensaje de correo electrónico de forma síncrona usando la configuración inyectada.</summary>
    public void SendEmail(EmailMessage message)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        SendEmailAsync(message).GetAwaiter().GetResult();
    }

    /// <summary>Envía un mensaje de correo electrónico de forma síncrona con configuración personalizada.</summary>
    public void SendEmail(EmailMessage message, MailKitSetting settings)
    {
        if (message == null)
        {
            throw new ArgumentNullException(nameof(message));
        }

        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        SendEmailAsync(message, settings).GetAwaiter().GetResult();
    }


    private static MimeMessage CreateMimeMessage(EmailMessage message)
    {
        MimeMessage mimeMessage = new MimeMessage();

        mimeMessage.From.Add(MailboxAddress.Parse(message.From));
        mimeMessage.To.Add(MailboxAddress.Parse(message.To));
        mimeMessage.Subject = message.Subject;

        if (!string.IsNullOrEmpty(message.Cc))
        {
            mimeMessage.Cc.Add(MailboxAddress.Parse(message.Cc));
        }

        if (!string.IsNullOrEmpty(message.Bcc))
        {
            mimeMessage.Bcc.Add(MailboxAddress.Parse(message.Bcc));
        }

        BodyBuilder bodyBuilder = new BodyBuilder();

        if (message.IsBodyHtml)
        {
            bodyBuilder.HtmlBody = message.Body;
        }
        else
        {
            bodyBuilder.TextBody = message.Body;
        }

        // Agregar attachments usando la nueva abstracción EmailAttachment
        if (message.Attachments?.Any() == true)
        {
            foreach (EmailAttachment attachment in message.Attachments)
            {
                if (!string.IsNullOrEmpty(attachment.FilePath))
                {
                    bodyBuilder.Attachments.Add(attachment.FilePath);
                }
                else if (attachment.ContentBytes != null)
                {
                    bodyBuilder.Attachments.Add(attachment.FileName, attachment.ContentBytes, ContentType.Parse(attachment.ContentType));
                }
                else if (attachment.ContentStream != null)
                {
                    bodyBuilder.Attachments.Add(attachment.FileName, attachment.ContentStream, ContentType.Parse(attachment.ContentType));
                }
            }
        }

        mimeMessage.Body = bodyBuilder.ToMessageBody();
        return mimeMessage;
    }

    /// <summary>Libera los recursos utilizados por el SmtpEmailSender.</summary>
    public void Dispose()
    {
        try
        {
            if (SmtpClient?.IsConnected == true)
            {
                SmtpClient.Disconnect(true);
            }
        }
        catch (Exception ex)
        {
            Logger?.LogWarning(ex, "Error desconectando SmtpClient: {Error}", ex.Message);
        }
        finally
        {
            SmtpClient?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
