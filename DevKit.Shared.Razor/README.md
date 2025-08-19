# DotNet.Shared.RazorComponent

Una biblioteca de componentes Razor compartidos para aplicaciones Blazor, que proporciona componentes reutilizables y personalizables para la interfaz de usuario.

## Componentes Principales

1. **Autocomplete<T>**
   - Componente de autocompletado genérico
   - Características:
     - Búsqueda asíncrona
     - Debounce para optimizar búsquedas
     - Mínimo de caracteres configurable
     - Personalización del formato de los items
     - Evento de selección
     - Manejo de estados

2. **ComboBox<T>**
   - Componente de selección múltiple genérico
   - Características:
     - Filtro en tiempo real
     - Selección de valor
     - Manejo de eventos
     - Personalización de ID y etiquetas
     - Mantenimiento de estado

## Características Comunes

- Tipos genéricos para flexibilidad
- Eventos y callbacks
- Manejo de estados
- Validación integrada
- Soporte para filtros
- Documentación completa

## Instalación

El paquete se puede instalar usando NuGet:

```bash
dotnet add package DotNet.Shared.RazorComponent
```

También está disponible en el Visual Studio Package Manager:

```bash
Install-Package DotNet.Shared.RazorComponent
```

## Uso Básico

### Autocomplete

```razor
@page "/autocomplete"

<Autocomplete @bind-Value="selectedItem" 
               SearchHandler="SearchHandler" 
               DisplayItem="@(item => item.Name)" 
               MinCharacters="2" 
               DebounceInterval="300" 
               OnSelected="OnItemSelected" />

@code {
    private Item selectedItem;

    private async Task<IEnumerable<Item>> SearchHandler(string searchTerm, CancellationToken token)
    {
        return await _service.SearchItems(searchTerm, token);
    }

    private void OnItemSelected(Item item)
    {
        // Manejar selección
    }
}
```

### ComboBox

```razor
@page "/combobox"

<ComboBox @bind-Value="selectedValue" 
          Items="options" 
          Label="Seleccione una opción" 
          Placeholder="Elegir..." 
          OnValueChanged="OnValueChanged" />

@code {
    private string selectedValue;
    private List<string> options = new List<string> { "Opción 1", "Opción 2", "Opción 3" };

    private async Task OnValueChanged(string value)
    {
        // Manejar cambio de valor
    }
}
```

## Parámetros de Autocomplete

- `SearchHandler`: Función que maneja la búsqueda
- `DisplayItem`: Función que formatea los items
- `MinCharacters`: Mínimo de caracteres para mostrar sugerencias
- `DebounceInterval`: Tiempo de espera antes de realizar la búsqueda
- `OnSelected`: Evento cuando se selecciona un item
- `Clearable`: Booleano que indica si se muestra el botón para limpiar la búsqueda (por defecto: true)
- `Placeholder`: Texto de marcador de posición para el campo de búsqueda
- `Class`: Clases CSS adicionales para personalizar el componente

## Personalización de Estilos

El componente Autocomplete incluye los siguientes estilos CSS que pueden ser sobrescritos:

```css
/* Contenedor principal */
.autocomplete-container {
    overflow: visible;
}

/* Menú desplegable */
.autocomplete-container .dropdown-menu {
    z-index: 3000 !important;
}

/* Ícono de búsqueda */
.input-icon {
    font-size: 1.2rem;
    pointer-events: none;
}

/* Botón de limpiar */
.btn-clear {
    background: transparent;
    border: none;
    font-size: 1.25rem;
    line-height: 1;
    color: #999;
    cursor: pointer;
}

/* Ítem activo en el menú desplegable */
.dropdown-item.active {
    background-color: #0d6efd;
    color: white;
}
```

Puedes sobrescribir cualquiera de estas clases en tu propio archivo CSS para personalizar la apariencia del componente.

## Parámetros de ComboBox

- `Items`: Lista de items disponibles
- `SelectedValue`: Valor seleccionado
- `Label`: Etiqueta del componente
- `Placeholder`: Texto de ayuda
- `OnValueChanged`: Evento cuando cambia el valor
