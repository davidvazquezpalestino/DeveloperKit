namespace DevKit.Shared.Razor.Components.Notifications;

/// <summary>
/// Interfaz para el servicio de AppNotificationService2
/// </summary>
public interface IAppNotification
{
    /// <summary>
    /// Muestra una alerta básica
    /// </summary>
    Task ShowAlertAsync(string message, AppNotificationIcon icon = AppNotificationIcon.None);

    /// <summary>
    /// Muestra un diálogo de confirmación
    /// </summary>
    Task<bool> ShowConfirmAsync(string message, string confirmButtonText = "Aceptar", string cancelButtonText = "Cancelar", AppNotificationIcon icon = AppNotificationIcon.Question);

    /// <summary>
    /// Muestra una notificación toast
    /// </summary>
    Task ShowToastAsync(string message, AppNotificationIcon icon = AppNotificationIcon.Success, int duration = 3000);

    /// <summary>
    /// Muestra una alerta de éxito
    /// </summary>
    Task ShowSuccessAsync(string message);

    /// <summary>
    /// Muestra una notificación toast de éxito
    /// </summary>
    Task ShowSuccessToastAsync(string message, int duration = 3000);

    /// <summary>
    /// Muestra una alerta de error
    /// </summary>
    Task ShowErrorAsync(string message);

    /// <summary>
    /// Muestra una notificación toast de error
    /// </summary>
    Task ShowErrorToastAsync(string message, int duration = 3000);

    /// <summary>
    /// Muestra una alerta de advertencia
    /// </summary>
    Task ShowWarningAsync(string message);

    /// <summary>
    /// Muestra una notificación toast de advertencia
    /// </summary>
    Task ShowWarningToastAsync(string message, int duration = 3000);

    /// <summary>
    /// Muestra una alerta informativa
    /// </summary>
    Task ShowInfoAsync(string message);

    /// <summary>
    /// Muestra una notificación toast informativa
    /// </summary>
    Task ShowInfoToastAsync(string message, int duration = 3000);
}