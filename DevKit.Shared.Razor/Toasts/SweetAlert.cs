using System.Text.Json;

namespace DevKit.Shared.Razor.Toasts;

/// <summary>
/// Servicio para mostrar alertas de SweetAlert2 en aplicaciones Blazor
/// </summary>
public class SweetAlert(IJSRuntime jsRuntime) : ISweetAlert
{
    private const string SweetAlert2 = "Swal";
    private bool IsInitialized;

    /// <summary>
    /// Muestra una alerta básica
    /// </summary>
    public async Task ShowAlertAsync(string message, SweetAlertIcon icon = SweetAlertIcon.None)
    {
        await EnsureInitializedAsync();
        await jsRuntime.InvokeVoidAsync(
            $"{SweetAlert2}.fire",
            new
            {
                title = "Soluciones Tecnológicas para su negocio",
                text = message,
                icon = icon != SweetAlertIcon.None ? icon.ToString().ToLower() : null
            });
    }

    /// <summary>
    /// Muestra un diálogo de confirmación
    /// </summary>
    public async Task<bool> ShowConfirmAsync(
        string message,
        string confirmButtonText = "Sí, continuar",
        string cancelButtonText = "No, cancelar",
        SweetAlertIcon icon = SweetAlertIcon.Question)
    {
        await EnsureInitializedAsync();
        JsonElement result = await jsRuntime.InvokeAsync<JsonElement>(
            $"{SweetAlert2}.fire",
            new
            {
                title = "Soluciones Tecnológicas para su negocio",
                text = message,
                icon = icon.ToString().ToLower(),
                showCancelButton = true,
                confirmButtonText = confirmButtonText,
                cancelButtonText = cancelButtonText,
                confirmButtonColor = "#3085d6",
                cancelButtonColor = "#d33"
            });

        // Check the result using JsonElement
        return result.TryGetProperty("isConfirmed", out JsonElement isConfirmed) &&
               isConfirmed.GetBoolean();
    }

    /// <summary>
    /// Muestra una notificación toast
    /// </summary>
    public async Task ShowToastAsync(string message, SweetAlertIcon icon = SweetAlertIcon.Success, int duration = 3000)
    {
        await EnsureInitializedAsync();
        await jsRuntime.InvokeVoidAsync(
            $"{SweetAlert2}.fire",
            new
            {
                toast = true,
                position = "top-end",
                showConfirmButton = false,
                timer = duration,
                title = "Soluciones Tecnológicas para su negocio",
                text = message,
                icon = icon != SweetAlertIcon.None ? icon.ToString().ToLower() : null
            });
    }

    /// <summary>
    /// Muestra una alerta de éxito
    /// </summary>
    public async Task ShowSuccessAsync(string message, bool asToast = false)
    {
        if (asToast)
        {
            await ShowToastAsync(message);
        }
        else
        {
            await jsRuntime.InvokeVoidAsync(
                $"{SweetAlert2}.fire",
                new
                {
                    title = "Soluciones Tecnológicas para su negocio",
                    text = message,
                    icon = nameof(SweetAlertIcon.Success).ToLower()
                });
        }
    }

    /// <summary>
    /// Muestra una alerta de error
    /// </summary>
    public async Task ShowErrorAsync(string message, bool asToast = false)
    {
        if (asToast)
        {
            await ShowToastAsync(message, SweetAlertIcon.Error);
        }
        else
        {
            await jsRuntime.InvokeVoidAsync(
                $"{SweetAlert2}.fire",
                new
                {
                    title = "Soluciones Tecnológicas para su negocio",
                    text = message,
                    icon = nameof(SweetAlertIcon.Error).ToLower()
                });
        }
    }

    /// <summary>
    /// Muestra una alerta de advertencia
    /// </summary>
    public async Task ShowWarningAsync(string message, bool asToast = true)
    {
        if (asToast)
        {
            await ShowToastAsync(message, SweetAlertIcon.Warning);
        }
        else
        {
            await jsRuntime.InvokeVoidAsync(
                $"{SweetAlert2}.fire",
                new
                {
                    title = "Soluciones Tecnológicas para su negocio",
                    text = message,
                    icon = nameof(SweetAlertIcon.Warning).ToLower()
                });
        }
    }

    /// <summary>
    /// Muestra una alerta informativa
    /// </summary>
    public async Task ShowInfoAsync(string message, bool asToast = true)
    {
        if (asToast)
        {
            await ShowToastAsync(message, SweetAlertIcon.Info);
        }
        else
        {
            await jsRuntime.InvokeVoidAsync(
                $"{SweetAlert2}.fire",
                new
                {
                    title = "Soluciones Tecnológicas para su negocio",
                    text = message,
                    icon = nameof(SweetAlertIcon.Info).ToLower()
                });
        }
    }

    private async ValueTask EnsureInitializedAsync()
    {
        if (IsInitialized == false)
        {
            try
            {
                // Verificar si SweetAlert2 está disponible
                await jsRuntime.InvokeAsync<object>("eval", "if (typeof Swal === 'undefined') throw new Error('SweetAlert2 no está cargado. Asegúrate de incluir el script de SweetAlert2 en tu _Host.cshtml o index.html');");
                IsInitialized = true;
            }
            catch (JSException ex) when (ex.Message.Contains("SweetAlert2"))
            {
                throw new InvalidOperationException("SweetAlert2 no está cargado. Asegúrate de incluir el script de SweetAlert2 en tu _Host.cshtml o index.html", ex);
            }
        }
    }
}