namespace DevKit.Shared.Razor.Components.Notifications;

/// <summary>
/// Representa una solicitud para mostrar un diálogo modal.
/// </summary>
public class DialogRequest
{
    /// <summary>Título del diálogo.</summary>
    public string Title { get; set; } = "Soluciones Tecnológicas para su negocio";

    /// <summary>Mensaje a mostrar en el cuerpo del diálogo.</summary>
    public string Message { get; set; }

    /// <summary>Icono a mostrar en el diálogo.</summary>
    public AppNotificationIcon Icon { get; set; }

    /// <summary>Indica si se debe mostrar el botón de cancelar.</summary>
    public bool ShowCancelButton { get; set; }

    /// <summary>Texto para el botón de confirmación.</summary>
    public string ConfirmButtonText { get; set; } = "Aceptar";

    /// <summary>Texto para el botón de cancelación.</summary>
    public string CancelButtonText { get; set; } = "Cancelar";

    /// <summary>TaskCompletionSource para manejar la respuesta del usuario de forma asíncrona.</summary>
    public TaskCompletionSource<bool> Tcs { get; set; }
}

/// <summary>
/// Representa una solicitud para mostrar una notificación tipo toast.
/// </summary>
public class ToastRequest
{
    /// <summary>Mensaje a mostrar en el toast.</summary>
    public string Message { get; set; }

    /// <summary>Icono a mostrar en el toast.</summary>
    public AppNotificationIcon Icon { get; set; }

    /// <summary>Duración en milisegundos antes de que el toast desaparezca automáticamente.</summary>
    public int Duration { get; set; }
}

/// <summary>
/// Servicio para mostrar mensajes y diálogos profesionales 100% nativos (Bootstrap 5).
/// </summary>
public class AppNotificationService : IAppNotification
{
    /// <summary>Evento que se dispara cuando se solicita mostrar un diálogo.</summary>
    public event Action<DialogRequest> OnShowDialog;

    /// <summary>Evento que se dispara cuando se solicita mostrar un toast.</summary>
    public event Action<ToastRequest> OnShowToast;

    /// <inheritdoc />
    public Task ShowAlertAsync(string message, AppNotificationIcon icon = AppNotificationIcon.None)
    {
        TaskCompletionSource<bool> tcs = new();
        OnShowDialog?.Invoke(new DialogRequest
        {
            Message = message,
            Icon = icon,
            ShowCancelButton = false,
            Tcs = tcs
        });
        return tcs.Task;
    }

    /// <inheritdoc />
    public Task<bool> ShowConfirmAsync(
        string message,
        string confirmButtonText = "Sí, continuar",
        string cancelButtonText = "No, cancelar",
        AppNotificationIcon icon = AppNotificationIcon.Question)
    {
        TaskCompletionSource<bool> tcs = new();
        OnShowDialog?.Invoke(new DialogRequest
        {
            Message = message,
            ConfirmButtonText = confirmButtonText,
            CancelButtonText = cancelButtonText,
            ShowCancelButton = true,
            Icon = icon,
            Tcs = tcs
        });
        return tcs.Task;
    }

    /// <inheritdoc />
    public Task ShowToastAsync(string message, AppNotificationIcon icon = AppNotificationIcon.Success, int duration = 3000)
    {
        OnShowToast?.Invoke(new ToastRequest
        {
            Message = message,
            Icon = icon,
            Duration = duration
        });
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task ShowSuccessAsync(string message)
    {
        await ShowAlertAsync(message, AppNotificationIcon.Success);
    }

    /// <inheritdoc />
    public async Task ShowSuccessToastAsync(string message, int duration = 3000)
    {
        await ShowToastAsync(message, AppNotificationIcon.Success, duration);
    }

    /// <inheritdoc />
    public async Task ShowErrorAsync(string message)
    {
        await ShowAlertAsync(message, AppNotificationIcon.Error);
    }

    /// <inheritdoc />
    public async Task ShowErrorToastAsync(string message, int duration = 3000)
    {
        await ShowToastAsync(message, AppNotificationIcon.Error, duration);
    }

    /// <inheritdoc />
    public async Task ShowWarningAsync(string message)
    {
        await ShowAlertAsync(message, AppNotificationIcon.Warning);
    }

    /// <inheritdoc />
    public async Task ShowWarningToastAsync(string message, int duration = 3000)
    {
        await ShowToastAsync(message, AppNotificationIcon.Warning, duration);
    }

    /// <inheritdoc />
    public async Task ShowInfoAsync(string message)
    {
        await ShowAlertAsync(message, AppNotificationIcon.Info);
    }

    /// <inheritdoc />
    public async Task ShowInfoToastAsync(string message, int duration = 3000)
    {
        await ShowToastAsync(message, AppNotificationIcon.Info, duration);
    }
}