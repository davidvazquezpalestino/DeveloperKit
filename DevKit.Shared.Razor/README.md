# DevKit.Shared.Razor

Biblioteca de componentes Razor reutilizables para aplicaciones Blazor, optimizada para facilitar el desarrollo de interfaces modernas.

## Instalación

El paquete se puede instalar usando NuGet:

```bash
dotnet add package DevKit.Shared.RazorComponent
```

También está disponible en el Visual Studio Package Manager:

```bash
Install-Package DevKit.Shared.RazorComponent
```

## Uso de Componentes

### 1. Autocomplete<T>

Componente genérico para búsqueda y selección con soporte para debounce asíncrono.

```razor
<Autocomplete @bind-Value="selectedItem" 
               SearchHandler="SearchHandler" 
               DisplayItem="@(item => item.Name)" 
               MinCharacters="2" 
               DebounceInterval="300" 
               OnSelected="OnItemSelected" />

@code {
    private Item selectedItem;

    private async Task<List<Item>> SearchHandler(string searchTerm, CancellationToken token)
    {
        return await _service.SearchItems(searchTerm, token);
    }

    private void OnItemSelected(Item item)
    {
        // Manejar selección
    }
}
```

#### Parámetros Principales
- `SearchHandler`: Función asíncrona que maneja la búsqueda de elementos.
- `DisplayItem`: Selector para definir el texto a mostrar por item.
- `MinCharacters`: Mínimo de caracteres para iniciar la búsqueda.
- `DebounceInterval`: Tiempo de espera (ms) antes de ejecutar la búsqueda.
- `Clearable`: Permite mostrar el botón de limpieza.
- `Class`: Clases CSS personalizadas para el input.

### 2. ComboBox<TItem, TValue>

Componente de selección simple con soporte para tipos genéricos y reflexión opcional.

```razor
<ComboBox @bind-Value="selectedValue" 
          Items="options" 
          Placeholder="Seleccione una opción..." 
          TextField="Descripcion"
          ValueField="Id" />

@code {
    private int selectedValue;
    private List<OptionItem> options = new List<OptionItem>();
}
```

#### Parámetros Principales
- `Items`: Lista de elementos disponibles.
- `TextField`: Nombre de la propiedad a mostrar (por defecto "Descripcion").
- `ValueField`: Nombre de la propiedad valor (por defecto "Id").
- `TextSelector` / `ValueSelector`: Selectores opcionales para evitar reflexión.
- `Placeholder`: Texto inicial del select.

## Personalización de Estilos

El componente Autocomplete permite sobrescribir las siguientes clases para adaptarse al diseño de tu aplicación:

```css
.autocomplete-container { /* Contenedor principal */ }
.autocomplete-container .dropdown-menu { /* Menú de sugerencias */ }
.input-icon { /* Ícono de búsqueda */ }
.dropdown-item.active { /* Ítem seleccionado mediante teclado */ }
```
