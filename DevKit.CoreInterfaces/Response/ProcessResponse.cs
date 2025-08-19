namespace CoreInterfaces.Response;

/// <summary>Implementación concreta de la interfaz IProcessResponse que contiene los resultados de una operación.</summary>
public class ProcessResponse<T> : IProcessResponse<T>
{
    /// <summary>Obtiene o establece los datos asociados a la respuesta del proceso.</summary>
    public T Data { get; set; }
    /// <summary>Obtiene o establece el resultado del proceso.</summary>
    public ProcessResult ProcessResult { get; set; }
    /// <summary>Obtiene o establece el mensaje de éxito del proceso.</summary>
    public string SuccessMessage { get; set; }
    /// <summary>Obtiene o establece el mensaje de error del proceso.</summary>
    public string ErrorMessage { get; set; }
    /// <summary>Obtiene o establece la lista de mensajes de seguimiento o detalles del proceso.</summary>
    public List<string> StackMessage { get; set; }
}