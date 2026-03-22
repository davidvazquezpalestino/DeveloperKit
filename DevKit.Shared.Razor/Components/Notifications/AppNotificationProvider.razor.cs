

namespace DevKit.Shared.Razor.Components.Notifications;

/// <summary>
/// Componente proveedor que renderiza los diálogos modales y notificaciones toast en la aplicación.
/// </summary>
public partial class AppNotificationProvider
{
    /// <summary>
    /// Diálogo actualmente visible. Si es null, no se muestra ningún diálogo.
    /// </summary>
    private DialogRequest CurrentDialog;

    /// <summary>
    /// Lista de notificaciones toast activas que se están mostrando en pantalla.
    /// </summary>
    private readonly List<ToastItem> ActiveToasts = new();

    /// <summary>
    /// Inicializa el componente y se suscribe a los eventos del servicio de notificaciones.
    /// </summary>
    protected override void OnInitialized()
    {
        if (AppNotification is AppNotificationService service)
        {
            service.OnShowDialog += HandleShowDialog;
            service.OnShowToast += HandleShowToast;
        }
    }

    /// <summary>
    /// Maneja la solicitud para mostrar un nuevo diálogo modal.
    /// </summary>
    /// <param name="request">La solicitud con los datos del diálogo.</param>
    private async void HandleShowDialog(DialogRequest request)
    {
        CurrentDialog = request;
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Maneja la solicitud para mostrar un nuevo toast.
    /// </summary>
    /// <param name="request">La solicitud con los datos del toast.</param>
    private async void HandleShowToast(ToastRequest request)
    {
        ToastItem toast = new ToastItem { Request = request };
        ActiveToasts.Add(toast);
        await InvokeAsync(StateHasChanged);

        System.Timers.Timer timer = new System.Timers.Timer(request.Duration);
        timer.Elapsed += async (s, e) =>
        {
            timer.Dispose();
            ActiveToasts.Remove(toast);
            await InvokeAsync(StateHasChanged);
        };
        timer.Start();
    }

    /// <summary>
    /// Confirma el diálogo actual, retornando true y cerrando el diálogo.
    /// </summary>
    private void ConfirmDialog()
    {
        TaskCompletionSource<bool> tcs = CurrentDialog?.Tcs;
        CurrentDialog = null;
        StateHasChanged();
        tcs?.TrySetResult(true);
    }

    /// <summary>
    /// Cancela el diálogo actual, retornando false y cerrando el diálogo.
    /// </summary>
    private void CancelDialog()
    {
        TaskCompletionSource<bool> tcs = CurrentDialog?.Tcs;
        CurrentDialog = null;
        StateHasChanged();
        tcs?.TrySetResult(false);
    }

    /// <summary>
    /// Cierra un toast específico de forma manual.
    /// </summary>
    /// <param name="toast">El elemento toast a cerrar.</param>
    private void CloseToast(ToastItem toast)
    {
        ActiveToasts.Remove(toast);
        StateHasChanged();
    }

    /// <summary>
    /// Libera los recursos del componente y se desuscribe de los eventos del servicio para evitar memory leaks.
    /// </summary>
    public void Dispose()
    {
        if (AppNotification is AppNotificationService service)
        {
            service.OnShowDialog -= HandleShowDialog;
            service.OnShowToast -= HandleShowToast;
        }
    }

    /// <summary>
    /// Representa de manera interna un elemento toast activo dentro del componente.
    /// </summary>
    private class ToastItem
    {
        /// <summary>Solicitud original del toast.</summary>
        public ToastRequest Request { get; set; }

        /// <summary>Mensaje del toast.</summary>
        public string Message => Request.Message;

        /// <summary>Icono del toast.</summary>
        public AppNotificationIcon Icon => Request.Icon;
    }

    private RenderFragment GetIconHtml(AppNotificationIcon icon) => builder =>
    {
        (string iconClass, string colorClass) = icon switch
        {
            AppNotificationIcon.Success => ("bi-check-circle", "text-success"),
            AppNotificationIcon.Error => ("bi-x-circle", "text-danger"),
            AppNotificationIcon.Warning => ("bi-exclamation-triangle", "text-warning"),
            AppNotificationIcon.Info => ("bi-info-circle", "text-info"),
            AppNotificationIcon.Question => ("bi-question-circle", "text-primary"),
            _ => ("bi-info-circle", "text-secondary")
        };
        builder.OpenElement(0, "i");
        builder.AddAttribute(1, "class", $"bi {iconClass} {colorClass}");
        builder.AddAttribute(2, "style", "font-size: 3.5rem; line-height: 1;");
        builder.CloseElement();
    };

    private string GetButtonClass(AppNotificationIcon icon) => icon switch
    {
        AppNotificationIcon.Error => "btn-danger",
        AppNotificationIcon.Warning => "btn-warning text-dark",
        AppNotificationIcon.Info => "btn-info text-dark",
        AppNotificationIcon.Question => "btn-primary",
        AppNotificationIcon.Success => "btn-success",
        _ => "btn-primary"
    };

    private string GetToastColor(AppNotificationIcon icon) => icon switch
    {
        AppNotificationIcon.Success => "success",
        AppNotificationIcon.Error => "danger",
        AppNotificationIcon.Warning => "warning",
        AppNotificationIcon.Info => "info",
        _ => "dark"
    };

    private RenderFragment GetToastIconHtml(AppNotificationIcon icon) => builder =>
    {
        string iconClass = icon switch
        {
            AppNotificationIcon.Success => "bi-check-circle-fill",
            AppNotificationIcon.Error => "bi-dash-circle-fill",
            AppNotificationIcon.Warning => "bi-exclamation-triangle-fill",
            AppNotificationIcon.Info => "bi-info-circle-fill",
            _ => "bi-bell-fill"
        };
        builder.OpenElement(0, "i");
        builder.AddAttribute(1, "class", $"bi {iconClass} me-2");
        builder.CloseElement();
    };
}