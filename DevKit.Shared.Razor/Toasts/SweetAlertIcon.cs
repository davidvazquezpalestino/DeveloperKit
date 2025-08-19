namespace DevKit.Shared.Razor.Toasts;

/// <summary>
/// Tipos de iconos disponibles para las alertas.
/// </summary>
public enum SweetAlertIcon
{
    /// <summary>
    /// Sin icono. No se muestra ningún icono en la alerta.
    /// </summary>
    None,

    /// <summary>
    /// Éxito. Indica que la operación se realizó correctamente.
    /// </summary>
    Success,

    /// <summary>
    /// Error. Indica que ocurrió un problema o fallo.
    /// </summary>
    Error,

    /// <summary>
    /// Advertencia. Señala una situación que requiere precaución.
    /// </summary>
    Warning,

    /// <summary>
    /// Información. Proporciona datos o detalles relevantes al usuario.
    /// </summary>
    Info,

    /// <summary>
    /// Pregunta. Solicita una acción o respuesta del usuario.
    /// </summary>
    Question
}