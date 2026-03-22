# DevKit.Rfc

Biblioteca .NET para el cálculo de CURP y RFC mexicanos, implementada siguiendo los principios SOLID del Tío Bob (Robert C. Martin).

## 🎯 Características

- ✅ **Cálculo de CURP** para personas físicas
- ✅ **Cálculo de RFC** para personas físicas y morales
- ✅ **Homoclave** opcional para RFC
- ✅ **Validación de palabras altisonantes**
- ✅ **Normalización de acentos y caracteres especiales**
- ✅ **Arquitectura SOLID** con inyección de dependencias
- ✅ **Resultados tipados** con manejo de errores
- ✅ **Métodos de conveniencia** para uso rápido

## 📦 Instalación

```bash
dotnet add package DevKit.Rfc
```

## 🚀 Uso Rápido

### CURP - Método Simple

```csharp
using DevKit.Rfc;

// Calcular CURP directamente
var curp = RfcCurpHelper.CalcularCurp(
    "JUAN", 
    "PEREZ", 
    "LOPEZ", 
    new DateTime(1980, 5, 15), 
    'H', 
    "DF"
);
// Resultado: "PELJ800515HDFXXX00"
```

### RFC - Métodos Simples

```csharp
// RFC Persona Física sin homoclave
var rfc = RfcCurpHelper.CalcularRfcPersonaFisica(
    "JUAN", 
    "PEREZ", 
    "LOPEZ", 
    new DateTime(1980, 5, 15)
);

// RFC Persona Física con homoclave
var rfcConHomoclave = RfcCurpHelper.CalcularRfcPersonaFisica(
    "JUAN", 
    "PEREZ", 
    "LOPEZ", 
    new DateTime(1980, 5, 15), 
    conHomoclave: true
);

// RFC Persona Moral
var rfcMoral = RfcCurpHelper.CalcularRfcPersonaMoral(
    "EMPRESA SA DE CV", 
    new DateTime(2000, 1, 1), 
    conHomoclave: true
);
```

## 🏗️ Uso Avanzado con Inyección de Dependencias

### Modelos de Datos

```csharp
using DevKit.Rfc.Models;

// Persona Física
var persona = new PersonaFisica(
    nombre: "JUAN",
    apellidoPaterno: "PEREZ",
    apellidoMaterno: "LOPEZ",
    fechaNacimiento: new DateTime(1980, 5, 15),
    sexo: 'H',
    estadoNacimiento: "DF"
);

// Persona Moral
var personaMoral = new PersonaMoral(
    razonSocial: "EMPRESA SA DE CV",
    fechaConstitucion: new DateTime(2000, 1, 1)
);
```

### Resultados Detallados

```csharp
using DevKit.Rfc.Models;

// CURP con resultado detallado
var curpResult = RfcCurpHelper.CalcularCurp(persona);
if (curpResult.IsValid)
{
    Console.WriteLine($"CURP: {curpResult.Curp}");
}
else
{
    Console.WriteLine($"Error: {curpResult.ErrorMessage}");
}

// RFC con resultado detallado
var rfcResult = RfcCurpHelper.CalcularRfc(persona, conHomoclave: true);
if (rfcResult.IsValid)
{
    Console.WriteLine($"RFC: {rfcResult.Rfc}");
    Console.WriteLine($"Con homoclave: {rfcResult.HasHomoclave}");
}
else
{
    Console.WriteLine($"Error: {rfcResult.ErrorMessage}");
}
```

### Inyección de Dependencias Manual

```csharp
using DevKit.Rfc.Factories;
using DevKit.Rfc.Interfaces;

// Crear factory
var factory = new RfcCurpFactory();

// Crear calculadores
ICurpCalculator curpCalculator = factory.CreateCurpCalculator();
IRfcCalculator rfcCalculator = factory.CreateRfcCalculator();

// Usar calculadores
var curpResult = curpCalculator.Calculate(persona);
var rfcResult = rfcCalculator.Calculate(persona, conHomoclave: true);
```

## 🏛️ Principios SOLID Implementados

### **S** - Single Responsibility Principle
- `TextNormalizer`: Solo normaliza texto
- `ProfanityValidator`: Solo valida palabras altisonantes
- `LetterExtractor`: Solo extrae letras y vocales
- `CurpDigitCalculator`: Solo calcula dígitos verificadores
- `HomoclaveCalculator`: Solo calcula homoclaves

### **O** - Open/Closed Principle
- Extensible sin modificar código existente
- Nuevos validadores pueden agregarse mediante interfaces

### **L** - Liskov Substitution Principle
- Todas las implementaciones pueden sustituir a sus interfaces

### **I** - Interface Segregation Principle
- Interfaces específicas y pequeñas (`ICurpCalculator`, `IRfcCalculator`)

### **D** - Dependency Inversion Principle
- Depende de abstracciones, no de concreciones
- Inyección de dependencias via constructor

## 📋 Especificaciones

### CURP (18 caracteres)
- **4 letras**: Apellido paterno (1 letra + 1 vocal) + Apellido materno (1 letra) + Nombre (1 letra)
- **6 dígitos**: Fecha de nacimiento (YYMMDD)
- **1 carácter**: Sexo (H/M)
- **2 caracteres**: Estado de nacimiento (código SAT)
- **3 caracteres**: Consonantes internas
- **2 caracteres**: Dígito verificador

### RFC (12-13 caracteres)
- **3-4 letras**: Apellidos y nombre
- **6 dígitos**: Fecha de nacimiento/constitución (YYMMDD)
- **3 caracteres**: Homoclave (opcional)
- **1 carácter**: Dígito verificador

## 🧪 Ejemplos

```csharp
// Ejemplo completo
var persona = new PersonaFisica(
    "MARÍA DE LOS ÁNGELES",
    "GONZÁLEZ",
    "MARTÍNEZ",
    new DateTime(1985, 12, 3),
    'M',
    "JAL"
);

var curp = RfcCurpHelper.CalcularCurp(persona);
// "GOMA851203MJZRNN04"

var rfc = RfcCurpHelper.CalcularRfcPersonaFisica(
    "MARÍA DE LOS ÁNGELES", 
    "GONZÁLEZ", 
    "MARTÍNEZ", 
    new DateTime(1985, 12, 3), 
    true
);
// "GOMA851203MJ5"
```

## 🔧 Configuración

La biblioteca no requiere configuración adicional. Los valores y catálogos están incluidos:

- Palabras altisonantes para validación
- Catálogo de estados del SAT
- Tablas de conversión para homoclave y dígitos verificadores

## 📝 Notas

- La biblioteca sigue las especificaciones oficiales del SAT
- Incluye manejo de nombres compuestos (CH, LL, TR)
- Filtra palabras comunes (DE, LA, LOS, etc.)
- Reemplaza caracteres especiales por X cuando es necesario
- Valida y corrige palabras altisonantes

## 🤝 Contribuciones

Las contribuciones son bienvenidas. Por favor sigue los principios SOLID y las convenciones de código establecidas.

## 📄 Licencia

Este proyecto está licenciado bajo la MIT License.
