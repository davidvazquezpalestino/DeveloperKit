namespace DevKit.Shared.Razor.Components.Notifications;

/// <summary>
/// Tipos de iconos disponibles para las alertas.
/// </summary>
public enum AppNotificationIcon
{
    /// <summary>
    /// Sin icono. No se muestra ning�n icono en la alerta.
    /// </summary>
    None,

    /// <summary>
    /// �xito. Indica que la operaci�n se realiz� correctamente.
    /// </summary>
    Success,

    /// <summary>
    /// Error. Indica que ocurri� un problema o fallo.
    /// </summary>
    Error,

    /// <summary>
    /// Advertencia. Se�ala una situaci�n que requiere precauci�n.
    /// </summary>
    Warning,

    /// <summary>
    /// Informaci�n. Proporciona datos o detalles relevantes al usuario.
    /// </summary>
    Info,

    /// <summary>
    /// Pregunta. Solicita una acci�n o respuesta del usuario.
    /// </summary>
    Question
}