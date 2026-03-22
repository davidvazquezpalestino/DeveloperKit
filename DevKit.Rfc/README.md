# DevKit.Rfc

Biblioteca .NET para el cálculo de CURP y RFC mexicanos para personas físicas, implementada siguiendo los principios SOLID del Tío Bob (Robert C. Martin) con Value Objects para mayor robustez.

## 🎯 Características

- ✅ **Cálculo de CURP** para personas físicas
- ✅ **Cálculo de RFC** para personas físicas
- ✅ **Homoclave** siempre generada
- ✅ **Value Objects** inmutables (`CurpVO`, `RfcVO`)
- ✅ **Validación de formato** y palabras altisonantes
- ✅ **Normalización de acentos y caracteres especiales**
- ✅ **Arquitectura SOLID** con clases especializadas
- ✅ **Resultados tipados** con manejo de errores
- ✅ **Métodos de conveniencia** para uso rápido

## 📦 Instalación

```bash
dotnet add package DevKit.Rfc
```

## 🚀 Uso Rápido

### CURP - Cálculo Básico

```csharp
using DevKit.Rfc;
using DevKit.Rfc.ValueObjects;

// Crear calculadora de CURP
var curpCalc = new CurpCalc();

// Calcular CURP
CurpVO curp = curpCalc.CalcularCurp(
    nombre: "Juan",
    apellidoPaterno: "Pérez",
    apellidoMaterno: "López",
    fechaNacimiento: new DateTime(1980, 5, 15),
    genero: "H",
    entidadFederativa: "DF"
);

Console.WriteLine($"CURP: {curp.Valor}");
// Resultado: "PELJ800515HDFXXX00"
```

### RFC - Cálculo Básico

```csharp
// RFC Persona Física (siempre con homoclave)
var rfcCalc = new RfcCalc();
RfcVO rfc = rfcCalc.CalcularRfcPersonaFisica(
    nombre: "Juan",
    apellidoPaterno: "Pérez",
    apellidoMaterno: "López",
    fechaNacimiento: new DateTime(1980, 5, 15)
);

Console.WriteLine($"RFC: {rfc.Value}");
Console.WriteLine($"Homoclave: {rfc.Homoclave}");
Console.WriteLine($"¿Tiene Homoclave?: {rfc.TieneHomoclave}");
// Resultado: "PELJ800515XXX"
```

## 🏗️ Uso Avanzado con Value Objects

### Validación y Creación Segura

```csharp
using DevKit.Rfc.ValueObjects;

// Validar y crear RFC de forma segura
string rfcTexto = "VAPD900710CA8";

if (RfcVO.TryCrear(rfcTexto, out RfcVO rfcValido))
{
    Console.WriteLine($"RFC válido: {rfcValido.Value}");
    Console.WriteLine($"Homoclave: {rfcValido.Homoclave}");
    Console.WriteLine($"¿Es Persona Física?: {rfcValido.EsPersonaFisica}");
    Console.WriteLine($"¿Tiene Homoclave?: {rfcValido.TieneHomoclave}");
}
else
{
    Console.WriteLine("RFC inválido");
}

// Validar y crear CURP de forma segura
string curpTexto = "VAPD900710HVZZLV04";

if (CurpVO.TryCrear(curpTexto, out CurpVO curpValida))
{
    Console.WriteLine($"CURP válida: {curpValida.Valor}");
    Console.WriteLine($"Género: {curpValida.Genero}");
    Console.WriteLine($"Fecha Nacimiento: {curpValida.FechaNacimiento}");
    Console.WriteLine($"Entidad: {curpValida.EntidadFederativa}");
    Console.WriteLine($"Consonantes: {curpValida.ConsonantesInternas}");
}
else
{
    Console.WriteLine("CURP inválida");
}
```

### Comparación de Value Objects

```csharp
// Crear RFCs para comparar
var rfc1 = RfcVO.Crear("VAPD900710CA8");
var rfc2 = RfcVO.Crear("VAPD900710CA8");
var rfc3 = RfcVO.Crear("VAPD900710CA9");

// Comparaciones
Console.WriteLine($"RFC 1 == RFC 2: {rfc1 == rfc2}");  // True
Console.WriteLine($"RFC 1 == RFC 3: {rfc1 == rfc3}");  // False
Console.WriteLine($"RFC 1.Equals(RFC 2): {rfc1.Equals(rfc2)}");  // True

// Comparación de CURPs
var curp1 = CurpVO.Crear("VAPD900710HVZZLV04");
var curp2 = CurpVO.Crear("VAPD900710HVZZLV04");
var curp3 = CurpVO.Crear("VAPD900710HVZZLV05");

Console.WriteLine($"CURP 1 == CURP 2: {curp1 == curp2}");  // True
Console.WriteLine($"CURP 1 == CURP 3: {curp1 == curp3}");  // False
```

### Acceso a Componentes

```csharp
// Acceder a componentes del RFC
var rfc = RfcVO.Crear("VAPD900710CA8");
Console.WriteLine($"Letras Nombre: {rfc.LetrasNombre}");      // "VAPD"
Console.WriteLine($"Fecha: {rfc.Fecha}");                    // "900710"
Console.WriteLine($"Homoclave: {rfc.Homoclave}");             // "CA"
Console.WriteLine($"Dígito Verificador: {rfc.DigitoVerificador}"); // "8"

// Acceder a componentes de la CURP
var curp = CurpVO.Crear("VAPD900710HVZZLV04");
Console.WriteLine($"Letras Nombre: {curp.LetrasNombre}");      // "VAPD"
Console.WriteLine($"Fecha Nacimiento: {curp.FechaNacimiento}"); // "900710"
Console.WriteLine($"Género: {curp.Genero}");                   // 'H'
Console.WriteLine($"Entidad: {curp.EntidadFederativa}");    // "VZ"
Console.WriteLine($"Consonantes: {curp.ConsonantesInternas}"); // "ZLV"
Console.WriteLine($"Dígito Verificador: {curp.DigitoVerificador}"); // "04"
```

## 🏛️ Principios SOLID Implementados

### **S** - Single Responsibility Principle
- `CurpCalc`: Solo calcula CURP
- `RfcCalc`: Solo calcula RFC
- `CurpVO`: Solo representa y valida CURP
- `RfcVO`: Solo representa y valida RFC

### **O** - Open/Closed Principle
- Extensible sin modificar código existente
- Value Objects inmutables y extensibles

### **L** - Liskov Substitution Principle
- Todos los Value Objects pueden sustituirse entre sí

### **I** - Interface Segregation Principle
- Clases especializadas con responsabilidades específicas

### **D** - Dependency Inversion Principle
- Sin dependencias externas, todo autocontenido

## 📋 Especificaciones

### CURP (18 caracteres)
- **4 letras**: Apellido paterno (1 letra + 1 vocal) + Apellido materno (1 letra) + Nombre (1 letra)
- **6 dígitos**: Fecha de nacimiento (YYMMDD)
- **1 carácter**: Género (H/M)
- **2 caracteres**: Estado de nacimiento (código SAT)
- **3 caracteres**: Consonantes internas
- **2 caracteres**: Dígito verificador

### RFC (13 caracteres - siempre con homoclave)
- **4 letras**: Apellidos y nombre
- **6 dígitos**: Fecha de nacimiento (YYMMDD)
- **3 caracteres**: Homoclave (siempre generada)
- **1 carácter**: Dígito verificador

## 🧪 Ejemplos Completos

### Ejemplo 1: Cálculo de RFC para David Vazquez Palestino

```csharp
using DevKit.Rfc;
using DevKit.Rfc.ValueObjects;

// Datos de ejemplo
var rfcCalc = new RfcCalc();
RfcVO rfc = rfcCalc.CalcularRfcPersonaFisica(
    nombre: "David",
    apellidoPaterno: "Vazquez",
    apellidoMaterno: "Palestino",
    fechaNacimiento: new DateTime(1990, 7, 10)
);

Console.WriteLine($"RFC: {rfc.Value}");
Console.WriteLine($"Homoclave: {rfc.Homoclave}");
// Resultado: "VAPD900710CA8" ✅ RFC correcto de David
```

### Ejemplo 2: Cálculo de CURP con nombres compuestos

```csharp
var curpCalc = new CurpCalc();
CurpVO curp = curpCalc.CalcularCurp(
    nombre: "José Daniel",
    apellidoPaterno: "Vázquez",
    apellidoMaterno: "Palestino",
    fechaNacimiento: new DateTime(2003, 1, 31),
    genero: "H",
    entidadFederativa: "VZ"
);

Console.WriteLine($"CURP: {curp.Valor}");
// Resultado: "VAPD030131HVZZLNA5"
```

## 🔧 Configuración

La biblioteca no requiere configuración adicional. Los valores y catálogos están incluidos:

- Palabras altisonantes para validación
- Catálogo de estados del SAT
- Tablas de conversión para homoclave y dígitos verificadores
- Algoritmo oficial del SAT para dígito verificador

## 📝 Notas

- La biblioteca sigue las especificaciones oficiales del SAT
- Incluye manejo de nombres compuestos (CH, LL, TR)
- Filtra palabras comunes (DE, LA, LOS, etc.)
- Reemplaza caracteres especiales por X cuando es necesario
- Valida y corrige palabras altisonantes
- Los Value Objects son inmutables y thread-safe
- El dígito verificador usa la tabla oficial del SAT (incluye ñ = 24)

## 🤝 Contribuciones

Las contribuciones son bienvenidas. Por favor sigue los principios SOLID y las convenciones de código establecidas.

## 📄 Licencia

Este proyecto está licenciado bajo la MIT License.
