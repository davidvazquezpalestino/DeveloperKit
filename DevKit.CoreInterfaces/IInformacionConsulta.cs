namespace CoreInterfaces;

/// <summary>Define un contrato para tipos que representan información de consulta con códigos y descripciones.</summary>
public interface IInformacionConsulta : IPrincipal<int>
{
    /// <summary>Obtiene el código de la información de consulta.</summary>
    string Codigo { get; }
    /// <summary>Obtiene la descripción de la información de consulta.</summary>
    string Descripcion { get; }
    /// <summary>Establece los valores de la información de consulta.</summary>
    void SetItem(int principalID, string code, string description);
    /// <summary>Obtiene la instancia actual de la información de consulta.</summary>
    IInformacionConsulta GetItem();
    /// <summary>Obtiene el identificador principal.</summary>
    int GetPrincipal();
}