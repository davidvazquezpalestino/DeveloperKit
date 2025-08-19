namespace DevKit.Shared.Razor.Components;

/// <summary>
/// Componente de autocompletado genérico para Blazor.
/// Permite buscar y seleccionar elementos de una lista filtrada.
/// </summary>
/// <typeparam name="T">Tipo de los elementos en la lista.</typeparam>
public partial class Autocomplete<T>
{
    /// <summary>
    /// Clases CSS personalizadas para el campo de entrada.
    /// Se combinarán con las clases por defecto: 'form-control pe-5'.
    /// </summary>
    [Parameter] public string Class { get; set; } = "form-control pe-5";

    /// <summary>
    /// Texto de placeholder para el campo de búsqueda.
    /// </summary>
    [Parameter] public string Placeholder { get; set; } = "Escribe aquí...";

    /// <summary>
    /// Función que maneja la búsqueda de elementos.
    /// </summary>
    [Parameter] public Func<string, CancellationToken, Task<IEnumerable<T>>> SearchHandler { get; set; }

    /// <summary>
    /// Función que define cómo se muestra cada elemento en la lista.
    /// </summary>
    [Parameter] public Func<T, string> DisplayItem { get; set; } = item => item?.ToString() ?? "";

    /// <summary>
    /// Número mínimo de caracteres para iniciar la búsqueda.
    /// </summary>
    [Parameter] public int MinCharacters { get; set; } = 1;

    /// <summary>
    /// Intervalo en milisegundos para debounce de la búsqueda.
    /// </summary>
    [Parameter] public int DebounceInterval { get; set; } = 300;

    /// <summary>
    /// Indica si se muestra un botón para limpiar la búsqueda.
    /// </summary>
    [Parameter] public bool Clearable { get; set; } = true;

    /// <summary>
    /// Indica si se reinicia el valor al limpiar el texto.
    /// </summary>
    [Parameter] public bool ResetValueOnEmptyText { get; set; } = true;

    // Two-way binding
    /// <summary>
    /// Valor seleccionado en el componente.
    /// </summary>
    [Parameter] public T Value { get; set; }

    /// <summary>
    /// Evento que se dispara cuando el valor cambia.
    /// </summary>
    [Parameter] public EventCallback<T> ValueChanged { get; set; }

    // Callback adicional
    /// <summary>
    /// Evento que se dispara cuando se selecciona un elemento.
    /// </summary>
    [Parameter] public EventCallback<T> OnSelected { get; set; }

    /// <summary>
    /// Diccionario de atributos adicionales para el elemento input.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> InputAttributes { get; set; } = new Dictionary<string, object>();

    private string SearchText { get; set; } = string.Empty;
    private IEnumerable<T> Items { get; set; } = [];
    private bool IsDropdownVisible { get; set; }
    private int SelectedIndex { get; set; } = -1;

    private System.Timers.Timer DebounceTimer;
    private CancellationTokenSource CancellationTokenSource;
    private bool Disposed;

    /// <summary>
    /// Inicializa el componente configurando el temporizador para el debounce.
    /// Este método se llama una vez cuando el componente es inicializado.
    /// Configura un temporizador que se utiliza para retrasar la búsqueda mientras el usuario escribe,
    /// mejorando el rendimiento al evitar múltiples búsquedas innecesarias.
    /// </summary>
    protected override void OnInitialized()
    {
        DebounceTimer = new System.Timers.Timer(DebounceInterval)
        {
            AutoReset = false
        };
        DebounceTimer.Elapsed += DebounceElapsed;
    }

    private async Task HandleInput(ChangeEventArgs e)
    {
        SearchText = e.Value?.ToString() ?? string.Empty;
        SelectedIndex = -1;

        if (string.IsNullOrEmpty(SearchText) && ResetValueOnEmptyText)
        {
            await UpdateValueAsync(default);
            Items = [];
            IsDropdownVisible = false;
        }
        else
        {
            // Reinicia el debounce
            DebounceTimer.Stop();
            DebounceTimer.Start();
        }
    }

    private async void DebounceElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (Disposed) return;

        CancellationTokenSource?.Cancel();
        CancellationTokenSource = new CancellationTokenSource();

        await InvokeAsync(async () =>
        {
            await PerformSearch(CancellationTokenSource.Token);
        });
    }

    private async Task PerformSearch(CancellationToken token)
    {
        if (SearchHandler != null && SearchText.Length >= MinCharacters)
        {
            try
            {
                IEnumerable<T> results = await SearchHandler(SearchText, token);
                Items = results ?? [];
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
        else
        {
            Items = [];
        }

        SelectedIndex = -1;
        IsDropdownVisible = Items.Any();
        StateHasChanged();
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (!IsDropdownVisible || !Items.Any()) return;

        if (e.Key == "ArrowDown")
            SelectedIndex = (SelectedIndex + 1) % Items.Count();
        else if (e.Key == "ArrowUp")
            SelectedIndex = (SelectedIndex - 1 + Items.Count()) % Items.Count();
        else if (e.Key == "Enter" && SelectedIndex >= 0)
            _ = SelectOption(Items.ElementAt(SelectedIndex));
    }

    private void ShowSuggestions()
    {
        if (SearchText.Length >= MinCharacters && Items.Any())
            IsDropdownVisible = true;
    }

    private async Task HideSuggestionsWithDelay()
    {
        await Task.Delay(200);
        if (!Disposed)
            IsDropdownVisible = false;
    }

    private async Task SelectOption(T option)
    {
        SearchText = DisplayItem(option);
        Items = [];
        IsDropdownVisible = false;
        SelectedIndex = -1;

        await UpdateValueAsync(option);

        if (OnSelected.HasDelegate)
            await OnSelected.InvokeAsync(option);
    }

    private async Task ClearSearch()
    {
        SearchText = string.Empty;
        Items = [];
        IsDropdownVisible = false;
        SelectedIndex = -1;

        if (ResetValueOnEmptyText)
            await UpdateValueAsync(default);
    }

    private async Task UpdateValueAsync(T val)
    {
        Value = val;
        await ValueChanged.InvokeAsync(val);
    }
    /// <summary>
    /// Libera los recursos administrados utilizados por la instancia.
    /// </summary>
    public void Dispose()
    {
        Disposed = true;
        DebounceTimer?.Stop();
        DebounceTimer?.Dispose();
        CancellationTokenSource?.Cancel();
    }
}