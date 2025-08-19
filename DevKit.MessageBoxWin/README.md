# CoreMessageBox

Una biblioteca robusta para mostrar mensajes de diálogo en aplicaciones Windows Forms, con soporte para múltiples tipos de mensajes y manejo consistente de errores.

## Características Principales

1. **Tipos de Mensajes**
   - Mensajes simples
   - Mensajes OK/Cancelar
   - Preguntas
   - Advertencias
   - Errores
   - Errores con excepciones

2. **Características de Mensajes**
   - Interfaz genérica para resultados
   - Manejo de estados
   - Soporte para resultados personalizados
   - Documentación integrada

3. **Manejo de Errores**
   - Visualización de excepciones
   - Formateo de mensajes de error
   - Manejo consistente de errores
   - Soporte para múltiples niveles de error

## Instalación

El paquete se puede instalar usando NuGet:

```bash
dotnet add package DotNet.CoreMessageBox
```

También está disponible en el Visual Studio Package Manager:

```bash
Install-Package DotNet.CoreMessageBox
```

## Uso Básico

```csharp
// Crear una instancia del MessageBox
var messageBox = new WindowsMessageBox();

// Mostrar un mensaje simple
messageBox.ShowMessage("Este es un mensaje simple");

// Mostrar un diálogo OK/Cancelar
var resultado = messageBox.ShowOkCancel("¿Desea continuar?");

// Mostrar una pregunta
resultado = messageBox.ShowQuestion("¿Está seguro?");

// Mostrar una advertencia
messageBox.ShowWarning("Esta acción no se puede deshacer");

// Mostrar un error
messageBox.ShowError("Ha ocurrido un error");

// Mostrar un error con excepción
messageBox.ShowError(new Exception("Error específico"));

// El resultado del diálogo depende del tipo de MessageBox implementado
// Por ejemplo, para WindowsMessageBox devuelve DialogResult
if (resultado == DialogResult.OK)
{
    // Acción cuando se presiona OK
}
```
```

## Requisitos

- .NET Framework 4.8
- Windows Forms
- Visual Studio 2019 o superior

## Tipos de Mensajes Disponibles

- `MessageBoxType.Info`: Mensajes informativos
- `MessageBoxType.Warning`: Advertencias
- `MessageBoxType.Error`: Errores
- `MessageBoxType.Question`: Preguntas
- `MessageBoxType.Success`: Éxitos
- `MessageBoxType.Custom`: Personalizado

## Uso

### Mensajes Básicos

```csharp
using CoreMessageBox;

// Mensaje básico
MessageBox.Show("Este es un mensaje", "Título");

// Mensaje con botones personalizados
var result = MessageBox.Show(
    message: "¿Estás seguro que quieres continuar?",
    title: "Confirmación",
    buttons: MessageBoxButtons.YesNo,
    icon: MessageBoxIcon.Question
);

// Manejar el resultado
if (result == DialogResult.Yes)
{
    // Acción cuando el usuario hace clic en Sí
}
```

### Mensajes con Temas

```csharp
// Configurar tema global
MessageBoxManager.SetTheme(MessageBoxTheme.Light);

// Mostrar mensaje con tema específico
MessageBox.Show(
    message: "Mensaje importante",
    title: "Atención",
    theme: MessageBoxTheme.Warning
);
```

### Mensajes Asíncronos

```csharp
// Mostrar mensaje asíncrono
await MessageBox.ShowAsync(
    message: "Procesando datos...",
    title: "Progreso",
    buttons: MessageBoxButtons.OKCancel,
    icon: MessageBoxIcon.Information
);
```

### Mensajes con Recursos

```csharp
// Usar recursos para internacionalización
MessageBox.Show(
    message: Resources.Messages.ProcessingData,
    title: Resources.Messages.ProgressTitle,
    icon: MessageBoxIcon.Information
);
```

### Manejo de Errores

```csharp
try
{
    // Operación que puede fallar
    await ProcessData();
}
catch (Exception ex)
{
    // Mostrar mensaje de error
    MessageBox.ShowError(
        message: ex.Message,
        title: "Error",
        details: ex.StackTrace
    );
}
```

## Mejores Prácticas

1. Usar mensajes consistentes en toda la aplicación
2. Implementar internacionalización para aplicaciones multilenguaje
3. Manejar estados de manera adecuada
4. Usar temas para mantener consistencia visual
5. Manejar errores de manera robusta
6. Usar mensajes asíncronos para operaciones largas
7. Documentar los mensajes que se muestran al usuario

## Soporte

Para reportar errores o solicitar características, por favor abre un issue en el repositorio de GitHub.

## Licencia

Este proyecto está bajo licencia MIT. Consulta el archivo LICENSE para más detalles.
