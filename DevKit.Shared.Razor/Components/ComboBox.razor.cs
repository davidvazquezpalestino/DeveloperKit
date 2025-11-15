namespace DevKit.Shared.Razor.Components;

/// <summary>
/// Componente de selección simple genérico para Blazor.
/// Permite seleccionar un valor tipado de una lista de opciones.
/// </summary>
/// <typeparam name="TItem">Tipo de los elementos en la lista.</typeparam>
/// <typeparam name="TValue">Tipo del valor seleccionado.</typeparam>
public partial class ComboBox<TItem, TValue>
{
    private string SelectedKey;

    /// <summary>
    /// Clases CSS personalizadas para el campo de entrada.
    /// Clase por defecto: 'form-select'.
    /// </summary>
    [Parameter] public string Class { get; set; } = "form-select";

    /// <summary>
    /// Valor seleccionado en el ComboBox.
    /// </summary>
    [Parameter]
    public TValue Value { get; set; }

    /// <summary>
    /// Evento que se dispara cuando el valor cambia.
    /// </summary>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>
    /// Lista de elementos disponibles para seleccionar.
    /// </summary>
    [Parameter] public IEnumerable<TItem> Items { get; set; }

    /// <summary>
    /// Texto a mostrar cuando no hay selección.
    /// </summary>
    [Parameter] public string Placeholder { get; set; }

    /// <summary>
    /// Nombre de la propiedad que se mostrará en el ComboBox.
    /// </summary>
    [Parameter] public string TextField { get; set; } = "Descripcion";

    /// <summary>
    /// Nombre de la propiedad que se usará como valor.
    /// </summary>
    [Parameter] public string ValueField { get; set; } = "Id";

    /// <summary>
    /// Selector opcional para obtener el texto a mostrar.
    /// Si no se establece, se usa reflexión con TextField.
    /// </summary>
    [Parameter] public Func<TItem, string> TextSelector { get; set; }

    /// <summary>
    /// Selector opcional para obtener el valor tipado del item.
    /// Si no se establece, se usa reflexión con ValueField.
    /// </summary>
    [Parameter] public Func<TItem, TValue> ValueSelector { get; set; }

    /// <summary>
    /// Sincroniza la clave seleccionada del <select> a partir del valor tipado
    /// para asegurar que la opción correcta aparezca marcada en el render.</select>
    /// </summary>
    protected override void OnParametersSet()
    {
        // Sincroniza SelectedKey a partir de Value para que el <select> muestre el valor correcto
        if (Items != null)
        {
            // Si Value es default/null, clave vacía para que el placeholder pueda mostrarse
            if (EqualityComparer<TValue>.Default.Equals(Value, default))
            {
                SelectedKey = string.Empty;
            }
            else
            {
                // Busca el item cuyo valor coincide con Value y toma su clave
                TItem item = Items.FirstOrDefault(i => EqualityComparer<TValue>.Default.Equals(GetItemValue(i), Value));
                SelectedKey = item is null ? string.Empty : GetItemKey(item);
            }
        }
    }

    /// <summary>
    /// Obtiene el texto a mostrar para un elemento.
    /// Usa <see cref="TextSelector"/> si está definido; de lo contrario, reflexión con <see cref="TextField"/>.
    /// </summary>
    /// <param name="item">Elemento origen.</param>
    /// <returns>Texto representativo del elemento.</returns>
    private string GetDisplayText(TItem item)
    {
        if (item != null)
        {
            if (TextSelector != null)
            {
                return TextSelector(item) ?? string.Empty;
            }

            PropertyInfo prop = typeof(TItem).GetProperty(TextField);
            return prop?.GetValue(item)?.ToString() ?? item.ToString();
        }

        return string.Empty;
    }
    /// <summary>
    /// Obtiene el valor tipado de un elemento.
    /// Usa <see cref="ValueSelector"/> si está definido; de lo contrario, reflexión con <see cref="ValueField"/>.
    /// Incluye intentos de conversión a <typeparamref name="TValue"/> cuando es posible.
    /// </summary>
    /// <param name="item">Elemento origen.</param>
    /// <returns>Valor del elemento en el tipo <typeparamref name="TValue"/>.</returns>
    private TValue GetItemValue(TItem item)
    {
        if (item == null)
        {
            return default;
        }

        if (ValueSelector != null)
        {
            return ValueSelector(item);
        }

        if (!string.IsNullOrEmpty(ValueField))
        {
            PropertyInfo prop = typeof(TItem).GetProperty(ValueField);
            if (prop != null)
            {
                object obj = prop.GetValue(item);
                if (obj is TValue tv)
                {
                    return tv;
                }
                if (obj == null)
                {
                    return default;
                }
                // Intentar convertir
                try
                {
                    return (TValue)Convert.ChangeType(obj, typeof(TValue));
                }
                catch
                {
                    // Fallback si no se puede convertir
                    return default;
                }
            }
        }

        return default;
    }

    /// <summary>
    /// Obtiene la clave string que se utilizará en el atributo value del option.
    /// </summary>
    /// <param name="item">Elemento origen.</param>
    /// <returns>Cadena representando la clave del elemento.</returns>
    private string GetItemKey(TItem item)
    {
        // Clave string para el atributo value de <option>
        TValue v = GetItemValue(item);
        return ConvertToKey(v);
    }

    /// <summary>
    /// Convierte un valor de tipo <typeparamref name="TValue"/> en su representación string para el DOM.
    /// </summary>
    /// <param name="value">Valor tipado.</param>
    /// <returns>Representación en texto (o cadena vacía si es null/default).</returns>
    private string ConvertToKey(TValue value) => value?.ToString() ?? string.Empty;

    /// <summary>
    /// Maneja el evento de cambio del <select/>.
    /// Resuelve la clave seleccionada a un elemento y obtiene su <typeparamref name="TValue"/> para propagar el cambio.
    /// </summary>
    /// <param name="e">Argumentos del evento de cambio.</param>
    private async Task HandleChange(ChangeEventArgs e)
    {
        SelectedKey = e.Value?.ToString();

        TValue newValue = default;
        if (Items != null)
        {
            TItem match = Items.FirstOrDefault(i => GetItemKey(i) == SelectedKey);
            if (match != null)
            {
                newValue = GetItemValue(match);
            }
        }

        Value = newValue;
        await ValueChanged.InvokeAsync(Value);
    }
}