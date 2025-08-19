# CoreInterfaces

Una biblioteca de interfaces base que proporciona una capa de abstracción común para aplicaciones .NET, facilitando la implementación de patrones de diseño consistentes y la definición de contratos estándar.

## Características Principales

1. **Interfaces de Control de Formularios**
   - [IForm](cci:2://file:///c:/GitHub/DeveloperKitV2/CoreInterfaces/IForm.cs:3:4-7:5): Define la estructura básica para formularios
   - [IPrincipal](cci:2://file:///c:/GitHub/DeveloperKitV2/CoreInterfaces/IPrincipal.cs:3:4-6:5): Interfaz base para componentes principales
   - [IInformacionConsulta](cci:2://file:///c:/GitHub/DeveloperKitV2/CoreInterfaces/IInformacionConsulta.cs:3:4-12:5): Define la estructura para información de consulta

2. **Interfaces de Estado**
   - [IReadOnly](cci:2://file:///c:/GitHub/DeveloperKitV2/CoreInterfaces/IReadOnly.cs:2:4-6:5): Manejo de estado de solo lectura
   - [IProcessResponse<T>](cci:2://file:///c:/GitHub/DeveloperKitV2/CoreInterfaces/Response/IProcessResponse.cs:2:4-28:5): Respuestas de procesos con datos genéricos
   - [IWebApiResponse<T>](cci:2://file:///c:/GitHub/DeveloperKitV2/CoreInterfaces/WebApi/IWebApiResponse.cs:2:4-23:5): Respuestas específicas para Web API

3. **Características de las Interfaces**
   - Definición clara de contratos
   - Tipos genéricos para flexibilidad
   - Documentación integrada
   - Patrones de diseño consistentes
   - Soporte para resultados de procesos
   - Manejo de estados de solo lectura

## Instalación

El paquete se puede instalar usando NuGet:

```bash
dotnet add package DotNet.CoreInterfaces
```

También está disponible en el Visual Studio Package Manager:

```bash
Install-Package DotNet.CoreInterfaces
```

## Uso Básico

```csharp
// Implementación de IProcessResponse
public class ProcessResponse<T> : IProcessResponse<T>
{
    public T Data { get; set; }
    public ProcessResult ProcessResult { get; set; }
    public string SuccessMessage { get; set; }
    public string ErrorMessage { get; set; }
}

// Implementación de IWebApiResponse
public class WebApiResponse<T> : IWebApiResponse<T>
{
    public T Data { get; set; }
    public bool IsSuccessful { get; set; }
    public string SuccessMessage { get; set; }
    public string ErrorMessage { get; set; }
}

// Uso de IForm
public class MyForm : IForm
{
    public Type Form { get; set; }
}

// Uso de IReadOnly
public class MyReadOnlyComponent : IReadOnly
{
    public bool ReadOnly { get; set; }
    public bool IgnoreReadOnly { get; set; }
}
```
