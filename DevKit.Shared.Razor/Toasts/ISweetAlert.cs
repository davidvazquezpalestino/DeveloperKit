namespace DevKit.Shared.Razor.Toasts;

/// <summary>
/// Interfaz para el servicio de SweetAlert2
/// </summary>
public interface ISweetAlert
{
    /// <summary>
    /// Muestra una alerta básica
    /// </summary>
    Task ShowAlertAsync(string message, SweetAlertIcon icon = SweetAlertIcon.None);

    /// <summary>
    /// Muestra un diálogo de confirmación
    /// </summary>
    Task<bool> ShowConfirmAsync(string message, string confirmButtonText = "Aceptar", string cancelButtonText = "Cancelar", SweetAlertIcon icon = SweetAlertIcon.Question);

    /// <summary>
    /// Muestra una notificación toast
    /// </summary>
    Task ShowToastAsync(string message, SweetAlertIcon icon = SweetAlertIcon.Success, int duration = 3000);

    /// <summary>
    /// Muestra una alerta de éxito
    /// </summary>
    Task ShowSuccessAsync(string message, bool asToast = false);

    /// <summary>
    /// Muestra una alerta de error
    /// </summary>
    Task ShowErrorAsync(string message, bool asToast = false);

    /// <summary>
    /// Muestra una alerta de advertencia
    /// </summary>
    Task ShowWarningAsync(string message, bool asToast = true);

    /// <summary>
    /// Muestra una alerta informativa
    /// </summary>
    Task ShowInfoAsync(string message, bool asToast = true);
}