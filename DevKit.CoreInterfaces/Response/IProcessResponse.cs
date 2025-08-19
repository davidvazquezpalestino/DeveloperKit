namespace CoreInterfaces.Response;

/// <summary>Define un contrato para las respuestas de procesos que pueden contener datos, resultados y mensajes.</summary>
public interface IProcessResponse<T>
{
    /// <summary>Datos asociados a la respuesta del proceso.</summary>
    T Data { get; set; }

    /// <summary>Resultado del proceso.</summary>
    ProcessResult ProcessResult { get; set; }

    /// <summary>Mensaje de éxito del proceso.</summary>
    string SuccessMessage { get; set; }

    /// <summary>Mensaje de error del proceso.</summary>
    string ErrorMessage { get; set; }

    /// <summary>Lista de mensajes de seguimiento o detalles del proceso.</summary>
    List<string> StackMessage { get; set; }
}