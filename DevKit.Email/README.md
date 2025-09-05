# Email Component for DeveloperKit

[![NuGet](https://img.shields.io/nuget/v/DevKit.Email.svg?style=flat-square)](https://www.nuget.org/packages/DevKit.Email/)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)
[![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.0-blue)](https://dotnet.microsoft.com/)

A modern, robust, and fully-featured email component for .NET applications, built on top of **MailKit** for reliable and secure email delivery. This component is part of the DeveloperKit ecosystem and provides a clean, easy-to-use API for sending emails with support for attachments, HTML content, and more.

## ✨ Features

- **Modern SMTP Configuration**
  - Built on MailKit (not the legacy System.Net.Mail)
  - Full SSL/TLS support with modern security protocols
  - Secure credential handling
  - Flexible port and timeout configuration

- **Email Capabilities**
  - Full async/await support with `ConfigureAwait(false)`
  - Comprehensive `CancellationToken` support
  - HTML and plain text email support
  - CC and BCC recipients
  - Structured logging for auditing

- **Advanced Attachments**
  - `EmailAttachment` class for easy attachment handling
  - Support for files, streams, and byte arrays
  - Automatic MIME type detection
  - Robust handling of multiple attachments

- **Security & Performance**
  - Automatic SMTP connection management
  - Resource cleanup and disposal
  - Comprehensive error handling
  - Optimized for async workloads

## 🚀 Getting Started

### Prerequisites

- .NET Standard 2.0+ or .NET 6.0+
- Visual Studio 2022 or VS Code with C# Dev Kit (recommended)
- SMTP server access (e.g., Gmail, Office 365, or your own SMTP server)

### Installation

```bash
dotnet add package DevKit.Email
```

## 🔧 Configuration

### 1. Configure in appsettings.json

```json
{
  "MailKitSetting": {
    "Server": "smtp.gmail.com",
    "Port": 587,
    "Account": "your-email@gmail.com",
    "Password": "your-app-password",
    "EnableSsl": true,
    "UseDefaultCredentials": false,
    "DisplayName": "Your Application Name",
    "Timeout": 30000,
    "RequireAuthentication": true
  }
}
```

### 2. Register Services

In your `Program.cs` or `Startup.cs`:

```csharp
// Using the default configuration from appsettings.json
services.AddEmailServices();

// Or configure options programmatically
services.AddEmailServices(options =>
{
    options.Server = "smtp.gmail.com";
    options.Port = 587;
    options.Account = "your-email@gmail.com";
    options.Password = "your-app-password";
    options.EnableSsl = true;
    options.DisplayName = "Your Application";
});
```

### **2. Registrar servicios en DI**
```csharp
// En Program.cs o Startup.cs
services.Configure<MailKitSetting>(configuration.GetSection("MailKitSetting"));
services.AddScoped<IEmailSender, SmtpEmailSender>();
services.AddLogging();
```

## Usage Examples

### 1. Basic Email Sending

```csharp
public class EmailService
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IEmailSender emailSender, ILogger<EmailService> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task SendWelcomeEmailAsync(string email, string userName, CancellationToken cancellationToken = default)
    {
        var emailMessage = new EmailMessage
        {
            To = new List<MailboxAddress> { new(userName, email) },
            Subject = "Welcome to Our Service!",
            Body = $"""
                <h1>Welcome, {userName}!</h1>
                <p>Thank you for joining our service. We're excited to have you on board.</p>
                <p>Your account has been successfully created with the email: <strong>{email}</strong></p>
                <p>If you have any questions, please don't hesitate to contact our support team.</p>
                <p>Best regards,<br>The Team</p>
            """,
            IsBodyHtml = true
        };

        try
        {
            await _emailSender.SendEmailAsync(emailMessage, cancellationToken);
            _logger.LogInformation("Welcome email sent successfully to {Email}", email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", email);
            throw;
        }
    }
}
```

### 2. Sending Emails with Attachments

```csharp
public async Task SendInvoiceAsync(
    string customerEmail,
    string customerName,
    string invoiceNumber,
    byte[] invoicePdf,
    CancellationToken cancellationToken = default)
{
    var message = new EmailMessage
    {
        To = new List<MailboxAddress> { new(customerName, customerEmail) },
        Subject = $"Invoice #{invoiceNumber}",
        Body = $"""
            <h1>Invoice #{invoiceNumber}</h1>
            <p>Dear {customerName},</p>
            <p>Please find attached your invoice #{invoiceNumber}.</p>
            <p>Thank you for your business!</p>
        """,
        IsBodyHtml = true
    };

    // Add PDF attachment
    var attachment = new EmailAttachment($"Invoice-{invoiceNumber}.pdf", invoicePdf);
    message.Attachments.Add(attachment);

    // Add terms and conditions
    var termsPath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "TermsAndConditions.pdf");
    if (File.Exists(termsPath))
    {
        message.Attachments.Add(new EmailAttachment("TermsAndConditions.pdf", termsPath));
    }

    await _emailSender.SendEmailAsync(message, cancellationToken);
}
```

### 3. Using Templates with Model Binding

```csharp
public class EmailTemplateService
{
    private readonly IEmailSender _emailSender;
    private readonly IWebHostEnvironment _env;

    public EmailTemplateService(IEmailSender emailSender, IWebHostEnvironment env)
    {
        _emailSender = emailSender;
        _env = env;
    }

    public async Task SendPasswordResetEmailAsync(
        string email,
        string userName,
        string resetToken,
        CancellationToken cancellationToken = default)
    {
        var templatePath = Path.Combine(_env.ContentRootPath, "Templates", "PasswordReset.html");
        var template = await File.ReadAllTextAsync(templatePath, cancellationToken);
        
        var resetLink = $"https://yourapp.com/reset-password?token={resetToken}&email={WebUtility.UrlEncode(email)}";
        
        var body = template
            .Replace("{{UserName}}", userName)
            .Replace("{{ResetLink}}", resetLink)
            .Replace("{{ExpirationHours}}", "24");

        var message = new EmailMessage
        {
            To = new List<MailboxAddress> { new(userName, email) },
            Subject = "Password Reset Request",
            Body = body,
            IsBodyHtml = true
        };

        await _emailSender.SendEmailAsync(message, cancellationToken);
    }
}
```

### 4. Bulk Email with Error Handling and Progress Reporting

```csharp
public class BulkEmailService
{
    private readonly IEmailSender _emailSender;
    private readonly ILogger<BulkEmailService> _logger;

    public BulkEmailService(IEmailSender emailSender, ILogger<BulkEmailService> logger)
    {
        _emailSender = emailSender;
        _logger = logger;
    }

    public async Task<BulkEmailResult> SendBulkEmailAsync(
        IEnumerable<Recipient> recipients,
        string subject,
        string body,
        bool isBodyHtml = true,
        IProgress<BulkEmailProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var result = new BulkEmailResult
        {
            TotalRecipients = recipients.Count(),
            StartTime = DateTime.UtcNow
        };

        var options = new ParallelOptions
        {
            MaxDegreeOfParallelism = 3, // Limit concurrent emails to avoid overwhelming the SMTP server
            CancellationToken = cancellationToken
        };

        try
        {
            await Parallel.ForEachAsync(recipients, options, async (recipient, ct) =>
            {
                try
                {
                    var message = new EmailMessage
                    {
                        To = new List<MailboxAddress> { new(recipient.Name, recipient.Email) },
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = isBodyHtml
                    };

                    await _emailSender.SendEmailAsync(message, ct);
                    
                    Interlocked.Increment(ref result.SuccessCount);
                    _logger.LogInformation("Successfully sent email to {Email}", recipient.Email);
                }
                catch (Exception ex)
                {
                    Interlocked.Increment(ref result.FailedCount);
                    result.Errors.Add((recipient.Email, ex.Message));
                    _logger.LogError(ex, "Failed to send email to {Email}", recipient.Email);
                }
                finally
                {
                    var processed = Interlocked.Increment(ref result.ProcessedCount);
                    progress?.Report(new BulkEmailProgress
                    {
                        Processed = processed,
                        Total = result.TotalRecipients,
                        Successful = result.SuccessCount,
                        Failed = result.FailedCount
                    });
                }
            });
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Bulk email operation was canceled");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during bulk email sending");
            throw;
        }
        finally
        {
            result.EndTime = DateTime.UtcNow;
            result.Duration = result.EndTime - result.StartTime;
        }

        return result;
    }
}

public record Recipient(string Email, string Name);

public class BulkEmailResult
{
    public int TotalRecipients { get; set; }
    public int ProcessedCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailedCount { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public List<(string Email, string Error)> Errors { get; } = new();
}

public class BulkEmailProgress
{
    public int Processed { get; set; }
    public int Total { get; set; }
    public int Successful { get; set; }
    public int Failed { get; set; }
    public double Percentage => Total > 0 ? (double)Processed / Total * 100 : 0;
}

## API Reference

### Interfaces

#### `IEmailSender`
Main interface for sending emails.

**Methods:**
- `Task SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)`

### Models

#### `EmailMessage`
Represents an email message.

**Properties:**
- `List<MailboxAddress> To` - Recipients
- `List<MailboxAddress> Cc` - CC recipients
- `List<MailboxAddress> Bcc` - BCC recipients
- `string Subject` - Email subject
- `string Body` - Message body
- `bool IsBodyHtml` - Whether the body is HTML
- `List<EmailAttachment> Attachments` - Email attachments
- `MailboxAddress? From` - Sender (optional, uses default from config if not set)
- `MailboxAddress? ReplyTo` - Reply-to address (optional)

#### `EmailAttachment`
Represents an email attachment.

**Constructors:**
- `EmailAttachment(string fileName, string filePath)` - Create from file path
- `EmailAttachment(string fileName, byte[] fileContent)` - Create from byte array
- `EmailAttachment(string fileName, Stream fileStream)` - Create from stream

**Properties:**
- `string FileName` - Name of the file
- `string? ContentId` - Content ID for inline images (e.g., `<img src="cid:content-id">`)
- `string? ContentType` - MIME type (auto-detected if not specified)
- `Dictionary<string, string> Headers` - Custom headers for the attachment

### Configuration

#### `MailKitSetting`
Configuration options for the email service.

**Properties:**
- `string Server` - SMTP server address (required)
- `int Port` - SMTP port (default: 25 for non-SSL, 465 for SSL, 587 for StartTLS)
- `string Account` - SMTP username (required for authenticated SMTP)
- `string Password` - SMTP password (required for authenticated SMTP)
- `bool EnableSsl` - Whether to enable SSL/TLS (default: true)
- `string? DisplayName` - Display name for the sender (default: Account email)
- `int Timeout` - Connection timeout in milliseconds (default: 30000)
- `bool UseDefaultCredentials` - Whether to use default network credentials (default: false)
- `bool RequireAuthentication` - Whether authentication is required (default: true)
- `string? PickupDirectory` - If set, emails will be saved to this directory instead of being sent (for development)

## Security Considerations

1. **Credentials Security**
   - Never hardcode credentials in source code
   - Use environment variables or a secure secret manager in production
   - Consider using Azure Key Vault or AWS Secrets Manager for production environments

2. **TLS Configuration**
   - Always use SSL/TLS for production environments
   - Consider implementing certificate pinning for additional security
   - Keep MailKit and .NET runtime updated for the latest security patches

3. **Rate Limiting**
   - Implement rate limiting to prevent abuse
   - Consider using a dedicated email service (SendGrid, Amazon SES, etc.) for high-volume sending

## Performance Tips

1. **Connection Pooling**
   - The `IEmailSender` is designed to be registered as a singleton
   - It manages SMTP connection pooling internally for optimal performance

2. **Bulk Operations**
   - Use the bulk email pattern shown above for sending to multiple recipients
   - Tune the `MaxDegreeOfParallelism` based on your SMTP server's capabilities

3. **Async All the Way**
   - Always use async/await when calling `SendEmailAsync`
   - Avoid blocking on async calls with `.Result` or `.Wait()`

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please read our [contributing guidelines](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

## Support

For support, please open an issue in our [issue tracker](https://github.com/davidvazquezpalestino/DeveloperKit/issues).

---

<div align="center">
  Made with ❤️ by the DeveloperKit Team
</div>
