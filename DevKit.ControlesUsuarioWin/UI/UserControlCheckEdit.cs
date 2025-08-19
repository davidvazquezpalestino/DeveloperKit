namespace DevKit.ControlesUsuarioWin.UI;

/// <summary>
/// Control de usuario personalizado que extiende CheckEdit con soporte para solo lectura.
/// Implementa la interfaz IReadOnly para manejar el estado de solo lectura.
/// </summary>
public class UserControlCheckEdit : CheckEdit, IReadOnly
{
    /// <summary>
    /// Obtiene o establece si se debe ignorar la propiedad ReadOnly.
    /// Útil para sobrescribir el comportamiento de solo lectura en ciertos escenarios.
    /// </summary>
    public bool IgnoreReadOnly { get; set; }

    /// <summary>
    /// Se llama cuando se crea el control.
    /// Configura el comportamiento de navegación con la tecla Enter.
    /// </summary>
    protected override void OnCreateControl()
    {
        EnterMoveNextControl = true;
    }
}