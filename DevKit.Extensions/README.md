# DotNet.ExtensionsToolkit

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

Extensiones para `IDataReader` que permiten acceder a columnas por nombre y convertirlas de forma segura.

```csharp
// Ejemplo de uso básico
using (var reader = command.ExecuteReader())
{
    while (reader.Read())
    {
        int id = reader.GetInt32("Id");
        string nombre = reader.GetString("Nombre");
        DateTime fecha = reader.GetDate("FechaRegistro");
        
        // O convertir directamente a un objeto
        var usuario = reader.GetItem<Usuario>();
    }
}
```

#### Métodos Disponibles:
- `GetInt32(string columnName)`
- `GetString(string columnName)`
- `GetDecimal(string columnName)`
- `GetDate(string columnName)`
- `GetGuid(string columnName)`
- `GetBoolean(string columnName)`
- `GetDouble(string columnName)`
- `GetBytes(string columnName)`
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
// Añadir columna si no existe
tablaUsuarios.AddColumnIfNotExist("FechaActualizacion", typeof(DateTime));

// Seleccionar columnas específicas
var columnasSeleccionadas = tablaUsuarios.SelectColumns("Id", "Nombre", "Email");

// Convertir a diccionario
var diccionario = tablaUsuarios.ToDictionary();
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
// Verificar si un tipo es primitivo
bool esPrimitivo = typeof(int).IsPrimitive(); // true
bool esPrimitivo2 = typeof(Usuario).IsPrimitive(); // false

// Validar que un tipo no sea primitivo (útil para genéricos)
public void Procesar<T>()
{
    typeof(T).GuardIsPrimitive(); // Lanza excepción si T es primitivo
    // ...
}
```

#### 📤 Exportación de Datos

```csharp
// Exportar a CSV
string csv = tablaUsuarios.ToCsv(delimiter: ";");

// Exportar a JSON (usando System.Text.Json)
string json = JsonSerializer.Serialize(tablaUsuarios);
```

---

### 🧾 DataRowExtensions

Extensiones para acceder de manera segura a los datos de una fila `DataRow`, manejando valores nulos y conversiones de tipos.

```csharp
// Ejemplo de uso básico
DataRow fila = tablaUsuarios.Rows[0];

// Acceso seguro a los datos
int id = fila.GetInt32("Id");
string nombre = fila.GetString("Nombre");
DateTime? fechaNacimiento = fila.GetDate("FechaNacimiento"); // Devuelve DateTime?

// Conversión directa a objeto
var usuario = fila.GetItem<Usuario>();
```

#### Métodos Disponibles:

- `GetInt32(string columnName)` - Obtiene un entero de 32 bits
- `GetString(string columnName)` - Obtiene una cadena (maneja DBNull)
- `GetDecimal(string columnName)` - Obtiene un valor decimal
- `GetDate(string columnName)` - Obtiene un DateTime? (nullable)
- `GetGuid(string columnName)` - Obtiene un Guid
- `GetBoolean(string columnName)` - Obtiene un valor booleano
- `GetDouble(string columnName)` - Obtiene un número de punto flotante
- `GetBytes(string columnName)` - Obtiene un array de bytes
- `GetItem<T>()` - Mapea automáticamente la fila a un objeto del tipo `T`

#### Manejo de Valores Nulos:

```csharp
// Valor predeterminado personalizado
int edad = fila.GetInt32("Edad", -1); // Retorna -1 si es nulo

// Valor predeterminado genérico
DateTime fecha = fila.GetDate("FechaRegistro") ?? DateTime.MinValue;
```

---

### 🔐 SecurityAes

Implementación de cifrado simétrico AES (Advanced Encryption Standard) para proteger datos sensibles.

```csharp
// Inicialización con clave personalizada
var crypto = new SecurityAes("clave-secreta-32-caracteres");

// Cifrar datos
string textoPlano = "Información confidencial";
string textoCifrado = crypto.Encrypt(textoPlano);

// Descifrar datos
string textoDescifrado = crypto.Decrypt(textoCifrado);
```

#### Características:

- **Tamaños de clave soportados**: 128, 192 y 256 bits
- **Modo de operación**: CBC (Cipher Block Chaining)
- **Relleno**: PKCS7
- **IV (Vector de Inicialización)**: Generado aleatoriamente e incluido en el resultado

#### Mejores Prácticas:

1. **Almacenamiento seguro de claves**:
   ```csharp
   // En producción, usa un gestor de secretos
   var keyVaultUrl = "https://tu-keyvault.vault.azure.net/";
   var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
   string encryptionKey = (await secretClient.GetSecretAsync("EncryptionKey")).Value.Value;
   ```

2. **Rotación de claves**: Implementa un sistema para rotar las claves periódicamente

3. **Validación de datos**: Siempre valida los datos antes de cifrarlos/descifrarlos

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

Utilidad para generar cadenas de consulta (query strings) a partir de objetos, ideal para construir URLs de API.

#### Uso Básico:

```csharp
// Crear parámetros a partir de un objeto anónimo
var filtros = new 
{ 
    Nombre = "Juan Pérez",
    Edad = 30,
    Activo = true,
    FechaNacimiento = new DateTime(1990, 5, 15),
    Intereses = new[] { "programación", "música", "deportes" }
};

// Generar query string
var queryString = QueryStringExtensions.BindFrom(filtros);

// Resultado: "Nombre=Juan%20P%C3%A9rez&Edad=30&Activo=true&FechaNacimiento=1990-05-15&Intereses=programaci%C3%B3n&Intereses=m%C3%BAsica&Intereses=deportes"
```

#### Características:

- **Tipos soportados**:
  - Tipos primitivos (int, string, bool, etc.)
  - DateTime (formateado como "yyyy-MM-dd")
  - Enumeraciones (usando el valor numérico)
  - Arrays y colecciones (se repite el parámetro para cada valor)
  - Objetos anidados (usando notación de puntos)

- **Personalización de formato de fecha**:
  ```csharp
  // Especificar formato personalizado
  var opciones = new QueryStringOptions 
  { 
      DateTimeFormat = "yyyy/MM/dd HH:mm:ss" 
  };
  
  var queryString = QueryStringExtensions.BindFrom(filtros, opciones);
  ```

- **Exclusión de propiedades**:
  ```csharp
  // Excluir propiedades específicas
  var opciones = new QueryStringOptions
  {
      ExcludeProperties = new[] { "Token", "Contraseña" }
  };
  ```

#### Uso con HttpClient:

```csharp
// Crear URL con parámetros
var baseUrl = "https://api.ejemplo.com/usuarios";
var queryParams = QueryStringExtensions.BindFrom(new { Pagina = 1, TamañoPagina = 10 });
var urlCompleta = $"{baseUrl}?{queryParams}";

// Realizar petición HTTP
using var httpClient = new HttpClient();
var respuesta = await httpClient.GetAsync(urlCompleta);
```

#### Consideraciones de Seguridad:

- **Codificación URL**: Todos los valores son correctamente codificados para URL
- **Datos sensibles**: No incluir nunca contraseñas o tokens en query strings (usar headers HTTP en su lugar)
- **Tamaño de URL**: Las URLs tienen un límite de longitud (alrededor de 2000 caracteres en la mayoría de navegadores)

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
  <p>Última actualización: Julio 2025</p>
</div>
