namespace DevKit.ControlesUsuarioWin.Servicios;

/// <summary>Representa la información de conexión a una base de datos.</summary>
public class Conexion
{
    /// <summary>Obtiene o establece el identificador único de la conexión.</summary>
    public int IdConexion { get; set; }
    /// <summary>Obtiene o establece el nombre o dirección del servidor.</summary>
    public string Servidor { get; set; }
    /// <summary>Obtiene o establece el nombre de la base de datos.</summary>
    public string BaseDatos { get; set; }
    /// <summary>Obtiene o establece el nombre de usuario para la autenticación.</summary>
    public string Usuario { get; set; }
    /// <summary>Obtiene o establece la contraseña para la autenticación.</summary>
    public string Contrasenia { get; set; }
    /// <summary>Indica si esta es la conexión principal.</summary>
    public bool Principal { get; set; }

    /// <summary>Devuelve una representación en cadena de la conexión.</summary>
    /// <returns>Cadena que representa la conexión actual.</returns>
    public override string ToString() =>
        $"{Servidor} - {BaseDatos} {(Principal ? "Principal" : "")} ";
}