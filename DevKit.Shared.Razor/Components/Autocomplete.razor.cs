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

    private T ValueField;
    private bool ValueFromExternalSourceField;
    private bool IsSettingValueInternallyField;

    /// <summary>
    /// Valor seleccionado en el componente.
    /// </summary>
    [Parameter]
    public T Value
    {
        get => ValueField;
        set
        {
            if (!EqualityComparer<T>.Default.Equals(ValueField, value))
            {
                ValueField = value;
                ValueFromExternalSourceField = true;
            }
        }
    }

    /// <summary>
    /// Evento que se dispara cuando el valor cambia.
    /// </summary>
    [Parameter] public EventCallback<T> ValueChanged { get; set; }

    /// <summary>
    /// Comparador personalizado para determinar igualdad de objetos.
    /// </summary>
    [Parameter] public IEqualityComparer<T> Comparer { get; set; } = EqualityComparer<T>.Default;

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

    private string SearchTextField = string.Empty;
    private List<T> ItemsField = new();
    private bool IsDropdownVisibleField;
    private int SelectedIndexField = -1;
    private System.Timers.Timer DebounceTimerField;
    private CancellationTokenSource CancellationTokenSourceField;
    private bool DisposedField;

    private string SearchText
    {
        get => SearchTextField;
        set
        {
            if (SearchTextField != value)
            {
                SearchTextField = value;
                SelectedIndexField = -1;
            }
        }
    }

    private IReadOnlyList<T> Items => ItemsField;

    private bool IsDropdownVisible
    {
        get => IsDropdownVisibleField;
        set
        {
            if (IsDropdownVisibleField != value)
            {
                IsDropdownVisibleField = value;
                if (!IsDropdownVisibleField)
                    SelectedIndexField = -1;
            }
        }
    }

    /// <summary>
    /// Inicializa el componente y configura el temporizador para el debounce de búsqueda.
    /// Este método se ejecuta una vez durante la inicialización del componente.
    /// </summary>
    /// <remarks>
    /// Configura un temporizador que se activa después del intervalo de debounce especificado
    /// para evitar realizar búsquedas con cada pulsación de tecla, mejorando el rendimiento.
    /// </remarks>
    protected override void OnInitialized()
    {
        DebounceTimerField = new System.Timers.Timer(DebounceInterval)
        {
            AutoReset = false
        };
        DebounceTimerField.Elapsed += DebounceElapsed;
    }

    /// <summary>
    /// Maneja la actualización de parámetros del componente, sincronizando el valor externo
    /// con el estado interno del autocomplete.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Este método se ejecuta cuando los parámetros del componente cambian, detectando
    /// específicamente cuando el valor (Value) ha sido establecido desde una fuente externa
    /// (como una base de datos) y actualizando el texto de búsqueda en consecuencia.
    /// </para>
    /// <para>
    /// Utiliza flags de control (ValueFromExternalSourceField, IsSettingValueInternallyField)
    /// para determinar el origen de los cambios y evitar bucles de actualización.
    /// </para>
    /// </remarks>
    protected override async Task OnParametersSetAsync()
    {
        if (ValueFromExternalSourceField && !IsSettingValueInternallyField)
        {
            ValueFromExternalSourceField = false;
            SearchTextField = Value != null ? DisplayItem(Value) : string.Empty;
            await UpdateItemsFromValue();
        }
    }

    private async Task UpdateItemsFromValue()
    {
        if (Value == null)
        {
            ItemsField.Clear();
            IsDropdownVisible = false;
            return;
        }

        if (SearchHandler != null)
        {
            try
            {
                IEnumerable<T> results = await SearchHandler(DisplayItem(Value), CancellationToken.None);
                ItemsField = results?.ToList() ?? new List<T>();

                // Ensure the current value is in the list
                if (Value != null && !ItemsField.Any(item => Comparer.Equals(item, Value)))
                {
                    ItemsField.Add(Value);
                }
            }
            catch
            {
                ItemsField = new List<T> { Value };
            }
        }
        else
        {
            ItemsField = new List<T> { Value };
        }
    }

    private async Task HandleInput(ChangeEventArgs e)
    {
        SearchText = e.Value?.ToString() ?? string.Empty;
        SelectedIndexField = -1;

        if (string.IsNullOrEmpty(SearchText) && ResetValueOnEmptyText)
        {
            await UpdateValueAsync(default);
            ItemsField.Clear();
            IsDropdownVisible = false;
        }
        else
        {
            // Reinicia el debounce
            DebounceTimerField.Stop();
            DebounceTimerField.Start();
        }
    }

    private async void DebounceElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        if (DisposedField)
        {
            return;
        }

        CancellationTokenSourceField?.Cancel();
        CancellationTokenSourceField = new CancellationTokenSource();

        await InvokeAsync(async () =>
        {
            await PerformSearch(CancellationTokenSourceField.Token);
        });
    }

    private async Task PerformSearch(CancellationToken token)
    {
        if (SearchHandler != null && SearchText.Length >= MinCharacters)
        {
            try
            {
                IEnumerable<T> results = await SearchHandler(SearchText, token);
                ItemsField = results?.ToList() ?? new List<T>();
            }
            catch (OperationCanceledException)
            {
                return;
            }
        }
        else
        {
            ItemsField.Clear();
        }

        SelectedIndexField = -1;
        IsDropdownVisibleField = ItemsField.Any();
        StateHasChanged();
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (!IsDropdownVisible || !Items.Any())
        {
            return;
        }

        if (e.Key == "ArrowDown")
        {
            SelectedIndexField = (SelectedIndexField + 1) % ItemsField.Count;
        }
        else if (e.Key == "ArrowUp")
        {
            SelectedIndexField = (SelectedIndexField - 1 + ItemsField.Count) % ItemsField.Count;
        }
        else if (e.Key == "Enter" && SelectedIndexField >= 0)
        {
            _ = SelectOption(ItemsField[SelectedIndexField]);
        }
    }

    private void ShowSuggestions()
    {
        if (SearchText.Length >= MinCharacters && Items.Any())
        {
            IsDropdownVisible = true;
        }
    }

    private async Task HideSuggestionsWithDelay()
    {
        await Task.Delay(200);
        if (!DisposedField)
        {
            IsDropdownVisible = false;
        }
    }

    private async Task SelectOption(T option)
    {
        SearchText = DisplayItem(option);
        ItemsField.Clear();
        IsDropdownVisible = false;
        SelectedIndexField = -1;

        await UpdateValueAsync(option);

        if (OnSelected.HasDelegate)
        {
            await OnSelected.InvokeAsync(option);
        }
    }

    private async Task ClearSearch()
    {
        SearchText = string.Empty;
        ItemsField.Clear();
        IsDropdownVisible = false;
        SelectedIndexField = -1;

        if (ResetValueOnEmptyText)
        {
            await UpdateValueAsync(default);
        }
    }

    private async Task UpdateValueAsync(T val)
    {
        if (Comparer.Equals(ValueField, val)) return;

        IsSettingValueInternallyField = true;
        try
        {
            ValueField = val;
            SearchTextField = val != null ? DisplayItem(val) : string.Empty;
            await ValueChanged.InvokeAsync(val);

            if (OnSelected.HasDelegate)
            {
                await OnSelected.InvokeAsync(val);
            }
        }
        finally
        {
            IsSettingValueInternallyField = false;
        }
    }

    /// <summary>
    /// Libera los recursos administrados utilizados por la instancia.
    /// </summary>
    public void Dispose()
    {
        if (!DisposedField)
        {
            DisposedField = true;
            DebounceTimerField?.Stop();
            DebounceTimerField?.Dispose();
            CancellationTokenSourceField?.Cancel();
            CancellationTokenSourceField?.Dispose();
        }
    }
}