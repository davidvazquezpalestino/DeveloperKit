# CoreControlesUsuario

Biblioteca de controles de usuario personalizados para aplicaciones Windows Forms.

## Características

- Controles de usuario reutilizables y personalizados
- Temas y estilos consistentes
- Manejo de recursos centralizado
- Soporte para internacionalización
- Integración con el resto del DeveloperKit
- Eventos personalizados
- Validaciones integradas
- Manejo de estados (habilitado/deshabilitado)

## Instalación

El componente se puede instalar como un paquete NuGet:

```bash
dotnet add package DeveloperKit.CoreControlesUsuario
```

## Requisitos

- .NET Framework 4.8
- Windows Forms
- Visual Studio 2019 o superior

## Controles Disponibles

### Controles Básicos

- CustomButton: Botón personalizado con estilos
- CustomTextBox: TextBox con validaciones
- CustomComboBox: ComboBox mejorado
- CustomDataGridView: DataGridView personalizado
- CustomTabControl: TabControl con temas

### Controles Especializados

- CustomSearchControl: Control de búsqueda avanzada
- CustomDateRangePicker: Selector de rango de fechas
- CustomFilePicker: Selector de archivos personalizado
- CustomProgressIndicator: Indicador de progreso
- CustomNotification: Sistema de notificaciones

## Uso

### Agregando Controles al Proyecto

1. Agrega una referencia al paquete NuGet
2. Importa los namespaces necesarios:

```csharp
using CoreControlesUsuario.UI;
using CoreControlesUsuario.Services;
```

### Ejemplo de Uso

```csharp
// 1. Agregar el control al formulario
private CustomButton _customButton;

public Form1()
{
    InitializeComponent();
    
    // Crear y configurar el botón
    _customButton = new CustomButton
    {
        Text = "Guardar",
        Width = 100,
        Height = 30,
        Theme = CustomButtonTheme.Primary,
        ValidationEnabled = true
    };
    
    // Agregar eventos
    _customButton.Click += CustomButton_Click;
    
    // Agregar al formulario
    Controls.Add(_customButton);
}

private void CustomButton_Click(object sender, EventArgs e)
{
    // Manejar el evento
    if (_customButton.IsValid)
    {
        // Lógica de negocio
    }
}
```

### Configuración de Temas

```csharp
// Configurar tema global
CustomThemeManager.SetTheme(CustomTheme.Light);

// Configurar tema específico
_customButton.Theme = CustomButtonTheme.Primary;
_customTextBox.Theme = CustomTextBoxTheme.Secondary;
```

### Manejo de Recursos

```csharp
// Acceder a recursos compartidos
var image = CustomResources.Images.Save;
var text = CustomResources.Strings.SaveButtonText;

// Manejar recursos de forma segura
try
{
    var resource = CustomResources.GetResource("ResourceKey");
}
catch (ResourceNotFoundException ex)
{
    // Manejo de recursos no encontrados
}
```

## Mejores Prácticas

1. Siempre validar los controles antes de procesar datos
2. Usar temas consistentes en toda la aplicación
3. Manejar eventos de manera centralizada
4. Utilizar recursos compartidos para consistencia
5. Implementar validaciones en los controles
6. Manejar estados de habilitado/deshabilitado
7. Usar nombres descriptivos para los controles

## Soporte

Para reportar errores o solicitar características, por favor abre un issue en el repositorio de GitHub.

## Licencia

Este proyecto está bajo licencia MIT. Consulta el archivo LICENSE para más detalles.
