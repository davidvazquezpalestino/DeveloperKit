# 🛠️ DeveloperKit V2

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Licencia](https://img.shields.io/badge/Licencia-MIT-blue.svg)](LICENSE)
[![Estado del Proyecto](https://img.shields.io/badge/Estado-En%20Desarrollo-yellowgreen.svg)]()

DeveloperKit V2 es un conjunto integral de bibliotecas y utilidades reutilizables diseñadas para agilizar el desarrollo de aplicaciones empresariales en el ecosistema .NET. Este proyecto proporciona componentes modulares y de alto rendimiento que siguen las mejores prácticas de la industria.

## 🚀 Características Principales

- **Multiplataforma**: Compatible con .NET Standard 2.0+ y .NET 6.0+
- **Modular**: Usa solo los componentes que necesites
- **Extensible**: Fácil de extender y personalizar
- **Documentado**: Código documentado y ejemplos de uso

## 📦 Componentes Principales

| Componente | Descripción |
|------------|-------------|
| **CoreCrystalReports** | Integración avanzada con Crystal Reports para generación de reportes profesionales |
| **CoreDatabase** | Utilidades para acceso unificado a bases de datos SQL |
| **CoreExcelPackage** | Manejo eficiente de archivos Excel con soporte para formatos modernos |
| **CoreInterfaces** | Contratos y definiciones comunes para todo el ecosistema |
| **CoreMailKit** | Servicio de correo electrónico robusto y configurable |
| **CoreMessageBox** | Componentes de interfaz de usuario para mensajes al usuario |
| **CoreOracleDatabase** | Implementación específica para bases de datos Oracle |
| **CoreSpecificationValidation** | Patrón de especificación para validaciones complejas |
| **CoreUtilerias** | Utilidades generales y extensiones de uso común |
| **DotNet.CoreAuthorizationJwt** | Autenticación y autorización basada en JWT |
| **DotNet.CoreExportToDisk** | Utilidades para exportación segura de archivos |
| **DotNet.DependencyInjection** | Extensiones para el sistema de inyección de dependencias |
| **DotNet.Shared.RazorComponent** | Componentes Razor reutilizables |

## 🚀 Requisitos Previos

- **SDK de .NET 8.0** o superior
- **Visual Studio 2022** (Recomendado) o VS Code
- **Git** para control de versiones
- **SQL Server** (para componentes de base de datos)
- **Oracle Client** (solo si se usa CoreOracleDatabase)

## ⚙️ Instalación

1. **Clonar el repositorio**:
   ```bash
   git clone https://github.com/Desarrollos/DeveloperKit.git
   cd DeveloperKit
   ```

2. **Restaurar dependencias**:
   ```bash
   dotnet restore
   ```

3. **Compilar la solución**:
   ```bash
   dotnet build
   ```

## 🛠 Uso Básico

Cada componente está diseñado para ser independiente. Para usarlos en tu proyecto, agrega la referencia al paquete NuGet correspondiente o referencia el proyecto directamente.

### Ejemplo de uso de CoreUtilerias:

```csharp
using CoreUtilerias.Extensions;

// Ejemplo de extensión para DataTable
var dataTable = new DataTable();
dataTable.AddColumnIfNotExist("Id", typeof(int));
dataTable.AddColumnIfNotExist("Nombre", typeof(string));

// Convertir DataTable a lista de objetos
var lista = dataTable.ToDataList<MiClase>();
```

## 📚 Documentación

Cada componente incluye su propia documentación detallada en su archivo README.md. Para consultar la documentación específica de cada módulo, navega a la carpeta correspondiente.

## 🤝 Contribución

¡Las contribuciones son bienvenidas! Para contribuir al proyecto:

1. Haz un Fork del repositorio
2. Crea una rama para tu funcionalidad (`git checkout -b feature/nueva-funcionalidad`)
3. Haz commit de tus cambios (`git commit -am 'Agrega nueva funcionalidad'`)
4. Haz push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Abre un Pull Request

### 📋 Guía de Estilo de Código

- Sigue las [convenciones de codificación de C#](https://docs.microsoft.com/es-es/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Incluye documentación XML en métodos públicos
- Escribe pruebas unitarias para nuevo código
- Mantén la cobertura de código por encima del 80%

## 📄 Licencia

Para reportar errores o solicitar características, por favor abre un issue en el repositorio de GitHub.

## Créditos

Este proyecto fue desarrollado por el equipo de Desarrollos.

If you want to learn more about creating good readme files then refer the following [guidelines](https://docs.microsoft.com/en-us/azure/devops/repos/git/create-a-readme?view=azure-devops). You can also seek inspiration from the below readme files:
- [ASP.NET Core](https://github.com/aspnet/Home)
- [Visual Studio Code](https://github.com/Microsoft/vscode)
- [Chakra Core](https://github.com/Microsoft/ChakraCore)