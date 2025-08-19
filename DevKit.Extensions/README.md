# DevKit.Extensions

[![.NET](https://img.shields.io/badge/.NET-6.0%20%7C%207.0%20%7C%208.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![Licencia](https://img.shields.io/badge/Licencia-MIT-blue.svg)](LICENSE)

Extensión de utilidades para .NET que simplifica el manejo de datos, incluyendo operaciones con `DataTable`, `DataRow`, `IDataReader`, cifrado AES y generación de parámetros de consulta.

## 📦 Características Principales

- **Manejo de Datos Avanzado**: Conversión entre objetos y DataTables
- **Seguridad Integrada**: Cifrado AES listo para usar
- **Manipulación de Consultas**: Generación de cadenas de consulta
- **Extensible**: Fácil de ampliar con nuevas funcionalidades
- **Compatible**: Funciona con .NET Standard 2.0+ y .NET 6.0+

---

## 📚 Documentación

### 🧩 DataReaderExtensions

Extensiones para `IDataReader` que permiten acceder a columnas por nombre de forma segura y mapear a objetos.

```csharp
// Ejemplo de uso básico
using (var reader = command.ExecuteReader())
{
    while (reader.Read())
    {
        int id = reader.GetValue<int>("Id");
        string nombre = reader.GetValue<string>("Nombre");
        DateTime fecha = reader.GetValue<DateTime>("FechaRegistro");

        // O convertir directamente a un objeto
        var usuario = reader.GetItem<Usuario>();
    }
}
```

### 🧠 JsonExtensions

Extensiones para trabajar con JSON de manera segura y conveniente.

```csharp
// Cadena JSON a diccionario (nivel superior)
Dictionary<string, object> dict = jsonString.ToDictionary();

// Cadena JSON (array) a lista de diccionarios
IEnumerable<Dictionary<string, object>> rows = jsonArrayString.ToDictionaryList();

// Deserializar a tipo fuerte
var obj = jsonString.ToObject<MiTipo>();
```

#### Notas:

- Ignora mayúsculas/minúsculas de nombres de propiedad
- Ignora valores nulos al escribir y evita ciclos de referencia
- Normaliza `JsonElement` a tipos .NET primarios cuando es posible

#### Métodos Disponibles:
- `GetValue<T>(string columnName)` - Obtiene el valor tipado (devuelve default para DBNull)
- `GetItem<T>()` - Convierte la fila actual en un objeto del tipo `T`

---

### 📊 DataTableExtensions

Extensión completa para trabajar con `DataTable` que incluye conversión, manipulación y análisis de datos.

#### 🔄 Conversión de Datos

```csharp
// Convertir lista a DataTable
List<Usuario> usuarios = ObtenerUsuarios();
DataTable tablaUsuarios = usuarios.ToDataTable();

// Convertir objeto único a DataTable
Usuario usuario = ObtenerUsuario(1);
DataTable tablaUsuario = usuario.ToDataTable();

// Convertir DataTable a lista
List<Usuario> listaUsuarios = tablaUsuarios.ToDataList<Usuario>();
```

#### 🔍 Filtrado y Búsqueda

```csharp
// Filtrar por columna
DataTable usuariosActivos = tablaUsuarios.FilterByColumn("Activo", true);

// Búsqueda personalizada
var resultados = tablaUsuarios.FindRows(
    row => row.Field<string>("Nombre").Contains("Juan")
);
```

#### 📊 Manipulación de Columnas

```csharp
// Seleccionar columnas específicas (ignora inexistentes)
var columnasSeleccionadas = tablaUsuarios.SelectColumns("Id", "Nombre", "Email");

// Convertir a lista de diccionarios
IEnumerable<Dictionary<string, object>> filas = tablaUsuarios.ToDictionary();
```

#### 📈 Ordenamiento

```csharp
// Ordenar por columna
DataTable ordenado = tablaUsuarios.OrderBy("Nombre");

// Orden descendente
DataTable ordenDescendente = tablaUsuarios.OrderBy("FechaRegistro", false);
```

#### 📋 Validaciones

```csharp
// Verificar si existe una columna
bool existeColumna = tablaUsuarios.HasColumn("Email");

// Verificar si la tabla está vacía
bool estaVacia = tablaUsuarios.IsEmpty();
```

#### 🧹 Manejo de Valores Nulos

```csharp
// Reemplazar nulos por valor predeterminado
tablaUsuarios.ReplaceNulls(0); // Para números

// Eliminar filas con valores nulos
DataTable sinNulos = tablaUsuarios.RemoveRowsWithNulls();
```

#### 🔄 Operaciones de Conjunto

```csharp
// Diferencia entre tablas
DataTable diferencia = tabla1.Except(tabla2, "Id");

// Unión de tablas
DataTable union = tabla1.Union(tabla2);
```

#### 🔍 Análisis y Depuración

```csharp
// Obtener estadísticas de columnas numéricas
var estadisticas = tablaUsuarios.GetColumnStatistics();

// Imprimir esquema de la tabla
string esquema = tablaUsuarios.PrintSchema();
```

#### 🏷 Nombres de Tablas Temporales

```csharp
// Tabla temporal local (solo visible en la sesión actual)
string nombreLocal = tablaUsuarios.GetTableNameLocal(); // "#Usuarios"

// Tabla temporal global (visible en todas las sesiones)
string nombreGlobal = tablaUsuarios.GetTableNameGlobal(); // "##Usuarios"
```

#### 🧪 Validación de Tipos

```csharp
// Verificar si un tipo es "simple" (string, numéricos, bool, fecha, enum, etc.)
bool esSimple = typeof(int).IsSimpleType(); // true
bool esSimple2 = typeof(Usuario).IsSimpleType(); // false

// Validar que un tipo genérico no sea primitivo/simple para conversión a DataTable
public void Procesar<T>()
{
    DataTableExtensions.GuardNotPrimitiveType<T>(); // Lanza excepción si T es primitivo/simple
    // ...
}
```

#### 📤 Exportación de Datos

```csharp
// Exportar a CSV
string csv = tablaUsuarios.WriteToCsv(delimiter: ";");

// Exportar a JSON (formato pretty)
string json = tablaUsuarios.WriteToJson();
```

---

### 🧾 DataRowExtensions

Extensiones para acceder de manera segura a los datos de una fila `DataRow`, manejando valores nulos y conversiones de tipos.

```csharp
// Ejemplo de uso básico
DataRow fila = tablaUsuarios.Rows[0];

// Acceso seguro a los datos
int id = fila.GetValue<int>("Id");
string nombre = fila.GetValue<string>("Nombre");
DateTime? fechaNacimiento = fila.GetValue<DateTime?>("FechaNacimiento");

// Conversión directa a objeto
var usuario = fila.GetItem<Usuario>();
```

#### Métodos Disponibles:

- `GetValue<T>(string columnName)` - Obtiene el valor tipado (devuelve default para DBNull)
- `GetItem<T>()` - Mapea automáticamente la fila a un objeto del tipo `T`

---

### 🔐 SecurityAes

Utilidades estáticas de cifrado simétrico AES para proteger datos sensibles.

```csharp
// Cifrar datos
string textoPlano = "Información confidencial";
string textoCifrado = SecurityAes.Encrypt(textoPlano);

// Descifrar datos
string textoDescifrado = SecurityAes.Decrypt(textoCifrado);
```

#### Características:

- Clave interna de 32 bytes (256-bit)
- Modo de operación: CBC
- Relleno: PKCS7
- IV generado aleatoriamente e incluido en el resultado (prefijo)

#### Consideraciones de Seguridad:

1. La clave está embebida en código para simplicidad; en escenarios reales, considera administrar la clave externamente.
2. Valida y controla excepciones de descifrado (texto corrupto o clave incompatible).

#### Manejo de Errores:

```csharp
try
{
    string resultado = crypto.Decrypt(datosCifrados);
}
catch (CryptographicException ex)
{
    // Manejar error de descifrado (clave incorrecta, datos corruptos, etc.)
    _logger.LogError(ex, "Error al descifrar los datos");
    throw new SecurityException("No se pudo descifrar la información", ex);
}
```

---

### 🌐 QueryStringExtensions

Utilidad para proyectar objetos simples a un diccionario clave-valor para query strings.

#### Uso Básico:

```csharp
// Crear parámetros a partir de un objeto anónimo
var filtros = new 
{ 
    Nombre = "Juan Pérez",
    Edad = 30,
    Activo = true,
    FechaNacimiento = new DateTime(1990, 5, 15)
};

// Generar diccionario clave-valor
IDictionary<string,string> qs = QueryStringExtensions.BindFrom(filtros);

// Si necesitas la cadena, constrúyela manualmente
string queryString = string.Join("&", qs.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
```

#### Características:

- Tipos soportados: primitivos, `string`, `bool`, `DateTime`/`DateTimeOffset` (formato "yyyy-MM-dd")
- Omite valores nulos
- No soporta: arrays/colecciones, objetos anidados, exclusiones ni formatos personalizados (puedes extenderlo si lo necesitas)

#### Uso con HttpClient:

```csharp
// Crear URL con parámetros
var baseUrl = "https://api.ejemplo.com/usuarios";
var qs = QueryStringExtensions.BindFrom(new { Pagina = 1, TamanoPagina = 10 });
var query = string.Join("&", qs.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
var urlCompleta = $"{baseUrl}?{query}";

// Realizar petición HTTP
using var httpClient = new HttpClient();
var respuesta = await httpClient.GetAsync(urlCompleta);
```

#### Consideraciones de Seguridad:

- Codifica siempre con `Uri.EscapeDataString`
- Evita incluir datos sensibles en query strings
- Considera límites de longitud de URL

#### Pruebas Unitarias:

```csharp
[Fact]
public void BindFrom_WithNullValues_ShouldSkipThem()
{
    // Arrange
    var parametros = new { Nombre = "Juan", Token = (string)null };
    
    // Act
    var resultado = QueryStringExtensions.BindFrom(parametros);
    
    // Assert
    Assert.Contains("Nombre=Juan", resultado);
    Assert.DoesNotContain("Token", resultado);
}
```

## 🚀 Cómo Contribuir

¡Agradecemos las contribuciones! Si deseas contribuir a este proyecto, sigue estos pasos:

1. Haz un fork del repositorio
2. Crea una rama para tu característica (`git checkout -b feature/nueva-funcionalidad`)
3. Haz commit de tus cambios (`git commit -am 'Añadir nueva funcionalidad'`)
4. Haz push a la rama (`git push origin feature/nueva-funcionalidad`)
5. Abre un Pull Request

### 📋 Guía de Estilo de Código

- Sigue las [convenciones de codificación de C#](https://docs.microsoft.com/es-es/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Incluye documentación XML en clases y métodos públicos
- Escribe pruebas unitarias para nuevo código
- Mantén la cobertura de código por encima del 80%
- Actualiza la documentación cuando sea necesario

### 🧪 Ejecutando las Pruebas

```bash
# Ejecutar todas las pruebas
dotnet test

# Ejecutar pruebas con cobertura de código
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## 📄 Licencia

Este proyecto está licenciado bajo la Licencia MIT - ver el archivo [LICENSE](LICENSE) para más detalles.

## ✨ Créditos

- **Equipo de Desarrollo** - [David Vázquez Palestino]
- **Contribuidores** - [Lista de contribuidores](CONTRIBUTORS.md)

## 📞 Soporte

Para reportar problemas o solicitar características, por favor abre un [issue](https://github.com/tu-usuario/tu-repositorio/issues) en el repositorio.

## 📚 Recursos Adicionales

- [Documentación de .NET](https://docs.microsoft.com/es-es/dotnet/)
- [Guías de estilo de C#](https://github.com/dotnet/runtime/blob/main/docs/coding-guidelines/coding-style.md)
- [Mejores prácticas de seguridad en .NET](https://docs.microsoft.com/es-es/dotnet/standard/security/)

---

<div align="center">
  <p>Hecho con ❤️ por el equipo de desarrollo</p>
  <p>Última actualización: Agosto 2025</p>
</div>
