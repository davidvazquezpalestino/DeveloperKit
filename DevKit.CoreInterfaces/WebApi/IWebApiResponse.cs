namespace CoreInterfaces.WebApi;

/// <summary>Define un contrato para las respuestas de API web que contienen datos y estado de la operación.</summary>
public interface IWebApiResponse<T>
{
    /// <summary>Datos asociados a la respuesta del proceso.</summary>
    T Data { get; set; }

    /// <summary>Resultado del proceso.</summary>
    bool IsSuccessful { get; set; }

    /// <summary>Mensaje de éxito del proceso.</summary>
    string SuccessMessage { get; set; }

    /// <summary>Mensaje de error del proceso.</summary>
    string ErrorMessage { get; set; }
}