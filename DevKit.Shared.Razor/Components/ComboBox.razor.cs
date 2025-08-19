using System.Reflection;

namespace DevKit.Shared.Razor.Components;

/// <summary>
/// Componente de selección múltiple genérico para Blazor.
/// Permite seleccionar un valor de una lista de opciones.
/// </summary>
/// <typeparam name="T">Tipo de los elementos en la lista.</typeparam>
public partial class ComboBox<T>
{
    private string CurrentValue;

    /// <summary>
    /// Valor seleccionado en el ComboBox.
    /// </summary>
    [Parameter]
    public string Value
    {
        get => CurrentValue;
        set
        {
            if (CurrentValue != value)
            {
                CurrentValue = value;
                ValueChanged.InvokeAsync(value);
            }
        }
    }

    /// <summary>
    /// Evento que se dispara cuando el valor cambia.
    /// </summary>
    [Parameter]
    public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// Lista de elementos disponibles para seleccionar.
    /// </summary>
    [Parameter] public IEnumerable<T> Items { get; set; }

    /// <summary>
    /// Texto a mostrar cuando no hay selección.
    /// </summary>
    [Parameter] public string Placeholder { get; set; } = "";

    /// <summary>
    /// Nombre de la propiedad que se mostrará en el ComboBox.
    /// </summary>
    [Parameter] public string TextField { get; set; } = "Descripcion";

    /// <summary>
    /// Nombre de la propiedad que se usará como valor.
    /// </summary>
    [Parameter] public string ValueField { get; set; } = "Moneda";

    /// <summary>
    /// Establece el valor inicial del ComboBox.
    /// </summary>
    protected override void OnParametersSet()
    {
        if (string.IsNullOrEmpty(CurrentValue) && !string.IsNullOrEmpty(Value))
        {
            CurrentValue = Value;
        }
        base.OnParametersSet();
    }

    private string GetDisplayText(T item)
    {
        if (item == null) return string.Empty;

        PropertyInfo prop = typeof(T).GetProperty(TextField);
        return prop?.GetValue(item)?.ToString() ?? item.ToString();
    }

    private string GetItemValue(T item)
    {
        if (item == null) return string.Empty;

        PropertyInfo prop = typeof(T).GetProperty(ValueField);
        return prop?.GetValue(item)?.ToString() ?? string.Empty;
    }
    private async Task HandleChange(ChangeEventArgs e)
    {
        Value = e.Value?.ToString();
        await ValueChanged.InvokeAsync(Value);
    }
}