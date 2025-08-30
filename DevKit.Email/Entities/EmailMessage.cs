namespace CoreMailKit.Entities;

/// <summary> Representa un mensaje de correo electrónico con patrón de construcción fluido. </summary>
public class EmailMessage
{
    /// <summary>Dirección de correo del remitente</summary>
    public string From { get; private set; }

    /// <summary>Dirección de correo del destinatario</summary>
    public string To { get; private set; }

    /// <summary>Asunto del mensaje</summary>
    public string Subject { get; private set; }

    /// <summary>Contenido del mensaje</summary>
    public string Body { get; private set; }

    /// <summary>Indica si el cuerpo del mensaje es HTML</summary>
    public bool IsBodyHtml { get; private set; } = true;

    /// <summary>Dirección de copia (opcional)</summary>
    public string Cc { get; private set; }

    /// <summary>Dirección de copia oculta (opcional)</summary>
    public string Bcc { get; private set; }

    /// <summary>Lista de archivos adjuntos</summary>
    public IReadOnlyList<EmailAttachment> Attachments { get; private set; } = new List<EmailAttachment>();

    private EmailMessage() { }

    /// <summary> Crea una nueva instancia de EmailMessage con los campos requeridos.</summary>
    public static EmailMessage Create(string from, string to, string subject, string body)
    {
        return new EmailMessage
        {
            From = from ?? throw new ArgumentNullException(nameof(from)),
            To = to ?? throw new ArgumentNullException(nameof(to)),
            Subject = subject ?? string.Empty,
            Body = body ?? string.Empty
        };
    }

    /// <summary>Establece el remitente del mensaje.</summary>
    public EmailMessage WithFrom(string from)
    {
        From = from ?? throw new ArgumentNullException(nameof(from));
        return this;
    }

    /// <summary>Establece el destinatario del mensaje.</summary>
    public EmailMessage WithTo(string to)
    {
        To = to ?? throw new ArgumentNullException(nameof(to));
        return this;
    }

    /// <summary>Establece el asunto del mensaje.</summary>
    public EmailMessage WithSubject(string subject)
    {
        Subject = subject ?? string.Empty;
        return this;
    }

    /// <summary>Establece el cuerpo del mensaje.</summary>
    public EmailMessage WithBody(string body, bool isHtml = true)
    {
        Body = body ?? string.Empty;
        IsBodyHtml = isHtml;
        return this;
    }

    /// <summary>Establece la dirección de copia (CC) del mensaje.</summary>
    public EmailMessage WithCc(string cc)
    {
        Cc = cc;
        return this;
    }

    /// <summary>Establece la dirección de copia oculta (BCC) del mensaje.</summary>
    public EmailMessage WithBcc(string bcc)
    {
        Bcc = bcc;
        return this;
    }

    /// <summary>Agrega un archivo adjunto al mensaje.</summary>
    public EmailMessage WithAttachment(EmailAttachment attachment)
    {
        if (attachment == null)
        {
            throw new ArgumentNullException(nameof(attachment));
        }

        List<EmailAttachment> attachments = new List<EmailAttachment>(Attachments) { attachment };
        Attachments = attachments.AsReadOnly();
        return this;
    }

    /// <summary>Agrega múltiples archivos adjuntos al mensaje.</summary>
    public EmailMessage WithAttachments(IEnumerable<EmailAttachment> attachments)
    {
        if (attachments == null)
        {
            throw new ArgumentNullException(nameof(attachments));
        }

        List<EmailAttachment> newAttachments = new List<EmailAttachment>(Attachments);
        newAttachments.AddRange(attachments);
        Attachments = newAttachments.AsReadOnly();
        return this;
    }
}
