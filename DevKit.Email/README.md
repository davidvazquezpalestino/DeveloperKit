# CoreMailKit

Una biblioteca especializada para el envío de correos electrónicos en .NET utilizando MailKit, con soporte para múltiples destinatarios, copias, adjuntos y configuración flexible.

## Características Principales

1. **Configuración Flexible**
   - Configuración de SMTP
   - Soporte para SSL/TLS
   - Manejo de credenciales
   - Configuración de puertos

2. **Destinatarios**
   - Soporte para múltiples destinatarios
   - Copias (CC)
   - Copias ocultas (BCC)
   - Manejo de listas de correo

3. **Adjuntos**
   - Soporte para múltiples adjuntos
   - Manejo de archivos
   - Validación de tipos de archivo

4. **Características de Email**
   - Soporte para HTML
   - Manejo de codificación
   - Limpieza automática de recursos
   - Manejo de excepciones

## Instalación

El paquete se puede instalar usando NuGet:

```bash
dotnet add package DotNet.CoreMailKit
```

También está disponible en el Visual Studio Package Manager:

```bash
Install-Package DotNet.CoreMailKit
```

## Uso Básico

```csharp
// Configuración del servicio de correo
MailKitSetting mailKitSetting = new MailKitSetting
{
    Servidor = "smtp.example.com",
    Puerto = 587,
    Cuenta = "usuario@example.com",
    Contrasena = "password",
    HabilitarSSL = true
};

// Crear instancia del servicio
EmailService emailService = new EmailService();

// Configurar el servicio
emailService.SetMailKitSetting(mailKitSetting);

// Agregar destinatarios
emailService.AddRecipient("destinatario@example.com");
emailService.AddRecipientWithCopy("copia@example.com");
emailService.AddRecipientWithCopyBlind("copia-oculta@example.com");

// Agregar adjuntos
emailService.Attachments(collection =>
{
    collection.Add(new Attachment("archivo.pdf"));
});

// Enviar correo
bool enviado = emailService.Send(
    subject: "Asunto del correo",
    body: "<h1>Mensaje en HTML</h1><p>Contenido del correo</p>"
);

// El servicio se limpia automáticamente después de cada envío
```
