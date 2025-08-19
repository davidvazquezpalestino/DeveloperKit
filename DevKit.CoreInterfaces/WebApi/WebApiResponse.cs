namespace CoreInterfaces.WebApi;

/// <summary>Implementación concreta de IWebApiResponse que representa una respuesta estándar para APIs web.</summary>
public class WebApiResponse<T> : IWebApiResponse<T>
{
    /// <summary>Obtiene o establece los datos asociados a la respuesta del proceso.</summary>
    public T Data { get; set; }
    /// <summary>Obtiene o establece un valor que indica si la operación fue exitosa.</summary>
    public bool IsSuccessful { get; set; }
    /// <summary>Obtiene o establece el mensaje de éxito de la operación.</summary>
    public string SuccessMessage { get; set; }
    /// <summary>Obtiene o establece el mensaje de error en caso de fallo en la operación.</summary>
    public string ErrorMessage { get; set; }
}