# DevKit.Extensions

Biblioteca de utilidades esenciales para .NET que simplifica la manipulación de datos, seguridad y manejo de expresiones.

## Instalación

Agrega la referencia al proyecto o instala el paquete NuGet correspondiente:

```bash
dotnet add package DevKit.Extensions
```

## Uso de Utilidades

### 1. DataTableExtensions

Conversión y manipulación avanzada de `DataTable`.

```csharp
// Convertir lista a DataTable
var tabla = miLista.ToDataTable();

// Convertir DataTable a Lista de objetos
var lista = tabla.ToDataList<MiClase>();

// Filtrar y Ordenar
var filtrada = tabla.FilterByColumn("Activo", true);
var ordenada = tabla.OrderBy("Nombre", ascending: true);
```

### 2. JSON Extensions

Serialización y deserialización segura con `System.Text.Json`.

```csharp
// JSON a Diccionario
var dict = jsonString.ToDictionary();

// JSON a Objeto tipado
var obj = jsonString.ToObject<MiTipo>();

// Objeto a JSON (Pretty Print)
var json = miObjeto.WriteToJson();
```

### 3. Seguridad (AES)

Cifrado simétrico simple para datos sensibles.

```csharp
// Cifrar
string cifrado = SecurityAes.Encrypt("texto secreto");

// Descifrar
string plano = SecurityAes.Decrypt(cifrado);
```

### 4. DataReader e IDataRecord

Acceso seguro a datos de base de datos manejando nulos.

```csharp
while (reader.Read())
{
    // Obtener valor con manejo de DBNull
    int id = reader.GetValue<int>("Id");
    
    // Mapeo automático de fila a objeto
    var item = reader.GetItem<MiTipo>();
}
```

### 5. URLs y Parámetros

Extensiones para construir URLs de forma segura con parámetros de consulta.

```csharp
// Agregar parámetros de consulta desde un objeto a una URL
string url = "api/usuarios".UrlFromQuery(new { Activo = true, Rol = "Admin" });
// Resultado: "api/usuarios?Activo=true&Rol=Admin"

// Reemplazar marcadores en la ruta
string route = "api/ventas/{0}/{1}".UrlFromRoute(DateTime.Now, "12345");
```

## Características Técnicas

- Soporte para **.NET Standard 2.0+** y **.NET 6.0+**.
- Manejo automático de `DBNull`.
- Cifrado AES con IV dinámico.
- Optimización de expresiones para generación de claves.
