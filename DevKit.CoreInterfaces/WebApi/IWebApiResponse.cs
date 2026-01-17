namespace CoreInterfaces.WebApi;

/// <summary>Define un contrato para las respuestas de API web que contienen datos y estado de la operación.</summary>
public interface IWebApiResponse<T>
{
    /// <summary>Metadatos adicionales asociados a la respuesta.</summary>
    object[] Metadata { get; set; }
    /// <summary>Datos asociados a la respuesta del proceso.</summary>
    T Data { get; set; }

    /// <summary>Resultado del proceso.</summary>
    bool Successful { get; set; }

    /// <summary>Mensaje de éxito del proceso.</summary>
    string SuccessMessage { get; set; }

    /// <summary>Mensaje de error del proceso.</summary>
    string ErrorMessage { get; set; }
}