using CoreInterfaces;

namespace DevKit.ControlesUsuarioWin.Servicios;

/// <summary>Clase que implementa la interfaz IInformacionConsulta para manejar información de consulta.</summary>
public class InformacionConsulta : IInformacionConsulta
{
    /// <summary>Obtiene el identificador principal del elemento.</summary>
    public int PrincipalID { get; private set; }
    /// <summary>Obtiene el código del elemento.</summary>
    public string Codigo { get; private set; }
    /// <summary>Obtiene la descripción del elemento.</summary>
    public string Descripcion { get; private set; }

    /// <summary>Establece los valores del elemento.</summary>
    /// <param name="principalID">Identificador principal.</param>
    /// <param name="code">Código del elemento.</param>
    /// <param name="description">Descripción del elemento.</param>
    public void SetItem(int principalID, string code, string description)
    {
        PrincipalID = principalID;
        Codigo = code;
        Descripcion = description;
    }
    /// <summary>Obtiene el identificador principal del elemento.</summary>
    /// <returns>El identificador principal.</returns>
    public int GetPrincipal()
    {
        return PrincipalID;
    }
    /// <summary>Obtiene la instancia actual como IInformacionConsulta.</summary>
    /// <returns>La instancia actual.</returns>
    public IInformacionConsulta GetItem()
    {
        return this;
    }
}